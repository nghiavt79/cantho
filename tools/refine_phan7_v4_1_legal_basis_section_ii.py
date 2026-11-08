from pathlib import Path

from docx import Document


BASE = Path(r"D:\2026\CanTho\docs\ho so ky thuat")
SRC = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.0.1_ky_thuat_hoa_so_do_chuyen_nghiep.docx"
DST = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.1_bo_sung_can_cu_phap_ly_muc_II.docx"


SECTION_II_ROWS = [
    (
        "1",
        "Tiêu chuẩn kỹ thuật ứng dụng CNTT",
        "Các tiêu chuẩn về web, dữ liệu, trao đổi thông tin, mã hóa, định dạng và khả năng tương thích.",
        "Thông tư 39/2017/TT-BTTTT và văn bản sửa đổi, thay thế nếu có.",
        "Bắt buộc theo phạm vi",
    ),
    (
        "2",
        "Kiến trúc và chuyển đổi số",
        "Nguyên tắc kiến trúc, thiết kế hệ thống số, tính mở, khả năng tích hợp và mở rộng.",
        "Luật Chuyển đổi số số 148/2025/QH15; Nghị định 224/2026/NĐ-CP và quy định hướng dẫn hiện hành.",
        "Bắt buộc theo phạm vi",
    ),
    (
        "3",
        "Kết nối, chia sẻ dữ liệu",
        "Quản lý kết nối, chia sẻ, khai thác dữ liệu giữa các hệ thống.",
        "Nghị định 47/2020/NĐ-CP về quản lý, kết nối và chia sẻ dữ liệu số của cơ quan nhà nước.",
        "Áp dụng khi kết nối",
    ),
    (
        "4",
        "Giao diện lập trình ứng dụng (API) và tích hợp",
        "API có xác thực, phân quyền, định dạng dữ liệu, truy vết, tài liệu đặc tả và xử lý lỗi.",
        "Nghị định 47/2020/NĐ-CP; Thông tư 39/2017/TT-BTTTT; quy định kiến trúc và kết nối hiện hành.",
        "Áp dụng khi tích hợp",
    ),
    (
        "5",
        "An toàn hệ thống thông tin theo cấp độ",
        "Xác định cấp độ, phương án bảo đảm an toàn thông tin, kiểm tra và giám sát.",
        "Nghị định 85/2016/NĐ-CP; Thông tư 12/2022/TT-BTTTT và văn bản hướng dẫn liên quan.",
        "Bắt buộc",
    ),
    (
        "6",
        "An ninh mạng",
        "Bảo vệ hệ thống, dữ liệu và hoạt động trên không gian mạng.",
        "Luật An ninh mạng số 116/2025/QH15 và văn bản hướng dẫn thi hành.",
        "Bắt buộc",
    ),
    (
        "7",
        "Bảo vệ dữ liệu cá nhân",
        "Thu thập, xử lý, lưu trữ, chia sẻ, bảo vệ quyền của chủ thể dữ liệu.",
        "Luật Bảo vệ dữ liệu cá nhân số 91/2025/QH15; Nghị định 356/2025/NĐ-CP.",
        "Bắt buộc",
    ),
    (
        "8",
        "Giao dịch điện tử và chữ ký điện tử",
        "Hợp đồng điện tử, thông điệp dữ liệu, chữ ký điện tử, dịch vụ tin cậy và lưu trữ hồ sơ điện tử.",
        "Luật Giao dịch điện tử số 20/2023/QH15; Nghị định 23/2025/NĐ-CP.",
        "Theo phạm vi triển khai",
    ),
    (
        "9",
        "Truy cập web và truyền dữ liệu bảo mật",
        "Mã hóa đường truyền, chứng thư số, phiên truy cập, tên miền và giao thức bảo mật.",
        "Thông tư 39/2017/TT-BTTTT; quy định về an toàn thông tin hiện hành.",
        "Bắt buộc",
    ),
    (
        "10",
        "Dữ liệu, tiếng Việt và định dạng trao đổi",
        "Unicode tiếng Việt, dữ liệu có cấu trúc, định dạng phổ biến, xuất/nhập dữ liệu.",
        "Thông tư 39/2017/TT-BTTTT và tiêu chuẩn kỹ thuật hiện hành.",
        "Bắt buộc",
    ),
    (
        "11",
        "Tài liệu và hồ sơ điện tử",
        "PDF, văn bản, bảng tính, ảnh, video, hồ sơ điện tử và tài liệu lưu trữ liên quan.",
        "Luật Giao dịch điện tử số 20/2023/QH15; tiêu chuẩn kỹ thuật hiện hành.",
        "Theo phạm vi triển khai",
    ),
    (
        "12",
        "IPv6",
        "Sẵn sàng IPv4/IPv6, tên miền, truy cập bảo mật và kết nối khi hạ tầng đáp ứng.",
        "Thông tư 39/2017/TT-BTTTT và yêu cầu hạ tầng hiện hành.",
        "Theo yêu cầu hạ tầng",
    ),
    (
        "13",
        "Hạ tầng và cung cấp dịch vụ CNTT",
        "Tài nguyên tính toán, lưu trữ, mở rộng, sao lưu, khôi phục, giám sát và bàn giao dữ liệu.",
        "Luật Chuyển đổi số số 148/2025/QH15; Nghị định 224/2026/NĐ-CP; quy định quản lý thuê dịch vụ, đầu tư ứng dụng CNTT hiện hành.",
        "Bắt buộc",
    ),
    (
        "14",
        "Chất lượng dịch vụ",
        "Tính sẵn sàng, thời gian phản hồi, thời gian khắc phục, mức chất lượng dịch vụ và báo cáo vận hành.",
        "Quy định quản lý thuê dịch vụ, hợp đồng và các tiêu chí chất lượng được phê duyệt.",
        "Bắt buộc",
    ),
    (
        "15",
        "Nhật ký, giám sát và ứng cứu sự cố",
        "Nhật ký truy cập, thao tác, tích hợp, cảnh báo, truy vết sự cố và báo cáo xử lý.",
        "Nghị định 85/2016/NĐ-CP; Thông tư 12/2022/TT-BTTTT; quy định an toàn thông tin hiện hành.",
        "Bắt buộc",
    ),
]


