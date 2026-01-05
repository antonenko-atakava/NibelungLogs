namespace NibelungLog.Api.Dto;

public sealed class EncounterDto
{
    public int Id { get; set; }
    public required string EncounterEntry { get; set; }
    public string? EncounterName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool Success { get; set; }
    public long TotalDamage { get; set; }
    public long TotalHealing { get; set; }
    public int Tanks { get; set; }
    public int Healers { get; set; }
    public int DamageDealers { get; set; }
}

