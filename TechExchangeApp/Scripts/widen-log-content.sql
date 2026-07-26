/*
Nới cột Log.Content từ nvarchar(400) → nvarchar(2000) để lưu đầy đủ nội dung log
(snapshot old->new của các thao tác dài hơn 200 ký tự, tránh lỗi
"String or binary data would be truncated").
Dùng 2000 (không dùng MAX) vì Content nằm trong INCLUDE của index IX_Log_SiteId_ActTime*.
An toàn, chạy lại được (idempotent).
*/
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Log')
      AND name = 'Content'
      AND max_length <> -1        -- bỏ qua nếu đang là MAX
      AND max_length < 4000       -- 2000 ký tự * 2 byte
)
BEGIN
    ALTER TABLE dbo.Log ALTER COLUMN Content nvarchar(2000) NULL;
    PRINT 'Log.Content -> nvarchar(2000)';
END
ELSE
    PRINT 'Log.Content already >= nvarchar(2000)';
