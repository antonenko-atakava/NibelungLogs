using System.Text.Json.Serialization;

namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class GuildListItemData
{
    [JsonPropertyName("guildid")]
    public required string Guildid { get; set; }
    
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("leaderguid")]
    public required string Leaderguid { get; set; }
    
    [JsonPropertyName("createdate")]
    public required string Createdate { get; set; }
    
    [JsonPropertyName("leaderName")]
    public required string LeaderName { get; set; }
    
    [JsonPropertyName("membersCount")]
    public required string MembersCount { get; set; }
}
