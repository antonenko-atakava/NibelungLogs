using Microsoft.EntityFrameworkCore;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Entities;
using NibelungLog.Domain.Interfaces.Repositories;

namespace NibelungLog.DAL.Repositories;

public sealed class GuildRepository : IGuildRepository
{
    private readonly ApplicationDbContext _context;

    public GuildRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guild?> FindByGuildIdAsync(string guildId, CancellationToken cancellationToken = default)
    {
        return await _context.Guilds
            .FirstOrDefaultAsync(g => g.GuildId == guildId, cancellationToken);
    }

    public async Task<Guild> AddAsync(Guild guild, CancellationToken cancellationToken = default)
    {
        _context.Guilds.Add(guild);
        await _context.SaveChangesAsync(cancellationToken);
        return guild;
    }

    public async Task UpdateAsync(Guild guild, CancellationToken cancellationToken = default)
    {
        _context.Guilds.Update(guild);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
