namespace NibelungLog.DiscordBot.Interfaces;

public interface IDiscordBotService
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}

