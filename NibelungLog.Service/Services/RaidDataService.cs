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

        foreach (var playerEncounter in playerEncounters)
        {
            if (!playersDict.ContainsKey(playerEncounter.CharacterGuid))
            {
                var player = await _playerRepository.FindByCharacterGuidAsync(playerEncounter.CharacterGuid, cancellationToken);

                if (player == null)
                {
                    player = new Player
                    {
                        CharacterGuid = playerEncounter.CharacterGuid,
                        CharacterName = playerEncounter.CharacterName,
                        CharacterRace = playerEncounter.CharacterRace,
                        CharacterClass = playerEncounter.CharacterClass,
                        ClassName = ClassMappings.GetClassName(playerEncounter.CharacterClass),
                        CharacterGender = playerEncounter.CharacterGender,
                        CharacterLevel = playerEncounter.CharacterLevel
                    };
                    await _playerRepository.AddAsync(player, cancellationToken);
                }
                else if (string.IsNullOrEmpty(player.ClassName))
                {
                    player.ClassName = ClassMappings.GetClassName(player.CharacterClass);
                    await _playerRepository.UpdateAsync(player, cancellationToken);
                }

                playersDict[playerEncounter.CharacterGuid] = player;
            }

            var specKey = (playerEncounter.CharacterClass, playerEncounter.CharacterSpec);
            if (!characterSpecsDict.ContainsKey(specKey))
            {
                var spec = await _characterSpecRepository.FindByClassAndSpecAsync(playerEncounter.CharacterClass, playerEncounter.CharacterSpec, cancellationToken);

                if (spec == null)
                {
                    spec = new CharacterSpec
                    {
                        CharacterClass = playerEncounter.CharacterClass,
                        Spec = playerEncounter.CharacterSpec,
                        Name = ClassMappings.GetSpecName(playerEncounter.CharacterClass, playerEncounter.CharacterSpec)
                    };
                    await _characterSpecRepository.AddAsync(spec, cancellationToken);
                }
                else if (string.IsNullOrEmpty(spec.Name))
                {
                    spec.Name = ClassMappings.GetSpecName(spec.CharacterClass, spec.Spec);
                    await _characterSpecRepository.UpdateAsync(spec, cancellationToken);
                }

                characterSpecsDict[specKey] = spec;
            }
        }

        foreach (var raid in raids)
        {
            var raidTypeKey = (raid.Map, raid.Difficulty, raid.InstanceType);
            if (!raidTypesDict.ContainsKey(raidTypeKey))
            {
                var raidType = await _raidTypeRepository.FindByMapDifficultyInstanceTypeAsync(raid.Map, raid.Difficulty, raid.InstanceType, cancellationToken);

                if (raidType == null)
                {
                    var displayName = RaidMappings.GetRaidDisplayName(raid.Map, raid.Difficulty, raid.InstanceType);
                    raidType = new RaidType
                    {
                        Name = displayName,
                        Map = raid.Map,
                        Difficulty = raid.Difficulty,
                        InstanceType = raid.InstanceType
                    };
                    await _raidTypeRepository.AddAsync(raidType, cancellationToken);
                }

                raidTypesDict[raidTypeKey] = raidType;
            }

            var savedRaid = await _raidRepository.FindByRaidIdAsync(raid.Id, cancellationToken);

            if (savedRaid == null)
            {
                savedRaid = new Raid
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
                await _raidRepository.AddAsync(savedRaid, cancellationToken);
            }

            savedRaidsDict[raid.Id] = savedRaid;
        }

        foreach (var encounter in encounters)
        {
            if (!savedRaidsDict.ContainsKey(encounter.LogInstanceId))
                continue;

            var encounterKey = (encounter.LogInstanceId, encounter.EncounterEntry, encounter.StartTime);
            if (!savedEncountersDict.ContainsKey(encounterKey))
            {
                var startTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(encounter.StartTime)).UtcDateTime;
                var savedEncounter = await _encounterRepository.FindByRaidIdEncounterEntryStartTimeAsync(
                    savedRaidsDict[encounter.LogInstanceId].Id, 
                    encounter.EncounterEntry, 
                    startTime, 
                    cancellationToken);

                if (savedEncounter == null)
                {
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
                    
                    savedEncounter = new Encounter
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
                    await _encounterRepository.AddAsync(savedEncounter, cancellationToken);
                }

                savedEncountersDict[encounterKey] = savedEncounter;
            }
        }

        foreach (var playerEncounter in playerEncounters)
        {
            if (!savedEncountersDict.ContainsKey((playerEncounter.LogInstanceId, playerEncounter.EncounterEntry, playerEncounter.StartTime)))
                continue;

            var encounter = savedEncountersDict[(playerEncounter.LogInstanceId, playerEncounter.EncounterEntry, playerEncounter.StartTime)];
            var player = playersDict[playerEncounter.CharacterGuid];
            var spec = characterSpecsDict[(playerEncounter.CharacterClass, playerEncounter.CharacterSpec)];

            var encounterDuration = (encounter.EndTime - encounter.StartTime).TotalSeconds;
            var dps = encounterDuration > 0 ? (double)long.Parse(playerEncounter.DamageDone) / encounterDuration : 0;

            var savedPlayerEncounter = await _playerEncounterRepository.FindByPlayerIdAndEncounterIdAsync(player.Id, encounter.Id, cancellationToken);

            if (savedPlayerEncounter == null)
            {
                savedPlayerEncounter = new PlayerEncounter
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
                await _playerEncounterRepository.AddAsync(savedPlayerEncounter, cancellationToken);
            }
        }
    }

    public async Task SaveSingleRaidDataAsync(RaidRecord raid, List<EncounterRecord> encounters, List<PlayerEncounterRecord> playerEncounters, CancellationToken cancellationToken = default)
    {
        await SaveRaidDataAsync([raid], encounters, playerEncounters, cancellationToken);
    }
}
