using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NibelungLog.Domain.Entities;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types;
using NibelungLog.Domain.Types.Dto;
using Npgsql;

namespace NibelungLog.Service.Services;

public sealed class RaidDataService : IRaidDataService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ICharacterSpecRepository _characterSpecRepository;
    private readonly IRaidTypeRepository _raidTypeRepository;
    private readonly IRaidRepository _raidRepository;
    private readonly IEncounterRepository _encounterRepository;
    private readonly IPlayerEncounterRepository _playerEncounterRepository;
    private readonly ILogger<RaidDataService> _logger;

    public RaidDataService(
        IPlayerRepository playerRepository,
        ICharacterSpecRepository characterSpecRepository,
        IRaidTypeRepository raidTypeRepository,
        IRaidRepository raidRepository,
        IEncounterRepository encounterRepository,
        IPlayerEncounterRepository playerEncounterRepository,
        ILogger<RaidDataService> logger)
    {
        _playerRepository = playerRepository;
        _characterSpecRepository = characterSpecRepository;
        _raidTypeRepository = raidTypeRepository;
        _raidRepository = raidRepository;
        _encounterRepository = encounterRepository;
        _playerEncounterRepository = playerEncounterRepository;
        _logger = logger;
    }

    public async Task SaveRaidDataAsync(List<RaidRecord> raids, List<EncounterRecord> encounters, List<PlayerEncounterRecord> playerEncounters, CancellationToken cancellationToken = default)
    {
        var playersDict = new Dictionary<string, Player>();
        var characterSpecsDict = new Dictionary<(string Class, string Spec), CharacterSpec>();
        var raidTypesDict = new Dictionary<(string Map, string Difficulty, string InstanceType), RaidType>();
        var savedRaidsDict = new Dictionary<string, Raid>();
        var savedEncountersDict = new Dictionary<(int RaidId, string EncounterEntry, DateTime StartTime), Encounter>();

        var characterGuids = playerEncounters.Select(pe => pe.CharacterGuid).Distinct().ToList();
        var existingPlayers = await _playerRepository.GetByCharacterGuidsAsync(characterGuids, cancellationToken);
        
        foreach (var player in existingPlayers)
            playersDict[player.CharacterGuid] = player;

        var newPlayers = new List<Player>();
        foreach (var playerEncounter in playerEncounters)
        {
            if (!playersDict.ContainsKey(playerEncounter.CharacterGuid))
            {
                var player = new Player
                {
                    CharacterGuid = playerEncounter.CharacterGuid,
                    CharacterName = playerEncounter.CharacterName,
                    CharacterRace = playerEncounter.CharacterRace,
                    CharacterClass = playerEncounter.CharacterClass,
                    ClassName = ClassMappings.GetClassName(playerEncounter.CharacterClass),
                    CharacterGender = playerEncounter.CharacterGender,
                    CharacterLevel = playerEncounter.CharacterLevel
                };
                newPlayers.Add(player);
                playersDict[playerEncounter.CharacterGuid] = player;
            }
        }

        var hasNewPlayers = newPlayers.Count > 0;
        if (hasNewPlayers)
        {
            await _playerRepository.AddRangeAsync(newPlayers, cancellationToken);
            await _playerRepository.SaveChangesAsync(cancellationToken);
        }

        newPlayers.Clear();

        var characterSpecs = playerEncounters
            .Select(pe => (pe.CharacterClass, pe.CharacterSpec))
            .Distinct()
            .ToList();

        var existingSpecs = await _characterSpecRepository.GetByClassAndSpecAsync(characterSpecs, cancellationToken);
        
        foreach (var spec in existingSpecs)
            characterSpecsDict[(spec.CharacterClass, spec.Spec)] = spec;

        var newSpecs = new List<CharacterSpec>();
        foreach (var (characterClass, spec) in characterSpecs)
        {
            if (!characterSpecsDict.ContainsKey((characterClass, spec)))
            {
                var characterSpec = new CharacterSpec
                {
                    CharacterClass = characterClass,
                    Spec = spec,
                    Name = ClassMappings.GetSpecName(characterClass, spec)
                };
                newSpecs.Add(characterSpec);
                characterSpecsDict[(characterClass, spec)] = characterSpec;
            }
        }

        var hasNewSpecs = newSpecs.Count > 0;
        if (hasNewSpecs)
        {
            try
            {
                await _characterSpecRepository.AddRangeAsync(newSpecs, cancellationToken);
                await _characterSpecRepository.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogWarning("Duplicate CharacterSpec detected, clearing context and reloading from database");
                await _characterSpecRepository.ClearChangeTrackerAsync(cancellationToken);
                
                foreach (var newSpec in newSpecs)
                {
                    var existingSpec = await _characterSpecRepository.FindByClassAndSpecAsync(
                        newSpec.CharacterClass, 
                        newSpec.Spec, 
                        cancellationToken);
                    
                    if (existingSpec != null)
                        characterSpecsDict[(existingSpec.CharacterClass, existingSpec.Spec)] = existingSpec;
                }
            }
        }

        newSpecs.Clear();

        var raidTypeKeys = raids
            .Select(r => (r.Map, r.Difficulty, r.InstanceType))
            .Distinct()
            .ToList();

        var existingRaidTypes = await _raidTypeRepository.GetByMapDifficultyInstanceTypeAsync(raidTypeKeys, cancellationToken);
        
        foreach (var raidType in existingRaidTypes)
            raidTypesDict[(raidType.Map, raidType.Difficulty, raidType.InstanceType)] = raidType;

        var missingRaidTypes = raidTypeKeys
            .Where(key => !raidTypesDict.ContainsKey(key))
            .ToList();

        if (missingRaidTypes.Count > 0)
        {
            _logger.LogWarning("Skipping {Count} raids with missing RaidTypes: {MissingTypes}",
                missingRaidTypes.Count,
                string.Join(", ", missingRaidTypes.Select(k => $"{k.Map}-{k.Difficulty}-{k.InstanceType}")));
        }

        var raidIds = raids.Select(r => r.Id).Distinct().ToList();
        var existingRaids = await _raidRepository.GetByRaidIdsAsync(raidIds, cancellationToken);
        
        foreach (var raid in existingRaids)
            savedRaidsDict[raid.RaidId] = raid;

        var newRaids = new List<Raid>();
        foreach (var raid in raids)
        {
            if (!savedRaidsDict.ContainsKey(raid.Id))
            {
                var raidTypeKey = (raid.Map, raid.Difficulty, raid.InstanceType);
                
                if (!raidTypesDict.ContainsKey(raidTypeKey))
                {
                    _logger.LogWarning("Skipping raid {RaidId}: RaidType not found (Map={Map}, Difficulty={Difficulty}, InstanceType={InstanceType})",
                        raid.Id, raid.Map, raid.Difficulty, raid.InstanceType);
                    continue;
                }
                
                var savedRaid = new Raid
                {
                    RaidId = raid.Id,
                    RaidTypeId = raidTypesDict[raidTypeKey].Id,
                    GuildName = raid.GuildName,
                    LeaderName = raid.LeaderName,
                    LeaderGuid = raid.LeaderGuid,
                    StartTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(raid.StartTime)).UtcDateTime,
                    TotalTime = long.Parse(raid.TotalTime),
                    TotalDamage = long.Parse(raid.TotalDamage),
                    TotalHealing = long.Parse(raid.TotalHealing),
                    AverageGearScore = raid.AverageGearScore,
                    MaxGearScore = raid.MaxGearScore,
                    Wipes = int.Parse(raid.Wipes),
                    CompletedBosses = int.Parse(raid.CompletedBossNumber),
                    TotalBosses = int.Parse(raid.TotalBossNumber)
                };
                newRaids.Add(savedRaid);
                savedRaidsDict[raid.Id] = savedRaid;
            }
        }

        var hasNewRaids = newRaids.Count > 0;
        if (hasNewRaids)
        {
            await _raidRepository.AddRangeAsync(newRaids, cancellationToken);
            await _raidRepository.SaveChangesAsync(cancellationToken);
        }

        newRaids.Clear();
        raidIds.Clear();

        var encounterKeys = encounters
            .Where(e => savedRaidsDict.ContainsKey(e.LogInstanceId))
            .Select(e => (
                RaidId: savedRaidsDict[e.LogInstanceId].Id,
                EncounterEntry: e.EncounterEntry,
                StartTime: DateTimeOffset.FromUnixTimeSeconds(long.Parse(e.StartTime)).UtcDateTime
            ))
            .Distinct()
            .ToList();

        var existingEncounters = await _encounterRepository.GetByRaidIdEncounterEntryStartTimeAsync(encounterKeys, cancellationToken);
        
        foreach (var encounter in existingEncounters)
            savedEncountersDict[(encounter.RaidId, encounter.EncounterEntry, encounter.StartTime)] = encounter;

        var newEncounters = new List<Encounter>();
        foreach (var encounter in encounters)
        {
            if (!savedRaidsDict.ContainsKey(encounter.LogInstanceId))
                continue;

            var raid = savedRaidsDict[encounter.LogInstanceId];
            var startTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(encounter.StartTime)).UtcDateTime;
            var encounterKey = (raid.Id, encounter.EncounterEntry, startTime);

            if (!savedEncountersDict.ContainsKey(encounterKey))
            {
                var savedEncounter = new Encounter
                {
                    RaidId = raid.Id,
                    EncounterEntry = encounter.EncounterEntry,
                    EncounterName = encounter.EncounterName,
                    StartTime = startTime,
                    EndTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(encounter.EndTime)).UtcDateTime,
                    Success = encounter.Success == "1",
                    TotalDamage = long.Parse(encounter.TotalDamage),
                    TotalHealing = long.Parse(encounter.TotalHealing),
                    Tanks = int.Parse(encounter.Tanks),
                    Healers = int.Parse(encounter.Healers),
                    DamageDealers = int.Parse(encounter.DamageDealers),
                    AverageGearScore = encounter.AverageGearScore
                };
                newEncounters.Add(savedEncounter);
                savedEncountersDict[encounterKey] = savedEncounter;
            }
        }

        var hasNewEncounters = newEncounters.Count > 0;
        if (hasNewEncounters)
        {
            await _encounterRepository.AddRangeAsync(newEncounters, cancellationToken);
            await _encounterRepository.SaveChangesAsync(cancellationToken);
        }

        newEncounters.Clear();
        encounterKeys.Clear();

        var playerEncounterKeys = playerEncounters
            .Where(pe => savedEncountersDict.ContainsKey((
                savedRaidsDict[pe.LogInstanceId].Id,
                pe.EncounterEntry,
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(pe.StartTime)).UtcDateTime
            )))
            .Select(pe => {
                var encounter = savedEncountersDict[(
                    savedRaidsDict[pe.LogInstanceId].Id,
                    pe.EncounterEntry,
                    DateTimeOffset.FromUnixTimeSeconds(long.Parse(pe.StartTime)).UtcDateTime
                )];
                var player = playersDict[pe.CharacterGuid];
                return (player.Id, encounter.Id);
            })
            .Distinct()
            .ToList();

        var existingPlayerEncounters = await _playerEncounterRepository.GetByPlayerIdAndEncounterIdAsync(playerEncounterKeys, cancellationToken);
        var existingPlayerEncounterSet = existingPlayerEncounters
            .Select(pe => (pe.PlayerId, pe.EncounterId))
            .ToHashSet();

        var newPlayerEncounters = new List<PlayerEncounter>();
        foreach (var playerEncounter in playerEncounters)
        {
            if (!savedRaidsDict.ContainsKey(playerEncounter.LogInstanceId))
                continue;

            var raid = savedRaidsDict[playerEncounter.LogInstanceId];
            var startTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(playerEncounter.StartTime)).UtcDateTime;
            var encounterKey = (raid.Id, playerEncounter.EncounterEntry, startTime);

            if (!savedEncountersDict.ContainsKey(encounterKey))
                continue;

            var encounter = savedEncountersDict[encounterKey];
            var player = playersDict[playerEncounter.CharacterGuid];
            var playerEncounterKey = (player.Id, encounter.Id);

            if (!existingPlayerEncounterSet.Contains(playerEncounterKey))
            {
                var characterSpec = characterSpecsDict[(playerEncounter.CharacterClass, playerEncounter.CharacterSpec)];
                var fightDuration = (encounter.EndTime - encounter.StartTime).TotalSeconds;
                var damageDone = long.Parse(playerEncounter.DamageDone);
                var dps = fightDuration > 0 ? (double)damageDone / fightDuration : 0;

                var savedPlayerEncounter = new PlayerEncounter
                {
                    PlayerId = player.Id,
                    EncounterId = encounter.Id,
                    CharacterSpecId = characterSpec.Id,
                    Role = playerEncounter.CharacterRole,
                    DamageDone = damageDone,
                    HealingDone = long.Parse(playerEncounter.HealingDone),
                    AbsorbProvided = long.Parse(playerEncounter.AbsorbProvided),
                    Dps = dps,
                    MaxAverageGearScore = playerEncounter.MaxAverageGearScore,
                    MaxGearScore = playerEncounter.MaxGearScore
                };
                newPlayerEncounters.Add(savedPlayerEncounter);
                existingPlayerEncounterSet.Add(playerEncounterKey);
            }
        }

        if (newPlayerEncounters.Count > 0)
            await _playerEncounterRepository.AddRangeAsync(newPlayerEncounters, cancellationToken);

        newPlayerEncounters.Clear();
        playerEncounterKeys.Clear();
        existingPlayerEncounterSet.Clear();

        if (!hasNewPlayers)
        {
            await _playerRepository.SaveChangesAsync(cancellationToken);
        }
        if (!hasNewSpecs)
        {
            await _characterSpecRepository.SaveChangesAsync(cancellationToken);
        }
        if (!hasNewRaids)
        {
            await _raidRepository.SaveChangesAsync(cancellationToken);
        }
        if (!hasNewEncounters)
        {
            await _encounterRepository.SaveChangesAsync(cancellationToken);
        }
        await _playerEncounterRepository.SaveChangesAsync(cancellationToken);

        await _playerRepository.ClearChangeTrackerAsync(cancellationToken);

        playersDict.Clear();
        characterSpecsDict.Clear();
        raidTypesDict.Clear();
        savedRaidsDict.Clear();
        savedEncountersDict.Clear();

        _logger.LogInformation("Saved {RaidCount} raids, {EncounterCount} encounters, {PlayerEncounterCount} player encounters",
            raids.Count, encounters.Count, playerEncounters.Count);
    }

    public async Task SaveSingleRaidDataAsync(RaidRecord raid, List<EncounterRecord> encounters, List<PlayerEncounterRecord> playerEncounters, CancellationToken cancellationToken = default)
    {
        await SaveRaidDataAsync([raid], encounters, playerEncounters, cancellationToken);
    }
}
