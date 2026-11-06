import sys, os
from docx import Document
from docx.shared import Pt, Cm
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml.ns import qn
from docx.oxml import OxmlElement

OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)

BEN_THUE = 'Trung tâm Ứng dụng tiến bộ khoa học và công nghệ thành phố Cần Thơ'
NHA_THAU = 'Tổng Công ty Viễn thông MobiFone'
GOI = 'Gói thầu số 4: Thuê dịch vụ thiết lập, vận hành trang web Sàn giao dịch công nghệ Cần Thơ'
ST = 'Chưa hoàn tất'
C, J, L = WD_ALIGN_PARAGRAPH.CENTER, WD_ALIGN_PARAGRAPH.JUSTIFY, WD_ALIGN_PARAGRAPH.LEFT


class Doc:
    def __init__(self):
        d = self.d = Document()
        s = d.sections[0]
        s.page_width, s.page_height = Cm(21), Cm(29.7)
        s.left_margin, s.right_margin, s.top_margin, s.bottom_margin = Cm(2.5), Cm(1.5), Cm(2), Cm(2)
        st = d.styles['Normal']
        st.font.name = 'Times New Roman'; st.font.size = Pt(13)
        st.element.rPr.rFonts.set(qn('w:eastAsia'), 'Times New Roman')
        st.paragraph_format.space_after = Pt(4); st.paragraph_format.line_spacing = 1.15

    def p(self, text='', bold=False, italic=False, align=J, size=None, indent=False, after=None, before=None):
        para = self.d.add_paragraph(); para.alignment = align
        if indent: para.paragraph_format.first_line_indent = Cm(1)
        if after is not None: para.paragraph_format.space_after = Pt(after)
        if before is not None: para.paragraph_format.space_before = Pt(before)
        r = para.add_run(text); r.bold, r.italic = bold, italic
        if size: r.font.size = Pt(size)
        return para

    def h(self, text):
        return self.p(text, bold=True, before=6)

    def bullets(self, items):
        for x in items: self.p('- ' + x, indent=True)

    def _shade(self, cell, color):
        tcPr = cell._tc.get_or_add_tcPr(); s = OxmlElement('w:shd')
        s.set(qn('w:val'), 'clear'); s.set(qn('w:color'), 'auto'); s.set(qn('w:fill'), color); tcPr.append(s)

    def table(self, headers, rows, widths, center_cols=(0,), bold_rows=(), size=11.5, border=True):
        t = self.d.add_table(rows=1 if headers else 0, cols=len(widths))
        if border: t.style = 'Table Grid'
        t.alignment = WD_TABLE_ALIGNMENT.CENTER
        if headers:
            for i, x in enumerate(headers):
                c = t.rows[0].cells[i]; c.text = ''
                r = c.paragraphs[0].add_run(x); r.bold = True; r.font.size = Pt(size)
                c.paragraphs[0].alignment = C; self._shade(c, 'D9E2F3')
        for ri, row in enumerate(rows):
            cells = t.add_row().cells
            for i, x in enumerate(row):
                cells[i].text = ''
                para = cells[i].paragraphs[0]; para.paragraph_format.space_after = Pt(0)
                r = para.add_run(str(x)); r.font.size = Pt(size); r.bold = ri in bold_rows
                if i in center_cols or (headers and headers[i] == 'Trạng thái'): para.alignment = C
        for row in t.rows:
            for i, w in enumerate(widths): row.cells[i].width = Cm(w)
        self.d.add_paragraph().paragraph_format.space_after = Pt(0)
        return t

    def header(self, title, sub=None, so=True):
        t = self.d.add_table(rows=1, cols=2); t.alignment = WD_TABLE_ALIGNMENT.CENTER
        l, r = t.rows[0].cells
        l.width, r.width = Cm(7), Cm(10)
        for cell, lines in ((l, [('', False)]), (r, [('CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM', True), ('Độc lập - Tự do - Hạnh phúc', True)])):
            cell.text = ''
            for k, (txt, b) in enumerate(lines):
                para = cell.paragraphs[0] if k == 0 else cell.add_paragraph()
                para.alignment = C; para.paragraph_format.space_after = Pt(0)
                run = para.add_run(txt); run.bold = b; run.font.size = Pt(12.5)
        if so:
            l.paragraphs[0].add_run('Số: ……/BB-SGDCN').font.size = Pt(12.5)
        self.p('Cần Thơ, ngày …… tháng …… năm 2026', italic=True, align=WD_ALIGN_PARAGRAPH.RIGHT, before=6)
        self.p(title, bold=True, align=C, size=14, after=0, before=6)
        if sub: self.p(sub, bold=True, align=C, after=0)
        self.p(GOI, italic=True, align=C, after=8)

    def can_cu(self, extra=()):
        self.p('Căn cứ Hợp đồng số ……/……… ngày ……/……/2026 giữa ' + BEN_THUE + ' và ' + NHA_THAU + ';', italic=True, indent=True)
        self.p('Căn cứ Chương V – Yêu cầu về kỹ thuật của Hồ sơ mời thầu;', italic=True, indent=True)
        for x in extra: self.p(x, italic=True, indent=True)

    def thanh_phan(self, time_place=True, vtisc=True):
        if time_place:
            self.p('Hôm nay, ngày …… tháng …… năm 2026, tại ……………………………………, chúng tôi gồm:', indent=True)
        self.p('1. Bên thuê (Chủ trì thuê): ' + BEN_THUE, bold=True)
        self.bullets(['Ông/Bà: ………………………… Chức vụ: …………………………', 'Ông/Bà: ………………………… Chức vụ: …………………………'])
        self.p('2. Nhà thầu: ' + NHA_THAU, bold=True)
        self.bullets(['Ông/Bà: ………………………… Chức vụ: …………………………', 'Ông/Bà: ………………………… Chức vụ: …………………………'])
        if vtisc:
            self.p('3. Đơn vị phối hợp kỹ thuật: VTISC', bold=True)
            self.bullets(['Ông/Bà: ………………………… Chức vụ: …………………………'])

    def ket_luan(self, text, note=None):
        self.h('Kết luận')
        self.p(text, indent=True)
        if note: self.p(note, indent=True)
        self.p('Biên bản được lập thành …… bản có giá trị như nhau, mỗi bên giữ …… bản.', indent=True)

    def ky(self, parties=('ĐẠI DIỆN BÊN THUÊ', 'ĐẠI DIỆN NHÀ THẦU')):
        t = self.d.add_table(rows=1, cols=len(parties)); t.alignment = WD_TABLE_ALIGNMENT.CENTER
        for i, x in enumerate(parties):
            c = t.rows[0].cells[i]; c.text = ''
            pa = c.paragraphs[0]; pa.alignment = C
            pa.add_run(x).bold = True
            pb = c.add_paragraph(); pb.alignment = C
            pb.add_run('(Ký, ghi rõ họ tên, đóng dấu)').italic = True

    def save(self, name):
        cp = self.d.core_properties
        cp.author = 'NghiaVT'; cp.last_modified_by = 'NghiaVT'
        cp.title = name.replace('_', ' ').replace('.docx', '')
        self.d.save(os.path.join(OUT, name)); print('ok', name)


