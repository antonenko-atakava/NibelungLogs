using System.Text.Json.Serialization;

namespace NibelungLog.Types.Dto;

public sealed record AchievementRecord
{
    [JsonPropertyName("log_instance_id")]
    public required string LogInstanceId { get; init; }
    
    [JsonPropertyName("encounter_entry")]
    public required string EncounterEntry { get; init; }
    
    [JsonPropertyName("start_time")]
    public required string StartTime { get; init; }
    
    [JsonPropertyName("achievement_entry")]
    public required string AchievementEntry { get; init; }
    
    [JsonPropertyName("pve_points")]
    public required string PvePoints { get; init; }
}

