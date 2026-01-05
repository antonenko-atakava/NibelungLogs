const classIdToName: Record<string, string> = {
  '1': 'warrior',
  '2': 'paladin',
  '3': 'hunter',
  '4': 'rogue',
  '5': 'priest',
  '6': 'deathknight',
  '7': 'shaman',
  '8': 'mage',
  '9': 'warlock',
  '11': 'druid',
};

const classNameToName: Record<string, string> = {
  'Воин': 'warrior',
  'Паладин': 'paladin',
  'Охотник': 'hunter',
  'Разбойник': 'rogue',
  'Жрец': 'priest',
  'Рыцарь смерти': 'deathknight',
  'Шаман': 'shaman',
  'Маг': 'mage',
  'Чернокнижник': 'warlock',
  'Друид': 'druid',
};

const specNameToFile: Record<string, string> = {
  'Оружие': 'arms',
  'Неистовство': 'fury',
  'Защита': 'protection',
  'Свет': 'holy',
  'Воздаяние': 'retribution',
  'Повелитель зверей': 'beastmastery',
  'Стрельба': 'marksman',
  'Выживание': 'survival',
  'Убийство': 'assassination',
  'Бой': 'combat',
  'Скрытность': 'subtlety',
  'Послушание': 'discipline',
  'Тьма': 'shadow',
  'Кровь': 'blood',
  'Лед': 'frost',
  'Нечестивость': 'unholy',
  'Стихии': 'elemental',
  'Улучшение': 'enhancement',
  'Исцеление': 'restoration',
  'Тайная магия': 'arcane',
  'Огонь': 'fire',
  'Колдовство': 'affliction',
  'Демонология': 'demonology',
  'Разрушение': 'destruction',
  'Баланс': 'balance',
  'Сила зверя': 'feral',
};

export function getClassIcon(characterClass: string, className?: string | null): string | null {
  const classKey = className ? classNameToName[className] : classIdToName[characterClass];
  if (!classKey) return null;
  return `/class/64/${classKey}.png`;
}

export function getSpecIcon(characterClass: string, specName?: string | null, className?: string | null): string | null {
  if (!specName) return null;
  
  const classKey = className ? classNameToName[className] : classIdToName[characterClass];
  if (!classKey) return null;
  
  const specFile = specNameToFile[specName];
  if (!specFile) return null;
  
  return `/spec/${classKey}/${specFile}.png`;
}

export function getClassColor(characterClass: string, className?: string | null): string {
  const classKey = className ? classNameToName[className] : classIdToName[characterClass];
  
  const classColors: Record<string, string> = {
    'warrior': '#C79C6E',
    'paladin': '#F58CBA',
    'hunter': '#ABD473',
    'rogue': '#FFF569',
    'priest': '#FFFFFF',
    'deathknight': '#C41F3B',
    'shaman': '#0070DE',
    'mage': '#69CCF0',
    'warlock': '#9482C9',
    'druid': '#FF7D0A',
  };
  
  return classColors[classKey || ''] || '#e5e5e5';
}

