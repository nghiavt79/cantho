from copy import deepcopy
from pathlib import Path
from docx import Document
from docx.oxml import OxmlElement
from docx.shared import Pt, RGBColor

ROOT = Path('D:/2026/CanTho/docs/ho so thau')
SOURCE = ROOT / 'De_xuat_ky_thuat_Goi4_12102026.docx'
OUTPUT = ROOT / 'De_xuat_ky_thuat_Goi4_12102026_v2.docx'
doc = Document(SOURCE)


def set_text(paragraph, text):
    props = deepcopy(paragraph.runs[0]._r.rPr) if paragraph.runs and paragraph.runs[0]._r.rPr is not None else None
    paragraph.clear()
    run = paragraph.add_run(text)
    if props is not None:
        run._r.insert(0, props)


def replace(prefix, text):
    matches = [p for p in doc.paragraphs if p.text.startswith(prefix)]
    assert len(matches) == 1, (prefix, len(matches))
    set_text(matches[0], text)


def cell_text(cell, text):
    set_text(cell.paragraphs[0], text)
    for p in cell.paragraphs[1:]:
        p._element.getparent().remove(p._element)


def row_update(table_index, key, column, text):
    rows = [r for r in doc.tables[table_index].rows if r.cells[0].text == key]
    assert len(rows) == 1, (table_index, key)
    cell_text(rows[0].cells[column], text)


def add_after(prefix, text):
    anchor = next(p for p in doc.paragraphs if p.text.startswith(prefix))
    p = doc.add_paragraph(text)
    p.style = doc.styles['Normal']
    anchor._p.addnext(p._p)
    return p


replace('Sẵn sàng khai thác ngay:', 'Sẵn sàng triển khai: Nhà thầu sử dụng nền tảng hiện có để cấu hình và hoàn thiện theo phạm vi gói thầu. Dữ liệu khởi tạo được kiểm kê, đối chiếu nguồn và quyền sử dụng trước khi nhập; chỉ công bố số lượng hồ sơ công nghệ, thiết bị và chuyên gia khi có biên bản xác nhận tại PL-07. Quy trình 14 bước được kiểm thử trọn vòng và lập biên bản tại PL-06 trước khi đưa vào vận hành thử; không dùng dữ liệu mẫu để chứng minh khả năng vận hành với tài khoản dịch vụ thật.')
replace('Liên thông quốc gia:', 'Liên thông quốc gia: triển khai đồng bộ sản phẩm chào bán, tin chào mua và đăng ký gian hàng địa phương theo tài liệu API, danh mục mã và quyền truy cập được cơ quan quản lý Sàn quốc gia cấp; phạm vi hoàn thành được xác nhận bằng kiểm thử kết nối tại PL-06.')
replace('Dữ liệu thuộc Bên thuê:', 'Dữ liệu thuộc Bên thuê: toàn bộ dữ liệu được bàn giao khi kết thúc hợp đồng theo Mục 7.')
replace('Giải pháp cho 12 nhóm chức năng.', 'Giải pháp cho 12 nhóm chức năng. Nhà thầu tổ chức các mô-đun để cung cấp đủ 12 nhóm chức năng theo hợp đồng: quản trị hệ thống; nhà cung cấp, người mua, tổ chức và chuyên gia; sản phẩm, công nghệ, phòng và vị trí trưng bày; kết nối cung – cầu và quy trình chuyển giao 14 bước; sự kiện; cổng thông tin song ngữ; VR360; AI; tích hợp; quản trị dữ liệu và ATTT; hạ tầng; báo cáo thống kê. Mô tả tại Mục 4 là phương án cung cấp và cam kết thực hiện, không thay thế biên bản kiểm chứng hiện trạng. Các chức năng bổ sung phải được kiểm thử tại PL-06 trước vận hành thử.')
replace('(*) Chức năng đã có', '(*) Các hạng mục phụ thuộc tài khoản, chứng thư, cấu hình và nội dung thực địa được hoàn tất trong giai đoạn thiết lập: SMS Brandname; ký số thật; 10 điểm VR360; dịch vụ họp trực tuyến; kết nối Sàn quốc gia. Chỉ xác nhận đã có hoặc đã vận hành khi có minh chứng PL-06; chế độ mô phỏng, tài khoản thử nghiệm và dữ liệu mẫu không thay cho kiểm thử dịch vụ thật.')
add_after('Nhà thầu đáp ứng đủ 63', 'Cột “Đáp ứng” thể hiện cam kết cung cấp theo hợp đồng. Các mô tả được bổ sung theo góp ý phải được đội thực hiện xác nhận và kiểm thử; bảng này không tự xác nhận toàn bộ chức năng đã có trên môi trường vận hành.')
replace('Nhà đầu tư:', 'Nhà đầu tư: hồ sơ gồm thông tin tổ chức/cá nhân, liên hệ, lĩnh vực quan tâm, nhu cầu đầu tư và tài liệu được phép công bố; quản trị viên thêm, sửa, duyệt, ẩn và phân quyền công bố. Nhà đầu tư có thể đăng nhu cầu, mời báo giá, theo dõi giao dịch bằng tài khoản thành viên. Hồ sơ viện/trường, chuyên gia và tư vấn viên quản lý thông tin định danh, lĩnh vực, năng lực, tài liệu, trạng thái duyệt và lịch sử cập nhật; dịch vụ tư vấn có đơn vị cung cấp, mô tả, lĩnh vực và đầu mối liên hệ. Phạm vi dữ liệu công khai được Bên thuê phê duyệt.')

