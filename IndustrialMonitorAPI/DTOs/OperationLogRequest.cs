using System.ComponentModel.DataAnnotations;

namespace IndustrialMonitorAPI.DTOs;

/// <summary>
/// 新增操作日志请求。UserName / Token 由后端从 Authorization 头解析，前端不必传。
/// </summary>
public class OperationLogRequest
{
    [Required(ErrorMessage = "操作类型不能为空")]
    public string Action { get; set; } = "";

    public string Target { get; set; } = "";

    public string Detail { get; set; } = "";
}
