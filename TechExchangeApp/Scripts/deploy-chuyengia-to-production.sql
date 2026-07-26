/*
Production deployment script for the "Chuyen gia" data fix (see
fix-chuyengia-3-wrong-and-avatars.sql for the version already run against the
local dev DB "TechExchangeNew"). This version is safe to run on a different
server/database because:

  - It matches rows by Email instead of TuVanId (IDENTITY values on production
    are very likely different from dev's 4122/4124/4132/etc).
  - The image domain is a variable you must set below instead of hardcoded
    localhost.

BEFORE RUNNING ON PRODUCTION:

  1. Make sure the 12-expert CV import has already been applied to this
     database (TechExchangeApp/Scripts/seed-chuyengia-sample-experts.sql).
     If it has not, run that script FIRST — this script only UPDATEs
     existing rows by email, it does not INSERT new experts.

  2. Set @ImgBase below to the real production image domain (same value as
     AppSettings:ImageDomain in production appsettings, e.g.
     N'https://<your-production-domain>/Uploads/2026/07/14/').

  3. Copy the 55 image files from
     TechExchangeApp/wwwroot/uploads/2026/07/14/ (11 originals +
     44 resized 254-170/108-84/460-275/400-220 variants) to the SAME
     relative path under production's wwwroot, i.e.
     wwwroot/uploads/2026/07/14/ — filenames must match exactly, they are
     referenced by name in the UPDATE statements below.

What this script does once run:
  - Overwrites 3 experts (Nguyen Van Muoi, Nhan Minh Tri, Nguyen Chi Ngon)
    that may still hold old/placeholder data (wrong DOB, shared phone/address,
    wrong academic title, empty CV sections) with the correct data extracted
    from their submitted CVs.
  - Attaches the correct avatar to all 11 experts that have a photo.
  - Unpublishes Nguyen Duc Vuong (StatusId = 1, draft) per customer request —
    he has no photo yet; his CV data is left untouched so he can be
    republished later without re-entering anything.

Safe to run more than once; every statement is keyed by Email or is
idempotent.
*/

DECLARE @Now DATETIME = GETDATE();
DECLARE @ImgBase NVARCHAR(200) = N'https://SET-THIS-TO-PRODUCTION-DOMAIN/Uploads/2026/07/14/';

IF @ImgBase = N'https://SET-THIS-TO-PRODUCTION-DOMAIN/Uploads/2026/07/14/'
BEGIN
    RAISERROR(N'Edit @ImgBase at the top of this script to the real production image domain before running.', 16, 1);
    RETURN;
END

-- =====================================================================
-- 1. Fix the 3 experts that may still be showing stale/placeholder data
--    (matched by Email — safe regardless of TuVanId on this server)
-- =====================================================================

-- Nguyen Van Muoi
UPDATE NhaTuVan SET
    FullName = N'Nguyễn Văn Mười',
    DateOfBirth = N'10/7/1960',
    DiaChi = N'133C, Mậu Thân, phường An Hòa, quận Ninh Kiều, TP. Cần Thơ',
    Phone = N'0913185179',
    HocHam = N'Giáo sư',
    CoQuan = N'Trường Đại học Cần Thơ',
    ChucVu = N'Giảng viên cao cấp',
    LinhVucId = N'12',
    DichVu = N'Tư vấn công nghệ bảo quản và chế biến thực phẩm, xây dựng quy trình sản xuất cho doanh nghiệp nông sản - thực phẩm.',
    KetQuaNghienCuu = N'Chủ nhiệm/tham gia 25 đề tài nghiên cứu (1997-2024), tác giả 191 bài báo khoa học (145 trong nước, 46 quốc tế WoS/Scopus) và 8 đầu sách/giáo trình.',
    QuaTrinhDaoTao = N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Trường Đại học Cần Thơ</td><td>Bảo quản và Chế biến thực phẩm</td><td>Việt Nam</td><td>1982</td></tr>
<tr><td>Tiến sĩ</td><td>Viện Hàn lâm Quốc gia Công nghệ Sinh học Ứng dụng Moscow</td><td>Công nghệ thực phẩm</td><td>CHLB Nga</td><td>1993</td></tr></table>',
    QuaTrinhCongTac = N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Nơi công tác</th><th>Công việc</th></tr>
