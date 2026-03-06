namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class GuildRaidStatisticsDto
{
    public double AverageWipesPerRaid { get; set; }
    public double SuccessRate { get; set; }
    public double AverageRaidTimeMinutes { get; set; }
    public long TotalDamage { get; set; }
    public long TotalHealing { get; set; }
    public double AverageGearScore { get; set; }
    public double MaxGearScore { get; set; }
    public int TotalSuccessfulEncounters { get; set; }
    public int TotalFailedEncounters { get; set; }
    public double AverageRaidSize { get; set; }
}
