/*
    Auto classify SanPhamCNTB into one Category row under ParentId = 1.

    Server rerun usage:
      - This file is ready to run on server with @Apply = 1.
      - It updates products with empty CategoryId and products previously updated by this script.
      - It does not touch manually categorized products unless @ReclassifyAllCategory = 1.

    Safety:
      - Products are assigned exactly one Category.
      - dbo.SanPhamCNTBCategory rows for updated products are deleted and recreated.
*/

SET NOCOUNT ON;

DECLARE @Apply bit = 1;
DECLARE @OnlyCatexImport bit = 0;
DECLARE @ProductId int = NULL; -- Set a specific product ID to test one known row.
DECLARE @MaxProducts int = NULL;  -- Set 1 to test; keep NULL to process all target rows.
DECLARE @ReclassifyAllCategory bit = 0;
DECLARE @ReclassifyAutoCategory bit = 1;
DECLARE @ShowDetail bit = 0;

IF OBJECT_ID('tempdb..#CategoryRule') IS NOT NULL DROP TABLE #CategoryRule;
CREATE TABLE #CategoryRule
(
    CatId int NOT NULL,
    Weight int NOT NULL,
    Keyword nvarchar(200) NOT NULL
);

/*
    CatId list:
      5 Co khi
      6 Hoa hoc
      4 Dien, dien tu, cong nghe thong tin
      7 Vat lieu va luyen kim
      8 Y hoc
      9 Moi truong
      10 Cong nghe sinh hoc
      11 Cong nghe Nano
      12 Cong nghe thuc pham
      13 Duoc hoc
      14 Trong trot
      15 Chan nuoi
      16 Thu y
      17 Lam nghiep
      18 Thuy san
      1076 Tieu chuan Do luong Chat luong
      1077 Du lieu va thong ke
      1078 Khoa hoc may tinh
      1079 Tri tue nhan tao AI
      1080 Vat ly
      1081 Khoa hoc trai dat va khong gian
      1082 Ky thuat cong trinh xay dung
*/

INSERT INTO #CategoryRule (CatId, Weight, Keyword)
VALUES
-- Co khi
(5, 120, N'cơ khí'),
(5, 120, N'co khi'),
(5, 110, N'gia công'),
(5, 110, N'gia cong'),
(5, 110, N'cắt gọt'),
(5, 100, N'máy tiện'),
(5, 100, N'máy phay'),
(5, 100, N'máy CNC'),
(5, 95, N'khuôn'),
(5, 95, N'hàn'),
(5, 90, N'bơm'),
(5, 90, N'bom'),
(5, 90, N'khớp nối'),
(5, 90, N'khop noi'),
(5, 90, N'gia công theo bản vẽ'),
(5, 90, N'gia cong theo ban ve'),
(5, 90, N'máy taro'),
(5, 90, N'may taro'),
(5, 90, N'trục làm sạch'),
(5, 90, N'truc lam sach'),
(5, 90, N'trao đổi nhiệt'),
(5, 90, N'trao doi nhiet'),
(5, 90, N'heat exchanger'),
(5, 90, N'van bi'),
(5, 90, N'van bướm'),
(5, 90, N'van buom'),
(5, 90, N'van màng'),
(5, 90, N'van mang'),
(5, 90, N'giảm chấn'),
(5, 85, N'phụ kiện bơm'),
(5, 85, N'phu kien bom'),
(5, 70, N'thân bơm'),
(5, 70, N'cánh bơm'),

