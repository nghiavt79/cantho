/*
Imports 10 real intellectual property assets (sáng chế/giải pháp hữu ích) tied
to Cần Thơ, sourced from ipvietnam.gov.vn's official "Sáng chế - Giải pháp
hữu ích của thành phố Cần Thơ" gazette listing and CTU's own news releases.
8 items are owned by Trường Đại học Cần Thơ (new NhaCungUng record created
here); 2 are owned by named individual inventors (OwnerType=2). Application
numbers ("1-20xx-xxxxx"/"2-20xx-xxxxx") are as published in the Công báo Sở
hữu công nghiệp; only the 2 most recent 2026 items have a confirmed granted
patent number (SoBang). Published directly (StatusId=3) — real, verifiable
public-record content. Idempotent — guarded by Name + ProductType.
*/

SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Now DATETIME = GETDATE();

-- Supplier: Trường Đại học Cần Thơ (owner of 8 of the 10 items)
IF NOT EXISTS (SELECT 1 FROM NhaCungUng WHERE FullName = N'Trường Đại học Cần Thơ')
BEGIN
    INSERT INTO NhaCungUng
        (FullName, DiaChi, Phone, Email, LoaiHinhToChuc, ChucNangChinh, Website,
         StatusId, IsActivated, LanguageId, Domain, SiteId, Created, Modified, CreatedBy)
    VALUES
        (N'Trường Đại học Cần Thơ', N'Khu II, đường 3/2, phường Xuân Khánh, quận Ninh Kiều, TP. Cần Thơ',
         '02923838474', 'dhct@ctu.edu.vn', 'DNKHCN',
         N'Trường đại học đa ngành trọng điểm vùng Đồng bằng sông Cửu Long, đơn vị sở hữu nhiều bằng sáng chế/giải pháp hữu ích nhất tại Cần Thơ.',
         'https://www.ctu.edu.vn/', 3, 1, 1, 'VN', 1, @Now, @Now, 'ip-asset-import');
END

DECLARE @CTU INT = (SELECT TOP 1 CungUngId FROM NhaCungUng WHERE FullName = N'Trường Đại học Cần Thơ');

CREATE TABLE #TriTueSeed (
    Name NVARCHAR(500), OwnerType INT, NCUId INT, HoTen NVARCHAR(500),
    ApplicationNumber NVARCHAR(100), SoBang NVARCHAR(100), CategoryId NVARCHAR(100),
    MoTa NVARCHAR(MAX), DevelopmentStageValue INT
);

INSERT INTO #TriTueSeed VALUES (N'Máy tách cuống ớt', 1, @CTU, NULL, '1-2019-04725', NULL, '5',
    N'Sáng chế cơ khí nông nghiệp của Trường Đại học Cần Thơ, đơn đăng ký số 1-2019-04725, công bố trên Công báo Sở hữu công nghiệp (Cục Sở hữu trí tuệ). Thiết bị giúp tách cuống ớt tự động, giảm công lao động thủ công trong sơ chế nông sản.', 3);
INSERT INTO #TriTueSeed VALUES (N'Thiết bị tước chỉ xơ dừa', 1, @CTU, NULL, '1-2017-04842', NULL, '5',
    N'Sáng chế cơ khí chế biến nông sản của Trường Đại học Cần Thơ, đơn đăng ký số 1-2017-04842, công bố trên Công báo Sở hữu công nghiệp. Thiết bị tách chỉ xơ từ vỏ dừa phục vụ ngành chế biến sản phẩm từ dừa.', 3);
INSERT INTO #TriTueSeed VALUES (N'Máy cán vỏ dừa', 1, @CTU, NULL, '1-2017-04153', NULL, '5',
    N'Sáng chế cơ khí chế biến nông sản của Trường Đại học Cần Thơ, đơn đăng ký số 1-2017-04153, công bố trên Công báo Sở hữu công nghiệp. Máy cán, xử lý vỏ dừa phục vụ sơ chế nguyên liệu.', 3);
INSERT INTO #TriTueSeed VALUES (N'Quy trình xử lý nước cấp sinh hoạt bằng plasma lạnh', 1, @CTU, NULL, '1-2018-04189', NULL, '9',
    N'Sáng chế công nghệ môi trường của Trường Đại học Cần Thơ, đơn đăng ký số 1-2018-04189, công bố trên Công báo Sở hữu công nghiệp. Quy trình ứng dụng công nghệ plasma lạnh để xử lý, khử trùng nước cấp sinh hoạt.', 2);
INSERT INTO #TriTueSeed VALUES (N'Quy trình chế biến nước ép gấc - cà rốt', 1, @CTU, NULL, '1-2017-01652', NULL, '12',
    N'Sáng chế công nghệ thực phẩm của Trường Đại học Cần Thơ, đơn đăng ký số 1-2017-01652, công bố trên Công báo Sở hữu công nghiệp. Quy trình chế biến nước ép kết hợp gấc và cà rốt, tận dụng nguồn nguyên liệu giàu beta-carotene tại địa phương.', 3);
