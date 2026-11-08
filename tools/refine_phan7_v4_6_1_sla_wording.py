from pathlib import Path

from docx import Document


SRC = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.6_chuan_hoa_so_lieu_MobiFone.docx"
)
DST = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.6.1_chot_so_lieu_MobiFone_SLA.docx"
)


doc = Document(SRC)
replacements = {
    "mức chất lượng dịch vụ (SLA)": "mức chất lượng dịch vụ theo SLA",
    "nghiệm thu SLA": "nghiệm thu theo SLA",
}

for p in doc.paragraphs:
    for run in p.runs:
        if not run.text:
            continue
        new = run.text
        for old, value in replacements.items():
            new = new.replace(old, value)
        if new != run.text:
            run.text = new

for tbl in doc.tables:
    for row in tbl.rows:
        for cell in row.cells:
            for p in cell.paragraphs:
                for run in p.runs:
                    if not run.text:
                        continue
                    new = run.text
                    for old, value in replacements.items():
                        new = new.replace(old, value)
                    if new != run.text:
                        run.text = new

doc.save(DST)
print(DST)
