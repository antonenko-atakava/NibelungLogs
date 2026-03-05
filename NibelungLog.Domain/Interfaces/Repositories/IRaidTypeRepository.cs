using NibelungLog.Domain.Entities;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface IRaidTypeRepository
{
    Task<List<RaidType>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RaidType?> FindByMapDifficultyInstanceTypeAsync(string map, string difficulty, string instanceType, CancellationToken cancellationToken = default);
    Task<List<RaidType>> GetByMapDifficultyInstanceTypeAsync(List<(string Map, string Difficulty, string InstanceType)> keys, CancellationToken cancellationToken = default);
    Task<RaidType> AddAsync(RaidType raidType, CancellationToken cancellationToken = default);
    Task AddRangeAsync(List<RaidType> raidTypes, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ClearChangeTrackerAsync(CancellationToken cancellationToken = default);
}
