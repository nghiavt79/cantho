"""Bộ hồ sơ nghiệm thu đợt 1 theo mẫu BBNT khối lượng công việc hoàn thành (chị Vân gửi 21/10/2026),
điều chỉnh theo dự thảo hợp đồng Gói 4: chỉ có Chủ đầu tư và Nhà thầu (không có tư vấn QLDA, giám sát)."""
import sys, os, json
from docx import Document
from docx.shared import Pt, Cm
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.enum.section import WD_ORIENT
from docx.oxml.ns import qn
from docx.oxml import OxmlElement

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from data_dot1 import NHOM, D11, D12, TONG_HD, TONG_D1

OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)
PL3 = json.load(open(os.path.join(HERE, 'pl3.json'), encoding='utf-8'))

C, J, L, R = WD_ALIGN_PARAGRAPH.CENTER, WD_ALIGN_PARAGRAPH.JUSTIFY, WD_ALIGN_PARAGRAPH.LEFT, WD_ALIGN_PARAGRAPH.RIGHT
GOI = 'Gói thầu số 4: Thuê dịch vụ thiết lập, vận hành trang web Sàn giao dịch công nghệ Cần Thơ'
NV = 'Thuê dịch vụ thiết lập, vận hành trang web Sàn giao dịch công nghệ Cần Thơ tại Trung tâm Ứng dụng tiến bộ khoa học và công nghệ thành phố Cần Thơ'
CDT = 'Trung tâm Ứng dụng tiến bộ khoa học và công nghệ thành phố Cần Thơ'
NT = 'Tổng Công ty Viễn thông MobiFone'
HD = 'Hợp đồng số ……/2026/TTƯD-MBF.CT ký ngày ……/……/2026'
BANG_CHU = 'Hai tỷ, hai trăm bảy mươi sáu triệu, bốn trăm ba mươi bảy nghìn, hai trăm mười tám đồng'


def vnd(n):
    return f'{n:,.0f}'.replace(',', '.')


