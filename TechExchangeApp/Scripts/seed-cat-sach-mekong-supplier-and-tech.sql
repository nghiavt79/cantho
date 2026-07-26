/*
Seeds the supplier profile and technology listing submitted by Cong ty Co phan
Cong nghe Cat Sach MeKong, from the 2 forms + attachments in
CSDL Mau\Cty Cat sach Mekong_*.docx (verified against the 3 embedded legal
documents: Giay chung nhan DKKD, Giay chung nhan Doanh nghiep KH&CN so
30/DNKHCN, and its appendix listing the same 2 patents referenced below).

Inserted as StatusId = 1 (draft / cho duyet), same convention as
seed-chuyengia-sample-experts.sql: real submitted business data pending a
human reviewer before going live.

Left blank / not invented (form did not provide a usable single value):
  - NgayCapBang: the 2 patents have 2 different grant dates (17/02/2023 and
    26/12/2024) and SanPhamCNTB only has one date column. Both dates are
    written out in MoTa instead of picking one arbitrarily.
  - TRLLevel: the form's TRL checklist has 3 boxes checked at once (6, 8, 9),
    which is not a valid single value - ask the customer which one is
    correct before publishing.
  - GiaBanDuKien / ChiPhiPhatSinh / BaoHanhHoTro / Website: left blank in the
    submitted form.

Images/attachments prepared under wwwroot/uploads/2026/07/16/ (see @ImgBase):
  - logo-cat-sach-mekong.png            -> NhaCungUng.Logo (confirmed correct by customer)
  - chung-nhan-cat-sach-mekong.pdf      -> NhaCungUng.ChungNhan (DKKD + DNKHCN cert + appendix, combined into 1 PDF since the field only holds a single file URL)
  - cat-sach-mekong-quy-trinh-tuyen-rua.jpeg -> SanPhamCNTB.QuyTrinhHinhAnh (annotated equipment/process diagram)
  - video-cat-sach-mekong.mp4           -> SanPhamCNTB.URL (operation video, IsYoutube = 0)

Idempotent: guarded by MaSoThue for the supplier and by Name + ProductType
for the technology.
*/

DECLARE @Now DATETIME = GETDATE();
DECLARE @ImgBase NVARCHAR(200) = N'https://localhost:7232/Uploads/2026/07/16/';

-- =====================================================================
-- 1. Supplier: Cong ty Co phan Cong nghe Cat Sach MeKong
-- =====================================================================
IF NOT EXISTS (SELECT 1 FROM NhaCungUng WHERE MaSoThue = N'1801742453')
BEGIN
    INSERT INTO NhaCungUng
        (FullName, QueryString, DiaChi, Phone, Email, Website, NguoiDaiDien, ChucVu,
         LinhVucId, LoaiHinhToChuc, MaSoThue, ChucNangChinh, SanPham,
         Logo, ChungNhan,
         StatusId, IsActivated, LanguageId, SiteId, Domain, Created, Modified, CreatedBy)
    VALUES
        (N'Công ty Cổ phần Công nghệ Cát Sạch MeKong', 'cong-ty-cat-sach-mekong',
         N'Số 01, Đường B30, KDC Hưng Phú, Khu vực 8, Phường Hưng Phú, TP. Cần Thơ',
         '0939803803', 'catdasachct@gmail.com', NULL,
         N'Mai Phương Trang', N'Giám đốc',
         N';5;', N'DNKHCN', N'1801742453',
         N'Nghiên cứu chế tạo ứng dụng công nghệ tuyển rửa cát biển, cát đồi núi, cát sông suối, bùn cát nạo vét từ luồng lạch cửa sông cửa biển, cát tận thu từ ruộng vườn còn lẫn cát để cải tạo đất nông nghiệp những vùng hoang mạc,... theo quy mô công nghiệp. Sử dụng hợp lý nguồn tài nguyên cát: khắc phục tình trạng khan hiếm cát và cát bẩn gây hút ẩm thấm, khó quy về tiêu chuẩn module để cấp phối bê tông. Phấn đấu trở thành doanh nghiệp uy tín, hàng đầu cung cấp giải pháp thiết bị tuyển rửa cốt liệu nhỏ tốt nhất và hiệu quả nhất tại Việt Nam.',
         N'Dịch vụ sàng rửa và phân loại cát cho các đơn vị có nhu cầu. Sản phẩm công nghệ/kết quả nghiên cứu chủ lực: Hệ thống và phương pháp sàng rửa, phân loại cát nhiễm mặn; hệ thống thiết bị, dây chuyền tuyển rửa cát; quy trình tuyển rửa cốt liệu nhỏ dùng cho xây dựng; sản phẩm cát sau tuyển rửa, xử lý nâng cao chất lượng được tạo ra từ việc ứng dụng công nghệ.',
         @ImgBase + N'logo-cat-sach-mekong.png', @ImgBase + N'chung-nhan-cat-sach-mekong.pdf',
         1, 1, 1, 1, N'VN', @Now, @Now, N'catsach-mekong-import');
