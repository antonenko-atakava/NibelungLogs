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
            .FirstOrDefaultAsync(rt => rt.Map == map && rt.Difficulty == difficulty && rt.InstanceType == instanceType, cancellationToken);
    }

    public async Task<RaidType> AddAsync(RaidType raidType, CancellationToken cancellationToken = default)
    {
        _context.RaidTypes.Add(raidType);
        await _context.SaveChangesAsync(cancellationToken);
        return raidType;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
