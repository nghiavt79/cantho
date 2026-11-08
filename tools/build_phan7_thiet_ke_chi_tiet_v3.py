from __future__ import annotations

from pathlib import Path
import importlib.util

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs" / "ho so ky thuat" / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.0.docx"

spec = importlib.util.spec_from_file_location("hsyc_base", ROOT / "tools" / "build_hsyc_full_cdt.py")
base = importlib.util.module_from_spec(spec)
assert spec.loader is not None
spec.loader.exec_module(base)


def build():
    doc = Document()
    base.style_doc(doc)

    base.title(doc, "PHẦN 7. THIẾT KẾ CHI TIẾT", 17)
    base.p(
        doc,
        "Dự án: Sàn giao dịch Khoa học và Công nghệ",
        bold=True,
        align=WD_ALIGN_PARAGRAPH.CENTER,
    )
    base.p(
        doc,
        "Tài liệu này trình bày thiết kế chi tiết hệ thống theo khung Báo cáo kinh tế kỹ thuật, gồm yêu cầu thiết kế, tiêu chuẩn áp dụng, phân tích kiến trúc số, phương án kỹ thuật, giải pháp triển khai, đào tạo và tài liệu bàn giao.",
        italic=True,
        align=WD_ALIGN_PARAGRAPH.CENTER,
    )

    base.h(doc, 1, "A. YÊU CẦU THIẾT KẾ CHI TIẾT:")
    base.bullets(doc, [
        "Thiết kế chi tiết phải tuân thủ Khung kiến trúc tổng thể quốc gia số, Khung kiến trúc Chính phủ số Việt Nam, Khung kiến trúc số của địa phương và các quy định về ứng dụng công nghệ thông tin trong cơ quan nhà nước.",
        "Thiết kế phải thể hiện được đầy đủ yêu cầu nghiệp vụ, yêu cầu chức năng, yêu cầu phi chức năng, mô hình kiến trúc, phương án hạ tầng, cơ sở dữ liệu, tích hợp, an toàn thông tin, vận hành, đào tạo và chuyển giao.",
        "Thiết kế phải xác định được các nhóm sản phẩm, phần mềm, phần cứng, dịch vụ kỹ thuật, dịch vụ triển khai và tài liệu bàn giao làm căn cứ xác định khối lượng, dự toán và tổng mức đầu tư.",
        "Thiết kế phải bảo đảm hệ thống có khả năng triển khai thực tế, mở rộng, tích hợp, vận hành ổn định, bảo mật và phù hợp với điều kiện hạ tầng của Chủ đầu tư.",
        "Thiết kế phải bảo đảm không phụ thuộc cứng vào một nhà cung cấp dịch vụ bên ngoài; các tích hợp như AI, email, SMS, chữ ký số, API phải có cấu hình bật/tắt, kiểm soát dữ liệu và phương án dự phòng.",
    ])

    base.h(doc, 1, "B. NỘI DUNG CHÍNH CỦA THIẾT KẾ CHI TIẾT:")
    base.h(doc, 2, "I. TÓM TẮT NỘI DUNG YÊU CẦU CỦA DỰ ÁN:")
    base.h(doc, 3, "1. Yêu cầu chung:")
    base.p(doc, "Dự án Sàn giao dịch Khoa học và Công nghệ nhằm xây dựng một nền tảng số tập trung phục vụ công bố, quản lý, tìm kiếm, kết nối cung - cầu, tư vấn, giao dịch và chuyển giao công nghệ. Hệ thống cần đáp ứng nhu cầu của cơ quan quản lý, doanh nghiệp, nhà cung ứng, chuyên gia, tổ chức khoa học công nghệ và người dân trong việc khai thác dữ liệu, gửi nhu cầu, đề xuất giải pháp, theo dõi giao dịch, ký kết hợp đồng và quản lý kết quả chuyển giao.")
    base.bullets(doc, [
        "Xây dựng cổng thông tin công khai về công nghệ, thiết bị, tài sản trí tuệ, nhu cầu công nghệ, nhà cung ứng, chuyên gia, tin tức, sự kiện, video, diễn đàn và thông tin liên hệ.",
        "Xây dựng phân hệ quản trị/CMS cho phép quản lý danh mục, nội dung, người dùng, phân quyền, sản phẩm, nhà cung ứng, chuyên gia, phản hồi, quảng cáo, dashboard và cấu hình hệ thống.",
        "Số hóa quy trình giao dịch/chuyển giao công nghệ từ tiếp nhận nhu cầu, NDA, RFQ, đề xuất, đánh giá, đàm phán, rà soát pháp lý, hợp đồng, ký số, tạm ứng, thử nghiệm, bàn giao, đào tạo, tài liệu kỹ thuật, nghiệm thu đến thanh lý.",
        "Hỗ trợ tìm kiếm nâng cao, gợi ý thông minh, AI Support/chatbot, báo cáo thống kê và thông báo tự động.",
        "Bảo đảm an toàn thông tin, bảo vệ dữ liệu cá nhân, khả năng sao lưu/khôi phục, khả năng tích hợp, khả năng triển khai IPv6 và vận hành lâu dài.",
    ])

    base.h(doc, 3, "2. Yêu cầu cụ thể:")
    base.table(doc, ["STT", "Nhóm yêu cầu", "Nội dung cụ thể"], [
        ["1", "Người dùng và phân quyền", "Quản lý tài khoản, đăng nhập/đăng xuất, OTP/quên mật khẩu, vai trò, nhóm quyền, phân quyền theo chức năng, theo hồ sơ và theo bước quy trình."],
        ["2", "Cổng thông tin công khai", "Trang chủ, giới thiệu, tin tức, sự kiện, video, banner, công nghệ, thiết bị, tài sản trí tuệ, nhu cầu công nghệ, nhà cung ứng, chuyên gia, diễn đàn, liên hệ."],
        ["3", "Quản trị nội dung/CMS", "Quản lý menu, danh mục, bài viết, sản phẩm, nhà cung ứng, chuyên gia, quảng cáo, phản hồi, người dùng, dashboard, tham số hệ thống."],
        ["4", "Sản phẩm và công nghệ", "Quản lý hồ sơ công nghệ, thiết bị, tài sản trí tuệ, hình ảnh, video, tài liệu, thông số, tình trạng pháp lý, lĩnh vực và trạng thái kiểm duyệt."],
        ["5", "Nhà cung ứng/chuyên gia", "Quản lý hồ sơ pháp lý, năng lực, lĩnh vực, sản phẩm liên quan, lịch sử tham gia, hồ sơ chuyên gia và phân công tư vấn."],
        ["6", "Nhu cầu công nghệ", "Tiếp nhận nhu cầu, phân loại, gợi ý nguồn cung, theo dõi xử lý, phản hồi kết quả và chuyển thành hồ sơ giao dịch."],
        ["7", "Quy trình chuyển giao", "Quản lý hồ sơ giao dịch/chuyển giao theo các bước nghiệp vụ, có trạng thái, người phụ trách, thời hạn, tài liệu, thông báo và log."],
        ["8", "Hợp đồng/ký số", "Tạo/upload hợp đồng, quản lý phiên bản, phê duyệt nhiều bên, ký số, hash kiểm tra toàn vẹn, audit log, tải hồ sơ hợp đồng."],
        ["9", "Tìm kiếm/AI", "Tìm kiếm toàn văn, tìm kiếm nâng cao, gợi ý sản phẩm/nhà cung ứng/chuyên gia, AI Support/chatbot, knowledge base và log truy vấn."],
        ["10", "Thông báo/báo cáo", "Thông báo web/email/SMS khi cấu hình, dashboard, báo cáo sản phẩm, nhà cung ứng, nhu cầu, giao dịch, hợp đồng, sự kiện, truy cập."],
        ["11", "Vận hành", "Cấu hình hệ thống, backup/restore, log, giám sát, quản lý file upload, phân quyền CSDL, tài liệu vận hành và hỗ trợ kỹ thuật."],
    ])

    base.h(doc, 2, "II. DANH MỤC QUY CHUẨN KỸ THUẬT, TIÊU CHUẨN ĐƯỢC ÁP DỤNG")
    base.p(doc, "Hệ thống phải tuân thủ các quy chuẩn, tiêu chuẩn và văn bản kỹ thuật hiện hành liên quan đến ứng dụng công nghệ thông tin, an toàn thông tin, giao dịch điện tử, dữ liệu cá nhân, kết nối/chia sẻ dữ liệu và triển khai hệ thống web trong cơ quan nhà nước.")
    standards = [
        ["1", "Ứng dụng CNTT trong cơ quan nhà nước", "Thông tư 39/2017/TT-BTTTT", "Danh mục tiêu chuẩn kỹ thuật về ứng dụng CNTT trong cơ quan nhà nước", "Bắt buộc áp dụng"],
        ["2", "Kết nối hệ thống thông tin", "Thông tư 13/2017/TT-BTTTT và văn bản liên quan", "Yêu cầu kỹ thuật về kết nối hệ thống thông tin, cơ sở dữ liệu", "Áp dụng khi tích hợp"],
        ["3", "An toàn thông tin", "Luật An toàn thông tin mạng", "Quy định chung về bảo đảm an toàn thông tin mạng", "Bắt buộc áp dụng"],
        ["4", "ATTT theo cấp độ", "Nghị định 85/2016/NĐ-CP", "Bảo đảm an toàn hệ thống thông tin theo cấp độ", "Bắt buộc áp dụng"],
        ["5", "ATTT theo cấp độ", "Thông tư 12/2022/TT-BTTTT", "Hướng dẫn xác định cấp độ và phương án bảo đảm ATTT", "Bắt buộc áp dụng"],
        ["6", "Dữ liệu cá nhân", "Nghị định 13/2023/NĐ-CP", "Bảo vệ dữ liệu cá nhân", "Bắt buộc áp dụng"],
        ["7", "Dữ liệu cá nhân", "Nghị định 356/2025/NĐ-CP", "Sửa đổi, bổ sung quy định liên quan bảo vệ dữ liệu cá nhân", "Áp dụng theo hiệu lực văn bản"],
        ["8", "Giao dịch điện tử", "Luật Giao dịch điện tử và văn bản hướng dẫn", "Giao dịch, hồ sơ, chứng từ, hợp đồng điện tử", "Áp dụng khi triển khai giao dịch điện tử"],
        ["9", "Chữ ký số", "Quy định về chữ ký số, chứng thư số", "Ký hợp đồng/tài liệu điện tử", "Áp dụng khi triển khai ký số"],
        ["10", "Kết nối web", "HTTP/HTTPS", "Giao thức truyền tải ứng dụng web", "Bắt buộc"],
        ["11", "Bảo mật truyền tải", "TLS 1.2 trở lên", "Mã hóa kết nối giữa người dùng và hệ thống", "Bắt buộc"],
        ["12", "API", "REST/JSON", "Trao đổi dữ liệu với hệ thống ngoài", "Bắt buộc khi tích hợp"],
        ["13", "Dữ liệu", "Unicode/UTF-8; TCVN 6909:2001", "Lưu trữ và hiển thị tiếng Việt", "Bắt buộc"],
        ["14", "Tài liệu", "PDF, DOCX, XLSX, CSV", "Lưu trữ, trao đổi, xuất báo cáo và biểu mẫu", "Bắt buộc/khuyến nghị"],
        ["15", "Ảnh/video", "JPEG, PNG, WebP, MP4 hoặc nhúng video", "Lưu hình ảnh, video giới thiệu sản phẩm/sự kiện", "Áp dụng theo chức năng"],
        ["16", "CSDL", "Hệ quản trị CSDL quan hệ, transaction, backup/restore", "Lưu trữ dữ liệu nghiệp vụ", "Bắt buộc"],
        ["17", "ATTT ứng dụng web", "OWASP Top 10/ASVS tham khảo", "Phòng chống rủi ro ứng dụng web phổ biến", "Khuyến nghị áp dụng"],
        ["18", "IPv6", "IPv4/IPv6 dual-stack", "Sẵn sàng triển khai IPv6", "Bắt buộc khi hạ tầng đáp ứng"],
        ["19", "Email", "SMTP/SMTPS", "Gửi thông báo, OTP, nhắc việc", "Áp dụng khi cấu hình email"],
        ["20", "SMS", "API SMS/Brandname", "Gửi OTP/thông báo quan trọng", "Theo yêu cầu"],
        ["21", "Quản lý chất lượng", "ISO 9001 hoặc tương đương", "Quy trình chất lượng phần mềm", "Khuyến nghị/yêu cầu theo HSMT"],
        ["22", "Quản lý ATTT", "ISO/IEC 27001 hoặc tương đương", "Quản lý an toàn thông tin", "Khuyến nghị/yêu cầu theo HSMT"],
        ["23", "Trình duyệt", "Chrome, Edge, Firefox, Safari, Cốc Cốc", "Tương thích trình duyệt phổ biến", "Bắt buộc"],
        ["24", "Giao diện", "HTML5, CSS3, responsive", "Hiển thị trên PC, tablet, mobile", "Bắt buộc"],
        ["25", "Nhật ký", "Audit log, system log", "Truy vết thao tác và sự kiện hệ thống", "Bắt buộc"],
    ]
    base.table(doc, ["Số TT", "Loại tiêu chuẩn", "Ký hiệu/nhóm tiêu chuẩn", "Tên đầy đủ/nội dung tiêu chuẩn", "Quy định áp dụng"], standards)

    base.h(doc, 2, "III. PHÂN TÍCH SỰ TUÂN THỦ KHUNG KIẾN TRÚC TỔNG THỂ QUỐC GIA SỐ:")
    base.p(doc, "Dự án Sàn giao dịch Khoa học và Công nghệ được thiết kế phù hợp định hướng Khung kiến trúc Chính phủ số Việt Nam, Khung kiến trúc tổng thể quốc gia số và Khung kiến trúc số của địa phương. Hệ thống đóng vai trò là nền tảng ứng dụng chuyên ngành phục vụ chuyển đổi số lĩnh vực khoa học và công nghệ, kết nối dữ liệu, dịch vụ, người dùng và quy trình nghiệp vụ.")
    base.table(doc, ["Lớp kiến trúc", "Nội dung tuân thủ"], [
        ["Lớp người dùng/kênh truy cập", "Hỗ trợ người dân, doanh nghiệp, nhà cung ứng, chuyên gia, cán bộ, lãnh đạo truy cập qua trình duyệt web trên PC, tablet, mobile."],
        ["Lớp ứng dụng", "Tổ chức thành các phân hệ: cổng thông tin, CMS, sản phẩm/công nghệ, nhu cầu, nhà cung ứng, chuyên gia, workflow chuyển giao, hợp đồng/ký số, AI/tìm kiếm, báo cáo."],
        ["Lớp dữ liệu", "Cơ sở dữ liệu tập trung, dữ liệu danh mục dùng chung, dữ liệu nghiệp vụ, dữ liệu giao dịch, dữ liệu báo cáo, log, dữ liệu AI/knowledge base."],
        ["Lớp tích hợp", "Có khả năng kết nối email, SMS, chữ ký số, AI provider, API, hệ thống/nền tảng dùng chung, chia sẻ dữ liệu theo yêu cầu quản lý."],
        ["Lớp an toàn thông tin", "Xác thực, phân quyền, log/audit, kiểm soát upload, bảo vệ dữ liệu cá nhân, quản lý secret, backup/restore, phương án ATTT theo cấp độ."],
        ["Lớp vận hành", "Có giám sát, logging, backup, tài liệu vận hành, đào tạo, chuyển giao, bảo hành, bảo trì và hỗ trợ kỹ thuật."],
    ])
    base.bullets(doc, [
        "Tuân thủ nguyên tắc lấy dữ liệu làm trung tâm, thúc đẩy khai thác và chia sẻ dữ liệu có kiểm soát.",
        "Tuân thủ nguyên tắc phát triển nền tảng số dùng chung, tránh trùng lặp, có khả năng tích hợp và mở rộng.",
        "Tuân thủ nguyên tắc an toàn, an ninh mạng ngay từ thiết kế; dữ liệu cá nhân và dữ liệu hồ sơ giao dịch được phân quyền rõ ràng.",
        "Tuân thủ nguyên tắc triển khai dịch vụ số thuận tiện, dễ sử dụng, minh bạch trạng thái xử lý và hỗ trợ người dùng đa kênh.",
    ])

    base.h(doc, 2, "IV. PHÂN TÍCH LỰA CHỌN PHƯƠNG ÁN KỸ THUẬT, CÔNG NGHỆ, THIẾT BỊ")
    base.h(doc, 3, "1. Các ràng buộc đối với hệ thống gồm: ràng buộc môi trường, sự phụ thuộc vào hệ thống nền tảng")
    base.bullets(doc, [
        "Hệ thống phải triển khai được trên hạ tầng máy chủ của Chủ đầu tư hoặc hạ tầng được chỉ định, bảo đảm kết nối internet/intranet theo phương án vận hành.",
        "Hệ thống cần tương thích với mô hình triển khai web tập trung, CSDL tập trung và kho lưu trữ file/tài liệu.",
        "Hệ thống phụ thuộc vào các dịch vụ ngoài theo cấu hình như email, SMS, AI, chữ ký số; khi dịch vụ ngoài lỗi phải có thông báo/fallback, không làm dừng toàn bộ hệ thống.",
        "Hệ thống phải đáp ứng các giới hạn về dung lượng upload, dung lượng lưu trữ, băng thông, phân quyền dữ liệu và tài nguyên máy chủ.",
    ])
    base.h(doc, 3, "2. Yêu cầu về cài đặt, hạ tầng, đường truyền, an toàn vận hành, khai thác, sử dụng, khả nâng cấu hình hệ thống chạy trên nền Ipv6")
    base.table(doc, ["Hạng mục", "Yêu cầu"], [
        ["Máy chủ ứng dụng", "Đáp ứng khả năng chạy ứng dụng web, phục vụ người dùng đồng thời, xử lý file tĩnh, log, job nền, HTTPS."],
        ["Máy chủ CSDL", "Đáp ứng lưu trữ dữ liệu nghiệp vụ, có backup/restore, phân quyền, tối ưu chỉ mục và khả năng mở rộng."],
        ["Đường truyền", "Bảo đảm băng thông, độ ổn định, tên miền, chứng thư số TLS và kết nối outbound đến dịch vụ tích hợp khi cần."],
        ["IPv6", "Sẵn sàng hoạt động trên mô hình dual-stack IPv4/IPv6 thông qua web server/reverse proxy, không hard-code địa chỉ IPv4 trong nghiệp vụ."],
        ["An toàn vận hành", "Có firewall, phân quyền máy chủ/CSDL, log, backup định kỳ, kiểm soát upload và quy trình cập nhật phiên bản."],
    ])
    base.h(doc, 3, "3. Yêu cầu kỹ thuật, giải pháp công nghệ đối với thiết bị phần cứng, phần mềm thương mại")
    base.table(doc, ["Nhóm", "Yêu cầu kỹ thuật/giải pháp"], [
        ["Máy chủ/vCPU/RAM/SSD", "Tài nguyên phù hợp quy mô triển khai; khuyến nghị tách máy chủ ứng dụng và CSDL khi vận hành chính thức hoặc khi lưu lượng tăng."],
        ["Hệ điều hành", "Windows Server hoặc Linux tùy phương án triển khai; có bản quyền/hỗ trợ phù hợp nếu sử dụng phần mềm thương mại."],
        ["Web server", "IIS/Kestrel/Nginx/Apache hoặc reverse proxy tương đương; hỗ trợ HTTPS, log, giới hạn request, header bảo mật."],
        ["CSDL", "SQL Server hoặc hệ CSDL quan hệ tương đương; có license nếu dùng bản thương mại; hỗ trợ backup/restore và Unicode tiếng Việt."],
        ["Chữ ký số", "Thiết bị USB Token/HSM hoặc dịch vụ ký số từ nhà cung cấp được cấp phép khi triển khai ký số."],
        ["AI/SMS/Email", "Dịch vụ thương mại được cấu hình qua tham số; có hợp đồng/tài khoản riêng nếu Chủ đầu tư yêu cầu sử dụng chính thức."],
    ])
    base.h(doc, 3, "4. Phân tích, lựa chọn phương án bảo đảm tính kết nối, liên thông, chia sẻ dữ liệu")
    base.bullets(doc, [
        "Lựa chọn phương án API REST/JSON làm cơ chế tích hợp chính do phổ biến, dễ tài liệu hóa, dễ kiểm thử và phù hợp hệ thống web.",
        "Dữ liệu xuất/nhập hỗ trợ các định dạng thông dụng như Excel, CSV, PDF, JSON; phục vụ báo cáo, chuyển đổi dữ liệu và tích hợp.",
        "Các API phải có xác thực, phân quyền, giới hạn truy cập, log giao dịch và tài liệu đặc tả request/response.",
        "Phân hệ AI/tìm kiếm sử dụng dữ liệu nội bộ đã được đồng bộ/kiểm duyệt, bảo đảm không chia sẻ dữ liệu nhạy cảm trái phép ra dịch vụ ngoài.",
    ])
    base.h(doc, 3, "5. Phân tích, lựa chọn phương án công nghệ, kỹ thuật bảo đảm an toàn, an ninh mạng")
    base.bullets(doc, [
        "Áp dụng xác thực tài khoản, phân quyền theo vai trò, phân quyền theo hồ sơ và bước quy trình.",
        "Bảo vệ phiên đăng nhập bằng cookie HttpOnly/Secure, timeout, chống giả mạo request và validate dữ liệu đầu vào.",
        "Mã hóa đường truyền HTTPS/TLS; không truyền dữ liệu nhạy cảm qua kênh không bảo mật.",
        "Kiểm soát file upload theo extension, dung lượng, tên file, quyền truy cập; không cho thực thi file upload.",
        "Ghi log/audit các thao tác quan trọng: đăng nhập, phân quyền, cập nhật hồ sơ, phê duyệt, ký số, thay đổi hợp đồng, tích hợp API.",
        "Thiết lập backup/restore, quản lý secret, phân quyền CSDL, firewall và kiểm thử bảo mật trước nghiệm thu.",
    ])
    base.h(doc, 3, "6. Các ràng buộc đối với hệ thống gồm: ràng buộc môi trường, sự phụ thuộc vào hệ thống nền tảng")
    base.p(doc, "Ngoài các ràng buộc tại mục 1, hệ thống cần tuân thủ ràng buộc về nhân sự vận hành, năng lực hạ tầng, chính sách bảo mật của Chủ đầu tư, tài khoản tích hợp, bản quyền phần mềm thương mại và khả năng duy trì dịch vụ sau nghiệm thu. Các thành phần tích hợp phải có thông tin cấu hình, tài khoản, endpoint, giới hạn sử dụng và phương án xử lý khi dịch vụ gián đoạn.")
    base.h(doc, 3, "7. Yêu cầu về cài đặt, hạ tầng, đường truyền, an toàn vận hành, khai thác, sử dụng, khả năng cấu hình hệ thống chạy trên nền IPv6")
    base.p(doc, "Hệ thống phải có khả năng cấu hình để vận hành trên hạ tầng hỗ trợ IPv6 khi Chủ đầu tư triển khai. Nhà thầu cần phối hợp kiểm tra tên miền, DNS AAAA, firewall, reverse proxy/web server, chứng thư số, log truy cập, API callback và các tích hợp ngoài để bảo đảm hoạt động trong môi trường IPv4/IPv6 dual-stack.")

    base.h(doc, 2, "V. THUYẾT MINH GIẢI PHÁP KỸ THUẬT:")
    base.h(doc, 3, "1. Quan điểm xây dựng")
    base.bullets(doc, [
        "Xây dựng hệ thống theo hướng nền tảng số mở, module hóa, dễ mở rộng, dễ tích hợp và dễ vận hành.",
        "Lấy dữ liệu và quy trình nghiệp vụ của Chủ đầu tư làm trung tâm; không thiết kế theo hướng phụ thuộc vào sản phẩm riêng của nhà thầu.",
        "Ưu tiên trải nghiệm người dùng, thao tác rõ ràng, biểu mẫu kiểm tra dữ liệu, thông báo trạng thái minh bạch.",
        "Bảo đảm an toàn thông tin, bảo vệ dữ liệu cá nhân và kiểm soát truy cập ngay từ giai đoạn thiết kế.",
        "Bảo đảm có đủ tài liệu kỹ thuật, tài liệu người dùng, tài liệu vận hành, đào tạo và chuyển giao.",
    ])
    base.h(doc, 3, "2. Mô hình kiến trúc hệ thống tổng thể")
    base.code(doc, """
Người dùng công khai / Doanh nghiệp / Nhà cung ứng / Chuyên gia / Cán bộ / Lãnh đạo
        |
        v
Trình duyệt Web PC - Tablet - Mobile
        |
        v
Tên miền + HTTPS + Firewall/WAF + Reverse Proxy/Web Server
        |
        v
Ứng dụng Sàn giao dịch Khoa học và Công nghệ
  |-- Cổng thông tin công khai
  |-- CMS/quản trị dữ liệu
  |-- Sản phẩm, công nghệ, thiết bị, tài sản trí tuệ
  |-- Nhu cầu công nghệ, nhà cung ứng, chuyên gia
  |-- Workflow giao dịch/chuyển giao công nghệ
  |-- Hợp đồng điện tử, ký số, tài liệu
  |-- Tìm kiếm, AI Support, gợi ý thông minh
  |-- Báo cáo, dashboard, thông báo
        |
        +--> Cơ sở dữ liệu tập trung
        +--> Kho file/tài liệu/hình ảnh
        +--> Email/SMS/OTP
        +--> Chữ ký số/Hợp đồng điện tử
        +--> API/nền tảng dùng chung/hệ thống ngoài
    """)
    base.h(doc, 3, "3. Nguyên lý hoạt động")
    base.bullets(doc, [
        "Người dùng truy cập hệ thống qua trình duyệt web; các chức năng công khai cho phép tra cứu và gửi thông tin liên hệ/nhu cầu.",
        "Người dùng đăng nhập được phân quyền theo vai trò; hệ thống xác định quyền truy cập chức năng, dữ liệu và hồ sơ giao dịch.",
        "Cán bộ quản trị/CMS cập nhật nội dung, danh mục, sản phẩm, nhà cung ứng, chuyên gia, phản hồi và báo cáo.",
        "Hồ sơ giao dịch/chuyển giao được tạo và luân chuyển theo các bước nghiệp vụ; mỗi bước có trạng thái, tài liệu, thông báo, người phụ trách và log.",
        "Hợp đồng/tài liệu được tạo hoặc upload, phê duyệt, ký số khi cấu hình, lưu phiên bản và audit log.",
        "Tìm kiếm/AI khai thác dữ liệu nội bộ đã được đồng bộ/kiểm duyệt để gợi ý và hỗ trợ người dùng.",
        "Dashboard/báo cáo tổng hợp dữ liệu theo phân quyền phục vụ cán bộ và lãnh đạo.",
    ])
    base.h(doc, 3, "4. Giải pháp quản lý hệ thống")
    base.table(doc, ["Nhóm quản lý", "Giải pháp"], [
        ["Tài khoản/phân quyền", "Quản lý người dùng, vai trò, nhóm quyền, quyền theo hồ sơ/quy trình, khóa/mở tài khoản và log truy cập."],
        ["Cấu hình", "Quản lý tham số hệ thống, email, SMS, AI, ký số, upload, dashboard, thông tin đơn vị và mẫu thông báo."],
        ["Dữ liệu", "Quản lý danh mục dùng chung, dữ liệu nghiệp vụ, dữ liệu báo cáo, dữ liệu AI/knowledge base và log."],
        ["Nội dung", "CMS quản lý menu, bài viết, tin tức, sự kiện, video, banner, quảng cáo, trang giới thiệu/quy định."],
        ["Vận hành", "Theo dõi log, backup/restore, dung lượng lưu trữ, job nền, trạng thái tích hợp và cảnh báo lỗi."],
        ["Hỗ trợ", "Tiếp nhận phản hồi, yêu cầu tư vấn, lead từ chatbot, khiếu nại/hỗ trợ và trạng thái xử lý."],
    ])
    base.h(doc, 3, "5. Giải pháp kết nối")
    base.table(doc, ["Kết nối", "Phương án"], [
        ["Người dùng - hệ thống", "HTTPS qua tên miền chính thức, hỗ trợ trình duyệt phổ biến, responsive trên thiết bị di động."],
        ["Ứng dụng - CSDL", "Kết nối nội bộ bảo mật, tài khoản CSDL phân quyền, không mở trực tiếp CSDL ra internet."],
        ["Email/SMS", "Kết nối qua SMTP/API nhà cung cấp; có log gửi, cấu hình bật/tắt, xử lý lỗi."],
        ["Chữ ký số", "Kết nối nhà cung cấp chữ ký số/hợp đồng điện tử hoặc ký số cục bộ theo yêu cầu; lưu trạng thái ký và audit log."],
        ["AI provider", "Kết nối API khi được bật; kiểm soát dữ liệu gửi đi, có timeout/fallback và không lưu secret ở frontend."],
        ["Nền tảng dùng chung/API", "REST/JSON, xác thực, phân quyền, log, tài liệu đặc tả; sẵn sàng tích hợp khi Chủ đầu tư yêu cầu."],
    ])
    base.h(doc, 3, "6. Phương án bố trí lắp đặt thiết bị")
    base.p(doc, "Đối với hệ thống phần mềm web tập trung, thiết bị phần cứng chủ yếu là hạ tầng máy chủ, lưu trữ, thiết bị mạng, firewall/reverse proxy, thiết bị ký số hoặc dịch vụ ký số nếu có. Phương án bố trí có thể triển khai theo mô hình tối thiểu hoặc mô hình tách lớp tùy quy mô vận hành.")
    base.table(doc, ["Mô hình", "Bố trí đề xuất", "Ghi chú"], [
        ["Mô hình tối thiểu", "01 máy chủ ứng dụng kiêm web server, 01 máy chủ CSDL hoặc CSDL tách biệt, vùng lưu trữ file, backup định kỳ.", "Phù hợp giai đoạn đầu/lưu lượng vừa."],
        ["Mô hình khuyến nghị", "Reverse proxy/firewall, máy chủ ứng dụng riêng, máy chủ CSDL riêng, vùng file storage/backup riêng, giám sát log.", "Phù hợp vận hành chính thức."],
        ["Mô hình mở rộng", "Load balancer, nhiều máy chủ ứng dụng, CSDL dự phòng, storage tập trung, giám sát tập trung, WAF.", "Áp dụng khi lưu lượng và yêu cầu sẵn sàng cao."],
        ["Thiết bị ký số", "USB Token/HSM hoặc dịch vụ ký số từ nhà cung cấp được cấp phép.", "Áp dụng khi triển khai ký số/hợp đồng điện tử."],
    ])

    base.h(doc, 2, "VI. DANH MỤC THIẾT BỊ PHẦN CỨNG, PHẦN MỀM THƯƠNG MẠI:")
    base.table(doc, ["STT", "Hạng mục", "Mô tả/yêu cầu", "Ghi chú"], [
        ["1", "Máy chủ ứng dụng", "CPU/RAM/SSD phù hợp quy mô; chạy ứng dụng web, job nền, log.", "Có thể dùng hạ tầng hiện có/cloud/ảo hóa."],
        ["2", "Máy chủ cơ sở dữ liệu", "Lưu CSDL tập trung, backup/restore, phân quyền, tối ưu hiệu năng.", "Có thể tách riêng để bảo mật/hiệu năng."],
        ["3", "Hệ điều hành máy chủ", "Windows Server hoặc Linux tùy phương án triển khai.", "Bảo đảm bản quyền nếu dùng phần mềm thương mại."],
        ["4", "Web server/reverse proxy", "IIS/Kestrel/Nginx/Apache hoặc tương đương; hỗ trợ HTTPS, log, header bảo mật.", "Theo hạ tầng thực tế."],
        ["5", "Hệ quản trị CSDL", "SQL Server hoặc CSDL quan hệ tương đương; hỗ trợ Unicode, transaction, backup.", "Bản quyền theo chính sách sử dụng."],
        ["6", "Thiết bị mạng/firewall/WAF", "Bảo vệ truy cập, phân vùng mạng, lọc request, log truy cập.", "Khuyến nghị khi public internet."],
        ["7", "Kho lưu trữ/backup", "Lưu file upload, tài liệu, hợp đồng, backup CSDL/file.", "Cần dung lượng và chính sách lưu giữ."],
        ["8", "Chứng thư số TLS", "Bảo mật đường truyền HTTPS.", "Bắt buộc khi triển khai chính thức."],
        ["9", "Chữ ký số/HSM/USB Token", "Ký hợp đồng/tài liệu điện tử.", "Theo phạm vi triển khai ký số."],
        ["10", "Dịch vụ email", "SMTP/SMTPS gửi OTP, thông báo.", "Tài khoản do Chủ đầu tư/nhà cung cấp cấp."],
        ["11", "Dịch vụ SMS", "SMS OTP/thông báo/nhắc lịch.", "Theo yêu cầu."],
        ["12", "Dịch vụ AI", "AI Support/gợi ý thông minh/tìm kiếm ngữ nghĩa.", "Theo cấu hình và chính sách dữ liệu."],
        ["13", "Công cụ giám sát/log", "Theo dõi dịch vụ, tài nguyên, lỗi, dung lượng.", "Có thể dùng công cụ hiện có hoặc triển khai mới."],
    ])

    base.h(doc, 2, "VII. TỔ CHỨC THỰC HIỆN:")
    base.table(doc, ["Vai trò", "Trách nhiệm"], [
        ["Chủ đầu tư/đơn vị sử dụng", "Cung cấp yêu cầu nghiệp vụ, dữ liệu, hạ tầng, đầu mối phối hợp; rà soát, kiểm thử, nghiệm thu và tiếp nhận vận hành."],
        ["Đơn vị triển khai/nhà thầu", "Khảo sát, phân tích, thiết kế, phát triển/cấu hình, kiểm thử, triển khai, đào tạo, chuyển giao, bảo hành, bảo trì."],
        ["Quản trị hệ thống", "Quản lý tài khoản, phân quyền, cấu hình, backup, log, giám sát, vận hành kỹ thuật."],
        ["Cán bộ nghiệp vụ", "Quản trị nội dung, kiểm duyệt hồ sơ, điều phối giao dịch, xử lý phản hồi, theo dõi báo cáo."],
        ["Người dùng doanh nghiệp/nhà cung ứng/chuyên gia", "Cập nhật hồ sơ, đăng sản phẩm/nhu cầu, tham gia giao dịch, ký hồ sơ, theo dõi thông báo."],
        ["Đơn vị tích hợp/nhà cung cấp dịch vụ ngoài", "Cung cấp endpoint, tài khoản, tài liệu tích hợp, hỗ trợ vận hành email/SMS/ký số/AI khi có."],
    ])
    base.table(doc, ["STT", "Vai trò nhân sự triển khai", "Nhiệm vụ", "Số lượng dự kiến"], [
        ["1", "Quản trị dự án", "Lập kế hoạch, điều phối, quản lý tiến độ, rủi ro, nghiệm thu.", "1"],
        ["2", "Kiến trúc sư/Trưởng nhóm kỹ thuật", "Thiết kế kiến trúc, dữ liệu, tích hợp, bảo mật, triển khai.", "1"],
        ["3", "Phân tích nghiệp vụ", "Khảo sát, đặc tả yêu cầu, quy trình, use case, test scenario.", "1-2"],
        ["4", "Lập trình backend/frontend", "Phát triển/cấu hình chức năng, giao diện, API, báo cáo.", "3-4"],
        ["5", "Chuyên gia CSDL", "Thiết kế, tối ưu, backup/restore CSDL.", "1"],
        ["6", "Chuyên gia ATTT", "Rà soát bảo mật, phân quyền, cấu hình an toàn.", "1"],
        ["7", "QA/Tester", "Lập test case, kiểm thử, quản lý lỗi, hỗ trợ UAT.", "1-2"],
        ["8", "Triển khai/DevOps", "Cài đặt, cấu hình, deploy, giám sát, backup.", "1"],
        ["9", "Đào tạo/hỗ trợ", "Soạn tài liệu, đào tạo, tiếp nhận hỗ trợ.", "1"],
    ])

    base.h(doc, 2, "VIII. PHƯƠNG ÁN TỐ CHỨC LẮP ĐẶT, TRIỂN KHAI HỆ THỐNG:")
    base.h(doc, 3, "1. Cài đặt, kiểm tra:")
    base.bullets(doc, [
        "Chuẩn bị máy chủ, hệ điều hành, web server/reverse proxy, CSDL, tên miền, SSL/TLS, firewall và vùng lưu trữ file.",
        "Cài đặt ứng dụng, cấu hình connection string, email, SMS, AI, ký số, upload, dashboard và các tham số vận hành.",
        "Khởi tạo CSDL, chạy script/migration, tạo dữ liệu danh mục ban đầu và tài khoản quản trị.",
        "Kiểm tra truy cập HTTPS, đăng nhập, phân quyền, CMS, public portal, workflow, upload, báo cáo, thông báo và backup.",
    ])
    base.h(doc, 3, "2. Kiểm tra, nghiệm thu:")
    base.table(doc, ["Nhóm kiểm tra", "Nội dung nghiệm thu"], [
        ["Chức năng", "Kiểm tra các chức năng theo danh mục yêu cầu và quyền của từng tác nhân."],
        ["Quy trình", "Kiểm tra luồng nghiệp vụ từ đăng ký, đăng sản phẩm/nhu cầu, giao dịch, hợp đồng, nghiệm thu."],
        ["Tích hợp", "Kiểm tra email, SMS, AI, ký số, API theo phạm vi được bật/cấu hình."],
        ["Dữ liệu", "Kiểm tra danh mục, dữ liệu mẫu, báo cáo, xuất/nhập dữ liệu, backup/restore."],
        ["ATTT", "Kiểm tra đăng nhập, phân quyền, upload, CSRF/XSS/SQL Injection cơ bản, secret, log."],
        ["Triển khai", "Kiểm tra HTTPS, tên miền, log, giám sát, tài khoản, tài liệu và biên bản."],
    ])
    base.h(doc, 3, "3. Vận hành thử nghiệm và hiệu chỉnh:")
    base.bullets(doc, [
        "Vận hành thử trên môi trường UAT/production có kiểm soát trong thời gian thống nhất.",
        "Ghi nhận lỗi, góp ý, yêu cầu hiệu chỉnh giao diện, dữ liệu, báo cáo, phân quyền và quy trình.",
        "Theo dõi log, thời gian phản hồi, dung lượng file, trạng thái job nền, email/SMS/AI/ký số.",
        "Hiệu chỉnh cấu hình và tài liệu trước khi nghiệm thu chính thức.",
    ])
    base.h(doc, 3, "4. Công tác triển khai:")
    base.h(doc, 3, "4.1. Một số công tác trong quá trình triển khai:")
    base.bullets(doc, [
        "Lập kế hoạch triển khai chi tiết, phân công đầu mối, lịch làm việc và kênh trao đổi.",
        "Khảo sát dữ liệu hiện có, chuẩn hóa danh mục, nhập dữ liệu mẫu/dữ liệu ban đầu.",
        "Thiết lập tài khoản theo vai trò: quản trị, cán bộ, doanh nghiệp, nhà cung ứng, chuyên gia, lãnh đạo.",
        "Tổ chức UAT với kịch bản nghiệp vụ chính và danh sách tiêu chí nghiệm thu.",
        "Lập biên bản cài đặt, kiểm thử, đào tạo, nghiệm thu và bàn giao.",
    ])
    base.h(doc, 3, "4.2. Phương án đảm bảo an toàn hệ thống:")
    base.bullets(doc, [
        "Bật HTTPS/TLS và cấu hình bảo mật web server/reverse proxy.",
        "Áp dụng nguyên tắc phân quyền tối thiểu cho tài khoản ứng dụng, CSDL, máy chủ và file upload.",
        "Quản lý secret qua cấu hình môi trường/secret store, không lưu công khai trong mã nguồn/giao diện.",
        "Sao lưu định kỳ CSDL và file upload; kiểm tra khả năng phục hồi.",
        "Theo dõi log truy cập, log lỗi, log nghiệp vụ và cảnh báo bất thường.",
        "Rà soát an toàn thông tin trước khi nghiệm thu chính thức.",
    ])

    base.h(doc, 2, "IX. YÊU CẦU CHI TIẾT VỀ VIỆC ĐÀO TẠO, TẬP HUẤN HƯỚNG DẪN SỬ DỤNG, QUẢN TRỊ, VẬN HÀNH")
    base.h(doc, 3, "1. Mục tiêu đào tạo:")
    base.p(doc, "Đào tạo nhằm bảo đảm cán bộ quản trị, cán bộ nghiệp vụ, lãnh đạo và người dùng liên quan có thể sử dụng, quản trị, vận hành, khai thác báo cáo và xử lý các tình huống thường gặp sau khi hệ thống được bàn giao.")
    base.h(doc, 3, "2. Phạm vi đào tạo:")
    base.table(doc, ["Nhóm đào tạo", "Nội dung"], [
        ["Quản trị hệ thống", "Cài đặt, cấu hình, tài khoản, phân quyền, backup/restore, log, giám sát, tích hợp, xử lý lỗi thường gặp."],
        ["Quản trị CMS", "Menu, danh mục, bài viết, sản phẩm, nhà cung ứng, chuyên gia, phản hồi, quảng cáo, dashboard."],
        ["Cán bộ nghiệp vụ", "Quy trình nhu cầu, giao dịch, RFQ, đề xuất, đánh giá, đàm phán, pháp lý, hợp đồng, nghiệm thu, thanh lý."],
        ["Người dùng doanh nghiệp/nhà cung ứng/chuyên gia", "Đăng ký, cập nhật hồ sơ, đăng sản phẩm/nhu cầu, tham gia giao dịch, theo dõi thông báo, ký hồ sơ."],
        ["Lãnh đạo/đơn vị quản lý", "Xem dashboard, báo cáo, lọc dữ liệu, xuất báo cáo và theo dõi chỉ số điều hành."],
    ])
    base.h(doc, 3, "3. Phương án đào tạo:")
    base.bullets(doc, [
        "Đào tạo trực tiếp kết hợp tài liệu hướng dẫn và thực hành trên môi trường demo/UAT.",
        "Đào tạo theo nhóm vai trò, mỗi nhóm có kịch bản thực hành tương ứng.",
        "Cung cấp tài liệu hướng dẫn sử dụng, tài liệu quản trị và tài liệu vận hành.",
        "Ghi nhận câu hỏi, vướng mắc và cập nhật tài liệu sau đào tạo.",
    ])
    base.h(doc, 3, "4. Trình tự thực hiện:")
    base.bullets(doc, [
        "Chuẩn bị tài khoản, dữ liệu mẫu, kịch bản đào tạo và tài liệu.",
        "Đào tạo quản trị hệ thống và CMS.",
        "Đào tạo nghiệp vụ giao dịch/chuyển giao và hợp đồng/ký số.",
        "Đào tạo người dùng doanh nghiệp, nhà cung ứng, chuyên gia.",
        "Kiểm tra kết quả thực hành, lập biên bản đào tạo và danh sách học viên.",
    ])
    base.h(doc, 3, "5. Nội dung và phương án chuyển giao công nghệ:")
    base.bullets(doc, [
        "Chuyển giao mã nguồn, hướng dẫn build/deploy, cấu hình mẫu và cấu trúc thư mục.",
        "Chuyển giao CSDL, script tạo/cập nhật, dữ liệu mẫu/khởi tạo và hướng dẫn backup/restore.",
        "Chuyển giao tài liệu kiến trúc, thiết kế, API/tích hợp, vận hành, người dùng và quản trị.",
        "Chuyển giao tài khoản quản trị, quy trình vận hành, quy trình xử lý lỗi và quy trình cập nhật phiên bản.",
        "Chuyển giao danh sách cấu hình cần bảo mật; giá trị secret thực tế bàn giao qua kênh riêng theo quy định.",
    ])
    base.h(doc, 3, "6. Yêu cầu cho đào tạo:")
    base.bullets(doc, [
        "Có kế hoạch đào tạo được thống nhất với Chủ đầu tư.",
        "Có tài liệu đào tạo phù hợp phiên bản phần mềm bàn giao.",
        "Có môi trường thực hành ổn định, tài khoản và dữ liệu mẫu theo vai trò.",
        "Có danh sách học viên, biên bản đào tạo và kết quả thực hành.",
        "Có đầu mối hỗ trợ sau đào tạo trong giai đoạn vận hành ban đầu.",
    ])

    base.h(doc, 2, "X. TÀI LIỆU BÀN GIAO")
    base.table(doc, ["STT", "Tài liệu/sản phẩm", "Nội dung bàn giao"], [
        ["1", "Hệ thống phần mềm vận hành", "Phiên bản triển khai trên môi trường chính thức hoặc môi trường do Chủ đầu tư chỉ định."],
        ["2", "Mã nguồn", "Mã nguồn theo phạm vi hợp đồng, hướng dẫn build/deploy, cấu trúc thư mục."],
        ["3", "Cơ sở dữ liệu", "Script tạo/cập nhật CSDL, dữ liệu mẫu/khởi tạo, hướng dẫn backup/restore."],
        ["4", "Tài liệu thiết kế chi tiết", "Kiến trúc, mô hình dữ liệu, sơ đồ triển khai, giải pháp kỹ thuật, tích hợp, an toàn thông tin."],
        ["5", "Tài liệu yêu cầu/chức năng", "Danh sách chức năng, quy trình nghiệp vụ, tác nhân, use case, tiêu chí nghiệm thu."],
        ["6", "Tài liệu API/tích hợp", "Đặc tả API, email, SMS, AI, ký số, cấu hình endpoint, xác thực và log."],
        ["7", "Tài liệu người dùng", "Hướng dẫn khách truy cập, doanh nghiệp, nhà cung ứng, chuyên gia, cán bộ nghiệp vụ."],
        ["8", "Tài liệu quản trị CMS", "Hướng dẫn quản lý nội dung, danh mục, sản phẩm, nhà cung ứng, chuyên gia, phản hồi, dashboard."],
        ["9", "Tài liệu vận hành", "Cài đặt, cấu hình, phân quyền, backup/restore, giám sát, log, xử lý lỗi, cập nhật phiên bản."],
        ["10", "Tài liệu kiểm thử", "Test plan, test case, kết quả test, danh sách lỗi đã xử lý, biên bản UAT."],
        ["11", "Tài liệu đào tạo", "Slide/tài liệu hướng dẫn, danh sách học viên, biên bản đào tạo, câu hỏi thường gặp."],
        ["12", "Biên bản bàn giao/nghiệm thu", "Biên bản cài đặt, nghiệm thu kỹ thuật, nghiệm thu chức năng, nghiệm thu đào tạo, bàn giao tài khoản."],
        ["13", "Thông tin cấu hình bảo mật", "Danh sách cấu hình cần bảo mật, tài khoản tích hợp, secret/API key bàn giao qua kênh riêng theo quy định."],
    ])

    doc.core_properties.title = "Phần 7 - Thiết kế chi tiết - Sàn giao dịch KHCN"
    doc.core_properties.subject = "Bản v3.0 theo khung A/B/I-X"
    doc.core_properties.author = "Codex hỗ trợ tổng hợp"
    OUT.parent.mkdir(parents=True, exist_ok=True)
    doc.save(str(OUT))
    print(str(OUT).encode("ascii", errors="backslashreplace").decode("ascii"))


if __name__ == "__main__":
    build()