-- Hoa hoc
(6, 130, N'hóa chất'),
(6, 130, N'hoa chat'),
(6, 125, N'hoá chất'),
(6, 120, N'nhũ tương'),
(6, 120, N'nhu tuong'),
(6, 120, N'acrylic'),
(6, 120, N'keo dán'),
(6, 120, N'keo dan'),
(6, 115, N'keo không khô'),
(6, 115, N'keo khong kho'),
(6, 110, N'dung môi'),
(6, 110, N'dung moi'),
(6, 110, N'phụ gia'),
(6, 110, N'phu gia'),
(6, 105, N'polymer'),
(6, 105, N'cation'),
(6, 100, N'nhựa trao đổi'),
(6, 100, N'nhua trao doi'),
(6, 95, N'sơn nước'),
(6, 95, N'son nuoc'),
(6, 90, N'chất trợ'),
(6, 90, N'chat tro'),
(6, 90, N'trợ chất'),
(6, 90, N'tro chat'),
(6, 90, N'bột màu'),
(6, 90, N'bot mau'),
(6, 90, N'màu dạng paste'),
(6, 90, N'mau dang paste'),
(6, 90, N'nhuộm'),
(6, 90, N'nhuom'),
(6, 90, N'mực in'),
(6, 90, N'muc in'),
(6, 90, N'chất phủ bóng'),
(6, 90, N'chat phu bong'),
(6, 90, N'in bông'),
(6, 90, N'in bong'),
(6, 90, N'in hấp'),
(6, 90, N'in hap'),
(6, 90, N'pvc'),
(6, 90, N'pvdf'),
(6, 90, N'pfa'),
(6, 90, N'pph'),
(6, 85, N'ptfe'),
(6, 85, N'pp-h'),
(6, 80, N'synthomer'),

-- Dien, dien tu, cong nghe thong tin
(4, 130, N'điện tử'),
(4, 130, N'dien tu'),
(4, 125, N'công nghệ thông tin'),
(4, 125, N'cong nghe thong tin'),
(4, 115, N'phần mềm'),
(4, 115, N'phan mem'),
(4, 110, N'thiết bị điện'),
(4, 110, N'thiet bi dien'),
(4, 105, N'cảm biến'),
(4, 105, N'vi điều khiển'),
(4, 100, N'mạch'),
(4, 100, N'iot'),
(4, 95, N'camera'),
(4, 95, N'voip'),
(4, 95, N'gateway'),
(4, 95, N'zoom'),
(4, 95, N'hội nghị truyền hình'),
(4, 95, N'hoi nghi truyen hinh'),
(4, 95, N'call center'),
(4, 95, N'máy lạnh'),
(4, 95, N'may lanh'),
(4, 95, N'inverter'),
(4, 95, N'loa hội nghị'),
(4, 90, N'jabra'),
(4, 90, N'yealink'),
(4, 90, N'mspeech'),

-- Vat lieu va luyen kim
(7, 130, N'vật liệu'),
(7, 130, N'vat lieu'),
(7, 125, N'luyện kim'),
(7, 120, N'kim loại'),
(7, 120, N'kim loai'),
(7, 115, N'hợp kim'),
(7, 110, N'composite'),
(7, 105, N'nhựa'),
(7, 105, N'nhua'),
(7, 105, N'vải'),
(7, 105, N'vai'),
(7, 105, N'dệt'),
(7, 105, N'det'),
(7, 100, N'chống cháy'),
(7, 100, N'chong chay'),
(7, 100, N'tính cháy'),
(7, 95, N'tỷ trọng khói'),
(7, 95, N'lan truyền ngọn lửa'),
(7, 90, N'nhiệt lượng cháy'),
(7, 90, N'oxy oi'),
(7, 85, N'nbs smoke'),
(7, 85, N'cone calorimeter'),

-- Y hoc
(8, 130, N'y học'),
(8, 125, N'y tế'),
(8, 120, N'chẩn đoán'),
(8, 115, N'xét nghiệm'),
(8, 110, N'bệnh viện'),
(8, 110, N'thiết bị y tế'),
(8, 100, N'sinh phẩm'),
(8, 95, N'kháng thể'),
(8, 90, N'kit test'),