def set_para(paragraph, text):
    paragraph._p.clear_content()
    paragraph.add_run(text)


def remove_row(row):
    row._element.getparent().remove(row._element)


def normalize_section_ii_table(doc):
    table = doc.tables[2]

    if len(table.columns) == 4:
        table.add_column(1200000)

    target_rows = len(SECTION_II_ROWS) + 1
    while len(table.rows) < target_rows:
        table.add_row()
    while len(table.rows) > target_rows:
        remove_row(table.rows[-1])

    headers = [
        "STT",
        "Nhóm quy chuẩn, tiêu chuẩn/yêu cầu kỹ thuật",
        "Nội dung áp dụng",
        "Căn cứ pháp lý, văn bản tham chiếu",
        "Mức áp dụng",
    ]
    for i, header in enumerate(headers):
        table.rows[0].cells[i].text = header

    for row, values in zip(table.rows[1:], SECTION_II_ROWS):
        for i, value in enumerate(values):
            row.cells[i].text = value


def clean_language(doc):
    replacements = {
        "phải có cơ chế cấu hình và truy vết và điều kiện sử dụng rõ ràng": "phải có cơ chế cấu hình, truy vết và điều kiện sử dụng rõ ràng",
        "giao diện lập trình ứng dụng (API) tích hợp có xác thực, phân quyền và truy vết và tài liệu đặc tả": "giao diện lập trình ứng dụng (API) tích hợp có cơ chế xác thực, phân quyền, truy vết và tài liệu đặc tả",
        "Căn cứ thiết kế các tiêu chuẩn nêu trên là bảo đảm": "Các tiêu chuẩn nêu trên được lựa chọn nhằm bảo đảm",
        "Kết nối nền tảng ngoài theo phạm vi xác định": "Kết nối nền tảng ngoài theo phạm vi triển khai",
        "kết nối nền tảng ngoài theo phạm vi xác định": "kết nối nền tảng ngoài theo phạm vi triển khai",
        "theo phạm vi xác định": "theo phạm vi triển khai",
        "nội dung cài đặt được hiểu là": "Nội dung cài đặt được hiểu là",
        "chuyển giao được hiểu là": "Chuyển giao được hiểu là",
        "trí tuệ nhân tạo (AI)": "Trí tuệ nhân tạo (AI)",
    }

    for paragraph in doc.paragraphs:
        text = paragraph.text
        for old, new in replacements.items():
            text = text.replace(old, new)
        if text != paragraph.text:
            set_para(paragraph, text)

    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                text = cell.text
                for old, new in replacements.items():
                    text = text.replace(old, new)
                # Avoid over-capitalizing in the middle of sentences for AI after broad replacement.
                text = text.replace("Ứng dụng Trí tuệ nhân tạo (AI)", "Ứng dụng trí tuệ nhân tạo (AI)")
                text = text.replace("dịch vụ Trí tuệ nhân tạo (AI)", "dịch vụ trí tuệ nhân tạo (AI)")
                text = text.replace("cho Trí tuệ nhân tạo (AI)", "cho trí tuệ nhân tạo (AI)")
                text = text.replace("vụ Trí tuệ nhân tạo (AI)", "vụ trí tuệ nhân tạo (AI)")
                if text != cell.text:
                    cell.text = text


def remove_trailing_blank_paragraphs(doc):
    while doc.paragraphs and not doc.paragraphs[-1].text.strip():
        p = doc.paragraphs[-1]._element
        p.getparent().remove(p)


def main():
    doc = Document(SRC)
    normalize_section_ii_table(doc)
    clean_language(doc)
    remove_trailing_blank_paragraphs(doc)
    doc.save(DST)
    print(DST)


if __name__ == "__main__":
    main()
