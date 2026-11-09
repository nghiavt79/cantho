/* =====================================================================
   BƯỚC 0 – KIỂM TRA MÁY CHỦ TRƯỚC KHI KHẮC PHỤC LỖI L2 LẦN 2 (09/11/2026)
   CHỈ ĐỌC, không sửa dữ liệu.
   Trả về MỘT bảng, chỉ gồm các mục CHUA DAT / CAN XAC MINH (mục DAT không hiện).
   Đặt @HienTatCa = 1 nếu muốn xem cả mục DAT.
   sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -W -s "|" ^
          -i kiem-tra-buoc0-l2-lan2-2026-11-09.sql -o ket-qua-buoc0-l2-lan2.txt
   ===================================================================== */
SET NOCOUNT ON;

DECLARE @HienTatCa BIT = 0;

IF OBJECT_ID('tempdb..#KQ') IS NOT NULL DROP TABLE #KQ;
CREATE TABLE #KQ (Stt INT IDENTITY, Muc NVARCHAR(40), DoiTuong NVARCHAR(200), ChiTiet NVARCHAR(1000), KetLuan VARCHAR(20));

DECLARE @S1 INT = (SELECT UserId FROM dbo.Users WHERE UserName = N'vht_seller01');
DECLARE @S2 INT = (SELECT UserId FROM dbo.Users WHERE UserName = N'vht_seller02');
DECLARE @B1 INT = (SELECT UserId FROM dbo.Users WHERE UserName = N'vht_buyer01');

/* ---------- Tài khoản vận hành thử ---------- */
INSERT #KQ (Muc, DoiTuong, ChiTiet, KetLuan)
SELECT N'Tài khoản', x.UserName,
       CASE WHEN u.UserId IS NULL THEN N'Không có tài khoản'
            ELSE N'UserId=' + CAST(u.UserId AS NVARCHAR) + N'; Khóa=' + CAST(ISNULL(u.IsLocked, 0) AS NVARCHAR) END,
       CASE WHEN u.UserId IS NULL THEN 'CAN XAC MINH' WHEN u.IsLocked = 1 THEN 'CHUA DAT' ELSE 'DAT' END
FROM (VALUES (N'vht_seller01'), (N'vht_seller02'), (N'vht_buyer01'), (N'vht_expert01'), (N'vht_admin')) x(UserName)
LEFT JOIN dbo.Users u ON u.UserName = x.UserName;

/* ---------- L2-14: SessionVersion để thu hồi phiên ---------- */
INSERT #KQ (Muc, DoiTuong, ChiTiet, KetLuan)
SELECT N'L2-14', N'Cột Users.SessionVersion',
       CASE WHEN COL_LENGTH('dbo.Users', 'SessionVersion') IS NULL THEN N'Chưa có cột – chạy add-users-sessionversion.sql' ELSE N'Đã có cột' END,
       CASE WHEN COL_LENGTH('dbo.Users', 'SessionVersion') IS NULL THEN 'CHUA DAT' ELSE 'DAT' END;

INSERT #KQ (Muc, DoiTuong, ChiTiet, KetLuan)
SELECT N'L2-14', p.ProjectCode,
       N'Hồ sơ eContract tạo bằng ' + ISNULL(u.UserName, N'?') + N'; StatusId=' + CAST(p.StatusId AS NVARCHAR)
       + N' – cần đưa ra khỏi phạm vi vận hành thử',
       'CHUA DAT'
FROM dbo.Projects p LEFT JOIN dbo.Users u ON u.UserId = p.CreatedBy
WHERE p.ProjectCode IN (N'PJ-20261106162141', N'PJ-20261107164420');

