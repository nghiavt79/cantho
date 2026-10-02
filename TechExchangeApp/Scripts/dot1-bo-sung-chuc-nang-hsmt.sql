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
