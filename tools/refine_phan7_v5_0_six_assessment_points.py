from copy import deepcopy
from pathlib import Path

from docx import Document
from docx.shared import Inches
from docx.text.paragraph import Paragraph


SRC = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.9_chot_noi_dung_phan_7.docx"
)
DST = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v5.0_bo_sung_6_diem_tham_dinh.docx"
)


def set_cell(cell, text: str, bold: bool = False) -> None:
    cell.text = text
    for p in cell.paragraphs:
        for r in p.runs:
            r.bold = bold


def replace_runs(paragraph, replacements):
    for run in paragraph.runs:
        if not run.text:
            continue
        new_text = run.text
        for old, new in replacements.items():
            new_text = new_text.replace(old, new)
        if new_text != run.text:
            run.text = new_text


def insert_paragraph_after(paragraph: Paragraph, text: str = "", style=None, bold=False) -> Paragraph:
    new_p = deepcopy(paragraph._p)
    paragraph._p.addnext(new_p)
    new_para = Paragraph(new_p, paragraph._parent)
    new_para.clear()
    if style is not None:
        new_para.style = style
    if text:
        run = new_para.add_run(text)
        run.bold = bold
    return new_para


def insert_table_after(document: Document, paragraph: Paragraph, data, style="Table Grid"):
    table = document.add_table(rows=len(data), cols=len(data[0]))
    table.style = style
    for i, row in enumerate(data):
        for j, value in enumerate(row):
            set_cell(table.cell(i, j), value, bold=(i == 0))
    paragraph._p.addnext(table._tbl)
    return table


doc = Document(SRC)
normal = doc.styles["Normal"]


# 1. Bổ sung QĐ 3090/QĐ-BKHCN vào Mục II và căn cứ kiến trúc.
standards = doc.tables[2]
row = standards.add_row()
values = [
    "18",
    "Khung kiến trúc tổng thể quốc gia số",
    "Nguyên tắc tổ chức kiến trúc tổng thể, liên thông, chia sẻ dữ liệu, quản trị dữ liệu, sử dụng hạ tầng số và đánh giá tuân thủ kiến trúc đối với hệ thống số.",
    "Quyết định 3090/QĐ-BKHCN ngày 08/10/2025 của Bộ Khoa học và Công nghệ ban hành Khung kiến trúc tổng thể quốc gia số.",
    "Bắt buộc theo phạm vi",
]
for i, value in enumerate(values):
    set_cell(row.cells[i], value)

standards.cell(2, 3).text = (
    standards.cell(2, 3).text
    + " Quyết định 3090/QĐ-BKHCN ngày 08/10/2025 ban hành Khung kiến trúc tổng thể quốc gia số."
)


