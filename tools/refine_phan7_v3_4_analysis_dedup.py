from pathlib import Path

from docx import Document


BASE = Path(r"D:\2026\CanTho\docs\ho so ky thuat")
SRC = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.3_viet_hoa_phan_tich.docx"
DST = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.4.1_tang_phan_tich_giam_trung_lap_viet_hoa.docx"


def set_para_text(paragraph, text):
    paragraph._p.clear_content()
    paragraph.add_run(text)


def delete_paragraph(paragraph):
    element = paragraph._element
    element.getparent().remove(element)


def replace_exact_paragraph(doc, old, new):
    for paragraph in doc.paragraphs:
        if paragraph.text.strip() == old:
            set_para_text(paragraph, new)
            return True
    return False


def replace_text_in_paragraphs(doc, old, new):
    changed = 0
    for paragraph in doc.paragraphs:
        if old in paragraph.text:
            set_para_text(paragraph, paragraph.text.replace(old, new))
            changed += 1
    return changed


def replace_contains_paragraph(doc, needle, new):
    for paragraph in doc.paragraphs:
        if needle in paragraph.text:
            set_para_text(paragraph, new)
            return True
    return False


def rewrite_solution_table(doc):
    table = doc.tables[7]
    analyses = {
        "Quản trị hệ thống": (
            "Nhóm quản trị hệ thống được lựa chọn làm lớp kiểm soát nền vì Sàn giao dịch có nhiều nhóm người dùng, "
            "nhiều quyền thao tác và yêu cầu truy vết trách nhiệm trong suốt thời gian thuê dịch vụ. Thiết kế đề xuất "
            "quản lý tập trung tài khoản, hồ sơ người dùng, vai trò, quyền theo chức năng, tham số vận hành, thông báo "
            "và nhật ký thao tác. Ưu điểm là giảm rủi ro cấp quyền sai, hỗ trợ kiểm tra sau sự kiện và giúp Chủ đầu tư "
            "chủ động kiểm soát dịch vụ mà không phụ thuộc vào công cụ nội bộ của đơn vị cung cấp. Phương án này đáp ứng "
            "các yêu cầu đăng nhập, đăng xuất, quên mật khẩu, phân quyền, cấu hình, thông báo qua thư điện tử/web và giao diện "
            "tương thích máy tính, máy tính bảng, thiết bị di động."
        ),
        "Quản lý nhà cung cấp": (
            "Nhà cung cấp là chủ thể tạo nguồn cung công nghệ, vì vậy thiết kế cần chuẩn hóa hồ sơ pháp lý, năng lực, lĩnh vực "
            "công nghệ, gian hàng, hạng thành viên và lịch sử hoạt động. Cơ sở lựa chọn là hồ sơ nhà cung cấp thường cần được "
            "thẩm định trước khi công bố sản phẩm hoặc tham gia giao dịch. Ưu điểm của phương án quản lý theo hồ sơ chuẩn và quy "
            "trình duyệt là nâng cao độ tin cậy của thông tin trên sàn, hỗ trợ phân loại nguồn cung và tạo căn cứ xử lý khi có "
            "thay đổi. Phương án này đáp ứng yêu cầu duyệt hồ sơ, duyệt sản phẩm, quản lý bài viết giới thiệu, theo dõi lịch sử "
            "giao dịch và lịch sử chỉnh sửa."
        ),
        "Quản lý sản phẩm và công nghệ": (
            "Sản phẩm và công nghệ là dữ liệu lõi của sàn, do đó thiết kế lựa chọn mô hình hồ sơ có cấu trúc, gắn với danh mục "
            "nhóm sản phẩm, hồ sơ công nghệ, tài liệu, hình ảnh, video, trạng thái pháp lý và thông tin trưng bày. Cơ sở lựa chọn "
            "là người mua cần tra cứu, so sánh và đánh giá thông tin theo nhiều tiêu chí, trong khi cơ quan quản lý cần kiểm soát "
            "nguồn gốc và trạng thái công bố. Ưu điểm là dữ liệu dễ tìm kiếm, dễ kiểm duyệt, dễ đưa vào phòng trưng bày hoặc các "
            "kênh giới thiệu khác. Phương án này đáp ứng yêu cầu quản lý danh mục sản phẩm, hồ sơ công nghệ, phòng trưng bày, vị trí "
            "trưng bày và trạng thái pháp lý sản phẩm."
        ),
        "Quản lý sự kiện - triển lãm": (
            "Sự kiện và triển lãm là kênh xúc tiến cung - cầu, kết hợp giữa hoạt động trực tuyến và hoạt động thực tế. Thiết kế đề "
            "xuất quản lý theo vòng đời sự kiện: tạo sự kiện, công bố, đăng ký tham dự, bố trí gian hàng, truyền thông, nhắc lịch, "
            "ghi nhận lượt quan tâm và tổng hợp kết quả. Cơ sở lựa chọn là mỗi sự kiện có nhiều mốc thời gian, nhiều nhóm tham gia "
            "và cần số liệu đánh giá sau khi kết thúc. Ưu điểm là giúp Chủ đầu tư kiểm soát kế hoạch truyền thông, mức độ tham gia "
            "và hiệu quả tổ chức. Phương án này đáp ứng yêu cầu hội thảo, triển lãm, gian hàng sự kiện, banner quảng bá, nhắc lịch "
            "qua tin nhắn ngắn/thư điện tử và thống kê lượt quan tâm, lượt tham gia."
        ),
        "Quản lý người mua": (
            "Người mua đại diện cho phía cầu của thị trường khoa học và công nghệ, vì vậy thiết kế cần lưu được hồ sơ, nhu cầu tìm "
            "mua, giỏ hàng, đánh giá, trao đổi, lịch sử mua hàng và yêu cầu hỗ trợ. Cơ sở lựa chọn là nhu cầu công nghệ thường cần "
            "được làm rõ qua nhiều lần tương tác trước khi hình thành giao dịch. Ưu điểm là hệ thống có thể theo dõi đầy đủ hành trình "
            "của người mua, hỗ trợ nhà cung cấp phản hồi đúng nhu cầu và giúp cơ quan quản lý nhận diện xu hướng thị trường. Phương án "
            "này đáp ứng yêu cầu đăng nhu cầu tìm mua, bình luận, đánh giá, lịch sử trao đổi, lịch sử mua hàng, khiếu nại và hỗ trợ."
        ),
        "Quản lý giao dịch": (
            "Đối với nhóm quản lý giao dịch, thiết kế lựa chọn mô hình quản lý theo quy trình trạng thái nhằm kiểm soát xuyên suốt "
            "từ yêu cầu báo giá, báo giá, đơn hàng, hợp đồng đến ghi nhận doanh thu, hoa hồng, thu chi và trạng thái xử lý. Cơ sở lựa "
            "chọn là giao dịch khoa học và công nghệ thường có thời gian xử lý dài, nhiều chủ thể tham gia và cần lưu vết đầy đủ từng "
            "bước. Ưu điểm là tăng khả năng giám sát tiến độ, truy xuất trách nhiệm, tổng hợp báo cáo theo giai đoạn và giảm rủi ro "
            "thất lạc thông tin trao đổi. Phương án này đáp ứng các yêu cầu báo giá, đơn hàng, hợp đồng, doanh thu, hoa hồng, thu chi "
            "và theo dõi trạng thái giao dịch."
        ),
        "Cổng thông tin Sàn giao dịch": (
            "Cổng thông tin là lớp tiếp cận công khai của sàn, vì vậy thiết kế cần kết hợp quản trị nội dung, danh mục công nghệ, tra cứu "
            "sản phẩm và đăng ký nhận thông tin. Cơ sở lựa chọn là người dùng bên ngoài thường bắt đầu từ nhu cầu tìm hiểu thị trường, "
            "tin tức, sự kiện hoặc sản phẩm trước khi đăng ký tương tác sâu hơn. Ưu điểm là mở rộng phạm vi tiếp cận, nâng cao tính minh "
            "bạch của thông tin và tạo nguồn dữ liệu quan tâm phục vụ xúc tiến. Phương án này đáp ứng trang chủ, tin tức, sự kiện, banner, "
            "danh mục công nghệ, tra cứu sản phẩm, tìm kiếm nâng cao và đăng ký nhận thông tin."
        ),
        "Tiện ích và tích hợp": (
            "Các tiện ích tương tác và tích hợp được thiết kế theo hướng cấu hình theo phạm vi dịch vụ được phê duyệt, không gắn cứng vào "
            "một nhà cung cấp công nghệ cụ thể. Cơ sở lựa chọn là nhu cầu trao đổi trên sàn có thể diễn ra qua nhiều kênh, từ trò chuyện "
            "trực tuyến, gọi hình, phát trực tuyến đến mã QR, mạng xã hội và tham quan thực tế ảo. Ưu điểm là hệ thống có thể mở rộng kênh "
            "tương tác theo nhu cầu vận hành, đồng thời vẫn kiểm soát được bảo mật, nhật ký và chất lượng dịch vụ. Phương án này đáp ứng "
            "chat trực tuyến, gọi video, phát trực tuyến, mã QR sản phẩm, liên kết mạng xã hội, tích hợp sàn khác, song ngữ Việt - Anh và VR360."
        ),
        "Ứng dụng trí tuệ nhân tạo (AI)": (
            "Ứng dụng trí tuệ nhân tạo (AI) được lựa chọn ở vai trò hỗ trợ, không tự động thay thế quyết định nghiệp vụ của cán bộ hoặc người "
            "dùng có thẩm quyền. Cơ sở lựa chọn là dữ liệu sản phẩm, công nghệ, nhu cầu và nhà cung cấp có khối lượng lớn, ngôn ngữ mô tả đa "
            "dạng, cần hỗ trợ tìm kiếm theo ngữ nghĩa và gợi ý phù hợp. Thiết kế cần tách dữ liệu phục vụ trí tuệ nhân tạo với dữ liệu gốc, "
            "có khả năng bật/tắt, kiểm soát nhật ký sử dụng và có phương án quay về tìm kiếm thông thường khi dịch vụ trí tuệ nhân tạo bên "
            "ngoài gián đoạn. Ưu điểm là nâng cao khả năng khám phá thông tin nhưng vẫn bảo toàn tính kiểm soát, minh bạch và an toàn dữ liệu. "
            "Phương án này đáp ứng chatbot tư vấn, tìm kiếm thông minh, tìm kiếm bằng giọng nói, gợi ý sản phẩm, gợi ý nhà cung cấp và gợi ý "
            "công nghệ phù hợp."
        ),
        "Giải pháp tích hợp": (
            "Giải pháp tích hợp được thiết kế như một lớp kết nối có kiểm soát giữa sàn và các dịch vụ liên quan, thay vì đưa từng dịch vụ vào "
            "như một thành phần phụ thuộc cứng. Cơ sở lựa chọn là các dịch vụ như VR360, tin nhắn thương hiệu, thư điện tử, chữ ký số, hợp đồng "
            "điện tử, thông báo tự động hoặc giao diện lập trình ứng dụng có thể thay đổi theo phạm vi thuê dịch vụ và nhà cung cấp tương ứng. "
            "Ưu điểm là giảm rủi ro khóa công nghệ, dễ thay thế, dễ kiểm soát nhật ký kết nối và trách nhiệm xử lý lỗi. Phương án này đáp ứng "
            "yêu cầu tích hợp được phê duyệt, có xác thực, phân quyền, cấu hình tham số, ghi nhật ký và tài liệu đặc tả."
        ),
        "Hạ tầng triển khai": (
            "Do dự án theo hình thức thuê dịch vụ CNTT, hạ tầng triển khai được mô tả bằng yêu cầu năng lực và mức chất lượng dịch vụ thay vì "
            "chỉ định máy chủ, hệ điều hành, cơ sở dữ liệu hoặc phần mềm thương mại cụ thể. Cơ sở lựa chọn là Chủ đầu tư cần kiểm soát khả năng "
            "sẵn sàng, an toàn, sao lưu, giám sát và quyền khai thác dữ liệu, trong khi đơn vị cung cấp dịch vụ cần có không gian đề xuất giải "
            "pháp tối ưu. Ưu điểm là bảo đảm cạnh tranh, tránh khóa công nghệ và vẫn xác định rõ đầu ra cần nghiệm thu. Phương án này đáp ứng "
            "yêu cầu máy chủ ứng dụng, bản quyền phần mềm nền tảng nếu có, cơ sở dữ liệu, sao lưu, giám sát, bảo mật, đào tạo, chuyển giao, bảo hành "
            "và bảo trì 05 năm theo phạm vi được phê duyệt."
        ),
        "Báo cáo - thống kê": (
            "Báo cáo - thống kê được lựa chọn như lớp phục vụ quản lý, đánh giá hiệu quả vận hành và ra quyết định của Chủ đầu tư. Cơ sở lựa chọn "
            "là dữ liệu trên sàn phát sinh từ nhiều phân hệ: nhà cung cấp, sản phẩm, giao dịch, doanh thu, sự kiện và truy cập. Thiết kế cần bảo đảm "
            "dữ liệu báo cáo có nguồn gốc rõ ràng, có kỳ thống kê, có quyền truy cập và có khả năng xuất dữ liệu. Ưu điểm là hỗ trợ giám sát thường "
            "xuyên, tổng hợp nhanh khi cần báo cáo lãnh đạo hoặc cơ quan quản lý, đồng thời tạo căn cứ đánh giá chất lượng dịch vụ thuê. Phương án "
            "này đáp ứng báo cáo nhà cung cấp, sản phẩm, giao dịch, doanh thu, sự kiện, thống kê lượt truy cập và xuất dữ liệu Excel/PDF."
        ),
    }

    for row in table.rows[1:]:
        key = row.cells[0].text.strip()
        if key in analyses:
            row.cells[1].text = analyses[key]


