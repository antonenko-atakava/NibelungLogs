namespace NibelungLog.Entities;

public sealed class GuildMember
{
    public int Id { get; set; }
    public int GuildId { get; set; }
    public int PlayerId { get; set; }
    public required string Rank { get; set; }
    public DateTime? JoinedDate { get; set; }
    public required DateTime LastUpdated { get; set; }
    
    public Guild Guild { get; set; } = null!;
    public Player Player { get; set; } = null!;
}