class Doc:
    def __init__(self):
        d = self.d = Document()
        s = d.sections[0]
        s.page_width, s.page_height = Cm(21), Cm(29.7)
        s.left_margin, s.right_margin, s.top_margin, s.bottom_margin = Cm(3), Cm(2), Cm(2), Cm(2)
        st = d.styles['Normal']
        st.font.name = 'Times New Roman'; st.font.size = Pt(13)
        st.element.rPr.rFonts.set(qn('w:eastAsia'), 'Times New Roman')
        st.paragraph_format.space_after = Pt(3); st.paragraph_format.line_spacing = 1.15

    def p(self, text='', bold=False, italic=False, align=J, size=None, indent=False, after=None, before=None):
        para = self.d.add_paragraph(); para.alignment = align
        if indent: para.paragraph_format.first_line_indent = Cm(1)
        if after is not None: para.paragraph_format.space_after = Pt(after)
        if before is not None: para.paragraph_format.space_before = Pt(before)
        r = para.add_run(text); r.bold, r.italic = bold, italic
        if size: r.font.size = Pt(size)
        return para

    def _shade(self, cell, color):
        tcPr = cell._tc.get_or_add_tcPr(); s = OxmlElement('w:shd')
        s.set(qn('w:val'), 'clear'); s.set(qn('w:color'), 'auto'); s.set(qn('w:fill'), color); tcPr.append(s)

    def table(self, headers, rows, widths, center_cols=(0,), right_cols=(), bold_rows=(), group_rows=(), size=11.5):
        t = self.d.add_table(rows=1, cols=len(widths)); t.style = 'Table Grid'; t.alignment = WD_TABLE_ALIGNMENT.CENTER
        for i, x in enumerate(headers):
            c = t.rows[0].cells[i]; c.text = ''
            r = c.paragraphs[0].add_run(x); r.bold = True; r.font.size = Pt(size)
            c.paragraphs[0].alignment = C; self._shade(c, 'D9E2F3')
        for ri, row in enumerate(rows):
            cells = t.add_row().cells
            if ri in group_rows:
                m = cells[0].merge(cells[-1]); m.text = ''
                r = m.paragraphs[0].add_run(row[0]); r.bold = True; r.font.size = Pt(size)
                self._shade(m, 'EDEDED'); continue
            for i, x in enumerate(row):
                cells[i].text = ''
                lines = str(x).split('\n')
                for k, ln in enumerate(lines):
                    para = cells[i].paragraphs[0] if k == 0 else cells[i].add_paragraph()
                    para.paragraph_format.space_after = Pt(0)
                    r = para.add_run(ln); r.font.size = Pt(size); r.bold = ri in bold_rows
                    para.alignment = C if i in center_cols else (R if i in right_cols else L)
        t.autofit = False
        for i, w in enumerate(widths): t.columns[i].width = Cm(w)
        for row in t.rows:
            for i, w in enumerate(widths): row.cells[i].width = Cm(w)
        self.d.add_paragraph().paragraph_format.space_after = Pt(0)
        return t

    def quoc_hieu(self, left_lines=None, date_line='Cần Thơ, ngày …… tháng …… năm 2026'):
        t = self.d.add_table(rows=1, cols=2); t.alignment = WD_TABLE_ALIGNMENT.CENTER
        t.autofit = False
        a, b = t.rows[0].cells
        a.width, b.width = Cm(6.0), Cm(10.0)
        t.columns[0].width, t.columns[1].width = Cm(6.0), Cm(10.0)
        for cell, lines in ((a, left_lines or [('', False)]), (b, [('CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM', True), ('Độc lập - Tự do - Hạnh phúc', True)])):
            cell.text = ''
            for k, (txt, bd) in enumerate(lines):
                para = cell.paragraphs[0] if k == 0 else cell.add_paragraph()
                para.alignment = C; para.paragraph_format.space_after = Pt(0)
                run = para.add_run(txt); run.bold = bd; run.font.size = Pt(12.5)
        self.p(date_line, italic=True, align=R, before=6)

    def thanh_phan(self):
        self.p('1. Đại diện Chủ đầu tư: ' + CDT, bold=True)
        self.p('- Ông: Nghiêm Phong Vũ, Chức vụ: Giám đốc', indent=True)
        self.p('- Ông/Bà: ………………………………, Chức vụ: ………………………………', indent=True)
        self.p('2. Đại diện Nhà thầu: ' + NT, bold=True)
        self.p('- Ông: Lã Thành Trung, Chức vụ: Phó Giám đốc Phụ trách MobiFone Cần Thơ', indent=True)
        self.p('- Ông/Bà: ………………………………, Chức vụ: ………………………………', indent=True)

    def ky(self, left='ĐẠI DIỆN NHÀ THẦU', right='ĐẠI DIỆN CHỦ ĐẦU TƯ'):
        t = self.d.add_table(rows=1, cols=2); t.alignment = WD_TABLE_ALIGNMENT.CENTER
        for i, x in enumerate((left, right)):
            c = t.rows[0].cells[i]; c.text = ''
            pa = c.paragraphs[0]; pa.alignment = C; pa.add_run(x).bold = True
            pb = c.add_paragraph(); pb.alignment = C
            pb.add_run('(Ký, ghi rõ họ tên, chức vụ và đóng dấu)').italic = True

    def landscape(self):
        s = self.d.add_section()
        s.orientation = WD_ORIENT.LANDSCAPE
        s.page_width, s.page_height = Cm(29.7), Cm(21)
        s.left_margin, s.right_margin, s.top_margin, s.bottom_margin = Cm(2), Cm(2), Cm(2), Cm(2)

    def save(self, name, title):
        cp = self.d.core_properties
        cp.author = 'NghiaVT'; cp.last_modified_by = 'NghiaVT'; cp.title = title
        self.d.save(os.path.join(OUT, name)); print('ok', name)


# ===================================================================== 1. BIÊN BẢN NGHIỆM THU
D = Doc()
D.quoc_hieu()
D.p('BIÊN BẢN NGHIỆM THU KHỐI LƯỢNG CÔNG VIỆC HOÀN THÀNH (ĐỢT 1)', bold=True, align=C, size=14, before=6, after=2)
D.p(GOI, bold=True, align=C, after=0)
D.p('Thuộc nhiệm vụ: ' + NV, italic=True, align=C, after=8)

