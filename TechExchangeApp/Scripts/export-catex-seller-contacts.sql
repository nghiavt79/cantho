/*
    Export seller phone/email list for catex-imported products.

    Purpose:
      - Send a clean seller contact list to the project owner/investor.
      - Uses the authoritative catex Store when it can be matched by Product.UserId.
      - Falls back to imported NhaCungUng and product-level contact fields.

    Outputs:
      1) Summary by seller/contact.
      2) Detail by product for audit/reconciliation.

    Assumptions:
      - Imported products are marked SanPhamCNTB.Creator = 'catex-import'.
      - Legacy catex database is available as [catex.vn].
      - Legacy catex product names match imported SanPhamCNTB.Name after trim.
*/

SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#CatexSellerContacts') IS NOT NULL
    DROP TABLE #CatexSellerContacts;

;WITH ProductContacts AS
(
    SELECT
        sp.ID AS SanPhamId,
        sp.Code AS MaSanPham,
        sp.Name AS TenSanPham,
        sp.ProductType,
        sp.PublishedDate,
        sp.NCUId,

        cp.Id AS CatexProductId,
        cp.UserId AS CatexUserId,

        n.CungUngId,
        n.FullName AS TenNhaCungUng,
        n.NguoiDaiDien AS NguoiDaiDienNhaCungUng,
        n.Phone AS PhoneNhaCungUng,
        n.Email AS EmailNhaCungUng,
        n.DiaChi AS DiaChiNhaCungUng,
        n.Website AS WebsiteNhaCungUng,

        st.StoreId AS CatexStoreId,
        st.Title AS TenStoreCatex,
        st.HoTen AS NguoiLienHeStore,
        st.Phone AS PhoneStore,
        st.PhoneOther AS PhoneOtherStore,
        st.Email AS EmailStore,
        st.DiaChi AS DiaChiStore,
        st.URLWEB AS WebsiteStore,

        sp.HoTen AS NguoiLienHeSanPham,
        sp.Phone AS PhoneSanPham,
        sp.PhoneOther AS PhoneOtherSanPham,
        sp.OwnerEmail AS EmailSanPham,
        sp.DiaChi AS DiaChiSanPham,
        sp.WebUrl AS WebsiteSanPham
    FROM dbo.SanPhamCNTB sp
    LEFT JOIN dbo.NhaCungUng n
        ON n.CungUngId = sp.NCUId
    LEFT JOIN [catex.vn].dbo.Product cp
        ON LTRIM(RTRIM(cp.TenSanPham)) = LTRIM(RTRIM(sp.Name))
    LEFT JOIN [catex.vn].dbo.Store st
        ON st.UserId = cp.UserId
    WHERE sp.Creator = 'catex-import'
),
NormalizedContacts AS
(
    SELECT
        *,
        COALESCE(
            NULLIF(LTRIM(RTRIM(TenNhaCungUng)), N''),
            NULLIF(LTRIM(RTRIM(TenStoreCatex)), N''),
            NULLIF(LTRIM(RTRIM(NguoiLienHeStore)), N''),
            NULLIF(LTRIM(RTRIM(NguoiLienHeSanPham)), N''),
            N'(Chưa xác định)'
        ) AS TenNguoiBan,
        COALESCE(
            NULLIF(LTRIM(RTRIM(NguoiDaiDienNhaCungUng)), N''),
            NULLIF(LTRIM(RTRIM(NguoiLienHeStore)), N''),
            NULLIF(LTRIM(RTRIM(NguoiLienHeSanPham)), N'')
        ) AS NguoiLienHe,
        COALESCE(
            NULLIF(LTRIM(RTRIM(PhoneNhaCungUng)), N''),
            NULLIF(LTRIM(RTRIM(PhoneStore)), N''),
            NULLIF(LTRIM(RTRIM(PhoneOtherStore)), N''),
            NULLIF(LTRIM(RTRIM(PhoneSanPham)), N''),
            NULLIF(LTRIM(RTRIM(PhoneOtherSanPham)), N'')
        ) AS SoDienThoai,
        COALESCE(
            NULLIF(LTRIM(RTRIM(EmailNhaCungUng)), N''),
            NULLIF(LTRIM(RTRIM(EmailStore)), N''),
            NULLIF(LTRIM(RTRIM(EmailSanPham)), N'')
        ) AS Email,
        COALESCE(
            NULLIF(LTRIM(RTRIM(DiaChiNhaCungUng)), N''),
            NULLIF(LTRIM(RTRIM(DiaChiStore)), N''),
            NULLIF(LTRIM(RTRIM(DiaChiSanPham)), N'')
        ) AS DiaChi,
        COALESCE(
            NULLIF(LTRIM(RTRIM(WebsiteNhaCungUng)), N''),
            NULLIF(LTRIM(RTRIM(WebsiteStore)), N''),
            NULLIF(LTRIM(RTRIM(WebsiteSanPham)), N'')
        ) AS Website,
        CASE
            WHEN CungUngId IS NOT NULL THEN N'NhaCungUng'
            WHEN CatexStoreId IS NOT NULL THEN N'catex.vn Store'
            ELSE N'SanPhamCNTB'
        END AS NguonLienHe
    FROM ProductContacts
)
SELECT *
INTO #CatexSellerContacts
FROM NormalizedContacts;

