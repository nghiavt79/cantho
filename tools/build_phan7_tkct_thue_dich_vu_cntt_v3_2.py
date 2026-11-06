from __future__ import annotations

from pathlib import Path
import importlib.util

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs" / "ho so ky thuat" / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.2_thue_dich_vu_CNTT.docx"

spec = importlib.util.spec_from_file_location("hsyc_base", ROOT / "tools" / "build_hsyc_full_cdt.py")
base = importlib.util.module_from_spec(spec)
assert spec.loader is not None
spec.loader.exec_module(base)


FUNCTION_ROWS = [
    ["1", "Quản trị hệ thống", "Đăng nhập, đăng xuất, quên mật khẩu.\nQuản lý tài khoản và hồ sơ người dùng.\nPhân quyền theo vai trò và chức năng.\nNhật ký thao tác.\nCấu hình hệ thống, tham số vận hành.\nThông báo qua Email/Web.\nGiao diện tương thích PC, Tablet, Mobile."],
    ["2", "Quản lý nhà cung cấp", "Quản lý hồ sơ pháp lý.\nDanh mục lĩnh vực công nghệ.\nQuản lý gian hàng.\nQuản lý hạng thành viên.\nDuyệt hồ sơ và sản phẩm.\nQuản lý bài viết, tin giới thiệu.\nTheo dõi lịch sử giao dịch và lịch sử chỉnh sửa."],
    ["3", "Quản lý sản phẩm và công nghệ", "Danh mục nhóm sản phẩm.\nDanh mục sản phẩm.\nHồ sơ công nghệ.\nHình ảnh, video, tài liệu.\nQuản lý phòng trưng bày.\nQuản lý vị trí trưng bày.\nQuản lý trạng thái pháp lý sản phẩm."],
    ["4", "Quản lý sự kiện - triển lãm", "Tạo và quản lý hội thảo.\nQuản lý triển lãm.\nĐăng ký tham dự.\nQuản lý gian hàng sự kiện.\nBanner quảng bá.\nNhắc lịch qua SMS, Email.\nThống kê lượt quan tâm và lượt tham gia."],
    ["5", "Quản lý người mua", "Hồ sơ người mua.\nĐăng nhu cầu tìm mua.\nGiỏ hàng.\nBình luận, đánh giá.\nLịch sử mua hàng.\nLịch sử trao đổi.\nQuản lý khiếu nại và hỗ trợ."],
    ["6", "Quản lý giao dịch", "Báo giá.\nĐơn hàng.\nHợp đồng.\nDoanh thu.\nHoa hồng.\nThu chi.\nTheo dõi trạng thái giao dịch."],
    ["7", "Cổng thông tin Sàn giao dịch", "Trang chủ.\nTin tức.\nSự kiện.\nBanner.\nDanh mục công nghệ.\nTra cứu sản phẩm.\nTìm kiếm nâng cao.\nĐăng ký nhận thông tin."],
    ["8", "Tiện ích và tích hợp", "Chat trực tuyến.\nVideo Call.\nLivestream.\nMã QR sản phẩm.\nLiên kết mạng xã hội.\nTích hợp sàn khác.\nSong ngữ Việt - Anh.\nTham quan VR360."],
    ["9", "Ứng dụng AI", "AI Chatbot tư vấn.\nAI tìm kiếm thông minh.\nTìm kiếm bằng giọng nói.\nGợi ý sản phẩm.\nGợi ý nhà cung cấp.\nGợi ý công nghệ phù hợp."],
    ["10", "Giải pháp tích hợp", "VR360.\nSMS Brandname.\nEmail.\nChữ ký số.\nHợp đồng điện tử.\nThông báo tự động.\nKết nối API tích hợp."],
    ["11", "Hạ tầng triển khai", "Máy chủ ứng dụng.\nBản quyền hệ điều hành.\nBản quyền CSDL.\nSao lưu dữ liệu.\nGiám sát hệ thống.\nBảo mật và an toàn thông tin.\nĐào tạo, chuyển giao.\nBảo hành, bảo trì 05 năm."],
    ["12", "Báo cáo - thống kê", "Báo cáo nhà cung cấp.\nBáo cáo sản phẩm.\nBáo cáo giao dịch.\nBáo cáo doanh thu.\nBáo cáo sự kiện.\nThống kê lượt truy cập.\nXuất dữ liệu Excel/PDF."],
]


