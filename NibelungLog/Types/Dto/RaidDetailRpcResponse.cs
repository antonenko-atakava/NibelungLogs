namespace NibelungLog.Types.Dto;

public sealed record RaidDetailRpcResponse
{
    public required string Type { get; init; }
    public required int Tid { get; init; }
    public required string Action { get; init; }
    public required string Method { get; init; }
    public required RaidDetailResponse Result { get; init; }
}

