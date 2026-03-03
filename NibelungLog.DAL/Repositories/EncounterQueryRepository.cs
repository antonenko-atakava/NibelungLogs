using Microsoft.EntityFrameworkCore;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.DAL.Repositories;

public sealed class EncounterQueryRepository : IEncounterQueryRepository
{
    private readonly ApplicationDbContext _context;

    public EncounterQueryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EncounterGroupedDto>> GetEncountersGroupedByRaidAsync(CancellationToken cancellationToken = default)
    {
        var grouped = await _context.Encounters
            .Include(e => e.Raid)
                .ThenInclude(r => r!.RaidType)
            .Where(e => e.EncounterName != null && e.Raid != null && e.Raid.RaidType != null && e.EncounterEntry != "33113")
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
            .Select(raidGroup => new EncounterGroupedDto
            {
                RaidTypeName = raidGroup.Key,
                Encounters = raidGroup
                    .GroupBy(e => e.EncounterName)
                    .Select(nameGroup => new EncounterListItemDto
                    {
                        EncounterEntry = nameGroup.OrderBy(e => e.EncounterEntry).First().EncounterEntry,
                        EncounterName = nameGroup.Key!
                    })
                    .OrderBy(e => e.EncounterName)
                    .ToList()
            })
            .OrderBy(r => r.RaidTypeName)
            .ToList();

        return result;
    }

    public async Task<List<EncounterListItemDto>> GetEncountersListAsync(CancellationToken cancellationToken = default)
    {
        var encounters = await _context.Encounters
            .Where(e => e.EncounterName != null)
            .GroupBy(e => new { e.EncounterEntry, e.EncounterName })
            .Select(g => new EncounterListItemDto
            {
                EncounterEntry = g.Key.EncounterEntry,
                EncounterName = g.Key.EncounterName!
            })
            .OrderBy(e => e.EncounterName)
            .ToListAsync(cancellationToken);

        return encounters;
    }

    public async Task<RaidDetailDto?> GetRaidByEncounterIdAsync(int encounterId, CancellationToken cancellationToken = default)
    {
        var encounter = await _context.Encounters
            .AsNoTracking()
            .Include(e => e.Raid)
                .ThenInclude(r => r.RaidType)
            .FirstOrDefaultAsync(e => e.Id == encounterId, cancellationToken);

        if (encounter == null || encounter.Raid == null)
            return null;

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

        return new RaidDetailDto
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
        };
    }

    public async Task<List<PlayerEncounterDto>> GetEncounterPlayersAsync(int encounterId, CancellationToken cancellationToken = default)
    {
        var encounter = await _context.Encounters
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == encounterId, cancellationToken);

        if (encounter == null)
            return [];

        var fightDuration = (encounter.EndTime - encounter.StartTime).TotalSeconds;

        var players = await _context.PlayerEncounters
            .Include(pe => pe.Player)
            .Include(pe => pe.CharacterSpec)
            .Where(pe => pe.EncounterId == encounterId)
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

        return players;
    }
}
