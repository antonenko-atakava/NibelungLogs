using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NibelungLog.Api.Dto;
using NibelungLog.Data;

namespace NibelungLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RaidsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RaidsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RaidDto>>> GetRaids(
        [FromQuery] int? raidTypeId,
        [FromQuery] string? raidTypeName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Raids
            .Include(r => r.RaidType)
            .AsQueryable();

        if (raidTypeId.HasValue)
            query = query.Where(r => r.RaidTypeId == raidTypeId.Value);

        if (!string.IsNullOrWhiteSpace(raidTypeName))
            query = query.Where(r => r.RaidType.Name.Contains(raidTypeName));

        var totalCount = await query.CountAsync(cancellationToken);

        var raids = await query
            .OrderByDescending(r => r.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RaidDto
            {
                Id = r.Id,
                RaidId = r.RaidId,
                RaidTypeName = r.RaidType.Name,
                GuildName = r.GuildName,
                LeaderName = r.LeaderName,
                StartTime = r.StartTime,
                TotalTime = r.TotalTime,
                TotalDamage = r.TotalDamage,
                TotalHealing = r.TotalHealing,
                Wipes = r.Wipes,
                CompletedBosses = r.CompletedBosses,
                TotalBosses = r.TotalBosses
            })
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<RaidDto>
        {
            Data = raids,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RaidDetailDto>> GetRaid(int id, CancellationToken cancellationToken = default)
    {
        var raid = await _context.Raids
            .Include(r => r.RaidType)
            .Include(r => r.Encounters)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (raid == null)
            return NotFound();

        var encounters = await _context.Encounters
            .Where(e => e.RaidId == id)
            .OrderBy(e => e.StartTime)
            .Select(e => new EncounterDto
            {
                Id = e.Id,
                EncounterEntry = e.EncounterEntry,
                EncounterName = e.EncounterName,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Success = e.Success,
                TotalDamage = e.TotalDamage,
                TotalHealing = e.TotalHealing,
                Tanks = e.Tanks,
                Healers = e.Healers,
                DamageDealers = e.DamageDealers
            })
            .ToListAsync(cancellationToken);

        return Ok(new RaidDetailDto
        {
            Id = raid.Id,
            RaidId = raid.RaidId,
            RaidTypeName = raid.RaidType.Name,
            GuildName = raid.GuildName,
            LeaderName = raid.LeaderName,
            StartTime = raid.StartTime,
            TotalTime = raid.TotalTime,
            TotalDamage = raid.TotalDamage,
            TotalHealing = raid.TotalHealing,
            Wipes = raid.Wipes,
            CompletedBosses = raid.CompletedBosses,
            TotalBosses = raid.TotalBosses,
            Encounters = encounters
        });
    }
}

