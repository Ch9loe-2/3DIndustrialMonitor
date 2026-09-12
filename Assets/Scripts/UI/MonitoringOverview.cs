using UnityEngine;
using TMPro;

public class MonitoringOverview : MonoBehaviour
{
    [Header("系统概览")]
    [SerializeField] private TMP_Text totalDevicesValue;
    [SerializeField] private TMP_Text normalDevicesValue;
    [SerializeField] private TMP_Text warningDevicesValue;
    [SerializeField] private TMP_Text faultDevicesValue;
    [SerializeField] private TMP_Text offlineDevicesValue;

    [Header("监控设备")]
    [SerializeField] private DeviceData[] devices;

    private void OnEnable()
    {
        SystemEvents.DeviceStatusChanged += UpdateOverview;
        UpdateOverview();
    }

    private void OnDisable()
    {
        SystemEvents.DeviceStatusChanged -= UpdateOverview;
    }

    public void UpdateOverview()
    {
        if (devices == null)
        {
            return;
        }

        int total = devices.Length;
        int normal = 0;
        int warning = 0;
        int fault = 0;
        int offline = 0;

        foreach (DeviceData device in devices)
        {
            if (device == null)
            {
                continue;
            }

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

                case "离线":
                    offline++;
                    break;
            }
        }

        totalDevicesValue.text = total.ToString();
        normalDevicesValue.text = normal.ToString();
        warningDevicesValue.text = warning.ToString();
        faultDevicesValue.text = fault.ToString();

        if (offlineDevicesValue != null)
        {
            offlineDevicesValue.text = offline.ToString();
        }
    }
}
