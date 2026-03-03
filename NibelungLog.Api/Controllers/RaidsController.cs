using Microsoft.AspNetCore.Mvc;
using NibelungLog.Api.Validators;
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
        [FromQuery] GetRaidsQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        query ??= new GetRaidsQuery();

        var guildName = !string.IsNullOrWhiteSpace(query.GuildName) 
            ? query.GuildName 
            : query.Guild;
        
        var leaderName = !string.IsNullOrWhiteSpace(query.LeaderName) 
            ? query.LeaderName 
            : query.Leader;

        var result = await _raidQueryService.GetRaidsAsync(
            query.RaidTypeId, query.RaidTypeName, guildName, leaderName,
            query.Page, query.PageSize, cancellationToken);
        
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RaidDetailDto>> GetRaid(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return BadRequest("Id must be greater than 0");

        var raid = await _raidQueryService.GetRaidByIdAsync(id, cancellationToken);

        if (raid == null)
            return NotFound();

        return Ok(raid);
    }
}
