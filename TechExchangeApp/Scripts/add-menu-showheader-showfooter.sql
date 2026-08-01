-- Thêm 2 cột cờ hiển thị cho Menu: ShowHeader / ShowFooter (bit, mặc định 0).
-- ShowHeader = 1 -> mục hiển thị trên thanh header (HeaderMenuService lọc theo cột này).
-- ShowFooter = 1 -> đánh dấu hiển thị footer (footer render động làm sau).
-- Idempotent: chạy lại an toàn.
SET NOCOUNT ON;

IF COL_LENGTH('Menu', 'ShowHeader') IS NULL
    ALTER TABLE Menu ADD ShowHeader bit NOT NULL CONSTRAINT DF_Menu_ShowHeader DEFAULT 0;

IF COL_LENGTH('Menu', 'ShowFooter') IS NULL
    ALTER TABLE Menu ADD ShowFooter bit NOT NULL CONSTRAINT DF_Menu_ShowFooter DEFAULT 0;
GO

-- Migrate: chỉ mục GỐC (ParentId 0/null) đang MenuPosition='1' mới là mục header cấp 1.
-- (Con có MenuPosition='1' vẫn hiện qua ParentId, không tính là mục header riêng.)
UPDATE Menu SET ShowHeader = 0;
UPDATE Menu SET ShowHeader = 1
WHERE LTRIM(RTRIM(ISNULL(MenuPosition, ''))) = '1' AND (ParentId = 0 OR ParentId IS NULL);

SELECT
    (SELECT COUNT(*) FROM Menu WHERE ShowHeader = 1) AS HeaderOn,
    (SELECT COUNT(*) FROM Menu WHERE ShowFooter = 1) AS FooterOn;
