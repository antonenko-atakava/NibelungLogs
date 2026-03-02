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
            .FirstOrDefaultAsync(pe => pe.PlayerId == playerId && pe.EncounterId == encounterId, cancellationToken);
    }

    public async Task<PlayerEncounter> AddAsync(PlayerEncounter playerEncounter, CancellationToken cancellationToken = default)
    {
        _context.PlayerEncounters.Add(playerEncounter);
        await _context.SaveChangesAsync(cancellationToken);
        return playerEncounter;
    }

    public async Task UpdateAsync(PlayerEncounter playerEncounter, CancellationToken cancellationToken = default)
    {
        _context.PlayerEncounters.Update(playerEncounter);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
