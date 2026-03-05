namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class GuildDetailDto
{
    public int Id { get; set; }
    public required string GuildId { get; set; }
    public required string GuildName { get; set; }
    public required string LeaderName { get; set; }
    public int MembersCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime? CreateDate { get; set; }
    public int FullRaidsCount { get; set; }
    public int TotalRaidsCount { get; set; }
    public int UniqueRaidLeadersCount { get; set; }
    public int TopDamageDealersCount { get; set; }
    public int TotalEncountersCount { get; set; }
    public double Rating { get; set; }
}

public sealed class GuildMemberDto
{
    public int PlayerId { get; set; }
    public required string CharacterName { get; set; }
    public required string CharacterClass { get; set; }
    public string? ClassName { get; set; }
    public string? SpecName { get; set; }
    public string? Role { get; set; }
    public required string Rank { get; set; }
    public DateTime? JoinedDate { get; set; }
    public int TotalEncounters { get; set; }
    public double AverageDps { get; set; }
    public double MaxDps { get; set; }
    public double? AverageHps { get; set; }
    public double? MaxHps { get; set; }
}
