namespace NibelungLog.DiscordBot.Models;

public sealed class RaidDetailsModel
{
    public required int RaidId { get; set; }
    public required string RaidName { get; set; }
    public required string GuildName { get; set; }
    public required string LeaderName { get; set; }
    public required DateTime StartTime { get; set; }
    public required long TotalTime { get; set; }
    public required int Wipes { get; set; }
    public required List<EncounterDetailsModel> Encounters { get; set; }
}

public sealed class EncounterDetailsModel
{
    public required int Id { get; set; }
    public required string EncounterEntry { get; set; }
    public required string EncounterName { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required bool Success { get; set; }
    public required long TotalDamage { get; set; }
    public required long TotalHealing { get; set; }
    public required double AverageDps { get; set; }
    public required int Attempts { get; set; }
    public required int Wipes { get; set; }
    public required int Tanks { get; set; }
    public required int Healers { get; set; }
    public required int DamageDealers { get; set; }
}