END

DECLARE @NCUId INT = (SELECT TOP 1 CungUngId FROM NhaCungUng WHERE MaSoThue = N'1801742453');

-- =====================================================================
-- 2. Technology: He thong / cong nghe tuyen rua va xu ly cat sach
-- =====================================================================
IF NOT EXISTS (SELECT 1 FROM SanPhamCNTB WHERE Name = N'Hệ thống và công nghệ tuyển rửa, xử lý cát sạch (cát biển, cát sông, cát nhiễm mặn)' AND ProductType = 1)
BEGIN
    DECLARE @NextId INT = ISNULL((SELECT MAX(ID) FROM SanPhamCNTB WHERE ProductType = 1), 0) + 1;

    INSERT INTO SanPhamCNTB
        (Code, Name, QueryString, ProductType, CategoryId, XuatXuId, MucDoId,
         OwnerType, NCUId,
         SoBang, MoTa, ThongSo, UuDiem, TargetCustomer, TransferMethod,
         ChungNhanISO, ChungNhanQuatest,
         QuyTrinhHinhAnh, URL, IsYoutube,
         StatusId, LanguageId, SiteId, Created, Modified, Creator)
    VALUES
        ('CN-' + RIGHT('00000' + CAST(@NextId AS VARCHAR(5)), 5),
         N'Hệ thống và công nghệ tuyển rửa, xử lý cát sạch (cát biển, cát sông, cát nhiễm mặn)',
         'he-thong-cong-nghe-tuyen-rua-xu-ly-cat-sach',
         1, N'5', 1, 2,
         1, @NCUId,
         N'35015; 42793',
         N'<p>Gồm 2 sáng chế đã được cấp bằng độc quyền:</p>
<table border="1" cellpadding="4" cellspacing="0"><tr><th>Tên sáng chế</th><th>Số bằng</th><th>Số quyết định</th><th>Ngày cấp</th></tr>
<tr><td>Hệ thống và phương pháp sàng rửa và phân loại cát nhiễm mặn</td><td>35015</td><td>1907w/QĐ-SHTT</td><td>17/02/2023</td></tr>
<tr><td>Quy trình tuyển rửa cốt liệu nhỏ dùng cho xây dựng</td><td>42793</td><td>154666/QĐ-SHTT</td><td>26/12/2024</td></tr></table>
<p><b>Bối cảnh & vấn đề giải quyết:</b> Đầu tư xây dựng trạm tuyển rửa và xử lý cát biển nhằm sản xuất cát sạch đạt yêu cầu kỹ thuật để làm vật liệu cho bê tông, vữa xây dựng, nền đường giao thông và san lấp mặt bằng, thay thế nguồn cát sông đang ngày càng khan hiếm. Việt Nam có nguồn tài nguyên cát biển ước tính khoảng 195 tỷ m³, nhưng cát biển tự nhiên thường chứa bùn, bụi, sét, tạp chất hữu cơ, muối hòa tan và ion Clo nên phải qua tuyển rửa mới đạt chuẩn TCVN 13754:2023, TCCS 49:2025/CĐBVN, TCVN 9436:2012. Công nghệ cũng tận dụng được nguồn cát từ hoạt động nạo vét luồng lạch, cửa sông cửa biển, giúp giảm chi phí nạo vét - đổ thải và bổ sung nguồn cung vật liệu xây dựng.</p>
<p><b>Hiệu quả kinh tế - xã hội:</b> Giảm lượng xi măng để phối trộn từ 10%-17% (Đề tài NCKH Trường Đại học Cần Thơ, 2014); cường độ bê tông tăng 10%-20% (Tài liệu hướng dẫn thí nghiệm viên Quatest 3, 2007); kiểm soát ion Cl⁻ đạt chuẩn dùng cho đắp đường do Bộ GTVT quy định (<0,01% cho bê tông dự ứng lực, <0,05% cho bê tông thông thường).</p>',
         N'<p><b>THÔNG SỐ KỸ THUẬT & NĂNG LỰC HỆ THỐNG TUYỂN RỬA CÁT BIỂN</b></p>
<table border="1" cellpadding="4" cellspacing="0">
<tr><th>Hạng mục</th><th>Thông số</th></tr>
<tr><td>Công suất thiết kế</td><td>150 m³/giờ (cát nguyên liệu, vận hành 9 tháng/năm); có thể thiết kế đến 1.000 m³/giờ theo nhu cầu</td></tr>
<tr><td>Công suất sản xuất</td><td>1 ca/ngày: 259.200 m³/năm — 2 ca/ngày: 518.400 m³/năm</td></tr>
<tr><td>Điện tiêu thụ</td><td>Tổng công suất 100 HP (đề xuất trạm điện ≥ 150 KVA)</td></tr>
<tr><td>Nhu cầu nước ngọt</td><td>4 m³ nước / 1 m³ cát nguyên liệu (nguồn nước mặt sông, kênh rạch)</td></tr>
<tr><td>Mặt bằng lắp đặt + bãi chứa thành phẩm</td><td>≥ 0,8 ha</td></tr>
<tr><td>Hồ lắng nước & tạp chất</td><td>≥ 1,5 ha</td></tr>
<tr><td>Nước thải cần xử lý</td><td>4.800 m³/ca (8 giờ) — tổng 9.600 m³/ngày (2 ca), xử lý qua hồ lắng trước khi xả thải</td></tr>
<tr><td>Chất lượng sau xử lý</td><td>Tạp chất hữu cơ < 1%, ion Clo (Cl⁻) < 0,01%</td></tr>
<tr><td>Nguyên liệu đầu vào</td><td>Cát đồi, cát núi, cát sông, cát suối, cát biển, cát nhiễm mặn</td></tr>
<tr><td>Tiêu chuẩn áp dụng</td><td>TCVN 13754:2023 (cát nhiễm mặn cho bê tông, vữa); TCCS 49:2025/CĐBVN (nền đường ô tô dùng cát biển); TCVN 9436:2012 (cát dùng cho san lấp)</td></tr>
</table>',
         N'<ul>
<li>Hệ thống thiết bị tuyển rửa cát đã đạt giải thưởng khoa học trong nước và quốc tế; được Thủ tướng Chính phủ tặng Bằng khen vì đóng góp trong nghiên cứu, phát triển công nghệ ứng dụng vào thực tiễn sản xuất.</li>
<li>Sản xuất theo quy mô công nghiệp, công suất 100-500 m³/giờ/thiết bị, có thể nâng đến 2.000 m³/giờ/thiết bị khi cần — đáp ứng khai thác quy mô lớn tại khu công nghiệp, cảng biển, mỏ cát, dự án trọng điểm.</li>
<li>Sản xuất trong nước, giá thành hợp lý hơn thiết bị nhập khẩu, tối ưu chi phí đầu tư mà vẫn đảm bảo chất lượng.</li>
<li>Tự động hóa cao, dễ vận hành, không dùng hóa chất trong quá trình rửa cát (chỉ dùng nước ngọt tuần hoàn qua nhiều hầm lắng lọc trước khi thải ra môi trường) — giải pháp thân thiện môi trường, phù hợp định hướng kinh tế xanh, bền vững.</li>
</ul>',
         N'Các tổ chức và cá nhân khai thác, kinh doanh cát có nhu cầu nhận chuyển giao công nghệ hoặc nhận chuyển nhượng quyền sử dụng công nghệ, hoặc có nhu cầu cung cấp dịch vụ tuyển rửa các loại cát tự nhiên/cát nghiền thành cốt liệu nhỏ (cát sạch dùng cho bê tông, vữa xây tô, san lấp), hoặc đối tác có nhu cầu lắp đặt, chuyển giao hệ thống thiết bị.',
         N'Chuyển giao quyền sử dụng (Li-xăng),Góp vốn bằng công nghệ',
         0, 1,
         @ImgBase + N'cat-sach-mekong-quy-trinh-tuyen-rua.jpeg',
         @ImgBase + N'video-cat-sach-mekong.mp4', 0,
         1, 1, 1, @Now, @Now, N'catsach-mekong-import');
END

SELECT CungUngId, FullName, MaSoThue, StatusId FROM NhaCungUng WHERE MaSoThue = N'1801742453';
SELECT ID, Code, Name, NCUId, StatusId FROM SanPhamCNTB WHERE Name = N'Hệ thống và công nghệ tuyển rửa, xử lý cát sạch (cát biển, cát sông, cát nhiễm mặn)' AND ProductType = 1;

GO
