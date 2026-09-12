using System.ComponentModel.DataAnnotations;

namespace IndustrialMonitorAPI.DTOs;

/// <summary>单个采样数据点</summary>
public class MetricPointRequest
{
    /// <summary>采样值</summary>
    public float Value { get; set; }

    /// <summary>采样时间（不传则使用服务端当前时间）</summary>
    public DateTime? Timestamp { get; set; }
}

/// <summary>
/// 指标历史批量上报请求。
/// Unity 端采集一批数据后一次性提交，减少请求次数。
/// </summary>
public class MetricBatchRequest
{
    [Required(ErrorMessage = "设备名称不能为空")]
    [MaxLength(50)]
    public string DeviceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "指标名称不能为空")]
    [MaxLength(20)]
    public string MetricName { get; set; } = string.Empty;

    [Required(ErrorMessage = "采样点不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个采样点")]
    public List<MetricPointRequest> Points { get; set; } = new();
}
