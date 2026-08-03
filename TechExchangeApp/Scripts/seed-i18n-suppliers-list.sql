-- Seed i18n cho trang danh sách Nhà cung ứng (Views/NhaCungUng/Index.cshtml). Idempotent.
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'supplier.list.subtitle',   N'Kết nối với các nhà cung ứng công nghệ, thiết bị và giải pháp phục vụ sản xuất, kinh doanh và chuyển giao công nghệ.', N'Connect with suppliers of technology, equipment and solutions for production, business and technology transfer.'),
 (N'supplier.list.fieldTitle', N'Lĩnh vực cung ứng',            N'Supply fields'),
 (N'supplier.list.heading',    N'Danh sách nhà cung ứng',        N'Supplier directory'),
 (N'supplier.list.countSuffix',N'nhà cung ứng đang tham gia',    N'suppliers currently participating'),
 (N'supplier.list.empty',      N'Không có nhà cung ứng nào.',    N'No suppliers found.');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
