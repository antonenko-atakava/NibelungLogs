namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class GuildBossStatisticsDto
{
    public required string EncounterName { get; set; }
    public required string EncounterEntry { get; set; }
    public int TotalAttempts { get; set; }
    public int SuccessfulAttempts { get; set; }
    public double SuccessRate { get; set; }
    public double AverageKillTimeSeconds { get; set; }
    public int TotalKills { get; set; }
}
