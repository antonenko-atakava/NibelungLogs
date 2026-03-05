using Microsoft.AspNetCore.Mvc;
using NibelungLog.Api.Validators;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class GuildsController : ControllerBase
{
    private readonly IGuildQueryService _guildQueryService;

    public GuildsController(IGuildQueryService guildQueryService)
    {
        _guildQueryService = guildQueryService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<GuildDto>>> GetGuilds(
        [FromQuery] GetGuildsQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        query ??= new GetGuildsQuery();

        var result = await _guildQueryService.GetGuildsAsync(
            query.Search, query.SortField, query.SortDirection,
            query.Page, query.PageSize, cancellationToken);
        
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GuildDetailDto>> GetGuildById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var guild = await _guildQueryService.GetGuildByIdAsync(id, cancellationToken);
        
        if (guild == null)
            return NotFound();
        
        return Ok(guild);
    }

    [HttpGet("{id:int}/members")]
    public async Task<ActionResult<PagedResult<GuildMemberDto>>> GetGuildMembers(
        int id,
        [FromQuery] GetGuildMembersQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        query ??= new GetGuildMembersQuery();

        var result = await _guildQueryService.GetGuildMembersAsync(
            id, query.Search, query.Role, query.CharacterClass, query.Spec,
            query.ItemLevelMin, query.ItemLevelMax, query.RaidTypeId, query.EncounterName,
            query.SortField, query.SortDirection, query.Page, query.PageSize, cancellationToken);
        
        return Ok(result);
    }

    [HttpGet("{id:int}/statistics")]
    public async Task<ActionResult<GuildStatisticsDto>> GetGuildStatistics(
        int id,
        CancellationToken cancellationToken = default)
    {
        var result = await _guildQueryService.GetGuildStatisticsAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/encounters")]
    public async Task<ActionResult<List<EncounterListItemDto>>> GetGuildUniqueEncounters(
        int id,
        [FromQuery] int? raidTypeId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _guildQueryService.GetGuildUniqueEncountersAsync(id, raidTypeId, cancellationToken);
        return Ok(result);
    }
}