license_text = 'Windows Server Standard cho 02 máy chủ và SQL Server Standard cho máy chủ CSDL; quyền sử dụng hợp pháp đủ 05 năm, phù hợp số lõi/đối tượng truy cập theo mô hình cấp phép được chọn; nâng cấp trước khi hết hỗ trợ. [CẦN XÁC NHẬN: phiên bản, SKU, mô hình và số lượng giấy phép tại PL-01/PL-02 trước khi nộp HSDT].'
row_update(2, 'Bản quyền hệ điều hành máy chủ, hệ quản trị CSDL', 2, license_text)
row_update(3, 'Hạ tầng, vận hành', 2, 'Máy chủ 1: ứng dụng web và dịch vụ nền; máy chủ 2: CSDL. Kho sao lưu độc lập với hai máy chủ và dữ liệu gốc, lưu tại Việt Nam; không đặt bản sao lưu duy nhất trên máy chủ CSDL. Windows Server Standard, SQL Server Standard theo PL-01/PL-02; giám sát và mở rộng tài nguyên trong giá chào.')
row_update(3, 'Dữ liệu', 2, 'SQL Server Standard; mã định danh thống nhất; lịch sử phiên bản; xuất JSON, XML, CSV, Excel; phiên bản và cấp phép theo PL-01/PL-02.')
replace('Tài liệu chứng minh kèm theo', 'Tài liệu chứng minh được quản lý theo mã và tên file tại Mục 1.4. PL-01, PL-02, PL-03, PL-04, PL-05, PL-06 và PL-07 phải được kiểm tra, đính kèm hoặc dẫn chiếu đúng vị trí trong bộ HSDT trước khi nộp; danh mục không thay cho tài liệu thực tế. Chứng từ kích hoạt tài khoản, cấu hình môi trường thật và biên bản vận hành được bổ sung trước vận hành thử; không trì hoãn minh chứng bắt buộc của HSDT đến khi triển khai.')
table = doc.tables[1]
for child in list(table._tbl)[3:]:
    table._tbl.remove(child)
headers = ['Mã phụ lục', 'Tên file hoặc vị trí dẫn chiếu', 'Nội dung và thời điểm cung cấp']
for i, value in enumerate(headers):
    cell_text(table.rows[0].cells[i], value)
