-- ============================================================================
-- Seed thanh menu HEADER 2 CẤP data-driven (VI + EN) vào bảng Menu.
-- Cấp 1: MenuPosition = '1' (khớp tuyệt đối), ParentId = 0, Sort = thứ tự trái->phải.
-- Cấp 2: row Menu có ParentId = MenuId của mục cấp 1 (KHÔNG cần MenuPosition).
--   - NavigateUrl = href thật; Description (cấp 1) = "icon=...;css=..." cho mục đặc biệt.
-- Con của Sản phẩm/Dịch vụ được seed từ bảng Category (ParentId 1/2). Sau này thêm/sửa
-- menu con => làm trực tiếp trên MenuAdmin (không tự đồng bộ với Category nữa).
-- Idempotent: chạy lại an toàn (xoá theo Creator='header-seed').
-- ============================================================================
SET NOCOUNT ON;

BEGIN TRAN;

-- 1) Gỡ toàn bộ row header đã seed (cả cấp 1 và cấp 2).
DELETE FROM Menu WHERE Creator = 'header-seed';

-- 2) Trung hoà row lẻ đang để MenuPosition đúng '1' (không phải header seed).
UPDATE Menu SET MenuPosition = NULL
WHERE LTRIM(RTRIM(MenuPosition)) = '1' AND ISNULL(Creator, '') <> 'header-seed';

-- 3) Cấp 1 tiếng Việt (LanguageId = 1). Sản phẩm/Dịch vụ Description = NULL (con lấy từ Menu).
INSERT INTO Menu (Title, Description, Sort, MenuPosition, StatusId, Created, Creator, ParentId, QueryString, NavigateUrl, LanguageId, Domain, SiteId)
VALUES
 (N'Giới thiệu',       NULL,                                               1, '1', 1, GETDATE(), 'header-seed', 0, N'gioi-thieu-chung-3', N'/gioi-thieu-chung-3', 1, '', 1),
 (N'Sản phẩm',         NULL,                                               2, '1', 1, GETDATE(), 'header-seed', 0, N'san-pham',           N'/san-pham',           1, '', 1),
 (N'Dịch vụ',          NULL,                                               3, '1', 1, GETDATE(), 'header-seed', 0, N'dich-vu-tu-van',     N'/dich-vu-tu-van',     1, '', 1),
 (N'Tin tức sự kiện',  NULL,                                               4, '1', 1, GETDATE(), 'header-seed', 0, N'tin-su-kien',        N'/tin-su-kien',        1, '', 1),
 (N'Tìm mua',          NULL,                                               5, '1', 1, GETDATE(), 'header-seed', 0, N'tim-mua-cong-nghe',  N'/tim-mua-cong-nghe',  1, '', 1),
 (N'OCOP',             N'icon=bi-star-fill;css=site-header-v2__ocop-link', 6, '1', 1, GETDATE(), 'header-seed', 0, N'ocop',               N'/ocop',               1, '', 1);

-- 4) Cấp 1 tiếng Anh (LanguageId = 2).
INSERT INTO Menu (Title, Description, Sort, MenuPosition, StatusId, Created, Creator, ParentId, QueryString, NavigateUrl, LanguageId, Domain, SiteId)
VALUES
 (N'About',             NULL,                                               1, '1', 1, GETDATE(), 'header-seed', 0, N'en/about',             N'/en/about',             2, '', 1),
 (N'Products',          NULL,                                               2, '1', 1, GETDATE(), 'header-seed', 0, N'en/products',          N'/en/products',          2, '', 1),
 (N'Services',          NULL,                                               3, '1', 1, GETDATE(), 'header-seed', 0, N'en/services',          N'/en/services',          2, '', 1),
 (N'News & Events',     NULL,                                               4, '1', 1, GETDATE(), 'header-seed', 0, N'en/news-event',        N'/en/news-event',        2, '', 1),
 (N'Technology Demand', NULL,                                               5, '1', 1, GETDATE(), 'header-seed', 0, N'en/technology-demand', N'/en/technology-demand', 2, '', 1),
 (N'OCOP',              N'icon=bi-star-fill;css=site-header-v2__ocop-link', 6, '1', 1, GETDATE(), 'header-seed', 0, N'en/ocop',              N'/en/ocop',              2, '', 1);