<tr><td>1982-2007</td><td>Trường Đại học Cần Thơ</td><td>Cán bộ giảng dạy, Trưởng bộ môn</td></tr>
<tr><td>2007-2020</td><td>Trường Đại học Cần Thơ</td><td>Phó Giáo sư, giảng viên</td></tr>
<tr><td>2012-2020</td><td>Trường Đại học Cần Thơ</td><td>Giám đốc Trung tâm Dịch vụ Khoa học Nông nghiệp</td></tr>
<tr><td>2020-nay</td><td>Trường Đại học Cần Thơ</td><td>Giáo sư, giảng viên cao cấp</td></tr></table>',
    CongBoKhoaHoc = N'<p>GS. Nguyễn Văn Mười đã công bố 191 bài báo khoa học (145 trong nước, 46 quốc tế thuộc WoS/Scopus), trong đó 84 bài trong 5 năm gần nhất (38 bài WoS/Scopus), cùng 8 đầu sách/giáo trình về công nghệ thực phẩm (2006-2024, NXB Giáo dục và NXB Đại học Cần Thơ).</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Nội dung</th><th>Số lượng</th><th>Giai đoạn</th></tr>
<tr><td>Sách/giáo trình công nghệ bảo quản, chế biến thực phẩm</td><td>8</td><td>2006-2024</td></tr>
<tr><td>Bài báo khoa học trong nước và quốc tế (WoS/Scopus)</td><td>191</td><td>1982-2024</td></tr></table>',
    SangChe = N'',
    DuAnNghienCuu = N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Số lượng</th><th>Cấp</th><th>Thời gian</th></tr>
<tr><td>25 đề tài/dự án nghiên cứu</td><td>Cấp trường, cấp Bộ, cấp tỉnh, quốc tế (VLIR - Bỉ)</td><td>1997-2024</td></tr></table>',
    HinhDaiDien = @ImgBase + N'GS-TS-NGUYEN-VAN-MUOI.jpg',
    Modified = @Now
WHERE Email = N'nvmuoi@ctu.edu.vn';

-- Nhan Minh Tri
UPDATE NhaTuVan SET
    FullName = N'Nhan Minh Trí',
    DateOfBirth = N'01/01/1973',
    DiaChi = N'Khu II Trường Đại học Cần Thơ',
    Phone = N'0908808207',
    HocHam = N'Phó giáo sư',
    CoQuan = N'Bộ môn Công nghệ sau thu hoạch, Viện CNSH và Thực phẩm, Trường Đại học Cần Thơ',
    ChucVu = N'Trưởng phòng thí nghiệm Hóa học Thực phẩm tiên tiến',
    LinhVucId = N'12',
    DichVu = N'Tư vấn công nghệ sau thu hoạch, hóa học thực phẩm, hợp tác nghiên cứu quốc tế (Bỉ, Úc).',
    KetQuaNghienCuu = N'Chủ nhiệm/tham gia 16 đề tài (2007-2025), tác giả hơn 34 bài báo khoa học (2004-2024) và 4 đầu sách/giáo trình.',
    QuaTrinhDaoTao = N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Trường Đại học Cần Thơ</td><td>Công nghệ thực phẩm</td><td>Việt Nam</td><td>1996</td></tr>
<tr><td>Thạc sĩ</td><td>KU.Leuven</td><td>Kỹ thuật sau thu hoạch và bảo quản thực phẩm</td><td>Bỉ</td><td>2001</td></tr>
<tr><td>Tiến sĩ</td><td>Đại học Sydney</td><td>Công nghệ thực phẩm</td><td>Úc</td><td>2013</td></tr></table>',
    QuaTrinhCongTac = N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Nơi công tác</th><th>Công việc</th></tr>
<tr><td>1996-2022</td><td>Trường Đại học Cần Thơ</td><td>Giảng viên</td></tr>
<tr><td>Nhiều đợt</td><td>KU.Leuven, Đại học Sydney, Đại học Ghent</td><td>Tập huấn, nghiên cứu ngắn hạn</td></tr></table>',
    CongBoKhoaHoc = N'<p>PGS. Nhan Minh Trí đã công bố hơn 34 bài báo khoa học (2004-2024, nhiều bài Scopus Q2-Q4), cùng 4 đầu sách/giáo trình (2000-2017) về công nghệ sau thu hoạch và hóa học thực phẩm.</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Nội dung</th><th>Số lượng</th><th>Giai đoạn</th></tr>
