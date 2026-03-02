using NibelungLog.Domain.Types.Dto;

namespace NibelungLog.Domain.Types.Dto.Response;

public sealed record RaidDetailResponse
{
    public required List<EncounterRecord> Data { get; init; }
    public required string Total { get; init; }
}
