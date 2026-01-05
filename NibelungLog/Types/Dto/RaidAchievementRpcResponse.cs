namespace NibelungLog.Types.Dto;

public sealed record RaidAchievementRpcResponse
{
    public required string Type { get; init; }
    public required int Tid { get; init; }
    public required string Action { get; init; }
    public required string Method { get; init; }
    public required RaidAchievementResponse Result { get; init; }
}

