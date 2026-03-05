using Newtonsoft.Json;

namespace Parser.Models;

public class RaidEncounterDetailLoot
{
    [JsonProperty("entry")]
    public string Entry { get; set; }

    [JsonProperty("count")]
    public string Count { get; set; }
}