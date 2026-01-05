using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NibelungLog.Api.Dto;
using NibelungLog.Data;

namespace NibelungLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RaidTypesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RaidTypesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RaidTypeDto>>> GetRaidTypes(CancellationToken cancellationToken = default)
    {
        var raidTypes = await _context.RaidTypes
            .OrderBy(rt => rt.Name)
            .Select(rt => new RaidTypeDto
            {
                Id = rt.Id,
                Name = rt.Name,
                Map = rt.Map,
                Difficulty = rt.Difficulty,
                InstanceType = rt.InstanceType
            })
            .ToListAsync(cancellationToken);

        return Ok(raidTypes);
    }
}