-- Moi truong
(9, 135, N'môi trường'),
(9, 135, N'moi truong'),
(9, 130, N'xử lý nước'),
(9, 130, N'xu ly nuoc'),
(9, 125, N'nước thải'),
(9, 125, N'nuoc thai'),
(9, 120, N'nước cấp'),
(9, 120, N'nuoc cap'),
(9, 115, N'lọc nước'),
(9, 115, N'loc nuoc'),
(9, 110, N'màng lọc'),
(9, 110, N'mang loc'),
(9, 105, N'ro filmtec'),
(9, 105, N'khử khoáng'),
(9, 100, N'làm mềm nước'),
(9, 100, N'xử lý khí'),
(9, 95, N'quan trắc'),
(9, 95, N'chất thải'),
(9, 95, N'chat thai'),
(9, 95, N'rác thải'),
(9, 95, N'rac thai'),
(9, 95, N'bãi rác'),
(9, 95, N'bai rac'),
(9, 95, N'mùi hôi'),
(9, 95, N'mui hoi'),
(9, 95, N'bod'),
(9, 95, N'cod'),
(9, 95, N'amonia'),
(9, 95, N'nito'),
(9, 95, N'bùn sinh học'),
(9, 95, N'bun sinh hoc'),
(9, 95, N'bể tự hoại'),
(9, 95, N'be tu hoai'),
(9, 90, N'bionetix'),
(9, 85, N'men vi sinh hiếu khí'),

-- Cong nghe sinh hoc
(10, 130, N'công nghệ sinh học'),
(10, 130, N'cong nghe sinh hoc'),
(10, 120, N'vi sinh'),
(10, 115, N'enzym'),
(10, 110, N'enzyme'),
(10, 110, N'protein'),
(10, 105, N'dna'),
(10, 105, N'rna'),
(10, 100, N'nuôi cấy'),
(10, 95, N'lên men'),
(10, 90, N'bionetix'),

-- Cong nghe Nano
(11, 140, N'nano'),
(11, 120, N'nanoparticle'),
(11, 120, N'graphene'),
(11, 110, N'vật liệu nano'),

-- Cong nghe thuc pham
(12, 135, N'thực phẩm'),
(12, 135, N'thuc pham'),
(12, 125, N'nông sản'),
(12, 125, N'nong san'),
(12, 125, N'ocop'),
(12, 125, N'trà'),
(12, 125, N'snack'),
(12, 125, N'bột gạo'),
(12, 125, N'bot gao'),
(12, 125, N'bánh'),
(12, 125, N'bún'),
(12, 125, N'mắm'),
(12, 125, N'khô cá'),
(12, 125, N'kho ca'),
(12, 125, N'rượu'),
(12, 125, N'nước ép'),
(12, 125, N'nuoc ep'),
(12, 125, N'mật ong'),
(12, 125, N'mat ong'),
(12, 125, N'gạo'),
(12, 125, N'nhãn'),
(12, 125, N'dâu'),
(12, 120, N'chế biến'),
(12, 115, N'bảo quản'),
(12, 110, N'an toàn thực phẩm'),
(12, 105, N'phụ gia thực phẩm'),
(12, 100, N'đồ uống'),
(12, 95, N'cà phê'),
(12, 95, N'trà'),
(12, 90, N'gia vị'),
(12, 90, N'ngũ cốc'),

-- Duoc hoc
(13, 135, N'dược'),
(13, 130, N'dược phẩm'),
(13, 120, N'thuốc'),
(13, 115, N'hoạt chất'),
(13, 110, N'chiết xuất'),
(13, 100, N'viên nang'),

-- Trong trot
(14, 135, N'trồng trọt'),
(14, 135, N'trong trot'),
(14, 130, N'cây trồng'),
(14, 130, N'cay trong'),
(14, 125, N'nông nghiệp'),
(14, 125, N'nong nghiep'),
(14, 115, N'phân bón'),
(14, 115, N'phan bon'),
(14, 110, N'giống cây'),
(14, 105, N'nhà kính'),
(14, 100, N'tưới'),
(14, 95, N'đất trồng'),
(14, 120, N'vitamin cho cây'),
(14, 120, N'vitamin cho cay'),
(14, 120, N'magie cho cây'),
(14, 120, N'magie cho cay'),
(14, 120, N'kali'),
(14, 120, N'npk'),
(14, 120, N'tuyến trùng'),
(14, 120, N'tuyen trung'),
(14, 120, N'nấm bệnh'),
(14, 120, N'nam benh'),
(14, 120, N'quang hợp'),
(14, 120, N'quang hop'),
(14, 120, N'sâu hại'),
(14, 120, N'sau hai'),
(14, 120, N'thuốc bảo vệ thực vật'),
(14, 120, N'thuoc bao ve thuc vat'),
(14, 120, N'kiến lửa'),
(14, 120, N'kien lua'),

