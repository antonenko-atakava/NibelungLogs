namespace NibelungLog.Domain.Types;

public static class RaceMappings
{
    public static readonly Dictionary<string, string> FactionByRaceId = new()
    {
        ["1"] = "Альянс",
        ["2"] = "Орда",
        ["3"] = "Альянс",
        ["4"] = "Альянс",
        ["5"] = "Орда",
        ["6"] = "Орда",
        ["7"] = "Альянс",
        ["8"] = "Орда",
        ["10"] = "Орда",
        ["11"] = "Альянс"
    };

    public static string? GetFactionByRaceId(string? raceId)
    {
        if (string.IsNullOrWhiteSpace(raceId))
            return null;
        
        return FactionByRaceId.TryGetValue(raceId, out var faction) ? faction : null;
    }

    public static List<string> GetRaceIdsByFaction(string? faction)
    {
        if (string.IsNullOrWhiteSpace(faction))
            return [];
        
        return FactionByRaceId
            .Where(kvp => kvp.Value.Equals(faction, StringComparison.OrdinalIgnoreCase))
            .Select(kvp => kvp.Key)
            .ToList();
    }
}
