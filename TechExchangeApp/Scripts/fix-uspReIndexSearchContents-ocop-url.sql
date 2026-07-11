USE [TechExchangeNew]
GO
/****** Object:  StoredProcedure [dbo].[uspReIndexSearchContents]    Fix: OCOP URL uses /ocop/ instead of /san-pham/chi-tiet/ ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[uspReIndexSearchContents]
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH ABC AS (
        -- ═══ SanPhamCNTB: tách theo ProductType ═══
        SELECT
            s.ID AS Id,
            REPLACE(S.Name, '-', ' ') AS Title,
            S.MoTaNgan AS [Description],
            s.QuyTrinhHinhAnh AS [Image],
            CASE WHEN s.ProductType = 4
                THEN '/ocop/' + s.QueryString + '-' + CAST(s.ID AS NVARCHAR(50)) + '.html'
                ELSE '/san-pham/chi-tiet/' + s.QueryString + '-' + CAST(s.ID AS NVARCHAR(50)) + '.html'
            END AS URL,
            CASE s.ProductType
                WHEN 1 THEN N'Công nghệ'
                WHEN 2 THEN N'Thiết bị'
                WHEN 3 THEN N'Tài sản trí tuệ'
				WHEN 4 THEN N'OCOP'
            END AS TypeId,
            ISNULL(S.MoTaNgan,'') + ' ' + ISNULL(S.Thongso,'') + ' ' + ISNULL(S.UuDiem,'') AS Contents,
            s.ID AS RefId
        FROM dbo.SanPhamCNTB S
        WHERE s.StatusId = 3 AND s.LanguageId = 1

        UNION ALL
        -- ═══ NhaCungUng → Nhà cung ứng ═══
        SELECT
            s.CungUngId, S.FullName, s.DiaChi,
            '' AS [Image],
            '/nha-cung-ung/' + s.QueryString + '-' + CAST(s.CungUngId AS NVARCHAR(50)) + '.html',
            N'Nhà cung ứng',
            ISNULL(Phone,'') + ' ' + ISNULL(Email,'') + ' ' + ISNULL(Fax,'')
                + ' ' + ISNULL(Website,'') + ' ' + ISNULL(ChucNangChinh,''),
            s.CungUngId
        FROM dbo.NhaCungUng S
        WHERE s.LanguageId = 1 AND s.IsActivated = 1

        UNION ALL
        -- ═══ NhaTuVan → Chuyên gia ═══
        SELECT
            s.TuVanId, S.FullName, s.HocHam,
            s.HinhDaiDien,
            '/nha-tu-van/' + s.QueryString + '-' + CAST(s.TuVanId AS NVARCHAR(50)) + '.html',
            N'Chuyên gia',
            '',
            s.TuVanId
        FROM dbo.NhaTuVan S
        WHERE s.StatusId = 3 AND s.LanguageId = 1

        UNION ALL
        -- ═══ Contents → Tin bài ═══
        SELECT
            s.Id, S.Title, S.[Description],
            s.[Image],
            '/' + CAST(s.MenuId AS NVARCHAR(50)) + '/' + s.QueryString + '-' + CAST(s.Id AS NVARCHAR(50)) + '.html',
            N'Tin bài',
            s.Contents,
            s.Id
        FROM dbo.Contents S
        WHERE s.LanguageId = 1 AND s.StatusId = 3
    )
    INSERT INTO SearchIndexContents (ImgPreview, Title, [Description], Contents, RefId, TypeName, URL, Created, Creator, LanguageId)
    SELECT A.[Image], A.Title, A.[Description], A.Contents, A.RefId, A.TypeId, A.URL, GETDATE(), 'sys', '1'
    FROM ABC A
    LEFT JOIN SearchIndexContents B ON A.TypeId = B.TypeName AND A.RefId = B.RefId
    WHERE B.Id IS NULL;

    -- Handle nulls
    UPDATE SearchIndexContents SET [Description] = '' WHERE [Description] IS NULL;
    UPDATE SearchIndexContents SET Contents = '' WHERE Contents IS NULL;
END
GO

-- Patch the 20 OCOP rows already indexed with the old wrong URL pattern
UPDATE si
SET si.URL = '/ocop/' + s.QueryString + '-' + CAST(s.ID AS NVARCHAR(50)) + '.html'
FROM SearchIndexContents si
JOIN SanPhamCNTB s ON s.ID = si.RefId
WHERE si.TypeName = N'OCOP';

SELECT Id, Title, URL FROM SearchIndexContents WHERE TypeName = N'OCOP' ORDER BY Id;
GO
