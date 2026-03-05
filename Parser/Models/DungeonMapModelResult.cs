using Newtonsoft.Json;

namespace Parser.Models;

public sealed class DungeonMapModelResult
{
    [JsonProperty("ID")]
    public int Id { get; set; }

    [JsonProperty("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("Name_loc8")]
    public string NameLocalized { get; set; } = string.Empty;

    [JsonProperty("Type")]
    public int Type { get; set; }

    [JsonProperty("Expansion")]
    public int Expansion { get; set; }

    [JsonProperty("EncountersNumber")]
    public int EncountersNumber { get; set; }

    [JsonProperty("UnusedInPvELog")]
    public int UnusedInPvELog { get; set; }
}