appendices = [
('PL-00', 'Đề xuất này: Mục 3–7', 'Kiến trúc, bảng đáp ứng, ma trận tích hợp, ATTT và thoát dịch vụ; nằm trong bản đề xuất.'),
('PL-01', 'PL01_Nguon_cung_va_cau_hinh_dich_vu.pdf', 'Nguồn cung, gói cloud, cấu hình, kho sao lưu, email, SMS, AI, họp, CA và hợp đồng điện tử; nộp cùng HSDT.'),
('PL-02', 'PL02_Ban_quyen_va_quyen_su_dung.pdf', 'Phiên bản, SKU, phạm vi cấp phép Standard; xác nhận quyền cung cấp/sử dụng đủ 05 năm; nộp cùng HSDT. Chứng từ kích hoạt bổ sung trước vận hành thử.'),
('PL-03', 'PL03_Dac_ta_API.json', 'OpenAPI xuất từ phiên bản được đề xuất; xác thực, schema và mã lỗi; nộp cùng HSDT.'),
('PL-04', 'PL04_Dac_ta_CSDL.pdf', 'Bảng, trường, khóa, quan hệ, từ điển dữ liệu và phương án xuất/khôi phục; nộp cùng HSDT.'),
('PL-05', 'PL05_Anh_chuc_nang_va_huong_dan.pdf', 'Ảnh có phiên bản/ngày chụp, hướng dẫn quản trị và nghiệp vụ; nộp cùng HSDT.'),
('PL-06', 'PL06_Bien_ban_kiem_thu_va_tich_hop.pdf', 'Bằng chứng hiện trạng, quy trình 14 bước, chức năng bổ sung và tích hợp; nộp bằng chứng sẵn có cùng HSDT; bổ sung kết quả môi trường thật trước vận hành thử.'),
('PL-07', 'PL07_Kiem_ke_va_quyen_su_dung_du_lieu.pdf', 'Nguồn, số lượng, quyền sử dụng, điều kiện công bố dữ liệu khởi tạo; nộp cùng HSDT.'),
('PL-08', 'PL08_Phan_hoi_22_gop_y.pdf', 'Phản hồi từng góp ý, vị trí sửa, nội dung cần xác nhận và người phụ trách; gửi cùng bản đề xuất hoàn thiện.'),
]
for values in appendices:
    cells = table.add_row().cells
    for cell, value in zip(cells, values):
        cell_text(cell, value)
add_after('1.4. Tài liệu kỹ thuật', 'Tên file PL-01 đến PL-08 là quy ước đóng gói hồ sơ; chỉ ghi nhận đã nộp khi tài liệu thực tế được đính kèm. [CẦN XÁC NHẬN: MobiFone chốt nguồn cung, thông tin cấp phép và hoàn thiện phụ lục trước khi phát hành hồ sơ].')

updates = {
'A.4.4': 'Thống kê riêng lượt nhấp banner, nhấp đường dẫn/trang sự kiện, chia sẻ qua nút chia sẻ của Sàn, lượt xem, quan tâm, nhắc lịch, đăng ký và điểm danh theo ngày/sự kiện. Ghi thời điểm, mã sự kiện, loại hành động; lượt nhấp chia sẻ không được diễn giải thành lượt chia sẻ thành công trên nền tảng ngoài. Kiểm thử tại PL-06.',
'A.5.2': 'Tra cứu theo người mua: mã đơn, ngày đặt, sản phẩm, nhà cung cấp, tổng giá trị, trạng thái thanh toán, giao hàng/hoàn thành; liên kết hồ sơ giao dịch. Tổng tiền đã chi tính theo khoản thanh toán được xác nhận, tách hủy/hoàn tiền; giỏ hàng và đăng ký sự kiện không được tính là đơn đã thanh toán. Kiểm thử tại PL-06.',
'A.8.5': 'Nhật ký đăng, sửa, duyệt, xem, tải tệp và tìm kiếm; nhật ký quy trình, hợp đồng, ký số, truy cập hồ sơ. Tra cứu theo người dùng, thời gian, chức năng; ghi từ khóa tìm kiếm trong phạm vi cần thiết, tránh dữ liệu nhạy cảm. Lưu trong suốt 05 năm thuê theo Mục 6.1; chỉ cấp quyền cán bộ được phân công, ghi vết truy cập/xuất nhật ký. Kiểm thử tại PL-06.',
'A.6.1': 'Tỷ lệ hoa hồng, hoa hồng giao dịch/phí gian hàng, sổ thu chi; báo cáo Excel/PDF và biểu đồ theo kỳ, phân quyền theo Mục 3.1; số liệu từ chứng từ đã xác nhận, tách dự kiến và thực thu.',
'A.8.1': 'Báo cáo giao dịch, doanh thu và cảnh báo giao dịch mới; Excel/PDF, biểu đồ/dashboard; nguồn hồ sơ giao dịch và chứng từ đã xác nhận; kỳ ngày/tháng/quý/năm hoặc khoảng ngày, chỉ tiêu và quyền xem theo Mục 3.1. Kiểm thử tại PL-06.',
}
for key, value in updates.items():
    row_update(6, key, 3, value)
row_update(7, 'B.2.3', 3, 'Lượt xem, lượt liên hệ và số lượt giao dịch theo sản phẩm/nhà cung cấp/khoảng thời gian; tra cứu danh sách đơn đặt hàng và hồ sơ giao dịch liên quan, tách các trạng thái hoàn thành/hủy. Quy định mã giao dịch để chống đếm trùng; tổng hợp và xuất báo cáo. Kiểm thử tại PL-06.')
row_update(8, 'C.2', 3, 'Triển khai bộ kết nối theo Mục 5.3: gian hàng địa phương, sản phẩm chào bán, tin chào mua, mã định danh, ánh xạ lĩnh vực; kiểm thử bằng tài khoản được cấp. Không xác nhận đang vận hành khi chưa có PL-06; API cho sàn khác có tài liệu và phân quyền.')

