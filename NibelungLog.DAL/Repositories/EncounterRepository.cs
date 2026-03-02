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
            .FirstOrDefaultAsync(e => e.RaidId == raidId && e.EncounterEntry == encounterEntry && e.StartTime == startTime, cancellationToken);
    }

    public async Task<Encounter> AddAsync(Encounter encounter, CancellationToken cancellationToken = default)
    {
        _context.Encounters.Add(encounter);
        await _context.SaveChangesAsync(cancellationToken);
        return encounter;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
