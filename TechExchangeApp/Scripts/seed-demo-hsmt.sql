-- DỮ LIỆU MẪU CHO BUỔI DEMO các chức năng bổ sung theo HSMT (docs/ho so thau/KichBan_Demo_ChuongV.md).
-- CHỈ chạy trên máy demo / máy thử. KHÔNG chạy trên máy chủ thật: giá gói, câu hỏi FAQ, sự kiện đều là số liệu mẫu.
-- Idempotent (chạy lại không nhân đôi). Gỡ: xem khối "GỠ DỮ LIỆU MẪU" ở cuối file.
-- Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -I -i seed-demo-hsmt.sql
SET NOCOUNT ON;

-- ── FAQ (A.7.3) ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.Faqs WHERE Creator = 'demo-seed')
INSERT INTO dbo.Faqs(GroupName, Question, Answer, SortOrder, IsActive, LanguageId, Creator) VALUES
 (N'Tài khoản', N'Làm thế nào để đăng ký tài khoản?', N'Bấm "Đăng ký" ở góc trên trang, nhập tên đăng nhập, họ tên, email, mật khẩu và đồng ý điều khoản xử lý dữ liệu cá nhân.', 1, 1, 1, 'demo-seed'),
 (N'Tài khoản', N'Tôi quên mật khẩu thì làm sao?', N'Tại trang đăng nhập, bấm "Quên mật khẩu?", nhập email hoặc tên đăng nhập. Hệ thống gửi liên kết đặt lại mật khẩu về email, liên kết có hiệu lực 30 phút.', 2, 1, 1, 'demo-seed'),
 (N'Nhà cung cấp', N'Làm sao để đăng sản phẩm công nghệ, thiết bị?', N'Đăng ký hồ sơ nhà cung cấp, sau đó vào "Quản lý sản phẩm" trong khu tài khoản để thêm mới. Sản phẩm được Sàn duyệt trước khi xuất bản.', 1, 1, 1, 'demo-seed'),
 (N'Nhà cung cấp', N'Nhà cung cấp có thể gửi bài viết giới thiệu không?', N'Có. Vào "Bài viết của tôi", soạn bài và bấm "Gửi duyệt". Sàn sẽ duyệt và xuất bản.', 2, 1, 1, 'demo-seed'),
 (N'Người mua', N'Làm sao để lưu lại sản phẩm quan tâm?', N'Bấm nút "Quan tâm" ở trang chi tiết sản phẩm. Danh sách sản phẩm đã lưu nằm trong mục "Sản phẩm quan tâm" của khu tài khoản.', 1, 1, 1, 'demo-seed'),
 (N'Sự kiện', N'Đăng ký tham dự sự kiện thế nào?', N'Mở tin sự kiện, điền form "Đăng ký tham gia". Anh/chị nhận email xác nhận kèm vé có mã QR để điểm danh tại sự kiện.', 1, 1, 1, 'demo-seed'),
 (N'Account', N'How do I reset my password?', N'On the login page click "Forgot password?" and enter your email or username. A reset link valid for 30 minutes will be sent to your email.', 1, 1, 2, 'demo-seed'),
 (N'Events', N'How do I register for an event?', N'Open the event page and fill in the "Register to attend" form. You will receive a confirmation email with a QR ticket for check-in.', 1, 1, 2, 'demo-seed');

-- ── Sự kiện mẫu gắn với tin bài 117099 (A.4.1, A.4.3) ─────────────────────
DECLARE @evt BIGINT = 117099;
IF EXISTS (SELECT 1 FROM dbo.Contents WHERE Id = @evt) AND NOT EXISTS (SELECT 1 FROM dbo.EventInfos WHERE ContentId = @evt)
BEGIN
    INSERT INTO dbo.EventInfos(ContentId, StartAt, EndAt, Location, LivestreamUrl, RegistrationEnabled, RegistrationDeadline, MaxAttendees, ReminderOffsetsMinutes, ReminderChannels, Modifier)
    VALUES (@evt, DATEADD(HOUR, 8, CAST(CAST(DATEADD(DAY, 7, GETDATE()) AS DATE) AS DATETIME)),
            DATEADD(HOUR, 11, CAST(CAST(DATEADD(DAY, 7, GETDATE()) AS DATE) AS DATETIME)),
            N'Trung tâm Ứng dụng Tiến bộ KH&CN Cần Thơ, 166 Nguyễn Văn Cừ, P. An Bình', NULL,
            1, DATEADD(DAY, 6, GETDATE()), 100, '1440,60', 1, 'demo-seed');
    INSERT INTO dbo.EventBoothTypes(ContentId, Name, AreaM2, Position, Price, Quantity, Services, SortOrder, IsActive) VALUES
     (@evt, N'Gian tiêu chuẩn', 9,  N'Sảnh chính', 3000000, 10, N'Bàn, 2 ghế, điện, bảng tên', 1, 1),
     (@evt, N'Gian đôi',        18, N'Sảnh chính', 5500000, 4,  N'2 bàn, 4 ghế, điện, bảng tên, kệ trưng bày', 2, 1);
