namespace NibelungLog.Types.Dto;

public sealed class GuildInfoRpcResponse
{
    public required GuildInfoResult Result { get; set; }
}

public sealed class GuildInfoResult
{
    public required string GuildId { get; set; }
    public required string GuildName { get; set; }
}

