using NibelungLog.Domain.Types.Dto;

namespace NibelungLog.Domain.Interfaces;

public interface IGuildParserService
{
    Task<GuildInfoRecord?> GetGuildInfoAsync(string guildName, int serverId, CancellationToken cancellationToken = default);
    Task<List<GuildMemberRecord>> GetGuildMembersAsync(string guildId, int serverId, CancellationToken cancellationToken = default);
    Task<List<GuildListItemRecord>> GetAllGuildsAsync(int serverId, CancellationToken cancellationToken = default);
    Task<List<GuildMemberRecord>> GetGuildMembersPageAsync(string guildId, int serverId, int page, int limit, CancellationToken cancellationToken = default);
}