# 2. Viết lại Chương III thành phân tích tuân thủ thực sự.
chapter_replacements = {
    "Phương án kiến trúc được lựa chọn vì phù hợp nguyên tắc phát triển nền tảng số chuyên ngành: dữ liệu được quản lý tập trung, các chức năng và tiện ích của Hệ thống được cung cấp qua kênh số, các phân hệ có khả năng tích hợp và mở rộng. Ưu điểm của cách tiếp cận này là Chủ đầu tư có thể khai thác dữ liệu phục vụ điều hành, đồng thời giảm phụ thuộc vào từng công cụ riêng lẻ của đơn vị cung cấp dịch vụ.": (
        "Căn cứ Quyết định 3090/QĐ-BKHCN ngày 08/10/2025 ban hành Khung kiến trúc tổng thể quốc gia số, Chương này phân tích mức độ tuân thủ của phương án thiết kế đối với các nguyên tắc kiến trúc số, quản trị dựa trên dữ liệu, liên thông, chia sẻ dữ liệu, an toàn thông tin và vận hành dịch vụ. Việc phân tích không chỉ khẳng định tính phù hợp mà còn chỉ ra cách các yêu cầu của Khung được chuyển hóa thành yêu cầu thiết kế, tiêu chí kỹ thuật và điều kiện nghiệm thu."
    ),
    "Phương án thuê dịch vụ CNTT cho Sàn giao dịch Khoa học và Công nghệ phù hợp định hướng phát triển nền tảng số chuyên ngành, khai thác dữ liệu phục vụ quản lý nhà nước, thúc đẩy tương tác số giữa cơ quan quản lý, doanh nghiệp, nhà cung cấp, chuyên gia và người dân. Thiết kế đề xuất tổ chức hệ thống theo các lớp kiến trúc sau:": (
        "Hệ thống Sàn giao dịch Khoa học và Công nghệ được xem là nền tảng số chuyên ngành phục vụ kết nối cung - cầu, công bố thông tin, quản lý giao dịch và hỗ trợ chuyển giao công nghệ. Vì vậy, thiết kế cần chứng minh khả năng tổ chức theo lớp kiến trúc, quản lý dữ liệu tập trung, tích hợp có kiểm soát, bảo đảm an toàn thông tin và vận hành được theo mức chất lượng dịch vụ."
    ),
    "Tính phù hợp: Dự án tạo ra nền tảng số chuyên ngành phục vụ kết nối cung - cầu khoa học và công nghệ.": (
        "Phạm vi tuân thủ: thiết kế tuân thủ ở mức yêu cầu đầu ra của hệ thống số chuyên ngành, không sao chép nguyên mẫu kiến trúc của một nhà cung cấp cụ thể."
    ),
    "Tính liên thông: Thiết kế ưu tiên giao diện lập trình ứng dụng (API) và định dạng dữ liệu phổ biến, thuận lợi kết nối với hệ thống dùng chung khi có yêu cầu.": (
        "Nguyên tắc dữ liệu: dữ liệu được tổ chức có cấu trúc, phân quyền, có nhật ký, có khả năng trích xuất và chia sẻ theo phạm vi được phê duyệt."
    ),
    "Tính dữ liệu: Dữ liệu được xem là tài sản của Chủ đầu tư, được quản lý, bảo vệ, sao lưu và khai thác có kiểm soát.": (
        "Nguyên tắc tích hợp: các kết nối được thực hiện thông qua giao diện lập trình ứng dụng (API), định dạng dữ liệu phổ biến hoặc cơ chế tương đương, có xác thực, phân quyền, nhật ký và khả năng thay thế."
    ),
    "Tính cạnh tranh: Thiết kế không chỉ định sản phẩm cụ thể; cho phép nhiều phương án công nghệ hoặc dịch vụ tương đương tham gia.": (
        "Nguyên tắc cạnh tranh: tài liệu không chỉ định sản phẩm, nhãn hiệu, công nghệ độc quyền hoặc nhà cung cấp cụ thể; các thông số khảo sát chỉ là cơ sở tham chiếu chi phí và phải được đánh giá theo năng lực đầu ra."
    ),
}
for p in doc.paragraphs:
    replace_runs(p, chapter_replacements)

