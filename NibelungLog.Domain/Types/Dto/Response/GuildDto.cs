namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class GuildDto
{
    public int Id { get; set; }
    public required string GuildId { get; set; }
    public required string GuildName { get; set; }
    public required string LeaderName { get; set; }
    public int MembersCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime? CreateDate { get; set; }
    public int FullRaidsCount { get; set; }
    public int UniqueRaidLeadersCount { get; set; }
    public int TopDamageDealersCount { get; set; }
    public double Rating { get; set; }
}