def status_rows(items, cols_blank=1):
    return [(str(i + 1),) + tuple(x if isinstance(x, tuple) else (x,)) + ('',) * cols_blank + (ST,) for i, x in enumerate(items)]


# 01 ------------------------------------------------------------------
D = Doc(); D.header('BIÊN BẢN HỌP', 'Thống nhất kế hoạch triển khai, vận hành thử')
D.can_cu(); D.thanh_phan()
D.h('I. Nội dung đã thống nhất')
D.p('1. Kế hoạch: thống nhất Kế hoạch triển khai, vận hành thử tháng 10/2026; mục tiêu hoàn thành 85% khối lượng trước ngày 30/10/2026 để thanh toán lần 1.', indent=True)
D.p('2. Ngày bàn giao 02 máy chủ cloud và chứng từ bản quyền Windows Server Standard, SQL Server Standard: ……/10/2026 (chậm nhất 24/10/2026).', indent=True)
D.p('3. Đầu mối các bên:', indent=True)
D.table(['Bên', 'Họ tên', 'Chức vụ', 'Điện thoại', 'Email'],
        [('Bên thuê', '', '', '', ''), ('Nhà thầu', '', '', '', ''), ('VTISC', '', '', '', '')], [2.6, 4, 3.4, 2.8, 4.2])
D.p('4. Việc các bên cần cung cấp, thực hiện:', indent=True)
D.table(['TT', 'Nội dung', 'Bên thực hiện', 'Thời hạn', 'Trạng thái'], [
    ('1', 'Quyền cấu hình tên miền', 'Bên thuê', '20/10', ST),
    ('2', 'Hồ sơ pháp lý, giấy ủy quyền đăng ký thương hiệu SMS', 'Bên thuê', '20/10', ST),
    ('3', 'Ký, gửi văn bản đề nghị Bộ KH&CN cấp tài khoản kết nối (Nhà thầu soạn dự thảo)', 'Bên thuê', '21/10', ST),
    ('4', 'Thống nhất danh sách 10 điểm cảnh VR360; quay dựng thực hiện tháng 11/2026 (ngày bắt đầu: ……/11/2026)', 'Bên thuê, Nhà thầu', '24/10', ST),
    ('5', 'Danh sách cán bộ tham gia vận hành thử, học viên đào tạo', 'Bên thuê', '22/10', ST),
    ('6', 'Bàn giao 02 máy chủ cloud, chứng từ bản quyền', 'Nhà thầu', '24/10', ST),
    ('7', 'Cấp tài khoản thư điện tử, SMS, ký số, hợp đồng điện tử', 'Nhà thầu', '21/10', ST),
], [0.9, 8.2, 2.6, 2.3, 3])
D.h('II. Ý kiến khác'); D.p('…………………………………………………………………………………………………………………')
D.p('Cuộc họp kết thúc lúc …… giờ …… cùng ngày.', indent=True)
D.ky(); D.save('01_Bien_ban_hop_thong_nhat_ke_hoach.docx')

