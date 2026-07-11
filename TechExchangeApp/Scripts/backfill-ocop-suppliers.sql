/*
Backfills NhaCungUng (supplier) records for the 20 sample OCOP products and links
them via SanPhamCNTB.NCUId, since OCOP admin now requires a real Nhà cung ứng
(CungUngId) instead of the free-text HoTen field.
Guarded by FullName existence check — safe to re-run.
*/

DECLARE @Now DATETIME = GETDATE();

CREATE TABLE #OcopSuppliers (FullName NVARCHAR(200), DiaChi NVARCHAR(500));

INSERT INTO #OcopSuppliers (FullName, DiaChi)
SELECT DISTINCT HoTen, DiaChi
FROM SanPhamCNTB
WHERE ProductType = 4 AND HoTen IS NOT NULL;

INSERT INTO NhaCungUng (FullName, DiaChi, Domain, StatusId, LanguageId, SiteId, Created, Modified)
SELECT s.FullName, s.DiaChi, 'VN', 3, 1, 1, @Now, @Now
FROM #OcopSuppliers s
WHERE NOT EXISTS (SELECT 1 FROM NhaCungUng n WHERE n.FullName = s.FullName);

UPDATE p
SET p.NCUId = n.CungUngId
FROM SanPhamCNTB p
JOIN NhaCungUng n ON n.FullName = p.HoTen
WHERE p.ProductType = 4 AND p.NCUId IS NULL;

DROP TABLE #OcopSuppliers;
GO
