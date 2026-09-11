
using UnityEngine;
using TMPro;

public class DeviceController : MonoBehaviour
{
    [Header("设备数据")]
    [SerializeField] private DeviceData deviceData;

    [Header("设备详情面板")]
    [SerializeField] private GameObject deviceDetailPanel;

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

    [Header("设备状态灯")]
    [SerializeField] private Renderer statusLightRenderer;

    private bool isTemperatureFaultRunning = false;
    private bool isPressureFaultRunning = false;

    private void OnMouseDown()
    {
        Debug.Log($"点击了{deviceData.deviceName}");

        if (deviceDetailPanel != null)
        {
            deviceDetailPanel.SetActive(true);
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
        }
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
            }

            temperatureLabel.text =
                $"温度    {deviceData.temperature:F1} ℃";

            deviceStatusLabel.text =
                $"状态    ● {deviceData.status}";

            UpdateStatusLight();

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
            }

            pressureLabel.text =
                $"压力    {deviceData.pressure:F2} MPa";

            deviceStatusLabel.text =
                $"状态    ● {deviceData.status}";

            UpdateStatusLight();

            yield return new WaitForSeconds(1f);
        }

        isPressureFaultRunning = false;
    }

    private void RecoverDevice()
    {
        StopAllCoroutines();

        isTemperatureFaultRunning = false;
        isPressureFaultRunning = false;

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
    }
}
