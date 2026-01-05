namespace NibelungLog.Types.Dto;

public sealed record LoginResult
{
    public required string ArmoryUrl { get; init; }
    public required bool IsAuth { get; init; }
    public required string Name { get; init; }
    public required string Id { get; init; }
    public required int AccountLevel { get; init; }
    public required string ServerType { get; init; }
    public required string Realm { get; init; }
    public required int Version { get; init; }
    public required string RecaptchaKey { get; init; }
}

