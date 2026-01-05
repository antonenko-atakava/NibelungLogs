namespace NibelungLog.DiscordBot.Utils;

public static class ClassNameMapper
{
    private static readonly Dictionary<string, string> EnglishToRussian = new()
    {
        { "warlock", "Чернокнижник" },
        { "warrior", "Воин" },
        { "paladin", "Паладин" },
        { "hunter", "Охотник" },
        { "rogue", "Разбойник" },
        { "priest", "Жрец" },
        { "deathknight", "Рыцарь смерти" },
        { "shaman", "Шаман" },
        { "mage", "Маг" },
        { "druid", "Друид" }
    };

    private static readonly Dictionary<string, string> RussianToEnglish = new()
    {
        { "чернокнижник", "warlock" },
        { "воин", "warrior" },
        { "паладин", "paladin" },
        { "охотник", "hunter" },
        { "разбойник", "rogue" },
        { "жрец", "priest" },
        { "рыцарь смерти", "deathknight" },
        { "шаман", "shaman" },
        { "маг", "mage" },
        { "друид", "druid" }
    };

    public static string? GetRussianName(string englishName)
    {
        return EnglishToRussian.TryGetValue(englishName.ToLowerInvariant(), out var russian) ? russian : null;
    }

    public static string? GetEnglishName(string russianName)
    {
        return RussianToEnglish.TryGetValue(russianName.ToLowerInvariant(), out var english) ? english : null;
    }

    public static string? GetRussianNameFromInput(string input)
    {
        var lowerInput = input.ToLowerInvariant();
        if (RussianToEnglish.ContainsKey(lowerInput))
            return input;
        
        if (EnglishToRussian.TryGetValue(lowerInput, out var russian))
            return russian;
        
        return null;
    }
}

