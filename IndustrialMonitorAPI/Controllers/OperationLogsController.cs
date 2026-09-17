using IndustrialMonitorAPI.Common;
using IndustrialMonitorAPI.DTOs;
using IndustrialMonitorAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialMonitorAPI.Controllers;

/// <summary>
/// 操作日志：记录与查询用户操作（需登录才能写入，见 Program.cs 中间件）。
/// </summary>
[ApiController]
[Route("api/logs")]
public class OperationLogsController : ControllerBase
{
    private readonly OperationLogService _svc;
    private readonly AuthService _auth;

    public OperationLogsController(OperationLogService svc, AuthService auth)
    {
        _svc = svc;
        _auth = auth;
    }

    /// <summary>查询最近操作日志</summary>
    [HttpGet]
    [EndpointSummary("查询操作日志")]
    [EndpointDescription("按时间倒序返回最近的操作日志。")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 200)
    {
        if (limit < 1) limit = 1;
        if (limit > 1000) limit = 1000;
        var list = await _svc.GetRecentAsync(limit);
        return Ok(new ApiResponse<object>(200, "查询成功", list));
    }

    /// <summary>新增操作日志（需登录：中间件要求 Bearer token）</summary>
    [HttpPost]
    [EndpointSummary("记录操作日志")]
    [EndpointDescription("写入一条操作日志。用户身份由 Authorization: Bearer &lt;token&gt; 解析；未登录请求被中间件拦截返回 401。")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Add([FromBody] OperationLogRequest req)
    {
        // 从 Authorization 头解析操作人（中间件已确保存在 Bearer）
        string user = "匿名";
        string authHeader = Request.Headers["Authorization"].ToString();
        if (authHeader.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase))
        {
            string? u = _auth.GetUser(authHeader.Substring(7).Trim());
            if (u != null) user = u;
        }

        var log = new Models.OperationLog
        {
            UserName = user,
            Action = req.Action,
            Target = req.Target,
            Detail = req.Detail,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
        };

        await _svc.AddAsync(log);
        return Ok(new ApiResponse<object>(200, "记录成功", null));
    }
}
