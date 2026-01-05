namespace NibelungLog.DiscordBot.Models;

public sealed class TopPlayersModel
{
    public required List<TopPlayerDpsModel> Players { get; set; }
}

public sealed class TopPlayerDpsModel
{
    public required string PlayerName { get; set; }
    public required string ClassName { get; set; }
    public required double MaxDps { get; set; }
}

