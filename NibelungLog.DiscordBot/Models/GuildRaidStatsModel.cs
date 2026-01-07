namespace NibelungLog.DiscordBot.Models;

public sealed class GuildRaidStatsModel
{
    public required string GuildName { get; set; }
    public required List<RaidStatsItem> Raids { get; set; }
}

public sealed class RaidStatsItem
{
    public required int Id { get; set; }
    public required string RaidName { get; set; }
    public required string LeaderName { get; set; }
    public required DateTime StartTime { get; set; }
    public required int CompletedBosses { get; set; }
    public required int TotalBosses { get; set; }
    public required int Wipes { get; set; }
    public required long TotalDamage { get; set; }
    public required long TotalHealing { get; set; }
    public required long TotalTime { get; set; }
}

