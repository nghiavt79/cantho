-- Gán số gian hàng Triển lãm 3D (/trienlam) cho 9 NCC đợt này (12/11/2026).
-- NCC có BoothNumber = đã thiết lập gian hàng; trang NCC/sản phẩm trên Sàn hiện nút "Xem gian hàng triển lãm 3D".
-- A01–A03 đã thiết lập sẵn trên trang quản trị triển lãm (giữ nguyên); script gán A04–A09 cho 6 NCC còn lại.
-- Chạy lại được: chỉ gán khi NCC chưa có số gian hàng, không đổi số đã có.
-- sqlcmd ... -f 65001 -b -I -i gan-so-gian-hang-trien-lam-9-ncc.sql
SET NOCOUNT ON;

-- Máy chủ đã có các cột này (do ứng dụng triển lãm thêm); chỉ thêm khi chạy trên CSDL cũ/local.
IF COL_LENGTH('dbo.NhaCungUng', 'BoothNumber') IS NULL
    ALTER TABLE dbo.NhaCungUng ADD BoothNumber nvarchar(20) NULL;
IF COL_LENGTH('dbo.NhaCungUng', 'PanoramaUrl') IS NULL
    ALTER TABLE dbo.NhaCungUng ADD PanoramaUrl nvarchar(500) NULL;
GO

DECLARE @g TABLE (CungUngId int PRIMARY KEY, BoothNumber nvarchar(20));
INSERT @g VALUES
    (8904, N'A01'),  -- Công ty Cổ phần Công nghệ Cát Sạch MeKong (đã có)
    (8096, N'A02'),  -- Công ty Cổ phần MISA (đã có)
    (8887, N'A03'),  -- Công ty TNHH China Ecotek Việt Nam (đã có)
    (7225, N'A04'),  -- Công ty Cổ phần Machinex Việt Nam
    (8888, N'A05'),  -- Công ty Cổ phần Công nghệ KATEC
    (8889, N'A06'),  -- Công ty Cổ phần TPT
    (8890, N'A07'),  -- Công ty Cổ phần Công nghệ Aquadelta
    (8891, N'A08'),  -- Công ty CP XNK Hóa chất và Thiết bị Kim Ngưu
    (8892, N'A09');  -- Công ty TNHH Phúc Trường Hải

-- Không gán trùng số đã dùng cho NCC khác
IF EXISTS (SELECT 1 FROM dbo.NhaCungUng n JOIN @g g ON g.BoothNumber = n.BoothNumber AND g.CungUngId <> n.CungUngId)
BEGIN
    SELECT n.CungUngId, n.FullName, n.BoothNumber AS SoDaDung
    FROM dbo.NhaCungUng n JOIN @g g ON g.BoothNumber = n.BoothNumber AND g.CungUngId <> n.CungUngId;
    RAISERROR(N'Số gian hàng đã được dùng cho NCC khác, dừng script.', 16, 1);
    RETURN;
END

UPDATE n SET
    n.BoothNumber = g.BoothNumber,
    n.PanoramaUrl = CASE WHEN ISNULL(n.PanoramaUrl, '') = '' THEN N'/images/panoramas/booth_default.png' ELSE n.PanoramaUrl END
FROM dbo.NhaCungUng n
JOIN @g g ON g.CungUngId = n.CungUngId
WHERE ISNULL(n.BoothNumber, '') = '';

PRINT CONCAT(N'Đã gán số gian hàng: ', @@ROWCOUNT, N' NCC');

SELECT n.CungUngId, n.BoothNumber, n.PanoramaUrl, n.IsActivated, n.StatusId, n.FullName
FROM dbo.NhaCungUng n
WHERE ISNULL(n.BoothNumber, '') <> ''
ORDER BY n.BoothNumber;
