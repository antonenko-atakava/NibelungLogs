using NibelungLog.Domain.Entities;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface IPlayerEncounterRepository
{
    Task<PlayerEncounter?> FindByPlayerIdAndEncounterIdAsync(int playerId, int encounterId, CancellationToken cancellationToken = default);
    Task<List<PlayerEncounter>> GetByPlayerIdAndEncounterIdAsync(List<(int PlayerId, int EncounterId)> keys, CancellationToken cancellationToken = default);
    Task<PlayerEncounter> AddAsync(PlayerEncounter playerEncounter, CancellationToken cancellationToken = default);
    Task AddRangeAsync(List<PlayerEncounter> playerEncounters, CancellationToken cancellationToken = default);
    Task UpdateAsync(PlayerEncounter playerEncounter, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
