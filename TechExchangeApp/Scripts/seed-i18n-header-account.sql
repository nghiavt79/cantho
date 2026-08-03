-- Seed i18n cho dropdown tài khoản + khối thông báo + aria-label trong _Header.cshtml. Idempotent.
-- LƯU Ý: chạy bằng  sqlcmd ... -f 65001 -i <file>  (file UTF-8 no-BOM, thiếu cờ này dữ liệu vào DB bị mojibake).
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'header.acct.dashboard',    N'Dashboard',            N'Dashboard'),
 (N'header.acct.system',       N'Hệ thống',             N'System'),
 (N'header.acct.myProjects',   N'Dự án của tôi',        N'My projects'),
 (N'header.acct.profile',      N'Thông tin cá nhân',    N'Personal information'),
 (N'header.acct.menuAria',     N'Menu tài khoản',       N'Account menu'),
 (N'header.acct.fallbackName', N'Tài khoản',            N'Account'),
 (N'header.notify.title',      N'Thông báo',            N'Notifications'),
 (N'header.notify.markAllRead', N'Đánh dấu đã đọc',     N'Mark all as read'),
 (N'header.notify.empty',      N'Không có thông báo',   N'No notifications'),
 (N'header.mainMenuAria',      N'Menu chính',           N'Main menu'),
 (N'header.openMenuAria',      N'Mở menu',              N'Open menu'),
 (N'common.close',             N'Đóng',                 N'Close');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
