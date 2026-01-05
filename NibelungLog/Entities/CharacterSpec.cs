namespace NibelungLog.Entities;

public sealed class CharacterSpec
{
    public int Id { get; set; }
    public required string CharacterClass { get; set; }
    public required string Spec { get; set; }
    public string? Name { get; set; }
    
    public List<PlayerEncounter> PlayerEncounters { get; set; } = [];
}

