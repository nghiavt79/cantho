-- Seed i18n cho trang danh sách Chuyên gia (Views/ChuyenGia/Index.cshtml). Idempotent.
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 (N'common.page',  N'Trang', N'Page'),
 (N'common.total', N'Tổng',  N'Total'),
 (N'expert.list.subtitle',   N'Kết nối với đội ngũ chuyên gia trong chuyển giao công nghệ, sở hữu trí tuệ, đầu tư và đổi mới sáng tạo.', N'Connect with our network of experts in technology transfer, intellectual property, investment and innovation.'),
 (N'expert.list.fieldTitle', N'Lĩnh vực tư vấn',            N'Consulting fields'),
 (N'expert.list.heading',    N'Đội ngũ chuyên gia',          N'Our expert team'),
 (N'expert.list.countSuffix',N'chuyên gia đang tham gia tư vấn', N'experts currently available for consulting'),
 (N'expert.list.empty',      N'Không có chuyên gia nào.',    N'No experts found.'),
 (N'expert.list.viewProfile',N'Xem hồ sơ',                   N'View profile');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
