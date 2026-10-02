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
