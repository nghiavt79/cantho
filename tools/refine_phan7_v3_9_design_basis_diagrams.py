from pathlib import Path

from docx import Document
from docx.oxml import OxmlElement
from docx.shared import Pt


BASE = Path(r"D:\2026\CanTho\docs\ho so ky thuat")
SRC = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3 - 05072026.docx"
DST = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.9.2_bo_sung_vai_tro_so_do_thiet_ke.docx"


def set_para_text(paragraph, text):
    paragraph._p.clear_content()
    run = paragraph.add_run(text)
    return run


def insert_paragraph_after(paragraph, text="", style=None, mono=False):
    new_p = OxmlElement("w:p")
    paragraph._p.addnext(new_p)
    new_para = paragraph._parent.add_paragraph()
    new_para._p = new_p
    if style:
        new_para.style = style
    run = new_para.add_run(text)
    if mono:
        run.font.name = "Courier New"
        run.font.size = Pt(9)
    return new_para


def find_para(doc, exact):
    for paragraph in doc.paragraphs:
        if paragraph.text.strip() == exact:
            return paragraph
    raise RuntimeError(f"Không tìm thấy đoạn: {exact}")


def replace_text_everywhere(doc, replacements):
    def repl(text):
        for old, new in replacements.items():
            text = text.replace(old, new)
        return text

    for paragraph in doc.paragraphs:
        new_text = repl(paragraph.text)
        if new_text != paragraph.text:
            set_para_text(paragraph, new_text)
    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                new_text = repl(cell.text)
                if new_text != cell.text:
                    cell.text = new_text


def enhance_section_a(doc):
    set_para_text(
        doc.paragraphs[45],
        "Thiết kế chi tiết được lập dưới góc độ đơn vị tư vấn, nhằm phân tích yêu cầu, lựa chọn phương án kỹ thuật và đề xuất giải pháp thuê dịch vụ công nghệ thông tin phù hợp với mục tiêu của dự án. Tài liệu là căn cứ để Chủ đầu tư lập hồ sơ mời thầu/hồ sơ yêu cầu, lựa chọn nhà cung cấp dịch vụ, tổ chức triển khai, kiểm tra nghiệm thu, đưa dịch vụ vào vận hành và giám sát chất lượng trong thời gian khai thác. Tài liệu không mô tả phương án tổ chức thi công nội bộ của một nhà cung cấp cụ thể, không chỉ định sản phẩm/công nghệ độc quyền và không thay thế hồ sơ dự thầu hoặc phương án triển khai chi tiết của đơn vị cung cấp dịch vụ.",
    )
    set_para_text(
        doc.paragraphs[49],
        "Không quy định các nội dung thuộc biện pháp thực hiện riêng của đơn vị cung cấp dịch vụ như quy trình phát triển nội bộ, tổ chức nhân sự chi tiết, công cụ kỹ thuật nội bộ, khóa kỹ thuật hoặc thông tin bí mật của nhà cung cấp, trừ trường hợp hồ sơ mời thầu, hợp đồng hoặc phạm vi được phê duyệt có yêu cầu cụ thể.",
    )
    set_para_text(
        doc.paragraphs[50],
        "Bảo đảm đủ cơ sở để Chủ đầu tư lập dự toán, xác định danh mục dịch vụ, xây dựng tiêu chí đánh giá kỹ thuật, lựa chọn nhà cung cấp, nghiệm thu kết quả thuê dịch vụ và tổ chức vận hành sau khi dịch vụ được cung cấp.",
    )


