/*
One-off cleanup of unused/orphan Users in TechExchangeNew (requested 2026-07-16).

"Orphan" = a user referenced NOWHERE by id across the DB — checked against every
id-based reference column (all 18 FK columns to Users PLUS every UserId/MemberId/
CreatorId/SellerId/ConsultantId/... column, including nvarchar id columns) — AND
whose UserName is not an author (Creator/Modifier/CreatedBy) in any content table,
AND is not an admin. Result: 1681 rows (subset of the 1757 id-orphans; 76 kept
because admin or username-authored content). These are overwhelmingly spam/bot
signups that registered and never engaged.

SAFETY: every deleted row is copied to Users_DeletedBackup_20260716 first, inside
one transaction, so the delete is fully reversible. The delete is FK-safe because
the set excludes every FK-referenced UserId.
*/

SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO
USE TechExchangeNew;
GO

IF OBJECT_ID('dbo.Users_DeletedBackup_20260716') IS NOT NULL
BEGIN
    RAISERROR('Backup table Users_DeletedBackup_20260716 already exists — aborting to avoid clobbering a prior backup.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

-- id-based reference set (every column that points at a user by id) ---------------------
CREATE TABLE #ref (UserId int primary key);
INSERT #ref SELECT DISTINCT u FROM (
  SELECT UserId u FROM Rating WHERE UserId>0 UNION SELECT UserId FROM PhieuYeuCauCNTB WHERE UserId>0
  UNION SELECT MemberId FROM Comments WHERE MemberId>0 UNION SELECT UserId FROM ShoppingCart WHERE UserId>0
  UNION SELECT UserId FROM UserRole WHERE UserId>0 UNION SELECT MemberId FROM CommentsYCTB WHERE MemberId>0
  UNION SELECT UserId FROM UserLinhVuc WHERE UserId>0 UNION SELECT UserId FROM UsersMenu WHERE UserId>0
  UNION SELECT UserId FROM TimKiemDoiTac WHERE UserId>0 UNION SELECT UserId FROM ForumYCTB WHERE UserId>0
  UNION SELECT UserId FROM AiChatSessions WHERE UserId>0 UNION SELECT UserId FROM NhaCungUng WHERE UserId>0
  UNION SELECT MemberId FROM CommentsYeuCau WHERE MemberId>0 UNION SELECT UserId FROM ProjectMembers WHERE UserId>0
  UNION SELECT UserId FROM NhaTuVan WHERE UserId>0 UNION SELECT CreatorId FROM Product WHERE CreatorId>0
  UNION SELECT ModifierId FROM Product WHERE ModifierId>0 UNION SELECT UserId FROM ProjectAccessLogs WHERE UserId>0
  UNION SELECT ActorUserId FROM ContractAuditLogs WHERE ActorUserId>0 UNION SELECT SignerUserId FROM ESignSignatures WHERE SignerUserId>0
  UNION SELECT UserId FROM ESignAuditLogs WHERE UserId>0 UNION SELECT UserId FROM ContractApprovals WHERE UserId>0
  UNION SELECT UserId FROM ContractSignatureRequests WHERE UserId>0 UNION SELECT UserId FROM ContractSignatures WHERE UserId>0
  UNION SELECT UserId FROM UserVerificationDocs WHERE UserId>0 UNION SELECT UserId FROM ForumYCDV WHERE UserId>0
  UNION SELECT ConsultantId FROM ProposalScores WHERE ConsultantId>0 UNION SELECT SellerId FROM NegotiationForms WHERE SellerId>0
  UNION SELECT SellerId FROM RFQInvitations WHERE SellerId>0 UNION SELECT UserId FROM UserOtps WHERE UserId>0
  UNION SELECT UserId FROM PhieuYeuCauDichVu WHERE UserId>0 UNION SELECT AuthorId FROM ContractComments WHERE AuthorId>0
  UNION SELECT CreatedBy FROM ESignDocuments WHERE CreatedBy>0 UNION SELECT SelectedSellerId FROM Projects WHERE SelectedSellerId>0
  UNION SELECT ConsultantId FROM ProjectConsultants WHERE ConsultantId>0 UNION SELECT UserId FROM Store WHERE UserId>0
  UNION SELECT UserId FROM UserStore WHERE UserId>0 UNION SELECT ActorUserId FROM WorkflowTransitionLogs WHERE ActorUserId>0
  UNION SELECT TRY_CAST(UserId AS int) FROM Likepage WHERE TRY_CAST(UserId AS int)>0
  UNION SELECT TRY_CAST(UserId AS int) FROM Notifications WHERE TRY_CAST(UserId AS int)>0
  UNION SELECT TRY_CAST(UserId AS int) FROM EntityActionLogs WHERE TRY_CAST(UserId AS int)>0
) x;

