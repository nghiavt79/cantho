/*
Seeds 12 real "Ly lich khoa hoc" (scientific CV) experts submitted by the customer into the
"Chuyen gia" (Expert) directory (NhaTuVan table), for the public /chuyen-gia page.

IMPORTANT: All rows are inserted with StatusId = 1 (draft / cho duyet). This is REAL personal
and professional data of named individuals and must NOT go live without a human reviewer
approving each record first.

A few CVs were missing data for NOT-NULL columns (DateOfBirth, DiaChi). Where that happened,
an empty string '' was used as a placeholder instead of inventing a value - see the report
accompanying this script for the exact list of who needs follow-up.

Idempotent: guarded by Email (unique per person) so re-running the script does not duplicate
rows. Follows the same temp-table + WHERE NOT EXISTS pattern as seed-ocop-sample-products.sql.
*/

DECLARE @Now DATETIME = GETDATE();

CREATE TABLE #ChuyenGiaSeed (
    Email NVARCHAR(255),
    FullName NVARCHAR(255),
    QueryString NVARCHAR(255),
    DateOfBirth NVARCHAR(50),
    DiaChi NVARCHAR(500),
    Phone NVARCHAR(50),
    HocHam NVARCHAR(100),
    CoQuan NVARCHAR(500),
    ChucVu NVARCHAR(255),
    LinhVucId NVARCHAR(50),
    DichVu NVARCHAR(1000),
    KetQuaNghienCuu NVARCHAR(1000),
    QuaTrinhDaoTao NVARCHAR(MAX),
    QuaTrinhCongTac NVARCHAR(MAX),
    CongBoKhoaHoc NVARCHAR(MAX),
    SangChe NVARCHAR(MAX),
    DuAnNghienCuu NVARCHAR(MAX)
);

-- 1. Nguyen Van Muoi
INSERT INTO #ChuyenGiaSeed (Email, FullName, QueryString, DateOfBirth, DiaChi, Phone, HocHam, CoQuan, ChucVu, LinhVucId, DichVu, KetQuaNghienCuu, QuaTrinhDaoTao, QuaTrinhCongTac, CongBoKhoaHoc, SangChe, DuAnNghienCuu)
VALUES (
N'nvmuoi@ctu.edu.vn', N'Nguyễn Văn Mười', 'nguyen-van-muoi', N'10/7/1960',
N'133C, Mậu Thân, phường An Hòa, quận Ninh Kiều, TP. Cần Thơ', N'0913185179',
N'Giáo sư', N'Trường Đại học Cần Thơ', N'Giảng viên cao cấp', N'12',
N'Tư vấn công nghệ bảo quản và chế biến thực phẩm, xây dựng quy trình sản xuất cho doanh nghiệp nông sản - thực phẩm.',
N'Chủ nhiệm/tham gia 25 đề tài nghiên cứu (1997-2024), tác giả 191 bài báo khoa học (145 trong nước, 46 quốc tế WoS/Scopus) và 8 đầu sách/giáo trình.',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Trường Đại học Cần Thơ</td><td>Bảo quản và Chế biến thực phẩm</td><td>Việt Nam</td><td>1982</td></tr>
<tr><td>Tiến sĩ</td><td>Viện Hàn lâm Quốc gia Công nghệ Sinh học Ứng dụng Moscow</td><td>Công nghệ thực phẩm</td><td>CHLB Nga</td><td>1993</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Nơi công tác</th><th>Công việc</th></tr>
<tr><td>1982-2007</td><td>Trường Đại học Cần Thơ</td><td>Cán bộ giảng dạy, Trưởng bộ môn</td></tr>
<tr><td>2007-2020</td><td>Trường Đại học Cần Thơ</td><td>Phó Giáo sư, giảng viên</td></tr>
<tr><td>2012-2020</td><td>Trường Đại học Cần Thơ</td><td>Giám đốc Trung tâm Dịch vụ Khoa học Nông nghiệp</td></tr>
<tr><td>2020-nay</td><td>Trường Đại học Cần Thơ</td><td>Giáo sư, giảng viên cao cấp</td></tr></table>',
N'<p>GS. Nguyễn Văn Mười đã công bố 191 bài báo khoa học (145 trong nước, 46 quốc tế thuộc WoS/Scopus), trong đó 84 bài trong 5 năm gần nhất (38 bài WoS/Scopus), cùng 8 đầu sách/giáo trình về công nghệ thực phẩm (2006-2024, NXB Giáo dục và NXB Đại học Cần Thơ).</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Nội dung</th><th>Số lượng</th><th>Giai đoạn</th></tr>
<tr><td>Sách/giáo trình công nghệ bảo quản, chế biến thực phẩm</td><td>8</td><td>2006-2024</td></tr>
<tr><td>Bài báo khoa học trong nước và quốc tế (WoS/Scopus)</td><td>191</td><td>1982-2024</td></tr></table>',
N'',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Số lượng</th><th>Cấp</th><th>Thời gian</th></tr>
<tr><td>25 đề tài/dự án nghiên cứu</td><td>Cấp trường, cấp Bộ, cấp tỉnh, quốc tế (VLIR - Bỉ)</td><td>1997-2024</td></tr></table>'
);

-- 2. Nhan Minh Tri
INSERT INTO #ChuyenGiaSeed (Email, FullName, QueryString, DateOfBirth, DiaChi, Phone, HocHam, CoQuan, ChucVu, LinhVucId, DichVu, KetQuaNghienCuu, QuaTrinhDaoTao, QuaTrinhCongTac, CongBoKhoaHoc, SangChe, DuAnNghienCuu)
VALUES (
N'nhanmtri@ctu.edu.vn', N'Nhan Minh Trí', 'nhan-minh-tri', N'01/01/1973',
N'Khu II Trường Đại học Cần Thơ', N'0908808207',
N'Phó giáo sư', N'Bộ môn Công nghệ sau thu hoạch, Viện CNSH và Thực phẩm, Trường Đại học Cần Thơ', N'Trưởng phòng thí nghiệm Hóa học Thực phẩm tiên tiến', N'12',
N'Tư vấn công nghệ sau thu hoạch, hóa học thực phẩm, hợp tác nghiên cứu quốc tế (Bỉ, Úc).',
N'Chủ nhiệm/tham gia 16 đề tài (2007-2025), tác giả hơn 34 bài báo khoa học (2004-2024) và 4 đầu sách/giáo trình.',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Trường Đại học Cần Thơ</td><td>Công nghệ thực phẩm</td><td>Việt Nam</td><td>1996</td></tr>
<tr><td>Thạc sĩ</td><td>KU.Leuven</td><td>Kỹ thuật sau thu hoạch và bảo quản thực phẩm</td><td>Bỉ</td><td>2001</td></tr>
<tr><td>Tiến sĩ</td><td>Đại học Sydney</td><td>Công nghệ thực phẩm</td><td>Úc</td><td>2013</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Nơi công tác</th><th>Công việc</th></tr>
<tr><td>1996-2022</td><td>Trường Đại học Cần Thơ</td><td>Giảng viên</td></tr>
<tr><td>Nhiều đợt</td><td>KU.Leuven, Đại học Sydney, Đại học Ghent</td><td>Tập huấn, nghiên cứu ngắn hạn</td></tr></table>',
N'<p>PGS. Nhan Minh Trí đã công bố hơn 34 bài báo khoa học (2004-2024, nhiều bài Scopus Q2-Q4), cùng 4 đầu sách/giáo trình (2000-2017) về công nghệ sau thu hoạch và hóa học thực phẩm.</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Nội dung</th><th>Số lượng</th><th>Giai đoạn</th></tr>
<tr><td>Sách/giáo trình</td><td>4</td><td>2000-2017</td></tr>
<tr><td>Bài báo khoa học (nhiều bài Scopus Q2-Q4)</td><td>34+</td><td>2004-2024</td></tr></table>',
N'',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Số lượng</th><th>Cấp</th><th>Thời gian</th></tr>
<tr><td>16 đề tài</td><td>Cấp trường, cấp tỉnh, quốc tế (VLIR)</td><td>2007-2025</td></tr></table>'
);

