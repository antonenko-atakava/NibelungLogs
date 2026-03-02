using Microsoft.AspNetCore.Mvc;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RaidsController : ControllerBase
{
    private readonly IRaidQueryService _raidQueryService;

    public RaidsController(IRaidQueryService raidQueryService)
    {
        _raidQueryService = raidQueryService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<RaidDto>>> GetRaids(
        [FromQuery] int? raidTypeId,
        [FromQuery] string? raidTypeName,
        [FromQuery] string? guildName,
        [FromQuery] string? leaderName,
        [FromQuery] string? guild,
        [FromQuery] string? leader,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        guildName = !string.IsNullOrWhiteSpace(guildName) ? guildName : guild;
        leaderName = !string.IsNullOrWhiteSpace(leaderName) ? leaderName : leader;

        var result = await _raidQueryService.GetRaidsAsync(
            raidTypeId, raidTypeName, guildName, leaderName, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RaidDetailDto>> GetRaid(int id, CancellationToken cancellationToken = default)
    {
        var raid = await _raidQueryService.GetRaidByIdAsync(id, cancellationToken);

        if (raid == null)
            return NotFound();

        return Ok(raid);
    }
}