<tr><td>Sách/giáo trình</td><td>4</td><td>2000-2017</td></tr>
<tr><td>Bài báo khoa học (nhiều bài Scopus Q2-Q4)</td><td>34+</td><td>2004-2024</td></tr></table>',
    SangChe = N'',
    DuAnNghienCuu = N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Số lượng</th><th>Cấp</th><th>Thời gian</th></tr>
<tr><td>16 đề tài</td><td>Cấp trường, cấp tỉnh, quốc tế (VLIR)</td><td>2007-2025</td></tr></table>',
    HinhDaiDien = @ImgBase + N'PGS-TS-NHAN-MINH-TRI.jpg',
    Modified = @Now
WHERE Email = N'nhanmtri@ctu.edu.vn';

-- Nguyen Chi Ngon
UPDATE NhaTuVan SET
    FullName = N'Nguyễn Chí Ngôn',
    DateOfBirth = N'1972',
    DiaChi = N'X3, Đường số 13, Khu Công ty 8, phường Hưng Thạnh, quận Cái Răng, thành phố Cần Thơ',
    Phone = N'0918538224',
    HocHam = N'Phó giáo sư',
    CoQuan = N'Trường Đại học Cần Thơ',
    ChucVu = N'Phó chủ tịch Hội đồng trường',
    LinhVucId = N'4;1079',
    DichVu = N'Tư vấn tự động hóa, điều khiển thông minh, IoT, thị giác máy, ứng dụng AI trong nông nghiệp và công nghiệp.',
    KetQuaNghienCuu = N'Chủ nhiệm/tham gia 3 đề tài/dự án (2015-2021), tác giả 15 bài báo tạp chí quốc tế, 5 báo cáo hội nghị quốc tế và 8 báo cáo hội nghị quốc gia (2020-2024), 2 bằng độc quyền sáng chế.',
    QuaTrinhDaoTao = N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Đại học Cần Thơ</td><td>Điện tử</td><td>Việt Nam</td><td>1996</td></tr>
<tr><td>Thạc sĩ</td><td>Đại học Bách khoa TP.HCM</td><td>Điện tử vô tuyến</td><td>Việt Nam</td><td>2001</td></tr>
<tr><td>Tiến sĩ</td><td>Đại học Rostock</td><td>Tự động hóa</td><td>CHLB Đức</td><td>2007</td></tr></table>',
    QuaTrinhCongTac = N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Vị trí</th><th>Cơ quan</th></tr>
<tr><td>1996-2007</td><td>Giảng viên</td><td>Đại học Cần Thơ</td></tr>
<tr><td>2007-2008</td><td>Trưởng bộ môn</td><td>Đại học Cần Thơ</td></tr>
<tr><td>2008-2012</td><td>Phó trưởng khoa</td><td>Đại học Cần Thơ</td></tr>
<tr><td>2012-2021</td><td>Trưởng khoa</td><td>Đại học Cần Thơ</td></tr>
<tr><td>2021-nay</td><td>Phó chủ tịch Hội đồng trường</td><td>Đại học Cần Thơ</td></tr></table>',
    CongBoKhoaHoc = N'<p>PGS.TS Nguyễn Chí Ngôn đã công bố 15 bài báo tạp chí quốc tế (chủ yếu Scopus/SCIE Q2-Q4), 5 báo cáo hội nghị quốc tế và 8 báo cáo hội nghị quốc gia trong giai đoạn 2020-2024, tập trung vào tự động hóa, robot, cảm biến và ứng dụng AI.</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bài báo tiêu biểu</th><th>Tạp chí</th><th>Năm</th></tr>
<tr><td>Early detection of slight bruises in apples by cost-efficient near-infrared imaging</td><td>Int. J. of Electrical and Computer Eng. (Scopus/Q2)</td><td>2022</td></tr>
<tr><td>In situ measurement of fish color based on machine vision: A case study of measuring a clownfish''s color</td><td>Measurement</td><td>2022</td></tr>
<tr><td>Localized automation solutions in response to the first wave of COVID-19: a story from Vietnam</td><td>Int. J. of Pervasive Computing and Communications (Scopus/Q2)</td><td>2022</td></tr>
<tr><td>Predictive Modeling of Landslide Susceptibility in Soft Soil Canal Regions</td><td>Int. J. of Advanced Computer Science and Applications (Scopus/Q3)</td><td>2023</td></tr>
<tr><td>A multi-microcontroller-based hardware for deploying Tiny machine learning model</td><td>Int. J. of Electrical and Computer Engineering (Scopus/Q3)</td><td>2023</td></tr>
<tr><td>Adaptive PID sliding mode control based on new Quasi-sliding mode and radial basis function neural network for Omnidirectional mobile robot</td><td>AIMS Electronics and Electrical Engineering</td><td>2023</td></tr>
<tr><td>Development of a soil electrical conductivity measurement system in paddy fields</td><td>Int. J. of Advances in Applied Sciences (Scopus)</td><td>2024</td></tr>
<tr><td colspan="3">(và các bài báo/báo cáo hội nghị khác, tổng cộng 28 công trình 5 năm gần đây)</td></tr></table>',
    SangChe = N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Tên sáng chế</th><th>Cơ quan cấp</th><th>Năm</th></tr>
