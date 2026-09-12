using IndustrialMonitorAPI.Data;
using IndustrialMonitorAPI.DTOs;
using IndustrialMonitorAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace IndustrialMonitorAPI.Services;

/// <summary>
/// 设备业务服务：封装设备数据查询与更新逻辑。
/// </summary>
public class DeviceService
{
    private static readonly string[] ValidStatuses = { "正常", "警告", "故障", "离线" };

    private readonly AppDbContext _db;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(AppDbContext db, ILogger<DeviceService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>获取全部设备</summary>
    public async Task<List<Device>> GetAllAsync()
    {
        return await _db.Devices.ToListAsync();
    }

    /// <summary>根据 ID 获取设备</summary>
    public async Task<Device?> GetByIdAsync(int id)
    {
        return await _db.Devices.FindAsync(id);
    }

    /// <summary>更新设备数据与状态</summary>
    public async Task<Device?> UpdateAsync(int id, DeviceUpdateRequest request)
    {
        if (!ValidStatuses.Contains(request.Status))
        {
            throw new ArgumentException(
                $"无效的设备状态：{request.Status}（可选值：{string.Join("/", ValidStatuses)}）");
        }

        var device = await _db.Devices.FindAsync(id);

        if (device == null)
        {
            _logger.LogWarning("更新设备失败：未找到设备 Id={Id}", id);
            return null;
        }

        device.Temperature = request.Temperature;
        device.Pressure = request.Pressure;
        device.Rpm = request.Rpm;
        device.Runtime = request.Runtime;
        device.Status = request.Status;
        device.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "设备数据更新成功：Id={Id}，设备={Name}，状态={Status}，温度={Temp}℃，压力={Pressure}MPa",
            device.Id, device.DeviceName, device.Status, device.Temperature, device.Pressure);

        return device;
    }
}
