namespace NibelungLog.Entities;

public sealed class Encounter
{
    public int Id { get; set; }
    public int RaidId { get; set; }
    public required string EncounterEntry { get; set; }
    public string? EncounterName { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required bool Success { get; set; }
    public required long TotalDamage { get; set; }
    public required long TotalHealing { get; set; }
    public required int Tanks { get; set; }
    public required int Healers { get; set; }
    public required int DamageDealers { get; set; }
    public required string AverageGearScore { get; set; }
    
    public Raid Raid { get; set; } = null!;
    public List<PlayerEncounter> PlayerEncounters { get; set; } = [];
}

