using Microsoft.AspNetCore.Mvc;
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
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await _playerQueryService.GetPlayersAsync(search, role, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerDetailDto>> GetPlayer(int id, CancellationToken cancellationToken = default)
    {
        var player = await _playerQueryService.GetPlayerByIdAsync(id, cancellationToken);

        if (player == null)
            return NotFound();

        return Ok(player);
    }

    [HttpGet("by-class")]
    public async Task<ActionResult<PagedResult<PlayerDto>>> GetPlayersByClass(
        [FromQuery] string characterClass,
        [FromQuery] string? spec,
        [FromQuery] string? encounterEntry,
        [FromQuery] string? encounterName,
        [FromQuery] string? role,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(characterClass))
            return BadRequest("characterClass parameter is required");

        var result = await _playerQueryService.GetPlayersByClassAsync(
            characterClass, spec, encounterEntry, encounterName, role, search, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-encounter")]
    public async Task<ActionResult<PagedResult<PlayerDto>>> GetPlayersByEncounter(
        [FromQuery] string? encounterName,
        [FromQuery] string? encounterEntry,
        [FromQuery] string? search,
        [FromQuery] string? characterClass,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await _playerQueryService.GetPlayersByEncounterAsync(
            encounterName, encounterEntry, search, characterClass, role, page, pageSize, cancellationToken);
        return Ok(result);
    }
}

