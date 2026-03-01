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

    private static string EscapeLike(string input)
        => input.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    [HttpGet]
    public async Task<ActionResult<PagedResult<RaidDto>>> GetRaids(
        [FromQuery] int? raidTypeId,
        [FromQuery] string? raidTypeName,

        // ✅ новые фильтры
        [FromQuery] string? guildName,
        [FromQuery] string? leaderName,

        // ✅ алиасы
        [FromQuery] string? guild,
        [FromQuery] string? leader,

        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 25;
        if (pageSize > 200) pageSize = 200;

        guildName = !string.IsNullOrWhiteSpace(guildName) ? guildName : guild;
        leaderName = !string.IsNullOrWhiteSpace(leaderName) ? leaderName : leader;

        var query = _context.Raids
            .AsNoTracking()
            .Include(r => r.RaidType)
            .AsQueryable();

        if (raidTypeId.HasValue)
            query = query.Where(r => r.RaidTypeId == raidTypeId.Value);

        if (!string.IsNullOrWhiteSpace(raidTypeName))
        {
            var rt = EscapeLike(raidTypeName.Trim());
            query = query.Where(r =>
                r.RaidType != null &&
                EF.Functions.ILike(r.RaidType.Name, $"%{rt}%", "\\"));
        }

        if (!string.IsNullOrWhiteSpace(guildName))
        {
            var g = EscapeLike(guildName.Trim());
            query = query.Where(r =>
                r.GuildName != null &&
                EF.Functions.ILike(r.GuildName, $"%{g}%", "\\"));
        }

        if (!string.IsNullOrWhiteSpace(leaderName))
        {
            var l = EscapeLike(leaderName.Trim());
            query = query.Where(r =>
                r.LeaderName != null &&
                EF.Functions.ILike(r.LeaderName, $"%{l}%", "\\"));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // мягко зажмём page, чтобы не улетать в пустоту
        var totalPages = pageSize <= 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages < 1) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var raids = await query
            .OrderByDescending(r => r.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RaidDto
            {
                Id = r.Id,
                RaidId = r.RaidId,
                RaidTypeName = r.RaidType != null ? r.RaidType.Name : "",
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
            // ❌ TotalPages не трогаем (у тебя read-only)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RaidDetailDto>> GetRaid(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return NotFound();

        var raid = await _context.Raids
            .AsNoTracking()
            .Include(r => r.RaidType)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (raid == null)
            return NotFound();

        var encounters = await _context.Encounters
            .AsNoTracking()
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
            RaidTypeName = raid.RaidType != null ? raid.RaidType.Name : "",
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
