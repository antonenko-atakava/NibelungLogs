export const raceIdMap: Record<string, string> = {
  "1": "Человек",
  "2": "Орк",
  "3": "Дворф",
  "4": "Ночной эльф",
  "5": "Нежить",
  "6": "Таурен",
  "7": "Гном",
  "8": "Тролль",
  "10": "Кровный эльф",
  "11": "Дреней",
};

export function getRaceName(raceId: string | null | undefined): string | null {
  if (!raceId)
    return null;
  return raceIdMap[raceId] || null;
}

export const raceNameToIdMap: Record<string, string> = {
  "Человек": "1",
  "Орк": "2",
  "Дворф": "3",
  "Ночной эльф": "4",
  "Нежить": "5",
  "Таурен": "6",
  "Гном": "7",
  "Тролль": "8",
  "Кровный эльф": "10",
  "Дреней": "11",
};

export const raceList = [
  "Человек",
  "Орк",
  "Дворф",
  "Ночной эльф",
  "Нежить",
  "Таурен",
  "Гном",
  "Тролль",
  "Кровный эльф",
  "Дреней",
];

export const factionByRaceId: Record<string, "Альянс" | "Орда"> = {
  "1": "Альянс",
  "2": "Орда",
  "3": "Альянс",
  "4": "Альянс",
  "5": "Орда",
  "6": "Орда",
  "7": "Альянс",
  "8": "Орда",
  "10": "Орда",
  "11": "Альянс",
};

export const factionByRaceName: Record<string, "Альянс" | "Орда"> = {
  "Человек": "Альянс",
  "Орк": "Орда",
  "Дворф": "Альянс",
  "Ночной эльф": "Альянс",
  "Нежить": "Орда",
  "Таурен": "Орда",
  "Гном": "Альянс",
  "Тролль": "Орда",
  "Кровный эльф": "Орда",
  "Дреней": "Альянс",
};

export function getRaceId(raceName: string | null | undefined): string | null {
  if (!raceName)
    return null;
  return raceNameToIdMap[raceName] || null;
}

export function getFactionByRaceId(raceId: string): "Альянс" | "Орда" | null {
  return factionByRaceId[raceId] || null;
}

export function getFactionByRaceName(raceName: string): "Альянс" | "Орда" | null {
  return factionByRaceName[raceName] || null;
}
