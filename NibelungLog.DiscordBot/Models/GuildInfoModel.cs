namespace NibelungLog.DiscordBot.Models;

public sealed class GuildInfoModel
{
    public required string GuildName { get; set; }
    public required int TotalMembers { get; set; }
    public required DateTime LastUpdated { get; set; }
    public required Dictionary<string, int> MembersByRank { get; set; }
    public required Dictionary<string, int> MembersByClass { get; set; }
}

