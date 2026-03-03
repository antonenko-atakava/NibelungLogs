export const classIdMap: Record<string, string> = {
  "Воин": "1",
  "Паладин": "2",
  "Охотник": "3",
  "Разбойник": "4",
  "Жрец": "5",
  "Рыцарь смерти": "6",
  "Шаман": "7",
  "Маг": "8",
  "Чернокнижник": "9",
  "Друид": "11",
};

export const specIdMap: Record<string, Record<string, string>> = {
  "Воин": {
    "Оружие": "0",
    "Неистовство": "1",
    "Защита": "2",
  },
  "Паладин": {
    "Свет": "0",
    "Защита": "1",
    "Воздаяние": "2",
  },
  "Охотник": {
    "Повелитель зверей": "0",
    "Стрельба": "1",
    "Выживание": "2",
  },
  "Разбойник": {
    "Убийство": "0",
    "Бой": "1",
    "Скрытность": "2",
  },
  "Жрец": {
    "Послушание": "0",
    "Свет": "1",
    "Тьма": "2",
  },
  "Рыцарь смерти": {
    "Кровь": "0",
    "Лед": "1",
    "Нечестивость": "2",
  },
  "Шаман": {
    "Стихии": "0",
    "Улучшение": "1",
    "Исцеление": "2",
  },
  "Маг": {
    "Тайная магия": "0",
    "Огонь": "1",
    "Лед": "2",
  },
  "Чернокнижник": {
    "Колдовство": "0",
    "Демонология": "1",
    "Разрушение": "2",
  },
  "Друид": {
    "Баланс": "0",
    "Сила зверя": "1",
    "Исцеление": "2",
  },
};

export function getClassId(className: string | null | undefined): string | null {
  if (!className)
    return null;
  
  return classIdMap[className] || null;
}

export function getSpecId(className: string | null | undefined, specName: string | null | undefined): string | null {
  if (!className || !specName)
    return null;
  
  const classSpecs = specIdMap[className];
  if (!classSpecs)
    return null;
  
  return classSpecs[specName] || null;
}
