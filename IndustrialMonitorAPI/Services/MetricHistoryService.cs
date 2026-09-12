using IndustrialMonitorAPI.Data;
using IndustrialMonitorAPI.DTOs;
using IndustrialMonitorAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace IndustrialMonitorAPI.Services;

/// <summary>
/// 设备指标历史数据服务：负责采样数据的持久化与查询。
/// </summary>
public class MetricHistoryService
{
    private readonly AppDbContext _db;
    private readonly ILogger<MetricHistoryService> _logger;

    public MetricHistoryService(AppDbContext db, ILogger<MetricHistoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>批量保存采样数据，返回保存条数</summary>
    public async Task<int> SaveBatchAsync(MetricBatchRequest request)
    {
        var entities = request.Points
            .Select(p => new DeviceMetricHistory
            {
                DeviceName = request.DeviceName,
                MetricName = request.MetricName,
                Value = p.Value,
                Timestamp = p.Timestamp ?? DateTime.UtcNow
            })
            .ToList();

        _db.DeviceMetricHistories.AddRange(entities);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "历史数据已保存：设备={Device}，指标={Metric}，共 {Count} 个采样点",
            request.DeviceName, request.MetricName, entities.Count);

        return entities.Count;
    }

    /// <summary>
    /// 查询指定设备与指标在最近 N 分钟内的历史数据（按时间正序）。
    /// </summary>
    public async Task<List<DeviceMetricHistory>> GetHistoryAsync(
        string deviceName,
        string metricName,
        int minutes = 30)
    {
        var since = DateTime.UtcNow.AddMinutes(-minutes);

        return await _db.DeviceMetricHistories
            .Where(h => h.DeviceName == deviceName
                     && h.MetricName == metricName
                     && h.Timestamp >= since)
            .OrderBy(h => h.Timestamp)
            .ToListAsync();
    }

    /// <summary>清理超过保留天数的历史数据，避免数据库无限增长</summary>
    public async Task<int> CleanupOldDataAsync(int retentionDays = 7)
    {
        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

        var oldData = await _db.DeviceMetricHistories
            .Where(h => h.Timestamp < cutoff)
            .ToListAsync();

        if (oldData.Count == 0)
        {
            return 0;
        }

        _db.DeviceMetricHistories.RemoveRange(oldData);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "历史数据清理完成：删除 {Count} 条超过 {Days} 天的记录",
            oldData.Count, retentionDays);

        return oldData.Count;
    }
}