-- 3. Nguyen Duc Vuong
INSERT INTO #ChuyenGiaSeed (Email, FullName, QueryString, DateOfBirth, DiaChi, Phone, HocHam, CoQuan, ChucVu, LinhVucId, DichVu, KetQuaNghienCuu, QuaTrinhDaoTao, QuaTrinhCongTac, CongBoKhoaHoc, SangChe, DuAnNghienCuu)
VALUES (
N'nguyenducvuong@iuh.edu.vn', N'Nguyễn Đức Vượng', 'nguyen-duc-vuong', N'20/08/1980',
N'12 Nguyễn Văn Bảo, phường 4, quận Gò Vấp, TP. Hồ Chí Minh', N'0946616465',
N'', N'Viện Công nghệ Sinh học và Thực phẩm, Trường Đại học Công nghiệp TP.HCM', N'Giảng viên', N'12',
N'Tư vấn quy trình chế biến sản phẩm OCOP (trà, nước quả lên men), phân tích hóa lý thực phẩm.',
N'Chủ nhiệm/tham gia 6 đề tài (2006-2024), tác giả khoảng 30 bài báo quốc tế ISI/Scopus, 3 sách/chương sách quốc tế và 2 bằng sáng chế.',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Đại học Bách Khoa Hà Nội</td><td>Công nghệ thực phẩm</td><td>Việt Nam</td><td>2003</td></tr>
<tr><td>Thạc sĩ</td><td>Đại học Bách Khoa TP.HCM</td><td>Công nghệ Thực phẩm và Đồ uống</td><td>Việt Nam</td><td>2011</td></tr>
<tr><td>Tiến sĩ</td><td>Đại học Szent Itsvan</td><td>Khoa học Thực phẩm</td><td>Hungary</td><td>2017</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Nơi công tác</th><th>Công việc</th></tr>
<tr><td>2003-2005</td><td>Công ty Bibica II</td><td>Nhân viên</td></tr>
<tr><td>2005-2007</td><td>Trung tâm phân tích phân loại hàng hóa XNK</td><td>Nhân viên</td></tr>
<tr><td>2007-nay</td><td>Trường Đại học Công nghiệp TP.HCM</td><td>Giảng viên</td></tr></table>',
N'<p>TS. Nguyễn Đức Vượng đã công bố khoảng 30 bài báo quốc tế ISI/Scopus (Fuel, Food Chemistry, Journal of Molecular Liquids...) từ 2011-2024, cùng 3 sách/chương sách quốc tế (Bentham Science 2024, ĐH Công nghiệp TPHCM 2023).</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Nội dung</th><th>Số lượng</th><th>Giai đoạn</th></tr>
<tr><td>Sách/chương sách quốc tế</td><td>3</td><td>2023-2024</td></tr>
<tr><td>Bài báo quốc tế ISI/Scopus</td><td>~30</td><td>2011-2024</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Tên sáng chế</th><th>Cơ quan cấp</th><th>Ngày cấp</th></tr>
<tr><td>Quy trình sản xuất trà/nước quả mãng cầu gai lên men (2 bằng sáng chế)</td><td>Cục Sở hữu trí tuệ</td><td>24/05/2023</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Số lượng</th><th>Cấp</th><th>Thời gian</th></tr>
<tr><td>6 đề tài (chủ yếu về OCOP, mãng cầu gai, chanh mật ong)</td><td>Cấp tỉnh Sóc Trăng</td><td>2006-2024</td></tr></table>'
);

-- 4. Nguyen Thi Lang
INSERT INTO #ChuyenGiaSeed (Email, FullName, QueryString, DateOfBirth, DiaChi, Phone, HocHam, CoQuan, ChucVu, LinhVucId, DichVu, KetQuaNghienCuu, QuaTrinhDaoTao, QuaTrinhCongTac, CongBoKhoaHoc, SangChe, DuAnNghienCuu)
VALUES (
N'ntlang.prof@gmail.com', N'Nguyễn Thị Lang', 'nguyen-thi-lang', N'',
N'G9-11, Đường 31, Khu 586, P. Phú Thứ, Q. Cái Răng, TP. Cần Thơ', N'0909273484',
N'Giáo sư', N'Viện Nghiên cứu Nông nghiệp Công nghệ cao Đồng bằng sông Cửu Long', N'Viện Trưởng', N'10;14',
N'Tư vấn chọn tạo giống lúa (chống chịu mặn, hạn, ngập, phẩm chất cao), ứng dụng chỉ thị phân tử trong chọn giống cây trồng.',
N'Chủ nhiệm/tham gia 157 đề tài/dự án nghiên cứu (1993-2026), tác giả hơn 437 bài báo khoa học và 24 đầu sách/giáo trình, 4 bằng phát minh sáng chế và 20 bằng giải pháp hữu ích (giống lúa).',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Trường Đại học Tổng hợp TP Hồ Chí Minh (văn bằng 2: Đại học Kinh tế Chính trị, 2009)</td><td>Sinh học</td><td>Việt Nam</td><td>1979</td></tr>
<tr><td>Tiến sĩ</td><td>Viện Khoa học Nông nghiệp Việt Nam</td><td>Di truyền chọn giống</td><td>Việt Nam</td><td>1994</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Nơi công tác</th><th>Chức vụ</th></tr>
<tr><td>1979-1989</td><td>Sở Khoa học Bến Tre</td><td>Phó trưởng phòng kế hoạch khoa học</td></tr>
<tr><td>2000-2/2012</td><td>Viện Lúa Đồng Bằng Sông Cửu Long</td><td>Trưởng bộ môn</td></tr>
<tr><td>2012-nay</td><td>Trường Đại học An Giang / Đại học Mekong</td><td>Trưởng bộ môn Công nghệ sinh học</td></tr>
<tr><td>2/2012-2/2017</td><td>Viện lúa ĐBSCL</td><td>Nghiên cứu viên cao cấp</td></tr>
<tr><td>6/2017-nay</td><td>Viện Nghiên cứu Nông nghiệp Công nghệ cao ĐBSCL</td><td>Viện trưởng</td></tr></table>',
N'<p>GS.TS Nguyễn Thị Lang đã công bố hơn 437 bài báo khoa học trong nước và quốc tế (Omon Rice, SABRAO Journal of Breeding and Genetics, Rice Genetics Newsletter, PLOS ONE, New Phytologist, Ecology and Evolution...), cùng 24 đầu sách/giáo trình về di truyền, giống lúa và công nghệ sinh học thực vật.</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Sách tiêu biểu</th><th>Nhà xuất bản</th><th>Năm</th></tr>
<tr><td>Khoa học Cây lúa</td><td>NXB Nông Nghiệp TP.HCM</td><td>2011</td></tr>
<tr><td>Công nghệ gen</td><td>NXB Nông Nghiệp TP.HCM</td><td>2012</td></tr>
<tr><td>Giáo trình Di Truyền Phân Tử (tái bản lần 3)</td><td>NXB Nông Nghiệp</td><td>2008</td></tr>
<tr><td>Chọn giống lúa ngập và mặn phục vụ ĐBSCL</td><td>NXB Nông Nghiệp</td><td>2015</td></tr>
<tr><td>Chọn tạo các giống lúa chống chịu mặn thích nghi biến đổi khí hậu cho ĐBSCL</td><td>NXB Giáo dục</td><td>2020</td></tr>
<tr><td colspan="3">(và các sách/giáo trình khác, tổng cộng 24 đầu sách)</td></tr></table>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bài báo tiêu biểu</th><th>Tạp chí</th><th>Năm</th></tr>
<tr><td>Diversity of Global Rice Markets and the Science Required for Consumer-Targeted Rice Breeding</td><td>PLOS ONE</td><td>2014</td></tr>
<tr><td>Pathogenicity of Rice Blast (Pyricularia oryzae Cavara) Isolates from Mekong River Delta, Vietnam</td><td>JARQ</td><td>2020</td></tr>
<tr><td>Genetic diversity among perennial wild rice Oryza rufipogon Griff. in the Mekong Delta</td><td>Ecology and Evolution</td><td>2019</td></tr>
<tr><td>Development of rice genotypes tolerant to salinity stress in the Mekong Delta using marker-assisted selection</td><td>SABRAO Journal of Breeding and Genetics</td><td>2018</td></tr>
<tr><td>The influences of stomatal size and density on rice abiotic stress resilience</td><td>New Phytologist</td><td>2023</td></tr>
<tr><td>Breeding rice (Oryza sativa L.) for salt tolerance and grain quality traits</td><td>-</td><td>2020</td></tr>
<tr><td>Quantitative trait loci (QTLs) associated with drought tolerance in rice (Oryza sativa L.)</td><td>SABRAO Journal of Breeding and Genetics</td><td>2013</td></tr>
<tr><td>Evaluation of rice germplasm for yield traits and amylose content under drought stress</td><td>SABRAO Journal of Breeding and Genetics</td><td>2020</td></tr>
<tr><td colspan="3">(và nhóm bài báo khác, tổng cộng hơn 437 bài)</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Tên sáng chế</th><th>Năm cấp</th></tr>
<tr><td>Bằng phát minh, sáng chế Giống lúa OM 4900</td><td>2010</td></tr>
<tr><td>Bằng phát minh, sáng chế Giống lúa OM 6161</td><td>2010</td></tr>
<tr><td>Bằng phát minh, sáng chế Giống lúa OM 6162</td><td>2010</td></tr>
<tr><td>Bằng phát minh, sáng chế Giống lúa OM7347</td><td>2012</td></tr>
<tr><td colspan="2">(và 20 bằng giải pháp hữu ích khác cho các giống lúa OM, 2010-2013)</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Đề tài tiêu biểu</th><th>Cấp</th><th>Vai trò</th></tr>
<tr><td>Nghiên cứu chọn tạo giống lúa chống chịu mặn thích nghi biến đổi khí hậu cho ĐBSCL</td><td>Nhà nước</td><td>Đồng chủ trì</td></tr>
<tr><td>Nghiên cứu tạo chọn các giống lúa chống chịu mặn thích nghi với biến đổi khí hậu cho vùng ĐBSCL</td><td>Nhà nước</td><td>Đồng chủ trì</td></tr>
<tr><td>Producing climate-ready rice with improved salt and drought tolerance for the Mekong Delta of Vietnam</td><td>Quốc tế (Anh - Đại học Sheffield)</td><td>Chủ trì</td></tr>
<tr><td>Development of Inbred Rice Varieties Adaptable to Southern Region (Mekong Delta) of Vietnam Under Climate Change</td><td>Quốc tế (Hàn Quốc)</td><td>Chủ trì</td></tr>
<tr><td>Evaluted new IRRI varieties</td><td>Quốc tế (IRRI)</td><td>Chủ trì</td></tr>
<tr><td>Bảo tồn tài nguyên lúa hoang và lúa địa phương tại Miền Nam</td><td>Nhà nước</td><td>Chủ trì</td></tr>
<tr><td colspan="3">(và nhóm đề tài/dự án khác, tổng cộng 157 đề tài từ 1993 đến nay)</td></tr></table>'
);

