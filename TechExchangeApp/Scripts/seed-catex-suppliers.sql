/*
Creates 2 real suppliers for the catex.vn-imported product batch, found via web
search — used only where the product's own description text self-identifies a
matching company name/phone/website (Vạn Nghĩa, Điện Lạnh Triều An). The other
44 catex products have no such self-identified, verifiable company and are left
without NCUId pending user decision (see chat).
Idempotent — guarded by FullName.
*/

SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Now DATETIME = GETDATE();

CREATE TABLE #NcuSeed (
    FullName NVARCHAR(200), DiaChi NVARCHAR(500), Phone NVARCHAR(50), Email NVARCHAR(50),
    LoaiHinhToChuc NVARCHAR(500), ChucNangChinh NVARCHAR(MAX), Website NVARCHAR(50)
);

INSERT INTO #NcuSeed VALUES (N'Công ty TNHH TM DV XNK TBCN Vạn Nghĩa', NULL, '02873009929', NULL, 'DNSX', N'Chuyên cung cấp thiết bị công nghiệp: van bi/van màng/van bướm PP/PPH/PVDF, ống mềm bọc PFA/PTFE, khớp nối xoay, bộ trao đổi nhiệt, lưu lượng kế, béc phun, trục làm sạch tôn — gia công theo bản vẽ, nhập khẩu trực tiếp.', 'http://vannghia.com/');
INSERT INTO #NcuSeed VALUES (N'Công ty TNHH Điện Lạnh Triều An', N'403/38/55 Tân Chánh Hiệp 10, P. Tân Chánh Hiệp, Q.12, TP. Hồ Chí Minh', '02836100330', 'dienlanhtrieuan@gmail.com', 'Khac', N'Đại lý cung cấp, thi công, lắp đặt máy lạnh âm trần, tủ đứng, giấu trần ống gió, multi, áp trần các thương hiệu LG, Daikin...', 'https://maylanhtrieuan.com/');

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

-- Link the 10 Vạn Nghĩa products (self-identified in listing text: hotline 028 7300 9929)
UPDATE p
SET p.NCUId = n.CungUngId
FROM SanPhamCNTB p
CROSS JOIN (SELECT TOP 1 CungUngId FROM NhaCungUng WHERE FullName = N'Công ty TNHH TM DV XNK TBCN Vạn Nghĩa') n
WHERE p.Creator = 'catex-import'
  AND p.Name IN (
    N'LƯU LƯỢNG KẾ (Rotametter acid)', N'TRỤC LÀM SẠCH TÔN',
    N'BÉC PHUN (GIA CÔNG THEO BẢN VẼ) - SPRAY HEADER', N'KHỚP NỐI XOAY (BHDTW025A - 010A.03.N727)',
    N'BỘ TRAO ĐỔI NHIỆT - GRAPHITE HEAT EXCHANGER', N'VAN MÀNG PPH',
    N'VAN BI PVDF (MẶT BÍCH/SOCKET)', N'VAN BI PPH (MẶT BÍCH/SOCKET)',
    N'ỐNG MỀM BỌC PFA/PTFE', N'VAN BƯỚM PP/PPH'
  );

-- Link the 1 Triều An product (self-identified in listing text: maylanhtrieuan.com + matching phone)
UPDATE p
SET p.NCUId = n.CungUngId
FROM SanPhamCNTB p
CROSS JOIN (SELECT TOP 1 CungUngId FROM NhaCungUng WHERE FullName = N'Công ty TNHH Điện Lạnh Triều An') n
WHERE p.Creator = 'catex-import'
  AND p.Name = N'Máy lạnh âm trần LG 1 hướng thổi ZTNQ12GULA0 inverter';

SELECT p.ID, p.Name, p.NCUId, n.FullName
FROM SanPhamCNTB p
LEFT JOIN NhaCungUng n ON n.CungUngId = p.NCUId
WHERE p.Creator = 'catex-import' AND p.NCUId IS NOT NULL
ORDER BY p.ID;
GO
