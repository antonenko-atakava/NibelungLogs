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
        logPath = "/var/log/nibelunglog/parser-naxxramas.log";
    else
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var logsDirectory = Path.Combine(appDirectory, "logs");
        logPath = Path.Combine(logsDirectory, "parser-naxxramas.log");
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
serviceCollection.AddScoped<ICharacterSpecRepository, CharacterSpecRepository>();
serviceCollection.AddScoped<IRaidTypeRepository, RaidTypeRepository>();
serviceCollection.AddScoped<IRaidRepository, RaidRepository>();
serviceCollection.AddScoped<IEncounterRepository, EncounterRepository>();
serviceCollection.AddScoped<IPlayerEncounterRepository, PlayerEncounterRepository>();

serviceCollection.AddScoped<IRaidDataService, RaidDataService>();
serviceCollection.AddScoped<IRaidParserService, RaidParserService>();

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

var serviceProvider = serviceCollection.BuildServiceProvider();

var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("═══════════════════════════════════════════════════════════");
logger.LogInformation("Запуск парсера рейдов Наксрамас");
logger.LogInformation("═══════════════════════════════════════════════════════════");
logger.LogInformation("Логи записываются в: {LogPath}", logPath);

var accountName = Environment.GetEnvironmentVariable("WOWCIRCLE_LOGIN");
var accountPassword = Environment.GetEnvironmentVariable("WOWCIRCLE_PASSWORD");
var serverIdStr = Environment.GetEnvironmentVariable("WOWCIRCLE_SERVER_ID");
var mapId = Environment.GetEnvironmentVariable("RAID_MAP_ID");
var difficultyStr = Environment.GetEnvironmentVariable("RAID_DIFFICULTY");

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

if (string.IsNullOrEmpty(mapId))
{
    logger.LogError("❌ Не указан RAID_MAP_ID");
    return;
}

if (string.IsNullOrEmpty(difficultyStr) || !int.TryParse(difficultyStr, out var difficulty))
{
    logger.LogError("❌ Не указан или некорректный RAID_DIFFICULTY");
    return;
}

var authService = serviceProvider.GetRequiredService<IWowCircleAuthService>();

logger.LogInformation("Подключение к WowCircle...");
var result = await authService.LoginAsync(accountName, accountPassword, serverId);

if (!result.IsAuth)
{
    logger.LogError("❌ Ошибка авторизации в WowCircle");
    return;
}

logger.LogInformation("✅ Авторизация успешна: {AccountName} (Сервер: {Realm})", result.Name, result.Realm);

using var scope = serviceProvider.CreateScope();
var raidParserService = scope.ServiceProvider.GetRequiredService<IRaidParserService>();

await raidParserService.ParseRaidsAsync(serverId, mapId, difficulty);