-- 5. Nguyen Huu Hiep
INSERT INTO #ChuyenGiaSeed (Email, FullName, QueryString, DateOfBirth, DiaChi, Phone, HocHam, CoQuan, ChucVu, LinhVucId, DichVu, KetQuaNghienCuu, QuaTrinhDaoTao, QuaTrinhCongTac, CongBoKhoaHoc, SangChe, DuAnNghienCuu)
VALUES (
N'hiepngu@gmail.com', N'Nguyễn Hữu Hiệp', 'nguyen-huu-hiep', N'07/08/1955',
N'22/2A Trần Hoàng Na, Phường Hưng Lợi, Quận Ninh Kiều, Thành phố Cần Thơ', N'0941928238',
N'Phó giáo sư', N'Hội Sinh học, Liên hiệp các Hội KHKT, TP Cần Thơ', N'Phó Chủ tịch', N'9;10',
N'Tư vấn phân lập, tuyển chọn vi sinh vật (cố định đạm, nội sinh, kháng khuẩn) phục vụ nông nghiệp và xử lý môi trường.',
N'Chủ nhiệm/tham gia 13 đề tài (1985-2017), tác giả 19 bài báo khoa học (6 quốc tế, 13 trong nước) và 5 đầu sách/giáo trình.',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Đại học Cần Thơ</td><td>Trồng trọt</td><td>Việt Nam</td><td>1978</td></tr>
<tr><td>Tiến sĩ</td><td>Đại học Cần Thơ - Đại học Wageningen</td><td>Vi sinh vật</td><td>Việt Nam - Hà Lan</td><td>1994</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Nơi công tác</th><th>Vị trí</th></tr>
<tr><td>1978-1994</td><td>Đại học Cần Thơ</td><td>Tập sự giảng dạy, giảng viên, nghiên cứu sinh</td></tr>
<tr><td>1994-2004</td><td>Đại học Cần Thơ</td><td>Giảng viên chính, Trưởng phòng thí nghiệm Vi sinh</td></tr>
<tr><td>2004-2017</td><td>Đại học Cần Thơ</td><td>Phó Giáo sư, Phó trưởng bộ môn</td></tr>
<tr><td>2018-2/2020</td><td>Đại học Cần Thơ</td><td>Giảng viên cao cấp loại 1</td></tr>
<tr><td>2/2020-nay</td><td>Hội Sinh học, Liên hiệp các Hội KHKT TP Cần Thơ</td><td>Về hưu; Phó Chủ tịch Hội Sinh học</td></tr></table>',
N'<p>PGS.TS Nguyễn Hữu Hiệp đã công bố 19 bài báo khoa học (6 quốc tế, 13 trong nước) và 5 đầu sách/giáo trình về vi sinh vật học và ứng dụng vi khuẩn nội sinh, cố định đạm.</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Sách/giáo trình</th><th>Nhà xuất bản</th><th>Năm</th></tr>
<tr><td>Giáo trình Vi sinh vật Đại cương</td><td>NXB Đại học Cần Thơ</td><td>2012</td></tr>
<tr><td>Giáo trình Vi sinh vật học môi trường</td><td>NXB Đại học Cần Thơ</td><td>2013</td></tr>
<tr><td>Vi khuẩn liên kết với thực vật (Plant Associated Bacteria)</td><td>NXB Đại học Cần Thơ</td><td>2016</td></tr>
<tr><td>Nghiên cứu ứng dụng Công nghệ Sinh học tại Trường Đại học Cần Thơ và định hướng phát triển</td><td>NXB Đại học Cần Thơ</td><td>2016</td></tr>
<tr><td>Quản lý độ phì nhiêu đất lúa ở đồng bằng sông Cửu Long</td><td>NXB Đại học Cần Thơ</td><td>2016</td></tr></table>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bài báo tiêu biểu</th><th>Tạp chí</th><th>Năm</th></tr>
<tr><td>Diversity of Methylobacterium spp. in the Rice of the Vietnamese Mekong Delta</td><td>Microbes Environment</td><td>2020</td></tr>
<tr><td>Biodegradation of propanil by Acinetobacter baumannii DT in a biofilm-batch reactor</td><td>FEMS Microbiology Letters</td><td>2020</td></tr>
<tr><td>Isolation, Selection and Identification Nitrogen Fixation Rhizopheric and Endophytic bacteria from maize</td><td>Int. J. of Environmental and Agriculture Research</td><td>2021</td></tr>
<tr><td>Beneficial effects of nitrogen fixing bacteria on the growth and yield of corn cultivated at An Giang province</td><td>Int. J. of Environmental and Agriculture Research</td><td>2022</td></tr>
<tr><td>Relationships between endophytic bacteria and medicinal plants on bioactive compounds production</td><td>Rhizosphere</td><td>2023</td></tr>
<tr><td>Phân lập và nhận diện vi khuẩn nội sinh trong cây bắp có khả năng cố định nitơ ở tỉnh An Giang</td><td>Tạp chí Nông nghiệp và PTNT</td><td>2021</td></tr>
<tr><td colspan="3">(và các bài báo trong nước khác, tổng cộng 19 bài)</td></tr></table>',
N'',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Đề tài</th><th>Vai trò</th><th>Thời gian</th></tr>
<tr><td>Ảnh hưởng của kỹ thuật canh tác đậu nành chủng Bradyrhizobium japonicum trong luân canh đậu-lúa (Cần Thơ, Kiên Giang)</td><td>Chủ trì</td><td>1999-2000</td></tr>
<tr><td>Increasing yields and nitrogen fixation of soybean, groundnut and mungbean in Vietnam through rhizobial inoculation (ACIAR, Úc)</td><td>Chủ trì</td><td>1999-2001</td></tr>
<tr><td>Ảnh hưởng của chủng vi khuẩn cố định đạm Rhizobium cho đậu phộng trồng ở đất giồng cát Trà Vinh</td><td>Chủ trì</td><td>2005-2006</td></tr>
<tr><td>Molecular and genetic analysis of nitrogen-fixation microbial communities isolated from rice and sugarcane (Nghị định thư với Ý)</td><td>Chủ trì</td><td>2007-2008</td></tr>
<tr><td>Chọn lọc vi sinh vật bản địa sản xuất chế phẩm sinh học phục vụ nuôi tôm Sú (Sóc Trăng)</td><td>Chủ trì</td><td>2008-2010</td></tr>
<tr><td>Sử dụng vi khuẩn Bacillus subtilis bản địa xử lý nước thải giết mổ gia súc</td><td>Chủ trì</td><td>2009-2011</td></tr>
<tr><td>Study of the mechanism of the association of nitrogen-fixing Pseudomonas stutzeri with rice roots (Bỉ - NAFOSTED)</td><td>Chủ trì</td><td>2012-2017</td></tr>
<tr><td>Studies on Tropical Plant-Microbe Interaction (VLIR-B3, Bỉ)</td><td>Tham gia</td><td>1998-2003</td></tr>
<tr><td>Establishment of an International Research Core for New Bio-research Fields with Microbes from Tropical Areas</td><td>Tham gia</td><td>2014-2019</td></tr></table>'
);

