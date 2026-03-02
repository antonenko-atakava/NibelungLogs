using NibelungLog.Domain.Entities;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface IRaidTypeRepository
{
    Task<RaidType?> FindByMapDifficultyInstanceTypeAsync(string map, string difficulty, string instanceType, CancellationToken cancellationToken = default);
    Task<RaidType> AddAsync(RaidType raidType, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
