namespace NibelungLog.Domain.Entities;

public sealed class Guild
{
    public int Id { get; set; }
    public required string GuildId { get; set; }
    public required string GuildName { get; set; }
    public required string LeaderGuid { get; set; }
    public required string CreateDate { get; set; }
    public required string LeaderName { get; set; }
    public required DateTime LastUpdated { get; set; }
    
    public List<GuildMember> Members { get; set; } = [];
}