D.p('I. Đối tượng nghiệm thu:', bold=True)
D.p(f'Nghiệm thu khối lượng công việc hoàn thành đợt 1 theo {HD} giữa {CDT} và {NT} về việc thực hiện {GOI}.', indent=True)
D.p('II. Thành phần tham gia nghiệm thu:', bold=True)
D.thanh_phan()
D.p('III. Thời gian, địa điểm nghiệm thu:', bold=True)
D.p('Bắt đầu: …… giờ …… ngày …… tháng 10 năm 2026', indent=True)
D.p('Kết thúc: …… giờ …… ngày …… tháng 10 năm 2026', indent=True)
D.p('Tại: ' + CDT + ', số 166 Nguyễn Văn Cừ, phường An Bình, thành phố Cần Thơ.', indent=True)

D.p('IV. Đánh giá công việc thực hiện:', bold=True)
D.p('1. Về tài liệu làm căn cứ nghiệm thu:', bold=True)
for x in [
    'Quyết định số 2861/QĐ-UBND ngày 17/6/2026 của Ủy ban nhân dân thành phố Cần Thơ phê duyệt chủ trương và dự kiến kinh phí thuê dịch vụ;',
    'Quyết định số 669/QĐ-SKHCN ngày 23/9/2026 của Sở Khoa học và Công nghệ phê duyệt Kế hoạch thuê dịch vụ công nghệ số;',
    'Quyết định số 310/QĐ-TTƯD ngày 20/10/2026 của Trung tâm Ứng dụng tiến bộ khoa học và công nghệ thành phố Cần Thơ phê duyệt kết quả lựa chọn nhà thầu ' + GOI + ';',
    HD + ' giữa ' + CDT + ' và ' + NT + ';',
    'Biên bản kiểm tra điều kiện triển khai ngày ……/10/2026;',
    'Biên bản kiểm thử chức năng phần mềm đợt 1 ngày ……/10/2026 (kèm báo cáo kết quả kiểm thử);',
    'Văn bản số ……/…… ngày ……/10/2026 của ' + NT + ' về việc đề nghị nghiệm thu đợt 1 Hợp đồng.',
]:
    D.p('- ' + x, indent=True)
D.p('2. Nghiệm thu về chất lượng:', bold=True)
D.p('Nhà thầu đã thực hiện các hạng mục công việc đợt 1 đáp ứng yêu cầu kỹ thuật tại Chương V Hồ sơ mời thầu, Phụ lục 3 Hợp đồng, '
    'các tiêu chuẩn, quy chuẩn kỹ thuật áp dụng và đúng nội dung ' + HD + ' (chi tiết tại Phụ lục 02 kèm theo).', indent=True)
D.p('3. Nghiệm thu về khối lượng:', bold=True)
D.p('Nhà thầu đã hoàn thành khối lượng công việc đợt 1 theo Phụ lục 5 Hợp đồng (chi tiết tại Phụ lục 01 kèm theo); khối lượng còn lại '
    '(giải pháp thực tế ảo VR360; phần còn lại của mục 1.1 và mục 1.2) sẽ được nghiệm thu sau khi Nhà thầu hoàn thành theo đúng yêu cầu của Hợp đồng.', indent=True)
D.p('4. Nghiệm thu về tiến độ thực hiện:', bold=True)
D.p('Đảm bảo tiến độ theo ' + HD + '.', indent=True)
D.p('5. Các ý kiến khác:', bold=True)
D.p('Trên cơ sở Hợp đồng đã ký, Chủ đầu tư đồng ý thanh toán cho Nhà thầu giá trị khối lượng được các bên thống nhất nghiệm thu, theo quy định của Hợp đồng.', indent=True)
D.p('Các bên thống nhất giá trị nghiệm thu:', indent=True)
for k, v in [('Giá trị hợp đồng', vnd(TONG_HD)), ('Giá trị nghiệm thu đợt 1', vnd(TONG_D1)),
             ('Giá trị được thanh toán đợt 1 theo điều khoản thanh toán của Hợp đồng', vnd(TONG_D1)),
             ('+ Thu hồi tạm ứng (Hợp đồng không tạm ứng)', '0'), ('+ Giá trị đề nghị thanh toán', vnd(TONG_D1))]:
    D.p(f'{k}: {v} đồng', indent=True)
D.p('(Bằng chữ: ' + BANG_CHU + ')', italic=True, indent=True)

