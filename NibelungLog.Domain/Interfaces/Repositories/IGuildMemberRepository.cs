using NibelungLog.Domain.Entities;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface IGuildMemberRepository
{
    Task<GuildMember?> FindByGuildIdAndPlayerIdAsync(int guildId, int playerId, CancellationToken cancellationToken = default);
    Task<GuildMember> AddAsync(GuildMember guildMember, CancellationToken cancellationToken = default);
    Task UpdateAsync(GuildMember guildMember, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
