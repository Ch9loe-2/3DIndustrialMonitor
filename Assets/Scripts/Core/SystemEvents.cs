using System;

/// <summary>
/// 系统级事件中心：解耦各模块之间的状态通知。
/// 例如：设备状态变化 → 概览自动刷新、报警栏刷新。
/// 这样就不需要每帧轮询，属于面试可以讲的技术点（观察者模式）。
/// </summary>
public static class SystemEvents
{
    /// <summary>设备状态发生变化（正常/警告/故障）。</summary>
    public static event Action DeviceStatusChanged;

    public static void RaiseDeviceStatusChanged()
    {
        if (DeviceStatusChanged != null)
        {
            DeviceStatusChanged();
        }
    }
}
