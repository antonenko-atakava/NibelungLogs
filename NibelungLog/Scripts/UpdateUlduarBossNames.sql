-- Проверка текущих имен боссов Ульдуара
SELECT "EncounterEntry", "EncounterName", COUNT(*) as count
FROM "Encounters"
WHERE "EncounterEntry" IN ('33113', '33114', '33115', '33116', '33117', '33118', '33119', '33120', '33121', '33122', '33123', '33124', '33125', '33126')
GROUP BY "EncounterEntry", "EncounterName"
ORDER BY "EncounterEntry";

-- Обновление имен боссов Ульдуара
UPDATE "Encounters"
SET "EncounterName" = CASE "EncounterEntry"
    WHEN '33113' THEN 'Огненный Левиафан'
    WHEN '33114' THEN 'Повелитель Горнов Игнис'
    WHEN '33115' THEN 'Острокрылая'
    WHEN '33116' THEN 'Разрушитель XT-002'
    WHEN '33117' THEN 'Железное собрание'
    WHEN '33118' THEN 'Кологарн'
    WHEN '33119' THEN 'Ауриайя'
    WHEN '33120' THEN 'Мимирон'
    WHEN '33121' THEN 'Фрейя'
    WHEN '33122' THEN 'Торим'
    WHEN '33123' THEN 'Ходир'
    WHEN '33124' THEN 'Генерал Везакс'
    WHEN '33125' THEN 'Йогг-Сарон'
    WHEN '33126' THEN 'Алгалон Наблюдатель'
    ELSE "EncounterName"
END
WHERE "EncounterEntry" IN ('33113', '33114', '33115', '33116', '33117', '33118', '33119', '33120', '33121', '33122', '33123', '33124', '33125', '33126');

-- Проверка обновленных имен
SELECT "EncounterEntry", "EncounterName", COUNT(*) as count
FROM "Encounters"
WHERE "EncounterEntry" IN ('33113', '33114', '33115', '33116', '33117', '33118', '33119', '33120', '33121', '33122', '33123', '33124', '33125', '33126')
GROUP BY "EncounterEntry", "EncounterName"
ORDER BY "EncounterEntry";
