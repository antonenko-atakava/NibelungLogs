namespace NibelungLog.DiscordBot.Utils;

public static class SpecIconMapper
{
    private static readonly Dictionary<string, string> SpecToIcon = new(StringComparer.OrdinalIgnoreCase)
    {
        { "deathknight/blood", "deathknight/blood.png" },
        { "deathknight/frost", "deathknight/frost.png" },
        { "deathknight/unholy", "deathknight/unholy.png" },
        { "druid/balance", "druid/balance.png" },
        { "druid/feral", "druid/feral.png" },
        { "druid/guardian", "druid/guardian.png" },
        { "druid/restoration", "druid/restoration.png" },
        { "hunter/beastmastery", "hunter/beastmastery.png" },
        { "hunter/marksman", "hunter/marksman.png" },
        { "hunter/survival", "hunter/survival.png" },
        { "mage/arcane", "mage/arcane.png" },
        { "mage/fire", "mage/fire.png" },
        { "mage/frost", "mage/frost.png" },
        { "monk/brewmaster", "monk/brewmaster.png" },
        { "monk/mistweaver", "monk/mistweaver.png" },
        { "monk/windwalker", "monk/windwalker.png" },
        { "paladin/holy", "paladin/holy.png" },
        { "paladin/protection", "paladin/protection.png" },
        { "paladin/retribution", "paladin/retribution.png" },
        { "priest/discipline", "priest/discipline.png" },
        { "priest/holy", "priest/holy.png" },
        { "priest/shadow", "priest/shadow.png" },
        { "rogue/assassination", "rogue/assassination.png" },
        { "rogue/combat", "rogue/combat.png" },
        { "rogue/subtlety", "rogue/subtlety.png" },
        { "shaman/elemental", "shaman/elemental.png" },
        { "shaman/enhancement", "shaman/enhancement.png" },
        { "shaman/restoration", "shaman/restoration.png" },
        { "warlock/affliction", "warlock/affliction.png" },
        { "warlock/demonology", "warlock/demonology.png" },
        { "warlock/destruction", "warlock/destruction.png" },
        { "warrior/arms", "warrior/arms.png" },
        { "warrior/fury", "warrior/fury.png" },
        { "warrior/protection", "warrior/protection.png" }
    };

    private static readonly Dictionary<string, string> ClassNameMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Рыцарь смерти", "deathknight" },
        { "Друид", "druid" },
        { "Охотник", "hunter" },
        { "Маг", "mage" },
        { "Монах", "monk" },
        { "Паладин", "paladin" },
        { "Жрец", "priest" },
        { "Разбойник", "rogue" },
        { "Шаман", "shaman" },
        { "Чернокнижник", "warlock" },
        { "Воин", "warrior" }
    };

    private static readonly Dictionary<(string Class, string Spec), string> SpecNameMapping = new()
    {
        { ("Рыцарь смерти", "Кровь"), "blood" },
        { ("Рыцарь смерти", "Лед"), "frost" },
        { ("Рыцарь смерти", "Нечестивость"), "unholy" },
        { ("Друид", "Баланс"), "balance" },
        { ("Друид", "Сила зверя"), "feral" },
        { ("Друид", "Страж"), "guardian" },
        { ("Друид", "Исцеление"), "restoration" },
        { ("Охотник", "Повелитель зверей"), "beastmastery" },
        { ("Охотник", "Стрельба"), "marksman" },
        { ("Охотник", "Выживание"), "survival" },
        { ("Маг", "Тайная магия"), "arcane" },
        { ("Маг", "Огонь"), "fire" },
        { ("Маг", "Лед"), "frost" },
        { ("Монах", "Пивовар"), "brewmaster" },
        { ("Монах", "Ткач туманов"), "mistweaver" },
        { ("Монах", "Танцующий с ветром"), "windwalker" },
        { ("Паладин", "Свет"), "holy" },
        { ("Паладин", "Защита"), "protection" },
        { ("Паладин", "Воздаяние"), "retribution" },
        { ("Жрец", "Послушание"), "discipline" },
        { ("Жрец", "Свет"), "holy" },
        { ("Жрец", "Тьма"), "shadow" },
        { ("Разбойник", "Убийство"), "assassination" },
        { ("Разбойник", "Бой"), "combat" },
        { ("Разбойник", "Скрытность"), "subtlety" },
        { ("Шаман", "Стихии"), "elemental" },
        { ("Шаман", "Улучшение"), "enhancement" },
        { ("Шаман", "Исцеление"), "restoration" },
        { ("Чернокнижник", "Колдовство"), "affliction" },
        { ("Чернокнижник", "Демонология"), "demonology" },
        { ("Чернокнижник", "Разрушение"), "destruction" },
        { ("Воин", "Оружие"), "arms" },
        { ("Воин", "Неистовство"), "fury" },
        { ("Воин", "Защита"), "protection" }
    };

    public static string? GetIconFileName(string specName, string className)
    {
        if (string.IsNullOrWhiteSpace(specName) || string.IsNullOrWhiteSpace(className))
            return null;

        var classNameLower = className.Trim();
        var specNameTrimmed = specName.Trim();

        if (SpecNameMapping.TryGetValue((classNameLower, specNameTrimmed), out var mappedSpecName))
        {
            if (ClassNameMapping.TryGetValue(classNameLower, out var mappedClassName))
            {
                var fullKey = $"{mappedClassName}/{mappedSpecName}";
                if (SpecToIcon.TryGetValue(fullKey, out var iconPath))
                    return iconPath;
            }
        }

        if (ClassNameMapping.TryGetValue(classNameLower, out var mappedClass))
        {
            classNameLower = mappedClass;
        }
        else
        {
            classNameLower = classNameLower.ToLowerInvariant();
        }

        var specKey = specNameTrimmed.ToLowerInvariant();

        var fullKeyDirect = $"{classNameLower}/{specKey}";
        if (SpecToIcon.TryGetValue(fullKeyDirect, out var iconPathDirect))
            return iconPathDirect;

        foreach (var kvp in SpecToIcon)
        {
            if (kvp.Key.StartsWith($"{classNameLower}/", StringComparison.OrdinalIgnoreCase))
            {
                var iconSpecName = kvp.Key.Split('/').Last().Replace(".png", "");
                if (specKey.Contains(iconSpecName, StringComparison.OrdinalIgnoreCase) ||
                    iconSpecName.Contains(specKey, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }
        }

        return null;
    }
}

