using System.Text.Json.Serialization;

namespace NibelungLog.ParserGuild.Models;

public sealed class ResponseEnvelope<TResult>
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("tid")]
    public int RequestIdentifier { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public TResult? Result { get; set; }
}
