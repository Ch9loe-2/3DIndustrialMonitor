using System;
using System.Collections.Generic;

namespace IndustrialMonitorAPI.Services;

/// <summary>
/// 简易身份认证服务（Demo 用）。
/// 生产环境应改为：用户存数据库 + 密码哈希（ASP.NET Core Identity / BCrypt），
/// Token 用 JWT 并校验签名与过期时间。此处用内存字典仅用于演示"用户权限"。
/// </summary>
public class AuthService
{
    // Demo 账号（明文仅用于演示，生产必须替换并加密）
    private static readonly Dictionary<string, string> DemoUsers = new()
    {
        { "admin", "admin123" },
        { "operator", "operator123" }
    };

    // token -> 用户名（内存态，服务重启即失效，符合 Demo 定位）
    private static readonly Dictionary<string, string> Tokens = new();

    /// <summary>校验用户名密码</summary>
    public bool Validate(string? user, string? pass)
    {
        return !string.IsNullOrEmpty(user)
            && DemoUsers.TryGetValue(user, out var pwd)
            && pwd == pass;
    }

    /// <summary>签发 token</summary>
    public string IssueToken(string user)
    {
        string token = Guid.NewGuid().ToString("N");
        Tokens[token] = user;
        return token;
    }

    /// <summary>按 token 取用户名；无效返回 null</summary>
    public string? GetUser(string? token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        return Tokens.TryGetValue(token, out var user) ? user : null;
    }
}
