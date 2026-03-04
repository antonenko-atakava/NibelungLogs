using Microsoft.AspNetCore.Mvc;
using NibelungLog.Api.Validators;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PlayersController : ControllerBase
{
    private readonly IPlayerQueryService _playerQueryService;

    public PlayersController(IPlayerQueryService playerQueryService)
    {
        _playerQueryService = playerQueryService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PlayerDto>>> GetPlayers(
        [FromQuery] GetPlayersQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        query ??= new GetPlayersQuery();

        var result = await _playerQueryService.GetPlayersAsync(
            query.Search, query.Role, query.Race, query.Faction, query.ItemLevelMin, query.ItemLevelMax, 
            query.SortField, query.SortDirection, query.Page, query.PageSize, cancellationToken);
        
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlayerDetailDto>> GetPlayer(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return BadRequest("Id must be greater than 0");

        var player = await _playerQueryService.GetPlayerByIdAsync(id, cancellationToken);

        if (player == null)
            return NotFound();

        return Ok(player);
    }

    [HttpGet("by-class")]
    public async Task<ActionResult<PagedResult<PlayerDto>>> GetPlayersByClass(
        [FromQuery] GetPlayersByClassQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _playerQueryService.GetPlayersByClassAsync(
            query.CharacterClass, query.Spec, query.EncounterEntry, query.EncounterName,
            query.Role, query.Search, query.Page, query.PageSize, cancellationToken);
        
        return Ok(result);
    }

    [HttpGet("by-encounter")]
    public async Task<ActionResult<PagedResult<PlayerDto>>> GetPlayersByEncounter(
        [FromQuery] GetPlayersByEncounterQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        query ??= new GetPlayersByEncounterQuery();

        var result = await _playerQueryService.GetPlayersByEncounterAsync(
            query.EncounterName, query.EncounterEntry, query.Search,
            query.CharacterClass, query.Role, query.Page, query.PageSize, cancellationToken);
        
        return Ok(result);
    }

    [HttpGet("{id:int}/extended")]
    public async Task<ActionResult<PlayerExtendedDetailDto>> GetPlayerExtended(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return BadRequest("Id must be greater than 0");

        var player = await _playerQueryService.GetPlayerExtendedDetailAsync(id, cancellationToken);

        if (player == null)
            return NotFound();

        return Ok(player);
    }

    [HttpGet("{id:int}/encounters")]
    public async Task<ActionResult<PagedResult<PlayerEncounterDetailDto>>> GetPlayerEncounters(
        int id,
        [FromQuery] string? encounterName = null,
        [FromQuery] string? specName = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? success = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return BadRequest("Id must be greater than 0");

        if (page <= 0)
            return BadRequest("Page must be greater than 0");

        if (pageSize <= 0 || pageSize > 100)
            return BadRequest("PageSize must be between 1 and 100");

        var result = await _playerQueryService.GetPlayerEncountersAsync(
            id, encounterName, specName, role, success, page, pageSize, cancellationToken);
        
        return Ok(result);
    }

    [HttpGet("{id:int}/encounters/{encounterEntry}/timeline")]
    public async Task<ActionResult<List<PlayerEncounterTimelineDto>>> GetPlayerEncounterTimeline(
        int id,
        string encounterEntry,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return BadRequest("Id must be greater than 0");

        if (string.IsNullOrWhiteSpace(encounterEntry))
            return BadRequest("EncounterEntry is required");

        var result = await _playerQueryService.GetPlayerEncounterTimelineAsync(id, encounterEntry, cancellationToken);
        
        return Ok(result);
    }

    [HttpGet("{id:int}/encounters/unique")]
    public async Task<ActionResult<List<EncounterListItemDto>>> GetPlayerUniqueEncounters(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return BadRequest("Id must be greater than 0");

        var result = await _playerQueryService.GetPlayerUniqueEncountersAsync(id, cancellationToken);
        
        return Ok(result);
    }

    [HttpGet("{id:int}/spec-comparison")]
    public async Task<ActionResult<PlayerSpecComparisonDto>> GetPlayerSpecComparison(
        int id,
        [FromQuery] string specName,
        [FromQuery] bool useAverageDps = true,
        [FromQuery] int topCount = 20,
        [FromQuery] int? raidTypeId = null,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return BadRequest("Id must be greater than 0");

        if (string.IsNullOrWhiteSpace(specName))
            return BadRequest("SpecName is required");

        if (topCount <= 0 || topCount > 100)
            return BadRequest("TopCount must be between 1 and 100");

        var result = await _playerQueryService.GetPlayerSpecComparisonAsync(id, specName, useAverageDps, topCount, raidTypeId, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
