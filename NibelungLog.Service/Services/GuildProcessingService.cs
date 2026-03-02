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
        var guilds = await _guildRepository.GetAllAsync(cancellationToken);

        _logger.LogInformation("Найдено гильдий в базе: {Count}", guilds.Count);

        if (guilds.Count == 0)
        {
            _logger.LogWarning("⚠️  Гильдии не найдены в базе данных");
            return;
        }

        var totalProcessed = 0;
        var totalMembers = 0;
        var guildIndex = 0;
        var startTime = DateTime.Now;

        _logger.LogInformation("───────────────────────────────────────────────────────────");
        _logger.LogInformation("Обработка гильдий...");
        _logger.LogInformation("───────────────────────────────────────────────────────────");

        foreach (var guild in guilds)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            guildIndex++;

            try
            {
                _logger.LogInformation("Обработка гильдии {Index}/{Total}: {GuildName} ({GuildId})",
                    guildIndex, guilds.Count, guild.GuildName, guild.GuildId);

                var members = await _guildParserService.GetGuildMembersAsync(guild.GuildId, serverId, cancellationToken);

                if (members.Count > 0)
                {
                    var guildInfo = new GuildInfoRecord
                    {
                        GuildId = guild.GuildId,
                        GuildName = guild.GuildName
                    };

                    await _guildDataService.SaveGuildDataAsync(guildInfo, members, cancellationToken);
                    totalMembers += members.Count;
                    totalProcessed++;

                    _logger.LogInformation("✅ Обработано: {GuildName} | Участников: {Count}", guild.GuildName, members.Count);
                }
                else
                {
                    _logger.LogWarning("⚠️  Участники не найдены для гильдии {GuildName}", guild.GuildName);
                }

                if (guildIndex % 5 == 0 || guildIndex == guilds.Count)
                {
                    var progress = (double)guildIndex / guilds.Count * 100;
                    _logger.LogInformation("Прогресс: {Current}/{Total} ({Progress:F1}%) | Обработано: {Processed} | Участников: {Members}",
                        guildIndex, guilds.Count, progress, totalProcessed, totalMembers);
                }

                await Task.Delay(500, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при обработке гильдии {GuildName} ({GuildId})", guild.GuildName, guild.GuildId);
            }
        }

        var elapsedTime = DateTime.Now - startTime;

        _logger.LogInformation("───────────────────────────────────────────────────────────");
        _logger.LogInformation("✅ Обработка всех гильдий завершена");
        _logger.LogInformation("───────────────────────────────────────────────────────────");
        _logger.LogInformation("Обработано:");
        _logger.LogInformation("  • Гильдий: {Guilds}", totalProcessed);
        _logger.LogInformation("  • Всего участников: {Members}", totalMembers);
        _logger.LogInformation("Время выполнения: {Time}", elapsedTime.ToString(@"mm\:ss"));
        _logger.LogInformation("═══════════════════════════════════════════════════════════");
    }
}
