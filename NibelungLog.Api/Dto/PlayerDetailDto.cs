namespace NibelungLog.Api.Dto;

public sealed class PlayerDetailDto
{
    public int Id { get; set; }
    public required string CharacterName { get; set; }
    public required string CharacterClass { get; set; }
    public required string CharacterRace { get; set; }
    public required string CharacterLevel { get; set; }
    public int TotalEncounters { get; set; }
    public long TotalDamage { get; set; }
    public long TotalHealing { get; set; }
    public double AverageDps { get; set; }
}

