-- Очистка базы данных от всех данных о рейдах
-- ВНИМАНИЕ: Этот скрипт удалит ВСЕ данные из таблиц!

-- Удаление данных в правильном порядке (с учетом внешних ключей)
TRUNCATE TABLE "PlayerEncounters" CASCADE;
TRUNCATE TABLE "Encounters" CASCADE;
TRUNCATE TABLE "Raids" CASCADE;
TRUNCATE TABLE "RaidTypes" CASCADE;
TRUNCATE TABLE "Players" CASCADE;
TRUNCATE TABLE "CharacterSpecs" CASCADE;
TRUNCATE TABLE "GuildMembers" CASCADE;
TRUNCATE TABLE "Guilds" CASCADE;

-- Сброс счетчиков автоинкремента (опционально, если нужно)
-- ALTER SEQUENCE "Players_Id_seq" RESTART WITH 1;
-- ALTER SEQUENCE "CharacterSpecs_Id_seq" RESTART WITH 1;
-- ALTER SEQUENCE "RaidTypes_Id_seq" RESTART WITH 1;
-- ALTER SEQUENCE "Raids_Id_seq" RESTART WITH 1;
-- ALTER SEQUENCE "Encounters_Id_seq" RESTART WITH 1;
-- ALTER SEQUENCE "PlayerEncounters_Id_seq" RESTART WITH 1;
-- ALTER SEQUENCE "Guilds_Id_seq" RESTART WITH 1;
-- ALTER SEQUENCE "GuildMembers_Id_seq" RESTART WITH 1;