-- Chan nuoi
(15, 135, N'chăn nuôi'),
(15, 135, N'chan nuoi'),
(15, 125, N'gia súc'),
(15, 125, N'gia cầm'),
(15, 115, N'thức ăn chăn nuôi'),
(15, 105, N'chuồng trại'),
(15, 105, N'thức ăn cho bò'),
(15, 105, N'trang trại chăn nuôi'),

-- Thu y
(16, 135, N'thú y'),
(16, 125, N'vắc xin thú y'),
(16, 120, N'vacxin thú y'),
(16, 110, N'thuốc thú y'),
(16, 105, N'bệnh vật nuôi'),

-- Lam nghiep
(17, 135, N'lâm nghiệp'),
(17, 125, N'rừng'),
(17, 115, N'gỗ'),
(17, 110, N'cây lâm nghiệp'),

-- Thuy san
(18, 135, N'thủy sản'),
(18, 135, N'thuỷ sản'),
(18, 135, N'thuy san'),
(18, 130, N'ao nuôi'),
(18, 130, N'ao nuoi'),
(18, 130, N'tôm cá'),
(18, 130, N'tom ca'),
(18, 125, N'tôm'),
(18, 115, N'nuôi trồng thủy sản'),
(18, 115, N'nuôi trồng thuỷ sản'),
(18, 105, N'ao nuôi'),

-- Tieu chuan Do luong Chat luong
(1076, 140, N'tiêu chuẩn'),
(1076, 140, N'tieu chuan'),
(1076, 135, N'đo lường'),
(1076, 135, N'do luong'),
(1076, 125, N'kiểm định'),
(1076, 120, N'kiểm tra'),
(1076, 120, N'kiem tra'),
(1076, 115, N'thử nghiệm'),
(1076, 115, N'thu nghiem'),
(1076, 110, N'phân tích'),
(1076, 110, N'phan tich'),
(1076, 105, N'quatest'),
(1076, 100, N'iso'),
(1076, 95, N'kjeldahl'),
(1076, 90, N'kjelroc'),
(1076, 90, N'máy quang phổ'),
(1076, 90, N'phòng thí nghiệm'),
(1076, 90, N'phong thi nghiem'),
(1076, 90, N'thiết bị đo'),
(1076, 90, N'thiet bi do'),
(1076, 90, N'máy đo'),
(1076, 90, N'may do'),
(1076, 200, N'load cell'),
(1076, 90, N'cảm biến áp suất'),
(1076, 90, N'cam bien ap suat'),
(1076, 90, N'độ ẩm'),
(1076, 90, N'do am'),
(1076, 90, N'độ bền'),
(1076, 90, N'do ben'),
(1076, 90, N'áp suất'),
(1076, 90, N'ap suat'),
(1076, 90, N'lưu lượng kế'),
(1076, 90, N'luu luong ke'),
(1076, 90, N'đo lưu lượng'),
(1076, 90, N'do luu luong'),
(1076, 90, N'khối lượng'),
(1076, 90, N'khoi luong'),
(1076, 90, N'trọng lượng'),
(1076, 90, N'trong luong'),
(1076, 90, N'bộ hiển thị'),
(1076, 90, N'bo hien thi'),
(1076, 90, N'astm'),
(1076, 85, N'tester'),

-- Du lieu va thong ke
(1077, 140, N'dữ liệu'),
(1077, 135, N'thống kê'),
(1077, 120, N'cơ sở dữ liệu'),
(1077, 110, N'big data'),
(1077, 105, N'phân tích dữ liệu'),
(1077, 100, N'dashboard'),

