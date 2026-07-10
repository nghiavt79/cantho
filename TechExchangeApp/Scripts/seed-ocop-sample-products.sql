/*
Seeds 20 sample OCOP products (ProductType = 4) for the "Góc trưng bày Sản phẩm OCOP
& Truy xuất nguồn gốc" corner. Names/locations/star ratings are based on real Cần Thơ
OCOP products and districts (Báo Cần Thơ, cổng thông tin Cần Thơ) mixed with typical
regional specialties, used here as realistic placeholder/demo data — replace or edit
via CMS once real producer submissions are available.
Not idempotent by primary key (SanPhamCNTB.ID is IDENTITY); guarded by MaTruyXuat so
re-running the script does not duplicate rows.
*/

DECLARE @Now DATETIME = GETDATE();

CREATE TABLE #OcopSeed (
    MaTruyXuat NVARCHAR(50),
    Name NVARCHAR(255),
    QueryString NVARCHAR(255),
    XuatXu NVARCHAR(255),
    SoSaoOCOP INT,
    MoTaNgan NVARCHAR(500)
);

INSERT INTO #OcopSeed (MaTruyXuat, Name, QueryString, XuatXu, SoSaoOCOP, MoTaNgan) VALUES
('OCOP-CT-001', N'Trà mãng cầu Long Giang', 'tra-mang-cau-long-giang', N'Ô Môn, Cần Thơ', 5, N'Trà mãng cầu nguyên chất của Công ty TNHH SumoFood Việt Nam, đạt chứng nhận OCOP 5 sao.'),
('OCOP-CT-002', N'Snack da cá Vidaca', 'snack-da-ca-vidaca', N'Ô Môn, Cần Thơ', 5, N'Snack da cá giòn tan của Công ty CP Thực phẩm Vidaca, đạt chứng nhận OCOP 5 sao.'),
('OCOP-CT-003', N'Snack da cá vị trứng muối Vidaca', 'snack-da-ca-vi-trung-muoi-vidaca', N'Ô Môn, Cần Thơ', 5, N'Snack da cá vị trứng muối của Công ty CP Thực phẩm Vidaca, đạt chứng nhận OCOP 5 sao.'),
('OCOP-CT-004', N'Bột gạo lứt mè đen Thuận Hòa', 'bot-gao-lut-me-den-thuan-hoa', N'Cái Răng, Cần Thơ', 4, N'Bột gạo lứt mè đen dinh dưỡng của Cơ sở sản xuất Thuận Hòa, đạt chứng nhận OCOP 4 sao.'),
('OCOP-CT-005', N'Trà gạo lứt đậu đỏ Thuận Hòa', 'tra-gao-lut-dau-do-thuan-hoa', N'Cái Răng, Cần Thơ', 4, N'Trà gạo lứt đậu đỏ hỗ trợ sức khỏe của Cơ sở sản xuất Thuận Hòa, đạt chứng nhận OCOP 4 sao.'),
('OCOP-CT-006', N'Trà 5 thứ đậu Thuận Hòa', 'tra-5-thu-dau-thuan-hoa', N'Cái Răng, Cần Thơ', 4, N'Trà ngũ cốc từ 5 loại đậu của Cơ sở sản xuất Thuận Hòa, đạt chứng nhận OCOP 4 sao.'),
('OCOP-CT-007', N'Trà đậu đen xanh lòng Thuận Hòa', 'tra-dau-den-xanh-long-thuan-hoa', N'Cái Răng, Cần Thơ', 4, N'Trà đậu đen xanh lòng rang thơm của Cơ sở sản xuất Thuận Hòa, đạt chứng nhận OCOP 4 sao.'),
('OCOP-CT-008', N'Trà hòa tan đinh lăng Hygie', 'tra-hoa-tan-dinh-lang-hygie', N'Ninh Kiều, Cần Thơ', 4, N'Trà hòa tan từ đinh lăng của Công ty TNHH MTV Hygie & Panacee, đạt chứng nhận OCOP 4 sao.'),
('OCOP-CT-009', N'Trà hòa tan tía tô Hygie', 'tra-hoa-tan-tia-to-hygie', N'Ninh Kiều, Cần Thơ', 4, N'Trà hòa tan từ lá tía tô của Công ty TNHH MTV Hygie & Panacee, đạt chứng nhận OCOP 4 sao.'),
('OCOP-CT-010', N'Nhãn Idol Tân Lộc', 'nhan-idol-tan-loc', N'Thốt Nốt, Cần Thơ', 3, N'Nhãn Idol tươi của HTX trái cây Tân Lộc, đạt chứng nhận OCOP 3 sao.'),
('OCOP-CT-011', N'Mắm cá lóc Út Anh', 'mam-ca-loc-ut-anh', N'Thốt Nốt, Cần Thơ', 3, N'Mắm cá lóc truyền thống của Cơ sở Út Anh, đạt chứng nhận OCOP 3 sao.'),
('OCOP-CT-012', N'Khô cá lóc một nắng Thốt Nốt', 'kho-ca-loc-mot-nang-thot-not', N'Thốt Nốt, Cần Thơ', 3, N'Khô cá lóc một nắng đặc sản Thốt Nốt, đạt chứng nhận OCOP 3 sao.'),
('OCOP-CT-013', N'Rượu mận lên men Thốt Nốt', 'ruou-man-len-men-thot-not', N'Thốt Nốt, Cần Thơ', 3, N'Rượu lên men từ mận địa phương, đạt chứng nhận OCOP 3 sao.'),
('OCOP-CT-014', N'Nước ép ổi lên men Thốt Nốt', 'nuoc-ep-oi-len-men-thot-not', N'Thốt Nốt, Cần Thơ', 3, N'Nước ép ổi lên men tự nhiên, đạt chứng nhận OCOP 3 sao.'),
('OCOP-CT-015', N'Bánh tráng ngọt Thới Lai', 'banh-trang-ngot-thoi-lai', N'Thới Lai, Cần Thơ', 3, N'Bánh tráng ngọt thủ công của làng nghề Thới Lai, đạt chứng nhận OCOP 3 sao.'),
('OCOP-CT-016', N'Gạo sạch Thới Lai', 'gao-sach-thoi-lai', N'Thới Lai, Cần Thơ', 3, N'Gạo sạch canh tác theo tiêu chuẩn VietGAP của HTX Nông nghiệp Thới Lai, đạt chứng nhận OCOP 3 sao.'),
('OCOP-CT-017', N'Mật ong hoa nhãn Phong Điền', 'mat-ong-hoa-nhan-phong-dien', N'Phong Điền, Cần Thơ', 4, N'Mật ong nguyên chất từ vườn nhãn Phong Điền, đạt chứng nhận OCOP 4 sao.'),
('OCOP-CT-018', N'Dâu Hạ Châu Phong Điền', 'dau-ha-chau-phong-dien', N'Phong Điền, Cần Thơ', 4, N'Dâu Hạ Châu — đặc sản trái cây nổi tiếng của Phong Điền, đạt chứng nhận OCOP 4 sao.'),
('OCOP-CT-019', N'Bánh tét lá cẩm Cần Thơ', 'banh-tet-la-cam-can-tho', N'Bình Thủy, Cần Thơ', 4, N'Bánh tét lá cẩm truyền thống, đạt chứng nhận OCOP 4 sao.'),
('OCOP-CT-020', N'Bún khô Nhà Bè', 'bun-kho-nha-be', N'Cái Răng, Cần Thơ', 3, N'Bún khô thủ công của Cơ sở Nhà Bè, chợ nổi Cái Răng, đạt chứng nhận OCOP 3 sao.');

INSERT INTO SanPhamCNTB
    (Code, Name, QueryString, XuatXu, MoTaNgan, ProductType, StatusId, LanguageId, SiteId,
     MaTruyXuat, SoSaoOCOP, Created, Modified, PublishedDate)
SELECT
    s.MaTruyXuat, s.Name, s.QueryString, s.XuatXu, s.MoTaNgan, 4, 3, 1, 1,
    s.MaTruyXuat, s.SoSaoOCOP, @Now, @Now, @Now
FROM #OcopSeed s
WHERE NOT EXISTS (SELECT 1 FROM SanPhamCNTB p WHERE p.MaTruyXuat = s.MaTruyXuat);

DROP TABLE #OcopSeed;
GO
