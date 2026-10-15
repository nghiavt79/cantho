/* ============================================================================
   TRIỂN KHAI LÊN MÁY CHỦ – mekongx.vn
   Bù toàn bộ thay đổi dữ liệu từ bản code 04/08/2026 đến 16/10/2026
   (HSMT Đợt 1–6 + bản vá pentest MobiFone 10/2026).

   - Mọi phần đều kiểm tra trước khi tạo/chèn → CHẠY LẠI NHIỀU LẦN AN TOÀN.
   - Không xóa dữ liệu. Không gồm dữ liệu demo (seed-demo-hsmt.sql).
   - Không chạy lại seed-huong-dan-su-dung-v2.sql (máy chủ đã chạy; chạy lại sẽ ghi đè bài viết đã sửa).

   CÁCH CHẠY (BẮT BUỘC -f 65001 để không lỗi font tiếng Việt; -b dừng ngay khi có lỗi):
     sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -i deploy-server-2026-10-16.sql

   Nên sao lưu cơ sở dữ liệu trước khi chạy. Chạy xong mới khởi động lại ứng dụng (IIS).
   Tự sinh từ các script gốc trong thư mục Scripts – sửa script gốc rồi sinh lại, đừng sửa tay file này.
   ============================================================================ */
SET NOCOUNT ON;
-- sqlcmd mặc định tắt QUOTED_IDENTIFIER → ghi vào bảng có filtered index sẽ lỗi (Msg 1934)
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ############################################################################
   1. CẤU TRÚC DỮ LIỆU (bắt buộc – thiếu là đăng nhập và nhiều trang lỗi)
   ############################################################################ */

-- ============================== dot1-bo-sung-chuc-nang-hsmt.sql ==============================
PRINT N'>> dot1-bo-sung-chuc-nang-hsmt.sql';
GO
-- Đợt 1 bổ sung chức năng theo HSMT (docs/ho so thau/Ke_hoach_phat_trien_chuc_nang_con_thieu.md).
-- Idempotent — chạy lại không lỗi. Chạy bằng: sqlcmd -S <server> -d TechExchangeNew -f 65001 -i dot1-bo-sung-chuc-nang-hsmt.sql
SET NOCOUNT ON;

-- ── A.5.1: Khóa tài khoản ──────────────────────────────────────────────────
IF COL_LENGTH('dbo.Users', 'IsLocked') IS NULL
    ALTER TABLE dbo.Users ADD IsLocked BIT NOT NULL CONSTRAINT DF_Users_IsLocked DEFAULT(0);
IF COL_LENGTH('dbo.Users', 'LockedReason') IS NULL
    ALTER TABLE dbo.Users ADD LockedReason NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.Users', 'LockedAt') IS NULL
    ALTER TABLE dbo.Users ADD LockedAt DATETIME NULL;
IF COL_LENGTH('dbo.Users', 'LockedBy') IS NULL
    ALTER TABLE dbo.Users ADD LockedBy INT NULL;
GO

