-- CHI DOC, khong sua du lieu. Chay tren CSDL may chu sau khi chay chay-mot-lan-2026-11-05.sql.
SET NOCOUNT ON;

-- 1. eContract: tat ca phai = 1
SELECT CASE WHEN COL_LENGTH('ProjectContracts', 'SigningProvider') IS NULL THEN 0 ELSE 1 END AS CotHopDong,
       CASE WHEN OBJECT_ID('EContractRecipients') IS NULL THEN 0 ELSE 1 END AS BangRecipients,
       CASE WHEN OBJECT_ID('EContractFields') IS NULL THEN 0 ELSE 1 END AS BangFields,
       CASE WHEN OBJECT_ID('EContractOperations') IS NULL THEN 0 ELSE 1 END AS BangOperations;

-- 2. Du lieu tim kiem: CoMoTaChiTiet phai = 1, TongDong > 0
SELECT CASE WHEN OBJECT_DEFINITION(OBJECT_ID('uspReIndexSearchContents')) LIKE '%S.MoTa AS NVARCHAR%' THEN 1 ELSE 0 END AS CoMoTaChiTiet,
       (SELECT COUNT(*) FROM SearchIndexContents) AS TongDong,
       (SELECT COUNT(*) FROM SearchIndexContents WHERE RefId IN (44050, 44051)) AS DongSanPhamVHT2;

-- 3. Linh vuc: san pham co CategoryId nhung chua co dong linh vuc (lan chay truoc bi loi)
SELECT COUNT(*) AS SanPhamThieuLinhVuc FROM SanPhamCNTB s
 WHERE NULLIF(LTRIM(RTRIM(s.CategoryId)), '') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM SanPhamCNTBCategory c WHERE c.SanPhamCNTBId = s.ID);

-- 3b. Ma linh vuc khong con trong danh muc (nguyen nhan loi)
SELECT TRY_CAST(LTRIM(RTRIM(v.value)) AS INT) AS MaKhongTonTai, COUNT(*) AS SoSanPham
  FROM SanPhamCNTB s
 CROSS APPLY STRING_SPLIT(ISNULL(s.CategoryId, ''), ';') v
 WHERE TRY_CAST(LTRIM(RTRIM(v.value)) AS INT) > 0
   AND NOT EXISTS (SELECT 1 FROM Category k WHERE k.CatId = TRY_CAST(LTRIM(RTRIM(v.value)) AS INT))
 GROUP BY TRY_CAST(LTRIM(RTRIM(v.value)) AS INT)
 ORDER BY 1;

-- 3c. Linh vuc cua san pham VHT2
SELECT s.ID, s.Code, s.CategoryId, (SELECT COUNT(*) FROM SanPhamCNTBCategory c WHERE c.SanPhamCNTBId = s.ID) AS SoDongLinhVuc
  FROM SanPhamCNTB s WHERE s.ID IN (44050, 44051);

-- 4. San pham da xoa o buoc 3 (13 dong): doc lai xem co san pham that khong
IF OBJECT_ID('SanPhamCNTB_DeletedBackup_20261031') IS NOT NULL
    EXEC(N'SELECT ID, LanguageId, OriginalId, Code, Name, Creator, Created, StatusId FROM SanPhamCNTB_DeletedBackup_20261031 ORDER BY ID');

-- 4b. San pham thu con sot
SELECT ID, Code, Name, StatusId FROM SanPhamCNTB WHERE Name LIKE N'%pentest%';

-- 5. Du lieu rac: buoc 5 da hoan tac neu BackupNhaCungUng = 0, BackupHoSo = 0
SELECT (SELECT COUNT(*) FROM NhaCungUng) AS TongNhaCungUng,
       (SELECT COUNT(*) FROM NhaCungUng WHERE FullName LIKE N'pHqghUme%' OR Phone = '555-666-0606') AS NhaCungUngRac,
       (SELECT COUNT(*) FROM Projects) AS TongHoSo,
       (SELECT COUNT(*) FROM Projects WHERE ProjectName LIKE N'%pHqghUme%' OR ProjectName LIKE N'%<img src=%' OR ProjectName LIKE N'%pentest%') AS HoSoRacTheoTen,
       CASE WHEN OBJECT_ID('NhaCungUng_DeletedBackup_20261104') IS NULL THEN 0 ELSE 1 END AS BackupNhaCungUng,
       CASE WHEN OBJECT_ID('Projects_DeletedBackup_20261104') IS NULL THEN 0 ELSE 1 END AS BackupHoSo;

-- 5b. Du lieu dang gan voi ho so rac (de biet can xoa gi truoc)
;WITH Rac AS (
    SELECT p.Id FROM Projects p
     WHERE (p.ProjectName LIKE N'%pHqghUme%' OR p.ProjectName LIKE N'%<img src=%' OR p.ProjectName LIKE N'%pentest%'
            OR EXISTS (SELECT 1 FROM TechTransferRequests t WHERE t.ProjectId = p.Id AND t.DienThoai = '555-666-0606'))
       AND p.ProjectName NOT LIKE N'%VHT2%'
)
SELECT (SELECT COUNT(*) FROM Rac) AS HoSoRac,
       (SELECT COUNT(*) FROM ChatConversations WHERE ProjectId IN (SELECT Id FROM Rac)) AS HoiThoai,
       (SELECT COUNT(*) FROM SupportRequests WHERE ProjectId IN (SELECT Id FROM Rac)) AS YeuCauHoTro,
       (SELECT COUNT(*) FROM RFQInvitations WHERE ProjectId IN (SELECT Id FROM Rac)) AS LoiMoi,
       (SELECT COUNT(*) FROM ProposalSubmissions WHERE ProjectId IN (SELECT Id FROM Rac)) AS DeXuat,
       (SELECT COUNT(*) FROM ProjectContracts WHERE ProjectId IN (SELECT Id FROM Rac)) AS HopDong;

-- 6. Du lieu VHT2 phai con: HoSoVHT2 = 2, SanPhamVHT2 = 2
SELECT (SELECT COUNT(*) FROM Projects WHERE ProjectCode IN ('PJ-20261103134013', 'PJ-20261103134112')) AS HoSoVHT2,
       (SELECT COUNT(*) FROM SanPhamCNTB WHERE ID IN (44050, 44051)) AS SanPhamVHT2;
