using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NibelungLog.DiscordBot.Handlers;
using NibelungLog.DiscordBot.Interfaces;

namespace NibelungLog.DiscordBot.Services;

public sealed class DiscordBotHostedService : IHostedService
{
    private readonly IDiscordBotService _discordBotService;
    private readonly CommandHandler _commandHandler;
    private readonly ILogger<DiscordBotHostedService> _logger;

    public DiscordBotHostedService(
        IDiscordBotService discordBotService,
        CommandHandler commandHandler,
        ILogger<DiscordBotHostedService> logger)
    {
        _discordBotService = discordBotService;
        _commandHandler = commandHandler;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Discord bot...");
        
        await _commandHandler.InitializeAsync();
        await _discordBotService.StartAsync(cancellationToken);
        
        _logger.LogInformation("Discord bot started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Discord bot...");
        
        await _discordBotService.StopAsync(cancellationToken);
        
        _logger.LogInformation("Discord bot stopped");
    }
}

