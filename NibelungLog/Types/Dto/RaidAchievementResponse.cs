namespace NibelungLog.Types.Dto;

public sealed record RaidAchievementResponse
{
    public required List<AchievementRecord> Data { get; init; }
    public required string Total { get; init; }
}

