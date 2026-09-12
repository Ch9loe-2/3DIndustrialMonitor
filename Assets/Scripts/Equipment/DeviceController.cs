
using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class DeviceController : MonoBehaviour
{
    [Header("设备数据")]
    [SerializeField] private DeviceData deviceData;

    [Header("设备详情面板")]
    [SerializeField] private GameObject deviceDetailPanel;
    [Header("页面切换")]
    [SerializeField] private PanelSwitcher panelSwitcher;

    [Header("UI 文本")]
    [SerializeField] private TMP_Text deviceNameTitle;
    [SerializeField] private TMP_Text deviceTypeLabel;
    [SerializeField] private TMP_Text deviceStatusLabel;
    [SerializeField] private TMP_Text temperatureLabel;
    [SerializeField] private TMP_Text pressureLabel;
    [SerializeField] private TMP_Text speedLabel;
    [SerializeField] private TMP_Text runtimeLabel;

    [Header("故障模拟按钮")]
    [SerializeField] private UnityEngine.UI.Button temperatureFaultButton;
    [SerializeField] private UnityEngine.UI.Button pressureFaultButton;
    [SerializeField] private UnityEngine.UI.Button recoveryButton;

    [Header("离线按钮")]
    [SerializeField] private UnityEngine.UI.Button offlineButton;

    [Header("设备状态灯")]
    [SerializeField] private Renderer statusLightRenderer;

    // API 中该设备的 ID（设备 A=1, B=2, C=3）
    private int ApiDeviceId
    {
        get
        {
            if (deviceData == null) return 1;
            if (deviceData.deviceName == "设备 A") return 1;
            if (deviceData.deviceName == "设备 B") return 2;
            if (deviceData.deviceName == "设备 C") return 3;
            return 1;
        }
    }

    private bool isTemperatureFaultRunning = false;
    private bool isPressureFaultRunning = false;
    private bool temperatureAlarmAdded = false;
    private bool pressureAlarmAdded = false;

    [Header("自动阈值报警")]
    [Tooltip("是否启用超阈值自动报警：温度/压力达到阈值即自动触发报警，回到正常后自动恢复")]
    public bool enableAutoThresholdAlarm = true;

    [Header("温度阈值 (℃)")]
    public float tempWarnThreshold = 78f;
    public float tempFaultThreshold = 90f;

    [Header("压力阈值 (MPa)")]
    public float pressureWarnThreshold = 2.0f;
    public float pressureFaultThreshold = 2.3f;

    // 自动仿真的内部状态
    private float simEventTimer = 6f;
    private bool simEventActive = false;
    private float simEventRemaining = 0f;
    private string simEventKind = "temp";
    private float targetTemperature;
    private float targetPressure;
    private bool simInitialized = false;
    private float apiSyncTimer = 2f;


    private void OnMouseDown()
    {
        Debug.Log($"点击了{deviceData.deviceName}");

        if (panelSwitcher != null)
        {
            panelSwitcher.ShowDeviceDetail();
        }

        deviceNameTitle.text = deviceData.deviceName;
        deviceTypeLabel.text = deviceData.deviceType;
        deviceStatusLabel.text = $"状态    ● {deviceData.status}";
        temperatureLabel.text = $"温度    {deviceData.temperature:F1} ℃";
        pressureLabel.text = $"压力    {deviceData.pressure:F2} MPa";
        speedLabel.text = $"转速    {deviceData.rpm} RPM";
        runtimeLabel.text = $"运行时间    {deviceData.runtime:F1} h";

        UpdateStatusLight();

        temperatureFaultButton.onClick.RemoveAllListeners();
        temperatureFaultButton.onClick.AddListener(SimulateTemperatureFault);

        pressureFaultButton.onClick.RemoveAllListeners();
        pressureFaultButton.onClick.AddListener(SimulatePressureFault);

        recoveryButton.onClick.RemoveAllListeners();
        recoveryButton.onClick.AddListener(RecoverDevice);

        // 离线/上线按钮：动态绑定到当前选中设备
        if (offlineButton != null)
        {
            offlineButton.onClick.RemoveAllListeners();
            offlineButton.onClick.AddListener(ToggleOffline);
        }
    }

    private void UpdateStatusLight()
    {
        if (statusLightRenderer == null)
        {
            return;
        }

        switch (deviceData.status)
        {
            case "正常":
                statusLightRenderer.material.color = Color.green;
                break;

            case "警告":
                statusLightRenderer.material.color = Color.yellow;
                break;

            case "故障":
                statusLightRenderer.material.color = Color.red;
                break;

            case "离线":
                // 离线用灰色，与其他状态区分
                statusLightRenderer.material.color = Color.gray;
                break;
        }
    }

    /// <summary>
    /// 切换设备的在线/离线状态（供"离线/上线"按钮调用）。
    /// 离线时状态灯变灰、概览"离线"计数 +1。
    /// </summary>
    public void ToggleOffline()
    {
        if (deviceData == null)
        {
            return;
        }

        if (deviceData.isOnline)
        {
            deviceData.SetOffline();
        }
        else
        {
            deviceData.SetOnline();
        }

        // 刷新详情面板显示
        if (deviceStatusLabel != null)
        {
            deviceStatusLabel.text = $"状态    ● {deviceData.status}";
        }

        UpdateStatusLight();

        // 通知概览刷新
        SystemEvents.RaiseDeviceStatusChanged();

        // 同步到 API
        _ = SyncToApiAsync();

        Debug.Log($"设备 {deviceData.deviceName} 状态：{deviceData.status}");
    }

    private void SimulateTemperatureFault()
    {
        if (isTemperatureFaultRunning)
        {
            return;
        }

        isTemperatureFaultRunning = true;
        StartCoroutine(TemperatureFaultRoutine());
    }

    private System.Collections.IEnumerator TemperatureFaultRoutine()
    {
        float[] temperatures = { 75f, 85f, 95f, 105f };

        foreach (float temperature in temperatures)
        {
            deviceData.temperature = temperature;

            if (temperature < 95f)
            {
                deviceData.status = "警告";
            }
            else
            {
                deviceData.status = "故障";

                if (!temperatureAlarmAdded && AlarmManager.Instance != null)
                {
                    temperatureAlarmAdded = true;

                    AlarmManager.Instance.AddAlarm(
                        deviceData.deviceName,
                        "温度过高",
                        "故障",
                        $"温度达到 {deviceData.temperature:F1} ℃"
                    );
                }
            }

            temperatureLabel.text =
                $"温度    {deviceData.temperature:F1} ℃";

            deviceStatusLabel.text =
                $"状态    ● {deviceData.status}";

            UpdateStatusLight();

            SystemEvents.RaiseDeviceStatusChanged();

            // 同步到 API
            _ = SyncToApiAsync();

            yield return new WaitForSeconds(1f);
        }

        isTemperatureFaultRunning = false;
    }

    private void SimulatePressureFault()
    {
        if (isPressureFaultRunning)
        {
            return;
        }

        isPressureFaultRunning = true;
        StartCoroutine(PressureFaultRoutine());
    }

    private System.Collections.IEnumerator PressureFaultRoutine()
    {
        float[] pressures = { 1.9f, 2.1f, 2.3f, 2.5f };

        foreach (float pressure in pressures)
        {
            deviceData.pressure = pressure;

            if (pressure < 2.3f)
            {
                deviceData.status = "警告";
            }
            else
            {
                deviceData.status = "故障";

                if (!pressureAlarmAdded && AlarmManager.Instance != null)
                {
                    pressureAlarmAdded = true;

                    AlarmManager.Instance.AddAlarm(
                        deviceData.deviceName,
                        "压力异常",
                        "故障",
                        $"压力达到 {deviceData.pressure:F2} MPa"
                    );
                }
            }
            pressureLabel.text =
                $"压力    {deviceData.pressure:F2} MPa";

            deviceStatusLabel.text =
                $"状态    ● {deviceData.status}";

            UpdateStatusLight();

            SystemEvents.RaiseDeviceStatusChanged();

            yield return new WaitForSeconds(1f);
        }

        isPressureFaultRunning = false;
    }

    private void RecoverDevice()
    {
        StopAllCoroutines();

        isTemperatureFaultRunning = false;
        isPressureFaultRunning = false;
        temperatureAlarmAdded = false;
        pressureAlarmAdded = false;

        deviceData.temperature = deviceData.initialTemperature;
        deviceData.pressure = deviceData.initialPressure;
        deviceData.rpm = deviceData.initialRpm;
        deviceData.runtime = deviceData.initialRuntime;
        deviceData.status = "正常";

        temperatureLabel.text =
            $"温度    {deviceData.temperature:F1} ℃";

        pressureLabel.text =
            $"压力    {deviceData.pressure:F2} MPa";

        speedLabel.text =
            $"转速    {deviceData.rpm} RPM";

        runtimeLabel.text =
            $"运行时间    {deviceData.runtime:F1} h";

        deviceStatusLabel.text =
            $"状态    ● {deviceData.status}";

        UpdateStatusLight();

        // 恢复该设备的报警记录
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.RecoverAlarm(deviceData.deviceName);
        }

        // 同步设备数据到 API
        _ = SyncToApiAsync();

        // 重置自动仿真：恢复后温度/压力回到初始，避免自动事件把数值又拉高
        simEventActive = false;
        simEventTimer = UnityEngine.Random.Range(8f, 16f);
        targetTemperature = deviceData.initialTemperature;
        targetPressure = deviceData.initialPressure;
    }

    private async Task SyncToApiAsync()
    {
        if (ApiClient.Instance == null || !ApiClient.Instance.IsConnected) return;

        string json = JsonUtility.ToJson(new DeviceUpdateJson
        {
            temperature = deviceData.temperature,
            pressure = deviceData.pressure,
            rpm = deviceData.rpm,
            runtime = deviceData.runtime,
            status = deviceData.status
        });

        await ApiClient.Instance.PutAsync($"/api/devices/{ApiDeviceId}", json);
    }

    // ───────────────────────────────────────────────────────────
    // 自动仿真 + 阈值报警
    // ───────────────────────────────────────────────────────────

    private void EnsureSimInit()
    {
        if (simInitialized) return;
        simInitialized = true;
        targetTemperature = deviceData.initialTemperature;
        targetPressure = deviceData.initialPressure;
    }

    private void Update()
    {
        if (deviceData == null || !deviceData.isOnline) return;

        EnsureSimInit();
        TickSimulation(Time.deltaTime);

        if (enableAutoThresholdAlarm)
        {
            CheckThresholds();
        }

        // 节流（每 2s）把波动后的数值同步到后端，让设备表反映实时工况
        apiSyncTimer -= Time.deltaTime;
        if (apiSyncTimer <= 0f)
        {
            apiSyncTimer = 2f;
            _ = SyncToApiAsync();
        }
    }

    /// <summary>
    /// 自然仿真：温度/压力围绕初始值小幅浮动；不定时触发一次升温/升压工况事件，
    /// 让数据"活"起来，也让"超阈值自动报警"有机会真正自行触发。
    /// 手动故障协程运行时跳过对应维度，交给协程控制数值。
    /// </summary>
    private void TickSimulation(float dt)
    {
        if (!simEventActive)
        {
            simEventTimer -= dt;
            if (simEventTimer <= 0f)
            {
                if (UnityEngine.Random.value < 0.5f)
                {
                    simEventActive = true;
                    simEventRemaining = UnityEngine.Random.Range(4f, 8f);
                    simEventKind = UnityEngine.Random.value < 0.6f ? "temp" : "pressure";
                    bool fault = UnityEngine.Random.value < 0.35f;
                    if (simEventKind == "temp")
                    {
                        targetTemperature = fault
                            ? UnityEngine.Random.Range(90f, 100f)
                            : UnityEngine.Random.Range(78f, 86f);
                    }
                    else
                    {
                        targetPressure = fault
                            ? UnityEngine.Random.Range(2.3f, 2.6f)
                            : UnityEngine.Random.Range(2.0f, 2.25f);
                    }
                }
                else
                {
                    simEventTimer = UnityEngine.Random.Range(6f, 14f);
                }
            }
        }
        else
        {
            simEventRemaining -= dt;
            if (simEventRemaining <= 0f)
            {
                simEventActive = false;
                simEventTimer = UnityEngine.Random.Range(8f, 16f);
                targetTemperature = deviceData.initialTemperature;
                targetPressure = deviceData.initialPressure;
            }
        }

        // 向目标值平滑插值（手动故障时跳过对应维度）
        if (!isTemperatureFaultRunning)
        {
            deviceData.temperature = Mathf.Lerp(deviceData.temperature, targetTemperature, 3f * dt)
                + UnityEngine.Random.Range(-0.15f, 0.15f);
        }
        if (!isPressureFaultRunning)
        {
            deviceData.pressure = Mathf.Lerp(deviceData.pressure, targetPressure, 3f * dt)
                + UnityEngine.Random.Range(-0.01f, 0.01f);
        }
    }

    private int StatusRank(string s)
    {
        if (s == "故障") return 3;
        if (s == "警告") return 2;
        return 1;
    }

    /// <summary>
    /// 根据当前温度/压力与阈值判定状态：超阈值自动设状态 + 自动报警（限频），
    /// 回到正常后自动恢复报警。
    /// </summary>
    private void CheckThresholds()
    {
        string target = "正常";

        if (deviceData.temperature >= tempFaultThreshold)
        {
            target = "故障";
            EnsureTempAlarm("故障", $"温度达到 {deviceData.temperature:F1} ℃");
        }
        else if (deviceData.temperature >= tempWarnThreshold)
        {
            target = StatusRank(target) >= 2 ? target : "警告";
            EnsureTempAlarm("警告", $"温度偏高 {deviceData.temperature:F1} ℃");
        }

        if (deviceData.pressure >= pressureFaultThreshold)
        {
            target = "故障";
            EnsurePressAlarm("故障", $"压力达到 {deviceData.pressure:F2} MPa");
        }
        else if (deviceData.pressure >= pressureWarnThreshold)
        {
            target = StatusRank(target) >= 2 ? target : "警告";
            EnsurePressAlarm("警告", $"压力偏高 {deviceData.pressure:F2} MPa");
        }

        // 全部回到正常 → 自动恢复该设备报警
        if (target == "正常")
        {
            if (temperatureAlarmAdded || pressureAlarmAdded)
            {
                if (AlarmManager.Instance != null)
                {
                    AlarmManager.Instance.RecoverAlarm(deviceData.deviceName);
                }
                temperatureAlarmAdded = false;
                pressureAlarmAdded = false;
            }
        }

        // 应用状态（离线状态不在此覆盖）
        if (target != deviceData.status && deviceData.status != "离线")
        {
            deviceData.status = target;
            ApplyStatusVisualAndSync();
        }
    }

    private void EnsureTempAlarm(string level, string msg)
    {
        if (!temperatureAlarmAdded && AlarmManager.Instance != null)
        {
            temperatureAlarmAdded = true;
            AlarmManager.Instance.AddAlarm(deviceData.deviceName, "温度过高", level, msg);
        }
    }

    private void EnsurePressAlarm(string level, string msg)
    {
        if (!pressureAlarmAdded && AlarmManager.Instance != null)
        {
            pressureAlarmAdded = true;
            AlarmManager.Instance.AddAlarm(deviceData.deviceName, "压力异常", level, msg);
        }
    }

    /// <summary>统一刷新状态灯 / 概览 / 后端同步（状态变化时调用）</summary>
    private void ApplyStatusVisualAndSync()
    {
        if (deviceStatusLabel != null)
        {
            deviceStatusLabel.text = $"状态    ● {deviceData.status}";
        }
        UpdateStatusLight();
        SystemEvents.RaiseDeviceStatusChanged();
        _ = SyncToApiAsync();
    }

    [System.Serializable]
    private class DeviceUpdateJson
    {
        public float temperature;
        public float pressure;
        public int rpm;
        public float runtime;
        public string status;
    }
}