-- ── A.1.1: Quên mật khẩu (token đặt lại, lưu dạng băm, dùng 1 lần) ──────────
IF OBJECT_ID('dbo.PasswordResetTokens', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetTokens (
        Id         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
        UserId     INT           NOT NULL,
        TokenHash  VARCHAR(64)   NOT NULL,
        ExpiresAt  DATETIME      NOT NULL,
        UsedAt     DATETIME      NULL,
        Created    DATETIME      NOT NULL CONSTRAINT DF_PasswordResetTokens_Created DEFAULT(GETDATE()),
        RequestIp  VARCHAR(64)   NULL
    );
    CREATE UNIQUE INDEX UX_PasswordResetTokens_TokenHash ON dbo.PasswordResetTokens(TokenHash);
    CREATE INDEX IX_PasswordResetTokens_UserId ON dbo.PasswordResetTokens(UserId, Created);
END
GO

-- ── A.7.3: FAQ ──────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.Faqs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Faqs (
        Id         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Faqs PRIMARY KEY,
        GroupName  NVARCHAR(200)  NOT NULL,
        Question   NVARCHAR(1000) NOT NULL,
        Answer     NVARCHAR(MAX)  NOT NULL,
        SortOrder  INT            NOT NULL CONSTRAINT DF_Faqs_SortOrder DEFAULT(0),
        IsActive   BIT            NOT NULL CONSTRAINT DF_Faqs_IsActive DEFAULT(1),
        LanguageId INT            NOT NULL CONSTRAINT DF_Faqs_LanguageId DEFAULT(1),
        Created    DATETIME       NOT NULL CONSTRAINT DF_Faqs_Created DEFAULT(GETDATE()),
        Creator    NVARCHAR(100)  NULL,
        Modified   DATETIME       NULL,
        Modifier   NVARCHAR(100)  NULL
    );
    CREATE INDEX IX_Faqs_Lang ON dbo.Faqs(LanguageId, IsActive, GroupName, SortOrder);
END
GO

-- ── B.3.3: Sản phẩm quan tâm ────────────────────────────────────────────────
IF OBJECT_ID('dbo.ProductFavorites', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductFavorites (
        Id         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductFavorites PRIMARY KEY,
        UserId     INT      NOT NULL,
        SanPhamId  INT      NOT NULL,
        Created    DATETIME NOT NULL CONSTRAINT DF_ProductFavorites_Created DEFAULT(GETDATE())
    );
    CREATE UNIQUE INDEX UX_ProductFavorites_User_Product ON dbo.ProductFavorites(UserId, SanPhamId);
    CREATE INDEX IX_ProductFavorites_Product ON dbo.ProductFavorites(SanPhamId);
END
GO

-- ── A.3.1, B.1.4: Hình thức trưng bày (1 = Trực tuyến, 2 = Trưng bày tại Sàn, 3 = Cả hai) ──
IF COL_LENGTH('dbo.SanPhamCNTB', 'DisplayMode') IS NULL
    ALTER TABLE dbo.SanPhamCNTB ADD DisplayMode TINYINT NOT NULL CONSTRAINT DF_SanPhamCNTB_DisplayMode DEFAULT(1);
GO

SELECT
    (SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name IN ('IsLocked','LockedReason','LockedAt','LockedBy')) AS UsersLockCols,
    OBJECT_ID('dbo.PasswordResetTokens') AS PasswordResetTokens,
    OBJECT_ID('dbo.Faqs') AS Faqs,
    OBJECT_ID('dbo.ProductFavorites') AS ProductFavorites,
    COL_LENGTH('dbo.SanPhamCNTB', 'DisplayMode') AS DisplayModeCol;
GO

-- ============================== dot2-bo-sung-chuc-nang-hsmt.sql ==============================
PRINT N'>> dot2-bo-sung-chuc-nang-hsmt.sql';
GO
-- Đợt 2 bổ sung chức năng theo HSMT (docs/ho so thau/Ke_hoach_phat_trien_chuc_nang_con_thieu.md).
-- Idempotent — chạy lại không lỗi. Chạy bằng: sqlcmd -S <server> -d TechExchangeNew -f 65001 -i dot2-bo-sung-chuc-nang-hsmt.sql
SET NOCOUNT ON;

-- ── A.2.7, A.3.2, A.8.3: Lịch sử phiên bản sản phẩm ─────────────────────────
IF OBJECT_ID('dbo.SanPhamCNTBVersions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SanPhamCNTBVersions (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SanPhamCNTBVersions PRIMARY KEY,
        SanPhamId       INT            NOT NULL,
        VersionNo       INT            NOT NULL,
        -- 0 = trạng thái trước khi bắt đầu theo dõi, 1 = tạo mới, 2 = chỉnh sửa
        Action          TINYINT        NOT NULL,
        SnapshotJson    NVARCHAR(MAX)  NOT NULL,
        ChangedFields   NVARCHAR(2000) NULL,
        ChangedBy       NVARCHAR(256)  NULL,
        ChangedByUserId INT            NULL,
        Created         DATETIME       NOT NULL CONSTRAINT DF_SanPhamCNTBVersions_Created DEFAULT(GETDATE())
    );
    CREATE UNIQUE INDEX UX_SanPhamCNTBVersions_Product_Version ON dbo.SanPhamCNTBVersions(SanPhamId, VersionNo);
END
GO

-- ── A.8.2: Cảnh báo sản phẩm trùng lặp / quá hạn cập nhật ───────────────────
IF OBJECT_ID('dbo.ProductWarnings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductWarnings (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductWarnings PRIMARY KEY,
        SanPhamId   INT            NOT NULL,
        -- 1 = trùng tên, 2 = trùng mô tả, 3 = quá hạn cập nhật
        WarningType TINYINT        NOT NULL,
        GroupKey    NVARCHAR(450)  NULL,
        Detail      NVARCHAR(1000) NULL,
        DetectedAt  DATETIME       NOT NULL CONSTRAINT DF_ProductWarnings_DetectedAt DEFAULT(GETDATE()),
        LastSeenAt  DATETIME       NOT NULL CONSTRAINT DF_ProductWarnings_LastSeenAt DEFAULT(GETDATE()),
        -- 0 = đang mở, 1 = đã xử lý (quản trị đánh dấu), 2 = tự đóng (lần quét sau không còn)
        Status      TINYINT        NOT NULL CONSTRAINT DF_ProductWarnings_Status DEFAULT(0),
        ResolvedAt  DATETIME       NULL,
        ResolvedBy  NVARCHAR(256)  NULL,
        Note        NVARCHAR(500)  NULL
    );
    CREATE INDEX IX_ProductWarnings_Status ON dbo.ProductWarnings(Status, WarningType);
    CREATE INDEX IX_ProductWarnings_Product ON dbo.ProductWarnings(SanPhamId, WarningType);
END
GO

-- ── B.3.5, A.7.1, A.5.4: Bình luận (dùng chung: 1 = sản phẩm, 2 = gian hàng/nhà cung ứng, 3 = bài viết) ──
-- Đặt tên UserComments vì CSDL đã có bảng Comments cũ (2016, cấu trúc và mã đối tượng khác) — không đụng tới.
IF OBJECT_ID('dbo.UserComments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserComments (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_UserComments PRIMARY KEY,
        TargetType  TINYINT        NOT NULL,
        TargetId    INT            NOT NULL,
        UserId      INT            NOT NULL,
        ParentId    INT            NULL,
        Content     NVARCHAR(2000) NOT NULL,
        -- 1 = hiển thị, 0 = đã ẩn
        StatusId    TINYINT        NOT NULL CONSTRAINT DF_UserComments_StatusId DEFAULT(1),
        Created     DATETIME       NOT NULL CONSTRAINT DF_UserComments_Created DEFAULT(GETDATE()),
        Modified    DATETIME       NULL,
        ModifiedBy  NVARCHAR(256)  NULL,
        IpAddress   VARCHAR(64)    NULL
    );
    CREATE INDEX IX_UserComments_Target ON dbo.UserComments(TargetType, TargetId, StatusId, Created);
    CREATE INDEX IX_UserComments_User ON dbo.UserComments(UserId, Created);
END
GO

-- ── A.5.3, A.2.10: Gắn cờ hội thoại chat vi phạm ────────────────────────────
IF COL_LENGTH('dbo.ChatConversations', 'IsFlagged') IS NULL
    ALTER TABLE dbo.ChatConversations ADD IsFlagged BIT NOT NULL CONSTRAINT DF_ChatConversations_IsFlagged DEFAULT(0);
IF COL_LENGTH('dbo.ChatConversations', 'FlagReason') IS NULL
    ALTER TABLE dbo.ChatConversations ADD FlagReason NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.ChatConversations', 'FlaggedAt') IS NULL
    ALTER TABLE dbo.ChatConversations ADD FlaggedAt DATETIME NULL;
IF COL_LENGTH('dbo.ChatConversations', 'FlaggedBy') IS NULL
    ALTER TABLE dbo.ChatConversations ADD FlaggedBy NVARCHAR(256) NULL;
GO

-- ── Tham số quét cảnh báo sản phẩm ──────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.SYS_PARAMETERS WHERE Name = 'PRODUCT_STALE_MONTHS')
    INSERT INTO dbo.SYS_PARAMETERS(Name, Val, Description, IsSystem, Activated, Domain, LanguageId, ParentId)
    VALUES ('PRODUCT_STALE_MONTHS', '12', N'Số tháng không cập nhật thì cảnh báo sản phẩm quá hạn', 0, 1, 'SAN PHAM', 1, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.SYS_PARAMETERS WHERE Name = 'PRODUCT_WARNING_SCAN_HOUR')
    INSERT INTO dbo.SYS_PARAMETERS(Name, Val, Description, IsSystem, Activated, Domain, LanguageId, ParentId)
    VALUES ('PRODUCT_WARNING_SCAN_HOUR', '2', N'Giờ trong ngày (0-23) chạy quét cảnh báo sản phẩm trùng lặp, quá hạn', 0, 1, 'SAN PHAM', 1, 0);
GO

SELECT
    OBJECT_ID('dbo.SanPhamCNTBVersions') AS Versions,
    OBJECT_ID('dbo.ProductWarnings') AS Warnings,
    OBJECT_ID('dbo.UserComments') AS UserComments,
    COL_LENGTH('dbo.ChatConversations', 'IsFlagged') AS ChatFlagCol,
    (SELECT COUNT(*) FROM dbo.SYS_PARAMETERS WHERE Name IN ('PRODUCT_STALE_MONTHS','PRODUCT_WARNING_SCAN_HOUR')) AS Params;
GO

-- ============================== dot3-su-kien-hsmt.sql ==============================
PRINT N'>> dot3-su-kien-hsmt.sql';
GO
-- Đợt 3 bổ sung chức năng theo HSMT — Sự kiện (docs/ho so thau/Ke_hoach_phat_trien_chuc_nang_con_thieu.md).
-- Sự kiện là tin bài (Contents); các bảng dưới gắn theo Contents.Id của bản tiếng Việt.
-- Idempotent. Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -I -i dot3-su-kien-hsmt.sql
SET NOCOUNT ON;

-- ── A.4.1, C.6: Thông tin sự kiện gắn với tin bài ───────────────────────────
IF OBJECT_ID('dbo.EventInfos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventInfos (
        Id                     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EventInfos PRIMARY KEY,
        ContentId              BIGINT         NOT NULL,
        StartAt                DATETIME       NOT NULL,
        EndAt                  DATETIME       NULL,
        Location               NVARCHAR(500)  NULL,
        LivestreamUrl          NVARCHAR(1000) NULL,
        RegistrationEnabled    BIT            NOT NULL CONSTRAINT DF_EventInfos_RegEnabled DEFAULT(1),
        RegistrationDeadline   DATETIME       NULL,
        MaxAttendees           INT            NULL,
        -- Mốc nhắc lịch, số phút trước giờ bắt đầu, phân cách dấu phẩy (mặc định 24 giờ và 1 giờ)
        ReminderOffsetsMinutes VARCHAR(100)   NOT NULL CONSTRAINT DF_EventInfos_Offsets DEFAULT('1440,60'),
        -- Kênh nhắc: 1 = email (kèm chuông thông báo trên web cho người đã đăng nhập), 2 = SMS
        ReminderChannels       TINYINT        NOT NULL CONSTRAINT DF_EventInfos_Channels DEFAULT(1),
        Created                DATETIME       NOT NULL CONSTRAINT DF_EventInfos_Created DEFAULT(GETDATE()),
        Modified               DATETIME       NULL,
        Modifier               NVARCHAR(256)  NULL
    );
    CREATE UNIQUE INDEX UX_EventInfos_Content ON dbo.EventInfos(ContentId);
    CREATE INDEX IX_EventInfos_StartAt ON dbo.EventInfos(StartAt);
END
GO

-- ── A.4.1, B.1.6: Đăng ký tham gia, điểm danh ───────────────────────────────
IF OBJECT_ID('dbo.EventRegistrations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventRegistrations (
        Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EventRegistrations PRIMARY KEY,
        ContentId    BIGINT         NOT NULL,
        UserId       INT            NULL,
        FullName     NVARCHAR(200)  NOT NULL,
        Email        NVARCHAR(256)  NOT NULL,
        Phone        VARCHAR(30)    NULL,
        Organization NVARCHAR(300)  NULL,
        Note         NVARCHAR(1000) NULL,
        -- 1 = đã đăng ký, 2 = đã điểm danh, 0 = đã hủy
        Status       TINYINT        NOT NULL CONSTRAINT DF_EventRegistrations_Status DEFAULT(1),
        CheckInCode  VARCHAR(32)    NOT NULL,
        Created      DATETIME       NOT NULL CONSTRAINT DF_EventRegistrations_Created DEFAULT(GETDATE()),
        CheckedInAt  DATETIME       NULL,
        CheckedInBy  NVARCHAR(256)  NULL,
        IpAddress    VARCHAR(64)    NULL
    );
    CREATE UNIQUE INDEX UX_EventRegistrations_Code ON dbo.EventRegistrations(CheckInCode);
    CREATE UNIQUE INDEX UX_EventRegistrations_Content_Email ON dbo.EventRegistrations(ContentId, Email);
END
GO

-- ── A.4.4, A.4.5: Lượt xem, "Quan tâm", "Nhắc lịch" ─────────────────────────
IF OBJECT_ID('dbo.EventInteractions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventInteractions (
        Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EventInteractions PRIMARY KEY,
        ContentId       BIGINT         NOT NULL,
        -- 1 = xem trang sự kiện, 2 = quan tâm, 3 = đăng ký nhắc lịch
        InteractionType TINYINT        NOT NULL,
        UserId          INT            NULL,
        VisitorKey      VARCHAR(64)    NULL,
        Email           NVARCHAR(256)  NULL,
        Phone           VARCHAR(30)    NULL,
        Created         DATETIME       NOT NULL CONSTRAINT DF_EventInteractions_Created DEFAULT(GETDATE())
    );
    CREATE INDEX IX_EventInteractions_Content ON dbo.EventInteractions(ContentId, InteractionType, Created);
END
GO

-- ── A.4.5, A.8.4: Nhật ký đã gửi nhắc lịch (chống gửi trùng) ─────────────────
IF OBJECT_ID('dbo.EventReminderLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventReminderLogs (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EventReminderLogs PRIMARY KEY,
        ContentId     BIGINT        NOT NULL,
        RecipientKey  NVARCHAR(300) NOT NULL,
        OffsetMinutes INT           NOT NULL,
        Created       DATETIME      NOT NULL CONSTRAINT DF_EventReminderLogs_Created DEFAULT(GETDATE())
    );
    CREATE UNIQUE INDEX UX_EventReminderLogs ON dbo.EventReminderLogs(ContentId, RecipientKey, OffsetMinutes);
END
GO

-- ── A.4.3: Gian hàng sự kiện ────────────────────────────────────────────────
IF OBJECT_ID('dbo.EventBoothTypes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventBoothTypes (
        Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EventBoothTypes PRIMARY KEY,
        ContentId BIGINT         NOT NULL,
        Name      NVARCHAR(200)  NOT NULL,
        AreaM2    DECIMAL(8,2)   NULL,
        Position  NVARCHAR(200)  NULL,
        Price     DECIMAL(18,0)  NOT NULL CONSTRAINT DF_EventBoothTypes_Price DEFAULT(0),
        Quantity  INT            NOT NULL CONSTRAINT DF_EventBoothTypes_Qty DEFAULT(1),
        Services  NVARCHAR(1000) NULL,
        SortOrder INT            NOT NULL CONSTRAINT DF_EventBoothTypes_Sort DEFAULT(0),
        IsActive  BIT            NOT NULL CONSTRAINT DF_EventBoothTypes_Active DEFAULT(1)
    );
    CREATE INDEX IX_EventBoothTypes_Content ON dbo.EventBoothTypes(ContentId);
END
GO

IF OBJECT_ID('dbo.EventBoothBookings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventBoothBookings (
        Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EventBoothBookings PRIMARY KEY,
        BoothTypeId  INT            NOT NULL,
        ContentId    BIGINT         NOT NULL,
        UserId       INT            NULL,
        Organization NVARCHAR(300)  NOT NULL,
        ContactName  NVARCHAR(200)  NOT NULL,
        Email        NVARCHAR(256)  NOT NULL,
        Phone        VARCHAR(30)    NOT NULL,
        Quantity     INT            NOT NULL CONSTRAINT DF_EventBoothBookings_Qty DEFAULT(1),
        Note         NVARCHAR(1000) NULL,
        -- 1 = chờ duyệt, 2 = đã duyệt, 3 = từ chối, 0 = đã hủy
        Status       TINYINT        NOT NULL CONSTRAINT DF_EventBoothBookings_Status DEFAULT(1),
        Created      DATETIME       NOT NULL CONSTRAINT DF_EventBoothBookings_Created DEFAULT(GETDATE()),
        ReviewedBy   NVARCHAR(256)  NULL,
        ReviewedAt   DATETIME       NULL,
        ReviewNote   NVARCHAR(500)  NULL
    );
    CREATE INDEX IX_EventBoothBookings_Content ON dbo.EventBoothBookings(ContentId, Status);
END
GO

SELECT OBJECT_ID('dbo.EventInfos') EventInfos, OBJECT_ID('dbo.EventRegistrations') Registrations,
       OBJECT_ID('dbo.EventInteractions') Interactions, OBJECT_ID('dbo.EventReminderLogs') ReminderLogs,
       OBJECT_ID('dbo.EventBoothTypes') BoothTypes, OBJECT_ID('dbo.EventBoothBookings') BoothBookings;
GO

-- ============================== dot4-bo-sung-chuc-nang-hsmt.sql ==============================
PRINT N'>> dot4-bo-sung-chuc-nang-hsmt.sql';
GO
-- Đợt 4 bổ sung chức năng theo HSMT: phân hạng nhà cung cấp, gói gian hàng, thu chi – hoa hồng,
-- phòng / vị trí trưng bày, biểu mẫu động (docs/ho so thau/Ke_hoach_phat_trien_chuc_nang_con_thieu.md).
-- Idempotent. Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -I -i dot4-bo-sung-chuc-nang-hsmt.sql
SET NOCOUNT ON;

-- ── A.2.2, A.2.4: Hạng nhà cung cấp ─────────────────────────────────────────
IF OBJECT_ID('dbo.SupplierTiers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SupplierTiers (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupplierTiers PRIMARY KEY,
        Name            NVARCHAR(100) NOT NULL,
        -- Hạng cao hơn có Rank lớn hơn
        [Rank]          INT           NOT NULL,
        BadgeColor      VARCHAR(20)   NOT NULL CONSTRAINT DF_SupplierTiers_Color DEFAULT('#6c757d'),
        MinProducts     INT           NOT NULL CONSTRAINT DF_SupplierTiers_MinP DEFAULT(0),
        MinContacts     INT           NOT NULL CONSTRAINT DF_SupplierTiers_MinC DEFAULT(0),
        MinViews        INT           NOT NULL CONSTRAINT DF_SupplierTiers_MinV DEFAULT(0),
        RequireVerified BIT           NOT NULL CONSTRAINT DF_SupplierTiers_Ver DEFAULT(0),
        Description     NVARCHAR(500) NULL,
        IsActive        BIT           NOT NULL CONSTRAINT DF_SupplierTiers_Active DEFAULT(1)
    );
    INSERT INTO dbo.SupplierTiers(Name, [Rank], BadgeColor, MinProducts, MinContacts, MinViews, RequireVerified, Description) VALUES
     (N'Thành viên', 1, '#6c757d', 0,  0,  0,    0, N'Nhà cung cấp đã có hồ sơ trên Sàn'),
     (N'Bạc',        2, '#8a94a6', 5,  3,  500,  1, N'Từ 5 sản phẩm đang xuất bản, 3 lượt liên hệ, 500 lượt xem, đã xác minh pháp lý'),
     (N'Vàng',       3, '#c99a06', 15, 10, 2000, 1, N'Từ 15 sản phẩm đang xuất bản, 10 lượt liên hệ, 2.000 lượt xem, đã xác minh pháp lý');
END
GO
IF COL_LENGTH('dbo.NhaCungUng', 'TierId') IS NULL
    ALTER TABLE dbo.NhaCungUng ADD TierId INT NULL, TierAssignedAt DATETIME NULL, TierAssignedBy NVARCHAR(256) NULL;
GO

-- ── A.2.3: Gói gian hàng tính theo số sản phẩm ──────────────────────────────
IF OBJECT_ID('dbo.StorePackages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StorePackages (
        Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StorePackages PRIMARY KEY,
        Name         NVARCHAR(200) NOT NULL,
        MaxProducts  INT           NOT NULL,
        Price        DECIMAL(18,0) NOT NULL CONSTRAINT DF_StorePackages_Price DEFAULT(0),
        DurationDays INT           NOT NULL CONSTRAINT DF_StorePackages_Days DEFAULT(365),
        Description  NVARCHAR(500) NULL,
        SortOrder    INT           NOT NULL CONSTRAINT DF_StorePackages_Sort DEFAULT(0),
        IsActive     BIT           NOT NULL CONSTRAINT DF_StorePackages_Active DEFAULT(1)
    );
END
GO
IF OBJECT_ID('dbo.SupplierSubscriptions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SupplierSubscriptions (
        Id         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupplierSubscriptions PRIMARY KEY,
        CungUngId  INT           NOT NULL,
        PackageId  INT           NOT NULL,
        StartDate  DATE          NOT NULL,
        EndDate    DATE          NOT NULL,
        MaxProducts INT          NOT NULL,
        Price      DECIMAL(18,0) NOT NULL,
        -- 1 = hiệu lực, 0 = đã hủy
        Status     TINYINT       NOT NULL CONSTRAINT DF_SupplierSubscriptions_Status DEFAULT(1),
        Note       NVARCHAR(500) NULL,
        Created    DATETIME      NOT NULL CONSTRAINT DF_SupplierSubscriptions_Created DEFAULT(GETDATE()),
        CreatedBy  NVARCHAR(256) NULL
    );
    CREATE INDEX IX_SupplierSubscriptions_Supplier ON dbo.SupplierSubscriptions(CungUngId, Status, EndDate);
END
GO
-- Hạn mức mặc định khi nhà cung cấp chưa mua gói; tắt áp dụng hạn mức (0) để không chặn nhà cung cấp hiện có cho tới khi Sở chốt biểu phí
IF NOT EXISTS (SELECT 1 FROM dbo.SYS_PARAMETERS WHERE Name = 'STORE_QUOTA_ENFORCED')
    INSERT INTO dbo.SYS_PARAMETERS(Name, Val, Description, IsSystem, Activated, Domain, LanguageId, ParentId)
    VALUES ('STORE_QUOTA_ENFORCED', '0', N'1 = chặn nhà cung cấp đăng vượt số sản phẩm của gói gian hàng; 0 = chỉ hiển thị, không chặn', 0, 1, 'SAN PHAM', 1, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.SYS_PARAMETERS WHERE Name = 'STORE_FREE_PRODUCT_QUOTA')
    INSERT INTO dbo.SYS_PARAMETERS(Name, Val, Description, IsSystem, Activated, Domain, LanguageId, ParentId)
    VALUES ('STORE_FREE_PRODUCT_QUOTA', '5', N'Số sản phẩm được đăng khi nhà cung cấp chưa có gói gian hàng', 0, 1, 'SAN PHAM', 1, 0);
GO

-- ── A.6.1: Thu chi, hoa hồng ────────────────────────────────────────────────
IF OBJECT_ID('dbo.CommissionRates', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CommissionRates (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CommissionRates PRIMARY KEY,
        Code        VARCHAR(50)   NOT NULL,
        Name        NVARCHAR(200) NOT NULL,
        RatePercent DECIMAL(5,2)  NOT NULL,
        IsActive    BIT           NOT NULL CONSTRAINT DF_CommissionRates_Active DEFAULT(1)
    );
    CREATE UNIQUE INDEX UX_CommissionRates_Code ON dbo.CommissionRates(Code);
    INSERT INTO dbo.CommissionRates(Code, Name, RatePercent) VALUES
     ('CHUYEN_GIAO', N'Hợp đồng chuyển giao công nghệ', 0),
     ('THIET_BI',    N'Mua bán thiết bị',               0),
     ('SO_HUU_TRI_TUE', N'Chuyển nhượng tài sản trí tuệ', 0),
     ('OCOP',        N'Đơn hàng OCOP',                  0);
END
GO
IF OBJECT_ID('dbo.FinanceEntries', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FinanceEntries (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FinanceEntries PRIMARY KEY,
        EntryDate   DATE           NOT NULL,
        -- 1 = thu, 2 = chi
        Direction   TINYINT        NOT NULL,
        -- PHI_GIAN_HANG | PHI_GIAN_HANG_SU_KIEN | HOA_HONG | KHAC
        Category    VARCHAR(50)    NOT NULL,
        Amount      DECIMAL(18,0)  NOT NULL,
        CungUngId   INT            NULL,
        -- Nguồn phát sinh tự động: SUBSCRIPTION / BOOTH_BOOKING / COMMISSION (+ RefId)
        RefType     VARCHAR(50)    NULL,
        RefId       INT            NULL,
        BaseAmount  DECIMAL(18,0)  NULL,
        RatePercent DECIMAL(5,2)   NULL,
        Description NVARCHAR(500)  NULL,
        Created     DATETIME       NOT NULL CONSTRAINT DF_FinanceEntries_Created DEFAULT(GETDATE()),
        CreatedBy   NVARCHAR(256)  NULL
    );
    CREATE INDEX IX_FinanceEntries_Date ON dbo.FinanceEntries(EntryDate, Category);
    CREATE INDEX IX_FinanceEntries_Ref ON dbo.FinanceEntries(RefType, RefId);
END
GO

-- ── A.3.3, A.3.4: Phòng trưng bày, vị trí trưng bày ─────────────────────────
IF OBJECT_ID('dbo.Showrooms', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Showrooms (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Showrooms PRIMARY KEY,
        Name        NVARCHAR(200) NOT NULL,
        Location    NVARCHAR(300) NULL,
        Description NVARCHAR(500) NULL,
        SortOrder   INT           NOT NULL CONSTRAINT DF_Showrooms_Sort DEFAULT(0),
        IsActive    BIT           NOT NULL CONSTRAINT DF_Showrooms_Active DEFAULT(1)
    );
END
GO
IF OBJECT_ID('dbo.ShowroomSlots', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShowroomSlots (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ShowroomSlots PRIMARY KEY,
        ShowroomId  INT           NOT NULL,
        Code        NVARCHAR(50)  NOT NULL,
        Description NVARCHAR(300) NULL,
        SanPhamId   INT           NULL,
        AssignedAt  DATETIME      NULL,
        AssignedBy  NVARCHAR(256) NULL,
        IsActive    BIT           NOT NULL CONSTRAINT DF_ShowroomSlots_Active DEFAULT(1)
    );
    CREATE INDEX IX_ShowroomSlots_Room ON dbo.ShowroomSlots(ShowroomId);
    CREATE INDEX IX_ShowroomSlots_Product ON dbo.ShowroomSlots(SanPhamId);
END
GO

-- ── A.7.2, B.1.6: Biểu mẫu động ─────────────────────────────────────────────
IF OBJECT_ID('dbo.DynamicForms', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DynamicForms (
        Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DynamicForms PRIMARY KEY,
        Title               NVARCHAR(300)  NOT NULL,
        Slug                VARCHAR(200)   NOT NULL,
        Description         NVARCHAR(1000) NULL,
        -- [{ "key","label","type","required","options","placeholder" }]
        FieldsJson          NVARCHAR(MAX)  NOT NULL,
        SubmitText          NVARCHAR(100)  NULL,
        SuccessMessage      NVARCHAR(500)  NULL,
        RedirectUrl         NVARCHAR(500)  NULL,
        NotifyEmails        NVARCHAR(500)  NULL,
        SendConfirmation    BIT            NOT NULL CONSTRAINT DF_DynamicForms_Confirm DEFAULT(0),
        LanguageId          INT            NOT NULL CONSTRAINT DF_DynamicForms_Lang DEFAULT(1),
        IsActive            BIT            NOT NULL CONSTRAINT DF_DynamicForms_Active DEFAULT(1),
        Created             DATETIME       NOT NULL CONSTRAINT DF_DynamicForms_Created DEFAULT(GETDATE()),
        Modified            DATETIME       NULL,
        Modifier            NVARCHAR(256)  NULL
    );
    CREATE UNIQUE INDEX UX_DynamicForms_Slug ON dbo.DynamicForms(Slug);
END
GO
IF OBJECT_ID('dbo.DynamicFormSubmissions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DynamicFormSubmissions (
        Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DynamicFormSubmissions PRIMARY KEY,
        FormId    INT           NOT NULL,
        DataJson  NVARCHAR(MAX) NOT NULL,
        Email     NVARCHAR(256) NULL,
        UserId    INT           NULL,
        IpAddress VARCHAR(64)   NULL,
        Created   DATETIME      NOT NULL CONSTRAINT DF_DynamicFormSubmissions_Created DEFAULT(GETDATE())
    );
    CREATE INDEX IX_DynamicFormSubmissions_Form ON dbo.DynamicFormSubmissions(FormId, Created);
END
GO
-- Biểu mẫu mẫu: đăng ký nhận bản tin (B.1.6)
IF NOT EXISTS (SELECT 1 FROM dbo.DynamicForms WHERE Slug = 'dang-ky-nhan-ban-tin')
    INSERT INTO dbo.DynamicForms(Title, Slug, Description, FieldsJson, SubmitText, SuccessMessage, SendConfirmation, LanguageId)
    VALUES (N'Đăng ký nhận bản tin', 'dang-ky-nhan-ban-tin',
            N'Nhận tin công nghệ mới, sự kiện và nhu cầu tìm mua trên Sàn qua email.',
            N'[{"key":"ho_ten","label":"Họ tên","type":"text","required":true},{"key":"email","label":"Email","type":"email","required":true},{"key":"don_vi","label":"Đơn vị","type":"text","required":false},{"key":"linh_vuc","label":"Lĩnh vực quan tâm","type":"select","required":false,"options":["Nông nghiệp","Thủy sản","Công nghiệp chế biến","Môi trường","Năng lượng","Công nghệ thông tin","Khác"]}]',
            N'Đăng ký', N'Cảm ơn anh/chị đã đăng ký nhận bản tin.', 1, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.DynamicForms WHERE Slug = 'newsletter')
    INSERT INTO dbo.DynamicForms(Title, Slug, Description, FieldsJson, SubmitText, SuccessMessage, SendConfirmation, LanguageId)
    VALUES (N'Newsletter subscription', 'newsletter',
            N'Receive new technologies, events and sourcing requests from the Exchange by email.',
            N'[{"key":"full_name","label":"Full name","type":"text","required":true},{"key":"email","label":"Email","type":"email","required":true},{"key":"organization","label":"Organization","type":"text","required":false}]',
            N'Subscribe', N'Thank you for subscribing.', 1, 2);
GO

SELECT (SELECT COUNT(*) FROM dbo.SupplierTiers) Tiers, COL_LENGTH('dbo.NhaCungUng','TierId') TierCol,
       OBJECT_ID('dbo.StorePackages') Packages, OBJECT_ID('dbo.SupplierSubscriptions') Subs,
       (SELECT COUNT(*) FROM dbo.CommissionRates) Rates, OBJECT_ID('dbo.FinanceEntries') Finance,
       OBJECT_ID('dbo.Showrooms') Showrooms, OBJECT_ID('dbo.ShowroomSlots') Slots,
       (SELECT COUNT(*) FROM dbo.DynamicForms) Forms, OBJECT_ID('dbo.DynamicFormSubmissions') Submissions;
GO

-- ============================== dot5-attt-hsmt.sql ==============================
PRINT N'>> dot5-attt-hsmt.sql';
GO
-- Đợt 5 bổ sung theo HSMT: an toàn thông tin, bảo vệ dữ liệu cá nhân, gọi video.
-- Idempotent. Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -I -i dot5-attt-hsmt.sql
SET NOCOUNT ON;

-- ── 2.11: Đồng ý xử lý dữ liệu cá nhân khi đăng ký ─────────────────────────
IF COL_LENGTH('dbo.Users', 'ConsentAt') IS NULL
    ALTER TABLE dbo.Users ADD ConsentAt DATETIME NULL, ConsentVersion VARCHAR(20) NULL;
GO

-- ── Tham số ────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.SYS_PARAMETERS WHERE Name = 'CMS_MFA_ENABLED')
    INSERT INTO dbo.SYS_PARAMETERS(Name, Val, Description, IsSystem, Activated, Domain, LanguageId, ParentId)
    VALUES ('CMS_MFA_ENABLED', '0', N'1 = tài khoản quản trị phải nhập mã OTP gửi qua email khi vào CMS (cần cấu hình email trước khi bật)', 0, 1, 'BAO MAT', 1, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.SYS_PARAMETERS WHERE Name = 'PRIVACY_POLICY_URL')
    INSERT INTO dbo.SYS_PARAMETERS(Name, Val, Description, IsSystem, Activated, Domain, LanguageId, ParentId)
    VALUES ('PRIVACY_POLICY_URL', '', N'Đường dẫn trang chính sách bảo vệ dữ liệu cá nhân (vd /trang/chinh-sach-...-123); để trống thì ô đồng ý không có liên kết', 0, 1, 'BAO MAT', 1, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.SYS_PARAMETERS WHERE Name = 'PRIVACY_CONSENT_VERSION')
    INSERT INTO dbo.SYS_PARAMETERS(Name, Val, Description, IsSystem, Activated, Domain, LanguageId, ParentId)
    VALUES ('PRIVACY_CONSENT_VERSION', '2026-10', N'Phiên bản điều khoản xử lý dữ liệu cá nhân người dùng đồng ý khi đăng ký', 0, 1, 'BAO MAT', 1, 0);
IF NOT EXISTS (SELECT 1 FROM dbo.SYS_PARAMETERS WHERE Name = 'VIDEO_CALL_BASE_URL')
    INSERT INTO dbo.SYS_PARAMETERS(Name, Val, Description, IsSystem, Activated, Domain, LanguageId, ParentId)
    VALUES ('VIDEO_CALL_BASE_URL', 'https://meet.jit.si/', N'Địa chỉ máy chủ họp trực tuyến (Jitsi) dùng cho nút Gọi video trong chat; nên thay bằng máy chủ Jitsi tự dựng', 0, 1, 'TUONG TAC', 1, 0);
GO

SELECT COL_LENGTH('dbo.Users','ConsentAt') ConsentCol,
       (SELECT COUNT(*) FROM dbo.SYS_PARAMETERS WHERE Name IN ('CMS_MFA_ENABLED','PRIVACY_POLICY_URL','PRIVACY_CONSENT_VERSION','VIDEO_CALL_BASE_URL')) Params;
GO

-- ============================== add-users-sessionversion.sql ==============================
PRINT N'>> add-users-sessionversion.sql';
GO
/*
Thêm cột Users.SessionVersion — vá lỗi "Improper Logout Functionality" (pentest MobiFone 10/2026).
Mỗi cookie đăng nhập mang số phiên lúc đăng nhập; khi đăng xuất / đổi / đặt lại mật khẩu
số này tăng lên nên cookie cũ (kể cả bị đánh cắp) không dùng lại được.
Chạy TRƯỚC khi deploy code mới. An toàn, chạy lại được (idempotent).
*/
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'SessionVersion'
)
BEGIN
    ALTER TABLE dbo.Users ADD SessionVersion int NOT NULL CONSTRAINT DF_Users_SessionVersion DEFAULT 0;
    PRINT 'Users.SessionVersion added';
END
ELSE
    PRINT 'Users.SessionVersion already exists';
GO

/* ############################################################################
   2. NỘI DUNG GIAO DIỆN SONG NGỮ
   ############################################################################ */

-- ============================== seed-i18n-dot1-hsmt.sql ==============================
PRINT N'>> seed-i18n-dot1-hsmt.sql';
GO
-- Seed i18n cho Đợt 1 bổ sung chức năng HSMT: quên mật khẩu, khóa tài khoản, FAQ,
-- quan tâm sản phẩm, hình thức trưng bày, mã QR sản phẩm. Idempotent.
-- Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -i seed-i18n-dot1-hsmt.sql
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 -- Đăng nhập / quên mật khẩu / khóa tài khoản
 (N'auth.forgotPasswordQ',        N'Quên mật khẩu?',                                N'Forgot password?'),
 (N'auth.backToLogin',            N'Quay lại đăng nhập',                            N'Back to login'),
 (N'auth.err.locked',             N'Tài khoản đã bị tạm khóa. Vui lòng liên hệ Ban quản lý Sàn để được hỗ trợ.', N'This account has been suspended. Please contact the Exchange administrators for support.'),
 (N'auth.forgot.title',           N'Quên mật khẩu',                                 N'Forgot password'),
 (N'auth.forgot.intro',           N'Nhập email hoặc tên đăng nhập. Hệ thống sẽ gửi liên kết đặt lại mật khẩu tới email đã đăng ký.', N'Enter your email or username. We will send a password reset link to your registered email.'),
 (N'auth.forgot.label',           N'Email hoặc tên đăng nhập',                      N'Email or username'),
 (N'auth.forgot.submit',          N'Gửi liên kết đặt lại',                          N'Send reset link'),
 (N'auth.forgot.sent',            N'Nếu thông tin trùng khớp với một tài khoản, liên kết đặt lại mật khẩu đã được gửi tới email đăng ký. Liên kết có hiệu lực 30 phút.', N'If the details match an account, a password reset link has been sent to the registered email. The link is valid for 30 minutes.'),
 (N'auth.forgot.err.required',    N'Vui lòng nhập email hoặc tên đăng nhập.',       N'Please enter your email or username.'),
 (N'auth.reset.title',            N'Đặt lại mật khẩu',                              N'Reset password'),
 (N'auth.reset.newPassword',      N'Mật khẩu mới',                                  N'New password'),
 (N'auth.reset.confirmPassword',  N'Nhập lại mật khẩu mới',                         N'Confirm new password'),
 (N'auth.reset.submit',           N'Đặt mật khẩu mới',                              N'Set new password'),
 (N'auth.reset.done',             N'Đã đặt lại mật khẩu. Anh/chị có thể đăng nhập bằng mật khẩu mới.', N'Your password has been reset. You can now log in with the new password.'),
 (N'auth.reset.invalidToken',     N'Liên kết đặt lại mật khẩu không hợp lệ, đã hết hạn hoặc đã được sử dụng.', N'The password reset link is invalid, has expired or has already been used.'),
 (N'auth.reset.requestAgain',     N'Yêu cầu liên kết mới',                          N'Request a new link'),
 (N'auth.reset.err.passwordRequired', N'Vui lòng nhập mật khẩu mới.',               N'Please enter a new password.'),
 (N'auth.reset.err.passwordMin',  N'Mật khẩu tối thiểu 6 ký tự.',                   N'Password must be at least 6 characters.'),
 (N'auth.reset.err.passwordMismatch', N'Mật khẩu nhập lại không khớp.',             N'Passwords do not match.'),
 -- FAQ
 (N'faq.pageTitle',               N'Câu hỏi thường gặp',                            N'Frequently asked questions'),
 (N'faq.intro',                   N'Giải đáp các thắc mắc thường gặp khi sử dụng Sàn giao dịch công nghệ.', N'Answers to common questions about using the Technology Exchange.'),
 (N'faq.searchPh',                N'Tìm câu hỏi...',                                N'Search questions...'),
 (N'faq.searchBtn',               N'Tìm',                                           N'Search'),
 (N'faq.empty',                   N'Chưa có câu hỏi phù hợp.',                      N'No matching questions yet.'),
 (N'faq.moreHelp',                N'Chưa tìm thấy câu trả lời?',                    N'Still need help?'),
 (N'faq.contactLink',             N'Liên hệ với chúng tôi',                         N'Contact us'),
 -- Sản phẩm: quan tâm, hình thức trưng bày, QR
 (N'product.fav.save',            N'Quan tâm',                                      N'Save'),
 (N'product.fav.saved',           N'Đã quan tâm',                                   N'Saved'),
 (N'product.display.label',       N'Hình thức trưng bày',                           N'Display'),
 (N'product.display.all',         N'Mọi hình thức',                                 N'All displays'),
 (N'product.display.online',      N'Trực tuyến',                                    N'Online'),
 (N'product.display.showroom',    N'Trưng bày tại Sàn',                             N'On display at the Exchange'),
 (N'product.display.both',        N'Trực tuyến và tại Sàn',                         N'Online and at the Exchange'),
 (N'product.qr.alt',              N'Mã QR trang sản phẩm',                          N'Product page QR code'),
 (N'product.qr.hint',             N'Quét mã để mở trang sản phẩm',                  N'Scan to open this product page'),
 (N'product.qr.print',            N'In mã QR',                                      N'Print QR code'),
 -- Khu tài khoản: sản phẩm quan tâm
 (N'fav.pageTitle',               N'Sản phẩm quan tâm',                             N'Saved products'),
 (N'fav.empty',                   N'Chưa có sản phẩm nào. Bấm "Quan tâm" ở trang chi tiết sản phẩm để lưu lại.', N'No saved products yet. Click "Save" on a product page to keep it here.'),
 (N'fav.savedAt',                 N'Lưu lúc',                                       N'Saved on'),
 (N'fav.remove',                  N'Bỏ quan tâm',                                   N'Remove');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-dot1-hsmt' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
GO

-- ============================== seed-i18n-dot2-hsmt.sql ==============================
PRINT N'>> seed-i18n-dot2-hsmt.sql';
GO
-- Seed i18n cho Đợt 2 bổ sung chức năng HSMT: bình luận sản phẩm, bài viết nhà cung cấp. Idempotent.
-- Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -i seed-i18n-dot2-hsmt.sql
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'comment.title',          N'Bình luận',                                        N'Comments'),
 (N'comment.placeholder',    N'Chia sẻ câu hỏi hoặc nhận xét về sản phẩm...',     N'Share a question or comment about this product...'),
 (N'comment.submit',         N'Gửi bình luận',                                    N'Post comment'),
 (N'comment.loginToComment', N'để bình luận.',                                    N'to comment.'),
 (N'comment.empty',          N'Chưa có bình luận nào.',                           N'No comments yet.'),
 (N'comment.delete',         N'Xóa bình luận',                                    N'Delete comment'),
 (N'comment.confirmDelete',  N'Xóa bình luận này?',                               N'Delete this comment?'),
 (N'comment.err.empty',      N'Vui lòng nhập nội dung bình luận.',                N'Please enter a comment.'),
 (N'comment.err.tooLong',    N'Bình luận tối đa 2000 ký tự.',                     N'Comments are limited to 2000 characters.'),
 (N'comment.err.tooFast',    N'Anh/chị gửi quá nhanh, vui lòng thử lại sau ít giây.', N'You are posting too quickly. Please try again in a few seconds.'),
 (N'comment.err.notAllowed', N'Không thể gửi bình luận.',                         N'Unable to post the comment.'),
 (N'sidebar.myPosts',        N'Bài viết của tôi',                                 N'My posts');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-dot2-hsmt' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
GO

-- ============================== seed-i18n-dot3-su-kien.sql ==============================
PRINT N'>> seed-i18n-dot3-su-kien.sql';
GO
-- Seed i18n cho Đợt 3 (sự kiện): khối sự kiện trên trang tin, vé điểm danh. Idempotent.
-- Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -I -i seed-i18n-dot3-su-kien.sql
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'event.panel.title',        N'Thông tin sự kiện',                         N'Event information'),
 (N'event.panel.time',         N'Thời gian',                                 N'Time'),
 (N'event.panel.location',     N'Địa điểm',                                  N'Venue'),
 (N'event.panel.livestream',   N'Trực tuyến',                                N'Online'),
 (N'event.panel.watchLive',    N'Xem phát trực tiếp',                        N'Watch live'),
 (N'event.panel.registered',   N'Đã đăng ký',                                N'Registered'),
 (N'event.panel.interest',     N'Quan tâm',                                  N'Interested'),
 (N'event.panel.remind',       N'Nhắc lịch',                                 N'Remind me'),
 (N'event.remind.intro',       N'Hệ thống sẽ gửi nhắc lịch trước giờ diễn ra sự kiện.', N'We will send you a reminder before the event starts.'),
 (N'event.remind.submit',      N'Đăng ký nhắc',                              N'Subscribe'),
 (N'event.remind.subscribed',  N'Đã đăng ký nhắc lịch',                      N'Reminder set'),
 (N'event.remind.success',     N'Đã đăng ký nhắc lịch.',                     N'Your reminder has been set.'),
 (N'event.reg.title',          N'Đăng ký tham gia',                          N'Register to attend'),
 (N'event.reg.submit',         N'Đăng ký',                                   N'Register'),
 (N'event.reg.success',        N'Đăng ký thành công. Email xác nhận kèm vé đã được gửi.', N'Registration successful. A confirmation email with your ticket has been sent.'),
 (N'event.reg.already',        N'Anh/chị đã đăng ký sự kiện này.',           N'You have already registered for this event.'),
 (N'event.reg.viewTicket',     N'Xem vé (mã QR)',                            N'View ticket (QR code)'),
 (N'event.reg.closed',         N'Sự kiện không mở đăng ký trực tuyến.',      N'Online registration is not open for this event.'),
 (N'event.reg.ended',          N'Sự kiện đã diễn ra.',                       N'This event has already taken place.'),
 (N'event.reg.deadlinePassed', N'Đã hết hạn đăng ký.',                       N'The registration deadline has passed.'),
 (N'event.reg.full',           N'Sự kiện đã đủ số lượng đăng ký.',           N'This event is fully booked.'),
 (N'event.form.fullName',      N'Họ tên',                                    N'Full name'),
 (N'event.form.phone',         N'Điện thoại',                                N'Phone'),
 (N'event.form.organization',  N'Đơn vị',                                    N'Organization'),
 (N'event.form.note',          N'Ghi chú',                                   N'Note'),
 (N'event.booth.title',        N'Gian hàng tại sự kiện',                     N'Exhibition booths'),
 (N'event.booth.type',         N'Loại gian hàng',                            N'Booth type'),
 (N'event.booth.position',     N'Vị trí',                                    N'Location'),
 (N'event.booth.price',        N'Giá',                                       N'Price'),
 (N'event.booth.services',     N'Dịch vụ đi kèm',                            N'Included services'),
 (N'event.booth.remaining',    N'Còn trống',                                 N'Available'),
 (N'event.booth.book',         N'Đăng ký thuê gian hàng',                    N'Book a booth'),
 (N'event.booth.submit',       N'Gửi đăng ký thuê',                          N'Send booking request'),
 (N'event.booth.success',      N'Đã gửi đăng ký thuê gian hàng. Sàn sẽ liên hệ xác nhận.', N'Your booth request has been sent. The Exchange will contact you to confirm.'),
 (N'event.err.notFound',       N'Không tìm thấy sự kiện.',                   N'Event not found.'),
 (N'event.err.name',           N'Vui lòng nhập họ tên / đơn vị.',            N'Please enter your name / organization.'),
 (N'event.err.email',          N'Email không hợp lệ.',                       N'Invalid email address.'),
 (N'event.err.phone',          N'Số điện thoại không hợp lệ.',               N'Invalid phone number.'),
 (N'event.err.tooMany',        N'Quá nhiều lượt đăng ký, vui lòng thử lại sau.', N'Too many registrations, please try again later.'),
 (N'event.err.alreadyRegistered', N'Email này đã đăng ký sự kiện.',          N'This email is already registered for the event.'),
 (N'event.err.boothFull',      N'Loại gian hàng này không còn đủ chỗ trống.', N'Not enough booths of this type are available.'),
 (N'event.ticket.title',       N'Vé tham dự sự kiện',                        N'Event ticket'),
 (N'event.ticket.valid',       N'Hợp lệ',                                    N'Valid'),
 (N'event.ticket.checkedIn',   N'Đã điểm danh',                              N'Checked in'),
 (N'event.ticket.cancelled',   N'Đã hủy',                                    N'Cancelled'),
 (N'event.ticket.hint',        N'Xuất trình mã QR này tại bàn đón tiếp để điểm danh.', N'Show this QR code at the reception desk to check in.'),
 (N'event.ticket.print',       N'In vé',                                     N'Print ticket');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-dot3-su-kien' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
GO

-- ============================== seed-i18n-dot4-hsmt.sql ==============================
PRINT N'>> seed-i18n-dot4-hsmt.sql';
GO
-- Seed i18n cho Đợt 4: huy hiệu hạng nhà cung cấp, vị trí trưng bày, liên kết đăng ký bản tin. Idempotent.
-- Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -I -i seed-i18n-dot4-hsmt.sql
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'supplier.tier.label',      N'Hạng nhà cung cấp',     N'Supplier tier'),
 (N'supplier.tier.1',          N'Thành viên',            N'Member'),
 (N'supplier.tier.2',          N'Bạc',                   N'Silver'),
 (N'supplier.tier.3',          N'Vàng',                  N'Gold'),
 (N'product.display.position', N'Vị trí trưng bày',      N'Display location'),
 (N'footer.newsletter',        N'Đăng ký nhận bản tin',  N'Newsletter');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-dot4-hsmt' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
GO

-- ============================== seed-i18n-dot5-hsmt.sql ==============================
PRINT N'>> seed-i18n-dot5-hsmt.sql';
GO
-- Seed i18n cho Đợt 5: đồng ý xử lý dữ liệu cá nhân, gọi video trong chat. Idempotent.
-- Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -I -i seed-i18n-dot5-hsmt.sql
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'auth.consent.prefix', N'Tôi đồng ý với',                                  N'I agree to the'),
 (N'auth.consent.policy', N'điều khoản xử lý dữ liệu cá nhân',                N'personal data processing terms'),
 (N'auth.consent.suffix', N'của Sàn giao dịch công nghệ.',                    N'of the Technology Exchange.'),
 (N'chat.video.button',   N'Gọi video',                                       N'Video call'),
 (N'chat.video.confirm',  N'Tạo phòng gọi video và gửi lời mời vào cuộc trò chuyện này?', N'Create a video call room and send the invitation to this conversation?');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-dot5-hsmt' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
