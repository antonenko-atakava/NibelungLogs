using System.Text.Json.Serialization;

namespace NibelungLog.Domain.Types.Dto;

public sealed record LootItem
{
    [JsonPropertyName("entry")]
    public required string Entry { get; init; }
    
    [JsonPropertyName("count")]
    public required string Count { get; init; }
}
