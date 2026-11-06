import sys
from docx import Document
from docx.shared import Pt, Cm, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml.ns import qn
from docx.oxml import OxmlElement

out = sys.argv[1]
d = Document()
sec = d.sections[0]
sec.page_width, sec.page_height = Cm(21), Cm(29.7)
sec.left_margin, sec.right_margin = Cm(2.5), Cm(1.5)
sec.top_margin, sec.bottom_margin = Cm(2), Cm(2)

st = d.styles['Normal']
st.font.name = 'Times New Roman'
st.font.size = Pt(13)
st.element.rPr.rFonts.set(qn('w:eastAsia'), 'Times New Roman')
st.paragraph_format.space_after = Pt(4)
st.paragraph_format.line_spacing = 1.15


def p(text='', bold=False, italic=False, align=None, size=None, indent=True, after=None):
    para = d.add_paragraph()
    if align is not None:
        para.alignment = align
    else:
        para.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
        if indent:
            para.paragraph_format.first_line_indent = Cm(1)
    if after is not None:
        para.paragraph_format.space_after = Pt(after)
    r = para.add_run(text)
    r.bold, r.italic = bold, italic
    if size:
        r.font.size = Pt(size)
    return para


def h(text):
    para = p(text, bold=True, indent=False)
    para.paragraph_format.space_before = Pt(8)
    return para


def shade(cell, color):
    tcPr = cell._tc.get_or_add_tcPr()
    s = OxmlElement('w:shd')
    s.set(qn('w:val'), 'clear'); s.set(qn('w:color'), 'auto'); s.set(qn('w:fill'), color)
    tcPr.append(s)


def table(headers, rows, widths, bold_rows=(), fill_rows=None):
    t = d.add_table(rows=1, cols=len(headers))
    t.style = 'Table Grid'
    t.alignment = WD_TABLE_ALIGNMENT.CENTER
    for i, x in enumerate(headers):
        c = t.rows[0].cells[i]
        c.text = ''
        r = c.paragraphs[0].add_run(x); r.bold = True; r.font.size = Pt(11.5)
        c.paragraphs[0].alignment = WD_ALIGN_PARAGRAPH.CENTER
        shade(c, 'D9E2F3')
    for ri, row in enumerate(rows):
        cells = t.add_row().cells
        for i, x in enumerate(row):
            cells[i].text = ''
            para = cells[i].paragraphs[0]
            para.paragraph_format.space_after = Pt(0)
            r = para.add_run(str(x)); r.font.size = Pt(11.5)
            if ri in bold_rows:
                r.bold = True
            if i == 0 or (isinstance(x, str) and (x.endswith('%') or x[:2].isdigit() and '/' in x[:5])):
                para.alignment = WD_ALIGN_PARAGRAPH.CENTER
            if fill_rows and ri in fill_rows:
                shade(cells[i], fill_rows[ri])
    for row in t.rows:
        for i, w in enumerate(widths):
            row.cells[i].width = Cm(w)
    d.add_paragraph().paragraph_format.space_after = Pt(0)
    return t


# ---- Tiêu đề
p('KẾ HOẠCH TRIỂN KHAI, VẬN HÀNH THỬ', bold=True, align=WD_ALIGN_PARAGRAPH.CENTER, size=14, after=0)
p('NGHIỆM THU ĐỢT 1 – THÁNG 10/2026', bold=True, align=WD_ALIGN_PARAGRAPH.CENTER, size=14, after=0)
p('Gói thầu số 4: Thuê dịch vụ thiết lập, vận hành trang web Sàn giao dịch công nghệ Cần Thơ',
  italic=True, align=WD_ALIGN_PARAGRAPH.CENTER, after=10)

# ---- 1
h('1. Mục tiêu và căn cứ')
p('Theo hợp đồng, thanh toán lần 1 căn cứ biên bản nghiệm thu giai đoạn và bảng xác định giá trị khối lượng. Giá trị nghiệm thu '
  'dự kiến đợt 1 theo Phụ lục 5 là 2.276.437.218 đồng (giá hợp đồng 2.730.119.729 đồng). Kế hoạch này bố trí công việc từ 21/10 đến '
  '30/10/2026 để hoàn thành khối lượng đợt 1; VR360 và phần còn lại thực hiện ở đợt 2.')
p('Căn cứ: dự thảo hợp đồng Gói thầu số 4 (Điều 5, Phụ lục 3, 4, 5); Chương V Hồ sơ mời thầu (mục 2.7, phần V); '
  'hồ sơ đề xuất kỹ thuật của MobiFone (Mục 3.2, Mục 8).')

