from __future__ import annotations

from pathlib import Path
import importlib.util

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs" / "ho so ky thuat" / "HSYC_KT_San_GD_KHCN_CDT_v2.0_day_du.docx"

spec = importlib.util.spec_from_file_location("hsyc_base", ROOT / "tools" / "build_hsyc_full_cdt.py")
base = importlib.util.module_from_spec(spec)
assert spec.loader is not None
spec.loader.exec_module(base)


def req_rows(groups):
    rows = []
    idx = 1
    for group_code, group_name, items in groups:
        rows.append([group_code, group_name, "", "Nhóm chức năng", "Bắt buộc"])
        for name, desc, actor in items:
            rows.append([str(idx), name, desc, actor, "Bắt buộc"])
            idx += 1
    return rows


def uc_rows(groups):
    rows = []
    idx = 1
    for group_code, group_name, items in groups:
        rows.append([group_code, group_name, "", "", "Nhóm trường hợp sử dụng"])
        for name, desc, actor in items:
            rows.append([str(idx), name, actor, "Hệ thống/Quản trị viên/Cán bộ liên quan", desc])
            idx += 1
    return rows


def add_process(doc, title, steps):
    base.h(doc, 3, title)
    base.p(doc, "Mô tả:")
    for i, step in enumerate(steps, 1):
        base.p(doc, f"Bước {i}: {step}")


