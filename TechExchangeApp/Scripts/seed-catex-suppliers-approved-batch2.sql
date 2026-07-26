/*
Suppliers (NhaCungUng) for the approved batch-2 catex.vn import (seed-catex-products-approved-batch2.sql).
Unlike batch 1 (which guessed distributors from product brands via web search), sellers here
are taken from the AUTHORITATIVE catex.vn Store record for each product (Product.UserId -> Store).

The 81 batch-2 products belong to 6 catex sellers (Product.UserId):
  403 CÔNG TY TNHH BETA TECHNOLOGY ....... 60 -> new NCU (this script)
  426 Keo_Công_Nghiệp (@ttkco.com) ........  7 -> existing 8898 (T.T.K)
  430 Vạn Nghĩa ...........................  6 -> existing 8893 (Vạn Nghĩa)
  226 (conference audio) .................    2 -> existing 8897 (GTC Tech, batch-1 mapped 226->GTC)
  427 An Vi Group (chemicals) ............    3 -> new NCU (this script)
  374 (Jabra/VoIP/Zoom, no catex store) ..    3 -> left NULL (no seller info)

Also corrects a batch-1 mis-attribution: catex seller UserId 427 is really "An Vi Group",
but batch 1 filed its 3 products (BCP50/BCP54/Eco-Sept, Bionetix) under a web-guessed
"TĐQ Việt Nam" (CungUngId 8896). This script creates the real An Vi Group NCU, moves all 6
of that seller's products (3 batch-1 + 3 batch-2) onto it, and retires the empty 8896.

Idempotent — NCUs guarded by FullName; links are set-based UPDATEs keyed on catex UserId.
*/

SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

USE TechExchangeNew;
GO

BEGIN TRANSACTION;

-- 1. Create the two new suppliers from catex Store data --------------------------------
INSERT INTO NhaCungUng
    (FullName, DiaChi, Phone, Email, LoaiHinhToChuc, ChucNangChinh, Website,
     StatusId, IsActivated, LanguageId, Domain, SiteId, Created, Modified, CreatedBy)
SELECT v.FullName, v.DiaChi, v.Phone, v.Email, v.LoaiHinhToChuc, v.ChucNangChinh, v.Website,
       3, 1, 1, 'VN', 1, GETDATE(), GETDATE(), 'catex-import'
FROM (VALUES
    (N'CÔNG TY TNHH BETA TECHNOLOGY',
     N'Số nhà 17, Đường số 12, Khu dân cư Cityland Park Hills, Phường 10, Quận Gò Vấp, TP Hồ Chí Minh',
     '0903042747', 'sales@betatechco.com', 'Khac',
     N'Đơn vị hàng đầu tại Việt Nam chuyên cung cấp thiết bị phân tích thí nghiệm cho các ngành dầu khí, vật liệu, thực phẩm, bò sữa…',
     'https://betatechco.com/'),
    (N'Công ty TNHH Đầu tư Phát triển An Vi',
     N'48A Dân Tộc, P. Tân Thành, Q. Tân Phú, TP Hồ Chí Minh',
     '02866518768', 'bichvi@anvigroup.com.vn', 'Khac',
     N'Đơn vị phân phối các loại hóa chất tại thị trường Việt Nam; chính sách giá tốt cho nhà thầu, công ty thương mại và End User.',
     'https://anvigroup.com.vn/')
) v(FullName, DiaChi, Phone, Email, LoaiHinhToChuc, ChucNangChinh, Website)
WHERE NOT EXISTS (SELECT 1 FROM NhaCungUng n WHERE n.FullName = v.FullName);

DECLARE @beta INT = (SELECT TOP 1 CungUngId FROM NhaCungUng WHERE FullName = N'CÔNG TY TNHH BETA TECHNOLOGY');
DECLARE @anvi INT = (SELECT TOP 1 CungUngId FROM NhaCungUng WHERE FullName = N'Công ty TNHH Đầu tư Phát triển An Vi');

-- 2. Link batch-2 products to their seller's NCU (join catex by approved name -> UserId) --
UPDATE s SET s.NCUId = CASE p.UserId
                          WHEN 403 THEN @beta
                          WHEN 427 THEN @anvi
                          WHEN 426 THEN 8898
                          WHEN 430 THEN 8893
                          WHEN 226 THEN 8897
                       END
FROM SanPhamCNTB s
JOIN [catex.vn].dbo.Product p
     ON LTRIM(RTRIM(p.TenSanPham)) = s.Name AND p.ApprovedStatus = 1
WHERE s.Creator = 'catex-import'
  AND CAST(s.Created AS date) = '2026-07-16'
  AND p.UserId IN (403, 427, 426, 430, 226);   -- 374 intentionally omitted (no seller info)

-- 3. Move batch-1's UserId-427 products off the mis-named TĐQ (8896) onto An Vi ----------
UPDATE s SET s.NCUId = @anvi, s.Modified = GETDATE(), s.Modifier = 'catex-import'
FROM SanPhamCNTB s
JOIN [catex.vn].dbo.Product p
     ON LTRIM(RTRIM(p.TenSanPham)) = s.Name AND p.ApprovedStatus = 1
WHERE s.Creator = 'catex-import'
  AND CAST(s.Created AS date) < '2026-07-16'
  AND p.UserId = 427;

-- 4. Retire the now-empty mis-attributed TĐQ Việt Nam NCU (8896) -------------------------
UPDATE NhaCungUng
SET IsActivated = 0, StatusId = 0, Modified = GETDATE(), Modifier = 'catex-import'
WHERE CungUngId = 8896
  AND NOT EXISTS (SELECT 1 FROM SanPhamCNTB WHERE NCUId = 8896);

-- Report -------------------------------------------------------------------------------
SELECT n.CungUngId, n.FullName, n.IsActivated, COUNT(s.ID) AS nProducts
FROM NhaCungUng n
LEFT JOIN SanPhamCNTB s ON s.NCUId = n.CungUngId
WHERE n.CreatedBy = 'catex-import'
GROUP BY n.CungUngId, n.FullName, n.IsActivated
ORDER BY n.CungUngId;

COMMIT TRANSACTION;
GO
