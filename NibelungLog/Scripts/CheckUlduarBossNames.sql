-- Проверка имен боссов Ульдуара в базе данных
SELECT 
    "EncounterEntry",
    "EncounterName",
    COUNT(*) as count
FROM "Encounters"
WHERE "EncounterEntry" IN ('33113', '33114', '33115', '33116', '33117', '33118', '33119', '33120', '33121', '33122', '33123', '33124', '33125', '33126', '32845', '32857', '32865', '32906', '32930', '33186', '33271', '33288', '33293', '33350', '33515')
GROUP BY "EncounterEntry", "EncounterName"
ORDER BY "EncounterEntry", "EncounterName";

-- Проверка конкретно Игниса
SELECT 
    "EncounterEntry",
    "EncounterName",
    COUNT(*) as count
FROM "Encounters"
WHERE "EncounterEntry" = '33114'
GROUP BY "EncounterEntry", "EncounterName";

-- Проверка Кологарна
SELECT 
    "EncounterEntry",
    "EncounterName",
    COUNT(*) as count
FROM "Encounters"
WHERE "EncounterEntry" IN ('33118', '32930')
GROUP BY "EncounterEntry", "EncounterName";
