/* ============================================================================
   Backfill GiaBanDuKien cho thiết bị/công nghệ import từ catex store.
   Lý do: seed-catex-storeproduct-full.sql KHÔNG map cột giá, nên toàn bộ
   sản phẩm Creator='catex-store-import' để trống giá (hiển thị "Liên hệ báo giá").
   Nguồn giá: [catex.vn].dbo.StoreProduct.GiaBan (money).

   Quy tắc:
   - Chỉ đổ vào GiaBanDuKien (text), KHÔNG động SellPrice (theo quyết định 2026-07-29).
   - Chỉ update dòng đang TRỐNG GiaBanDuKien (không đè giá đã nhập tay).
   - Chỉ lấy nguồn GiaBan > 0; chọn bản ghi StoreProduct mới nhất theo Id cho mỗi tên.
   - Idempotent: chạy lại không đổi (dòng đã có giá bị loại bởi điều kiện trống).

   PHỤ THUỘC: linked DB [catex.vn] phải truy cập được khi chạy.
   ============================================================================ */

SET NOCOUNT ON;

;WITH srcp AS (
    SELECT
        LTRIM(RTRIM(TenSanPham)) AS Nm,
        GiaBan,
        ROW_NUMBER() OVER (PARTITION BY LTRIM(RTRIM(TenSanPham)) ORDER BY Id DESC) AS rn
    FROM [catex.vn].dbo.StoreProduct
    WHERE ApprovedStatus = 1
      AND HinhAnh LIKE '~/%'
      AND GiaBan > 0
)
UPDATE p
-- NCHAR(272) = 'Đ' (U+0110): dùng NCHAR thay literal để KHÔNG hỏng khi sqlcmd đọc file UTF-8 sai codepage
-- (literal N' VNĐ' từng bị lưu thành 'VNÄ' + ký tự điều khiển).
SET p.GiaBanDuKien = FORMAT(CAST(srcp.GiaBan AS bigint), 'N0', 'vi-VN') + N' VN' + NCHAR(272),
    p.Modified = GETDATE()
FROM SanPhamCNTB p
JOIN srcp ON srcp.Nm = p.Name AND srcp.rn = 1
WHERE p.Creator = 'catex-store-import'
  AND (p.GiaBanDuKien IS NULL OR LTRIM(RTRIM(p.GiaBanDuKien)) = '');

PRINT CONCAT('Updated GiaBanDuKien rows: ', @@ROWCOUNT);

/* Kiểm tra nhanh sau khi chạy */
SELECT COUNT(*) AS Con_trong_gia
FROM SanPhamCNTB
WHERE Creator = 'catex-store-import'
  AND (GiaBanDuKien IS NULL OR LTRIM(RTRIM(GiaBanDuKien)) = '');
