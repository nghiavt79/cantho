-- Seed i18n cho SEO title/description trang chủ (Views/Home/Index.cshtml). Idempotent.
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'home.seo.title',       N'Sàn Giao dịch & Chuyển giao Công nghệ Cần Thơ | CASTA', N'Can Tho Technology Exchange & Transfer Platform | CASTA'),
 (N'home.seo.description', N'Kết nối cung – cầu công nghệ, thiết bị, tài sản trí tuệ, chuyên gia và dịch vụ chuyển giao công nghệ tại Cần Thơ và Đồng bằng sông Cửu Long.', N'Connecting technology supply and demand — equipment, intellectual property, experts and technology transfer services in Can Tho and the Mekong Delta.');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
