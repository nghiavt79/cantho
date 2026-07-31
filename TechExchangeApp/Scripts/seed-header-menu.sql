-- ============================================================================
-- Seed thanh menu HEADER data-driven (VI + EN) vào bảng Menu.
-- Quy ước: MenuPosition = '1' (khớp tuyệt đối) => mục hiển thị trên header.
--   - StatusId = 1 (đang bật), ParentId = 0 (mục gốc), Sort = thứ tự trái->phải.
--   - NavigateUrl  = href thật của mục.
--   - Description  = meta cho header: "cat=1"/"cat=2" (bơm danh mục SP/DV),
--                    "icon=bi-star-fill;css=..." (icon + class phụ). NULL = mục thường.
-- Idempotent: chạy lại an toàn (xoá theo Creator='header-seed', trung hoà stray '1').
-- ============================================================================
SET NOCOUNT ON;

BEGIN TRAN;

-- 1) Gỡ các row header đã seed trước đó (nếu chạy lại).
DELETE FROM Menu WHERE Creator = 'header-seed';

-- 2) Trung hoà mọi row lẻ đang để MenuPosition đúng '1' (không phải header seed),
--    để selector 'MenuPosition = 1' chỉ trả về đúng tập header mới.
UPDATE Menu SET MenuPosition = NULL
WHERE LTRIM(RTRIM(MenuPosition)) = '1' AND ISNULL(Creator, '') <> 'header-seed';

-- 3) Chèn header tiếng Việt (LanguageId = 1).
INSERT INTO Menu (Title, Description, Sort, MenuPosition, StatusId, Created, Creator, ParentId, QueryString, NavigateUrl, LanguageId, Domain, SiteId)
VALUES
 (N'Giới thiệu',       NULL,                                                   1, '1', 1, GETDATE(), 'header-seed', 0, N'gioi-thieu-chung-3', N'/gioi-thieu-chung-3', 1, '', 1),
 (N'Sản phẩm',         N'cat=1',                                               2, '1', 1, GETDATE(), 'header-seed', 0, N'san-pham',           N'/san-pham',           1, '', 1),
 (N'Dịch vụ',          N'cat=2',                                               3, '1', 1, GETDATE(), 'header-seed', 0, N'dich-vu-tu-van',     N'/dich-vu-tu-van',     1, '', 1),
 (N'Tin tức sự kiện',  NULL,                                                   4, '1', 1, GETDATE(), 'header-seed', 0, N'tin-su-kien',        N'/tin-su-kien',        1, '', 1),
 (N'Tìm mua',          NULL,                                                   5, '1', 1, GETDATE(), 'header-seed', 0, N'tim-mua-cong-nghe',  N'/tim-mua-cong-nghe',  1, '', 1),
 (N'OCOP',             N'icon=bi-star-fill;css=site-header-v2__ocop-link',     6, '1', 1, GETDATE(), 'header-seed', 0, N'ocop',               N'/ocop',               1, '', 1);

-- 4) Chèn header tiếng Anh (LanguageId = 2).
INSERT INTO Menu (Title, Description, Sort, MenuPosition, StatusId, Created, Creator, ParentId, QueryString, NavigateUrl, LanguageId, Domain, SiteId)
VALUES
 (N'About',             NULL,                                                  1, '1', 1, GETDATE(), 'header-seed', 0, N'en/about',            N'/en/about',             2, '', 1),
 (N'Products',          N'cat=1',                                              2, '1', 1, GETDATE(), 'header-seed', 0, N'en/products',         N'/en/products',          2, '', 1),
 (N'Services',          N'cat=2',                                              3, '1', 1, GETDATE(), 'header-seed', 0, N'en/services',         N'/en/services',          2, '', 1),
 (N'News & Events',     NULL,                                                  4, '1', 1, GETDATE(), 'header-seed', 0, N'en/news-event',       N'/en/news-event',        2, '', 1),
 (N'Technology Demand', NULL,                                                  5, '1', 1, GETDATE(), 'header-seed', 0, N'en/technology-demand',N'/en/technology-demand', 2, '', 1),
 (N'OCOP',              N'icon=bi-star-fill;css=site-header-v2__ocop-link',    6, '1', 1, GETDATE(), 'header-seed', 0, N'en/ocop',             N'/en/ocop',              2, '', 1);

COMMIT TRAN;

-- Kiểm tra kết quả.
SELECT MenuId, LanguageId, Sort, MenuPosition, Title, NavigateUrl, Description
FROM Menu WHERE Creator = 'header-seed' ORDER BY LanguageId, Sort;