-- 5) Lấy MenuId cấp 1 vừa chèn để làm ParentId cho cấp 2.
DECLARE @viProd int = (SELECT MenuId FROM Menu WHERE Creator='header-seed' AND LanguageId=1 AND QueryString=N'san-pham');
DECLARE @viServ int = (SELECT MenuId FROM Menu WHERE Creator='header-seed' AND LanguageId=1 AND QueryString=N'dich-vu-tu-van');
DECLARE @enProd int = (SELECT MenuId FROM Menu WHERE Creator='header-seed' AND LanguageId=2 AND QueryString=N'en/products');
DECLARE @enServ int = (SELECT MenuId FROM Menu WHERE Creator='header-seed' AND LanguageId=2 AND QueryString=N'en/services');

-- 6) Menu con "Sản phẩm" VI (slug từ Category.QueryString; route bỏ qua slug, chỉ cần -{CatId}).
INSERT INTO Menu (Title, Sort, MenuPosition, StatusId, Created, Creator, ParentId, QueryString, NavigateUrl, LanguageId, Domain, SiteId)
SELECT c.Title, c.Sort, NULL, 1, GETDATE(), 'header-seed', @viProd,
       c.QueryString,
       N'/san-pham/' + c.QueryString + N'-' + CAST(c.CatId AS nvarchar(10)),
       1, '', 1
FROM Category c WHERE c.ParentId=1 AND c.MainCate=1 AND c.StatusId=1 AND c.LanguageId=1;

-- 7) Menu con "Sản phẩm" EN (label = TitleEn; slug làm sạch: bỏ ',', gộp ' & ', space -> '-').
INSERT INTO Menu (Title, Sort, MenuPosition, StatusId, Created, Creator, ParentId, QueryString, NavigateUrl, LanguageId, Domain, SiteId)
SELECT ISNULL(NULLIF(c.TitleEn, N''), c.Title), c.Sort, NULL, 1, GETDATE(), 'header-seed', @enProd,
       NULL,
       N'/en/products/' + REPLACE(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(ISNULL(NULLIF(c.TitleEn, N''), c.QueryString))), N',', N''), N' & ', N' '), N'  ', N' '), N' ', N'-') + N'-' + CAST(c.CatId AS nvarchar(10)),
       2, '', 1
FROM Category c WHERE c.ParentId=1 AND c.MainCate=1 AND c.StatusId=1 AND c.LanguageId=1;

-- 8) Menu con "Dịch vụ" VI.
INSERT INTO Menu (Title, Sort, MenuPosition, StatusId, Created, Creator, ParentId, QueryString, NavigateUrl, LanguageId, Domain, SiteId)
SELECT c.Title, c.Sort, NULL, 1, GETDATE(), 'header-seed', @viServ,
       c.QueryString,
       N'/dich-vu-tu-van/' + c.QueryString + N'-' + CAST(c.CatId AS nvarchar(10)),
       1, '', 1
FROM Category c WHERE c.ParentId=2 AND c.MainCate=1 AND c.StatusId=1 AND c.LanguageId=1;

-- 9) Menu con "Dịch vụ" EN.
INSERT INTO Menu (Title, Sort, MenuPosition, StatusId, Created, Creator, ParentId, QueryString, NavigateUrl, LanguageId, Domain, SiteId)
SELECT ISNULL(NULLIF(c.TitleEn, N''), c.Title), c.Sort, NULL, 1, GETDATE(), 'header-seed', @enServ,
       NULL,
       N'/en/services/' + REPLACE(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(ISNULL(NULLIF(c.TitleEn, N''), c.QueryString))), N',', N''), N' & ', N' '), N'  ', N' '), N' ', N'-') + N'-' + CAST(c.CatId AS nvarchar(10)),
       2, '', 1
FROM Category c WHERE c.ParentId=2 AND c.MainCate=1 AND c.StatusId=1 AND c.LanguageId=1;

COMMIT TRAN;

-- Kiểm tra: số con theo từng mục cha.
SELECT p.LanguageId, p.Title AS Parent, COUNT(ch.MenuId) AS Children
FROM Menu p
LEFT JOIN Menu ch ON ch.ParentId = p.MenuId AND ch.Creator='header-seed'
WHERE p.Creator='header-seed' AND (p.ParentId = 0 OR p.ParentId IS NULL)
GROUP BY p.LanguageId, p.Title, p.Sort
ORDER BY p.LanguageId, p.Sort;