/* ---------- L2-07: hồ sơ nhà cung ứng của bên bán ---------- */
INSERT #KQ (Muc, DoiTuong, ChiTiet, KetLuan)
SELECT N'L2-07 NCC', x.UserName,
       CASE WHEN n.CungUngId IS NULL THEN N'Chưa có hồ sơ nhà cung ứng'
            ELSE N'NCC ' + CAST(n.CungUngId AS NVARCHAR) + N': ' + ISNULL(n.FullName, N'') + N'; ĐT ' + ISNULL(n.Phone, N'')
                 + N'; Status=' + ISNULL(CAST(n.StatusId AS NVARCHAR), N'NULL') + N'; KichHoat=' + ISNULL(CAST(CAST(n.IsActivated AS INT) AS NVARCHAR), N'NULL')
                 + N'; QueryString=' + ISNULL(n.QueryString, N'NULL') END,
       CASE WHEN n.CungUngId IS NULL THEN 'CHUA DAT'
            WHEN n.StatusId = 3 AND n.IsActivated = 1 AND n.LanguageId = 1 AND ISNULL(n.QueryString, N'') <> N''
                 AND ISNULL(n.Phone, N'') NOT LIKE N'%555-666%' AND ISNULL(n.FullName, N'') NOT LIKE N'%tao boi%' THEN 'DAT'
            ELSE 'CHUA DAT' END
FROM (VALUES (N'vht_seller01'), (N'vht_seller02')) x(UserName)
LEFT JOIN dbo.Users u ON u.UserName = x.UserName
LEFT JOIN dbo.NhaCungUng n ON n.UserId = u.UserId;

/* ---------- L2-07: lời mời bên bán trên 2 hồ sơ thử ---------- */
INSERT #KQ (Muc, DoiTuong, ChiTiet, KetLuan)
SELECT N'L2-07 Hồ sơ thử', x.ProjectCode,
       CASE WHEN p.Id IS NULL THEN N'Không tìm thấy hồ sơ'
            ELSE N'Id=' + CAST(p.Id AS NVARCHAR)
                 + N'; RFQ mới nhất=' + ISNULL(CAST(r.Id AS NVARCHAR), N'chưa có')
                 + N'; Bên bán được mời: ' + ISNULL((SELECT STRING_AGG(u.UserName + N'(RFQ ' + CAST(i.RFQId AS NVARCHAR) + N')', N', ')
                                                      FROM dbo.RFQInvitations i JOIN dbo.Users u ON u.UserId = i.SellerId
                                                     WHERE i.ProjectId = p.Id AND i.IsActive = 1), N'chưa có')
                 + N'; Bước hoàn thành cuối=' + ISNULL(CAST((SELECT MAX(s.StepNumber) FROM dbo.ProjectSteps s
                                                            WHERE s.ProjectId = p.Id AND s.StatusId = 2) AS NVARCHAR), N'0') END,
       CASE WHEN p.Id IS NULL OR r.Id IS NULL THEN 'CAN XAC MINH'
            WHEN EXISTS (SELECT 1 FROM dbo.RFQInvitations i WHERE i.ProjectId = p.Id AND i.RFQId = r.Id AND i.IsActive = 1 AND i.SellerId = @S1)
            THEN 'DAT' ELSE 'CHUA DAT' END
FROM (VALUES (N'PJ-20261103134013'), (N'PJ-20261103134112')) x(ProjectCode)
LEFT JOIN dbo.Projects p ON p.ProjectCode = x.ProjectCode
OUTER APPLY (SELECT TOP 1 r.Id FROM dbo.RFQRequests r WHERE r.ProjectId = p.Id ORDER BY r.Id DESC) r;

/* ---------- L2-02: sản phẩm pentest ---------- */
INSERT #KQ (Muc, DoiTuong, ChiTiet, KetLuan)
SELECT N'L2-02', N'ID ' + CAST(s.ID AS NVARCHAR) + N' ' + ISNULL(s.Code, N'') + N' ' + ISNULL(s.Name, N''),
       N'StatusId=' + ISNULL(CAST(s.StatusId AS NVARCHAR), N'NULL') + N'; Lang=' + CAST(s.LanguageId AS NVARCHAR)
       + N'; dòng tìm kiếm=' + CAST(ix.SoDong AS NVARCHAR),
       CASE WHEN ISNULL(s.StatusId, 0) <> 3 AND ix.SoDong = 0 THEN 'DAT' ELSE 'CHUA DAT' END
