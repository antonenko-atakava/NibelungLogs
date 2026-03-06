using System.Net;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Service.Services;

namespace NibelungLog.ParserGuild;

public sealed class Program
{
    public static async Task Main(string[] arguments)
    {
        var environmentFilePath = GetEnvironmentFilePath();

        if (!string.IsNullOrEmpty(environmentFilePath))
        {
            Env.Load(environmentFilePath);
        }

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddLogging(
            builder =>
            {
                builder.AddSimpleConsole(
                    options =>
                    {
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss ";
                        options.UseUtcTimestamp = false;
                        options.IncludeScopes = false;
                    });
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
            });

        var postgresConnectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=nibelunglog;Username=postgres;Password=1234";

        serviceCollection.AddDbContext<ApplicationDbContext>(
            options => options.UseNpgsql(postgresConnectionString));

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
        httpClient.DefaultRequestHeaders.Add(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

        serviceCollection.AddSingleton(httpClient);
        serviceCollection.AddTransient<IWowCircleAuthService, WowCircleAuthService>();
        serviceCollection.AddScoped<GuildParserDataService>();
        serviceCollection.AddScoped<GuildParser>();

        using var serviceProvider = serviceCollection.BuildServiceProvider();

        var logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger<Program>();

        var serverIdText = "5";
        var accountName = "invoker1103";
        var accountPassword = "110320041914";

        if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(accountPassword))
        {
            logger.LogError("Не указаны WOWCIRCLE_LOGIN и WOWCIRCLE_PASSWORD");
            return;
        }

        if (string.IsNullOrEmpty(serverIdText) || !int.TryParse(serverIdText, out var serverId))
        {
            logger.LogError("Не указан или некорректен WOWCIRCLE_SERVER_ID");
            return;
        }

        var wowCircleAuthService = serviceProvider.GetRequiredService<IWowCircleAuthService>();
        var loginResult = await wowCircleAuthService.LoginAsync(accountName, accountPassword, serverId);

        if (!loginResult.IsAuth)
        {
            logger.LogError("Авторизация WowCircle завершилась неуспешно");
            return;
        }

        logger.LogInformation("Авторизация выполнена: {AccountName} ({Realm})", loginResult.Name, loginResult.Realm);

        using var scope = serviceProvider.CreateScope();
        var guildParser = scope.ServiceProvider.GetRequiredService<GuildParser>();

        guildParser.SetAuthorizedHttpClient(httpClient);

        var guildParserOptions = new GuildParserOptions
        {
            ServerId = serverId
        };

        await guildParser.InvokeAsync(guildParserOptions);
    }

    private static string? GetEnvironmentFilePath()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var currentDirectory = Directory.GetCurrentDirectory();
        var assemblyLocation = typeof(Program).Assembly.Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? baseDirectory;
        var projectRoot = Directory.GetParent(baseDirectory)?.Parent?.Parent?.FullName;

        if (string.IsNullOrEmpty(projectRoot))
        {
            var currentSearchDirectory = new DirectoryInfo(baseDirectory);

            while (currentSearchDirectory != null
                   && !File.Exists(Path.Combine(currentSearchDirectory.FullName, ".env"))
                   && currentSearchDirectory.Name != "NibelungLogs")
            {
                currentSearchDirectory = currentSearchDirectory.Parent;
            }

            if (currentSearchDirectory != null)
            {
                projectRoot = currentSearchDirectory.FullName;
            }
        }

        var environmentFilePaths = new[]
        {
            Path.Combine(baseDirectory, ".env"),
            Path.Combine(currentDirectory, ".env"),
            Path.Combine(assemblyDirectory, ".env"),
            projectRoot != null ? Path.Combine(projectRoot, ".env") : null
        };

        return environmentFilePaths
            .Where(environmentFilePath => !string.IsNullOrEmpty(environmentFilePath))
            .Cast<string>()
            .FirstOrDefault(File.Exists);
    }
}
