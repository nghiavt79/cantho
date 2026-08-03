-- Seed i18n cho khối "Sản phẩm tương tự" (ProductPortletIOSanpham) và "Sản phẩm OCOP khác".
-- LƯU Ý: chạy bằng  sqlcmd ... -f 65001 -i <file>  (file UTF-8 no-BOM).
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'product.related.title', N'Sản phẩm tương tự', N'Similar products'),
 (N'product.related.code',  N'Mã sản phẩm:',      N'Product code:');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