# 02 ------------------------------------------------------------------
D = Doc()
t = D.d.add_table(rows=1, cols=2); t.alignment = WD_TABLE_ALIGNMENT.CENTER
for cell, lines in ((t.rows[0].cells[0], [('SỞ KHOA HỌC VÀ CÔNG NGHỆ', False), ('TRUNG TÂM ỨNG DỤNG TIẾN BỘ', True), ('KHOA HỌC VÀ CÔNG NGHỆ', True), ('Số: ……/TTUD-……', False), ('V/v đề nghị cấp tài khoản kết nối với Sàn giao dịch KH&CN Việt Nam', False)]),
                    (t.rows[0].cells[1], [('CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM', True), ('Độc lập - Tự do - Hạnh phúc', True), ('', False), ('Cần Thơ, ngày …… tháng 10 năm 2026', False)])):
    cell.text = ''
    for k, (txt, b) in enumerate(lines):
        para = cell.paragraphs[0] if k == 0 else cell.add_paragraph()
        para.alignment = C; para.paragraph_format.space_after = Pt(0)
        r = para.add_run(txt); r.bold = b; r.font.size = Pt(12); r.italic = txt.startswith('Cần Thơ')
t.rows[0].cells[0].width, t.rows[0].cells[1].width = Cm(7.5), Cm(9.5)
D.p('', after=0)
D.p('Kính gửi: Bộ Khoa học và Công nghệ (đơn vị quản lý Sàn giao dịch khoa học và công nghệ Việt Nam).', align=C, before=6)
D.p('Thực hiện hướng dẫn của Bộ Khoa học và Công nghệ về kết nối, liên thông dữ liệu giữa các sàn giao dịch công nghệ địa phương với Sàn giao dịch khoa học và công nghệ Việt Nam, '
    + BEN_THUE + ' đang triển khai thuê dịch vụ thiết lập, vận hành trang web Sàn giao dịch công nghệ Cần Thơ.', indent=True)
D.p('Để hoàn thành kết nối trong giai đoạn thiết lập, Trung tâm kính đề nghị Bộ Khoa học và Công nghệ:', indent=True)
D.bullets(['Cấp tài khoản và mã xác thực kết nối cho Sàn giao dịch công nghệ Cần Thơ;',
           'Cung cấp tài liệu kỹ thuật giao diện kết nối (đăng ký gian hàng địa phương, gửi danh sách sản phẩm chào bán, tin chào mua);',
           'Cung cấp danh mục mã lĩnh vực công nghệ dùng chung để ánh xạ dữ liệu.'])
D.p('Đầu mối liên hệ: Ông/Bà ………………………, chức vụ ………………, điện thoại ………………, email ……………… (Bên thuê); '
    'đầu mối kỹ thuật: Ông/Bà ………………………, điện thoại ………………, email ……………… (Nhà thầu).', indent=True)
D.p('Trân trọng./.', indent=True)
t = D.d.add_table(rows=1, cols=2)
a, b = t.rows[0].cells
a.text = ''; ra = a.paragraphs[0].add_run('Nơi nhận:\n- Như trên;\n- Lưu: VT, ……'); ra.font.size = Pt(11); ra.italic = True
b.text = ''; pb = b.paragraphs[0]; pb.alignment = C; pb.add_run('GIÁM ĐỐC').bold = True
D.save('02_Du_thao_Cong_van_de_nghi_Bo_KHCN_cap_tai_khoan_ket_noi.docx')

