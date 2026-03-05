using Microsoft.EntityFrameworkCore;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Entities;
using NibelungLog.Domain.Interfaces.Repositories;

namespace NibelungLog.DAL.Repositories;

public sealed class PlayerRepository : IPlayerRepository
{
    private readonly ApplicationDbContext _context;

    public PlayerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Player?> FindByCharacterGuidAsync(string characterGuid, CancellationToken cancellationToken = default)
    {
        return await _context.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.CharacterGuid == characterGuid, cancellationToken);
    }

    public async Task<List<Player>> GetByCharacterGuidsAsync(List<string> characterGuids, CancellationToken cancellationToken = default)
    {
        return await _context.Players
            .AsNoTracking()
            .Where(p => characterGuids.Contains(p.CharacterGuid))
            .ToListAsync(cancellationToken);
    }

    public async Task<Player> AddAsync(Player player, CancellationToken cancellationToken = default)
    {
        _context.Players.Add(player);
        return player;
    }

    public async Task AddRangeAsync(List<Player> players, CancellationToken cancellationToken = default)
    {
        await _context.Players.AddRangeAsync(players, cancellationToken);
    }

    public async Task UpdateAsync(Player player, CancellationToken cancellationToken = default)
    {
        _context.Players.Update(player);
    }

    public async Task UpdateRangeAsync(List<Player> players, CancellationToken cancellationToken = default)
    {
        _context.Players.UpdateRange(players);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearChangeTrackerAsync(CancellationToken cancellationToken = default)
    {
        _context.ChangeTracker.Clear();
        await Task.CompletedTask;
    }
}
