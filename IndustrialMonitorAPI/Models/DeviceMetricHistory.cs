using System.ComponentModel.DataAnnotations;

namespace IndustrialMonitorAPI.Models;

/// <summary>
/// 设备指标历史数据点。
/// 用于持久化设备运行数据（温度/压力等）的采样记录，
/// 供前端历史趋势图查询绘制。
/// </summary>
public class DeviceMetricHistory
{
    [Key]
    public int Id { get; set; }

    /// <summary>设备名称</summary>
    [Required]
    [MaxLength(50)]
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>指标名称：温度 / 压力 / 转速</summary>
    [Required]
    [MaxLength(20)]
    public string MetricName { get; set; } = string.Empty;

    /// <summary>采样值</summary>
    public float Value { get; set; }

    /// <summary>采样时间</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
