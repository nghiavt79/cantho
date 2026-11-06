from __future__ import annotations

from pathlib import Path

from docx import Document
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs" / "ho so ky thuat" / "HSYC_KT_San_GD_KHCN_suon_CDT_v0.1.docx"


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
    for name, size in [("Heading 1", 15), ("Heading 2", 14), ("Heading 3", 13)]:
        st = doc.styles[name]
        st.font.name = "Times New Roman"
        st._element.rPr.rFonts.set(qn("w:eastAsia"), "Times New Roman")
        st.font.size = Pt(size)
        st.font.bold = True
        st.font.color.rgb = RGBColor(31, 78, 121)


def p(doc: Document, text="", bold=False, italic=False, align=WD_ALIGN_PARAGRAPH.JUSTIFY):
    para = doc.add_paragraph()
    para.alignment = align
    para.paragraph_format.space_after = Pt(6)
    para.paragraph_format.line_spacing = 1.15
    run = para.add_run(text)
    set_font(run, bold=bold, italic=italic)
    return para


def title(doc: Document, text: str):
    para = doc.add_paragraph()
    para.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run = para.add_run(text)
    set_font(run, 16, True, color="1F4E79")


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


def cell_text(cell, text, bold=False):
    cell.text = ""
    para = cell.paragraphs[0]
    para.alignment = WD_ALIGN_PARAGRAPH.LEFT
    run = para.add_run(text)
    set_font(run, 12, bold)


def table(doc: Document, headers: list[str], rows: list[list[str]]):
    tbl = doc.add_table(rows=1, cols=len(headers))
    tbl.style = "Table Grid"
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    for i, h in enumerate(headers):
        cell_text(tbl.rows[0].cells[i], h, True)
        shade(tbl.rows[0].cells[i])
        tbl.rows[0].cells[i].vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
    for row in rows:
        cells = tbl.add_row().cells
        for i, val in enumerate(row):
            cell_text(cells[i], val)
            cells[i].vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.TOP
    p(doc, "")


def code(doc: Document, text: str):
    para = doc.add_paragraph()
    para.alignment = WD_ALIGN_PARAGRAPH.LEFT
    para.paragraph_format.space_after = Pt(6)
    for line in text.strip("\n").splitlines():
        run = para.add_run(line + "\n")
        run.font.name = "Consolas"
        run._element.rPr.rFonts.set(qn("w:eastAsia"), "Consolas")
        run.font.size = Pt(9)


def h(doc: Document, level: int, text: str):
    doc.add_heading(text, level=level)


