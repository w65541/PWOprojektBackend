-- ============================================================
-- Seed script for MySQL (Pomelo EF Core)
-- Run after: dotnet ef database update
-- ============================================================

USE `Database`;

-- ----------------------------------------------------------------
-- 1. User Types
-- ----------------------------------------------------------------
INSERT IGNORE INTO `UserTypes` (`Id`, `Name`) VALUES
(1, 'Admin'),
(2, 'User'),
(3, 'Moderator');

-- ----------------------------------------------------------------
-- 2. Users  (Haslo stored as plaintext – dev only!)
-- ----------------------------------------------------------------
INSERT INTO `Users` 
    (`Login`, `Haslo`, `Email`, `TypeId`, `IsActive`, `CreationDate`, `LastLoginDate`, `ArchiveDate`, `ArchiverId`)
VALUES
('admin',      'admin123',   'admin@example.com',       1, 1, '2024-01-01 08:00:00', '2025-06-01 10:00:00', NULL, NULL),
('jan.kowal',  'pass123',    'jan.kowal@example.com',   2, 1, '2024-02-15 09:30:00', '2025-05-28 14:22:00', NULL, NULL),
('anna.nowak', 'pass123',    'anna.nowak@example.com',  2, 1, '2024-03-10 11:00:00', '2025-06-10 09:15:00', NULL, NULL),
('piotr.wis',  'pass123',    'piotr.wis@example.com',   3, 1, '2024-04-05 13:45:00', '2025-06-05 16:00:00', NULL, NULL),
('mod1',       'mod123',     'mod1@example.com',        3, 1, '2024-05-20 10:00:00', '2025-06-08 11:30:00', NULL, NULL),
('user.test1', 'test123',    'test1@example.com',       2, 1, '2024-06-01 08:00:00', '2025-06-12 08:45:00', NULL, NULL),
('user.test2', 'test123',    'test2@example.com',       2, 1, '2024-06-15 08:00:00', '2025-05-30 17:00:00', NULL, NULL),
('user.test3', 'test123',    'test3@example.com',       2, 1, '2024-07-01 08:00:00', NULL,                  NULL, NULL),
('archived1',  'arch123',    'arch1@example.com',       2, 0, '2024-01-10 08:00:00', '2024-12-01 10:00:00', '2025-01-15 12:00:00', 1),
('archived2',  'arch123',    'arch2@example.com',       2, 0, '2024-02-20 08:00:00', '2024-11-20 09:00:00', '2025-02-01 14:00:00', 1);

-- ----------------------------------------------------------------
-- 3. Login Logs  (use subquery to get IDs safely)
-- ----------------------------------------------------------------
INSERT INTO `LoginLogs` (`UserId`, `LoginDate`)
SELECT Id, '2025-01-10 08:00:00' FROM `Users` WHERE Login = 'admin'
UNION ALL SELECT Id, '2025-01-15 09:30:00' FROM `Users` WHERE Login = 'admin'
UNION ALL SELECT Id, '2025-02-01 10:00:00' FROM `Users` WHERE Login = 'admin'
UNION ALL SELECT Id, '2025-03-05 11:15:00' FROM `Users` WHERE Login = 'admin'
UNION ALL SELECT Id, '2025-04-10 08:45:00' FROM `Users` WHERE Login = 'admin'
UNION ALL SELECT Id, '2025-05-20 14:00:00' FROM `Users` WHERE Login = 'admin'
UNION ALL SELECT Id, '2025-06-01 10:00:00' FROM `Users` WHERE Login = 'admin'
UNION ALL SELECT Id, '2025-02-10 09:00:00' FROM `Users` WHERE Login = 'jan.kowal'
UNION ALL SELECT Id, '2025-03-15 10:30:00' FROM `Users` WHERE Login = 'jan.kowal'
UNION ALL SELECT Id, '2025-04-20 11:00:00' FROM `Users` WHERE Login = 'jan.kowal'
UNION ALL SELECT Id, '2025-05-28 14:22:00' FROM `Users` WHERE Login = 'jan.kowal'
UNION ALL SELECT Id, '2025-03-01 08:30:00' FROM `Users` WHERE Login = 'anna.nowak'
UNION ALL SELECT Id, '2025-04-05 09:45:00' FROM `Users` WHERE Login = 'anna.nowak'
UNION ALL SELECT Id, '2025-05-10 13:00:00' FROM `Users` WHERE Login = 'anna.nowak'
UNION ALL SELECT Id, '2025-06-10 09:15:00' FROM `Users` WHERE Login = 'anna.nowak'
UNION ALL SELECT Id, '2025-04-15 10:00:00' FROM `Users` WHERE Login = 'piotr.wis'
UNION ALL SELECT Id, '2025-05-25 15:30:00' FROM `Users` WHERE Login = 'piotr.wis'
UNION ALL SELECT Id, '2025-06-05 16:00:00' FROM `Users` WHERE Login = 'piotr.wis'
UNION ALL SELECT Id, '2025-05-01 08:00:00' FROM `Users` WHERE Login = 'mod1'
UNION ALL SELECT Id, '2025-06-08 11:30:00' FROM `Users` WHERE Login = 'mod1'
UNION ALL SELECT Id, '2025-05-15 09:00:00' FROM `Users` WHERE Login = 'user.test1'
UNION ALL SELECT Id, '2025-06-12 08:45:00' FROM `Users` WHERE Login = 'user.test1'
UNION ALL SELECT Id, '2025-05-30 17:00:00' FROM `Users` WHERE Login = 'user.test2';

