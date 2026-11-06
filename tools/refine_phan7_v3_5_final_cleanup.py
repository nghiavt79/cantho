from pathlib import Path

from docx import Document


BASE = Path(r"D:\2026\CanTho\docs\ho so ky thuat")
SRC = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.4.1_tang_phan_tich_giam_trung_lap_viet_hoa.docx"
DST = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.5.1_hoan_thien_truoc_gui.docx"


def set_para_text(paragraph, text):
    paragraph._p.clear_content()
    paragraph.add_run(text)


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


def rewrite_table_cells(doc):
    # Table 1: original function list.
    table = doc.tables[0]
    table.rows[8].cells[1].text = "Tiện ích tương tác số"
    table.rows[8].cells[2].text = (
        "Trò chuyện trực tuyến (Chat).\n"
        "Gọi video (Video Call).\n"
        "Phát trực tuyến (Livestream).\n"
        "Mã QR sản phẩm.\n"
        "Liên kết mạng xã hội.\n"
        "Tích hợp sàn khác.\n"
        "Song ngữ Việt - Anh.\n"
        "Tham quan thực tế ảo 360 độ (VR360)."
    )
    table.rows[9].cells[2].text = (
        "Trợ lý tư vấn dùng trí tuệ nhân tạo (AI Chatbot).\n"
        "Tìm kiếm thông minh ứng dụng trí tuệ nhân tạo (AI).\n"
        "Tìm kiếm bằng giọng nói.\n"
        "Gợi ý sản phẩm.\n"
        "Gợi ý nhà cung cấp.\n"
        "Gợi ý công nghệ phù hợp."
    )
    table.rows[10].cells[1].text = "Tích hợp dịch vụ và liên thông"
    table.rows[10].cells[2].text = (
        "Tin nhắn thương hiệu (SMS Brandname).\n"
        "Thư điện tử (Email).\n"
        "Chữ ký số.\n"
        "Hợp đồng điện tử.\n"
        "Thông báo tự động.\n"
        "Kết nối giao diện lập trình ứng dụng (API).\n"
        "Kết nối nền tảng ngoài theo phạm vi được phê duyệt."
    )
    table.rows[11].cells[2].text = (
        "Tài nguyên vận hành ứng dụng.\n"
        "Quyền sử dụng phần mềm nền tảng, nếu có.\n"
        "Nền tảng quản trị dữ liệu và quyền sử dụng, nếu có.\n"
        "Sao lưu dữ liệu.\n"
        "Giám sát hệ thống.\n"
        "Bảo mật và an toàn thông tin.\n"
        "Đào tạo, chuyển giao.\n"
        "Bảo hành, bảo trì 05 năm."
    )

    # Table 8: analysis by function group.
    table = doc.tables[7]
    table.rows[8].cells[0].text = "Tiện ích tương tác số"
    table.rows[8].cells[1].text = (
        "Các tiện ích tương tác số được thiết kế theo hướng cấu hình theo phạm vi dịch vụ được phê duyệt, "
        "không gắn cứng vào một nhà cung cấp công nghệ cụ thể. Cơ sở lựa chọn là nhu cầu trao đổi trên sàn "
        "có thể diễn ra qua nhiều kênh, từ trò chuyện trực tuyến, gọi video, phát trực tuyến đến mã QR, mạng "
        "xã hội, song ngữ và tham quan thực tế ảo 360 độ (VR360). Ưu điểm là hệ thống có thể mở rộng kênh "
        "tương tác theo nhu cầu vận hành, đồng thời vẫn kiểm soát được bảo mật, nhật ký và chất lượng dịch vụ. "
        "Phương án này đáp ứng nhóm tiện ích phục vụ tương tác người dùng và trình diễn sản phẩm/công nghệ trên sàn."
    )
    table.rows[10].cells[0].text = "Tích hợp dịch vụ và liên thông"
    table.rows[10].cells[1].text = (
        "Tích hợp dịch vụ và liên thông được thiết kế như lớp kết nối có kiểm soát giữa sàn và các dịch vụ/nền tảng "
        "liên quan, thay vì đưa từng dịch vụ vào như một thành phần phụ thuộc cứng. Cơ sở lựa chọn là các dịch vụ "
        "như thư điện tử, tin nhắn thương hiệu, chữ ký số, hợp đồng điện tử, thông báo tự động và giao diện lập trình "
        "ứng dụng có thể thay đổi theo phạm vi thuê dịch vụ và nhà cung cấp tương ứng. Ưu điểm là giảm rủi ro khóa "
        "công nghệ, dễ thay thế, dễ kiểm soát nhật ký kết nối và trách nhiệm xử lý lỗi. Phương án này đáp ứng yêu cầu "
        "tích hợp được phê duyệt, có xác thực, phân quyền, cấu hình tham số, ghi nhật ký và tài liệu đặc tả."
    )
    table.rows[11].cells[1].text = (
        "Do dự án theo hình thức thuê dịch vụ CNTT, hạ tầng triển khai được mô tả bằng yêu cầu năng lực và mức chất "
        "lượng dịch vụ thay vì chỉ định máy chủ, hệ điều hành, cơ sở dữ liệu hoặc phần mềm thương mại cụ thể. Cơ sở lựa "
        "chọn là Chủ đầu tư cần kiểm soát khả năng sẵn sàng, an toàn, sao lưu, giám sát và quyền khai thác dữ liệu, trong "
        "khi đơn vị cung cấp dịch vụ cần có không gian đề xuất giải pháp tối ưu. Ưu điểm là bảo đảm cạnh tranh, tránh khóa "
        "công nghệ và vẫn xác định rõ đầu ra cần nghiệm thu. Phương án này đáp ứng yêu cầu tài nguyên vận hành ứng dụng, "
        "quyền sử dụng phần mềm nền tảng nếu có, nền tảng quản trị dữ liệu, sao lưu, giám sát, bảo mật, đào tạo, chuyển giao, "
        "bảo hành và bảo trì 05 năm theo phạm vi được phê duyệt."
    )

    # Table 9: management scope, avoiding overlap with interaction utilities.
    doc.tables[8].rows[5].cells[1].text = (
        "Quản lý cấu hình thư điện tử (Email), tin nhắn thương hiệu (SMS Brandname), chữ ký số, hợp đồng điện tử, "
        "trí tuệ nhân tạo (AI), giao diện lập trình ứng dụng (API) và các nền tảng ngoài theo quyền quản trị."
    )

    # Table 12: service/component list.
    table = doc.tables[11]
    table.rows[6].cells[1].text = "Dịch vụ thư điện tử (Email) và thông báo trên hệ thống"
    table.rows[8].cells[2].text = "Ký hợp đồng/tài liệu điện tử, lưu trạng thái, nhật ký kiểm toán (audit log)."


