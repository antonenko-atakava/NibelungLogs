namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class GuildProgressDto
{
    public DateTime StartTime { get; set; }
    public string RaidTypeName { get; set; } = string.Empty;
    public int Wipes { get; set; }
    public int CompletedBosses { get; set; }
    public int TotalBosses { get; set; }
    public double ProgressScore { get; set; }
}
