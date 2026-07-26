/*
    Auto classify NhaCungUng into one service Category row under ParentId = 2.

    Server usage:
      - Ready to run with @Apply = 1.
      - Updates suppliers with empty DichVu and suppliers previously updated by this script.
      - Does not touch manually categorized suppliers unless @ReclassifyAllService = 1.

    Notes:
      - NhaCungUng service filter uses dbo.NhaCungUng.DichVu.
      - DichVu is stored in semicolon format, e.g. ;25;.
      - Default service category is 25: Tu van lua chon cong nghe va nha cung ung.
*/

SET NOCOUNT ON;

DECLARE @Apply bit = 1;
DECLARE @SupplierId int = NULL; -- Set a specific CungUngId to test one row.
DECLARE @MaxSuppliers int = NULL; -- Set 1 to test; keep NULL to process all target rows.
DECLARE @ReclassifyAllService bit = 0;
DECLARE @ReclassifyAutoService bit = 1;
DECLARE @ShowDetail bit = 0;

IF OBJECT_ID('tempdb..#ServiceRule') IS NOT NULL DROP TABLE #ServiceRule;
CREATE TABLE #ServiceRule
(
    CatId int NOT NULL,
    Weight int NOT NULL,
    Keyword nvarchar(200) NOT NULL
);

/*
    Service categories:
      24 Tu van phap ly va cac chinh sach chuyen giao cong nghe
      25 Tu van lua chon cong nghe va nha cung ung
      26 Tu van thanh lap doanh nghiep khoa hoc cong nghe
      27 Tu van lap du an dau tu
      28 Dinh gia, danh gia cong nghe
      29 Truy xuat nguon goc
      30 So huu tri tue
*/

INSERT INTO #ServiceRule (CatId, Weight, Keyword)
VALUES
-- 24: Phap ly, chinh sach, chuyen giao cong nghe
(24, 160, N'pháp lý'),
(24, 160, N'phap ly'),
(24, 150, N'chính sách'),
(24, 150, N'chinh sach'),
(24, 145, N'chuyển giao công nghệ'),
(24, 145, N'chuyen giao cong nghe'),
(24, 120, N'hợp đồng chuyển giao'),
(24, 120, N'hop dong chuyen giao'),

-- 25: Lua chon cong nghe va nha cung ung
(25, 300, N'nhà cung ứng'),
(25, 300, N'nha cung ung'),
(25, 260, N'nhà phân phối'),
(25, 260, N'nha phan phoi'),
(25, 250, N'phân phối'),
(25, 250, N'phan phoi'),
(25, 240, N'cung cấp'),
(25, 240, N'cung cap'),
(25, 230, N'đại lý'),
(25, 230, N'dai ly'),
(25, 220, N'nhập khẩu'),
(25, 220, N'nhap khau'),
(25, 210, N'thi công'),
(25, 210, N'thi cong'),
(25, 210, N'lắp đặt'),
(25, 210, N'lap dat'),
(25, 200, N'thiết bị'),
(25, 200, N'thiet bi'),
(25, 190, N'sản xuất'),
(25, 190, N'san xuat'),
(25, 180, N'chế tạo'),
(25, 180, N'che tao'),
(25, 170, N'công nghệ'),
(25, 170, N'cong nghe'),
(25, 160, N'giải pháp'),
(25, 160, N'giai phap'),

-- 26: Thanh lap DN KHCN
(26, 180, N'doanh nghiệp khoa học công nghệ'),
(26, 180, N'doanh nghiep khoa hoc cong nghe'),
(26, 160, N'dnkhcn'),
(26, 150, N'thành lập doanh nghiệp'),
(26, 150, N'thanh lap doanh nghiep'),

-- 27: Du an dau tu
(27, 170, N'dự án đầu tư'),
(27, 170, N'du an dau tu'),
(27, 150, N'lập dự án'),
(27, 150, N'lap du an'),
(27, 130, N'đầu tư'),
(27, 130, N'dau tu'),

-- 28: Dinh gia, danh gia, kiem dinh cong nghe
(28, 180, N'định giá'),
(28, 180, N'dinh gia'),
(28, 170, N'đánh giá công nghệ'),
(28, 170, N'danh gia cong nghe'),
(28, 160, N'kiểm định'),
(28, 160, N'kiem dinh'),
(28, 150, N'kiểm tra chất lượng'),
(28, 150, N'kiem tra chat luong'),
(28, 140, N'thử nghiệm'),
(28, 140, N'thu nghiem'),
(28, 130, N'phân tích thí nghiệm'),
(28, 130, N'phan tich thi nghiem'),
(28, 120, N'đo lường'),
(28, 120, N'do luong'),

-- 29: Truy xuat nguon goc
(29, 180, N'truy xuất nguồn gốc'),
(29, 180, N'truy xuat nguon goc'),
(29, 150, N'mã truy xuất'),
(29, 150, N'ma truy xuat'),
(29, 140, N'qr code'),
(29, 140, N'qrcode'),
(29, 120, N'vietgap'),
(29, 120, N'ocop'),

