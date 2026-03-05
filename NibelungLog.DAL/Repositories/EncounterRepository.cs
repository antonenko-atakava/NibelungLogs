using Microsoft.EntityFrameworkCore;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Entities;
using NibelungLog.Domain.Interfaces.Repositories;

namespace NibelungLog.DAL.Repositories;

public sealed class EncounterRepository : IEncounterRepository
{
    private readonly ApplicationDbContext _context;

    public EncounterRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Encounter?> FindByRaidIdEncounterEntryStartTimeAsync(int raidId, string encounterEntry, DateTime startTime, CancellationToken cancellationToken = default)
    {
        return await _context.Encounters
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.RaidId == raidId && e.EncounterEntry == encounterEntry && e.StartTime == startTime, cancellationToken);
    }

    public async Task<List<Encounter>> GetByRaidIdEncounterEntryStartTimeAsync(List<(int RaidId, string EncounterEntry, DateTime StartTime)> keys, CancellationToken cancellationToken = default)
    {
        if (keys.Count == 0)
            return [];

        var raidIds = keys.Select(k => k.RaidId).Distinct().ToList();
        var allEncounters = await _context.Encounters
            .AsNoTracking()
            .Where(e => raidIds.Contains(e.RaidId))
            .ToListAsync(cancellationToken);

        var keysSet = keys.ToHashSet();
        return allEncounters
            .Where(e => keysSet.Contains((e.RaidId, e.EncounterEntry, e.StartTime)))
            .ToList();
    }

    public async Task<Encounter> AddAsync(Encounter encounter, CancellationToken cancellationToken = default)
    {
        _context.Encounters.Add(encounter);
        return encounter;
    }

    public async Task AddRangeAsync(List<Encounter> encounters, CancellationToken cancellationToken = default)
    {
        await _context.Encounters.AddRangeAsync(encounters, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
