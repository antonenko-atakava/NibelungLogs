using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.Data;
using NibelungLog.Entities;
using NibelungLog.Interfaces;
using NibelungLog.Types.Dto;
using NibelungLog.Types.Encounters;
using NibelungLog.Types;

namespace NibelungLog.Services;

public sealed class RaidDataService : IRaidDataService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RaidDataService> _logger;

    public RaidDataService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<RaidDataService>>();
    }

    public async Task SaveRaidDataAsync(List<RaidRecord> raids, List<EncounterRecord> encounters, List<PlayerEncounterRecord> playerEncounters, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var playersDict = new Dictionary<string, Player>();
        var characterSpecsDict = new Dictionary<(string Class, string Spec), CharacterSpec>();
        var raidTypesDict = new Dictionary<(string Map, string Difficulty, string InstanceType), RaidType>();
        var savedRaidsDict = new Dictionary<string, Raid>();
        var savedEncountersDict = new Dictionary<(string RaidId, string EncounterEntry, string StartTime), Encounter>();

        foreach (var playerEncounter in playerEncounters)
        {
            if (!playersDict.ContainsKey(playerEncounter.CharacterGuid))
            {
                var player = await context.Players
                    .FirstOrDefaultAsync(p => p.CharacterGuid == playerEncounter.CharacterGuid, cancellationToken);

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
                    context.Players.Add(player);
                    await context.SaveChangesAsync(cancellationToken);
                }
                else if (string.IsNullOrEmpty(player.ClassName))
                {
                    player.ClassName = ClassMappings.GetClassName(player.CharacterClass);
                    await context.SaveChangesAsync(cancellationToken);
                }

                playersDict[playerEncounter.CharacterGuid] = player;
            }

            var specKey = (playerEncounter.CharacterClass, playerEncounter.CharacterSpec);
            if (!characterSpecsDict.ContainsKey(specKey))
            {
                var spec = await context.CharacterSpecs
                    .FirstOrDefaultAsync(cs => cs.CharacterClass == playerEncounter.CharacterClass && cs.Spec == playerEncounter.CharacterSpec, cancellationToken);

                if (spec == null)
                {
                    spec = new CharacterSpec
                    {
                        CharacterClass = playerEncounter.CharacterClass,
                        Spec = playerEncounter.CharacterSpec,
                        Name = ClassMappings.GetSpecName(playerEncounter.CharacterClass, playerEncounter.CharacterSpec)
                    };
                    context.CharacterSpecs.Add(spec);
                    await context.SaveChangesAsync(cancellationToken);
                }
                else if (string.IsNullOrEmpty(spec.Name))
                {
                    spec.Name = ClassMappings.GetSpecName(spec.CharacterClass, spec.Spec);
                    await context.SaveChangesAsync(cancellationToken);
                }

                characterSpecsDict[specKey] = spec;
            }
        }

        foreach (var raid in raids)
        {
            var raidTypeKey = (raid.Map, raid.Difficulty, raid.InstanceType);
            if (!raidTypesDict.ContainsKey(raidTypeKey))
            {
                var raidType = await context.RaidTypes
                    .FirstOrDefaultAsync(rt => rt.Map == raid.Map && rt.Difficulty == raid.Difficulty && rt.InstanceType == raid.InstanceType, cancellationToken);

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
                    context.RaidTypes.Add(raidType);
                    await context.SaveChangesAsync(cancellationToken);
                }

                raidTypesDict[raidTypeKey] = raidType;
            }

            var savedRaid = await context.Raids
                .FirstOrDefaultAsync(r => r.RaidId == raid.Id, cancellationToken);

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
                context.Raids.Add(savedRaid);
                await context.SaveChangesAsync(cancellationToken);
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
                var savedEncounter = await context.Encounters
                    .FirstOrDefaultAsync(e => e.RaidId == savedRaidsDict[encounter.LogInstanceId].Id && 
                                               e.EncounterEntry == encounter.EncounterEntry && 
                                               e.StartTime == DateTimeOffset.FromUnixTimeSeconds(long.Parse(encounter.StartTime)).UtcDateTime, cancellationToken);

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
                        StartTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(encounter.StartTime)).UtcDateTime,
                        EndTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(encounter.EndTime)).UtcDateTime,
                        Success = encounter.Success == "1",
                        TotalDamage = long.Parse(encounter.TotalDamage),
                        TotalHealing = long.Parse(encounter.TotalHealing),
                        Tanks = int.Parse(encounter.Tanks),
                        Healers = int.Parse(encounter.Healers),
                        DamageDealers = int.Parse(encounter.DamageDealers),
                        AverageGearScore = encounter.AverageGearScore
                    };
                    context.Encounters.Add(savedEncounter);
                    await context.SaveChangesAsync(cancellationToken);
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

            var savedPlayerEncounter = await context.PlayerEncounters
                .FirstOrDefaultAsync(pe => pe.PlayerId == player.Id && 
                                            pe.EncounterId == encounter.Id, cancellationToken);

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
                context.PlayerEncounters.Add(savedPlayerEncounter);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveSingleRaidDataAsync(RaidRecord raid, List<EncounterRecord> encounters, List<PlayerEncounterRecord> playerEncounters, CancellationToken cancellationToken = default)
    {
        await SaveRaidDataAsync([raid], encounters, playerEncounters, cancellationToken);
    }
}

