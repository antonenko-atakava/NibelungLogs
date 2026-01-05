namespace NibelungLog.Types.Dto;

public sealed record RaidDetailRequestData
{
    public required int Page { get; init; }
    public required int Start { get; init; }
    public required int Limit { get; init; }
    public required List<SortOption> Sort { get; init; }
    public required string Id { get; init; }
    public required List<FilterOption> Filter { get; init; }
    public string? Time { get; init; }
}