-- username-based reference set (authors of content) -------------------------------------
CREATE TABLE #uref (nm nvarchar(256) primary key);
INSERT #uref SELECT DISTINCT nm FROM (
  SELECT Creator nm FROM [Order] WHERE Creator IS NOT NULL UNION SELECT Modifier FROM [Order] WHERE Modifier IS NOT NULL
  UNION SELECT Creator FROM ChuyenGia WHERE Creator IS NOT NULL UNION SELECT Modifier FROM ChuyenGia WHERE Modifier IS NOT NULL
  UNION SELECT Creator FROM Contents WHERE Creator IS NOT NULL UNION SELECT Modifier FROM Contents WHERE Modifier IS NOT NULL
  UNION SELECT Creator FROM HoiThao WHERE Creator IS NOT NULL UNION SELECT Modifier FROM HoiThao WHERE Modifier IS NOT NULL
  UNION SELECT CreatedBy FROM TimKiemDoiTac WHERE CreatedBy IS NOT NULL UNION SELECT Modifier FROM TimKiemDoiTac WHERE Modifier IS NOT NULL
  UNION SELECT Creator FROM Menu WHERE Creator IS NOT NULL UNION SELECT Modifier FROM Menu WHERE Modifier IS NOT NULL
  UNION SELECT Creator FROM SanPhamCNTB WHERE Creator IS NOT NULL UNION SELECT Modifier FROM SanPhamCNTB WHERE Modifier IS NOT NULL
  UNION SELECT Creator FROM SanPhamKhoiNghiep WHERE Creator IS NOT NULL UNION SELECT Modifier FROM SanPhamKhoiNghiep WHERE Modifier IS NOT NULL
  UNION SELECT Creator FROM ContentsYeucau WHERE Creator IS NOT NULL UNION SELECT Modifier FROM ContentsYeucau WHERE Modifier IS NOT NULL
  UNION SELECT Creator FROM Category WHERE Creator IS NOT NULL UNION SELECT Modifier FROM Category WHERE Modifier IS NOT NULL
  UNION SELECT CreatedBy FROM NhaCungUng WHERE CreatedBy IS NOT NULL UNION SELECT Modifier FROM NhaCungUng WHERE Modifier IS NOT NULL
  UNION SELECT CreatedBy FROM ForumYCTB WHERE CreatedBy IS NOT NULL UNION SELECT CreatedBy FROM PhieuYeuCauCNTB WHERE CreatedBy IS NOT NULL
  UNION SELECT Creator FROM KeywordLienKet WHERE Creator IS NOT NULL UNION SELECT Creator FROM HoiThaoLienKet WHERE Creator IS NOT NULL
  UNION SELECT CreatedBy FROM NhaTuVan WHERE CreatedBy IS NOT NULL UNION SELECT Modifier FROM NhaTuVan WHERE Modifier IS NOT NULL
) y WHERE nm<>'';

-- final delete set ----------------------------------------------------------------------
SELECT u.* INTO #del
FROM Users u
WHERE ISNULL(u.IsAdmin,0)=0
  AND NOT EXISTS (SELECT 1 FROM #ref  r WHERE r.UserId = u.UserId)
  AND NOT EXISTS (SELECT 1 FROM #uref n WHERE n.nm    = u.UserName);

-- backup, then delete -------------------------------------------------------------------
SELECT * INTO dbo.Users_DeletedBackup_20260716 FROM #del;

DELETE u FROM Users u WHERE EXISTS (SELECT 1 FROM #del d WHERE d.UserId = u.UserId);
DECLARE @deleted INT = @@ROWCOUNT;

SELECT
  (SELECT COUNT(*) FROM #del) AS finalDeleteSet,
  (SELECT COUNT(*) FROM dbo.Users_DeletedBackup_20260716) AS backedUp,
  @deleted AS deleted,
  (SELECT COUNT(*) FROM Users) AS usersRemaining;

COMMIT TRANSACTION;
GO
