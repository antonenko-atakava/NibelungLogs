namespace NibelungLog.Api.Dto;

public sealed class PlayerClassStatsDto
{
    public int Rank { get; set; }
    public int PlayerId { get; set; }
    public required string CharacterName { get; set; }
    public required string CharacterRace { get; set; }
    public required string CharacterLevel { get; set; }
    public required string CharacterSpec { get; set; }
    public int TotalEncounters { get; set; }
    public long TotalDamage { get; set; }
    public long TotalHealing { get; set; }
    public double AverageDps { get; set; }
    public double MaxDps { get; set; }
}

