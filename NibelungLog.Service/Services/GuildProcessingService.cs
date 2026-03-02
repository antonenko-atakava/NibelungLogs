using Microsoft.Extensions.Logging;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types.Dto;

namespace NibelungLog.Service.Services;

public sealed class GuildProcessingService : IGuildProcessingService
{
    private readonly IGuildParserService _guildParserService;
    private readonly IGuildDataService _guildDataService;
    private readonly IGuildRepository _guildRepository;
    private readonly ILogger<GuildProcessingService> _logger;

    public GuildProcessingService(
        IGuildParserService guildParserService,
        IGuildDataService guildDataService,
        IGuildRepository guildRepository,
        ILogger<GuildProcessingService> logger)
    {
        _guildParserService = guildParserService;
        _guildDataService = guildDataService;
        _guildRepository = guildRepository;
        _logger = logger;
    }

    public async Task ProcessGuildAsync(string guildName, string guildId, int serverId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("───────────────────────────────────────────────────────────");
        _logger.LogInformation("Обработка гильдии: {GuildName} ({GuildId})", guildName, guildId);
        _logger.LogInformation("───────────────────────────────────────────────────────────");

        var members = await _guildParserService.GetGuildMembersAsync(guildId, serverId, cancellationToken);

        var guildInfo = new GuildInfoRecord
        {
            GuildId = guildId,
            GuildName = guildName
        };

        _logger.LogInformation("Найдено участников: {Count}", members.Count);

        if (members.Count == 0)
        {
            _logger.LogWarning("⚠️  Участники не найдены");
            return;
        }

        _logger.LogInformation("Сохранение данных в базу...");
        await _guildDataService.SaveGuildDataAsync(guildInfo, members, cancellationToken);

        _logger.LogInformation("✅ Данные гильдии успешно сохранены: {Count} участников", members.Count);
    }

    public async Task ProcessAllGuildsAsync(int serverId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("═══════════════════════════════════════════════════════════");
        _logger.LogInformation("Этап 1: Парсинг списка гильдий");
        _logger.LogInformation("═══════════════════════════════════════════════════════════");

        var allGuilds = await _guildParserService.GetAllGuildsAsync(serverId, cancellationToken);

        _logger.LogInformation("Найдено гильдий: {Count}", allGuilds.Count);

        if (allGuilds.Count == 0)
        {
            _logger.LogWarning("⚠️  Гильдии не найдены");
            return;
        }

        var filteredGuilds = allGuilds
            .Where(g => int.TryParse(g.MembersCount, out var count) && count > 10)
            .ToList();

        _logger.LogInformation("Гильдий с количеством участников > 10: {Count}", filteredGuilds.Count);

        _logger.LogInformation("───────────────────────────────────────────────────────────");
        _logger.LogInformation("Сохранение гильдий в базу данных...");
        _logger.LogInformation("───────────────────────────────────────────────────────────");

        var savedGuilds = 0;
        foreach (var guildItem in filteredGuilds)
        {
            var guildInfo = new GuildInfoRecord
            {
                GuildId = guildItem.GuildId,
                GuildName = guildItem.Name
            };

            await _guildDataService.SaveGuildAsync(guildInfo, cancellationToken);
            savedGuilds++;

            if (savedGuilds % 10 == 0)
            {
                _logger.LogInformation("Сохранено гильдий: {Current}/{Total}", savedGuilds, filteredGuilds.Count);
            }
        }

        _logger.LogInformation("✅ Сохранено гильдий: {Count}", savedGuilds);

        _logger.LogInformation("═══════════════════════════════════════════════════════════");
        _logger.LogInformation("Этап 2: Парсинг участников гильдий");
        _logger.LogInformation("═══════════════════════════════════════════════════════════");

        var totalMembers = 0;
        var guildIndex = 0;
        var startTime = DateTime.Now;
        const int pageLimit = 25;

        _logger.LogInformation("───────────────────────────────────────────────────────────");
        _logger.LogInformation("Обработка участников гильдий...");
        _logger.LogInformation("───────────────────────────────────────────────────────────");

        foreach (var guildItem in filteredGuilds)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            guildIndex++;

            try
            {
                _logger.LogInformation("Обработка гильдии {Index}/{Total}: {GuildName} ({GuildId}) | Участников: {Members}",
                    guildIndex, filteredGuilds.Count, guildItem.Name, guildItem.GuildId, guildItem.MembersCount);

                var page = 1;
                var hasMorePages = true;
                var guildMembersCount = 0;

                while (hasMorePages)
                {
                    var members = await _guildParserService.GetGuildMembersPageAsync(
                        guildItem.GuildId, serverId, page, pageLimit, cancellationToken);

                    if (members.Count == 0)
                    {
                        hasMorePages = false;
                        break;
                    }

                    await _guildDataService.SaveGuildMembersPageAsync(guildItem.GuildId, members, cancellationToken);

                    guildMembersCount += members.Count;
                    totalMembers += members.Count;

                    _logger.LogInformation("  Страница {Page}: сохранено {Count} участников | Всего для гильдии: {Total}",
                        page, members.Count, guildMembersCount);

                    if (members.Count < pageLimit)
                        hasMorePages = false;
                    else
                        page++;

                    await Task.Delay(300, cancellationToken);
                }

                _logger.LogInformation("✅ Обработано: {GuildName} | Участников: {Count}", guildItem.Name, guildMembersCount);

                if (guildIndex % 5 == 0 || guildIndex == filteredGuilds.Count)
                {
                    var progress = (double)guildIndex / filteredGuilds.Count * 100;
                    _logger.LogInformation("Прогресс: {Current}/{Total} ({Progress:F1}%) | Участников: {Members}",
                        guildIndex, filteredGuilds.Count, progress, totalMembers);
                }

                await Task.Delay(500, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при обработке гильдии {GuildName} ({GuildId})", guildItem.Name, guildItem.GuildId);
            }
        }

        var elapsedTime = DateTime.Now - startTime;

        _logger.LogInformation("───────────────────────────────────────────────────────────");
        _logger.LogInformation("✅ Обработка всех гильдий завершена");
        _logger.LogInformation("───────────────────────────────────────────────────────────");
        _logger.LogInformation("Обработано:");
        _logger.LogInformation("  • Гильдий: {Guilds}", guildIndex);
        _logger.LogInformation("  • Всего участников: {Members}", totalMembers);
        _logger.LogInformation("Время выполнения: {Time}", elapsedTime.ToString(@"mm\:ss"));
        _logger.LogInformation("═══════════════════════════════════════════════════════════");
    }
}
