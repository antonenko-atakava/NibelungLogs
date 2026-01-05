namespace NibelungLog.Entities;

public sealed class Player
{
    public int Id { get; set; }
    public required string CharacterGuid { get; set; }
    public required string CharacterName { get; set; }
    public required string CharacterRace { get; set; }
    public required string CharacterClass { get; set; }
    public string? ClassName { get; set; }
    public required string CharacterGender { get; set; }
    public required string CharacterLevel { get; set; }
    
    public List<PlayerEncounter> PlayerEncounters { get; set; } = [];
    public List<GuildMember> GuildMemberships { get; set; } = [];
}

