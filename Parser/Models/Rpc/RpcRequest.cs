using Newtonsoft.Json;

namespace Parser.Models.Rpc;

public class RpcRequest
{
    [JsonProperty("tid")]
    public int TId { get; set; } = 1;

    [JsonProperty("action")]
    public string Action { get; set; } = "wow_Services";

    [JsonProperty("method")]
    public string Method { get; set; }

    [JsonProperty("type")]
    public object Type { get; set; } = "rpc";

    [JsonProperty("data")]
    public object[]? Data { get; set; } = [];
}