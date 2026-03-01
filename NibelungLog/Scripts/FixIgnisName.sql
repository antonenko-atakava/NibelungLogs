-- Исправление имени для Игниса (33114) - должно быть "Повелитель Горнов Игнис", а не "Кологарн"
UPDATE "Encounters"
SET "EncounterName" = 'Повелитель Горнов Игнис'
WHERE "EncounterEntry" = '33114'
AND EXISTS (
    SELECT 1 FROM "Raids" r
    INNER JOIN "RaidTypes" rt ON r."RaidTypeId" = rt."Id"
    WHERE r."Id" = "Encounters"."RaidId"
    AND rt."Map" = '603'
);

-- Проверка результата
SELECT 
    "EncounterEntry",
    "EncounterName",
    COUNT(*) as count
FROM "Encounters"
WHERE "EncounterEntry" IN ('33114', '33118', '32930')
AND EXISTS (
    SELECT 1 FROM "Raids" r
    INNER JOIN "RaidTypes" rt ON r."RaidTypeId" = rt."Id"
    WHERE r."Id" = "Encounters"."RaidId"
    AND rt."Map" = '603'
)
GROUP BY "EncounterEntry", "EncounterName"
ORDER BY "EncounterEntry", "EncounterName";
