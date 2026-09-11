using UnityEngine;
using TMPro;

public class MonitoringOverview : MonoBehaviour
{
    [Header("系统概览")]
    [SerializeField] private TMP_Text totalDevicesValue;
    [SerializeField] private TMP_Text normalDevicesValue;
    [SerializeField] private TMP_Text warningDevicesValue;
    [SerializeField] private TMP_Text faultDevicesValue;

    [Header("监控设备")]
    [SerializeField] private DeviceData[] devices;

    private void Start()
    {
        UpdateOverview();
    }

    private void Update()
    {
        UpdateOverview();
    }

    public void UpdateOverview()
    {
        int total = devices.Length;
        int normal = 0;
        int warning = 0;
        int fault = 0;

        foreach (DeviceData device in devices)
        {
            switch (device.status)
            {
                case "正常":
                    normal++;
                    break;

                case "警告":
                    warning++;
                    break;

                case "故障":
                    fault++;
                    break;
            }
        }

        totalDevicesValue.text = total.ToString();
        normalDevicesValue.text = normal.ToString();
        warningDevicesValue.text = warning.ToString();
        faultDevicesValue.text = fault.ToString();
    }
}