from __future__ import annotations

from pathlib import Path

from docx import Document
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs" / "ho so ky thuat" / "HSYC_KT_San_GD_KHCN_CDT_v1.0.docx"


def set_font(run, size=13, bold=False, italic=False, color=None):
    run.font.name = "Times New Roman"
    run._element.rPr.rFonts.set(qn("w:eastAsia"), "Times New Roman")
    run.font.size = Pt(size)
    run.bold = bold
    run.italic = italic
    if color:
        run.font.color.rgb = RGBColor.from_string(color)


def style_doc(doc: Document):
    sec = doc.sections[0]
    sec.top_margin = Cm(2)
    sec.bottom_margin = Cm(2)
    sec.left_margin = Cm(3)
    sec.right_margin = Cm(2)
    normal = doc.styles["Normal"]
    normal.font.name = "Times New Roman"
    normal._element.rPr.rFonts.set(qn("w:eastAsia"), "Times New Roman")
    normal.font.size = Pt(13)
    for name, size, color in [
        ("Heading 1", 15, "1F4E79"),
        ("Heading 2", 14, "1F4E79"),
        ("Heading 3", 13, "2F5597"),
    ]:
        st = doc.styles[name]
        st.font.name = "Times New Roman"
        st._element.rPr.rFonts.set(qn("w:eastAsia"), "Times New Roman")
        st.font.size = Pt(size)
        st.font.bold = True
        st.font.color.rgb = RGBColor.from_string(color)


def p(doc: Document, text="", bold=False, italic=False, align=WD_ALIGN_PARAGRAPH.JUSTIFY):
    para = doc.add_paragraph()
    para.alignment = align
    para.paragraph_format.space_after = Pt(6)
    para.paragraph_format.line_spacing = 1.15
    run = para.add_run(text)
    set_font(run, bold=bold, italic=italic)
    return para


def title(doc: Document, text: str, size=16):
    para = doc.add_paragraph()
    para.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run = para.add_run(text)
    set_font(run, size, True, color="1F4E79")


def h(doc: Document, level: int, text: str):
    doc.add_heading(text, level=level)


def bullets(doc: Document, items: list[str]):
    for item in items:
        para = doc.add_paragraph(style="List Bullet")
        para.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
        para.paragraph_format.space_after = Pt(3)
        run = para.add_run(item)
        set_font(run)


def number_lines(doc: Document, items: list[str]):
    for i, item in enumerate(items, 1):
        para = doc.add_paragraph()
        para.paragraph_format.left_indent = Cm(0.7)
        para.paragraph_format.first_line_indent = Cm(-0.7)
        para.paragraph_format.space_after = Pt(3)
        para.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
        run = para.add_run(f"{i}. {item}")
        set_font(run)


def shade(cell, fill="D9EAF7"):
    tc_pr = cell._tc.get_or_add_tcPr()
    shd = OxmlElement("w:shd")
    shd.set(qn("w:fill"), fill)
    tc_pr.append(shd)


def cell_text(cell, text, bold=False, fill=None):
    cell.text = ""
    para = cell.paragraphs[0]
    para.alignment = WD_ALIGN_PARAGRAPH.LEFT
    para.paragraph_format.space_after = Pt(0)
    run = para.add_run(text)
    set_font(run, 11, bold)
    if fill:
        shade(cell, fill)


def table(doc: Document, headers: list[str], rows: list[list[str]], header_fill="D9EAF7"):
    tbl = doc.add_table(rows=1, cols=len(headers))
    tbl.style = "Table Grid"
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    for i, text in enumerate(headers):
        cell_text(tbl.rows[0].cells[i], text, True, header_fill)
        tbl.rows[0].cells[i].vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
    for row in rows:
        cells = tbl.add_row().cells
        for i, text in enumerate(row):
            cell_text(cells[i], text)
            cells[i].vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.TOP
    p(doc, "")
    return tbl


def code(doc: Document, text: str):
    para = doc.add_paragraph()
    para.alignment = WD_ALIGN_PARAGRAPH.LEFT
    para.paragraph_format.space_after = Pt(6)
    for line in text.strip("\n").splitlines():
        run = para.add_run(line + "\n")
        run.font.name = "Consolas"
        run._element.rPr.rFonts.set(qn("w:eastAsia"), "Consolas")
        run.font.size = Pt(9)


def rows_from_groups(groups: list[tuple[str, list[tuple[str, str, str]]]]) -> list[list[str]]:
    rows: list[list[str]] = []
    for group, items in groups:
        rows.append([group, group, group, group])
        for code_, name, desc in items:
            rows.append([code_, name, desc, "Bắt buộc"])
    return rows


