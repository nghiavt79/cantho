/* =====================================================================
   Kiểm tra trên máy chủ các nội dung ghi trong Thông báo kết quả khắc phục lỗi L2 (09/11/2026).
   CHỈ ĐỌC, không sửa dữ liệu. Mỗi khối trả một bảng, cột KetLuan = DAT / CHUA DAT.
   sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -W -s "|" -i kiem-tra-l2-2026-11-09.sql
   ===================================================================== */
SET NOCOUNT ON;

DECLARE @Seller01 INT = (SELECT UserId FROM dbo.Users WHERE UserName = 'vht_seller01');
DECLARE @Seller02 INT = (SELECT UserId FROM dbo.Users WHERE UserName = 'vht_seller02');
DECLARE @Expert01 INT = (SELECT UserId FROM dbo.Users WHERE UserName = 'vht_expert01');

/* L2-07a: hồ sơ nhà cung ứng 17924 của vht_seller01 đã kích hoạt */
SELECT N'L2-07a NCC 17924' AS Muc, n.CungUngId, n.FullName, n.UserId, n.IsActivated, n.StatusId,
       CASE WHEN n.IsActivated = 1 AND n.UserId = @Seller01 THEN 'DAT' ELSE 'CHUA DAT' END AS KetLuan
FROM dbo.NhaCungUng n WHERE n.CungUngId = 17924;

/* L2-07b: hồ sơ nhà cung ứng của vht_seller02 (báo cáo 03/11 ghi là chưa có) */
SELECT N'L2-07b NCC vht_seller02' AS Muc, @Seller02 AS UserId, COUNT(n.CungUngId) AS SoHoSo,
       SUM(CASE WHEN n.IsActivated = 1 THEN 1 ELSE 0 END) AS SoKichHoat
FROM dbo.NhaCungUng n WHERE n.UserId = @Seller02;

/* L2-07c: lời mời bên bán trên 02 hồ sơ thử */
SELECT N'L2-07c Loi moi' AS Muc, p.Id AS ProjectId, p.ProjectCode,
       (SELECT COUNT(*) FROM dbo.RFQRequests r WHERE r.ProjectId = p.Id) AS SoYeuCauBaoGia,
       (SELECT COUNT(*) FROM dbo.RFQInvitations i WHERE i.ProjectId = p.Id AND i.IsActive = 1) AS SoLoiMoi,
       (SELECT STRING_AGG(u.UserName + ' (' + CONVERT(varchar(16), i.InvitedDate, 120) + ')', ', ')
          FROM dbo.RFQInvitations i JOIN dbo.Users u ON u.UserId = i.SellerId
         WHERE i.ProjectId = p.Id AND i.IsActive = 1) AS BenBanDuocMoi,
       (SELECT MAX(l.ToStep) FROM dbo.WorkflowTransitionLogs l WHERE l.ProjectId = p.Id) AS BuocCaoNhat,
       CASE WHEN EXISTS (SELECT 1 FROM dbo.RFQInvitations i WHERE i.ProjectId = p.Id AND i.IsActive = 1)
            THEN 'DAT' ELSE 'CHUA DAT' END AS KetLuan
FROM dbo.Projects p
WHERE p.ProjectCode IN ('PJ-20261103134013', 'PJ-20261103134112');

/* L2-11: vht_expert01 có hồ sơ chuyên gia kích hoạt */
SELECT N'L2-11 vht_expert01' AS Muc, @Expert01 AS UserId, t.TuVanId, t.IsActivated, t.StatusId,
       CASE WHEN t.IsActivated = 1 THEN 'DAT' ELSE 'CHUA DAT' END AS KetLuan
FROM (SELECT @Expert01 AS UserId) x
LEFT JOIN dbo.NhaTuVan t ON t.UserId = x.UserId;

/* L2-04: nhật ký chuyển bước của 02 hồ sơ thử */
SELECT N'L2-04 Nhat ky chuyen buoc' AS Muc, p.ProjectCode, COUNT(l.Id) AS SoBanGhi, MAX(l.CreatedAt) AS LanCuoi,
       CASE WHEN COUNT(l.Id) > 0 THEN 'DAT' ELSE 'CHUA DAT' END AS KetLuan
FROM dbo.Projects p LEFT JOIN dbo.WorkflowTransitionLogs l ON l.ProjectId = p.Id
WHERE p.ProjectCode IN ('PJ-20261103134013', 'PJ-20261103134112')
GROUP BY p.ProjectCode;

/* L2-02, L2-03: sản phẩm "Quy trinh pentest" và tên có chuỗi mã thử còn hiển thị.
   Kiểm tra online 09/11/2026: /san-pham/chi-tiet/quy-trinh-pentest-44048 (CN-43687) VẪN hiện công khai. */
SELECT N'L2-02/03 Quy trinh pentest' AS Muc, s.ID, s.Code, s.Name, s.StatusId, s.PublishedDate, s.LanguageId,
       'CHUA DAT' AS KetLuan
FROM dbo.SanPhamCNTB s
WHERE s.Name LIKE N'%pentest%' OR s.Code = 'CN-43687';

SELECT N'L2-02/03 Ten/noi dung co ma thu' AS Muc, s.StatusId, COUNT(*) AS SoSanPham, MIN(s.ID) AS IdNhoNhat, MAX(s.ID) AS IdLonNhat
FROM dbo.SanPhamCNTB s
WHERE s.Name LIKE N'%<script%' OR s.Name LIKE N'%onerror%' OR s.Name LIKE N'%alert(%'
   OR s.MoTa LIKE N'%onerror=%' OR s.ThongSo LIKE N'%onerror=%' OR s.UuDiem LIKE N'%onerror=%'
GROUP BY s.StatusId;

/* Phụ lục: số liệu sau dọn (thông báo ghi còn lại 91 NCC, 15 hồ sơ, 4.361 sản phẩm) */
SELECT N'Phu luc' AS Muc,
       (SELECT COUNT(*) FROM dbo.NhaCungUng) AS NhaCungUng_PhaiLa91,
       (SELECT COUNT(*) FROM dbo.Projects) AS HoSo_PhaiLa15,
       (SELECT COUNT(*) FROM dbo.SanPhamCNTB) AS SanPham_PhaiLa4361;

/* L2-12: giờ máy chủ CSDL (so với giờ chuẩn khi chạy) */
SELECT N'L2-12 Gio may chu CSDL' AS Muc, SYSDATETIMEOFFSET() AS GioMayChu;