def enhance_chapter_v(doc):
    set_para_text(
        doc.paragraphs[104],
        "Giải pháp kỹ thuật được thuyết minh theo hướng thiết kế hệ thống, không chỉ mô tả danh sách chức năng. Trọng tâm của Chương V là xác định mô hình kiến trúc, cách phân lớp, nguyên tắc tổ chức dữ liệu, định hướng tích hợp, yêu cầu an toàn thông tin, khả năng giám sát vận hành và giá trị kỹ thuật của từng nhóm chức năng đối với việc thuê, nghiệm thu và khai thác dịch vụ.",
    )
    set_para_text(
        doc.paragraphs[106],
        "Thiết kế mô tả yêu cầu bằng tiêu chí đầu ra, mức chất lượng dịch vụ, yêu cầu dữ liệu, tích hợp, an toàn thông tin và khả năng vận hành, không chỉ định biện pháp triển khai nội bộ của nhà cung cấp.",
    )

    anchor = doc.paragraphs[110]
    additions = [
        "Các nhóm chức năng được xem như các mô-đun dịch vụ logic trong kiến trúc tổng thể. Mỗi mô-đun phải có phạm vi dữ liệu, quyền truy cập, trạng thái xử lý, nhật ký và tiêu chí nghiệm thu tương ứng để tránh tình trạng chỉ có chức năng nhưng thiếu điều kiện kiểm soát kỹ thuật.",
        "Định hướng kỹ thuật chung là tách lớp hiển thị, lớp xử lý nghiệp vụ, lớp dữ liệu và lớp tích hợp; các tiện ích như trí tuệ nhân tạo, thông báo, chữ ký số, hợp đồng điện tử hoặc tham quan thực tế ảo được đặt ở lớp dịch vụ tích hợp có cấu hình, nhật ký và khả năng bật/tắt.",
        "Giá trị của cách thiết kế này là giúp Chủ đầu tư đánh giá được năng lực dịch vụ theo đầu ra: khả năng phục vụ người dùng, mức độ an toàn dữ liệu, khả năng mở rộng, tính liên thông, chất lượng vận hành và khả năng bàn giao dữ liệu khi kết thúc hợp đồng.",
    ]
    current = anchor
    for text in additions:
        current = insert_paragraph_after(current, text)


def replace_architecture_diagram(doc):
    p = find_para(doc, "Sơ đồ khái quát kiến trúc hệ thống:")
    set_para_text(p, "Hình 7.1. Sơ đồ kiến trúc tổng thể đề xuất:")
    diagram = (
        "+----------------------+     +---------------------------+\n"
        "| Người dùng/CĐT       | --> | Lớp truy cập bảo mật      |\n"
        "| NCC/Người mua        |     | Web, đăng nhập, thông báo |\n"
        "+----------------------+     +-------------+-------------+\n"
        "                                      |\n"
        "                                      v\n"
        "+----------------------+     +---------------------------+\n"
        "| Lớp tích hợp         | <-> | Lớp ứng dụng nghiệp vụ    |\n"
        "| Email, SMS, ký số,   |     | 12 nhóm chức năng của sàn |\n"
        "| hợp đồng điện tử, AI |     +-------------+-------------+\n"
        "+----------+-----------+                   |\n"
        "           |                               v\n"
        "+----------+-----------+     +---------------------------+\n"
        "| Lớp an toàn thông tin| <-> | Lớp dữ liệu và báo cáo    |\n"
        "| phân quyền, nhật ký  |     | hồ sơ, sản phẩm, giao dịch|\n"
        "+----------+-----------+     +-------------+-------------+\n"
        "           |                               |\n"
        "           +-------------> Lớp vận hành, giám sát, sao lưu"
    )
    diagram_p = insert_paragraph_after(p, diagram, mono=True)

    current = insert_paragraph_after(diagram_p, "Hình 7.2. Mô hình phân lớp kiểm soát dịch vụ:")
    layered = (
        "[Lớp người dùng]\n"
        "        |\n"
        "[Lớp truy cập và trải nghiệm]\n"
        "        |\n"
        "[Lớp ứng dụng nghiệp vụ]\n"
        "        |\n"
        "[Lớp dữ liệu - báo cáo]\n"
        "        |\n"
        "[Lớp tích hợp dịch vụ]\n"
        "        |\n"
        "[Lớp an toàn thông tin và vận hành giám sát]"
    )
    insert_paragraph_after(current, layered, mono=True)

    # Remove the former linear list because the two figures above now carry the visual content.
    for paragraph in list(doc.paragraphs):
        text = paragraph.text.strip()
        if (
            text == "Người dùng/cơ quan quản lý/nhà cung cấp/người mua/chuyên gia"
            or text.startswith("-> Lớp truy cập:")
            or text.startswith("-> Lớp ứng dụng nghiệp vụ:")
            or text.startswith("-> Lớp dữ liệu:")
            or text.startswith("-> Lớp tích hợp dịch vụ")
            or text.startswith("-> Lớp an toàn thông tin:")
            or text.startswith("-> Lớp vận hành giám sát:")
        ):
            paragraph._element.getparent().remove(paragraph._element)


