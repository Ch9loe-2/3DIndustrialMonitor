using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 历史数据采样器：定期采集各设备的运行数据，供历史记录页绘制趋势图。
/// 单例，场景启动时自动开始采样。
/// </summary>
public class HistoryRecorder : MonoBehaviour
{
    public static HistoryRecorder Instance { get; private set; }

    [Header("采样设置")]
    [SerializeField] private float sampleInterval = 2f;   // 采样间隔（秒）
    [SerializeField] private int maxPoints = 60;          // 最多保留点数

    // 设备名 → 温度序列
    private Dictionary<string, List<float>> temperatureHistory
        = new Dictionary<string, List<float>>();

    // 设备名 → 压力序列
    private Dictionary<string, List<float>> pressureHistory
        = new Dictionary<string, List<float>>();

    private float timer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= sampleInterval)
        {
            timer = 0f;
            SampleAllDevices();
        }
    }

    /// <summary>采集所有设备的当前数据</summary>
    private void SampleAllDevices()
    {
        DeviceData[] devices = FindObjectsOfType<DeviceData>();

        foreach (DeviceData device in devices)
        {
            if (device == null) continue;

            RecordValue(temperatureHistory, device.deviceName, device.temperature);
            RecordValue(pressureHistory, device.deviceName, device.pressure);
        }
    }

    private void RecordValue(Dictionary<string, List<float>> dict, string deviceName, float value)
    {
        if (!dict.ContainsKey(deviceName))
        {
            dict[deviceName] = new List<float>();
        }

        List<float> list = dict[deviceName];
        list.Add(value);

        // 超出上限则移除最旧的数据
        while (list.Count > maxPoints)
        {
            list.RemoveAt(0);
        }
    }

    /// <summary>获取某设备的温度历史</summary>
    public List<float> GetTemperatureHistory(string deviceName)
    {
        return GetHistory(temperatureHistory, deviceName);
    }

    /// <summary>获取某设备的压力历史</summary>
    public List<float> GetPressureHistory(string deviceName)
    {
        return GetHistory(pressureHistory, deviceName);
    }

    private List<float> GetHistory(Dictionary<string, List<float>> dict, string deviceName)
    {
        if (dict.ContainsKey(deviceName))
        {
            return dict[deviceName];
        }
        return new List<float>();
    }

    /// <summary>供外部手动记录（例如故障模拟时立即采样）</summary>
    public void RecordNow(DeviceData device)
    {
        if (device == null) return;

        RecordValue(temperatureHistory, device.deviceName, device.temperature);
        RecordValue(pressureHistory, device.deviceName, device.pressure);
    }
}