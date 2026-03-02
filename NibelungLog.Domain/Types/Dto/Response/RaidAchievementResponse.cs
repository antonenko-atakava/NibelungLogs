using NibelungLog.Domain.Types.Dto;

namespace NibelungLog.Domain.Types.Dto.Response;

public sealed record RaidAchievementResponse
{
    public required List<AchievementRecord> Data { get; init; }
    public required string Total { get; init; }
}
