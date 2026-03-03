namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class PlayerEncounterTimelineDto
{
    public int EncounterId { get; set; }
    public required string EncounterName { get; set; }
    public DateTime StartTime { get; set; }
    public long Duration { get; set; }
    public bool Success { get; set; }
    public double Dps { get; set; }
    public double? Hps { get; set; }
    public long DamageDone { get; set; }
    public long HealingDone { get; set; }
    public required string SpecName { get; set; }
    public required string Role { get; set; }
    public required string ItemLevel { get; set; }
}
