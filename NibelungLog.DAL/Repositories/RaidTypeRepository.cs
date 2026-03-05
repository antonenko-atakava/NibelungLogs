using Microsoft.EntityFrameworkCore;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Entities;
using NibelungLog.Domain.Interfaces.Repositories;

namespace NibelungLog.DAL.Repositories;

public sealed class RaidTypeRepository : IRaidTypeRepository
{
    private readonly ApplicationDbContext _context;

    public RaidTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RaidType?> FindByMapDifficultyInstanceTypeAsync(string map, string difficulty, string instanceType, CancellationToken cancellationToken = default)
    {
        return await _context.RaidTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Map == map && rt.Difficulty == difficulty && rt.InstanceType == instanceType, cancellationToken);
    }

    public async Task<List<RaidType>> GetByMapDifficultyInstanceTypeAsync(List<(string Map, string Difficulty, string InstanceType)> keys, CancellationToken cancellationToken = default)
    {
        if (keys.Count == 0)
            return [];

        var maps = keys.Select(k => k.Map).Distinct().ToList();
        var allRaidTypes = await _context.RaidTypes
            .AsNoTracking()
            .Where(rt => maps.Contains(rt.Map))
            .ToListAsync(cancellationToken);

        var keysSet = keys.ToHashSet();
        return allRaidTypes
            .Where(rt => keysSet.Contains((rt.Map, rt.Difficulty, rt.InstanceType)))
            .ToList();
    }

    public async Task<RaidType> AddAsync(RaidType raidType, CancellationToken cancellationToken = default)
    {
        _context.RaidTypes.Add(raidType);
        return raidType;
    }

    public async Task AddRangeAsync(List<RaidType> raidTypes, CancellationToken cancellationToken = default)
    {
        await _context.RaidTypes.AddRangeAsync(raidTypes, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