def build():
    doc = Document()
    base.style_doc(doc)

    base.title(doc, "HỒ SƠ YÊU CẦU KỸ THUẬT", 17)
    base.title(doc, "DỰ ÁN: SÀN GIAO DỊCH KHOA HỌC VÀ CÔNG NGHỆ", 16)
    base.p(doc, "Phiên bản: v2.0 - Bản đầy đủ theo cấu trúc tham chiếu hồ sơ kỹ thuật mẫu", italic=True, align=WD_ALIGN_PARAGRAPH.CENTER)
    base.p(doc, "Tài liệu tham chiếu cấu trúc: D:\\2026\\CanTho\\docs\\ho so ky thuat mau\\CTO_HSKT_20251124.docx và CTO-TBDU-20251124.docx", italic=True, align=WD_ALIGN_PARAGRAPH.CENTER)

    base.h(doc, 1, "I. TỔNG QUAN")
    base.h(doc, 2, "1. Giới thiệu dự án")
    base.p(doc, "Chủ đầu tư có nhu cầu xây dựng Sàn giao dịch Khoa học và Công nghệ nhằm hình thành một nền tảng số tập trung phục vụ công bố, tra cứu, tư vấn, kết nối cung - cầu, giao dịch và chuyển giao công nghệ. Hệ thống là công cụ hỗ trợ cơ quan quản lý tổ chức dữ liệu khoa học công nghệ, hỗ trợ doanh nghiệp tìm kiếm giải pháp công nghệ, hỗ trợ nhà cung ứng/chuyên gia giới thiệu năng lực, đồng thời giúp số hóa các quy trình giao dịch và theo dõi kết quả triển khai.")
    base.p(doc, "Hồ sơ yêu cầu kỹ thuật này được lập theo góc nhìn của Chủ đầu tư/đơn vị mời thầu. Nhà thầu tham gia phải đề xuất giải pháp đáp ứng tối thiểu các yêu cầu nêu trong tài liệu, bao gồm yêu cầu chức năng, yêu cầu phi chức năng, tiêu chuẩn kỹ thuật, phương án triển khai, nhân sự, dịch vụ đi kèm, bảo hành, bảo trì và tài liệu bàn giao.", bold=True)

    base.h(doc, 2, "2. Mục tiêu tổng quát")
    base.bullets(doc, [
        "Xây dựng nền tảng số tập trung phục vụ quản lý và khai thác dữ liệu về khoa học, công nghệ, thiết bị, tài sản trí tuệ, nhu cầu công nghệ, nhà cung ứng, chuyên gia, sự kiện và giao dịch chuyển giao công nghệ.",
        "Số hóa các quy trình nghiệp vụ từ đăng ký tài khoản, cập nhật hồ sơ, đăng sản phẩm/nhu cầu, tìm kiếm - kết nối, yêu cầu báo giá, đề xuất, đánh giá, đàm phán, hợp đồng, ký số, triển khai, bàn giao, nghiệm thu đến thanh lý.",
        "Cung cấp công cụ quản trị, báo cáo, thống kê, dashboard và dữ liệu phục vụ điều hành, theo dõi hiệu quả hoạt động sàn giao dịch.",
        "Tăng khả năng hỗ trợ người dùng qua tìm kiếm nâng cao, gợi ý thông minh, AI Support/chatbot và các kênh thông báo tự động.",
        "Bảo đảm hệ thống vận hành an toàn, ổn định, có khả năng mở rộng, có khả năng tích hợp với hệ thống dùng chung và phù hợp quy định về ứng dụng CNTT trong cơ quan nhà nước.",
    ])

    base.h(doc, 2, "3. Mục tiêu chi tiết")
    base.table(doc, ["STT", "Mục tiêu", "Yêu cầu kết quả"], [
        ["1", "Cổng thông tin công khai", "Người dùng tra cứu được dữ liệu công nghệ, thiết bị, tài sản trí tuệ, nhu cầu công nghệ, nhà cung ứng, chuyên gia, tin tức, sự kiện, video, diễn đàn và thông tin hỗ trợ."],
        ["2", "Quản trị dữ liệu tập trung", "Cán bộ quản trị cập nhật và kiểm duyệt được dữ liệu danh mục, nội dung, sản phẩm, nhà cung ứng, chuyên gia, phản hồi, quảng cáo, dashboard."],
        ["3", "Số hóa giao dịch", "Hồ sơ giao dịch/chuyển giao công nghệ được quản lý trên hệ thống, có bước xử lý, trạng thái, phân quyền, tài liệu, thông báo và log."],
        ["4", "Tìm kiếm và AI", "Hệ thống hỗ trợ tìm kiếm, lọc, gợi ý sản phẩm/nhà cung ứng/chuyên gia phù hợp và chatbot hỗ trợ người dùng dựa trên dữ liệu nội bộ."],
        ["5", "Tích hợp và ký số", "Có khả năng tích hợp email, SMS/OTP, chữ ký số, hợp đồng điện tử, API và nền tảng chia sẻ dữ liệu theo yêu cầu."],
        ["6", "Báo cáo điều hành", "Cung cấp dashboard và báo cáo tổng hợp về dữ liệu, người dùng, giao dịch, hợp đồng, sự kiện, phản hồi, hiệu quả kết nối cung - cầu."],
        ["7", "Vận hành bền vững", "Có tài liệu, đào tạo, chuyển giao, backup, bảo hành, bảo trì và hỗ trợ vận hành sau nghiệm thu."],
    ])

    base.h(doc, 2, "4. Phạm vi cung cấp")
    base.table(doc, ["STT", "Phạm vi", "Nội dung tối thiểu"], [
        ["1", "Phần mềm ứng dụng", "Cổng thông tin, CMS, quản lý sản phẩm/công nghệ, nhà cung ứng, chuyên gia, nhu cầu, giao dịch, hợp đồng, AI/tìm kiếm, báo cáo, thông báo."],
        ["2", "Cơ sở dữ liệu", "Thiết kế CSDL, khởi tạo dữ liệu, phân quyền, backup/restore, dữ liệu mẫu và công cụ nhập/xuất dữ liệu."],
        ["3", "Tích hợp", "Email, SMS/OTP khi cần, ký số/hợp đồng điện tử, AI provider khi được phê duyệt, API/nền tảng dùng chung."],
        ["4", "Hạ tầng triển khai", "Cấu hình máy chủ ứng dụng, máy chủ CSDL, tên miền, SSL/TLS, lưu trữ file, logging, giám sát, backup."],
        ["5", "Dịch vụ triển khai", "Khảo sát, phân tích, thiết kế, phát triển/cấu hình, kiểm thử, triển khai, nhập liệu ban đầu, đào tạo, chuyển giao."],
        ["6", "Dịch vụ sau nghiệm thu", "Bảo hành, bảo trì, hỗ trợ vận hành, xử lý lỗi, cập nhật tài liệu, tư vấn mở rộng khi Chủ đầu tư yêu cầu."],
    ])

    base.h(doc, 2, "5. Yêu cầu chung đối với nhà thầu/giải pháp")
    base.bullets(doc, [
        "Có phương án triển khai phù hợp quy mô, tiến độ, hạ tầng và yêu cầu quản lý của Chủ đầu tư.",
        "Có năng lực phân tích nghiệp vụ, phát triển phần mềm, triển khai hệ thống web, cơ sở dữ liệu, an toàn thông tin và tích hợp.",
        "Có tài liệu mô tả phương án kỹ thuật, kiến trúc, dữ liệu, quy trình, giao diện, kiểm thử, đào tạo và vận hành.",
        "Có cam kết bàn giao mã nguồn, tài liệu, CSDL, tài khoản, cấu hình và hướng dẫn vận hành theo phạm vi hợp đồng.",
        "Có phương án bảo đảm an toàn thông tin, bảo vệ dữ liệu cá nhân, quản lý secret, sao lưu và khôi phục dữ liệu.",
    ])

    base.h(doc, 1, "II. THUYẾT MINH VỀ GIẢI PHÁP KỸ THUẬT, CÔNG NGHỆ")
    base.h(doc, 2, "1. Mô hình kiến trúc phần mềm")
    base.p(doc, "Giải pháp phải được thiết kế theo mô hình ứng dụng web tập trung, phân lớp rõ ràng, có khả năng mở rộng theo module. Các lớp chức năng tối thiểu gồm: lớp giao diện người dùng, lớp điều phối ứng dụng/API, lớp nghiệp vụ, lớp truy cập dữ liệu, lớp tích hợp, lớp an toàn thông tin và lớp giám sát/vận hành.")
    base.code(doc, """
Người dùng/Doanh nghiệp/Nhà cung ứng/Chuyên gia/Cán bộ/Lãnh đạo
        |
        v
Trình duyệt Web PC - Tablet - Mobile
        |
        v
HTTPS + Firewall/WAF + Reverse Proxy/Web Server
        |
        v
Ứng dụng Sàn giao dịch KHCN
  |-- Public Portal
  |-- CMS/Quản trị
  |-- Quản lý sản phẩm, công nghệ, nhu cầu, nhà cung ứng, chuyên gia
  |-- Workflow giao dịch/chuyển giao
  |-- Hợp đồng điện tử, ký số, tài liệu
  |-- AI Support, tìm kiếm, gợi ý
  |-- Báo cáo, dashboard, thông báo
        |
        +--> CSDL tập trung
        +--> Kho file/tài liệu
        +--> Email/SMS/OTP
        +--> Chữ ký số/Hợp đồng điện tử
        +--> API/nền tảng dùng chung/hệ thống ngoài
    """)

    base.h(doc, 2, "2. Nguyên tắc lựa chọn công nghệ")
    base.table(doc, ["Tiêu chí", "Yêu cầu bắt buộc", "Yêu cầu khuyến nghị"], [
        ["Tính phổ biến", "Công nghệ được sử dụng rộng rãi, có cộng đồng/tài liệu hỗ trợ, dễ tuyển dụng nhân sự vận hành.", "Ưu tiên nền tảng web hiện đại, hỗ trợ lâu dài."],
        ["Tính mở rộng", "Dễ bổ sung phân hệ, API, báo cáo, workflow, biểu mẫu, vai trò.", "Có kiến trúc module hóa, tách lớp rõ ràng."],
        ["An toàn thông tin", "Hỗ trợ xác thực, phân quyền, mã hóa, log, kiểm soát session, bảo vệ dữ liệu.", "Tuân thủ khuyến nghị OWASP và yêu cầu ATTT theo cấp độ."],
        ["Cơ sở dữ liệu", "Hệ quản trị CSDL ổn định, hỗ trợ backup/restore, Unicode, giao dịch, phân quyền.", "Có khả năng tối ưu chỉ mục, tìm kiếm, báo cáo, phân vùng/dự phòng khi cần."],
        ["Tích hợp", "Hỗ trợ API, JSON/XML, email, SMS, ký số, AI, xuất/nhập dữ liệu.", "Có tài liệu OpenAPI/Swagger hoặc đặc tả tương đương."],
        ["Triển khai", "Triển khai được trên hạ tầng của Chủ đầu tư hoặc hạ tầng được chỉ định.", "Hỗ trợ ảo hóa/container hoặc mô hình triển khai linh hoạt."],
        ["Bảo trì", "Có tài liệu, mã nguồn, quy trình build/deploy, quản lý phiên bản.", "Có công cụ CI/CD, giám sát, kiểm thử tự động khi phù hợp."],
    ])

    base.h(doc, 2, "3. Mô hình kiến trúc triển khai hệ thống")
    base.table(doc, ["Thành phần", "Vai trò", "Yêu cầu"], [
        ["Người dùng cuối", "Truy cập hệ thống qua trình duyệt", "Không yêu cầu cài phần mềm riêng, trừ trường hợp ký số cục bộ nếu có."],
        ["Web server/reverse proxy", "Tiếp nhận HTTPS, chuyển tiếp request, cấu hình SSL/TLS", "Có bảo mật header, log truy cập, giới hạn request, chuyển hướng HTTPS."],
        ["Máy chủ ứng dụng", "Chạy phần mềm sàn giao dịch", "Đủ tài nguyên CPU/RAM/ổ đĩa; hỗ trợ cập nhật phiên bản; có giám sát dịch vụ."],
        ["Máy chủ CSDL", "Lưu dữ liệu nghiệp vụ", "Chỉ mở kết nối nội bộ; có backup; có phân quyền; có kế hoạch tối ưu."],
        ["Kho lưu trữ file", "Lưu ảnh, video, tài liệu, hợp đồng", "Kiểm soát quyền, không cho thực thi file upload, có backup, quản lý dung lượng."],
        ["Dịch vụ tích hợp", "Email, SMS, ký số, AI, API", "Có cấu hình riêng, timeout, log, fallback và kiểm soát dữ liệu."],
        ["Giám sát/vận hành", "Theo dõi hoạt động hệ thống", "Theo dõi log, trạng thái dịch vụ, dung lượng, job nền, lỗi tích hợp."],
    ])

    base.h(doc, 2, "4. Áp dụng chuẩn, tiêu chuẩn")
    standards = [
        ("1.1", "Kết nối web", "HTTP/HTTPS", "Truyền tải thông tin ứng dụng web", "Bắt buộc"),
        ("1.2", "Bảo mật truyền tải", "TLS 1.2 trở lên", "Mã hóa kết nối giữa người dùng và hệ thống", "Bắt buộc"),
        ("1.3", "API", "REST/JSON", "Trao đổi dữ liệu với hệ thống khác", "Bắt buộc khi tích hợp"),
        ("1.4", "API", "XML/CSV", "Trao đổi dữ liệu dạng bảng hoặc tích hợp hệ thống cũ", "Khuyến nghị"),
        ("1.5", "Tên miền", "DNS/AAAA", "Hỗ trợ tên miền, IPv4/IPv6", "Bắt buộc khi triển khai internet"),
        ("2.1", "Dữ liệu", "Unicode/UTF-8", "Lưu trữ và hiển thị tiếng Việt", "Bắt buộc"),
        ("2.2", "Dữ liệu", "TCVN 6909:2001", "Chuẩn tiếng Việt Unicode", "Bắt buộc"),
        ("2.3", "Tài liệu", "PDF", "Lưu trữ/xuất biểu mẫu, hợp đồng, báo cáo", "Bắt buộc"),
        ("2.4", "Tài liệu", "DOCX/XLSX", "Trao đổi tài liệu văn phòng, xuất báo cáo", "Khuyến nghị"),
        ("2.5", "Ảnh", "JPEG/PNG/WebP", "Lưu hình ảnh sản phẩm, banner, hồ sơ", "Bắt buộc"),
        ("2.6", "Video", "MP4/HLS hoặc nhúng nền tảng video", "Hiển thị video giới thiệu, sự kiện", "Khuyến nghị"),
        ("3.1", "CSDL", "SQL/Relational DBMS", "Lưu trữ dữ liệu nghiệp vụ có quan hệ", "Bắt buộc"),
        ("3.2", "CSDL", "Backup/Restore", "Sao lưu và khôi phục dữ liệu", "Bắt buộc"),
        ("3.3", "CSDL", "Transaction/ACID", "Bảo toàn dữ liệu giao dịch", "Bắt buộc"),
        ("3.4", "CSDL", "Index/Search", "Tối ưu tìm kiếm và báo cáo", "Bắt buộc"),
        ("4.1", "ATTT", "OWASP Top 10", "Phòng chống rủi ro ứng dụng web phổ biến", "Khuyến nghị áp dụng"),
        ("4.2", "ATTT", "Nghị định 85/2016/NĐ-CP", "An toàn hệ thống thông tin theo cấp độ", "Bắt buộc"),
        ("4.3", "ATTT", "Thông tư 12/2022/TT-BTTTT", "Hướng dẫn xác định cấp độ và phương án bảo đảm ATTT", "Bắt buộc"),
        ("4.4", "Dữ liệu cá nhân", "Nghị định 13/2023/NĐ-CP", "Bảo vệ dữ liệu cá nhân", "Bắt buộc"),
        ("4.5", "Dữ liệu cá nhân", "Nghị định 356/2025/NĐ-CP", "Sửa đổi, bổ sung quy định liên quan bảo vệ dữ liệu cá nhân", "Bắt buộc nếu áp dụng"),
        ("5.1", "Giao dịch điện tử", "Luật Giao dịch điện tử và văn bản hướng dẫn", "Hồ sơ điện tử, hợp đồng điện tử", "Bắt buộc khi triển khai"),
        ("5.2", "Chữ ký số", "Chuẩn chứng thư số/chữ ký số hợp lệ", "Ký hợp đồng, tài liệu điện tử", "Bắt buộc khi triển khai ký số"),
        ("6.1", "Email", "SMTP/SMTPS", "Gửi email thông báo, OTP", "Bắt buộc khi dùng email"),
        ("6.2", "SMS", "API SMS/Brandname", "Gửi SMS/OTP/nhắc việc", "Theo yêu cầu"),
        ("7.1", "Trình duyệt", "Chrome/Edge/Firefox/Safari", "Tương thích trình duyệt phổ biến", "Bắt buộc"),
        ("7.2", "Responsive", "HTML5/CSS3", "Giao diện PC, tablet, mobile", "Bắt buộc"),
        ("7.3", "Khả dụng", "WCAG tham khảo", "Tăng khả năng tiếp cận", "Khuyến nghị"),
        ("8.1", "IPv6", "IPv4/IPv6 dual-stack", "Sẵn sàng triển khai IPv6", "Bắt buộc khi hạ tầng đáp ứng"),
        ("9.1", "Quản lý chất lượng", "ISO 9001 hoặc tương đương", "Quy trình chất lượng phần mềm", "Khuyến nghị/yêu cầu theo HSMT"),
        ("9.2", "An ninh thông tin", "ISO/IEC 27001 hoặc tương đương", "Quản lý an toàn thông tin", "Khuyến nghị/yêu cầu theo HSMT"),
        ("10.1", "Nhật ký", "Audit log", "Truy vết thao tác người dùng và hệ thống", "Bắt buộc"),
        ("10.2", "Giám sát", "Monitoring/Alert", "Giám sát dịch vụ, tài nguyên, lỗi", "Bắt buộc"),
    ]
    base.table(doc, ["Số TT", "Loại tiêu chuẩn", "Ký hiệu/nhóm tiêu chuẩn", "Tên/nội dung áp dụng", "Quy định áp dụng"], [list(x) for x in standards])

    base.h(doc, 1, "III. THUYẾT MINH YÊU CẦU CHỨC NĂNG CỦA PHẦN MỀM")
    base.h(doc, 2, "1. Quy trình nghiệp vụ được tin học hóa")
    processes = [
        ("Quy trình đăng ký và kích hoạt tài khoản", [
            "Người dùng hoặc quản trị viên cung cấp thông tin tài khoản, thông tin hồ sơ và thông tin liên hệ.",
            "Hệ thống kiểm tra dữ liệu bắt buộc, định dạng email/số điện thoại, trùng tài khoản và chính sách mật khẩu.",
            "Nếu thông tin hợp lệ, hệ thống tạo tài khoản ở trạng thái phù hợp; nếu không hợp lệ, hiển thị thông báo lỗi.",
            "Người dùng xác thực email/OTP hoặc quản trị viên phê duyệt tài khoản theo chính sách của Chủ đầu tư.",
            "Hệ thống ghi nhận trạng thái kích hoạt, thời điểm kích hoạt và gửi thông báo kết quả cho người dùng/quản trị viên.",
        ]),
        ("Quy trình quản lý hồ sơ nhà cung ứng", [
            "Nhà cung ứng nhập thông tin tổ chức/cá nhân, năng lực, lĩnh vực, pháp lý, người liên hệ và tài liệu chứng minh.",
            "Hệ thống kiểm tra dữ liệu và lưu hồ sơ ở trạng thái chờ duyệt.",
            "Cán bộ nghiệp vụ xem xét hồ sơ, yêu cầu bổ sung, phê duyệt hoặc từ chối.",
            "Hệ thống thông báo kết quả, lưu lịch sử xử lý và cập nhật trạng thái hiển thị công khai nếu được duyệt.",
        ]),
        ("Quy trình quản lý hồ sơ chuyên gia", [
            "Chuyên gia hoặc cán bộ nhập thông tin cá nhân, chuyên môn, kinh nghiệm, lĩnh vực tư vấn, tài liệu chứng minh.",
            "Hệ thống kiểm tra dữ liệu, lưu hồ sơ và gửi yêu cầu kiểm duyệt.",
            "Cán bộ xem xét, phê duyệt, yêu cầu bổ sung hoặc từ chối.",
            "Hệ thống cập nhật danh sách chuyên gia công khai và hỗ trợ phân công chuyên gia vào hồ sơ giao dịch.",
        ]),
        ("Quy trình đăng và duyệt sản phẩm/công nghệ", [
            "Nhà cung ứng/cán bộ nhập thông tin sản phẩm, công nghệ, thiết bị hoặc tài sản trí tuệ.",
            "Người nhập bổ sung hình ảnh, video, tài liệu, thông số kỹ thuật, tình trạng pháp lý, lĩnh vực và từ khóa.",
            "Hệ thống kiểm tra dữ liệu, lưu nháp hoặc gửi duyệt.",
            "Cán bộ kiểm duyệt hồ sơ, yêu cầu bổ sung, duyệt công khai hoặc từ chối.",
            "Hệ thống ghi lịch sử chỉnh sửa, cập nhật trạng thái và thông báo cho người nộp hồ sơ.",
        ]),
        ("Quy trình đăng nhu cầu công nghệ", [
            "Doanh nghiệp/người dùng nhập bài toán, lĩnh vực, quy mô, địa bàn, thời hạn, tài liệu đính kèm và thông tin liên hệ.",
            "Hệ thống tiếp nhận, phân loại và chuyển cán bộ phụ trách.",
            "Hệ thống hỗ trợ gợi ý nguồn cung, chuyên gia hoặc công nghệ liên quan từ dữ liệu nội bộ.",
            "Cán bộ cập nhật kết quả tư vấn/kết nối, yêu cầu bổ sung hoặc chuyển thành hồ sơ giao dịch.",
        ]),
        ("Quy trình tìm kiếm và gợi ý thông minh", [
            "Người dùng nhập từ khóa hoặc chọn bộ lọc theo lĩnh vực, loại sản phẩm, nhà cung ứng, trạng thái, địa bàn.",
            "Hệ thống tìm kiếm dữ liệu phù hợp, xếp hạng kết quả và hiển thị thông tin tóm tắt.",
            "Nếu bật tính năng AI/gợi ý, hệ thống gợi ý thêm sản phẩm, nhà cung ứng, chuyên gia hoặc nội dung liên quan.",
            "Hệ thống ghi nhận log truy vấn phục vụ thống kê và cải thiện chất lượng tìm kiếm.",
        ]),
        ("Quy trình AI Support/chatbot", [
            "Người dùng mở chatbot, nhập câu hỏi hoặc chọn câu hỏi gợi ý.",
            "Hệ thống tìm thông tin trong dữ liệu nội bộ/knowledge base và tạo phản hồi phù hợp.",
            "Nếu dùng AI provider bên ngoài, hệ thống chỉ gửi dữ liệu được phép, có timeout và fallback.",
            "Nếu chưa đủ dữ liệu trả lời, hệ thống đề nghị người dùng để lại thông tin liên hệ.",
            "Hệ thống lưu lead/yêu cầu hỗ trợ và chuyển cán bộ xử lý.",
        ]),
        ("Quy trình khởi tạo hồ sơ giao dịch/chuyển giao", [
            "Cán bộ hoặc người dùng đủ quyền khởi tạo hồ sơ từ nhu cầu công nghệ, sản phẩm hoặc yêu cầu riêng.",
            "Hệ thống ghi nhận thông tin dự án/giao dịch, các bên tham gia, vai trò và phạm vi quyền.",
            "Hệ thống tạo các bước xử lý theo quy trình đã cấu hình và thông báo cho các bên liên quan.",
        ]),
        ("Quy trình NDA/bảo mật thông tin", [
            "Hệ thống tạo hoặc cho phép upload thỏa thuận bảo mật.",
            "Các bên liên quan xem xét, phê duyệt/ký theo vai trò.",
            "Hệ thống ghi trạng thái, thời điểm, tài liệu ký và log xử lý.",
            "Sau khi hoàn tất NDA, hệ thống mở quyền truy cập hồ sơ chi tiết theo chính sách.",
        ]),
        ("Quy trình RFQ và mời nhà cung ứng", [
            "Bên có nhu cầu/cán bộ tạo yêu cầu báo giá, mô tả yêu cầu kỹ thuật, tài chính, thời hạn và tài liệu đính kèm.",
            "Hệ thống hỗ trợ chọn/mời nhà cung ứng phù hợp và gửi thông báo.",
            "Nhà cung ứng xác nhận tham gia hoặc từ chối.",
            "Hệ thống theo dõi thời hạn, trạng thái mời và phản hồi.",
        ]),
        ("Quy trình nộp và đánh giá đề xuất", [
            "Nhà cung ứng nộp hồ sơ đề xuất kỹ thuật, tài chính, tiến độ, tài liệu năng lực.",
            "Hệ thống lưu phiên bản đề xuất, thời điểm nộp, trạng thái hợp lệ.",
            "Cán bộ/tư vấn chấm điểm theo tiêu chí được cấu hình.",
            "Hệ thống tổng hợp điểm, xếp hạng, ghi nhận kết quả và thông báo.",
        ]),
        ("Quy trình đàm phán thương mại", [
            "Các bên cập nhật nội dung đàm phán, điều khoản, giá trị, tiến độ, trách nhiệm, ghi chú.",
            "Hệ thống lưu lịch sử thay đổi và ý kiến của các bên.",
            "Khi đạt thống nhất, hồ sơ được chuyển sang rà soát pháp lý/hợp đồng.",
        ]),
        ("Quy trình rà soát pháp lý", [
            "Cán bộ/tư vấn pháp lý xem xét hồ sơ, điều khoản, tài liệu và đề xuất.",
            "Hệ thống ghi nhận kết quả rà soát, yêu cầu chỉnh sửa hoặc xác nhận đủ điều kiện.",
            "Các bên được thông báo và thực hiện chỉnh sửa nếu có.",
        ]),
        ("Quy trình hợp đồng điện tử và ký số", [
            "Hệ thống tạo dự thảo hợp đồng từ dữ liệu giao dịch hoặc cho phép upload file hợp đồng.",
            "Các bên phê duyệt hợp đồng theo vai trò và điều kiện quy trình.",
            "Hệ thống chuyển trạng thái sẵn sàng ký, gửi yêu cầu ký số và ghi nhận trạng thái ký.",
            "Sau khi ký, hệ thống lưu file cuối, hash/tài liệu đối chiếu và audit log.",
        ]),
        ("Quy trình tạm ứng, thử nghiệm, bàn giao", [
            "Cập nhật xác nhận tạm ứng/thanh toán theo hồ sơ.",
            "Ghi nhận kế hoạch và kết quả thử nghiệm/pilot.",
            "Lập biên bản bàn giao thiết bị/dịch vụ, đào tạo vận hành và tài liệu kỹ thuật.",
        ]),
        ("Quy trình nghiệm thu và thanh lý", [
            "Các bên cập nhật biên bản nghiệm thu, kết quả kiểm tra, tồn tại nếu có.",
            "Hệ thống lưu tài liệu nghiệm thu, trạng thái hoàn tất hoặc yêu cầu khắc phục.",
            "Sau khi hoàn tất, lập hồ sơ thanh lý và đóng giao dịch.",
        ]),
        ("Quy trình báo cáo thống kê", [
            "Người dùng có quyền chọn kỳ báo cáo, bộ lọc, nhóm dữ liệu và chỉ tiêu.",
            "Hệ thống tổng hợp dữ liệu, hiển thị dashboard/bảng/biểu đồ.",
            "Người dùng xuất dữ liệu ra Excel/PDF hoặc định dạng được cấu hình.",
        ]),
    ]
    for name, steps in processes:
        add_process(doc, name, steps)

    base.h(doc, 2, "2. Các tác nhân tham gia vào hệ thống")
    actors = [
        ["1", "Khách truy cập", "Đơn giản", "Tra cứu thông tin công khai, tìm kiếm, xem sản phẩm/công nghệ, gửi liên hệ, sử dụng chatbot."],
        ["2", "Người mua/doanh nghiệp có nhu cầu", "Phức tạp", "Đăng ký tài khoản, đăng nhu cầu, khởi tạo giao dịch, theo dõi hồ sơ, phê duyệt/ký tài liệu."],
        ["3", "Nhà cung ứng/chủ sở hữu công nghệ", "Phức tạp", "Đăng hồ sơ, đăng sản phẩm, nhận RFQ, nộp đề xuất, đàm phán, ký hợp đồng, bàn giao."],
        ["4", "Chuyên gia/tư vấn", "Phức tạp", "Quản lý hồ sơ chuyên gia, tham gia tư vấn, đánh giá, rà soát và hỗ trợ giao dịch."],
        ["5", "Cán bộ nghiệp vụ", "Phức tạp", "Kiểm duyệt nội dung, điều phối giao dịch, xử lý phản hồi, theo dõi báo cáo."],
        ["6", "Quản trị hệ thống", "Phức tạp", "Quản lý người dùng, phân quyền, cấu hình, danh mục, log, tích hợp, backup."],
        ["7", "Lãnh đạo/đơn vị quản lý", "Trung bình", "Theo dõi dashboard, báo cáo tổng hợp, hiệu quả kết nối cung - cầu và tình hình xử lý."],
        ["8", "Hệ thống tích hợp ngoài", "Tự động", "Email, SMS, ký số, AI provider, API/nền tảng dùng chung."],
    ]
    base.table(doc, ["TT", "Tên tác nhân", "Phân loại tác nhân", "Vai trò"], actors)

    base.h(doc, 2, "3. Danh sách các chức năng")
    function_groups = [
        ("A", "Quản trị người dùng", [
            ("Đăng nhập hệ thống", "Cho phép người dùng đăng nhập bằng tài khoản được cấp/đăng ký.", "NSD"),
            ("Đăng xuất hệ thống", "Cho phép kết thúc phiên làm việc an toàn.", "NSD"),
            ("Đổi mật khẩu", "Cho phép đổi mật khẩu theo chính sách mật khẩu.", "NSD"),
            ("Quên mật khẩu", "Khôi phục mật khẩu qua email/OTP hoặc quy trình quản trị.", "NSD"),
            ("Xem danh sách tài khoản", "Tra cứu danh sách tài khoản theo bộ lọc.", "QTHT"),
            ("Thêm tài khoản", "Tạo tài khoản cán bộ/người dùng.", "QTHT"),
            ("Cập nhật tài khoản", "Sửa thông tin, trạng thái, liên hệ, đơn vị.", "QTHT"),
            ("Khóa/mở khóa tài khoản", "Kiểm soát quyền truy cập tài khoản.", "QTHT"),
            ("Quản lý vai trò", "Tạo/sửa/xóa vai trò, nhóm quyền.", "QTHT"),
            ("Gán quyền cho người dùng", "Gán vai trò/quyền theo chức năng và hồ sơ.", "QTHT"),
            ("Tra cứu nhật ký tài khoản", "Xem lịch sử đăng nhập/thao tác quan trọng.", "QTHT"),
        ]),
        ("B", "Quản trị danh mục và cấu hình", [
            ("Quản lý danh mục lĩnh vực", "Thêm/sửa/xóa lĩnh vực khoa học công nghệ.", "Cán bộ"),
            ("Quản lý nhóm sản phẩm", "Phân loại công nghệ, thiết bị, tài sản trí tuệ.", "Cán bộ"),
            ("Quản lý trạng thái hồ sơ", "Cấu hình trạng thái xử lý/hiển thị.", "QTHT"),
            ("Quản lý tham số hệ thống", "Cấu hình email, OTP, AI, ký số, upload, dashboard.", "QTHT"),
            ("Quản lý biểu mẫu", "Cấu hình mẫu thông báo, mẫu email, mẫu tài liệu.", "QTHT"),
            ("Quản lý từ khóa", "Cấu hình từ khóa tìm kiếm, gợi ý, SEO.", "Cán bộ"),
        ]),
        ("C", "CMS và cổng thông tin", [
            ("Quản lý menu", "Tạo/sửa/xóa/sắp xếp menu.", "Cán bộ CMS"),
            ("Quản lý bài viết", "Soạn thảo, lưu nháp, duyệt, xuất bản bài viết.", "Cán bộ CMS"),
            ("Quản lý tin tức/sự kiện", "Đăng tin, sự kiện, hội thảo, lịch diễn ra.", "Cán bộ CMS"),
            ("Quản lý video", "Đăng video hoặc nhúng video truyền thông.", "Cán bộ CMS"),
            ("Quản lý banner", "Quản lý banner trang chủ/trang chuyên mục.", "Cán bộ CMS"),
            ("Quản lý quảng cáo", "Quản lý vị trí, thời gian, trạng thái quảng cáo.", "Cán bộ CMS"),
            ("Trang giới thiệu/quy định", "Cập nhật thông tin giới thiệu, quy định chung.", "Cán bộ CMS"),
            ("Quản lý liên hệ", "Cập nhật thông tin địa chỉ, hotline, email, bản đồ.", "Cán bộ CMS"),
        ]),
        ("D", "Quản lý nhà cung ứng", [
            ("Đăng ký nhà cung ứng", "Tiếp nhận hồ sơ tổ chức/cá nhân cung ứng công nghệ.", "Nhà cung ứng"),
            ("Cập nhật hồ sơ pháp lý", "Lưu giấy tờ, mã số thuế, người đại diện, lĩnh vực.", "Nhà cung ứng"),
            ("Quản lý năng lực", "Cập nhật kinh nghiệm, dự án, chứng chỉ, năng lực triển khai.", "Nhà cung ứng"),
            ("Quản lý gian hàng", "Hiển thị hồ sơ công khai và sản phẩm liên quan.", "Nhà cung ứng"),
            ("Duyệt hồ sơ nhà cung ứng", "Cán bộ phê duyệt/yêu cầu bổ sung/từ chối.", "Cán bộ"),
            ("Theo dõi lịch sử giao dịch", "Xem các RFQ, đề xuất, hợp đồng, kết quả tham gia.", "Nhà cung ứng/Cán bộ"),
        ]),
        ("E", "Quản lý chuyên gia/tư vấn", [
            ("Đăng ký chuyên gia", "Tiếp nhận hồ sơ chuyên gia tư vấn.", "Chuyên gia"),
            ("Cập nhật chuyên môn", "Lĩnh vực chuyên môn, học hàm/học vị, kinh nghiệm.", "Chuyên gia"),
            ("Duyệt hồ sơ chuyên gia", "Cán bộ xác thực/duyệt/từ chối hồ sơ.", "Cán bộ"),
            ("Phân công chuyên gia", "Gán chuyên gia vào hồ sơ giao dịch/chuyển giao.", "Cán bộ"),
            ("Theo dõi hoạt động tư vấn", "Thống kê hồ sơ tham gia, kết quả tư vấn.", "Cán bộ/Chuyên gia"),
        ]),
        ("F", "Quản lý sản phẩm, công nghệ, thiết bị, tài sản trí tuệ", [
            ("Tạo hồ sơ công nghệ", "Nhập thông tin công nghệ, ứng dụng, ưu điểm, thông số.", "Nhà cung ứng/Cán bộ"),
            ("Tạo hồ sơ thiết bị", "Nhập thông tin thiết bị, cấu hình, xuất xứ, tài liệu.", "Nhà cung ứng/Cán bộ"),
            ("Tạo hồ sơ tài sản trí tuệ", "Nhập sáng chế/giải pháp/nhãn hiệu/tài sản trí tuệ.", "Nhà cung ứng/Cán bộ"),
            ("Upload hình ảnh", "Thêm/sửa/xóa hình ảnh sản phẩm.", "Nhà cung ứng/Cán bộ"),
            ("Upload tài liệu", "Đính kèm PDF/DOCX/tài liệu kỹ thuật.", "Nhà cung ứng/Cán bộ"),
            ("Quản lý video/QR", "Gắn video giới thiệu và mã QR truy cập sản phẩm.", "Nhà cung ứng/Cán bộ"),
            ("Duyệt sản phẩm", "Kiểm duyệt và công bố/từ chối hồ sơ.", "Cán bộ"),
            ("Ẩn/hiện sản phẩm", "Quản lý trạng thái công khai.", "Cán bộ"),
            ("Lịch sử chỉnh sửa", "Theo dõi phiên bản và nội dung thay đổi.", "Cán bộ"),
        ]),
        ("G", "Quản lý nhu cầu công nghệ và người mua", [
            ("Đăng nhu cầu công nghệ", "Người mua/doanh nghiệp gửi bài toán cần giải quyết.", "Người mua"),
            ("Quản lý hồ sơ người mua", "Cập nhật thông tin doanh nghiệp/cá nhân có nhu cầu.", "Người mua"),
            ("Tiếp nhận nhu cầu", "Cán bộ phân loại và giao xử lý nhu cầu.", "Cán bộ"),
            ("Gợi ý nguồn cung", "Gợi ý công nghệ/nhà cung ứng/chuyên gia liên quan.", "Hệ thống"),
            ("Theo dõi kết quả kết nối", "Cập nhật trạng thái đã tư vấn/đã kết nối/chuyển giao.", "Cán bộ"),
            ("Quản lý quan tâm/giỏ hàng", "Lưu danh sách sản phẩm quan tâm hoặc yêu cầu báo giá.", "Người mua"),
            ("Đánh giá/bình luận", "Gửi đánh giá, bình luận, phản hồi sản phẩm/dịch vụ.", "Người mua"),
            ("Quản lý khiếu nại/hỗ trợ", "Gửi và theo dõi yêu cầu hỗ trợ/khiếu nại.", "Người mua/Cán bộ"),
        ]),
        ("H", "Sự kiện, hội thảo, triển lãm", [
            ("Tạo sự kiện", "Quản lý hội thảo, trình diễn, triển lãm công nghệ.", "Cán bộ"),
            ("Đăng ký tham dự", "Người dùng đăng ký tham dự sự kiện.", "NSD"),
            ("Quản lý gian hàng sự kiện", "Gán nhà cung ứng/sản phẩm vào gian hàng sự kiện.", "Cán bộ"),
            ("Banner quảng bá sự kiện", "Hiển thị thông tin quảng bá sự kiện.", "Cán bộ"),
            ("Nhắc lịch", "Gửi thông báo/email/SMS nhắc lịch khi được cấu hình.", "Hệ thống"),
            ("Thống kê sự kiện", "Thống kê lượt quan tâm, đăng ký, tham dự.", "Cán bộ"),
            ("Livestream/Video", "Hỗ trợ nhúng livestream/video sự kiện khi có.", "Cán bộ"),
            ("VR360/tham quan ảo", "Hỗ trợ tích hợp nội dung tham quan ảo nếu Chủ đầu tư yêu cầu.", "Cán bộ"),
        ]),
        ("I", "Giao dịch và chuyển giao công nghệ", [
            ("Khởi tạo hồ sơ giao dịch", "Tạo dự án/giao dịch từ nhu cầu hoặc sản phẩm.", "Cán bộ/Người mua"),
            ("Quản lý thành viên hồ sơ", "Gán buyer, seller, tư vấn, cán bộ, lãnh đạo.", "Cán bộ"),
            ("Theo dõi trạng thái bước", "Theo dõi tiến độ 14 bước chuyển giao.", "Các bên"),
            ("Ký NDA", "Quản lý thỏa thuận bảo mật.", "Các bên"),
            ("Tạo RFQ", "Lập yêu cầu báo giá/yêu cầu đề xuất.", "Người mua/Cán bộ"),
            ("Mời nhà cung ứng", "Gửi lời mời và theo dõi phản hồi.", "Cán bộ"),
            ("Nộp đề xuất", "Nhà cung ứng nộp hồ sơ đề xuất.", "Nhà cung ứng"),
            ("Chấm điểm đề xuất", "Cán bộ/tư vấn chấm điểm theo tiêu chí.", "Cán bộ/Tư vấn"),
            ("Lựa chọn nhà cung ứng", "Ghi nhận kết quả lựa chọn.", "Cán bộ/Lãnh đạo"),
            ("Đàm phán", "Cập nhật nội dung đàm phán.", "Các bên"),
            ("Rà soát pháp lý", "Ghi nhận ý kiến pháp lý.", "Cán bộ/Tư vấn"),
            ("Theo dõi tạm ứng", "Xác nhận tạm ứng/thanh toán liên quan.", "Các bên"),
            ("Thử nghiệm/pilot", "Cập nhật kế hoạch và kết quả thử nghiệm.", "Các bên"),
            ("Bàn giao", "Lập biên bản bàn giao thiết bị/dịch vụ.", "Các bên"),
            ("Đào tạo vận hành", "Ghi nhận đào tạo chuyển giao.", "Các bên"),
            ("Tài liệu kỹ thuật", "Bàn giao và quản lý tài liệu kỹ thuật.", "Các bên"),
            ("Nghiệm thu", "Lập biên bản nghiệm thu.", "Các bên"),
            ("Thanh lý", "Lập hồ sơ thanh lý và đóng giao dịch.", "Các bên"),
        ]),
        ("J", "Hợp đồng điện tử, ký số, thanh toán", [
            ("Tạo dự thảo hợp đồng", "Tạo hợp đồng từ dữ liệu đàm phán hoặc mẫu.", "Cán bộ"),
            ("Upload hợp đồng", "Tải lên file hợp đồng do các bên chuẩn bị.", "Cán bộ/Các bên"),
            ("Quản lý phiên bản", "Theo dõi phiên bản hợp đồng.", "Cán bộ"),
            ("Phê duyệt hợp đồng", "Các bên phê duyệt trước khi ký.", "Các bên"),
            ("Ký số hợp đồng", "Tích hợp ký số theo cấu hình.", "Các bên"),
            ("Kiểm tra hash/toàn vẹn", "Lưu thông tin đối chiếu file.", "Hệ thống"),
            ("Tải hợp đồng", "Tải file hợp đồng theo quyền.", "Các bên"),
            ("Audit hợp đồng", "Ghi log thao tác hợp đồng.", "Hệ thống"),
            ("Quản lý doanh thu/hoa hồng", "Theo dõi giá trị giao dịch, doanh thu, phí/hoa hồng nếu có.", "Cán bộ"),
            ("Quản lý thu chi", "Theo dõi thanh toán/tạm ứng/thu chi liên quan nếu có.", "Cán bộ"),
        ]),
        ("K", "Tìm kiếm, AI và tích hợp tiện ích", [
            ("Tìm kiếm toàn văn", "Tìm sản phẩm, bài viết, nhà cung ứng, chuyên gia, nhu cầu.", "NSD"),
            ("Tìm kiếm nâng cao", "Lọc theo nhiều tiêu chí.", "NSD"),
            ("AI Chatbot tư vấn", "Hỗ trợ hỏi đáp, hướng dẫn, thu thập lead.", "NSD"),
            ("AI gợi ý sản phẩm", "Gợi ý công nghệ/sản phẩm phù hợp nhu cầu.", "Hệ thống"),
            ("AI gợi ý nhà cung ứng", "Gợi ý nhà cung ứng phù hợp RFQ/nhu cầu.", "Hệ thống"),
            ("Tìm kiếm bằng giọng nói", "Hỗ trợ tích hợp nếu hạ tầng trình duyệt/thiết bị đáp ứng.", "NSD"),
            ("Chat trực tuyến", "Trao đổi/hỗ trợ người dùng.", "NSD/Cán bộ"),
            ("Video call", "Tích hợp họp/tư vấn trực tuyến nếu Chủ đầu tư yêu cầu.", "NSD/Cán bộ"),
            ("Mã QR sản phẩm", "Sinh/hiển thị QR truy cập hồ sơ sản phẩm.", "Hệ thống"),
            ("Liên kết mạng xã hội", "Chia sẻ nội dung qua mạng xã hội.", "NSD"),
            ("Song ngữ Việt - Anh", "Hỗ trợ mở rộng song ngữ theo phạm vi triển khai.", "NSD"),
            ("API tích hợp", "Kết nối hệ thống/nền tảng khác.", "Hệ thống ngoài"),
        ]),
        ("L", "Báo cáo, thống kê, dashboard", [
            ("Báo cáo nhà cung ứng", "Thống kê số lượng, trạng thái, lĩnh vực, hoạt động.", "Cán bộ/Lãnh đạo"),
            ("Báo cáo chuyên gia", "Thống kê chuyên gia, lĩnh vực, tham gia tư vấn.", "Cán bộ/Lãnh đạo"),
            ("Báo cáo sản phẩm", "Thống kê sản phẩm/công nghệ theo loại, lĩnh vực, lượt xem.", "Cán bộ/Lãnh đạo"),
            ("Báo cáo nhu cầu", "Thống kê nhu cầu theo lĩnh vực/trạng thái/kết quả.", "Cán bộ/Lãnh đạo"),
            ("Báo cáo giao dịch", "Thống kê hồ sơ theo bước, trạng thái, thời gian xử lý.", "Cán bộ/Lãnh đạo"),
            ("Báo cáo doanh thu", "Thống kê giá trị giao dịch/doanh thu/phí nếu có.", "Cán bộ/Lãnh đạo"),
            ("Báo cáo sự kiện", "Thống kê sự kiện, lượt đăng ký/tham dự.", "Cán bộ/Lãnh đạo"),
            ("Thống kê truy cập", "Thống kê lượt xem, lượt tìm kiếm, tương tác.", "Cán bộ/Lãnh đạo"),
            ("Xuất Excel/PDF", "Kết xuất báo cáo theo quyền.", "Cán bộ/Lãnh đạo"),
        ]),
        ("M", "Hạ tầng, vận hành, hỗ trợ", [
            ("Sao lưu dữ liệu", "Cấu hình backup CSDL và file.", "QTHT"),
            ("Khôi phục dữ liệu", "Restore dữ liệu khi có sự cố.", "QTHT"),
            ("Giám sát hệ thống", "Theo dõi dịch vụ, dung lượng, lỗi.", "QTHT"),
            ("Quản lý log", "Tra cứu log ứng dụng, nghiệp vụ, tích hợp.", "QTHT"),
            ("Cấu hình email", "Thiết lập tài khoản/SMTP gửi email.", "QTHT"),
            ("Cấu hình SMS", "Thiết lập SMS/OTP khi có.", "QTHT"),
            ("Cấu hình AI", "Bật/tắt AI, model, prompt, knowledge base.", "QTHT"),
            ("Cấu hình ký số", "Thiết lập nhà cung cấp ký số/hợp đồng điện tử.", "QTHT"),
            ("Quản lý hỗ trợ", "Tiếp nhận và xử lý yêu cầu hỗ trợ.", "Cán bộ"),
        ]),
    ]
    base.table(doc, ["Số TT", "Mô tả yêu cầu", "Diễn giải", "Tác nhân chính", "Phân loại"], req_rows(function_groups))

    base.h(doc, 2, "4. Bảng chuyển đổi yêu cầu chức năng sang trường hợp sử dụng")
    base.p(doc, "Bảng dưới đây chuyển đổi danh sách yêu cầu chức năng sang các trường hợp sử dụng làm cơ sở xây dựng kịch bản kiểm thử và nghiệm thu.")
    base.table(doc, ["Số TT", "Tên chức năng", "Tên tác nhân", "Tác nhân phụ", "Mô tả trường hợp sử dụng"], uc_rows(function_groups))

    base.h(doc, 1, "IV. THUYẾT MINH YÊU CẦU PHI CHỨC NĂNG")
    nonfunc_sections = [
        ("1. Yêu cầu cần đáp ứng đối với cơ sở dữ liệu", [
            "CSDL phải lưu trữ được số lượng bản ghi lớn, hỗ trợ nhiều người dùng truy cập đồng thời.",
            "CSDL phải có cơ chế phân quyền, sao lưu, phục hồi, log và bảo vệ dữ liệu quan trọng.",
            "CSDL phải hỗ trợ Unicode tiếng Việt, tìm kiếm, phân trang, sắp xếp và tối ưu truy vấn.",
            "Dữ liệu giao dịch/hợp đồng/quy trình phải bảo đảm toàn vẹn, có lịch sử và khả năng đối chiếu.",
            "Có cơ chế xuất/nhập dữ liệu theo định dạng phổ biến như Excel, CSV, PDF hoặc API.",
        ]),
        ("2. Yêu cầu về an toàn thông tin", [
            "Hệ thống phải bảo đảm xác thực người dùng, phân quyền theo vai trò, chức năng và phạm vi dữ liệu.",
            "Các thao tác quan trọng phải có log/audit: tài khoản, phân quyền, hồ sơ giao dịch, hợp đồng, ký số, tích hợp.",
            "Hệ thống phải có biện pháp chống truy cập trái phép, CSRF, XSS, SQL Injection, upload file nguy hiểm.",
            "Dữ liệu cá nhân phải được thu thập, xử lý, lưu trữ, chia sẻ theo mục đích rõ ràng và quyền truy cập phù hợp.",
            "Các thông tin bí mật như mật khẩu, API key, connection string, khóa ký số phải được quản lý an toàn.",
        ]),
        ("3. Yêu cầu về thời gian xử lý và độ phức tạp xử lý", [
            "Các trang tra cứu thông tin phổ biến cần phản hồi nhanh trong điều kiện bình thường.",
            "Các truy vấn tìm kiếm phải có chỉ mục, phân trang và giới hạn kết quả phù hợp.",
            "Báo cáo tổng hợp dữ liệu lớn cần có cache/snapshot hoặc xử lý nền để không ảnh hưởng giao dịch chính.",
            "Các tích hợp ngoài như AI, email, SMS, ký số phải có timeout và thông báo thân thiện khi lỗi.",
            "Khi tác vụ xử lý dài, hệ thống phải có trạng thái xử lý hoặc thông báo chờ để người dùng nhận biết.",
        ]),
        ("4. Ràng buộc môi trường và nền tảng", [
            "Người dùng cuối truy cập qua trình duyệt phổ biến trên PC, laptop, tablet, điện thoại.",
            "Hệ thống phải triển khai được trên máy chủ hoặc hạ tầng do Chủ đầu tư chỉ định.",
            "Hệ thống phải hỗ trợ HTTPS, tên miền, firewall/reverse proxy và cấu hình bảo mật web server.",
            "Hệ thống phải sẵn sàng triển khai IPv6/dual-stack khi hạ tầng đáp ứng.",
            "Hệ thống phải không phụ thuộc vào dịch vụ độc quyền không cần thiết làm cản trở vận hành lâu dài.",
        ]),
        ("5. Yêu cầu chịu lỗi và kiểm soát dữ liệu đầu vào", [
            "Không để lỗi cú pháp/lỗi xử lý làm treo toàn bộ hệ thống hoặc mất dữ liệu đang thao tác.",
            "Dữ liệu đầu vào phải được kiểm tra định dạng, độ dài, bắt buộc/tùy chọn và ràng buộc nghiệp vụ.",
            "Các lỗi phải hiển thị thông báo rõ ràng, không lộ thông tin kỹ thuật nhạy cảm.",
            "Hệ thống phải có rollback hoặc giữ nguyên trạng thái dữ liệu khi thao tác không thành công.",
            "Lỗi ở phân hệ phụ trợ không được làm dừng toàn bộ hệ thống.",
        ]),
        ("6. Yêu cầu về mỹ thuật, kỹ thuật giao diện", [
            "Giao diện web tiếng Việt, bố cục rõ ràng, dễ thao tác với cán bộ và doanh nghiệp.",
            "Hỗ trợ Unicode tiếng Việt, font phổ biến, không yêu cầu cài font riêng.",
            "Giao diện responsive trên desktop, tablet, mobile.",
            "Biểu mẫu phải hiển thị trường bắt buộc, định dạng dữ liệu, thông báo lỗi và trạng thái xử lý.",
            "Dashboard/báo cáo phải có bộ lọc, bảng dữ liệu, chỉ số/biểu đồ phù hợp nhu cầu quản lý.",
        ]),
        ("7. Yêu cầu năng lực nhà thầu và nhân sự", [
            "Nhân sự chủ chốt phải có kinh nghiệm phát triển/triển khai hệ thống phần mềm web, CSDL, tích hợp, bảo mật.",
            "Nhà thầu phải bố trí nhân sự BA, PM, kỹ thuật, CSDL, QA, triển khai, đào tạo và hỗ trợ.",
            "Nhà thầu phải có quy trình quản lý chất lượng, kiểm thử, nghiệm thu và bàn giao.",
            "Nhà thầu phải có phương án thay thế nhân sự khi cần mà không ảnh hưởng tiến độ.",
        ]),
        ("8. Yêu cầu tính sẵn sàng với IPv6", [
            "Phần mềm phải hoạt động sau reverse proxy/web server hỗ trợ IPv4/IPv6.",
            "Hỗ trợ tên miền, DNS và cấu hình mạng tương thích IPv6 khi hạ tầng sẵn sàng.",
            "Không hard-code địa chỉ IPv4 trong mã nguồn hoặc cấu hình nghiệp vụ.",
            "Nhà thầu phối hợp kiểm thử truy cập IPv6 khi Chủ đầu tư yêu cầu.",
        ]),
        ("9. Yêu cầu cài đặt, hạ tầng, đường truyền, an toàn vận hành", [
            "Hệ thống phải có hướng dẫn cài đặt, cấu hình, triển khai, backup/restore và giám sát.",
            "Có môi trường QA/UAT và production.",
            "Có cơ chế phân vùng quyền quản trị máy chủ, CSDL, ứng dụng và file upload.",
            "Có quy trình cập nhật phiên bản, kiểm tra sau triển khai và rollback.",
            "Có kế hoạch vận hành 24/7 hoặc theo khung giờ Chủ đầu tư yêu cầu.",
        ]),
    ]
    for title, items in nonfunc_sections:
        base.h(doc, 2, title)
        base.bullets(doc, items)

    base.h(doc, 2, "10. Phân loại lỗi nghiệm thu")
    base.table(doc, ["TT", "Mô tả"], [
        ["1", "Lỗi nghiêm trọng: hệ thống không khởi động, không đăng nhập được, mất dữ liệu, sai phân quyền nghiêm trọng hoặc quy trình chính không thể thực hiện."],
        ["2", "Lỗi cao: chức năng quan trọng hoạt động sai, gây cản trở xử lý nghiệp vụ nhưng có thể có phương án tạm thời."],
        ["3", "Lỗi trung bình: chức năng phụ hoạt động chưa đúng, giao diện chưa thuận tiện, báo cáo/biểu mẫu cần chỉnh sửa nhưng không dừng quy trình chính."],
        ["4", "Lỗi thấp: lỗi chính tả, căn chỉnh, thông báo, cải tiến nhỏ."],
        ["5", "Không cho phép nghiệm thu chính thức khi còn lỗi nghiêm trọng hoặc lỗi cao ảnh hưởng quy trình bắt buộc."],
    ])

    base.h(doc, 1, "V. YÊU CẦU TÍCH HỢP VÀ DỊCH VỤ NGOÀI")
    base.table(doc, ["STT", "Tích hợp", "Yêu cầu chi tiết", "Mức độ"], [
        ["1", "Email", "Gửi OTP, thông báo tài khoản, thông báo hồ sơ, nhắc việc, kết quả xử lý; cấu hình SMTP/SMTPS và log gửi.", "Bắt buộc"],
        ["2", "SMS/Brandname", "Gửi OTP/nhắc lịch/thông báo quan trọng khi Chủ đầu tư yêu cầu; có cơ chế bật/tắt.", "Theo yêu cầu"],
        ["3", "Chữ ký số", "Tích hợp nhà cung cấp chữ ký số/hợp đồng điện tử; quản lý trạng thái ký, file ký, chứng thư, audit log.", "Theo phạm vi triển khai"],
        ["4", "AI provider", "Tích hợp AI để chatbot/gợi ý khi được phê duyệt; kiểm soát dữ liệu gửi đi; có timeout/fallback.", "Theo cấu hình"],
        ["5", "API chia sẻ dữ liệu", "Cung cấp API có xác thực, phân quyền, log; tài liệu endpoint, request/response, mã lỗi.", "Theo yêu cầu"],
        ["6", "Nền tảng dùng chung", "Sẵn sàng tích hợp LGSP, hệ thống định danh, thanh toán, báo cáo hoặc nền tảng khác theo chỉ đạo.", "Theo yêu cầu"],
        ["7", "VR360/livestream/video call", "Hỗ trợ nhúng hoặc tích hợp dịch vụ phục vụ sự kiện/triển lãm/tư vấn trực tuyến.", "Tùy chọn"],
    ])

    base.h(doc, 1, "VI. KẾ HOẠCH TRIỂN KHAI")
    base.h(doc, 2, "1. Phương án đào tạo, lắp đặt, cài đặt và triển khai")
    base.h(doc, 3, "Triển khai lên môi trường kiểm thử (QA/UAT)")
    base.bullets(doc, [
        "Thiết lập môi trường cài đặt kiểm thử theo mô hình triển khai đã thống nhất.",
        "Cài đặt phần mềm, CSDL, dữ liệu mẫu, tài khoản mẫu, cấu hình email/AI/ký số giả lập nếu cần.",
        "Thực hiện kiểm tra chức năng, hiệu năng cơ bản, bảo mật cơ bản và tích hợp.",
        "Ghi nhận lỗi, hiệu chỉnh và chuẩn bị bản đủ điều kiện UAT.",
    ])
    base.h(doc, 3, "Triển khai lên môi trường thực tế (Production)")
    base.bullets(doc, [
        "Lập kế hoạch triển khai production, thời gian downtime nếu có, người phụ trách và phương án rollback.",
        "Rà soát hạ tầng, tên miền, SSL/TLS, CSDL, tài khoản, thư mục lưu trữ, backup, firewall.",
        "Cài đặt ứng dụng, cấu hình production, khởi tạo dữ liệu, phân quyền người dùng.",
        "Kiểm tra sau triển khai: đăng nhập, CMS, tìm kiếm, workflow, thông báo, backup, log.",
        "Lập biên bản cài đặt và bàn giao môi trường vận hành.",
    ])
    base.h(doc, 3, "Phương án đào tạo và chuyển giao công nghệ")
    base.bullets(doc, [
        "Biên soạn tài liệu hướng dẫn sử dụng, tài liệu quản trị, tài liệu vận hành và tài liệu cài đặt.",
        "Đào tạo người dùng cuối, cán bộ nghiệp vụ, quản trị CMS, quản trị hệ thống và bộ phận hỗ trợ.",
        "Đào tạo theo hình thức trực tiếp, hands-on/training on job, sử dụng dữ liệu mẫu và kịch bản nghiệp vụ.",
        "Chuyển giao mã nguồn, CSDL, tài liệu, cấu hình, tài khoản và quy trình vận hành theo hợp đồng.",
    ])
    base.table(doc, ["STT", "Hạng mục/Tiêu chuẩn", "Nội dung"], [
        ["A", "Đào tạo", "Nhà thầu lập kế hoạch đào tạo, tài liệu đào tạo và tổ chức đào tạo theo từng nhóm người dùng."],
        ["1", "Kế hoạch đào tạo", "Nêu rõ đối tượng, nội dung, thời lượng, hình thức, tài khoản demo, dữ liệu mẫu và kết quả đầu ra."],
        ["2", "Phương pháp đào tạo", "Kết hợp thuyết trình, thao tác trực tiếp, bài tập tình huống và hỏi đáp."],
        ["3", "Tài liệu đào tạo", "Tài liệu người dùng, tài liệu quản trị, tài liệu vận hành, video/hình ảnh minh họa nếu có."],
        ["4", "Đánh giá sau đào tạo", "Có danh sách học viên, biên bản đào tạo, câu hỏi thường gặp và kiến nghị cập nhật tài liệu."],
        ["B", "Chuyển giao", "Chuyển giao tri thức, mã nguồn, CSDL, cấu hình, tài khoản, tài liệu kỹ thuật và quy trình vận hành."],
        ["5", "Chuyển giao kỹ thuật", "Hướng dẫn cài đặt, build/deploy, backup/restore, log, xử lý lỗi, cập nhật phiên bản."],
        ["6", "Chuyển giao nghiệp vụ", "Hướng dẫn thực hiện các quy trình: CMS, sản phẩm, nhu cầu, giao dịch, hợp đồng, báo cáo."],
        ["C", "Hỗ trợ sau đào tạo", "Có đầu mối tiếp nhận và xử lý câu hỏi/vướng mắc trong giai đoạn đầu vận hành."],
    ])

    base.h(doc, 2, "2. Phương án bảo hành, bảo trì")
    base.bullets(doc, [
        "Nhà thầu bảo hành toàn bộ phần mềm sau nghiệm thu theo thời gian cam kết trong hợp đồng.",
        "Trong thời gian bảo hành, nhà thầu xử lý lỗi phát sinh do phần mềm, hỗ trợ cấu hình và hướng dẫn vận hành.",
        "Nhà thầu cung cấp phương án bảo trì định kỳ, tối thiểu bao gồm kiểm tra log, backup, dung lượng, hiệu năng, bảo mật và bản vá.",
        "Khi hết thời hạn bảo hành, nhà thầu tiếp tục cung cấp dịch vụ bảo trì/hỗ trợ khi Chủ đầu tư yêu cầu theo thỏa thuận riêng.",
    ])

    base.h(doc, 2, "3. Phương án kiểm tra và thử nghiệm")
    base.table(doc, ["STT", "Loại kiểm thử", "Nội dung"], [
        ["1", "Kiểm thử chức năng", "Kiểm tra từng chức năng theo danh mục yêu cầu và vai trò người dùng."],
        ["2", "Kiểm thử quy trình", "Kiểm tra luồng nghiệp vụ từ đầu đến cuối với hồ sơ mẫu."],
        ["3", "Kiểm thử tích hợp", "Kiểm tra email, SMS, ký số, AI, API hoặc tích hợp khác theo cấu hình."],
        ["4", "Kiểm thử bảo mật", "Kiểm tra xác thực, phân quyền, upload, CSRF, XSS, SQL Injection, dữ liệu cá nhân, secret."],
        ["5", "Kiểm thử hiệu năng", "Kiểm tra thời gian phản hồi trang phổ biến, tìm kiếm, báo cáo, upload/tải file."],
        ["6", "Kiểm thử hồi quy", "Kiểm tra lại các chức năng quan trọng sau khi sửa lỗi/cập nhật."],
        ["7", "Kiểm thử nghiệm thu", "Chủ đầu tư thực hiện UAT theo test case được thống nhất."],
    ])

    base.h(doc, 2, "4. Kế hoạch triển khai dự kiến")
    base.table(doc, ["STT", "Giai đoạn", "Nội dung", "Sản phẩm đầu ra", "Thời lượng dự kiến"], [
        ["1", "Khảo sát, thu thập yêu cầu", "Khảo sát nghiệp vụ, hạ tầng, dữ liệu, tích hợp, biểu mẫu, báo cáo.", "Biên bản khảo sát, danh sách yêu cầu.", "Theo kế hoạch"],
        ["2", "Phân tích và thiết kế", "Đặc tả yêu cầu, mô hình quy trình, kiến trúc, CSDL, giao diện mẫu.", "Tài liệu phân tích thiết kế.", "Theo kế hoạch"],
        ["3", "Phát triển/cấu hình", "Xây dựng/cấu hình các phân hệ, dữ liệu, phân quyền, tích hợp.", "Phiên bản QA/UAT.", "Theo kế hoạch"],
        ["4", "Chuẩn bị dữ liệu", "Chuẩn hóa, nhập liệu, chuyển đổi dữ liệu ban đầu.", "Dữ liệu khởi tạo.", "Theo kế hoạch"],
        ["5", "Kiểm thử nội bộ", "Test chức năng, quy trình, tích hợp, bảo mật, hiệu năng cơ bản.", "Biên bản test, lỗi đã xử lý.", "Theo kế hoạch"],
        ["6", "UAT", "Người dùng kiểm thử, góp ý, hiệu chỉnh.", "Biên bản UAT.", "Theo kế hoạch"],
        ["7", "Triển khai production", "Cài đặt chính thức, cấu hình, tài khoản, backup, kiểm tra.", "Biên bản cài đặt.", "Theo kế hoạch"],
        ["8", "Đào tạo và chuyển giao", "Đào tạo, bàn giao tài liệu/source/CSDL/tài khoản.", "Biên bản đào tạo, bàn giao.", "Theo kế hoạch"],
        ["9", "Vận hành hỗ trợ ban đầu", "Hỗ trợ người dùng, xử lý lỗi, tối ưu.", "Báo cáo hỗ trợ.", "Theo kế hoạch"],
    ])

    base.h(doc, 2, "5. Bố trí nhân sự")
    base.table(doc, ["STT", "Vai trò", "Viết tắt", "Công việc/Trách nhiệm", "Số lượng"], [
        ["1", "Quản trị dự án", "PM", "Lập kế hoạch, điều phối, quản lý phạm vi, tiến độ, rủi ro, nghiệm thu, bàn giao.", "1"],
        ["2", "Kiến trúc sư/Trưởng nhóm kỹ thuật", "Tech Lead", "Thiết kế kiến trúc, công nghệ, bảo mật, tích hợp, triển khai kỹ thuật.", "1"],
        ["3", "Phân tích nghiệp vụ", "BA", "Khảo sát, đặc tả yêu cầu, mô hình quy trình, use case, test scenario.", "1-2"],
        ["4", "Thiết kế UI/UX", "UI/UX", "Thiết kế giao diện, trải nghiệm người dùng, prototype, chuẩn responsive.", "1"],
        ["5", "Lập trình backend", "BE", "Phát triển nghiệp vụ, API, CSDL, phân quyền, workflow, tích hợp.", "2"],
        ["6", "Lập trình frontend", "FE", "Phát triển giao diện, form, dashboard, tương tác người dùng.", "1-2"],
        ["7", "Chuyên gia CSDL", "DBA", "Thiết kế, tối ưu, backup/restore, bảo mật CSDL.", "1"],
        ["8", "Chuyên gia AI/tìm kiếm", "AI/Search", "Thiết kế tìm kiếm, gợi ý, chatbot, knowledge base, log AI.", "1"],
        ["9", "Chuyên gia ATTT", "Security", "Rà soát bảo mật, phân quyền, cấu hình an toàn, checklist ATTT.", "1"],
        ["10", "Kiểm thử", "QA", "Lập test case, kiểm thử, quản lý lỗi, hỗ trợ UAT.", "1-2"],
        ["11", "Triển khai/DevOps", "DevOps", "Cài đặt, cấu hình, deploy, giám sát, backup, hỗ trợ vận hành.", "1"],
        ["12", "Đào tạo/hỗ trợ", "Support", "Soạn tài liệu, đào tạo, tiếp nhận hỗ trợ, tổng hợp FAQ.", "1"],
    ])

    base.h(doc, 1, "VII. YÊU CẦU NGHIỆM THU VÀ BÀN GIAO")
    base.table(doc, ["STT", "Nhóm nghiệm thu", "Tiêu chí"], [
        ["1", "Nghiệm thu chức năng", "Các chức năng bắt buộc hoạt động đúng theo danh mục yêu cầu, phân quyền và dữ liệu mẫu."],
        ["2", "Nghiệm thu quy trình", "Các quy trình nghiệp vụ số hóa thực hiện được từ đầu đến cuối, có trạng thái, thông báo, tài liệu, log."],
        ["3", "Nghiệm thu dữ liệu", "Dữ liệu khởi tạo, danh mục, sản phẩm, nhà cung ứng, chuyên gia, nhu cầu, báo cáo hiển thị đúng."],
        ["4", "Nghiệm thu tích hợp", "Các tích hợp email, SMS, AI, ký số, API hoạt động theo phạm vi được bật/cấu hình."],
        ["5", "Nghiệm thu bảo mật", "Đăng nhập, phân quyền, upload, dữ liệu cá nhân, secret, log, backup đạt checklist nghiệm thu."],
        ["6", "Nghiệm thu triển khai", "Hệ thống chạy trên môi trường production/được chỉ định, HTTPS, backup, tài khoản và giám sát đầy đủ."],
        ["7", "Nghiệm thu đào tạo", "Có tài liệu, danh sách học viên, biên bản đào tạo và kết quả thực hành."],
        ["8", "Nghiệm thu bàn giao", "Bàn giao đầy đủ mã nguồn, CSDL, tài liệu, tài khoản, cấu hình, biên bản theo hợp đồng."],
    ])
    base.table(doc, ["STT", "Sản phẩm/Tài liệu", "Nội dung bàn giao"], [
        ["1", "Phần mềm vận hành", "Phiên bản triển khai trên môi trường chính thức hoặc môi trường được chỉ định."],
        ["2", "Mã nguồn", "Mã nguồn theo phạm vi hợp đồng, hướng dẫn build/deploy, cấu trúc thư mục."],
        ["3", "CSDL", "Script tạo/cập nhật CSDL, dữ liệu mẫu/khởi tạo, hướng dẫn backup/restore."],
        ["4", "Tài liệu yêu cầu", "Đặc tả yêu cầu chức năng, phi chức năng, quy trình, use case."],
        ["5", "Tài liệu thiết kế", "Kiến trúc, mô hình dữ liệu, sơ đồ triển khai, tích hợp, bảo mật."],
        ["6", "Tài liệu người dùng", "Hướng dẫn khách truy cập, doanh nghiệp, nhà cung ứng, chuyên gia, cán bộ nghiệp vụ."],
        ["7", "Tài liệu quản trị", "Hướng dẫn CMS, tài khoản, phân quyền, cấu hình, dashboard, báo cáo."],
        ["8", "Tài liệu vận hành", "Cài đặt, cấu hình, log, backup/restore, giám sát, xử lý lỗi, cập nhật phiên bản."],
        ["9", "Tài liệu tích hợp", "Đặc tả API, email, SMS, AI, ký số/hợp đồng điện tử nếu có."],
        ["10", "Tài liệu kiểm thử", "Test plan, test case, kết quả test, danh sách lỗi, biên bản UAT."],
        ["11", "Tài liệu đào tạo", "Slide/tài liệu, video nếu có, danh sách học viên, biên bản đào tạo."],
        ["12", "Biên bản nghiệm thu", "Biên bản cài đặt, nghiệm thu kỹ thuật, chức năng, đào tạo, bàn giao."],
    ])

    base.h(doc, 1, "VIII. PHỤ LỤC - BẢNG YÊU CẦU ĐÁP ỨNG TÓM TẮT")
    response = []
    for i, (_, name, desc, actor, level) in enumerate(req_rows(function_groups)[1:], 1):
        if not name or actor == "Nhóm chức năng":
            continue
        response.append([str(i), name, level, "Hồ sơ yêu cầu kỹ thuật, mục III"])
    base.table(doc, ["STT", "Nội dung yêu cầu", "Mức độ yêu cầu", "Tài liệu tham chiếu"], response[:180])

    doc.core_properties.title = "Hồ sơ yêu cầu kỹ thuật đầy đủ - Sàn giao dịch Khoa học và Công nghệ"
    doc.core_properties.subject = "Bản v2.0 theo cấu trúc tham chiếu hồ sơ kỹ thuật mẫu"
    doc.core_properties.author = "Codex hỗ trợ tổng hợp"
    OUT.parent.mkdir(parents=True, exist_ok=True)
    doc.save(str(OUT))
    print(str(OUT).encode("ascii", errors="backslashreplace").decode("ascii"))


if __name__ == "__main__":
    build()
