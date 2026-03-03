namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class PlayerEncounterDetailDto
{
    public int PlayerEncounterId { get; set; }
    public int EncounterId { get; set; }
    public required string EncounterName { get; set; }
    public required string EncounterEntry { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public long Duration { get; set; }
    public bool Success { get; set; }
    
    public required string SpecName { get; set; }
    public required string Role { get; set; }
    
    public long DamageDone { get; set; }
    public long HealingDone { get; set; }
    public long AbsorbProvided { get; set; }
    public double Dps { get; set; }
    public double? Hps { get; set; }
    
    public required string ItemLevel { get; set; }
    
    public int RaidId { get; set; }
    public string? RaidName { get; set; }
    public string? RaidTypeName { get; set; }
    public string? CharacterClass { get; set; }
    public string? ClassName { get; set; }
}
