namespace NibelungLog.Api.Dto;

public sealed class PlayerDto
{
    public int Rank { get; set; }
    public int Id { get; set; }
    public required string CharacterName { get; set; }
    public required string CharacterClass { get; set; }
    public string? ClassName { get; set; }
    public string? SpecName { get; set; }
    public required string CharacterRace { get; set; }
    public required string CharacterLevel { get; set; }
    public int TotalEncounters { get; set; }
    public long TotalDamage { get; set; }
    public long TotalHealing { get; set; }
    public double AverageDps { get; set; }
    public double MaxDps { get; set; }
    public double? MaxHps { get; set; }
    public DateTime? EncounterDate { get; set; }
    public long? EncounterDuration { get; set; }
    public string? ItemLevel { get; set; }
    public double? Parse { get; set; }
    public string? Role { get; set; }
    public int? EncounterId { get; set; }
}

