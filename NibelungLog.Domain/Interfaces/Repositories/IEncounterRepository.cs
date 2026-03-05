using NibelungLog.Domain.Entities;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface IEncounterRepository
{
    Task<Encounter?> FindByRaidIdEncounterEntryStartTimeAsync(int raidId, string encounterEntry, DateTime startTime, CancellationToken cancellationToken = default);
    Task<List<Encounter>> GetByRaidIdEncounterEntryStartTimeAsync(List<(int RaidId, string EncounterEntry, DateTime StartTime)> keys, CancellationToken cancellationToken = default);
    Task<Encounter> AddAsync(Encounter encounter, CancellationToken cancellationToken = default);
    Task AddRangeAsync(List<Encounter> encounters, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