arch = doc.tables[3]
arch_data = [
    [
        "Lớp kiến trúc",
        "Phân tích tuân thủ và định hướng thiết kế",
    ],
    [
        "Lớp người dùng và kênh truy cập",
        "Tuân thủ nguyên tắc cung cấp dịch vụ số qua kênh truy cập thống nhất, bảo mật và thuận tiện. Thiết kế hỗ trợ khách truy cập, người mua, nhà cung cấp, chuyên gia, cán bộ nghiệp vụ, quản trị hệ thống và lãnh đạo; mỗi nhóm được phân quyền theo vai trò, chức năng và phạm vi dữ liệu.",
    ],
    [
        "Lớp nghiệp vụ/ứng dụng",
        "Tuân thủ nguyên tắc tổ chức hệ thống theo phân hệ, cấu hình được quy trình và không phụ thuộc vào công cụ đơn lẻ. Mười hai nhóm chức năng được tổ chức thành các phân hệ quản trị, nhà cung cấp, sản phẩm/công nghệ, sự kiện, người mua, giao dịch, cổng thông tin, tích hợp, trí tuệ nhân tạo và báo cáo.",
    ],
    [
        "Lớp dữ liệu và quản trị dữ liệu",
        "Tuân thủ định hướng quản trị dựa trên dữ liệu: dữ liệu được quản lý tập trung, có cấu trúc, phân quyền, lưu vết, sao lưu, khôi phục, trích xuất và bàn giao. Thiết kế phân biệt rõ dữ liệu do Chủ đầu tư quản lý với tài sản kỹ thuật nội bộ của đơn vị cung cấp dịch vụ.",
    ],
    [
        "Lớp tích hợp, liên thông",
        "Tuân thủ nguyên tắc mở và liên thông có kiểm soát: kết nối thư điện tử (Email), tin nhắn ngắn (SMS), chữ ký số, hợp đồng điện tử, tham quan thực tế ảo 360 độ (VR360), giao diện lập trình ứng dụng (API) và hệ thống khác khi có yêu cầu; mọi kết nối phải có xác thực, phân quyền, nhật ký và cơ chế xử lý lỗi.",
    ],
    [
        "Lớp an toàn thông tin và bảo vệ dữ liệu cá nhân",
        "Tuân thủ yêu cầu bảo đảm an toàn theo lớp: xác thực, phân quyền, mã hóa đường truyền, nhật ký, giám sát, sao lưu/khôi phục, kiểm soát tải tệp và bảo vệ dữ liệu cá nhân. Các yêu cầu này là tiêu chí nghiệm thu, không chỉ là khuyến nghị kỹ thuật.",
    ],
    [
        "Lớp hạ tầng và vận hành dịch vụ",
        "Tuân thủ nguyên tắc đánh giá theo năng lực đầu ra. Hạ tầng có thể là vật lý, ảo hóa, điện toán đám mây hoặc mô hình tương đương, miễn đáp ứng quy mô người dùng, truy cập đồng thời, dung lượng khả dụng, khả năng mở rộng, sao lưu, giám sát và SLA.",
    ],
    [
        "Lớp quản trị, giám sát và đo lường",
        "Tuân thủ yêu cầu quản trị và đo lường: có nhật ký vận hành, báo cáo SLA, phân loại sự cố, thời gian tiếp nhận/khôi phục, báo cáo hỗ trợ, cơ chế nghiệm thu và kế hoạch chuyển tiếp khi kết thúc dịch vụ.",
    ],
]
for r, rowdata in enumerate(arch_data):
    for c, value in enumerate(rowdata):
        set_cell(arch.cell(r, c), value, bold=(r == 0))

insert_after = doc.paragraphs[83]
insert_paragraph_after(
    insert_after,
    "Kết luận tuân thủ Chương III: Phương án thiết kế đáp ứng định hướng của Khung kiến trúc tổng thể quốc gia số ở mức thiết kế đầu ra: lấy dữ liệu làm trung tâm, tích hợp mở có kiểm soát, bảo đảm an toàn thông tin, vận hành theo SLA và không khóa vào một nền tảng hoặc nhà cung cấp cụ thể. Các nội dung này là cơ sở để Chương IV phân tích lựa chọn phương án kỹ thuật và Chương V thuyết minh giải pháp kỹ thuật chi tiết.",
    normal,
)


# 3. Xử lý triệt để cấu hình khảo sát 02 server/8 core/32GB/500GB.
server_replacements = {
    "Theo số liệu đầu vào và báo giá khảo sát phục vụ lập thiết kế, phương án hạ tầng tham chiếu có thể được cấu hình tương đương 02 máy chủ, mỗi máy có dung lượng lưu trữ SSD tối thiểu 500GB, tương ứng tổng dung lượng vật lý danh nghĩa 1TB. Dung lượng khai thác hữu dụng phụ thuộc mô hình phân bổ, dự phòng, nhân bản dữ liệu, hệ điều hành, cơ sở dữ liệu và phương án sao lưu.": (
        "Thông số khảo sát giá về 02 máy chủ, mỗi máy có lưu trữ SSD 500GB chỉ là dữ liệu đầu vào phục vụ ước lượng chi phí và không được xem là yêu cầu cấu hình bắt buộc. Yêu cầu thiết kế được xác định theo dung lượng khả dụng, khả năng mở rộng, cơ chế sao lưu/khôi phục, phân bổ dữ liệu và SLA. Đơn vị cung cấp dịch vụ phải chứng minh mô hình tài nguyên đáp ứng nhu cầu vận hành và tăng trưởng dữ liệu trong thời gian thuê."
    ),
    "Căn cứ số liệu báo giá khảo sát phục vụ lập thiết kế; đơn vị cung cấp dịch vụ phải chứng minh dung lượng khả dụng đáp ứng nhu cầu vận hành, dữ liệu hình ảnh, video, hợp đồng, nhật ký và tăng trưởng dữ liệu trong 05 năm.": (
        "Không dùng cấu hình khảo sát làm tiêu chí loại hoặc tiêu chí khóa công nghệ; việc nghiệm thu căn cứ dung lượng khả dụng, báo cáo giám sát tài nguyên, kết quả sao lưu/khôi phục và khả năng mở rộng được chứng minh."
    ),
}
for p in doc.paragraphs:
    replace_runs(p, server_replacements)
