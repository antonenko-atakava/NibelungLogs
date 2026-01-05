namespace NibelungLog.Api.Dto;

public sealed class RaidTypeDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Map { get; set; }
    public required string Difficulty { get; set; }
    public required string InstanceType { get; set; }
}