D.p('V. Kết luận:', bold=True)
D.p('Chấp nhận nghiệm thu khối lượng công việc hoàn thành đợt 1 của ' + GOI + '.', indent=True)
D.p('Yêu cầu Nhà thầu tiếp tục thực hiện các công việc còn lại và khắc phục các tồn tại (nếu có) theo nội dung Hợp đồng.', indent=True)
D.p('Các bên trực tiếp nghiệm thu chịu trách nhiệm trước pháp luật về việc nghiệm thu này. Biên bản này được lập thành 04 (bốn) bản có giá trị pháp lý như nhau, '
    'mỗi bên giữ 02 (hai) bản./.', indent=True)
D.ky()

# ---- Phụ lục 01
D.d.add_page_break()
D.p('PHỤ LỤC 01: KHỐI LƯỢNG VÀ GIÁ TRỊ NGHIỆM THU ĐỢT 1', bold=True, align=C, size=13.5, after=0)
D.p('(Kèm theo Biên bản nghiệm thu khối lượng công việc hoàn thành (đợt 1) ngày ……/10/2026)', italic=True, align=C)
D.p('Đơn vị tính: đồng (đã gồm thuế)', italic=True, align=R)
pl01 = [
    ('1', 'Thuê dịch vụ thiết lập, vận hành 01 trang web Sàn giao dịch công nghệ Cần Thơ', '', '', '', '', ''),
    ('1.1', 'Thiết lập trang web Sàn giao dịch công nghệ Cần Thơ', 'Gói', '1', '75%', vnd(1187709727), vnd(D11)),
    ('1.2', 'Cài đặt, cấu hình, hướng dẫn sử dụng, vận hành, bảo hành, bảo trì và hỗ trợ kỹ thuật hệ thống trong thời gian 05 năm', 'Gói', '1', '80%', vnd(558775400), vnd(D12)),
    ('1.3', 'Bản quyền phần mềm Windows Server Standard và SQL Server Standard', '', '', '', '', ''),
    ('1.3.1', 'Windows Server 2025 Standard – 16 Core License Pack', 'Gói', '1', '1', vnd(56000000), vnd(56000000)),
    ('1.3.2', 'Windows Server 2025 – 1 User CAL', 'Gói', '1', '1', vnd(3010000), vnd(3010000)),
    ('1.3.3', 'SQL Server 2025 Standard edition Perpetual 1 Server License', 'Gói', '1', '1', vnd(77000000), vnd(77000000)),
    ('1.3.4', 'SQL Server 2025 – 1 User CAL', 'Gói', '1', '1', vnd(21280000), vnd(21280000)),
    ('1.4', 'Giải pháp tích hợp kèm theo', '', '', '', '', ''),
    ('1.4.1', 'Giải pháp tin nhắn thương hiệu (05 năm, mỗi năm 3.600 SMS)', 'Gói', '1', '1', vnd(27500000), vnd(27500000)),
    ('1.4.2', 'Email Hosting gói 3 – 60 tháng', 'Gói', '1', '1', vnd(8250000), vnd(8250000)),
    ('1.4.3', 'Gói combo TOKEN cho khách hàng doanh nghiệp (03 năm, miễn phí thêm 02 năm)', 'Gói', '1', '1', vnd(2594602), vnd(2594602)),
    ('1.4.4', 'Gói eContract 10.000 tài liệu', 'Gói', '1', '1', vnd(39000000), vnd(39000000)),
    ('1.4.5', 'Gói SMS 5.000 tin nhắn', 'Gói', '2', '2', vnd(5500000), vnd(11000000)),
    ('–', 'Giải pháp thực tế ảo VR360 – 10 điểm cảnh quay dựng mới (nghiệm thu đợt sau)', 'Gói', '1', '0', vnd(45000000), '0'),
    ('2', 'Thuê hạ tầng máy chủ (cloud) phục vụ cài đặt, vận hành và lưu trữ dữ liệu của hệ thống (02 máy chủ)', 'Hệ thống', '1', '1', vnd(693000000), vnd(693000000)),
    ('', 'Tổng cộng', '', '', '', '', vnd(TONG_D1)),
]
D.table(['STT', 'Mô tả công việc', 'Đơn vị tính', 'Khối lượng hợp đồng', 'Khối lượng nghiệm thu đợt 1', 'Đơn giá', 'Thành tiền nghiệm thu đợt 1'],
        pl01, [1.2, 6.2, 1.5, 1.5, 1.8, 2.2, 2.4], center_cols=(0, 2, 3, 4), right_cols=(5, 6),
        bold_rows=(0, 3, 8, len(pl01) - 1), size=10.5)
