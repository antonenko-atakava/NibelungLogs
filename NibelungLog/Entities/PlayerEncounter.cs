namespace NibelungLog.Entities;

public sealed class PlayerEncounter
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int EncounterId { get; set; }
    public int CharacterSpecId { get; set; }
    public required string Role { get; set; }
    public required long DamageDone { get; set; }
    public required long HealingDone { get; set; }
    public required long AbsorbProvided { get; set; }
    public required double Dps { get; set; }
    public required string MaxAverageGearScore { get; set; }
    public required string MaxGearScore { get; set; }
    
    public Player Player { get; set; } = null!;
    public Encounter Encounter { get; set; } = null!;
    public CharacterSpec CharacterSpec { get; set; } = null!;
}

