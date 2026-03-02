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
        logPath = "/var/log/nibelunglog/parser-ulduar.log";
    else
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var logsDirectory = Path.Combine(appDirectory, "logs");
        logPath = Path.Combine(logsDirectory, "parser-ulduar.log");
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
logger.LogInformation("Запуск парсера рейдов Ульдуар");
logger.LogInformation("═══════════════════════════════════════════════════════════");
logger.LogInformation("Логи записываются в: {LogPath}", logPath);

var authService = serviceProvider.GetRequiredService<IWowCircleAuthService>();

logger.LogInformation("Подключение к WowCircle...");
var result = await authService.LoginAsync("godlix", "1010334v", 5);

if (!result.IsAuth)
{
    logger.LogError("❌ Ошибка авторизации в WowCircle");
    return;
}

logger.LogInformation("✅ Авторизация успешна: {AccountName} (Сервер: {Realm})", result.Name, result.Realm);

using var scope = serviceProvider.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await dbContext.Database.EnsureCreatedAsync();

logger.LogInformation("Поиск рейдов Ульдуар...");
var ulduarRaids = await authService.GetUlduarRaidsAsync(5, 1);

logger.LogInformation("Найдено рейдов: {Count}", ulduarRaids.Count);

if (ulduarRaids.Count == 0)
{
    logger.LogWarning("⚠️  Рейды не найдены");
    return;
}

var dataService = scope.ServiceProvider.GetRequiredService<IRaidDataService>();

var totalEncounters = 0;
var totalPlayerEncounters = 0;
var raidIndex = 0;
var startTime = DateTime.Now;

logger.LogInformation("───────────────────────────────────────────────────────────");
logger.LogInformation("Обработка рейдов...");
logger.LogInformation("───────────────────────────────────────────────────────────");

foreach (var raid in ulduarRaids)
{
    raidIndex++;
    
    var encounters = await authService.GetRaidDetailsAsync(5, raid.Id);
    var successfulEncounters = encounters.Where(e => e.Success == "1").ToList();
    
    var raidEncounters = new List<EncounterRecord>();
    var raidPlayerEncounters = new List<PlayerEncounterRecord>();
    
    foreach (var encounter in successfulEncounters)
    {
        raidEncounters.Add(encounter);
        
        var players = await authService.GetEncounterPlayersAsync(5, raid.Id, encounter.EncounterEntry, encounter.StartTime);
        
        await Task.Delay(300);
        
        raidPlayerEncounters.AddRange(players);
    }
    
    await dataService.SaveSingleRaidDataAsync(raid, raidEncounters, raidPlayerEncounters);
    
    totalEncounters += raidEncounters.Count;
    totalPlayerEncounters += raidPlayerEncounters.Count;
    
    if (raidIndex % 10 == 0 || raidIndex == ulduarRaids.Count)
    {
        var progress = (double)raidIndex / ulduarRaids.Count * 100;
        logger.LogInformation("Прогресс: {Current}/{Total} ({Progress:F1}%) | Энкаунтеров: {Encounters} | Игроков: {Players}", 
            raidIndex, ulduarRaids.Count, progress, totalEncounters, totalPlayerEncounters);
    }
}

var elapsedTime = DateTime.Now - startTime;

logger.LogInformation("───────────────────────────────────────────────────────────");
logger.LogInformation("✅ Обработка завершена");
logger.LogInformation("───────────────────────────────────────────────────────────");
logger.LogInformation("Сохранено:");
logger.LogInformation("  • Рейдов: {Raids}", raidIndex);
logger.LogInformation("  • Энкаунтеров: {Encounters}", totalEncounters);
logger.LogInformation("  • Записей игроков: {Players}", totalPlayerEncounters);
logger.LogInformation("Время выполнения: {Time}", elapsedTime.ToString(@"mm\:ss"));
logger.LogInformation("═══════════════════════════════════════════════════════════");
