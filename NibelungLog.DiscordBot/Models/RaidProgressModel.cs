namespace NibelungLog.DiscordBot.Models;

public sealed class RaidProgressModel
{
    public required string RaidName { get; set; }
    public required string GuildName { get; set; }
    public required DateTime StartTime { get; set; }
    public required long TotalTime { get; set; }
    public required int Wipes { get; set; }
    public required long TotalDamage { get; set; }
    public required long TotalHealing { get; set; }
    public required double AverageDps { get; set; }
    public required List<TopPlayerModel> TopDpsPlayers { get; set; }
    public required List<TopPlayerModel> TopHealingPlayers { get; set; }
    public required int CompletedBosses { get; set; }
    public required int TotalBosses { get; set; }
}

public sealed class TopPlayerModel
{
    public required string PlayerName { get; set; }
    public required string ClassName { get; set; }
    public required double Value { get; set; }
}
