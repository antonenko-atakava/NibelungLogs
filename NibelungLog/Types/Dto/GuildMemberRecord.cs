namespace NibelungLog.Types.Dto;

public sealed class GuildMemberRecord
{
    public required string CharacterGuid { get; set; }
    public required string CharacterName { get; set; }
    public required string CharacterRace { get; set; }
    public required string CharacterClass { get; set; }
    public required string CharacterGender { get; set; }
    public required string CharacterLevel { get; set; }
    public required string Rank { get; set; }
}

