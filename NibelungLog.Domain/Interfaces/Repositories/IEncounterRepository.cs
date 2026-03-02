using NibelungLog.Domain.Entities;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface IEncounterRepository
{
    Task<Encounter?> FindByRaidIdEncounterEntryStartTimeAsync(int raidId, string encounterEntry, DateTime startTime, CancellationToken cancellationToken = default);
    Task<Encounter> AddAsync(Encounter encounter, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
