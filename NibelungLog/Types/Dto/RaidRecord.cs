using System.Text.Json.Serialization;

namespace NibelungLog.Types.Dto;

public sealed record RaidRecord
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }
    
    [JsonPropertyName("realm_id")]
    public required string RealmId { get; init; }
    
    [JsonPropertyName("instance_id")]
    public required string InstanceId { get; init; }
    
    [JsonPropertyName("map")]
    public required string Map { get; init; }
    
    [JsonPropertyName("difficulty")]
    public required string Difficulty { get; init; }
    
    [JsonPropertyName("instance_type")]
    public required string InstanceType { get; init; }
    
    [JsonPropertyName("start_time")]
    public required string StartTime { get; init; }
    
    [JsonPropertyName("total_time")]
    public required string TotalTime { get; init; }
    
    [JsonPropertyName("total_boss_time")]
    public required string TotalBossTime { get; init; }
    
    [JsonPropertyName("total_pve_points_guild")]
    public required string TotalPvePointsGuild { get; init; }
    
    [JsonPropertyName("total_pve_points_character")]
    public required string TotalPvePointsCharacter { get; init; }
    
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
    
    [JsonPropertyName("leader_guid")]
    public required string LeaderGuid { get; init; }
    
    [JsonPropertyName("leader_name")]
    public required string LeaderName { get; init; }
    
    [JsonPropertyName("leader_race")]
    public required string LeaderRace { get; init; }
    
    [JsonPropertyName("guild_id")]
    public required string GuildId { get; init; }
    
    [JsonPropertyName("guild_name")]
    public required string GuildName { get; init; }
    
    [JsonPropertyName("total_boss_number")]
    public required string TotalBossNumber { get; init; }
    
    [JsonPropertyName("completed_boss_number")]
    public required string CompletedBossNumber { get; init; }
    
    [JsonPropertyName("last_boss_completed")]
    public required string LastBossCompleted { get; init; }
    
    [JsonPropertyName("wipes")]
    public required string Wipes { get; init; }
    
    [JsonPropertyName("trash_clear")]
    public required string TrashClear { get; init; }
    
    [JsonPropertyName("trash_first_not_killed_guid")]
    public required string TrashFirstNotKilledGuid { get; init; }
    
    [JsonPropertyName("special")]
    public required string Special { get; init; }
    
    [JsonPropertyName("special2")]
    public required string Special2 { get; init; }
    
    [JsonPropertyName("race")]
    public required string Race { get; init; }
    
    [JsonPropertyName("rank")]
    public required int Rank { get; init; }
}

