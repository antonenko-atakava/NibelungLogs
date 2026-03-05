using Newtonsoft.Json;

namespace Parser.Models.Rpc;

public sealed class RequestData
{
    [JsonProperty("page")]
    public uint Page { get; set; } = 1;

    [JsonProperty("start")]
    public uint Start { get; set; } = 0;

    [JsonProperty("limit")]
    public uint Limit { get; set; } = 100;

    [JsonProperty("id")]
    public object Id { get; set; }

    [JsonProperty("time")]
    public object Time { get; set; }

    [JsonProperty("sort")]
    public List<SortModel> Sort { get; set; } =
    [
        new()
        {
            Property = null,
            Direction = "ASC"
        }
    ];

    [JsonProperty("filter")]
    public List<FilterModel> Filter { get; set; } = [];
}