FROM dbo.SanPhamCNTB s
OUTER APPLY (SELECT COUNT(*) AS SoDong FROM dbo.SearchIndexContents c
              WHERE c.RefId = s.ID AND c.URL LIKE '%-' + CAST(s.ID AS VARCHAR(20))) ix
WHERE s.ID = 44048 OR s.Code = N'CN-43687' OR s.OriginalId = 44048
   OR (s.Name LIKE N'%pentest%' AND s.StatusId = 3);

/* ---------- L2-03: dự án pentest ---------- */
INSERT #KQ (Muc, DoiTuong, ChiTiet, KetLuan)
SELECT N'L2-03', x.ProjectCode,
       CASE WHEN p.Id IS NULL THEN N'Không còn hồ sơ (đã loại) – kiểm tra bằng chứng sao lưu'
            ELSE N'Id=' + CAST(p.Id AS NVARCHAR) + N'; ' + p.ProjectName + N'; StatusId=' + CAST(p.StatusId AS NVARCHAR)
                 + N'; người tạo=' + ISNULL(u.UserName, N'?') END,
       CASE WHEN p.Id IS NULL THEN 'CAN XAC MINH' ELSE 'CHUA DAT' END
FROM (VALUES (N'PJ-20261014173610'), (N'PJ-20261014173512')) x(ProjectCode)
LEFT JOIN dbo.Projects p ON p.ProjectCode = x.ProjectCode
LEFT JOIN dbo.Users u ON u.UserId = p.CreatedBy;

INSERT #KQ (Muc, DoiTuong, ChiTiet, KetLuan)
SELECT N'L2-03 khác', p.ProjectCode, N'Id=' + CAST(p.Id AS NVARCHAR) + N'; ' + p.ProjectName + N' – tên có pentest, ngoài 2 mã khách nêu',
       'CAN XAC MINH'
FROM dbo.Projects p
WHERE p.ProjectName LIKE N'%pentest%'
  AND p.ProjectCode NOT IN (N'PJ-20261014173610', N'PJ-20261014173512');

/* ---------- L2-15: giá âm ---------- */
INSERT #KQ (Muc, DoiTuong, ChiTiet, KetLuan)
SELECT N'L2-15', N'Sản phẩm có giá âm',
       N'Số sản phẩm=' + CAST(COUNT(*) AS NVARCHAR) + N'; xuất bản=' + CAST(SUM(CASE WHEN s.StatusId = 3 THEN 1 ELSE 0 END) AS NVARCHAR)
       + N'; ví dụ ID: ' + ISNULL(STRING_AGG(CASE WHEN s.StatusId = 3 THEN CAST(s.ID AS NVARCHAR) END, N','), N'-'),
       CASE WHEN COUNT(*) = 0 THEN 'DAT' ELSE 'CHUA DAT' END
FROM dbo.SanPhamCNTB s
WHERE s.OriginalPrice < 0 OR s.SellPrice < 0
   OR LTRIM(REPLACE(REPLACE(s.GiaBanDuKien, '<p>', ''), '&nbsp;', '')) LIKE '-%';

