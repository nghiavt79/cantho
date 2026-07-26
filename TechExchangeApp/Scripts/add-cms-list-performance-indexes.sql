/*
CMS list-page performance indexes.
Phục vụ các trang danh sách trong khu quản trị (Areas/Cms) — mỗi trang luôn lọc
theo cột "chủ" (SiteId / ProductType / MenuId ...) rồi phân trang theo cột ngày
giảm dần. Trước đây các bảng này không có index hợp filter nên mỗi lần mở trang
là quét toàn bảng 2 lần (1 lấy data + 1 CountAsync).

Mỗi index dẫn đầu bằng cột filter luôn có, kết thúc bằng cột sort mặc định (DESC)
để vừa seek được filter + COUNT, vừa trả đúng thứ tự phân trang, khỏi sort lại.

Lưu ý: phải chạy KÈM bản sửa code bỏ COALESCE trong ORDER BY (PostsController,
SanPhamCNTBController) thì SQL Server mới dùng được các index này cho phần sort.

Tất cả đều guarded (IF NOT EXISTS) nên chạy lại nhiều lần vô hại.
Chạy 1 lần trên database TechExchangeNew.
*/

-- 1) Contents  → Cms/Posts  (luôn lọc SiteId, sort PublishedDate DESC)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Contents_CmsList'
      AND object_id = OBJECT_ID(N'[dbo].[Contents]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Contents_CmsList
    ON [dbo].[Contents] ([SiteId], [PublishedDate] DESC, [Created] DESC);
END
GO

-- 2) SanPhamCNTB → Cms/SanPhamCNTB (CongNghe/ThietBi/SanPhamTriTue/Ocop)
--    luôn lọc ProductType + SiteId, sort bEffectiveDate DESC
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_SanPhamCNTB_CmsList'
      AND object_id = OBJECT_ID(N'[dbo].[SanPhamCNTB]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_SanPhamCNTB_CmsList
    ON [dbo].[SanPhamCNTB] ([ProductType], [SiteId], [bEffectiveDate] DESC, [Created] DESC);
END
GO

-- 3) ContentsYeuCau → Cms/ContentsYeuCauAdmin (luôn lọc MenuId, có SiteId + StatusId)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ContentsYeuCau_CmsList'
      AND object_id = OBJECT_ID(N'[dbo].[ContentsYeuCau]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_ContentsYeuCau_CmsList
    ON [dbo].[ContentsYeuCau] ([MenuId], [SiteId], [StatusId]);
END
GO

-- 4) NhaCungUng → Cms/NhaCungUngAdmin (lọc SiteId, sort Created DESC)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_NhaCungUng_CmsList'
      AND object_id = OBJECT_ID(N'[dbo].[NhaCungUng]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_NhaCungUng_CmsList
    ON [dbo].[NhaCungUng] ([SiteId], [Created] DESC);
END
GO

-- 5) NhaTuVan → Cms/NhaTuVanAdmin (lọc SiteId, sort Created DESC)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_NhaTuVan_CmsList'
      AND object_id = OBJECT_ID(N'[dbo].[NhaTuVan]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_NhaTuVan_CmsList
    ON [dbo].[NhaTuVan] ([SiteId], [Created] DESC);
END
GO

-- 6) Feedback → Cms/FeedbackAdmin (lọc SiteId, sort Created DESC)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Feedback_CmsList'
      AND object_id = OBJECT_ID(N'[dbo].[Feedback]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Feedback_CmsList
    ON [dbo].[Feedback] ([SiteId], [Created] DESC);
END
GO

-- 7) PhieuYeuCauCNTB → Cms/PhieuYeuCauCNTBAdmin (lọc SiteId, sort Created DESC)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_PhieuYeuCauCNTB_CmsList'
      AND object_id = OBJECT_ID(N'[dbo].[PhieuYeuCauCNTB]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_PhieuYeuCauCNTB_CmsList
    ON [dbo].[PhieuYeuCauCNTB] ([SiteId], [Created] DESC);
END
GO

-- 8) Projects → Cms/ProjectsAdmin (sort CreatedDate DESC)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Projects_CmsList'
      AND object_id = OBJECT_ID(N'[dbo].[Projects]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Projects_CmsList
    ON [dbo].[Projects] ([CreatedDate] DESC);
END
GO

-- 9) ImagesAdver → Cms/ImageAdverAdmin (luôn lọc LanguageID + SiteId, sort Sort ASC, Created DESC)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ImagesAdver_CmsList'
      AND object_id = OBJECT_ID(N'[dbo].[ImagesAdver]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_ImagesAdver_CmsList
    ON [dbo].[ImagesAdver] ([LanguageID], [SiteId], [Sort], [Created] DESC);
END
GO
