namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class GuildsResult
{
    public required string Total { get; set; }
    public required List<GuildListItemData> Data { get; set; }
}