-- 6. Nguyen Chi Ngon
INSERT INTO #ChuyenGiaSeed (Email, FullName, QueryString, DateOfBirth, DiaChi, Phone, HocHam, CoQuan, ChucVu, LinhVucId, DichVu, KetQuaNghienCuu, QuaTrinhDaoTao, QuaTrinhCongTac, CongBoKhoaHoc, SangChe, DuAnNghienCuu)
VALUES (
N'ncngon@ctu.edu.vn', N'Nguyễn Chí Ngôn', 'nguyen-chi-ngon', N'1972',
N'X3, Đường số 13, Khu Công ty 8, phường Hưng Thạnh, quận Cái Răng, thành phố Cần Thơ', N'0918538224',
N'Phó giáo sư', N'Trường Đại học Cần Thơ', N'Phó chủ tịch Hội đồng trường', N'4;1079',
N'Tư vấn tự động hóa, điều khiển thông minh, IoT, thị giác máy, ứng dụng AI trong nông nghiệp và công nghiệp.',
N'Chủ nhiệm/tham gia 3 đề tài/dự án (2015-2021), tác giả 15 bài báo tạp chí quốc tế, 5 báo cáo hội nghị quốc tế và 8 báo cáo hội nghị quốc gia (2020-2024), 2 bằng độc quyền sáng chế.',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Đại học Cần Thơ</td><td>Điện tử</td><td>Việt Nam</td><td>1996</td></tr>
<tr><td>Thạc sĩ</td><td>Đại học Bách khoa TP.HCM</td><td>Điện tử vô tuyến</td><td>Việt Nam</td><td>2001</td></tr>
<tr><td>Tiến sĩ</td><td>Đại học Rostock</td><td>Tự động hóa</td><td>CHLB Đức</td><td>2007</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Vị trí</th><th>Cơ quan</th></tr>
<tr><td>1996-2007</td><td>Giảng viên</td><td>Đại học Cần Thơ</td></tr>
<tr><td>2007-2008</td><td>Trưởng bộ môn</td><td>Đại học Cần Thơ</td></tr>
<tr><td>2008-2012</td><td>Phó trưởng khoa</td><td>Đại học Cần Thơ</td></tr>
<tr><td>2012-2021</td><td>Trưởng khoa</td><td>Đại học Cần Thơ</td></tr>
<tr><td>2021-nay</td><td>Phó chủ tịch Hội đồng trường</td><td>Đại học Cần Thơ</td></tr></table>',
N'<p>PGS.TS Nguyễn Chí Ngôn đã công bố 15 bài báo tạp chí quốc tế (chủ yếu Scopus/SCIE Q2-Q4), 5 báo cáo hội nghị quốc tế và 8 báo cáo hội nghị quốc gia trong giai đoạn 2020-2024, tập trung vào tự động hóa, robot, cảm biến và ứng dụng AI.</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bài báo tiêu biểu</th><th>Tạp chí</th><th>Năm</th></tr>
<tr><td>Early detection of slight bruises in apples by cost-efficient near-infrared imaging</td><td>Int. J. of Electrical and Computer Eng. (Scopus/Q2)</td><td>2022</td></tr>
<tr><td>In situ measurement of fish color based on machine vision: A case study of measuring a clownfish''s color</td><td>Measurement</td><td>2022</td></tr>
<tr><td>Localized automation solutions in response to the first wave of COVID-19: a story from Vietnam</td><td>Int. J. of Pervasive Computing and Communications (Scopus/Q2)</td><td>2022</td></tr>
<tr><td>Predictive Modeling of Landslide Susceptibility in Soft Soil Canal Regions</td><td>Int. J. of Advanced Computer Science and Applications (Scopus/Q3)</td><td>2023</td></tr>
<tr><td>A multi-microcontroller-based hardware for deploying Tiny machine learning model</td><td>Int. J. of Electrical and Computer Engineering (Scopus/Q3)</td><td>2023</td></tr>
<tr><td>Adaptive PID sliding mode control based on new Quasi-sliding mode and radial basis function neural network for Omnidirectional mobile robot</td><td>AIMS Electronics and Electrical Engineering</td><td>2023</td></tr>
<tr><td>Development of a soil electrical conductivity measurement system in paddy fields</td><td>Int. J. of Advances in Applied Sciences (Scopus)</td><td>2024</td></tr>
<tr><td colspan="3">(và các bài báo/báo cáo hội nghị khác, tổng cộng 28 công trình 5 năm gần đây)</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Tên sáng chế</th><th>Cơ quan cấp</th><th>Năm</th></tr>
<tr><td>Hệ thống truyền dữ liệu trong nước phục vụ quan trắc môi trường</td><td>Cục Sở hữu trí tuệ</td><td>2023</td></tr>
<tr><td>Phương pháp và hệ thống xác định màu sắc động vật thủy sản và vật thể dựa trên hình ảnh</td><td>Cục Sở hữu trí tuệ</td><td>2024</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Đề tài/dự án</th><th>Thời gian</th><th>Kết quả</th></tr>
<tr><td>Nghiên cứu giải pháp thúc đẩy hoạt động chuyển giao, ứng dụng, đổi mới công nghệ tại thành phố Cần Thơ</td><td>2018-2021</td><td>Đã nghiệm thu, xếp loại tốt</td></tr>
<tr><td>Xây dựng hệ thống trợ giúp khuyến nông trực tuyến tại Đồng bằng sông Cửu Long</td><td>2015-2018</td><td>Đã nghiệm thu, xếp loại tốt</td></tr>
<tr><td>ECO-RED (Erasmus+)</td><td>2015-2018</td><td>Đã nghiệm thu, xếp loại tốt</td></tr></table>'
);

-- 7. Truong Minh Nhat Quang
INSERT INTO #ChuyenGiaSeed (Email, FullName, QueryString, DateOfBirth, DiaChi, Phone, HocHam, CoQuan, ChucVu, LinhVucId, DichVu, KetQuaNghienCuu, QuaTrinhDaoTao, QuaTrinhCongTac, CongBoKhoaHoc, SangChe, DuAnNghienCuu)
VALUES (
N'tmnquang@ctuet.edu.vn', N'Trương Minh Nhật Quang', 'truong-minh-nhat-quang', N'20/02/1965',
N'256 Nguyễn Văn Cừ, Phường An Hòa, Quận Ninh Kiều, TP Cần Thơ', N'0918192592',
N'', N'Trường Đại học Kỹ thuật - Công nghệ Cần Thơ', N'Phó Hiệu trưởng', N'4;1078',
N'Tư vấn an ninh mạng, phòng chống virus/mã độc máy tính, chuyển đổi số cho tổ chức, doanh nghiệp.',
N'Chủ nhiệm/tham gia 4 đề tài (2004-2023), tác giả 8 bài báo khoa học (1997-2023) và 2 sách/giáo trình về công nghệ thông tin.',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Đại học Cần Thơ</td><td>Sư phạm Vật Lý</td><td>Việt Nam</td><td>1989</td></tr>
<tr><td>Thạc sĩ</td><td>Viện Tin học Pháp ngữ IFI</td><td>Công nghệ thông tin</td><td>Việt Nam</td><td>1999</td></tr>
<tr><td>Tiến sĩ</td><td>Trường Đại học Khoa học Tự nhiên - ĐHQG TP.HCM</td><td>Đảm bảo toán học cho máy tính và hệ thống tính toán</td><td>Việt Nam</td><td>2009</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Nơi công tác</th><th>Công việc</th></tr>
<tr><td>1989-1990</td><td>Trường THPT Hà Tiên, Kiên Giang</td><td>Giáo viên</td></tr>
<tr><td>1991-2013</td><td>Trung tâm Đại học Tại chức Cần Thơ</td><td>Giảng viên, giảng viên chính</td></tr>
<tr><td>2013-2014</td><td>Trường Đại học Kỹ thuật - Công nghệ Cần Thơ</td><td>Trưởng Khoa CNTT</td></tr>
<tr><td>2014-nay</td><td>Trường Đại học Kỹ thuật - Công nghệ Cần Thơ</td><td>Phó Hiệu trưởng, Trưởng Khoa CNTT</td></tr></table>',
N'<p>TS. Trương Minh Nhật Quang đã công bố 8 bài báo khoa học (1997-2023) về chẩn đoán/phòng chống virus máy tính và an ninh mạng, cùng 2 đầu sách/giáo trình.</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Sách</th><th>Nhà xuất bản</th><th>Năm</th></tr>
<tr><td>Giáo trình Tin học căn bản</td><td>NXB Trường Đại học Cần Thơ</td><td>2019</td></tr>
<tr><td>Các hình thức tấn công mạng Cyberspace</td><td>NXB Khoa học và Kỹ thuật</td><td>2022</td></tr></table>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bài báo</th><th>Tạp chí</th><th>Năm</th></tr>
<tr><td>Các giải pháp cho phần mềm chống virus thông minh</td><td>Tạp chí Tin học và Điều khiển</td><td>1997</td></tr>
<tr><td>Các cơ chế chẩn đoán virus tin học thông minh dựa trên tri thức</td><td>Tạp chí Tin học và Điều khiển</td><td>1998</td></tr>
<tr><td>Cây chỉ thị nhị phân biểu diễn không gian trạng thái chẩn đoán virus tin học</td><td>Tạp chí Tin học và Điều khiển</td><td>1999</td></tr>
<tr><td>Máy ảo, công cụ hỗ trợ chẩn đoán và diệt virus tin học thông minh</td><td>Tạp chí Tin học và Điều khiển</td><td>2000</td></tr>
<tr><td>Ứng dụng Máy học và Hệ chuyên gia trong phân loại và nhận dạng virus máy tính</td><td>Tạp chí Công nghệ thông tin và Truyền thông</td><td>2008</td></tr>
<tr><td>Cơ chế máy học chẩn đoán virus máy tính</td><td>Tạp chí Tin học và Điều khiển</td><td>2008</td></tr>
<tr><td>Nhận dạng mã độc sử dụng cơ chế băm theo chỉ mục trên không gian dữ liệu phân hoạch</td><td>Tạp chí Khoa học Trường Đại học Cần Thơ</td><td>2013</td></tr>
<tr><td>Abnormal network packets identification using header information collected from Honeywall architecture</td><td>Tạp chí Thông tin và Truyền thông, ĐH Tôn Đức Thắng</td><td>2023</td></tr></table>',
N'',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Đề tài/dự án</th><th>Thời gian</th><th>Kết quả</th></tr>
<tr><td>Hệ phòng chống virus máy tính hướng tiếp cận máy học (cấp Thành phố)</td><td>2004-2006</td><td>Đạt</td></tr>
<tr><td>Xây dựng hệ thống điều khiển truy xuất dữ liệu lớn cho trường đại học, cao đẳng (cấp Thành phố)</td><td>2016-2018</td><td>Đạt</td></tr>
<tr><td>Nghiên cứu xây dựng hệ thống HoneyWall bảo vệ website đơn vị hành chính, sự nghiệp TP Cần Thơ (cấp Thành phố)</td><td>2019-2021</td><td>Khá</td></tr>
<tr><td>Giải pháp chuyển đổi số của Trường Đại học Kỹ thuật - Công nghệ Cần Thơ đến 2025 (cấp Cơ sở)</td><td>2022-2023</td><td>Tốt</td></tr></table>'
);

