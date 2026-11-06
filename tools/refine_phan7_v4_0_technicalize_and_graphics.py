from pathlib import Path
import textwrap

from docx import Document
from docx.oxml import OxmlElement
from docx.shared import Inches
from PIL import Image, ImageDraw, ImageFont


BASE = Path(r"D:\2026\CanTho\docs\ho so ky thuat")
SRC = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.9.2_bo_sung_vai_tro_so_do_thiet_ke.docx"
DST = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.0_ky_thuat_hoa_so_do_chuyen_nghiep.docx"
IMG_DIR = BASE / "so_do_phan7_v4_0"


FONT = Path(r"C:\Windows\Fonts\arial.ttf")
FONT_BOLD = Path(r"C:\Windows\Fonts\arialbd.ttf")


def font(size=22, bold=False):
    return ImageFont.truetype(str(FONT_BOLD if bold else FONT), size)


def wrap_text(text, width):
    return "\n".join(textwrap.wrap(text, width=width, break_long_words=False))


def draw_box(draw, xy, title, body="", fill="#F7FAFC", outline="#2F5597", title_fill="#D9EAF7"):
    x1, y1, x2, y2 = xy
    draw.rounded_rectangle(xy, radius=16, fill=fill, outline=outline, width=3)
    draw.rounded_rectangle((x1, y1, x2, y1 + 42), radius=16, fill=title_fill, outline=outline, width=0)
    draw.text((x1 + 14, y1 + 10), title, fill="#17365D", font=font(20, True))
    if body:
        draw.multiline_text((x1 + 14, y1 + 58), wrap_text(body, 31), fill="#1F2937", font=font(17), spacing=5)


def arrow(draw, start, end, color="#4B5563"):
    draw.line((start, end), fill=color, width=4)
    ex, ey = end
    sx, sy = start
    if abs(ex - sx) > abs(ey - sy):
        direction = 1 if ex > sx else -1
        pts = [(ex, ey), (ex - 14 * direction, ey - 8), (ex - 14 * direction, ey + 8)]
    else:
        direction = 1 if ey > sy else -1
        pts = [(ex, ey), (ex - 8, ey - 14 * direction), (ex + 8, ey - 14 * direction)]
    draw.polygon(pts, fill=color)


def save_architecture(path):
    img = Image.new("RGB", (1400, 900), "white")
    d = ImageDraw.Draw(img)
    d.text((40, 28), "Sơ đồ kiến trúc tổng thể đề xuất", fill="#111827", font=font(30, True))

    draw_box(d, (50, 105, 360, 245), "Người dùng", "Cơ quan quản lý\nNhà cung cấp\nNgười mua, chuyên gia")
    draw_box(d, (520, 105, 880, 245), "Lớp truy cập", "Cổng web bảo mật\nĐăng nhập, thông báo\nGiao diện thích ứng")
    draw_box(d, (520, 330, 880, 525), "Lớp ứng dụng nghiệp vụ", "12 nhóm chức năng\nQuy trình, trạng thái\nBáo cáo và thống kê")
    draw_box(d, (990, 330, 1335, 525), "Lớp tích hợp", "Email, SMS, ký số\nHợp đồng điện tử\nAI, API, nền tảng ngoài")
    draw_box(d, (520, 620, 880, 800), "Lớp dữ liệu", "Hồ sơ, sản phẩm\nSự kiện, giao dịch\nBáo cáo, nhật ký, sao lưu")
    draw_box(d, (80, 620, 405, 800), "An toàn thông tin", "Xác thực, phân quyền\nKiểm soát truy cập\nBảo vệ dữ liệu")
    draw_box(d, (990, 620, 1335, 800), "Vận hành giám sát", "Giám sát trạng thái\nCảnh báo, hỗ trợ\nChất lượng dịch vụ")

    arrow(d, (360, 175), (520, 175))
    arrow(d, (700, 245), (700, 330))
    arrow(d, (880, 425), (990, 425))
    arrow(d, (700, 525), (700, 620))
    arrow(d, (520, 710), (405, 710))
    arrow(d, (880, 710), (990, 710))
    img.save(path)


