using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.Data;
using NibelungLog.DiscordBot.Interfaces;
using NibelungLog.DiscordBot.Models;

namespace NibelungLog.DiscordBot.Services;

public sealed class GuildService : IGuildService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GuildService> _logger;

    public GuildService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<GuildService>>();
    }

    public async Task<GuildInfoModel?> GetGuildInfoAsync(string guildName, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var guild = await context.Guilds
            .Include(g => g.Members)
                .ThenInclude(m => m.Player)
            .FirstOrDefaultAsync(g => g.GuildName == guildName, cancellationToken);

        if (guild == null)
            return null;

        var membersByRank = guild.Members
            .GroupBy(m => m.Rank)
            .ToDictionary(g => g.Key, g => g.Count());

        var membersByClass = guild.Members
            .Where(m => m.Player.ClassName != null)
            .GroupBy(m => m.Player.ClassName!)
            .ToDictionary(g => g.Key, g => g.Count());

        return new GuildInfoModel
        {
            GuildName = guild.GuildName,
            TotalMembers = guild.Members.Count,
            LastUpdated = guild.LastUpdated,
            MembersByRank = membersByRank,
            MembersByClass = membersByClass
        };
    }

    public async Task<TopPlayersModel> GetTopPlayersAsync(string guildName, int count, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var guild = await context.Guilds
            .Include(g => g.Members)
                .ThenInclude(m => m.Player)
            .FirstOrDefaultAsync(g => g.GuildName == guildName, cancellationToken);

        if (guild == null)
            return new TopPlayersModel { Players = [] };

        var guildPlayerIds = guild.Members.Select(m => m.PlayerId).ToList();

        var topPlayers = await context.PlayerEncounters
            .Include(pe => pe.Player)
            .Where(pe => guildPlayerIds.Contains(pe.PlayerId))
            .GroupBy(pe => new { pe.PlayerId, pe.Player.CharacterName, pe.Player.ClassName })
            .Select(g => new TopPlayerDpsModel
            {
                PlayerName = g.Key.CharacterName,
                ClassName = g.Key.ClassName ?? "Неизвестно",
                MaxDps = g.Max(pe => pe.Dps)
            })
            .OrderByDescending(p => p.MaxDps)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new TopPlayersModel { Players = topPlayers };
    }

    public async Task<TopPlayersModel> GetTopPlayersByClassAsync(string guildName, string className, int count, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var guild = await context.Guilds
            .Include(g => g.Members)
                .ThenInclude(m => m.Player)
            .FirstOrDefaultAsync(g => g.GuildName == guildName, cancellationToken);

        if (guild == null)
            return new TopPlayersModel { Players = [] };

        var guildPlayerIds = guild.Members.Select(m => m.PlayerId).ToList();

        var topPlayers = await context.PlayerEncounters
            .Include(pe => pe.Player)
            .Where(pe => guildPlayerIds.Contains(pe.PlayerId) && pe.Player.ClassName == className)
            .GroupBy(pe => new { pe.PlayerId, pe.Player.CharacterName, pe.Player.ClassName })
            .Select(g => new TopPlayerDpsModel
            {
                PlayerName = g.Key.CharacterName,
                ClassName = g.Key.ClassName ?? "Неизвестно",
                MaxDps = g.Max(pe => pe.Dps)
            })
            .OrderByDescending(p => p.MaxDps)
            .Take(count)
            .ToListAsync(cancellationToken);

        return new TopPlayersModel { Players = topPlayers };
    }
}

