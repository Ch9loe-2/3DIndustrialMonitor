using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndustrialMonitorAPI.Data;
using IndustrialMonitorAPI.Models;

namespace IndustrialMonitorAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly AppDbContext _db;

    public DevicesController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>GET /api/devices — 获取所有设备</summary>
    [HttpGet]
    public async Task<ActionResult<List<Device>>> GetAll()
    {
        return await _db.Devices.ToListAsync();
    }

    /// <summary>GET /api/devices/{id} — 获取单个设备</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Device>> GetById(int id)
    {
        var device = await _db.Devices.FindAsync(id);
        if (device == null) return NotFound();
        return device;
    }

    /// <summary>PUT /api/devices/{id} — 更新设备数据</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Device>> Update(int id, [FromBody] Device updated)
    {
        var device = await _db.Devices.FindAsync(id);
        if (device == null) return NotFound();

        device.Temperature = updated.Temperature;
        device.Pressure = updated.Pressure;
        device.Rpm = updated.Rpm;
        device.Runtime = updated.Runtime;
        device.Status = updated.Status;
        device.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return device;
    }
}