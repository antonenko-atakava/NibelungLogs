namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class GuildMembersResult
{
    public required string Total { get; set; }
    public required List<GuildMemberData> Data { get; set; }
}
