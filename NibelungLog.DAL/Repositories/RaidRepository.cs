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
            .FirstOrDefaultAsync(r => r.RaidId == raidId, cancellationToken);
    }

    public async Task<Raid> AddAsync(Raid raid, CancellationToken cancellationToken = default)
    {
        _context.Raids.Add(raid);
        await _context.SaveChangesAsync(cancellationToken);
        return raid;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
