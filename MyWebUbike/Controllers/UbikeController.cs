using Microsoft.AspNetCore.Mvc;
using MyWeb.DTOs;
using MyWeb.Services;

namespace MyWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UbikeController : ControllerBase
{
    private readonly IUbikeService _ubikeService;
    private readonly ILogger<UbikeController> _logger;

    public UbikeController(IUbikeService ubikeService, ILogger<UbikeController> logger)
    {
        _ubikeService = ubikeService;
        _logger = logger;
    }

    [HttpGet("areaQry")]
    public async Task<ActionResult<UbikeAreaQueryResponseDto>> AreaQry([FromQuery] string area)
    {
        if (string.IsNullOrWhiteSpace(area))
        {
            return BadRequest(new { error = "行政区域参数不能为空" });
        }

        try
        {
            var result = await _ubikeService.AreaQryAsync(area);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询 YouBike 数据时发生错误，行政区域: {Area}", area);
            return StatusCode(500, new { error = "查询数据时发生错误", message = ex.Message });
        }
    }
}

