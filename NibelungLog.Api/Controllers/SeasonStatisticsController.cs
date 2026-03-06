using Microsoft.AspNetCore.Mvc;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SeasonStatisticsController : ControllerBase
{
    private readonly ISeasonStatisticsService _seasonStatisticsService;

    public SeasonStatisticsController(ISeasonStatisticsService seasonStatisticsService)
    {
        _seasonStatisticsService = seasonStatisticsService;
    }

    [HttpGet("classes")]
    public async Task<ActionResult<List<SeasonClassStatisticsDto>>> GetSeasonClassStatistics(
        CancellationToken cancellationToken = default)
    {
        var result = await _seasonStatisticsService.GetSeasonClassStatisticsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("specs")]
    public async Task<ActionResult<List<SeasonSpecStatisticsDto>>> GetSeasonSpecStatistics(
        CancellationToken cancellationToken = default)
    {
        var result = await _seasonStatisticsService.GetSeasonSpecStatisticsAsync(cancellationToken);
        return Ok(result);
    }
}