-- 8. Nguyen Tan Tien
INSERT INTO #ChuyenGiaSeed (Email, FullName, QueryString, DateOfBirth, DiaChi, Phone, HocHam, CoQuan, ChucVu, LinhVucId, DichVu, KetQuaNghienCuu, QuaTrinhDaoTao, QuaTrinhCongTac, CongBoKhoaHoc, SangChe, DuAnNghienCuu)
VALUES (
N'nttien@hcmut.edu.vn', N'Nguyễn Tấn Tiến', 'nguyen-tan-tien', N'29/06/1968',
N'Block C6, 268 Lý Thường Kiệt, Q.10, TP. Hồ Chí Minh (Trường Đại học Bách khoa)', N'0918255355',
N'Phó Giáo sư', N'Phòng thí nghiệm Trọng điểm Quốc gia Điều khiển Số và Kỹ thuật Hệ thống, Trường Đại học Bách khoa TP.HCM', N'Giám đốc', N'5',
N'Tư vấn thiết kế cơ khí, cơ điện tử, robot công nghiệp và y tế, điều khiển tự động.',
N'Chủ nhiệm 20 đề tài/dự án (2002-2022), tác giả 20 bài báo tạp chí quốc tế và 34 bài tạp chí trong nước (5 năm gần đây), 2 giáo trình.',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Trường Đại học Bách khoa</td><td>Kỹ thuật Cơ khí</td><td>Việt Nam</td><td>1990</td></tr>
<tr><td>Thạc sĩ</td><td>Trường Đại học Quốc gia Pukyong, Busan</td><td>Kỹ thuật Cơ khí (Cơ điện tử)</td><td>Hàn Quốc</td><td>1998</td></tr>
<tr><td>Tiến sĩ</td><td>Trường Đại học Quốc gia Pukyong, Busan</td><td>Kỹ thuật Cơ khí (Cơ điện tử)</td><td>Hàn Quốc</td><td>2001</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Nơi công tác</th><th>Vị trí</th></tr>
<tr><td>1990-1999</td><td>Khoa Cơ khí, Trường Đại học Bách khoa</td><td>Giảng viên</td></tr>
<tr><td>1996-2002</td><td>ĐHQG Pukyong, Busan, Hàn Quốc</td><td>Học thạc sĩ, nghiên cứu sinh, PostDoc</td></tr>
<tr><td>2002-2010</td><td>Phòng KHCN-QHQT/Phòng QHĐN, Trường Đại học Bách khoa</td><td>Phó trưởng phòng, Trưởng phòng</td></tr>
<tr><td>2010-2019</td><td>Khoa Cơ khí / Phòng thí nghiệm Công nghệ thiết kế và Gia công tiên tiến</td><td>Phó trưởng khoa/phòng</td></tr>
<tr><td>2018-nay</td><td>Phòng thí nghiệm Trọng điểm Quốc gia Điều khiển Số và Kỹ thuật Hệ thống</td><td>Giám đốc</td></tr></table>',
N'<p>PGS.TS Nguyễn Tấn Tiến đã công bố 20 bài báo tạp chí quốc tế (chủ yếu SCIE/Scopus) và 34 bài tạp chí trong nước trong 5 năm gần đây, cùng 2 giáo trình về cơ khí và tin học kỹ thuật.</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Giáo trình</th><th>Nhà xuất bản</th><th>Năm</th></tr>
<tr><td>Giáo trình Cơ học máy</td><td>NXB Xây dựng</td><td>2023</td></tr>
<tr><td>Giáo trình Tin học Kỹ thuật</td><td>NXB Xây dựng</td><td>2023</td></tr></table>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bài báo tiêu biểu</th><th>Tạp chí</th><th>Năm</th></tr>
<tr><td>Investigation of Geometric and Hardness Parameters of Tank Track Grooves Equipped on Photovoltaic Cleaning Robot</td><td>Applied Sciences (SCIE/Q2)</td><td>2023</td></tr>
<tr><td>Development of a Multi-Suspension Unit for Solar Cleaning Robots</td><td>Applied Sciences (SCIE/Q2)</td><td>2023</td></tr>
<tr><td>Terminal Super-Twisting Sliding Mode based on High Order Sliding Mode Observer for Two DOFs Lower Limb System</td><td>Measurement and Control (SCIE/Q3)</td><td>2023</td></tr>
<tr><td>Parameter-Adaptive Event-Triggered Sliding Mode Control for a Mobile Robot</td><td>Robotics (ESCI/Q1)</td><td>2022</td></tr>
<tr><td>Force Optimization of Elongated Undulating Fin Robot using Improved PSO-Based CPG</td><td>Computational Intelligence and Neuroscience (SCIE/Q1)</td><td>2022</td></tr>
<tr><td>Dynamic Analysis of a Robotic Fish Propelled by Flexible Folding Pectoral Fins</td><td>Robotica (SCIE/Q2)</td><td>2020</td></tr>
<tr><td colspan="3">(và các bài báo khác, tổng cộng 54 bài quốc tế và trong nước 5 năm gần đây)</td></tr></table>',
N'',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Đề tài/dự án</th><th>Cấp</th><th>Thời gian</th><th>Kết quả</th></tr>
<tr><td>Nghiên cứu thiết kế và điều khiển cụm trục chính máy CNC</td><td>Thành phố</td><td>2019-2022</td><td>Đạt</td></tr>
<tr><td>Nghiên cứu thiết kế cơ cấu tác động đàn hồi ứng dụng cho humanoid</td><td>Bộ</td><td>2019-2021</td><td>Tốt</td></tr>
<tr><td>Nghiên cứu thiết kế, chế tạo và điều khiển robot đồng diễn</td><td>Trường</td><td>2017</td><td>Tốt</td></tr>
<tr><td>Nghiên cứu thiết kế, chế tạo giường y tế theo dạng module</td><td>Thành phố</td><td>2017-2018</td><td>Khá</td></tr>
<tr><td>Analysis of Current Status of Energy Industry and Strategy for ILI Market Entry in Vietnam (HCMUT-KOGAS)</td><td>Quốc tế</td><td>2016-2017</td><td>Đạt</td></tr>
<tr><td>Nghiên cứu, thiết kế và chế tạo robot dạng người ứng dụng cho dịch vụ chào hàng</td><td>Thành phố</td><td>2016-2018</td><td>Đạt</td></tr>
<tr><td>Nghiên cứu, thiết kế và chế tạo thực nghiệm bàn phẫu thuật</td><td>Thành phố</td><td>2014-2015</td><td>Khá</td></tr>
<tr><td>Nghiên cứu, thiết kế, chế tạo thử nghiệm robot thông đường ống trong dây chuyền sản xuất phân bón</td><td>Thành phố</td><td>2013-2014</td><td>Khá</td></tr>
<tr><td>Nghiên cứu điều khiển bền vững hệ thống treo 1/4 xe</td><td>Bộ</td><td>2012-2013</td><td>Tốt</td></tr>
<tr><td>Nghiên cứu thiết kế cảm biến hàn hồ quang quay ứng dụng dò đường hàn</td><td>Thành phố</td><td>2010-2012</td><td>Khá</td></tr>
<tr><td>Nghiên cứu thiết kế hệ thống tạo sóng</td><td>Bộ</td><td>2010-2011</td><td>Khá</td></tr>
<tr><td>Thiết kế, chế tạo thử nghiệm thiết bị đo lượng nước và điều áp ứng dụng trong máy nén 3 trục</td><td>Bộ</td><td>2009-2010</td><td>Khá</td></tr>
<tr><td>Mô hình hóa và mô phỏng quá trình hàn hồ quang</td><td>Bộ</td><td>2007-2008</td><td>Khá</td></tr>
<tr><td>Thiết kế hệ thống đo ứng dụng trong máy nén ba trục</td><td>Trường</td><td>2007</td><td>Tốt</td></tr>
<tr><td>Thiết kế, chế tạo và điều khiển mô hình robot di động dùng để hàn trong mặt phẳng</td><td>Thành phố</td><td>2005-2006</td><td>Khá</td></tr>
<tr><td>Thiết kế, chế tạo và điều khiển robot hàn di động (mô hình động học và động lực học)</td><td>Bộ</td><td>2003-2004</td><td>Tốt</td></tr>
<tr><td>Thiết kế, chế tạo robot hàn di động</td><td>Trường</td><td>2002-2003</td><td>Tốt</td></tr>
<tr><td colspan="4">(và các đề tài khác về hệ thống kiểm định phòng cháy chữa cháy, robot vớt lục bình, tổng cộng 20 đề tài)</td></tr></table>'
);

