using Microsoft.Extensions.Logging;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Types.Dto;

namespace NibelungLog.Service.Services;

public sealed class GuildProcessingService : IGuildProcessingService
{
    private readonly IGuildParserService _guildParserService;
    private readonly IGuildDataService _guildDataService;
    private readonly ILogger<GuildProcessingService> _logger;

    public GuildProcessingService(
        IGuildParserService guildParserService,
        IGuildDataService guildDataService,
        ILogger<GuildProcessingService> logger)
    {
        _guildParserService = guildParserService;
        _guildDataService = guildDataService;
        _logger = logger;
    }

    public async Task ProcessGuildAsync(string guildName, string guildId, int serverId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("───────────────────────────────────────────────────────────");
        _logger.LogInformation("Загрузка участников гильдии...");
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

        _logger.LogInformation("───────────────────────────────────────────────────────────");
        _logger.LogInformation("✅ Данные гильдии успешно сохранены");
        _logger.LogInformation("  • Участников: {Count}", members.Count);
        _logger.LogInformation("═══════════════════════════════════════════════════════════");
    }
}
