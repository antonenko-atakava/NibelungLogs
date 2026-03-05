using Newtonsoft.Json;

namespace Parser.Models;

public sealed class Raid
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("realm_id")]
    public int RealmId { get; set; }

    [JsonProperty("instance_id")]
    public int InstanceId { get; set; }

    [JsonProperty("map")]
    public int Map { get; set; }

    [JsonProperty("difficulty")]
    public int Difficulty { get; set; }

    [JsonProperty("instance_type")]
    public int InstanceType { get; set; }

    [JsonProperty("start_time")]
    public long StartTime { get; set; }

    [JsonProperty("total_time")]
    public int TotalTime { get; set; }

    [JsonProperty("total_boss_time")]
    public int TotalBossTime { get; set; }

    [JsonProperty("total_pve_points_guild")]
    public int TotalPvePointsGuild { get; set; }

    [JsonProperty("total_pve_points_character")]
    public int TotalPvePointsCharacter { get; set; }

    [JsonProperty("total_damage")]
    public long TotalDamage { get; set; }

    [JsonProperty("total_healing")]
    public long TotalHealing { get; set; }

    [JsonProperty("average_gear_score")]
    public decimal AverageGearScore { get; set; }

    [JsonProperty("max_average_gear_score")]
    public decimal MaxAverageGearScore { get; set; }

    [JsonProperty("max_gear_score")]
    public int MaxGearScore { get; set; }

    [JsonProperty("leader_guid")]
    public long LeaderGuid { get; set; }

    [JsonProperty("leader_name")]
    public string LeaderName { get; set; } = string.Empty;

    [JsonProperty("leader_race")]
    public int LeaderRace { get; set; }

    [JsonProperty("guild_id")]
    public int GuildId { get; set; }

    [JsonProperty("guild_name")]
    public string GuildName { get; set; } = string.Empty;

    [JsonProperty("total_boss_number")]
    public int TotalBossNumber { get; set; }

    [JsonProperty("completed_boss_number")]
    public int CompletedBossNumber { get; set; }

    [JsonProperty("last_boss_completed")]
    public int LastBossCompleted { get; set; }

    [JsonProperty("wipes")]
    public int Wipes { get; set; }

    [JsonProperty("trash_clear")]
    public int TrashClear { get; set; }

    [JsonProperty("trash_first_not_killed_guid")]
    public long TrashFirstNotKilledGuid { get; set; }

    [JsonProperty("special")]
    public int Special { get; set; }

    [JsonProperty("special2")]
    public int Special2 { get; set; }

    [JsonProperty("race")]
    public int Race { get; set; }

    [JsonProperty("rank")]
    public int Rank { get; set; }

    [JsonIgnore]
    public List<RaidEncounter> Details { get; set; } = [];
}