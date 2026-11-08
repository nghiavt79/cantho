from pathlib import Path

from docx import Document


BASE = Path(r"D:\2026\CanTho\docs\ho so ky thuat")
SRC = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.2_hoan_thien_muc_II_can_cu_phap_ly.docx"
DST = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.3_chot_muc_II_can_cu_phap_ly.docx"


def replace_in_doc(doc, replacements):
    for paragraph in doc.paragraphs:
        text = paragraph.text
        for old, new in replacements.items():
            text = text.replace(old, new)
        if text != paragraph.text:
            paragraph._p.clear_content()
            paragraph.add_run(text)

    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                text = cell.text
                for old, new in replacements.items():
                    text = text.replace(old, new)
                if text != cell.text:
                    cell.text = text


def main():
    doc = Document(SRC)

    # Section II, STT 14 and 15.
    table = doc.tables[2]
    table.rows[14].cells[3].text = (
        "Nghị định 224/2026/NĐ-CP; Nghị định 45/2026/NĐ-CP; Thông tư 16/2024/TT-BTTTT "
        "về triển khai, giám sát triển khai, nghiệm thu, xác định yêu cầu chất lượng dịch vụ "
        "và nội dung đặc thù của hợp đồng thuê dịch vụ CNTT theo yêu cầu riêng; hồ sơ mời thầu/"
        "hồ sơ yêu cầu, hợp đồng cung cấp dịch vụ, mức chất lượng dịch vụ (SLA) và các tiêu chí "
        "chất lượng được cấp có thẩm quyền phê duyệt."
    )
    table.rows[15].cells[3].text = (
        "Nghị định 85/2016/NĐ-CP; Thông tư 12/2022/TT-BTTTT; Thông tư 16/2024/TT-BTTTT; "
        "quy định an toàn thông tin hiện hành."
    )

    replace_in_doc(
        doc,
        {
            "Cấu trúc Buyer-Demand giúp hệ thống lưu vết": "Cấu trúc Người mua - Nhu cầu (Buyer-Demand) giúp hệ thống lưu vết",
            "Tìm kiếm thông minh ứng dụng Trí tuệ nhân tạo (AI).": "Tìm kiếm thông minh ứng dụng trí tuệ nhân tạo (AI).",
            "Các dịch vụ tích hợp được xác định phải được cấu hình": "Các dịch vụ tích hợp thuộc phạm vi triển khai phải được cấu hình",
            "Triển khai theo phạm vi triển khai.": "Thực hiện theo phạm vi dịch vụ.",
            "có cơ chế cấu hình và truy vết và khả năng bật/tắt": "có cơ chế cấu hình, truy vết và khả năng bật/tắt",
        },
    )

    doc.save(DST)
    print(DST)


if __name__ == "__main__":
    main()
