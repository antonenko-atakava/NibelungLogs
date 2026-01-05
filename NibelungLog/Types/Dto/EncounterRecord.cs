using System.Text.Json.Serialization;

namespace NibelungLog.Types.Dto;

public sealed record EncounterRecord
{
    [JsonPropertyName("log_instance_id")]
    public required string LogInstanceId { get; init; }
    
    [JsonPropertyName("encounter_entry")]
    public required string EncounterEntry { get; init; }
    
    [JsonPropertyName("start_time")]
    public required string StartTime { get; init; }
    
    [JsonPropertyName("end_time")]
    public required string EndTime { get; init; }
    
    [JsonPropertyName("success")]
    public required string Success { get; init; }
    
    [JsonPropertyName("pve_points_guild")]
    public required string PvePointsGuild { get; init; }
    
    [JsonPropertyName("pve_points_character")]
    public required string PvePointsCharacter { get; init; }
    
    [JsonPropertyName("master_looter_guid")]
    public required string MasterLooterGuid { get; init; }
    
    [JsonPropertyName("total_damage")]
    public required string TotalDamage { get; init; }
    
    [JsonPropertyName("total_healing")]
    public required string TotalHealing { get; init; }
    
    [JsonPropertyName("average_gear_score")]
    public required string AverageGearScore { get; init; }
    
    [JsonPropertyName("max_average_gear_score")]
    public required string MaxAverageGearScore { get; init; }
    
    [JsonPropertyName("max_gear_score")]
    public required string MaxGearScore { get; init; }
    
    [JsonPropertyName("tanks")]
    public required string Tanks { get; init; }
    
    [JsonPropertyName("healers")]
    public required string Healers { get; init; }
    
    [JsonPropertyName("damage_dealers")]
    public required string DamageDealers { get; init; }
    
    [JsonPropertyName("encounter_name")]
    public string? EncounterName { get; init; }
    
    [JsonPropertyName("encounter_name_loc8")]
    public string? EncounterNameLoc8 { get; init; }
}

