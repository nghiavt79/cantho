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
