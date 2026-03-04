namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class PlayerSpecComparisonDto
{
    public required string SpecName { get; set; }
    public List<PlayerSpecComparisonItemDto> Players { get; set; } = [];
    public int CurrentPlayerRank { get; set; }
    public int CurrentPlayerId { get; set; }
    public required string CurrentPlayerName { get; set; }
    public double CurrentPlayerValue { get; set; }
}

public sealed class PlayerSpecComparisonItemDto
{
    public int PlayerId { get; set; }
    public required string CharacterName { get; set; }
    public double Value { get; set; }
    public bool IsCurrentPlayer { get; set; }
    public int Rank { get; set; }
    public string? ClassName { get; set; }
}
