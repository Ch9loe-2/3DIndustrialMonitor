using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndustrialMonitorAPI.Data;
using IndustrialMonitorAPI.Models;

namespace IndustrialMonitorAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlarmsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AlarmsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>GET /api/alarms — 获取所有报警记录</summary>
    [HttpGet]
    public async Task<ActionResult<List<AlarmRecord>>> GetAll()
    {
        return await _db.AlarmRecords
            .OrderByDescending(a => a.Id)
            .ToListAsync();
    }

    /// <summary>GET /api/alarms/recent — 获取最近一条未恢复报警</summary>
    [HttpGet("recent")]
    public async Task<ActionResult<AlarmRecord>> GetRecent()
    {
        var alarm = await _db.AlarmRecords
            .Where(a => a.Status == "未恢复")
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync();

        if (alarm == null) return NoContent();
        return alarm;
    }

    /// <summary>POST /api/alarms — 添加报警</summary>
    [HttpPost]
    public async Task<ActionResult<AlarmRecord>> Create([FromBody] AlarmRecord record)
    {
        record.Time = DateTime.UtcNow;
        record.Status = "未恢复";

        _db.AlarmRecords.Add(record);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = record.Id }, record);
    }

    /// <summary>PUT /api/alarms/recover/{deviceName} — 恢复某设备所有未恢复报警</summary>
    [HttpPut("recover/{deviceName}")]
    public async Task<ActionResult<int>> Recover(string deviceName)
    {
        var alarms = await _db.AlarmRecords
            .Where(a => a.DeviceName == deviceName && a.Status == "未恢复")
            .ToListAsync();

        foreach (var alarm in alarms)
        {
            alarm.Status = "已恢复";
            alarm.RecoverTime = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(alarms.Count);
    }
}