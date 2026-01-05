namespace NibelungLog.Types.Dto;

public sealed class GuildMembersRequestData
{
    public required int Page { get; set; }
    public required int Start { get; set; }
    public required int Limit { get; set; }
    public required string GuildId { get; set; }
    public required List<SortOption> Sort { get; set; }
    public required List<FilterOption> Filter { get; set; }
}

