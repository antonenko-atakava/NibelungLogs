using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NibelungLog.DiscordBot.Configuration;
using NibelungLog.DiscordBot.Interfaces;

namespace NibelungLog.DiscordBot.Services;

public sealed class DiscordBotService : IDiscordBotService
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DiscordBotService> _logger;
    private readonly DiscordBotConfiguration _configuration;

    public DiscordBotService(
        DiscordSocketClient client,
        CommandService commands,
        IServiceProvider serviceProvider,
        ILogger<DiscordBotService> logger,
        IConfiguration configuration)
    {
        _client = client;
        _commands = commands;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration.GetSection("DiscordBot").Get<DiscordBotConfiguration>() 
            ?? throw new InvalidOperationException("DiscordBot configuration is missing");
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _client.Log += LogAsync;
        _commands.Log += LogAsync;

        await _client.LoginAsync(TokenType.Bot, _configuration.Token);
        await _client.StartAsync();

        _client.Ready += OnReadyAsync;
        _client.MessageReceived += OnMessageReceivedAsync;

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _client.StopAsync();
        await _client.LogoutAsync();
    }

    private Task OnReadyAsync()
    {
        _logger.LogInformation("Discord bot is ready. Logged in as {BotName}", _client.CurrentUser?.Username);
        return Task.CompletedTask;
    }

    private async Task OnMessageReceivedAsync(SocketMessage message)
    {
        if (message is not SocketUserMessage userMessage)
            return;

        if (userMessage.Author.Id == _client.CurrentUser?.Id)
            return;

        var argPos = 0;
        if (!userMessage.HasStringPrefix(_configuration.CommandPrefix, ref argPos))
            return;

        var context = new SocketCommandContext(_client, userMessage);
        await _commands.ExecuteAsync(context, argPos, _serviceProvider);
    }

    private Task LogAsync(LogMessage logMessage)
    {
        var logLevel = logMessage.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Debug,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel, "{Source}: {Message}", logMessage.Source, logMessage.Message);

        if (logMessage.Exception != null)
            _logger.LogError(logMessage.Exception, "Exception in Discord client");

        return Task.CompletedTask;
    }
}