def save_layers(path):
    img = Image.new("RGB", (900, 1050), "white")
    d = ImageDraw.Draw(img)
    d.text((40, 30), "Mô hình phân lớp kiểm soát dịch vụ", fill="#111827", font=font(30, True))
    layers = [
        ("Lớp người dùng", "Cán bộ quản lý, nhà cung cấp, người mua, chuyên gia"),
        ("Lớp truy cập và trải nghiệm", "Web bảo mật, giao diện thích ứng, thông báo"),
        ("Lớp ứng dụng nghiệp vụ", "Các mô-đun chức năng và quy trình trạng thái"),
        ("Lớp dữ liệu - báo cáo", "Dữ liệu nghiệp vụ, chỉ mục tìm kiếm, báo cáo"),
        ("Lớp tích hợp dịch vụ", "Email, SMS, ký số, hợp đồng điện tử, AI, API"),
        ("Lớp an toàn thông tin và vận hành", "Phân quyền, nhật ký kiểm toán, sao lưu, giám sát"),
    ]
    y = 105
    prev_mid = None
    for title, body in layers:
        draw_box(d, (125, y, 775, y + 110), title, body, fill="#F8FBFF", outline="#4472C4", title_fill="#DDEBF7")
        mid = (450, y + 110)
        if prev_mid:
            arrow(d, prev_mid, (450, y))
        prev_mid = mid
        y += 155
    img.save(path)


def save_connection(path):
    img = Image.new("RGB", (1350, 850), "white")
    d = ImageDraw.Draw(img)
    d.text((40, 28), "Mô hình kết nối và liên thông dữ liệu", fill="#111827", font=font(30, True))
    draw_box(d, (45, 330, 340, 475), "Người dùng cuối", "Truy cập, tra cứu\nTương tác, giao dịch")
    draw_box(d, (475, 310, 820, 500), "Cổng web Sàn giao dịch", "Điều phối truy cập\nXác thực, phân quyền\nGhi nhận nhật ký")
    draw_box(d, (970, 80, 1295, 205), "Dịch vụ thông báo", "Email, SMS Brandname")
    draw_box(d, (970, 250, 1295, 375), "Ký số - hợp đồng", "Chữ ký số\nHợp đồng điện tử")
    draw_box(d, (970, 420, 1295, 545), "Dịch vụ AI", "Tư vấn, tìm kiếm\nGợi ý phù hợp")
    draw_box(d, (970, 590, 1295, 715), "API/nền tảng ngoài", "Kết nối hệ thống khác\nĐặc tả, xác thực")
    draw_box(d, (475, 620, 820, 760), "Kho dữ liệu - báo cáo", "Dữ liệu nghiệp vụ\nBáo cáo, nhật ký")

    arrow(d, (340, 400), (475, 400))
    arrow(d, (820, 400), (970, 145))
    arrow(d, (820, 400), (970, 310))
    arrow(d, (820, 400), (970, 485))
    arrow(d, (820, 400), (970, 650))
    arrow(d, (650, 500), (650, 620))
    img.save(path)


