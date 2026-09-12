using IndustrialMonitorAPI.Common;
using IndustrialMonitorAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialMonitorAPI.Controllers;

/// <summary>
/// 身份认证：登录获取 token。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    public record LoginRequest(string Username, string Password);

    /// <summary>登录：校验用户名密码，成功返回 token</summary>
    [HttpPost("login")]
    [EndpointSummary("用户登录")]
    [EndpointDescription("用户名密码登录，成功返回 Bearer token。Demo 账号：admin/admin123、operator/operator123。")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrEmpty(req.Username) || string.IsNullOrEmpty(req.Password))
        {
            return BadRequest(new ApiResponse<object>(400, "用户名和密码不能为空", null));
        }

        if (!_auth.Validate(req.Username, req.Password))
        {
            return Ok(new ApiResponse<object>(401, "用户名或密码错误", null));
        }

        string token = _auth.IssueToken(req.Username);
        return Ok(new ApiResponse<object>(200, "登录成功", new { token, userName = req.Username }));
    }
}
