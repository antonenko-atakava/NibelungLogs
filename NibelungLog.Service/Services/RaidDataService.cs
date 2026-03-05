using Microsoft.Extensions.Logging;
using NibelungLog.Domain.Entities;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types;
using NibelungLog.Domain.Types.Dto;
using NibelungLog.Domain.Types.Encounters;

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
        var savedEncountersDict = new Dictionary<(string RaidId, string EncounterEntry, string StartTime), Encounter>();

        var characterGuids = playerEncounters
            .Select(p => p.CharacterGuid)
            .Distinct()
            .ToList();

        var existingPlayers = await _playerRepository.GetByCharacterGuidsAsync(characterGuids, cancellationToken);
        foreach (var player in existingPlayers)
            playersDict[player.CharacterGuid] = player;

        var newPlayers = new List<Player>();
        var playersToUpdate = new List<Player>();

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
            else if (string.IsNullOrEmpty(playersDict[playerEncounter.CharacterGuid].ClassName))
            {
                var player = playersDict[playerEncounter.CharacterGuid];
                player.ClassName = ClassMappings.GetClassName(player.CharacterClass);
                playersToUpdate.Add(player);
            }
        }

        var hasNewPlayers = newPlayers.Count > 0;
        if (hasNewPlayers)
        {
            await _playerRepository.AddRangeAsync(newPlayers, cancellationToken);
            await _playerRepository.SaveChangesAsync(cancellationToken);
            
            foreach (var player in newPlayers)
            {
                if (player.Id > 0 && playersDict.ContainsKey(player.CharacterGuid))
                {
                    playersDict[player.CharacterGuid] = player;
                }
            }
        }

        if (playersToUpdate.Count > 0)
            await _playerRepository.UpdateRangeAsync(playersToUpdate, cancellationToken);

        newPlayers.Clear();
        playersToUpdate.Clear();
        characterGuids.Clear();

        var specsKeys = playerEncounters
            .Select(p => (p.CharacterClass, p.CharacterSpec))
            .Distinct()
            .ToList();

        var existingSpecs = await _characterSpecRepository.GetByClassAndSpecAsync(specsKeys, cancellationToken);
        foreach (var spec in existingSpecs)
            characterSpecsDict[(spec.CharacterClass, spec.Spec)] = spec;

        var newSpecs = new List<CharacterSpec>();
        var specsToUpdate = new List<CharacterSpec>();

        foreach (var specKey in specsKeys)
        {
            if (!characterSpecsDict.ContainsKey(specKey))
            {
                var spec = new CharacterSpec
                    {
                    CharacterClass = specKey.CharacterClass,
                    Spec = specKey.CharacterSpec,
                    Name = ClassMappings.GetSpecName(specKey.CharacterClass, specKey.CharacterSpec)
                    };
                newSpecs.Add(spec);
                characterSpecsDict[specKey] = spec;
            }
            else if (string.IsNullOrEmpty(characterSpecsDict[specKey].Name))
            {
                var spec = characterSpecsDict[specKey];
                spec.Name = ClassMappings.GetSpecName(spec.CharacterClass, spec.Spec);
                specsToUpdate.Add(spec);
            }
        }

        var hasNewSpecs = newSpecs.Count > 0;
        if (hasNewSpecs)
        {
            await _characterSpecRepository.AddRangeAsync(newSpecs, cancellationToken);
            await _characterSpecRepository.SaveChangesAsync(cancellationToken);
        }

        if (specsToUpdate.Count > 0)
            await _characterSpecRepository.UpdateRangeAsync(specsToUpdate, cancellationToken);

        newSpecs.Clear();
        specsToUpdate.Clear();
        specsKeys.Clear();

        var raidTypeKeys = raids
            .Select(r => (r.Map, r.Difficulty, r.InstanceType))
            .Distinct()
            .ToList();

        var existingRaidTypes = await _raidTypeRepository.GetByMapDifficultyInstanceTypeAsync(raidTypeKeys, cancellationToken);
        foreach (var raidType in existingRaidTypes)
            raidTypesDict[(raidType.Map, raidType.Difficulty, raidType.InstanceType)] = raidType;

        var newRaidTypes = new List<RaidType>();
        foreach (var raidTypeKey in raidTypeKeys)
        {
            if (!raidTypesDict.ContainsKey(raidTypeKey))
            {
                var displayName = RaidMappings.GetRaidDisplayName(raidTypeKey.Map, raidTypeKey.Difficulty, raidTypeKey.InstanceType);
                var raidType = new RaidType
                    {
                        Name = displayName,
                    Map = raidTypeKey.Map,
                    Difficulty = raidTypeKey.Difficulty,
                    InstanceType = raidTypeKey.InstanceType
                    };
                newRaidTypes.Add(raidType);
                raidTypesDict[raidTypeKey] = raidType;
            }
        }

        var hasNewRaidTypes = newRaidTypes.Count > 0;
        if (hasNewRaidTypes)
        {
            await _raidTypeRepository.AddRangeAsync(newRaidTypes, cancellationToken);
            await _raidTypeRepository.SaveChangesAsync(cancellationToken);
        }

        newRaidTypes.Clear();
        raidTypeKeys.Clear();

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
        var encounterLookup = existingEncounters.ToLookup(e => (e.RaidId, e.EncounterEntry, e.StartTime));
        
        foreach (var encounter in encounters)
        {
            if (!savedRaidsDict.ContainsKey(encounter.LogInstanceId))
                continue;

            var startTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(encounter.StartTime)).UtcDateTime;
            var raidId = savedRaidsDict[encounter.LogInstanceId].Id;
            var lookupKey = (raidId, encounter.EncounterEntry, startTime);
            
            var existingEncounter = encounterLookup[lookupKey].FirstOrDefault();
            if (existingEncounter != null)
            {
                var encounterKey = (encounter.LogInstanceId, encounter.EncounterEntry, encounter.StartTime);
                savedEncountersDict[encounterKey] = existingEncounter;
        }
        }

        var newEncounters = new List<Encounter>();
        foreach (var encounter in encounters)
        {
            if (!savedRaidsDict.ContainsKey(encounter.LogInstanceId))
                continue;

            var encounterKey = (encounter.LogInstanceId, encounter.EncounterEntry, encounter.StartTime);
            if (!savedEncountersDict.ContainsKey(encounterKey))
            {
                var startTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(encounter.StartTime)).UtcDateTime;
                    var raidRecord = raids.FirstOrDefault(r => r.Id == encounter.LogInstanceId);
                    var map = raidRecord?.Map ?? "533";
                    
                    var encounterName = map switch
                    {
                        "533" => NaxxramasEncounter.Names.TryGetValue(encounter.EncounterEntry, out var naxxName) ? naxxName : null,
                        "616" => EyeOfEternityEncounter.Names.TryGetValue(encounter.EncounterEntry, out var eyeName) ? eyeName : null,
                        "615" => ObsidianSanctumEncounter.Names.TryGetValue(encounter.EncounterEntry, out var obsName) ? obsName : null,
                        "603" => UlduarEncounter.Names.TryGetValue(encounter.EncounterEntry, out var ulduarName) ? ulduarName : null,
                        _ => null
                    };
                    
                var savedEncounter = new Encounter
                    {
                        RaidId = savedRaidsDict[encounter.LogInstanceId].Id,
                        EncounterEntry = encounter.EncounterEntry,
                        EncounterName = encounterName,
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
            .Where(pe => savedEncountersDict.ContainsKey((pe.LogInstanceId, pe.EncounterEntry, pe.StartTime)))
            .Select(pe => {
                var encounter = savedEncountersDict[(pe.LogInstanceId, pe.EncounterEntry, pe.StartTime)];
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
            if (!savedEncountersDict.ContainsKey((playerEncounter.LogInstanceId, playerEncounter.EncounterEntry, playerEncounter.StartTime)))
                continue;

            var encounter = savedEncountersDict[(playerEncounter.LogInstanceId, playerEncounter.EncounterEntry, playerEncounter.StartTime)];
            var player = playersDict[playerEncounter.CharacterGuid];
            var spec = characterSpecsDict[(playerEncounter.CharacterClass, playerEncounter.CharacterSpec)];

            if (existingPlayerEncounterSet.Contains((player.Id, encounter.Id)))
                continue;

            var encounterDuration = (encounter.EndTime - encounter.StartTime).TotalSeconds;
            var dps = encounterDuration > 0 ? (double)long.Parse(playerEncounter.DamageDone) / encounterDuration : 0;

            var savedPlayerEncounter = new PlayerEncounter
                {
                    PlayerId = player.Id,
                    EncounterId = encounter.Id,
                    CharacterSpecId = spec.Id,
                    Role = playerEncounter.CharacterRole,
                    DamageDone = long.Parse(playerEncounter.DamageDone),
                    HealingDone = long.Parse(playerEncounter.HealingDone),
                    AbsorbProvided = long.Parse(playerEncounter.AbsorbProvided),
                    Dps = dps,
                    MaxAverageGearScore = playerEncounter.MaxAverageGearScore,
                    MaxGearScore = playerEncounter.MaxGearScore
                };
            newPlayerEncounters.Add(savedPlayerEncounter);
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
        if (!hasNewRaidTypes)
        {
            await _raidTypeRepository.SaveChangesAsync(cancellationToken);
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
    }

    public async Task SaveSingleRaidDataAsync(RaidRecord raid, List<EncounterRecord> encounters, List<PlayerEncounterRecord> playerEncounters, CancellationToken cancellationToken = default)
    {
        await SaveRaidDataAsync([raid], encounters, playerEncounters, cancellationToken);
    }
}
