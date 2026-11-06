from pathlib import Path

from docx import Document
from docx.oxml import OxmlElement


BASE = Path(r"D:\2026\CanTho\docs\ho so ky thuat")
SRC = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.5.1_hoan_thien_truoc_gui.docx"
DST = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.6_bo_sung_kien_truc_hoan_chinh.docx"


def set_para_text(paragraph, text):
    paragraph._p.clear_content()
    paragraph.add_run(text)


def insert_paragraph_after(paragraph, text, style=None):
    new_p = OxmlElement("w:p")
    paragraph._p.addnext(new_p)
    new_para = paragraph._parent.add_paragraph()
    new_para._p = new_p
    if style:
        new_para.style = style
    new_para.add_run(text)
    return new_para


def replace_all_text(doc, replacements):
    def apply(text):
        for old, new in replacements.items():
            text = text.replace(old, new)
        return text

    for paragraph in doc.paragraphs:
        new_text = apply(paragraph.text)
        if new_text != paragraph.text:
            set_para_text(paragraph, new_text)

    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                new_text = apply(cell.text)
                if new_text != cell.text:
                    cell.text = new_text


def add_architecture_section(doc):
    heading = None
    idx = None
    for i, paragraph in enumerate(doc.paragraphs):
        if paragraph.text.strip() == "2. Mô hình kiến trúc hệ thống tổng thể":
            heading = paragraph
            idx = i
            break
    if heading is None:
        raise RuntimeError("Không tìm thấy mục V.2")

    # Reuse the blank paragraph after the heading as the opening paragraph.
    first = doc.paragraphs[idx + 1]
    set_para_text(
        first,
        "Mô hình kiến trúc tổng thể được đề xuất theo hướng phân lớp, tách bạch kênh truy cập, xử lý nghiệp vụ, dữ liệu, tích hợp, an toàn thông tin và vận hành giám sát. Cách tổ chức này phù hợp với hình thức thuê dịch vụ CNTT vì Chủ đầu tư kiểm soát được năng lực đầu ra, dữ liệu và chất lượng dịch vụ, trong khi đơn vị cung cấp dịch vụ vẫn có quyền đề xuất nền tảng kỹ thuật tương đương đáp ứng yêu cầu.",
    )

    contents = [
        "Sơ đồ khái quát kiến trúc hệ thống:",
        "Người dùng/cơ quan quản lý/nhà cung cấp/người mua/chuyên gia",
        "-> Lớp truy cập: cổng web bảo mật, giao diện thích ứng nhiều kích thước màn hình, đăng nhập và thông báo.",
        "-> Lớp ứng dụng nghiệp vụ: quản trị hệ thống, nhà cung cấp, sản phẩm - công nghệ, sự kiện - triển lãm, người mua, giao dịch, cổng thông tin, tiện ích tương tác số, trí tuệ nhân tạo, báo cáo - thống kê.",
        "-> Lớp dữ liệu: hồ sơ người dùng, hồ sơ nhà cung cấp, sản phẩm/công nghệ, sự kiện, giao dịch, hợp đồng, báo cáo, nhật ký và dữ liệu sao lưu.",
        "-> Lớp tích hợp dịch vụ và liên thông: thư điện tử, tin nhắn thương hiệu, chữ ký số, hợp đồng điện tử, thông báo tự động, giao diện lập trình ứng dụng và kết nối nền tảng ngoài theo phạm vi được phê duyệt.",
        "-> Lớp an toàn thông tin: xác thực, phân quyền, kiểm soát truy cập, bảo vệ dữ liệu cá nhân, nhật ký kiểm toán, kiểm soát tải tệp lên, sao lưu và khôi phục.",
        "-> Lớp vận hành giám sát: theo dõi trạng thái dịch vụ, cảnh báo, ghi nhận sự cố, báo cáo chất lượng dịch vụ, quản lý cấu hình và hỗ trợ người dùng.",
        "Trong mô hình này, lớp ứng dụng nghiệp vụ là trung tâm xử lý yêu cầu của người dùng; lớp dữ liệu bảo đảm tính toàn vẹn và khả năng khai thác lâu dài; lớp tích hợp chỉ mở các kết nối được phê duyệt và có kiểm soát; lớp an toàn thông tin và vận hành giám sát được áp dụng xuyên suốt thay vì đặt riêng ở một phân hệ độc lập.",
        "Ưu điểm của mô hình phân lớp là dễ mở rộng từng nhóm chức năng, dễ thay thế dịch vụ tích hợp khi cần, thuận lợi nghiệm thu theo đầu ra và giảm rủi ro phụ thuộc vào một công nghệ cụ thể. Mô hình cũng hỗ trợ phân quyền rõ giữa nhóm quản trị, cán bộ nghiệp vụ, nhà cung cấp, người mua và người dùng công khai.",
        "Yêu cầu đáp ứng của kiến trúc là bảo đảm truy cập ổn định qua môi trường web; dữ liệu tiếng Việt được lưu trữ, tìm kiếm, sao lưu và xuất/bàn giao có kiểm soát; các giao dịch quan trọng có nhật ký; các dịch vụ tích hợp có cấu hình bật/tắt; các chức năng trí tuệ nhân tạo chỉ đóng vai trò hỗ trợ và không thay thế quyết định nghiệp vụ.",
    ]

    current = first
    for text in contents:
        current = insert_paragraph_after(current, text)


def main():
    doc = Document(SRC)

    replace_all_text(
        doc,
        {
            "Lưu ảnh, video, tài liệu, hợp đồng, file báo cáo; có phân quyền và sao lưu.": "Lưu ảnh, video, tài liệu, hợp đồng, tệp báo cáo; có phân quyền và sao lưu.",
            "Lưu dữ liệu nghiệp vụ, file, hình ảnh, video, tài liệu, hợp đồng, backup.": "Lưu dữ liệu nghiệp vụ, tệp, hình ảnh, video, tài liệu, hợp đồng và dữ liệu sao lưu.",
            "tương thích đa kích thước màn hình (responsive)": "giao diện thích ứng với nhiều kích thước màn hình",
            "trí tuệ nhân tạo (AI)\n": "Trí tuệ nhân tạo (AI)\n",
            "giao diện lập trình ứng dụng (API) và tích hợp": "Giao diện lập trình ứng dụng (API) và tích hợp",
            "Dịch vụ tích hợp thư điện tử": "Dịch vụ tích hợp Thư điện tử",
            "thư điện tử (Email), thông báo trên hệ thống thông báo": "thư điện tử (Email), thông báo trên hệ thống",
            "thư điện tử (Email)/thông báo trên hệ thống": "thư điện tử (Email), thông báo trên hệ thống",
        },
    )
    add_architecture_section(doc)
    doc.save(DST)
    print(DST)


if __name__ == "__main__":
    main()
