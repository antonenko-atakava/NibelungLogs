using NibelungLog.DiscordBot.Models;

namespace NibelungLog.DiscordBot.Interfaces;

public interface IGuildService
{
    Task<GuildInfoModel?> GetGuildInfoAsync(string guildName, CancellationToken cancellationToken = default);
    Task<TopPlayersModel> GetTopPlayersAsync(string guildName, int count, CancellationToken cancellationToken = default);
    Task<TopPlayersModel> GetTopPlayersByClassAsync(string guildName, string className, int count, CancellationToken cancellationToken = default);
}

