-- Принудительное исправление имени для Игниса (33114)
-- Обновляет ВСЕ записи с EncounterEntry = '33114' на правильное имя
UPDATE "Encounters"
SET "EncounterName" = 'Повелитель Горнов Игнис'
WHERE "EncounterEntry" = '33114';

-- Проверка результата
SELECT 
    "EncounterEntry",
    "EncounterName",
    COUNT(*) as count
FROM "Encounters"
WHERE "EncounterEntry" IN ('33114', '33118', '32930')
GROUP BY "EncounterEntry", "EncounterName"
ORDER BY "EncounterEntry", "EncounterName";
