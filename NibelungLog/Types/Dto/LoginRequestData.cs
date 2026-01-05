namespace NibelungLog.Types.Dto;

public sealed record LoginRequestData
{
    public required string AccountName { get; init; }
    public required string Password { get; init; }
    public string Captcha { get; init; } = string.Empty;
}

