-- Đợt 7 (10/2026): hoàn thiện 60 chức năng nghiệm thu đợt 1 theo Phụ lục 3 hợp đồng Gói 4.
--  - A.5.2 Lịch sử mua hàng: giá trị đơn, trạng thái thanh toán
--  - A.2.4 Hạng nhà cung cấp: tiêu chí số giao dịch, doanh thu, điểm đánh giá sản phẩm
--  - A.5.3 / C.7 Chat gửi hình ảnh, tệp
--  - Hướng dẫn trực tuyến: quy trình 14 bước
-- Idempotent. Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -I -i dot7-hoan-thien-nghiem-thu-dot1.sql
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

-- ── A.5.2: giá trị đơn, thanh toán ─────────────────────────────────────────
IF COL_LENGTH('dbo.OcopOrderRequests', 'GiaTriDon') IS NULL
    ALTER TABLE dbo.OcopOrderRequests ADD GiaTriDon DECIMAL(18,0) NULL;
IF COL_LENGTH('dbo.OcopOrderRequests', 'TrangThaiThanhToan') IS NULL
    ALTER TABLE dbo.OcopOrderRequests ADD TrangThaiThanhToan INT NOT NULL CONSTRAINT DF_OcopOrderRequests_TrangThaiThanhToan DEFAULT 0;
IF COL_LENGTH('dbo.OcopOrderRequests', 'NgayThanhToan') IS NULL
    ALTER TABLE dbo.OcopOrderRequests ADD NgayThanhToan DATETIME NULL;
GO

-- ── A.2.4: tiêu chí xếp hạng ───────────────────────────────────────────────
IF COL_LENGTH('dbo.SupplierTiers', 'MinTransactions') IS NULL
    ALTER TABLE dbo.SupplierTiers ADD MinTransactions INT NOT NULL CONSTRAINT DF_SupplierTiers_MinTransactions DEFAULT 0;
IF COL_LENGTH('dbo.SupplierTiers', 'MinRevenue') IS NULL
    ALTER TABLE dbo.SupplierTiers ADD MinRevenue DECIMAL(18,0) NOT NULL CONSTRAINT DF_SupplierTiers_MinRevenue DEFAULT 0;
IF COL_LENGTH('dbo.SupplierTiers', 'MinRating') IS NULL
    ALTER TABLE dbo.SupplierTiers ADD MinRating DECIMAL(3,1) NOT NULL CONSTRAINT DF_SupplierTiers_MinRating DEFAULT 0;
GO

-- ── A.5.3, C.7: tệp đính kèm trong chat ───────────────────────────────────
IF COL_LENGTH('dbo.ChatMessages', 'AttachmentPath') IS NULL
    ALTER TABLE dbo.ChatMessages ADD
        AttachmentPath NVARCHAR(500) NULL,
        AttachmentName NVARCHAR(255) NULL,
        AttachmentType NVARCHAR(100) NULL,
        AttachmentSize BIGINT NULL;
GO

-- Giá trị đơn đã hoàn tất trước đây: tạm tính từ giá tham khảo khi là số (nhà cung ứng sửa lại nếu khác)
UPDATE o SET GiaTriDon = TRY_CONVERT(DECIMAL(18,0), REPLACE(REPLACE(REPLACE(REPLACE(o.GiaThamKhao, '.', ''), ',', ''), N' đ', ''), N'đ', '')) * o.SoLuong
FROM dbo.OcopOrderRequests o
WHERE o.StatusId = 3 AND o.GiaTriDon IS NULL
  AND TRY_CONVERT(DECIMAL(18,0), REPLACE(REPLACE(REPLACE(REPLACE(o.GiaThamKhao, '.', ''), ',', ''), N' đ', ''), N'đ', '')) > 0;
GO

-- ── Hướng dẫn trực tuyến: quy trình 14 bước (không đổi đường dẫn bài viết) ──
DECLARE @now DATETIME = GETDATE();
UPDATE dbo.Contents SET
    Title = N'Quy trình hồ sơ giao dịch 14 bước',
    Description = N'Giải thích 14 bước của hồ sơ giao dịch chuyển giao công nghệ.',
    Contents = N'<h2>Quy trình 14 bước</h2>
<p>Hồ sơ giao dịch chuyển giao công nghệ trên Sàn gồm 14 bước, từ khi tạo yêu cầu đến khi thanh lý hợp đồng. Mỗi bước hoàn thành, hệ thống tự mở bước tiếp theo, gửi thông báo cho các bên và ghi nhật ký.</p>
<ol>
<li>Yêu cầu chuyển giao công nghệ</li>
<li>Thỏa thuận bảo mật (NDA) – chỉ áp dụng khi sản phẩm yêu cầu</li>
<li>Yêu cầu báo giá (RFQ), mời nhà cung cấp</li>
<li>Nộp và chấm điểm hồ sơ đề xuất</li>
<li>Đàm phán thương mại</li>
<li>Kiểm tra pháp lý</li>
<li>Lập và ký hợp đồng điện tử</li>
<li>Xác nhận tạm ứng</li>
<li>Thử nghiệm</li>
<li>Bàn giao và triển khai</li>
<li>Đào tạo và chuyển giao vận hành</li>
<li>Bàn giao hồ sơ kỹ thuật</li>
<li>Nghiệm thu</li>
<li>Thanh lý hợp đồng</li>
</ol>
<p>Cán bộ Sàn theo dõi tiến độ, thành viên tham gia và nhật ký từng bước tại trang quản trị (Hồ sơ giao dịch &gt; Nhật ký).</p>',
    Modified = @now, Modifier = 'system'
WHERE QueryString = 'quy-trinh-ho-so-giao-dich-6-buoc' AND LanguageId = 1;

UPDATE dbo.Contents SET
    Contents = REPLACE(CAST(Contents AS NVARCHAR(MAX)),
        N'Trên giao diện hiện tại, phần đàm phán và kiểm tra pháp lý được gộp cùng một khu vực thao tác.',
        N'Đàm phán thương mại (bước 5) và kiểm tra pháp lý (bước 6) là hai bước riêng trong quy trình 14 bước.'),
    Modified = @now, Modifier = 'system'
WHERE QueryString = 'nda-rfq-de-xuat-dam-phan-ky-hop-dong' AND LanguageId = 1
  AND CAST(Contents AS NVARCHAR(MAX)) LIKE N'%được gộp cùng một khu vực thao tác%';
GO

SELECT COL_LENGTH('dbo.OcopOrderRequests','GiaTriDon') GiaTriDon,
       COL_LENGTH('dbo.SupplierTiers','MinRevenue') MinRevenue,
       COL_LENGTH('dbo.ChatMessages','AttachmentPath') ChatAttach,
       (SELECT COUNT(*) FROM dbo.Contents WHERE QueryString = 'quy-trinh-ho-so-giao-dich-6-buoc' AND Title LIKE N'%14 bước%') Guide14;
