/*
Thêm cột Users.SessionVersion — vá lỗi "Improper Logout Functionality" (pentest MobiFone 10/2026).
Mỗi cookie đăng nhập mang số phiên lúc đăng nhập; khi đăng xuất / đổi / đặt lại mật khẩu
số này tăng lên nên cookie cũ (kể cả bị đánh cắp) không dùng lại được.
Chạy TRƯỚC khi deploy code mới. An toàn, chạy lại được (idempotent).
*/
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'SessionVersion'
)
BEGIN
    ALTER TABLE dbo.Users ADD SessionVersion int NOT NULL CONSTRAINT DF_Users_SessionVersion DEFAULT 0;
    PRINT 'Users.SessionVersion added';
END
ELSE
    PRINT 'Users.SessionVersion already exists';
