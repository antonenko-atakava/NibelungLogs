using Newtonsoft.Json;

namespace Parser.Models;

public class RaidEncounter
{
    [JsonProperty("log_instance_id")]
    public int LogInstanceId { get; set; }

    [JsonProperty("encounter_entry")]
    public int EncounterEntry { get; set; }

    [JsonProperty("start_time")]
    public int StartTime { get; set; }

    [JsonProperty("end_time")]
    public int EndTime { get; set; }

    [JsonProperty("success")]
    public string Success { get; set; }

    [JsonProperty("pve_points_guild")]
    public string PvePointsGuild { get; set; }

    [JsonProperty("pve_points_character")]
    public string PvePointsCharacter { get; set; }

    [JsonProperty("master_looter_guid")]
    public string MasterLooterGuid { get; set; }

    [JsonProperty("total_damage")]
    public string TotalDamage { get; set; }

    [JsonProperty("total_healing")]
    public string TotalHealing { get; set; }

    [JsonProperty("average_gear_score")]
    public string AverageGearScore { get; set; }

    [JsonProperty("max_average_gear_score")]
    public string MaxAverageGearScore { get; set; }

    [JsonProperty("max_gear_score")]
    public string MaxGearScore { get; set; }

    [JsonProperty("tanks")]
    public string Tanks { get; set; }

    [JsonProperty("healers")]
    public string Healers { get; set; }

    [JsonProperty("damage_dealers")]
    public string DamageDealers { get; set; }
}