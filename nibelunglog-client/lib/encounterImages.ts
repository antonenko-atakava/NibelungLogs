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
};

export function getEncounterImage(encounterName: string | null): string | null {
  if (!encounterName) return null;
  return encounterImageMap[encounterName] || null;
}

