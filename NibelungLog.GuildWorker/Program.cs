using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.DAL.Data;
using NibelungLog.DAL.Repositories;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Service.Services;
using NibelungLog.Service.Infrastructure;
using NibelungLog.Domain.Types.Dto;

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
serviceCollection.AddScoped<IGuildDataService, NibelungLog.Service.Services.GuildDataService>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var authService = serviceProvider.GetRequiredService<IWowCircleAuthService>();
var guildParserService = serviceProvider.GetRequiredService<IGuildParserService>();
var guildDataService = serviceProvider.GetRequiredService<IGuildDataService>();

const string guildName = "Сироты из Наксрамаса";
const int serverId = 5;

var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("═══════════════════════════════════════════════════════════");
logger.LogInformation("Запуск парсера гильдии");
logger.LogInformation("═══════════════════════════════════════════════════════════");
logger.LogInformation("Гильдия: {GuildName} | Сервер: {ServerId}", guildName, serverId);
logger.LogInformation("Логи записываются в: {LogPath}", logPath);

logger.LogInformation("Подключение к WowCircle...");
var loginResult = await authService.LoginAsync("bigdane21", "castiel2332", serverId);

if (!loginResult.IsAuth)
{
    logger.LogError("❌ Ошибка авторизации в WowCircle");
    return;
}

logger.LogInformation("✅ Авторизация успешна: {AccountName} (Сервер: {Realm})", loginResult.Name, loginResult.Realm);

using var scope = serviceProvider.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await dbContext.Database.EnsureCreatedAsync();

const string guildId = "23";

logger.LogInformation("───────────────────────────────────────────────────────────");
logger.LogInformation("Загрузка участников гильдии...");
logger.LogInformation("───────────────────────────────────────────────────────────");

var members = await guildParserService.GetGuildMembersAsync(guildId, serverId);

var guildInfo = new GuildInfoRecord
{
    GuildId = guildId,
    GuildName = guildName
};

logger.LogInformation("Найдено участников: {Count}", members.Count);

if (members.Count == 0)
{
    logger.LogWarning("⚠️  Участники не найдены");
    return;
}

logger.LogInformation("Сохранение данных в базу...");
await guildDataService.SaveGuildDataAsync(guildInfo, members);

logger.LogInformation("───────────────────────────────────────────────────────────");
logger.LogInformation("✅ Данные гильдии успешно сохранены");
logger.LogInformation("  • Участников: {Count}", members.Count);
logger.LogInformation("═══════════════════════════════════════════════════════════");

