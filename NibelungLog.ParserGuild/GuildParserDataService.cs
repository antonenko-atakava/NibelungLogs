using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Entities;
using NibelungLog.Domain.Types;
using NibelungLog.Domain.Types.Dto;

namespace NibelungLog.ParserGuild;

public sealed class GuildParserDataService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ApplicationDbContext applicationDbContext;
    private readonly ILogger<GuildParserDataService> logger;

    public GuildParserDataService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        applicationDbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        logger = serviceProvider.GetRequiredService<ILogger<GuildParserDataService>>();
    }

    public async Task SaveAsync(
        List<GuildListItemRecord> guilds,
        Dictionary<string, List<GuildMemberRecord>> guildMembersByGuildId,
        CancellationToken cancellationToken = default)
    {
        if (guilds.Count == 0)
        {
            return;
        }

        var distinctGuilds = guilds
            .GroupBy(guild => guild.GuildId)
            .Select(group => group.First())
            .ToList();

        await SaveGuildsAsync(distinctGuilds, cancellationToken);
        applicationDbContext.ChangeTracker.Clear();

        var guildsByGuildId = await GetGuildsByGuildIdAsync(
            distinctGuilds.Select(guild => guild.GuildId).ToList(),
            cancellationToken);

        var guildMemberImports = guildMembersByGuildId
            .SelectMany(pair => pair.Value.Select(guildMember => (GuildId: pair.Key, GuildMember: guildMember)))
            .ToList();

        if (guildMemberImports.Count == 0)
        {
            logger.LogInformation("Сохранены только гильдии без участников: {GuildCount}", distinctGuilds.Count);
            return;
        }

        applicationDbContext.ChangeTracker.Clear();

        var playersByCharacterGuid = await SavePlayersAsync(
            guildMemberImports.Select(importItem => importItem.GuildMember).ToList(),
            cancellationToken);

        applicationDbContext.ChangeTracker.Clear();

        await SaveGuildMembershipsAsync(
            guildMemberImports,
            guildsByGuildId,
            playersByCharacterGuid,
            cancellationToken);

        logger.LogInformation(
            "Сохранено гильдий: {GuildCount}, участников: {GuildMemberCount}",
            distinctGuilds.Count,
            guildMemberImports.Count);
    }

    private async Task SaveGuildsAsync(List<GuildListItemRecord> guilds, CancellationToken cancellationToken)
    {
        var existingGuildsByGuildId = await GetTrackedGuildsByGuildIdAsync(
            guilds.Select(guild => guild.GuildId).ToList(),
            cancellationToken);

        var createdAt = DateTime.UtcNow;
        var guildsToCreate = new List<Guild>();

        foreach (var guild in guilds)
        {
            if (existingGuildsByGuildId.TryGetValue(guild.GuildId, out var existingGuild))
            {
                existingGuild.GuildName = guild.Name;
                existingGuild.LeaderGuid = guild.LeaderGuid;
                existingGuild.CreateDate = guild.CreateDate;
                existingGuild.LeaderName = guild.LeaderName;
                existingGuild.LastUpdated = createdAt;
                continue;
            }

            guildsToCreate.Add(new Guild
            {
                GuildId = guild.GuildId,
                GuildName = guild.Name,
                LeaderGuid = guild.LeaderGuid,
                CreateDate = guild.CreateDate,
                LeaderName = guild.LeaderName,
                LastUpdated = createdAt
            });
        }

        if (guildsToCreate.Count > 0)
        {
            await applicationDbContext.Guilds.AddRangeAsync(guildsToCreate, cancellationToken);
        }

        await applicationDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Dictionary<string, Player>> SavePlayersAsync(
        List<GuildMemberRecord> guildMembers,
        CancellationToken cancellationToken)
    {
        var distinctGuildMembers = guildMembers
            .GroupBy(guildMember => guildMember.CharacterGuid)
            .Select(group => group.First())
            .ToList();

        var trackedPlayersByCharacterGuid = await GetTrackedPlayersByCharacterGuidAsync(
            distinctGuildMembers.Select(guildMember => guildMember.CharacterGuid).ToList(),
            cancellationToken);

        var playersToCreate = new List<Player>();

        foreach (var guildMember in distinctGuildMembers)
        {
            if (trackedPlayersByCharacterGuid.TryGetValue(guildMember.CharacterGuid, out var existingPlayer))
            {
                if (string.IsNullOrEmpty(existingPlayer.ClassName))
                {
                    existingPlayer.ClassName = ClassMappings.GetClassName(guildMember.CharacterClass);
                }

                continue;
            }

            var createdPlayer = new Player
            {
                CharacterGuid = guildMember.CharacterGuid,
                CharacterName = guildMember.CharacterName,
                CharacterRace = guildMember.CharacterRace,
                CharacterClass = guildMember.CharacterClass,
                ClassName = ClassMappings.GetClassName(guildMember.CharacterClass),
                CharacterGender = guildMember.CharacterGender,
                CharacterLevel = guildMember.CharacterLevel
            };

            playersToCreate.Add(createdPlayer);
            trackedPlayersByCharacterGuid[guildMember.CharacterGuid] = createdPlayer;
        }

        if (playersToCreate.Count > 0)
        {
            await applicationDbContext.Players.AddRangeAsync(playersToCreate, cancellationToken);
        }

        await applicationDbContext.SaveChangesAsync(cancellationToken);

        applicationDbContext.ChangeTracker.Clear();

        return await GetPlayersByCharacterGuidAsync(
            distinctGuildMembers.Select(guildMember => guildMember.CharacterGuid).ToList(),
            cancellationToken);
    }

    private async Task SaveGuildMembershipsAsync(
        List<(string GuildId, GuildMemberRecord GuildMember)> guildMemberImports,
        Dictionary<string, Guild> guildsByGuildId,
        Dictionary<string, Player> playersByCharacterGuid,
        CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var guildMembershipApplicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var guildIdentifiers = guildsByGuildId.Values
            .Select(guild => guild.Id)
            .Distinct()
            .ToList();

        var playerIdentifiers = guildMemberImports
            .Select(importItem => importItem.GuildMember.CharacterGuid)
            .Distinct()
            .Where(playersByCharacterGuid.ContainsKey)
            .Select(characterGuid => playersByCharacterGuid[characterGuid].Id)
            .Distinct()
            .ToList();

        var existingGuildMembersByCompositeKey = await GetTrackedGuildMembersByCompositeKeyAsync(
            guildMembershipApplicationDbContext,
            guildIdentifiers,
            playerIdentifiers,
            cancellationToken);

        var currentTimestamp = DateTime.UtcNow;
        var guildMembersToCreate = new List<GuildMember>();

        foreach (var guildMemberImport in guildMemberImports)
        {
            if (!guildsByGuildId.TryGetValue(guildMemberImport.GuildId, out var guild))
            {
                continue;
            }

            if (!playersByCharacterGuid.TryGetValue(guildMemberImport.GuildMember.CharacterGuid, out var player))
            {
                continue;
            }

            var compositeKey = CreateCompositeKey(guild.Id, player.Id);

            if (existingGuildMembersByCompositeKey.TryGetValue(compositeKey, out var existingGuildMember))
            {
                existingGuildMember.Rank = guildMemberImport.GuildMember.Rank;
                existingGuildMember.LastUpdated = currentTimestamp;
                continue;
            }

            guildMembersToCreate.Add(new GuildMember
            {
                GuildId = guild.Id,
                PlayerId = player.Id,
                Rank = guildMemberImport.GuildMember.Rank,
                JoinedDate = null,
                LastUpdated = currentTimestamp
            });
        }

        if (guildMembersToCreate.Count > 0)
        {
            await guildMembershipApplicationDbContext.GuildMembers.AddRangeAsync(guildMembersToCreate, cancellationToken);
        }

        await guildMembershipApplicationDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Dictionary<string, Guild>> GetGuildsByGuildIdAsync(
        List<string> guildIdentifiers,
        CancellationToken cancellationToken)
    {
        var guildsByGuildId = new Dictionary<string, Guild>();

        foreach (var guildIdentifierBatch in guildIdentifiers.Distinct().Chunk(1000))
        {
            var guildIdentifierList = guildIdentifierBatch.ToList();
            var guilds = await applicationDbContext.Guilds
                .AsNoTracking()
                .Where(guild => guildIdentifierList.Contains(guild.GuildId))
                .ToListAsync(cancellationToken);

            foreach (var guild in guilds)
            {
                guildsByGuildId[guild.GuildId] = guild;
            }
        }

        return guildsByGuildId;
    }

    private async Task<Dictionary<string, Guild>> GetTrackedGuildsByGuildIdAsync(
        List<string> guildIdentifiers,
        CancellationToken cancellationToken)
    {
        var guildsByGuildId = new Dictionary<string, Guild>();

        foreach (var guildIdentifierBatch in guildIdentifiers.Distinct().Chunk(1000))
        {
            var guildIdentifierList = guildIdentifierBatch.ToList();
            var guilds = await applicationDbContext.Guilds
                .Where(guild => guildIdentifierList.Contains(guild.GuildId))
                .ToListAsync(cancellationToken);

            foreach (var guild in guilds)
            {
                guildsByGuildId[guild.GuildId] = guild;
            }
        }

        return guildsByGuildId;
    }

    private async Task<Dictionary<string, Player>> GetPlayersByCharacterGuidAsync(
        List<string> characterGuids,
        CancellationToken cancellationToken)
    {
        var playersByCharacterGuid = new Dictionary<string, Player>();

        foreach (var characterGuidBatch in characterGuids.Distinct().Chunk(1000))
        {
            var characterGuidList = characterGuidBatch.ToList();
            var players = await applicationDbContext.Players
                .AsNoTracking()
                .Where(player => characterGuidList.Contains(player.CharacterGuid))
                .ToListAsync(cancellationToken);

            foreach (var player in players)
            {
                playersByCharacterGuid[player.CharacterGuid] = player;
            }
        }

        return playersByCharacterGuid;
    }

    private async Task<Dictionary<string, Player>> GetTrackedPlayersByCharacterGuidAsync(
        List<string> characterGuids,
        CancellationToken cancellationToken)
    {
        var playersByCharacterGuid = new Dictionary<string, Player>();

        foreach (var characterGuidBatch in characterGuids.Distinct().Chunk(1000))
        {
            var characterGuidList = characterGuidBatch.ToList();
            var players = await applicationDbContext.Players
                .Where(player => characterGuidList.Contains(player.CharacterGuid))
                .ToListAsync(cancellationToken);

            foreach (var player in players)
            {
                playersByCharacterGuid[player.CharacterGuid] = player;
            }
        }

        return playersByCharacterGuid;
    }

    private async Task<Dictionary<string, GuildMember>> GetTrackedGuildMembersByCompositeKeyAsync(
        ApplicationDbContext guildMembershipApplicationDbContext,
        List<int> guildIdentifiers,
        List<int> playerIdentifiers,
        CancellationToken cancellationToken)
    {
        var guildMembersByCompositeKey = new Dictionary<string, GuildMember>();

        foreach (var guildIdentifierBatch in guildIdentifiers.Distinct().Chunk(200))
        {
            var guildIdentifierList = guildIdentifierBatch.ToList();

            foreach (var playerIdentifierBatch in playerIdentifiers.Distinct().Chunk(1000))
            {
                var playerIdentifierList = playerIdentifierBatch.ToList();
                var guildMembers = await guildMembershipApplicationDbContext.GuildMembers
                    .Where(guildMember =>
                        guildIdentifierList.Contains(guildMember.GuildId)
                        && playerIdentifierList.Contains(guildMember.PlayerId))
                    .ToListAsync(cancellationToken);

                foreach (var guildMember in guildMembers)
                {
                    guildMembersByCompositeKey[CreateCompositeKey(guildMember.GuildId, guildMember.PlayerId)] = guildMember;
                }
            }
        }

        return guildMembersByCompositeKey;
    }

    private static string CreateCompositeKey(int guildId, int playerId)
    {
        return $"{guildId}:{playerId}";
    }
}
