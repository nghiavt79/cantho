from copy import deepcopy
from pathlib import Path

from docx import Document
from docx.text.paragraph import Paragraph


SRC = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.4.5_bo_sung_phan_tich_chuong_IV.docx"
)
DST = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.4.6_sua_muc_luc_ket_luan_chuong_IV.docx"
)

LABEL = "K\u1ebft lu\u1eadn Ch\u01b0\u01a1ng IV"
BODY_PREFIX = LABEL + ": "
ANCHOR = "Ph\u01b0\u01a1ng \u00e1n k\u1ef9 thu\u1eadt \u0111\u01b0\u1ee3c l\u1ef1a ch\u1ecdn"


def insert_paragraph_after(paragraph: Paragraph, text: str = "", style=None) -> Paragraph:
    new_p = deepcopy(paragraph._p)
    paragraph._p.addnext(new_p)
    new_para = Paragraph(new_p, paragraph._parent)
    new_para.clear()
    if style is not None:
        new_para.style = style
    if text:
        new_para.add_run(text)
    return new_para


def clear_outline_level(paragraph: Paragraph) -> None:
    ppr = paragraph._p.get_or_add_pPr()
    outline = ppr.find(
        "{http://schemas.openxmlformats.org/wordprocessingml/2006/main}outlineLvl"
    )
    if outline is not None:
        ppr.remove(outline)


doc = Document(SRC)

# Remove the accidental long conclusion paragraph from the static TOC area.
first_heading_idx = next(
    (i for i, p in enumerate(doc.paragraphs) if p.style.name.startswith("Heading")),
    len(doc.paragraphs),
)
for p in list(doc.paragraphs[:first_heading_idx]):
    text = p.text.strip()
    if text.startswith(BODY_PREFIX) and ANCHOR in text:
        p._element.getparent().remove(p._element)

# Split the real body conclusion so Word will not treat a long sentence as a TOC item.
for p in doc.paragraphs:
    text = p.text.strip()
    if text.startswith(BODY_PREFIX) and ANCHOR in text:
        body = text[len(BODY_PREFIX) :].strip()
        p.clear()
        p.style = doc.styles["Normal"]
        run = p.add_run(LABEL)
        run.bold = True
        clear_outline_level(p)

        next_p = insert_paragraph_after(p, body, doc.styles["Normal"])
        clear_outline_level(next_p)
        break

doc.save(DST)
print(DST)
