using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.Data;
using NibelungLog.DiscordBot.Interfaces;
using NibelungLog.DiscordBot.Models;
using NibelungLog.DiscordBot.Utils;

namespace NibelungLog.DiscordBot.Services;

public sealed class RaidService : IRaidService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RaidService> _logger;

    public RaidService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<RaidService>>();
    }

    public async Task<RaidProgressModel?> GetRaidProgressAsync(string guildName, string raidName, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var raidType = await context.RaidTypes
            .FirstOrDefaultAsync(rt => rt.Name == raidName, cancellationToken);

        if (raidType == null)
            return null;

        var raid = await context.Raids
            .Include(r => r.Encounters)
                .ThenInclude(e => e.PlayerEncounters)
                    .ThenInclude(pe => pe.Player)
            .Where(r => r.RaidTypeId == raidType.Id && r.GuildName == guildName)
            .OrderByDescending(r => r.StartTime)
            .FirstOrDefaultAsync(cancellationToken);

        if (raid == null)
            return null;

        var allPlayerEncounters = raid.Encounters
            .SelectMany(e => e.PlayerEncounters.Select(pe => new
            {
                pe.Player,
                pe.HealingDone,
                pe.Dps,
                Encounter = e
            }))
            .ToList();

        var averageDps = allPlayerEncounters.Any() 
            ? allPlayerEncounters.Average(pe => pe.Dps) 
            : 0;

        var topDpsPlayers = allPlayerEncounters
            .GroupBy(pe => new { pe.Player.CharacterName, pe.Player.ClassName })
            .Select(g => new TopPlayerModel
            {
                PlayerName = g.Key.CharacterName,
                ClassName = g.Key.ClassName ?? "Неизвестно",
                Value = g.Average(pe => pe.Dps)
            })
            .OrderByDescending(p => p.Value)
            .Take(5)
            .ToList();

        var topHealingPlayers = allPlayerEncounters
            .GroupBy(pe => new { pe.Player.CharacterName, pe.Player.ClassName })
            .Select(g =>
            {
                var totalHealing = g.Sum(pe => pe.HealingDone);
                var totalDuration = g.Sum(pe => (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds);
                var averageHps = totalDuration > 0 ? totalHealing / totalDuration : 0;
                return new TopPlayerModel
                {
                    PlayerName = g.Key.CharacterName,
                    ClassName = g.Key.ClassName ?? "Неизвестно",
                    Value = averageHps
                };
            })
            .OrderByDescending(p => p.Value)
            .Take(5)
            .ToList();

        return new RaidProgressModel
        {
            RaidName = raidName,
            GuildName = raid.GuildName,
            StartTime = raid.StartTime,
            TotalTime = raid.TotalTime,
            Wipes = raid.Wipes,
            TotalDamage = raid.TotalDamage,
            TotalHealing = raid.TotalHealing,
            AverageDps = averageDps,
            TopDpsPlayers = topDpsPlayers,
            TopHealingPlayers = topHealingPlayers,
            CompletedBosses = raid.CompletedBosses,
            TotalBosses = raid.TotalBosses
        };
    }

    public async Task<List<RaidProgressModel>> GetLastRaidsAsync(string guildName, string raidName, int count, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var raidType = await context.RaidTypes
            .FirstOrDefaultAsync(rt => rt.Name == raidName, cancellationToken);

        if (raidType == null)
            return [];

        var raids = await context.Raids
            .Include(r => r.Encounters)
                .ThenInclude(e => e.PlayerEncounters)
                    .ThenInclude(pe => pe.Player)
            .Where(r => r.RaidTypeId == raidType.Id && r.GuildName == guildName)
            .OrderByDescending(r => r.StartTime)
            .Take(count)
            .ToListAsync(cancellationToken);

        var result = new List<RaidProgressModel>();

        foreach (var raid in raids)
        {
            var allPlayerEncounters = raid.Encounters
                .SelectMany(e => e.PlayerEncounters.Select(pe => new
                {
                    pe.Player,
                    pe.HealingDone,
                    pe.Dps,
                    Encounter = e
                }))
                .ToList();

            var averageDps = allPlayerEncounters.Any() 
                ? allPlayerEncounters.Average(pe => pe.Dps) 
                : 0;

            var topDpsPlayers = allPlayerEncounters
                .GroupBy(pe => new { pe.Player.CharacterName, pe.Player.ClassName })
                .Select(g => new TopPlayerModel
                {
                    PlayerName = g.Key.CharacterName,
                    ClassName = g.Key.ClassName ?? "Неизвестно",
                    Value = g.Average(pe => pe.Dps)
                })
                .OrderByDescending(p => p.Value)
                .Take(5)
                .ToList();

            var topHealingPlayers = allPlayerEncounters
                .GroupBy(pe => new { pe.Player.CharacterName, pe.Player.ClassName })
                .Select(g =>
                {
                    var totalHealing = g.Sum(pe => pe.HealingDone);
                    var totalDuration = g.Sum(pe => (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds);
                    var averageHps = totalDuration > 0 ? totalHealing / totalDuration : 0;
                    return new TopPlayerModel
                    {
                        PlayerName = g.Key.CharacterName,
                        ClassName = g.Key.ClassName ?? "Неизвестно",
                        Value = averageHps
                    };
                })
                .OrderByDescending(p => p.Value)
                .Take(5)
                .ToList();

            result.Add(new RaidProgressModel
            {
                RaidName = raidName,
                GuildName = raid.GuildName,
                StartTime = raid.StartTime,
                TotalTime = raid.TotalTime,
                Wipes = raid.Wipes,
                TotalDamage = raid.TotalDamage,
                TotalHealing = raid.TotalHealing,
                AverageDps = averageDps,
                TopDpsPlayers = topDpsPlayers,
                TopHealingPlayers = topHealingPlayers,
                CompletedBosses = raid.CompletedBosses,
                TotalBosses = raid.TotalBosses
            });
        }

        return result;
    }

    public async Task<GuildRaidStatsModel> GetGuildRaidStatsAsync(string guildName, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var raids = await context.Raids
            .Include(r => r.RaidType)
            .Where(r => r.GuildName == guildName)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync(cancellationToken);

        var raidStats = raids.Select(raid => new RaidStatsItem
        {
            Id = raid.Id,
            RaidName = raid.RaidType.Name,
            LeaderName = raid.LeaderName,
            StartTime = raid.StartTime,
            CompletedBosses = raid.CompletedBosses,
            TotalBosses = raid.TotalBosses,
            Wipes = raid.Wipes,
            TotalDamage = raid.TotalDamage,
            TotalHealing = raid.TotalHealing,
            TotalTime = raid.TotalTime
        }).ToList();

        return new GuildRaidStatsModel
        {
            GuildName = guildName,
            Raids = raidStats
        };
    }

    public async Task<RaidDetailsModel?> GetRaidDetailsAsync(int raidId, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var raid = await context.Raids
            .Include(r => r.RaidType)
            .Include(r => r.Encounters)
                .ThenInclude(e => e.PlayerEncounters)
            .FirstOrDefaultAsync(r => r.Id == raidId, cancellationToken);

        if (raid == null)
            return null;

        var encounters = raid.Encounters
            .OrderBy(e => e.StartTime)
            .GroupBy(e => e.EncounterEntry)
            .Select(g =>
            {
                var successfulEncounters = g.Where(e => e.Success).ToList();
                var failedEncounters = g.Where(e => !e.Success).ToList();
                var allEncounters = g.ToList();
                var averageDps = allEncounters
                    .Where(e => e.PlayerEncounters.Any())
                    .SelectMany(e => e.PlayerEncounters)
                    .Select(pe => pe.Dps)
                    .DefaultIfEmpty(0)
                    .Average();

                var encounterName = allEncounters.First().EncounterName ?? g.Key;
                encounterName = BossIconMapper.GetDisplayName(encounterName);

                return new EncounterDetailsModel
                {
                    Id = allEncounters.First().Id,
                    EncounterEntry = g.Key,
                    EncounterName = encounterName,
                    StartTime = allEncounters.First().StartTime,
                    EndTime = successfulEncounters.Any() 
                        ? successfulEncounters.OrderByDescending(e => e.EndTime).First().EndTime 
                        : allEncounters.OrderByDescending(e => e.EndTime).First().EndTime,
                    Success = successfulEncounters.Any(),
                    TotalDamage = allEncounters.Sum(e => e.TotalDamage),
                    TotalHealing = allEncounters.Sum(e => e.TotalHealing),
                    AverageDps = averageDps,
                    Attempts = allEncounters.Count,
                    Wipes = failedEncounters.Count,
                    Tanks = allEncounters.First().Tanks,
                    Healers = allEncounters.First().Healers,
                    DamageDealers = allEncounters.First().DamageDealers
                };
            })
            .ToList();

        return new RaidDetailsModel
        {
            RaidId = raid.Id,
            RaidName = raid.RaidType.Name,
            GuildName = raid.GuildName,
            LeaderName = raid.LeaderName,
            StartTime = raid.StartTime,
            TotalTime = raid.TotalTime,
            Wipes = raid.Wipes,
            Encounters = encounters
        };
    }

    public async Task<BossEncounterDetailsModel?> GetEncounterDetailsAsync(int raidId, string bossName, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var raid = await context.Raids
            .Include(r => r.RaidType)
            .Include(r => r.Encounters)
                .ThenInclude(e => e.PlayerEncounters)
                    .ThenInclude(pe => pe.Player)
            .Include(r => r.Encounters)
                .ThenInclude(e => e.PlayerEncounters)
                    .ThenInclude(pe => pe.CharacterSpec)
            .FirstOrDefaultAsync(r => r.Id == raidId, cancellationToken);

        if (raid == null)
            return null;

        var displayBossName = BossIconMapper.GetDisplayName(bossName);
        var encounter = raid.Encounters
            .Where(e => (e.EncounterName != null && (e.EncounterName.Contains(bossName, StringComparison.OrdinalIgnoreCase) ||
                                                      BossIconMapper.GetDisplayName(e.EncounterName) == displayBossName)) ||
                        e.EncounterEntry.Contains(bossName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.StartTime)
            .FirstOrDefault();

        if (encounter == null)
            return null;

        var encounterDuration = (encounter.EndTime - encounter.StartTime).TotalSeconds;
        var players = encounter.PlayerEncounters
            .Select(pe => new PlayerEncounterDetailsModel
            {
                PlayerName = pe.Player.CharacterName,
                ClassName = pe.Player.ClassName ?? "Неизвестно",
                SpecName = pe.CharacterSpec.Name ?? pe.CharacterSpec.Spec,
                Role = pe.Role,
                Dps = pe.Dps,
                Hps = encounterDuration > 0 ? pe.HealingDone / encounterDuration : 0,
                DamageDone = pe.DamageDone,
                HealingDone = pe.HealingDone
            })
            .OrderByDescending(p => p.Dps)
            .ToList();

        return new BossEncounterDetailsModel
        {
            RaidId = raid.Id,
            RaidName = raid.RaidType.Name,
            GuildName = raid.GuildName,
            BossName = displayBossName,
            StartTime = encounter.StartTime,
            EndTime = encounter.EndTime,
            Success = encounter.Success,
            TotalTime = (long)encounterDuration,
            Players = players
        };
    }
}
