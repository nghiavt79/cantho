-- Nhập NCC thứ 10 đợt Triển lãm 3D: Công ty TNHH 3D Smart Solutions (12/11/2026).
-- Nguồn: CSDL Mau/SanphamCNTBMau/NHÀ CUNG ỨNG/3D Smart Solutions_Nhà cung ứng_Phiếu TT thông tin.docx
-- (phiếu khách cung cấp, sót ở đợt nhập 11/07/2026). Nội dung lấy nguyên văn từ phiếu.
-- Website để trống: phiếu ghi https://katec.vn/ (trùng KATEC), chờ khách xác nhận.
-- Dịch vụ KH&CN để trống: phiếu chọn "Khác" (đào tạo, chuyển giao công nghệ; nhân sự kỹ thuật theo dự án;
-- cho thuê chuyên gia kỹ thuật), không khớp danh mục của Sàn.
-- Gán số gian hàng A10 để hiện nút "Xem gian hàng triển lãm 3D".
-- Chạy lại được: kiểm tra theo mã số thuế; chạy sau gan-so-gian-hang-trien-lam-9-ncc.sql.
-- sqlcmd ... -f 65001 -b -I -i nhap-ncc-3d-smart-solutions.sql
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

IF COL_LENGTH('dbo.NhaCungUng', 'BoothNumber') IS NULL
BEGIN
    RAISERROR(N'Chưa có cột BoothNumber, chạy gan-so-gian-hang-trien-lam-9-ncc.sql trước.', 16, 1);
    RETURN;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungUng WHERE MaSoThue = N'0313566379' AND LanguageId = 1)
BEGIN
    INSERT INTO dbo.NhaCungUng
        (FullName, TenVietTat, QueryString, DiaChi, Phone, Email, Website, NguoiDaiDien, ChucVu,
         LinhVucId, LoaiHinhToChuc, MaSoThue, ChucNangChinh, SanPham, VideoUrl,
         StatusId, IsActivated, LanguageId, SiteId, Domain, Created, Modified, CreatedBy)
    VALUES
        (N'Công ty TNHH 3D Smart Solutions', N'3D SMART SOLUTIONS', 'cong-ty-tnhh-3d-smart-solutions',
         N'Số 58/6 Đường Võ Văn Hát, khu phố Phước Hiệp, phường Long Trường, TP. Hồ Chí Minh',
         N'028 888 99 039', N'info@3ds.vn', NULL,
         N'Trương Tấn Tòng', N'Chủ tịch Hội đồng thành viên kiêm Tổng Giám đốc',
         N'1078;5', N'Khac', N'0313566379',
         N'Được thành lập vào năm 2009, 3D Smart Solutions (3DS) là nhà tư vấn, cung cấp và chuyển giao công nghệ uy tín hàng đầu Việt Nam với các sản phẩm máy in 3D, máy quét 3D chất lượng cao và phần mềm Mastercam. Năm 2023, 3DS khánh thành công xưởng 3D lớn nhất Việt Nam có diện tích 1.200 mét vuông và nhiều hệ thống máy móc thiết bị hiện đại theo tiêu chuẩn quốc tế.',
         N'Những sản phẩm (thiết bị và phần mềm) Công ty đang cung cấp trong lĩnh vực công nghệ in 3D bao gồm: máy quét 3D; máy in 3D; phần mềm CAD | CAM | CAV. Dịch vụ công nghệ: đào tạo, chuyển giao công nghệ; dịch vụ nhân sự kỹ thuật theo dự án; dịch vụ cho thuê chuyên gia kỹ thuật on-site/remote.',
         N'https://www.youtube.com/watch?v=j2CDYWsbff0',
         3, 1, 1, 1, 'VN', GETDATE(), GETDATE(), 'phieu-mau-import');
    PRINT N'Đã tạo NCC 3D Smart Solutions.';
END
ELSE
    PRINT N'NCC 3D Smart Solutions đã có, không tạo lại.';

-- Số gian hàng A10 (không gán nếu A10 đã thuộc NCC khác)
IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungUng WHERE BoothNumber = N'A10' AND MaSoThue <> N'0313566379')
    UPDATE dbo.NhaCungUng SET
        BoothNumber = N'A10',
        PanoramaUrl = CASE WHEN ISNULL(PanoramaUrl, '') = '' THEN N'/images/panoramas/booth_default.png' ELSE PanoramaUrl END
    WHERE MaSoThue = N'0313566379' AND LanguageId = 1 AND ISNULL(BoothNumber, '') = '';
ELSE
    RAISERROR(N'Số gian hàng A10 đã thuộc NCC khác, chưa gán cho 3D Smart Solutions.', 10, 1);

SELECT CungUngId, BoothNumber, PanoramaUrl, QueryString, LinhVucId, StatusId, IsActivated, FullName
FROM dbo.NhaCungUng WHERE MaSoThue = N'0313566379';