-- 30: So huu tri tue
(30, 220, N'sở hữu trí tuệ'),
(30, 220, N'so huu tri tue'),
(30, 210, N'sáng chế'),
(30, 210, N'sang che'),
(30, 190, N'giải pháp hữu ích'),
(30, 190, N'giai phap huu ich'),
(30, 170, N'bằng sáng chế'),
(30, 170, N'bang sang che'),
(30, 150, N'nhãn hiệu'),
(30, 150, N'nhan hieu');

IF OBJECT_ID('tempdb..#CandidateSupplier') IS NOT NULL DROP TABLE #CandidateSupplier;
IF OBJECT_ID('tempdb..#ClassifyResult') IS NOT NULL DROP TABLE #ClassifyResult;

SELECT TOP (CASE WHEN @SupplierId IS NOT NULL THEN 1 WHEN @MaxSuppliers IS NULL THEN 2147483647 ELSE @MaxSuppliers END)
    n.CungUngId,
    n.FullName,
    n.DichVu,
    n.LinhVucId,
    n.CreatedBy,
    SearchText = LOWER(CONCAT_WS(N' ',
        n.FullName,
        n.TenVietTat,
        n.ChucNangChinh,
        n.DichVu,
        n.SanPham,
        n.Keywords,
        n.LoaiHinhToChuc,
        n.ChungNhan
    ))
INTO #CandidateSupplier
FROM dbo.NhaCungUng n
WHERE
(
    @ReclassifyAllService = 1
    OR ISNULL(LTRIM(RTRIM(n.DichVu)), '') = ''
    OR (@ReclassifyAutoService = 1 AND n.Modifier = N'auto-classify-service')
)
  AND (@SupplierId IS NULL OR n.CungUngId = @SupplierId)
ORDER BY n.CungUngId;

;WITH RawMatched AS
(
    SELECT
        s.CungUngId,
        r.CatId,
        r.Weight,
        MatchedKeyword = CONVERT(nvarchar(max), r.Keyword)
    FROM #CandidateSupplier s
    JOIN #ServiceRule r
      ON s.SearchText COLLATE Latin1_General_100_CI_AI
         LIKE N'%' + r.Keyword COLLATE Latin1_General_100_CI_AI + N'%'
    JOIN dbo.Category c
      ON c.CatId = r.CatId
     AND c.ParentId = 2

    UNION ALL

    SELECT
        s.CungUngId,
        CatId = 25,
        Weight = 50,
        MatchedKeyword = CONVERT(nvarchar(max), N'default supplier service')
    FROM #CandidateSupplier s
),
Matched AS
(
    SELECT
        CungUngId,
        CatId,
        Score = SUM(Weight),
        MatchedKeywords = STRING_AGG(MatchedKeyword, N', ')
    FROM RawMatched
    GROUP BY CungUngId, CatId
),
Ranked AS
(
    SELECT
        m.*,
        rn = ROW_NUMBER() OVER (PARTITION BY m.CungUngId ORDER BY m.Score DESC, m.CatId ASC)
    FROM Matched m
)
SELECT
    s.CungUngId,
    s.FullName,
    SuggestedCatId = r.CatId,
    SuggestedService = c.Title,
    r.Score,
    r.MatchedKeywords
INTO #ClassifyResult
FROM Ranked r
JOIN #CandidateSupplier s ON s.CungUngId = r.CungUngId
JOIN dbo.Category c ON c.CatId = r.CatId
WHERE r.rn = 1;

SELECT
    TotalNeedService = COUNT(*),
    AutoClassified = SUM(CASE WHEN cr.CungUngId IS NULL THEN 0 ELSE 1 END),
    NotMatched = SUM(CASE WHEN cr.CungUngId IS NULL THEN 1 ELSE 0 END)
FROM #CandidateSupplier s
LEFT JOIN #ClassifyResult cr ON cr.CungUngId = s.CungUngId;

SELECT
    SuggestedCatId,
    SuggestedService,
    SupplierCount = COUNT(*)
FROM #ClassifyResult
GROUP BY SuggestedCatId, SuggestedService
ORDER BY SupplierCount DESC, SuggestedCatId;

IF @ShowDetail = 1
BEGIN
    SELECT
        CungUngId,
        FullName,
        SuggestedCatId,
        SuggestedService,
        Score,
        MatchedKeywords
    FROM #ClassifyResult
    ORDER BY CungUngId;
END

IF @Apply = 1
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE n
           SET n.DichVu = CONCAT(N';', CONVERT(nvarchar(20), cr.SuggestedCatId), N';'),
               n.Modified = GETDATE(),
               n.Modifier = N'auto-classify-service'
        FROM dbo.NhaCungUng n
        JOIN #ClassifyResult cr ON cr.CungUngId = n.CungUngId;

        COMMIT TRANSACTION;

        SELECT UpdatedSuppliers = COUNT(*) FROM #ClassifyResult;

        SELECT MissingDichVuAfterUpdate = COUNT(*)
        FROM dbo.NhaCungUng
        WHERE ISNULL(LTRIM(RTRIM(DichVu)), '') = '';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END
ELSE
BEGIN
    PRINT N'Preview only. Change @Apply = 1 to update dbo.NhaCungUng.DichVu.';
END
