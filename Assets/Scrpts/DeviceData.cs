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
}