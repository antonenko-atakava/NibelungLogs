namespace NibelungLog.DiscordBot.Configuration;

public sealed class DiscordBotConfiguration
{
    public required string Token { get; set; }
    public required string CommandPrefix { get; set; }
    public required ulong? GuildId { get; set; }
}