def refine_paragraphs(doc):
    for i, paragraph in enumerate(doc.paragraphs):
        text = paragraph.text.strip()
        if text.startswith("Hạ tầng dịch vụ được xem xét ở góc độ năng lực đầu ra"):
            set_para_text(
                paragraph,
                "Hạ tầng dịch vụ được xem xét ở góc độ năng lực đầu ra gồm khả năng xử lý đồng thời, lưu trữ, đường truyền, giám sát, sao lưu và khôi phục; mục này tập trung vào điều kiện vận hành thường xuyên của dịch vụ.",
            )
        if text.startswith("Yêu cầu về IPv6 được xác định là điều kiện sẵn sàng mở rộng"):
            set_para_text(paragraph, "Yêu cầu về khả năng hỗ trợ IPv6 được phân tích riêng tại Mục 7.")
        elif i == 98 and text == "":
            set_para_text(
                paragraph,
                "Nội dung an toàn thông tin tại mục này chỉ tập trung vào cơ sở lựa chọn biện pháp kỹ thuật và yêu cầu kiểm soát rủi ro. Các căn cứ tiêu chuẩn được nêu tại Mục II, vị trí trong kiến trúc được nêu tại Mục III, còn nội dung kiểm tra khi đưa dịch vụ vào vận hành được trình bày tại Mục VIII.",
            )
        elif i == 123 and text == "":
            set_para_text(
                paragraph,
                "Mục này trình bày phương thức kết nối giữa người dùng, phân hệ và hệ thống/dịch vụ bên ngoài. Khác với Mục 4 tập trung vào quản trị cấu hình tích hợp ở phía Chủ đầu tư, Mục 5 làm rõ kênh kết nối, đối tượng kết nối, yêu cầu xác thực, nhật ký và điều kiện liên thông dữ liệu.",
            )


def main():
    doc = Document(SRC)

    replace_all_text(
        doc,
        {
            "thư điện tử (thư điện tử (Email))": "thư điện tử (Email)",
            "Thư điện tử (thư điện tử (Email))": "Thư điện tử (Email)",
            "tham quan tham quan thực tế ảo 360 độ": "tham quan thực tế ảo 360 độ",
            "Tham quan tham quan thực tế ảo 360 độ": "Tham quan thực tế ảo 360 độ",
            "trợ lý tư vấn dùng trợ lý trò chuyện tự động dùng trí tuệ nhân tạo (AI Chatbot).": "Trợ lý tư vấn dùng trí tuệ nhân tạo (AI Chatbot).",
            "trí tuệ nhân tạo (AI) tìm kiếm thông minh": "Tìm kiếm thông minh ứng dụng trí tuệ nhân tạo (AI)",
            "tin nhắn thương hiệu (tin nhắn ngắn (SMS) Brandname)": "tin nhắn thương hiệu (SMS Brandname)",
            "Tin nhắn thương hiệu (tin nhắn ngắn (SMS) Brandname)": "Tin nhắn thương hiệu (SMS Brandname)",
            "Dịch vụ thư điện tử (Email)/thông báo trên hệ thống thông báo": "Dịch vụ thư điện tử (Email) và thông báo trên hệ thống",
            "nhật ký kiểm tra (audit) nhật ký": "nhật ký kiểm toán (audit log)",
            "nhật ký kiểm tra (audit)": "nhật ký kiểm toán (audit log)",
        },
    )
    rewrite_table_cells(doc)
    refine_paragraphs(doc)
    doc.save(DST)
    print(DST)


if __name__ == "__main__":
    main()
