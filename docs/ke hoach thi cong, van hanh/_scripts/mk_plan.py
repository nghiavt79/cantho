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
p('KẾ HOẠCH TRIỂN KHAI', bold=True, align=WD_ALIGN_PARAGRAPH.CENTER, size=14, after=0)
p('NGHIỆM THU ĐỢT 1 – THÁNG 10/2026', bold=True, align=WD_ALIGN_PARAGRAPH.CENTER, size=14, after=0)
p('Gói thầu số 4: Thuê dịch vụ thiết lập, vận hành trang web Sàn giao dịch công nghệ Cần Thơ',
  italic=True, align=WD_ALIGN_PARAGRAPH.CENTER, after=10)

# ---- 1
h('1. Mục tiêu')
p('Hoàn thành khối lượng nghiệm thu đợt 1 theo Phụ lục 5 hợp đồng (2.276.437.218 đồng) trong thời gian 21/10 – 30/10/2026. '
  'Cuối đợt lập một biên bản nghiệm thu giai đoạn và bảng xác định giá trị khối lượng; trong quá trình thực hiện không lập biên bản riêng từng việc. '
  'VR360 và phần còn lại thực hiện ở đợt 2.')
p('Điều kiện: bàn giao 02 máy chủ cloud, chứng từ bản quyền (mục 1.3) và kích hoạt các gói dịch vụ (mục 1.4.1 – 1.4.5) trong tháng 10.')

# ---- 2
h('2. Tiến độ thực hiện (lịch dự kiến, cần các đầu mối xác nhận)')
rows = [
    ('2, 1.3', 'Bàn giao 02 máy chủ cloud; cấp, kích hoạt bản quyền Windows Server 2025, SQL Server 2025; tường lửa, chứng thư bảo mật, tên miền', '21 – 24/10', 'Máy chủ, bản quyền sẵn sàng'),
    ('1.4.1', 'Tin nhắn thương hiệu: đăng ký thương hiệu, kích hoạt, kết nối hệ thống', '21 – 26/10', 'Gửi được tin nhắn thử'),
    ('1.4.2', 'Email Hosting: khởi tạo 60 hộp thư, 05 nhóm thư; kết nối gửi thư tự động', '21 – 23/10', 'Gửi được thư thử'),
    ('1.4.3', 'Token ký số: cấp token, cài đặt', '22 – 24/10', 'Ký được hợp đồng PDF'),
    ('1.4.4', 'eContract: kích hoạt gói 10.000 tài liệu; kết nối luồng lập – ký – lưu trữ', '22 – 26/10', 'Lập, ký được hợp đồng thử'),
    ('1.4.5', 'Gói SMS 5.000 tin (02 gói): kích hoạt; kết nối gửi thông báo, mã xác thực', '22 – 24/10', 'Gửi được mã xác thực'),
    ('1.1', 'Cài đặt, cấu hình, kiểm thử 60 chức năng theo 12 nhóm (lịch tại mục 3)', '21 – 28/10', 'Chức năng chạy đúng'),
    ('1.2.1', 'Cài đặt hệ thống trên máy chủ cloud', '25 – 26/10', 'Hệ thống chạy trên máy chủ cloud'),
    ('1.2.2', 'Cấu hình danh mục, tham số, quy trình 14 bước, tài khoản, phân quyền', '21 – 23/10', 'Cấu hình xong'),
    ('1.2.3', 'Tích hợp các giải pháp mục 1.4 và trợ lý AI', '22 – 26/10', 'Kết nối hoạt động'),
    ('1.2.4', 'Kiểm kê, chuẩn hóa, nhập dữ liệu khởi tạo', '21 – 26/10', 'Dữ liệu đầy đủ'),
    ('1.2.5', 'Hướng dẫn sử dụng, đào tạo', 'Đã thực hiện', 'Đã tổ chức đào tạo'),
    ('1.2.6', 'Giám sát, sao lưu; vận hành thử, khắc phục lỗi', '27 – 30/10', 'Hệ thống ổn định'),
    ('', 'Nghiệm thu giai đoạn đợt 1', '30/10', 'Biên bản nghiệm thu giai đoạn, bảng xác định giá trị'),
]
table(['Mục', 'Công việc', 'Thời gian', 'Kết quả'], rows, [1.6, 9.4, 2.4, 3.6], fill_rows={13: 'FFF2CC'})

# ---- 3
h('3. Lịch thực hiện mục 1.1 theo 12 nhóm chức năng (Phụ lục 3)')
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
p('Danh sách chức năng con theo mã Phụ lục 3 tại bảng xác định giá trị khối lượng đợt 1.', italic=True)

# ---- 4
h('4. Giá trị nghiệm thu đợt 1 (theo Phụ lục 5)')
w = [
    ('1.1', 'Thiết lập trang web Sàn (60 chức năng)', '890.782.296'),
    ('1.2', 'Cài đặt, cấu hình, hướng dẫn sử dụng, vận hành, bảo hành, bảo trì, hỗ trợ kỹ thuật', '447.020.320'),
    ('1.3', 'Bản quyền Windows Server, SQL Server (1.3.1 – 1.3.4)', '157.290.000'),
    ('1.4', 'Giải pháp tích hợp (1.4.1 – 1.4.5)', '88.344.602'),
    ('2', 'Thuê hạ tầng máy chủ cloud', '693.000.000'),
    ('', 'Tổng cộng đợt 1', '2.276.437.218'),
]
table(['Mục', 'Hạng mục', 'Đợt 1 (đồng, sau thuế)'], w, [1.6, 11.4, 4], bold_rows=(5,), fill_rows={5: 'E2EFDA'})

d.core_properties.author = 'NghiaVT'
d.core_properties.last_modified_by = 'NghiaVT'
d.save(out)
print('ok', out)
