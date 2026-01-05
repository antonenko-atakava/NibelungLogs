namespace NibelungLog.Types.Dto;

public sealed record PveLadderResponse
{
    public required List<RaidRecord> Data { get; init; }
    public required string Total { get; init; }
}

