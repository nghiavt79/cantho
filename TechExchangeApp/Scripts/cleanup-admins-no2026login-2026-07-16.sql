/*
Delete admin users who did NOT log in during 2026 (requested 2026-07-16).
Target: IsAdmin=1 AND (LastLogin < '2026-01-01' OR LastLogin IS NULL) = 33 users
(the 10 admins with a 2026 login are kept). These are old admin/company accounts
(last login 2017–2025).

They created no Product/NhaCungUng/NhaTuVan (verified 0); only FK dependency is
UsersMenu (99). Their account artifacts (UserRole 58, UserLinhVuc 215) and residual
activity (ShoppingCart 67, Comments 26, Rating 7) are removed too so no new dangling
rows are left behind (consistent with the dangling-activity purge just done).

SAFETY: all deleted rows copied to *_Admin2026Cleanup_20260716 backups first, in one
transaction, children before parents. Reversible (plus the user's full DB .bak).
*/

SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO
USE TechExchangeNew;
GO

IF OBJECT_ID('dbo.Users_Admin2026Cleanup_20260716') IS NOT NULL
BEGIN
    RAISERROR('Backup Users_Admin2026Cleanup_20260716 already exists — aborting.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

CREATE TABLE #d (UserId int primary key);
INSERT #d SELECT UserId FROM Users WHERE IsAdmin=1 AND (LastLogin < '2026-01-01' OR LastLogin IS NULL);

-- Backups -------------------------------------------------------------------------------
SELECT x.* INTO dbo.Users_Admin2026Cleanup_20260716        FROM Users x        WHERE x.UserId   IN (SELECT UserId FROM #d);
SELECT x.* INTO dbo.UserRole_Admin2026Cleanup_20260716     FROM UserRole x     WHERE x.UserId   IN (SELECT UserId FROM #d);
SELECT x.* INTO dbo.UsersMenu_Admin2026Cleanup_20260716    FROM UsersMenu x    WHERE x.UserId   IN (SELECT UserId FROM #d);
SELECT x.* INTO dbo.UserLinhVuc_Admin2026Cleanup_20260716  FROM UserLinhVuc x  WHERE x.UserId   IN (SELECT UserId FROM #d);
SELECT x.* INTO dbo.Rating_Admin2026Cleanup_20260716       FROM Rating x       WHERE x.UserId   IN (SELECT UserId FROM #d);
SELECT x.* INTO dbo.Comments_Admin2026Cleanup_20260716     FROM Comments x     WHERE x.MemberId IN (SELECT UserId FROM #d);
SELECT x.* INTO dbo.ShoppingCart_Admin2026Cleanup_20260716 FROM ShoppingCart x WHERE x.UserId   IN (SELECT UserId FROM #d);

-- Delete children -----------------------------------------------------------------------
DELETE FROM UsersMenu    WHERE UserId   IN (SELECT UserId FROM #d);
DELETE FROM UserRole     WHERE UserId   IN (SELECT UserId FROM #d);
DELETE FROM UserLinhVuc  WHERE UserId   IN (SELECT UserId FROM #d);
DELETE FROM Rating       WHERE UserId   IN (SELECT UserId FROM #d);
DELETE FROM Comments     WHERE MemberId IN (SELECT UserId FROM #d);
DELETE FROM ShoppingCart WHERE UserId   IN (SELECT UserId FROM #d);

-- Delete users --------------------------------------------------------------------------
DELETE FROM Users WHERE UserId IN (SELECT UserId FROM #d);
DECLARE @del INT = @@ROWCOUNT;

SELECT
  @del AS adminsDeleted,
  (SELECT COUNT(*) FROM dbo.Users_Admin2026Cleanup_20260716) AS backedUp,
  (SELECT COUNT(*) FROM Users) AS usersRemaining,
  (SELECT COUNT(*) FROM Users WHERE IsAdmin=1) AS adminsRemaining;

COMMIT TRANSACTION;
GO