def clean_terms(doc):
    replacements = {
        "Email/Web": "thư điện tử (Email)/thông báo trên hệ thống",
        "PC, Tablet, Mobile": "máy tính, máy tính bảng, thiết bị di động",
        "máy tính cá nhân (PC), tablet, mobile": "máy tính, máy tính bảng, thiết bị di động",
        "email": "thư điện tử (Email)",
        "tin nhắn thương hiệu (tin nhắn ngắn (SMS) Brandname)": "tin nhắn thương hiệu (SMS Brandname)",
        "thư điện tử (thư điện tử (Email))": "thư điện tử (Email)",
        "trí tuệ nhân tạo (trí tuệ nhân tạo (AI) Chatbot) tư vấn": "trợ lý trò chuyện tự động dùng trí tuệ nhân tạo (AI Chatbot)",
        "trí tuệ nhân tạo (AI) Chatbot": "trợ lý trò chuyện tự động dùng trí tuệ nhân tạo (AI Chatbot)",
        "Chat trực tuyến.": "Trò chuyện trực tuyến (Chat).",
        "tham quan tham quan thực tế ảo 360 độ": "tham quan thực tế ảo 360 độ",
        "notification": "thông báo",
        "biên bản UAT": "biên bản nghiệm thu người dùng (UAT)",
        "Excel/PDF/CSV/JSON": "bảng tính, PDF, CSV, JSON",
        "Excel/PDF": "bảng tính/PDF",
        "nhật ký kiểm tra (audit) nhật ký": "nhật ký kiểm tra (audit)",
    }

    def replace_in_text(text):
        for old, new in replacements.items():
            text = text.replace(old, new)
        return text

    for paragraph in doc.paragraphs:
        new_text = replace_in_text(paragraph.text)
        if new_text != paragraph.text:
            set_para_text(paragraph, new_text)

    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                new_text = replace_in_text(cell.text)
                if new_text != cell.text:
                    cell.text = new_text