for tbl in doc.tables:
    for row in tbl.rows:
        for cell in row.cells:
            for p in cell.paragraphs:
                replace_runs(p, server_replacements)

service_table = doc.tables[13]
for row in service_table.rows:
    if row.cells[1].text.strip() == "Tài nguyên tính toán, lưu trữ và đường truyền":
        row.cells[2].text = (
            "Yêu cầu thiết kế đối với hạ tầng được xác định theo năng lực đầu ra: đáp ứng quy mô tài khoản, truy cập đồng thời, dung lượng khả dụng, sao lưu/khôi phục, giám sát, an toàn thông tin, mở rộng và SLA. Thông số khảo sát giá như 02 máy chủ, 8 lõi xử lý, RAM 32GB, SSD 500GB/máy và 01 địa chỉ IP chỉ là cơ sở tham chiếu chi phí, không phải cấu hình bắt buộc."
        )
        row.cells[3].text = (
            "Thuộc gói thuê hạ tầng 05 năm phục vụ cài đặt, vận hành và lưu trữ dữ liệu của Hệ thống; đơn vị cung cấp dịch vụ được đề xuất hạ tầng vật lý, ảo hóa, điện toán đám mây hoặc mô hình tương đương."
        )
        row.cells[4].text = (
            "Chứng minh năng lực tài nguyên, dung lượng khả dụng, phương án dự phòng, sao lưu, khôi phục, giám sát, cảnh báo, an toàn thông tin và báo cáo vận hành theo SLA."
        )
        row.cells[5].text = (
            "Không sử dụng cấu hình khảo sát làm tiêu chí khóa công nghệ; đánh giá theo năng lực đầu ra, khả năng mở rộng và tổng chi phí thuê dịch vụ được phê duyệt."
        )


