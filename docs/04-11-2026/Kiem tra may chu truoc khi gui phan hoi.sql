-- CHI DOC, khong sua du lieu. Chay tren CSDL may chu mekongx.vn truoc khi gui van ban phan hoi.
SET NOCOUNT ON;

-- L2-07: loi moi ban ban cua 02 ho so thu (0 dong = chua moi)
SELECT ProjectId, SellerId, StatusId, IsActive, InvitedDate FROM RFQInvitations WHERE ProjectId IN (9528, 9529);

-- L2-07: nguoi mua chi moi duoc khi ho so nha cung ung co StatusId = 3, IsActivated = 1 va dung UserId
SELECT CungUngId, FullName, UserId, StatusId, IsActivated, LanguageId FROM NhaCungUng
 WHERE CungUngId = 17924 OR UserId IN (SELECT UserId FROM Users WHERE UserName IN ('vht_seller01', 'vht_seller02'));
SELECT ID, Code, NCUId FROM SanPhamCNTB WHERE ID IN (44050, 44051);

-- L2-04: co dong = du lieu da ghi, chi man hinh tra cuu loi
SELECT ProjectId, FromStep, ToStep, Action, ActorUserId, CreatedAt FROM WorkflowTransitionLogs
 WHERE ProjectId IN (9528, 9529) ORDER BY Id;

-- L2-11: vht_expert01 da co ho so chuyen gia kich hoat chua
SELECT n.TuVanId, n.UserId, n.IsActivated, n.StatusId FROM NhaTuVan n
  JOIN Users u ON u.UserId = n.UserId WHERE u.UserName = 'vht_expert01';

-- L2-06: ho so, san pham cua tai khoan vht_* trong van hanh thu lan 1
SELECT p.Id, p.ProjectCode, u.UserName, p.CreatedDate FROM Projects p
  JOIN Users u ON u.UserId = p.CreatedBy
 WHERE u.UserName LIKE 'vht[_]%' AND p.CreatedDate >= '2026-10-27' AND p.CreatedDate < '2026-10-29';
SELECT ID, Code, StatusId, Creator, Created FROM SanPhamCNTB WHERE Creator LIKE 'vht[_]%' ORDER BY ID;

-- L2-09: 0 = san pham thu chua vao du lieu tra cuu
SELECT COUNT(*) AS SoDongTraCuuVHT2 FROM SearchIndexContents WHERE RefId IN (44050, 44051);
