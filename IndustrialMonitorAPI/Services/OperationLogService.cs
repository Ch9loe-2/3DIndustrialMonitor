using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndustrialMonitorAPI.Data;
using IndustrialMonitorAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace IndustrialMonitorAPI.Services;

/// <summary>
/// 操作日志业务逻辑。
/// </summary>
public class OperationLogService
{
    private readonly AppDbContext _db;

    public OperationLogService(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(OperationLog log)
    {
        _db.OperationLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<List<OperationLog>> GetRecentAsync(int limit = 200)
    {
        return await _db.OperationLogs
            .OrderByDescending(x => x.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
}
