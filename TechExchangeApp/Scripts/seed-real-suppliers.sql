/*
Imports 7 real sample suppliers + 7 real sample products (3 Công nghệ + 4 Thiết bị)
from D:\2026\CanTho\CSDL Mau\SanphamCNTBMau (customer-provided sample data sheets).
Published directly (StatusId=3) since this is real customer-submitted content,
not demo/fictional data. MISA products link to the pre-existing MISA supplier
(CungUngId 8096) instead of creating a duplicate. Idempotent — guarded by name.
*/

SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Now DATETIME = GETDATE();

CREATE TABLE #NcuSeed (
    FullName NVARCHAR(200), DiaChi NVARCHAR(500), Phone NVARCHAR(50), Email NVARCHAR(50),
    NguoiDaiDien NVARCHAR(500), ChucVu NVARCHAR(50), LoaiHinhToChuc NVARCHAR(500),
    LinhVucId NVARCHAR(500), ChucNangChinh NVARCHAR(MAX), DichVu NVARCHAR(500), Website NVARCHAR(50)
);

INSERT INTO #NcuSeed VALUES (N'Công ty Cổ phần Machinex Việt Nam', N'163/8 Phạm Đăng Giảng, phường Bình Hưng Hoà, quận Bình Tân, TP. Hồ Chí Minh', '', '', N'', N'', 'DNSX', '5', N'Nhà phân phối thiết bị sấy công nghiệp (máy sấy bơm nhiệt Aobote, Trung Quốc) tại Việt Nam.', '', '');
INSERT INTO #NcuSeed VALUES (N'Công ty TNHH China Ecotek Việt Nam', N'', '0902669771', 'cevcsales@cevc.com.vn', N'Hung Chen Yu', N'Giám đốc', 'DNSX', '9;4', N'Nhà cung ứng máy lọc nước công nghiệp RO (thương hiệu Rotek, Đài Loan) và đèn LED nhà xưởng.', '', '');
INSERT INTO #NcuSeed VALUES (N'Công ty Cổ phần Công nghệ KATEC', N'189 Phan Huy Chú, phường Tân An, TP. Cần Thơ', '0889881010', 'katec.cantho@gmail.com', N'Trương Hoàng Khải', N'Giám đốc', 'DNKHCN', '1077;1078', N'Doanh nghiệp khoa học công nghệ tại Cần Thơ, chuyên sản xuất phần mềm quản lý (Kafood, KACRM, Kalink) và thiết kế website.', '', '');
INSERT INTO #NcuSeed VALUES (N'Công ty Cổ phần TPT', N'17A Lý Tự Trọng, phường An Phú, quận Ninh Kiều, TP. Cần Thơ', '0907837171', 'kinhdoanh@tptcantho.com.vn', N'', N'', 'Khac', '1078;4', N'Trên 20 năm kinh doanh máy vi tính, thiết bị văn phòng, thiết bị điện và camera quan sát tại Cần Thơ.', '', 'https://tptcantho.com.vn/');
INSERT INTO #NcuSeed VALUES (N'Công ty Cổ phần Công nghệ Aquadelta', N'Tòa nhà Vạn Đạt, Lô II-1, Đường số 8, KCN Tân Bình, quận Tân Phú, TP. Hồ Chí Minh', '0939154554', 'info@aquadelta.vn', N'Lê Thị Thanh Thùy', N'Giám đốc', 'DNKHCN', '1080;8;6', N'Nhà phân phối hóa chất thí nghiệm, dụng cụ/thiết bị phòng Lab và hóa chất công nghiệp tại khu vực miền Nam.', '25', 'https://aquadelta.vn/');
INSERT INTO #NcuSeed VALUES (N'Công ty CP XNK Hóa chất và Thiết bị Kim Ngưu', N'VPGD chi nhánh Cần Thơ: Số 50 Lý Thái Tổ, phường Hưng Phú, TP. Cần Thơ', '0901081154', 'sales@hoachat.com.vn', N'Nguyễn Xuân Hải', N'Giám đốc', 'DNSX', '1080;8;6', N'Cung cấp hóa chất công nghiệp, hóa chất thí nghiệm, dụng cụ và thiết bị khoa học kỹ thuật.', '', '');
INSERT INTO #NcuSeed VALUES (N'Công ty TNHH Phúc Trường Hải', N'KCN Thạnh Phú, phường Tân Triều, TP. Đồng Nai, Việt Nam', '0906766812', 'linh.nguyen@phuctruonghai.vn', N'', N'', 'DNSX', '9;12;5', N'Chuyên thiết kế, chế tạo và lắp đặt lò hơi công nghiệp (lò hơi tầng sôi đốt biomass).', '', 'https://phuctruonghai.vn/');

INSERT INTO NhaCungUng
    (FullName, DiaChi, Phone, Email, NguoiDaiDien, ChucVu, LoaiHinhToChuc, LinhVucId,
     ChucNangChinh, DichVu, Website, StatusId, IsActivated, LanguageId, Domain, SiteId, Created, Modified, CreatedBy)
SELECT
    s.FullName, s.DiaChi, s.Phone, s.Email, s.NguoiDaiDien, s.ChucVu, s.LoaiHinhToChuc, s.LinhVucId,
    s.ChucNangChinh, s.DichVu, s.Website, 3, 1, 1, 'VN', 1, @Now, @Now, 'sample-import'
FROM #NcuSeed s
WHERE NOT EXISTS (SELECT 1 FROM NhaCungUng n WHERE n.FullName = s.FullName);

DROP TABLE #NcuSeed;
GO
