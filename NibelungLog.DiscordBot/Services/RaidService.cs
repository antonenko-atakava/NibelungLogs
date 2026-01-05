using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.Data;
using NibelungLog.DiscordBot.Interfaces;
using NibelungLog.DiscordBot.Models;

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
}
