using NibelungLog.Domain.Entities;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface IPlayerRepository
{
    Task<Player?> FindByCharacterGuidAsync(string characterGuid, CancellationToken cancellationToken = default);
    Task<List<Player>> GetByCharacterGuidsAsync(List<string> characterGuids, CancellationToken cancellationToken = default);
    Task<Player> AddAsync(Player player, CancellationToken cancellationToken = default);
    Task AddRangeAsync(List<Player> players, CancellationToken cancellationToken = default);
    Task UpdateAsync(Player player, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(List<Player> players, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ClearChangeTrackerAsync(CancellationToken cancellationToken = default);
}
