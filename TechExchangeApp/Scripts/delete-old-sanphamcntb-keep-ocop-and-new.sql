/*
Customer request: delete all old SanPhamCNTB products (Công nghệ/Thiết bị/Tài sản trí
tuệ), keeping ONLY:
  - OCOP products (ProductType = 4, 20 rows)
  - The 7 newly imported products (Creator = 'sample-import')

Safety: backs up all affected rows (parent + linked child tables) into
*_Backup_20260711 tables in the same DB before the hard delete, since this touches
2241 products and thousands of linked Category/Media/Price/IP rows.
*/

SET QUOTED_IDENTIFIER ON;
GO

-- Identify the IDs to delete
IF OBJECT_ID('tempdb..#IdsToDelete') IS NOT NULL DROP TABLE #IdsToDelete;
SELECT ID INTO #IdsToDelete
FROM SanPhamCNTB
WHERE ProductType <> 4 AND Creator <> 'sample-import';

-- Backups
IF OBJECT_ID('dbo.SanPhamCNTB_Backup_20260711') IS NOT NULL DROP TABLE dbo.SanPhamCNTB_Backup_20260711;
SELECT p.* INTO dbo.SanPhamCNTB_Backup_20260711
FROM SanPhamCNTB p JOIN #IdsToDelete d ON d.ID = p.ID;

IF OBJECT_ID('dbo.SanPhamCNTBCategory_Backup_20260711') IS NOT NULL DROP TABLE dbo.SanPhamCNTBCategory_Backup_20260711;
SELECT c.* INTO dbo.SanPhamCNTBCategory_Backup_20260711
FROM SanPhamCNTBCategory c JOIN #IdsToDelete d ON d.ID = c.SanPhamCNTBId;

IF OBJECT_ID('dbo.SanPhamCNTBMedia_Backup_20260711') IS NOT NULL DROP TABLE dbo.SanPhamCNTBMedia_Backup_20260711;
SELECT m.* INTO dbo.SanPhamCNTBMedia_Backup_20260711
FROM SanPhamCNTBMedia m JOIN #IdsToDelete d ON d.ID = m.SanPhamCNTBId;

IF OBJECT_ID('dbo.SanPhamCNTBPrice_Backup_20260711') IS NOT NULL DROP TABLE dbo.SanPhamCNTBPrice_Backup_20260711;
SELECT pr.* INTO dbo.SanPhamCNTBPrice_Backup_20260711
FROM SanPhamCNTBPrice pr JOIN #IdsToDelete d ON d.ID = pr.SanPhamCNTBId;

IF OBJECT_ID('dbo.SanPhamCNTBIP_Backup_20260711') IS NOT NULL DROP TABLE dbo.SanPhamCNTBIP_Backup_20260711;
SELECT ip.* INTO dbo.SanPhamCNTBIP_Backup_20260711
FROM SanPhamCNTBIP ip JOIN #IdsToDelete d ON d.ID = ip.SanPhamCNTBId;

-- Delete children first (FK constraints), then parents
DELETE c FROM SanPhamCNTBCategory c JOIN #IdsToDelete d ON d.ID = c.SanPhamCNTBId;
DELETE m FROM SanPhamCNTBMedia m JOIN #IdsToDelete d ON d.ID = m.SanPhamCNTBId;
DELETE pr FROM SanPhamCNTBPrice pr JOIN #IdsToDelete d ON d.ID = pr.SanPhamCNTBId;
DELETE ip FROM SanPhamCNTBIP ip JOIN #IdsToDelete d ON d.ID = ip.SanPhamCNTBId;
DELETE FROM SanPhamEmbeddings WHERE SanPhamId IN (SELECT ID FROM #IdsToDelete);
DELETE p FROM SanPhamCNTB p JOIN #IdsToDelete d ON d.ID = p.ID;

DROP TABLE #IdsToDelete;
GO
