-- Tài khoản CMS chỉ xem cho đơn vị giám sát (NCC-06; GS-02 mục 2.13)
-- Cần TechExchangeApp.dll bản 27/10/2026 (CmsReadOnlyMiddleware)
-- Chạy: sqlcmd -S <server> -U <user> -P <pwd> -d TechExchangeNew -C -f 65001 -i add-cms-readonly-users.sql

IF NOT EXISTS (SELECT 1 FROM SYS_PARAMETERS WHERE Name = N'CMS_READONLY_USERS')
    INSERT INTO SYS_PARAMETERS (Name, Val, Val2, Type, Description, IsSystem, Activated, Domain, LanguageId, ParentId, SiteId)
    VALUES (N'CMS_READONLY_USERS', N'gs_theodoi', NULL, NULL, N'Tài khoản CMS chỉ được xem (Tổng quan, Nhật ký, Thống kê, Yêu cầu hỗ trợ), không thêm, sửa, xóa; nhiều tài khoản cách nhau dấu phẩy', NULL, 1, N'BAO MAT', 1, 0, NULL);
ELSE
    UPDATE SYS_PARAMETERS SET Val = N'gs_theodoi', Activated = 1 WHERE Name = N'CMS_READONLY_USERS';

SELECT Name, Val, Activated FROM SYS_PARAMETERS WHERE Name = N'CMS_READONLY_USERS';

-- Kiểm tra tài khoản đã có trên máy chủ
SELECT UserId, UserName, FullName, IsAdmin, IsActivated FROM dbo.Users WHERE UserName = N'gs_theodoi';
