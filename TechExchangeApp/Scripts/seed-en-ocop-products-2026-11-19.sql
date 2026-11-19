/*
  Tạo bản tiếng Anh cho 6 sản phẩm OCOP (BUG-004 – trang /en/ocop không có dữ liệu).
  Ngày: 19/11/2026

  Cách làm: chép nguyên dòng tiếng Việt (ProductType = 4) sang dòng mới LanguageId = 2,
  OriginalId = ID bản Việt, rồi ghi đè các trường chữ bằng bản dịch. Danh mục được chép theo.
  - Chỉ tạo khi bản Việt tồn tại đúng ID + đúng tên (tránh lệch ID giữa các máy).
  - Đã có bản tiếng Anh (OriginalId) thì bỏ qua → chạy lại nhiều lần không sinh trùng.

  Chạy (file UTF-8, BẮT BUỘC có -f 65001):
    sqlcmd -S <server> -d TechExchangeNew -U <user> -P <pass> -f 65001 -i seed-en-ocop-products-2026-11-19.sql
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

DECLARE @T TABLE (
    ViId INT PRIMARY KEY,
    ViName NVARCHAR(500),
    Name NVARCHAR(500),
    QueryString NVARCHAR(500),
    XuatXu NVARCHAR(500),
    MoTaNgan NVARCHAR(MAX),
    MoTa NVARCHAR(MAX),
    ChungNhanKhacText NVARCHAR(MAX),
    HoTen NVARCHAR(500),
    DiaChi NVARCHAR(500)
);

INSERT INTO @T VALUES
(35373, N'Trà mãng cầu Long Giang',
 N'Long Giang Soursop Tea', N'long-giang-soursop-tea',
 N'O Mon, Can Tho',
 N'Pure soursop tea by SumoFood Vietnam Co., Ltd., certified 5-star OCOP.',
 N'Long Giang soursop tea is made from soursop grown on the alluvial land of O Mon and freeze-dried to preserve its natural flavor and nutrients. The product contains no preservatives and is suitable as an everyday drink that helps cool the body and support the immune system.',
 N'5-star OCOP (national level)',
 N'SumoFood Vietnam Co., Ltd.',
 N'Thoi An Area, O Mon Ward, Can Tho City'),

(35374, N'Snack da cá Vidaca',
 N'Vidaca Fish Skin Snack', N'vidaca-fish-skin-snack',
 N'O Mon, Can Tho',
 N'Crispy fish skin snack by Vidaca Food JSC, certified 5-star OCOP.',
 N'The snack is made from fresh pangasius skin, seasoned and fried using a closed-loop process that meets export food safety standards. It is crunchy, low in oil and free of industrial colorings.',
 N'5-star OCOP (national level)',
 N'Vidaca Food JSC',
 N'Tra Noc Industrial Park, O Mon District, Can Tho City'),

(35376, N'Bột gạo lứt mè đen Thuận Hòa',
 N'Thuan Hoa Brown Rice and Black Sesame Powder', N'thuan-hoa-brown-rice-and-black-sesame-powder',
 N'Cai Rang, Can Tho',
 N'Nutritious brown rice and black sesame powder by Thuan Hoa Production Facility, certified 4-star OCOP.',
 N'Finely roasted and ground from whole brown rice and black sesame, with no added sweeteners. It can be mixed as a drink or cooked as baby food, supporting digestion and adding dietary fiber.',
 N'4-star OCOP',
 N'Thuan Hoa Production Facility',
 N'Ba Lang Ward, Cai Rang District, Can Tho City'),

(35380, N'Trà hòa tan đinh lăng Hygie',
 N'Hygie Instant Polyscias Tea', N'hygie-instant-polyscias-tea',
 N'Ninh Kieu, Can Tho',
 N'Instant tea made from Polyscias (dinh lang) by Hygie & Panacee One-Member Co., Ltd., certified 4-star OCOP.',
 N'Instant tea extracted from Polyscias fruticosa (dinh lang) roots, in convenient soluble granules, helping to nourish the body and strengthen resistance.',
 N'4-star OCOP',
 N'Hygie & Panacee One-Member Co., Ltd.',
 N'An Khanh Ward, Ninh Kieu District, Can Tho City'),

(35382, N'Nhãn Idol Tân Lộc',
 N'Tan Loc Idol Longan', N'tan-loc-idol-longan',
 N'Thot Not, Can Tho',
 N'Fresh Idol longan from Tan Loc Fruit Cooperative, certified 3-star OCOP.',
 N'Idol (Ido) longan grown under VietGAP practices on Tan Loc islet, a well-known longan-growing area of Can Tho. The fruit is large, with thick flesh, a small seed and a mild sweet taste.',
 N'3-star OCOP',
 N'Tan Loc Fruit Cooperative',
 N'Tan Loc Islet, Thot Not District, Can Tho City'),

(35389, N'Mật ong hoa nhãn Phong Điền',
 N'Phong Dien Longan Blossom Honey', N'phong-dien-longan-blossom-honey',
 N'Phong Dien, Can Tho',
 N'Pure honey from Phong Dien longan orchards, certified 4-star OCOP.',
 N'Pure honey harvested from Phong Dien longan orchards during the blossom season, with no additives. It is smooth and thick, with the mild sweetness typical of Mekong Delta longan blossom honey.',
 N'4-star OCOP',
 N'Phong Dien Beekeeping Cooperative Group',
 N'Phong Dien District, Can Tho City');

-- Danh sách cột để chép (bỏ cột identity / computed / rowversion)
DECLARE @cols NVARCHAR(MAX) =
    STUFF((SELECT ',' + QUOTENAME(c.name)
           FROM sys.columns c
           JOIN sys.types ty ON ty.user_type_id = c.user_type_id
           WHERE c.object_id = OBJECT_ID('dbo.SanPhamCNTB')
             AND c.is_identity = 0 AND c.is_computed = 0
             AND ty.name NOT IN ('timestamp', 'rowversion')
           ORDER BY c.column_id
           FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '');

DECLARE @sql NVARCHAR(MAX) =
    N'INSERT INTO dbo.SanPhamCNTB (' + @cols + N') SELECT ' + @cols +
    N' FROM dbo.SanPhamCNTB WHERE ID = @id; SET @newId = CAST(SCOPE_IDENTITY() AS INT);';

DECLARE @id INT, @viName NVARCHAR(500), @newId INT, @created INT = 0, @skipped INT = 0;

DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT ViId, ViName FROM @T ORDER BY ViId;
OPEN cur;
FETCH NEXT FROM cur INTO @id, @viName;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.SanPhamCNTB
                   WHERE ID = @id AND ISNULL(LanguageId, 1) = 1 AND ProductType = 4 AND Name = @viName)
    BEGIN
        PRINT N'Bỏ qua ' + CAST(@id AS NVARCHAR(20)) + N': không thấy bản Việt khớp ID + tên';
        SET @skipped += 1;
    END
    ELSE IF EXISTS (SELECT 1 FROM dbo.SanPhamCNTB WHERE OriginalId = @id AND LanguageId = 2)
    BEGIN
        PRINT N'Bỏ qua ' + CAST(@id AS NVARCHAR(20)) + N': đã có bản tiếng Anh';
        SET @skipped += 1;
    END
    ELSE
    BEGIN
        BEGIN TRAN;

        EXEC sp_executesql @sql, N'@id INT, @newId INT OUTPUT', @id = @id, @newId = @newId OUTPUT;

        UPDATE p SET
            LanguageId        = 2,
            OriginalId        = @id,
            EnStale           = 0,
            SourceHash        = NULL,
            Name              = t.Name,
            QueryString       = t.QueryString,
            XuatXu            = t.XuatXu,
            MoTaNgan          = t.MoTaNgan,
            MoTa              = t.MoTa,
            ChungNhanKhacText = t.ChungNhanKhacText,
            HoTen             = t.HoTen,
            DiaChi            = t.DiaChi,
            Modified          = GETDATE()
        FROM dbo.SanPhamCNTB p
        JOIN @T t ON t.ViId = @id
        WHERE p.ID = @newId;

        INSERT INTO dbo.SanPhamCNTBCategory (SanPhamCNTBId, CatId)
        SELECT @newId, c.CatId
        FROM dbo.SanPhamCNTBCategory c
        WHERE c.SanPhamCNTBId = @id;

        COMMIT;
        PRINT N'Tạo ' + CAST(@newId AS NVARCHAR(20)) + N' (EN) từ ' + CAST(@id AS NVARCHAR(20));
        SET @created += 1;
    END

    FETCH NEXT FROM cur INTO @id, @viName;
END
CLOSE cur;
DEALLOCATE cur;

PRINT N'Đã tạo: ' + CAST(@created AS NVARCHAR(10)) + N' – Bỏ qua: ' + CAST(@skipped AS NVARCHAR(10));
