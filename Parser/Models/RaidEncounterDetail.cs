using Newtonsoft.Json;

namespace Parser.Models;

public class RaidEncounterDetail
{
    [JsonProperty("log_instance_id")]
    public long LogInstanceId { get; set; }

    [JsonProperty("encounter_entry")]
    public int EncounterEntry { get; set; }

    [JsonProperty("start_time")]
    public long StartTime { get; set; }

    [JsonProperty("character_guid")]
    public long CharacterGuid { get; set; }

    [JsonProperty("character_name")]
    public string CharacterName { get; set; }

    [JsonProperty("character_race")]
    public int CharacterRace { get; set; }

    [JsonProperty("character_class")]
    public int CharacterClass { get; set; }

    [JsonProperty("character_spec")]
    public int CharacterSpec { get; set; }

    [JsonProperty("character_gender")]
    public int CharacterGender { get; set; }

    [JsonProperty("character_level")]
    public int CharacterLevel { get; set; }

    [JsonProperty("character_role")]
    public int CharacterRole { get; set; }

    [JsonProperty("max_average_gear_score")]
    public decimal MaxAverageGearScore { get; set; }

    [JsonProperty("max_gear_score")]
    public int MaxGearScore { get; set; }

    [JsonProperty("damage_done")]
    public long DamageDone { get; set; }

    [JsonProperty("healing_done")]
    public long HealingDone { get; set; }

    [JsonProperty("absorb_provided")]
    public long AbsorbProvided { get; set; }

    [JsonProperty("valid_looter")]
    public int ValidLooter { get; set; }

    [JsonProperty("loot")]
    public List<RaidEncounterDetailLoot> Loot { get; set; }
}