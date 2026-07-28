/* ============================================================================
   Tính năng "Yêu cầu Sàn hỗ trợ" — bổ sung cột hỗ trợ vào bảng ChatConversations.
   Idempotent: chạy lại nhiều lần không lỗi, không đụng dữ liệu cũ.
   Không dùng EF migration (theo quyết định dự án).
   ============================================================================ */

SET NOCOUNT ON;

/* ── 1. Thêm cột (chỉ khi chưa có) ─────────────────────────────────────────── */

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

/* ── 2. Backfill dữ liệu cũ = chat sản phẩm ────────────────────────────────── */
/* (NOT NULL + DEFAULT đã tự set khi ADD; câu dưới chỉ để chắc chắn với mọi row.) */

UPDATE dbo.ChatConversations
SET ConversationType = 1
WHERE ConversationType IS NULL;

UPDATE dbo.ChatConversations
SET SupportStatus = 0
WHERE SupportStatus IS NULL;
GO

/* ── 3. Index thường ───────────────────────────────────────────────────────── */

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

/* ── 4. Unique filtered index: 1 support active / (dự án, người gửi) ─────────── */
/* Chỉ tạo khi KHÔNG còn duplicate active; nếu có thì cảnh báo, không làm chết deploy. */

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'UX_ChatConversations_Support_Active'
                 AND object_id = OBJECT_ID('dbo.ChatConversations'))
BEGIN
    IF EXISTS (
        SELECT ProjectId, BuyerUserId
        FROM dbo.ChatConversations
        WHERE ConversationType = 2
          AND ProjectId IS NOT NULL
          AND SupportStatus >= 1 AND SupportStatus <= 2
        GROUP BY ProjectId, BuyerUserId
        HAVING COUNT(*) > 1
    )
    BEGIN
        PRINT 'WARNING: Tồn tại nhiều support conversation active trùng (ProjectId, BuyerUserId). '
            + 'Bỏ qua tạo unique index UX_ChatConversations_Support_Active. '
            + 'Hãy dọn trùng rồi chạy lại script.';
    END
    ELSE
    BEGIN
        CREATE UNIQUE INDEX UX_ChatConversations_Support_Active
            ON dbo.ChatConversations (ProjectId, BuyerUserId)
            WHERE ConversationType = 2
              AND ProjectId IS NOT NULL
              AND SupportStatus >= 1 AND SupportStatus <= 2;
        PRINT 'Created unique filtered index UX_ChatConversations_Support_Active.';
    END
END
GO

PRINT 'Done: add-support-chat-columns.sql';