# 03 ------------------------------------------------------------------
D = Doc(); D.header('BIÊN BẢN BÀN GIAO', 'Hạ tầng máy chủ và bản quyền phần mềm')
D.can_cu(); D.thanh_phan()
D.h('I. Máy chủ cloud (02 máy chủ)')
D.table(['TT', 'Tiêu chí theo HSMT', 'Máy chủ 1', 'Máy chủ 2', 'Trạng thái'], [
    ('1', 'CPU Intel Xeon Gold 6330 hoặc tương đương', '', '', ST),
    ('2', 'Tối thiểu 8 lõi', '', '', ST),
    ('3', 'Lưu trữ tối thiểu 500 GB SSD', '', '', ST),
    ('4', 'RAM tối thiểu 32 GB', '', '', ST),
    ('5', '01 địa chỉ IP; hỗ trợ IPv4/IPv6', '', '', ST),
    ('6', 'Sao lưu, snapshot; giám sát; mở rộng tài nguyên; kiểm soát truy cập; báo cáo tài nguyên', '', '', ST),
    ('7', 'Dữ liệu và bản sao lưu lưu trữ tại Việt Nam', '', '', ST),
], [0.9, 7.2, 2.9, 2.9, 3.1])
D.h('II. Bản quyền phần mềm')
D.table(['TT', 'Phần mềm', 'Số lượng gói', 'Số chứng từ / mã kích hoạt', 'Trạng thái'], [
    ('1', 'Windows Server 2022 Standard (gói 16 lõi)', '02', '', ST),
    ('2', 'SQL Server 2022 Standard (gói 02 lõi)', '04', '', ST),
], [0.9, 6.4, 2.4, 4.2, 3.1])
D.p('Giấy phép đứng tên Chủ trì thuê, sử dụng trong suốt thời gian thuê và được bàn giao lại cho Chủ trì thuê khi kết thúc hợp đồng.', italic=True, indent=True)
D.h('III. Bảo vệ, truy cập')
D.table(['TT', 'Nội dung', 'Kết quả', 'Trạng thái'], status_rows([
    'Tường lửa chỉ mở các cổng cần thiết; cơ sở dữ liệu không mở ra Internet',
    'Chứng thư bảo mật (SSL) cho tên miền chính thức',
    'Tên miền trỏ về máy chủ chính thức',
    'Tài khoản quản trị máy chủ bàn giao cho đầu mối được chỉ định']), [0.9, 9, 4, 3.1])
D.ket_luan('Hạ tầng máy chủ và bản quyền phần mềm đã được bàn giao đủ / chưa đủ (gạch bỏ phần không phù hợp) theo yêu cầu tại Chương V Hồ sơ mời thầu.')
D.ky(); D.save('03_Bien_ban_ban_giao_ha_tang_va_ban_quyen.docx')

# 04 ------------------------------------------------------------------
D = Doc(); D.header('BIÊN BẢN BÀN GIAO', 'Nội dung tham quan thực tế ảo VR360 (thực hiện tháng 11/2026)')
D.can_cu(); D.thanh_phan()
D.h('I. Danh sách 10 điểm cảnh quay dựng mới')
D.table(['TT', 'Tên điểm cảnh', 'Vị trí tại Sàn', 'Ngày chụp', 'Trạng thái'],
        [(str(i), '', '', '', ST) for i in range(1, 11)], [0.9, 5.5, 5, 2.5, 3.1])
D.h('II. Kiểm tra chất lượng')
D.table(['TT', 'Tiêu chí', 'Kết quả', 'Trạng thái'], status_rows([
    'Xoay 360°, chuyển cảnh giữa các điểm',
    'Chạy trên trình duyệt máy tính và điện thoại, không cần cài phần mềm',
    'Có nút “Tham quan VR360” trên giao diện Sàn',
    'Bàn giao nội dung gốc (ảnh toàn cảnh, tệp tour) cho Chủ trì thuê']), [0.9, 9, 4, 3.1])
D.ket_luan('Nội dung VR360 gồm …… /10 điểm cảnh quay đã hoàn thành và được tích hợp trên Sàn.')
D.ky(); D.save('04_Bien_ban_ban_giao_noi_dung_VR360.docx')

# 05 ------------------------------------------------------------------
D = Doc(); D.header('BIÊN BẢN XÁC NHẬN', 'Cài đặt, cấu hình và dữ liệu khởi tạo')
D.can_cu(); D.thanh_phan()
D.h('I. Cài đặt ứng dụng')
D.table(['TT', 'Nội dung', 'Kết quả', 'Trạng thái'], status_rows([
    'Ứng dụng cài đặt trên máy chủ cloud chính thức',
    'Địa chỉ truy cập chính thức (tên miền, kết nối bảo mật)',
    'Giao diện tiếng Việt và tiếng Anh hoạt động',
    'Trang quản trị hoạt động; bật xác thực hai lớp cho tài khoản quản trị']), [0.9, 9, 4, 3.1])