-- 9. Thai Phuong Vu
INSERT INTO #ChuyenGiaSeed (Email, FullName, QueryString, DateOfBirth, DiaChi, Phone, HocHam, CoQuan, ChucVu, LinhVucId, DichVu, KetQuaNghienCuu, QuaTrinhDaoTao, QuaTrinhCongTac, CongBoKhoaHoc, SangChe, DuAnNghienCuu)
VALUES (
N'tpvu@hcmunre.edu.vn', N'Thái Phương Vũ', 'thai-phuong-vu', N'1974',
N'236B Lê Văn Sỹ, phường 1, quận Tân Bình, TP. Hồ Chí Minh (địa chỉ cơ quan - mẫu CV không có mục địa chỉ nhà riêng)', N'0942785007',
N'', N'Trường Đại học Tài Nguyên và Môi Trường TP.HCM', N'Phó Trưởng Khoa Môi trường, Viện Nghiên cứu phát triển bền vững', N'9',
N'Tư vấn công nghệ xử lý nước thải, vật liệu hấp phụ xử lý ô nhiễm nước, thẩm định giá công nghệ môi trường.',
N'Chủ nhiệm 3 đề tài (2021-2023), tác giả 5 bài báo khoa học (2024) về vật liệu hấp phụ xử lý nước, và 4 công nghệ đã ứng dụng thực tiễn (2013-2022).',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Trường Đại học Cần Thơ (văn bằng 2: Tài chính - Tín dụng, 2001)</td><td>Sư phạm Hóa học</td><td>Việt Nam</td><td>1995</td></tr>
<tr><td>Thạc sĩ</td><td>Viện Công nghệ Châu Á và Viện Khoa học và Công nghệ Hàn Quốc</td><td>Công nghệ và Quản lý môi trường</td><td>Thailand</td><td>2007</td></tr>
<tr><td>Tiến sĩ</td><td>Viện Khoa học và Công nghệ Hàn Quốc và Trường Đại học Khoa học và Công nghệ</td><td>Kỹ thuật môi trường xây dựng</td><td>Hàn Quốc</td><td>2013</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Vị trí</th><th>Tổ chức</th></tr>
<tr><td>1995-1999</td><td>Tổ Trưởng</td><td>Xí nghiệp chế biến thực phẩm MEKO</td></tr>
<tr><td>1999-2004</td><td>Chuyên viên</td><td>Sở Khoa học, Công nghệ và Môi trường Cần Thơ</td></tr>
<tr><td>2004-2013</td><td>Trưởng Phòng Thí nghiệm</td><td>Trung tâm Quan trắc Tài nguyên và Môi trường Cần Thơ</td></tr>
<tr><td>2013-2015</td><td>Phó giám đốc</td><td>Trung tâm Ứng dụng tiến bộ khoa học và công nghệ, Sở KH&CN Cần Thơ</td></tr>
<tr><td>2015-2018</td><td>Giảng viên</td><td>Khoa Môi trường, Trường Đại học Tài nguyên và Môi trường TP.HCM</td></tr>
<tr><td>2018-2020</td><td>Trưởng Bộ môn</td><td>Khoa Môi trường, Trường Đại học Tài nguyên và Môi trường TP.HCM</td></tr>
<tr><td>2020-2023</td><td>Phó Viện Trưởng</td><td>Viện Nghiên cứu Phát triển bền vững, Trường Đại học Tài nguyên và Môi trường TP.HCM</td></tr>
<tr><td>2023-nay</td><td>Phó Trưởng Khoa</td><td>Khoa Môi trường, Trường Đại học Tài nguyên và Môi trường TP.HCM</td></tr></table>',
N'<p>TS. Thái Phương Vũ đã công bố 5 bài báo khoa học (2024) về vật liệu hấp phụ xử lý nước thải và nước cấp.</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bài báo</th><th>Tạp chí</th><th>Năm</th></tr>
<tr><td>Eliminating hexavalent chromium from aqueous solutions utilizing a low-cost adsorbent derived from biomass of Fabaceae plant in Vietnam</td><td>Environmental Quality Management</td><td>2024</td></tr>
<tr><td>Ball-Milled Biochar from Waste Bamboo Chopsticks: A Potential Adsorbent for Methylene Blue Removal</td><td>Application Environment Research</td><td>2024</td></tr>
<tr><td>Đánh giá khả năng hấp phụ chất màu methylene blue trong môi trường nước bằng vật liệu dolomite</td><td>Tài nguyên và Môi trường</td><td>2024</td></tr>
<tr><td>Nghiên cứu loại bỏ Cr(VI) trong môi trường nước bằng vật liệu tổng hợp từ xiên que tre</td><td>Tạp chí Khoa học Đại học Cần Thơ</td><td>2024</td></tr>
<tr><td>Đánh giá khả năng loại bỏ xanh methylen (MB) trong môi trường nước của vật liệu hấp phụ điều chế từ đũa tre dùng một lần</td><td>Tạp chí Khoa học Đại học Cần Thơ</td><td>2024</td></tr></table>',
N'',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Đề tài/công nghệ</th><th>Vai trò</th><th>Thời gian</th></tr>
<tr><td>Upgrading relationship between Vietnam Southern Alumni and KIST</td><td>Chủ trì</td><td>2023</td></tr>
<tr><td>Khảo sát, đánh giá tác động của tro, xỉ, thạch cao FGD, thạch cao PG làm vật liệu san lấp</td><td>Chủ trì</td><td>2021-2022</td></tr>
<tr><td>Research on design of material burning kiln to produce water filter material</td><td>Chủ trì</td><td>2021</td></tr>
<tr><td>Công nghệ ô xy hóa sâu - hệ thống xử lý nước thải chăn nuôi heo (Cà Mau)</td><td>Ứng dụng thực tiễn</td><td>2022</td></tr>
<tr><td>Công nghệ oxy hóa sâu - trạm cấp nước nông thôn tại Cần Thơ và Hậu Giang</td><td>Ứng dụng thực tiễn</td><td>2015</td></tr>
<tr><td>Công nghệ điện từ trường - sản phẩm xử lý nước uống đóng chai</td><td>Ứng dụng thực tiễn</td><td>2013</td></tr></table>'
);

