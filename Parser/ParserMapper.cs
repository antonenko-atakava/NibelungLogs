using NibelungLog.Domain.Types.Dto;
using Parser.Models;

namespace Parser;

public static class ParserMapper
{
    public static RaidRecord ToRaidRecord(Models.Raid raid)
    {
        return new RaidRecord
        {
            Id = raid.Id.ToString(),
            RealmId = raid.RealmId.ToString(),
            InstanceId = raid.InstanceId.ToString(),
            Map = raid.Map.ToString(),
            Difficulty = raid.Difficulty.ToString(),
            InstanceType = raid.InstanceType.ToString(),
            StartTime = raid.StartTime.ToString(),
            TotalTime = raid.TotalTime.ToString(),
            TotalBossTime = raid.TotalBossTime.ToString(),
            TotalPvePointsGuild = raid.TotalPvePointsGuild.ToString(),
            TotalPvePointsCharacter = raid.TotalPvePointsCharacter.ToString(),
            TotalDamage = raid.TotalDamage.ToString(),
            TotalHealing = raid.TotalHealing.ToString(),
            AverageGearScore = raid.AverageGearScore.ToString(),
            MaxAverageGearScore = raid.MaxAverageGearScore.ToString(),
            MaxGearScore = raid.MaxGearScore.ToString(),
            LeaderGuid = raid.LeaderGuid.ToString(),
            LeaderName = raid.LeaderName,
            LeaderRace = raid.LeaderRace.ToString(),
            GuildId = raid.GuildId.ToString(),
            GuildName = raid.GuildName,
            TotalBossNumber = raid.TotalBossNumber.ToString(),
            CompletedBossNumber = raid.CompletedBossNumber.ToString(),
            LastBossCompleted = raid.LastBossCompleted.ToString(),
            Wipes = raid.Wipes.ToString(),
            TrashClear = raid.TrashClear.ToString(),
            TrashFirstNotKilledGuid = raid.TrashFirstNotKilledGuid.ToString(),
            Special = raid.Special.ToString(),
            Special2 = raid.Special2.ToString(),
            Race = raid.Race.ToString(),
            Rank = raid.Rank
        };
    }

    public static EncounterRecord ToEncounterRecord(RaidEncounter encounter)
    {
        return new EncounterRecord
        {
            LogInstanceId = encounter.LogInstanceId.ToString(),
            EncounterEntry = encounter.EncounterEntry.ToString(),
            StartTime = encounter.StartTime.ToString(),
            EndTime = encounter.EndTime.ToString(),
            Success = encounter.Success,
            PvePointsGuild = encounter.PvePointsGuild,
            PvePointsCharacter = encounter.PvePointsCharacter,
            MasterLooterGuid = encounter.MasterLooterGuid,
            TotalDamage = encounter.TotalDamage,
            TotalHealing = encounter.TotalHealing,
            AverageGearScore = encounter.AverageGearScore,
            MaxAverageGearScore = encounter.MaxAverageGearScore,
            MaxGearScore = encounter.MaxGearScore,
            Tanks = encounter.Tanks,
            Healers = encounter.Healers,
            DamageDealers = encounter.DamageDealers
        };
    }

    public static PlayerEncounterRecord ToPlayerEncounterRecord(RaidEncounterDetail detail)
    {
        var lootItems = detail.Loot?.Select(loot => new LootItem
        {
            Entry = loot.Entry ?? string.Empty,
            Count = loot.Count ?? string.Empty
        }).ToList() ?? [];

        return new PlayerEncounterRecord
        {
            LogInstanceId = detail.LogInstanceId.ToString(),
            EncounterEntry = detail.EncounterEntry.ToString(),
            StartTime = detail.StartTime.ToString(),
            CharacterGuid = detail.CharacterGuid.ToString(),
            CharacterName = detail.CharacterName ?? string.Empty,
            CharacterRace = detail.CharacterRace.ToString(),
            CharacterClass = detail.CharacterClass.ToString(),
            CharacterSpec = detail.CharacterSpec.ToString(),
            CharacterGender = detail.CharacterGender.ToString(),
            CharacterLevel = detail.CharacterLevel.ToString(),
            CharacterRole = detail.CharacterRole.ToString(),
            MaxAverageGearScore = detail.MaxAverageGearScore.ToString(),
            MaxGearScore = detail.MaxGearScore.ToString(),
            DamageDone = detail.DamageDone.ToString(),
            HealingDone = detail.HealingDone.ToString(),
            AbsorbProvided = detail.AbsorbProvided.ToString(),
            ValidLooter = detail.ValidLooter.ToString(),
            Loot = lootItems
        };
    }
}
