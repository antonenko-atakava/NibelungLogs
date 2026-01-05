using NibelungLog.Types.Dto;

namespace NibelungLog.GuildWorker.Interfaces;

public interface IGuildParserService
{
    Task<GuildInfoRecord?> GetGuildInfoAsync(string guildName, int serverId, CancellationToken cancellationToken = default);
    Task<List<GuildMemberRecord>> GetGuildMembersAsync(string guildId, int serverId, CancellationToken cancellationToken = default);
}

