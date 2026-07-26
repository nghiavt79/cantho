/*
Delete the 5 "clean" admin accounts (no project/e-signature/proposal involvement),
per user decision 2026-07-16. Keeps admin(55), maihuong(4793), and the 3 workflow-
entangled accounts (duykhanh 72, huynhlybuu 18973, dieutrang 14730) for now.

Targets: kimminh(69), thanhnghia(70), maymay(7659), hienluong(11381), vohuyentran(22191).
Children present: UsersMenu(FK 145), UserLinhVuc(116), UserRole(17), ShoppingCart(8),
Comments(1). No ESign/Projects/Proposals/Rating rows.

SAFETY: all deleted rows backed up to *_CleanAcct5_20260716 first, one transaction,
children before parent. Reversible (plus the user's full DB .bak).
*/

SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO
USE TechExchangeNew;
GO

IF OBJECT_ID('dbo.Users_CleanAcct5_20260716') IS NOT NULL
BEGIN
    RAISERROR('Backup Users_CleanAcct5_20260716 already exists — aborting.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

CREATE TABLE #d (UserId int primary key);
INSERT #d VALUES (69),(70),(7659),(11381),(22191);

-- Backups
SELECT x.* INTO dbo.Users_CleanAcct5_20260716        FROM Users x        WHERE x.UserId   IN (SELECT UserId FROM #d);
SELECT x.* INTO dbo.UserRole_CleanAcct5_20260716     FROM UserRole x     WHERE x.UserId   IN (SELECT UserId FROM #d);
SELECT x.* INTO dbo.UsersMenu_CleanAcct5_20260716    FROM UsersMenu x    WHERE x.UserId   IN (SELECT UserId FROM #d);
SELECT x.* INTO dbo.UserLinhVuc_CleanAcct5_20260716  FROM UserLinhVuc x  WHERE x.UserId   IN (SELECT UserId FROM #d);
SELECT x.* INTO dbo.ShoppingCart_CleanAcct5_20260716 FROM ShoppingCart x WHERE x.UserId   IN (SELECT UserId FROM #d);
SELECT x.* INTO dbo.Comments_CleanAcct5_20260716     FROM Comments x     WHERE x.MemberId IN (SELECT UserId FROM #d);

-- Delete children
DELETE FROM UsersMenu    WHERE UserId   IN (SELECT UserId FROM #d);
DELETE FROM UserRole     WHERE UserId   IN (SELECT UserId FROM #d);
DELETE FROM UserLinhVuc  WHERE UserId   IN (SELECT UserId FROM #d);
DELETE FROM ShoppingCart WHERE UserId   IN (SELECT UserId FROM #d);
DELETE FROM Comments     WHERE MemberId IN (SELECT UserId FROM #d);

-- Delete users
DELETE FROM Users WHERE UserId IN (SELECT UserId FROM #d);
DECLARE @del INT = @@ROWCOUNT;

SELECT @del AS deleted,
  (SELECT COUNT(*) FROM dbo.Users_CleanAcct5_20260716) AS backedUp,
  (SELECT COUNT(*) FROM Users) AS usersRemaining;

COMMIT TRANSACTION;
GO
