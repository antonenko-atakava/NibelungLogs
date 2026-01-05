namespace NibelungLog.Entities;

public sealed class RaidType
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Map { get; set; }
    public required string Difficulty { get; set; }
    public required string InstanceType { get; set; }
    
    public List<Raid> Raids { get; set; } = [];
}

