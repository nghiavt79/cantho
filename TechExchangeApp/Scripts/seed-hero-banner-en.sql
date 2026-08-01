-- Seed 2 hero banner tiếng Anh (LanguageID=2) từ 2 banner Cần Thơ tiếng Việt (ID 242, 59).
-- Chỉ dịch Title + Description; giữ nguyên ảnh (SRC), URL, Sort, SiteId, Domain của bản gốc.
-- Ẩn các hero banner EN cũ (SiteId=1) để /en chỉ hiện đúng 2 bản dịch này.
-- Idempotent: chạy lại an toàn (đánh dấu Creator='hero-en-seed').
SET NOCOUNT ON;
BEGIN TRAN;

-- 1) Xoá bản EN đã seed trước đó.
DELETE FROM ImagesAdver WHERE Creator = 'hero-en-seed';

-- 2) Ẩn hero banner EN cũ của site (không phải bản seed) -> StatusID=4 (không xuất bản). Khôi phục: set lại 3.
UPDATE ImagesAdver SET StatusID = 4
WHERE Subject = 1 AND LanguageID = 2 AND (SiteId = 1) AND ISNULL(Creator, '') <> 'hero-en-seed';

-- 3) EN copy của banner 242 (giữ SRC/URL/Sort/SiteId/Domain gốc).
INSERT INTO ImagesAdver (Title, Description, SRC, URL, Subject, StatusID, Created, Creator, Sort, LanguageID, Domain, ParentId, SiteId)
SELECT N'Can Tho Technology Exchange',
       N'<p class="hero-description">Connecting businesses, experts and technology solutions,<br>driving innovation and sustainable development.</p><div class="hero-actions"><a class="hero-btn hero-btn-primary" href="/en/products"><span>Explore technology</span> <span class="btn-arrow">→</span>&nbsp;</a></div>',
       SRC, URL, Subject, StatusID, GETDATE(), 'hero-en-seed', Sort, 2, Domain, ParentId, SiteId
FROM ImagesAdver WHERE ID = 242;

-- 4) EN copy của banner 59.
INSERT INTO ImagesAdver (Title, Description, SRC, URL, Subject, StatusID, Created, Creator, Sort, LanguageID, Domain, ParentId, SiteId)
SELECT N'Can Tho Technology Exchange',
       N'<p class="hero-description">Technology connection • Transparent transactions • Sustainable development</p><div class="hero-actions"><a class="hero-btn hero-btn-primary" href="/en/products"><span>Learn more</span> <span class="btn-arrow">→</span>&nbsp;</a></div>',
       SRC, URL, Subject, StatusID, GETDATE(), 'hero-en-seed', Sort, 2, Domain, ParentId, SiteId
FROM ImagesAdver WHERE ID = 59;

COMMIT TRAN;

SELECT ID, Sort, StatusID AS St, LanguageID AS L, LEFT(Title,40) AS Title, LEFT(SRC,45) AS SRC
FROM ImagesAdver WHERE Creator = 'hero-en-seed' ORDER BY Sort;
