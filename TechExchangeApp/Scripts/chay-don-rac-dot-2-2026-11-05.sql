/* =====================================================================
   DỌN DỮ LIỆU RÁC ĐỢT 2 (L2-01) - CÓ XÓA DỮ LIỆU, chạy sau chay-bo-sung-2026-11-05.sql
   Dữ liệu do công cụ rà quét tạo bằng 3 tài khoản nguoimua, ncMUFCMU, pHqghUme từ 05/10 đến 14/10/2026.
     1) Hồ sơ giao dịch của 3 tài khoản trong khoảng ngày trên, chưa có đề xuất, hợp đồng.
        Giữ PJ-20261013094940, PJ-20261014222726 và hồ sơ trước 05/10/2026.
     2) Sản phẩm trạng thái Nháp của 3 tài khoản.
     3) Nhà cung ứng của 3 tài khoản tạo từ 05/10/2026, không còn gắn sản phẩm.
   Sao lưu vào các bảng *_DeletedBackup_20261104, *_DeletedBackup_20261031. Chạy lại được.
   sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -W -i chay-don-rac-dot-2-2026-11-05.sql
   ===================================================================== */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
PRINT N'=== 1. Ho so giao dich ===';
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF OBJECT_ID('tempdb..#Prj') IS NOT NULL DROP TABLE #Prj;
CREATE TABLE #Prj (Id INT PRIMARY KEY);
INSERT INTO #Prj (Id)
SELECT p.Id FROM Projects p JOIN Users u ON u.UserId = p.CreatedBy
WHERE u.UserName IN ('nguoimua', 'ncMUFCMU', 'pHqghUme')
  AND p.CreatedDate >= '2026-10-05' AND p.CreatedDate < '2026-10-15'
  AND NOT EXISTS (SELECT 1 FROM ProposalSubmissions x WHERE x.ProjectId = p.Id)
  AND NOT EXISTS (SELECT 1 FROM ProjectContracts x WHERE x.ProjectId = p.Id)
  AND NOT EXISTS (SELECT 1 FROM EContracts x WHERE x.ProjectId = p.Id);
SELECT COUNT(*) AS HoSoSeXoa FROM #Prj;

BEGIN TRAN;
IF OBJECT_ID('Projects_DeletedBackup_20261104') IS NULL
    SELECT * INTO Projects_DeletedBackup_20261104 FROM Projects WHERE 1 = 0;
