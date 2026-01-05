using NibelungLog.DiscordBot.Models;

namespace NibelungLog.DiscordBot.Interfaces;

public interface IRaidService
{
    Task<RaidProgressModel?> GetRaidProgressAsync(string guildName, string raidName, CancellationToken cancellationToken = default);
    Task<List<RaidProgressModel>> GetLastRaidsAsync(string guildName, string raidName, int count, CancellationToken cancellationToken = default);
}