# ---- 2
h('2. Điều kiện thực hiện')
p('MobiFone sử dụng nền tảng phần mềm sẵn có để cài đặt, cấu hình và hoàn thiện theo phạm vi gói thầu, nhờ đó rút ngắn thời gian '
  'triển khai so với tiến độ 30 ngày. Dữ liệu khởi tạo được kiểm kê, đối chiếu nguồn và quyền sử dụng trước khi nhập; số lượng chính thức '
  'được chốt tại biên bản xác nhận dữ liệu khởi tạo.')
p('Điều kiện cần bảo đảm trong tháng 10: bàn giao 02 máy chủ cloud; chứng từ bản quyền (mục 1.3); kích hoạt các gói dịch vụ tích hợp '
  '(mục 1.4.1 – 1.4.5).')

# ---- 3
h('3. Tiến độ thực hiện (lịch dự kiến, cần các đầu mối xác nhận)')
rows = [
    ('2, 1.3', 'Bàn giao 02 máy chủ cloud; cấp, kích hoạt bản quyền Windows Server 2025, SQL Server 2025 (1.3.1 – 1.3.4); tường lửa, chứng thư bảo mật, tên miền', '21 – 24/10', 'Biên bản bàn giao hạ tầng, chứng từ bản quyền'),
    ('1.4.1', 'Tin nhắn thương hiệu: đăng ký thương hiệu, kích hoạt, kết nối hệ thống, gửi thử', '21 – 26/10', 'Biên bản kiểm tra kết nối'),
    ('1.4.2', 'Email Hosting: khởi tạo 60 hộp thư, 05 nhóm thư; kết nối gửi thư tự động', '21 – 23/10', 'Biên bản kiểm tra kết nối'),
    ('1.4.3', 'Token ký số: cấp token, cài đặt, ký thử trên hợp đồng PDF', '22 – 24/10', 'Biên bản kiểm tra kết nối'),
    ('1.4.4', 'eContract: kích hoạt gói 10.000 tài liệu; kết nối luồng lập – ký – lưu trữ hợp đồng', '22 – 26/10', 'Biên bản kiểm tra kết nối'),
    ('1.4.5', 'Gói SMS 5.000 tin (02 gói): kích hoạt; kết nối gửi thông báo, mã xác thực', '22 – 24/10', 'Biên bản kiểm tra kết nối'),
    ('1.1', 'Thiết lập trang web: cài đặt, cấu hình, kiểm thử 60 chức năng theo 12 nhóm (lịch từng ngày tại mục 4)', '21 – 28/10', 'Kết quả kiểm thử chức năng'),
    ('1.2.1', 'Cài đặt hệ thống trên máy chủ cloud', '25 – 26/10', 'Biên bản cài đặt'),
    ('1.2.2', 'Cấu hình danh mục, tham số, quy trình 14 bước, tài khoản, phân quyền', '21 – 23/10', 'Cấu hình vận hành'),
    ('1.2.3', 'Tích hợp, đồng bộ dữ liệu với các giải pháp mục 1.4 và trợ lý AI', '22 – 26/10', 'Biên bản kiểm tra kết nối'),
    ('1.2.4', 'Kiểm kê, chuẩn hóa, nhập dữ liệu khởi tạo', '21 – 26/10', 'Biên bản xác nhận dữ liệu'),
    ('1.2.5', 'Hướng dẫn sử dụng, đào tạo nhóm quản trị và nhóm nghiệp vụ (đã tổ chức buổi đào tạo; bàn giao tài liệu, tài khoản thực hành)', 'Đã thực hiện', 'Biên bản đào tạo, danh sách học viên'),
    ('1.2.6', 'Thiết lập giám sát, sao lưu; vận hành thử theo kịch bản nghiệm thu, khắc phục lỗi', '27 – 30/10', 'Báo cáo vận hành thử'),
    ('', 'Nghiệm thu giai đoạn; lập bảng xác định giá trị khối lượng đợt 1', '30/10', 'Biên bản nghiệm thu giai đoạn'),
]
table(['Mục', 'Công việc', 'Thời gian', 'Kết quả'], rows, [1.6, 9.2, 2.4, 3.8], fill_rows={13: 'FFF2CC'})

# ---- 4
h('4. Lịch thực hiện mục 1.1 theo 12 nhóm chức năng (Phụ lục 3)')
import sys as _s, os as _o
_s.path.insert(0, _o.path.dirname(_o.path.abspath(__file__)))
from data_dot1 import NHOM, tien_nhom, D11
_t = tien_nhom()
w = []
for so, ten, ngay, cns in NHOM:
    if cns:
        w.append((str(so), ten, ngay, str(len(cns)), f'{_t[so]:,}'.replace(',', '.')))
    else:
        w.append((str(so), ten + ' (thuộc mục 1.2 và mục 2)', '', '0', '0'))
