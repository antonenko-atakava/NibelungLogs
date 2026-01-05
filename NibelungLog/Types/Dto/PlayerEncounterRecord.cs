using System.Text.Json.Serialization;

namespace NibelungLog.Types.Dto;

public sealed record LootItem
{
    [JsonPropertyName("entry")]
    public required string Entry { get; init; }
    
    [JsonPropertyName("count")]
    public required string Count { get; init; }
}

public sealed record PlayerEncounterRecord
{
    [JsonPropertyName("log_instance_id")]
    public required string LogInstanceId { get; init; }
    
    [JsonPropertyName("encounter_entry")]
    public required string EncounterEntry { get; init; }
    
    [JsonPropertyName("start_time")]
    public required string StartTime { get; init; }
    
    [JsonPropertyName("character_guid")]
    public required string CharacterGuid { get; init; }
    
    [JsonPropertyName("character_name")]
    public required string CharacterName { get; init; }
    
    [JsonPropertyName("character_race")]
    public required string CharacterRace { get; init; }
    
    [JsonPropertyName("character_class")]
    public required string CharacterClass { get; init; }
    
    [JsonPropertyName("character_spec")]
    public required string CharacterSpec { get; init; }
    
    [JsonPropertyName("character_gender")]
    public required string CharacterGender { get; init; }
    
    [JsonPropertyName("character_level")]
    public required string CharacterLevel { get; init; }
    
    [JsonPropertyName("character_role")]
    public required string CharacterRole { get; init; }
    
    [JsonPropertyName("max_average_gear_score")]
    public required string MaxAverageGearScore { get; init; }
    
    [JsonPropertyName("max_gear_score")]
    public required string MaxGearScore { get; init; }
    
    [JsonPropertyName("damage_done")]
    public required string DamageDone { get; init; }
    
    [JsonPropertyName("healing_done")]
    public required string HealingDone { get; init; }
    
    [JsonPropertyName("absorb_provided")]
    public required string AbsorbProvided { get; init; }
    
    [JsonPropertyName("valid_looter")]
    public required string ValidLooter { get; init; }
    
    [JsonPropertyName("loot")]
    public required List<LootItem> Loot { get; init; }
}