GO

-- ============================== seed-i18n-pentest-dot3.sql ==============================
PRINT N'>> seed-i18n-pentest-dot3.sql';
GO
-- Seed i18n cho bản vá pentest MobiFone 10/2026 – Đợt 3:
-- tạm khóa đăng nhập sai nhiều lần + chính sách mật khẩu mạnh. Idempotent.
-- Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -i seed-i18n-pentest-dot3.sql
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'auth.err.tooManyAttempts', N'Bạn đã nhập sai quá nhiều lần. Vui lòng thử lại sau {0} phút.', N'Too many failed attempts. Please try again in {0} minute(s).'),
 (N'auth.err.passwordPolicy',  N'Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt.', N'Password must be at least 8 characters and include uppercase, lowercase, a digit and a special character.'),
 -- Cập nhật các gợi ý cũ "tối thiểu 6 ký tự"
 (N'auth.minChars',            N'Tối thiểu 8 ký tự, gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt', N'At least 8 characters with uppercase, lowercase, a digit and a special character'),
 (N'changepwd.newPasswordPh',  N'Nhập mật khẩu mới (tối thiểu 8 ký tự)',             N'Enter new password (min. 8 characters)'),
 (N'changepwd.note1',          N'Mật khẩu mới phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt', N'New password must be at least 8 characters with uppercase, lowercase, a digit and a special character'),
 (N'auth.reset.err.passwordMin', N'Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt.', N'Password must be at least 8 characters and include uppercase, lowercase, a digit and a special character.');