D.h('II. Dữ liệu khởi tạo')
D.table(['TT', 'Loại dữ liệu', 'Số lượng theo kiểm kê', 'Số lượng sau chuyển', 'Trạng thái'], [
    ('1', 'Hồ sơ công nghệ, thiết bị', '', '', ST),
    ('2', 'Chuyên gia', '', '', ST),
    ('3', 'Nhà cung ứng', '', '', ST),
    ('4', 'Tin tức, sự kiện, tài liệu hướng dẫn', '', '', ST),
], [0.9, 6.2, 3, 3.8, 3.1])
D.p('Dữ liệu được đối chiếu nguồn và quyền sử dụng trước khi nhập; số lượng tại biên bản này là số liệu chính thức.', italic=True, indent=True)
D.h('III. Cấu hình vận hành')
D.table(['TT', 'Nội dung', 'Kết quả', 'Trạng thái'], status_rows([
    'Danh mục dùng chung: lĩnh vực công nghệ, nhóm sản phẩm, phòng và vị trí trưng bày',
    'Tham số hệ thống; mẫu thư thông báo',
    'Tài khoản quản trị, vai trò và phân quyền cho cán bộ Bên thuê',
    'Sao lưu tự động hằng ngày, lưu tối thiểu 30 ngày, bản sao lưu tách biệt',
    'Giám sát: tài nguyên máy chủ, tình trạng website, kết quả sao lưu, hạn chứng thư bảo mật; cảnh báo khi có sự cố']), [0.9, 9, 4, 3.1])
D.ket_luan('Môi trường dịch vụ được thiết lập hoàn chỉnh; tài khoản, phân quyền, cấu hình vận hành và dữ liệu khởi tạo đã được thiết lập.')
D.ky(); D.save('05_Bien_ban_cai_dat_cau_hinh_du_lieu_khoi_tao.docx')

# 06 ------------------------------------------------------------------
D = Doc(); D.header('BIÊN BẢN KIỂM TRA', 'Kết nối các dịch vụ tích hợp')
D.can_cu(); D.thanh_phan()
D.table(['TT', 'Dịch vụ', 'Nội dung kiểm tra', 'Kết quả', 'Trạng thái'], [
    ('1', 'Thư điện tử', '60 hộp thư theo tên miền, 25 GB, 05 nhóm thư; hệ thống tự gửi thư, ghi trạng thái, tự gửi lại khi lỗi', '', ST),
    ('2', 'SMS Brandname', 'Gửi mã xác thực, thông báo; ghi trạng thái chờ gửi/đã gửi/lỗi; tự gửi lại; bật/tắt kênh. Ghi rõ: thương hiệu tạm / chính thức', '', ST),
    ('3', 'Chữ ký số', 'Tạo hợp đồng → gửi yêu cầu ký → các bên ký → nhúng chữ ký vào tệp PDF → kiểm tra toàn vẹn → tải tệp đã ký → tra cứu nhật ký. Ghi rõ: USB Token / ký từ xa', '', ST),
    ('4', 'Hợp đồng điện tử', 'Khởi tạo → dự thảo → gửi các bên → ký → hoàn thành → tra cứu → lưu trữ → kiểm tra nhật ký', '', ST),
    ('5', 'Trợ lý AI', 'Hỏi đáp dựa trên dữ liệu Sàn; tìm kiếm thông minh, bằng giọng nói; gợi ý nhà cung cấp; bật/tắt; tự chuyển về tìm kiếm thông thường khi gián đoạn', '', ST),
    ('6', 'VR360', 'Nút “Tham quan VR360” mở được tour trên máy tính, điện thoại (kiểm tra trong tháng 11/2026, sau khi quay dựng)', '', ST),
    ('7', 'Sàn KH&CN Việt Nam', 'Lấy mã xác thực, đăng ký gian hàng, gửi sản phẩm chào bán, tin chào mua (khi Bộ KH&CN cấp tài khoản)', '', ST),
    ('8', 'Công cụ kết nối mở', 'Tài liệu đặc tả kết nối; xác thực; nhật ký trao đổi', '', ST),
], [0.9, 2.7, 8, 2.4, 3])
D.ket_luan('Các kết nối đạt yêu cầu: ……/8. Các kết nối chưa hoàn tất và thời hạn hoàn thành: ……………………………………')
D.ky(); D.save('06_Bien_ban_kiem_tra_ket_noi_dich_vu_tich_hop.docx')

# 07 ------------------------------------------------------------------
D = Doc(); D.header('BÁO CÁO', 'Kết quả kiểm thử nội bộ và rà soát an toàn thông tin', so=False)
D.p('Đơn vị lập: ' + NHA_THAU + ' – VTISC', italic=True, align=C)
D.h('I. Kiểm thử 12 nhóm chức năng')
grp = ['Quản trị hệ thống', 'Quản lý tổ chức, doanh nghiệp và thành viên', 'Quản lý công nghệ, thiết bị và sản phẩm KH&CN',
       'Quản lý kết nối cung – cầu công nghệ', 'Quản lý sự kiện, hội thảo và triển lãm', 'Cổng thông tin điện tử',
       'Không gian triển lãm số (VR360)', 'Ứng dụng AI', 'Tích hợp và chia sẻ dữ liệu', 'Quản trị dữ liệu và an toàn thông tin',
       'Hạ tầng và vận hành dịch vụ', 'Báo cáo và thống kê']
