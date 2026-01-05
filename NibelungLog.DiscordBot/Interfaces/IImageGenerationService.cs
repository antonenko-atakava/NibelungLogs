using NibelungLog.DiscordBot.Models;

namespace NibelungLog.DiscordBot.Interfaces;

public interface IImageGenerationService
{
    Task<Stream> GenerateGuildInfoImageAsync(GuildInfoModel guildInfo, CancellationToken cancellationToken = default);
    Task<Stream> GenerateGuildClassesImageAsync(GuildInfoModel guildInfo, CancellationToken cancellationToken = default);
    Task<Stream> GenerateRaidProgressImageAsync(RaidProgressModel raidProgress, CancellationToken cancellationToken = default);
    Task<Stream> GenerateTopPlayersImageAsync(TopPlayersModel topPlayers, CancellationToken cancellationToken = default);
}

