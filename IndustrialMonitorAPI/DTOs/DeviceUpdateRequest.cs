using System.ComponentModel.DataAnnotations;

namespace IndustrialMonitorAPI.DTOs;

/// <summary>
/// 设备数据更新请求模型。
/// 使用独立的 DTO 接收客户端数据，避免直接暴露数据库实体。
/// </summary>
public class DeviceUpdateRequest
{
    [Range(-50f, 500f, ErrorMessage = "温度必须在 -50 ~ 500 ℃ 之间")]
    public float Temperature { get; set; }

    [Range(0f, 100f, ErrorMessage = "压力必须在 0 ~ 100 MPa 之间")]
    public float Pressure { get; set; }

    [Range(0, 100000, ErrorMessage = "转速必须在 0 ~ 100000 RPM 之间")]
    public int Rpm { get; set; }

    [Range(0f, float.MaxValue, ErrorMessage = "运行时长不能为负")]
    public float Runtime { get; set; }

    [Required(ErrorMessage = "设备状态不能为空")]
    public string Status { get; set; } = "正常";
}
