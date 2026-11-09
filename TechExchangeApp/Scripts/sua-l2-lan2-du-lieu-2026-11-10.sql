/* =====================================================================
   KHẮC PHỤC LỖI L2 LẦN 2 – PHẦN DỮ LIỆU (theo kết quả bước 0 trên máy chủ 09/11/2026 23:19)
     L2-07  NCC 17924 (vht_seller01): tên/điện thoại/email/người đại diện lấy theo tài khoản vht_seller01;
            QueryString "chi-tiet-nha-cung-ung" → đường dẫn riêng; Xuất bản, Kích hoạt.
            vht_seller02: tạo hồ sơ nhà cung ứng từ thông tin tài khoản (Xuất bản, Kích hoạt).
     L2-09  VHT2-SP-01 (44050), VHT2-SP-02 (44051): ghi bảng SanPhamCNTBCategory, đặt PublishedDate nếu trống,
            cập nhật dữ liệu tìm kiếm có cả mô tả chi tiết (MoTa).
            Thủ tục uspReIndexSearchContents: thêm MoTa (sửa đúng 1 chỗ của bản đang chạy).
     L2-14  vht_buyer01: tăng SessionVersion 1 lần duy nhất (đăng xuất mọi phiên). Đặt lại mật khẩu làm trên CMS.
   KHÔNG xử lý ở đây: L2-03, 2 hồ sơ eContract của L2-14, L2-15.

   CHẠY LẠI AN TOÀN: mỗi mục tự kiểm tra, mục nào đã xong thì bỏ qua; không còn gì cần sửa thì
   không sao lưu, không sửa gì. SessionVersion chỉ tăng nếu chưa từng chạy (chưa có bảng sao lưu
   Users_SessionVersion_Backup_L2Lan2_*).

     - @ThucHien = 0 (mặc định): XEM TRƯỚC – bảng "Trạng thái" cho biết mục nào Cần sửa / Đã xong.
     - @ThucHien = 1: sao lưu các mục Cần sửa (bảng *_Backup_L2Lan2_<ngàygiờ>) rồi sửa trong 1 giao dịch.
   sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -W -s "|" -i sua-l2-lan2-du-lieu-2026-11-10.sql
   Xong chạy lại kiem-tra-buoc0-l2-lan2-2026-11-09.sql.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

DECLARE @ThucHien BIT = 0;   -- 0 = xem trước, 1 = sửa thật

DECLARE @NccId INT = 17924;
DECLARE @S1 INT = (SELECT UserId FROM dbo.Users WHERE UserName = N'vht_seller01');
DECLARE @S2 INT = (SELECT UserId FROM dbo.Users WHERE UserName = N'vht_seller02');
DECLARE @B1 INT = (SELECT UserId FROM dbo.Users WHERE UserName = N'vht_buyer01');
DECLARE @SP TABLE (ID INT PRIMARY KEY);
INSERT @SP VALUES (44050), (44051);

/* ---------- Kiểm tra đầu vào ---------- */
IF @S1 IS NULL OR @S2 IS NULL OR @B1 IS NULL
BEGIN RAISERROR(N'Thiếu tài khoản vht_seller01 / vht_seller02 / vht_buyer01, dừng.', 16, 1); RETURN; END
IF NOT EXISTS (SELECT 1 FROM dbo.NhaCungUng WHERE CungUngId = @NccId AND UserId = @S1)
BEGIN RAISERROR(N'NCC 17924 không thuộc vht_seller01, dừng.', 16, 1); RETURN; END
IF (SELECT COUNT(*) FROM dbo.NhaCungUng WHERE UserId = @S1) <> 1 OR (SELECT COUNT(*) FROM dbo.NhaCungUng WHERE UserId = @S2) > 1
BEGIN RAISERROR(N'Tài khoản bên bán có nhiều hơn 1 hồ sơ NCC, cần xác minh trước.', 16, 1); RETURN; END
IF (SELECT COUNT(*) FROM dbo.SanPhamCNTB s JOIN @SP x ON x.ID = s.ID
     WHERE s.Name LIKE N'VHT2-SP-0%' AND s.LanguageId = 1 AND s.StatusId = 3 AND ISNULL(s.QueryString, N'') <> N'') <> 2
BEGIN RAISERROR(N'44050/44051 không phải 2 sản phẩm VHT2 đang xuất bản (hoặc thiếu QueryString), dừng.', 16, 1); RETURN; END

