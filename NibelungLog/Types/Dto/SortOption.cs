namespace NibelungLog.Types.Dto;

public sealed record SortOption
{
    public string? Property { get; init; }
    public required string Direction { get; init; }
}

