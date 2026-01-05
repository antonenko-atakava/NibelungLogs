using System.Text.Json;

namespace NibelungLog.Types.Dto;

public sealed record FilterOption
{
    public required string Property { get; init; }
    public required JsonElement Value { get; init; }
}

