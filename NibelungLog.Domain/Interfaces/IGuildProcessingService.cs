namespace NibelungLog.Domain.Interfaces;

public interface IGuildProcessingService
{
    Task ProcessGuildAsync(string guildName, string guildId, int serverId, CancellationToken cancellationToken = default);
}
