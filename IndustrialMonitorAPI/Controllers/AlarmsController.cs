using IndustrialMonitorAPI.Common;
using IndustrialMonitorAPI.DTOs;
using IndustrialMonitorAPI.Models;
using IndustrialMonitorAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialMonitorAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlarmsController : ControllerBase
{
    private readonly AlarmService _service;

    public AlarmsController(AlarmService service)
    {
        _service = service;
    }

    /// <summary>获取全部报警记录</summary>
    [HttpGet]
    [EndpointSummary("获取全部报警记录")]
    [EndpointDescription("返回所有报警记录，按发生时间倒序排列。")]
    [ProducesResponseType(typeof(ApiResponse<List<AlarmRecord>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<AlarmRecord>>>> GetAll()
    {
        var records = await _service.GetAllAsync();

        return Ok(new ApiResponse<List<AlarmRecord>>(200, "查询成功", records));
    }

    /// <summary>获取最近一条未恢复报警</summary>
    [HttpGet("recent")]
    [EndpointSummary("获取最近未恢复报警")]
    [EndpointDescription("返回最新一条状态为「未恢复」的报警，若无则返回 204。")]
    [ProducesResponseType(typeof(ApiResponse<AlarmRecord>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<ApiResponse<AlarmRecord>>> GetRecent()
    {
        var record = await _service.GetRecentUnresolvedAsync();

        if (record == null)
        {
            return NoContent();
        }

        return Ok(new ApiResponse<AlarmRecord>(200, "查询成功", record));
    }

    /// <summary>创建新报警</summary>
    [HttpPost]
    [EndpointSummary("创建报警")]
    [EndpointDescription("新增一条报警记录，状态默认为「未恢复」。")]
    [ProducesResponseType(typeof(ApiResponse<AlarmRecord>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AlarmRecord>>> Create(
        [FromBody] AlarmCreateRequest request)
    {
        var record = await _service.CreateAsync(request);

        return Created(
            $"/api/alarms/{record.Id}",
            new ApiResponse<AlarmRecord>(201, "报警创建成功", record));
    }

    /// <summary>恢复某设备的所有未恢复报警</summary>
    [HttpPut("recover/{deviceName}")]
    [EndpointSummary("恢复设备报警")]
    [EndpointDescription("将指定设备所有「未恢复」的报警标记为「已恢复」，并回填恢复时间。")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<int>>> Recover(string deviceName)
    {
        int count = await _service.RecoverByDeviceNameAsync(deviceName);

        return Ok(new ApiResponse<int>(200, $"已恢复 {count} 条报警", count));
    }
}
