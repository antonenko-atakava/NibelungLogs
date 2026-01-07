namespace NibelungLog.DiscordBot.Models;

public sealed class BossEncounterDetailsModel
{
    public required int RaidId { get; set; }
    public required string RaidName { get; set; }
    public required string GuildName { get; set; }
    public required string BossName { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required bool Success { get; set; }
    public required long TotalTime { get; set; }
    public required List<PlayerEncounterDetailsModel> Players { get; set; }
}

public sealed class PlayerEncounterDetailsModel
{
    public required string PlayerName { get; set; }
    public required string ClassName { get; set; }
    public required string SpecName { get; set; }
    public required string Role { get; set; }
    public required double Dps { get; set; }
    public required double Hps { get; set; }
    public required long DamageDone { get; set; }
    public required long HealingDone { get; set; }
}

