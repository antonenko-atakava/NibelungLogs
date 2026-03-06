namespace NibelungLog.ParserGuild;

public sealed class GuildParserOptions
{
    public int ServerId { get; set; } = 5;
    public int MinimumGuildMembersCount { get; set; } = 10;
    public int GuildPageSize { get; set; } = 25;
    public int GuildMemberPageSize { get; set; } = 25;
    public int RequestBatchSize { get; set; } = 10;
}
