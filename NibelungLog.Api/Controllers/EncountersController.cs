using Microsoft.AspNetCore.Mvc;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EncountersController : ControllerBase
{
    private readonly IEncounterQueryService _encounterQueryService;

    public EncountersController(IEncounterQueryService encounterQueryService)
    {
        _encounterQueryService = encounterQueryService;
    }

    [HttpGet("grouped-by-raid")]
    public async Task<ActionResult<List<EncounterGroupedDto>>> GetEncountersGroupedByRaid(CancellationToken cancellationToken = default)
    {
        var result = await _encounterQueryService.GetEncountersGroupedByRaidAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<EncounterListItemDto>>> GetEncountersList(CancellationToken cancellationToken = default)
    {
        var result = await _encounterQueryService.GetEncountersListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/raid")]
    public async Task<ActionResult<RaidDetailDto>> GetRaidByEncounterId(
        int id,
        CancellationToken cancellationToken = default)
    {
        var raid = await _encounterQueryService.GetRaidByEncounterIdAsync(id, cancellationToken);

        if (raid == null)
            return NotFound();

        return Ok(raid);
    }

    [HttpGet("{id:int}/players")]
    public async Task<ActionResult<List<PlayerEncounterDto>>> GetEncounterPlayers(
        int id,
        CancellationToken cancellationToken = default)
    {
        var players = await _encounterQueryService.GetEncounterPlayersAsync(id, cancellationToken);
        return Ok(players);
    }
}

