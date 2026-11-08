"""Hàm dựng văn bản Word dùng chung (nền định dạng: mẫu NCC-15, Times New Roman 13)."""
import copy
import docx
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Pt, Cm

BASE = r'D:\2026\CanTho\docs\ke hoach thi cong, van hanh\Bieu_mau_NCC_TT41\1_Bieu_mau_day_du\NCC-15 Bao cao ket qua khac phuc loi sau van hanh thu.docx'
AL = {'both': WD_ALIGN_PARAGRAPH.JUSTIFY, 'center': WD_ALIGN_PARAGRAPH.CENTER,
      'left': WD_ALIGN_PARAGRAPH.LEFT, 'right': WD_ALIGN_PARAGRAPH.RIGHT}

HD = ('Hợp đồng số HD2600273782_2610221351 ngày 22 tháng 10 năm 2026 giữa Trung tâm Ứng dụng tiến bộ khoa học '
      'và công nghệ thành phố Cần Thơ và Tổng Công ty Viễn thông MobiFone thực hiện Gói thầu số 4: Thuê dịch vụ '
      'thiết lập, vận hành trang web Sàn giao dịch công nghệ Cần Thơ')
GOI = ('Gói thầu số 4: Thuê dịch vụ thiết lập, vận hành trang web Sàn giao dịch công nghệ Cần Thơ')


def new_doc(landscape=False):
    d = docx.Document(BASE)
    body = d.element.body
    for el in list(body):
        if not el.tag.endswith('}sectPr'):
            body.remove(el)
    s = d.sections[0]
    if landscape:
        s.page_width, s.page_height = Cm(29.7), Cm(21.0)
        s.left_margin, s.right_margin = Cm(2.0), Cm(1.5)
    else:
        s.page_width, s.page_height = Cm(21.0), Cm(29.7)
        s.left_margin, s.right_margin = Cm(3.0), Cm(1.5)
        s.orientation = 0
    s.top_margin, s.bottom_margin = Cm(2.0), Cm(2.0)
    return d


def text_width(d):
    s = d.sections[-1]
    return s.page_width - s.left_margin - s.right_margin


def P(d, parts, align='both', indent=True, size=None, after=None, before=None, keep=False):
    """parts: chuỗi hoặc danh sách (text, 'b'/'i'/'bi'/'')."""
    p = d.add_paragraph()
    _fill(p, parts, size)
    p.alignment = AL[align]
    pf = p.paragraph_format
    if indent and align == 'both':
        pf.first_line_indent = Cm(1.0)
    if after is not None:
        pf.space_after = Pt(after)
    if before is not None:
        pf.space_before = Pt(before)
    if keep:
        pf.keep_with_next = True
    return p


def _fill(p, parts, size=None):
    if isinstance(parts, str):
        parts = [(parts, '')]
    for t, f in parts:
        r = p.add_run(t)
        r.bold = 'b' in f
        r.italic = 'i' in f
        if size:
            r.font.size = Pt(size)


def H(d, text):
    return P(d, [(text, 'b')], align='left', indent=False, keep=True, before=6)


def _cell_width(cell, w):
    tcPr = cell._tc.get_or_add_tcPr()
    tcW = tcPr.find(qn('w:tcW'))
    if tcW is None:
        tcW = OxmlElement('w:tcW'); tcPr.append(tcW)
    tcW.set(qn('w:w'), str(int(w / 635))); tcW.set(qn('w:type'), 'dxa')


def _fix_grid(t, ws):
    """Cố định độ rộng cột (tblGrid + layout fixed) để Word không tự co giãn."""
    tblPr = t._tbl.tblPr
    lay = OxmlElement('w:tblLayout'); lay.set(qn('w:type'), 'fixed'); tblPr.append(lay)
    for gc, w in zip(t._tbl.tblGrid.findall(qn('w:gridCol')), ws):
        gc.set(qn('w:w'), str(int(w / 635)))
    for r in t.rows:
        for c, w in zip(r.cells, ws):
            _cell_width(c, w)


def _shade(cell, fill):
    tcPr = cell._tc.get_or_add_tcPr()
    shd = OxmlElement('w:shd'); shd.set(qn('w:val'), 'clear'); shd.set(qn('w:color'), 'auto'); shd.set(qn('w:fill'), fill)
    tcPr.append(shd)


def _cell_text(cell, parts, size, align, bold=False):
    p = cell.paragraphs[0]
    lines = parts if isinstance(parts, list) and parts and isinstance(parts[0], list) else None
    if isinstance(parts, str):
        chunks = parts.split('\n')
        for i, line in enumerate(chunks):
            pp = p if i == 0 else cell.add_paragraph()
            _fill(pp, [(line, 'b' if bold else '')], size)
            pp.alignment = AL[align]
            pp.paragraph_format.space_after = Pt(2)
        return
    _fill(p, parts, size)
    p.alignment = AL[align]
    p.paragraph_format.space_after = Pt(2)


