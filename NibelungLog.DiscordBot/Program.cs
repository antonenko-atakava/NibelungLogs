using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NibelungLog.Data;
using NibelungLog.DiscordBot.Handlers;
using NibelungLog.DiscordBot.Interfaces;
using NibelungLog.DiscordBot.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        var discordConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
            LogGatewayIntentWarnings = false
        };

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=nibelunglog;Username=postgres;Password=4444";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddSingleton(discordConfig);
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton<CommandService>();
        services.AddSingleton<CommandHandler>();
        services.AddSingleton<IDiscordBotService, DiscordBotService>();
        services.AddScoped<IGuildService, GuildService>();
        services.AddScoped<IRaidService, RaidService>();
        services.AddScoped<IImageGenerationService, ImageGenerationService>();

        services.AddHostedService<DiscordBotHostedService>();
    })
    .Build();

await host.RunAsync();