# 4. Sửa quyền sở hữu/quản lý/khai thác dữ liệu.
data_replacements = {
    "Dữ liệu phát sinh trong thời gian thuê là dữ liệu phục vụ quản lý của Chủ đầu tư.": (
        "Dữ liệu phát sinh trong thời gian thuê thuộc phạm vi quản lý, khai thác và quyết định sử dụng của Chủ đầu tư theo quy định pháp luật và hợp đồng."
    ),
    "Yêu cầu quyền truy cập, trích xuất, sao lưu, bàn giao dữ liệu rõ ràng trong hợp đồng dịch vụ.": (
        "Yêu cầu quyền quản lý, truy cập, khai thác, trích xuất, sao lưu và bàn giao dữ liệu rõ ràng; đơn vị cung cấp dịch vụ không được xác lập quyền riêng đối với dữ liệu của Chủ đầu tư."
    ),
    "Dữ liệu thuộc Chủ đầu tư và người dùng, gồm dữ liệu cá nhân và dữ liệu giao dịch.": (
        "Dữ liệu do Chủ đầu tư quản lý gồm dữ liệu nghiệp vụ, dữ liệu giao dịch và dữ liệu cá nhân được xử lý theo quy định pháp luật."
    ),
    "Nhóm ràng buộc này tập trung vào dữ liệu và trách nhiệm dịch vụ, bao gồm quyền sở hữu và quyền khai thác dữ liệu của Chủ đầu tư; thời gian lưu giữ dữ liệu; khả năng trích xuất dữ liệu; trách nhiệm sao lưu, khôi phục; trách nhiệm bảo vệ dữ liệu cá nhân; cơ chế bàn giao khi kết thúc thuê dịch vụ và khả năng duy trì hoạt động khi thay đổi đơn vị cung cấp. Cách trình bày này phân biệt với mục 1, vốn chỉ xem xét ràng buộc môi trường và nền tảng dịch vụ.": (
        "Nhóm ràng buộc này tập trung vào dữ liệu và trách nhiệm dịch vụ, bao gồm quyền quản lý, khai thác, trích xuất, sao lưu và nhận bàn giao dữ liệu của Chủ đầu tư; thời gian lưu giữ dữ liệu; trách nhiệm khôi phục; trách nhiệm bảo vệ dữ liệu cá nhân; cơ chế bàn giao khi kết thúc thuê dịch vụ và khả năng duy trì hoạt động khi thay đổi đơn vị cung cấp. Thiết kế không quy định theo hướng chuyển giao quyền sở hữu tài sản kỹ thuật nội bộ của đơn vị cung cấp dịch vụ, nhưng phải bảo đảm Chủ đầu tư không bị phụ thuộc vào nhà cung cấp trong việc quản lý và khai thác dữ liệu."
    ),
    "Cơ sở lựa chọn là dữ liệu của Sàn giao dịch có giá trị quản lý lâu dài, bao gồm hồ sơ nhà cung cấp, sản phẩm/công nghệ, giao dịch, báo cáo và nhật ký vận hành. So với phương án chỉ quy định quyền sử dụng phần mềm, việc ràng buộc rõ quyền khai thác, trích xuất, sao lưu, bàn giao và duy trì dữ liệu giúp Chủ đầu tư giảm rủi ro phụ thuộc nhà cung cấp và bảo đảm khả năng chuyển tiếp dịch vụ khi cần.": (
        "Cơ sở lựa chọn là dữ liệu của Sàn giao dịch có giá trị quản lý lâu dài, bao gồm hồ sơ nhà cung cấp, sản phẩm/công nghệ, giao dịch, báo cáo và nhật ký vận hành. So với phương án chỉ quy định quyền sử dụng phần mềm, việc ràng buộc rõ quyền quản lý, khai thác, trích xuất, sao lưu, bàn giao và duy trì dữ liệu giúp Chủ đầu tư giảm rủi ro phụ thuộc nhà cung cấp và bảo đảm khả năng chuyển tiếp dịch vụ khi cần."
    ),
    "Chuyển giao được hiểu là chuyển giao tri thức sử dụng, quản trị, giám sát chất lượng dịch vụ, tài liệu cấu hình nghiệp vụ, dữ liệu và quyền khai thác theo hợp đồng.": (
        "Chuyển giao được hiểu là chuyển giao tri thức sử dụng, quản trị, giám sát chất lượng dịch vụ, tài liệu cấu hình nghiệp vụ, dữ liệu và quyền quản lý/khai thác dữ liệu theo hợp đồng."
    ),
}
for p in doc.paragraphs:
    replace_runs(p, data_replacements)
for tbl in doc.tables:
    for row in tbl.rows:
        for cell in row.cells:
            for p in cell.paragraphs:
                replace_runs(p, data_replacements)


