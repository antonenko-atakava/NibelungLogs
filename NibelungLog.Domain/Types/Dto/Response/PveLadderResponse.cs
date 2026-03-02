using NibelungLog.Domain.Types.Dto;

namespace NibelungLog.Domain.Types.Dto.Response;

public sealed record PveLadderResponse
{
    public required List<RaidRecord> Data { get; init; }
    public required string Total { get; init; }
}
