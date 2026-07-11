/*
Customer request: delete all old NhaCungUng suppliers, keeping only:
  - The 20 suppliers created this session (14 for OCOP products + 6 for the real
    sample product import)
  - 2 pre-existing suppliers still referenced by kept products: MISA (8096) and
    Machinex Việt Nam (7225)
Safety: backs up all affected rows into NhaCungUng_Backup_20260711 before the
hard delete.
*/

SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Keep TABLE (Id INT PRIMARY KEY);
INSERT INTO @Keep VALUES
    (7225),(8096),
    (8873),(8874),(8875),(8876),(8877),(8878),(8879),(8880),(8881),(8882),(8883),(8884),(8885),(8886),
    (8887),(8888),(8889),(8890),(8891),(8892);

IF OBJECT_ID('tempdb..#NcuToDelete') IS NOT NULL DROP TABLE #NcuToDelete;
SELECT CungUngId INTO #NcuToDelete
FROM NhaCungUng
WHERE CungUngId NOT IN (SELECT Id FROM @Keep);

IF OBJECT_ID('dbo.NhaCungUng_Backup_20260711') IS NOT NULL DROP TABLE dbo.NhaCungUng_Backup_20260711;
SELECT n.* INTO dbo.NhaCungUng_Backup_20260711
FROM NhaCungUng n JOIN #NcuToDelete d ON d.CungUngId = n.CungUngId;

DELETE n FROM NhaCungUng n JOIN #NcuToDelete d ON d.CungUngId = n.CungUngId;

DROP TABLE #NcuToDelete;
GO