# 5. Hoàn thiện SLA M1-M4 và điều kiện đo 3s/5s.
sla = doc.tables[5]
for row in sla.rows:
    key = row.cells[0].text.strip()
    if key == "Thời gian phản hồi chức năng thông thường":
        row.cells[2].text = (
            "Đo trong điều kiện vận hành bình thường, dữ liệu kiểm thử đại diện, đường truyền ổn định từ điểm đo thống nhất; không tính thời gian xử lý báo cáo lớn, tác vụ nền, tải/xử lý tệp lớn hoặc thời gian chờ dịch vụ bên thứ ba. Kết quả căn cứ nhật ký hệ thống, công cụ giám sát hoặc biên bản kiểm thử."
        )
    elif key == "Thời gian phản hồi kết nối tích hợp":
        row.cells[2].text = (
            "Đo tại điểm kết nối do Hệ thống kiểm soát, không bao gồm thời gian xử lý nội bộ của dịch vụ bên thứ ba ngoài phạm vi quản lý. Khi dịch vụ ngoài chậm/lỗi, Hệ thống phải ghi nhận nhật ký, trạng thái lỗi, thời điểm gửi/nhận và cơ chế thử lại hoặc thông báo lỗi."
        )
    elif key == "Tiếp nhận sự cố":
        row.cells[1].text = (
            "Thời gian tiếp nhận được xác định theo phân loại Mức 1-Mức 4 tại bảng phân loại sự cố; Mức 1 phải được tiếp nhận không quá 30 phút kể từ khi Chủ đầu tư thông báo qua kênh hỗ trợ thống nhất hoặc từ khi hệ thống giám sát phát hiện."
        )
        row.cells[2].text = "Theo phiếu yêu cầu hỗ trợ, nhật ký tiếp nhận, cảnh báo giám sát và biên bản xử lý."
    elif key == "Khắc phục và khôi phục dịch vụ":
        row.cells[1].text = (
            "Thời gian khắc phục/khôi phục được áp dụng theo phân loại Mức 1-Mức 4. Đối với Mức 1, dịch vụ trọng yếu phải được khôi phục trong thời gian không quá 4 giờ kể từ thời điểm sự cố được xác nhận, trừ trường hợp bất khả kháng hoặc trường hợp khác được quy định trong hợp đồng."
        )
        row.cells[2].text = (
            "Áp dụng theo bảng phân loại sự cố, nhật ký tiếp nhận, biên bản xác nhận sự cố, báo cáo khôi phục dịch vụ và xác nhận của Chủ đầu tư."
        )

incident = doc.tables[6]
incident.add_column(Inches(1.6))
incident.add_column(Inches(2.0))
incident_data = [
    [
        "Mức sự cố",
        "Tiêu chí phân loại",
        "Thời gian tiếp nhận/phản hồi",
        "Mục tiêu xử lý/khôi phục",
    ],
    [
        "Mức 1 - Nghiêm trọng",
        "Hệ thống ngừng hoạt động diện rộng; mất khả năng truy cập chức năng trọng yếu; có nguy cơ mất dữ liệu hoặc sự cố an toàn thông tin nghiêm trọng.",
        "Tiếp nhận không quá 30 phút; phản hồi tình trạng xử lý ban đầu ngay sau khi phân công xử lý.",
        "Khôi phục dịch vụ trọng yếu không quá 4 giờ kể từ thời điểm sự cố được xác nhận, trừ trường hợp bất khả kháng hoặc trường hợp khác được quy định trong hợp đồng.",
    ],
    [
        "Mức 2 - Cao",
        "Một phân hệ quan trọng bị gián đoạn, ảnh hưởng nhiều người dùng nhưng Hệ thống vẫn còn khả năng khai thác một phần.",
        "Tiếp nhận không quá 1 giờ làm việc kể từ khi ghi nhận yêu cầu.",
        "Mục tiêu khắc phục không quá 8 giờ làm việc hoặc theo mức SLA chi tiết được quy định trong hợp đồng.",
    ],
    [
        "Mức 3 - Thông thường",
        "Lỗi cục bộ, có phương án xử lý tạm thời, không làm gián đoạn chức năng trọng yếu.",
        "Tiếp nhận không quá 4 giờ làm việc.",
        "Mục tiêu xử lý không quá 2 ngày làm việc hoặc theo kế hoạch được Chủ đầu tư thống nhất.",
    ],
    [
        "Mức 4 - Yêu cầu hỗ trợ",
        "Yêu cầu cấu hình, hướng dẫn, hiệu chỉnh nhỏ hoặc hỗ trợ sử dụng; không phải sự cố gây gián đoạn dịch vụ.",
        "Tiếp nhận không quá 1 ngày làm việc.",
        "Thực hiện theo kế hoạch hỗ trợ/thay đổi được thống nhất giữa các bên.",
    ],
]
for r, rowdata in enumerate(incident_data):
    for c, value in enumerate(rowdata):
        set_cell(incident.cell(r, c), value, bold=(r == 0))