add_after('Nguyên tắc AI.', 'Báo cáo và dashboard: số liệu lấy từ đơn hàng, hồ sơ giao dịch, chứng từ thu chi, hồ sơ thành viên và nhật ký sự kiện; chỉ tiêu gồm số giao dịch theo trạng thái, doanh thu/thu chi được xác nhận, nhà cung cấp/người mua, sản phẩm và hoạt động sự kiện. Kỳ ngày, tháng, quý, năm hoặc khoảng ngày; lọc theo đơn vị/lĩnh vực và thời điểm chốt. Xuất Excel/PDF, biểu đồ và dashboard theo phân quyền; người bán/người mua chỉ xem dữ liệu của mình, cán bộ Sàn xem trong phạm vi được giao. Kiểm thử đối chiếu số liệu tại PL-06.')
add_after('3.3. Tiến độ và kế hoạch', 'Mục tiêu kiểm thử hiệu năng đề xuất, cần đội thực hiện xác nhận trước khi chốt HSDT: 200 người dùng đồng thời, thời gian phản hồi p95 không quá 3 giây cho tra cứu/danh sách thông thường, tỷ lệ lỗi dưới 1% trong 30 phút tải ổn định. Bộ dữ liệu thử tối thiểu 10.000 sản phẩm, 2.000 chuyên gia và 5.000 hồ sơ giao dịch là dữ liệu kiểm thử, không phải số hồ sơ thực tế. Kiểm thử đọc/ghi theo kịch bản tại PL-06; đo riêng AI, ký số, tải tệp và dịch vụ ngoài, không bỏ các phép đo này khỏi báo cáo. Khi chưa đạt, tối ưu truy vấn/chỉ mục, bộ nhớ đệm và mở rộng CPU/RAM/lưu trữ trong phạm vi giá chào, kiểm thử lại trước vận hành thử. [CẦN XÁC NHẬN: kỹ thuật/hạ tầng phê duyệt mục tiêu và tài nguyên tại PL-01].')

for row in doc.tables[10].rows[1:]:
    cell_text(row.cells[7], 'Nhà thầu chịu SLA tổng thể: sẵn sàng ≥ 99,5%/tháng; phản hồi ≤ 02 giờ theo Mục 9; khắc phục nghiêm trọng ≤ 08 giờ, thông thường ≤ 24 giờ. SLA riêng và phạm vi đo của đối tác dẫn chiếu PL-01; không loại trừ sự cố đối tác khỏi trách nhiệm Nhà thầu.')
replace('Nhà thầu đã có bộ kết nối đang vận hành', 'Nhà thầu triển khai bộ kết nối với Sàn giao dịch KH&CN Việt Nam theo tài liệu và quyền truy cập chính thức được cấp. Phạm vi bộ kết nối hiện có phải được chứng minh tại PL-06 trước khi ghi nhận đã vận hành; không suy ra kết nối sản xuất từ mô tả hoặc thử nghiệm trên dữ liệu mẫu.')
for row in doc.tables[11].rows[1:]:
    if row.cells[1].text.startswith('Đã có:') or row.cells[1].text.startswith('Đã có trường'):
        cell_text(row.cells[1], 'Cần kiểm chứng hiện trạng tại PL-06; chưa xác nhận vận hành thực tế trong bản đề xuất này.')
replace('Điều kiện: tài khoản kết nối', 'Điều kiện kết nối: tài khoản, tài liệu API, quyền truy cập và danh mục mã lĩnh vực do cơ quan quản lý Sàn quốc gia cung cấp. Nhà thầu lập đặc tả, phát triển bộ kết nối theo tài liệu được cấp, kiểm thử xác thực, tạo/cập nhật, chống trùng, gửi lại và đối soát; kết quả môi trường thật ghi tại PL-06. Endpoint, quyền truy cập, tần suất và giới hạn API được thống nhất với đơn vị quản lý, không tự coi JSON/XML/CSV đều là định dạng được hỗ trợ.')

