using Microsoft.EntityFrameworkCore;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Entities;
using NibelungLog.Domain.Interfaces.Repositories;

namespace NibelungLog.DAL.Repositories;

public sealed class RaidRepository : IRaidRepository
{
    private readonly ApplicationDbContext _context;

    public RaidRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Raid?> FindByRaidIdAsync(string raidId, CancellationToken cancellationToken = default)
    {
        return await _context.Raids
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RaidId == raidId, cancellationToken);
    }

    public async Task<List<Raid>> GetByRaidIdsAsync(List<string> raidIds, CancellationToken cancellationToken = default)
    {
        return await _context.Raids
            .AsNoTracking()
            .Where(r => raidIds.Contains(r.RaidId))
            .ToListAsync(cancellationToken);
    }

    public async Task<Raid> AddAsync(Raid raid, CancellationToken cancellationToken = default)
    {
        _context.Raids.Add(raid);
        return raid;
    }

    public async Task AddRangeAsync(List<Raid> raids, CancellationToken cancellationToken = default)
    {
        await _context.Raids.AddRangeAsync(raids, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
