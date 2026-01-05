namespace NibelungLog.Entities;

public sealed class Raid
{
    public int Id { get; set; }
    public required string RaidId { get; set; }
    public int RaidTypeId { get; set; }
    public required string GuildName { get; set; }
    public required string LeaderName { get; set; }
    public required string LeaderGuid { get; set; }
    public required DateTime StartTime { get; set; }
    public required long TotalTime { get; set; }
    public required long TotalDamage { get; set; }
    public required long TotalHealing { get; set; }
    public required string AverageGearScore { get; set; }
    public required string MaxGearScore { get; set; }
    public required int Wipes { get; set; }
    public required int CompletedBosses { get; set; }
    public required int TotalBosses { get; set; }
    
    public RaidType RaidType { get; set; } = null!;
    public List<Encounter> Encounters { get; set; } = [];
}

