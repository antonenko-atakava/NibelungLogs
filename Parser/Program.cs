using System.Diagnostics;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.DAL.Data;
using NibelungLog.DAL.Repositories;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Service.Services;

namespace Parser;

class Program
{
    static async Task Main(string[] arguments)
    {
        var serviceCollection = new ServiceCollection();
        
        serviceCollection.AddLogging(builder =>
        {
            builder.AddConsole();
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
            BaseAddress = new Uri("https://cp.wowcircle.net"),
            Timeout = TimeSpan.FromMinutes(5)
        };

        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        httpClient.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.9");
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

        serviceCollection.AddSingleton(httpClient);
        serviceCollection.AddTransient<IWowCircleAuthService, WowCircleAuthService>();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var authService = serviceProvider.GetRequiredService<IWowCircleAuthService>();
        
        var accountName = Environment.GetEnvironmentVariable("WOWCIRCLE_LOGIN") ?? "invoker1103";
        var accountPassword = Environment.GetEnvironmentVariable("WOWCIRCLE_PASSWORD") ?? "110320041914";
        var serverId = 5;

        if (!string.IsNullOrEmpty(accountName) && !string.IsNullOrEmpty(accountPassword))
        {
            Console.WriteLine("Авторизация через WowCircleAuthService...");
            var loginResult = await authService.LoginAsync(accountName, accountPassword, serverId);
        }
        else
        {
            Console.WriteLine("WARNING: WOWCIRCLE_LOGIN и WOWCIRCLE_PASSWORD не установлены. Используется неавторизованный режим.");
        }

        var raidTypeRepository = serviceProvider.GetRequiredService<IRaidTypeRepository>();
        var allRaidTypes = await raidTypeRepository.GetAllAsync();
        
        Console.WriteLine($"Loaded {allRaidTypes.Count} RaidTypes from database:");
        foreach (var raidType in allRaidTypes)
        {
            Console.WriteLine($"  - {raidType.Name}: Map={raidType.Map}, Difficulty={raidType.Difficulty}, InstanceType={raidType.InstanceType}");
        }

        if (allRaidTypes.Count == 0)
        {
            Console.WriteLine("ERROR: No RaidTypes found in database. Please add RaidTypes first.");
            return;
        }

        var mapIds = allRaidTypes.Select(rt => int.Parse(rt.Map)).Distinct().ToArray();
        
        var parserOptions = new ParserOptions
        {
            ServerId = serverId,
            RealmId = 5,
            MapIds = mapIds,
            SaveBatchSize = 50,
            RaidTypes = allRaidTypes
        };
        
        var parser = new Parser(serviceProvider.GetRequiredService<IRaidDataService>());

        parser.SetAuthorizedHttpClient(httpClient);

        var mapModels = await parser.GetDungeonMapsAsync(5);
        
        if (mapModels != null)
        {
            foreach (var mapModel in mapModels)
                Console.WriteLine($"{mapModel.Id} - {mapModel.NameLocalized}");
        }

        await parser.InvokeAsync(parserOptions);

        var q = 0;
    }
}