/*
Remove all "dangling" user-activity rows in TechExchangeNew — rows whose referenced
user no longer exists (only the 43 IsAdmin=1 users remain after the prior cleanups).
Requested 2026-07-16 ("dọn dangling activity" → "Tất cả dangling"); user has a full DB backup.

Scope: delete rows referencing a non-existent user from every user-activity table
(~1.18M rows). Keeps only rows belonging to the 43 remaining admin users.

Dependency: AiChatMessages.SessionId -> AiChatSessions (delete messages of dangling
sessions first). No other child FKs on the affected tables.

SAFETY: each table's deleted rows are copied to a *_DanglingBackup_20260716 table first,
inside one transaction (in addition to the user's full DB .bak). FK child rows deleted
before parents. Reversible.
*/

SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO
USE TechExchangeNew;
GO

IF OBJECT_ID('dbo.Rating_DanglingBackup_20260716') IS NOT NULL
BEGIN
    RAISERROR('Backup tables *_DanglingBackup_20260716 already exist — aborting to avoid clobbering.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

-- Backups (dangling rows only) ----------------------------------------------------------
SELECT t.* INTO dbo.Rating_DanglingBackup_20260716          FROM Rating t          WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);
SELECT t.* INTO dbo.PhieuYeuCauCNTB_DanglingBackup_20260716 FROM PhieuYeuCauCNTB t WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);
SELECT t.* INTO dbo.Comments_DanglingBackup_20260716        FROM Comments t        WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.MemberId);
SELECT t.* INTO dbo.ShoppingCart_DanglingBackup_20260716    FROM ShoppingCart t    WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);
SELECT t.* INTO dbo.CommentsYCTB_DanglingBackup_20260716    FROM CommentsYCTB t    WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.MemberId);
SELECT t.* INTO dbo.CommentsYeuCau_DanglingBackup_20260716  FROM CommentsYeuCau t  WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.MemberId);
SELECT t.* INTO dbo.Likepage_DanglingBackup_20260716        FROM Likepage t        WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = TRY_CAST(t.UserId AS int));
SELECT t.* INTO dbo.ForumYCTB_DanglingBackup_20260716       FROM ForumYCTB t       WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);
SELECT t.* INTO dbo.TimKiemDoiTac_DanglingBackup_20260716   FROM TimKiemDoiTac t   WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);
SELECT t.* INTO dbo.AiChatSessions_DanglingBackup_20260716  FROM AiChatSessions t  WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);
SELECT m.* INTO dbo.AiChatMessages_DanglingBackup_20260716  FROM AiChatMessages m  WHERE EXISTS (SELECT 1 FROM AiChatSessions s WHERE s.Id = m.SessionId AND NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = s.UserId));
SELECT t.* INTO dbo.UserRole_DanglingBackup_20260716        FROM UserRole t        WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);
SELECT t.* INTO dbo.UserLinhVuc_DanglingBackup_20260716     FROM UserLinhVuc t     WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);

-- Delete child rows first ---------------------------------------------------------------
DELETE m FROM AiChatMessages m
WHERE EXISTS (SELECT 1 FROM AiChatSessions s WHERE s.Id = m.SessionId
              AND NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = s.UserId));

-- Delete dangling activity --------------------------------------------------------------
DELETE t FROM Rating t          WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);
DELETE t FROM PhieuYeuCauCNTB t WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);
DELETE t FROM Comments t        WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.MemberId);
DELETE t FROM ShoppingCart t    WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);
DELETE t FROM CommentsYCTB t    WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.MemberId);
DELETE t FROM CommentsYeuCau t  WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.MemberId);
DELETE t FROM Likepage t        WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = TRY_CAST(t.UserId AS int));
DELETE t FROM ForumYCTB t       WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);
DELETE t FROM TimKiemDoiTac t   WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);
DELETE t FROM AiChatSessions t  WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);
DELETE t FROM UserRole t        WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);
DELETE t FROM UserLinhVuc t     WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = t.UserId);

SELECT
  (SELECT COUNT(*) FROM Rating)          AS RatingLeft,
  (SELECT COUNT(*) FROM PhieuYeuCauCNTB) AS PYCLeft,
  (SELECT COUNT(*) FROM Comments)        AS CommentsLeft,
  (SELECT COUNT(*) FROM ShoppingCart)    AS CartLeft,
  (SELECT COUNT(*) FROM UserRole)        AS UserRoleLeft,
  (SELECT COUNT(*) FROM AiChatSessions)  AS AiSessLeft;

COMMIT TRANSACTION;
GO
