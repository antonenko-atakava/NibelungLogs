namespace NibelungLog.Types.Dto;

public sealed class GuildMembersRpcRequest
{
    public required string Type { get; set; }
    public required int Tid { get; set; }
    public required string Action { get; set; }
    public required string Method { get; set; }
    public required List<GuildMembersRequestData> Data { get; set; }
}