D.table(['TT', 'Nhóm chức năng', 'Số trường hợp kiểm thử', 'Số lỗi / đã sửa', 'Trạng thái'],
        [(str(i + 1), g, '', '', ST) for i, g in enumerate(grp)], [0.9, 7.6, 2.8, 2.6, 3.1])
D.h('II. Rà soát an toàn thông tin')
D.table(['TT', 'Nội dung', 'Kết quả', 'Trạng thái'], status_rows([
    'Rà soát lỗ hổng: số lỗ hổng phát hiện ……; đã khắc phục ……',
    'Kiểm tra lại sau khắc phục',
    'Cấu hình kết nối bảo mật (TLS) trên máy chủ chính thức',
    'Rà soát tài khoản, mật khẩu mặc định; khóa tạm khi đăng nhập sai nhiều lần']), [0.9, 9, 4, 3.1])
D.h('III. Phục hồi thử bản sao lưu')
D.table(['TT', 'Nội dung', 'Kết quả', 'Trạng thái'], status_rows([
    'Phục hồi bản sao lưu ngày ……/10/2026 sang môi trường thử; thời gian phục hồi: ……',
    'Đối chiếu dữ liệu sau phục hồi']), [0.9, 9, 4, 3.1])
D.h('IV. Đánh giá chung'); D.p('…………………………………………………………………………………………………………………')
D.ky(('ĐẠI DIỆN VTISC', 'ĐẠI DIỆN NHÀ THẦU')); D.save('07_Bao_cao_kiem_thu_noi_bo_va_ra_soat_ATTT.docx')

# 08 ------------------------------------------------------------------
D = Doc(); D.header('BIÊN BẢN XÁC NHẬN', 'Đủ điều kiện triển khai vận hành thử')
D.can_cu(['Căn cứ Báo cáo kết quả kiểm thử nội bộ và rà soát an toàn thông tin ngày ……/10/2026;']); D.thanh_phan()
D.h('I. Kiểm tra điều kiện sẵn sàng (mục 2.7a Chương V)')
D.table(['TT', 'Kết quả đầu ra', 'Minh chứng', 'Trạng thái'], [
    ('1', 'Môi trường dịch vụ được thiết lập hoàn chỉnh theo phạm vi yêu cầu', 'Biên bản 03, 05', ST),
    ('2', 'Tài khoản, phân quyền, cấu hình vận hành và dữ liệu khởi tạo được thiết lập', 'Biên bản 05', ST),
    ('3', 'Kết nối các dịch vụ tích hợp đã được kiểm tra', 'Biên bản 06', ST),
    ('4', 'Báo cáo kết quả kiểm tra điều kiện sẵn sàng', 'Báo cáo 07', ST),
], [0.9, 9.5, 3.5, 3.1])
D.h('II. Tổ chức vận hành thử')
D.bullets(['Thời gian: từ ngày ……/10/2026 đến ngày ……/10/2026;',
           'Địa chỉ: ……………………… (môi trường hiện có / máy chủ cloud chính thức);',
           'Dữ liệu: dữ liệu mẫu và dữ liệu được Bên thuê cho phép;',
           'Cán bộ Bên thuê tham gia: ………………………………………………'])
D.ket_luan('Dịch vụ đủ điều kiện / chưa đủ điều kiện (gạch bỏ phần không phù hợp) đưa vào vận hành thử.')
D.ky(); D.save('08_Bien_ban_du_dieu_kien_van_hanh_thu.docx')

# 09 ------------------------------------------------------------------
D = Doc(); D.header('BÁO CÁO', 'Kết quả vận hành thử', so=False)
D.p('Thời gian vận hành thử: từ ngày ……/10/2026 đến ngày ……/10/2026. Địa chỉ: ………………………', indent=True)
D.h('I. Kết quả chạy kịch bản nghiệm thu (phần V Chương V)')
steps = ['Đăng ký nhà cung cấp', 'Duyệt hồ sơ nhà cung cấp', 'Đăng sản phẩm', 'Đăng nhu cầu công nghệ',
         'Mời báo giá, nộp đề xuất', 'Trao đổi trực tuyến', 'Lập hợp đồng điện tử', 'Ký số',
         'Thông báo qua email, SMS', 'Tra cứu nhật ký']
