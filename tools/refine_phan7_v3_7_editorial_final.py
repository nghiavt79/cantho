from pathlib import Path

from docx import Document


BASE = Path(r"D:\2026\CanTho\docs\ho so ky thuat")
SRC = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.6_bo_sung_kien_truc_hoan_chinh.docx"
DST = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.7_chot_gui.docx"


def replace_all_text(doc, replacements):
    def apply(text):
        for old, new in replacements.items():
            text = text.replace(old, new)
        return text

    for paragraph in doc.paragraphs:
        new_text = apply(paragraph.text)
        if new_text != paragraph.text:
            paragraph._p.clear_content()
            paragraph.add_run(new_text)

    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                new_text = apply(cell.text)
                if new_text != cell.text:
                    cell.text = new_text


def soften_function_analysis(doc):
    table = doc.tables[7]
    rewrites = {
        2: (
            "Nhà cung cấp là chủ thể tạo nguồn cung công nghệ, vì vậy thiết kế cần chuẩn hóa hồ sơ pháp lý, năng lực, "
            "lĩnh vực công nghệ, gian hàng, hạng thành viên và lịch sử hoạt động. Hồ sơ nhà cung cấp cần được thẩm định "
            "trước khi công bố sản phẩm hoặc tham gia giao dịch, nên quy trình duyệt và lưu lịch sử chỉnh sửa là thành phần "
            "quan trọng. Cách quản lý theo hồ sơ chuẩn giúp nâng độ tin cậy của thông tin trên sàn, hỗ trợ phân loại nguồn "
            "cung và tạo căn cứ xử lý khi có thay đổi. Nội dung này đáp ứng yêu cầu duyệt hồ sơ, duyệt sản phẩm, quản lý "
            "bài viết giới thiệu, theo dõi lịch sử giao dịch và lịch sử chỉnh sửa."
        ),
        3: (
            "Sản phẩm và công nghệ là dữ liệu lõi của sàn. Thiết kế tổ chức thông tin theo hồ sơ có cấu trúc, gắn với danh "
            "mục nhóm sản phẩm, tài liệu, hình ảnh, video, trạng thái pháp lý và thông tin trưng bày. Người mua cần tra cứu, "
            "so sánh, đánh giá theo nhiều tiêu chí; cơ quan quản lý cần kiểm soát nguồn gốc và trạng thái công bố. Nhờ đó dữ "
            "liệu dễ tìm kiếm, dễ kiểm duyệt và có thể sử dụng lại cho phòng trưng bày, cổng thông tin hoặc các hoạt động xúc tiến."
        ),
        4: (
            "Sự kiện và triển lãm là kênh xúc tiến cung - cầu, kết hợp giữa hoạt động trực tuyến và hoạt động thực tế. Thiết kế "
            "đề xuất quản lý theo vòng đời sự kiện: tạo sự kiện, công bố, đăng ký tham dự, bố trí gian hàng, truyền thông, nhắc "
            "lịch, ghi nhận lượt quan tâm và tổng hợp kết quả. Vì mỗi sự kiện có nhiều mốc thời gian và nhiều nhóm tham gia, cách "
            "tiếp cận theo vòng đời giúp Chủ đầu tư kiểm soát kế hoạch truyền thông, mức độ tham gia và hiệu quả tổ chức sau sự kiện."
        ),
        5: (
            "Người mua đại diện cho phía cầu của thị trường khoa học và công nghệ, vì vậy thiết kế cần lưu được hồ sơ, nhu cầu tìm "
            "mua, giỏ hàng, đánh giá, trao đổi, lịch sử mua hàng và yêu cầu hỗ trợ. Nhu cầu công nghệ thường cần được làm rõ qua "
            "nhiều lần tương tác trước khi hình thành giao dịch; hệ thống vì vậy phải ghi nhận được hành trình của người mua, giúp "
            "nhà cung cấp phản hồi đúng nhu cầu và hỗ trợ cơ quan quản lý nhận diện xu hướng thị trường."
        ),
        7: (
            "Cổng thông tin là lớp tiếp cận công khai của sàn, kết hợp quản trị nội dung, danh mục công nghệ, tra cứu sản phẩm và "
            "đăng ký nhận thông tin. Người dùng bên ngoài thường bắt đầu từ nhu cầu tìm hiểu thị trường, tin tức, sự kiện hoặc sản "
            "phẩm trước khi đăng ký tương tác sâu hơn. Cách tổ chức này mở rộng phạm vi tiếp cận, nâng cao tính minh bạch của thông "
            "tin và tạo nguồn dữ liệu quan tâm phục vụ xúc tiến cung - cầu."
        ),
        8: (
            "Các tiện ích tương tác số được thiết kế theo hướng cấu hình theo phạm vi dịch vụ được phê duyệt, không gắn cứng vào "
            "một nhà cung cấp công nghệ cụ thể. Nhu cầu trao đổi trên sàn có thể diễn ra qua nhiều kênh như trò chuyện trực tuyến, "
            "gọi video, phát trực tuyến, mã QR, mạng xã hội, song ngữ và tham quan thực tế ảo 360 độ (VR360). Khi được quản lý theo "
            "cơ chế cấu hình, hệ thống có thể mở rộng kênh tương tác theo nhu cầu vận hành mà vẫn kiểm soát được bảo mật, nhật ký "
            "và chất lượng dịch vụ."
        ),
        10: (
            "Tích hợp dịch vụ và liên thông được thiết kế như lớp kết nối có kiểm soát giữa sàn và các dịch vụ/nền tảng liên quan, "
            "thay vì đưa từng dịch vụ vào như một thành phần phụ thuộc cứng. Các dịch vụ như thư điện tử, tin nhắn thương hiệu, chữ "
            "ký số, hợp đồng điện tử, thông báo tự động và giao diện lập trình ứng dụng có thể thay đổi theo phạm vi thuê dịch vụ. "
            "Cách tiếp cận này giảm rủi ro khóa công nghệ, thuận lợi thay thế, kiểm soát nhật ký kết nối và xác định trách nhiệm xử lý lỗi."
        ),
        12: (
            "Báo cáo - thống kê là lớp phục vụ quản lý, đánh giá hiệu quả vận hành và ra quyết định của Chủ đầu tư. Dữ liệu báo cáo "
            "được tổng hợp từ nhiều phân hệ như nhà cung cấp, sản phẩm, giao dịch, doanh thu, sự kiện và truy cập. Thiết kế cần bảo "
            "đảm nguồn dữ liệu rõ ràng, có kỳ thống kê, có quyền truy cập và có khả năng xuất dữ liệu; qua đó hỗ trợ báo cáo lãnh đạo, "
            "cơ quan quản lý và đánh giá chất lượng dịch vụ thuê."
        ),
    }
    for row_idx, text in rewrites.items():
        table.rows[row_idx].cells[1].text = text


