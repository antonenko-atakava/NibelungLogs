using NibelungLog.Domain.Entities;

namespace Parser;

public class ParserOptions
{
    public int ServerId { get; set; } = 5;
    public int RealmId { get; set; } = 5;
    public int[] MapIds { get; set; } = [616];
    public int SaveBatchSize { get; set; } = 50;
    public List<RaidType> RaidTypes { get; set; } = [];
}