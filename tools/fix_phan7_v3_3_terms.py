from __future__ import annotations

from pathlib import Path

from docx import Document


ROOT = Path(__file__).resolve().parents[1]
PATH = ROOT / "docs" / "ho so ky thuat" / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.3_viet_hoa_phan_tich.docx"

FIXES = [
    ("TRIỂN KHtrí tuệ nhân tạo (AI)", "TRIỂN KHAI"),
    ("máy tính cá nhân (máy tính cá nhân (PC))", "máy tính cá nhân (PC)"),
    ("máy tính bảng (máy tính bảng (Tablet))", "máy tính bảng (Tablet)"),
    ("thiết bị di động (thiết bị di động (Mobile))", "thiết bị di động (Mobile)"),
    ("email, tin nhắn ngắn (SMS)", "thư điện tử (Email), tin nhắn ngắn (SMS)"),
    ("Kênh truy cập web bảo mật", "Kênh truy cập trang thông tin điện tử bảo mật"),
    ("Dịch vụ Sàn giao dịch", "Dịch vụ Sàn giao dịch"),
]


def set_text(paragraph, text):
    if paragraph.runs:
        paragraph.runs[0].text = text
        for run in paragraph.runs[1:]:
            run.text = ""
    else:
        paragraph.add_run(text)


def fix_text(text):
    for src, dst in FIXES:
        text = text.replace(src, dst)
    return text


def main():
    doc = Document(str(PATH))
    changed = 0
    for para in doc.paragraphs:
        new_text = fix_text(para.text)
        if new_text != para.text:
            set_text(para, new_text)
            changed += 1
    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                for para in cell.paragraphs:
                    new_text = fix_text(para.text)
                    if new_text != para.text:
                        set_text(para, new_text)
                        changed += 1
    doc.save(str(PATH))
    print(f"changed={changed}")


if __name__ == "__main__":
    main()