def build():
    doc = Document()
    base.style_doc(doc)

    base.title(doc, "PHẦN 7. THIẾT KẾ CHI TIẾT", 17)
    base.p(doc, "Dự án: Sàn giao dịch Khoa học và Công nghệ", bold=True, align=WD_ALIGN_PARAGRAPH.CENTER)
    base.p(doc, "Hình thức: Thiết kế chi tiết phục vụ kế hoạch thuê dịch vụ công nghệ thông tin", italic=True, align=WD_ALIGN_PARAGRAPH.CENTER)

    base.h(doc, 1, "A. YÊU CẦU THIẾT KẾ CHI TIẾT:")
    base.p(doc, "Thiết kế chi tiết được lập dưới góc độ đơn vị tư vấn, nhằm phân tích yêu cầu, lựa chọn phương án kỹ thuật và đề xuất giải pháp thuê dịch vụ công nghệ thông tin phù hợp với mục tiêu của dự án. Tài liệu không mô tả phương án tổ chức thi công nội bộ của một nhà cung cấp cụ thể, không chỉ định sản phẩm/công nghệ độc quyền, và không thay thế cho hồ sơ dự thầu hoặc phương án triển khai chi tiết của đơn vị cung cấp dịch vụ.")
    base.bullets(doc, [
        "Làm rõ nhu cầu nghiệp vụ, phạm vi dịch vụ CNTT cần thuê, yêu cầu đầu ra và tiêu chí kỹ thuật làm cơ sở lập kế hoạch thuê dịch vụ.",
        "Phân tích, lựa chọn phương án kỹ thuật theo hướng mở, cạnh tranh, có thể sử dụng các nền tảng, phần mềm, thiết bị, dịch vụ hoặc giải pháp tương đương đáp ứng yêu cầu.",
        "Đề xuất mô hình kiến trúc, yêu cầu kết nối, dữ liệu, an toàn thông tin, vận hành, đào tạo và tài liệu bàn giao phù hợp với hình thức thuê dịch vụ CNTT.",
        "Hạn chế quy định các nội dung thuộc biện pháp thực hiện riêng của đơn vị cung cấp dịch vụ như quy trình phát triển nội bộ, tổ chức nhân sự chi tiết, công cụ kỹ thuật nội bộ, khóa kỹ thuật hoặc thông tin bí mật của nhà cung cấp.",
        "Bảo đảm đủ cơ sở để Chủ đầu tư lập dự toán, xác định danh mục dịch vụ, đánh giá phương án kỹ thuật, nghiệm thu kết quả thuê dịch vụ và tổ chức vận hành sau khi dịch vụ được cung cấp.",
    ])

    base.h(doc, 1, "B. NỘI DUNG CHÍNH CỦA THIẾT KẾ CHI TIẾT:")
    base.h(doc, 2, "I. TÓM TẮT NỘI DUNG YÊU CẦU CỦA DỰ ÁN:")
    base.h(doc, 3, "1. Yêu cầu chung:")
    base.p(doc, "Dự án Sàn giao dịch Khoa học và Công nghệ hướng tới hình thành nền tảng số phục vụ quản lý, công bố, tra cứu, kết nối cung - cầu, hỗ trợ giao dịch và chuyển giao công nghệ. Dịch vụ CNTT được thuê cần cung cấp môi trường phần mềm, hạ tầng vận hành, khả năng quản trị dữ liệu, hỗ trợ người dùng, an toàn thông tin và các tiện ích tích hợp cần thiết để Chủ đầu tư khai thác ổn định trong thời gian thuê.")
    base.p(doc, "Về nguyên tắc, thiết kế lựa chọn phương án thuê dịch vụ theo hướng dịch vụ trọn gói hoặc dịch vụ theo cấu phần, trong đó đơn vị cung cấp dịch vụ chịu trách nhiệm cung cấp, cấu hình, vận hành, bảo trì và bảo đảm mức chất lượng dịch vụ theo hợp đồng; Chủ đầu tư quản lý yêu cầu, dữ liệu, tài khoản, quy trình khai thác, nghiệm thu kết quả và giám sát chất lượng dịch vụ.")
    base.bullets(doc, [
        "Dịch vụ phải đáp ứng các nhóm chức năng nghiệp vụ của Sàn giao dịch Khoa học và Công nghệ.",
        "Dịch vụ phải có khả năng mở rộng, tích hợp, sao lưu, khôi phục và duy trì vận hành liên tục theo yêu cầu hợp đồng.",
        "Dịch vụ phải bảo đảm an toàn thông tin, bảo vệ dữ liệu cá nhân và phân quyền truy cập phù hợp với vai trò người dùng.",
        "Dịch vụ phải có giao diện web thân thiện, tương thích thiết bị phổ biến, hỗ trợ quản trị nội dung và báo cáo thống kê.",
        "Dịch vụ phải có tài liệu hướng dẫn, đào tạo, hỗ trợ vận hành và cơ chế bảo hành/bảo trì trong suốt thời gian thuê.",
    ])

    base.h(doc, 3, "2. Yêu cầu cụ thể:")
    base.p(doc, "Các yêu cầu cụ thể được xác định theo 12 nhóm chức năng trọng tâm. Các nhóm chức năng này là cơ sở để phân tích phương án kỹ thuật, xác định phạm vi thuê dịch vụ, thiết kế dữ liệu, tích hợp, đào tạo và nghiệm thu.")
    base.table(doc, ["STT", "Nhóm chức năng", "Mô tả chi tiết"], FUNCTION_ROWS)
    base.table(doc, ["Nhóm yêu cầu", "Định hướng thiết kế chi tiết"], [
        ["Nghiệp vụ", "Dịch vụ phải đáp ứng đầy đủ 12 nhóm chức năng nêu trên, có phân quyền theo vai trò và có khả năng cấu hình quy trình xử lý phù hợp thực tế vận hành của Chủ đầu tư."],
        ["Dữ liệu", "Dữ liệu phải được quản lý tập trung, có cấu trúc, có khả năng tìm kiếm, thống kê, xuất dữ liệu và sao lưu/khôi phục theo quy định."],
        ["Tích hợp", "Dịch vụ phải sẵn sàng tích hợp email, SMS Brandname, chữ ký số, hợp đồng điện tử, VR360, API và nền tảng liên thông khác khi Chủ đầu tư có yêu cầu."],
        ["AI", "Các chức năng AI phải được thiết kế theo hướng hỗ trợ tư vấn, tìm kiếm và gợi ý; dữ liệu sử dụng cho AI phải được kiểm soát phạm vi, có cấu hình bật/tắt và không làm thay đổi tính đúng đắn của dữ liệu gốc."],
        ["Vận hành", "Dịch vụ phải có cơ chế giám sát, ghi log, sao lưu, khôi phục, hỗ trợ người dùng, bảo trì, bảo hành và báo cáo chất lượng dịch vụ trong suốt thời gian thuê."],
    ])

    base.h(doc, 2, "II. DANH MỤC QUY CHUẨN KỸ THUẬT, TIÊU CHUẨN ĐƯỢC ÁP DỤNG")
    base.p(doc, "Việc thiết kế và thuê dịch vụ CNTT phải tuân thủ quy định hiện hành về ứng dụng CNTT trong cơ quan nhà nước, an toàn thông tin, dữ liệu cá nhân, giao dịch điện tử, kết nối/chia sẻ dữ liệu và các tiêu chuẩn kỹ thuật thông dụng. Các tiêu chuẩn dưới đây được hiểu là yêu cầu tối thiểu; đơn vị cung cấp dịch vụ có thể đề xuất tiêu chuẩn tương đương hoặc cao hơn nhưng phải chứng minh khả năng đáp ứng.")
    base.table(doc, ["STT", "Nhóm tiêu chuẩn/quy chuẩn", "Nội dung áp dụng", "Yêu cầu áp dụng"], [
        ["1", "Tiêu chuẩn ứng dụng CNTT trong cơ quan nhà nước", "Danh mục tiêu chuẩn kỹ thuật về ứng dụng CNTT trong cơ quan nhà nước và văn bản thay thế/bổ sung nếu có.", "Bắt buộc"],
        ["2", "Kết nối hệ thống thông tin, cơ sở dữ liệu", "Yêu cầu kỹ thuật về kết nối, liên thông và chia sẻ dữ liệu với hệ thống khác.", "Áp dụng khi tích hợp"],
        ["3", "An toàn thông tin mạng", "Tuân thủ quy định về bảo đảm an toàn thông tin mạng, bảo vệ hệ thống thông tin theo cấp độ.", "Bắt buộc"],
        ["4", "Bảo vệ dữ liệu cá nhân", "Quy định về thu thập, xử lý, lưu trữ, chia sẻ và bảo vệ dữ liệu cá nhân.", "Bắt buộc"],
        ["5", "Giao dịch điện tử và chữ ký số", "Yêu cầu đối với hợp đồng điện tử, chứng từ điện tử, chữ ký số và lưu trữ hồ sơ điện tử.", "Áp dụng khi triển khai"],
        ["6", "Giao thức web bảo mật", "Truy cập web qua giao thức bảo mật; mã hóa đường truyền; tên miền và chứng thư số hợp lệ.", "Bắt buộc"],
        ["7", "Dữ liệu và tiếng Việt", "Hỗ trợ Unicode tiếng Việt; định dạng dữ liệu phổ biến; tìm kiếm và xuất báo cáo tiếng Việt.", "Bắt buộc"],
        ["8", "Tài liệu điện tử", "Hỗ trợ các định dạng thông dụng như PDF, bảng tính, văn bản, ảnh, video hoặc định dạng tương đương.", "Bắt buộc/khuyến nghị"],
        ["9", "API và tích hợp", "Giao diện tích hợp có xác thực, phân quyền, log, tài liệu đặc tả và định dạng dữ liệu phổ biến.", "Áp dụng khi tích hợp"],
        ["10", "IPv6", "Sẵn sàng triển khai trên môi trường IPv4/IPv6 hoặc tương đương khi hạ tầng đáp ứng.", "Khuyến nghị/bắt buộc theo yêu cầu hạ tầng"],
        ["11", "Quản lý chất lượng dịch vụ", "Có tiêu chí chất lượng, thời gian phản hồi, thời gian khắc phục lỗi, báo cáo vận hành.", "Bắt buộc trong hợp đồng dịch vụ"],
        ["12", "Nhật ký và giám sát", "Ghi nhận nhật ký vận hành, nhật ký thao tác, nhật ký tích hợp và giám sát tình trạng dịch vụ.", "Bắt buộc"],
    ])

    base.h(doc, 2, "III. PHÂN TÍCH SỰ TUÂN THỦ KHUNG KIẾN TRÚC TỔNG THỂ QUỐC GIA SỐ:")
    base.p(doc, "Phương án thuê dịch vụ CNTT cho Sàn giao dịch Khoa học và Công nghệ phù hợp định hướng phát triển nền tảng số chuyên ngành, khai thác dữ liệu phục vụ quản lý nhà nước, thúc đẩy tương tác số giữa cơ quan quản lý, doanh nghiệp, nhà cung cấp, chuyên gia và người dân. Thiết kế đề xuất tổ chức hệ thống theo các lớp kiến trúc sau:")
    base.table(doc, ["Lớp kiến trúc", "Phân tích tuân thủ và định hướng thiết kế"], [
        ["Lớp người dùng", "Hỗ trợ nhiều nhóm người dùng gồm khách truy cập, người mua, nhà cung cấp, chuyên gia, cán bộ nghiệp vụ, quản trị hệ thống và lãnh đạo. Mỗi nhóm được phân quyền theo chức năng và phạm vi dữ liệu."],
        ["Lớp kênh truy cập", "Ưu tiên kênh web bảo mật, tương thích PC, tablet, mobile; có thể mở rộng sang các kênh số khác thông qua API hoặc tích hợp tương đương."],
        ["Lớp ứng dụng", "Tổ chức thành các phân hệ bám sát 12 nhóm chức năng; cho phép cấu hình quy trình, danh mục, thông báo, báo cáo và tích hợp theo nhu cầu."],
        ["Lớp dữ liệu", "Dữ liệu được quản lý có cấu trúc, tập trung, có phân quyền, có sao lưu, có khả năng xuất/nhập và khai thác phục vụ báo cáo, tìm kiếm, AI."],
        ["Lớp tích hợp", "Thiết kế mở để kết nối email, SMS, chữ ký số, hợp đồng điện tử, VR360, API liên thông và nền tảng dùng chung khi có yêu cầu."],
        ["Lớp an toàn thông tin", "Thiết kế xác thực, phân quyền, mã hóa đường truyền, log, giám sát, sao lưu, khôi phục và kiểm soát dữ liệu cá nhân theo quy định."],
        ["Lớp vận hành dịch vụ", "Phù hợp hình thức thuê dịch vụ CNTT: có cam kết chất lượng dịch vụ, báo cáo vận hành, hỗ trợ người dùng, bảo hành/bảo trì và nghiệm thu theo kết quả dịch vụ."],
    ])
    base.bullets(doc, [
        "Tính phù hợp: Dự án tạo ra nền tảng số chuyên ngành phục vụ kết nối cung - cầu khoa học và công nghệ.",
        "Tính liên thông: Thiết kế ưu tiên API và định dạng dữ liệu phổ biến, thuận lợi kết nối với hệ thống dùng chung khi có yêu cầu.",
        "Tính dữ liệu: Dữ liệu được xem là tài sản của Chủ đầu tư, được quản lý, bảo vệ, sao lưu và khai thác có kiểm soát.",
        "Tính cạnh tranh: Thiết kế không chỉ định sản phẩm cụ thể; cho phép nhiều phương án công nghệ hoặc dịch vụ tương đương tham gia.",
    ])

    base.h(doc, 2, "IV. PHÂN TÍCH LỰA CHỌN PHƯƠNG ÁN KỸ THUẬT, CÔNG NGHỆ, THIẾT BỊ")
    base.h(doc, 3, "1. Các ràng buộc đối với hệ thống gồm: ràng buộc môi trường, sự phụ thuộc vào hệ thống nền tảng")
    base.p(doc, "Do dự án thực hiện theo định hướng thuê dịch vụ CNTT, phương án kỹ thuật cần cân bằng giữa yêu cầu kiểm soát của Chủ đầu tư và quyền chủ động công nghệ của đơn vị cung cấp dịch vụ. Thiết kế không quy định cứng nền tảng triển khai, nhưng đặt ra các ràng buộc kỹ thuật tối thiểu để bảo đảm dịch vụ vận hành an toàn, ổn định và có thể kiểm soát.")
    base.table(doc, ["Ràng buộc", "Phân tích", "Định hướng lựa chọn"], [
        ["Môi trường truy cập", "Người dùng truy cập từ nhiều thiết bị và mạng khác nhau.", "Lựa chọn ứng dụng web bảo mật, responsive, không yêu cầu cài đặt phức tạp ở người dùng cuối."],
        ["Nền tảng vận hành", "Dịch vụ có thể đặt trên hạ tầng của Chủ đầu tư, hạ tầng thuê ngoài hoặc hạ tầng do đơn vị cung cấp dịch vụ đề xuất.", "Yêu cầu đáp ứng tiêu chí an toàn, sao lưu, giám sát, hiệu năng và khả năng bàn giao dữ liệu."],
        ["Dịch vụ tích hợp", "Email, SMS, AI, chữ ký số, VR360 phụ thuộc tài khoản/dịch vụ bên ngoài.", "Thiết kế theo hướng cấu hình được, có log, có cơ chế bật/tắt, có phương án khi dịch vụ gián đoạn."],
        ["Dữ liệu", "Dữ liệu phát sinh trong thời gian thuê là dữ liệu phục vụ quản lý của Chủ đầu tư.", "Yêu cầu quyền truy cập, trích xuất, sao lưu, bàn giao dữ liệu rõ ràng trong hợp đồng dịch vụ."],
    ])

    base.h(doc, 3, "2. Yêu cầu về cài đặt, hạ tầng, đường truyền, an toàn vận hành, khai thác, sử dụng, khả nâng cấu hình hệ thống chạy trên nền Ipv6")
    base.bullets(doc, [
        "Dịch vụ phải được cung cấp trên hạ tầng đáp ứng yêu cầu về năng lực xử lý, lưu trữ, đường truyền, giám sát và sao lưu theo quy mô người dùng dự kiến.",
        "Đơn vị cung cấp dịch vụ có thể đề xuất nền tảng hạ tầng phù hợp; Chủ đầu tư đánh giá trên cơ sở năng lực đáp ứng, an toàn thông tin, khả năng mở rộng, khả năng trích xuất dữ liệu và chi phí thuê dịch vụ.",
        "Hệ thống phải hỗ trợ truy cập bảo mật, tên miền, chứng thư số, ghi nhật ký và cơ chế giám sát trạng thái dịch vụ.",
        "Hệ thống cần sẵn sàng cấu hình trong môi trường IPv6 hoặc dual-stack khi hạ tầng của Chủ đầu tư hoặc đơn vị cung cấp dịch vụ đáp ứng.",
    ])

    base.h(doc, 3, "3. Yêu cầu kỹ thuật, giải pháp công nghệ đối với thiết bị phần cứng, phần mềm thương mại")
    base.p(doc, "Đối với hình thức thuê dịch vụ CNTT, thiết kế chi tiết không chỉ định cụ thể máy chủ, hệ điều hành, hệ quản trị cơ sở dữ liệu, phần mềm trung gian hoặc công cụ kỹ thuật nội bộ. Các thành phần này do đơn vị cung cấp dịch vụ đề xuất, nhưng phải đáp ứng yêu cầu kỹ thuật, mức chất lượng dịch vụ và điều kiện nghiệm thu của Chủ đầu tư.")
    base.table(doc, ["Thành phần", "Yêu cầu thiết kế", "Ghi chú"], [
        ["Hạ tầng tính toán", "Đáp ứng số lượng người dùng, dung lượng dữ liệu, tần suất truy cập và khả năng mở rộng.", "Có thể là hạ tầng vật lý, ảo hóa, cloud hoặc tương đương."],
        ["Nền tảng ứng dụng", "Hỗ trợ ứng dụng web bảo mật, phân quyền, tích hợp, log, giám sát và nâng cấp dịch vụ.", "Không chỉ định sản phẩm cụ thể."],
        ["Cơ sở dữ liệu", "Hỗ trợ dữ liệu có cấu trúc, giao dịch, phân quyền, sao lưu/khôi phục, Unicode tiếng Việt và báo cáo.", "Hệ quản trị CSDL quan hệ hoặc tương đương."],
        ["Kho lưu trữ", "Lưu ảnh, video, tài liệu, hợp đồng, file báo cáo; có phân quyền và sao lưu.", "Có thể dùng lưu trữ nội bộ hoặc dịch vụ lưu trữ phù hợp."],
        ["Phần mềm thương mại", "Nếu dùng bản quyền thương mại, chi phí và quyền sử dụng phải được tính trong phương án thuê dịch vụ.", "Bảo đảm tuân thủ bản quyền."],
        ["Dịch vụ tích hợp", "Email, SMS, chữ ký số, AI, VR360, API phải có cấu hình, log và điều kiện sử dụng rõ ràng.", "Triển khai theo phạm vi được phê duyệt."],
    ])

    base.h(doc, 3, "4. Phân tích, lựa chọn phương án bảo đảm tính kết nối, liên thông, chia sẻ dữ liệu")
    base.p(doc, "Phương án kết nối được lựa chọn theo nguyên tắc mở, có kiểm soát, ưu tiên API và định dạng dữ liệu phổ biến. Dữ liệu cần chia sẻ phải được phân loại, xác định chủ thể khai thác, phạm vi truy cập, phương thức xác thực và nhật ký giao dịch.")
    base.bullets(doc, [
        "Kết nối người dùng: truy cập web bảo mật, tương thích đa thiết bị.",
        "Kết nối hệ thống: API tích hợp có xác thực, phân quyền, ghi log và tài liệu đặc tả.",
        "Kết nối dữ liệu: hỗ trợ xuất/nhập Excel/PDF/CSV/JSON hoặc định dạng tương đương phục vụ báo cáo và chuyển đổi dữ liệu.",
        "Kết nối dịch vụ ngoài: email, SMS, chữ ký số, hợp đồng điện tử, AI, VR360 được cấu hình theo phạm vi thuê dịch vụ.",
        "Chia sẻ dữ liệu: phải có phê duyệt, kiểm soát quyền, bảo vệ dữ liệu cá nhân và ghi nhận lịch sử truy cập.",
    ])

    base.h(doc, 3, "5. Phân tích, lựa chọn phương án công nghệ, kỹ thuật bảo đảm an toàn, an ninh mạng")
    base.table(doc, ["Nhóm biện pháp", "Phân tích lựa chọn", "Yêu cầu đề xuất"], [
        ["Xác thực và phân quyền", "Hệ thống có nhiều nhóm người dùng, nhiều mức truy cập dữ liệu.", "Áp dụng xác thực tài khoản, phân quyền theo vai trò, chức năng, hồ sơ và trạng thái xử lý."],
        ["Bảo mật truy cập", "Dữ liệu giao dịch và hồ sơ công nghệ có yêu cầu bảo mật.", "Sử dụng truy cập bảo mật, kiểm soát phiên, chống truy cập trái phép, ghi log."],
        ["Bảo vệ dữ liệu", "Dữ liệu thuộc Chủ đầu tư và người dùng, gồm dữ liệu cá nhân và dữ liệu giao dịch.", "Có phân quyền, sao lưu, khôi phục, kiểm soát xuất dữ liệu, quy định bàn giao dữ liệu khi kết thúc thuê dịch vụ."],
        ["An toàn ứng dụng", "Ứng dụng web có nguy cơ tấn công phổ biến.", "Kiểm tra dữ liệu đầu vào, kiểm soát upload, chống giả mạo yêu cầu, chống mã độc/tấn công phổ biến."],
        ["Giám sát và ứng cứu", "Dịch vụ cần vận hành liên tục trong thời gian thuê.", "Có log, giám sát, cảnh báo, quy trình xử lý sự cố và báo cáo chất lượng dịch vụ."],
    ])

    base.h(doc, 3, "6. Các ràng buộc đối với hệ thống gồm: ràng buộc môi trường, sự phụ thuộc vào hệ thống nền tảng")
    base.p(doc, "Các ràng buộc cần được đưa vào yêu cầu thuê dịch vụ gồm: quyền sở hữu và quyền khai thác dữ liệu của Chủ đầu tư; thời gian lưu giữ dữ liệu; khả năng trích xuất dữ liệu; trách nhiệm sao lưu, khôi phục; khả năng duy trì dịch vụ trong trường hợp thay đổi đơn vị cung cấp; trách nhiệm phối hợp tích hợp với nền tảng của cơ quan nhà nước; và trách nhiệm bảo đảm an toàn thông tin theo cấp độ được phê duyệt.")

    base.h(doc, 3, "7. Yêu cầu về cài đặt, hạ tầng, đường truyền, an toàn vận hành, khai thác, sử dụng, khả năng cấu hình hệ thống chạy trên nền IPv6")
    base.p(doc, "Thiết kế lựa chọn phương án yêu cầu dịch vụ sẵn sàng triển khai IPv6 thay vì chỉ định cấu hình hạ tầng cụ thể. Đơn vị cung cấp dịch vụ phải chứng minh khả năng hỗ trợ IPv6 hoặc dual-stack khi Chủ đầu tư yêu cầu, bao gồm tên miền, truy cập bảo mật, API callback, log truy cập và tích hợp bên ngoài.")

    base.h(doc, 2, "V. THUYẾT MINH GIẢI PHÁP KỸ THUẬT:")
    base.h(doc, 3, "1. Quan điểm xây dựng")
    base.bullets(doc, [
        "Thiết kế theo định hướng thuê dịch vụ CNTT, trong đó yêu cầu được mô tả bằng tiêu chí đầu ra và mức chất lượng dịch vụ thay vì chỉ định biện pháp triển khai nội bộ.",
        "Bảo đảm tính mở, cạnh tranh và trung lập công nghệ; các thành phần kỹ thuật được mô tả theo năng lực cần đáp ứng hoặc sử dụng cụm từ tương đương.",
        "Bám sát 12 nhóm chức năng trọng tâm của Sàn giao dịch Khoa học và Công nghệ.",
        "Ưu tiên khả năng cấu hình, mở rộng, tích hợp, sao lưu, giám sát, an toàn thông tin và bàn giao dữ liệu.",
        "Phân định rõ trách nhiệm của Chủ đầu tư, đơn vị tư vấn, đơn vị cung cấp dịch vụ và đơn vị vận hành/khai thác.",
    ])
    base.table(doc, ["Nhóm chức năng", "Định hướng giải pháp trong thiết kế chi tiết"], [
        [r[1], "Thiết kế dịch vụ phải đáp ứng nhóm chức năng này theo mô tả yêu cầu, có phân quyền, dữ liệu liên quan, tiêu chí nghiệm thu và khả năng báo cáo/giám sát phù hợp."] for r in FUNCTION_ROWS
    ])

    base.h(doc, 3, "2. Mô hình kiến trúc hệ thống tổng thể")
    base.code(doc, """
Người dùng/Doanh nghiệp/Nhà cung cấp/Chuyên gia/Cán bộ/Lãnh đạo
        |
        v
Kênh truy cập web bảo mật, tương thích PC - Tablet - Mobile
        |
        v
Dịch vụ Sàn giao dịch Khoa học và Công nghệ
  |-- Cổng thông tin Sàn giao dịch
  |-- Quản trị hệ thống
  |-- Quản lý nhà cung cấp, người mua, sản phẩm và công nghệ
  |-- Quản lý sự kiện - triển lãm
  |-- Quản lý giao dịch, hợp đồng, thu chi
  |-- Tiện ích, tích hợp, AI
  |-- Báo cáo - thống kê
        |
        +--> Kho dữ liệu và tài liệu thuộc phạm vi dịch vụ
        +--> Dịch vụ tích hợp: Email, SMS, chữ ký số, hợp đồng điện tử, VR360, API
        +--> Giám sát, sao lưu, bảo mật, hỗ trợ vận hành
    """)

    base.h(doc, 3, "3. Nguyên lý hoạt động")
    base.bullets(doc, [
        "Người dùng truy cập dịch vụ qua kênh web bảo mật; hệ thống xác thực và phân quyền theo vai trò.",
        "Các nhóm chức năng nghiệp vụ được tổ chức theo luồng dữ liệu chung: đăng ký/hồ sơ, công bố/tra cứu, kết nối/giao dịch, hợp đồng/thanh toán, báo cáo/thống kê.",
        "Các tiện ích tích hợp như chat, video call, livestream, QR, mạng xã hội, VR360 được kích hoạt theo phạm vi thuê dịch vụ và nhu cầu vận hành.",
        "Các chức năng AI chỉ đóng vai trò hỗ trợ tư vấn, tìm kiếm và gợi ý; kết quả AI cần được kiểm soát, không thay thế quyết định nghiệp vụ của cán bộ hoặc người dùng có thẩm quyền.",
        "Dữ liệu phát sinh được lưu trữ, sao lưu, phân quyền và có khả năng xuất/bàn giao theo quy định của hợp đồng thuê dịch vụ.",
    ])

    base.h(doc, 3, "4. Giải pháp quản lý hệ thống")
    base.table(doc, ["Nội dung quản lý", "Giải pháp đề xuất"], [
        ["Quản lý tài khoản", "Quản lý người dùng, vai trò, quyền chức năng, quyền dữ liệu, nhật ký đăng nhập và thao tác."],
        ["Quản lý danh mục", "Quản lý lĩnh vực công nghệ, nhóm sản phẩm, trạng thái pháp lý, hạng thành viên, sự kiện, cấu hình hiển thị."],
        ["Quản lý nội dung", "Quản lý trang chủ, tin tức, sự kiện, banner, bài giới thiệu, thông tin sản phẩm, gian hàng."],
        ["Quản lý giao dịch", "Quản lý báo giá, đơn hàng, hợp đồng, doanh thu, hoa hồng, thu chi, trạng thái giao dịch."],
        ["Quản lý tích hợp", "Quản lý cấu hình email, SMS, chữ ký số, hợp đồng điện tử, AI, VR360, API theo quyền quản trị."],
        ["Quản lý vận hành", "Theo dõi log, sao lưu, cảnh báo, chất lượng dịch vụ, yêu cầu hỗ trợ, bảo hành/bảo trì."],
    ])

    base.h(doc, 3, "5. Giải pháp kết nối")
    base.table(doc, ["Kênh/đối tượng kết nối", "Giải pháp đề xuất"], [
        ["Người dùng cuối", "Kết nối qua web bảo mật, tương thích đa thiết bị, hỗ trợ đăng ký nhận thông tin và thông báo."],
        ["Nhà cung cấp/người mua", "Kết nối qua hồ sơ tài khoản, gian hàng, giỏ hàng, nhu cầu tìm mua, báo giá, giao dịch và trao đổi."],
        ["Dịch vụ thông báo", "Kết nối Email/Web/SMS Brandname theo cấu hình; có log gửi và trạng thái xử lý."],
        ["Dịch vụ ký số/hợp đồng", "Kết nối chữ ký số và hợp đồng điện tử theo quy định pháp luật và phạm vi hợp đồng dịch vụ."],
        ["Dịch vụ AI", "Kết nối AI cho chatbot, tìm kiếm, gợi ý; có kiểm soát dữ liệu và cấu hình bật/tắt."],
        ["Hệ thống khác", "Kết nối API tích hợp theo yêu cầu; có tài liệu đặc tả, xác thực, phân quyền và log."],
    ])

    base.h(doc, 3, "6. Phương án bố trí lắp đặt thiết bị")
    base.p(doc, "Do dự án theo hình thức thuê dịch vụ CNTT, thiết kế không xác định chi tiết vị trí lắp đặt thiết bị của đơn vị cung cấp dịch vụ. Thay vào đó, thiết kế quy định yêu cầu tối thiểu đối với môi trường cung cấp dịch vụ, hạ tầng vận hành, kết nối, bảo mật, sao lưu và giám sát. Nếu dịch vụ sử dụng thiết bị tại địa điểm của Chủ đầu tư, phương án bố trí phải được trình duyệt riêng và bảo đảm không ảnh hưởng hạ tầng hiện hữu.")
    base.table(doc, ["Phương án", "Điều kiện áp dụng", "Yêu cầu kiểm soát"], [
        ["Dịch vụ đặt trên hạ tầng do đơn vị cung cấp dịch vụ quản lý", "Phù hợp khi thuê dịch vụ trọn gói.", "Cam kết chất lượng dịch vụ, bảo mật, sao lưu, quyền truy cập dữ liệu, bàn giao dữ liệu khi kết thúc dịch vụ."],
        ["Dịch vụ đặt trên hạ tầng của Chủ đầu tư", "Phù hợp khi Chủ đầu tư yêu cầu kiểm soát hạ tầng.", "Đơn vị cung cấp dịch vụ phối hợp cấu hình, vận hành, giám sát theo quy định của Chủ đầu tư."],
        ["Dịch vụ kết hợp", "Phù hợp khi một số thành phần cần đặt tại Chủ đầu tư, một số thành phần thuê ngoài.", "Phân định rõ trách nhiệm, điểm kết nối, bảo mật, dữ liệu và quy trình xử lý sự cố."],
    ])

    base.h(doc, 2, "VI. DANH MỤC THIẾT BỊ PHẦN CỨNG, PHẦN MỀM THƯƠNG MẠI:")
    base.p(doc, "Danh mục dưới đây được xác định theo yêu cầu dịch vụ và năng lực cần đáp ứng, không nhằm chỉ định nhãn hiệu, sản phẩm hoặc công nghệ cụ thể. Đơn vị cung cấp dịch vụ có thể đề xuất cấu hình hoặc sản phẩm tương đương, miễn đáp ứng yêu cầu kỹ thuật, an toàn thông tin, bản quyền và mức chất lượng dịch vụ.")
    base.table(doc, ["STT", "Hạng mục dịch vụ/thành phần", "Yêu cầu thiết kế", "Ghi chú"], [
        ["1", "Nền tảng cung cấp dịch vụ", "Đáp ứng vận hành ứng dụng web, quản trị dữ liệu, phân quyền, tích hợp, báo cáo, giám sát.", "Không chỉ định nền tảng cụ thể."],
        ["2", "Tài nguyên tính toán", "Đáp ứng số lượng người dùng, dung lượng dữ liệu, tần suất truy cập và khả năng mở rộng.", "Vật lý, ảo hóa, cloud hoặc tương đương."],
        ["3", "Tài nguyên lưu trữ", "Lưu dữ liệu nghiệp vụ, file, hình ảnh, video, tài liệu, hợp đồng, backup.", "Có chính sách lưu giữ và phục hồi."],
        ["4", "Hệ quản trị dữ liệu", "Hỗ trợ dữ liệu có cấu trúc, giao dịch, Unicode, phân quyền, sao lưu/khôi phục.", "CSDL quan hệ hoặc tương đương."],
        ["5", "Phần mềm/dịch vụ bảo mật", "Bảo vệ truy cập, giám sát, ghi log, cảnh báo, kiểm soát upload và phân quyền.", "Theo phương án cung cấp dịch vụ."],
        ["6", "Dịch vụ Email/Web notification", "Gửi thông báo, OTP hoặc nhắc việc theo cấu hình.", "Theo phạm vi thuê."],
        ["7", "Dịch vụ SMS Brandname", "Gửi SMS thông báo/OTP/nhắc lịch khi có yêu cầu.", "Theo phạm vi thuê."],
        ["8", "Dịch vụ chữ ký số/hợp đồng điện tử", "Ký hợp đồng/tài liệu điện tử, lưu trạng thái, audit log.", "Theo quy định pháp luật và hợp đồng."],
        ["9", "Dịch vụ AI", "Chatbot tư vấn, tìm kiếm thông minh, gợi ý sản phẩm/nhà cung cấp/công nghệ.", "Có kiểm soát dữ liệu và cấu hình bật/tắt."],
        ["10", "Dịch vụ VR360/livestream/video call", "Phục vụ triển lãm, gian hàng, tư vấn trực tuyến khi được phê duyệt.", "Tùy phạm vi triển khai."],
        ["11", "Dịch vụ giám sát, sao lưu", "Theo dõi trạng thái, sao lưu, khôi phục, báo cáo chất lượng dịch vụ.", "Bắt buộc trong thuê dịch vụ."],
        ["12", "Bản quyền phần mềm thương mại", "Nếu giải pháp sử dụng phần mềm thương mại, quyền sử dụng phải được tính trong chi phí thuê dịch vụ.", "Không phát sinh nghĩa vụ bản quyền ngoài hợp đồng nếu không được phê duyệt."],
    ])

    base.h(doc, 2, "VII. TỔ CHỨC THỰC HIỆN:")
    base.p(doc, "Tổ chức thực hiện trong thiết kế chi tiết được mô tả ở mức vai trò và trách nhiệm quản lý, không quy định cơ cấu nhân sự chi tiết của đơn vị cung cấp dịch vụ. Cơ cấu nhân sự, công cụ thực hiện và phương pháp triển khai cụ thể thuộc đề xuất của đơn vị cung cấp dịch vụ và được đánh giá ở giai đoạn lựa chọn nhà cung cấp hoặc ký hợp đồng.")
    base.table(doc, ["Vai trò", "Trách nhiệm chính"], [
        ["Chủ đầu tư/đơn vị sử dụng", "Phê duyệt yêu cầu, cung cấp dữ liệu, xác nhận quy trình, tổ chức nghiệm thu, quản lý tài khoản và giám sát chất lượng dịch vụ."],
        ["Đơn vị tư vấn lập thiết kế chi tiết", "Phân tích yêu cầu, đề xuất phương án kỹ thuật, xác định tiêu chí dịch vụ, lập tài liệu thiết kế chi tiết và hỗ trợ làm rõ khi thẩm định."],
        ["Đơn vị thẩm định/giám sát", "Rà soát tính phù hợp của thiết kế, yêu cầu kỹ thuật, tiêu chuẩn áp dụng, chi phí và khả năng nghiệm thu."],
        ["Đơn vị cung cấp dịch vụ CNTT", "Cung cấp, cấu hình, vận hành, bảo trì, hỗ trợ và bảo đảm chất lượng dịch vụ theo hợp đồng."],
        ["Đơn vị vận hành/khai thác", "Sử dụng hệ thống, quản trị nội dung/dữ liệu, theo dõi báo cáo, phản hồi lỗi và phối hợp hỗ trợ người dùng."],
        ["Đơn vị cung cấp dịch vụ tích hợp", "Cung cấp dịch vụ email, SMS, chữ ký số, AI, VR360 hoặc API theo phạm vi được phê duyệt."],
    ])

    base.h(doc, 2, "VIII. PHƯƠNG ÁN TỐ CHỨC LẮP ĐẶT, TRIỂN KHAI HỆ THỐNG:")
    base.h(doc, 3, "1. Cài đặt, kiểm tra:")
    base.p(doc, "Đối với thuê dịch vụ CNTT, nội dung cài đặt được hiểu là hoạt động thiết lập môi trường dịch vụ, cấu hình ban đầu, khởi tạo dữ liệu, tài khoản và kiểm tra điều kiện sẵn sàng khai thác. Thiết kế không quy định quy trình kỹ thuật nội bộ của đơn vị cung cấp dịch vụ.")
    base.bullets(doc, [
        "Thiết lập môi trường dịch vụ theo phương án được Chủ đầu tư phê duyệt.",
        "Cấu hình tên miền, truy cập bảo mật, tài khoản quản trị, phân quyền, danh mục ban đầu và tham số vận hành.",
        "Cấu hình các dịch vụ tích hợp được phê duyệt như Email, SMS, chữ ký số, hợp đồng điện tử, AI, VR360, API.",
        "Kiểm tra các chức năng cốt lõi, dữ liệu mẫu, báo cáo, thông báo, sao lưu và giám sát.",
    ])

    base.h(doc, 3, "2. Kiểm tra, nghiệm thu:")
    base.table(doc, ["Nhóm nghiệm thu", "Nội dung kiểm tra"], [
        ["Chức năng", "Kiểm tra 12 nhóm chức năng theo mô tả yêu cầu và vai trò người dùng."],
        ["Dữ liệu", "Kiểm tra danh mục, hồ sơ, sản phẩm, nhà cung cấp, người mua, giao dịch, báo cáo, xuất dữ liệu."],
        ["Tích hợp", "Kiểm tra Email/Web notification, SMS, chữ ký số, hợp đồng điện tử, AI, VR360, API theo phạm vi thuê."],
        ["An toàn thông tin", "Kiểm tra đăng nhập, phân quyền, log, bảo mật truy cập, bảo vệ dữ liệu cá nhân và sao lưu/khôi phục."],
        ["Chất lượng dịch vụ", "Kiểm tra thời gian phản hồi, tính sẵn sàng, hỗ trợ vận hành, báo cáo giám sát, quy trình xử lý sự cố."],
    ])

    base.h(doc, 3, "3. Vận hành thử nghiệm và hiệu chỉnh:")
    base.bullets(doc, [
        "Tổ chức vận hành thử trên dữ liệu mẫu hoặc dữ liệu được Chủ đầu tư cho phép.",
        "Theo dõi phản hồi người dùng, lỗi nghiệp vụ, lỗi giao diện, lỗi phân quyền, lỗi báo cáo và lỗi tích hợp.",
        "Hiệu chỉnh cấu hình chức năng, danh mục, biểu mẫu, báo cáo, quyền truy cập và thông báo trong phạm vi dịch vụ.",
        "Lập biên bản kết quả vận hành thử và danh sách nội dung đã hiệu chỉnh trước nghiệm thu chính thức.",
    ])

    base.h(doc, 3, "4. Công tác triển khai:")
    base.h(doc, 3, "4.1. Một số công tác trong quá trình triển khai:")
    base.bullets(doc, [
        "Thống nhất kế hoạch thiết lập dịch vụ, phạm vi dữ liệu khởi tạo, tài khoản và phân quyền ban đầu.",
        "Chuẩn hóa danh mục lĩnh vực công nghệ, nhóm sản phẩm, hạng thành viên, trạng thái pháp lý và trạng thái giao dịch.",
        "Tổ chức kiểm thử nghiệp vụ với đại diện các nhóm người dùng: quản trị, nhà cung cấp, người mua, cán bộ, lãnh đạo.",
        "Kiểm tra khả năng xuất dữ liệu, báo cáo, sao lưu, khôi phục và bàn giao dữ liệu theo yêu cầu thuê dịch vụ.",
    ])
    base.h(doc, 3, "4.2. Phương án đảm bảo an toàn hệ thống:")
    base.bullets(doc, [
        "Xác định cấp độ an toàn thông tin dự kiến và yêu cầu biện pháp bảo đảm tương ứng trong hợp đồng dịch vụ.",
        "Áp dụng xác thực, phân quyền, truy cập bảo mật, log/audit, kiểm soát upload và giám sát sự kiện.",
        "Quy định trách nhiệm sao lưu, khôi phục, bảo vệ dữ liệu cá nhân, xử lý sự cố và báo cáo vi phạm an toàn thông tin.",
        "Quy định quyền của Chủ đầu tư đối với dữ liệu, phương thức trích xuất dữ liệu và bàn giao dữ liệu khi kết thúc thuê dịch vụ.",
    ])

    base.h(doc, 2, "IX. YÊU CẦU CHI TIẾT VỀ VIỆC ĐÀO TẠO, TẬP HUẤN HƯỚNG DẪN SỬ DỤNG, QUẢN TRỊ, VẬN HÀNH")
    base.h(doc, 3, "1. Mục tiêu đào tạo:")
    base.p(doc, "Đào tạo nhằm giúp người dùng và cán bộ quản trị khai thác hiệu quả dịch vụ được thuê, hiểu phạm vi chức năng, quy trình nghiệp vụ, cách sử dụng báo cáo, cách phản hồi lỗi và cách phối hợp với đơn vị cung cấp dịch vụ trong quá trình vận hành.")
    base.h(doc, 3, "2. Phạm vi đào tạo:")
    base.table(doc, ["Đối tượng", "Nội dung đào tạo"], [
        ["Quản trị hệ thống phía Chủ đầu tư", "Tài khoản, phân quyền, danh mục, tham số vận hành, log, báo cáo, yêu cầu hỗ trợ."],
        ["Cán bộ nghiệp vụ", "Quản lý nhà cung cấp, sản phẩm/công nghệ, sự kiện, người mua, giao dịch, hợp đồng và báo cáo."],
        ["Nhà cung cấp/người mua/chuyên gia", "Cập nhật hồ sơ, gian hàng, sản phẩm, nhu cầu tìm mua, giỏ hàng, giao dịch, trao đổi và hỗ trợ."],
        ["Lãnh đạo/đơn vị quản lý", "Khai thác dashboard, báo cáo, thống kê và xuất dữ liệu."],
    ])
    base.h(doc, 3, "3. Phương án đào tạo:")
    base.bullets(doc, [
        "Đào tạo theo vai trò sử dụng; tập trung vào thao tác khai thác dịch vụ thay vì kỹ thuật triển khai nội bộ.",
        "Kết hợp tài liệu hướng dẫn, thực hành trên môi trường thử nghiệm và giải đáp tình huống nghiệp vụ.",
        "Ghi nhận câu hỏi thường gặp để hoàn thiện tài liệu hướng dẫn sử dụng trong thời gian thuê dịch vụ.",
    ])
    base.h(doc, 3, "4. Trình tự thực hiện:")
    base.bullets(doc, [
        "Chuẩn bị tài khoản, dữ liệu mẫu, kịch bản và tài liệu đào tạo.",
        "Đào tạo nhóm quản trị phía Chủ đầu tư.",
        "Đào tạo nhóm cán bộ nghiệp vụ và lãnh đạo khai thác báo cáo.",
        "Đào tạo/ hướng dẫn nhóm người dùng bên ngoài như nhà cung cấp, người mua, chuyên gia.",
        "Tổng hợp kết quả đào tạo và lập biên bản.",
    ])
    base.h(doc, 3, "5. Nội dung và phương án chuyển giao công nghệ:")
    base.p(doc, "Với hình thức thuê dịch vụ CNTT, chuyển giao được hiểu là chuyển giao tri thức sử dụng, quản trị, giám sát chất lượng dịch vụ, tài liệu cấu hình nghiệp vụ, dữ liệu và quyền khai thác theo hợp đồng. Các nội dung thuộc tài sản kỹ thuật nội bộ của đơn vị cung cấp dịch vụ không được đặt thành yêu cầu bàn giao, trừ trường hợp HSMT hoặc hợp đồng có quy định cụ thể.")
    base.bullets(doc, [
        "Chuyển giao tài liệu hướng dẫn sử dụng, tài liệu quản trị dịch vụ, tài liệu quy trình hỗ trợ và tài liệu nghiệm thu.",
        "Chuyển giao danh mục cấu hình nghiệp vụ, tài khoản quản trị phía Chủ đầu tư và quy trình tiếp nhận hỗ trợ.",
        "Chuyển giao dữ liệu, báo cáo, tài liệu phát sinh trong phạm vi thuê dịch vụ theo định dạng thống nhất.",
        "Chuyển giao hướng dẫn giám sát chất lượng dịch vụ, sao lưu/khôi phục theo trách nhiệm đã quy định.",
    ])
    base.h(doc, 3, "6. Yêu cầu cho đào tạo:")
    base.bullets(doc, [
        "Tài liệu đào tạo phải phù hợp với phiên bản dịch vụ được nghiệm thu.",
        "Có môi trường hoặc tài khoản thực hành phù hợp từng nhóm người dùng.",
        "Có biên bản đào tạo, danh sách người tham gia và nội dung đã hướng dẫn.",
        "Có kênh hỗ trợ sau đào tạo trong giai đoạn vận hành ban đầu.",
    ])

    base.h(doc, 2, "X. TÀI LIỆU BÀN GIAO")
    base.p(doc, "Danh mục tài liệu bàn giao được xác định phù hợp hình thức thuê dịch vụ CNTT. Trọng tâm là tài liệu thiết kế, tài liệu yêu cầu dịch vụ, tài liệu sử dụng/quản trị, tài liệu vận hành ở phía Chủ đầu tư, tài liệu nghiệm thu và dữ liệu phát sinh trong quá trình thuê dịch vụ.")
    base.table(doc, ["STT", "Tài liệu/sản phẩm", "Nội dung bàn giao"], [
        ["1", "Tài liệu thiết kế chi tiết", "Phạm vi dịch vụ, kiến trúc, yêu cầu chức năng, yêu cầu phi chức năng, tiêu chuẩn, tích hợp, an toàn thông tin."],
        ["2", "Tài liệu yêu cầu dịch vụ", "Mức chất lượng dịch vụ, trách nhiệm vận hành, thời gian phản hồi, bảo hành/bảo trì, báo cáo định kỳ."],
        ["3", "Tài liệu hướng dẫn sử dụng", "Hướng dẫn cho người dùng công khai, nhà cung cấp, người mua, chuyên gia, cán bộ nghiệp vụ."],
        ["4", "Tài liệu quản trị phía Chủ đầu tư", "Hướng dẫn tài khoản, phân quyền, danh mục, cấu hình nghiệp vụ, báo cáo, tiếp nhận hỗ trợ."],
        ["5", "Tài liệu tích hợp", "Đặc tả kết nối email, SMS, chữ ký số, AI, VR360, API nếu thuộc phạm vi thuê."],
        ["6", "Tài liệu kiểm thử/nghiệm thu", "Kịch bản kiểm thử, kết quả kiểm thử, danh sách lỗi, biên bản UAT, biên bản nghiệm thu."],
        ["7", "Tài liệu đào tạo", "Tài liệu tập huấn, danh sách học viên, biên bản đào tạo, câu hỏi thường gặp."],
        ["8", "Báo cáo vận hành dịch vụ", "Báo cáo chất lượng dịch vụ, sự cố, hỗ trợ, sao lưu, bảo trì trong thời gian thuê."],
        ["9", "Dữ liệu bàn giao/kết xuất", "Dữ liệu phát sinh trong phạm vi dịch vụ, kết xuất theo định dạng thống nhất khi Chủ đầu tư yêu cầu hoặc khi kết thúc dịch vụ."],
        ["10", "Biên bản bàn giao", "Biên bản thiết lập dịch vụ, đào tạo, nghiệm thu, bàn giao tài khoản quản trị và tài liệu."],
    ])

    doc.core_properties.title = "Phần 7 - Thiết kế chi tiết thuê dịch vụ CNTT - Sàn giao dịch KHCN"
    doc.core_properties.subject = "Bản v3.2 viết theo vai trò đơn vị tư vấn lập thiết kế chi tiết"
    doc.core_properties.author = "Codex hỗ trợ tổng hợp"
    OUT.parent.mkdir(parents=True, exist_ok=True)
    doc.save(str(OUT))
    print(str(OUT).encode("ascii", errors="backslashreplace").decode("ascii"))


if __name__ == "__main__":
    build()