def save_deploy(path):
    img = Image.new("RGB", (1300, 760), "white")
    d = ImageDraw.Draw(img)
    d.text((40, 28), "Quy trình triển khai, kiểm tra và đưa dịch vụ vào vận hành", fill="#111827", font=font(29, True))
    steps = [
        ("1", "Thống nhất phạm vi,\nkế hoạch"),
        ("2", "Thiết lập môi trường\nvà cấu hình ban đầu"),
        ("3", "Khởi tạo danh mục,\ntài khoản, dữ liệu mẫu"),
        ("4", "Kiểm tra chức năng,\ndữ liệu, tích hợp, ATTT"),
        ("5", "Đào tạo, vận hành thử,\nghi nhận hiệu chỉnh"),
        ("6", "Nghiệm thu, bàn giao,\nkhai thác chính thức"),
    ]
    x, y = 55, 250
    last = None
    for num, body in steps:
        draw_box(d, (x, y, x + 180, y + 150), f"Bước {num}", body, fill="#F7FFF7", outline="#548235", title_fill="#E2F0D9")
        if last:
            arrow(d, (last + 180, y + 75), (x, y + 75), color="#548235")
        last = x
        x += 205
    img.save(path)


def set_para_text(paragraph, text):
    paragraph._p.clear_content()
    paragraph.add_run(text)


def insert_picture_after(paragraph, image_path, width=6.5):
    new_p = OxmlElement("w:p")
    paragraph._p.addnext(new_p)
    new_para = paragraph._parent.add_paragraph()
    new_para._p = new_p
    new_para.add_run().add_picture(str(image_path), width=Inches(width))
    return new_para


def delete_paragraph(paragraph):
    paragraph._element.getparent().remove(paragraph._element)


def replace_diagram_after_caption(doc, caption, image_path, width=6.5):
    for i, paragraph in enumerate(doc.paragraphs):
        if paragraph.text.strip() == caption:
            # Remove the ASCII diagram paragraph that follows the caption.
            if i + 1 < len(doc.paragraphs):
                delete_paragraph(doc.paragraphs[i + 1])
            insert_picture_after(paragraph, image_path, width)
            return
    raise RuntimeError(f"Caption not found: {caption}")