D.table(['Bước', 'Nội dung', 'Người thực hiện', 'Kết quả', 'Trạng thái'],
        [(str(i + 1), s, '', '', ST) for i, s in enumerate(steps)], [1.2, 5.5, 3.6, 3.6, 3.1])
D.h('II. Lỗi phát sinh và khắc phục')
D.table(['TT', 'Mô tả', 'Mức độ', 'Ngày phát hiện', 'Ngày khắc phục', 'Trạng thái'],
        [(str(i), '', '', '', '', '') for i in range(1, 6)], [0.9, 6, 2, 2.4, 2.6, 3.1])
D.h('III. Ý kiến người sử dụng'); D.p('…………………………………………………………………………………………………………………')
D.h('IV. Đánh giá, kiến nghị')
D.p('Hệ thống vận hành ổn định / chưa ổn định; còn / không còn lỗi ảnh hưởng đến hoạt động. Kiến nghị: ……………………………', indent=True)
D.ky(('XÁC NHẬN CỦA BÊN THUÊ', 'ĐẠI DIỆN NHÀ THẦU')); D.save('09_Bao_cao_ket_qua_van_hanh_thu.docx')

# 10 ------------------------------------------------------------------
D = Doc(); D.header('BIÊN BẢN ĐÀO TẠO', 'Hướng dẫn sử dụng, quản trị hệ thống')
D.can_cu(); D.thanh_phan(vtisc=True)
D.h('I. Nội dung đào tạo')
D.table(['TT', 'Lớp', 'Thời gian', 'Nội dung chính', 'Số học viên', 'Trạng thái'], [
    ('1', 'Nhóm quản trị', '……/10/2026', 'Quản lý người dùng, phân quyền, tham số; duyệt hồ sơ, sản phẩm; theo dõi quy trình 14 bước; báo cáo; sao lưu, giám sát; tiếp nhận hỗ trợ', '', ST),
    ('2', 'Nhóm nghiệp vụ', '……/10/2026', 'Đăng tin, sản phẩm, sự kiện; kết nối cung – cầu; báo giá, hợp đồng điện tử, ký số; tra cứu, trợ lý AI', '', ST),
], [0.9, 2.5, 2.4, 7.1, 1.6, 2.5])
D.h('II. Tài liệu, tài khoản bàn giao')
D.table(['TT', 'Nội dung', 'Hình thức', 'Trạng thái'], status_rows([
    ('Tài liệu hướng dẫn sử dụng', 'Bản điện tử, bản in'),
    ('Tài liệu quản trị', 'Bản điện tử'),
    ('Tài khoản thực hành theo từng nhóm', 'Danh sách kèm theo'),
    ('Kênh hỗ trợ sau đào tạo (phiếu hỗ trợ, đường dây nóng, thư điện tử)', '')], cols_blank=0), [0.9, 9, 4, 3.1])
D.ket_luan('Đã tổ chức …… lớp với …… học viên theo danh sách kèm theo.')
D.ky()
D.d.add_page_break()
D.p('DANH SÁCH HỌC VIÊN', bold=True, align=C, size=14)
D.p('Lớp: ………………………   Ngày: ……/10/2026', align=C)
D.table(['TT', 'Họ và tên', 'Đơn vị', 'Chức vụ', 'Nhóm', 'Ký tên'],
        [(str(i), '', '', '', '', '') for i in range(1, 21)], [0.9, 4.5, 3.6, 2.8, 2.2, 3])
D.save('10_Bien_ban_dao_tao_va_danh_sach_hoc_vien.docx')

# 11 ------------------------------------------------------------------
D = Doc(); D.header('HỒ SƠ ĐỀ XUẤT CẤP ĐỘ AN TOÀN HỆ THỐNG THÔNG TIN', 'Trang web Sàn giao dịch công nghệ Cần Thơ – Cấp độ 2 (dự thảo)', so=False)
D.h('I. Thông tin chung')
D.table(['TT', 'Nội dung', 'Thông tin', 'Trạng thái'], status_rows([
    'Tên hệ thống: Trang web Sàn giao dịch công nghệ Cần Thơ',
    'Chủ quản hệ thống; đơn vị vận hành: ' + BEN_THUE,
    'Đơn vị cung cấp dịch vụ: ' + NHA_THAU,
    'Phạm vi, quy mô: người dùng, loại dữ liệu xử lý (có dữ liệu cá nhân)',
    'Mô hình triển khai: 02 máy chủ cloud đặt tại Việt Nam']), [0.9, 9, 4, 3.1])
