using Newtonsoft.Json;

namespace Parser.Models;

public sealed class RaidResult
{
    [JsonProperty("data")]
    public List<Raid> Data { get; set; } = new();

    [JsonProperty("total")]
    public long Total { get; set; }
}