D.p('Ghi chú: khối lượng nghiệm thu mục 1.1, 1.2 theo tỷ lệ tại Phụ lục 5 Hợp đồng; danh mục chức năng mục 1.1 tại Phụ lục 02.', italic=True, size=11)

# ---- Phụ lục 02 (khổ ngang)
D.landscape()
D.p('PHỤ LỤC 02: CHỨC NĂNG PHẦN MỀM ĐƯỢC NGHIỆM THU', bold=True, align=C, size=13.5, after=0)
D.p('(Kèm theo Biên bản nghiệm thu khối lượng công việc hoàn thành (đợt 1) ngày ……/10/2026)', italic=True, align=C)
D.p('Danh mục chức năng theo bảng "Các yêu cầu cụ thể về chức năng chính" tại Phụ lục 3 Hợp đồng; mã chức năng = phần (A, B, C) + số mục trong Phụ lục 3. '
    'Đợt 1 nghiệm thu 60/63 chức năng; B.1.2, C.1 (VR360), C.2 (tích hợp sàn giao dịch khác) và phần gọi video của C.7 nghiệm thu đợt sau.', italic=True, size=11.5)

D.p('Bảng 1. Danh mục yêu cầu chức năng', bold=True, before=4)
code_group = {m: (so, ten) for so, ten, _, cns in NHOM for m, _ in cns}
SECT = [('A', 'A. WEBSITE QUẢN LÝ'), ('B', 'B. WEBSITE BÁN HÀNG'), ('C', 'C. TIỆN ÍCH KHÁC')]


def sort_key(code):
    parts = code.split('.')
    return (parts[0], *[int(x) for x in parts[1:]])


codes = sorted(code_group, key=sort_key)
rows, groups, n = [], [], 0
for sec, title in SECT:
    groups.append(len(rows)); rows.append((title,))
    for code in [c for c in codes if c[0] == sec]:
        n += 1
        ten = PL3[code]['ten']
        if code == 'C.7': ten += ' (phần chat trực tiếp)'
        so, nhom = code_group[code]
        rows.append((str(n), code, ten, f'{so}. {nhom}', 'Đáp ứng'))
D.table(['STT', 'Mã', 'Yêu cầu chức năng (Phụ lục 3)', 'Nhóm chức năng', 'Đánh giá'], rows, [1.2, 1.6, 10.5, 9.0, 2.5],
        center_cols=(0, 1, 4), group_rows=groups, size=10.5)


def actors(code):
    if code.startswith('A.2'): return 'Quản trị Sàn', 'Nhà cung cấp'
    if code.startswith('A.5'): return 'Quản trị Sàn', 'Người mua'
    if code.startswith('A.'): return 'Quản trị Sàn', ''
    if code.startswith('B.1'): return 'Người dùng', 'Quản trị Sàn'
    if code.startswith('B.2'): return 'Nhà cung cấp', 'Quản trị Sàn'
    if code.startswith('B.3'): return 'Người mua', 'Nhà cung cấp'
    if code in ('C.4', 'C.7'): return 'Người mua, Nhà cung cấp', 'Quản trị Sàn'
    if code == 'C.6': return 'Quản trị Sàn', 'Người dùng'
    return 'Người dùng', ''


D.p('Bảng 2. Chuyển đổi yêu cầu chức năng sang trường hợp sử dụng', bold=True, before=6)
rows, groups, n = [], [], 0
for sec, title in SECT:
    groups.append(len(rows)); rows.append((title,))
    for code in [c for c in codes if c[0] == sec]:
        n += 1
        mota = [ln.strip(' -+') for ln in PL3[code]['mota'].split('\n') if ln.strip(' -+')]
        if code == 'C.7': mota = ['Chat trực tiếp giữa người bán, người mua và Sàn']
        if not mota: mota = [PL3[code]['ten']]
        k = len(mota)
        loai = 'Đơn giản' if k <= 2 else ('Trung bình' if k <= 4 else 'Phức tạp')
        a1, a2 = actors(code)
        rows.append((str(n), f'{code} {PL3[code]["ten"]}', a1, a2, '\n'.join('- ' + x for x in mota), 'B', loai, 'Đáp ứng'))
