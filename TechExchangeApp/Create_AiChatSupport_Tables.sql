-- Optional phase-2 tables for AI Support Chat.
-- The MVP chat box works without these tables by searching live website data
-- and saving final consultation requests into the existing Feedback table.

IF OBJECT_ID(N'[dbo].[AiChatSettings]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AiChatSettings] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [IsEnabled] BIT NOT NULL CONSTRAINT [DF_AiChatSettings_IsEnabled] DEFAULT 1,
        [BotName] NVARCHAR(255) NULL,
        [WelcomeMessage] NVARCHAR(MAX) NULL,
        [SystemPrompt] NVARCHAR(MAX) NULL,
        [ModelName] NVARCHAR(100) NULL,
        [MaxMessagesPerSession] INT NOT NULL CONSTRAINT [DF_AiChatSettings_MaxMessages] DEFAULT 30,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_AiChatSettings_CreatedAt] DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 NULL
    );
END;

IF OBJECT_ID(N'[dbo].[AiChatSessions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AiChatSessions] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [SessionKey] NVARCHAR(100) NOT NULL,
        [UserName] NVARCHAR(255) NULL,
        [UserPhone] NVARCHAR(50) NULL,
        [UserEmail] NVARCHAR(255) NULL,
        [Status] NVARCHAR(50) NOT NULL CONSTRAINT [DF_AiChatSessions_Status] DEFAULT 'Open',
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_AiChatSessions_CreatedAt] DEFAULT GETDATE(),
        [LastMessageAt] DATETIME2 NULL
    );

    CREATE INDEX [IX_AiChatSessions_SessionKey] ON [dbo].[AiChatSessions] ([SessionKey]);
END;

-- Ties an anonymous chat session to a logged-in account once the visitor signs in;
-- stays NULL for guests so history keeps working per-browser without an account.
IF COL_LENGTH('dbo.AiChatSessions', 'UserId') IS NULL
BEGIN
    ALTER TABLE [dbo].[AiChatSessions] ADD [UserId] INT NULL;
END;

IF OBJECT_ID(N'[dbo].[AiChatMessages]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AiChatMessages] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [SessionId] BIGINT NOT NULL,
        [Role] NVARCHAR(50) NOT NULL,
        [Content] NVARCHAR(MAX) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_AiChatMessages_CreatedAt] DEFAULT GETDATE(),
        CONSTRAINT [FK_AiChatMessages_AiChatSessions]
            FOREIGN KEY ([SessionId]) REFERENCES [dbo].[AiChatSessions]([Id])
    );
END;

IF OBJECT_ID(N'[dbo].[AiKnowledgeDocuments]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AiKnowledgeDocuments] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [SourceType] NVARCHAR(100) NOT NULL,
        [SourceId] NVARCHAR(100) NOT NULL,
        [SourceSlug] NVARCHAR(500) NULL,
        [Title] NVARCHAR(500) NOT NULL,
        [Url] NVARCHAR(1000) NULL,
        [ContentText] NVARCHAR(MAX) NOT NULL,
        [ContentHash] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_AiKnowledgeDocuments_IsActive] DEFAULT 1,
        [DatasetVersion] NVARCHAR(100) NULL,
        [LastSyncedAt] DATETIME2 NOT NULL CONSTRAINT [DF_AiKnowledgeDocuments_LastSyncedAt] DEFAULT GETDATE()
    );

    CREATE INDEX [IX_AiKnowledgeDocuments_Dataset_Active]
        ON [dbo].[AiKnowledgeDocuments] ([DatasetVersion], [IsActive]);
END;

IF OBJECT_ID(N'[dbo].[AiKnowledgeSyncJobs]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AiKnowledgeSyncJobs] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [DatasetVersion] NVARCHAR(100) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [TotalItems] INT NULL,
        [SuccessItems] INT NULL,
        [FailedItems] INT NULL,
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [StartedAt] DATETIME2 NULL,
        [CompletedAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_AiKnowledgeSyncJobs_CreatedAt] DEFAULT GETDATE()
    );
END;