def build():
    doc = Document()
    style_doc(doc)

    title(doc, "HỒ SƠ YÊU CẦU KỸ THUẬT")
    title(doc, "DỰ ÁN SÀN GIAO DỊCH KHOA HỌC VÀ CÔNG NGHỆ")
    p(
        doc,
        "Bản sườn định hướng v0.1 - viết theo góc nhìn của Chủ đầu tư/đơn vị mời thầu. Nội dung dùng để rà soát phạm vi yêu cầu trước khi hoàn thiện thành hồ sơ đầy đủ.",
        italic=True,
        align=WD_ALIGN_PARAGRAPH.CENTER,
    )

    h(doc, 1, "I. TỔNG QUAN")
    h(doc, 2, "1. Bối cảnh và sự cần thiết")
    p(
        doc,
        "Chủ đầu tư có nhu cầu xây dựng Sàn giao dịch Khoa học và Công nghệ nhằm hình thành môi trường số tập trung phục vụ công bố, tra cứu, kết nối, tư vấn, giao dịch và theo dõi quá trình chuyển giao công nghệ trên địa bàn. Hệ thống phải hỗ trợ cơ quan quản lý, doanh nghiệp, nhà cung ứng, chuyên gia, tổ chức khoa học công nghệ và người dân tiếp cận thông tin, gửi nhu cầu, đề xuất giải pháp, thực hiện các bước giao dịch/chuyển giao và khai thác dữ liệu phục vụ quản lý.",
    )
    p(
        doc,
        "Hồ sơ yêu cầu kỹ thuật này không mô tả một sản phẩm có sẵn của nhà thầu. Hồ sơ đặt ra các yêu cầu tối thiểu mà hệ thống, giải pháp, nhân sự, phương án triển khai và dịch vụ đi kèm phải đáp ứng để Chủ đầu tư có cơ sở lựa chọn, nghiệm thu và vận hành.",
        bold=True,
    )
    h(doc, 2, "2. Mục tiêu")
    bullets(
        doc,
        [
            "Xây dựng cổng thông tin/sàn giao dịch số về khoa học và công nghệ phục vụ kết nối cung - cầu công nghệ.",
            "Số hóa các quy trình nghiệp vụ liên quan đến đăng ký, công bố, tìm kiếm, tư vấn, yêu cầu báo giá, đề xuất, đàm phán, ký kết, bàn giao, nghiệm thu và thanh lý giao dịch/chuyển giao công nghệ.",
            "Cung cấp công cụ quản trị dữ liệu, nội dung, người dùng, nhà cung ứng, chuyên gia, sản phẩm công nghệ, nhu cầu công nghệ, báo cáo và thống kê.",
            "Tăng khả năng khai thác dữ liệu thông qua tìm kiếm nâng cao, gợi ý thông minh, chatbot/AI Support và báo cáo quản trị.",
            "Bảo đảm an toàn thông tin, khả năng mở rộng, tích hợp, vận hành ổn định và phù hợp định hướng chuyển đổi số của cơ quan nhà nước.",
        ],
    )
    h(doc, 2, "3. Phạm vi yêu cầu")
    table(
        doc,
        ["Nhóm phạm vi", "Yêu cầu tổng quát"],
        [
            ["Cổng thông tin công khai", "Cung cấp thông tin về sản phẩm/công nghệ/thiết bị/tài sản trí tuệ, nhu cầu công nghệ, nhà cung ứng, chuyên gia, tin tức, sự kiện, video, diễn đàn, liên hệ và các dashboard công khai."],
            ["Quản trị nội dung và dữ liệu", "Cho phép cán bộ được phân quyền quản lý menu, danh mục, bài viết, sản phẩm, nhà cung ứng, chuyên gia, quảng cáo, phản hồi, người dùng, tham số và dữ liệu báo cáo."],
            ["Sàn giao dịch/chuyển giao", "Số hóa các bước nghiệp vụ từ tiếp nhận nhu cầu, NDA, RFQ, đề xuất, đánh giá, đàm phán, pháp lý, hợp đồng, ký số đến bàn giao, nghiệm thu và thanh lý."],
            ["Tìm kiếm và AI", "Hỗ trợ tìm kiếm từ khóa, tìm kiếm nâng cao, gợi ý sản phẩm/nhà cung ứng/công nghệ phù hợp và chatbot hỗ trợ người dùng dựa trên dữ liệu của hệ thống."],
            ["Tích hợp", "Có khả năng tích hợp email, SMS/OTP khi cần, chữ ký số/hợp đồng điện tử, API trao đổi dữ liệu, nền tảng dùng chung hoặc hệ thống khác theo yêu cầu của Chủ đầu tư."],
            ["Dịch vụ triển khai", "Bao gồm khảo sát, phân tích yêu cầu, thiết kế, phát triển/cấu hình, nhập liệu ban đầu, kiểm thử, đào tạo, chuyển giao, bảo hành, bảo trì và hỗ trợ vận hành."],
        ],
    )

    h(doc, 1, "II. YÊU CẦU VỀ MÔ HÌNH VÀ KIẾN TRÚC HỆ THỐNG")
    h(doc, 2, "1. Mô hình tổng thể")
    p(doc, "Hệ thống yêu cầu triển khai theo mô hình ứng dụng web tập trung, có phân tách tối thiểu các lớp: giao diện người dùng, xử lý nghiệp vụ, dữ liệu, tích hợp và an toàn thông tin.")
    code(
        doc,
        """
Người dùng công khai / Doanh nghiệp / Nhà cung ứng / Chuyên gia / Cán bộ quản trị
        |
        v
Trình duyệt Web trên PC, Tablet, Mobile
        |
        v
Lớp truy cập HTTPS, tên miền, chứng thư số, firewall/reverse proxy
        |
        v
Ứng dụng Sàn giao dịch KHCN
  |-- Cổng thông tin công khai
  |-- Phân hệ tài khoản và phân quyền
  |-- Phân hệ CMS/quản trị dữ liệu
  |-- Phân hệ sản phẩm, công nghệ, nhu cầu, nhà cung ứng, chuyên gia
  |-- Phân hệ giao dịch/chuyển giao công nghệ
  |-- Phân hệ hợp đồng, ký số, tài liệu
  |-- Phân hệ tìm kiếm, AI Support, báo cáo, thống kê
        |
        +--> Cơ sở dữ liệu tập trung
        +--> Kho lưu trữ file/tài liệu
        +--> Email/SMS/OTP
        +--> Chữ ký số/hợp đồng điện tử
        +--> API/nền tảng tích hợp, chia sẻ dữ liệu khi có yêu cầu
        """,
    )
    h(doc, 2, "2. Yêu cầu kiến trúc phần mềm")
    bullets(
        doc,
        [
            "Kiến trúc phải có khả năng mở rộng theo module, không phụ thuộc cứng vào một quy trình duy nhất; khi bổ sung quy trình hoặc biểu mẫu mới phải hạn chế ảnh hưởng đến các chức năng hiện có.",
            "Phải có phân tách rõ giữa giao diện, nghiệp vụ, truy cập dữ liệu, tích hợp bên ngoài và cấu hình hệ thống.",
            "Phải hỗ trợ triển khai trên hạ tầng máy chủ của Chủ đầu tư hoặc hạ tầng được Chủ đầu tư chỉ định; không ràng buộc vào dịch vụ độc quyền không cần thiết.",
            "Phải có cơ chế cấu hình môi trường cho kiểm thử, vận hành chính thức và dự phòng; các thông tin bí mật không được hard-code trong mã nguồn hoặc giao diện.",
            "Phải hỗ trợ chuẩn Unicode tiếng Việt, đa trình duyệt phổ biến và giao diện đáp ứng trên thiết bị di động.",
        ],
    )
    h(doc, 2, "3. Yêu cầu kiến trúc dữ liệu")
    bullets(
        doc,
        [
            "Dữ liệu phải được quản lý tập trung, có khóa chính/khóa ngoại hoặc cơ chế bảo đảm toàn vẹn tương đương.",
            "Các nhóm dữ liệu tối thiểu gồm: người dùng, vai trò/quyền, danh mục, nội dung, sản phẩm/công nghệ, nhà cung ứng, chuyên gia, nhu cầu công nghệ, hồ sơ giao dịch, hợp đồng, tài liệu, thông báo, log, báo cáo và dữ liệu AI/tìm kiếm.",
            "Phải có cơ chế sao lưu, phục hồi, phân quyền truy cập dữ liệu và ghi nhận lịch sử thay đổi đối với dữ liệu quan trọng.",
            "Phải có khả năng xuất/nhập dữ liệu theo các định dạng thông dụng như Excel, PDF, CSV hoặc API khi Chủ đầu tư yêu cầu.",
        ],
    )
    h(doc, 2, "4. Yêu cầu triển khai hạ tầng")
    table(
        doc,
        ["Hạng mục", "Yêu cầu"],
        [
            ["Máy chủ ứng dụng", "Đáp ứng khả năng chạy ứng dụng web, xử lý đồng thời người dùng truy cập, hỗ trợ HTTPS, logging, giám sát và triển khai cập nhật phiên bản."],
            ["Máy chủ cơ sở dữ liệu", "Đáp ứng khả năng lưu trữ dữ liệu nghiệp vụ, dữ liệu file metadata, chỉ mục tìm kiếm, log và báo cáo; có cơ chế backup/restore định kỳ."],
            ["Lưu trữ tài liệu", "Phải có vùng lưu trữ riêng cho ảnh, video, tài liệu, hợp đồng, hồ sơ xác minh, tệp đính kèm; kiểm soát dung lượng và quyền truy cập."],
            ["Đường truyền", "Đảm bảo băng thông và độ ổn định phục vụ người dùng internet và cán bộ quản trị; hỗ trợ tên miền, SSL/TLS và khả năng mở rộng khi lưu lượng tăng."],
            ["IPv6", "Giải pháp phải sẵn sàng triển khai IPv6 hoặc dual-stack IPv4/IPv6 khi hạ tầng của Chủ đầu tư đáp ứng."],
            ["Dự phòng", "Có phương án sao lưu, phục hồi và khôi phục vận hành khi phát sinh lỗi hạ tầng, lỗi ứng dụng hoặc lỗi dữ liệu."],
        ],
    )

    h(doc, 1, "III. DANH MỤC TIÊU CHUẨN, QUY CHUẨN VÀ CĂN CỨ ÁP DỤNG")
    p(doc, "Nhà thầu phải đề xuất giải pháp đáp ứng các quy định, tiêu chuẩn kỹ thuật hiện hành liên quan đến ứng dụng CNTT trong cơ quan nhà nước, an toàn thông tin, dữ liệu cá nhân, giao dịch điện tử, chữ ký số và kết nối/chia sẻ dữ liệu.")
    table(
        doc,
        ["Nhóm", "Yêu cầu áp dụng"],
        [
            ["Ứng dụng CNTT trong cơ quan nhà nước", "Tuân thủ danh mục tiêu chuẩn kỹ thuật về ứng dụng CNTT trong cơ quan nhà nước theo Thông tư 39/2017/TT-BTTTT và các văn bản thay thế/bổ sung nếu có."],
            ["An toàn thông tin theo cấp độ", "Tuân thủ Luật An toàn thông tin mạng, Nghị định 85/2016/NĐ-CP, Thông tư 12/2022/TT-BTTTT và yêu cầu phân loại/cấp độ hệ thống do Chủ đầu tư xác định."],
            ["Dữ liệu cá nhân", "Tuân thủ Nghị định 13/2023/NĐ-CP, Nghị định 356/2025/NĐ-CP và chính sách bảo vệ dữ liệu cá nhân của Chủ đầu tư."],
            ["Giao dịch điện tử/chữ ký số", "Tuân thủ quy định pháp luật về giao dịch điện tử, chữ ký số, chứng thư số và lưu trữ chứng từ điện tử khi triển khai hợp đồng điện tử/ký số."],
            ["Kết nối, chia sẻ dữ liệu", "Bảo đảm khả năng kết nối qua API, sử dụng định dạng dữ liệu phổ biến, có cơ chế xác thực, phân quyền, nhật ký giao dịch và tài liệu đặc tả tích hợp."],
            ["Khả năng truy cập và trình duyệt", "Giao diện web hỗ trợ các trình duyệt phổ biến, responsive trên thiết bị di động, sử dụng Unicode tiếng Việt và bảo đảm trải nghiệm truy cập ổn định."],
            ["Quản lý chất lượng phần mềm", "Có quy trình kiểm thử, nghiệm thu, quản lý lỗi, quản lý phiên bản, tài liệu hướng dẫn và bàn giao mã nguồn/tài liệu theo yêu cầu hợp đồng."],
        ],
    )

    h(doc, 1, "IV. YÊU CẦU CHỨC NĂNG")
    h(doc, 2, "1. Các tác nhân tham gia hệ thống")
    table(
        doc,
        ["STT", "Tác nhân", "Vai trò/yêu cầu quyền hạn"],
        [
            ["1", "Khách truy cập", "Tra cứu thông tin công khai, tìm kiếm công nghệ/nhà cung ứng/chuyên gia, gửi liên hệ, gửi nhu cầu tư vấn, sử dụng chatbot nếu được bật."],
            ["2", "Người mua/doanh nghiệp có nhu cầu", "Đăng ký tài khoản, cập nhật hồ sơ, đăng nhu cầu công nghệ, tạo/yêu cầu giao dịch, theo dõi quy trình, trao đổi, phê duyệt/ký tài liệu theo vai trò."],
            ["3", "Nhà cung ứng/cá nhân/tổ chức sở hữu công nghệ", "Đăng ký hồ sơ, đăng sản phẩm/công nghệ/thiết bị/tài sản trí tuệ, nhận RFQ, nộp đề xuất, đàm phán, ký hợp đồng, bàn giao hồ sơ."],
            ["4", "Chuyên gia/tư vấn", "Cập nhật hồ sơ chuyên gia, tham gia tư vấn, rà soát hồ sơ, hỗ trợ đánh giá, theo dõi quy trình theo phân quyền."],
            ["5", "Cán bộ nghiệp vụ", "Quản lý nội dung, kiểm duyệt hồ sơ, điều phối giao dịch, xử lý phản hồi, theo dõi báo cáo và hỗ trợ người dùng."],
            ["6", "Quản trị hệ thống", "Quản lý tài khoản, vai trò, phân quyền, cấu hình, danh mục, log, sao lưu, tích hợp và tham số vận hành."],
            ["7", "Lãnh đạo/đơn vị quản lý", "Xem dashboard, báo cáo tổng hợp, thống kê hiệu quả kết nối cung - cầu và tình hình xử lý giao dịch."],
        ],
    )

    h(doc, 2, "2. Danh mục nhóm chức năng yêu cầu")
    table(
        doc,
        ["STT", "Nhóm chức năng", "Yêu cầu tối thiểu"],
        [
            ["1", "Quản trị tài khoản và phân quyền", "Đăng nhập/đăng xuất; quên mật khẩu/OTP; quản lý người dùng; phân quyền theo vai trò; phân quyền theo chức năng và theo hồ sơ/quy trình; khóa/mở tài khoản; nhật ký truy cập."],
            ["2", "Quản trị cổng thông tin", "Quản lý menu, danh mục, bài viết, tin tức, sự kiện, video, banner, quảng cáo, trang giới thiệu/quy định, thông tin liên hệ và SEO cơ bản."],
            ["3", "Quản lý sản phẩm, công nghệ, thiết bị, tài sản trí tuệ", "Tạo/sửa/xóa/duyệt hồ sơ; quản lý ảnh, video, tài liệu; phân loại lĩnh vực; trạng thái hiển thị; thông số kỹ thuật; chủ sở hữu/nhà cung ứng; lịch sử chỉnh sửa."],
            ["4", "Quản lý nhà cung ứng", "Đăng ký hồ sơ, hồ sơ pháp lý, năng lực, lĩnh vực cung ứng, gian hàng/hồ sơ công khai, sản phẩm liên quan, kiểm duyệt và lịch sử giao dịch."],
            ["5", "Quản lý chuyên gia/tư vấn", "Hồ sơ chuyên gia, lĩnh vực chuyên môn, kinh nghiệm, tổ chức công tác, thông tin liên hệ, trạng thái xác thực/hiển thị và phân công tư vấn."],
            ["6", "Quản lý nhu cầu công nghệ", "Tiếp nhận nhu cầu, mô tả bài toán, lĩnh vực, địa bàn, mức độ ưu tiên, tài liệu đính kèm, trạng thái xử lý, phản hồi và gợi ý nguồn cung phù hợp."],
            ["7", "Tìm kiếm, tra cứu, lọc dữ liệu", "Tìm kiếm toàn văn, lọc theo lĩnh vực/loại sản phẩm/đơn vị/trạng thái; tìm kiếm nâng cao; lưu lịch sử truy vấn; gợi ý kết quả liên quan."],
            ["8", "AI Support và gợi ý thông minh", "Chatbot hỗ trợ người dùng dựa trên dữ liệu nội bộ; gợi ý sản phẩm/nhà cung ứng/công nghệ; có cấu hình bật/tắt AI; lưu lead/yêu cầu hỗ trợ; kiểm soát dữ liệu gửi ra ngoài nếu dùng AI provider."],
            ["9", "Quy trình giao dịch/chuyển giao công nghệ", "Số hóa tối thiểu các bước: tiếp nhận yêu cầu, NDA, RFQ, nộp đề xuất, chấm điểm, đàm phán, rà soát pháp lý, hợp đồng, thanh toán tạm ứng, thử nghiệm, bàn giao, đào tạo, tài liệu kỹ thuật, nghiệm thu, thanh lý."],
            ["10", "RFQ, đề xuất và đánh giá", "Tạo yêu cầu báo giá; mời nhà cung ứng; nhận hồ sơ đề xuất; quản lý phiên bản; chấm điểm theo tiêu chí; lựa chọn/ghi nhận kết quả."],
            ["11", "Hợp đồng điện tử và ký số", "Tạo dự thảo hợp đồng; upload/tải hợp đồng; quản lý phiên bản; phê duyệt nhiều bên; ký số; lưu hash; nhật ký audit; tải hồ sơ hợp đồng."],
            ["12", "Thông báo và trao đổi", "Thông báo trên web/email/SMS khi cần; thông báo theo trạng thái quy trình; nhắc việc; trao đổi giữa các bên; lưu lịch sử thông báo."],
            ["13", "Báo cáo, thống kê, dashboard", "Thống kê sản phẩm, nhà cung ứng, nhu cầu, giao dịch, hợp đồng, lượt truy cập, phản hồi, hiệu quả kết nối; xuất Excel/PDF; dashboard cho lãnh đạo và quản trị."],
            ["14", "Quản lý phản hồi và hỗ trợ", "Tiếp nhận liên hệ, phản hồi, yêu cầu tư vấn; phân loại; giao xử lý; theo dõi trạng thái; ghi nhận lịch sử hỗ trợ."],
            ["15", "Quản lý cấu hình và tham số", "Cấu hình thông tin đơn vị, email, AI, ký số, danh mục hệ thống, mẫu thông báo, thời gian OTP, tham số dashboard và các feature flag."],
            ["16", "Tích hợp và API", "Cung cấp/tiếp nhận API theo yêu cầu; đặc tả dữ liệu; xác thực API; log giao dịch; hỗ trợ tích hợp nền tảng dùng chung khi có chỉ đạo."],
        ],
    )

    h(doc, 2, "3. Quy trình nghiệp vụ cần số hóa")
    table(
        doc,
        ["STT", "Quy trình", "Yêu cầu số hóa"],
        [
            ["1", "Đăng ký và xác thực tài khoản", "Người dùng đăng ký, cập nhật hồ sơ, xác thực email/điện thoại hoặc hồ sơ pháp lý theo quy định; quản trị duyệt/kích hoạt."],
            ["2", "Đăng và duyệt sản phẩm/công nghệ", "Nhà cung ứng gửi hồ sơ; cán bộ kiểm tra, yêu cầu bổ sung, duyệt hiển thị hoặc từ chối; hệ thống lưu lịch sử xử lý."],
            ["3", "Đăng nhu cầu công nghệ", "Doanh nghiệp/người dùng gửi nhu cầu; hệ thống phân loại, gợi ý nguồn cung/chuyên gia; cán bộ điều phối xử lý."],
            ["4", "Kết nối cung - cầu", "Tạo hồ sơ giao dịch/chuyển giao; mời bên liên quan; trao đổi; cập nhật trạng thái; lưu tài liệu và thông báo."],
            ["5", "Ký NDA/bảo mật thông tin", "Tạo thỏa thuận bảo mật, gửi các bên xem xét/ký, kiểm soát điều kiện truy cập dữ liệu sau khi ký."],
            ["6", "RFQ và nộp đề xuất", "Tạo RFQ, mời nhà cung ứng, nộp đề xuất kỹ thuật/tài chính, đóng/mở vòng nhận đề xuất, lưu phiên bản."],
            ["7", "Đánh giá và lựa chọn", "Chấm điểm theo tiêu chí; tổng hợp kết quả; phê duyệt lựa chọn; ghi nhận lý do và log xử lý."],
            ["8", "Đàm phán và rà soát pháp lý", "Cập nhật nội dung đàm phán, điều khoản, giá trị, tiến độ; rà soát pháp lý; yêu cầu bổ sung/chỉnh sửa."],
            ["9", "Hợp đồng và ký số", "Tạo/upload hợp đồng, phê duyệt nhiều bên, ký số, lưu file cuối cùng, hash và audit log."],
            ["10", "Triển khai, bàn giao, nghiệm thu, thanh lý", "Theo dõi thanh toán, pilot, bàn giao thiết bị/dịch vụ, đào tạo, tài liệu kỹ thuật, nghiệm thu, thanh lý và lưu hồ sơ hoàn tất."],
        ],
    )

    h(doc, 2, "4. Bảng chuyển đổi yêu cầu chức năng sang trường hợp sử dụng")
    table(
        doc,
        ["Mã UC", "Tên trường hợp sử dụng", "Tác nhân chính", "Mô tả ngắn"],
        [
            ["UC-01", "Đăng nhập/đăng xuất", "Người dùng", "Người dùng truy cập hệ thống theo tài khoản được cấp hoặc tự đăng ký."],
            ["UC-02", "Quản lý người dùng và phân quyền", "QTHT", "Tạo/sửa/khóa tài khoản, gán vai trò, phân quyền theo chức năng và hồ sơ."],
            ["UC-03", "Quản lý nội dung cổng thông tin", "Cán bộ CMS", "Cập nhật tin tức, sự kiện, menu, banner, video, trang thông tin."],
            ["UC-04", "Đăng hồ sơ sản phẩm/công nghệ", "Nhà cung ứng", "Nhập thông tin công nghệ, thiết bị, tài sản trí tuệ, tài liệu và gửi duyệt."],
            ["UC-05", "Duyệt hồ sơ sản phẩm/công nghệ", "Cán bộ nghiệp vụ", "Kiểm tra hồ sơ, yêu cầu bổ sung, duyệt hiển thị hoặc từ chối."],
            ["UC-06", "Đăng nhu cầu công nghệ", "Doanh nghiệp", "Tạo nhu cầu, đính kèm tài liệu, gửi hệ thống tiếp nhận và xử lý."],
            ["UC-07", "Tra cứu và tìm kiếm", "Mọi tác nhân", "Tìm kiếm công nghệ, thiết bị, nhà cung ứng, chuyên gia, nhu cầu, tin tức."],
            ["UC-08", "Chatbot/AI Support", "Khách truy cập", "Hỏi đáp thông tin, nhận gợi ý, gửi thông tin liên hệ khi cần hỗ trợ."],
            ["UC-09", "Tạo hồ sơ giao dịch", "Cán bộ/Doanh nghiệp", "Khởi tạo dự án/chuyển giao, phân công thành viên và theo dõi trạng thái."],
            ["UC-10", "Ký NDA", "Các bên tham gia", "Tạo, xem, phê duyệt/ký NDA và mở quyền truy cập hồ sơ tương ứng."],
            ["UC-11", "Tạo RFQ và mời nhà cung ứng", "Cán bộ/Doanh nghiệp", "Lập yêu cầu báo giá, gửi lời mời và theo dõi phản hồi."],
            ["UC-12", "Nộp và đánh giá đề xuất", "Nhà cung ứng/Cán bộ", "Nhà cung ứng nộp đề xuất; cán bộ chấm điểm và tổng hợp kết quả."],
            ["UC-13", "Đàm phán và rà soát pháp lý", "Các bên", "Cập nhật nội dung đàm phán, pháp lý, điều khoản hợp đồng."],
            ["UC-14", "Quản lý hợp đồng/ký số", "Các bên", "Tạo/upload hợp đồng, phê duyệt, ký số, lưu trữ và tải hồ sơ."],
            ["UC-15", "Bàn giao/nghiệm thu/thanh lý", "Các bên", "Cập nhật biên bản, tài liệu, trạng thái nghiệm thu và thanh lý."],
            ["UC-16", "Báo cáo thống kê", "Lãnh đạo/Cán bộ", "Xem dashboard, lọc dữ liệu, xuất báo cáo theo kỳ."],
        ],
    )

    h(doc, 1, "V. YÊU CẦU PHI CHỨC NĂNG")
    h(doc, 2, "1. Yêu cầu về an toàn thông tin")
    bullets(
        doc,
        [
            "Có cơ chế xác thực, phân quyền, quản lý phiên đăng nhập và giới hạn quyền theo vai trò.",
            "Các chức năng thay đổi dữ liệu phải kiểm soát quyền, xác thực request và ghi nhận lịch sử thao tác đối với dữ liệu quan trọng.",
            "Có cơ chế bảo vệ chống các rủi ro phổ biến của ứng dụng web như truy cập trái phép, CSRF, XSS, SQL Injection, upload file nguy hiểm và rò rỉ thông tin lỗi hệ thống.",
            "Thông tin bí mật như mật khẩu, khóa API, connection string, thông tin ký số không được lưu công khai trong mã nguồn/giao diện; phải có phương án quản lý secret khi triển khai.",
            "Có phương án sao lưu dữ liệu, phục hồi dữ liệu, phân quyền truy cập cơ sở dữ liệu và kiểm soát dữ liệu cá nhân.",
        ],
    )
    h(doc, 2, "2. Yêu cầu hiệu năng và thời gian xử lý")
    table(
        doc,
        ["Hạng mục", "Yêu cầu"],
        [
            ["Truy cập trang thông tin", "Thời gian phản hồi ở điều kiện vận hành bình thường phải đáp ứng trải nghiệm người dùng, ưu tiên dưới 3 giây với trang phổ biến."],
            ["Tìm kiếm", "Kết quả tìm kiếm thông thường phải trả về trong thời gian phù hợp; có chỉ mục, cache hoặc tối ưu truy vấn khi dữ liệu tăng."],
            ["Upload/tải tài liệu", "Có giới hạn kích thước file, kiểm tra định dạng và thông báo rõ ràng khi vượt giới hạn."],
            ["Báo cáo/dashboard", "Các báo cáo tổng hợp lớn cần có cơ chế snapshot/cache hoặc xử lý nền để không ảnh hưởng giao dịch chính."],
            ["AI/chatbot", "Có timeout, fallback và thông báo thân thiện khi dịch vụ AI hoặc nguồn dữ liệu không phản hồi."],
        ],
    )
    h(doc, 2, "3. Yêu cầu tính sẵn sàng và chịu lỗi")
    bullets(
        doc,
        [
            "Hệ thống phải có logging đủ để truy vết lỗi ứng dụng, lỗi tích hợp, lỗi xử lý nền và lỗi cơ sở dữ liệu.",
            "Lỗi ở một phân hệ phụ trợ như email, AI, SMS hoặc ký số không được làm sập toàn bộ hệ thống; cần có thông báo lỗi và cơ chế xử lý lại khi phù hợp.",
            "Có quy trình sao lưu, khôi phục, cập nhật phiên bản, rollback và bảo trì định kỳ.",
            "Có phân loại mức độ lỗi, thời gian phản hồi và thời gian khắc phục trong giai đoạn bảo hành/bảo trì.",
        ],
    )
    h(doc, 2, "4. Yêu cầu giao diện và trải nghiệm người dùng")
    bullets(
        doc,
        [
            "Giao diện tiếng Việt rõ ràng, dễ sử dụng, phù hợp với cán bộ quản trị và người dùng doanh nghiệp.",
            "Tương thích các trình duyệt phổ biến, hiển thị tốt trên máy tính, máy tính bảng và điện thoại.",
            "Các biểu mẫu nghiệp vụ phải có kiểm tra dữ liệu đầu vào, thông báo lỗi rõ ràng, trạng thái lưu/gửi/duyệt minh bạch.",
            "Các dashboard/báo cáo phải có bộ lọc, chỉ số, biểu đồ hoặc bảng dữ liệu phù hợp nhu cầu quản lý.",
        ],
    )
    h(doc, 2, "5. Yêu cầu về vận hành, bảo trì và mở rộng")
    bullets(
        doc,
        [
            "Có tài liệu cài đặt, cấu hình, vận hành, backup/restore, xử lý lỗi thường gặp và cập nhật phiên bản.",
            "Có khả năng mở rộng thêm danh mục, biểu mẫu, quy trình, vai trò, báo cáo và tích hợp mới.",
            "Có kế hoạch bảo hành, bảo trì, hỗ trợ kỹ thuật, cập nhật lỗi và tư vấn vận hành sau nghiệm thu.",
            "Có bàn giao mã nguồn, tài liệu kỹ thuật, tài liệu người dùng và tài khoản/quyền quản trị theo thỏa thuận hợp đồng.",
        ],
    )

    h(doc, 1, "VI. YÊU CẦU VỀ NHÂN SỰ THỰC HIỆN")
    table(
        doc,
        ["STT", "Vai trò", "Yêu cầu năng lực tối thiểu", "Số lượng dự kiến"],
        [
            ["1", "Quản trị dự án/PM", "Có kinh nghiệm quản lý dự án phần mềm, điều phối khảo sát, kế hoạch, chất lượng, nghiệm thu và bàn giao.", "1"],
            ["2", "Kiến trúc sư/Trưởng nhóm kỹ thuật", "Có kinh nghiệm thiết kế kiến trúc web, cơ sở dữ liệu, tích hợp, bảo mật và triển khai hệ thống.", "1"],
            ["3", "Chuyên gia phân tích nghiệp vụ", "Có khả năng khảo sát, mô hình hóa quy trình, đặc tả yêu cầu, xây dựng use case và tài liệu nghiệm thu.", "1-2"],
            ["4", "Lập trình viên backend", "Có kinh nghiệm phát triển API, nghiệp vụ, cơ sở dữ liệu, phân quyền, workflow và tích hợp.", "2"],
            ["5", "Lập trình viên frontend/UI", "Có kinh nghiệm xây dựng giao diện web responsive, biểu mẫu nghiệp vụ, dashboard và tối ưu trải nghiệm người dùng.", "1-2"],
            ["6", "Chuyên gia cơ sở dữ liệu", "Có kinh nghiệm thiết kế, tối ưu, backup/restore và vận hành cơ sở dữ liệu.", "1"],
            ["7", "Chuyên gia an toàn thông tin", "Có kinh nghiệm rà soát bảo mật ứng dụng web, phân quyền, cấu hình máy chủ và khuyến nghị ATTT.", "1"],
            ["8", "Kiểm thử/QA", "Có kinh nghiệm lập kế hoạch test, test case, kiểm thử chức năng, hồi quy, tích hợp và nghiệm thu.", "1-2"],
            ["9", "Triển khai/DevOps", "Có kinh nghiệm cài đặt, cấu hình server, triển khai ứng dụng, giám sát, backup và hỗ trợ vận hành.", "1"],
            ["10", "Đào tạo/hỗ trợ người dùng", "Có khả năng biên soạn tài liệu, đào tạo cán bộ quản trị/người dùng và tiếp nhận hỗ trợ sau bàn giao.", "1"],
        ],
    )
    p(doc, "Nhà thầu phải cung cấp hồ sơ năng lực nhân sự, vai trò tham gia, thời gian huy động và phương án thay thế nhân sự khi cần, bảo đảm không ảnh hưởng tiến độ dự án.", bold=True)

    h(doc, 1, "VII. YÊU CẦU VỀ PHƯƠNG ÁN TRIỂN KHAI")
    h(doc, 2, "1. Các giai đoạn triển khai")
    table(
        doc,
        ["Giai đoạn", "Nội dung công việc", "Sản phẩm đầu ra"],
        [
            ["Khảo sát và khởi động", "Khảo sát hiện trạng, yêu cầu nghiệp vụ, dữ liệu, hạ tầng, tích hợp, nhân sự vận hành.", "Biên bản khảo sát, kế hoạch triển khai, danh sách yêu cầu."],
            ["Phân tích và thiết kế", "Đặc tả yêu cầu, mô hình quy trình, mô hình dữ liệu, kiến trúc hệ thống, thiết kế giao diện mẫu.", "Tài liệu SRS/thiết kế, prototype/giao diện mẫu."],
            ["Phát triển/cấu hình", "Xây dựng hoặc cấu hình các phân hệ, dữ liệu, phân quyền, workflow, báo cáo, tích hợp.", "Phiên bản phần mềm trên môi trường kiểm thử."],
            ["Kiểm thử nội bộ", "Kiểm thử chức năng, tích hợp, bảo mật cơ bản, hiệu năng cơ bản, sửa lỗi.", "Biên bản test, danh sách lỗi và kết quả khắc phục."],
            ["UAT và hiệu chỉnh", "Chủ đầu tư/người dùng kiểm thử nghiệm thu, góp ý, hiệu chỉnh.", "Biên bản UAT, bản phần mềm đủ điều kiện triển khai."],
            ["Triển khai chính thức", "Cài đặt production, cấu hình tên miền/SSL, dữ liệu ban đầu, tài khoản, backup, giám sát.", "Biên bản cài đặt, tài khoản bàn giao, hệ thống vận hành."],
            ["Đào tạo và chuyển giao", "Đào tạo quản trị, nghiệp vụ, người dùng; bàn giao tài liệu, mã nguồn, CSDL theo hợp đồng.", "Biên bản đào tạo, biên bản bàn giao."],
            ["Bảo hành/bảo trì", "Hỗ trợ vận hành, xử lý lỗi, cập nhật tài liệu, tối ưu theo thực tế.", "Báo cáo hỗ trợ, biên bản nghiệm thu bảo hành/bảo trì nếu có."],
        ],
    )
    h(doc, 2, "2. Yêu cầu môi trường triển khai")
    bullets(
        doc,
        [
            "Phải có tối thiểu môi trường kiểm thử/UAT và môi trường vận hành chính thức.",
            "Dữ liệu kiểm thử không được chứa thông tin bí mật nếu chưa có biện pháp bảo vệ phù hợp.",
            "Môi trường production phải cấu hình HTTPS, backup, logging, tài khoản quản trị và quyền truy cập theo nguyên tắc tối thiểu.",
            "Nhà thầu phải có hướng dẫn rollback khi cập nhật phiên bản phát sinh lỗi.",
        ],
    )
    h(doc, 2, "3. Phương án đào tạo và chuyển giao")
    table(
        doc,
        ["Nhóm đào tạo", "Nội dung"],
        [
            ["Quản trị hệ thống", "Cài đặt, cấu hình, tài khoản, phân quyền, backup/restore, log, tích hợp, xử lý lỗi thường gặp."],
            ["Cán bộ CMS", "Quản trị menu, danh mục, bài viết, sản phẩm, nhà cung ứng, chuyên gia, phản hồi, quảng cáo, dashboard."],
            ["Cán bộ nghiệp vụ", "Xử lý hồ sơ giao dịch, RFQ, đề xuất, đánh giá, đàm phán, hợp đồng, nghiệm thu, thanh lý."],
            ["Người dùng doanh nghiệp/nhà cung ứng", "Đăng ký, cập nhật hồ sơ, đăng sản phẩm/nhu cầu, tham gia giao dịch, theo dõi thông báo và ký hồ sơ."],
            ["Bộ phận hỗ trợ", "Tiếp nhận phản hồi, sử dụng AI Support/chatbot, xử lý lead, tổng hợp câu hỏi thường gặp."],
        ],
    )

    h(doc, 1, "VIII. YÊU CẦU VỀ KIỂM TRA, NGHIỆM THU")
    table(
        doc,
        ["Nhóm nghiệm thu", "Tiêu chí kiểm tra"],
        [
            ["Nghiệm thu chức năng", "Các chức năng trong danh mục yêu cầu hoạt động đúng theo vai trò, dữ liệu đầu vào/đầu ra và luồng nghiệp vụ."],
            ["Nghiệm thu quy trình", "Các quy trình số hóa chính thực hiện được từ đầu đến cuối, có trạng thái, thông báo, log và hồ sơ lưu trữ."],
            ["Nghiệm thu dữ liệu", "Dữ liệu danh mục, nội dung mẫu, người dùng, sản phẩm, nhà cung ứng, chuyên gia, workflow và báo cáo hiển thị đúng."],
            ["Nghiệm thu bảo mật", "Đăng nhập, phân quyền, truy cập trái phép, upload, CSRF, dữ liệu cá nhân, secret và log được rà soát theo checklist."],
            ["Nghiệm thu triển khai", "Hệ thống chạy trên môi trường production, HTTPS, backup, tài khoản, cấu hình, giám sát và tài liệu cài đặt đầy đủ."],
            ["Nghiệm thu đào tạo/bàn giao", "Có biên bản đào tạo, danh sách học viên, tài liệu hướng dẫn, mã nguồn/tài liệu/CSDL/tài khoản bàn giao theo hợp đồng."],
        ],
    )

    h(doc, 1, "IX. YÊU CẦU VỀ DỊCH VỤ ĐI KÈM")
    bullets(
        doc,
        [
            "Khảo sát, tư vấn hoàn thiện yêu cầu nghiệp vụ và yêu cầu kỹ thuật.",
            "Thiết kế, phát triển/cấu hình, kiểm thử, triển khai và nhập liệu ban đầu.",
            "Đào tạo quản trị, đào tạo người dùng và chuyển giao công nghệ.",
            "Bảo hành phần mềm, xử lý lỗi phát sinh, hỗ trợ vận hành trong thời gian cam kết.",
            "Bảo trì, cập nhật, tối ưu hệ thống, hỗ trợ sao lưu/phục hồi và tư vấn mở rộng sau nghiệm thu nếu Chủ đầu tư yêu cầu.",
            "Hỗ trợ hoàn thiện hồ sơ nghiệm thu, tài liệu kỹ thuật, tài liệu hướng dẫn sử dụng, tài liệu vận hành và biên bản bàn giao.",
        ],
    )

    h(doc, 1, "X. DANH MỤC TÀI LIỆU/SẢN PHẨM YÊU CẦU BÀN GIAO")
    table(
        doc,
        ["STT", "Tài liệu/sản phẩm", "Yêu cầu bàn giao"],
        [
            ["1", "Phần mềm/hệ thống vận hành", "Phiên bản triển khai trên môi trường production hoặc môi trường do Chủ đầu tư chỉ định."],
            ["2", "Mã nguồn và hướng dẫn build/deploy", "Bàn giao theo thỏa thuận hợp đồng, kèm cấu trúc thư mục, hướng dẫn cấu hình và triển khai."],
            ["3", "Cơ sở dữ liệu", "Script tạo/cập nhật CSDL, dữ liệu mẫu/khởi tạo, hướng dẫn backup/restore."],
            ["4", "Tài liệu yêu cầu/thiết kế", "Đặc tả yêu cầu, kiến trúc hệ thống, mô hình dữ liệu, sơ đồ quy trình, đặc tả tích hợp/API."],
            ["5", "Tài liệu người dùng", "Hướng dẫn người dùng công khai, doanh nghiệp, nhà cung ứng, chuyên gia và cán bộ nghiệp vụ."],
            ["6", "Tài liệu quản trị/vận hành", "Hướng dẫn CMS, cấu hình, phân quyền, backup, monitoring, xử lý lỗi và cập nhật phiên bản."],
            ["7", "Tài liệu kiểm thử", "Kế hoạch test, test case, kết quả test, danh sách lỗi đã xử lý, biên bản UAT."],
            ["8", "Tài liệu đào tạo", "Slide/tài liệu hướng dẫn, biên bản đào tạo, danh sách học viên, câu hỏi thường gặp."],
            ["9", "Biên bản bàn giao/nghiệm thu", "Biên bản cài đặt, nghiệm thu kỹ thuật, nghiệm thu chức năng, nghiệm thu đào tạo và bàn giao tài khoản."],
        ],
    )

    h(doc, 1, "XI. GHI CHÚ ĐỂ HOÀN THIỆN BẢN FULL")
    bullets(
        doc,
        [
            "Sau khi Chủ đầu tư rà soát sườn, cần chốt phạm vi chức năng bắt buộc và chức năng mở rộng để tránh vượt dự toán.",
            "Cần xác định cấp độ an toàn thông tin dự kiến, hạ tầng triển khai, yêu cầu tích hợp với nền tảng dùng chung, chữ ký số và dữ liệu ban đầu.",
            "Cần tách riêng bảng yêu cầu kỹ thuật/đáp ứng nếu dùng cho hồ sơ mời thầu hoặc đánh giá nhà thầu.",
            "Các thuật ngữ và tên đơn vị sẽ được chuẩn hóa theo thông tin chính thức của Chủ đầu tư trong bản hoàn thiện.",
        ],
    )

    doc.core_properties.title = "Hồ sơ yêu cầu kỹ thuật - Sàn giao dịch Khoa học và Công nghệ"
    doc.core_properties.subject = "Bản sườn theo góc nhìn Chủ đầu tư"
    doc.core_properties.author = "Codex hỗ trợ tổng hợp"
    OUT.parent.mkdir(parents=True, exist_ok=True)
    doc.save(str(OUT))
    print(str(OUT).encode("ascii", errors="backslashreplace").decode("ascii"))


if __name__ == "__main__":
    build()
