using NibelungLog.Domain.Types.Dto;

namespace NibelungLog.Domain.Types.Dto.Request;

public sealed class GuildsRequestData
{
    public required int Page { get; set; }
    public required int Start { get; set; }
    public required int Limit { get; set; }
    public required List<SortOption> Sort { get; set; }
    public required List<FilterOption> Filter { get; set; }
}
