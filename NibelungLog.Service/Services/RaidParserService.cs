using Microsoft.Extensions.Logging;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Types.Dto;

namespace NibelungLog.Service.Services;

public sealed class RaidParserService : IRaidParserService
{
    private readonly IWowCircleAuthService _authService;
    private readonly IRaidDataService _raidDataService;
    private readonly ILogger<RaidParserService> _logger;

    public RaidParserService(
        IWowCircleAuthService authService,
        IRaidDataService raidDataService,
        ILogger<RaidParserService> logger)
    {
        _authService = authService;
        _raidDataService = raidDataService;
        _logger = logger;
    }

    public async Task ParseRaidsAsync(int serverId, string mapId, int difficulty, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Поиск рейдов...");
        var raids = await _authService.GetRaidsByMapAsync(serverId, mapId, difficulty, cancellationToken);

        _logger.LogInformation("Найдено рейдов: {Count}", raids.Count);

        if (raids.Count == 0)
        {
            _logger.LogWarning("⚠️  Рейды не найдены");
            return;
        }

        var totalEncounters = 0;
        var totalPlayerEncounters = 0;
        var raidIndex = 0;
        var startTime = DateTime.Now;

        _logger.LogInformation("───────────────────────────────────────────────────────────");
        _logger.LogInformation("Обработка рейдов...");
        _logger.LogInformation("───────────────────────────────────────────────────────────");

        foreach (var raid in raids)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            raidIndex++;

            var encounters = await _authService.GetRaidDetailsAsync(serverId, raid.Id, cancellationToken);
            var successfulEncounters = encounters.Where(e => e.Success == "1").ToList();

            var raidEncounters = new List<EncounterRecord>();
            var raidPlayerEncounters = new List<PlayerEncounterRecord>();

            foreach (var encounter in successfulEncounters)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                raidEncounters.Add(encounter);

                var players = await _authService.GetEncounterPlayersAsync(serverId, raid.Id, encounter.EncounterEntry, encounter.StartTime, cancellationToken);
                raidPlayerEncounters.AddRange(players);
            }

            await _raidDataService.SaveSingleRaidDataAsync(raid, raidEncounters, raidPlayerEncounters, cancellationToken);
            await Task.Delay(300, cancellationToken);
            
            
            totalEncounters += raidEncounters.Count;
            totalPlayerEncounters += raidPlayerEncounters.Count;

            if (raidIndex % 10 == 0 || raidIndex == raids.Count)
            {
                var progress = (double)raidIndex / raids.Count * 100;
                _logger.LogInformation("Прогресс: {Current}/{Total} ({Progress:F1}%) | Энкаунтеров: {Encounters} | Игроков: {Players}",
                    raidIndex, raids.Count, progress, totalEncounters, totalPlayerEncounters);
            }
        }

        var elapsedTime = DateTime.Now - startTime;

        _logger.LogInformation("───────────────────────────────────────────────────────────");
        _logger.LogInformation("✅ Обработка завершена");
        _logger.LogInformation("───────────────────────────────────────────────────────────");
        _logger.LogInformation("Сохранено:");
        _logger.LogInformation("  • Рейдов: {Raids}", raidIndex);
        _logger.LogInformation("  • Энкаунтеров: {Encounters}", totalEncounters);
        _logger.LogInformation("  • Записей игроков: {Players}", totalPlayerEncounters);
        _logger.LogInformation("Время выполнения: {Time}", elapsedTime.ToString(@"mm\:ss"));
        _logger.LogInformation("═══════════════════════════════════════════════════════════");
    }
}