D.table(['TT', 'Tên trường hợp sử dụng', 'Tác nhân chính', 'Tác nhân phụ', 'Mô tả trường hợp sử dụng', 'Mức độ cần thiết', 'Phân loại', 'Đánh giá'],
        rows, [1.0, 4.6, 2.6, 2.4, 9.6, 1.6, 2.0, 2.0], center_cols=(0, 5, 6, 7), group_rows=groups, size=10)
D.p('Ghi chú: Mức độ cần thiết: B – Bắt buộc, M – Mong muốn. Cột "Đánh giá" ghi "Đáp ứng" hoặc "Chưa đáp ứng" (nêu rõ lý do tại biên bản nếu chưa đáp ứng).', italic=True, size=11)
D.save('01_Bien_ban_nghiem_thu_khoi_luong_dot1.docx', 'Biên bản nghiệm thu khối lượng công việc hoàn thành đợt 1')

# ===================================================================== 2. CÔNG VĂN ĐỀ NGHỊ NGHIỆM THU
D = Doc()
D.quoc_hieu(left_lines=[('TỔNG CÔNG TY', False), ('VIỄN THÔNG MOBIFONE', True), ('Số: ……/MBF-……', False),
                        ('V/v đề nghị nghiệm thu đợt 1', False), ('Gói thầu số 4', False)])
D.p('Kính gửi: ' + CDT + '.', align=C, before=6, after=8)
D.p('Căn cứ ' + HD + ' giữa ' + CDT + ' và ' + NT + ' về việc thực hiện ' + GOI + ';', indent=True)
D.p(NT + ' đã hoàn thành khối lượng công việc đợt 1 theo Phụ lục 5 Hợp đồng với giá trị ' + vnd(TONG_D1) + ' đồng (đã gồm thuế), gồm: '
    'thiết lập trang web Sàn (60 chức năng theo Phụ lục 3); cài đặt, cấu hình, tích hợp, dữ liệu khởi tạo, hướng dẫn sử dụng, đào tạo, vận hành thử; '
    'bản quyền phần mềm Windows Server, SQL Server; các giải pháp tích hợp (tin nhắn thương hiệu, thư điện tử, token ký số, hợp đồng điện tử, gói SMS); '
    'hạ tầng máy chủ cloud.', indent=True)
D.p(NT + ' kính đề nghị ' + CDT + ' tổ chức nghiệm thu khối lượng công việc hoàn thành đợt 1 vào ngày ……/10/2026.', indent=True)
D.p('Hồ sơ kèm theo:', bold=True)
for x in ['Dự thảo Biên bản nghiệm thu khối lượng công việc hoàn thành đợt 1 (kèm Phụ lục 01, Phụ lục 02);',
          'Biên bản kiểm tra điều kiện triển khai;',
          'Biên bản kiểm thử chức năng phần mềm đợt 1 (kèm báo cáo kết quả kiểm thử);',
          'Bảng xác định giá trị khối lượng công việc đợt 1.']:
    D.p('- ' + x, indent=True)
D.p('Trân trọng./.', indent=True)
t = D.d.add_table(rows=1, cols=2)
a, b = t.rows[0].cells
a.text = ''; ra = a.paragraphs[0].add_run('Nơi nhận:\n- Như trên;\n- Lưu: VT, ……'); ra.font.size = Pt(11); ra.italic = True
b.text = ''; pb = b.paragraphs[0]; pb.alignment = C; pb.add_run('ĐẠI DIỆN NHÀ THẦU').bold = True
pc = b.add_paragraph(); pc.alignment = C; pc.add_run('(Ký, ghi rõ họ tên, chức vụ và đóng dấu)').italic = True
D.save('02_Cong_van_de_nghi_nghiem_thu_dot1.docx', 'Công văn đề nghị nghiệm thu đợt 1')

