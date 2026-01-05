namespace NibelungLog.Types;

public static class ClassMappings
{
    public static readonly Dictionary<string, string> ClassNames = new()
    {
        ["1"] = "Воин",
        ["2"] = "Паладин",
        ["3"] = "Охотник",
        ["4"] = "Разбойник",
        ["5"] = "Жрец",
        ["6"] = "Рыцарь смерти",
        ["7"] = "Шаман",
        ["8"] = "Маг",
        ["9"] = "Чернокнижник",
        ["11"] = "Друид"
    };

    public static readonly Dictionary<(string Class, string Spec), string> SpecNames = new()
    {
        [("1", "0")] = "Оружие",
        [("1", "1")] = "Неистовство",
        [("1", "2")] = "Защита",
        
        [("2", "0")] = "Свет",
        [("2", "1")] = "Защита",
        [("2", "2")] = "Воздаяние",
        
        [("3", "0")] = "Повелитель зверей",
        [("3", "1")] = "Стрельба",
        [("3", "2")] = "Выживание",
        
        [("4", "0")] = "Убийство",
        [("4", "1")] = "Бой",
        [("4", "2")] = "Скрытность",
        
        [("5", "0")] = "Послушание",
        [("5", "1")] = "Свет",
        [("5", "2")] = "Тьма",
        
        [("6", "0")] = "Кровь",
        [("6", "1")] = "Лед",
        [("6", "2")] = "Нечестивость",
        
        [("7", "0")] = "Стихии",
        [("7", "1")] = "Улучшение",
        [("7", "2")] = "Исцеление",
        
        [("8", "0")] = "Тайная магия",
        [("8", "1")] = "Огонь",
        [("8", "2")] = "Лед",
        
        [("9", "0")] = "Колдовство",
        [("9", "1")] = "Демонология",
        [("9", "2")] = "Разрушение",
        
        [("11", "0")] = "Баланс",
        [("11", "1")] = "Сила зверя",
        [("11", "2")] = "Исцеление"
    };

    public static string GetClassName(string classId)
    {
        return ClassNames.TryGetValue(classId, out var name) ? name : classId;
    }

    public static string GetSpecName(string classId, string specId)
    {
        return SpecNames.TryGetValue((classId, specId), out var name) ? name : specId;
    }
}

