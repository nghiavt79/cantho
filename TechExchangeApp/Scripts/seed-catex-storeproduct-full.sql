/*
FULL import from catex.vn StoreProduct (the real marketplace catalog) into SanPhamCNTB.
Covers all 43 approved stores (~4,366 approved products, all with images). Follows the
trial (store 443). Marker Creator/CreatedBy = 'catex-store-import'; StatusId=1 (draft).

- Suppliers: one NhaCungUng per approved store (guard by FullName; reuses any existing).
- Products: deduped by Name (keep newest per name), guarded by Name against all existing
  SanPhamCNTB (so the 71 already-imported store-443 rows are not duplicated).
- ProductType: keyword heuristic (chemical/bio/software -> 1 Công nghệ, else 2 Thiết bị);
  admin refines during draft review.
- Images: downloaded separately into wwwroot/uploads/san-pham-catex/; QuyTrinhHinhAnh
  points to the file basename.
Idempotent.
*/
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO
USE TechExchangeNew;
GO

BEGIN TRANSACTION;

-- 1) Suppliers: one per approved store --------------------------------------------------
INSERT INTO NhaCungUng
    (FullName, DiaChi, Phone, Email, LoaiHinhToChuc, ChucNangChinh, Website,
     StatusId, IsActivated, LanguageId, Domain, SiteId, Created, Modified, CreatedBy)
SELECT s.TenGianHang,
       NULLIF(LTRIM(RTRIM(s.DiaChi1)), N''),
       NULLIF(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(s.DT)),' ',''),'.',''),'-',''), ''),
       NULLIF(LTRIM(RTRIM(s.Email)), ''),
       'Khac',
       NULLIF(LTRIM(RTRIM(CAST(s.MoTa AS nvarchar(max)))), N''),
       NULLIF(LTRIM(RTRIM(s.Website)), ''),
       3, 1, 1, 'VN', 1, GETDATE(), GETDATE(), 'catex-store-import'
FROM [catex.vn].dbo.Store s
WHERE s.Id IN (SELECT DISTINCT StoreId FROM [catex.vn].dbo.StoreProduct WHERE ApprovedStatus=1)
  AND s.TenGianHang IS NOT NULL AND LTRIM(RTRIM(s.TenGianHang)) <> N''
  AND NOT EXISTS (SELECT 1 FROM NhaCungUng n WHERE n.FullName = s.TenGianHang);
DECLARE @ncuNew INT = @@ROWCOUNT;

-- 2) Products ---------------------------------------------------------------------------
;WITH src AS (
  SELECT
    LTRIM(RTRIM(sp.TenSanPham)) AS Nm,
    co.NationName AS Xuxu,
    pr.TenHangSX  AS Hang,
    st.TenGianHang AS StoreName,
    CAST(sp.MoTa AS nvarchar(max))   AS MoTaShort,
    CAST(sp.ChiTiet AS nvarchar(max)) AS ChiTiet,
    sp.HangMoi, sp.BaoHanh, sp.NgayDang, sp.HinhAnh,
    ROW_NUMBER() OVER (PARTITION BY LTRIM(RTRIM(sp.TenSanPham)) ORDER BY sp.Id DESC) AS rn
  FROM [catex.vn].dbo.StoreProduct sp
  JOIN [catex.vn].dbo.Store st ON st.Id = sp.StoreId
  LEFT JOIN [catex.vn].dbo.Country  co ON sp.NuocXX = co.Id
  LEFT JOIN [catex.vn].dbo.Producer pr ON sp.HangSX = pr.Id
  WHERE sp.ApprovedStatus = 1 AND sp.HinhAnh LIKE '~/%' AND sp.TenSanPham IS NOT NULL
    AND st.TenGianHang IS NOT NULL
)
INSERT INTO SanPhamCNTB
    (Name, ProductType, XuatXu, MoTaNgan, MoTa, ThongSo, QuyTrinhHinhAnh,
     Code, StatusId, LanguageId, SiteId, NCUId, PublishedDate, Created, Modified, Creator, OwnerType)
SELECT
    src.Nm,
    CASE WHEN LOWER(src.Nm) LIKE N'%hóa chất%' OR LOWER(src.Nm) LIKE N'%vi sinh%'
            OR LOWER(src.Nm) LIKE N'%chế phẩm%' OR LOWER(src.Nm) LIKE N'%dung dịch%'
            OR LOWER(src.Nm) LIKE N'%phần mềm%' OR LOWER(src.Nm) LIKE N'%nhũ tương%'
            OR LOWER(src.Nm) LIKE N'%mực in%'   OR LOWER(src.Nm) LIKE N'%bột màu%'
            OR LOWER(src.Nm) LIKE N'%enzyme%'   OR LOWER(src.Nm) LIKE N'%xúc tác%'
            OR LOWER(src.Nm) LIKE N'%phân bón%' OR LOWER(src.Nm) LIKE N'%dung môi%'
            OR LOWER(src.Nm) LIKE N'%men vi sinh%'
         THEN 1 ELSE 2 END,
    src.Xuxu,
    LTRIM(RTRIM(src.MoTaShort)),
    LTRIM(RTRIM(ISNULL(NULLIF(src.ChiTiet, N''), src.MoTaShort))),
    N'<ul>'
      + CASE WHEN src.Hang IS NOT NULL AND LTRIM(RTRIM(src.Hang)) NOT IN (N'',N'Khác')
             THEN N'<li><strong>Hãng sản xuất:</strong> ' + LTRIM(RTRIM(src.Hang)) + N'</li>' ELSE N'' END
      + N'<li><strong>Tình trạng:</strong> ' + CASE WHEN src.HangMoi=0 THEN N'đã qua sử dụng' ELSE N'hàng mới 100%' END + N'</li>'
      + CASE WHEN src.BaoHanh IS NOT NULL AND LTRIM(RTRIM(src.BaoHanh))<>N''
             THEN N'<li><strong>Bảo hành:</strong> ' + LTRIM(RTRIM(src.BaoHanh)) + N'</li>' ELSE N'' END
      + N'</ul>',
    N'/uploads/san-pham-catex/' + RIGHT(src.HinhAnh, CHARINDEX('/', REVERSE(src.HinhAnh)) - 1),
    NULL, 1, 1, 1, ncu.CungUngId, src.NgayDang, GETDATE(), GETDATE(), 'catex-store-import', NULL
FROM src
JOIN NhaCungUng ncu ON ncu.FullName = src.StoreName
WHERE src.rn = 1
  AND NOT EXISTS (SELECT 1 FROM SanPhamCNTB e WHERE e.Name = src.Nm);
DECLARE @ins INT = @@ROWCOUNT;

-- 3) Code = prefix + real ID
UPDATE SanPhamCNTB
SET Code = CASE WHEN ProductType=1 THEN 'CN-' ELSE 'TB-' END + RIGHT('00000' + CAST(ID AS VARCHAR(10)), 5)
WHERE Creator = 'catex-store-import' AND Code IS NULL;

SELECT @ncuNew AS NewSuppliers, @ins AS NewProducts,
  (SELECT COUNT(*) FROM SanPhamCNTB WHERE Creator='catex-store-import') AS TotalStoreProducts,
  (SELECT COUNT(*) FROM NhaCungUng WHERE CreatedBy='catex-store-import') AS TotalStoreSuppliers;

COMMIT TRANSACTION;
GO
