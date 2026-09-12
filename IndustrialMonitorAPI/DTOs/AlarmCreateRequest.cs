using System.ComponentModel.DataAnnotations;

namespace IndustrialMonitorAPI.DTOs;

/// <summary>
/// 报警创建请求模型。
/// </summary>
public class AlarmCreateRequest
{
    [Required(ErrorMessage = "设备名称不能为空")]
    [MaxLength(50, ErrorMessage = "设备名称不能超过 50 个字符")]
    public string DeviceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "报警类型不能为空")]
    [MaxLength(50, ErrorMessage = "报警类型不能超过 50 个字符")]
    public string AlarmType { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "报警等级不能超过 20 个字符")]
    public string AlarmLevel { get; set; } = "警告";

    [MaxLength(500, ErrorMessage = "报警描述不能超过 500 个字符")]
    public string AlarmMessage { get; set; } = string.Empty;
}
