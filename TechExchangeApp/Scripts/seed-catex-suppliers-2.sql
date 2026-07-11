/*
Creates 5 more real suppliers for the catex.vn-imported batch, found via web search
by matching each seller cluster's branded product line / model numbers to their
Vietnamese distributor (not self-identified in the listing text itself, unlike
Vạn Nghĩa / Điện Lạnh Triều An in seed-catex-suppliers.sql — these are best-match
distributors for the specific branded products, chosen because they carry the
exact product line/model numbers found in the listings).
1 product (MÁY TARO KHÍ NÉN AR-22, ID 35443) still has no confident match and is
left without NCUId.
Idempotent — guarded by FullName / by ID for the linking updates.
*/

SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Now DATETIME = GETDATE();

CREATE TABLE #NcuSeed (
    FullName NVARCHAR(200), DiaChi NVARCHAR(500), Phone NVARCHAR(50), Email NVARCHAR(50),
    LoaiHinhToChuc NVARCHAR(500), ChucNangChinh NVARCHAR(MAX), Website NVARCHAR(50)
);

INSERT INTO #NcuSeed VALUES (N'Công ty Cổ phần Công nghệ Sinh học Biotech Việt Nam', N'68 Nguyễn Huệ, Phường Sài Gòn, TP. Hồ Chí Minh', '0914838782', 'info@biotechvietnam.org', 'DNSX', N'Nhà phân phối độc quyền các dòng vi sinh S&E Microbiology (Mỹ) và Shakti Biotech (Ấn Độ): dòng BIO (phân bón/vi sinh cây trồng), BIOCLEAN/AQUACLEAN/AQUA (vi sinh xử lý nước thải, ao nuôi thủy sản).', 'https://biotechvietnam.org/');
INSERT INTO #NcuSeed VALUES (N'Công ty TNHH TĐQ Việt Nam', N'Số 28 đường Vũ Trọng Khánh, P. Đằng Giang, Q. Ngô Quyền, TP. Hải Phòng', '0902012608', 'congtytdq@gmail.com', 'Khac', N'Nhà phân phối vi sinh Bionetix (Canada) tại Việt Nam: BCP54, BCP50, MSBA100 xử lý nước thải/ao nuôi thủy sản/bể tự hoại.', NULL);
INSERT INTO #NcuSeed VALUES (N'GTC Tech JSC', N'Tầng 7, Số 49 Trung Kính, Phường Yên Hoà, TP. Hà Nội', '0902253263', 'contact@gtctelecom.vn', 'Khac', N'Nhà phân phối thiết bị hội nghị truyền hình: Yealink (bộ thiết bị MVC320, loa MSpeech) và Oneking (camera hội nghị).', 'https://gtctelecom.vn/');
INSERT INTO #NcuSeed VALUES (N'Công ty TNHH TM DV T.T.K', N'51 Bàu Cát 3, Phường Tân Bình, TP. Hồ Chí Minh', '0918428209', 'hoachat@ttkco.com', 'Khac', N'Nhà cung cấp hóa chất ngành in vải/dệt nhuộm và bao bì: trợ chất nhuộm/in hấp, màu paste in vải, bột màu hữu cơ, chất phủ bóng, nhũ tương sơn gỗ.', 'https://www.ttkco.com/');
INSERT INTO #NcuSeed VALUES (N'Công ty TNHH Thiết Bị Văn Lang', N'Số 40 Đường 27, Khu đô thị Vạn Phúc, Phường Hiệp Bình, TP. Hồ Chí Minh', '0909178528', 'info@vanlangvn.com', 'Khac', N'Nhập khẩu và phân phối thiết bị kiểm tra chất lượng: máy dò kim loại Cassel (Đức) cho ngành thực phẩm, dệt may.', 'https://vanlangvn.com/');

INSERT INTO NhaCungUng
    (FullName, DiaChi, Phone, Email, LoaiHinhToChuc, ChucNangChinh, Website,
     StatusId, IsActivated, LanguageId, Domain, SiteId, Created, Modified, CreatedBy)
SELECT
    s.FullName, s.DiaChi, s.Phone, s.Email, s.LoaiHinhToChuc, s.ChucNangChinh, s.Website,
    3, 1, 1, 'VN', 1, @Now, @Now, 'catex-import'
FROM #NcuSeed s
WHERE NOT EXISTS (SELECT 1 FROM NhaCungUng n WHERE n.FullName = s.FullName);

DROP TABLE #NcuSeed;
GO

-- Biotech Việt Nam: BIO*/AQUA*/AQUACLEAN*/BIOCLEAN* line (30 products)
UPDATE p
SET p.NCUId = n.CungUngId
FROM SanPhamCNTB p
CROSS JOIN (SELECT TOP 1 CungUngId FROM NhaCungUng WHERE FullName = N'Công ty Cổ phần Công nghệ Sinh học Biotech Việt Nam') n
WHERE p.Creator = 'catex-import'
  AND p.ID IN (35400,35401,35402,35403,35404,35405,35406,35407,35408,35409,35410,35411,
               35414,35415,35416,35417,35418,35419,35420,35421,35422,35423,35424,35425,
               35426,35427,35428,35429,35430,35431);

-- TDQ Vietnam: Bionetix line (3 products)
UPDATE p
SET p.NCUId = n.CungUngId
FROM SanPhamCNTB p
CROSS JOIN (SELECT TOP 1 CungUngId FROM NhaCungUng WHERE FullName = N'Công ty TNHH TĐQ Việt Nam') n
WHERE p.Creator = 'catex-import' AND p.ID IN (35412, 35453, 35454);

-- GTC Tech: Yealink/Oneking conference gear (3 products)
UPDATE p
SET p.NCUId = n.CungUngId
FROM SanPhamCNTB p
CROSS JOIN (SELECT TOP 1 CungUngId FROM NhaCungUng WHERE FullName = N'GTC Tech JSC') n
WHERE p.Creator = 'catex-import' AND p.ID IN (35432, 35435, 35438);

-- T.T.K: textile/printing/coating chemicals (6 products)
UPDATE p
SET p.NCUId = n.CungUngId
FROM SanPhamCNTB p
CROSS JOIN (SELECT TOP 1 CungUngId FROM NhaCungUng WHERE FullName = N'Công ty TNHH TM DV T.T.K') n
WHERE p.Creator = 'catex-import' AND p.ID IN (35434, 35436, 35440, 35444, 35446, 35447);

-- Văn Lang: Cassel metal detector (1 product)
UPDATE p
SET p.NCUId = n.CungUngId
FROM SanPhamCNTB p
CROSS JOIN (SELECT TOP 1 CungUngId FROM NhaCungUng WHERE FullName = N'Công ty TNHH Thiết Bị Văn Lang') n
WHERE p.Creator = 'catex-import' AND p.ID = 35441;

SELECT p.ID, p.NCUId, n.FullName
FROM SanPhamCNTB p
LEFT JOIN NhaCungUng n ON n.CungUngId = p.NCUId
WHERE p.Creator = 'catex-import'
ORDER BY p.NCUId, p.ID;
GO
