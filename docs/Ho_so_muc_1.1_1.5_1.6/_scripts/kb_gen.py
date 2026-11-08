import sys, copy
sys.path.insert(0, r'C:\Users\WINDOW~1\AppData\Local\Temp')
import docx
from docx.shared import Pt, Cm, Emu
from docx.enum.text import WD_ALIGN_PARAGRAPH as AL
from docx.enum.section import WD_ORIENT
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml.ns import qn
from docx.oxml import OxmlElement
from kb_data import CN, QT14, API, PHI

DIR = r'D:\2026\CanTho\docs\Ho_so_muc_1.1_1.5_1.6\1.6 Tu kiem tra noi bo, san sang van hanh thu'
BB = DIR + r'\Bien ban tu kiem thu chuc nang phan mem dot 1.docx'
HD = ('Căn cứ Hợp đồng số HD2600273782_2610221351 ngày 22/10/2026 giữa Trung tâm Ứng dụng tiến bộ khoa học và công nghệ '
      'thành phố Cần Thơ và Tổng Công ty Viễn thông MobiFone về việc thực hiện Gói thầu số 04: Thuê dịch vụ thiết lập, vận hành '
      'trang web Sàn giao dịch công nghệ Cần Thơ; Phụ lục 3 Hợp đồng; phần V Chương V Hồ sơ mời thầu; mục 1.6 Kế hoạch triển khai '
      'chi tiết Gói thầu số 04 của MobiFone Cần Thơ.')
BB_NAME = 'Biên bản tự kiểm thử chức năng phần mềm đợt 1'

def new_doc(landscape):
    d = docx.Document(BB)
    body = d.element.body
    for el in list(body):
        if el.tag != qn('w:sectPr'): body.remove(el)
    s = d.sections[0]
    w, h = Emu(10692130), Emu(7560310)
    if landscape:
        s.orientation, s.page_width, s.page_height = WD_ORIENT.LANDSCAPE, w, h
        s.left_margin, s.right_margin = Cm(2.0), Cm(1.5)
    else:
        s.orientation, s.page_width, s.page_height = WD_ORIENT.PORTRAIT, h, w
        s.left_margin, s.right_margin = Cm(3.0), Cm(2.0)
    s.top_margin, s.bottom_margin = Cm(2.0), Cm(2.0)
    return d

def para(d, text='', bold=False, italic=False, align=AL.JUSTIFY, size=None, after=None, parts=None):
    p = d.add_paragraph(); p.alignment = align
    for t, b in (parts or [(text, bold)]):
        r = p.add_run(t); r.bold = b; r.italic = italic
        if size: r.font.size = Pt(size)
    if after is not None: p.paragraph_format.space_after = Pt(after)
    return p

def heading(d, text): return para(d, text, bold=True, align=AL.LEFT, after=4)

def shade(cell, fill='D9D9D9'):
    tcPr = cell._tc.get_or_add_tcPr(); sh = OxmlElement('w:shd')
    sh.set(qn('w:val'), 'clear'); sh.set(qn('w:color'), 'auto'); sh.set(qn('w:fill'), fill); tcPr.append(sh)

def set_cell(cell, text, bold=False, size=11, align=None):
    cell.text = ''
    lines = text if isinstance(text, list) else [text]
    for i, line in enumerate(lines):
        p = cell.paragraphs[0] if i == 0 else cell.add_paragraph()
        p.paragraph_format.space_after = Pt(0); p.paragraph_format.line_spacing = 1.0
        if align is not None: p.alignment = align
        r = p.add_run(line); r.bold = bold; r.font.size = Pt(size)

def table(d, header, rows, widths, size=11, center_cols=()):
    t = d.add_table(rows=1, cols=len(header)); t.style = 'Table Grid'; t.alignment = WD_TABLE_ALIGNMENT.CENTER
    for i, h in enumerate(header):
        set_cell(t.rows[0].cells[i], h, True, size, AL.CENTER); shade(t.rows[0].cells[i])
    tr = t.rows[0]._tr; trPr = tr.get_or_add_trPr(); th = OxmlElement('w:tblHeader'); th.set(qn('w:val'), 'true'); trPr.append(th)
    for row in rows:
        cells = t.add_row().cells
        for i, v in enumerate(row):
            set_cell(cells[i], v, False, size, AL.CENTER if i in center_cols else None)
    for row in t.rows:
        for i, wd in enumerate(widths): row.cells[i].width = Cm(wd)
    d.add_paragraph().paragraph_format.space_after = Pt(0)
    return t

def no_border(t):
    tblPr = t._tbl.tblPr; b = OxmlElement('w:tblBorders')
    for e in ('top', 'left', 'bottom', 'right', 'insideH', 'insideV'):
        x = OxmlElement('w:' + e); x.set(qn('w:val'), 'nil'); b.append(x)
    tblPr.append(b)

def header_block(d, so, ngay, ve_viec=None):
    t = d.add_table(rows=1, cols=2); no_border(t)
    l = ['TỔNG CÔNG TY VIỄN THÔNG MOBIFONE', 'MOBIFONE CẦN THƠ', '──────────']
    if so: l.append(so)
    set_cell(t.cell(0, 0), l, False, 12, AL.CENTER)
    for i in (0, 1): t.cell(0, 0).paragraphs[i].runs[0].bold = (i == 1)
    set_cell(t.cell(0, 1), ['CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM', 'Độc lập - Tự do - Hạnh phúc', '───────────────'], False, 12, AL.CENTER)
    for i in (0, 1): t.cell(0, 1).paragraphs[i].runs[0].bold = True
    wl, wr = (7.0, 9.0) if d.sections[0].page_width < d.sections[0].page_height else (11.0, 14.5)
    t.cell(0, 0).width, t.cell(0, 1).width = Cm(wl), Cm(wr)
    para(d, ngay, italic=True, align=AL.RIGHT)

def signatures(d, cols):
    t = d.add_table(rows=1, cols=len(cols)); no_border(t)
    for i, (title, name) in enumerate(cols):
        set_cell(t.cell(0, i), [title, '(Ký, ghi rõ họ tên)', '', '', '', name], False, 12, AL.CENTER)
        ps = t.cell(0, i).paragraphs; ps[0].runs[0].font.bold = True; ps[1].runs[0].italic = True; ps[-1].runs[0].bold = True

def save(d, path, title):
    cp = d.core_properties; cp.author = 'NghiaVT'; cp.last_modified_by = 'NghiaVT'; cp.title = title
    d.save(path)
