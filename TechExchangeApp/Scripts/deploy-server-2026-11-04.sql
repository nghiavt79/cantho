/* =====================================================================
   Cập nhật máy chủ theo bản sửa lỗi L2 (báo cáo kiểm tra 03/11/2026). Chạy lại được nhiều lần.
   1) Bổ sung bảng lĩnh vực cho sản phẩm đã đăng mà chưa có (L2-09)
   2) Dữ liệu tra cứu sản phẩm thêm mô tả chi tiết (L2-09)
   3) Dựng lại dữ liệu tra cứu tiếng Việt, tiếng Anh
   Chạy:  sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -i deploy-server-2026-11-04.sql
   ===================================================================== */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
INSERT INTO dbo.SanPhamCNTBCategory (SanPhamCNTBId, CatId)
SELECT s.ID, TRY_CAST(LTRIM(RTRIM(v.value)) AS INT)
FROM dbo.SanPhamCNTB s
CROSS APPLY STRING_SPLIT(ISNULL(s.CategoryId, ''), ';') v
WHERE TRY_CAST(LTRIM(RTRIM(v.value)) AS INT) > 0
  AND NOT EXISTS (SELECT 1 FROM dbo.SanPhamCNTBCategory c WHERE c.SanPhamCNTBId = s.ID);
PRINT N'Đã bổ sung lĩnh vực: ' + CAST(@@ROWCOUNT AS NVARCHAR(20));
GO
ALTER PROCEDURE [dbo].[uspReIndexSearchContents]
	@LanguageId int
AS
BEGIN
    SET NOCOUNT ON;

    -- Chỉ xoá index của ĐÚNG ngôn ngữ đang reindex (không đụng ngôn ngữ còn lại).
    DELETE FROM SearchIndexContents WHERE LanguageId = CAST(@LanguageId AS NVARCHAR(10));

    DECLARE @UrlPrefix NVARCHAR(20) = CASE WHEN @LanguageId = 2 THEN N'/en' ELSE N'' END;

    ;WITH ABC AS (
        -- ═══ SanPhamCNTB: tách theo ProductType ═══
        SELECT
            s.ID AS Id,
            REPLACE(S.Name, '-', ' ') AS Title,
            S.MoTaNgan AS [Description],
            s.QuyTrinhHinhAnh AS [Image],
            CASE WHEN s.ProductType = 4
                THEN @UrlPrefix + '/ocop/' + s.QueryString + '-' + CAST(s.ID AS NVARCHAR(50))
                ELSE @UrlPrefix + CASE WHEN @LanguageId = 2 THEN '/products/' ELSE '/san-pham/chi-tiet/' END
                     + s.QueryString + '-' + CAST(s.ID AS NVARCHAR(50))
            END AS URL,
            CASE s.ProductType
                WHEN 1 THEN N'Công nghệ'
                WHEN 2 THEN N'Thiết bị'
                WHEN 3 THEN N'Tài sản trí tuệ'
				WHEN 4 THEN N'OCOP'
            END AS TypeId,
            ISNULL(S.MoTaNgan,'') + ' ' + ISNULL(S.Thongso,'') + ' ' + ISNULL(S.UuDiem,'') + ' ' + ISNULL(CAST(S.MoTa AS NVARCHAR(MAX)),'') AS Contents,
            s.ID AS RefId
        FROM dbo.SanPhamCNTB S
        WHERE s.StatusId = 3 AND s.LanguageId = @LanguageId

        UNION ALL
        -- ═══ NhaCungUng → Nhà cung ứng ═══
        SELECT
            s.CungUngId, S.FullName, s.DiaChi,
            '' AS [Image],
            @UrlPrefix + CASE WHEN @LanguageId = 2 THEN '/suppliers/' ELSE '/nha-cung-ung/' END
                + s.QueryString + '-' + CAST(s.CungUngId AS NVARCHAR(50)),
            N'Nhà cung ứng',
            ISNULL(Phone,'') + ' ' + ISNULL(Email,'') + ' ' + ISNULL(Fax,'')
                + ' ' + ISNULL(Website,'') + ' ' + ISNULL(ChucNangChinh,''),
            s.CungUngId
        FROM dbo.NhaCungUng S
        WHERE s.LanguageId = @LanguageId AND s.IsActivated = 1

        UNION ALL
        -- ═══ NhaTuVan → Chuyên gia ═══
        SELECT
            s.TuVanId, S.FullName, s.HocHam,
            s.HinhDaiDien,
            @UrlPrefix + CASE WHEN @LanguageId = 2 THEN '/experts/' ELSE '/chuyen-gia/' END
                + s.QueryString + '-' + CAST(s.TuVanId AS NVARCHAR(50)),
            N'Chuyên gia',
            '',
            s.TuVanId
        FROM dbo.NhaTuVan S
        WHERE s.StatusId = 3 AND s.LanguageId = @LanguageId

        UNION ALL
        -- ═══ Contents → Tin bài ═══
        SELECT
            s.Id, S.Title, S.[Description],
            s.[Image],
            @UrlPrefix + CASE WHEN @LanguageId = 2 THEN '/news-event/' ELSE '/tin-tuc-su-kien/' END
                + s.QueryString + '-' + CAST(s.Id AS NVARCHAR(50)),
            N'Tin bài',
            s.Contents,
            s.Id
        FROM dbo.Contents S
        WHERE s.LanguageId = @LanguageId AND s.StatusId = 3
    )
    INSERT INTO SearchIndexContents (ImgPreview, Title, [Description], Contents, RefId, TypeName, URL, Created, Creator, LanguageId)
    SELECT A.[Image], A.Title, A.[Description], A.Contents, A.RefId, A.TypeId, A.URL, GETDATE(), 'sys', CAST(@LanguageId AS NVARCHAR(10))
    FROM ABC A
    LEFT JOIN SearchIndexContents B ON A.TypeId = B.TypeName AND A.RefId = B.RefId AND B.LanguageId = CAST(@LanguageId AS NVARCHAR(10))
    WHERE B.Id IS NULL;

    -- Handle nulls
    UPDATE SearchIndexContents SET [Description] = '' WHERE [Description] IS NULL;
    UPDATE SearchIndexContents SET Contents = '' WHERE Contents IS NULL;
END
GO

EXEC dbo.uspReIndexSearchContents @LanguageId = 1;
EXEC dbo.uspReIndexSearchContents @LanguageId = 2;
GO
