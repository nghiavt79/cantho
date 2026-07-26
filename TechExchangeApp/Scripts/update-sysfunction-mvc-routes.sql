SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

DECLARE @Menu TABLE
(
    FunctionId int NOT NULL PRIMARY KEY,
    FunctionName nvarchar(500) NOT NULL,
    URL nvarchar(300) NULL,
    HrefName nvarchar(100) NULL,
    IsMenu bit NOT NULL,
    IsStatus bit NOT NULL,
    IsShow bit NOT NULL,
    ParentId int NULL,
    Sort int NULL,
    SiteId int NULL
);

INSERT INTO @Menu (FunctionId, FunctionName, URL, HrefName, IsMenu, IsStatus, IsShow, ParentId, Sort, SiteId)
VALUES
    (569, N'Dashboard', N'cms/Dashboard', N'Dashboard', 1, 1, 1, 0, 10, 1),
    (16, N'Users', N'cms/Users', N'Người dùng', 1, 1, 1, 0, 20, 1),
    (570, N'Projects', N'cms/ProjectsAdmin', N'Dự án', 1, 1, 1, 0, 30, 1),
    (40, N'Posts', N'cms/Posts', N'Bài viết', 1, 1, 1, 0, 40, 1),
    (53, N'MenuAdmin', N'cms/MenuAdmin', N'Menu', 1, 1, 1, 0, 50, 1),
    (46, N'CategoryAdmin', N'cms/CategoryAdmin', N'Danh mục', 1, 1, 1, 0, 60, 1),
    (48, N'ImageAdverAdmin', N'cms/ImageAdverAdmin', N'Banner / Quảng cáo', 1, 1, 1, 0, 70, 1),
    (421, N'SanPhamCNTB', N'cms/SanPhamCNTB/CongNghe', N'Công nghệ', 1, 1, 1, 0, 80, 1),
    (571, N'SanPhamCNTBThietBi', N'cms/SanPhamCNTB/ThietBi', N'Thiết bị', 1, 1, 1, 0, 90, 1),
    (572, N'SanPhamCNTBSanPhamTriTue', N'cms/SanPhamCNTB/SanPhamTriTue', N'Sản phẩm trí tuệ', 1, 1, 1, 0, 100, 1),
    (573, N'SanPhamCNTBOcop', N'cms/SanPhamCNTB/Ocop', N'OCOP', 1, 1, 1, 0, 110, 1),
    (42, N'NhaCungUng', N'cms/NhaCungUngAdmin', N'Nhà cung ứng', 1, 1, 1, 0, 120, 1),
    (50, N'NhaTuVan', N'cms/NhaTuVanAdmin', N'Nhà tư vấn', 1, 1, 1, 0, 130, 1),
    (422, N'PhieuYeuCauCNTB', N'cms/PhieuYeuCauCNTBAdmin', N'Phiếu yêu cầu', 1, 1, 1, 0, 140, 1),
    (574, N'ContentsYeuCauAdmin', N'cms/ContentsYeuCauAdmin', N'Tìm mua công khai', 1, 1, 1, 0, 150, 1),
    (41, N'Feedback', N'cms/FeedbackAdmin', N'Phản hồi (Feedback)', 1, 1, 1, 0, 160, 1),
    (555, N'Feedback', N'cms/FeedbackAdmin', N'Phản hồi (Feedback)', 1, 1, 1, 0, 161, 1),
    (575, N'DashboardData', N'cms/DashboardData', N'Số liệu Dashboard', 1, 1, 1, 0, 170, 1),
    (556, N'SysParam', N'cms/SystemParameterAdmin', N'Tham số hệ thống', 1, 1, 1, 0, 180, 1),
    (51, N'SysFunction', N'cms/SysFunctionAdmin', N'SysFunction', 1, 1, 1, 0, 190, 1);

UPDATE target
SET target.FunctionName = source.FunctionName,
    target.URL = source.URL,
    target.HrefName = source.HrefName,
    target.IsMenu = source.IsMenu,
    target.IsStatus = source.IsStatus,
    target.IsShow = source.IsShow,
    target.ParentId = source.ParentId,
    target.Sort = source.Sort,
    target.SiteId = source.SiteId
FROM dbo.SysFunction AS target
INNER JOIN @Menu AS source ON source.FunctionId = target.FunctionId;

INSERT INTO dbo.SysFunction (FunctionId, FunctionName, URL, HrefName, IsMenu, IsStatus, IsShow, ParentId, Sort, SiteId)
SELECT source.FunctionId, source.FunctionName, source.URL, source.HrefName, source.IsMenu, source.IsStatus,
       source.IsShow, source.ParentId, source.Sort, source.SiteId
FROM @Menu AS source
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.SysFunction AS target
    WHERE target.FunctionId = source.FunctionId
);

COMMIT;

SELECT FunctionId, FunctionName, URL, HrefName, IsMenu, IsShow, Sort, SiteId
FROM dbo.SysFunction
WHERE FunctionId IN (16, 40, 41, 42, 46, 48, 50, 51, 53, 421, 422, 555, 556, 569, 570, 571, 572, 573, 574, 575)
ORDER BY Sort, FunctionId;
