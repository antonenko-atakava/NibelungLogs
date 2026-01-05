namespace NibelungLog.Types.Dto;

public sealed record RpcRequest
{
    public required string Type { get; init; }
    public required int Tid { get; init; }
    public required string Action { get; init; }
    public required string Method { get; init; }
    public required List<LoginRequestData> Data { get; init; }
}

