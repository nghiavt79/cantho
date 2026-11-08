"""Hàm dùng chung cho bộ hồ sơ 28/10/2026: thay mã văn bản theo checklist V7, giữ nguyên định dạng run."""
import copy
import os
import re
import docx
from docx.text.paragraph import Paragraph
from docx.oxml.ns import qn

ROOT = r'D:\2026\CanTho\docs\28-10-2026'
OUT = os.path.join(ROOT, 'Ho_so_NCC')
HS = os.path.join(ROOT, 'Ho_so_muc_1.1_1.5_1.6')
BB = os.path.join(ROOT, 'BienBanKiemTra')

# Mã cũ (hồ sơ nhà thầu 26/10) -> mã theo checklist V7 (chị Vân 27/10/2026)
MA = [
    (r'kèm NCC-06, NCC-11, NCC-12, NCC-14', 'kèm 11 tài liệu'),
    (r'NCC-06', 'NCC-02'),
    (r'NCC-11', 'NCC-02, Tài liệu số 01'),
    (r'NCC-12', 'NCC-02, Tài liệu số 02'),
    (r'NCC-14', 'NCC-04'),
    (r'NCC-15', 'NCC-05'),
]


def all_paragraphs(d):
    """Mọi đoạn văn: thân, bảng lồng nhau, đầu trang, chân trang."""
    parts = [d.element.body]
    for s in d.sections:
        for hf in (s.header, s.footer, s.first_page_header, s.first_page_footer):
            if hf is not None and hf._element is not None:
                parts.append(hf._element)
    for part in parts:
        for p in part.iter(qn('w:p')):
            yield Paragraph(p, d)


def replace_in_paragraph(p, pairs):
    """Thay theo regex trên toàn đoạn; phần thay ghi vào run chứa ký tự đầu của chuỗi khớp."""
    n = 0
    for pat, new in pairs:
        while True:
            runs = [r for r in p.runs]
            text = ''.join(r.text for r in runs)
            m = re.search(pat, text)
            if not m:
                break
            s, e = m.span()
            pos = 0
            first = True
            for r in runs:
                rs, re_ = pos, pos + len(r.text)
                pos = re_
                if re_ <= s or rs >= e:
                    continue
                a, b = max(s, rs) - rs, min(e, re_) - rs
                if first:
                    r.text = r.text[:a] + new + r.text[b:]
                    first = False
                else:
                    r.text = r.text[:a] + r.text[b:]
            n += 1
            if new and re.search(pat, new):
                break
    return n


def replace_doc(d, pairs=MA):
    n = 0
    for p in all_paragraphs(d):
        if any(re.search(pat, p.text) for pat, _ in pairs):
            n += replace_in_paragraph(p, pairs)
    return n


def insert_after(p, text=None, like=None):
    """Chèn đoạn mới sau p, sao định dạng đoạn/run của `like` (mặc định p)."""
    src = like if like is not None else p
    new = copy.deepcopy(src._p)
    for r in new.findall(qn('w:r'))[1:]:
        new.remove(r)
    p._p.addnext(new)
    np_ = Paragraph(new, p._parent)
    if text is not None:
        if np_.runs:
            np_.runs[0].text = text
        else:
            np_.add_run(text)
    return np_


def set_par(p, text):
    """Đặt lại nội dung đoạn, giữ định dạng run đầu."""
    runs = p.runs
    runs[0].text = text
    for r in runs[1:]:
        r._r.getparent().remove(r._r)


def find_par(d, start):
    for p in all_paragraphs(d):
        if p.text.strip().startswith(start):
            return p
    raise KeyError(start)


def save(d, f):
    d.core_properties.author = 'MobiFone'
    d.core_properties.last_modified_by = 'MobiFone'
    os.makedirs(os.path.dirname(f), exist_ok=True)
    d.save(f)
