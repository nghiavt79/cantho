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