END

-- ── Gói gian hàng mẫu (A.2.3) ───────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.StorePackages WHERE Description = N'Gói mẫu cho demo')
INSERT INTO dbo.StorePackages(Name, MaxProducts, Price, DurationDays, Description, SortOrder, IsActive) VALUES
 (N'Gói 5 sản phẩm',  5,  1000000, 365, N'Gói mẫu cho demo', 1, 1),
 (N'Gói 10 sản phẩm', 10, 1800000, 365, N'Gói mẫu cho demo', 2, 1),
 (N'Gói 20 sản phẩm', 20, 3200000, 365, N'Gói mẫu cho demo', 3, 1);

-- ── Phòng trưng bày mẫu, 10 vị trí (A.3.3, A.3.4) ──────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.Showrooms WHERE Description = N'Phòng mẫu cho demo')
BEGIN
    INSERT INTO dbo.Showrooms(Name, Location, Description, SortOrder, IsActive) VALUES (N'Phòng trưng bày 1', N'Tầng 1', N'Phòng mẫu cho demo', 1, 1);
    DECLARE @room INT = SCOPE_IDENTITY(), @i INT = 1;
    WHILE @i <= 10
    BEGIN
        INSERT INTO dbo.ShowroomSlots(ShowroomId, Code, IsActive) VALUES (@room, CONCAT('A-', RIGHT('0' + CAST(@i AS VARCHAR(2)), 2)), 1);
        SET @i += 1;
    END
END

SELECT (SELECT COUNT(*) FROM dbo.Faqs WHERE Creator='demo-seed') Faqs,
       (SELECT COUNT(*) FROM dbo.EventInfos WHERE Modifier='demo-seed') Events,
       (SELECT COUNT(*) FROM dbo.StorePackages WHERE Description=N'Gói mẫu cho demo') Packages,
       (SELECT COUNT(*) FROM dbo.ShowroomSlots s JOIN dbo.Showrooms r ON r.Id=s.ShowroomId WHERE r.Description=N'Phòng mẫu cho demo') Slots;

/* ── GỠ DỮ LIỆU MẪU (bỏ chú thích và chạy riêng khi cần) ──
DELETE FROM dbo.Faqs WHERE Creator = 'demo-seed';
DELETE b FROM dbo.EventBoothBookings b JOIN dbo.EventInfos e ON e.ContentId = b.ContentId WHERE e.Modifier = 'demo-seed';
DELETE t FROM dbo.EventBoothTypes t JOIN dbo.EventInfos e ON e.ContentId = t.ContentId WHERE e.Modifier = 'demo-seed';
DELETE r FROM dbo.EventRegistrations r JOIN dbo.EventInfos e ON e.ContentId = r.ContentId WHERE e.Modifier = 'demo-seed';
DELETE i FROM dbo.EventInteractions i JOIN dbo.EventInfos e ON e.ContentId = i.ContentId WHERE e.Modifier = 'demo-seed';
DELETE FROM dbo.EventInfos WHERE Modifier = 'demo-seed';
DELETE FROM dbo.SupplierSubscriptions WHERE PackageId IN (SELECT Id FROM dbo.StorePackages WHERE Description = N'Gói mẫu cho demo');
DELETE FROM dbo.StorePackages WHERE Description = N'Gói mẫu cho demo';
DELETE s FROM dbo.ShowroomSlots s JOIN dbo.Showrooms r ON r.Id = s.ShowroomId WHERE r.Description = N'Phòng mẫu cho demo';
DELETE FROM dbo.Showrooms WHERE Description = N'Phòng mẫu cho demo';
*/
