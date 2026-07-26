/*
Đặt lại tên SysFunction:
- FunctionName = tên tiếng Anh / kỹ thuật (khớp controller/entity, tiện cho log & code)
- HrefName     = tên tiếng Việt (nhãn hiển thị trên menu)
UPDATE theo FunctionId nên KHÔNG xóa dữ liệu phân quyền.

⚠ FILE UTF-8 — chạy kèm codepage UTF-8 để tiếng Việt không lỗi:
    sqlcmd -S localhost -U sa -P *** -d TechExchangeNew -f 65001 -i relabel-sysfunction-names.sql
*/
SET NOCOUNT ON;

UPDATE dbo.SysFunction SET FunctionName = N'Dashboard',            HrefName = N'Dashboard'            WHERE FunctionId = 1;

UPDATE dbo.SysFunction SET FunctionName = N'Transaction',          HrefName = N'Giao dịch'            WHERE FunctionId = 10;
UPDATE dbo.SysFunction SET FunctionName = N'Projects',             HrefName = N'Dự án'                WHERE FunctionId = 11;
UPDATE dbo.SysFunction SET FunctionName = N'PhieuYeuCauCNTB',      HrefName = N'Phiếu yêu cầu'        WHERE FunctionId = 12;
UPDATE dbo.SysFunction SET FunctionName = N'ContentsYeuCau',       HrefName = N'Tìm mua công khai'    WHERE FunctionId = 13;
UPDATE dbo.SysFunction SET FunctionName = N'Feedback',             HrefName = N'Phản hồi'             WHERE FunctionId = 14;

UPDATE dbo.SysFunction SET FunctionName = N'ProductPartner',       HrefName = N'Sản phẩm & Đối tác'   WHERE FunctionId = 20;
UPDATE dbo.SysFunction SET FunctionName = N'Technology',           HrefName = N'Công nghệ'            WHERE FunctionId = 21;
UPDATE dbo.SysFunction SET FunctionName = N'Equipment',            HrefName = N'Thiết bị'             WHERE FunctionId = 22;
UPDATE dbo.SysFunction SET FunctionName = N'IntellectualProperty', HrefName = N'Sản phẩm trí tuệ'     WHERE FunctionId = 23;
UPDATE dbo.SysFunction SET FunctionName = N'OCOP',                 HrefName = N'OCOP'                 WHERE FunctionId = 24;
UPDATE dbo.SysFunction SET FunctionName = N'Supplier',             HrefName = N'Nhà cung ứng'         WHERE FunctionId = 25;
UPDATE dbo.SysFunction SET FunctionName = N'Consultant',           HrefName = N'Nhà tư vấn'           WHERE FunctionId = 26;

UPDATE dbo.SysFunction SET FunctionName = N'WebContent',           HrefName = N'Nội dung website'     WHERE FunctionId = 30;
UPDATE dbo.SysFunction SET FunctionName = N'Posts',                HrefName = N'Tin bài'              WHERE FunctionId = 31;
UPDATE dbo.SysFunction SET FunctionName = N'Menu',                 HrefName = N'Menu website'         WHERE FunctionId = 32;
UPDATE dbo.SysFunction SET FunctionName = N'Category',             HrefName = N'Danh mục'             WHERE FunctionId = 33;
UPDATE dbo.SysFunction SET FunctionName = N'Advertisement',        HrefName = N'Banner quảng cáo'     WHERE FunctionId = 34;

UPDATE dbo.SysFunction SET FunctionName = N'System',               HrefName = N'Quản trị hệ thống'    WHERE FunctionId = 40;
UPDATE dbo.SysFunction SET FunctionName = N'Users',                HrefName = N'Người dùng'           WHERE FunctionId = 41;
UPDATE dbo.SysFunction SET FunctionName = N'Log',                  HrefName = N'Nhật ký'              WHERE FunctionId = 42;
UPDATE dbo.SysFunction SET FunctionName = N'SystemParameter',      HrefName = N'Tham số hệ thống'     WHERE FunctionId = 43;
UPDATE dbo.SysFunction SET FunctionName = N'SysFunction',          HrefName = N'Tính năng hệ thống'   WHERE FunctionId = 44;
UPDATE dbo.SysFunction SET FunctionName = N'Role',                 HrefName = N'Nhóm quyền (Role)'    WHERE FunctionId = 45;
UPDATE dbo.SysFunction SET FunctionName = N'Permission',           HrefName = N'Phân quyền'           WHERE FunctionId = 46;

PRINT 'Relabeled SysFunction: FunctionName=English, HrefName=Vietnamese.';
