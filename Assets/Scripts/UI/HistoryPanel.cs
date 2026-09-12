using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 历史记录页面：展示设备运行数据的趋势折线图。
/// 支持切换设备与指标（温度 / 压力），并显示当前值、最高、最低统计。
/// </summary>
public class HistoryPanel : MonoBehaviour
{
    [Header("图表")]
    [SerializeField] private LineChart lineChart;

    [Header("信息显示")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statText;

    [Header("Y 轴范围")]
    [SerializeField] private float tempMin = 40f;
    [SerializeField] private float tempMax = 120f;
    [SerializeField] private float pressMin = 0f;
    [SerializeField] private float pressMax = 3f;

    private string currentDevice = "设备 A";
    private string currentMetric = "温度";

    private void OnEnable()
    {
        RefreshChart();
    }

    // ===== 供按钮绑定的无参数方法 =====

    public void SelectDeviceA() { SelectDevice("设备 A"); }
    public void SelectDeviceB() { SelectDevice("设备 B"); }
    public void SelectDeviceC() { SelectDevice("设备 C"); }

    public void SelectTemperature() { SelectMetric("温度"); }
    public void SelectPressure() { SelectMetric("压力"); }

    // ===== 核心逻辑 =====

    private void SelectDevice(string deviceName)
    {
        currentDevice = deviceName;
        RefreshChart();
    }

    private void SelectMetric(string metric)
    {
        currentMetric = metric;
        RefreshChart();
    }

    /// <summary>刷新图表与统计信息</summary>
    public void RefreshChart()
    {
        if (lineChart == null)
        {
            return;
        }

        if (HistoryRecorder.Instance == null)
        {
            Debug.LogWarning("HistoryPanel：场景中找不到 HistoryRecorder！");
            return;
        }

        bool isPressure = (currentMetric == "压力");

        List<float> data = isPressure
            ? HistoryRecorder.Instance.GetPressureHistory(currentDevice)
            : HistoryRecorder.Instance.GetTemperatureHistory(currentDevice);

        float min = isPressure ? pressMin : tempMin;
        float max = isPressure ? pressMax : tempMax;

        lineChart.SetData(data, min, max);

        // 标题
        if (titleText != null)
        {
            titleText.text = $"{currentDevice} · {currentMetric}趋势";
        }

        // 统计信息
        if (statText != null)
        {
            if (data == null || data.Count == 0)
            {
                statText.text = "暂无数据（等待采样中...）";
                return;
            }

            float current = data[data.Count - 1];
            float highest = data[0];
            float lowest = data[0];

            for (int i = 1; i < data.Count; i++)
            {
                if (data[i] > highest) highest = data[i];
                if (data[i] < lowest) lowest = data[i];
            }

            string unit = isPressure ? "MPa" : "℃";
            string format = isPressure ? "F2" : "F1";

            statText.text =
                $"当前 {current.ToString(format)} {unit}    " +
                $"最高 {highest.ToString(format)} {unit}    " +
                $"最低 {lowest.ToString(format)} {unit}    " +
                $"采样 {data.Count} 点";
        }
    }
}