-- 1) Summary list to send
SELECT
    TenNguoiBan,
    NguoiLienHe,
    SoDienThoai,
    Email,
    DiaChi,
    Website,
    NguonLienHe,
    CungUngId,
    CatexUserId,
    CatexStoreId,
    COUNT(DISTINCT SanPhamId) AS SoLuongSanPham,
    STRING_AGG(CAST(TenSanPham AS NVARCHAR(MAX)), N'; ') AS DanhSachSanPham
FROM #CatexSellerContacts
WHERE NULLIF(LTRIM(RTRIM(ISNULL(SoDienThoai, N''))), N'') IS NOT NULL
   OR NULLIF(LTRIM(RTRIM(ISNULL(Email, N''))), N'') IS NOT NULL
GROUP BY
    TenNguoiBan,
    NguoiLienHe,
    SoDienThoai,
    Email,
    DiaChi,
    Website,
    NguonLienHe,
    CungUngId,
    CatexUserId,
    CatexStoreId
ORDER BY TenNguoiBan;

-- 2) Product-level audit list
SELECT
    SanPhamId,
    MaSanPham,
    TenSanPham,
    CASE ProductType
        WHEN 1 THEN N'Công nghệ'
        WHEN 2 THEN N'Thiết bị'
        WHEN 3 THEN N'Tài sản trí tuệ'
        WHEN 4 THEN N'OCOP'
        ELSE N'Khác'
    END AS LoaiSanPham,
    TenNguoiBan,
    NguoiLienHe,
    SoDienThoai,
    Email,
    DiaChi,
    Website,
    NguonLienHe,
    CungUngId,
    CatexProductId,
    CatexUserId,
    CatexStoreId,
    CASE
        WHEN NULLIF(LTRIM(RTRIM(ISNULL(SoDienThoai, N''))), N'') IS NULL
         AND NULLIF(LTRIM(RTRIM(ISNULL(Email, N''))), N'') IS NULL
            THEN 1
        ELSE 0
    END AS ThieuPhoneVaEmail
FROM #CatexSellerContacts
ORDER BY TenNguoiBan, SanPhamId;

-- 3) Products still missing both phone and email
SELECT
    SanPhamId,
    MaSanPham,
    TenSanPham,
    TenNguoiBan,
    CungUngId,
    CatexProductId,
    CatexUserId,
    CatexStoreId
FROM #CatexSellerContacts
WHERE NULLIF(LTRIM(RTRIM(ISNULL(SoDienThoai, N''))), N'') IS NULL
  AND NULLIF(LTRIM(RTRIM(ISNULL(Email, N''))), N'') IS NULL
ORDER BY SanPhamId;
