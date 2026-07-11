/*
Adds real public contact info (found via web search) for the 3 identifiable real
companies among the 14 OCOP suppliers backfilled by backfill-ocop-suppliers.sql,
and fills in the required classification fields (LoaiHinhToChuc, LinhVucId,
ChucNangChinh) for all 14 so the NhaCungUngAdmin edit form doesn't fail validation.
Phone/Email/NguoiDaiDien are left blank for the smaller cơ sở/HTX/tổ hợp tác
entries — no verifiable public contact info was found for them, and inventing
values would be inaccurate. Safe to re-run (idempotent updates by CungUngId).
*/

DECLARE @Now DATETIME = GETDATE();

-- Real companies with verified public info
UPDATE NhaCungUng SET
    DiaChi = N'Số 330B/10, khu vực 4, phường An Bình, quận Ninh Kiều, TP. Cần Thơ',
    Website = 'https://cpfoods.vn',
    ChucNangChinh = N'Chuyên sản xuất snack da cá chiên giòn (vị nguyên bản, vị trứng muối), đạt chứng nhận OCOP 4 sao cấp thành phố.',
    Modified = @Now
WHERE FullName = N'Công ty CP Thực phẩm Vidaca';

UPDATE NhaCungUng SET
    DiaChi = N'Số 65-67, Đường B30, KDC 91B, phường An Khánh, quận Ninh Kiều, TP. Cần Thơ',
    Phone = '0937739030',
    Email = 'pydstham@gmail.com',
    Website = 'https://duoclieumientay.com',
    ChucNangChinh = N'Chuyên sản xuất trà thảo dược hòa tan (đinh lăng, tía tô) từ dược liệu miền Tây, đạt chứng nhận OCOP 4 sao.',
    Modified = @Now
WHERE FullName = N'Công ty TNHH MTV Hygie & Panacee';

UPDATE NhaCungUng SET
    Phone = '0918292298',
    Website = 'https://sumofood.vn',
    ChucNangChinh = N'Chuyên sản xuất trà mãng cầu xiêm Long Giang, liên kết với nông dân Hậu Giang và Vĩnh Long, đạt chứng nhận OCOP 4 sao (88/100 điểm), tiềm năng đạt 5 sao.',
    Modified = @Now
WHERE FullName = N'Công ty TNHH SumoFood Việt Nam';

-- All 14: fill required classification fields used by the admin form
-- LinhVucId 12 = "Công nghệ thực phẩm" (Category, ParentId=1)
UPDATE NhaCungUng SET
    LoaiHinhToChuc = ISNULL(LoaiHinhToChuc, 'DNSX'),
    LinhVucId = ISNULL(LinhVucId, '12'),
    ChucNangChinh = ISNULL(ChucNangChinh, N'Đơn vị sản xuất sản phẩm OCOP địa phương tại Cần Thơ.'),
    Modified = @Now
WHERE CungUngId BETWEEN 8873 AND 8886;

GO
