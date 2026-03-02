namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class GuildMemberData
{
    public required string Guid { get; set; }
    public required string Name { get; set; }
    public string? DeleteInfosName { get; set; }
    public required string Class { get; set; }
    public string? TotalTime { get; set; }
    public required string Race { get; set; }
    public required string Level { get; set; }
    public required string Gender { get; set; }
    public string? Money { get; set; }
    public required string Rank { get; set; }
}
