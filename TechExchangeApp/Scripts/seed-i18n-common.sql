-- Bước 1: seed các key UI dùng chung (common.*) để migrate inline T("vi","en") về DB.
-- Chỉ cặp (vi,en) giống hệt nơi dùng → gom 1 key (không đổi text hiển thị).
-- Idempotent: chỉ chèn key chưa tồn tại. Đã có sẵn: common.home, common.search, news.title.
SET NOCOUNT ON;

DECLARE @seed TABLE ([Key] NVARCHAR(250), Vi NVARCHAR(400), En NVARCHAR(400));
INSERT INTO @seed VALUES
 (N'common.technology',    N'Công nghệ',            N'Technology'),
 (N'common.equipment',     N'Thiết bị',             N'Equipment'),
 (N'common.services',      N'Dịch vụ',              N'Services'),
 (N'common.ipAssets',      N'Sản phẩm trí tuệ',     N'IP Assets'),
 (N'common.products',      N'Sản phẩm',             N'Products'),
 (N'common.about',         N'Giới thiệu',           N'About'),
 (N'common.contact',       N'Liên hệ',              N'Contact'),
 (N'common.details',       N'Chi tiết',             N'Details'),
 (N'common.viewDetails',   N'Xem chi tiết',         N'View details'),
 (N'common.viewAll',       N'Xem tất cả',           N'View all'),
 (N'common.all',           N'Tất cả',               N'All'),
 (N'common.prevPage',      N'Trang trước',          N'Previous page'),
 (N'common.nextPage',      N'Trang sau',            N'Next page'),
 (N'common.pagination',    N'Phân trang',           N'Pagination'),
 (N'common.backToTop',     N'Về đầu trang',         N'Back to top'),
 (N'common.top',           N'Lên đầu',              N'Top'),
 (N'common.login',         N'Đăng nhập',            N'Log in'),
 (N'common.supportChat',   N'Chat hỗ trợ',          N'Support chat'),
 (N'common.userGuide',     N'Hướng dẫn sử dụng',    N'User guide'),
 (N'common.vr360',         N'Trải nghiệm VR360°',   N'VR360° experience'),
 (N'common.expertNetwork', N'Mạng lưới chuyên gia', N'Expert network'),
 (N'common.traceability',  N'Truy xuất nguồn gốc',  N'Traceability');

INSERT INTO dbo.UiTranslations ([Key], Vi, En, Creator)
SELECT s.[Key], s.Vi, s.En, 'i18n-common-seed'
FROM @seed s
WHERE NOT EXISTS (SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key] = s.[Key]);

SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS TotalKeys;
