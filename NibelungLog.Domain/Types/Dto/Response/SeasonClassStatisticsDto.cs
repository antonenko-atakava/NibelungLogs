namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class SeasonClassStatisticsDto
{
    public int Season { get; set; }
    public required string ClassName { get; set; }
    public double AverageDps { get; set; }
    public double AverageHps { get; set; }
    public int TotalEncounters { get; set; }
    public int TotalPlayers { get; set; }
}

public sealed class SeasonSpecStatisticsDto
{
    public int Season { get; set; }
    public required string ClassName { get; set; }
    public required string SpecName { get; set; }
    public double AverageDps { get; set; }
    public double AverageHps { get; set; }
    public int TotalEncounters { get; set; }
    public int TotalPlayers { get; set; }
}
