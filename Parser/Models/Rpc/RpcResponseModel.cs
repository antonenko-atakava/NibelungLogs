using Newtonsoft.Json;

namespace Parser.Models.Rpc;

public sealed class RpcResponseModel<TResult>
{
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("tid")]
    public int Tid { get; set; }

    [JsonProperty("action")]
    public string Action { get; set; } = string.Empty;

    [JsonProperty("method")]
    public string Method { get; set; } = string.Empty;

    [JsonProperty("result")]
    public TResult? Result { get; set; }
}
