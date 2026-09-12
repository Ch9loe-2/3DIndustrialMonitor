using System;

namespace IndustrialMonitorAPI.Models;

/// <summary>
/// 操作日志：记录"谁(用户) 在什么时间 对什么对象 做了什么操作"。
/// 用于审计与追责，是"用户权限与操作日志"功能的数据载体。
/// </summary>
public class OperationLog
{
    public int Id { get; set; }

    /// <summary>操作人（来自登录 token；未登录记为"匿名"）</summary>
    public string UserName { get; set; } = "匿名";

    /// <summary>操作类型：登录 / 故障模拟 / 恢复 / 离线 / 上线 / 报警 / 恢复报警 ...</summary>
    public string Action { get; set; } = "";

    /// <summary>操作对象：设备名 / 接口名</summary>
    public string Target { get; set; } = "";

    /// <summary>操作详情</summary>
    public string Detail { get; set; } = "";

    /// <summary>来源 IP</summary>
    public string IpAddress { get; set; } = "";

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
