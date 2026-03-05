namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class GuildStatisticsDto
{
    public required List<GuildClassStatisticsDto> Classes { get; set; }
    public required List<GuildSpecStatisticsDto> Specs { get; set; }
    public required List<GuildRoleStatisticsDto> Roles { get; set; }
}

public sealed class GuildClassStatisticsDto
{
    public required string ClassName { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public sealed class GuildSpecStatisticsDto
{
    public required string SpecName { get; set; }
    public required string ClassName { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public sealed class GuildRoleStatisticsDto
{
    public required string Role { get; set; }
    public required string RoleName { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}