def technicalize_v1(doc):
    table = doc.tables[7]
    rows = {
        3: (
            "Sản phẩm và công nghệ được thiết kế như một hồ sơ dữ liệu lõi, có mã định danh, nhóm phân loại, bộ thuộc tính mô tả, tài liệu đính kèm, trạng thái pháp lý, trạng thái công bố và lịch sử cập nhật. Cấu trúc dữ liệu cần hỗ trợ phân loại theo lĩnh vực, nhóm sản phẩm, mức độ sẵn sàng chuyển giao và kênh trưng bày. Cơ chế duyệt, chỉ mục tìm kiếm và quản lý phiên bản giúp kiểm soát nguồn gốc thông tin, tránh công bố sai trạng thái và tạo dữ liệu ổn định cho tra cứu, phòng trưng bày, báo cáo và các kênh xúc tiến."
        ),
        4: (
            "Sự kiện - triển lãm được tổ chức theo mô hình vòng đời gồm khởi tạo, công bố, đăng ký, bố trí gian hàng, truyền thông, nhắc lịch, ghi nhận tham gia và tổng hợp kết quả. Mỗi sự kiện cần có trạng thái xử lý, mốc thời gian, nhóm đối tượng tham gia, dữ liệu gian hàng và chỉ số quan tâm. Cách thiết kế này giúp tách dữ liệu sự kiện với dữ liệu giao dịch thường xuyên, đồng thời vẫn liên kết được sản phẩm, nhà cung cấp và người mua phát sinh từ hoạt động xúc tiến."
        ),
        5: (
            "Người mua được quản lý theo mô hình hồ sơ nhu cầu thay vì chỉ là tài khoản truy cập. Mỗi nhu cầu tìm mua cần có nhóm công nghệ, tiêu chí kỹ thuật, mức độ ưu tiên, trạng thái xử lý, lịch sử trao đổi và liên kết với báo giá/giao dịch nếu phát sinh. Cấu trúc Buyer-Demand giúp hệ thống lưu vết quá trình làm rõ nhu cầu, hỗ trợ gợi ý nguồn cung phù hợp, theo dõi chất lượng phản hồi và hình thành dữ liệu phân tích xu hướng cầu công nghệ."
        ),
        7: (
            "Cổng thông tin được thiết kế tách lớp quản trị nội dung nội bộ và lớp công bố thông tin ra bên ngoài. Dữ liệu công khai cần có trạng thái biên tập, xem trước, phê duyệt và xuất bản; các danh mục, tin tức, sự kiện và sản phẩm được đưa vào chỉ mục tìm kiếm để phục vụ tra cứu nhanh. Định hướng kỹ thuật là tối ưu hiển thị, metadata, đường dẫn, hình ảnh/tài liệu công khai và cơ chế đăng ký nhận thông tin, qua đó nâng cao khả năng tiếp cận mà không làm ảnh hưởng dữ liệu nghiệp vụ bên trong."
        ),
        12: (
            "Báo cáo - thống kê được thiết kế như lớp khai thác dữ liệu quản trị, không chỉ là chức năng xuất tệp. Dữ liệu báo cáo cần xác định nguồn phát sinh, kỳ tổng hợp, chỉ tiêu đo lường, quyền xem và định dạng kết xuất. Các chỉ số về nhà cung cấp, sản phẩm, giao dịch, doanh thu, sự kiện và truy cập nên được tổ chức thành nhóm chỉ tiêu ổn định để phục vụ giám sát chất lượng dịch vụ, đánh giá hiệu quả vận hành và cung cấp số liệu cho lãnh đạo khi cần."
        ),
    }
    for row_idx, text in rows.items():
        table.rows[row_idx].cells[1].text = text

    table.rows[1].cells[1].text = (
        "Quản trị hệ thống là lớp kiểm soát nền, quyết định khả năng vận hành an toàn của toàn bộ dịch vụ. Thiết kế cần tập trung vào định danh người dùng, vai trò, quyền theo chức năng và dữ liệu, tham số vận hành, thông báo và nhật ký thao tác. Khi các quyền được quản lý tập trung và có khả năng truy vết, Chủ đầu tư có cơ sở kiểm soát trách nhiệm, xử lý sự cố và nghiệm thu mức độ tuân thủ của dịch vụ."
    )
    table.rows[6].cells[1].text = (
        "Giao dịch được thiết kế theo quy trình trạng thái để kiểm soát các bước từ yêu cầu báo giá, báo giá, đơn hàng, hợp đồng đến ghi nhận doanh thu, hoa hồng, thu chi và kết quả xử lý. Đặc thù giao dịch khoa học và công nghệ là thời gian kéo dài, nhiều chủ thể tham gia và thường cần bổ sung thông tin qua nhiều lượt trao đổi. Vì vậy, mô hình trạng thái, nhật ký xử lý và liên kết chứng từ là cơ sở kỹ thuật để giám sát tiến độ, truy xuất trách nhiệm và tổng hợp báo cáo theo từng giai đoạn."
    )
    table.rows[9].cells[1].text = (
        "Trí tuệ nhân tạo được đặt ở vai trò dịch vụ hỗ trợ, tách khỏi dữ liệu gốc và không thay thế quyết định nghiệp vụ. Thiết kế cần quy định phạm vi dữ liệu được sử dụng, cơ chế bật/tắt, lưu vết truy vấn, kiểm soát kết quả gợi ý và phương án chuyển về tìm kiếm thông thường khi dịch vụ ngoài gián đoạn. Cách tiếp cận này cho phép khai thác tìm kiếm ngữ nghĩa, tư vấn và gợi ý phù hợp nhưng vẫn giữ quyền kiểm soát dữ liệu, minh bạch trách nhiệm và an toàn vận hành."
    )
    table.rows[10].cells[1].text = (
        "Tích hợp dịch vụ và liên thông được đặt ở lớp kết nối có kiểm soát. Mỗi kết nối cần có mục đích sử dụng, phương thức xác thực, tham số cấu hình, nhật ký trao đổi, cơ chế xử lý lỗi và khả năng thay thế khi dịch vụ bên ngoài thay đổi. Nhờ tách lớp tích hợp khỏi nghiệp vụ lõi, hệ thống giảm phụ thuộc công nghệ, duy trì được khả năng mở rộng và vẫn kiểm soát được trách nhiệm giữa các bên khi phát sinh lỗi kết nối."
    )
    table.rows[11].cells[1].text = (
        "Hạ tầng triển khai được mô tả bằng năng lực dịch vụ cần đạt: tài nguyên tính toán, lưu trữ, sao lưu, giám sát, an toàn truy cập, khả năng mở rộng và khôi phục khi có sự cố. Thiết kế không khóa vào máy chủ, hệ điều hành hay hệ quản trị dữ liệu cụ thể; thay vào đó yêu cầu nhà cung cấp chứng minh năng lực đáp ứng, phương án duy trì chất lượng và khả năng bàn giao dữ liệu. Cách mô tả theo năng lực giúp bảo đảm cạnh tranh nhưng vẫn đủ căn cứ nghiệm thu."
    )


