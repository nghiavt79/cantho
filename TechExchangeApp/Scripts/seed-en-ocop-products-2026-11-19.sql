/*
  Tạo bản tiếng Anh cho 20 sản phẩm OCOP (BUG-004 – trang /en/ocop không có dữ liệu).
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
 N'Phong Dien District, Can Tho City'),

(35375, N'Snack da cá vị trứng muối Vidaca',
 N'Vidaca Salted Egg Fish Skin Snack', N'vidaca-salted-egg-fish-skin-snack',
 N'O Mon, Can Tho',
 N'Salted egg flavored fish skin snack by Vidaca Food JSC, certified 5-star OCOP.',
 N'The salted egg version of Vidaca fish skin snack, combining a rich, savory salted egg coating with crispy fish skin. It is popular in both domestic and international markets.',
 N'5-star OCOP (national level)',
 N'Vidaca Food JSC',
 N'Tra Noc Industrial Park, O Mon District, Can Tho City'),

(35377, N'Trà gạo lứt đậu đỏ Thuận Hòa',
 N'Thuan Hoa Brown Rice and Red Bean Tea', N'thuan-hoa-brown-rice-and-red-bean-tea',
 N'Cai Rang, Can Tho',
 N'Healthy brown rice and red bean tea by Thuan Hoa Production Facility, certified 4-star OCOP.',
 N'Tea bags made from roasted brown rice and red beans, with a naturally nutty aroma and quick, convenient preparation. Many families choose it as an everyday drink in place of traditional tea.',
 N'4-star OCOP',
 N'Thuan Hoa Production Facility',
 N'Ba Lang Ward, Cai Rang District, Can Tho City'),

(35378, N'Trà 5 thứ đậu Thuận Hòa',
 N'Thuan Hoa Five-Bean Tea', N'thuan-hoa-five-bean-tea',
 N'Cai Rang, Can Tho',
 N'Grain tea made from five kinds of beans by Thuan Hoa Production Facility, certified 4-star OCOP.',
 N'A grain tea blended from five roasted beans (black beans, red beans, mung beans, white beans and soybeans). It has a rich flavor, contains no sugar and is suitable for people on a diet.',
 N'4-star OCOP',
 N'Thuan Hoa Production Facility',
 N'Ba Lang Ward, Cai Rang District, Can Tho City'),

(35379, N'Trà đậu đen xanh lòng Thuận Hòa',
 N'Thuan Hoa Green-Kernel Black Bean Tea', N'thuan-hoa-green-kernel-black-bean-tea',
 N'Cai Rang, Can Tho',
 N'Fragrant roasted green-kernel black bean tea by Thuan Hoa Production Facility, certified 4-star OCOP.',
 N'Green-kernel black beans hand-roasted using a family recipe, retaining their distinctive aroma and helping to cleanse the body.',
 N'4-star OCOP',
 N'Thuan Hoa Production Facility',
 N'Ba Lang Ward, Cai Rang District, Can Tho City'),

(35381, N'Trà hòa tan tía tô Hygie',
 N'Hygie Instant Perilla Tea', N'hygie-instant-perilla-tea',
 N'Ninh Kieu, Can Tho',
 N'Instant tea made from perilla leaves by Hygie & Panacee One-Member Co., Ltd., certified 4-star OCOP.',
 N'Instant tea extracted from perilla leaves, with a light and refreshing taste. It helps relieve colds and strengthen resistance, and is convenient to carry.',
 N'4-star OCOP',
 N'Hygie & Panacee One-Member Co., Ltd.',
 N'An Khanh Ward, Ninh Kieu District, Can Tho City'),

(35383, N'Mắm cá lóc Út Anh',
 N'Ut Anh Fermented Snakehead Fish', N'ut-anh-fermented-snakehead-fish',
 N'Thot Not, Can Tho',
 N'Traditional fermented snakehead fish by Ut Anh Facility, certified 3-star OCOP.',
 N'Snakehead fish fermented using a traditional Mekong Delta recipe from fresh wild-caught snakehead. The fish is evenly fermented with a rich flavor, and can be steamed or mixed into a tamarind fish sauce.',
 N'3-star OCOP',
 N'Ut Anh Facility',
 N'Tan Loc Ward, Thot Not District, Can Tho City'),

(35384, N'Khô cá lóc một nắng Thốt Nốt',
 N'Thot Not One-Sun Dried Snakehead Fish', N'thot-not-one-sun-dried-snakehead-fish',
 N'Thot Not, Can Tho',
 N'One-sun dried snakehead fish, a Thot Not specialty, certified 3-star OCOP.',
 N'Snakehead fish dried naturally under the sun for one day, keeping the tender texture and sweetness of wild fish. It is seasoned only with salt and basic spices, with no preservatives.',
 N'3-star OCOP',
 N'Thot Not Dried Fish Production Facility',
 N'Thot Not District, Can Tho City'),

(35385, N'Rượu mận lên men Thốt Nốt',
 N'Thot Not Fermented Plum Wine', N'thot-not-fermented-plum-wine',
 N'Thot Not, Can Tho',
 N'Wine fermented from local plums, certified 3-star OCOP.',
 N'Wine handcrafted by fermenting local plums, with a balanced sweet and sour taste and low alcohol content, served as an aperitif.',
 N'3-star OCOP',
 N'Thot Not Production Facility',
 N'Thot Not District, Can Tho City'),

(35386, N'Nước ép ổi lên men Thốt Nốt',
 N'Thot Not Fermented Guava Juice', N'thot-not-fermented-guava-juice',
 N'Thot Not, Can Tho',
 N'Naturally fermented guava juice, certified 3-star OCOP.',
 N'Naturally fermented guava juice with no artificial sweeteners, retaining a light sour taste and the distinctive guava aroma, and supporting digestion.',
 N'3-star OCOP',
 N'Thot Not Production Facility',
 N'Thot Not District, Can Tho City'),

(35387, N'Bánh tráng ngọt Thới Lai',
 N'Thoi Lai Sweet Rice Paper', N'thoi-lai-sweet-rice-paper',
 N'Thoi Lai, Can Tho',
 N'Handmade sweet rice paper from the Thoi Lai craft village, certified 3-star OCOP.',
 N'Sweet rice paper hand-made from rice flour and coconut milk and dried naturally in the sun. It has a mild, rich sweetness and is a long-standing specialty of the Thoi Lai craft village.',
 N'3-star OCOP',
 N'Thoi Lai Rice Paper Craft Village',
 N'Thoi Lai District, Can Tho City'),

(35388, N'Gạo sạch Thới Lai',
 N'Thoi Lai Clean Rice', N'thoi-lai-clean-rice',
 N'Thoi Lai, Can Tho',
 N'Clean rice grown to VietGAP standards by Thoi Lai Agricultural Cooperative, certified 3-star OCOP.',
 N'Clean rice cultivated to VietGAP standards in the Thoi Lai rice-growing area. The grains are clear white and cook into soft, naturally fragrant rice, free of pesticide residues.',
 N'3-star OCOP, VietGAP',
 N'Thoi Lai Agricultural Cooperative',
 N'Thoi Lai District, Can Tho City'),

(35390, N'Dâu Hạ Châu Phong Điền',
 N'Phong Dien Ha Chau Burmese Grape', N'phong-dien-ha-chau-burmese-grape',
 N'Phong Dien, Can Tho',
 N'Ha Chau Burmese grape, a famous fruit specialty of Phong Dien, certified 4-star OCOP.',
 N'Ha Chau Burmese grape is a specialty variety that grows only in Phong Dien. The fruit is golden yellow with a mild sweetness and no sourness, and is regarded as the finest Burmese grape of the Mekong Delta.',
 N'4-star OCOP',
 N'Phong Dien Ha Chau Burmese Grape Cooperative',
 N'Phong Dien District, Can Tho City'),

(35391, N'Bánh tét lá cẩm Cần Thơ',
 N'Can Tho Purple Sticky Rice Cake (Banh Tet La Cam)', N'can-tho-purple-sticky-rice-cake-banh-tet-la-cam',
 N'Binh Thuy, Can Tho',
 N'Traditional purple sticky rice cake (banh tet la cam), certified 4-star OCOP.',
 N'Hand-wrapped cylindrical cake made from sticky rice naturally colored purple with magenta plant leaves (la cam), filled with pork and mung beans, preserving the traditional Tet flavor of Can Tho.',
 N'4-star OCOP',
 N'Chin Cam Banh Tet Facility',
 N'Binh Thuy District, Can Tho City'),

(35392, N'Bún khô Nhà Bè',
 N'Nha Be Dried Rice Vermicelli', N'nha-be-dried-rice-vermicelli',
 N'Cai Rang, Can Tho',
 N'Handmade dried rice vermicelli by Nha Be Facility at Cai Rang floating market, certified 3-star OCOP.',
 N'Dried rice vermicelli made from rice and sun-dried using a family recipe passed down for generations near Cai Rang floating market. The noodles are chewy, cook quickly and contain no bleaching agents.',
 N'3-star OCOP',
 N'Nha Be Facility',
 N'Cai Rang Floating Market, Cai Rang District, Can Tho City');

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
