namespace IndustrialMonitorAPI.Common;

/// <summary>
/// 统一 API 响应格式。
/// 所有接口返回 { code, message, data }，便于前端统一处理。
/// </summary>
public class ApiResponse<T>
{
    public int Code { get; set; }

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }

    public ApiResponse(int code, string message, T? data)
    {
        Code = code;
        Message = message;
        Data = data;
    }
}
