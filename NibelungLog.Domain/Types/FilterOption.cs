using System.Text.Json;

namespace NibelungLog.Domain.Types.Dto;

public sealed record FilterOption
{
    public required string Property { get; init; }
    public required JsonElement Value { get; init; }
}
