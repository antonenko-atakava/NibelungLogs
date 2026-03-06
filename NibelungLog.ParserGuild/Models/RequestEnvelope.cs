using System.Text.Json.Serialization;

namespace NibelungLog.ParserGuild.Models;

public sealed class RequestEnvelope<TRequestData>
{
    [JsonPropertyName("tid")]
    public int RequestIdentifier { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; } = "wow_Services";

    [JsonPropertyName("method")]
    public required string Method { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "rpc";

    [JsonPropertyName("data")]
    public required List<TRequestData> Data { get; set; }
}
