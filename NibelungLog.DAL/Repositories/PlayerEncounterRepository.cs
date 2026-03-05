using Microsoft.EntityFrameworkCore;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Entities;
using NibelungLog.Domain.Interfaces.Repositories;

namespace NibelungLog.DAL.Repositories;

public sealed class PlayerEncounterRepository : IPlayerEncounterRepository
{
    private readonly ApplicationDbContext _context;

    public PlayerEncounterRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlayerEncounter?> FindByPlayerIdAndEncounterIdAsync(int playerId, int encounterId, CancellationToken cancellationToken = default)
    {
        return await _context.PlayerEncounters
            .AsNoTracking()
            .FirstOrDefaultAsync(pe => pe.PlayerId == playerId && pe.EncounterId == encounterId, cancellationToken);
    }

    public async Task<List<PlayerEncounter>> GetByPlayerIdAndEncounterIdAsync(List<(int PlayerId, int EncounterId)> keys, CancellationToken cancellationToken = default)
    {
        if (keys.Count == 0)
            return [];

        var playerIds = keys.Select(k => k.PlayerId).Distinct().ToList();
        var encounterIds = keys.Select(k => k.EncounterId).Distinct().ToList();
        var allPlayerEncounters = await _context.PlayerEncounters
            .AsNoTracking()
            .Where(pe => playerIds.Contains(pe.PlayerId) && encounterIds.Contains(pe.EncounterId))
            .ToListAsync(cancellationToken);

        var keysSet = keys.ToHashSet();
        return allPlayerEncounters
            .Where(pe => keysSet.Contains((pe.PlayerId, pe.EncounterId)))
            .ToList();
    }

    public async Task<PlayerEncounter> AddAsync(PlayerEncounter playerEncounter, CancellationToken cancellationToken = default)
    {
        _context.PlayerEncounters.Add(playerEncounter);
        return playerEncounter;
    }

    public async Task AddRangeAsync(List<PlayerEncounter> playerEncounters, CancellationToken cancellationToken = default)
    {
        await _context.PlayerEncounters.AddRangeAsync(playerEncounters, cancellationToken);
    }

    public async Task UpdateAsync(PlayerEncounter playerEncounter, CancellationToken cancellationToken = default)
    {
        _context.PlayerEncounters.Update(playerEncounter);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
