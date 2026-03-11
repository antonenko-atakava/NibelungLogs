const ulduarEncounters: Record<string, string> = {
  "33113": "Огненный Левиафан",
  "33118": "Повелитель Горнов Игнис",
  "33115": "Острокрылая",
  "33186": "Острокрылая",
  "33116": "Разрушитель XT-002",
  "33293": "Разрушитель XT-002",
  "33117": "Железное собрание",
  "32857": "Железное собрание",
  "32930": "Кологарн",
  "33119": "Ауриайя",
  "33515": "Ауриайя",
  "33120": "Мимирон",
  "33350": "Мимирон",
  "33121": "Фрейя",
  "32906": "Фрейя",
  "33122": "Торим",
  "32865": "Торим",
  "33123": "Ходир",
  "32845": "Ходир",
  "33124": "Генерал Везакс",
  "33271": "Генерал Везакс",
  "33125": "Йогг-Сарон",
  "33288": "Йогг-Сарон",
  "33126": "Алгалон Наблюдатель",
  "32871": "Алгалон Наблюдатель"
};

const naxxramasEncounters: Record<string, string> = {
  "15956": "Ануб'Рекан",
  "30549": "Барон Ривендер",
  "15953": "Великая вдова Фарлина",
  "15932": "Глут",
  "16060": "Готик Жнец",
  "15931": "Гроббулус",
  "16061": "Инструктор Разувий",
  "15990": "Кел'Тузад",
  "16065": "Леди Бломе",
  "16028": "Лоскутик",
  "16011": "Лотхиб",
  "15952": "Мексна",
  "15954": "Нот Чумной",
  "15989": "Сапфирон",
  "16063": "Сэр Зелиек",
  "15928": "Таддиус",
  "16064": "Тан Кортазз",
  "15936": "Хейган Нечестивый"
};

const obsidianSanctumEncounters: Record<string, string> = {
  "28860": "Сартарион"
};

const eyeOfEternityEncounters: Record<string, string> = {
  "28859": "Малигос"
};

const allEncounters: Record<string, string> = {
  ...ulduarEncounters,
  ...naxxramasEncounters,
  ...obsidianSanctumEncounters,
  ...eyeOfEternityEncounters
};

export function getEncounterName(encounterEntry: string | null | undefined): string {
  if (!encounterEntry)
    return "Неизвестный энкаунтер";
  
  return allEncounters[encounterEntry] || encounterEntry;
}

export const encounterMappings = allEncounters;
