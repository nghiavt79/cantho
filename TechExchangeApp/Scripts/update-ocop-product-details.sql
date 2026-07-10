/*
Fills in producer/address/description/certification detail for the 20 sample OCOP
products seeded by seed-ocop-sample-products.sql, so the OCOP detail page has real
content to render. Keyed by MaTruyXuat (unique), safe to re-run.
Run with: sqlcmd ... -f 65001 -i update-ocop-product-details.sql  (UTF-8 codepage,
required for correct Vietnamese diacritics).
*/

DECLARE @Now DATETIME = GETDATE();

CREATE TABLE #OcopDetail (
    MaTruyXuat NVARCHAR(50),
    HoTen NVARCHAR(255),
    DiaChi NVARCHAR(500),
    MoTa NVARCHAR(MAX),
    ChungNhanKhacText NVARCHAR(255)
);

INSERT INTO #OcopDetail (MaTruyXuat, HoTen, DiaChi, MoTa, ChungNhanKhacText) VALUES
('OCOP-CT-001', N'Công ty TNHH SumoFood Việt Nam', N'Khu vực Thới An, phường Ô Môn, TP. Cần Thơ',
 N'Trà mãng cầu Long Giang được chế biến từ mãng cầu xiêm trồng tại vùng đất phù sa Ô Môn, sấy lạnh giữ trọn hương vị và dưỡng chất tự nhiên. Sản phẩm không dùng chất bảo quản, phù hợp pha uống hằng ngày, hỗ trợ thanh nhiệt và tăng đề kháng.',
 N'OCOP 5 sao cấp Quốc gia'),
('OCOP-CT-002', N'Công ty CP Thực phẩm Vidaca', N'Khu công nghiệp Trà Nóc, quận Ô Môn, TP. Cần Thơ',
 N'Snack da cá được làm từ da cá tra tươi, tẩm ướp gia vị và chiên giòn theo công nghệ khép kín, đạt tiêu chuẩn an toàn thực phẩm xuất khẩu. Sản phẩm giòn rụm, ít dầu, không dùng phẩm màu công nghiệp.',
 N'OCOP 5 sao cấp Quốc gia'),
('OCOP-CT-003', N'Công ty CP Thực phẩm Vidaca', N'Khu công nghiệp Trà Nóc, quận Ô Môn, TP. Cần Thơ',
 N'Phiên bản vị trứng muối của snack da cá Vidaca, kết hợp lớp phủ trứng muối béo bùi với da cá giòn tan, được thị trường trong và ngoài nước ưa chuộng.',
 N'OCOP 5 sao cấp Quốc gia'),
('OCOP-CT-004', N'Cơ sở sản xuất Thuận Hòa', N'Phường Ba Láng, quận Cái Răng, TP. Cần Thơ',
 N'Bột gạo lứt mè đen rang xay mịn từ gạo lứt và mè đen nguyên hạt, không chất tạo ngọt, dùng pha uống hoặc nấu ăn dặm, hỗ trợ tiêu hóa và bổ sung chất xơ.',
 N'OCOP 4 sao'),
('OCOP-CT-005', N'Cơ sở sản xuất Thuận Hòa', N'Phường Ba Láng, quận Cái Răng, TP. Cần Thơ',
 N'Trà túi lọc từ gạo lứt rang và đậu đỏ, vị thơm bùi tự nhiên, tiện lợi pha nhanh, được nhiều gia đình lựa chọn làm thức uống hằng ngày thay trà truyền thống.',
 N'OCOP 4 sao'),
('OCOP-CT-006', N'Cơ sở sản xuất Thuận Hòa', N'Phường Ba Láng, quận Cái Răng, TP. Cần Thơ',
 N'Trà ngũ cốc phối trộn từ 5 loại đậu rang (đậu đen, đậu đỏ, đậu xanh, đậu trắng, đậu nành), hương vị đậm đà, không đường, phù hợp người ăn kiêng.',
 N'OCOP 4 sao'),
('OCOP-CT-007', N'Cơ sở sản xuất Thuận Hòa', N'Phường Ba Láng, quận Cái Răng, TP. Cần Thơ',
 N'Trà đậu đen xanh lòng rang thủ công theo bí quyết gia truyền, giữ nguyên hương thơm đặc trưng, hỗ trợ thanh lọc cơ thể.',
 N'OCOP 4 sao'),
('OCOP-CT-008', N'Công ty TNHH MTV Hygie & Panacee', N'Phường An Khánh, quận Ninh Kiều, TP. Cần Thơ',
 N'Trà hòa tan chiết xuất từ củ đinh lăng, dạng cốm hòa tan tiện dùng, hỗ trợ bồi bổ cơ thể, tăng cường sức đề kháng.',
 N'OCOP 4 sao'),
('OCOP-CT-009', N'Công ty TNHH MTV Hygie & Panacee', N'Phường An Khánh, quận Ninh Kiều, TP. Cần Thơ',
 N'Trà hòa tan chiết xuất từ lá tía tô, vị thanh nhẹ, hỗ trợ giải cảm và tăng cường sức đề kháng, tiện lợi mang theo.',
 N'OCOP 4 sao'),
