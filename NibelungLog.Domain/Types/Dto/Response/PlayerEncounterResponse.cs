using NibelungLog.Domain.Types.Dto;

namespace NibelungLog.Domain.Types.Dto.Response;

public sealed record PlayerEncounterResponse
{
    public required List<PlayerEncounterRecord> Data { get; init; }
    public required string Total { get; init; }
}
