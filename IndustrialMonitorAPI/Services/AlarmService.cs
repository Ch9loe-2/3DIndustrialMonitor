using IndustrialMonitorAPI.Data;
using IndustrialMonitorAPI.DTOs;
using IndustrialMonitorAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace IndustrialMonitorAPI.Services;

/// <summary>
/// 报警业务服务：封装报警查询、创建与恢复逻辑。
/// </summary>
public class AlarmService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AlarmService> _logger;

    public AlarmService(AppDbContext db, ILogger<AlarmService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>获取全部报警（按时间倒序）</summary>
    public async Task<List<AlarmRecord>> GetAllAsync()
    {
        return await _db.AlarmRecords
            .OrderByDescending(a => a.Id)
            .ToListAsync();
    }

    /// <summary>获取最近一条未恢复报警</summary>
    public async Task<AlarmRecord?> GetRecentUnresolvedAsync()
    {
        return await _db.AlarmRecords
            .Where(a => a.Status == "未恢复")
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync();
    }

    /// <summary>创建新报警</summary>
    public async Task<AlarmRecord> CreateAsync(AlarmCreateRequest request)
    {
        var record = new AlarmRecord
        {
            DeviceName = request.DeviceName,
            AlarmType = request.AlarmType,
            AlarmLevel = request.AlarmLevel,
            AlarmMessage = request.AlarmMessage,
            Time = DateTime.UtcNow,
            Status = "未恢复"
        };

        _db.AlarmRecords.Add(record);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "报警创建成功：Id={Id}，设备={Device}，类型={Type}，等级={Level}",
            record.Id, record.DeviceName, record.AlarmType, record.AlarmLevel);

        return record;
    }

    /// <summary>恢复某设备的所有未恢复报警，返回恢复条数</summary>
    public async Task<int> RecoverByDeviceNameAsync(string deviceName)
    {
        var alarms = await _db.AlarmRecords
            .Where(a => a.DeviceName == deviceName && a.Status == "未恢复")
            .ToListAsync();

        if (alarms.Count == 0)
        {
            _logger.LogInformation("恢复报警：设备 {Device} 没有未恢复的报警", deviceName);
            return 0;
        }

        foreach (var alarm in alarms)
        {
            alarm.Status = "已恢复";
            alarm.RecoverTime = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "报警恢复成功：设备={Device}，共恢复 {Count} 条",
            deviceName, alarms.Count);

        return alarms.Count;
    }
}