('OCOP-CT-010', N'HTX Trái cây Tân Lộc', N'Cù lao Tân Lộc, quận Thốt Nốt, TP. Cần Thơ',
 N'Nhãn Idol (nhãn Ido) trồng theo hướng VietGAP tại cù lao Tân Lộc — vùng chuyên canh nhãn nổi tiếng của Cần Thơ, quả to, cơm dày, hạt nhỏ, vị ngọt thanh.',
 N'OCOP 3 sao'),
('OCOP-CT-011', N'Cơ sở Út Anh', N'Phường Tân Lộc, quận Thốt Nốt, TP. Cần Thơ',
 N'Mắm cá lóc ủ chượp theo công thức truyền thống miền Tây, cá lóc đồng tươi, mắm chín đều, đậm đà, dùng chưng hoặc pha nước mắm me.',
 N'OCOP 3 sao'),
('OCOP-CT-012', N'Cơ sở sản xuất khô Thốt Nốt', N'Quận Thốt Nốt, TP. Cần Thơ',
 N'Khô cá lóc một nắng phơi tự nhiên, giữ độ dẻo và vị ngọt của cá đồng, chỉ ướp muối và gia vị cơ bản, không chất bảo quản.',
 N'OCOP 3 sao'),
('OCOP-CT-013', N'Cơ sở sản xuất Thốt Nốt', N'Quận Thốt Nốt, TP. Cần Thơ',
 N'Rượu lên men từ mận địa phương theo phương pháp thủ công, vị chua ngọt hài hòa, nồng độ nhẹ, dùng khai vị.',
 N'OCOP 3 sao'),
('OCOP-CT-014', N'Cơ sở sản xuất Thốt Nốt', N'Quận Thốt Nốt, TP. Cần Thơ',
 N'Nước ép ổi lên men tự nhiên, không đường hóa học, giữ vị chua thanh và hương ổi đặc trưng, hỗ trợ tiêu hóa.',
 N'OCOP 3 sao'),
('OCOP-CT-015', N'Làng nghề bánh tráng Thới Lai', N'Huyện Thới Lai, TP. Cần Thơ',
 N'Bánh tráng ngọt tráng tay từ bột gạo và nước cốt dừa, phơi nắng tự nhiên, vị béo ngọt dịu, đặc sản lâu đời của làng nghề Thới Lai.',
 N'OCOP 3 sao'),
('OCOP-CT-016', N'HTX Nông nghiệp Thới Lai', N'Huyện Thới Lai, TP. Cần Thơ',
 N'Gạo sạch canh tác theo tiêu chuẩn VietGAP trên vùng chuyên canh lúa Thới Lai, hạt gạo trắng trong, cơm dẻo thơm tự nhiên, không dư lượng thuốc bảo vệ thực vật.',
 N'OCOP 3 sao, VietGAP'),
('OCOP-CT-017', N'Tổ hợp tác nuôi ong Phong Điền', N'Huyện Phong Điền, TP. Cần Thơ',
 N'Mật ong nguyên chất thu hoạch từ vườn nhãn Phong Điền vào mùa hoa nở, không pha tạp, sánh mịn, vị ngọt thanh đặc trưng của mật ong hoa nhãn miền Tây.',
 N'OCOP 4 sao'),
('OCOP-CT-018', N'HTX Dâu Hạ Châu Phong Điền', N'Huyện Phong Điền, TP. Cần Thơ',
 N'Dâu Hạ Châu — giống dâu đặc sản chỉ trồng được ở Phong Điền, quả vàng óng, vị ngọt thanh không chua, được xem là "đệ nhất dâu" của vùng đất Chín Rồng.',
 N'OCOP 4 sao'),
('OCOP-CT-019', N'Cơ sở bánh tét Chín Cẩm', N'Quận Bình Thủy, TP. Cần Thơ',
 N'Bánh tét lá cẩm gói thủ công từ nếp dẻo nhuộm màu tím tự nhiên từ lá cẩm, nhân thịt mỡ, đậu xanh, giữ hương vị Tết truyền thống của người Cần Thơ.',
 N'OCOP 4 sao'),
('OCOP-CT-020', N'Cơ sở Nhà Bè', N'Chợ nổi Cái Răng, quận Cái Răng, TP. Cần Thơ',
 N'Bún khô làm từ gạo, phơi nắng tự nhiên theo bí quyết gia truyền nhiều đời tại khu vực chợ nổi Cái Răng, sợi bún dai, nấu nhanh mềm, không chất tẩy trắng.',
 N'OCOP 3 sao');

UPDATE p
SET p.HoTen = d.HoTen,
    p.DiaChi = d.DiaChi,
    p.MoTa = d.MoTa,
    p.MoTaNgan = ISNULL(p.MoTaNgan, LEFT(d.MoTa, 150)),
    p.ChungNhanKhac = 1,
    p.ChungNhanKhacText = d.ChungNhanKhacText,
    p.Modified = @Now
FROM SanPhamCNTB p
JOIN #OcopDetail d ON d.MaTruyXuat = p.MaTruyXuat
WHERE p.ProductType = 4;

DROP TABLE #OcopDetail;
GO