DECLARE @Sql NVARCHAR(MAX);
SELECT @Sql = N'SET IDENTITY_INSERT Projects_DeletedBackup_20261104 ON;
INSERT INTO Projects_DeletedBackup_20261104 (' + STRING_AGG(QUOTENAME(name), ',') + N')
SELECT ' + STRING_AGG(QUOTENAME(name), ',') + N' FROM Projects WHERE Id IN (SELECT Id FROM #Prj);
SET IDENTITY_INSERT Projects_DeletedBackup_20261104 OFF;'
FROM sys.columns WHERE object_id = OBJECT_ID('Projects');
EXEC sp_executesql @Sql;

DELETE FROM SupportRequests WHERE ProjectId IN (SELECT Id FROM #Prj)
   OR ConversationId IN (SELECT Id FROM ChatConversations WHERE ProjectId IN (SELECT Id FROM #Prj));
DELETE FROM RFQInvitations WHERE ProjectId IN (SELECT Id FROM #Prj);
SELECT @Sql = STRING_AGG(CAST(N'DELETE FROM ' AS NVARCHAR(MAX)) + QUOTENAME(t.name) + N' WHERE ProjectId IN (SELECT Id FROM #Prj);', CHAR(10))
FROM sys.tables t JOIN sys.columns c ON c.object_id = t.object_id
WHERE c.name = 'ProjectId' AND t.name NOT LIKE '%Backup%';
EXEC sp_executesql @Sql;
DELETE FROM Projects WHERE Id IN (SELECT Id FROM #Prj);
COMMIT;
GO
PRINT N'=== 2. San pham ===';
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF OBJECT_ID('tempdb..#Xoa') IS NOT NULL DROP TABLE #Xoa;
CREATE TABLE #Xoa (ID INT PRIMARY KEY);
INSERT INTO #Xoa (ID) SELECT ID FROM SanPhamCNTB WHERE Creator IN ('nguoimua', 'ncMUFCMU', 'pHqghUme') AND StatusId = 1;
INSERT INTO #Xoa (ID) SELECT s.ID FROM SanPhamCNTB s
 WHERE s.OriginalId IN (SELECT ID FROM #Xoa) AND s.ID NOT IN (SELECT ID FROM #Xoa);
SELECT COUNT(*) AS SanPhamSeXoa FROM #Xoa;
BEGIN TRAN;

-- 1. Sao lưu rồi xóa bảng con; sao lưu bảng sản phẩm (bảng nào không có trên máy chủ thì bỏ qua)
DECLARE @Bang TABLE (STT INT IDENTITY, Bang SYSNAME, Cot SYSNAME, Xoa BIT);
INSERT INTO @Bang (Bang, Cot, Xoa) VALUES
 ('SanPhamCNTB','ID',0),
 ('SanPhamCNTBCategory','SanPhamCNTBId',1), ('SanPhamCNTBMedia','SanPhamCNTBId',1),
 ('SanPhamCNTBPrice','SanPhamCNTBId',1),    ('SanPhamCNTBIP','SanPhamCNTBId',1),
 ('SanPhamEmbeddings','SanPhamId',1),       ('SanPhamCNTBVersions','SanPhamId',1),
 ('ProductFavorites','SanPhamId',1),        ('ProductWarnings','SanPhamId',1),
 ('SanPhamKhachHang','SanPhamId',1);
-- TimKiemDoiTac.SanPhamId là chuỗi danh sách mã ("32;45;"), không phải khóa → không xử lý

DECLARE @i INT = 1, @n INT = (SELECT COUNT(*) FROM @Bang), @b SYSNAME, @c SYSNAME, @x BIT,
        @bk SYSNAME, @cols NVARCHAR(MAX), @sql NVARCHAR(MAX), @dong INT;
WHILE @i <= @n
BEGIN
    SELECT @b = Bang, @c = Cot, @x = Xoa FROM @Bang WHERE STT = @i;
    SET @i += 1;
    IF OBJECT_ID(@b) IS NULL OR COL_LENGTH(@b, @c) IS NULL CONTINUE;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(@b) AND name = @c
                   AND TYPE_NAME(system_type_id) IN ('int','bigint')) CONTINUE;   -- chỉ cột mã số

    SET @bk = @b + N'_DeletedBackup_20261031';
    IF OBJECT_ID(@bk) IS NULL
        SET @sql = N'SELECT * INTO ' + QUOTENAME(@bk) + N' FROM ' + QUOTENAME(@b)
                 + N' WHERE ' + QUOTENAME(@c) + N' IN (SELECT ID FROM #Xoa);';
    ELSE
    BEGIN   -- chạy lại lần sau: chèn thêm vào bảng sao lưu đã có
        SET @cols = STUFF((SELECT ',' + QUOTENAME(name) FROM sys.columns
                           WHERE object_id = OBJECT_ID(@b) AND is_computed = 0 AND system_type_id <> 189
                           ORDER BY column_id FOR XML PATH('')), 1, 1, '');
        SET @sql = N'INSERT INTO ' + QUOTENAME(@bk) + N' (' + @cols + N') SELECT ' + @cols + N' FROM ' + QUOTENAME(@b)
                 + N' WHERE ' + QUOTENAME(@c) + N' IN (SELECT ID FROM #Xoa);';
        IF OBJECTPROPERTY(OBJECT_ID(@bk), 'TableHasIdentity') = 1
            SET @sql = N'SET IDENTITY_INSERT ' + QUOTENAME(@bk) + N' ON; ' + @sql
                     + N' SET IDENTITY_INSERT ' + QUOTENAME(@bk) + N' OFF;';
    END
    EXEC (@sql);

    IF @x = 1
    BEGIN
        SET @sql = N'DELETE FROM ' + QUOTENAME(@b) + N' WHERE ' + QUOTENAME(@c) + N' IN (SELECT ID FROM #Xoa); SET @d = @@ROWCOUNT;';
        EXEC sp_executesql @sql, N'@d INT OUTPUT', @d = @dong OUTPUT;
        PRINT @b + N': xóa ' + CAST(@dong AS NVARCHAR(10)) + N' dòng';
    END
END

-- 2. Dữ liệu nghiệp vụ: giữ lại, chỉ bỏ liên kết tới sản phẩm đã xóa
IF OBJECT_ID('ChatConversations') IS NOT NULL
    UPDATE ChatConversations SET ProductId = NULL WHERE ProductId IN (SELECT ID FROM #Xoa);
IF OBJECT_ID('TechTransferRequests') IS NOT NULL
    UPDATE TechTransferRequests SET FromId = NULL WHERE TypeData = 1 AND FromId IN (SELECT ID FROM #Xoa);
IF OBJECT_ID('ShowroomSlots') IS NOT NULL
    UPDATE ShowroomSlots SET SanPhamId = NULL WHERE SanPhamId IN (SELECT ID FROM #Xoa);

-- 3. Xóa sản phẩm (bản tiếng Anh trước, bản gốc sau)
DELETE FROM SanPhamCNTB WHERE ID IN (SELECT ID FROM #Xoa) AND OriginalId IS NOT NULL;
DELETE FROM SanPhamCNTB WHERE ID IN (SELECT ID FROM #Xoa);
PRINT N'SanPhamCNTB: xóa ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' dòng (gốc)';

COMMIT;
GO
PRINT N'=== 3. Nha cung ung ===';
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF OBJECT_ID('tempdb..#Ncc') IS NOT NULL DROP TABLE #Ncc;
CREATE TABLE #Ncc (Id INT PRIMARY KEY);
INSERT INTO #Ncc (Id)
SELECT n.CungUngId FROM NhaCungUng n JOIN Users u ON u.UserId = n.UserId
WHERE u.UserName IN ('nguoimua', 'ncMUFCMU', 'pHqghUme') AND n.Created >= '2026-10-05'
  AND NOT EXISTS (SELECT 1 FROM SanPhamCNTB s WHERE s.NCUId = n.CungUngId)
  AND NOT EXISTS (SELECT 1 FROM SupplierSubscriptions x WHERE x.CungUngId = n.CungUngId)
  AND NOT EXISTS (SELECT 1 FROM FinanceEntries x WHERE x.CungUngId = n.CungUngId);
SELECT COUNT(*) AS NhaCungUngSeXoa FROM #Ncc;

BEGIN TRAN;
IF OBJECT_ID('NhaCungUng_DeletedBackup_20261104') IS NULL
    SELECT * INTO NhaCungUng_DeletedBackup_20261104 FROM NhaCungUng WHERE 1 = 0;
DECLARE @Sql NVARCHAR(MAX);
SELECT @Sql = N'SET IDENTITY_INSERT NhaCungUng_DeletedBackup_20261104 ON;
INSERT INTO NhaCungUng_DeletedBackup_20261104 (' + STRING_AGG(QUOTENAME(name), ',') + N')
SELECT ' + STRING_AGG(QUOTENAME(name), ',') + N' FROM NhaCungUng WHERE CungUngId IN (SELECT Id FROM #Ncc);
SET IDENTITY_INSERT NhaCungUng_DeletedBackup_20261104 OFF;'
FROM sys.columns WHERE object_id = OBJECT_ID('NhaCungUng');
EXEC sp_executesql @Sql;
DELETE FROM SanPhamEmbeddings WHERE NCUId IN (SELECT Id FROM #Ncc);
DELETE FROM NhaCungUng WHERE CungUngId IN (SELECT Id FROM #Ncc);
COMMIT;
GO
PRINT N'=== 4. Ket qua ===';
SET NOCOUNT ON;
SELECT (SELECT COUNT(*) FROM Projects) AS TongHoSo, (SELECT COUNT(*) FROM NhaCungUng) AS TongNhaCungUng,
       (SELECT COUNT(*) FROM SanPhamCNTB) AS TongSanPham, (SELECT COUNT(*) FROM SanPhamCNTB WHERE StatusId = 1) AS SanPhamNhap,
       (SELECT COUNT(*) FROM Projects WHERE ProjectCode IN ('PJ-20261103134013', 'PJ-20261103134112', 'PJ-20261014222726')) AS HoSoGiuLai_PhaiLa3;
SELECT p.Id, p.ProjectCode, u.UserName, CAST(p.CreatedDate AS DATE) AS Ngay, LEFT(p.ProjectName, 70) AS TenHoSo
  FROM Projects p LEFT JOIN Users u ON u.UserId = p.CreatedBy ORDER BY p.Id;
SELECT n.CungUngId, LEFT(n.FullName, 40) AS Ten, u.UserName, n.StatusId, n.IsActivated, n.Phone
  FROM NhaCungUng n JOIN Users u ON u.UserId = n.UserId WHERE u.UserName IN ('nguoimua', 'ncMUFCMU', 'pHqghUme');
PRINT N'=== KET THUC: chi thanh cong neu khong co dong Msg nao o tren ===';
GO