/* ---------- L2-09: sản phẩm VHT2 – lĩnh vực, chỉ mục tìm kiếm ---------- */
INSERT #KQ (Muc, DoiTuong, ChiTiet, KetLuan)
SELECT N'L2-09 Lĩnh vực', N'ID ' + CAST(s.ID AS NVARCHAR) + N' ' + LEFT(s.Name, 60),
       N'StatusId=' + ISNULL(CAST(s.StatusId AS NVARCHAR), N'NULL') + N'; CategoryId (chuỗi)=' + ISNULL(s.CategoryId, N'NULL')
       + N'; bảng SanPhamCNTBCategory=' + ISNULL((SELECT STRING_AGG(CAST(c.CatId AS NVARCHAR) + N':' + ISNULL(cat.Title, N'?'), N', ')
                                                  FROM dbo.SanPhamCNTBCategory c LEFT JOIN dbo.Category cat ON cat.CatId = c.CatId
                                                 WHERE c.SanPhamCNTBId = s.ID), N'trống'),
       CASE WHEN s.StatusId = 3 AND EXISTS (SELECT 1 FROM dbo.SanPhamCNTBCategory c WHERE c.SanPhamCNTBId = s.ID)
            THEN 'DAT' ELSE 'CHUA DAT' END
FROM dbo.SanPhamCNTB s
WHERE (s.Name LIKE N'%VHT2%' OR s.Code LIKE N'%VHT2%') AND s.LanguageId = 1;

INSERT #KQ (Muc, DoiTuong, ChiTiet, KetLuan)
SELECT N'L2-09 Tìm theo mô tả', N'ID ' + CAST(s.ID AS NVARCHAR) + N' – "' + k.CumTu + N'"',
       N'Nằm ở: ' + CASE WHEN s.MoTaNgan LIKE N'%' + k.CumTu + N'%' THEN N'MoTaNgan ' ELSE N'' END
                  + CASE WHEN s.Thongso LIKE N'%' + k.CumTu + N'%' THEN N'Thongso ' ELSE N'' END
                  + CASE WHEN s.UuDiem LIKE N'%' + k.CumTu + N'%' THEN N'UuDiem ' ELSE N'' END
                  + CASE WHEN CAST(s.MoTa AS NVARCHAR(MAX)) LIKE N'%' + k.CumTu + N'%' THEN N'MoTa ' ELSE N'' END
       + N'; có trong chỉ mục=' + CASE WHEN ix.Co = 1 THEN N'có' ELSE N'không' END,
       CASE WHEN ix.Co = 1 THEN 'DAT' ELSE 'CHUA DAT' END
FROM dbo.SanPhamCNTB s
CROSS JOIN (VALUES (N'tấm thu nhiệt'), (N'lọc sinh học')) k(CumTu)
OUTER APPLY (SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.SearchIndexContents c WHERE c.RefId = s.ID
                          AND c.URL LIKE '%-' + CAST(s.ID AS VARCHAR(20)) AND c.Contents LIKE N'%' + k.CumTu + N'%')
                         THEN 1 ELSE 0 END AS Co) ix
WHERE (s.Name LIKE N'%VHT2%' OR s.Code LIKE N'%VHT2%') AND s.LanguageId = 1
  AND (s.MoTaNgan LIKE N'%' + k.CumTu + N'%' OR s.Thongso LIKE N'%' + k.CumTu + N'%'
       OR s.UuDiem LIKE N'%' + k.CumTu + N'%' OR CAST(s.MoTa AS NVARCHAR(MAX)) LIKE N'%' + k.CumTu + N'%');

