using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.Data;
using NibelungLog.Entities;
using NibelungLog.GuildWorker.Interfaces;
using NibelungLog.Types;
using NibelungLog.Types.Dto;

namespace NibelungLog.GuildWorker.Services;

public sealed class GuildDataService : IGuildDataService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GuildDataService> _logger;

    public GuildDataService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<GuildDataService>>();
    }

    public async Task SaveGuildDataAsync(GuildInfoRecord guildInfo, List<GuildMemberRecord> members, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var guild = await context.Guilds
            .FirstOrDefaultAsync(g => g.GuildId == guildInfo.GuildId, cancellationToken);

        if (guild == null)
        {
            guild = new Guild
            {
                GuildId = guildInfo.GuildId,
                GuildName = guildInfo.GuildName,
                LastUpdated = DateTime.UtcNow
            };
            context.Guilds.Add(guild);
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created new guild: {GuildName} ({GuildId})", guildInfo.GuildName, guildInfo.GuildId);
        }
        else
        {
            guild.GuildName = guildInfo.GuildName;
            guild.LastUpdated = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated guild: {GuildName} ({GuildId})", guildInfo.GuildName, guildInfo.GuildId);
        }

        var playersDict = new Dictionary<string, Player>();

        foreach (var memberRecord in members)
        {
            if (!playersDict.ContainsKey(memberRecord.CharacterGuid))
            {
                var player = await context.Players
                    .FirstOrDefaultAsync(p => p.CharacterGuid == memberRecord.CharacterGuid, cancellationToken);

                if (player == null)
                {
                    player = new Player
                    {
                        CharacterGuid = memberRecord.CharacterGuid,
                        CharacterName = memberRecord.CharacterName,
                        CharacterRace = memberRecord.CharacterRace,
                        CharacterClass = memberRecord.CharacterClass,
                        ClassName = ClassMappings.GetClassName(memberRecord.CharacterClass),
                        CharacterGender = memberRecord.CharacterGender,
                        CharacterLevel = memberRecord.CharacterLevel
                    };
                    context.Players.Add(player);
                    await context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Created new player: {PlayerName} ({CharacterGuid})", memberRecord.CharacterName, memberRecord.CharacterGuid);
                }
                else
                {
                    if (string.IsNullOrEmpty(player.ClassName))
                    {
                        player.ClassName = ClassMappings.GetClassName(memberRecord.CharacterClass);
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }

                playersDict[memberRecord.CharacterGuid] = player;
            }

            var playerEntity = playersDict[memberRecord.CharacterGuid];

            var guildMember = await context.GuildMembers
                .FirstOrDefaultAsync(gm => gm.GuildId == guild.Id && gm.PlayerId == playerEntity.Id, cancellationToken);

            if (guildMember == null)
            {
                guildMember = new GuildMember
                {
                    GuildId = guild.Id,
                    PlayerId = playerEntity.Id,
                    Rank = memberRecord.Rank,
                    JoinedDate = null,
                    LastUpdated = DateTime.UtcNow
                };
                context.GuildMembers.Add(guildMember);
                _logger.LogInformation("Added member to guild: {PlayerName} as {Rank}", memberRecord.CharacterName, memberRecord.Rank);
            }
            else
            {
                guildMember.Rank = memberRecord.Rank;
                guildMember.LastUpdated = DateTime.UtcNow;
                _logger.LogInformation("Updated member in guild: {PlayerName} as {Rank}", memberRecord.CharacterName, memberRecord.Rank);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Saved {Count} members for guild {GuildName}", members.Count, guildInfo.GuildName);
    }
}

