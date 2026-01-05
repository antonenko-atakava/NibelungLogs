namespace NibelungLog.Api.Dto;

public sealed class RaidDetailDto
{
    public int Id { get; set; }
    public required string RaidId { get; set; }
    public required string RaidTypeName { get; set; }
    public required string GuildName { get; set; }
    public required string LeaderName { get; set; }
    public DateTime StartTime { get; set; }
    public long TotalTime { get; set; }
    public long TotalDamage { get; set; }
    public long TotalHealing { get; set; }
    public int Wipes { get; set; }
    public int CompletedBosses { get; set; }
    public int TotalBosses { get; set; }
    public required List<EncounterDto> Encounters { get; set; }
}