def build():
    doc = Document()
    style_doc(doc)

    title(doc, "HỒ SƠ YÊU CẦU KỸ THUẬT", 17)
    title(doc, "DỰ ÁN: SÀN GIAO DỊCH KHOA HỌC VÀ CÔNG NGHỆ", 16)
    p(doc, "Phiên bản: v1.0 - Dự thảo phục vụ Chủ đầu tư rà soát", italic=True, align=WD_ALIGN_PARAGRAPH.CENTER)
    p(doc, "Ngày lập: 04/07/2026", italic=True, align=WD_ALIGN_PARAGRAPH.CENTER)

    h(doc, 1, "I. TỔNG QUAN")
    h(doc, 2, "1. Bối cảnh và sự cần thiết")
    p(doc, "Trong bối cảnh chuyển đổi số và thúc đẩy đổi mới sáng tạo, việc hình thành một Sàn giao dịch Khoa học và Công nghệ là cần thiết nhằm tập trung hóa dữ liệu về công nghệ, thiết bị, tài sản trí tuệ, nhu cầu công nghệ, nhà cung ứng, chuyên gia và các hoạt động chuyển giao công nghệ. Hệ thống giúp Chủ đầu tư tổ chức, quản lý và khai thác thông tin phục vụ kết nối cung - cầu, hỗ trợ doanh nghiệp tiếp cận công nghệ, đồng thời tạo công cụ quản lý, thống kê, theo dõi giao dịch và đánh giá hiệu quả vận hành.")
    p(doc, "Hồ sơ yêu cầu kỹ thuật này được xây dựng theo góc nhìn của Chủ đầu tư. Nội dung không nhằm mô tả một sản phẩm có sẵn của nhà thầu, mà đặt ra các yêu cầu tối thiểu về chức năng, mô hình hệ thống, tiêu chuẩn kỹ thuật, quy trình số hóa, phương án triển khai, năng lực nhân sự, dịch vụ đi kèm và tài liệu bàn giao mà giải pháp/nhà thầu phải đáp ứng.", bold=True)

    h(doc, 2, "2. Mục tiêu tổng quát")
    bullets(doc, [
        "Xây dựng nền tảng số tập trung phục vụ quản lý, công bố, tìm kiếm, kết nối và giao dịch/chuyển giao khoa học và công nghệ.",
        "Số hóa các quy trình nghiệp vụ từ tiếp nhận nhu cầu, kết nối cung - cầu, yêu cầu báo giá, đề xuất, đàm phán, hợp đồng, ký số, bàn giao, nghiệm thu đến thanh lý.",
        "Tăng cường khả năng khai thác dữ liệu, báo cáo thống kê, hỗ trợ ra quyết định và nâng cao hiệu quả phục vụ doanh nghiệp, tổ chức, cá nhân.",
        "Bảo đảm an toàn thông tin, khả năng mở rộng, khả năng tích hợp với các hệ thống/nền tảng dùng chung và phù hợp định hướng chuyển đổi số của cơ quan nhà nước.",
    ])

    h(doc, 2, "3. Mục tiêu chi tiết")
    table(doc, ["STT", "Mục tiêu", "Kết quả mong muốn"], [
        ["1", "Xây dựng cổng thông tin công khai", "Người dùng có thể tra cứu công nghệ, thiết bị, tài sản trí tuệ, nhu cầu công nghệ, chuyên gia, nhà cung ứng, tin tức, sự kiện và thông tin hỗ trợ."],
        ["2", "Quản trị dữ liệu tập trung", "Cán bộ quản trị cập nhật được danh mục, nội dung, sản phẩm, hồ sơ nhà cung ứng/chuyên gia, phản hồi và dữ liệu báo cáo."],
        ["3", "Số hóa quy trình giao dịch", "Các hồ sơ giao dịch/chuyển giao công nghệ được theo dõi trên hệ thống, có trạng thái, phân quyền, tài liệu và log xử lý."],
        ["4", "Hỗ trợ tìm kiếm và gợi ý thông minh", "Người dùng tìm kiếm nhanh thông tin; hệ thống có khả năng gợi ý công nghệ/nhà cung ứng/chuyên gia phù hợp theo dữ liệu nội bộ."],
        ["5", "Quản lý hợp đồng và tài liệu", "Hợp đồng, tài liệu bàn giao, nghiệm thu, thanh lý được lưu trữ, phân quyền, phê duyệt và ký số khi có yêu cầu."],
        ["6", "Báo cáo phục vụ quản lý", "Lãnh đạo/cán bộ có dashboard và báo cáo tổng hợp về dữ liệu, giao dịch, kết nối, hiệu quả vận hành."],
    ])

    h(doc, 2, "4. Phạm vi hệ thống")
    table(doc, ["Nhóm phạm vi", "Mô tả yêu cầu"], [
        ["Cổng thông tin", "Trang chủ, giới thiệu, tin tức, sự kiện, video, banner, công nghệ/thiết bị/tài sản trí tuệ, nhu cầu công nghệ, chuyên gia, nhà cung ứng, diễn đàn, liên hệ, dashboard công khai."],
        ["Quản trị/CMS", "Quản lý menu, danh mục, bài viết, sản phẩm, nhà cung ứng, chuyên gia, phản hồi, quảng cáo, tài khoản, phân quyền, tham số hệ thống và dữ liệu thống kê."],
        ["Sàn giao dịch", "Tiếp nhận nhu cầu, tìm kiếm/ghép nối, RFQ, đề xuất, đánh giá, đàm phán, pháp lý, hợp đồng, ký số, thanh toán tạm ứng, thử nghiệm, bàn giao, đào tạo, nghiệm thu, thanh lý."],
        ["AI/tìm kiếm", "Tìm kiếm từ khóa, tìm kiếm nâng cao, tìm kiếm/gợi ý thông minh, AI Support/chatbot, knowledge base và ghi nhận lịch sử truy vấn."],
        ["Tích hợp", "Email/OTP, SMS khi cần, chữ ký số/hợp đồng điện tử, API tích hợp/chia sẻ dữ liệu, nền tảng dùng chung theo yêu cầu Chủ đầu tư."],
        ["Triển khai/dịch vụ", "Khảo sát, phân tích, thiết kế, phát triển/cấu hình, kiểm thử, nhập liệu ban đầu, đào tạo, chuyển giao, bảo hành, bảo trì và hỗ trợ vận hành."],
    ])

    h(doc, 2, "5. Thuật ngữ và viết tắt")
    table(doc, ["Thuật ngữ", "Giải thích"], [
        ["CĐT", "Chủ đầu tư/đơn vị chủ trì dự án."],
        ["NSD", "Người sử dụng hệ thống."],
        ["QTHT", "Quản trị hệ thống."],
        ["CMS", "Phân hệ quản trị nội dung và dữ liệu."],
        ["RFQ", "Yêu cầu báo giá/yêu cầu đề xuất từ bên có nhu cầu."],
        ["NDA", "Thỏa thuận bảo mật thông tin trước khi chia sẻ hồ sơ chi tiết."],
        ["AI Support", "Trợ lý/chatbot hỗ trợ người dùng dựa trên dữ liệu nội bộ và/hoặc dịch vụ AI được cấu hình."],
        ["API", "Giao diện lập trình ứng dụng phục vụ tích hợp dữ liệu/chức năng."],
        ["UAT", "Kiểm thử nghiệm thu người dùng."],
    ])

    h(doc, 1, "II. CĂN CỨ, TIÊU CHUẨN VÀ NGUYÊN TẮC ÁP DỤNG")
    h(doc, 2, "1. Căn cứ pháp lý và tiêu chuẩn kỹ thuật")
    table(doc, ["STT", "Nhóm căn cứ", "Yêu cầu áp dụng"], [
        ["1", "Ứng dụng CNTT trong cơ quan nhà nước", "Tuân thủ danh mục tiêu chuẩn kỹ thuật về ứng dụng CNTT trong cơ quan nhà nước theo Thông tư 39/2017/TT-BTTTT và văn bản thay thế/bổ sung nếu có."],
        ["2", "An toàn thông tin theo cấp độ", "Tuân thủ Luật An toàn thông tin mạng, Nghị định 85/2016/NĐ-CP, Thông tư 12/2022/TT-BTTTT và hồ sơ đề xuất cấp độ do Chủ đầu tư phê duyệt."],
        ["3", "Dữ liệu cá nhân", "Tuân thủ Nghị định 13/2023/NĐ-CP, Nghị định 356/2025/NĐ-CP và quy định nội bộ về thu thập, xử lý, lưu trữ, chia sẻ dữ liệu cá nhân."],
        ["4", "Giao dịch điện tử/chữ ký số", "Tuân thủ quy định pháp luật về giao dịch điện tử, chữ ký số, chứng thư số, lưu trữ và đối chiếu hồ sơ điện tử."],
        ["5", "Kết nối/chia sẻ dữ liệu", "Bảo đảm khả năng tích hợp qua API, định dạng dữ liệu phổ biến, xác thực, phân quyền, log giao dịch và tài liệu đặc tả."],
        ["6", "IPv6", "Sẵn sàng triển khai IPv6/dual-stack theo định hướng triển khai IPv6 trong cơ quan nhà nước khi hạ tầng của Chủ đầu tư đáp ứng."],
        ["7", "Quản lý chất lượng", "Có kế hoạch kiểm thử, nghiệm thu, quản lý lỗi, quản lý phiên bản, tài liệu kỹ thuật, tài liệu người dùng và bàn giao đầy đủ."],
    ])

    h(doc, 2, "2. Nguyên tắc thiết kế")
    bullets(doc, [
        "Lấy yêu cầu nghiệp vụ và dữ liệu của Chủ đầu tư làm trung tâm; không khóa cứng hệ thống vào mô hình riêng của một nhà cung cấp.",
        "Thiết kế theo hướng module hóa, dễ mở rộng chức năng, quy trình, báo cáo, vai trò và tích hợp mới.",
        "Ưu tiên sử dụng chuẩn mở, giao thức thông dụng, định dạng dữ liệu phổ biến và tài liệu tích hợp rõ ràng.",
        "Bảo đảm an toàn thông tin ngay từ thiết kế: xác thực, phân quyền, bảo vệ dữ liệu, log, sao lưu, kiểm soát file upload và quản lý secret.",
        "Bảo đảm tính sẵn sàng, khả năng vận hành, bảo trì, giám sát và khôi phục dữ liệu.",
    ])

    h(doc, 1, "III. YÊU CẦU VỀ MÔ HÌNH VÀ KIẾN TRÚC HỆ THỐNG")
    h(doc, 2, "1. Mô hình kiến trúc tổng thể")
    p(doc, "Hệ thống yêu cầu triển khai theo mô hình ứng dụng web tập trung, có các lớp: kênh truy cập, ứng dụng nghiệp vụ, dữ liệu, tích hợp và an toàn vận hành. Giải pháp phải cho phép truy cập qua trình duyệt web trên máy tính và thiết bị di động, có khu vực công khai và khu vực nghiệp vụ/quản trị được phân quyền.")
    code(doc, """
Người dùng công khai / Doanh nghiệp / Nhà cung ứng / Chuyên gia / Cán bộ / Lãnh đạo
        |
        v
Trình duyệt Web PC, Tablet, Mobile
        |
        v
Tên miền + HTTPS + Firewall/WAF/Reverse Proxy
        |
        v
Ứng dụng Sàn giao dịch Khoa học và Công nghệ
  |-- Public Portal
  |-- CMS/Quản trị dữ liệu
  |-- Quản lý sản phẩm, công nghệ, nhu cầu, nhà cung ứng, chuyên gia
  |-- Workflow giao dịch/chuyển giao công nghệ
  |-- Hợp đồng điện tử, ký số, tài liệu
  |-- Tìm kiếm, AI Support, báo cáo, dashboard
  |-- Thông báo, trao đổi, hỗ trợ người dùng
        |
        +--> Cơ sở dữ liệu tập trung
        +--> Kho file/tài liệu/hình ảnh
        +--> Email/SMS/OTP
        +--> Chữ ký số/Hợp đồng điện tử
        +--> API/nền tảng dùng chung/hệ thống ngoài
    """)

    h(doc, 2, "2. Yêu cầu kiến trúc phần mềm")
    table(doc, ["STT", "Yêu cầu", "Mô tả"], [
        ["1", "Phân lớp rõ ràng", "Giải pháp phải tách lớp giao diện, nghiệp vụ, dữ liệu, tích hợp, cấu hình và bảo mật."],
        ["2", "Module hóa", "Các phân hệ như CMS, sản phẩm, nhà cung ứng, chuyên gia, workflow, hợp đồng, AI, báo cáo phải có khả năng phát triển/bảo trì tương đối độc lập."],
        ["3", "Cấu hình theo môi trường", "Có cấu hình riêng cho kiểm thử, vận hành chính thức, tài khoản tích hợp, email, AI, ký số, đường dẫn lưu trữ và tham số vận hành."],
        ["4", "Không hard-code secret", "Mật khẩu, khóa API, connection string, thông tin chứng thư/ký số phải được quản lý theo cơ chế bảo mật, không hiển thị trên frontend."],
        ["5", "Hỗ trợ mở rộng", "Có khả năng bổ sung biểu mẫu, trường dữ liệu, báo cáo, quy trình, vai trò, tích hợp và giao diện mà không phải xây dựng lại toàn bộ hệ thống."],
        ["6", "Khả năng kiểm thử", "Các chức năng chính phải có kịch bản kiểm thử, dữ liệu mẫu và tiêu chí nghiệm thu rõ ràng."],
    ])

    h(doc, 2, "3. Yêu cầu kiến trúc dữ liệu")
    table(doc, ["Nhóm dữ liệu", "Yêu cầu quản lý"], [
        ["Tài khoản và phân quyền", "Lưu thông tin người dùng, vai trò, quyền, trạng thái tài khoản, OTP/xác thực, lịch sử đăng nhập khi cần."],
        ["Danh mục và nội dung", "Quản lý menu, danh mục, bài viết, sự kiện, video, banner, quảng cáo, trang giới thiệu và thông tin tĩnh."],
        ["Sản phẩm/công nghệ", "Quản lý công nghệ, thiết bị, tài sản trí tuệ, hình ảnh, video, tài liệu, lĩnh vực, trạng thái pháp lý, nhà cung ứng và lịch sử chỉnh sửa."],
        ["Nhà cung ứng/chuyên gia", "Quản lý hồ sơ pháp lý/năng lực, lĩnh vực chuyên môn, sản phẩm liên quan, trạng thái kiểm duyệt và dữ liệu liên hệ."],
        ["Nhu cầu công nghệ", "Quản lý nội dung nhu cầu, lĩnh vực, địa bàn, tài liệu đính kèm, trạng thái tiếp nhận/xử lý và kết quả kết nối."],
        ["Giao dịch/chuyển giao", "Quản lý hồ sơ dự án/giao dịch, thành viên, bước xử lý, trạng thái, biểu mẫu, tài liệu và log chuyển trạng thái."],
        ["Hợp đồng/ký số", "Quản lý hợp đồng, phiên bản, phê duyệt, chữ ký, hash, file ký, tài liệu liên quan và audit log."],
        ["AI/tìm kiếm", "Quản lý chỉ mục tìm kiếm, log truy vấn, knowledge base, dữ liệu gợi ý/embedding nếu có và cấu hình bật/tắt AI."],
        ["Báo cáo/log", "Lưu log xử lý, log tích hợp, báo cáo snapshot, thống kê lượt xem/tương tác và dữ liệu hỗ trợ kiểm tra vận hành."],
    ])

    h(doc, 2, "4. Yêu cầu hạ tầng triển khai")
    table(doc, ["Hạng mục", "Yêu cầu tối thiểu/khuyến nghị"], [
        ["Máy chủ ứng dụng", "Đáp ứng khả năng chạy ứng dụng web, xử lý request đồng thời, phục vụ file tĩnh, HTTPS, logging, giám sát, cập nhật phiên bản."],
        ["Máy chủ CSDL", "Có tài nguyên phù hợp với quy mô dữ liệu; hỗ trợ backup/restore; chỉ cho phép truy cập từ ứng dụng hoặc IP được cấp quyền."],
        ["Lưu trữ file", "Có vùng lưu trữ ảnh, video, tài liệu, hợp đồng, hồ sơ xác minh; có quota, backup, kiểm soát quyền truy cập và không cho thực thi file upload."],
        ["Tên miền/SSL", "Có tên miền chính thức, chứng thư số TLS hợp lệ, chuyển hướng HTTPS và cấu hình bảo mật web server."],
        ["Đường truyền", "Bảo đảm băng thông, độ ổn định và khả năng truy cập từ internet/nội bộ theo mô hình của Chủ đầu tư."],
        ["Giám sát", "Theo dõi trạng thái ứng dụng, CSDL, dung lượng ổ đĩa, log lỗi, job nền, email/SMS, AI, ký số và cảnh báo khi có sự cố."],
        ["Dự phòng", "Có phương án sao lưu định kỳ, lưu bản backup an toàn, kiểm thử khôi phục và kế hoạch phục hồi khi sự cố."],
        ["IPv6", "Hỗ trợ triển khai dual-stack IPv4/IPv6 qua web server/reverse proxy khi hạ tầng mạng của Chủ đầu tư sẵn sàng."],
    ])

    h(doc, 1, "IV. YÊU CẦU CHỨC NĂNG")
    h(doc, 2, "1. Tác nhân tham gia hệ thống")
    table(doc, ["STT", "Tác nhân", "Vai trò"], [
        ["1", "Khách truy cập", "Tra cứu thông tin công khai, tìm kiếm, xem sản phẩm/công nghệ, gửi liên hệ, gửi nhu cầu tư vấn, sử dụng chatbot."],
        ["2", "Người mua/doanh nghiệp có nhu cầu", "Đăng ký tài khoản, cập nhật hồ sơ, đăng nhu cầu công nghệ, tạo/yêu cầu giao dịch, theo dõi quy trình, phê duyệt/ký tài liệu."],
        ["3", "Nhà cung ứng/tổ chức sở hữu công nghệ", "Đăng ký hồ sơ, đăng sản phẩm/công nghệ, nhận RFQ, nộp đề xuất, đàm phán, ký hợp đồng, bàn giao."],
        ["4", "Chuyên gia/tư vấn", "Cập nhật hồ sơ, tham gia tư vấn, hỗ trợ đánh giá, rà soát hồ sơ và theo dõi quy trình theo phân quyền."],
        ["5", "Cán bộ nghiệp vụ", "Kiểm duyệt nội dung/hồ sơ, điều phối giao dịch, xử lý phản hồi, quản lý báo cáo và hỗ trợ người dùng."],
        ["6", "Quản trị hệ thống", "Quản lý tài khoản, vai trò, phân quyền, cấu hình, danh mục, log, sao lưu, tích hợp, tham số vận hành."],
        ["7", "Lãnh đạo/đơn vị quản lý", "Theo dõi dashboard, báo cáo tổng hợp, tình hình giao dịch, hiệu quả kết nối cung - cầu và các chỉ số quản trị."],
    ])

    h(doc, 2, "2. Danh sách yêu cầu chức năng chi tiết")
    groups = [
        ("A. Quản trị người dùng và phân quyền", [
            ("A.1", "Đăng nhập/đăng xuất", "Cho phép người dùng đăng nhập/đăng xuất an toàn; thông báo lỗi rõ ràng; kiểm soát phiên đăng nhập."),
            ("A.2", "Quên mật khẩu/OTP", "Hỗ trợ khôi phục mật khẩu hoặc xác thực OTP qua email/SMS khi được cấu hình."),
            ("A.3", "Quản lý tài khoản", "Thêm, sửa, khóa/mở, tìm kiếm, phân loại tài khoản; quản lý thông tin hồ sơ người dùng."),
            ("A.4", "Quản lý vai trò", "Tạo/sửa vai trò, gán người dùng vào vai trò, phân quyền theo khu vực chức năng."),
            ("A.5", "Phân quyền theo hồ sơ/quy trình", "Kiểm soát quyền xem/sửa/phê duyệt/ký theo từng dự án, thành viên, bước xử lý."),
            ("A.6", "Nhật ký truy cập", "Ghi nhận các thao tác quan trọng như đăng nhập, cập nhật dữ liệu, phê duyệt, ký số, thay đổi quyền."),
        ]),
        ("B. Quản trị cổng thông tin/CMS", [
            ("B.1", "Quản lý menu", "Tạo/sửa/xóa/sắp xếp menu, đường dẫn, trạng thái hiển thị và cấu trúc cây menu."),
            ("B.2", "Quản lý danh mục", "Quản lý danh mục nội dung, lĩnh vực công nghệ, nhóm sản phẩm, trạng thái và thứ tự hiển thị."),
            ("B.3", "Quản lý bài viết/tin tức", "Soạn thảo, lưu nháp, duyệt, xuất bản, gắn ảnh, gắn danh mục, lịch hiển thị và trạng thái."),
            ("B.4", "Quản lý sự kiện/video", "Đăng sự kiện, hội thảo, video, tài liệu truyền thông và thông tin liên quan."),
            ("B.5", "Quản lý banner/quảng cáo", "Quản lý banner, ảnh quảng cáo, vị trí, thời gian hiển thị, trạng thái và liên kết."),
            ("B.6", "Quản lý phản hồi/liên hệ", "Tiếp nhận, phân loại, giao xử lý, cập nhật trạng thái và lưu lịch sử hỗ trợ."),
        ]),
        ("C. Quản lý sản phẩm, công nghệ, thiết bị, tài sản trí tuệ", [
            ("C.1", "Tạo hồ sơ sản phẩm/công nghệ", "Nhập thông tin tên, mô tả, lĩnh vực, thông số, ứng dụng, mức độ sẵn sàng, chủ sở hữu/nhà cung ứng."),
            ("C.2", "Quản lý hình ảnh/video/tài liệu", "Upload, xem, tải, xóa, kiểm soát định dạng và kích thước file."),
            ("C.3", "Phân loại và gắn danh mục", "Gắn sản phẩm vào nhóm công nghệ, thiết bị, tài sản trí tuệ, lĩnh vực và từ khóa."),
            ("C.4", "Quy trình duyệt", "Cán bộ kiểm tra, yêu cầu bổ sung, duyệt công khai, tạm ẩn hoặc từ chối hồ sơ."),
            ("C.5", "Quản lý trạng thái pháp lý", "Ghi nhận thông tin sở hữu trí tuệ, bằng sáng chế, tài liệu chứng minh, trạng thái công bố."),
            ("C.6", "Lịch sử chỉnh sửa", "Ghi nhận người sửa, thời điểm sửa, nội dung thay đổi chính và trạng thái trước/sau."),
        ]),
        ("D. Quản lý nhà cung ứng và chuyên gia", [
            ("D.1", "Đăng ký nhà cung ứng", "Tiếp nhận hồ sơ tổ chức/cá nhân, pháp lý, lĩnh vực, năng lực và thông tin liên hệ."),
            ("D.2", "Duyệt hồ sơ nhà cung ứng", "Cán bộ kiểm tra, yêu cầu bổ sung, phê duyệt hoặc từ chối hồ sơ."),
            ("D.3", "Quản lý gian hàng/hồ sơ công khai", "Hiển thị thông tin nhà cung ứng, sản phẩm liên quan, năng lực và thông tin liên hệ theo cấu hình."),
            ("D.4", "Đăng ký chuyên gia", "Tiếp nhận hồ sơ chuyên gia, lĩnh vực chuyên môn, kinh nghiệm, tổ chức công tác và tài liệu chứng minh."),
            ("D.5", "Phân công tư vấn", "Gắn chuyên gia/tư vấn vào hồ sơ giao dịch/chuyển giao theo vai trò và phạm vi quyền hạn."),
            ("D.6", "Theo dõi lịch sử tham gia", "Thống kê số hồ sơ, đề xuất, tư vấn, giao dịch và kết quả liên quan đến nhà cung ứng/chuyên gia."),
        ]),
        ("E. Quản lý nhu cầu công nghệ", [
            ("E.1", "Đăng nhu cầu", "Người dùng/cán bộ nhập nhu cầu, bài toán, lĩnh vực, quy mô, địa bàn, tài liệu và thông tin liên hệ."),
            ("E.2", "Tiếp nhận và phân loại", "Cán bộ phân loại lĩnh vực, mức độ ưu tiên, trạng thái xử lý và người phụ trách."),
            ("E.3", "Gợi ý nguồn cung", "Hệ thống hỗ trợ gợi ý sản phẩm/nhà cung ứng/chuyên gia phù hợp theo dữ liệu hiện có."),
            ("E.4", "Theo dõi xử lý", "Cập nhật trạng thái tiếp nhận, đang xử lý, chờ bổ sung, đã kết nối, hoàn tất hoặc hủy."),
            ("E.5", "Phản hồi kết quả", "Lưu kết quả tư vấn/kết nối, ý kiến người dùng và tài liệu liên quan."),
        ]),
        ("F. Tìm kiếm, AI Support và gợi ý thông minh", [
            ("F.1", "Tìm kiếm toàn văn", "Tìm kiếm sản phẩm, công nghệ, tin tức, nhà cung ứng, chuyên gia, nhu cầu theo từ khóa tiếng Việt."),
            ("F.2", "Bộ lọc nâng cao", "Lọc theo lĩnh vực, loại sản phẩm, địa bàn, nhà cung ứng, trạng thái, thời gian, mức độ sẵn sàng."),
            ("F.3", "Gợi ý thông minh", "Gợi ý sản phẩm/nhà cung ứng/chuyên gia phù hợp với nhu cầu hoặc nội dung tìm kiếm."),
            ("F.4", "AI Support/chatbot", "Trả lời câu hỏi dựa trên dữ liệu nội bộ; hướng dẫn người dùng; mở form liên hệ khi cần."),
            ("F.5", "Quản lý knowledge base", "Đồng bộ/cập nhật nguồn dữ liệu cho chatbot/tìm kiếm; kiểm soát dữ liệu được phép dùng."),
            ("F.6", "Log truy vấn", "Ghi nhận truy vấn, kết quả, thời gian xử lý và dữ liệu phục vụ cải thiện chất lượng tìm kiếm."),
        ]),
        ("G. Quy trình giao dịch/chuyển giao công nghệ", [
            ("G.1", "Khởi tạo hồ sơ giao dịch", "Tạo hồ sơ dự án/giao dịch từ nhu cầu, sản phẩm hoặc yêu cầu của cán bộ/người dùng."),
            ("G.2", "Quản lý thành viên", "Gán buyer, seller, tư vấn, cán bộ xử lý, người phê duyệt và phạm vi quyền."),
            ("G.3", "Theo dõi trạng thái bước", "Mỗi bước có trạng thái, người phụ trách, thời hạn, tài liệu, ghi chú và log chuyển bước."),
            ("G.4", "NDA", "Tạo/ký/quản lý thỏa thuận bảo mật; kiểm soát điều kiện truy cập hồ sơ chi tiết."),
            ("G.5", "RFQ", "Lập yêu cầu báo giá, mời nhà cung ứng, gửi thông báo và quản lý thời hạn trả lời."),
            ("G.6", "Đề xuất và chấm điểm", "Nhận đề xuất kỹ thuật/tài chính, quản lý phiên bản, chấm điểm, tổng hợp kết quả."),
            ("G.7", "Đàm phán", "Ghi nhận nội dung đàm phán, điều khoản, giá trị, tiến độ, ý kiến các bên."),
            ("G.8", "Rà soát pháp lý", "Lưu biểu mẫu rà soát, kết quả, yêu cầu chỉnh sửa và người phê duyệt."),
            ("G.9", "Hợp đồng", "Tạo/upload, phê duyệt, ký số, lưu trữ và quản lý phiên bản hợp đồng."),
            ("G.10", "Sau hợp đồng", "Theo dõi tạm ứng, thử nghiệm, bàn giao, đào tạo, tài liệu kỹ thuật, nghiệm thu, thanh lý."),
        ]),
        ("H. Hợp đồng điện tử, ký số và tài liệu", [
            ("H.1", "Tạo dự thảo hợp đồng", "Tự tạo từ dữ liệu đàm phán hoặc cho phép upload file hợp đồng."),
            ("H.2", "Quản lý phiên bản", "Mỗi lần cập nhật hợp đồng phải có phiên bản, trạng thái, người tạo và thời điểm."),
            ("H.3", "Phê duyệt nhiều bên", "Buyer, seller, tư vấn/cán bộ hoặc vai trò được chỉ định phê duyệt trước khi ký."),
            ("H.4", "Ký số", "Hỗ trợ tích hợp chữ ký số/hợp đồng điện tử theo cấu hình và quy định pháp luật."),
            ("H.5", "Kiểm tra toàn vẹn", "Lưu hash hoặc cơ chế đối chiếu file để kiểm tra thay đổi sau ký."),
            ("H.6", "Audit log", "Ghi nhận thao tác tạo, upload, phê duyệt, ký, tải xuống, hủy/thu hồi khi có."),
        ]),
        ("I. Thông báo, trao đổi và hỗ trợ", [
            ("I.1", "Thông báo web", "Thông báo trên hệ thống khi có hồ sơ mới, yêu cầu xử lý, thay đổi trạng thái, bình luận, phê duyệt."),
            ("I.2", "Email/SMS/OTP", "Gửi OTP, thông báo hoặc nhắc việc qua email/SMS khi được cấu hình."),
            ("I.3", "Trao đổi nội bộ hồ sơ", "Cho phép các bên trao đổi/bình luận trong phạm vi hồ sơ được cấp quyền."),
            ("I.4", "Quản lý yêu cầu hỗ trợ", "Tiếp nhận, phân loại, xử lý và đóng yêu cầu hỗ trợ/liên hệ từ người dùng."),
        ]),
        ("J. Báo cáo, thống kê và dashboard", [
            ("J.1", "Dashboard tổng quan", "Hiển thị số lượng sản phẩm, nhu cầu, nhà cung ứng, chuyên gia, giao dịch, hợp đồng, phản hồi."),
            ("J.2", "Báo cáo sản phẩm/công nghệ", "Thống kê theo lĩnh vực, loại, trạng thái, nhà cung ứng, lượt xem, lượt quan tâm."),
            ("J.3", "Báo cáo giao dịch", "Thống kê hồ sơ theo bước, trạng thái, thời gian xử lý, giá trị, kết quả kết nối."),
            ("J.4", "Báo cáo người dùng/nhà cung ứng", "Thống kê tài khoản, hồ sơ duyệt/chờ duyệt, hoạt động và hiệu quả tham gia."),
            ("J.5", "Xuất dữ liệu", "Xuất Excel/PDF hoặc định dạng phổ biến theo phân quyền và bộ lọc."),
        ]),
        ("K. Tích hợp và quản trị cấu hình", [
            ("K.1", "Cấu hình hệ thống", "Quản lý tham số vận hành, thông tin đơn vị, email, AI, ký số, dashboard, file upload."),
            ("K.2", "API tích hợp", "Cung cấp/tiếp nhận API có xác thực, phân quyền, log, tài liệu đặc tả khi Chủ đầu tư yêu cầu."),
            ("K.3", "Đồng bộ dữ liệu", "Có khả năng nhập/xuất/đồng bộ dữ liệu với hệ thống khác theo kế hoạch tích hợp."),
            ("K.4", "Quản lý log", "Tra cứu log lỗi, log nghiệp vụ, log tích hợp và log bảo mật trong phạm vi phân quyền."),
        ]),
    ]
    req_rows = rows_from_groups(groups)
    table(doc, ["Mã", "Tên yêu cầu", "Mô tả yêu cầu", "Mức độ"], req_rows)

    h(doc, 2, "3. Quy trình nghiệp vụ cần số hóa")
    table(doc, ["STT", "Quy trình", "Tác nhân", "Yêu cầu xử lý"], [
        ["1", "Đăng ký và xác thực tài khoản", "NSD, QTHT", "Đăng ký, xác thực, duyệt/kích hoạt, cập nhật hồ sơ, khóa/mở tài khoản."],
        ["2", "Đăng và duyệt sản phẩm/công nghệ", "Nhà cung ứng, cán bộ", "Nhập hồ sơ, upload tài liệu, gửi duyệt, yêu cầu bổ sung, duyệt công khai/từ chối."],
        ["3", "Đăng nhu cầu công nghệ", "Doanh nghiệp, cán bộ", "Tạo nhu cầu, phân loại, tiếp nhận, gợi ý nguồn cung/chuyên gia, theo dõi kết quả."],
        ["4", "Kết nối cung - cầu", "Cán bộ, doanh nghiệp, nhà cung ứng", "Khởi tạo hồ sơ giao dịch, mời bên tham gia, phân quyền, thông báo và theo dõi trạng thái."],
        ["5", "Ký NDA", "Các bên tham gia", "Tạo/duyệt/ký NDA, kiểm soát quyền truy cập dữ liệu chi tiết sau khi hoàn tất."],
        ["6", "RFQ và đề xuất", "Buyer, seller, cán bộ", "Lập RFQ, mời nhà cung ứng, nhận đề xuất, quản lý phiên bản, đóng/mở thời hạn."],
        ["7", "Đánh giá và lựa chọn", "Cán bộ, tư vấn, lãnh đạo", "Chấm điểm, tổng hợp, phê duyệt lựa chọn, lưu lý do và kết quả."],
        ["8", "Đàm phán và pháp lý", "Các bên, tư vấn", "Ghi nhận điều khoản, giá trị, tiến độ, rà soát pháp lý và yêu cầu chỉnh sửa."],
        ["9", "Hợp đồng/ký số", "Các bên", "Tạo/upload hợp đồng, phê duyệt nhiều bên, ký số, lưu hash, lưu file cuối."],
        ["10", "Triển khai sau hợp đồng", "Các bên", "Theo dõi tạm ứng, pilot, bàn giao, đào tạo, tài liệu kỹ thuật, nghiệm thu, thanh lý."],
        ["11", "Báo cáo quản trị", "Cán bộ, lãnh đạo", "Tổng hợp số liệu, lọc theo kỳ, xuất báo cáo, theo dõi hiệu quả vận hành."],
        ["12", "Hỗ trợ người dùng", "Khách truy cập, cán bộ", "Tiếp nhận chatbot/liên hệ/feedback, phân loại, xử lý và đóng yêu cầu."],
    ])

    h(doc, 2, "4. Bảng trường hợp sử dụng chính")
    use_cases = [
        ("UC-01", "Đăng nhập/đăng xuất", "NSD", "Truy cập hệ thống theo tài khoản được cấp/tự đăng ký."),
        ("UC-02", "Quản lý tài khoản", "QTHT", "Tạo, sửa, khóa, mở, phân quyền và tra cứu tài khoản."),
        ("UC-03", "Quản lý nội dung", "Cán bộ CMS", "Tạo/sửa/duyệt/xuất bản nội dung cổng thông tin."),
        ("UC-04", "Đăng sản phẩm/công nghệ", "Nhà cung ứng", "Tạo hồ sơ sản phẩm/công nghệ và gửi duyệt."),
        ("UC-05", "Duyệt sản phẩm/công nghệ", "Cán bộ", "Kiểm tra, yêu cầu bổ sung, duyệt hoặc từ chối."),
        ("UC-06", "Đăng hồ sơ nhà cung ứng", "Nhà cung ứng", "Cập nhật hồ sơ pháp lý/năng lực/lĩnh vực."),
        ("UC-07", "Đăng hồ sơ chuyên gia", "Chuyên gia", "Cập nhật hồ sơ chuyên môn và tài liệu liên quan."),
        ("UC-08", "Đăng nhu cầu công nghệ", "Doanh nghiệp", "Gửi nhu cầu, tài liệu, thông tin liên hệ và theo dõi xử lý."),
        ("UC-09", "Tìm kiếm và lọc", "Mọi tác nhân", "Tra cứu dữ liệu theo từ khóa và bộ lọc nâng cao."),
        ("UC-10", "Sử dụng AI Support", "Khách truy cập", "Hỏi đáp, nhận gợi ý, gửi lead/yêu cầu tư vấn."),
        ("UC-11", "Khởi tạo hồ sơ giao dịch", "Cán bộ/Doanh nghiệp", "Tạo hồ sơ giao dịch/chuyển giao và phân công thành viên."),
        ("UC-12", "Ký NDA", "Các bên", "Tạo/duyệt/ký NDA và mở quyền truy cập hồ sơ."),
        ("UC-13", "Tạo RFQ", "Buyer/Cán bộ", "Tạo yêu cầu báo giá và mời nhà cung ứng."),
        ("UC-14", "Nộp đề xuất", "Seller", "Nộp hồ sơ đề xuất kỹ thuật/tài chính và tài liệu liên quan."),
        ("UC-15", "Chấm điểm đề xuất", "Cán bộ/Tư vấn", "Đánh giá theo tiêu chí và tổng hợp kết quả."),
        ("UC-16", "Đàm phán", "Các bên", "Ghi nhận nội dung đàm phán, điều khoản và kết quả."),
        ("UC-17", "Rà soát pháp lý", "Cán bộ/Tư vấn", "Cập nhật kết quả pháp lý và yêu cầu chỉnh sửa."),
        ("UC-18", "Quản lý hợp đồng", "Các bên", "Tạo/upload/phê duyệt/quản lý phiên bản hợp đồng."),
        ("UC-19", "Ký số hợp đồng", "Các bên", "Ký số, lưu file, hash và audit log."),
        ("UC-20", "Theo dõi bàn giao/nghiệm thu", "Các bên", "Cập nhật biên bản, tài liệu, nghiệm thu và thanh lý."),
        ("UC-21", "Xem báo cáo", "Cán bộ/Lãnh đạo", "Xem dashboard, lọc dữ liệu và xuất báo cáo."),
        ("UC-22", "Quản lý cấu hình", "QTHT", "Cập nhật tham số, tích hợp, email, AI, ký số và hệ thống."),
    ]
    table(doc, ["Mã UC", "Tên trường hợp sử dụng", "Tác nhân chính", "Mô tả"], [list(x) for x in use_cases])

    h(doc, 1, "V. YÊU CẦU PHI CHỨC NĂNG")
    h(doc, 2, "1. Yêu cầu cơ sở dữ liệu")
    bullets(doc, [
        "Dữ liệu phải được lưu trữ tập trung, có cơ chế ràng buộc/toàn vẹn dữ liệu và tránh trùng lặp không kiểm soát.",
        "Hỗ trợ Unicode tiếng Việt, tìm kiếm tiếng Việt, phân trang, sắp xếp, lọc và tối ưu truy vấn với dữ liệu lớn.",
        "Có cơ chế backup/restore; bản backup phải bao gồm CSDL và file upload/tài liệu liên quan.",
        "Có phân quyền truy cập CSDL, không dùng tài khoản quyền cao cho các kết nối không cần thiết.",
        "Có khả năng xuất/nhập dữ liệu theo định dạng phổ biến phục vụ báo cáo và chuyển đổi dữ liệu khi cần.",
    ])

    h(doc, 2, "2. Yêu cầu an toàn thông tin")
    table(doc, ["Nhóm kiểm soát", "Yêu cầu"], [
        ["Xác thực", "Đăng nhập an toàn; kiểm soát phiên; OTP/xác thực bổ sung cho tác vụ nhạy cảm khi cần."],
        ["Phân quyền", "Phân quyền theo vai trò, chức năng, hồ sơ, bước quy trình; người dùng chỉ thấy dữ liệu được phép."],
        ["Bảo vệ request", "Các thao tác thay đổi dữ liệu phải có kiểm soát request, chống giả mạo, validate dữ liệu và xử lý lỗi an toàn."],
        ["Bảo vệ dữ liệu", "Mật khẩu, secret, token, API key phải được bảo vệ; dữ liệu cá nhân phải có phạm vi truy cập và mục đích xử lý rõ ràng."],
        ["Upload file", "Kiểm tra định dạng, dung lượng, tên file; không cho thực thi file upload; phân quyền tải/xem/xóa."],
        ["Log và audit", "Ghi nhận thao tác quan trọng, đặc biệt với tài khoản, phân quyền, hồ sơ giao dịch, hợp đồng, ký số, tích hợp."],
        ["Kiểm thử bảo mật", "Rà soát các rủi ro XSS, SQL Injection, CSRF, truy cập trái phép, rò rỉ thông tin lỗi, cấu hình sai."],
        ["Cấp độ ATTT", "Phối hợp Chủ đầu tư lập/hoàn thiện hồ sơ đề xuất cấp độ và đáp ứng các yêu cầu kỹ thuật theo cấp độ được phê duyệt."],
    ])

    h(doc, 2, "3. Yêu cầu hiệu năng")
    table(doc, ["Chỉ tiêu", "Yêu cầu"], [
        ["Truy cập thông tin", "Các trang phổ biến phản hồi trong thời gian phù hợp với trải nghiệm người dùng; ưu tiên dưới 3 giây ở điều kiện bình thường."],
        ["Tìm kiếm", "Có chỉ mục/cơ chế tối ưu để tìm kiếm dữ liệu lớn; hỗ trợ phân trang và giới hạn kết quả."],
        ["Báo cáo", "Báo cáo tổng hợp lớn cần có cơ chế cache/snapshot/xử lý nền để không ảnh hưởng giao dịch chính."],
        ["Upload/tải file", "Có giới hạn dung lượng, thông báo lỗi rõ ràng và không làm treo phiên người dùng khi file không hợp lệ."],
        ["AI/tích hợp ngoài", "Có timeout, retry hợp lý, fallback và thông báo thân thiện khi dịch vụ ngoài không phản hồi."],
    ])

    h(doc, 2, "4. Yêu cầu chịu lỗi và tính sẵn sàng")
    bullets(doc, [
        "Lỗi tại một phân hệ phụ trợ như email, SMS, AI, ký số không được làm dừng toàn bộ hệ thống.",
        "Có cơ chế ghi log lỗi đủ thông tin để điều tra nhưng không lộ secret/dữ liệu nhạy cảm cho người dùng cuối.",
        "Có quy trình phục hồi dịch vụ, khôi phục dữ liệu và rollback khi cập nhật phiên bản phát sinh lỗi.",
        "Có phân loại mức độ lỗi và cam kết thời gian phản hồi/khắc phục trong giai đoạn bảo hành, bảo trì.",
    ])

    h(doc, 2, "5. Yêu cầu giao diện")
    bullets(doc, [
        "Giao diện tiếng Việt, rõ ràng, dễ sử dụng, phù hợp cán bộ quản trị và người dùng doanh nghiệp.",
        "Tương thích các trình duyệt phổ biến, hiển thị tốt trên desktop, tablet, mobile.",
        "Biểu mẫu phải có kiểm tra dữ liệu đầu vào, thông báo lỗi rõ ràng, trạng thái lưu/gửi/duyệt minh bạch.",
        "Dashboard/báo cáo phải có bộ lọc, chỉ số, bảng dữ liệu/biểu đồ phù hợp nhu cầu quản lý.",
        "Các tác vụ quan trọng cần có xác nhận thao tác, cảnh báo mất dữ liệu và thông báo kết quả xử lý.",
    ])

    h(doc, 2, "6. Yêu cầu vận hành, bảo trì, mở rộng")
    table(doc, ["Nội dung", "Yêu cầu"], [
        ["Tài liệu vận hành", "Có hướng dẫn cài đặt, cấu hình, backup/restore, giám sát, xử lý lỗi, cập nhật phiên bản."],
        ["Bảo trì", "Có quy trình tiếp nhận lỗi, phân loại, sửa lỗi, kiểm thử và triển khai bản vá."],
        ["Mở rộng", "Cho phép bổ sung danh mục, biểu mẫu, quy trình, vai trò, báo cáo và tích hợp mới."],
        ["Giám sát", "Theo dõi trạng thái ứng dụng, CSDL, job nền, tích hợp, dung lượng file, log lỗi và cảnh báo sự cố."],
        ["Chuyển giao", "Bàn giao mã nguồn, CSDL, tài liệu, tài khoản quản trị, cấu hình mẫu và hướng dẫn vận hành theo hợp đồng."],
    ])

    h(doc, 1, "VI. YÊU CẦU VỀ TÍCH HỢP")
    table(doc, ["STT", "Nhóm tích hợp", "Yêu cầu"], [
        ["1", "Email", "Gửi OTP, thông báo, nhắc việc, xác nhận giao dịch và thông tin hỗ trợ; có cấu hình máy chủ email/tài khoản gửi."],
        ["2", "SMS/OTP", "Hỗ trợ tích hợp SMS brandname/OTP khi Chủ đầu tư yêu cầu; có cơ chế bật/tắt và log gửi."],
        ["3", "Chữ ký số/hợp đồng điện tử", "Hỗ trợ tích hợp nhà cung cấp chữ ký số/hợp đồng điện tử; quản lý trạng thái ký, file ký, audit log."],
        ["4", "AI provider", "Nếu sử dụng dịch vụ AI ngoài, phải có cấu hình bật/tắt, kiểm soát dữ liệu gửi đi, timeout, fallback và quản lý khóa API an toàn."],
        ["5", "Nền tảng dùng chung/API", "Có khả năng tích hợp hệ thống khác qua API; tài liệu endpoint, dữ liệu, xác thực, phân quyền và log giao dịch."],
        ["6", "Xuất/nhập dữ liệu", "Hỗ trợ import/export Excel/PDF/CSV hoặc định dạng theo yêu cầu quản lý."],
    ])

    h(doc, 1, "VII. YÊU CẦU NHÂN SỰ THỰC HIỆN")
    table(doc, ["STT", "Vai trò", "Năng lực tối thiểu", "Số lượng"], [
        ["1", "Quản trị dự án/PM", "Kinh nghiệm quản lý dự án phần mềm, lập kế hoạch, điều phối, quản lý rủi ro, nghiệm thu và bàn giao.", "1"],
        ["2", "Kiến trúc sư/Trưởng nhóm kỹ thuật", "Kinh nghiệm thiết kế kiến trúc web, CSDL, tích hợp, bảo mật, triển khai hệ thống.", "1"],
        ["3", "Phân tích nghiệp vụ/BA", "Kinh nghiệm khảo sát, mô hình hóa quy trình, đặc tả yêu cầu, use case, tài liệu nghiệm thu.", "1-2"],
        ["4", "Lập trình viên backend", "Kinh nghiệm phát triển nghiệp vụ, API, cơ sở dữ liệu, phân quyền, workflow, tích hợp.", "2"],
        ["5", "Lập trình viên frontend/UI", "Kinh nghiệm xây dựng giao diện web responsive, biểu mẫu nghiệp vụ, dashboard, tối ưu trải nghiệm.", "1-2"],
        ["6", "Chuyên gia CSDL", "Kinh nghiệm thiết kế, tối ưu, backup/restore, vận hành và bảo mật CSDL.", "1"],
        ["7", "Chuyên gia an toàn thông tin", "Kinh nghiệm rà soát bảo mật ứng dụng web, phân quyền, cấu hình máy chủ, hồ sơ ATTT.", "1"],
        ["8", "QA/Tester", "Kinh nghiệm lập test plan, test case, kiểm thử chức năng, hồi quy, tích hợp, UAT.", "1-2"],
        ["9", "Triển khai/DevOps", "Kinh nghiệm cài đặt, cấu hình server, deploy, giám sát, backup và hỗ trợ vận hành.", "1"],
        ["10", "Đào tạo/hỗ trợ người dùng", "Kinh nghiệm biên soạn tài liệu, đào tạo, tiếp nhận hỗ trợ và tổng hợp phản hồi.", "1"],
    ])
    p(doc, "Nhà thầu phải cung cấp danh sách nhân sự dự kiến, vai trò, kinh nghiệm, thời gian huy động và phương án thay thế khi nhân sự thay đổi. Việc thay thế nhân sự chủ chốt phải bảo đảm năng lực tương đương và không ảnh hưởng tiến độ.", bold=True)

    h(doc, 1, "VIII. KẾ HOẠCH VÀ PHƯƠNG ÁN TRIỂN KHAI")
    h(doc, 2, "1. Giai đoạn triển khai")
    table(doc, ["Giai đoạn", "Nội dung công việc", "Sản phẩm đầu ra"], [
        ["Khởi động", "Thống nhất phạm vi, kế hoạch, đầu mối, phương thức phối hợp, môi trường làm việc.", "Kế hoạch triển khai, biên bản khởi động."],
        ["Khảo sát", "Khảo sát nghiệp vụ, dữ liệu, hạ tầng, tích hợp, biểu mẫu, báo cáo, nhân sự vận hành.", "Biên bản khảo sát, danh sách yêu cầu."],
        ["Phân tích thiết kế", "Đặc tả yêu cầu, mô hình quy trình, mô hình dữ liệu, kiến trúc, giao diện mẫu.", "Tài liệu SRS/thiết kế, prototype nếu có."],
        ["Phát triển/cấu hình", "Xây dựng/cấu hình phân hệ, phân quyền, workflow, báo cáo, tích hợp, dữ liệu mẫu.", "Phiên bản trên môi trường kiểm thử."],
        ["Kiểm thử nội bộ", "Kiểm thử chức năng, tích hợp, bảo mật cơ bản, dữ liệu, hiệu năng cơ bản.", "Biên bản kiểm thử, danh sách lỗi đã xử lý."],
        ["UAT", "Người dùng kiểm thử nghiệm thu, góp ý, hiệu chỉnh.", "Biên bản UAT, bản đủ điều kiện triển khai."],
        ["Triển khai production", "Cài đặt, cấu hình tên miền/SSL, CSDL, tài khoản, dữ liệu ban đầu, backup, giám sát.", "Biên bản cài đặt, hệ thống vận hành."],
        ["Đào tạo/chuyển giao", "Đào tạo quản trị, nghiệp vụ, người dùng; bàn giao tài liệu, source/CSDL theo hợp đồng.", "Biên bản đào tạo, biên bản bàn giao."],
        ["Bảo hành/bảo trì", "Hỗ trợ vận hành, sửa lỗi, tối ưu, cập nhật tài liệu.", "Báo cáo hỗ trợ, biên bản xử lý lỗi."],
    ])

    h(doc, 2, "2. Yêu cầu môi trường")
    bullets(doc, [
        "Có môi trường kiểm thử/UAT và môi trường vận hành chính thức.",
        "Dữ liệu kiểm thử phải được kiểm soát, không làm lộ dữ liệu cá nhân/bí mật nếu chưa có biện pháp bảo vệ.",
        "Môi trường production phải cấu hình HTTPS, backup, logging, tài khoản quản trị và quyền truy cập theo nguyên tắc tối thiểu.",
        "Có quy trình triển khai phiên bản, kiểm tra sau triển khai và rollback khi phát sinh lỗi nghiêm trọng.",
    ])

    h(doc, 2, "3. Đào tạo và chuyển giao")
    table(doc, ["Nhóm đào tạo", "Nội dung", "Kết quả"], [
        ["Quản trị hệ thống", "Cài đặt, cấu hình, tài khoản, phân quyền, backup/restore, log, tích hợp, xử lý lỗi thường gặp.", "Quản trị có thể vận hành kỹ thuật cơ bản."],
        ["Cán bộ CMS", "Quản lý menu, danh mục, bài viết, sản phẩm, nhà cung ứng, chuyên gia, phản hồi, quảng cáo, dashboard.", "Cán bộ tự cập nhật dữ liệu/nội dung."],
        ["Cán bộ nghiệp vụ", "Xử lý hồ sơ giao dịch, RFQ, đề xuất, đánh giá, đàm phán, hợp đồng, nghiệm thu, thanh lý.", "Cán bộ thực hiện được quy trình chính."],
        ["Doanh nghiệp/nhà cung ứng/chuyên gia", "Đăng ký, cập nhật hồ sơ, đăng sản phẩm/nhu cầu, tham gia giao dịch, theo dõi thông báo.", "Người dùng biết thao tác cơ bản theo vai trò."],
        ["Bộ phận hỗ trợ", "Tiếp nhận phản hồi, sử dụng AI Support/chatbot, xử lý lead, tổng hợp FAQ.", "Hỗ trợ người dùng sau vận hành."],
    ])

    h(doc, 1, "IX. YÊU CẦU KIỂM TRA, THỬ NGHIỆM VÀ NGHIỆM THU")
    h(doc, 2, "1. Phương án kiểm thử")
    table(doc, ["Loại kiểm thử", "Nội dung"], [
        ["Kiểm thử chức năng", "Kiểm tra từng chức năng theo danh mục yêu cầu, vai trò, trạng thái và dữ liệu đầu vào/đầu ra."],
        ["Kiểm thử quy trình", "Kiểm tra các quy trình số hóa từ đầu đến cuối, gồm thông báo, trạng thái, phân quyền, log và tài liệu."],
        ["Kiểm thử tích hợp", "Kiểm tra email/SMS/AI/ký số/API hoặc tích hợp khác theo cấu hình thực tế."],
        ["Kiểm thử bảo mật", "Rà soát xác thực, phân quyền, upload, CSRF, XSS, SQL Injection, dữ liệu cá nhân, secret, log."],
        ["Kiểm thử hiệu năng cơ bản", "Kiểm tra phản hồi các trang phổ biến, tìm kiếm, báo cáo và upload/tải file trong điều kiện kiểm thử."],
        ["Kiểm thử UAT", "Chủ đầu tư/người dùng đại diện kiểm tra theo kịch bản nghiệp vụ và xác nhận kết quả."],
    ])

    h(doc, 2, "2. Tiêu chí nghiệm thu")
    table(doc, ["Nhóm nghiệm thu", "Tiêu chí đạt"], [
        ["Chức năng", "Các chức năng bắt buộc trong hồ sơ yêu cầu được triển khai, kiểm thử và hoạt động đúng theo vai trò."],
        ["Quy trình", "Các quy trình chính thực hiện được từ đầu đến cuối, có trạng thái, thông báo, tài liệu và log."],
        ["Dữ liệu", "Dữ liệu danh mục, nội dung, sản phẩm, nhà cung ứng, chuyên gia, nhu cầu, workflow và báo cáo hiển thị đúng."],
        ["Bảo mật", "Đăng nhập, phân quyền, truy cập trái phép, upload, CSRF, dữ liệu cá nhân, secret và log đạt checklist."],
        ["Triển khai", "Hệ thống chạy trên môi trường chính thức/được chỉ định, HTTPS, backup, tài khoản, cấu hình, giám sát đầy đủ."],
        ["Đào tạo/bàn giao", "Có biên bản đào tạo, tài liệu hướng dẫn, mã nguồn/CSDL/tài khoản/tài liệu bàn giao theo hợp đồng."],
    ])

    h(doc, 2, "3. Phân loại lỗi trong nghiệm thu")
    table(doc, ["Mức lỗi", "Mô tả", "Yêu cầu xử lý"], [
        ["Nghiêm trọng", "Hệ thống không khởi động, không đăng nhập được, mất dữ liệu, sai phân quyền nghiêm trọng, quy trình chính không thể thực hiện.", "Phải khắc phục trước nghiệm thu."],
        ["Cao", "Chức năng quan trọng hoạt động sai, gây cản trở xử lý nghiệp vụ nhưng có phương án tạm thời.", "Khắc phục theo thời hạn thống nhất trước hoặc ngay sau nghiệm thu có điều kiện."],
        ["Trung bình", "Chức năng phụ lỗi, giao diện chưa thuận tiện, báo cáo/biểu mẫu cần chỉnh sửa nhưng không làm dừng quy trình chính.", "Lập danh sách và kế hoạch khắc phục."],
        ["Thấp", "Lỗi chính tả, căn chỉnh, thông báo, cải tiến nhỏ.", "Khắc phục trong giai đoạn hoàn thiện/bảo hành."],
    ])

    h(doc, 1, "X. YÊU CẦU BẢO HÀNH, BẢO TRÌ VÀ HỖ TRỢ")
    table(doc, ["Nội dung", "Yêu cầu"], [
        ["Bảo hành", "Nhà thầu bảo hành phần mềm sau nghiệm thu theo thời gian cam kết trong hợp đồng; xử lý lỗi phát sinh do phần mềm."],
        ["Bảo trì", "Cung cấp phương án bảo trì định kỳ, cập nhật bản vá, tối ưu, sao lưu, kiểm tra log và hỗ trợ vận hành."],
        ["Hỗ trợ kỹ thuật", "Có đầu mối hỗ trợ, kênh tiếp nhận lỗi, thời gian phản hồi, phân loại mức độ lỗi và báo cáo xử lý."],
        ["Cập nhật tài liệu", "Khi có thay đổi chức năng/cấu hình, tài liệu hướng dẫn và tài liệu vận hành phải được cập nhật tương ứng."],
        ["Chuyển giao tri thức", "Hướng dẫn cán bộ vận hành tự xử lý các tác vụ thường gặp như tài khoản, nội dung, backup, báo cáo, cấu hình."],
    ])

    h(doc, 1, "XI. DỊCH VỤ ĐI KÈM")
    bullets(doc, [
        "Khảo sát, tư vấn hoàn thiện yêu cầu nghiệp vụ và yêu cầu kỹ thuật.",
        "Phân tích, thiết kế, phát triển/cấu hình, kiểm thử, triển khai và nhập liệu dữ liệu ban đầu.",
        "Đào tạo quản trị, đào tạo người dùng và chuyển giao công nghệ.",
        "Tư vấn an toàn thông tin, cấu hình môi trường, backup/restore và vận hành.",
        "Hỗ trợ tích hợp email, SMS/OTP, chữ ký số, AI, API/nền tảng dùng chung theo phạm vi hợp đồng.",
        "Hỗ trợ hoàn thiện hồ sơ nghiệm thu, tài liệu kỹ thuật, tài liệu hướng dẫn sử dụng, tài liệu vận hành và biên bản bàn giao.",
    ])

    h(doc, 1, "XII. DANH MỤC SẢN PHẨM/TÀI LIỆU BÀN GIAO")
    table(doc, ["STT", "Sản phẩm/tài liệu", "Yêu cầu bàn giao"], [
        ["1", "Hệ thống phần mềm vận hành", "Phiên bản triển khai trên môi trường chính thức hoặc môi trường do Chủ đầu tư chỉ định."],
        ["2", "Mã nguồn", "Bàn giao theo thỏa thuận hợp đồng; kèm hướng dẫn build/deploy và cấu trúc thư mục."],
        ["3", "Cơ sở dữ liệu", "Script tạo/cập nhật CSDL, dữ liệu mẫu/khởi tạo, hướng dẫn backup/restore."],
        ["4", "Tài liệu yêu cầu/thiết kế", "Đặc tả yêu cầu, kiến trúc hệ thống, mô hình dữ liệu, sơ đồ quy trình, đặc tả tích hợp/API."],
        ["5", "Tài liệu người dùng", "Hướng dẫn khách truy cập, doanh nghiệp, nhà cung ứng, chuyên gia, cán bộ nghiệp vụ."],
        ["6", "Tài liệu quản trị CMS", "Hướng dẫn quản lý menu, danh mục, nội dung, sản phẩm, nhà cung ứng, chuyên gia, phản hồi, báo cáo."],
        ["7", "Tài liệu vận hành", "Cài đặt, cấu hình, phân quyền, backup/restore, giám sát, log, xử lý lỗi, cập nhật phiên bản."],
        ["8", "Tài liệu tích hợp", "Đặc tả email/SMS/AI/ký số/API hoặc tích hợp khác theo phạm vi triển khai."],
        ["9", "Tài liệu kiểm thử", "Test plan, test case, kết quả test, danh sách lỗi, kết quả khắc phục, biên bản UAT."],
        ["10", "Tài liệu đào tạo", "Slide/tài liệu hướng dẫn, biên bản đào tạo, danh sách học viên, câu hỏi thường gặp."],
        ["11", "Biên bản bàn giao/nghiệm thu", "Biên bản cài đặt, nghiệm thu kỹ thuật, nghiệm thu chức năng, nghiệm thu đào tạo, bàn giao tài khoản."],
    ])

    h(doc, 1, "XIII. PHỤ LỤC: BẢNG YÊU CẦU ĐÁP ỨNG TÓM TẮT")
    summary_rows = [
        ["1", "Mô hình web tập trung, phân quyền nhiều vai trò", "Bắt buộc", "Hồ sơ kỹ thuật mục III, IV"],
        ["2", "Cổng thông tin công khai về công nghệ, nhu cầu, nhà cung ứng, chuyên gia", "Bắt buộc", "Mục IV"],
        ["3", "CMS quản trị nội dung, danh mục, người dùng, phản hồi, báo cáo", "Bắt buộc", "Mục IV"],
        ["4", "Quản lý sản phẩm/công nghệ/thiết bị/tài sản trí tuệ", "Bắt buộc", "Mục IV"],
        ["5", "Quản lý nhà cung ứng/chuyên gia", "Bắt buộc", "Mục IV"],
        ["6", "Quản lý nhu cầu công nghệ", "Bắt buộc", "Mục IV"],
        ["7", "Tìm kiếm nâng cao và gợi ý thông minh/AI", "Bắt buộc/cấu hình", "Mục IV, VI"],
        ["8", "Quy trình giao dịch/chuyển giao công nghệ", "Bắt buộc", "Mục IV"],
        ["9", "RFQ, proposal, scoring, negotiation", "Bắt buộc", "Mục IV"],
        ["10", "Hợp đồng điện tử, ký số, audit log", "Bắt buộc khi triển khai ký số", "Mục IV, VI"],
        ["11", "Thông báo web/email/SMS khi cấu hình", "Bắt buộc/cấu hình", "Mục IV, VI"],
        ["12", "Dashboard, báo cáo, xuất Excel/PDF", "Bắt buộc", "Mục IV"],
        ["13", "An toàn thông tin, phân quyền, log, backup", "Bắt buộc", "Mục V"],
        ["14", "Tài liệu, đào tạo, chuyển giao", "Bắt buộc", "Mục VIII, XII"],
        ["15", "Bảo hành, bảo trì, hỗ trợ vận hành", "Bắt buộc", "Mục X"],
    ]
    table(doc, ["STT", "Nội dung yêu cầu", "Mức độ", "Tham chiếu"], summary_rows)

    doc.core_properties.title = "Hồ sơ yêu cầu kỹ thuật - Sàn giao dịch Khoa học và Công nghệ"
    doc.core_properties.subject = "Bản v1.0 theo góc nhìn Chủ đầu tư"
    doc.core_properties.author = "Codex hỗ trợ tổng hợp"
    OUT.parent.mkdir(parents=True, exist_ok=True)
    doc.save(str(OUT))
    print(str(OUT).encode("ascii", errors="backslashreplace").decode("ascii"))


if __name__ == "__main__":
    build()