-- ----------------------------------------------------------------
-- 4. Notifications
-- ----------------------------------------------------------------
INSERT INTO `Notifications` (`UserId`, `Created`, `Read`, `Message`)
SELECT Id, '2025-06-01 10:05:00', 1, 'Witaj w systemie! Twoje konto zostało aktywowane.' 
FROM `Users` WHERE Login = 'admin'
UNION ALL
SELECT Id, '2025-06-10 14:30:00', 0, 'Twoje dane są gotowe do pobrania: users_export_20250610.csv. Kliknij aby pobrać: https://localhost:7184/DataExport/download/users_export_20250610.csv'
FROM `Users` WHERE Login = 'admin'
UNION ALL
SELECT Id, '2025-05-28 14:25:00', 1, 'Witaj w systemie! Twoje konto zostało aktywowane.'
FROM `Users` WHERE Login = 'jan.kowal'
UNION ALL
SELECT Id, '2025-06-01 09:00:00', 0, 'Masz nową wiadomość od administratora.'
FROM `Users` WHERE Login = 'jan.kowal'
UNION ALL
SELECT Id, '2025-06-10 09:20:00', 0, 'Twoje dane są gotowe do pobrania: logs_export_20250610.csv. Kliknij aby pobrać: https://localhost:7184/DataExport/download/logs_export_20250610.csv'
FROM `Users` WHERE Login = 'anna.nowak'
UNION ALL
SELECT Id, '2025-06-05 16:05:00', 0, 'Przypomnienie: zaktualizuj swoje hasło.'
FROM `Users` WHERE Login = 'piotr.wis';

-- ----------------------------------------------------------------
-- 5. User Modification Logs
-- ----------------------------------------------------------------
INSERT INTO `UserModificationLogs` (`UserId`, `UpdateDate`, `modColumn`, `oldValue`, `newValue`)
SELECT Id, '2025-03-01 10:00:00', 'Email', 'old_jan@example.com', 'jan.kowal@example.com'
FROM `Users` WHERE Login = 'jan.kowal'
UNION ALL
SELECT Id, '2025-04-15 11:30:00', 'TypeId', '2', '3'
FROM `Users` WHERE Login = 'piotr.wis';

-- ----------------------------------------------------------------
-- Verify counts
-- ----------------------------------------------------------------
SELECT 'UserTypes'          AS tbl, COUNT(*) AS cnt FROM `UserTypes`
UNION ALL SELECT 'Users',           COUNT(*) FROM `Users`
UNION ALL SELECT 'LoginLogs',       COUNT(*) FROM `LoginLogs`
UNION ALL SELECT 'Notifications',   COUNT(*) FROM `Notifications`
UNION ALL SELECT 'UserModLogs',     COUNT(*) FROM `UserModificationLogs`;