-- Khoa hoc may tinh
(1078, 135, N'khoa học máy tính'),
(1078, 135, N'khoa hoc may tinh'),
(1078, 125, N'thuật toán'),
(1078, 120, N'phần mềm'),
(1078, 115, N'ứng dụng'),
(1078, 110, N'nền tảng'),
(1078, 105, N'website'),
(1078, 100, N'blockchain'),
(1078, 95, N'cloud'),

-- Tri tue nhan tao AI
(1079, 150, N'trí tuệ nhân tạo'),
(1079, 145, N'artificial intelligence'),
(1079, 140, N' ai '),
(1079, 135, N'machine learning'),
(1079, 135, N'học máy'),
(1079, 130, N'deep learning'),
(1079, 125, N'chatbot'),
(1079, 120, N'nhận dạng'),
(1079, 115, N'computer vision'),

-- Vat ly
(1080, 130, N'vật lý'),
(1080, 130, N'vat ly'),
(1080, 125, N'quang phổ'),
(1080, 125, N'quang pho'),
(1080, 120, N'hồng ngoại'),
(1080, 120, N'hong ngoai'),
(1080, 115, N'tử ngoại'),
(1080, 110, N'nhiệt lượng'),
(1080, 105, N'bức xạ'),
(1080, 100, N'laser'),
(1080, 95, N'uvvis'),
(1080, 95, N'nir'),

-- Khoa hoc trai dat va khong gian
(1081, 135, N'trái đất'),
(1081, 130, N'không gian'),
(1081, 125, N'địa chất'),
(1081, 120, N'khí tượng'),
(1081, 115, N'viễn thám'),
(1081, 110, N'gis'),
(1081, 105, N'địa lý'),

-- Ky thuat cong trinh xay dung
(1082, 135, N'xây dựng'),
(1082, 130, N'công trình'),
(1082, 125, N'bê tông'),
(1082, 120, N'xi măng'),
(1082, 115, N'kết cấu'),
(1082, 110, N'vật liệu xây dựng'),
(1082, 105, N'cầu đường'),
(1082, 100, N'kiến trúc');

IF OBJECT_ID('tempdb..#CandidateProduct') IS NOT NULL DROP TABLE #CandidateProduct;
IF OBJECT_ID('tempdb..#ClassifyResult') IS NOT NULL DROP TABLE #ClassifyResult;

SELECT TOP (CASE WHEN @ProductId IS NOT NULL THEN 1 WHEN @MaxProducts IS NULL THEN 2147483647 ELSE @MaxProducts END)
        p.ID,
        p.Name,
        p.MoTaNgan,
        p.CategoryId,
        p.Creator,
        SearchText = LOWER(CONCAT_WS(N' ',
            p.Name,
            p.MoTaNgan,
            p.Keywords,
            p.ThongSo,
            p.UuDiem,
            p.TargetCustomer
        ))
INTO #CandidateProduct
    FROM dbo.SanPhamCNTB p
    WHERE
    (
        @ReclassifyAllCategory = 1
        OR
        ISNULL(LTRIM(RTRIM(p.CategoryId)), '') = ''
        OR (@ReclassifyAutoCategory = 1 AND p.Modifier = N'auto-classify-category')
    )
      AND (@OnlyCatexImport = 0 OR p.Creator = 'catex-import')
      AND (@ProductId IS NULL OR p.ID = @ProductId)
    ORDER BY p.ID;

;WITH Products AS
(
    SELECT *
    FROM #CandidateProduct
),
RawMatched AS
(
    SELECT
        p.ID,
        r.CatId,
        r.Weight,
        MatchedKeyword = CONVERT(nvarchar(max), r.Keyword)
    FROM Products p
    JOIN #CategoryRule r
      ON p.SearchText COLLATE Latin1_General_100_CI_AI
         LIKE N'%' + r.Keyword COLLATE Latin1_General_100_CI_AI + N'%'
    JOIN dbo.Category c
      ON c.CatId = r.CatId
     AND c.ParentId = 1
),
Matched AS
(
    SELECT
        ID,
        CatId,
        Score = SUM(Weight),
        MatchedKeywords = STRING_AGG(MatchedKeyword, N', ')
    FROM RawMatched
    GROUP BY ID, CatId
),
Ranked AS
(
    SELECT
        m.*,
        rn = ROW_NUMBER() OVER (PARTITION BY m.ID ORDER BY m.Score DESC, m.CatId ASC)
    FROM Matched m
)
SELECT
    p.ID,
    p.Name,
    p.MoTaNgan,
    SuggestedCatId = r.CatId,
    SuggestedCategory = c.Title,
    r.Score,
    r.MatchedKeywords