UPDATE t SET t.Vi = s.vi, t.En = s.en
FROM dbo.UiTranslations t JOIN @s s ON t.[Key] = s.k
WHERE t.Vi <> s.vi OR t.En <> s.en;
DECLARE @updated INT = @@ROWCOUNT;

INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-pentest-dot3' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @updated AS Updated, @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
GO

/* ############################################################################
   3. BÀI HƯỚNG DẪN SỬ DỤNG CÁC CHỨC NĂNG MỚI
   ############################################################################ */

-- ============================== seed-huong-dan-su-dung-hsmt.sql ==============================
PRINT N'>> seed-huong-dan-su-dung-hsmt.sql';
GO
/*
Bổ sung bài hướng dẫn sử dụng (/huong-dan) cho các chức năng mới theo HSMT (Đợt 1–6).

- Thêm bài vào menu con có sẵn của cây 'huong-dan-su-dung': 'nguoi-mua-hang', 'nha-cung-ung'.
- Upsert theo QueryString; Creator = 'huong-dan-hsmt' để seed-huong-dan-su-dung-v2.sql không ẩn các bài này khi chạy lại.
- Chạy sau seed-huong-dan-su-dung(-v2).sql. Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -I -i seed-huong-dan-su-dung-hsmt.sql
*/
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
SET NOCOUNT ON;

