/* ============================================================================
   Support chat columns for ChatConversations.
   Idempotent script, no EF migration.
   Ticket layer now allows multiple active support requests per project/requester,
   so the old unique active-support index is dropped if present.
   ============================================================================ */

SET NOCOUNT ON;

IF COL_LENGTH('dbo.ChatConversations', 'ConversationType') IS NULL
BEGIN
    ALTER TABLE dbo.ChatConversations
        ADD ConversationType INT NOT NULL
        CONSTRAINT DF_ChatConversations_ConversationType DEFAULT (1);
    PRINT 'Added column ConversationType.';
END

IF COL_LENGTH('dbo.ChatConversations', 'ProjectId') IS NULL
BEGIN
    ALTER TABLE dbo.ChatConversations ADD ProjectId INT NULL;
    PRINT 'Added column ProjectId.';
END

IF COL_LENGTH('dbo.ChatConversations', 'StepNumber') IS NULL
BEGIN
    ALTER TABLE dbo.ChatConversations ADD StepNumber INT NULL;
    PRINT 'Added column StepNumber.';
END

IF COL_LENGTH('dbo.ChatConversations', 'SupportStatus') IS NULL
BEGIN
    ALTER TABLE dbo.ChatConversations
        ADD SupportStatus INT NOT NULL
        CONSTRAINT DF_ChatConversations_SupportStatus DEFAULT (0);
    PRINT 'Added column SupportStatus.';
END

IF COL_LENGTH('dbo.ChatConversations', 'AssignedStaffUserId') IS NULL
BEGIN
    ALTER TABLE dbo.ChatConversations ADD AssignedStaffUserId INT NULL;
    PRINT 'Added column AssignedStaffUserId.';
END
GO

UPDATE dbo.ChatConversations
SET ConversationType = 1
WHERE ConversationType IS NULL;

UPDATE dbo.ChatConversations
SET SupportStatus = 0
WHERE SupportStatus IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_ChatConversations_Type_Project_Buyer'
                 AND object_id = OBJECT_ID('dbo.ChatConversations'))
BEGIN
    CREATE INDEX IX_ChatConversations_Type_Project_Buyer
        ON dbo.ChatConversations (ConversationType, ProjectId, BuyerUserId);
    PRINT 'Created index IX_ChatConversations_Type_Project_Buyer.';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_ChatConversations_Type_Status_LastMessage'
                 AND object_id = OBJECT_ID('dbo.ChatConversations'))
BEGIN
    CREATE INDEX IX_ChatConversations_Type_Status_LastMessage
        ON dbo.ChatConversations (ConversationType, SupportStatus, LastMessageAt);
    PRINT 'Created index IX_ChatConversations_Type_Status_LastMessage.';
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE name = 'UX_ChatConversations_Support_Active'
             AND object_id = OBJECT_ID('dbo.ChatConversations'))
BEGIN
    DROP INDEX UX_ChatConversations_Support_Active ON dbo.ChatConversations;
    PRINT 'Dropped obsolete unique index UX_ChatConversations_Support_Active.';
END
GO

PRINT 'Done: add-support-chat-columns.sql';
