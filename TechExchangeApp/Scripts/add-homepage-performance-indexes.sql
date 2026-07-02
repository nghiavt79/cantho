/*
Homepage performance indexes.
Run once on the target SQL Server database used by the public site.
All indexes are guarded so the script can be re-run safely.
*/

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Menu_ParentId'
      AND object_id = OBJECT_ID(N'[dbo].[Menu]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Menu_ParentId
    ON [dbo].[Menu] ([ParentId])
    INCLUDE ([MenuId], [Sort], [Title]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Menu_MenuId_Sort'
      AND object_id = OBJECT_ID(N'[dbo].[Menu]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Menu_MenuId_Sort
    ON [dbo].[Menu] ([MenuId], [Sort])
    INCLUDE ([Title]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Contents_Homepage'
      AND object_id = OBJECT_ID(N'[dbo].[Contents]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Contents_Homepage
    ON [dbo].[Contents] ([StatusId], [MenuId], [PublishedDate] DESC)
    INCLUDE ([Id], [Title], [Description], [Image], [QueryString]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ContentsYeuCau_Homepage'
      AND object_id = OBJECT_ID(N'[dbo].[ContentsYeuCau]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_ContentsYeuCau_Homepage
    ON [dbo].[ContentsYeuCau] ([StatusId], [MenuId], [PublishedDate] DESC)
    INCLUDE ([Id], [Title], [Image], [QueryString], [Viewed]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ImagesAdver_Homepage'
      AND object_id = OBJECT_ID(N'[dbo].[ImagesAdver]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_ImagesAdver_Homepage
    ON [dbo].[ImagesAdver] ([StatusID], [LanguageID], [Subject], [SiteId], [Sort])
    INCLUDE ([ID], [Title], [SRC], [URL], [Created]);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_SanPhamCNTB_Homepage'
      AND object_id = OBJECT_ID(N'[dbo].[SanPhamCNTB]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_SanPhamCNTB_Homepage
    ON [dbo].[SanPhamCNTB] ([StatusId], [LanguageId], [Modified] DESC, [Created] DESC)
    INCLUDE ([ID], [Name], [QueryString], [QuyTrinhHinhAnh], [MoTaNgan], [ProductType]);
END
GO