# ===================================================================== 3. BIÊN BẢN KIỂM TRA ĐIỀU KIỆN TRIỂN KHAI
D = Doc()
D.quoc_hieu()
D.p('BIÊN BẢN KIỂM TRA ĐIỀU KIỆN TRIỂN KHAI', bold=True, align=C, size=14, before=6, after=2)
D.p(GOI, bold=True, align=C, after=8)
D.p('Căn cứ ' + HD + '; Chương V Hồ sơ mời thầu.', italic=True, indent=True)
D.p('Hôm nay, ngày …… tháng 10 năm 2026, tại ' + CDT + ', chúng tôi gồm:', indent=True)
D.thanh_phan()
D.p('Hai bên cùng kiểm tra điều kiện triển khai, gồm cả kiểm tra phần mềm trước khi cài đặt, kết quả như sau:', indent=True)
kt = [
    ('Hạ tầng máy chủ cloud (mục 2)', '02 máy chủ; mỗi máy CPU Intel Xeon Gold 6330 hoặc tương đương, tối thiểu 8 lõi, 32 GB RAM, 500 GB SSD, 01 IP; sao lưu, giám sát; IPv4/IPv6; dữ liệu lưu tại Việt Nam'),
    ('Bản quyền phần mềm (mục 1.3)', 'Windows Server 2025 Standard 16 lõi + User CAL; SQL Server 2025 Standard + User CAL. Số chứng từ: ……………………'),
    ('Tên miền, bảo mật', 'Tên miền chính thức; chứng thư bảo mật SSL; tường lửa chỉ mở cổng cần thiết; cơ sở dữ liệu không mở ra Internet'),
    ('Phần mềm trước khi cài đặt', 'Bộ cài đặt đúng phiên bản bàn giao; đã rà soát lỗ hổng an toàn thông tin'),
    ('Cài đặt ứng dụng', 'Ứng dụng chạy trên máy chủ cloud; giao diện tiếng Việt, tiếng Anh; trang quản trị có xác thực hai lớp'),
    ('Cấu hình và dữ liệu khởi tạo', 'Danh mục, tham số, quy trình giao dịch 14 bước; tài khoản, phân quyền; dữ liệu khởi tạo đã đối chiếu nguồn: hồ sơ công nghệ ……; chuyên gia ……; nhà cung ứng ……'),
    ('Giải pháp tích hợp (mục 1.4.1 – 1.4.5)', 'Tin nhắn thương hiệu; email 60 địa chỉ, 05 nhóm thư; token ký số; eContract; gói SMS: đã kích hoạt, kết nối hệ thống'),
    ('Sao lưu, giám sát', 'Sao lưu tự động hằng ngày, lưu tối thiểu 30 ngày, bản sao lưu tách biệt; giám sát tài nguyên, tình trạng website, hạn chứng thư; đã phục hồi thử'),
]
D.table(['TT', 'Nội dung kiểm tra', 'Yêu cầu (Chương V HSMT, Hợp đồng)', 'Kết quả'],
        [(str(i + 1), a, b, 'Đạt') for i, (a, b) in enumerate(kt)], [0.9, 4.2, 9.4, 1.5], center_cols=(0, 3), size=11)
D.p('Kết luận: Hệ thống đủ điều kiện triển khai và đưa vào vận hành thử.', bold=True, indent=True)
D.p('Biên bản được lập thành 04 (bốn) bản, mỗi bên giữ 02 (hai) bản./.', indent=True)
D.ky()
D.save('03_Bien_ban_kiem_tra_dieu_kien_trien_khai.docx', 'Biên bản kiểm tra điều kiện triển khai')

# ===================================================================== 4. BIÊN BẢN KIỂM THỬ CHỨC NĂNG + BÁO CÁO
D = Doc()
D.quoc_hieu()
D.p('BIÊN BẢN KIỂM THỬ CHỨC NĂNG PHẦN MỀM ĐỢT 1', bold=True, align=C, size=14, before=6, after=0)
D.p('(kèm báo cáo kết quả kiểm thử)', italic=True, align=C, after=2)
D.p(GOI, bold=True, align=C, after=8)
D.p('Căn cứ ' + HD + '; Phụ lục 3 Hợp đồng; phần V Chương V Hồ sơ mời thầu.', italic=True, indent=True)
D.p('Hôm nay, ngày …… tháng 10 năm 2026, tại ' + CDT + ', chúng tôi gồm:', indent=True)
D.thanh_phan()
D.p('Thời gian kiểm thử: từ ngày ……/10/2026 đến ngày ……/10/2026. Môi trường: máy chủ cloud của Gói thầu.', indent=True)

