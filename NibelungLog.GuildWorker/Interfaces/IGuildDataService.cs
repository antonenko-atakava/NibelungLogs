using NibelungLog.Types.Dto;

namespace NibelungLog.GuildWorker.Interfaces;

public interface IGuildDataService
{
    Task SaveGuildDataAsync(GuildInfoRecord guildInfo, List<GuildMemberRecord> members, CancellationToken cancellationToken = default);
}