w.append(('', 'Kiểm thử tổng thể 60 chức năng, khắc phục lỗi', '28/10', '', ''))
w.append(('', 'Cộng mục 1.1 đợt 1', '', '60', f'{D11:,}'.replace(',', '.')))
table(['Nhóm', 'Nhóm chức năng', 'Ngày', 'Số chức năng', 'Giá trị đợt 1 (đồng)'], w, [1.3, 8.7, 1.8, 2.2, 3],
      bold_rows=(len(w) - 1,), fill_rows={len(w) - 1: 'E2EFDA'})
p('Danh sách từng chức năng con theo mã Phụ lục 3 và lịch từng ngày tại file kèm theo “Bảng xác định giá trị khối lượng đợt 1”. '
  'Giá trị chia theo số chức năng của từng nhóm, để các bên cùng xác nhận.', italic=True)

# ---- 5
h('5. Giá trị nghiệm thu đợt 1 (theo Phụ lục 5)')
w = [
    ('1.1', 'Thiết lập trang web Sàn (60 chức năng)', '890.782.296'),
    ('1.2', 'Cài đặt, cấu hình, hướng dẫn sử dụng, vận hành, bảo hành, bảo trì, hỗ trợ kỹ thuật', '447.020.320'),
    ('1.3', 'Bản quyền Windows Server, SQL Server (1.3.1 – 1.3.4)', '157.290.000'),
    ('1.4', 'Giải pháp tích hợp (1.4.1 – 1.4.5): tin nhắn thương hiệu, email, token ký số, eContract, gói SMS', '88.344.602'),
    ('2', 'Thuê hạ tầng máy chủ cloud', '693.000.000'),
    ('', 'Tổng cộng đợt 1', '2.276.437.218'),
]
table(['Mục', 'Hạng mục', 'Đợt 1 (đồng, sau thuế)'], w, [1.6, 11.4, 4], bold_rows=(5,), fill_rows={5: 'E2EFDA'})
p('Đợt 2: VR360 (10 điểm cảnh quay dựng mới, chức năng B.1.2, C.1); C.2 tích hợp sàn giao dịch khác; phần gọi video của C.7; '
  'nghiệm thu đưa vào vận hành; vận hành, bảo hành, bảo trì, hỗ trợ kỹ thuật 05 năm.', italic=True)

# ---- 6
h('6. Hồ sơ nghiệm thu giai đoạn (dự kiến 30/10/2026)')
for x in [
    'Biên bản nghiệm thu giai đoạn;',
    'Bảng xác định giá trị khối lượng đợt 1;',
    'Kèm theo: chứng từ bản quyền, chứng từ kích hoạt gói dịch vụ, danh sách học viên, tài liệu hướng dẫn sử dụng.',
]:
    p('- ' + x)

# ---- 7
h('7. Việc cần Chủ trì thuê phối hợp')
for x in [
    'Cung cấp quyền cấu hình tên miền, hồ sơ pháp lý đăng ký thương hiệu SMS.',
    'Ngày 27 – 30/10: cử cán bộ tham gia vận hành thử.',
    'Ngày 30/10: tham gia nghiệm thu giai đoạn.',
]:
    p('- ' + x)

# ---- 8
h('8. Rủi ro và cách xử lý')
r = [
    ('Máy chủ cloud, bản quyền chưa bàn giao kịp 24/10', 'Kiểm thử tiếp tục trên môi trường hiện có để không chậm mốc 30/10; mục 1.3 và mục 2 chỉ nghiệm thu khi có máy chủ và chứng từ bản quyền.'),
    ('Thương hiệu SMS chưa được cấp', 'Gói dịch vụ kích hoạt trong tháng 10; vận hành thử bằng thương hiệu tạm trong thời gian chờ cấp.'),
    ('Phát sinh lỗi trong quá trình vận hành thử', 'Sao lưu trước mỗi lần cập nhật; lỗi được khắc phục trong ngày.'),
]
table(['Rủi ro', 'Cách xử lý'], r, [5.5, 11.5])

p('Vướng mắc phát sinh được MobiFone báo Chủ trì thuê trong 01 ngày làm việc, kèm đề xuất cách xử lý.', italic=True)

d.core_properties.author = 'NghiaVT'
d.core_properties.last_modified_by = 'NghiaVT'
d.save(out)
print('ok', out)