def T(d, widths, header, rows, size=11, aligns=None, header_size=None):
    """Bảng có kẻ ô; widths theo tỷ lệ, tự co theo bề rộng trang."""
    tot = sum(widths); tw = text_width(d)
    ws = [tw * w / tot for w in widths]
    t = d.add_table(rows=1 + len(rows), cols=len(widths))
    t.style = 'Table Grid'
    t.alignment = 1
    _fix_grid(t, ws)
    aligns = aligns or ['center'] + ['left'] * (len(widths) - 1)
    for j, h in enumerate(header):
        c = t.rows[0].cells[j]; _cell_width(c, ws[j]); _shade(c, 'D9E2F3')
        _cell_text(c, h, header_size or size, 'center', bold=True)
    trPr = t.rows[0]._tr.get_or_add_trPr()
    th = OxmlElement('w:tblHeader'); th.set(qn('w:val'), 'true'); trPr.append(th)
    for i, row in enumerate(rows, 1):
        for j, v in enumerate(row):
            c = t.rows[i].cells[j]; _cell_width(c, ws[j])
            _cell_text(c, v, size, aligns[j])
    d.add_paragraph().paragraph_format.space_after = Pt(0)
    return t


def merge_row(t, i, text, size=11, bold=True, fill='F2F2F2'):
    r = t.rows[i]
    c = r.cells[0].merge(r.cells[-1])
    for p in c.paragraphs[1:]:
        p._p.getparent().remove(p._p)
    p = c.paragraphs[0]
    for r_ in list(p.runs):
        r_._r.getparent().remove(r_._r)
    _fill(p, [(text, 'b' if bold else '')], size)
    p.alignment = AL['left']
    _shade(c, fill)


def quoc_hieu(d, date_line):
    for t, f in [('CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM', 'b'), ('Độc lập - Tự do - Hạnh phúc', 'b'), ('───────────────', '')]:
        P(d, [(t, f)], align='center', indent=False, after=0)
    P(d, [(date_line, 'i')], align='right', indent=False, before=6)


def header_cv(d, so, trich_yeu, date_line):
    """Tiêu đề công văn MobiFone: bảng 2 cột không kẻ."""
    tw = text_width(d)
    t = d.add_table(rows=1, cols=2)
    t.alignment = 1
    ws = [tw * 0.42, tw * 0.58]
    _fix_grid(t, ws)
    left = [('TỔNG CÔNG TY VIỄN THÔNG MOBIFONE', 'b'), ('MOBIFONE CẦN THƠ', 'b'), ('──────────', ''), (so, ''), (trich_yeu, '')]
    right = [('CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM', 'b'), ('Độc lập - Tự do - Hạnh phúc', 'b'), ('───────────────', ''), (date_line, 'i')]
    for j, lines in enumerate([left, right]):
        c = t.rows[0].cells[j]; _cell_width(c, ws[j])
        for k, (txt, f) in enumerate(lines):
            p = c.paragraphs[0] if k == 0 else c.add_paragraph()
            _fill(p, [(txt, f)], 11.5 if f == 'b' else (12 if k == 4 else None))
            p.alignment = AL['center']; p.paragraph_format.space_after = Pt(2)
    d.add_paragraph()


def sign_grid(d, parties, note='(Ký, ghi rõ họ tên)'):
    """parties: [(chức danh, họ tên)] — tối đa 3 cột một hàng, không tách trang."""
    tw = text_width(d)
    k = min(3, len(parties))
    n = (len(parties) + k - 1) // k
    t = d.add_table(rows=n, cols=k)
    t.alignment = 1
    for r in t.rows:
        trPr = r._tr.get_or_add_trPr()
        cs = OxmlElement('w:cantSplit'); cs.set(qn('w:val'), 'true'); trPr.append(cs)
    if d.paragraphs:
        d.paragraphs[-1].paragraph_format.keep_with_next = True
    _fix_grid(t, [tw / k] * k)
    for i, (title, name) in enumerate(parties):
        c = t.rows[i // k].cells[i % k]
        lines = [(title, 'b'), (note, 'i'), ('', ''), ('', ''), ('', ''), (name, 'b'), ('', '')]
        for k, (txt, f) in enumerate(lines):
            p = c.paragraphs[0] if k == 0 else c.add_paragraph()
            _fill(p, [(txt, f)], 12)
            p.alignment = AL['center']; p.paragraph_format.space_after = Pt(0)
    return t


def sign_cv(d, noi_nhan, title, name, note='(Ký, ghi rõ họ tên, đóng dấu)'):
    tw = text_width(d)
    t = d.add_table(rows=1, cols=2)
    ws = [tw * 0.48, tw * 0.52]
    _fix_grid(t, ws)
    c0, c1 = t.rows[0].cells
    p = c0.paragraphs[0]; _fill(p, [('Nơi nhận:', 'bi')], 12); p.paragraph_format.space_after = Pt(0)
    for x in noi_nhan:
        p = c0.add_paragraph(); _fill(p, [(x, '')], 11); p.paragraph_format.space_after = Pt(0)
    for k, (txt, f) in enumerate([(title, 'b'), (note, 'i'), ('', ''), ('', ''), ('', ''), ('', ''), (name, 'b')]):
        p = c1.paragraphs[0] if k == 0 else c1.add_paragraph()
        _fill(p, [(txt, f)], 12); p.alignment = AL['center']; p.paragraph_format.space_after = Pt(0)


def save(d, f):
    d.core_properties.author = 'MobiFone'
    d.core_properties.last_modified_by = 'MobiFone'
    d.save(f)