INSERT INTO #TriTueSeed VALUES (N'Bộ điều khiển IoT dùng cho thiết bị lọc nước', 1, @CTU, NULL, '2-2019-00005', NULL, '4',
    N'Giải pháp hữu ích công nghệ IoT của Trường Đại học Cần Thơ, đơn đăng ký số 2-2019-00005, công bố trên Công báo Sở hữu công nghiệp. Bộ điều khiển thông minh giám sát và vận hành từ xa cho thiết bị lọc nước.', 2);
INSERT INTO #TriTueSeed VALUES (N'Phương pháp mô phỏng liên kết phân tử Xystein trên bề mặt kim loại bạc', 1, @CTU, NULL, NULL, '54675', '11;6',
    N'Bằng độc quyền Sáng chế số 54675, do nhóm tác giả PGS.TS Nguyễn Thành Tiên, PGS.TS Phạm Vũ Nhật, TS Phạm Thị Bích Thảo (Trường Khoa học Tự nhiên) và PGS.TS Đặng Minh Triết (Trường Sư phạm) thực hiện. Phương pháp mô phỏng liên kết phân tử Cystein trên bề mặt kim loại bạc, mở ra triển vọng ứng dụng trong vật liệu nano, hóa học tính toán và công nghệ sinh học.', 2);
INSERT INTO #TriTueSeed VALUES (N'Quy trình nuôi tôm thẻ chân trắng siêu thâm canh trong hệ thống tuần hoàn kết hợp đa loài', 1, @CTU, NULL, NULL, 'GPHI 4743', '18',
    N'Bằng độc quyền Giải pháp hữu ích số 4743, do nhóm nghiên cứu GS.TS Trần Ngọc Hải (Trường Thủy sản, Đại học Cần Thơ) thực hiện. Quy trình nuôi tôm thẻ chân trắng (Litopenaeus vannamei) mật độ siêu thâm canh trong hệ thống tuần hoàn nước kết hợp nuôi đa loài, giúp nâng cao hiệu quả sản xuất, tối ưu nguồn lực và phát triển bền vững nghề nuôi tôm ở Đồng bằng sông Cửu Long.', 3);
INSERT INTO #TriTueSeed VALUES (N'Phương pháp sản xuất giống tôm càng xanh', 2, NULL, N'Lương Thị Bảo Thanh', '1-2010-01458', NULL, '18',
    N'Sáng chế của cá nhân tác giả Lương Thị Bảo Thanh, đơn đăng ký số 1-2010-01458, công bố trên Công báo Sở hữu công nghiệp (Cục Sở hữu trí tuệ). Phương pháp sản xuất, ương giống tôm càng xanh phục vụ nuôi trồng thủy sản tại Cần Thơ.', 3);
INSERT INTO #TriTueSeed VALUES (N'Thiết bị đo kích thước ô tô tự động, không tiếp xúc', 2, NULL, N'Châu Ngọc Ý', '1-2019-03118', NULL, '4',
    N'Sáng chế của cá nhân tác giả Châu Ngọc Ý, đơn đăng ký số 1-2019-03118, công bố trên Công báo Sở hữu công nghiệp. Thiết bị đo kích thước xe ô tô tự động bằng phương pháp không tiếp xúc, ứng dụng trong kiểm định, giám sát giao thông.', 2);

DECLARE @BaseId INT = (SELECT ISNULL(MAX(ID),0)+1 FROM SanPhamCNTB);

INSERT INTO SanPhamCNTB
    (Name, ProductType, LoaiDeTai, OwnerType, NCUId, HoTen, ApplicationNumber, SoBang, CategoryId,
     MoTa, DevelopmentStageValue, XuatXu,
     Code, StatusId, LanguageId, SiteId, Created, Modified, Creator)
SELECT
    s.Name, 3, 1, s.OwnerType, s.NCUId, s.HoTen, s.ApplicationNumber, s.SoBang, s.CategoryId,
    s.MoTa, s.DevelopmentStageValue, N'Cần Thơ, Việt Nam',
    'TT-' + RIGHT('00000' + CAST(@BaseId + ROW_NUMBER() OVER (ORDER BY s.Name) - 1 AS VARCHAR(5)), 5),
    3, 1, 1, @Now, @Now, 'ip-asset-import'
FROM #TriTueSeed s
WHERE NOT EXISTS (SELECT 1 FROM SanPhamCNTB p WHERE p.Name = s.Name AND p.ProductType = 3);

SELECT ID, Name, OwnerType, NCUId, HoTen, ApplicationNumber, SoBang FROM SanPhamCNTB WHERE ProductType = 3 ORDER BY ID;

DROP TABLE #TriTueSeed;
GO