def add_connection_diagram(doc):
    anchor = None
    for paragraph in doc.paragraphs:
        if paragraph.text.strip().startswith("Mục này tập trung mô tả kênh kết nối"):
            anchor = paragraph
            break
    if anchor is None:
        raise RuntimeError("Không tìm thấy đoạn mở đầu mục Giải pháp kết nối")
    current = insert_paragraph_after(anchor, "Hình 7.3. Mô hình kết nối và liên thông dữ liệu:")
    diagram = (
        "Người dùng cuối <--> Cổng web Sàn giao dịch <--> Các phân hệ nghiệp vụ\n"
        "                                      |\n"
        "                                      +--> Dịch vụ thông báo: Email, SMS Brandname\n"
        "                                      +--> Dịch vụ ký số, hợp đồng điện tử\n"
        "                                      +--> Dịch vụ AI hỗ trợ tìm kiếm, tư vấn, gợi ý\n"
        "                                      +--> API kết nối hệ thống/nền tảng ngoài\n"
        "                                      +--> Kho dữ liệu, báo cáo, nhật ký kiểm toán"
    )
    insert_paragraph_after(current, diagram, mono=True)


def add_deployment_process_diagram(doc):
    anchor = find_para(doc, "VIII. PHƯƠNG ÁN TỔ CHỨC LẮP ĐẶT, TRIỂN KHAI HỆ THỐNG:")
    intro = insert_paragraph_after(
        anchor,
        "Phương án triển khai được mô tả theo chuỗi công việc đầu ra cần kiểm soát, không mô tả quy trình kỹ thuật nội bộ của nhà cung cấp. Trình tự dưới đây là căn cứ để phối hợp, kiểm tra điều kiện sẵn sàng và nghiệm thu dịch vụ.",
    )
    caption = insert_paragraph_after(intro, "Hình 7.4. Quy trình triển khai, kiểm tra và đưa dịch vụ vào vận hành:")
    diagram = (
        "[1. Thống nhất phạm vi, kế hoạch]\n"
        "          |\n"
        "[2. Thiết lập môi trường dịch vụ và cấu hình ban đầu]\n"
        "          |\n"
        "[3. Khởi tạo danh mục, tài khoản, phân quyền, dữ liệu mẫu]\n"
        "          |\n"
        "[4. Kiểm tra chức năng, dữ liệu, tích hợp, an toàn thông tin]\n"
        "          |\n"
        "[5. Đào tạo, vận hành thử, ghi nhận hiệu chỉnh]\n"
        "          |\n"
        "[6. Nghiệm thu, bàn giao tài liệu, đưa vào khai thác chính thức]"
    )
    insert_paragraph_after(caption, diagram, mono=True)


def soften_repeated_service_phrase(doc):
    replacements = {
        "phù hợp với hình thức thuê dịch vụ CNTT": "phù hợp với phạm vi và mục tiêu của dự án",
        "Do dự án theo hình thức thuê dịch vụ CNTT, ": "",
        "Với hình thức thuê dịch vụ CNTT, ": "",
        "Đối với thuê dịch vụ CNTT, ": "",
        "trong thiết kế chi tiết cho thuê dịch vụ": "trong thiết kế chi tiết",
    }
    replace_text_everywhere(doc, replacements)


def main():
    doc = Document(SRC)
    enhance_section_a(doc)
    enhance_chapter_v(doc)
    replace_architecture_diagram(doc)
    add_connection_diagram(doc)
    add_deployment_process_diagram(doc)
    soften_repeated_service_phrase(doc)

    doc.save(DST)
    print(DST)


if __name__ == "__main__":
    main()
