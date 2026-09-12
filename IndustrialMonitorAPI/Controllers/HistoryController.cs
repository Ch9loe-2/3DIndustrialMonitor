using IndustrialMonitorAPI.Common;
using IndustrialMonitorAPI.DTOs;
using IndustrialMonitorAPI.Models;
using IndustrialMonitorAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialMonitorAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly MetricHistoryService _service;

    public HistoryController(MetricHistoryService service)
    {
        _service = service;
    }

    /// <summary>批量上报采样数据</summary>
    [HttpPost]
    [EndpointSummary("批量上报历史数据")]
    [EndpointDescription("接收 Unity 端采集的一批采样点并持久化到数据库。")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> SaveBatch(
        [FromBody] MetricBatchRequest request)
    {
        int count = await _service.SaveBatchAsync(request);

        return Ok(new ApiResponse<int>(200, $"已保存 {count} 个采样点", count));
    }

    /// <summary>查询历史数据</summary>
    [HttpGet("{deviceName}/{metricName}")]
    [EndpointSummary("查询设备指标历史")]
    [EndpointDescription("查询指定设备在最近 N 分钟内某个指标的采样数据，按时间正序返回。")]
    [ProducesResponseType(typeof(ApiResponse<List<DeviceMetricHistory>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<DeviceMetricHistory>>>> GetHistory(
        string deviceName,
        string metricName,
        [FromQuery] int minutes = 30)
    {
        // 限制查询范围，避免一次拉取过多数据
        if (minutes < 1) minutes = 1;
        if (minutes > 1440) minutes = 1440;

        var data = await _service.GetHistoryAsync(deviceName, metricName, minutes);

        return Ok(new ApiResponse<List<DeviceMetricHistory>>(200, "查询成功", data));
    }

    /// <summary>清理过期历史数据</summary>
    [HttpDelete("cleanup")]
    [EndpointSummary("清理过期历史数据")]
    [EndpointDescription("删除超过指定天数的历史采样记录，避免数据库无限增长。")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<int>>> Cleanup([FromQuery] int days = 7)
    {
        if (days < 1) days = 1;

        int count = await _service.CleanupOldDataAsync(days);

        return Ok(new ApiResponse<int>(200, $"已清理 {count} 条历史数据", count));
    }
}
