/*
Remove non-SiteId=1 user accounts from TechExchangeNew (requested 2026-07-16).
This DB is a multi-province (multi-tenant) platform; SiteId=1 = Cần Thơ (this site).
The 175 users with SiteId <> 1 (or NULL) are other provinces' admin/staff accounts.

Scope (agreed with user): the 175 user accounts + their account-level artifacts only
(UserRole, UsersMenu, UserLinhVuc, UserOtps, UserVerificationDocs, ProposalScores).
Other-province CONTENT (Contents/HoiThao/Menu/PhieuYeuCauCNTB…) is intentionally kept.
Those users authored no Product/NhaCungUng/NhaTuVan (verified 0), so no seller/product
data is affected.

SAFETY: every deleted row (users + each child table's rows) is copied to a
*_SiteCleanup_20260716 backup table first, inside one transaction. FK children are
deleted before the parent users. Fully reversible from the backups.
*/

SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO
USE TechExchangeNew;
GO

IF OBJECT_ID('dbo.Users_SiteCleanup_20260716') IS NOT NULL
BEGIN
    RAISERROR('Backup Users_SiteCleanup_20260716 already exists — aborting to avoid clobbering.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

-- Target set: users NOT on SiteId=1 -----------------------------------------------------
CREATE TABLE #o (UserId int primary key);
INSERT #o SELECT UserId FROM Users WHERE ISNULL(SiteId,-1) <> 1;

-- Backups (full-row copies) -------------------------------------------------------------
SELECT u.*      INTO dbo.Users_SiteCleanup_20260716                FROM Users u                 WHERE u.UserId IN (SELECT UserId FROM #o);
SELECT r.*      INTO dbo.UserRole_SiteCleanup_20260716             FROM UserRole r              WHERE r.UserId IN (SELECT UserId FROM #o);
SELECT m.*      INTO dbo.UsersMenu_SiteCleanup_20260716            FROM UsersMenu m             WHERE m.UserId IN (SELECT UserId FROM #o);
SELECT l.*      INTO dbo.UserLinhVuc_SiteCleanup_20260716          FROM UserLinhVuc l           WHERE l.UserId IN (SELECT UserId FROM #o);
SELECT o.*      INTO dbo.UserOtps_SiteCleanup_20260716             FROM UserOtps o              WHERE o.UserId IN (SELECT UserId FROM #o);
SELECT v.*      INTO dbo.UserVerificationDocs_SiteCleanup_20260716 FROM UserVerificationDocs v  WHERE v.UserId IN (SELECT UserId FROM #o);
SELECT ps.*     INTO dbo.ProposalScores_SiteCleanup_20260716       FROM ProposalScores ps       WHERE ps.ConsultantId IN (SELECT UserId FROM #o);

-- Delete children first (FK-constrained + account artifacts) -----------------------------
DELETE FROM ProposalScores       WHERE ConsultantId IN (SELECT UserId FROM #o);
DELETE FROM UserVerificationDocs WHERE UserId       IN (SELECT UserId FROM #o);
DELETE FROM UserOtps             WHERE UserId       IN (SELECT UserId FROM #o);
DELETE FROM UsersMenu            WHERE UserId       IN (SELECT UserId FROM #o);
DELETE FROM UserRole             WHERE UserId       IN (SELECT UserId FROM #o);
DELETE FROM UserLinhVuc          WHERE UserId       IN (SELECT UserId FROM #o);

-- Delete the users ----------------------------------------------------------------------
DELETE FROM Users WHERE UserId IN (SELECT UserId FROM #o);
DECLARE @delUsers INT = @@ROWCOUNT;

-- Report --------------------------------------------------------------------------------
SELECT
  @delUsers AS usersDeleted,
  (SELECT COUNT(*) FROM dbo.Users_SiteCleanup_20260716) AS usersBackedUp,
  (SELECT COUNT(*) FROM Users) AS usersRemaining,
  (SELECT COUNT(*) FROM Users WHERE ISNULL(SiteId,-1) <> 1) AS nonSite1Left,
  (SELECT COUNT(*) FROM Users WHERE SiteId = 1) AS site1Left;

COMMIT TRANSACTION;
GO