D.p('I. Kết quả kiểm thử chức năng theo 12 nhóm (Phụ lục 3)', bold=True, before=4)
rows = []
for so, ten, _, cns in NHOM:
    rows.append((str(so), ten, str(len(cns)), str(len(cns)), 'Đạt' if cns else 'Thuộc mục 1.2, mục 2'))
rows.append(('', 'Cộng', '60', '60', ''))
D.table(['Nhóm', 'Nhóm chức năng', 'Số chức năng', 'Số chức năng đạt', 'Kết quả'], rows, [1.3, 8.2, 2.0, 2.3, 2.2],
        center_cols=(0, 2, 3, 4), bold_rows=(len(rows) - 1,), size=11)
D.p('Danh mục từng chức năng tại Phụ lục 02 Biên bản nghiệm thu khối lượng đợt 1.', italic=True, indent=True, size=11.5)

D.p('II. Kịch bản kiểm thử nghiệm thu (phần V Chương V Hồ sơ mời thầu)', bold=True, before=4)
steps = ['Đăng ký nhà cung cấp', 'Duyệt hồ sơ nhà cung cấp', 'Đăng sản phẩm', 'Đăng nhu cầu công nghệ', 'Mời báo giá, nộp đề xuất',
         'Trao đổi trực tuyến', 'Lập hợp đồng điện tử', 'Ký số', 'Thông báo qua email, SMS', 'Tra cứu nhật ký']
D.table(['Bước', 'Nội dung', 'Kết quả'], [(str(i + 1), s, 'Đạt') for i, s in enumerate(steps)], [1.3, 12.2, 2.5], center_cols=(0, 2), size=11)

D.p('III. Đối chiếu quy trình giao dịch 14 bước với kịch bản kiểm thử', bold=True, before=4)
W14 = [('1', 'Tạo yêu cầu chuyển giao', 'Bước 4'), ('2', 'Ký thỏa thuận bảo mật (khi sản phẩm yêu cầu)', 'Bổ sung'),
       ('3', 'Tạo yêu cầu báo giá, mời nhà cung cấp', 'Bước 5'), ('4', 'Nộp và chấm điểm hồ sơ đề xuất', 'Bước 5'),
       ('5', 'Đàm phán thương mại', 'Bước 6'), ('6', 'Kiểm tra pháp lý', 'Bổ sung'), ('7', 'Lập và ký hợp đồng điện tử', 'Bước 7, 8'),
       ('8', 'Xác nhận tạm ứng', 'Bổ sung'), ('9', 'Thử nghiệm', 'Bổ sung'), ('10', 'Bàn giao và triển khai', 'Bổ sung'),
       ('11', 'Đào tạo và chuyển giao vận hành', 'Bổ sung'), ('12', 'Bàn giao hồ sơ kỹ thuật', 'Bổ sung'),
       ('13', 'Nghiệm thu', 'Bổ sung'), ('14', 'Thanh lý hợp đồng', 'Bổ sung')]
D.table(['Bước quy trình', 'Nội dung', 'Kiểm thử tương ứng', 'Kết quả'], [x + ('Đạt',) for x in W14], [1.9, 8.6, 3.0, 2.5],
        center_cols=(0, 2, 3), size=11)
D.p('"Bổ sung": kiểm thử bổ sung trên cùng hồ sơ giao dịch ngoài kịch bản 10 bước. Thông báo email, SMS và nhật ký được kiểm tra ở tất cả 14 bước; '
    'minh chứng là nhật ký hồ sơ giao dịch trên trang quản trị.', italic=True, indent=True, size=11.5)

D.p('IV. Lỗi phát sinh và khắc phục', bold=True, before=4)
D.p('Số lỗi phát sinh: ……; đã khắc phục: ……; còn tồn tại: ……………………………………', indent=True)
D.p('Kết luận: Các chức năng phần mềm đợt 1 đạt yêu cầu kiểm thử.', bold=True, indent=True)
D.p('Biên bản được lập thành 04 (bốn) bản, mỗi bên giữ 02 (hai) bản./.', indent=True)
D.ky()
D.save('04_Bien_ban_kiem_thu_chuc_nang_dot1.docx', 'Biên bản kiểm thử chức năng phần mềm đợt 1')
