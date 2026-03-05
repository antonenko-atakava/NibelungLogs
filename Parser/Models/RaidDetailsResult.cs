using Newtonsoft.Json;

namespace Parser.Models;

public class RaidDetailsResult
{
    [JsonProperty("total")]
    public long Total { get; set; }

    [JsonProperty("data")]
    public List<RaidEncounter> Data { get; set; } = new();
}