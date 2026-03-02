namespace NibelungLog.Domain.Interfaces;

public interface IRaidParserService
{
    Task ParseRaidsAsync(int serverId, string mapId, int difficulty, CancellationToken cancellationToken = default);
}
