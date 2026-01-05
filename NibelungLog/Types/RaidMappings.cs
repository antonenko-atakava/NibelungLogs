namespace NibelungLog.Types;

public static class RaidMappings
{
    public static readonly Dictionary<string, string> MapNames = new()
    {
        ["533"] = "Наксрамас",
        ["615"] = "Око Вечности",
        ["616"] = "Логово Магтеридона",
        ["617"] = "Крепость Бурь",
        ["603"] = "Ульдуар",
        ["649"] = "Испытание Крестоносца",
        ["631"] = "Ледяная Цитадель",
        ["724"] = "Рубиновое Святилище"
    };

    public static readonly Dictionary<string, string> DifficultyNames = new()
    {
        ["0"] = "10",
        ["1"] = "25"
    };

    public static string GetRaidDisplayName(string map, string difficulty, string instanceType)
    {
        var mapName = MapNames.TryGetValue(map, out var m) ? m : $"Карта {map}";
        var difficultyName = DifficultyNames.TryGetValue(difficulty, out var d) ? d : difficulty;
        
        return $"{mapName} {difficultyName}";
    }
}