-- 10. Nguyen Hoai Anh
INSERT INTO #ChuyenGiaSeed (Email, FullName, QueryString, DateOfBirth, DiaChi, Phone, HocHam, CoQuan, ChucVu, LinhVucId, DichVu, KetQuaNghienCuu, QuaTrinhDaoTao, QuaTrinhCongTac, CongBoKhoaHoc, SangChe, DuAnNghienCuu)
VALUES (
N'nghoaianhart@gmail.com', N'Nguyễn Hoài Anh', 'nguyen-hoai-anh', N'13/05/1989',
N'', N'0909180004',
N'', N'Trường Cao Đẳng Việt Mỹ Cần Thơ; Viện ứng dụng khoa học tâm lý giáo dục tại Cần Thơ', N'Trưởng Khoa Thiết Kế; Giám Đốc', N'',
N'Tư vấn thiết kế đồ họa - quảng cáo, ứng dụng công nghệ AR/VR trong mỹ thuật ứng dụng và truyền thông.',
N'Giảng viên thỉnh giảng tại nhiều trường đại học/cao đẳng; tham gia sản xuất nhiều chương trình truyền hình tại Đài Truyền hình Hậu Giang và VTV. Không có công bố khoa học/bằng sáng chế trong hồ sơ.',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Đại học Quốc tế Hồng Bàng</td><td>Thiết Kế Đồ Hoạ - Quảng Cáo</td><td>Việt Nam</td><td>2013</td></tr>
<tr><td>Thạc sĩ</td><td>Đại học Văn Lang</td><td>Mỹ Thuật Ứng Dụng (luận văn: Nghiên cứu ứng dụng công nghệ AR/VR)</td><td>Việt Nam</td><td>-</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Nơi công tác</th><th>Công việc</th></tr>
<tr><td>2013-nay</td><td>Đại học Quốc tế Hồng Bàng, Đại học Văn Lang, Đại học Kỹ thuật Công nghệ TP.HCM, Đại học Cần Thơ, Đại học Nam Cần Thơ, Đại học Tây Đô</td><td>Giảng viên</td></tr>
<tr><td>2015-2020</td><td>Đài Phát thanh và Truyền hình Hậu Giang</td><td>Trưởng bộ phận thiết kế</td></tr>
<tr><td>2020-nay</td><td>Trường Cao Đẳng Việt Mỹ Cần Thơ</td><td>Trưởng Khoa Thiết Kế</td></tr>
<tr><td>2023-nay</td><td>Viện Ứng dụng khoa học tâm lý giáo dục</td><td>Phó Viện Trưởng</td></tr>
<tr><td>2024-nay</td><td>Viện Ứng dụng khoa học tâm lý giáo dục tại Cần Thơ</td><td>Giám Đốc</td></tr></table>',
N'<p>Hồ sơ không liệt kê bài báo khoa học hay sách/giáo trình xuất bản. Kinh nghiệm chuyên môn chủ yếu qua các sản phẩm truyền thông - thiết kế (Đài Truyền hình Hậu Giang, VTV): Tin tức Mekong, Thời sự, Chào buổi sáng, Giá cả thị trường, Hoa lúa, Khuyến học khuyến tài, Chuyển động Đông Tây, Áo trắng ngời sáng tương lai, Vì nước vì dân, Vì bình yên cuộc sống.</p>',
N'',
N''
);

-- 11. Pham Hong Quach
INSERT INTO #ChuyenGiaSeed (Email, FullName, QueryString, DateOfBirth, DiaChi, Phone, HocHam, CoQuan, ChucVu, LinhVucId, DichVu, KetQuaNghienCuu, QuaTrinhDaoTao, QuaTrinhCongTac, CongBoKhoaHoc, SangChe, DuAnNghienCuu)
VALUES (
N'phquach@most.gov.vn', N'Phạm Hồng Quách', 'pham-hong-quach', N'24/06/1974',
N'Nhà B6, Lô 9, Định Công, Hoàng Mai, Hà Nội', N'0918689255',
N'', N'Viện Đánh giá khoa học và Định giá công nghệ', N'Giám đốc Trung tâm tư vấn Đánh giá khoa học và Định giá công nghệ', N'',
N'Tư vấn thẩm định giá công nghệ, đánh giá trình độ công nghệ doanh nghiệp, định giá tài sản trí tuệ và chuyển giao công nghệ.',
N'Chủ nhiệm 6 đề tài cấp Bộ/tỉnh (2017-2023) và tham gia nhiều đề tài khác, tác giả 6 bài báo khoa học (2022-2024) về thẩm định giá công nghệ.',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Trường Đại học Kinh Tế Quốc Dân Hà Nội (văn bằng 2: Đại học Ngoại Ngữ Hà Nội - Tiếng Anh, 1996)</td><td>Tài chính tiền tệ</td><td>Việt Nam</td><td>1995</td></tr>
<tr><td>Thạc sĩ</td><td>Trường Đại học Meijo, Nagoya</td><td>Quản trị kinh doanh</td><td>Nhật Bản</td><td>2010</td></tr>
<tr><td>Tiến sĩ</td><td>Trường Đại học Kinh tế - ĐHQG Hà Nội</td><td>Quản lý kinh tế</td><td>Việt Nam</td><td>2024</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Vị trí</th><th>Tổ chức</th></tr>
<tr><td>1996-1998</td><td>Phòng Kinh doanh</td><td>Công ty Đầu tư Phát triển Công nghệ (FPT)</td></tr>
<tr><td>1998-2005</td><td>Phòng Tổ chức</td><td>Công ty TNHH MTV Giầy Thượng Đình</td></tr>
<tr><td>2010-2011</td><td>Chuyên viên tài chính</td><td>Tổng Công ty CP Tài chính Dầu khí (PVFC)</td></tr>
<tr><td>2011-2015</td><td>Nghiên cứu viên</td><td>Viện Đánh giá Khoa học và Định giá Công nghệ</td></tr>
<tr><td>2016-2019</td><td>Phó Giám đốc Trung tâm Tư vấn</td><td>Viện Đánh giá Khoa học và Định giá Công nghệ</td></tr>
<tr><td>2019-nay</td><td>Giám đốc Trung tâm Tư vấn</td><td>Viện Đánh giá Khoa học và Định giá Công nghệ</td></tr></table>',
N'<p>TS. Phạm Hồng Quách đã công bố 6 bài báo khoa học (2022-2024) về thẩm định giá công nghệ, khởi nghiệp sáng tạo và logistics xuất khẩu.</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bài báo</th><th>Tạp chí</th><th>Năm</th></tr>
<tr><td>Phát triển cơ sở ươm tạo doanh nghiệp, khởi nghiệp sáng tạo, kinh nghiệm thế giới và gợi ý cho Việt Nam</td><td>Tạp chí Kinh tế dự báo</td><td>2022</td></tr>
<tr><td>Assessment of creative enterprise and business infrastructure in Vietnam</td><td>International Journal of Pandemic, Disaster and Crisis Management</td><td>2022</td></tr>
<tr><td>Thực tiễn hoạt động thẩm định giá công nghệ sử dụng Ngân sách nhà nước</td><td>Tạp chí kinh tế dự báo</td><td>2022</td></tr>
<tr><td>Dịch vụ thẩm định giá công nghệ, xác định giá trị công nghệ sử dụng NSNN</td><td>Tạp chí Công thương</td><td>2022</td></tr>
<tr><td>The efficiency of logistics activities to export: a practical study in Vietnam 2018-2021</td><td>Sustainability, MDPI</td><td>2023</td></tr>
<tr><td>Dịch vụ định giá công nghệ, kinh nghiệm quốc tế và thực tiễn tại Việt Nam</td><td>Tạp chí Công thương</td><td>2024</td></tr></table>',
N'',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Đề tài/dự án</th><th>Vai trò</th><th>Thời gian</th></tr>
<tr><td>Nghiên cứu áp dụng phương pháp phân tích chu kỳ sống của công nghệ đối với ngành năng lượng tái tạo (cấp Bộ)</td><td>Chủ trì</td><td>2017-2018</td></tr>
<tr><td>Đánh giá trình độ công nghệ doanh nghiệp sản xuất tỉnh Bình Dương (cấp tỉnh)</td><td>Chủ trì</td><td>2018-2019</td></tr>
<tr><td>Đánh giá trình độ công nghệ doanh nghiệp sản xuất tỉnh Thái Bình (cấp tỉnh)</td><td>Chủ trì</td><td>2019-2020</td></tr>
<tr><td>Xây dựng quy trình thử nghiệm dịch vụ tư vấn thẩm định giá công nghệ từ kết quả NCKH sử dụng NSNN (cấp Bộ)</td><td>Chủ trì</td><td>2021-2022</td></tr>
<tr><td>Xây dựng và phát triển công cụ dữ liệu trực tuyến về dịch vụ tư vấn đánh giá, định giá KQNC/công nghệ (cấp Bộ)</td><td>Chủ trì</td><td>2023</td></tr>
<tr><td>Đề án 844: Hỗ trợ phát triển thị trường cho doanh nghiệp khởi nghiệp đổi mới sáng tạo</td><td>Chủ trì</td><td>2022-2023</td></tr>
<tr><td colspan="3">(và nhiều đề tài cấp Nhà nước/Bộ/tỉnh đã tham gia khác, ví dụ đánh giá trình độ công nghệ tại Thanh Hóa, Tiền Giang, Thái Nguyên, Ninh Thuận, Bắc Giang)</td></tr></table>'
);

