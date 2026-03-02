using NibelungLog.Domain.Types.Dto;

namespace NibelungLog.Domain.Interfaces;

public interface IGuildDataService
{
    Task SaveGuildDataAsync(GuildInfoRecord guildInfo, List<GuildMemberRecord> members, CancellationToken cancellationToken = default);
    Task SaveGuildAsync(GuildInfoRecord guildInfo, CancellationToken cancellationToken = default);
    Task SaveGuildMembersPageAsync(string guildId, List<GuildMemberRecord> members, CancellationToken cancellationToken = default);
}
