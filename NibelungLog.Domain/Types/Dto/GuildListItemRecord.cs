namespace NibelungLog.Domain.Types.Dto;

public sealed class GuildListItemRecord
{
    public required string GuildId { get; set; }
    public required string Name { get; set; }
    public required string LeaderGuid { get; set; }
    public required string CreateDate { get; set; }
    public required string LeaderName { get; set; }
    public required string MembersCount { get; set; }
}