D.h('II. Đề xuất cấp độ')
D.p('Đề xuất cấp độ 2, theo hồ sơ kế hoạch thuê đã được phê duyệt và quy định hiện hành. Lý do: …………………………………………', indent=True)
D.h('III. Phương án bảo đảm an toàn thông tin')
D.table(['TT', 'Nhóm biện pháp', 'Nội dung thực hiện', 'Trạng thái'], status_rows([
    ('Phân quyền, xác thực', 'Phân quyền theo vai trò; xác thực hai lớp cho quản trị; khóa tạm khi đăng nhập sai'),
    ('Nhật ký', 'Ghi nhật ký thao tác quản trị, giao dịch, ký số, truy cập hồ sơ; lưu theo thời hạn quy định'),
    ('Mã hóa', 'Kết nối bảo mật; mật khẩu lưu dạng mã hóa một chiều'),
    ('Sao lưu, phục hồi', 'Sao lưu hằng ngày, lưu tối thiểu 30 ngày, bản sao lưu tách biệt; phục hồi thử định kỳ'),
    ('Giám sát', 'Giám sát tài nguyên, tính sẵn sàng, sao lưu, hạn chứng thư; cảnh báo sự cố'),
    ('Ứng cứu sự cố', 'Đầu mối, quy trình, thời gian xử lý theo mức độ'),
    ('Rà soát lỗ hổng', 'Rà soát trước vận hành chính thức và định kỳ')], cols_blank=0), [0.9, 3.6, 9.4, 3.1])
D.h('IV. Tài liệu kèm theo'); D.bullets(['Sơ đồ hệ thống, sơ đồ kết nối;', 'Quy chế bảo đảm an toàn thông tin của hệ thống;', 'Báo cáo rà soát an toàn thông tin.'])
D.ky(('ĐƠN VỊ VẬN HÀNH', 'ĐƠN VỊ CUNG CẤP DỊCH VỤ')); D.save('11_Ho_so_de_xuat_cap_do_ATTT_cap_2_du_thao.docx')

# 12 ------------------------------------------------------------------
D = Doc(); D.header('BIÊN BẢN XÁC NHẬN', 'Khối lượng công việc hoàn thành – Thanh toán lần 1')
D.can_cu(['Căn cứ các biên bản, báo cáo kèm theo;']); D.thanh_phan()
D.h('I. Khối lượng hoàn thành')
w = [('1', 'Khởi động, kế hoạch chi tiết, khởi động thủ tục bên ngoài', '3%', '01, 02'),
     ('2', 'Hạ tầng máy chủ, bản quyền phần mềm, bảo vệ mạng', '16%', '03'),
     ('3', 'Cài đặt ứng dụng, dữ liệu khởi tạo', '17%', '05'),
     ('4', 'Cấu hình nghiệp vụ, tài khoản, phân quyền, sao lưu, giám sát', '11%', '05'),
     ('5', 'Kết nối thư điện tử, hợp đồng điện tử, AI, ký số USB Token, SMS (thương hiệu tạm)', '11%', '06'),
     ('6', 'Kiểm thử nội bộ, rà soát an toàn thông tin, phục hồi thử', '7%', '07'),
     ('7', 'Kiểm tra điều kiện sẵn sàng, đưa vào vận hành thử', '2%', '08'),
     ('8', 'Vận hành thử, báo cáo kết quả', '10%', '09'),
     ('9', 'Đào tạo, bàn giao tài liệu', '5%', '10'),
     ('10', 'Dự thảo hồ sơ an toàn thông tin cấp độ 2', '3%', '11')]
rows = [x + (ST,) for x in w] + [('', 'Cộng khối lượng hoàn thành (kế hoạch 85%)', '……%', '', '')]
D.table(['TT', 'Hạng mục', 'Tỷ trọng', 'Hồ sơ số', 'Trạng thái'], rows, [0.9, 9.2, 1.9, 1.9, 3.1], center_cols=(0, 2, 3), bold_rows=(10,))
D.h('II. Phần còn lại (15%) – hoàn thành tháng 11/2026')
D.bullets(['VR360 – quay dựng mới 10 điểm cảnh, tích hợp, bàn giao (2%);',
           'SMS gửi bằng thương hiệu chính thức; ký số từ xa bằng chứng thư thật (4%);',
           'Kết nối Sàn giao dịch KH&CN Việt Nam (3%);',
           'Hoàn thiện hồ sơ an toàn thông tin theo ý kiến thẩm định (1%);',
           'Nghiệm thu, đưa vào vận hành chính thức (5%).'])
D.ket_luan('Hai bên thống nhất khối lượng công việc hoàn thành đến ngày ……/10/2026 là ……% , làm căn cứ thanh toán lần 1 theo hợp đồng.')
D.ky(); D.save('12_Bien_ban_xac_nhan_khoi_luong_thanh_toan_lan_1.docx')
