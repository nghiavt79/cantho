/* =====================================================================
   BỔ SUNG SAU KHI CHẠY chay-mot-lan-2026-11-05.sql (CÓ XÓA DỮ LIỆU)
   Làm lại 2 phần lần trước bị lỗi: bổ sung lĩnh vực, xóa nhà cung ứng và hồ sơ rác.
   Chạy lại được. Chạy được bằng sqlcmd hoặc SSMS.
   sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -W -i chay-bo-sung-2026-11-05.sql
   ===================================================================== */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
PRINT N'=== 1. Bo sung linh vuc ===';
GO
INSERT INTO dbo.SanPhamCNTBCategory (SanPhamCNTBId, CatId)
SELECT DISTINCT s.ID, k.CatId
FROM dbo.SanPhamCNTB s
CROSS APPLY STRING_SPLIT(REPLACE(ISNULL(s.CategoryId, ''), ',', ';'), ';') v
JOIN dbo.Category k ON k.CatId = TRY_CAST(LTRIM(RTRIM(v.value)) AS INT)
WHERE NOT EXISTS (SELECT 1 FROM dbo.SanPhamCNTBCategory c WHERE c.SanPhamCNTBId = s.ID);
PRINT N'Đã bổ sung lĩnh vực: ' + CAST(@@ROWCOUNT AS NVARCHAR(20));
GO
PRINT N'=== 2. Xoa nha cung ung, ho so rac ===';
GO
/* =====================================================================
   Dọn dữ liệu không hợp lệ theo L2-01, L2-03 (báo cáo kiểm tra 03/11/2026)
   ---------------------------------------------------------------------
   Chọn:
     - Nhà cung ứng tên "pHqghUme..." hoặc số 555-666-0606, không gắn sản phẩm, gói gian hàng, thu chi, không thuộc tài khoản vht_*.
       (Hồ sơ 17924 gắn sản phẩm VHT2 nên không bị chọn: sửa thông tin trong CMS.)
     - Hồ sơ giao dịch có tên chứa pHqghUme, chuỗi mã thử hoặc chữ pentest, hoặc yêu cầu chuyển giao
       ghi số 555-666-0606; không gồm hồ sơ thử VHT2 và hồ sơ đã có hồ sơ đề xuất, hợp đồng.
   Sản phẩm "Quy trinh pentest": dùng script xoa-san-pham-pentest.sql.
   Chạy 2 lần:
     1) Để nguyên @ThucHien = 0  → chỉ LIỆT KÊ, không thay đổi gì. Đọc kỹ số lượng.
     2) Sửa @ThucHien = 1        → sao lưu rồi xóa trong 1 giao dịch.
   Sao lưu: NhaCungUng_DeletedBackup_20261104, Projects_DeletedBackup_20261104.
   Chạy:  sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -i xoa-du-lieu-ra-quet-2026-11.sql
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @ThucHien BIT = 1;

IF OBJECT_ID('tempdb..#Ncc') IS NOT NULL DROP TABLE #Ncc;
IF OBJECT_ID('tempdb..#Prj') IS NOT NULL DROP TABLE #Prj;
CREATE TABLE #Ncc (Id INT PRIMARY KEY);
CREATE TABLE #Prj (Id INT PRIMARY KEY);

INSERT INTO #Ncc (Id)
SELECT n.CungUngId FROM NhaCungUng n
WHERE (n.FullName LIKE N'pHqghUme%' OR n.Phone = '555-666-0606')
  AND NOT EXISTS (SELECT 1 FROM SanPhamCNTB s WHERE s.NCUId = n.CungUngId)
  AND NOT EXISTS (SELECT 1 FROM Users u WHERE u.UserId = n.UserId AND u.UserName LIKE 'vht[_]%')
  AND NOT EXISTS (SELECT 1 FROM SupplierSubscriptions x WHERE x.CungUngId = n.CungUngId)
  AND NOT EXISTS (SELECT 1 FROM FinanceEntries x WHERE x.CungUngId = n.CungUngId);

INSERT INTO #Prj (Id)
SELECT p.Id FROM Projects p
WHERE (p.ProjectName LIKE N'%pHqghUme%' OR p.ProjectName LIKE N'%<img src=%' OR p.ProjectName LIKE N'%pentest%'
       OR EXISTS (SELECT 1 FROM TechTransferRequests t WHERE t.ProjectId = p.Id AND t.DienThoai = '555-666-0606'))
  AND p.ProjectName NOT LIKE N'%VHT2%'
  AND NOT EXISTS (SELECT 1 FROM ProposalSubmissions x WHERE x.ProjectId = p.Id)
  AND NOT EXISTS (SELECT 1 FROM ProjectContracts x WHERE x.ProjectId = p.Id)
  AND NOT EXISTS (SELECT 1 FROM EContracts x WHERE x.ProjectId = p.Id);

SELECT (SELECT COUNT(*) FROM NhaCungUng) AS TongNhaCungUng, (SELECT COUNT(*) FROM #Ncc) AS NhaCungUngSeXoa,
       (SELECT COUNT(*) FROM Projects) AS TongHoSo, (SELECT COUNT(*) FROM #Prj) AS HoSoSeXoa;
SELECT TOP 20 n.CungUngId, n.FullName, n.Phone, n.Email FROM NhaCungUng n JOIN #Ncc x ON x.Id = n.CungUngId ORDER BY n.CungUngId;
SELECT TOP 20 p.Id, p.ProjectCode, LEFT(p.ProjectName, 100) AS ProjectName, p.CreatedDate FROM Projects p JOIN #Prj x ON x.Id = p.Id ORDER BY p.Id;

IF @ThucHien = 0
BEGIN
    PRINT N'Chỉ liệt kê. Sửa @ThucHien = 1 để sao lưu và xóa.';
    RETURN;
END

BEGIN TRAN;

IF OBJECT_ID('NhaCungUng_DeletedBackup_20261104') IS NULL
    SELECT * INTO NhaCungUng_DeletedBackup_20261104 FROM NhaCungUng WHERE 1 = 0;
IF OBJECT_ID('Projects_DeletedBackup_20261104') IS NULL
    SELECT * INTO Projects_DeletedBackup_20261104 FROM Projects WHERE 1 = 0;

DECLARE @Sql NVARCHAR(MAX);

-- Sao lưu (bảng sao lưu có cột IDENTITY nên phải liệt kê cột)
SELECT @Sql = N'SET IDENTITY_INSERT Projects_DeletedBackup_20261104 ON;
INSERT INTO Projects_DeletedBackup_20261104 (' + STRING_AGG(QUOTENAME(name), ',') + N')
SELECT ' + STRING_AGG(QUOTENAME(name), ',') + N' FROM Projects WHERE Id IN (SELECT Id FROM #Prj);
SET IDENTITY_INSERT Projects_DeletedBackup_20261104 OFF;'
FROM sys.columns WHERE object_id = OBJECT_ID('Projects');
EXEC sp_executesql @Sql;

SELECT @Sql = N'SET IDENTITY_INSERT NhaCungUng_DeletedBackup_20261104 ON;
INSERT INTO NhaCungUng_DeletedBackup_20261104 (' + STRING_AGG(QUOTENAME(name), ',') + N')
SELECT ' + STRING_AGG(QUOTENAME(name), ',') + N' FROM NhaCungUng WHERE CungUngId IN (SELECT Id FROM #Ncc);
SET IDENTITY_INSERT NhaCungUng_DeletedBackup_20261104 OFF;'
FROM sys.columns WHERE object_id = OBJECT_ID('NhaCungUng');
EXEC sp_executesql @Sql;

-- Bảng con có khóa ngoại tới bảng cũng có ProjectId: phải xóa trước
DELETE FROM SupportRequests WHERE ProjectId IN (SELECT Id FROM #Prj)
   OR ConversationId IN (SELECT Id FROM ChatConversations WHERE ProjectId IN (SELECT Id FROM #Prj));
DELETE FROM RFQInvitations WHERE ProjectId IN (SELECT Id FROM #Prj);

-- Xóa dữ liệu con ở mọi bảng có cột ProjectId (trừ bảng sao lưu), rồi xóa hồ sơ
SELECT @Sql = STRING_AGG(CAST(N'DELETE FROM ' AS NVARCHAR(MAX)) + QUOTENAME(t.name) + N' WHERE ProjectId IN (SELECT Id FROM #Prj);', CHAR(10))
FROM sys.tables t JOIN sys.columns c ON c.object_id = t.object_id
WHERE c.name = 'ProjectId' AND t.name NOT LIKE '%Backup%';
EXEC sp_executesql @Sql;

DELETE FROM Projects WHERE Id IN (SELECT Id FROM #Prj);
DELETE FROM NhaCungUng WHERE CungUngId IN (SELECT Id FROM #Ncc);

COMMIT;
PRINT N'Đã sao lưu và xóa.';
GO
SET NOCOUNT ON;
PRINT N'=== 3. Ket qua: doi chieu cac dong duoi day ===';
SELECT (SELECT COUNT(*) FROM NhaCungUng) AS TongNhaCungUng,
       (SELECT COUNT(*) FROM NhaCungUng WHERE FullName LIKE N'pHqghUme%' OR Phone = '555-666-0606') AS NhaCungUngRacConLai,
       (SELECT COUNT(*) FROM Projects) AS TongHoSo,
       (SELECT COUNT(*) FROM Projects WHERE ProjectCode IN ('PJ-20261103134013', 'PJ-20261103134112')) AS HoSoVHT2_PhaiLa2,
       (SELECT COUNT(*) FROM SanPhamCNTB WHERE ID IN (44050, 44051)) AS SanPhamVHT2_PhaiLa2,
       (SELECT COUNT(*) FROM SanPhamCNTBCategory WHERE SanPhamCNTBId IN (44050, 44051)) AS LinhVucVHT2_PhaiLa4;
PRINT N'=== 4. Thong ke ho so, san pham con lai (chi doc) ===';
SELECT TOP 10 u.UserName AS NguoiTaoHoSo, COUNT(*) AS SoHoSo, MIN(p.CreatedDate) AS TuNgay, MAX(p.CreatedDate) AS DenNgay
  FROM Projects p LEFT JOIN Users u ON u.UserId = p.CreatedBy GROUP BY u.UserName ORDER BY COUNT(*) DESC;
SELECT TOP 10 LEFT(ProjectName, 70) AS TenHoSo, COUNT(*) AS SoHoSo
  FROM Projects GROUP BY LEFT(ProjectName, 70) ORDER BY COUNT(*) DESC;
SELECT TOP 10 LEFT(t.DienThoai, 20) AS DienThoai, LEFT(t.Email, 40) AS Email, COUNT(*) AS SoYeuCau
  FROM TechTransferRequests t GROUP BY LEFT(t.DienThoai, 20), LEFT(t.Email, 40) ORDER BY COUNT(*) DESC;
SELECT TOP 10 s.Creator AS NguoiTaoSanPham, s.StatusId, COUNT(*) AS SoSanPham,
       SUM(CASE WHEN c.SanPhamCNTBId IS NULL THEN 1 ELSE 0 END) AS ThieuLinhVuc
  FROM SanPhamCNTB s
  LEFT JOIN (SELECT DISTINCT SanPhamCNTBId FROM SanPhamCNTBCategory) c ON c.SanPhamCNTBId = s.ID
 GROUP BY s.Creator, s.StatusId ORDER BY COUNT(*) DESC;
PRINT N'=== KET THUC: chi thanh cong neu khong co dong Msg nao o tren ===';
GO