clarification_anchor = next(
    p
    for p in doc.paragraphs
    if p.text.startswith("Các chỉ tiêu về tỷ lệ sẵn sàng và thời gian khôi phục dịch vụ")
)
insert_paragraph_after(
    clarification_anchor,
    "Điều kiện đo thời gian phản hồi 3 giây/5 giây phải được thống nhất trong kịch bản kiểm thử hoặc phương án giám sát, bao gồm phạm vi chức năng đo, dữ liệu kiểm thử, điểm đo, thời điểm đo, số lượng mẫu yêu cầu và các trường hợp loại trừ. Các kết quả đo phải có nhật ký hoặc báo cáo kỹ thuật để làm căn cứ nghiệm thu.",
    normal,
)


# 6. Bổ sung kế hoạch kết thúc/chuyển tiếp dịch vụ.
transition_anchor = doc.paragraphs[197]
p = insert_paragraph_after(transition_anchor, "5. Kế hoạch kết thúc và chuyển tiếp dịch vụ:", doc.styles["Heading 3"])
transition_rows = [
    ["Nội dung", "Yêu cầu thực hiện", "Sản phẩm/điều kiện nghiệm thu"],
    [
        "Thông báo và lập kế hoạch chuyển tiếp",
        "Trước khi hết hạn hợp đồng hoặc khi có yêu cầu thay đổi đơn vị cung cấp dịch vụ, các bên phải thống nhất kế hoạch chuyển tiếp, phạm vi dữ liệu, thời điểm bàn giao, trách nhiệm hỗ trợ và đầu mối phối hợp.",
        "Kế hoạch chuyển tiếp được Chủ đầu tư xác nhận.",
    ],
    [
        "Đóng băng thay đổi và đối soát dữ liệu",
        "Xác định thời điểm chốt dữ liệu, hạn chế thay đổi nghiệp vụ trong giai đoạn bàn giao, đối soát số lượng hồ sơ, tài khoản, sản phẩm/công nghệ, giao dịch, hợp đồng, báo cáo và nhật ký.",
        "Biên bản đối soát dữ liệu và danh mục dữ liệu bàn giao.",
    ],
    [
        "Kết xuất và bàn giao dữ liệu",
        "Dữ liệu phải được kết xuất theo định dạng phổ biến, có cấu trúc, đọc được bằng công cụ thông dụng hoặc đặc tả kèm theo; bao gồm dữ liệu nghiệp vụ, tài liệu, tệp đính kèm, báo cáo, nhật ký cần bàn giao theo hợp đồng.",
        "Tệp dữ liệu/kho dữ liệu bàn giao, tài liệu mô tả cấu trúc dữ liệu và biên bản bàn giao.",
    ],
    [
        "Hỗ trợ chuyển tiếp kỹ thuật",
        "Đơn vị cung cấp dịch vụ phải hỗ trợ giải thích cấu trúc dữ liệu, quy trình khôi phục, tài liệu tích hợp và các điểm cấu hình cần thiết trong thời gian chuyển tiếp được quy định trong hợp đồng.",
        "Biên bản hỗ trợ chuyển tiếp, danh sách vấn đề tồn tại và xác nhận hoàn tất.",
    ],
    [
        "Kết thúc truy cập và bảo vệ dữ liệu",
        "Sau khi hoàn tất bàn giao, quyền truy cập của đơn vị cung cấp dịch vụ phải được xử lý theo hợp đồng; dữ liệu còn lưu trên môi trường của đơn vị cung cấp dịch vụ phải được bảo vệ, lưu giữ hoặc xóa theo thỏa thuận và quy định pháp luật.",
        "Biên bản xác nhận xử lý tài khoản, quyền truy cập và dữ liệu sau chuyển tiếp.",
    ],
]
insert_table_after(doc, p, transition_rows)


