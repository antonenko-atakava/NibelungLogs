namespace NibelungLog.Types.Dto;

public sealed record PlayerEncounterResponse
{
    public required List<PlayerEncounterRecord> Data { get; init; }
    public required string Total { get; init; }
}