INTO #ClassifyResult
FROM Ranked r
JOIN dbo.SanPhamCNTB p ON p.ID = r.ID
JOIN dbo.Category c ON c.CatId = r.CatId
WHERE r.rn = 1;

SELECT
    TotalNeedCategory = COUNT(*),
    AutoClassified = SUM(CASE WHEN cr.ID IS NULL THEN 0 ELSE 1 END),
    NotMatched = SUM(CASE WHEN cr.ID IS NULL THEN 1 ELSE 0 END)
FROM dbo.SanPhamCNTB p
LEFT JOIN #ClassifyResult cr ON cr.ID = p.ID
WHERE EXISTS (SELECT 1 FROM #CandidateProduct cp WHERE cp.ID = p.ID);

SELECT
    SuggestedCatId,
    SuggestedCategory,
    ProductCount = COUNT(*)
FROM #ClassifyResult
GROUP BY SuggestedCatId, SuggestedCategory
ORDER BY ProductCount DESC, SuggestedCatId;

IF @ShowDetail = 1
BEGIN
    SELECT
        ID,
        Name,
        MoTaNgan,
        SuggestedCatId,
        SuggestedCategory,
        Score,
        MatchedKeywords
    FROM #ClassifyResult
    ORDER BY ID;

    SELECT
        p.ID,
        p.Name,
        p.MoTaNgan
    FROM dbo.SanPhamCNTB p
    LEFT JOIN #ClassifyResult cr ON cr.ID = p.ID
    WHERE cr.ID IS NULL
      AND EXISTS (SELECT 1 FROM #CandidateProduct cp WHERE cp.ID = p.ID)
    ORDER BY p.ID;
END

IF @Apply = 1
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE spc
        FROM dbo.SanPhamCNTBCategory spc
        JOIN #ClassifyResult cr ON cr.ID = spc.SanPhamCNTBId;

        UPDATE p
           SET p.CategoryId = CONVERT(nvarchar(20), cr.SuggestedCatId),
               p.Modified = GETDATE(),
               p.Modifier = N'auto-classify-category'
        FROM dbo.SanPhamCNTB p
        JOIN #ClassifyResult cr ON cr.ID = p.ID;

        INSERT INTO dbo.SanPhamCNTBCategory (SanPhamCNTBId, CatId)
        SELECT cr.ID, cr.SuggestedCatId
        FROM #ClassifyResult cr
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM dbo.SanPhamCNTBCategory spc
            WHERE spc.SanPhamCNTBId = cr.ID
              AND spc.CatId = cr.SuggestedCatId
        );

        COMMIT TRANSACTION;

        SELECT UpdatedProducts = COUNT(*) FROM #ClassifyResult;

        SELECT MissingCategoryAfterUpdate = COUNT(*)
        FROM dbo.SanPhamCNTB
        WHERE ISNULL(LTRIM(RTRIM(CategoryId)), '') = '';

        SELECT AutoProductsWithoutExactlyOneJoinRow = COUNT(*)
        FROM dbo.SanPhamCNTB p
        OUTER APPLY
        (
            SELECT JoinRows = COUNT(*)
            FROM dbo.SanPhamCNTBCategory spc
            WHERE spc.SanPhamCNTBId = p.ID
        ) j
        WHERE p.Modifier = N'auto-classify-category'
          AND ISNULL(LTRIM(RTRIM(p.CategoryId)), '') <> ''
          AND j.JoinRows <> 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END
ELSE
BEGIN
    PRINT N'Preview only. Change @Apply = 1 to update dbo.SanPhamCNTB and dbo.SanPhamCNTBCategory.';
END
