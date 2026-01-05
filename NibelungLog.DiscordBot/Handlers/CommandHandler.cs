using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NibelungLog.DiscordBot.Handlers;

public sealed class CommandHandler
{
    private readonly CommandService _commands;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandHandler> _logger;

    public CommandHandler(
        CommandService commands,
        IServiceProvider serviceProvider,
        ILogger<CommandHandler> logger)
    {
        _commands = commands;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(typeof(CommandHandler).Assembly, _serviceProvider);
        _logger.LogInformation("Command modules loaded");
    }
}

