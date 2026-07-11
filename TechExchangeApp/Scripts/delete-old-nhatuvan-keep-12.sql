/*
Customer request: delete all NhaTuVan (chuyên gia/consultant) records except the
12 imported from the customer-provided CVs in CSDL Mau/ChuyenGIa/ this session
(3 pre-existing: 4122, 4124, 4132; 9 newly imported: 4544-4552).
No FK constraints reference NhaTuVan; the only soft references found
(ThongKeDichVu.NhaTuvan, ThongKeGiaoDich.Yeucautuvan, ThongKeHoatDong.NhaTuVan,
ThongKeHoatDongTheoLinhVuc.NhaTuvan) are independent int counters, not row IDs.
Safety: backs up all affected rows into NhaTuVan_Backup_20260711 before the
hard delete.
*/

SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Keep TABLE (Id INT PRIMARY KEY);
INSERT INTO @Keep VALUES
    (4122),(4124),(4132),
    (4544),(4545),(4546),(4547),(4548),(4549),(4550),(4551),(4552);

IF OBJECT_ID('tempdb..#NtvToDelete') IS NOT NULL DROP TABLE #NtvToDelete;
SELECT TuVanId INTO #NtvToDelete
FROM NhaTuVan
WHERE TuVanId NOT IN (SELECT Id FROM @Keep);

IF OBJECT_ID('dbo.NhaTuVan_Backup_20260711') IS NOT NULL DROP TABLE dbo.NhaTuVan_Backup_20260711;
SELECT n.* INTO dbo.NhaTuVan_Backup_20260711
FROM NhaTuVan n JOIN #NtvToDelete d ON d.TuVanId = n.TuVanId;

DELETE n FROM NhaTuVan n JOIN #NtvToDelete d ON d.TuVanId = n.TuVanId;

SELECT (SELECT COUNT(*) FROM NhaTuVan_Backup_20260711) AS BackedUp,
       (SELECT COUNT(*) FROM NhaTuVan) AS RemainingInTable;

DROP TABLE #NtvToDelete;
GO
