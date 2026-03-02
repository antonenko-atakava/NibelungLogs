using Microsoft.EntityFrameworkCore;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Entities;
using NibelungLog.Domain.Interfaces.Repositories;

namespace NibelungLog.DAL.Repositories;

public sealed class GuildMemberRepository : IGuildMemberRepository
{
    private readonly ApplicationDbContext _context;

    public GuildMemberRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GuildMember?> FindByGuildIdAndPlayerIdAsync(int guildId, int playerId, CancellationToken cancellationToken = default)
    {
        return await _context.GuildMembers
            .FirstOrDefaultAsync(gm => gm.GuildId == guildId && gm.PlayerId == playerId, cancellationToken);
    }

    public async Task<GuildMember> AddAsync(GuildMember guildMember, CancellationToken cancellationToken = default)
    {
        _context.GuildMembers.Add(guildMember);
        await _context.SaveChangesAsync(cancellationToken);
        return guildMember;
    }

    public async Task UpdateAsync(GuildMember guildMember, CancellationToken cancellationToken = default)
    {
        _context.GuildMembers.Update(guildMember);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
