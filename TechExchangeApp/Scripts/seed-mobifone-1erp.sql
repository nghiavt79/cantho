/*
    Seed: MobiFone supplier + "Quản trị doanh nghiệp - Smart Sale for SME (1ERP)" product.
    Source: https://solutions.mobifone.vn/vi/quan-tri-doanh-nghiep-smart-sale-for-sme-1erp

    Safe to re-run: checks for existing rows by FullName / QueryString before inserting,
    so running this twice (or on a DB that already has MobiFone) won't create duplicates.

    IMPORTANT — before running on production:
    1. Copy ALL 5 files below from this repo checkout to the production server's wwwroot,
       at the SAME relative path — these are the original image plus the 4 thumbnail sizes
       from AppSettings:ImageSizes (254-170, 108-84, 460-275, 400-220), generated the same
       way UploadController.UploadImage does (ResizeMode.Max + Lanczos3). Product listing
       pages / cards use the thumbnail sizes, not the original — skipping them means this
       product's image silently fails to show wherever a thumbnail size is requested.
         wwwroot/uploads/san-pham-mobifone/1erp-banner.jpg
         wwwroot/uploads/san-pham-mobifone/254-170-1erp-banner.jpg
         wwwroot/uploads/san-pham-mobifone/108-84-1erp-banner.jpg
         wwwroot/uploads/san-pham-mobifone/460-275-1erp-banner.jpg
         wwwroot/uploads/san-pham-mobifone/400-220-1erp-banner.jpg
    2. Both the new supplier and product are inserted with StatusId = 1 (Nháp/Draft) —
       they will NOT be publicly visible until an admin reviews and publishes them via
       /cms/SanPhamCNTB/CongNghe (product) and the supplier admin screen.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @NCUId INT;
DECLARE @SanPhamId INT;
DECLARE @QueryString NVARCHAR(500) = N'quan-tri-doanh-nghiep-smart-sale-for-sme-1erp';

-- 1) Nhà cung ứng: MobiFone (chỉ tạo nếu chưa có)
SELECT @NCUId = CungUngId FROM NhaCungUng WHERE FullName = N'Tổng công ty Viễn thông MobiFone';

IF @NCUId IS NULL
BEGIN
    INSERT INTO NhaCungUng
    (FullName, QueryString, DiaChi, Phone, Email, Website, LinhVucId, ChucNangChinh, DichVu,
     Domain, Created, CreatedBy, StatusId, LanguageId, SiteId, Keywords)
    VALUES
    (N'Tổng công ty Viễn thông MobiFone',
     N'tong-cong-ty-vien-thong-mobifone',
     N'Số 01 phố Phạm Văn Bạch, Phường Cầu Giấy, Hà Nội',
     N'18001290',
     N'contact-itc@mobifone.vn',
     N'https://solutions.mobifone.vn',
     N'4',
     N'Cung cấp giải pháp công nghệ số, viễn thông, phần mềm doanh nghiệp',
     N'Giải pháp số, nội dung số, hạ tầng số',
     N'VN',
     GETDATE(),
     N'system-import',
     1,
     1,
     1,
     N'MobiFone, viễn thông, giải pháp số, ERP, CRM');

    SET @NCUId = SCOPE_IDENTITY();
END

-- 2) Sản phẩm: Quản trị doanh nghiệp - Smart Sale for SME (1ERP) (chỉ tạo nếu chưa có)
SELECT @SanPhamId = ID FROM SanPhamCNTB WHERE QueryString = @QueryString;

IF @SanPhamId IS NULL
BEGIN
    DECLARE @NextId INT = ISNULL((SELECT MAX(ID) FROM SanPhamCNTB WHERE ProductType = 1), 0) + 1;
    DECLARE @Code NVARCHAR(50) = N'CN-' + CAST(@NextId AS NVARCHAR(20));

    INSERT INTO SanPhamCNTB
    (Code, Name, QueryString, QuyTrinhHinhAnh, IsYoutube,
     CategoryId, MoTa, ThongSo, UuDiem, MoTaNgan,
     OriginalPrice, SellPrice, Currency, GiaiThuong,
     NCUId, StatusId, ProductType, Rating, TRLLevel,
     TargetCustomer, BrochureUrl, Keywords,
     LanguageId, SiteId, Created, Creator)
    VALUES
    (@Code,
     N'Quản trị doanh nghiệp - Smart Sale for SME (1ERP)',
     @QueryString,
     N'/uploads/san-pham-mobifone/1erp-banner.jpg',
     0,
     N'4',
     N'Smart Sale for SME (1ERP) là một Nền tảng quản lý tài nguyên doanh nghiệp tích hợp, được thiết kế để hỗ trợ các doanh nghiệp quản lý và tối ưu hóa các hoạt động kinh doanh.

Với Smart Sale for SME (1ERP), doanh nghiệp có thể tùy chỉnh quy trình kinh doanh của mình để phù hợp với nhu cầu cụ thể, tối ưu hóa quá trình mua hàng, bán hàng, dịch vụ khách hàng, quản lý nhân sự và nhiều hơn nữa.

Ưu điểm nổi bật:
- Nâng cao hiệu suất quản lý: 1ERP hoạt động như một trung tâm kết nối trên tất cả các khía cạnh của quản lý doanh nghiệp.
- Cải thiện sự phối hợp: Hệ thống gắn kết hợp nhất dữ liệu kinh doanh và thông tin khách hàng, tăng cường hợp tác giữa các phòng ban.
- Khả năng lập kế hoạch hiệu quả: Dễ dàng truy cập nguồn thông tin kinh doanh giúp đơn giản hóa việc phân tích.
- Khả năng mở rộng và linh hoạt: Cấu trúc mô-đun cho phép điều chỉnh quy mô theo nhu cầu doanh nghiệp.

Thông tin thêm: https://it.mobifone.vn/giai_phap/1erp — Video giới thiệu: https://www.youtube.com/watch?v=9XDo-1ZdNrI',
     N'Tính năng của giải pháp:
1. Quản lý Quan hệ khách hàng – CRM: thu thập dữ liệu tự động từ các kênh, cập nhật quản lý thông tin khách hàng.
2. Quản lý Bán hàng: quản lý báo giá, sản phẩm, theo dõi chi tiết đơn hàng, công nợ.
3. Quản lý Mua hàng: quản lý quy trình mua hàng từ tạo đơn, quản lý nhà cung cấp đến xử lý đơn hàng.
4. Quản lý Kho hàng: quản lý tồn kho, vị trí hàng hóa.
5. Quản lý Hóa đơn điện tử: tích hợp với MobiFone Invoice.
6. Xây dựng Landing page cho doanh nghiệp.
7. Diễn đàn & Livechat & Blog nội bộ/công khai.
8. Tuyển dụng: quản lý quy trình tuyển dụng.

Gói cước tham khảo: từ 2.400.000đ (gói User ES 5 user / 6 tháng) đến 14.400.000đ (gói SCM 10 user / 12 tháng). Liên hệ nhà cung cấp để được tư vấn gói phù hợp.',
     N'Nâng cao hiệu suất quản lý; Cải thiện sự phối hợp giữa các phòng ban; Khả năng lập kế hoạch hiệu quả; Khả năng mở rộng và linh hoạt theo cấu trúc mô-đun.',
     N'Nền tảng quản lý tài nguyên doanh nghiệp tích hợp (ERP) dành cho doanh nghiệp vừa và nhỏ, hỗ trợ CRM, bán hàng, mua hàng, kho, hóa đơn điện tử và nhiều tính năng khác.',
     2400000, 2400000, N'VND',
     NULL,
     @NCUId, 1, 1, 9, 9,
     N'Doanh nghiệp vừa và nhỏ (SMEs)',
     N'https://it.mobifone.vn/giai_phap/1erp',
     N'ERP, quản trị doanh nghiệp, CRM, quản lý bán hàng, MobiFone, 1ERP, SME, Smart Sale',
     1, 1, GETDATE(), N'system-import');

    SET @SanPhamId = SCOPE_IDENTITY();
END

-- 3) Ảnh sản phẩm (VSImage, FunctionId=2 = SanPhamCNTB) — chỉ tạo nếu chưa có
IF NOT EXISTS (SELECT 1 FROM VSImage WHERE ContentId = @SanPhamId AND FunctionId = 2)
BEGIN
    INSERT INTO VSImage
    (Title, FileURL, Created, Creator, StatusId, ContentId, FunctionId, LanguageId, SiteId)
    VALUES
    (N'Quản trị doanh nghiệp - Smart Sale for SME (1ERP)',
     N'/uploads/san-pham-mobifone/1erp-banner.jpg',
     GETDATE(), N'system-import', 1, @SanPhamId, 2, 1, 1);
END

COMMIT TRANSACTION;

SELECT @NCUId AS NhaCungUngId, @SanPhamId AS SanPhamId;
