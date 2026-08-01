/* ============================================================================
   SupportRequests - ticket layer for general support and transaction consulting.
   Idempotent script, no EF migration.
   ============================================================================ */

SET NOCOUNT ON;

IF OBJECT_ID('dbo.SupportRequests', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SupportRequests
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupportRequests PRIMARY KEY,
        ProjectId INT NOT NULL,
        RequestedByUserId INT NOT NULL,
        RequestType INT NOT NULL,
        ServiceType INT NULL,
        SupportContextCode NVARCHAR(100) NULL,
        DisplayStepNumber INT NULL,
        InternalStepNumber INT NULL,
        Subject NVARCHAR(300) NULL,
        Description NVARCHAR(MAX) NULL,
        Status INT NOT NULL CONSTRAINT DF_SupportRequests_Status DEFAULT (1),
        AssignedStaffUserId INT NULL,
        ConversationId BIGINT NULL,
        IsPrivateToRequester BIT NOT NULL CONSTRAINT DF_SupportRequests_IsPrivate DEFAULT (1),
        IsChargeable BIT NULL,
        FeePolicy NVARCHAR(200) NULL,
        AssignedAt DATETIME2 NULL,
        FirstRespondedAt DATETIME2 NULL,
        DueAt DATETIME2 NULL,
        LastStatusChangedByUserId INT NULL,
        LastStatusChangedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_SupportRequests_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NULL,
        ClosedAt DATETIME2 NULL
    );
    PRINT 'Created table dbo.SupportRequests.';
END
GO

IF OBJECT_ID('dbo.SupportRequests', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.SupportRequests', 'RequestType') IS NULL
        ALTER TABLE dbo.SupportRequests ADD RequestType INT NOT NULL CONSTRAINT DF_SupportRequests_RequestType DEFAULT (1);

    IF COL_LENGTH('dbo.SupportRequests', 'ServiceType') IS NULL
        ALTER TABLE dbo.SupportRequests ADD ServiceType INT NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'SupportContextCode') IS NULL
        ALTER TABLE dbo.SupportRequests ADD SupportContextCode NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'DisplayStepNumber') IS NULL
        ALTER TABLE dbo.SupportRequests ADD DisplayStepNumber INT NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'InternalStepNumber') IS NULL
        ALTER TABLE dbo.SupportRequests ADD InternalStepNumber INT NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'Subject') IS NULL
        ALTER TABLE dbo.SupportRequests ADD Subject NVARCHAR(300) NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'Description') IS NULL
        ALTER TABLE dbo.SupportRequests ADD Description NVARCHAR(MAX) NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'Status') IS NULL
        ALTER TABLE dbo.SupportRequests ADD Status INT NOT NULL CONSTRAINT DF_SupportRequests_Status DEFAULT (1);

    IF COL_LENGTH('dbo.SupportRequests', 'AssignedStaffUserId') IS NULL
        ALTER TABLE dbo.SupportRequests ADD AssignedStaffUserId INT NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'ConversationId') IS NULL
        ALTER TABLE dbo.SupportRequests ADD ConversationId BIGINT NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'IsPrivateToRequester') IS NULL
        ALTER TABLE dbo.SupportRequests ADD IsPrivateToRequester BIT NOT NULL CONSTRAINT DF_SupportRequests_IsPrivate DEFAULT (1);

    IF COL_LENGTH('dbo.SupportRequests', 'IsChargeable') IS NULL
        ALTER TABLE dbo.SupportRequests ADD IsChargeable BIT NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'FeePolicy') IS NULL
        ALTER TABLE dbo.SupportRequests ADD FeePolicy NVARCHAR(200) NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'AssignedAt') IS NULL
        ALTER TABLE dbo.SupportRequests ADD AssignedAt DATETIME2 NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'FirstRespondedAt') IS NULL
        ALTER TABLE dbo.SupportRequests ADD FirstRespondedAt DATETIME2 NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'DueAt') IS NULL
        ALTER TABLE dbo.SupportRequests ADD DueAt DATETIME2 NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'LastStatusChangedByUserId') IS NULL
        ALTER TABLE dbo.SupportRequests ADD LastStatusChangedByUserId INT NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'LastStatusChangedAt') IS NULL
        ALTER TABLE dbo.SupportRequests ADD LastStatusChangedAt DATETIME2 NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'CreatedAt') IS NULL
        ALTER TABLE dbo.SupportRequests ADD CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_SupportRequests_CreatedAt DEFAULT (SYSUTCDATETIME());

    IF COL_LENGTH('dbo.SupportRequests', 'UpdatedAt') IS NULL
        ALTER TABLE dbo.SupportRequests ADD UpdatedAt DATETIME2 NULL;

    IF COL_LENGTH('dbo.SupportRequests', 'ClosedAt') IS NULL
        ALTER TABLE dbo.SupportRequests ADD ClosedAt DATETIME2 NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SupportRequests_ProjectId' AND object_id = OBJECT_ID('dbo.SupportRequests'))
BEGIN
    CREATE INDEX IX_SupportRequests_ProjectId ON dbo.SupportRequests(ProjectId);
    PRINT 'Created IX_SupportRequests_ProjectId.';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SupportRequests_Assigned_Status' AND object_id = OBJECT_ID('dbo.SupportRequests'))
BEGIN
    CREATE INDEX IX_SupportRequests_Assigned_Status ON dbo.SupportRequests(AssignedStaffUserId, Status);
    PRINT 'Created IX_SupportRequests_Assigned_Status.';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SupportRequests_Type_Status_Created' AND object_id = OBJECT_ID('dbo.SupportRequests'))
BEGIN
    CREATE INDEX IX_SupportRequests_Type_Status_Created ON dbo.SupportRequests(RequestType, Status, CreatedAt);
    PRINT 'Created IX_SupportRequests_Type_Status_Created.';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SupportRequests_ConversationId' AND object_id = OBJECT_ID('dbo.SupportRequests'))
BEGIN
    CREATE INDEX IX_SupportRequests_ConversationId ON dbo.SupportRequests(ConversationId);
    PRINT 'Created IX_SupportRequests_ConversationId.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SupportRequests_Projects_ProjectId')
BEGIN
    ALTER TABLE dbo.SupportRequests
        ADD CONSTRAINT FK_SupportRequests_Projects_ProjectId
        FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id);
    PRINT 'Created FK_SupportRequests_Projects_ProjectId.';
END
GO

IF OBJECT_ID('dbo.ChatConversations', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SupportRequests_ChatConversations_ConversationId')
BEGIN
    ALTER TABLE dbo.SupportRequests
        ADD CONSTRAINT FK_SupportRequests_ChatConversations_ConversationId
        FOREIGN KEY (ConversationId) REFERENCES dbo.ChatConversations(Id);
    PRINT 'Created FK_SupportRequests_ChatConversations_ConversationId.';
END
GO

PRINT 'Done: add-support-request-table.sql';
