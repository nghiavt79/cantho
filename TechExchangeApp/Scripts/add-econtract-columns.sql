/* ============================================================================
   Tích hợp eContract MobiFone - Giai đoạn 1 (docs/PLAN_EContract_USBToken.md).
   Idempotent script, no EF migration. Không sửa dữ liệu chữ ký cũ.
   PHẢI chạy trước khi deploy code có các cột mới của ProjectContracts.
   Chạy: sqlcmd -f 65001 -i add-econtract-columns.sql (file UTF-8 không BOM).
   ============================================================================ */

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---- ProjectContracts ---------------------------------------------------- */
IF COL_LENGTH('dbo.ProjectContracts', 'SigningProvider') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD SigningProvider NVARCHAR(30) NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'EContractEnv') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD EContractEnv NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'EContractId') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD EContractId BIGINT NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'EContractNo') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD EContractNo NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'EContractStatus') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD EContractStatus INT NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'IntegrationState') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD IntegrationState INT NOT NULL
        CONSTRAINT DF_ProjectContracts_IntegrationState DEFAULT (0);
IF COL_LENGTH('dbo.ProjectContracts', 'FrozenPdfPath') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD FrozenPdfPath NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'FrozenPdfSha256') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD FrozenPdfSha256 NVARCHAR(128) NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'EContractIssuedAt') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD EContractIssuedAt DATETIME2 NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'EContractSyncedAt') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD EContractSyncedAt DATETIME2 NULL;
IF COL_LENGTH('dbo.ProjectContracts', 'EContractCompletedAt') IS NULL
    ALTER TABLE dbo.ProjectContracts ADD EContractCompletedAt DATETIME2 NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_ProjectContracts_EContractId' AND object_id = OBJECT_ID('dbo.ProjectContracts'))
    CREATE UNIQUE INDEX UX_ProjectContracts_EContractId
        ON dbo.ProjectContracts (EContractEnv, EContractId)
        WHERE EContractId IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_ProjectContracts_EContractNo' AND object_id = OBJECT_ID('dbo.ProjectContracts'))
    CREATE UNIQUE INDEX UX_ProjectContracts_EContractNo
        ON dbo.ProjectContracts (EContractEnv, EContractNo)
        WHERE EContractNo IS NOT NULL;
GO

