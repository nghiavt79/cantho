/*
TRIAL import from the catex.vn StoreProduct catalog (the real marketplace catalog,
4,366 approved products with images — the earlier import mistakenly used the small
Product table). This trial does ONE store: 443 = CÔNG TY TNHH THIẾT BỊ Y TẾ THÀNH PHÁT
(71 approved medical-equipment products), so the customer can review the approach
before the full import.

Images: all 71 already downloaded from catex.vn into wwwroot/uploads/san-pham-catex/.
Marker: Creator/CreatedBy = 'catex-store-import' (distinct from the Product-table
batch's 'catex-import'). Status: StatusId=1 (draft/pending review).
Idempotent — guarded by FullName (supplier) and Name (products).
*/
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO
USE TechExchangeNew;
GO

BEGIN TRANSACTION;

-- 1) Supplier from the catex Store record ----------------------------------------------
INSERT INTO NhaCungUng
    (FullName, DiaChi, Phone, Email, LoaiHinhToChuc, ChucNangChinh, Website,
     StatusId, IsActivated, LanguageId, Domain, SiteId, Created, Modified, CreatedBy)
SELECT N'CÔNG TY TNHH THIẾT BỊ Y TẾ THÀNH PHÁT',
       N'156/1/12/4 Cộng Hòa, Phường Bảy Hiền, TP. Hồ Chí Minh',
       '0988510820', 'info@tpmedical.com.vn', 'Khac',
       N'Nhà cung cấp thiết bị y tế: bơm tiêm điện, máy theo dõi bệnh nhân và thiết bị điều trị.',
       'https://tpmedical.com.vn/',
       3, 1, 1, 'VN', 1, GETDATE(), GETDATE(), 'catex-store-import'
WHERE NOT EXISTS (SELECT 1 FROM NhaCungUng n WHERE n.FullName = N'CÔNG TY TNHH THIẾT BỊ Y TẾ THÀNH PHÁT');

DECLARE @ncu INT = (SELECT TOP 1 CungUngId FROM NhaCungUng WHERE FullName = N'CÔNG TY TNHH THIẾT BỊ Y TẾ THÀNH PHÁT');

-- 2) Products from StoreProduct (store 443, approved) -----------------------------------
INSERT INTO SanPhamCNTB
    (Name, ProductType, XuatXu, MoTaNgan, MoTa, ThongSo, QuyTrinhHinhAnh,
     Code, StatusId, LanguageId, SiteId, NCUId, PublishedDate, Created, Modified, Creator, OwnerType)
SELECT
    LTRIM(RTRIM(sp.TenSanPham)),
    2,                                        -- Thiết bị (store 443 = thiết bị y tế)
    co.NationName,
    LTRIM(RTRIM(CAST(sp.MoTa AS NVARCHAR(MAX)))),
    LTRIM(RTRIM(CAST(ISNULL(NULLIF(CAST(sp.ChiTiet AS NVARCHAR(MAX)),N''), sp.MoTa) AS NVARCHAR(MAX)))),
    N'<ul>'
      + CASE WHEN pr.TenHangSX IS NOT NULL AND LTRIM(RTRIM(pr.TenHangSX)) NOT IN (N'',N'Khác')
             THEN N'<li><strong>Hãng sản xuất:</strong> ' + LTRIM(RTRIM(pr.TenHangSX)) + N'</li>' ELSE N'' END
      + N'<li><strong>Tình trạng:</strong> ' + CASE WHEN sp.HangMoi=0 THEN N'đã qua sử dụng' ELSE N'hàng mới 100%' END + N'</li>'
      + CASE WHEN sp.BaoHanh IS NOT NULL AND LTRIM(RTRIM(sp.BaoHanh))<>N''
             THEN N'<li><strong>Bảo hành:</strong> ' + LTRIM(RTRIM(sp.BaoHanh)) + N'</li>' ELSE N'' END
      + N'</ul>',
    N'/uploads/san-pham-catex/' + RIGHT(sp.HinhAnh, CHARINDEX('/', REVERSE(sp.HinhAnh)) - 1),
    NULL, 1, 1, 1, @ncu, sp.NgayDang, GETDATE(), GETDATE(), 'catex-store-import', NULL
FROM [catex.vn].dbo.StoreProduct sp
LEFT JOIN [catex.vn].dbo.Country  co ON sp.NuocXX = co.Id
LEFT JOIN [catex.vn].dbo.Producer pr ON sp.HangSX = pr.Id
WHERE sp.StoreId = 443 AND sp.ApprovedStatus = 1
  AND sp.HinhAnh LIKE '~/%' AND sp.TenSanPham IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM SanPhamCNTB e WHERE e.Name = LTRIM(RTRIM(sp.TenSanPham)));

DECLARE @ins INT = @@ROWCOUNT;

-- 3) Code = prefix + real ID
UPDATE SanPhamCNTB
SET Code = 'TB-' + RIGHT('00000' + CAST(ID AS VARCHAR(10)), 5)
WHERE Creator = 'catex-store-import' AND Code IS NULL;

SELECT @ins AS ProductsInserted, @ncu AS SupplierId,
       (SELECT COUNT(*) FROM SanPhamCNTB WHERE Creator='catex-store-import') AS TotalStoreImport;

COMMIT TRANSACTION;
GO
