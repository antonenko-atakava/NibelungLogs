using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Types.Dto;

namespace NibelungLog.Service.Services;

public sealed class RaidParserService : IRaidParserService
{
    private readonly IWowCircleAuthService _authService;
    private readonly IRaidDataService _raidDataService;
    private readonly ILogger<RaidParserService> _logger;
    private readonly SemaphoreSlim _raidSemaphore;
    private readonly SemaphoreSlim _encounterSemaphore;
    private readonly SemaphoreSlim _batchSaveSemaphore;

    public RaidParserService(
        IWowCircleAuthService authService,
        IRaidDataService raidDataService,
        ILogger<RaidParserService> logger)
    {
        _authService = authService;
        _raidDataService = raidDataService;
        _logger = logger;
        _raidSemaphore = new SemaphoreSlim(3, 3);
        _encounterSemaphore = new SemaphoreSlim(10, 10);
        _batchSaveSemaphore = new SemaphoreSlim(1, 1);
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
        var lastProgressUpdate = DateTime.Now;
        var progressLock = new object();
        const int batchSize = 50;
        var batchBuffer = new ConcurrentQueue<(RaidRecord Raid, List<EncounterRecord> Encounters, List<PlayerEncounterRecord> PlayerEncounters)>();
        var batchLock = new object();
        var processedCount = 0;
        var last503Time = DateTime.MinValue;
        var adaptiveDelay = 200;
        var delayLock = new object();

        _logger.LogInformation("═══════════════════════════════════════════════════════════");
        _logger.LogInformation("Обработка рейдов (параллелизм: 3 рейда, батч: {BatchSize} рейдов)...", batchSize);
        _logger.LogInformation("═══════════════════════════════════════════════════════════");

        var raidTasks = raids.Select(async raid =>
        {
            await _raidSemaphore.WaitAsync(cancellationToken);
            var raidStartTime = DateTime.Now;
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return (0, 0);

                var currentIndex = Interlocked.Increment(ref raidIndex);
                
                int currentDelay;
                lock (delayLock)
                {
                    currentDelay = adaptiveDelay;
                    if (DateTime.Now - last503Time < TimeSpan.FromMinutes(1))
                    {
                        currentDelay = Math.Min(adaptiveDelay * 2, 1000);
                    }
                }
                
                var encounters = await _authService.GetRaidDetailsAsync(serverId, raid.Id, cancellationToken);
                await Task.Delay(currentDelay, cancellationToken);
                
                var successfulEncounters = encounters.Where(e => e.Success == "1").ToList();

                var raidEncounters = new List<EncounterRecord>();
                var raidPlayerEncounters = new List<PlayerEncounterRecord>();

                if (successfulEncounters.Count > 0)
                {
                    var encounterTasks = successfulEncounters.Select(async encounter =>
                    {
                        await _encounterSemaphore.WaitAsync(cancellationToken);
                        try
                        {
                            return await _authService.GetEncounterPlayersAsync(serverId, raid.Id, encounter.EncounterEntry, encounter.StartTime, cancellationToken);
                        }
                        finally
                        {
                            _encounterSemaphore.Release();
                        }
                    });

                    var playersResults = await Task.WhenAll(encounterTasks);

                    foreach (var encounter in successfulEncounters)
                    {
                        raidEncounters.Add(encounter);
                    }

                    foreach (var players in playersResults)
                    {
                        raidPlayerEncounters.AddRange(players);
                    }

                    await Task.Delay(currentDelay, cancellationToken);
                }

                batchBuffer.Enqueue((raid, raidEncounters.ToList(), raidPlayerEncounters.ToList()));
                
                var encountersCount = raidEncounters.Count;
                var playersCount = raidPlayerEncounters.Count;

                Interlocked.Add(ref totalEncounters, encountersCount);
                Interlocked.Add(ref totalPlayerEncounters, playersCount);

                var shouldSaveBatch = false;
                lock (batchLock)
                {
                    processedCount++;
                    if (processedCount % batchSize == 0 || processedCount == raids.Count)
                    {
                        shouldSaveBatch = true;
                    }
                }

                if (shouldSaveBatch)
                {
                    await _batchSaveSemaphore.WaitAsync(cancellationToken);
                    try
                    {
                        if (!batchBuffer.IsEmpty)
                        {
                            await SaveBatchAsync(batchBuffer, cancellationToken);
                        }
                    }
                    finally
                    {
                        _batchSaveSemaphore.Release();
                    }
                }

                var totalElapsed = DateTime.Now - startTime;
                var avgTimePerRaid = totalElapsed.TotalSeconds / currentIndex;
                var estimatedTimeRemaining = TimeSpan.FromSeconds(avgTimePerRaid * (raids.Count - currentIndex));
                var speed = currentIndex / totalElapsed.TotalMinutes;
                var progress = (double)currentIndex / raids.Count * 100;

                lock (progressLock)
                {
                    if (DateTime.Now - lastProgressUpdate >= TimeSpan.FromMilliseconds(500) || currentIndex == raids.Count)
                    {
                        lastProgressUpdate = DateTime.Now;
                        DrawProgressBar(currentIndex, raids.Count, progress, totalElapsed, estimatedTimeRemaining, speed, totalEncounters, totalPlayerEncounters);
                    }
                }

                raidEncounters.Clear();
                raidPlayerEncounters.Clear();
                successfulEncounters.Clear();
                encounters.Clear();
                
                if (currentIndex % 50 == 0)
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
                    GC.WaitForPendingFinalizers();
                }

                return (encountersCount, playersCount);
            }
            catch (Exception ex)
            {
                lock (delayLock)
                {
                    if (ex.Message.Contains("503") || ex.Message.Contains("Rate limit"))
                    {
                        last503Time = DateTime.Now;
                        adaptiveDelay = Math.Min(adaptiveDelay + 100, 1000);
                    }
                    else
                    {
                        if (DateTime.Now - last503Time > TimeSpan.FromMinutes(2))
                        {
                            adaptiveDelay = Math.Max(adaptiveDelay - 10, 200);
                        }
                    }
                }
                
                _logger.LogError(ex, "❌ Ошибка при обработке рейда {RaidId}: {Message}", raid.Id, ex.Message);
                return (0, 0);
            }
            finally
            {
                _raidSemaphore.Release();
            }
        });

        await Task.WhenAll(raidTasks);

        await _batchSaveSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (!batchBuffer.IsEmpty)
            {
                await SaveBatchAsync(batchBuffer, cancellationToken);
            }
        }
        finally
        {
            _batchSaveSemaphore.Release();
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

    private void DrawProgressBar(int current, int total, double progress, TimeSpan elapsed, TimeSpan remaining, double speed, int encounters, int players)
    {
        const int barWidth = 50;
        var filled = (int)(progress / 100 * barWidth);
        var empty = barWidth - filled;

        var progressBar = new string('█', filled) + new string('░', empty);
        
        var elapsedStr = elapsed.TotalHours >= 1 
            ? elapsed.ToString(@"hh\:mm\:ss") 
            : elapsed.ToString(@"mm\:ss");
        var remainingStr = remaining.TotalHours >= 1 
            ? remaining.ToString(@"hh\:mm\:ss") 
            : remaining.ToString(@"mm\:ss");
        
        Console.Write($"\r[{current}/{total}] {progressBar} {progress:F1}% | ⏱ {elapsedStr} | ⏳ {remainingStr} | 🚀 {speed:F1}/мин | 📊 {encounters} энк. | 👥 {players} игр.");
        
        if (current == total)
            Console.WriteLine();
    }

    private async Task SaveBatchAsync(ConcurrentQueue<(RaidRecord Raid, List<EncounterRecord> Encounters, List<PlayerEncounterRecord> PlayerEncounters)> buffer, CancellationToken cancellationToken)
    {
        if (buffer.IsEmpty)
            return;

        var raids = new List<RaidRecord>();
        var encounters = new List<EncounterRecord>();
        var playerEncounters = new List<PlayerEncounterRecord>();

        while (buffer.TryDequeue(out var item))
        {
            raids.Add(item.Raid);
            encounters.AddRange(item.Encounters);
            playerEncounters.AddRange(item.PlayerEncounters);
        }

        if (raids.Count > 0)
        {
            await _raidDataService.SaveRaidDataAsync(raids, encounters, playerEncounters, cancellationToken);
        }
    }
}