# 7. Làm rõ trách nhiệm đầu cuối với dịch vụ tích hợp.
intro_p = doc.paragraphs[164]
insert_paragraph_after(
    intro_p,
    "Đối với các dịch vụ tích hợp có sử dụng nền tảng hoặc nhà cung cấp bên thứ ba, đơn vị cung cấp dịch vụ CNTT là đầu mối chịu trách nhiệm đầu cuối trong phạm vi gói thuê đã được phê duyệt: cấu hình, tích hợp, theo dõi trạng thái, ghi nhật ký, phối hợp xử lý lỗi, đối soát sản lượng và báo cáo cho Chủ đầu tư. Trường hợp lỗi phát sinh từ nền tảng bên thứ ba, đơn vị cung cấp dịch vụ CNTT vẫn phải tiếp nhận, phân loại, phối hợp xử lý và cung cấp bằng chứng kỹ thuật để xác định nguyên nhân, trừ phần nghĩa vụ tài chính hoặc điều kiện sử dụng đã được tách riêng trong HSMT/hợp đồng.",
    normal,
)

for row in service_table.rows:
    item = row.cells[1].text.strip()
    if item in {
        "Dịch vụ thư điện tử",
        "Tin nhắn thương hiệu SMS Brandname",
        "Giải pháp thực tế ảo VR360",
        "Chữ ký số",
        "Hợp đồng điện tử",
        "Thông báo tự động qua web/email/SMS",
        "Trò chuyện trực tuyến, cuộc gọi video, phát trực tuyến",
        "Ứng dụng trí tuệ nhân tạo",
        "Kết nối giao diện lập trình ứng dụng (API)",
    }:
        row.cells[4].text = row.cells[4].text + (
            " Đơn vị cung cấp dịch vụ CNTT là đầu mối chịu trách nhiệm đầu cuối trong phạm vi tích hợp đã được phê duyệt, bao gồm tiếp nhận lỗi, phối hợp với bên thứ ba, ghi nhận nhật ký và báo cáo kết quả xử lý cho Chủ đầu tư."
        )

roles = doc.tables[14]
roles.cell(4, 1).text = (
    "Cung cấp, cấu hình, vận hành, bảo trì, hỗ trợ, bảo đảm chất lượng dịch vụ theo hợp đồng và chịu trách nhiệm đầu cuối đối với các dịch vụ tích hợp thuộc phạm vi gói thuê, kể cả trường hợp phải phối hợp với nhà cung cấp bên thứ ba."
)
roles.cell(6, 1).text = (
    "Cung cấp dịch vụ thư điện tử (Email), tin nhắn ngắn (SMS), chữ ký số, trí tuệ nhân tạo (AI), tham quan thực tế ảo 360 độ (VR360) hoặc giao diện lập trình ứng dụng (API) theo phạm vi triển khai; phối hợp với đơn vị cung cấp dịch vụ CNTT trong xử lý lỗi, đối soát sản lượng và cung cấp bằng chứng kỹ thuật khi cần."
)

acceptance = doc.tables[15]
acceptance.cell(3, 1).text = (
    "Kiểm tra thư điện tử (Email), tin nhắn thương hiệu SMS Brandname, VR360, chữ ký số, hợp đồng điện tử, thông báo tự động và các giao diện lập trình ứng dụng (API) theo phạm vi đã được phê duyệt; đối chiếu hạn mức/sản lượng tích hợp gồm 10 điểm VR360, 3.600 SMS/năm, 60 địa chỉ thư điện tử/năm, 2.000 hợp đồng điện tử/năm, điều kiện riêng về chữ ký số, nhật ký tích hợp, trạng thái xử lý lỗi và trách nhiệm phối hợp đầu cuối."
)

handover = next(
    tbl
    for tbl in doc.tables
    if len(tbl.rows) > 0 and tbl.cell(0, 1).text.strip() == "Tài liệu/sản phẩm"
)
handover.cell(9, 2).text = (
    "Dữ liệu phát sinh trong phạm vi dịch vụ, kết xuất theo định dạng thống nhất khi Chủ đầu tư yêu cầu, khi chuyển tiếp nhà cung cấp hoặc khi kết thúc dịch vụ; kèm tài liệu mô tả cấu trúc dữ liệu, phạm vi dữ liệu, thời điểm chốt dữ liệu và biên bản đối soát."
)


doc.save(DST)
print(DST)
