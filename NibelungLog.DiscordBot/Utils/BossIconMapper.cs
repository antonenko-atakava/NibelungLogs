namespace NibelungLog.DiscordBot.Utils;

public static class BossIconMapper
{
    private static readonly Dictionary<string, string> BossNameToIcon = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Ануб'Рекан", "анубрекан.png" },
        { "Великий военачальник Варенн", "всадники.png" },
        { "Тан Варенн", "всадники.png" },
        { "Тан Кортазз", "всадники.png" },
        { "Всадники", "всадники.png" },
        { "Глут", "глут.png" },
        { "Готик Жнец", "готик жнец.png" },
        { "Гробулус", "гробулус.png" },
        { "Гроббулус", "гробулус.png" },
        { "Кель'Тузад", "культузад.png" },
        { "Кельтузад", "культузад.png" },
        { "Культузад", "культузад.png" },
        { "Лоскутик", "лоскутик.png" },
        { "Лотхиб", "лотхиб.png" },
        { "Мексна", "мексна.png" },
        { "Нот Чумной", "нот чумной.png" },
        { "Разувий", "разувий.png" },
        { "Сапфирон", "сапфирон.png" },
        { "Тадиус", "тадиус.png" },
        { "Таддиус", "тадиус.png" },
        { "Фарлина", "фарлина.png" },
        { "Хейган Нечестивый", "хейган.png" },
        { "Хейган", "хейган.png" }
    };

    private static readonly Dictionary<string, string> BossNameDisplay = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Тан Кортазз", "Всадники" },
        { "Великий военачальник Варенн", "Всадники" },
        { "Тан Варенн", "Всадники" }
    };

    public static string? GetIconFileName(string encounterName)
    {
        if (string.IsNullOrWhiteSpace(encounterName))
            return null;

        if (BossNameToIcon.TryGetValue(encounterName, out var iconFileName))
            return iconFileName;

        foreach (var kvp in BossNameToIcon)
        {
            if (encounterName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase) ||
                kvp.Key.Contains(encounterName, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        return null;
    }

    public static string GetDisplayName(string encounterName)
    {
        if (string.IsNullOrWhiteSpace(encounterName))
            return encounterName;

        if (BossNameDisplay.TryGetValue(encounterName, out var displayName))
            return displayName;

        foreach (var kvp in BossNameDisplay)
        {
            if (encounterName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase) ||
                kvp.Key.Contains(encounterName, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        return encounterName;
    }
}

