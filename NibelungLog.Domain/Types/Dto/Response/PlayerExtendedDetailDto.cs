namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class PlayerExtendedDetailDto
{
    public int Id { get; set; }
    public required string CharacterName { get; set; }
    public required string CharacterClass { get; set; }
    public string? ClassName { get; set; }
    public required string CharacterRace { get; set; }
    public required string CharacterLevel { get; set; }
    
    public int TotalEncounters { get; set; }
    public int SuccessfulEncounters { get; set; }
    public int FailedEncounters { get; set; }
    
    public long TotalDamage { get; set; }
    public long TotalHealing { get; set; }
    public long TotalAbsorbProvided { get; set; }
    
    public double AverageDps { get; set; }
    public double MaxDps { get; set; }
    public double MinDps { get; set; }
    
    public double? AverageHps { get; set; }
    public double? MaxHps { get; set; }
    public double? MinHps { get; set; }
    
    public string? BestItemLevel { get; set; }
    public string? CurrentItemLevel { get; set; }
    
    public DateTime? FirstEncounterDate { get; set; }
    public DateTime? LastEncounterDate { get; set; }
    
    public List<PlayerSpecStatisticsDto> SpecStatistics { get; set; } = [];
    public List<PlayerRoleStatisticsDto> RoleStatistics { get; set; } = [];
}

public sealed class PlayerSpecStatisticsDto
{
    public required string SpecName { get; set; }
    public int EncountersCount { get; set; }
    public double AverageDps { get; set; }
    public double MaxDps { get; set; }
    public double? AverageHps { get; set; }
    public double? MaxHps { get; set; }
}

public sealed class PlayerRoleStatisticsDto
{
    public required string Role { get; set; }
    public int EncountersCount { get; set; }
    public double AverageDps { get; set; }
    public double MaxDps { get; set; }
    public double? AverageHps { get; set; }
    public double? MaxHps { get; set; }
}