/* ---- EContractRecipients: người ký, snapshot lúc phát hành ------------------ */
IF OBJECT_ID('dbo.EContractRecipients', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EContractRecipients
    (
        Id                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EContractRecipients PRIMARY KEY,
        ProjectContractId INT NOT NULL
            CONSTRAINT FK_EContractRecipients_ProjectContracts REFERENCES dbo.ProjectContracts (Id),
        UserId            INT NULL,
        BusinessRole      INT NOT NULL,          -- 1=Buyer (Bên B), 2=Seller (Bên A)
        RecipientId       BIGINT NULL,           -- id phía eContract, chưa rõ response có trả không
        FullName          NVARCHAR(200) NOT NULL,
        Email             NVARCHAR(200) NULL,
        Phone             NVARCHAR(30) NULL,
        CardId            NVARCHAR(50) NULL,     -- MST/CCCD
        Ordering          INT NOT NULL CONSTRAINT DF_EContractRecipients_Ordering DEFAULT (1),
        Status            INT NULL,
        CreatedDate       DATETIME2 NOT NULL CONSTRAINT DF_EContractRecipients_CreatedDate DEFAULT (SYSUTCDATETIME()),
        ModifiedDate      DATETIME2 NULL
    );
    CREATE INDEX IX_EContractRecipients_Contract ON dbo.EContractRecipients (ProjectContractId);
    PRINT 'Created table EContractRecipients.';
END
GO

/* ---- EContractFields: vùng ký đã gửi lên eContract ------------------------- */
IF OBJECT_ID('dbo.EContractFields', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EContractFields
    (
        Id                   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EContractFields PRIMARY KEY,
        EContractRecipientId INT NOT NULL
            CONSTRAINT FK_EContractFields_Recipients REFERENCES dbo.EContractRecipients (Id),
        FieldId              BIGINT NULL,
        DocumentId           BIGINT NULL,
        FieldType            INT NOT NULL CONSTRAINT DF_EContractFields_FieldType DEFAULT (3),
        Page                 INT NOT NULL,
        CoordinateX          FLOAT NOT NULL,
        CoordinateY          FLOAT NOT NULL,     -- cộng dồn theo trang
        Width                FLOAT NOT NULL,
        Height               FLOAT NOT NULL
    );
    CREATE INDEX IX_EContractFields_Recipient ON dbo.EContractFields (EContractRecipientId);
    PRINT 'Created table EContractFields.';
END
GO

/* ---- EContractOperations: checkpoint chống tạo trùng và đối soát ------------ */
IF OBJECT_ID('dbo.EContractOperations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EContractOperations
    (
        Id                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EContractOperations PRIMARY KEY,
        OperationKey      NVARCHAR(200) NOT NULL,
        Action            NVARCHAR(50) NOT NULL,
        ProjectContractId INT NOT NULL
            CONSTRAINT FK_EContractOperations_ProjectContracts REFERENCES dbo.ProjectContracts (Id),
        InputSha256       NVARCHAR(128) NULL,
        State             INT NOT NULL CONSTRAINT DF_EContractOperations_State DEFAULT (0), -- 0=Pending,1=Succeeded,2=Failed,3=Unknown
        CorrelationId     NVARCHAR(64) NULL,
        Error             NVARCHAR(1000) NULL,   -- đã lược dữ liệu nhạy cảm
        LeaseUntil        DATETIME2 NULL,
        CreatedDate       DATETIME2 NOT NULL CONSTRAINT DF_EContractOperations_CreatedDate DEFAULT (SYSUTCDATETIME()),
        ModifiedDate      DATETIME2 NULL,
        CONSTRAINT UX_EContractOperations_Key UNIQUE (OperationKey)
    );
    CREATE INDEX IX_EContractOperations_Contract ON dbo.EContractOperations (ProjectContractId);
    PRINT 'Created table EContractOperations.';
END
GO

/* ---- SYS_PARAMETERS: chỉ thêm nếu chưa có, không ghi đè giá trị đã cấu hình -- */
DECLARE @p TABLE (Name VARCHAR(100), Val NVARCHAR(500), Descr NVARCHAR(500));
INSERT INTO @p (Name, Val, Descr) VALUES
('ECONTRACT_ENABLED',        '0', N'Bật gửi hợp đồng sang eContract MobiFone: 1 = bật, 0 = tắt'),
('ECONTRACT_ENV',            'PRODUCTION', N'Tên môi trường eContract, lưu kèm mã hợp đồng để đối soát'),
('ECONTRACT_BASE_URL',       'https://mobifone-econtract.vn/service', N'Địa chỉ eContract, không gồm /api/v1'),
('ECONTRACT_EMAIL',          '', N'Tài khoản tích hợp eContract (username trong mail tạo tài khoản)'),
('ECONTRACT_PASSWORD',       '', N'Mật khẩu tài khoản tích hợp eContract, lưu đã mã hóa qua /Secure/Encrypt'),
('ECONTRACT_TYPE_ID',        '', N'Mã Loại tài liệu trên eContract, để trống nếu không dùng'),
('ECONTRACT_ORG_ID',         '13592', N'Mã tổ chức của Trung tâm trên eContract'),
('ECONTRACT_DOWNLOAD_HOSTS', '', N'Các host được phép tải PDF đã ký ngoài host eContract, cách nhau dấu phẩy');

INSERT INTO dbo.SYS_PARAMETERS (Name, Val, [Description], Domain, Activated)
SELECT p.Name, p.Val, p.Descr, N'CONTRACT', 1
FROM @p p
WHERE NOT EXISTS (SELECT 1 FROM dbo.SYS_PARAMETERS s WHERE s.Name = p.Name);
GO

PRINT 'add-econtract-columns: done.';
GO
