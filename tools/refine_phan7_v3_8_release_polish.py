from pathlib import Path

from docx import Document


BASE = Path(r"D:\2026\CanTho\docs\ho so ky thuat")
SRC = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.7_chot_gui.docx"
DST = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.8.1_ban_phat_hanh.docx"


def set_para_text(paragraph, text):
    paragraph._p.clear_content()
    paragraph.add_run(text)


def main():
    doc = Document(SRC)

    # V.5: keep the boundary clear, but remove the self-referential tone.
    for paragraph in doc.paragraphs:
        if paragraph.text.strip().startswith("Mục này trình bày phương thức kết nối giữa người dùng"):
            set_para_text(
                paragraph,
                "Mục này tập trung mô tả kênh kết nối, đối tượng kết nối, yêu cầu xác thực, nhật ký và điều kiện liên thông dữ liệu giữa các thành phần trong và ngoài hệ thống.",
            )

    # Initial function list.
    table = doc.tables[0]
    table.rows[9].cells[2].text = (
        "Trợ lý tư vấn ứng dụng trí tuệ nhân tạo (AI Chatbot).\n"
        "Tìm kiếm thông minh ứng dụng trí tuệ nhân tạo (AI).\n"
        "Tìm kiếm bằng giọng nói.\n"
        "Gợi ý sản phẩm.\n"
        "Gợi ý nhà cung cấp.\n"
        "Gợi ý công nghệ phù hợp."
    )
    table.rows[12].cells[2].text = (
        "Báo cáo nhà cung cấp.\n"
        "Báo cáo sản phẩm.\n"
        "Báo cáo giao dịch.\n"
        "Báo cáo doanh thu.\n"
        "Báo cáo sự kiện.\n"
        "Thống kê lượt truy cập.\n"
        "Xuất dữ liệu dưới định dạng bảng tính, PDF."
    )

    # VI.4: make the database requirement less product-like.
    doc.tables[11].rows[4].cells[1].text = "Nền tảng quản trị dữ liệu"
    doc.tables[11].rows[4].cells[2].text = "Hỗ trợ dữ liệu có cấu trúc, giao dịch, Unicode, phân quyền, sao lưu/khôi phục."
    doc.tables[11].rows[4].cells[3].text = "Không chỉ định hệ quản trị dữ liệu cụ thể."

    # Use Vietnamese-only term where sufficient.
    for paragraph in doc.paragraphs:
        if "nhật ký kiểm toán (audit log)" in paragraph.text:
            set_para_text(paragraph, paragraph.text.replace("nhật ký kiểm toán (audit log)", "nhật ký kiểm toán"))
    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                if "nhật ký kiểm toán (audit log)" in cell.text:
                    cell.text = cell.text.replace("nhật ký kiểm toán (audit log)", "nhật ký kiểm toán")

    doc.save(DST)
    print(DST)


if __name__ == "__main__":
    main()