row_update(12, 'Nhật ký', 1, 'Ghi đăng nhập, quản trị, tìm kiếm, quy trình, hợp đồng, ký số và truy cập hồ sơ; lưu trong suốt 05 năm thuê và bàn giao khi kết thúc. Nhật ký được lưu tập trung, phân quyền riêng, bảo vệ khỏi sửa/xóa trái phép, ghi vết việc truy cập và xuất; chỉ cán bộ được phân công được xem. Chính sách lưu nhật ký độc lập với thời hạn giữ bản sao lưu 30 ngày; rà soát dữ liệu nhạy cảm, dung lượng và nghĩa vụ lưu giữ theo hồ sơ ATTT.')
row_update(12, 'Sao lưu, phục hồi', 1, 'CSDL: sao lưu toàn phần hằng ngày, bổ sung 6 giờ/lần; tệp: hằng ngày. Giữ các điểm phục hồi ít nhất 30 ngày trên kho sao lưu riêng tại Việt Nam, độc lập với máy chủ ứng dụng, máy chủ CSDL và dữ liệu gốc; tài khoản sao lưu riêng, mã hóa, hạn chế quyền xóa. Dung lượng kho tính theo kích thước bản toàn phần, phần tăng thêm/tệp thay đổi trong 30 ngày và dự phòng 20%; mức GB, gói và vị trí được chốt tại PL-01. Kho và lưu lượng sao lưu đã nằm trong giá chào. RPO CSDL ≤ 6 giờ, tệp ≤ 24 giờ; RTO ≤ 8 giờ, đo bằng phục hồi thử trước vận hành và hằng quý. Kiểm tra tính nhất quán giữa CSDL và tệp tại điểm phục hồi; không coi bản CSDL mới nhất luôn khớp bản tệp cũ hơn.')
row_update(12, 'Bảo vệ mạng, máy chủ', 1, 'Tường lửa chỉ mở cổng cần thiết; quản trị giới hạn IP; CSDL không mở Internet. WAF phát hiện/chặn mẫu tấn công ứng dụng, giới hạn tốc độ và đăng nhập bất thường; giám sát nhật ký, chống mã độc máy chủ và phối hợp chống DDoS của cloud. Sản phẩm, gói bảo vệ, tài nguyên, hạn mức và đầu mối chốt tại PL-01; chi phí nằm trong giá chào; kiểm thử cảnh báo/chặn và ứng cứu trước vận hành thử.')
add_after('Vướng mắc phát sinh được Chủ nhiệm', 'Kiểm thử mô phỏng, thương hiệu SMS thử và USB Token không thay thế nghiệm thu chức năng SMS Brandname, ký số từ xa hoặc kết nối Sàn quốc gia theo phạm vi hợp đồng. Trước vận hành thử phải kiểm tra tài khoản thật và lập biên bản điều kiện sẵn sàng. Nếu phụ thuộc chưa được giải quyết, Nhà thầu báo ngay Bên thuê và phối hợp cơ quan/nhà cung cấp theo đầu mối đã thống nhất; không tự xác nhận đã hoàn thành, không tự gia hạn mốc 20/30 ngày hoặc thu hẹp phạm vi.')

for table in doc.tables:
    if table.rows:
        pr = table.rows[0]._tr.get_or_add_trPr()
        if pr.find('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}tblHeader') is None:
            pr.append(OxmlElement('w:tblHeader'))
    for row in table.rows:
        for cell in row.cells:
            for p in cell.paragraphs:
                for run in p.runs:
                    run.font.size = Pt(10)
for style in ['Title', 'Heading 1', 'Heading 2', 'Heading 3']:
    doc.styles[style].font.color.rgb = RGBColor(0, 0, 0)
doc.save(OUTPUT)
check = Document(OUTPUT)
text = '\n'.join(p.text for p in check.paragraphs) + '\n' + '\n'.join(c.text for t in check.tables for r in t.rows for c in r.cells)
for phrase in ['Windows Server Standard', 'SQL Server Standard', 'PL-08', 'nhấp banner', 'tổng giá trị', 'số lượt giao dịch', 'nhật ký', '200 người dùng', 'RPO CSDL', '[CẦN XÁC NHẬN']:
    assert phrase.casefold() in text.casefold(), phrase
for phrase in ['máy chủ 2: CSDL và lưu trữ sao lưu', 'có sẵn trên 4.300', 'Bộ kết nối Sàn KH&CN Việt Nam đang vận hành thực tế', 'Theo SLA nhà mạng']:
    assert phrase.casefold() not in text.casefold(), phrase
print(OUTPUT)
print('Verified replacement content; tables:', len(check.tables))
