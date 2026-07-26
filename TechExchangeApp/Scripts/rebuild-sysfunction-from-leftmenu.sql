/*
Dựng lại bảng SysFunction khớp với left-menu CMS hiện tại (Areas/Cms/Views/Shared/_LayoutAdminLTE.cshtml)
và XÓA SẠCH dữ liệu bảng phân quyền SysFuncRolesStatusPermission.

- FunctionName = tên tiếng Anh / kỹ thuật (khớp controller/entity, tiện cho log & code).
- HrefName     = tên tiếng Việt (nhãn hiển thị).
- Nhóm cha (Transaction, ProductPartner, WebContent, System) không có controller → URL = '#'.
- Mục con dùng URL 'cms/<Controller>[/<Action>]'.
- Tất cả IsMenu=1, IsShow=1, IsStatus=0 (phân quyền theo trang, dùng cột "Chung").

⚠ THAO TÁC GHI ĐÈ/XÓA DỮ LIỆU. Script tự backup 2 bảng trước khi chạy.
Có thể phục hồi từ *_Backup_20260726 nếu cần.
Chạy 1 lần trên TechExchangeNew.

⚠ FILE UTF-8 — phải chạy sqlcmd kèm codepage UTF-8, nếu không tên tiếng Việt sẽ bị lỗi font:
    sqlcmd -S localhost -U sa -P *** -d TechExchangeNew -f 65001 -i rebuild-sysfunction-from-leftmenu.sql
(hoặc mở bằng SSMS rồi Execute — SSMS đọc UTF-8 đúng.)

Lưu ý: nếu chỉ muốn ĐỔI TÊN mà giữ phân quyền, dùng relabel-sysfunction-names.sql (UPDATE, không xóa).
*/

SET NOCOUNT ON;
BEGIN TRAN;

-- 1) Backup (chỉ tạo nếu chưa có, tránh ghi đè bản backup cũ)
IF OBJECT_ID('dbo.SysFunction_Backup_20260726') IS NULL
    SELECT * INTO dbo.SysFunction_Backup_20260726 FROM dbo.SysFunction;

IF OBJECT_ID('dbo.SysFuncRolesStatusPermission_Backup_20260726') IS NULL
    SELECT * INTO dbo.SysFuncRolesStatusPermission_Backup_20260726 FROM dbo.SysFuncRolesStatusPermission;

-- 2) Xóa sạch phân quyền, rồi xóa toàn bộ function cũ
DELETE FROM dbo.SysFuncRolesStatusPermission;
DELETE FROM dbo.SysFunction;

-- 3) Nạp lại function theo left-menu hiện tại (FunctionName=EN, HrefName=VI)
INSERT INTO dbo.SysFunction
    (FunctionId, FunctionName, URL, IsMenu, IsStatus, LanguageId, IsShow, Domain, HrefName, ParentId, Sort, SiteId)
VALUES
    -- Dashboard (đứng lẻ)
    (1,  N'Dashboard',            N'cms/Dashboard',                 1, 0, 1, 1, NULL, N'Dashboard',          0,  1, 1),

    -- Nhóm: Giao dịch
    (10, N'Transaction',          N'#',                             1, 0, 1, 1, NULL, N'Giao dịch',          0,  2, 1),
    (11, N'Projects',             N'cms/ProjectsAdmin',             1, 0, 1, 1, NULL, N'Dự án',              10, 1, 1),
    (12, N'PhieuYeuCauCNTB',      N'cms/PhieuYeuCauCNTBAdmin',      1, 0, 1, 1, NULL, N'Phiếu yêu cầu',      10, 2, 1),
    (13, N'ContentsYeuCau',       N'cms/ContentsYeuCauAdmin',       1, 0, 1, 1, NULL, N'Tìm mua công khai',  10, 3, 1),
    (14, N'Feedback',             N'cms/FeedbackAdmin',             1, 0, 1, 1, NULL, N'Phản hồi',           10, 4, 1),

    -- Nhóm: Sản phẩm & Đối tác
    (20, N'ProductPartner',       N'#',                             1, 0, 1, 1, NULL, N'Sản phẩm & Đối tác', 0,  3, 1),
    (21, N'Technology',           N'cms/SanPhamCNTB/CongNghe',      1, 0, 1, 1, NULL, N'Công nghệ',          20, 1, 1),
    (22, N'Equipment',            N'cms/SanPhamCNTB/ThietBi',       1, 0, 1, 1, NULL, N'Thiết bị',           20, 2, 1),
    (23, N'IntellectualProperty', N'cms/SanPhamCNTB/SanPhamTriTue', 1, 0, 1, 1, NULL, N'Sản phẩm trí tuệ',   20, 3, 1),
    (24, N'OCOP',                 N'cms/SanPhamCNTB/Ocop',          1, 0, 1, 1, NULL, N'OCOP',               20, 4, 1),
    (25, N'Supplier',             N'cms/NhaCungUngAdmin',           1, 0, 1, 1, NULL, N'Nhà cung ứng',       20, 5, 1),
    (26, N'Consultant',           N'cms/NhaTuVanAdmin',             1, 0, 1, 1, NULL, N'Nhà tư vấn',         20, 6, 1),

    -- Nhóm: Nội dung website
    (30, N'WebContent',           N'#',                             1, 0, 1, 1, NULL, N'Nội dung website',   0,  4, 1),
    (31, N'Posts',                N'cms/Posts',                     1, 0, 1, 1, NULL, N'Tin bài',            30, 1, 1),
    (32, N'Menu',                 N'cms/MenuAdmin',                 1, 0, 1, 1, NULL, N'Menu website',       30, 2, 1),
    (33, N'Category',             N'cms/CategoryAdmin',             1, 0, 1, 1, NULL, N'Danh mục',           30, 3, 1),
    (34, N'Advertisement',        N'cms/ImageAdverAdmin',           1, 0, 1, 1, NULL, N'Banner quảng cáo',   30, 4, 1),

    -- Nhóm: Quản trị hệ thống
    (40, N'System',               N'#',                             1, 0, 1, 1, NULL, N'Quản trị hệ thống',  0,  5, 1),
    (41, N'Users',                N'cms/Users',                     1, 0, 1, 1, NULL, N'Người dùng',         40, 1, 1),
    (42, N'Log',                  N'cms/LogAdmin',                  1, 0, 1, 1, NULL, N'Nhật ký',            40, 2, 1),
    (43, N'SystemParameter',      N'cms/SystemParameterAdmin',      1, 0, 1, 1, NULL, N'Tham số hệ thống',   40, 3, 1),
    (44, N'SysFunction',          N'cms/SysFunctionAdmin',          1, 0, 1, 1, NULL, N'Tính năng hệ thống', 40, 4, 1),
    (45, N'Role',                 N'cms/RoleAdmin',                 1, 0, 1, 1, NULL, N'Nhóm quyền (Role)',  40, 5, 1),
    (46, N'Permission',           N'cms/SysFuncPermissionAdmin',    1, 0, 1, 1, NULL, N'Phân quyền',         40, 6, 1);

COMMIT;

DECLARE @f int = (SELECT COUNT(*) FROM dbo.SysFunction);
DECLARE @p int = (SELECT COUNT(*) FROM dbo.SysFuncRolesStatusPermission);
PRINT 'Done. SysFunction rows: ' + CAST(@f AS varchar(10)) + ' | Permission rows: ' + CAST(@p AS varchar(10));
