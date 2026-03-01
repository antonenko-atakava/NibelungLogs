using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NibelungLog.Api.Dto;
using NibelungLog.Data;

namespace NibelungLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EncountersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EncountersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("grouped-by-raid")]
    public async Task<ActionResult<IEnumerable<object>>> GetEncountersGroupedByRaid(CancellationToken cancellationToken = default)
    {
        var grouped = await _context.Encounters
            .Include(e => e.Raid)
                .ThenInclude(r => r!.RaidType)
            .Where(e => e.EncounterName != null && e.Raid != null && e.Raid.RaidType != null)
            .GroupBy(e => new
            {
                RaidTypeName = e.Raid!.RaidType!.Name,
                EncounterEntry = e.EncounterEntry,
                EncounterName = e.EncounterName
            })
            .Select(g => new
            {
                g.Key.RaidTypeName,
                EncounterEntry = g.Key.EncounterEntry,
                EncounterName = g.Key.EncounterName
            })
            .ToListAsync(cancellationToken);

        var result = grouped
            .GroupBy(g => g.RaidTypeName)
            .Select(raidGroup => new
            {
                RaidTypeName = raidGroup.Key,
                Encounters = raidGroup
                    .GroupBy(e => e.EncounterName)
                    .Select(nameGroup => new
                    {
                        EncounterEntry = nameGroup.OrderBy(e => e.EncounterEntry).First().EncounterEntry,
                        EncounterName = nameGroup.Key
                    })
                    .OrderBy(e => e.EncounterName)
                    .ToList()
            })
            .OrderBy(r => r.RaidTypeName)
            .ToList();

        return Ok(result);
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<object>>> GetEncountersList(CancellationToken cancellationToken = default)
    {
        var encounters = await _context.Encounters
            .Where(e => e.EncounterName != null)
            .GroupBy(e => new { e.EncounterEntry, e.EncounterName })
            .Select(g => new
            {
                EncounterEntry = g.Key.EncounterEntry,
                EncounterName = g.Key.EncounterName
            })
            .OrderBy(e => e.EncounterName)
            .ToListAsync(cancellationToken);

        return Ok(encounters);
    }

    [HttpGet("{id:int}/raid")]
    public async Task<ActionResult<RaidDetailDto>> GetRaidByEncounterId(
        int id,
        CancellationToken cancellationToken = default)
    {
        var encounter = await _context.Encounters
            .AsNoTracking()
            .Include(e => e.Raid)
                .ThenInclude(r => r.RaidType)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (encounter == null || encounter.Raid == null)
            return NotFound();

        var encounters = await _context.Encounters
            .AsNoTracking()
            .Where(e => e.RaidId == encounter.RaidId)
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
            Id = encounter.Raid.Id,
            RaidId = encounter.Raid.RaidId,
            RaidTypeName = encounter.Raid.RaidType != null ? encounter.Raid.RaidType.Name : "",
            GuildName = encounter.Raid.GuildName,
            LeaderName = encounter.Raid.LeaderName,
            StartTime = encounter.Raid.StartTime,
            TotalTime = encounter.Raid.TotalTime,
            TotalDamage = encounter.Raid.TotalDamage,
            TotalHealing = encounter.Raid.TotalHealing,
            Wipes = encounter.Raid.Wipes,
            CompletedBosses = encounter.Raid.CompletedBosses,
            TotalBosses = encounter.Raid.TotalBosses,
            Encounters = encounters
        });
    }

    [HttpGet("{id:int}/players")]
    public async Task<ActionResult<IEnumerable<PlayerEncounterDto>>> GetEncounterPlayers(
        int id,
        CancellationToken cancellationToken = default)
    {
        var encounter = await _context.Encounters
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (encounter == null)
            return NotFound();

        var fightDuration = (encounter.EndTime - encounter.StartTime).TotalSeconds;

        var players = await _context.PlayerEncounters
            .Include(pe => pe.Player)
            .Include(pe => pe.CharacterSpec)
            .Where(pe => pe.EncounterId == id)
            .OrderByDescending(pe => pe.Role == "2" && fightDuration > 0
                ? (double)(pe.HealingDone + pe.AbsorbProvided) / fightDuration
                : pe.Dps)
            .ThenByDescending(pe => pe.DamageDone)
            .Select(pe => new PlayerEncounterDto
            {
                Id = pe.Id,
                PlayerName = pe.Player.CharacterName,
                CharacterClass = pe.Player.CharacterClass,
                ClassName = pe.Player.ClassName,
                CharacterSpec = pe.CharacterSpec.Spec,
                SpecName = pe.CharacterSpec.Name,
                Role = pe.Role,
                DamageDone = pe.DamageDone,
                HealingDone = pe.HealingDone,
                AbsorbProvided = pe.AbsorbProvided,
                Dps = pe.Dps,
                MaxAverageGearScore = pe.MaxAverageGearScore,
                MaxGearScore = pe.MaxGearScore
            })
            .ToListAsync(cancellationToken);

        return Ok(players);
    }
}

