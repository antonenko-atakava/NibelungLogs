using Newtonsoft.Json;

namespace Parser.Models.Rpc;

public sealed class SortModel
{
    [JsonProperty("property")]
    public string? Property { get; set; }

    [JsonProperty("direction")]
    public string Direction { get; set; } = string.Empty;
}