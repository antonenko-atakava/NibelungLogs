using NibelungLog.Domain.Entities;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface IRaidRepository
{
    Task<Raid?> FindByRaidIdAsync(string raidId, CancellationToken cancellationToken = default);
    Task<List<Raid>> GetByRaidIdsAsync(List<string> raidIds, CancellationToken cancellationToken = default);
    Task<Raid> AddAsync(Raid raid, CancellationToken cancellationToken = default);
    Task AddRangeAsync(List<Raid> raids, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
