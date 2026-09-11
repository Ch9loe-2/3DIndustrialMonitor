using UnityEngine;

public class DeviceData : MonoBehaviour
{
    [Header("设备信息")]
    public string deviceName = "设备 A";
    public string deviceType = "生产设备";

    [Header("运行数据")]
    public float temperature = 65.2f;
    public float pressure = 1.62f;
    public int rpm = 1450;
    public float runtime = 128.5f;

    [Header("设备状态")]
    public string status = "正常";

    [Header("初始数据")]
    [HideInInspector] public float initialTemperature;
    [HideInInspector] public float initialPressure;
    [HideInInspector] public int initialRpm;
    [HideInInspector] public float initialRuntime;

    private void Awake()
    {
        initialTemperature = temperature;
        initialPressure = pressure;
        initialRpm = rpm;
        initialRuntime = runtime;
    }
}