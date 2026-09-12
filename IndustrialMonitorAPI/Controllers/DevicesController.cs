using IndustrialMonitorAPI.Common;
using IndustrialMonitorAPI.DTOs;
using IndustrialMonitorAPI.Models;
using IndustrialMonitorAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialMonitorAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly DeviceService _service;

    public DevicesController(DeviceService service)
    {
        _service = service;
    }

    /// <summary>获取全部设备</summary>
    [HttpGet]
    [EndpointSummary("获取全部设备")]
    [EndpointDescription("返回系统中所有设备的基础信息与实时运行数据。")]
    [ProducesResponseType(typeof(ApiResponse<List<Device>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<Device>>>> GetAll()
    {
        var devices = await _service.GetAllAsync();

        return Ok(new ApiResponse<List<Device>>(200, "查询成功", devices));
    }

    /// <summary>根据 ID 获取设备</summary>
    [HttpGet("{id:int}")]
    [EndpointSummary("根据 ID 获取设备")]
    [EndpointDescription("根据设备 ID 查询指定设备的详细信息。")]
    [ProducesResponseType(typeof(ApiResponse<Device>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Device>>> GetById(int id)
    {
        var device = await _service.GetByIdAsync(id);

        if (device == null)
        {
            return NotFound(new ApiResponse<object>(404, "设备不存在", null));
        }

        return Ok(new ApiResponse<Device>(200, "查询成功", device));
    }

    /// <summary>更新设备数据与状态</summary>
    [HttpPut("{id:int}")]
    [EndpointSummary("更新设备数据")]
    [EndpointDescription("更新指定设备的温度、压力、转速、运行时长与状态。")]
    [ProducesResponseType(typeof(ApiResponse<Device>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Device>>> Update(
        int id,
        [FromBody] DeviceUpdateRequest request)
    {
        try
        {
            var device = await _service.UpdateAsync(id, request);

            if (device == null)
            {
                return NotFound(new ApiResponse<object>(404, "设备不存在", null));
            }

            return Ok(new ApiResponse<Device>(200, "更新成功", device));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object>(400, ex.Message, null));
        }
    }
}