def reduce_repetition(doc):
    replacements = {
        "trong suốt thời gian thuê dịch vụ": "trong suốt thời gian khai thác",
        "phạm vi thuê dịch vụ": "phạm vi dịch vụ",
        "theo phạm vi thuê": "theo phạm vi hợp đồng",
        "theo phạm vi được phê duyệt": "theo phạm vi xác định",
        "được phê duyệt": "được xác định",
        "có cấu hình, phân quyền, ghi nhật ký": "có cơ chế quản trị và kiểm soát truy vết",
        "có cấu hình, nhật ký": "có cơ chế cấu hình và truy vết",
        "cấu hình, phân quyền, ghi nhật ký": "quản trị cấu hình, phân quyền và truy vết",
        "có xác thực, phân quyền, ghi nhật ký": "có xác thực, phân quyền và truy vết",
        "không chỉ định công nghệ cụ thể": "giữ trung lập về công nghệ",
        "không chỉ định nền tảng cụ thể": "không khóa vào nền tảng cụ thể",
        "Cơ sở lựa chọn": "Căn cứ thiết kế",
    }
    for paragraph in doc.paragraphs:
        text = paragraph.text
        for old, new in replacements.items():
            text = text.replace(old, new)
        if text != paragraph.text:
            set_para_text(paragraph, text)
    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                text = cell.text
                for old, new in replacements.items():
                    text = text.replace(old, new)
                if text != cell.text:
                    cell.text = text


def main():
    IMG_DIR.mkdir(exist_ok=True)
    images = {
        "arch": IMG_DIR / "hinh_7_1_kien_truc_tong_the.png",
        "layers": IMG_DIR / "hinh_7_2_mo_hinh_phan_lop.png",
        "connection": IMG_DIR / "hinh_7_3_mo_hinh_ket_noi.png",
        "deploy": IMG_DIR / "hinh_7_4_quy_trinh_trien_khai.png",
    }
    save_architecture(images["arch"])
    save_layers(images["layers"])
    save_connection(images["connection"])
    save_deploy(images["deploy"])

    doc = Document(SRC)
    replace_diagram_after_caption(doc, "Hình 7.1. Sơ đồ kiến trúc tổng thể đề xuất:", images["arch"], 6.6)
    replace_diagram_after_caption(doc, "Hình 7.2. Mô hình phân lớp kiểm soát dịch vụ:", images["layers"], 4.9)
    replace_diagram_after_caption(doc, "Hình 7.3. Mô hình kết nối và liên thông dữ liệu:", images["connection"], 6.6)
    replace_diagram_after_caption(doc, "Hình 7.4. Quy trình triển khai, kiểm tra và đưa dịch vụ vào vận hành:", images["deploy"], 6.6)

    technicalize_v1(doc)
    reduce_repetition(doc)
    doc.save(DST)
    print(DST)


if __name__ == "__main__":
    main()
