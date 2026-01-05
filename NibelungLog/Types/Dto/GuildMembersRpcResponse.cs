namespace NibelungLog.Types.Dto;

public sealed class GuildMembersRpcResponse
{
    public required GuildMembersResult Result { get; set; }
}

public sealed class GuildMembersResult
{
    public required string Total { get; set; }
    public required List<GuildMemberData> Data { get; set; }
}

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