/* ---------- L2-09: Full-Text và thủ tục reindex ---------- */
INSERT #KQ (Muc, DoiTuong, ChiTiet, KetLuan)
SELECT N'L2-09 Full-Text', N'SearchIndexContents',
       N'Cài FT=' + ISNULL(CAST(FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') AS NVARCHAR), N'NULL')
       + N'; chỉ mục FT=' + CASE WHEN fi.object_id IS NULL THEN N'không có' WHEN fi.is_enabled = 1 THEN N'có, đang bật' ELSE N'có, đang tắt' END
       + N'; cột=' + ISNULL((SELECT STRING_AGG(COL_NAME(ic.object_id, ic.column_id), N', ')
                             FROM sys.fulltext_index_columns ic WHERE ic.object_id = OBJECT_ID('dbo.SearchIndexContents')), N'-'),
       CASE WHEN FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 1 AND fi.is_enabled = 1 THEN 'DAT' ELSE 'CHUA DAT' END
FROM (SELECT 1 AS x) d
LEFT JOIN sys.fulltext_indexes fi ON fi.object_id = OBJECT_ID('dbo.SearchIndexContents');

INSERT #KQ (Muc, DoiTuong, ChiTiet, KetLuan)
SELECT N'L2-09 Reindex', N'uspReIndexSearchContents',
       CASE WHEN OBJECT_ID('dbo.uspReIndexSearchContents') IS NULL THEN N'Không có thủ tục'
            WHEN OBJECT_DEFINITION(OBJECT_ID('dbo.uspReIndexSearchContents')) LIKE N'%S.MoTa AS NVARCHAR%' THEN N'Có ghép MoTa'
            ELSE N'Không ghép MoTa' END
       + N'; sửa lần cuối ' + ISNULL((SELECT CONVERT(NVARCHAR(19), modify_date, 120) FROM sys.objects
                                       WHERE object_id = OBJECT_ID('dbo.uspReIndexSearchContents')), N'-'),
       CASE WHEN OBJECT_DEFINITION(OBJECT_ID('dbo.uspReIndexSearchContents')) LIKE N'%S.MoTa AS NVARCHAR%' THEN 'DAT' ELSE 'CHUA DAT' END;

/* ---------- L2-09: gợi ý ở tin nhu cầu #109 ---------- */
DECLARE @Lv NVARCHAR(500) = (SELECT LinhVucId FROM dbo.ContentsYeuCau WHERE Id = 109);
INSERT #KQ (Muc, DoiTuong, ChiTiet, KetLuan)
SELECT N'L2-09 Gợi ý #109', N'Tin nhu cầu 109 (lĩnh vực ' + ISNULL(@Lv, N'NULL') + N')',
       N'4 gợi ý đầu theo code: ' + ISNULL((SELECT STRING_AGG(CAST(g.ID AS NVARCHAR) + N' ' + LEFT(g.Name, 30), N' | ')
          FROM (SELECT DISTINCT TOP 4 sp.ID, sp.Name, sp.PublishedDate
                  FROM dbo.SanPhamCNTB sp JOIN dbo.SanPhamCNTBCategory c ON c.SanPhamCNTBId = sp.ID
                 WHERE sp.StatusId = 3 AND sp.LanguageId = 1 AND sp.ProductType <> 4
                   AND c.CatId IN (SELECT TRY_CAST(LTRIM(value) AS INT) FROM STRING_SPLIT(REPLACE(ISNULL(@Lv, ''), ';', ','), ','))
                 ORDER BY sp.PublishedDate DESC) g), N'không có'),
       CASE WHEN EXISTS (SELECT 1 FROM dbo.SanPhamCNTB sp JOIN dbo.SanPhamCNTBCategory c ON c.SanPhamCNTBId = sp.ID
                          WHERE (sp.Name LIKE N'%VHT2%') AND sp.StatusId = 3
                            AND c.CatId IN (SELECT TRY_CAST(LTRIM(value) AS INT) FROM STRING_SPLIT(REPLACE(ISNULL(@Lv, ''), ';', ','), ',')))
            THEN 'DAT' ELSE 'CHUA DAT' END;

/* ---------- Kết quả ---------- */
SELECT N'Máy chủ ' + @@SERVERNAME + N' / ' + DB_NAME() + N' / ' + CONVERT(NVARCHAR(19), GETDATE(), 120) AS ThongTin,
       SUM(CASE WHEN KetLuan = 'DAT' THEN 1 ELSE 0 END) AS SoDat,
       SUM(CASE WHEN KetLuan <> 'DAT' THEN 1 ELSE 0 END) AS SoChuaDat
FROM #KQ;

SELECT Muc, DoiTuong, ChiTiet, KetLuan
FROM #KQ
WHERE @HienTatCa = 1 OR KetLuan <> 'DAT'
ORDER BY Stt;
