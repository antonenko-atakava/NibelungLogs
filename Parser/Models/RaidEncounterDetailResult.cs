using Newtonsoft.Json;

namespace Parser.Models;

public class RaidEncounterDetailResult
{
    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("data")]
    public List<RaidEncounterDetail> Data { get; set; }
}