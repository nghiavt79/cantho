/*
Delete all non-admin users from TechExchangeNew, keeping only IsAdmin=1 accounts
(requested 2026-07-16, new-site reset; user confirmed and took a full DB backup).

At run time: 4210 users (all SiteId=1 after prior cleanup), 43 admins, 4167 non-admin.
Scope (confirmed): delete the 4167 non-admin users + their account artifacts only
(UserRole, UsersMenu, UserLinhVuc). Their activity data (Rating/Comments/ShoppingCart)
is intentionally KEPT. These users have no Product/NhaCungUng/NhaTuVan/OTP/ESign/
verification rows (verified 0), so only UsersMenu(FK, 28), UserRole(3914) and
UserLinhVuc(15) need clearing.

SAFETY: every deleted row (users + each child table) is copied to a
*_NonAdminCleanup_20260716 backup table first, inside one transaction. FK children
deleted before parents. Fully reversible from the backups (plus the user's DB backup).
*/

SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO
USE TechExchangeNew;
GO

IF OBJECT_ID('dbo.Users_NonAdminCleanup_20260716') IS NOT NULL
BEGIN
    RAISERROR('Backup Users_NonAdminCleanup_20260716 already exists — aborting to avoid clobbering.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

CREATE TABLE #na (UserId int primary key);
INSERT #na SELECT UserId FROM Users WHERE ISNULL(IsAdmin,0) <> 1;

-- Backups -------------------------------------------------------------------------------
SELECT u.* INTO dbo.Users_NonAdminCleanup_20260716       FROM Users u        WHERE u.UserId IN (SELECT UserId FROM #na);
SELECT r.* INTO dbo.UserRole_NonAdminCleanup_20260716    FROM UserRole r     WHERE r.UserId IN (SELECT UserId FROM #na);
SELECT m.* INTO dbo.UsersMenu_NonAdminCleanup_20260716   FROM UsersMenu m    WHERE m.UserId IN (SELECT UserId FROM #na);
SELECT l.* INTO dbo.UserLinhVuc_NonAdminCleanup_20260716 FROM UserLinhVuc l  WHERE l.UserId IN (SELECT UserId FROM #na);

-- Delete children (FK + artifacts) ------------------------------------------------------
DELETE FROM UsersMenu   WHERE UserId IN (SELECT UserId FROM #na);
DELETE FROM UserRole    WHERE UserId IN (SELECT UserId FROM #na);
DELETE FROM UserLinhVuc WHERE UserId IN (SELECT UserId FROM #na);

-- Delete users --------------------------------------------------------------------------
DELETE FROM Users WHERE UserId IN (SELECT UserId FROM #na);
DECLARE @del INT = @@ROWCOUNT;

SELECT
  @del AS usersDeleted,
  (SELECT COUNT(*) FROM dbo.Users_NonAdminCleanup_20260716) AS usersBackedUp,
  (SELECT COUNT(*) FROM Users) AS usersRemaining,
  (SELECT COUNT(*) FROM Users WHERE IsAdmin=1) AS adminsLeft,
  (SELECT COUNT(*) FROM Users WHERE ISNULL(IsAdmin,0)<>1) AS nonAdminLeft;

COMMIT TRANSACTION;
GO
