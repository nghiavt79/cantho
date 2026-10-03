-- Seed i18n cho Đợt 4: huy hiệu hạng nhà cung cấp, vị trí trưng bày, liên kết đăng ký bản tin. Idempotent.
-- Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -I -i seed-i18n-dot4-hsmt.sql
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'supplier.tier.label',      N'Hạng nhà cung cấp',     N'Supplier tier'),
 (N'supplier.tier.1',          N'Thành viên',            N'Member'),
 (N'supplier.tier.2',          N'Bạc',                   N'Silver'),
 (N'supplier.tier.3',          N'Vàng',                  N'Gold'),
 (N'product.display.position', N'Vị trí trưng bày',      N'Display location'),
 (N'footer.newsletter',        N'Đăng ký nhận bản tin',  N'Newsletter');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-dot4-hsmt' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
