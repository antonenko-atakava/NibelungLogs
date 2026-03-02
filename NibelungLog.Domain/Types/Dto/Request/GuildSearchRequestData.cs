namespace NibelungLog.Domain.Types.Dto.Request;

public sealed class GuildSearchRequestData
{
    public required string GuildName { get; set; }
    public required int ServerId { get; set; }
}
