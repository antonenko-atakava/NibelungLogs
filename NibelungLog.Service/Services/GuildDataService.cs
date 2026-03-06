using Microsoft.Extensions.Logging;
using NibelungLog.Domain.Entities;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types;
using NibelungLog.Domain.Types.Dto;

namespace NibelungLog.Service.Services;

public sealed class GuildDataService : IGuildDataService
{
    private readonly IGuildRepository _guildRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IGuildMemberRepository _guildMemberRepository;
    private readonly ILogger<GuildDataService> _logger;

    public GuildDataService(
        IGuildRepository guildRepository,
        IPlayerRepository playerRepository,
        IGuildMemberRepository guildMemberRepository,
        ILogger<GuildDataService> logger)
    {
        _guildRepository = guildRepository;
        _playerRepository = playerRepository;
        _guildMemberRepository = guildMemberRepository;
        _logger = logger;
    }

    public async Task SaveGuildDataAsync(GuildInfoRecord guildInfo, List<GuildMemberRecord> members, CancellationToken cancellationToken = default)
    {
        var guild = await _guildRepository.FindByGuildIdAsync(guildInfo.GuildId, cancellationToken);

        if (guild == null)
        {
            guild = new Guild
            {
                GuildId = guildInfo.GuildId,
                GuildName = guildInfo.GuildName,
                LeaderGuid = string.Empty,
                CreateDate = string.Empty,
                LeaderName = string.Empty,
                LastUpdated = DateTime.UtcNow
            };
            await _guildRepository.AddAsync(guild, cancellationToken);
            _logger.LogInformation("Created new guild: {GuildName} ({GuildId})", guildInfo.GuildName, guildInfo.GuildId);
        }
        else
        {
            guild.GuildName = guildInfo.GuildName;
            guild.LastUpdated = DateTime.UtcNow;
            await _guildRepository.UpdateAsync(guild, cancellationToken);
            _logger.LogInformation("Updated guild: {GuildName} ({GuildId})", guildInfo.GuildName, guildInfo.GuildId);
        }

        var playersByCharacterGuid = await GetOrCreatePlayersAsync(members, cancellationToken);
        await SaveGuildMembersAsync(guild, members, playersByCharacterGuid, true, cancellationToken);

        _logger.LogInformation("Saved {Count} members for guild {GuildName}", members.Count, guildInfo.GuildName);
    }

    public async Task SaveGuildAsync(GuildInfoRecord guildInfo, CancellationToken cancellationToken = default)
    {
        var guild = await _guildRepository.FindByGuildIdAsync(guildInfo.GuildId, cancellationToken);

        if (guild == null)
        {
            guild = new Guild
            {
                GuildId = guildInfo.GuildId,
                GuildName = guildInfo.GuildName,
                LeaderGuid = string.Empty,
                CreateDate = string.Empty,
                LeaderName = string.Empty,
                LastUpdated = DateTime.UtcNow
            };
            await _guildRepository.AddAsync(guild, cancellationToken);
        }
        else
        {
            guild.GuildName = guildInfo.GuildName;
            guild.LastUpdated = DateTime.UtcNow;
            await _guildRepository.UpdateAsync(guild, cancellationToken);
        }
    }

    public async Task SaveGuildMembersPageAsync(string guildId, List<GuildMemberRecord> members, CancellationToken cancellationToken = default)
    {
        var guild = await _guildRepository.FindByGuildIdAsync(guildId, cancellationToken);

        if (guild == null)
        {
            return;
        }

        var playersByCharacterGuid = await GetOrCreatePlayersAsync(members, cancellationToken);
        await SaveGuildMembersAsync(guild, members, playersByCharacterGuid, false, cancellationToken);
    }

    private async Task<Dictionary<string, Player>> GetOrCreatePlayersAsync(
        List<GuildMemberRecord> members,
        CancellationToken cancellationToken)
    {
        var playersByCharacterGuid = new Dictionary<string, Player>();
        var playersWereChanged = false;

        foreach (var memberRecord in members)
        {
            if (playersByCharacterGuid.ContainsKey(memberRecord.CharacterGuid))
            {
                continue;
            }

            var player = await _playerRepository.FindByCharacterGuidAsync(memberRecord.CharacterGuid, cancellationToken);

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
                await _playerRepository.AddAsync(player, cancellationToken);
                playersWereChanged = true;
            }
            else
            {
                if (string.IsNullOrEmpty(player.ClassName))
                {
                    player.ClassName = ClassMappings.GetClassName(memberRecord.CharacterClass);
                    await _playerRepository.UpdateAsync(player, cancellationToken);
                    playersWereChanged = true;
                }
            }

            playersByCharacterGuid[memberRecord.CharacterGuid] = player;
        }

        if (playersWereChanged)
        {
            await _playerRepository.SaveChangesAsync(cancellationToken);
            await _playerRepository.ClearChangeTrackerAsync(cancellationToken);
        }

        var characterGuids = playersByCharacterGuid.Keys.ToList();
        var persistedPlayers = await _playerRepository.GetByCharacterGuidsAsync(characterGuids, cancellationToken);

        return persistedPlayers.ToDictionary(player => player.CharacterGuid, player => player);
    }

    private async Task SaveGuildMembersAsync(
        Guild guild,
        List<GuildMemberRecord> members,
        Dictionary<string, Player> playersByCharacterGuid,
        bool writeLogs,
        CancellationToken cancellationToken)
    {
        foreach (var memberRecord in members)
        {
            if (!playersByCharacterGuid.TryGetValue(memberRecord.CharacterGuid, out var player))
            {
                continue;
            }

            var guildMember = await _guildMemberRepository.FindByGuildIdAndPlayerIdAsync(guild.Id, player.Id, cancellationToken);

            if (guildMember == null)
            {
                guildMember = new GuildMember
                {
                    GuildId = guild.Id,
                    PlayerId = player.Id,
                    Rank = memberRecord.Rank,
                    JoinedDate = null,
                    LastUpdated = DateTime.UtcNow
                };
                await _guildMemberRepository.AddAsync(guildMember, cancellationToken);

                if (writeLogs)
                {
                    _logger.LogInformation("Added member to guild: {PlayerName} as {Rank}", memberRecord.CharacterName, memberRecord.Rank);
                }
            }
            else
            {
                guildMember.Rank = memberRecord.Rank;
                guildMember.LastUpdated = DateTime.UtcNow;
                await _guildMemberRepository.UpdateAsync(guildMember, cancellationToken);

                if (writeLogs)
                {
                    _logger.LogInformation("Updated member in guild: {PlayerName} as {Rank}", memberRecord.CharacterName, memberRecord.Rank);
                }
            }
        }
    }
}
