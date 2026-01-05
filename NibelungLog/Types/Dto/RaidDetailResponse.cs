namespace NibelungLog.Types.Dto;

public sealed record RaidDetailResponse
{
    public required List<EncounterRecord> Data { get; init; }
    public required string Total { get; init; }
}