def main():
    doc = Document(SRC)

    replacements = {
        "1. Các ràng buộc đối với hệ thống gồm: ràng buộc môi trường, sự phụ thuộc vào hệ thống nền tảng":
            "1. Ràng buộc môi trường và phụ thuộc nền tảng dịch vụ",
        "2. Yêu cầu về cài đặt, hạ tầng, đường truyền, an toàn vận hành, khai thác, sử dụng, khả nâng cấu hình hệ thống chạy trên nền Ipv6":
            "2. Yêu cầu hạ tầng dịch vụ, đường truyền và an toàn vận hành",
        "6. Các ràng buộc đối với hệ thống gồm: ràng buộc môi trường, sự phụ thuộc vào hệ thống nền tảng":
            "6. Ràng buộc về dữ liệu, quyền khai thác và trách nhiệm dịch vụ",
        "7. Yêu cầu về cài đặt, hạ tầng, đường truyền, an toàn vận hành, khai thác, sử dụng, khả năng cấu hình hệ thống chạy trên nền IPv6":
            "7. Yêu cầu sẵn sàng IPv6 và điều kiện khai thác mở rộng",
    }
    for old, new in replacements.items():
        replace_exact_paragraph(doc, old, new)
        replace_text_in_paragraphs(doc, old, new)

    replace_contains_paragraph(
        doc,
        "Dịch vụ phải được cung cấp trên hạ tầng đáp ứng yêu cầu về năng lực xử lý",
        "Hạ tầng dịch vụ được xem xét ở góc độ năng lực đầu ra gồm khả năng xử lý đồng thời, lưu trữ, đường truyền, giám sát, sao lưu và khôi phục. Việc tách yêu cầu hạ tầng khỏi yêu cầu IPv6 giúp tránh lặp nội dung: mục này tập trung vào điều kiện vận hành thường xuyên của dịch vụ, còn yêu cầu IPv6 được phân tích riêng tại mục 7.",
    )
    replace_contains_paragraph(
        doc,
        "Hệ thống cần sẵn sàng cấu hình trong môi trường IPv6",
        "Yêu cầu về IPv6 được xác định là điều kiện sẵn sàng mở rộng, được đánh giá theo khả năng cấu hình tên miền, truy cập bảo mật và kết nối khi hạ tầng của Chủ đầu tư hoặc đơn vị cung cấp dịch vụ chuyển sang mô hình IPv6 hoặc song song IPv4/IPv6.",
    )
    replace_contains_paragraph(
        doc,
        "Kết nối dịch vụ ngoài: thư điện tử",
        "Kết nối dịch vụ ngoài: các dịch vụ tích hợp được phê duyệt phải được cấu hình theo phạm vi thuê dịch vụ, có xác thực, phân quyền, ghi nhật ký, cơ chế theo dõi lỗi và khả năng thay thế khi cần."
    )
    replace_contains_paragraph(
        doc,
        "Các ràng buộc cần được đưa vào yêu cầu thuê dịch vụ gồm: quyền sở hữu",
        "Nhóm ràng buộc này tập trung vào dữ liệu và trách nhiệm dịch vụ, bao gồm quyền sở hữu và quyền khai thác dữ liệu của Chủ đầu tư; thời gian lưu giữ dữ liệu; khả năng trích xuất dữ liệu; trách nhiệm sao lưu, khôi phục; trách nhiệm bảo vệ dữ liệu cá nhân; cơ chế bàn giao khi kết thúc thuê dịch vụ và khả năng duy trì hoạt động khi thay đổi đơn vị cung cấp. Cách trình bày này phân biệt với mục 1, vốn chỉ xem xét ràng buộc môi trường và nền tảng dịch vụ."
    )
    replace_contains_paragraph(
        doc,
        "Thiết kế lựa chọn phương án yêu cầu dịch vụ sẵn sàng triển khai IPv6",
        "Đối với IPv6, thiết kế không quy định cấu hình kỹ thuật chi tiết mà đặt ra yêu cầu sẵn sàng kết nối và khai thác mở rộng. Đơn vị cung cấp dịch vụ phải chứng minh khả năng hỗ trợ IPv6 hoặc mô hình song song IPv4/IPv6 khi Chủ đầu tư yêu cầu, bao gồm tên miền, truy cập bảo mật, kết nối tích hợp và giám sát. Ưu điểm của cách tiếp cận này là đáp ứng định hướng phát triển Internet thế hệ mới mà vẫn bảo đảm tính mở, phù hợp nhiều phương án hạ tầng."
    )
    replace_contains_paragraph(
        doc,
        "Các tiện ích tích hợp như chat, gọi video",
        "Các tiện ích tích hợp được kích hoạt theo phạm vi thuê dịch vụ và được quản lý qua cơ chế cấu hình, phân quyền, ghi nhật ký, theo dõi lỗi; không coi danh sách tiện ích là chỉ định cố định về công nghệ hoặc nhà cung cấp."
    )
    replace_contains_paragraph(
        doc,
        "Cấu hình các dịch vụ tích hợp được phê duyệt như thư điện tử",
        "Cấu hình các dịch vụ tích hợp được phê duyệt theo phạm vi thuê dịch vụ; kiểm tra xác thực, phân quyền, nhật ký, thông báo lỗi và khả năng vận hành thay thế khi một dịch vụ ngoài bị gián đoạn."
    )
    replace_contains_paragraph(
        doc,
        "Áp dụng xác thực, phân quyền, truy cập bảo mật, nhật ký/nhật ký kiểm tra",
        "Áp dụng xác thực, phân quyền, truy cập bảo mật, nhật ký kiểm tra (audit), kiểm soát tải tệp lên và giám sát sự kiện."
    )
    replace_contains_paragraph(
        doc,
        "Với hình thức thuê dịch vụ CNTT, chuyển giao được hiểu",
        "Với hình thức thuê dịch vụ CNTT, chuyển giao được hiểu là chuyển giao tri thức sử dụng, quản trị, giám sát chất lượng dịch vụ, tài liệu cấu hình nghiệp vụ, dữ liệu và quyền khai thác theo hợp đồng. Các nội dung thuộc tài sản kỹ thuật nội bộ của đơn vị cung cấp dịch vụ không mặc nhiên đặt thành yêu cầu bàn giao, trừ trường hợp HSMT, hợp đồng hoặc phạm vi sản phẩm được phê duyệt có quy định cụ thể."
    )

    rewrite_solution_table(doc)
    clean_terms(doc)

    # Remove duplicate opening paragraphs in VI, VII and X, keeping the richer first paragraph in each section.
    duplicate_starts = [
        "Danh mục dưới đây được xác định theo yêu cầu dịch vụ",
        "Tổ chức thực hiện trong thiết kế chi tiết được mô tả",
        "Phù hợp hình thức thuê dịch vụ CNTT.",
    ]
    for paragraph in list(doc.paragraphs):
        text = paragraph.text.strip()
        if any(text.startswith(start) for start in duplicate_starts):
            delete_paragraph(paragraph)

    doc.save(DST)
    print(DST)


if __name__ == "__main__":
    main()
