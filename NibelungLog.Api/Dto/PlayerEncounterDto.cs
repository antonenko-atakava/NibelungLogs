namespace NibelungLog.Api.Dto;

public sealed class PlayerEncounterDto
{
    public int Id { get; set; }
    public required string PlayerName { get; set; }
    public required string CharacterClass { get; set; }
    public string? ClassName { get; set; }
    public required string CharacterSpec { get; set; }
    public string? SpecName { get; set; }
    public required string Role { get; set; }
    public long DamageDone { get; set; }
    public long HealingDone { get; set; }
    public long AbsorbProvided { get; set; }
    public double Dps { get; set; }
    public required string MaxAverageGearScore { get; set; }
    public required string MaxGearScore { get; set; }
}

