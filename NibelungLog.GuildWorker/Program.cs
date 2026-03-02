using System.Net;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.DAL.Data;
using NibelungLog.DAL.Repositories;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Service.Services;
using NibelungLog.Service.Infrastructure;

var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
var currentDirectory = Directory.GetCurrentDirectory();
var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? baseDirectory;

var projectRoot = Directory.GetParent(baseDirectory)?.Parent?.Parent?.FullName;
if (string.IsNullOrEmpty(projectRoot))
{
    var dir = new DirectoryInfo(baseDirectory);
    while (dir != null && !File.Exists(Path.Combine(dir.FullName, ".env")) && dir.Name != "NibelungLogs")
    {
        dir = dir.Parent;
    }
    if (dir != null)
        projectRoot = dir.FullName;
}

var envPaths = new[]
{
    Path.Combine(baseDirectory, ".env"),
    Path.Combine(currentDirectory, ".env"),
    Path.Combine(assemblyDirectory, ".env"),
    projectRoot != null ? Path.Combine(projectRoot, ".env") : null
}.Where(p => p != null).Cast<string>().ToArray();

string? envPath = null;
foreach (var path in envPaths)
{
    if (File.Exists(path))
    {
        envPath = path;
        break;
    }
}

if (envPath != null)
{
    Env.Load(envPath);
}

var serviceCollection = new ServiceCollection();

var logPath = Environment.GetEnvironmentVariable("LOG_PATH");
if (string.IsNullOrEmpty(logPath))
{
    var isLinux = Environment.OSVersion.Platform == PlatformID.Unix || 
                  Environment.OSVersion.Platform == PlatformID.MacOSX;
    
    if (isLinux)
        logPath = "/var/log/nibelunglog/guild-worker.log";
    else
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var logsDirectory = Path.Combine(appDirectory, "logs");
        logPath = Path.Combine(logsDirectory, "guild-worker.log");
    }
}

serviceCollection.AddLogging(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
        options.UseUtcTimestamp = false;
        options.IncludeScopes = false;
    });
    builder.AddProvider(new FileLoggerProvider(logPath));
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
    builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
});

var postgresConnectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") 
    ?? "Host=localhost;Port=5432;Database=nibelunglog;Username=postgres;Password=1234";

serviceCollection.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

serviceCollection.AddScoped<IPlayerRepository, PlayerRepository>();
serviceCollection.AddScoped<IGuildRepository, GuildRepository>();
serviceCollection.AddScoped<IGuildMemberRepository, GuildMemberRepository>();

var httpClientHandler = new HttpClientHandler
{
    CookieContainer = new CookieContainer(),
    UseCookies = true
};

var httpClient = new HttpClient(httpClientHandler)
{
    BaseAddress = new Uri("https://cp.wowcircle.net")
};

httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
httpClient.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7,uk;q=0.6");
httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36");

serviceCollection.AddSingleton(httpClient);
serviceCollection.AddTransient<IWowCircleAuthService, WowCircleAuthService>();
serviceCollection.AddTransient<IGuildParserService, GuildParserService>();
serviceCollection.AddScoped<IGuildDataService, GuildDataService>();
serviceCollection.AddScoped<IGuildProcessingService, GuildProcessingService>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger<Program>();

var authService = serviceProvider.GetRequiredService<IWowCircleAuthService>();

var guildName = Environment.GetEnvironmentVariable("GUILD_NAME");
var guildId = Environment.GetEnvironmentVariable("GUILD_ID");
var serverIdStr = Environment.GetEnvironmentVariable("WOWCIRCLE_SERVER_ID");
var accountName = Environment.GetEnvironmentVariable("WOWCIRCLE_LOGIN");
var accountPassword = Environment.GetEnvironmentVariable("WOWCIRCLE_PASSWORD");

if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(accountPassword))
{
    logger.LogError("❌ Не указаны учетные данные: WOWCIRCLE_LOGIN и WOWCIRCLE_PASSWORD");
    return;
}

if (string.IsNullOrEmpty(serverIdStr) || !int.TryParse(serverIdStr, out var serverId))
{
    logger.LogError("❌ Не указан или некорректный WOWCIRCLE_SERVER_ID");
    return;
}

if (string.IsNullOrEmpty(guildName))
{
    logger.LogError("❌ Не указан GUILD_NAME");
    return;
}

if (string.IsNullOrEmpty(guildId))
{
    logger.LogError("❌ Не указан GUILD_ID");
    return;
}

logger.LogInformation("═══════════════════════════════════════════════════════════");
logger.LogInformation("Запуск парсера гильдии");
logger.LogInformation("═══════════════════════════════════════════════════════════");
logger.LogInformation("Гильдия: {GuildName} | Сервер: {ServerId}", guildName, serverId);
logger.LogInformation("Логи записываются в: {LogPath}", logPath);

logger.LogInformation("Подключение к WowCircle...");
var loginResult = await authService.LoginAsync(accountName, accountPassword, serverId);

if (!loginResult.IsAuth)
{
    logger.LogError("❌ Ошибка авторизации в WowCircle");
    return;
}

logger.LogInformation("✅ Авторизация успешна: {AccountName} (Сервер: {Realm})", loginResult.Name, loginResult.Realm);

using var scope = serviceProvider.CreateScope();
var guildProcessingService = scope.ServiceProvider.GetRequiredService<IGuildProcessingService>();

await guildProcessingService.ProcessGuildAsync(guildName, guildId, serverId);

