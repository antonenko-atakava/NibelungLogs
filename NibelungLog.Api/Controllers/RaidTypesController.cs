using Microsoft.AspNetCore.Mvc;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RaidTypesController : ControllerBase
{
    private readonly IRaidTypeQueryService _raidTypeQueryService;

    public RaidTypesController(IRaidTypeQueryService raidTypeQueryService)
    {
        _raidTypeQueryService = raidTypeQueryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RaidTypeDto>>> GetRaidTypes(CancellationToken cancellationToken = default)
    {
        var raidTypes = await _raidTypeQueryService.GetRaidTypesAsync(cancellationToken);
        return Ok(raidTypes);
    }
}

