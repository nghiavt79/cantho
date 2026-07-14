/*
Fixes the 2 issues found when auditing "Chuyen gia" data against the original CV
source files in CSDL Mau\ChuyenGia and CSDL Mau\ChuyenGiaHinhAnh:

1. Three NhaTuVan rows (TuVanId 4122, 4124, 4132) predate the 2026-07-11 CV import and
   were never overwritten because the import script only inserts when the email does not
   already exist. They currently show generic/duplicate placeholder data (same phone
   number and address for all three, wrong DOB, wrong academic title). This script
   overwrites them with the correct data extracted from the submitted CVs, matching what
   seed-chuyengia-sample-experts.sql already inserted for the other 9 experts.

2. None of the 12 NhaTuVan rows had HinhDaiDien (avatar) set. The 11 available portraits
   from CSDL Mau\ChuyenGiaHinhAnh were copied + resized (254-170/108-84/460-275/400-220
   variants) into wwwroot/uploads/2026/07/14/ using the same convention as
   Areas/Cms/Controllers/UploadController.cs, and are linked here. Nguyen Duc Vuong has no
   source photo yet, so his HinhDaiDien is left untouched.

Safe to run once; the WHERE clauses target specific TuVanId/Email so re-running just
re-applies the same values.
*/

DECLARE @Now DATETIME = GETDATE();
DECLARE @ImgBase NVARCHAR(200) = N'https://localhost:7232/Uploads/2026/07/14/';

-- =====================================================================
-- 1. Fix the 3 rows showing stale/wrong data
-- =====================================================================

-- Nguyen Van Muoi (TuVanId 4124)
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
WHERE TuVanId = 4124;

-- Nhan Minh Tri (TuVanId 4122)
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
WHERE TuVanId = 4122;

-- Nguyen Chi Ngon (TuVanId 4132)
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
WHERE TuVanId = 4132;

-- =====================================================================
-- 2. Attach avatars for the other 9 experts that were already correct
--    (matched by Email, same as the original seed script)
-- =====================================================================
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'GS-TS-NGUYEN-THI-LANG.jpg',        Modified = @Now WHERE Email = N'ntlang.prof@gmail.com';
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'PGs-Ts-Nguyen-Huu-Hiep.jpg',       Modified = @Now WHERE Email = N'hiepngu@gmail.com';
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'Ts-Truong-Minh-Nhat-Quang.jpg',    Modified = @Now WHERE Email = N'tmnquang@ctuet.edu.vn';
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'PGs-Ts-Nguyen-Tan-Tien.jpeg',      Modified = @Now WHERE Email = N'nttien@hcmut.edu.vn';
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'Ts-Thai-Phuong-Vu.jpg',            Modified = @Now WHERE Email = N'tpvu@hcmunre.edu.vn';
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'Nguyen-Hoai-Anh.jpg',              Modified = @Now WHERE Email = N'nghoaianhart@gmail.com';
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'TS-PHAM-HONG-QUACH.jpg',           Modified = @Now WHERE Email = N'phquach@most.gov.vn';
UPDATE NhaTuVan SET HinhDaiDien = @ImgBase + N'PGS-TS-NGUYEN-DUY-dAT.jpg',        Modified = @Now WHERE Email = N'duydatvcu@gmail.com';

-- Nguyen Duc Vuong (nguyenducvuong@iuh.edu.vn) intentionally NOT updated here:
-- no portrait exists yet in CSDL Mau\ChuyenGiaHinhAnh. If the customer supplies a
-- photo and wants him republished later, run:
--   UPDATE NhaTuVan SET HinhDaiDien = N'https://localhost:7232/Uploads/2026/07/14/<file>', StatusId = 3, Modified = GETDATE()
--   WHERE Email = N'nguyenducvuong@iuh.edu.vn';

-- =====================================================================
-- 3. Customer asked to pull Nguyen Duc Vuong's profile (2026-07-14) — for now
--    just unpublish (StatusId 1 = draft) rather than delete, so the data is not
--    lost and he can be republished later without re-entering the CV.
-- =====================================================================
UPDATE NhaTuVan SET StatusId = 1, Modified = @Now WHERE Email = N'nguyenducvuong@iuh.edu.vn';

GO