<tr><td>Hệ thống truyền dữ liệu trong nước phục vụ quan trắc môi trường</td><td>Cục Sở hữu trí tuệ</td><td>2023</td></tr>
<tr><td>Phương pháp và hệ thống xác định màu sắc động vật thủy sản và vật thể dựa trên hình ảnh</td><td>Cục Sở hữu trí tuệ</td><td>2024</td></tr></table>',
    DuAnNghienCuu = N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Đề tài/dự án</th><th>Thời gian</th><th>Kết quả</th></tr>
<tr><td>Nghiên cứu giải pháp thúc đẩy hoạt động chuyển giao, ứng dụng, đổi mới công nghệ tại thành phố Cần Thơ</td><td>2018-2021</td><td>Đã nghiệm thu, xếp loại tốt</td></tr>
<tr><td>Xây dựng hệ thống trợ giúp khuyến nông trực tuyến tại Đồng bằng sông Cửu Long</td><td>2015-2018</td><td>Đã nghiệm thu, xếp loại tốt</td></tr>
<tr><td>ECO-RED (Erasmus+)</td><td>2015-2018</td><td>Đã nghiệm thu, xếp loại tốt</td></tr></table>',
    HinhDaiDien = @ImgBase + N'PGs-Ts-Nguyen-Chi-Ngon.jpg',
    Modified = @Now
WHERE Email = N'ncngon@ctu.edu.vn';

-- =====================================================================
-- 2. Attach avatars for the other 8 experts whose CV data should already
--    be correct (matched by Email; UPDATE is a no-op if the email doesn't
--    exist yet on this server — run seed-chuyengia-sample-experts.sql first
--    if that's the case)
-- =====================================================================
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'GS-TS-NGUYEN-THI-LANG.jpg',        Modified = @Now WHERE Email = N'ntlang.prof@gmail.com';
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'PGs-Ts-Nguyen-Huu-Hiep.jpg',       Modified = @Now WHERE Email = N'hiepngu@gmail.com';
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'Ts-Truong-Minh-Nhat-Quang.jpg',    Modified = @Now WHERE Email = N'tmnquang@ctuet.edu.vn';
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'PGs-Ts-Nguyen-Tan-Tien.jpeg',      Modified = @Now WHERE Email = N'nttien@hcmut.edu.vn';
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'Ts-Thai-Phuong-Vu.jpg',            Modified = @Now WHERE Email = N'tpvu@hcmunre.edu.vn';
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'Nguyen-Hoai-Anh.jpg',              Modified = @Now WHERE Email = N'nghoaianhart@gmail.com';
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'TS-PHAM-HONG-QUACH.jpg',           Modified = @Now WHERE Email = N'phquach@most.gov.vn';
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'PGS-TS-NGUYEN-DUY-dAT.jpg',        Modified = @Now WHERE Email = N'duydatvcu@gmail.com';

-- Nguyen Duc Vuong (nguyenducvuong@iuh.edu.vn) intentionally NOT given a
-- HinhDaiDien here: no portrait exists yet in CSDL Mau\ChuyenGiaHinhAnh.
-- If the customer supplies a photo later and wants him republished, run:
--   UPDATE NhaTuVan SET HinhDaiDien = N'<ImgBase>/<file>', StatusId = 3, Modified = GETDATE()
--   WHERE Email = N'nguyenducvuong@iuh.edu.vn';

-- =====================================================================
-- 3. Customer asked to pull Nguyen Duc Vuong's profile (2026-07-14) — for now
--    just unpublish (StatusId 1 = draft) rather than delete, so the data is
--    not lost and he can be republished later without re-entering the CV.
-- =====================================================================
UPDATE NhaTuVan SET StatusId = 1, Modified = @Now WHERE Email = N'nguyenducvuong@iuh.edu.vn';

GO
