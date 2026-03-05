using Newtonsoft.Json;

namespace Parser.Models.Rpc;

public sealed class FilterModel
{
    [JsonProperty("property")]
    public string Property { get; set; } = string.Empty;

    [JsonProperty("value")]
    public object? Value { get; set; }
}