def main():
    doc = Document(SRC)

    replace_all_text(
        doc,
        {
            "Dịch vụ tích hợp | thư điện tử (Email), tin nhắn ngắn": "Dịch vụ tích hợp | Thư điện tử (Email), tin nhắn ngắn",
            "thông báo trên hệ thống/tin nhắn thương hiệu": "thông báo trên hệ thống và tin nhắn thương hiệu",
            "thông báo trên hệ thống thông báo": "thông báo trên hệ thống",
            "Khai thác dashboard, báo cáo, thống kê và xuất dữ liệu.": "Khai thác bảng điều khiển (dashboard), báo cáo, thống kê và xuất dữ liệu.",
        },
    )

    # Direct cell edits are safer for table text that may not contain the visual separator used above.
    for table_idx, row_idx in [(4, 3), (5, 6)]:
        cell = doc.tables[table_idx].rows[row_idx].cells[1]
        if cell.text.startswith("thư điện tử"):
            cell.text = cell.text[0].upper() + cell.text[1:]

    doc.tables[9].rows[3].cells[1].text = (
        "Kết nối thư điện tử (Email), thông báo trên hệ thống và tin nhắn thương hiệu (SMS Brandname) "
        "theo cấu hình; có nhật ký gửi và trạng thái xử lý."
    )
    doc.tables[13].rows[3].cells[1].text = (
        "Kiểm tra thư điện tử (Email), thông báo trên hệ thống, tin nhắn ngắn (SMS), chữ ký số, hợp đồng "
        "điện tử, trí tuệ nhân tạo (AI), tham quan thực tế ảo 360 độ (VR360), giao diện lập trình ứng dụng "
        "(API) theo phạm vi thuê."
    )
    doc.tables[14].rows[4].cells[1].text = (
        "Khai thác bảng điều khiển (dashboard), báo cáo, thống kê và xuất dữ liệu."
    )

    soften_function_analysis(doc)

    doc.save(DST)
    print(DST)


if __name__ == "__main__":
    main()
