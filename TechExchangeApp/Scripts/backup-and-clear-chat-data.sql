/*
    Backup and clear legacy chat/support data before go-live.

    Use only after confirming old chat data is not needed in production.
    This script keeps a timestamped copy in dbo.Backup_* tables, then clears:
    - SupportRequests
    - ChatMessages
    - ChatConversations
*/

SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @Suffix NVARCHAR(32) = FORMAT(SYSUTCDATETIME(), 'yyyyMMddHHmmss');
DECLARE @Sql NVARCHAR(MAX);

IF OBJECT_ID('dbo.SupportRequests', 'U') IS NOT NULL
BEGIN
    SET @Sql = N'SELECT * INTO dbo.Backup_SupportRequests_' + @Suffix + N' FROM dbo.SupportRequests;';
    EXEC sp_executesql @Sql;
END

IF OBJECT_ID('dbo.ChatMessages', 'U') IS NOT NULL
BEGIN
    SET @Sql = N'SELECT * INTO dbo.Backup_ChatMessages_' + @Suffix + N' FROM dbo.ChatMessages;';
    EXEC sp_executesql @Sql;
END

IF OBJECT_ID('dbo.ChatConversations', 'U') IS NOT NULL
BEGIN
    SET @Sql = N'SELECT * INTO dbo.Backup_ChatConversations_' + @Suffix + N' FROM dbo.ChatConversations;';
    EXEC sp_executesql @Sql;
END

IF OBJECT_ID('dbo.SupportRequests', 'U') IS NOT NULL
    DELETE FROM dbo.SupportRequests;

IF OBJECT_ID('dbo.ChatMessages', 'U') IS NOT NULL
    DELETE FROM dbo.ChatMessages;

IF OBJECT_ID('dbo.ChatConversations', 'U') IS NOT NULL
    DELETE FROM dbo.ChatConversations;

IF OBJECT_ID('dbo.SupportRequests', 'U') IS NOT NULL
    DBCC CHECKIDENT ('dbo.SupportRequests', RESEED, 0);

IF OBJECT_ID('dbo.ChatMessages', 'U') IS NOT NULL
    DBCC CHECKIDENT ('dbo.ChatMessages', RESEED, 0);

IF OBJECT_ID('dbo.ChatConversations', 'U') IS NOT NULL
    DBCC CHECKIDENT ('dbo.ChatConversations', RESEED, 0);

COMMIT TRANSACTION;

PRINT 'Backed up and cleared chat/support data.';
