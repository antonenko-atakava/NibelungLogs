using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.Data;
using NibelungLog.Interfaces;
using NibelungLog.Services;
using NibelungLog.Types.Dto;
using NibelungLog.Types.Encounters;

var serviceCollection = new ServiceCollection();

serviceCollection.AddLogging(builder => builder.AddConsole());

var postgresConnectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") 
    ?? "Host=localhost;Port=5432;Database=nibelunglog;Username=postgres;Password=password";

serviceCollection.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));
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

var authService = serviceProvider.GetRequiredService<IWowCircleAuthService>();

var result = await authService.LoginAsync("godlix", "1010334v", 5);

Console.WriteLine($"Login successful: {result.IsAuth}");
Console.WriteLine($"Account ID: {result.Id}");
Console.WriteLine($"Account Name: {result.Name}");
Console.WriteLine($"Realm: {result.Realm}");

using var scope = serviceProvider.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await dbContext.Database.EnsureCreatedAsync();

var ulduarRaids = await authService.GetUlduarRaidsAsync(5, 1);

Console.WriteLine($"Total Ulduar raids found: {ulduarRaids.Count}");
Console.WriteLine("Processing Ulduar raids...\n");

var dataService = scope.ServiceProvider.GetRequiredService<IRaidDataService>();

var totalEncounters = 0;
var totalPlayerEncounters = 0;
var raidIndex = 0;

foreach (var raid in ulduarRaids)
{
    raidIndex++;
    
    if (raidIndex % 10 == 0)
        Console.WriteLine($"Processed and saved {raidIndex}/{ulduarRaids.Count} raids...");
    
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
}

Console.WriteLine($"\nSaved {raidIndex} Ulduar raids, {totalEncounters} encounters, {totalPlayerEncounters} player encounters to database.");
