export const specIconMap: Record<string, string> = {
  "Воин/Оружие": "/images/wow/specs/warrior_arms.png",
  "Воин/Неистовство": "/images/wow/specs/warrior_fury.png",
  "Воин/Защита": "/images/wow/specs/warrior_prot.png",
  
  "Паладин/Свет": "/images/wow/specs/paladin_holy.png",
  "Паладин/Защита": "/images/wow/specs/paladin_protection.png",
  "Паладин/Воздаяние": "/images/wow/specs/paladin_ret.png",
  
  "Охотник/Повелитель зверей": "/images/wow/specs/hunter_bm.png",
  "Охотник/Стрельба": "/images/wow/specs/hunter_mm.png",
  "Охотник/Выживание": "/images/wow/specs/hunter_survival.png",
  
  "Разбойник/Убийство": "/images/wow/specs/rogue_assa.png",
  "Разбойник/Бой": "/images/wow/specs/rogue_outlaw.png",
  "Разбойник/Скрытность": "/images/wow/specs/rogue_sub.png",
  
  "Жрец/Послушание": "/images/wow/specs/priest_disc.png",
  "Жрец/Свет": "/images/wow/specs/priest_holy.png",
  "Жрец/Тьма": "/images/wow/specs/priest_shadow.png",
  
  "Рыцарь смерти/Кровь": "/images/wow/specs/dk_blood.png",
  "Рыцарь смерти/Лед": "/images/wow/specs/dk_frost.png",
  "Рыцарь смерти/Нечестивость": "/images/wow/specs/dk_unholy.png",
  
  "Шаман/Стихии": "/images/wow/specs/shaman_elem.png",
  "Шаман/Улучшение": "/images/wow/specs/shaman_enhancement.png",
  "Шаман/Исцеление": "/images/wow/specs/shaman_resto.png",
  
  "Маг/Тайная магия": "/images/wow/specs/mage_arcane.png",
  "Маг/Огонь": "/images/wow/specs/mage_fire.png",
  "Маг/Лед": "/images/wow/specs/mage_frost.png",
  
  "Чернокнижник/Колдовство": "/images/wow/specs/warlock_affli.png",
  "Чернокнижник/Демонология": "/images/wow/specs/warlock_demono.png",
  "Чернокнижник/Разрушение": "/images/wow/specs/warlock_destru.png",
  
  "Друид/Баланс": "/images/wow/specs/druid_balance.png",
  "Друид/Сила зверя": "/images/wow/specs/druid_feral.png",
  "Друид/Исцеление": "/images/wow/specs/druid_resto.png",
};

export const classIconMap: Record<string, string> = {
  "Воин": "/images/wow/specs/warrior_arms.png",
  "Паладин": "/images/wow/specs/paladin_holy.png",
  "Охотник": "/images/wow/specs/hunter_bm.png",
  "Разбойник": "/images/wow/specs/rogue_assa.png",
  "Жрец": "/images/wow/specs/priest_holy.png",
  "Рыцарь смерти": "/images/wow/specs/dk_blood.png",
  "Шаман": "/images/wow/specs/shaman_elem.png",
  "Маг": "/images/wow/specs/mage_arcane.png",
  "Чернокнижник": "/images/wow/specs/warlock_affli.png",
  "Друид": "/images/wow/specs/druid_balance.png",
};

export const specIconMapByClassAndSpec: Record<string, Record<string, string>> = {
  "Воин": {
    "Оружие": "/images/wow/specs/warrior_arms.png",
    "Неистовство": "/images/wow/specs/warrior_fury.png",
    "Защита": "/images/wow/specs/warrior_prot.png",
  },
  "Паладин": {
    "Свет": "/images/wow/specs/paladin_holy.png",
    "Защита": "/images/wow/specs/paladin_protection.png",
    "Воздаяние": "/images/wow/specs/paladin_ret.png",
  },
  "Охотник": {
    "Повелитель зверей": "/images/wow/specs/hunter_bm.png",
    "Стрельба": "/images/wow/specs/hunter_mm.png",
    "Выживание": "/images/wow/specs/hunter_survival.png",
  },
  "Разбойник": {
    "Убийство": "/images/wow/specs/rogue_assa.png",
    "Бой": "/images/wow/specs/rogue_outlaw.png",
    "Скрытность": "/images/wow/specs/rogue_sub.png",
  },
  "Жрец": {
    "Послушание": "/images/wow/specs/priest_disc.png",
    "Свет": "/images/wow/specs/priest_holy.png",
    "Тьма": "/images/wow/specs/priest_shadow.png",
  },
  "Рыцарь смерти": {
    "Кровь": "/images/wow/specs/dk_blood.png",
    "Лед": "/images/wow/specs/dk_frost.png",
    "Нечестивость": "/images/wow/specs/dk_unholy.png",
  },
  "Шаман": {
    "Стихии": "/images/wow/specs/shaman_elem.png",
    "Улучшение": "/images/wow/specs/shaman_enhancement.png",
    "Исцеление": "/images/wow/specs/shaman_resto.png",
  },
  "Маг": {
    "Тайная магия": "/images/wow/specs/mage_arcane.png",
    "Огонь": "/images/wow/specs/mage_fire.png",
    "Лед": "/images/wow/specs/mage_frost.png",
  },
  "Чернокнижник": {
    "Колдовство": "/images/wow/specs/warlock_affli.png",
    "Демонология": "/images/wow/specs/warlock_demono.png",
    "Разрушение": "/images/wow/specs/warlock_destru.png",
  },
  "Друид": {
    "Баланс": "/images/wow/specs/druid_balance.png",
    "Сила зверя": "/images/wow/specs/druid_feral.png",
    "Исцеление": "/images/wow/specs/druid_resto.png",
  },
};

export function getSpecIcon(className: string | null | undefined, specName: string | null | undefined): string | null {
  if (!className || !specName)
    return null;
  
  const classSpecs = specIconMapByClassAndSpec[className];
  if (!classSpecs)
    return null;
  
  return classSpecs[specName] || specIconMap[`${className}/${specName}`] || null;
}

export function getClassIcon(className: string | null | undefined): string | null {
  if (!className)
    return null;
  
  return classIconMap[className] || null;
}
