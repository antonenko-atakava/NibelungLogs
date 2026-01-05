namespace NibelungLog.DiscordBot.Utils;

public static class ClassIconMapper
{
    private static readonly Dictionary<string, string> ClassNameToIconFile = new()
    {
        { "Паладин", "paladin.png" },
        { "Друид", "druid.png" },
        { "Рыцарь смерти", "deathknight.png" },
        { "Воин", "warrior.png" },
        { "Маг", "mage.png" },
        { "Шаман", "shaman.png" },
        { "Жрец", "priest.png" },
        { "Разбойник", "rogue.png" },
        { "Охотник", "hunter.png" },
        { "Чернокнижник", "warlock.png" }
    };

    public static string? GetIconFileName(string className)
    {
        return ClassNameToIconFile.TryGetValue(className, out var fileName) ? fileName : null;
    }

    public static string GetIconPath(string className)
    {
        var fileName = GetIconFileName(className);
        if (fileName == null)
            return string.Empty;
        
        return Path.Combine("images", "class", "64", fileName);
    }
}