/* Thủ tục reindex */
DECLARE @Def NVARCHAR(MAX) = OBJECT_DEFINITION(OBJECT_ID('dbo.uspReIndexSearchContents'));
DECLARE @Cu NVARCHAR(200)  = N'ISNULL(S.UuDiem,'''') AS Contents';
DECLARE @Moi NVARCHAR(300) = N'ISNULL(S.UuDiem,'''') + '' '' + ISNULL(CAST(S.MoTa AS NVARCHAR(MAX)),'''') AS Contents';
DECLARE @SoCho INT = CASE WHEN @Def IS NULL THEN 0 ELSE (LEN(@Def) - LEN(REPLACE(@Def, @Cu, N''))) / LEN(@Cu) END;
DECLARE @ViTriCreate INT = PATINDEX(N'%CREATE%PROCEDURE%', @Def);

/* ---------- Xác định mục nào cần sửa ---------- */
DECLARE @Can1 BIT = CASE WHEN EXISTS (
        SELECT 1 FROM dbo.NhaCungUng n JOIN dbo.Users u ON u.UserId = n.UserId
        WHERE n.CungUngId = @NccId
          AND (ISNULL(n.FullName, N'') <> ISNULL(u.FullName, N'') OR ISNULL(n.Phone, N'') <> ISNULL(u.Phone, N'')
               OR ISNULL(n.Email, N'') <> ISNULL(u.Email, N'') OR ISNULL(n.QueryString, N'') IN (N'', N'chi-tiet-nha-cung-ung')
               OR ISNULL(n.StatusId, 0) <> 3 OR ISNULL(n.IsActivated, 0) <> 1 OR ISNULL(n.LanguageId, 0) <> 1))
    THEN 1 ELSE 0 END;
DECLARE @Can2 BIT = CASE WHEN NOT EXISTS (
        SELECT 1 FROM dbo.NhaCungUng n WHERE n.UserId = @S2 AND n.StatusId = 3 AND n.IsActivated = 1 AND n.LanguageId = 1
          AND ISNULL(n.QueryString, N'') NOT IN (N'', N'chi-tiet-nha-cung-ung'))
    THEN 1 ELSE 0 END;
DECLARE @Can3 BIT = CASE WHEN EXISTS (SELECT 1 FROM @SP x
        WHERE NOT EXISTS (SELECT 1 FROM dbo.SanPhamCNTBCategory c WHERE c.SanPhamCNTBId = x.ID)) THEN 1 ELSE 0 END;
DECLARE @Can4 BIT = CASE WHEN EXISTS (SELECT 1 FROM dbo.SanPhamCNTB s JOIN @SP x ON x.ID = s.ID WHERE s.PublishedDate IS NULL)
    THEN 1 ELSE 0 END;
/* chỉ mục: mỗi sản phẩm phải có đúng dòng tìm kiếm chứa phần đầu MoTa */
DECLARE @Can5 BIT = CASE WHEN EXISTS (SELECT 1 FROM dbo.SanPhamCNTB s JOIN @SP x ON x.ID = s.ID
        WHERE NOT EXISTS (SELECT 1 FROM dbo.SearchIndexContents c
                          WHERE c.RefId = s.ID AND c.TypeName IN (N'Công nghệ', N'Thiết bị', N'Tài sản trí tuệ', N'OCOP')
                            AND CHARINDEX(LEFT(LTRIM(ISNULL(CAST(s.MoTa AS NVARCHAR(MAX)), N'')), 200), c.Contents) > 0))
    THEN 1 ELSE 0 END;
DECLARE @Can6 BIT = CASE WHEN @Def LIKE N'%S.MoTa AS NVARCHAR%' THEN 0 ELSE 1 END;
DECLARE @Can7 BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.tables WHERE name LIKE N'Users[_]SessionVersion[_]Backup[_]L2Lan2[_]%')
    THEN 0 ELSE 1 END;

IF @Can6 = 1 AND (@SoCho <> 1 OR @ViTriCreate = 0)
BEGIN RAISERROR(N'Thủ tục uspReIndexSearchContents khác mẫu dự kiến (số chỗ = %d), dừng để xác minh.', 16, 1, @SoCho); RETURN; END

/* ---------- Xem trước ---------- */
SELECT Muc, NoiDung, CASE WHEN Can = 1 THEN N'Cần sửa' ELSE N'Đã xong – bỏ qua' END AS TrangThai
FROM (VALUES
    (1, N'L2-07', N'Hồ sơ NCC 17924 của vht_seller01', @Can1),
    (2, N'L2-07', N'Hồ sơ NCC của vht_seller02', @Can2),
    (3, N'L2-09', N'Lĩnh vực của VHT2-SP-01/02 (bảng SanPhamCNTBCategory)', @Can3),
    (4, N'L2-09', N'Ngày xuất bản của VHT2-SP-01/02', @Can4),
    (5, N'L2-09', N'Dữ liệu tìm kiếm của VHT2-SP-01/02 có mô tả chi tiết', @Can5),
    (6, N'L2-09', N'Thủ tục uspReIndexSearchContents ghép MoTa', @Can6),
    (7, N'L2-14', N'Thu hồi phiên đăng nhập vht_buyer01 (tăng SessionVersion 1 lần)', @Can7)
) t(Stt, Muc, NoiDung, Can)
ORDER BY Stt;

IF @Can1 | @Can2 | @Can3 | @Can4 | @Can5 | @Can6 | @Can7 = 0
BEGIN
    PRINT N'Tất cả các mục đã xong, không cần chạy.';
    RETURN;
END

IF @ThucHien = 0
BEGIN
    PRINT N'XEM TRƯỚC: chưa sửa gì. Đặt @ThucHien = 1 để sửa các mục "Cần sửa".';
    RETURN;
END

/* ---------- Sao lưu (chỉ phần sắp sửa) ---------- */
DECLARE @Tg NVARCHAR(20) = FORMAT(GETDATE(), 'yyyyMMdd_HHmmss');
DECLARE @Sql NVARCHAR(MAX) = N'';
IF @Can1 | @Can2 = 1
    SET @Sql += N'SELECT * INTO dbo.' + QUOTENAME(N'NhaCungUng_Backup_L2Lan2_' + @Tg) + N' FROM dbo.NhaCungUng WHERE UserId IN (@a, @b);';
IF @Can4 = 1
    SET @Sql += N'SELECT * INTO dbo.' + QUOTENAME(N'SanPhamCNTB_Backup_L2Lan2_' + @Tg) + N' FROM dbo.SanPhamCNTB WHERE ID IN (44050, 44051);';
IF @Can3 = 1
    SET @Sql += N'SELECT * INTO dbo.' + QUOTENAME(N'SanPhamCNTBCategory_Backup_L2Lan2_' + @Tg) + N' FROM dbo.SanPhamCNTBCategory WHERE SanPhamCNTBId IN (44050, 44051);';
IF @Can5 = 1
    SET @Sql += N'SELECT * INTO dbo.' + QUOTENAME(N'SearchIndexContents_Backup_L2Lan2_' + @Tg) + N' FROM dbo.SearchIndexContents WHERE RefId IN (44050, 44051);';
IF @Can7 = 1
    SET @Sql += N'SELECT UserId, SessionVersion INTO dbo.' + QUOTENAME(N'Users_SessionVersion_Backup_L2Lan2_' + @Tg) + N' FROM dbo.Users WHERE UserId = @c;';
IF @Can6 = 1
    SET @Sql += N'SELECT CAST(@d AS NVARCHAR(MAX)) AS DinhNghiaCu INTO dbo.' + QUOTENAME(N'uspReIndex_Backup_L2Lan2_' + @Tg) + N';';
EXEC sp_executesql @Sql, N'@a INT, @b INT, @c INT, @d NVARCHAR(MAX)', @S1, @S2, @B1, @Def;
PRINT N'Đã sao lưu các mục cần sửa vào bảng *_Backup_L2Lan2_' + @Tg;

BEGIN TRY
    BEGIN TRAN;

    /* 1) L2-07: NCC 17924 */
    IF @Can1 = 1
    BEGIN
        UPDATE n SET
            n.FullName     = u.FullName,
            n.Phone        = u.Phone,
            n.Email        = u.Email,
            n.NguoiDaiDien = u.FullName,
            n.Domain       = N'VN',            -- quy ước của các NCC đang xuất bản
            n.QueryString  = CASE WHEN ISNULL(n.QueryString, N'') IN (N'', N'chi-tiet-nha-cung-ung')
                                  THEN 'nha-cung-ung-01-van-hanh-thu' ELSE n.QueryString END,
            n.StatusId = 3, n.IsActivated = 1, n.LanguageId = 1,
            n.Modified = GETDATE(), n.Modifier = N'script-L2-lan2'
        FROM dbo.NhaCungUng n JOIN dbo.Users u ON u.UserId = n.UserId
        WHERE n.CungUngId = @NccId AND n.UserId = @S1;
        IF @@ROWCOUNT <> 1 THROW 50001, N'Cập nhật NCC 17924 không đúng 1 dòng.', 1;
    END

    /* 2) L2-07: NCC của vht_seller02 */
    IF @Can2 = 1
    BEGIN
        IF EXISTS (SELECT 1 FROM dbo.NhaCungUng WHERE UserId = @S2)
            UPDATE n SET
                n.FullName = ISNULL(NULLIF(n.FullName, N''), u.FullName),
                n.QueryString = CASE WHEN ISNULL(n.QueryString, N'') IN (N'', N'chi-tiet-nha-cung-ung')
                                     THEN 'nha-cung-ung-02-van-hanh-thu' ELSE n.QueryString END,
                n.StatusId = 3, n.IsActivated = 1, n.LanguageId = 1,
                n.Modified = GETDATE(), n.Modifier = N'script-L2-lan2'
            FROM dbo.NhaCungUng n JOIN dbo.Users u ON u.UserId = n.UserId
            WHERE n.UserId = @S2;
        ELSE
            INSERT dbo.NhaCungUng (FullName, QueryString, DiaChi, Phone, Email, NguoiDaiDien, LinhVucId,
                                   Created, CreatedBy, UserId, IsActivated, Domain, StatusId, LanguageId, SiteId)
            SELECT u.FullName, 'nha-cung-ung-02-van-hanh-thu', n1.DiaChi, u.Phone, u.Email, u.FullName, n1.LinhVucId,
                   GETDATE(), N'script-L2-lan2', u.UserId, 1, N'VN', 3, 1, ISNULL(u.SiteId, n1.SiteId)
            FROM dbo.Users u
            CROSS JOIN (SELECT DiaChi, LinhVucId, SiteId FROM dbo.NhaCungUng WHERE CungUngId = @NccId) n1
            WHERE u.UserId = @S2;
    END

    /* 3) L2-09: lĩnh vực (giống ProductPublishSync.SyncCategoriesAsync) */
    IF @Can3 = 1
    BEGIN
        DECLARE @Cat TABLE (SanPhamCNTBId INT, CatId INT, PRIMARY KEY (SanPhamCNTBId, CatId));
        INSERT @Cat (SanPhamCNTBId, CatId)
        SELECT DISTINCT s.ID, k.CatId
        FROM dbo.SanPhamCNTB s JOIN @SP x ON x.ID = s.ID
        CROSS APPLY STRING_SPLIT(REPLACE(ISNULL(s.CategoryId, ''), ',', ';'), ';') v
        JOIN dbo.Category k ON k.CatId = TRY_CAST(LTRIM(RTRIM(v.value)) AS INT);
        IF (SELECT COUNT(DISTINCT SanPhamCNTBId) FROM @Cat) <> 2
            THROW 50002, N'Không tách được lĩnh vực cho đủ 2 sản phẩm VHT2.', 1;

        DELETE c FROM dbo.SanPhamCNTBCategory c JOIN @SP x ON x.ID = c.SanPhamCNTBId;
        INSERT dbo.SanPhamCNTBCategory (SanPhamCNTBId, CatId) SELECT SanPhamCNTBId, CatId FROM @Cat;
    END

    /* 4) L2-09: ngày xuất bản (chỉ khi đang trống) */
    IF @Can4 = 1
        UPDATE s SET s.PublishedDate = COALESCE(s.Modified, s.Created, GETDATE())
        FROM dbo.SanPhamCNTB s JOIN @SP x ON x.ID = s.ID
        WHERE s.PublishedDate IS NULL;

    /* 5) L2-09: dữ liệu tìm kiếm (giống ProductPublishSync.SyncSearchIndexAsync) */
    IF @Can5 = 1
    BEGIN
        DELETE c FROM dbo.SearchIndexContents c JOIN @SP x ON x.ID = c.RefId
        WHERE c.TypeName IN (N'Công nghệ', N'Thiết bị', N'Tài sản trí tuệ', N'OCOP');

        INSERT dbo.SearchIndexContents (ImgPreview, Title, [Description], Contents, RefId, TypeName, URL, Created, Creator, LanguageId)
        SELECT s.QuyTrinhHinhAnh, REPLACE(s.Name, '-', ' '), ISNULL(s.MoTaNgan, ''),
               ISNULL(s.MoTaNgan, '') + ' ' + ISNULL(s.Thongso, '') + ' ' + ISNULL(s.UuDiem, '') + ' ' + ISNULL(CAST(s.MoTa AS NVARCHAR(MAX)), ''),
               s.ID,
               CASE s.ProductType WHEN 1 THEN N'Công nghệ' WHEN 2 THEN N'Thiết bị' WHEN 3 THEN N'Tài sản trí tuệ' WHEN 4 THEN N'OCOP' END,
               CASE WHEN s.ProductType = 4 THEN N'/ocop/' ELSE N'/san-pham/chi-tiet/' END + s.QueryString + N'-' + CAST(s.ID AS NVARCHAR(50)),
               GETDATE(), 'sys', CAST(s.LanguageId AS NVARCHAR(10))
        FROM dbo.SanPhamCNTB s JOIN @SP x ON x.ID = s.ID
        WHERE s.StatusId = 3;
    END

    /* 7) L2-14: thu hồi phiên đăng nhập (1 lần duy nhất) */
    IF @Can7 = 1
        UPDATE dbo.Users SET SessionVersion = SessionVersion + 1 WHERE UserId = @B1;

    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH

/* 6) L2-09: thủ tục reindex (bản cũ đã sao lưu) */
IF @Can6 = 1
BEGIN
    DECLARE @DefMoi NVARCHAR(MAX) = REPLACE(@Def, @Cu, @Moi);
    SET @DefMoi = STUFF(@DefMoi, @ViTriCreate, 6, N'ALTER');
    EXEC sp_executesql @DefMoi;
    PRINT N'Đã thêm MoTa vào thủ tục uspReIndexSearchContents.';
END

/* ---------- Kết quả sau khi chạy ---------- */
SELECT N'L2-07' AS Muc, u.UserName AS DoiTuong,
       N'NCC ' + CAST(n.CungUngId AS NVARCHAR) + N': ' + n.FullName + N'; ' + ISNULL(n.Phone, N'') + N'; ' + ISNULL(n.QueryString, N'') AS ChiTiet,
       CASE WHEN n.StatusId = 3 AND n.IsActivated = 1 AND n.LanguageId = 1 THEN 'DAT' ELSE 'CHUA DAT' END AS KetLuan
FROM dbo.NhaCungUng n JOIN dbo.Users u ON u.UserId = n.UserId WHERE n.UserId IN (@S1, @S2)
UNION ALL
SELECT N'L2-09', N'ID ' + CAST(s.ID AS NVARCHAR),
       N'Lĩnh vực ' + ISNULL((SELECT STRING_AGG(CAST(c.CatId AS NVARCHAR), N',') FROM dbo.SanPhamCNTBCategory c WHERE c.SanPhamCNTBId = s.ID), N'trống')
       + N'; xuất bản ' + ISNULL(CONVERT(NVARCHAR(10), s.PublishedDate, 103), N'trống'),
       CASE WHEN s.PublishedDate IS NOT NULL
                 AND EXISTS (SELECT 1 FROM dbo.SanPhamCNTBCategory c WHERE c.SanPhamCNTBId = s.ID)
                 AND EXISTS (SELECT 1 FROM dbo.SearchIndexContents c WHERE c.RefId = s.ID
                              AND CHARINDEX(LEFT(LTRIM(ISNULL(CAST(s.MoTa AS NVARCHAR(MAX)), N'')), 200), c.Contents) > 0)
            THEN 'DAT' ELSE 'CHUA DAT' END
FROM dbo.SanPhamCNTB s JOIN @SP x ON x.ID = s.ID
UNION ALL
SELECT N'L2-09', N'uspReIndexSearchContents', N'Ghép MoTa',
       CASE WHEN OBJECT_DEFINITION(OBJECT_ID('dbo.uspReIndexSearchContents')) LIKE N'%S.MoTa AS NVARCHAR%' THEN 'DAT' ELSE 'CHUA DAT' END
UNION ALL
SELECT N'L2-14', UserName, N'SessionVersion = ' + CAST(SessionVersion AS NVARCHAR), 'DAT'
FROM dbo.Users WHERE UserId = @B1;