BEGIN TRANSACTION;

DECLARE @now DATETIME = SYSDATETIME();
DECLARE @rootId INT = (SELECT TOP 1 MenuId FROM Menu WHERE QueryString = 'huong-dan-su-dung' AND ParentId = 0 ORDER BY MenuId);
IF @rootId IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR(N'Chưa có cây menu huong-dan-su-dung — chạy seed-huong-dan-su-dung-v2.sql trước.', 16, 1);
    RETURN;
END

DECLARE @Articles TABLE
(
    QueryString NVARCHAR(300) NOT NULL PRIMARY KEY,
    MenuQueryString NVARCHAR(300) NOT NULL,
    Title NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Body NVARCHAR(MAX) NOT NULL
);

INSERT INTO @Articles (QueryString, MenuQueryString, Title, Description, Body) VALUES
(N'tai-khoan-quen-mat-khau-va-bao-mat', N'nguoi-mua-hang',
 N'Tài khoản: quên mật khẩu, bảo mật và dữ liệu cá nhân',
 N'Cách tự lấy lại mật khẩu, điều khoản dữ liệu cá nhân khi đăng ký và khi tài khoản bị tạm khóa.',
 N'<h2>Quên mật khẩu</h2>
<p>Tại trang đăng nhập, bấm <strong>Quên mật khẩu?</strong> rồi nhập email hoặc tên đăng nhập. Hệ thống gửi liên kết đặt lại mật khẩu tới email đã đăng ký.</p>
<ul>
<li>Liên kết có hiệu lực <strong>30 phút</strong> và chỉ dùng được một lần.</li>
<li>Không thấy thư: kiểm tra mục thư rác, hoặc đợi vài phút rồi gửi lại.</li>
<li>Không còn dùng được email đã đăng ký: liên hệ Trung tâm để được hỗ trợ.</li>
</ul>
<h2>Đồng ý xử lý dữ liệu cá nhân</h2>
<p>Khi đăng ký tài khoản, bạn cần đánh dấu đồng ý điều khoản xử lý dữ liệu cá nhân của Sàn. Hệ thống ghi nhận thời điểm và phiên bản điều khoản bạn đã đồng ý.</p>
<h2>Tài khoản bị tạm khóa</h2>
<p>Nếu đăng nhập báo tài khoản đã bị tạm khóa, vui lòng liên hệ Ban quản lý Sàn để biết lý do và được hỗ trợ mở khóa.</p>'),

(N'quan-tam-binh-luan-va-ma-qr-san-pham', N'nguoi-mua-hang',
 N'Lưu sản phẩm quan tâm, bình luận, mã QR và tìm kiếm',
 N'Các tiện ích trên trang sản phẩm, tin bài, trang nhà cung cấp và ô tìm kiếm.',
 N'<h2>Sản phẩm quan tâm</h2>
<p>Trên trang chi tiết sản phẩm, bấm <strong>Quan tâm</strong> để lưu lại. Danh sách sản phẩm đã lưu nằm ở mục <strong>Sản phẩm quan tâm</strong> trong menu tài khoản; bấm <strong>Bỏ quan tâm</strong> để xóa khỏi danh sách.</p>
<h2>Bình luận</h2>
<p>Bạn có thể bình luận ở trang sản phẩm, trang tin bài và trang nhà cung cấp sau khi đăng nhập. Bình luận hiển thị ngay; bạn xóa được bình luận của mình. Bình luận vi phạm có thể bị Ban quản lý ẩn.</p>
<h2>Mã QR sản phẩm</h2>
<p>Cột bên phải trang sản phẩm có mã QR dẫn tới trang đó và nút <strong>In mã QR</strong> để in nhãn dán tại gian trưng bày.</p>
<h2>Hình thức trưng bày</h2>
<p>Trang <strong>Sản phẩm</strong> có bộ lọc <strong>Trực tuyến</strong> / <strong>Trưng bày tại Sàn</strong>. Sản phẩm trưng bày tại Sàn hiển thị vị trí trưng bày ở trang chi tiết.</p>
<h2>Tìm kiếm bằng giọng nói</h2>
<p>Bấm biểu tượng micro trong ô tìm kiếm, cho phép trình duyệt dùng micro rồi nói từ khóa. Hệ thống tự điền và tìm. Tính năng hoạt động trên Chrome và Edge.</p>'),

(N'dang-ky-tham-gia-su-kien', N'nguoi-mua-hang',
 N'Đăng ký tham gia sự kiện và nhận nhắc lịch',
 N'Đăng ký tham dự, nhận vé QR, đăng ký nhắc lịch và xem phát trực tiếp.',
 N'<h2>Đăng ký tham gia</h2>
<p>Mở tin sự kiện, điền form <strong>Đăng ký tham gia</strong> (họ tên, email bắt buộc). Bạn không cần đăng nhập; nếu đã đăng nhập, thông tin được điền sẵn.</p>
<h2>Vé và điểm danh</h2>
<p>Sau khi đăng ký, hệ thống gửi email xác nhận kèm liên kết <strong>vé có mã QR</strong>. Mang vé (in hoặc trên điện thoại) đến sự kiện để cán bộ quét điểm danh.</p>
<h2>Quan tâm và nhắc lịch</h2>
<ul>
<li><strong>Quan tâm</strong>: đánh dấu sự kiện bạn quan tâm.</li>
<li><strong>Nhắc lịch</strong>: nhập email (và số điện thoại nếu muốn), hệ thống gửi nhắc trước giờ diễn ra. Người đã đăng ký tham gia được nhắc tự động.</li>
</ul>
<h2>Xem trực tuyến</h2>
<p>Nếu sự kiện có phát trực tiếp, nút <strong>Xem phát trực tiếp</strong> hiển thị trong khối thông tin sự kiện.</p>'),

(N'goi-video-hoi-dap-va-nhan-ban-tin', N'nguoi-mua-hang',
 N'Gọi video với nhà cung cấp, câu hỏi thường gặp và nhận bản tin',
 N'Trao đổi trực tuyến bằng video, tra cứu câu hỏi thường gặp và đăng ký nhận bản tin.',
 N'<h2>Gọi video</h2>
<p>Trong <strong>Tin nhắn</strong>, mở cuộc trò chuyện rồi bấm <strong>Gọi video</strong>. Hệ thống tạo phòng họp trực tuyến, mở trong tab mới và gửi lời mời vào cuộc trò chuyện để bên kia bấm tham gia. Cán bộ Sàn có thể cùng tham gia khi cần hỗ trợ.</p>
<h2>Câu hỏi thường gặp</h2>
<p>Liên kết <strong>Câu hỏi thường gặp</strong> ở chân trang tổng hợp giải đáp theo nhóm; có ô tìm kiếm câu hỏi.</p>
<h2>Đăng ký nhận bản tin</h2>
<p>Bấm <strong>Đăng ký nhận bản tin</strong> ở chân trang, điền họ tên, email và lĩnh vực quan tâm để nhận tin công nghệ, sự kiện, nhu cầu tìm mua mới.</p>'),

(N'goi-gian-hang-hang-nha-cung-cap-va-thong-ke', N'nha-cung-ung',
 N'Gói gian hàng, hạng nhà cung cấp và thống kê lượt xem, liên hệ',
 N'Hạn mức số sản phẩm theo gói, hạng của nhà cung cấp và số liệu theo dõi gian hàng.',
 N'<h2>Gói gian hàng và hạn mức</h2>
<p>Trang <strong>Quản lý sản phẩm</strong> hiển thị số sản phẩm đã đăng so với hạn mức của gói gian hàng (ví dụ 4 / 10 sản phẩm) và ngày hết hạn gói. Khi hết hạn mức, liên hệ Sàn để nâng gói.</p>
<h2>Hạng nhà cung cấp</h2>
<p>Sàn xếp hạng nhà cung cấp (Vàng, Bạc, Thành viên) dựa trên số sản phẩm đang xuất bản, lượt liên hệ, lượt xem và việc xác minh hồ sơ pháp lý. Hạng hiển thị trên trang gian hàng của bạn. Tải đủ giấy tờ xác minh trong mục hồ sơ để đủ điều kiện xét hạng cao.</p>
<h2>Thống kê lượt xem và liên hệ</h2>
<p>Danh sách sản phẩm có cột <strong>Lượt xem</strong> và <strong>Liên hệ</strong> (số cuộc trò chuyện và yêu cầu đặt mua/quan tâm), cùng tổng số ở đầu bảng.</p>'),

(N'gui-bai-viet-cho-san-duyet', N'nha-cung-ung',
 N'Gửi bài viết giới thiệu để Sàn duyệt',
 N'Nhà cung cấp soạn bài giới thiệu sản phẩm, giải pháp và gửi Sàn duyệt xuất bản.',
 N'<h2>Soạn bài</h2>
<p>Vào menu tài khoản → <strong>Bài viết của tôi</strong> → <strong>Viết bài mới</strong>. Nhập tiêu đề, tóm tắt và nội dung (để một dòng trống giữa các đoạn).</p>
<h2>Lưu nháp hoặc gửi duyệt</h2>
<ul>
<li><strong>Lưu nháp</strong>: lưu lại để sửa tiếp.</li>
<li><strong>Gửi duyệt</strong>: bài chuyển sang trạng thái chờ duyệt; Sàn có thể bổ sung ảnh, định dạng trước khi xuất bản.</li>
<li>Bài bị trả về sẽ hiện trạng thái <strong>Bị trả về</strong>; sửa rồi gửi duyệt lại.</li>
</ul>'),

(N'dang-ky-thue-gian-hang-su-kien', N'nha-cung-ung',
 N'Đăng ký thuê gian hàng tại sự kiện',
 N'Xem loại gian hàng, giá, dịch vụ đi kèm và gửi đăng ký thuê.',
 N'<h2>Xem gian hàng</h2>
<p>Mở tin sự kiện có mục <strong>Gian hàng tại sự kiện</strong>: bảng liệt kê loại gian hàng (diện tích, vị trí), giá, dịch vụ đi kèm và số gian còn trống.</p>
<h2>Gửi đăng ký</h2>
<p>Bấm <strong>Đăng ký thuê gian hàng</strong>, chọn loại, số lượng và điền thông tin đơn vị, người liên hệ. Đăng ký ở trạng thái chờ duyệt; Sàn sẽ liên hệ xác nhận.</p>');

UPDATE c
SET c.Title = src.Title, c.Description = src.Description, c.Contents = src.Body, c.MenuId = m.MenuId,
    c.StatusId = 3, c.Modified = @now, c.Modifier = 'huong-dan-hsmt', c.Creator = 'huong-dan-hsmt',
    c.PublishedDate = COALESCE(c.PublishedDate, @now), c.LanguageId = 1, c.Domain = 'VN', c.SiteId = 1
FROM Contents c
JOIN @Articles src ON src.QueryString = c.QueryString
JOIN Menu m ON m.ParentId = @rootId AND m.QueryString = src.MenuQueryString;

INSERT INTO Contents (Title, QueryString, Description, Contents, StatusId, MenuId, Created, Creator, PublishedDate, LanguageId, Domain, SiteId)
SELECT src.Title, src.QueryString, src.Description, src.Body, 3, m.MenuId, @now, 'huong-dan-hsmt', @now, 1, 'VN', 1
FROM @Articles src
JOIN Menu m ON m.ParentId = @rootId AND m.QueryString = src.MenuQueryString
WHERE NOT EXISTS (SELECT 1 FROM Contents c WHERE c.QueryString = src.QueryString);

COMMIT TRANSACTION;

SELECT m.QueryString AS Menu, c.Id, c.Title FROM Contents c JOIN Menu m ON m.MenuId = c.MenuId
WHERE c.Creator = 'huong-dan-hsmt' ORDER BY m.Sort, c.Id;
GO

PRINT N'== HOÀN TẤT: đã áp dụng đủ các script triển khai ==';
GO
