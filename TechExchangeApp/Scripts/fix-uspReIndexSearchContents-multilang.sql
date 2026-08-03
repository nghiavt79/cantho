USE [TechExchangeNew]
GO
/****** Object:  StoredProcedure [dbo].[uspReIndexSearchContents] — thêm hỗ trợ tiếng Anh ******/
-- Sửa 3 lỗi trong bản hiện tại:
--  1) @LanguageId hoàn toàn bị bỏ qua trong body — mọi WHERE luôn = 1, INSERT luôn ghi '1'
--     => reindex EN (languageId=2) không tạo ra dòng nào, /en/search không có kết quả.
--  2) "truncate table SearchIndexContents" xoá SẠCH TOÀN BẢNG bất kể @LanguageId
--     => reindex 1 ngôn ngữ sẽ xoá luôn index của ngôn ngữ kia. Đổi sang DELETE có điều kiện.
--  3) URL build sai/lỗi thời cho CẢ 2 ngôn ngữ (hậu tố ".html" không route nào khớp; Chuyên gia
--     trỏ nhầm "/nha-tu-van/" thay vì route thật "/chuyen-gia/") — sửa lại đúng theo route hiện có,
--     và thêm nhánh EN cho từng loại (Sản phẩm/OCOP/Nhà cung ứng/Chuyên gia/Tin bài).
--
-- LƯU Ý: TypeName ("Công nghệ","Thiết bị",...) là KHOÁ LỌC NỘI BỘ dùng xuyên suốt
-- SearchService/SearchEntityTypeHelper (so khớp chuỗi chính xác) — GIỮ NGUYÊN tiếng Việt
-- cho mọi ngôn ngữ, KHÔNG dịch. Nhãn hiển thị tiếng Anh xử lý riêng ở tầng C#
-- (SearchEntityTypeHelper.ToLabel), không lưu trong bảng này.
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
            ISNULL(S.MoTaNgan,'') + ' ' + ISNULL(S.Thongso,'') + ' ' + ISNULL(S.UuDiem,'') AS Contents,
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
