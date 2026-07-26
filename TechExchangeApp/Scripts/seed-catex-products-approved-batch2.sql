/*
Imports the remaining APPROVED (ApprovedStatus=1) products from the legacy
catex.vn database into SanPhamCNTB — the second batch after seed-catex-products.sql
(which brought in the first 55). Scope agreed with user: approved products only.

Text is read directly from [catex.vn].dbo.Product (clean Unicode at rest), so this
is a cross-database INSERT..SELECT — no hand-retyped VALUES.

Field mapping (same convention as seed-catex-products.sql):
  Name        <- Product.TenSanPham (trimmed)
  MoTaNgan    <- Product.MoTa
  MoTa        <- Product.MoTa
  XuatXu      <- Country.NationName (via Product.NuocXX)
  ThongSo     <- <ul> built from: Hãng SX (Producer, skip 'Khác'), Tình trạng
                 (HangMoi), Bảo hành (BaoHanh)
  ProductType <- 1 (Công nghệ) for the 11 chemical/material/software listings below,
                 else 2 (Thiết bị)
  PublishedDate <- Product.NgayDang (original listing date)
  Code        <- CN-#####/TB-##### = prefix + real identity ID (set after insert)
  StatusId=1 (draft/pending review), LanguageId=1, SiteId=1, Creator='catex-import',
  OwnerType=NULL, QuyTrinhHinhAnh=NULL (images backfilled later — catex.vn was under
  maintenance at import time, so no image files were available).

Only public product data kept; personal seller info (name/phone/username) excluded.
Idempotent — guarded by Name alone (catex-import product names are unique, and a
product may have been classified with a different ProductType in the first batch;
guarding by Name+ProductType would wrongly re-insert those as duplicates). Code is
only assigned to rows that still have Code IS NULL, so re-running does not duplicate
or renumber.
*/

SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

USE TechExchangeNew;
GO

BEGIN TRANSACTION;

-- catex.vn Product Ids classified as ProductType=1 (Công nghệ: chế phẩm/vật liệu/phần mềm).
-- Everything else in the approved set is ProductType=2 (Thiết bị).
DECLARE @Type1 TABLE (Id BIGINT PRIMARY KEY);
INSERT INTO @Type1 (Id) VALUES
    (1242), -- HẠT NHỰA CATION PUROLITE C100
    (1251), -- Keo không khí cho sản xuất băng keo, decal, nhãn
    (1245), -- Nhũ tương Pure Acrylic cho sản xuất sơn nước
    (1247), -- Nhũ tương Styrene Acrylic cho sản xuất sơn nước
    (1248), -- Nhũ tương Vinyl Acrylic / VaVeo cho sản xuất sơn nước
    (1252), -- Trợ chất cho in bông
    (1249), -- Trợ chất cho sản xuất sơn nước
    (1173), -- Zoom cho học trực tuyến (phần mềm)
    (1241), -- KEO DAN DM-5000
    (1179), -- Phần Mềm SIGMA
    (1253); -- Men Vi Sinh Hiếu Khí BCP10 Bionetix

INSERT INTO SanPhamCNTB
    (Name, ProductType, XuatXu, MoTaNgan, MoTa, ThongSo,
     Code, QuyTrinhHinhAnh, StatusId, LanguageId, SiteId,
     PublishedDate, Created, Modified, Creator, OwnerType)
SELECT
    LTRIM(RTRIM(p.TenSanPham)),
    CASE WHEN t.Id IS NOT NULL THEN 1 ELSE 2 END,
    co.NationName,
    LTRIM(RTRIM(CAST(p.MoTa AS NVARCHAR(MAX)))),
    LTRIM(RTRIM(CAST(p.MoTa AS NVARCHAR(MAX)))),
    N'<ul>'
      + CASE WHEN pr.TenHangSX IS NOT NULL
                  AND LTRIM(RTRIM(pr.TenHangSX)) NOT IN (N'', N'Khác')
             THEN N'<li><strong>Hãng sản xuất:</strong> ' + LTRIM(RTRIM(pr.TenHangSX)) + N'</li>'
             ELSE N'' END
      + N'<li><strong>Tình trạng:</strong> '
      + CASE WHEN p.HangMoi = 0 THEN N'đã qua sử dụng' ELSE N'hàng mới 100%' END
      + N'</li>'
      + CASE WHEN p.BaoHanh IS NOT NULL AND LTRIM(RTRIM(p.BaoHanh)) <> N''
             THEN N'<li><strong>Bảo hành:</strong> ' + LTRIM(RTRIM(p.BaoHanh)) + N'</li>'
             ELSE N'' END
      + N'</ul>',
    NULL,                       -- Code assigned below (= real ID)
    NULL,                       -- QuyTrinhHinhAnh (image backfilled later)
    1, 1, 1,
    p.NgayDang,
    GETDATE(), GETDATE(), 'catex-import', NULL
FROM [catex.vn].dbo.Product p
LEFT JOIN [catex.vn].dbo.Country  co ON p.NuocXX = co.Id
LEFT JOIN [catex.vn].dbo.Producer pr ON p.HangSX = pr.Id
LEFT JOIN @Type1 t ON t.Id = p.Id
WHERE p.ApprovedStatus = 1
  AND p.TenSanPham IS NOT NULL
  AND NOT EXISTS (
        SELECT 1 FROM SanPhamCNTB e
        WHERE e.Name = LTRIM(RTRIM(p.TenSanPham))
      );

DECLARE @Inserted INT = @@ROWCOUNT;

-- Code = prefix + real identity ID (guarantees Code number matches ID exactly)
UPDATE SanPhamCNTB
SET Code = CASE WHEN ProductType = 1 THEN 'CN-' ELSE 'TB-' END + RIGHT('00000' + CAST(ID AS VARCHAR(10)), 5)
WHERE Creator = 'catex-import' AND Code IS NULL;

SELECT @Inserted AS RowsInserted;

COMMIT TRANSACTION;
GO
