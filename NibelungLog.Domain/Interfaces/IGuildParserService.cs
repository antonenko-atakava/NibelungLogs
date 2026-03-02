using NibelungLog.Domain.Types.Dto;

namespace NibelungLog.Domain.Interfaces;

public interface IGuildParserService
{
    Task<GuildInfoRecord?> GetGuildInfoAsync(string guildName, int serverId, CancellationToken cancellationToken = default);
    Task<List<GuildMemberRecord>> GetGuildMembersAsync(string guildId, int serverId, CancellationToken cancellationToken = default);
}
