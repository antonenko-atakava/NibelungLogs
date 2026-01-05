using Discord;

namespace NibelungLog.DiscordBot.Utils;

public static class ClassColors
{
    private static readonly Dictionary<string, Color> ClassColorMap = new()
    {
        { "Паладин", new Color(0xF58CBA) },
        { "Друид", new Color(0xFF7D0A) },
        { "Рыцарь смерти", new Color(0xC41F3B) },
        { "Воин", new Color(0xC79C6E) },
        { "Маг", new Color(0x69CCF0) },
        { "Шаман", new Color(0x0070DE) },
        { "Жрец", new Color(0xFFFFFF) },
        { "Разбойник", new Color(0xFFF569) },
        { "Охотник", new Color(0xABD473) },
        { "Чернокнижник", new Color(0x9482C9) }
    };

    private static readonly Dictionary<string, string> ClassColorSquare = new()
    {
        { "Паладин", "🟣" },
        { "Друид", "🟠" },
        { "Рыцарь смерти", "🔴" },
        { "Воин", "🟤" },
        { "Маг", "🔵" },
        { "Шаман", "🔷" },
        { "Жрец", "⚪" },
        { "Разбойник", "🟡" },
        { "Охотник", "🟢" },
        { "Чернокнижник", "🟪" }
    };

    public static Color GetClassColor(string className)
    {
        return ClassColorMap.TryGetValue(className, out var color) 
            ? color 
            : Color.LightGrey;
    }

    public static string FormatClassWithColor(string className, int count)
    {
        var square = ClassColorSquare.TryGetValue(className, out var emoji) ? emoji : "⬜";
        return $"{square} **{className}**: {count}";
    }
}