-- 12. Nguyen Duy Dat
INSERT INTO #ChuyenGiaSeed (Email, FullName, QueryString, DateOfBirth, DiaChi, Phone, HocHam, CoQuan, ChucVu, LinhVucId, DichVu, KetQuaNghienCuu, QuaTrinhDaoTao, QuaTrinhCongTac, CongBoKhoaHoc, SangChe, DuAnNghienCuu)
VALUES (
N'duydatvcu@gmail.com', N'Nguyễn Duy Đạt', 'nguyen-duy-dat', N'11/07/1981',
N'CHCC 2007, toà R2, Sunshine RiverSide, phường Phú Thượng, quận Tây Hồ, Hà Nội', N'0987331111',
N'Phó giáo sư', N'Trường Đại học Thương Mại, Khoa Kinh tế & Kinh doanh quốc tế', N'Trưởng khoa', N'',
N'Tư vấn kinh tế quốc tế, chuỗi cung ứng - chuỗi giá trị xuất khẩu, chính sách thu hút đầu tư trực tiếp nước ngoài (FDI).',
N'Chủ nhiệm/tham gia 7 đề tài (2010-2024), tác giả 38 bài báo khoa học (2006-2024, nhiều bài Q1/Q2/Q3 trên Sustainability, Journal of the Asia Pacific Economy...) và 8 đầu sách/giáo trình.',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bậc</th><th>Nơi đào tạo</th><th>Chuyên ngành</th><th>Nước</th><th>Năm</th></tr>
<tr><td>Đại học</td><td>Trường Đại học Thương Mại</td><td>Kinh tế thương mại</td><td>Việt Nam</td><td>2003</td></tr>
<tr><td>Thạc sĩ</td><td>Viện Khoa học xã hội Hà Lan</td><td>Kinh tế phát triển</td><td>Việt Nam</td><td>2007</td></tr>
<tr><td>Tiến sĩ</td><td>Trường Đại học Thương Mại</td><td>Quản lý kinh tế</td><td>Việt Nam</td><td>2017</td></tr></table>',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Thời gian</th><th>Nơi công tác</th><th>Công việc</th></tr>
<tr><td>2004-2008</td><td>Bộ môn Kinh tế phát triển/căn bản, Khoa Kinh tế, ĐH Thương Mại</td><td>Giảng viên</td></tr>
<tr><td>2008-2010</td><td>Bộ môn Kinh tế căn bản, Khoa Thương mại quốc tế, ĐH Thương Mại</td><td>Phó trưởng bộ môn</td></tr>
<tr><td>2010-T10/2016</td><td>Bộ môn Kinh tế quốc tế, Khoa Thương mại quốc tế, ĐH Thương Mại</td><td>Trưởng bộ môn</td></tr>
<tr><td>T10/2016-T11/2018</td><td>Khoa Kinh tế và Kinh doanh quốc tế, ĐH Thương Mại</td><td>Phó trưởng khoa</td></tr>
<tr><td>T11/2018-nay</td><td>Khoa Kinh tế và Kinh doanh quốc tế, ĐH Thương Mại</td><td>Trưởng khoa</td></tr></table>',
N'<p>PGS.TS Nguyễn Duy Đạt đã công bố 38 bài báo khoa học (2006-2024), trong đó có nhiều bài Q1/Q2/Q3 trên Sustainability, Journal of the Asia Pacific Economy, Environment, Development and Sustainability, cùng 8 đầu sách/giáo trình về kinh tế quốc tế và đầu tư.</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Sách/giáo trình</th><th>Nhà xuất bản</th><th>Năm</th></tr>
<tr><td>Nền kinh tế các quốc gia ASEAN (chủ biên)</td><td>NXB Tài chính</td><td>2022</td></tr>
<tr><td>Tổng quan về đầu tư trực tiếp nước ngoài (chủ biên)</td><td>NXB Tài chính</td><td>2023</td></tr>
<tr><td>Chuỗi cung ứng và chuỗi giá trị sản phẩm cà phê xuất khẩu của Việt Nam (chủ biên)</td><td>NXB Tài chính</td><td>2023</td></tr>
<tr><td>Giáo trình Kinh tế quốc tế 2 (chủ biên)</td><td>NXB Thống kê</td><td>2024</td></tr>
<tr><td>Giáo trình Đầu tư quốc tế (chủ biên)</td><td>NXB Thống kê</td><td>2024</td></tr>
<tr><td colspan="3">(và 3 đầu sách khác, tổng cộng 8 đầu sách/giáo trình)</td></tr></table>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Bài báo tiêu biểu</th><th>Tạp chí</th><th>Năm</th></tr>
<tr><td>Evaluating the Consumer Attitude and Behavioral Consumption of Green Products in Vietnam</td><td>Sustainability (Q1)</td><td>2023</td></tr>
<tr><td>Analyzing the Feasibility of Eco-Industrial Parks in Developing Countries: Thang Long II Industrial Park in Vietnam</td><td>Sustainability</td><td>2023</td></tr>
<tr><td>Asymmetric impacts of economic factors on CO2 emissions in Pakistan: evidence from the NARDL model</td><td>Environment, Development and Sustainability (Q1)</td><td>2024</td></tr>
<tr><td>Direct and spillover effects of social insurance reform: evidence from Vietnam</td><td>Journal of the Asia Pacific Economy (Q2)</td><td>2021</td></tr>
<tr><td>Labour Law reform and Labour Market outcomes in Vietnam</td><td>Asia & the Pacific Policy Studies (Q1)</td><td>2021</td></tr>
<tr><td>Factors Affecting Enterprise''s Satisfaction toward Social Security''s Online Public Service</td><td>Journal of Asian Finance, Economics and Business (Q3)</td><td>2020</td></tr>
<tr><td>Analyzing the Commercial Capacity of Agribusiness Enterprises in Dien Bien Province</td><td>International Journal of Entrepreneurship (Q3)</td><td>2019</td></tr>
<tr><td>Tác động của FDI tới giảm nghèo: minh chứng từ điều tra mức sống hộ dân cư (VHLSS)</td><td>Tạp chí khoa học Thương Mại</td><td>2016</td></tr>
<tr><td colspan="3">(và các bài báo khác, tổng cộng 38 bài 2006-2024)</td></tr></table>',
N'',
N'<table border="1" cellpadding="4" cellspacing="0"><tr><th>Đề tài/dự án</th><th>Vai trò</th><th>Thời gian</th><th>Kết quả</th></tr>
<tr><td>Vai trò của vốn đầu tư trực tiếp nước ngoài FDI tới tạo việc làm: trường hợp tỉnh Hải Dương (cấp cơ sở)</td><td>Chủ trì</td><td>2012-2013</td><td>Xuất sắc</td></tr>
<tr><td>Chuỗi cung ứng, chuỗi giá trị sản phẩm cà phê xuất khẩu của tỉnh Gia Lai (cấp tỉnh/bộ)</td><td>Chủ trì</td><td>2020-2022</td><td>Đạt</td></tr>
<tr><td>Thu nhập, việc làm của các hộ gia đình bị thu hồi đất ở ngoại thành Hà Nội (cấp bộ)</td><td>Tham gia</td><td>2010-2011</td><td>Xuất sắc</td></tr>
<tr><td>Liên kết kinh tế giữa các địa phương ven biển của Việt Nam (cấp bộ)</td><td>Tham gia</td><td>2020-2022</td><td>Đạt</td></tr>
<tr><td>Giải pháp phát triển và quản lý TMĐT ở thành phố Hà Nội (cấp tỉnh/bộ)</td><td>Tham gia</td><td>2022-2024</td><td>Đạt</td></tr>
<tr><td>Đánh giá điều kiện và đề xuất giải pháp phát triển hệ thống Logistics tỉnh Gia Lai (cấp tỉnh/bộ)</td><td>Tham gia</td><td>2022-2024</td><td>Đạt</td></tr>
<tr><td>Nghiên cứu thị trường và dịch vụ Logistic đẩy mạnh xuất khẩu trái cây tỉnh Sơn La sang Châu Âu (cấp tỉnh/bộ)</td><td>Tham gia</td><td>2022-2024</td><td>Đạt</td></tr></table>'
);

-- Merge into NhaTuVan, guarded by Email so re-running does not duplicate rows.
-- StatusId = 1 (draft / cho duyet) for ALL rows: this is real personal data pending human review.
INSERT INTO NhaTuVan
    (FullName, QueryString, Email, DateOfBirth, DiaChi, Phone, HocHam, CoQuan, ChucVu, LinhVucId,
     DichVu, KetQuaNghienCuu, QuaTrinhDaoTao, QuaTrinhCongTac, CongBoKhoaHoc, SangChe, DuAnNghienCuu,
     Domain, StatusId, LanguageId, SiteId, Created, Modified)
SELECT
    s.FullName, s.QueryString, s.Email, s.DateOfBirth, s.DiaChi, s.Phone, s.HocHam, s.CoQuan, s.ChucVu, s.LinhVucId,
    s.DichVu, s.KetQuaNghienCuu, s.QuaTrinhDaoTao, s.QuaTrinhCongTac, s.CongBoKhoaHoc, s.SangChe, s.DuAnNghienCuu,
    N'VN', 1, 1, 1, @Now, @Now
FROM #ChuyenGiaSeed s
WHERE NOT EXISTS (SELECT 1 FROM NhaTuVan n WHERE n.Email = s.Email);

DROP TABLE #ChuyenGiaSeed;
GO
