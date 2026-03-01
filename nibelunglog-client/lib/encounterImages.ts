export const encounterImageMap: Record<string, string> = {
  "Ануб'Рекан": "/raids/naxxramas/анубрекан.png",
  "Барон Ривендер": "/raids/naxxramas/всадники.png",
  "Великая вдова Фарлина": "/raids/naxxramas/фарлина.png",
  "Глут": "/raids/naxxramas/глут.png",
  "Готик Жнец": "/raids/naxxramas/готик жнец.png",
  "Гроббулус": "/raids/naxxramas/гробулус.png",
  "Инструктор Разувий": "/raids/naxxramas/разувий.png",
  "Кел'Тузад": "/raids/naxxramas/культузад.png",
  "Леди Бломе": "/raids/naxxramas/всадники.png",
  "Лоскутик": "/raids/naxxramas/лоскутик.png",
  "Лотхиб": "/raids/naxxramas/лотхиб.png",
  "Мексна": "/raids/naxxramas/мексна.png",
  "Нот Чумной": "/raids/naxxramas/нот чумной.png",
  "Сапфирон": "/raids/naxxramas/сапфирон.png",
  "Сэр Зелиек": "/raids/naxxramas/всадники.png",
  "Таддиус": "/raids/naxxramas/тадиус.png",
  "Тан Кортазз": "/raids/naxxramas/всадники.png",
  "Хейган Нечестивый": "/raids/naxxramas/хейган.png",
  "Малигос": "/raids/eye-of-eternity/малигос.png",
  "Сартарион": "/raids/obsidian-sanctum/сартарион.png",
  "Огненный Левиафан": "/raids/ulduar/огненный левиафан.png",
  "Повелитель Горнов Игнис": "/raids/ulduar/игнис.png",
  "Острокрылая": "/raids/ulduar/острокрылая.png",
  "Разрушитель XT-002": "/raids/ulduar/разрушитель.png",
  "Железное собрание": "/raids/ulduar/собрание.png",
  "Кологарн": "/raids/ulduar/кологарн.png",
  "Ауриайя": "/raids/ulduar/ауриайя.png",
  "Мимирон": "/raids/ulduar/мимирон.png",
  "Фрейя": "/raids/ulduar/фрейя.png",
  "Торим": "/raids/ulduar/торим.png",
  "Ходир": "/raids/ulduar/ходир.png",
  "Генерал Везакс": "/raids/ulduar/везакс.png",
  "Йогг-Сарон": "/raids/ulduar/йогг-сарон.png",
  "Алгалон Наблюдатель": "/raids/ulduar/алгалон.png",
};

export function getEncounterImage(encounterName: string | null): string | null {
  if (!encounterName) return null;
  return encounterImageMap[encounterName] || null;
}

