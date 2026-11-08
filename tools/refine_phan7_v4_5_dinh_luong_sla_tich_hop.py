from copy import deepcopy
from pathlib import Path

from docx import Document
from docx.shared import Pt
from docx.text.paragraph import Paragraph


SRC = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3 - 05072026_003.docx"
)
DST = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.5_bo_sung_dinh_luong_SLA_tich_hop.docx"
)


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


def insert_table_after(document: Document, paragraph: Paragraph, rows: int, cols: int, style="Table Grid"):
    table = document.add_table(rows=rows, cols=cols)
    if style:
        table.style = style
    paragraph._p.addnext(table._tbl)
    return table


def remove_table(table) -> None:
    el = table._tbl
    el.getparent().remove(el)


def set_cell(cell, text: str, bold=False) -> None:
    cell.text = ""
    p = cell.paragraphs[0]
    run = p.add_run(text)
    run.bold = bold
    run.font.size = Pt(9)


def format_table(table) -> None:
    for row_i, row in enumerate(table.rows):
        for cell in row.cells:
            for p in cell.paragraphs:
                for run in p.runs:
                    run.font.size = Pt(9)
                    if row_i == 0:
                        run.bold = True


doc = Document(SRC)
normal = doc.styles["Normal"]

# I.1. Bổ sung cơ sở lựa chọn hình thức thuê dịch vụ CNTT và quy ước thuật ngữ.
i1_anchor = next(
    p
    for p in doc.paragraphs
    if p.text.strip().startswith("Về nguyên tắc, thiết kế lựa chọn phương án thuê dịch vụ")
)

p = i1_anchor
for text, bold in [
    ("Cơ sở lựa chọn hình thức thuê dịch vụ công nghệ thông tin", True),
    (
        "Hình thức thuê dịch vụ công nghệ thông tin được lựa chọn trên cơ sở đặc điểm khai thác của Hệ thống Sàn giao dịch Khoa học và Công nghệ. Phạm vi dự án không chỉ bao gồm phần mềm ứng dụng mà còn gắn với hạ tầng vận hành, lưu trữ dữ liệu, giám sát, sao lưu, khôi phục, bảo trì, hỗ trợ kỹ thuật, an toàn thông tin và duy trì mức chất lượng dịch vụ trong suốt thời gian khai thác. Nếu thực hiện theo phương án đầu tư, mua sắm riêng lẻ từng thành phần, Chủ đầu tư phải đồng thời tổ chức quản lý nhiều cấu phần kỹ thuật, duy trì nguồn lực vận hành và xử lý các thay đổi công nghệ trong vòng đời hệ thống.",
        False,
    ),
    (
        "Bên cạnh đó, quy mô người dùng, dung lượng dữ liệu, nhu cầu tích hợp và yêu cầu kỹ thuật của Hệ thống có thể thay đổi theo quá trình vận hành thực tế. Việc lựa chọn hình thức thuê dịch vụ tạo điều kiện điều chỉnh, mở rộng tài nguyên và cập nhật giải pháp theo nhu cầu khai thác mà không phụ thuộc vào một cấu hình đầu tư cố định ngay từ đầu. Chủ đầu tư tập trung quản lý yêu cầu đầu ra, chất lượng dịch vụ, dữ liệu và kết quả nghiệm thu; đơn vị cung cấp dịch vụ chịu trách nhiệm đề xuất giải pháp kỹ thuật, tổ chức vận hành, bảo trì, hỗ trợ và thực hiện các cam kết theo hợp đồng.",
        False,
    ),
    (
        "So với phương án đầu tư hoặc mua sắm theo cấu hình cố định, hình thức thuê dịch vụ phù hợp hơn với yêu cầu duy trì vận hành liên tục, mở rộng linh hoạt, tích hợp nhiều dịch vụ bên ngoài và kiểm soát chất lượng theo kết quả đầu ra. Cách tiếp cận này đồng thời hạn chế nguy cơ phụ thuộc vào một công nghệ cụ thể, tạo điều kiện cạnh tranh trong lựa chọn nhà cung cấp và bảo đảm khả năng thay thế, chuyển tiếp dịch vụ khi cần thiết, với điều kiện quyền quản lý, khai thác, trích xuất và bàn giao dữ liệu của Chủ đầu tư được quy định rõ trong hồ sơ mời thầu và hợp đồng.",
        False,
    ),
    ("Quy ước sử dụng thuật ngữ", True),
    (
        "Trong phạm vi tài liệu này, “Hệ thống Sàn giao dịch Khoa học và Công nghệ” hoặc “Hệ thống” là đối tượng của dự án, bao gồm các phân hệ chức năng, dữ liệu, thành phần tích hợp và các cấu phần kỹ thuật phục vụ khai thác. “Dịch vụ công nghệ thông tin” hoặc “dịch vụ CNTT” là hình thức cung cấp, triển khai, vận hành và duy trì Hệ thống theo phạm vi thuê được xác định. “Nền tảng số” được sử dụng để mô tả tính chất của Hệ thống trong việc cung cấp môi trường số phục vụ quản lý, công bố thông tin, kết nối cung - cầu, giao dịch và chuyển giao công nghệ; không được hiểu là một đối tượng độc lập khác với Hệ thống. “Đơn vị cung cấp dịch vụ” là tổ chức chịu trách nhiệm cung cấp dịch vụ CNTT theo hồ sơ mời thầu/hồ sơ yêu cầu và hợp đồng.",
        False,
    ),
    (
        "Các chương, mục tiếp theo sử dụng các thuật ngữ trên theo đúng phạm vi và ý nghĩa đã quy ước; trường hợp cần dùng thuật ngữ tiếng Anh thì ưu tiên viết tiếng Việt trước, tên tiếng Anh hoặc chữ viết tắt được đặt trong ngoặc khi cần thiết.",
        False,
    ),
]:
    p = insert_paragraph_after(p, text, normal, bold=bold)

# IV.2. Bổ sung bảng chỉ tiêu đầu ra, hiệu năng và SLA.
iv2_anchor = next(
    p
    for p in doc.paragraphs
    if p.text.strip().startswith("Phương án được ưu tiên là mô tả hạ tầng theo năng lực cần đạt")
)
intro = insert_paragraph_after(
    iv2_anchor,
    "Các chỉ tiêu quy mô khai thác, hiệu năng và mức chất lượng dịch vụ (SLA) dự kiến",
    normal,
    bold=True,
)
intro2 = insert_paragraph_after(
    intro,
    "Thỏa thuận mức dịch vụ (Service Level Agreement - SLA) được hiểu là các cam kết về mức độ sẵn sàng, thời gian phản hồi, tiếp nhận sự cố, khắc phục sự cố, khôi phục dịch vụ và trách nhiệm của đơn vị cung cấp dịch vụ. Các chỉ tiêu dưới đây là yêu cầu đầu ra tối thiểu/dự kiến để làm cơ sở lập hồ sơ mời thầu, đánh giá kỹ thuật và nghiệm thu; giá trị chính thức được rà soát theo quy mô dữ liệu, số lượng người dùng và phạm vi dịch vụ được phê duyệt.",
    normal,
)
sla_rows = [
    ["Nhóm chỉ tiêu", "Yêu cầu đầu ra dự kiến", "Cơ sở nghiệm thu/ghi chú"],
    [
        "Quy mô tài khoản và nhóm người dùng",
        "Đáp ứng các nhóm người dùng gồm quản trị, cán bộ nghiệp vụ, lãnh đạo, nhà cung cấp, người mua, chuyên gia và khách truy cập; có khả năng mở rộng mà không thay đổi kiến trúc tổng thể.",
        "Số lượng chính thức xác định tại HSMT theo phạm vi dữ liệu và kế hoạch vận hành.",
    ],
    [
        "Truy cập đồng thời",
        "Đơn vị cung cấp dịch vụ phải chứng minh khả năng đáp ứng mức truy cập đồng thời được xác định trong HSMT và khả năng mở rộng khi nhu cầu tăng.",
        "Kiểm tra qua hồ sơ năng lực, phương án kỹ thuật và thử nghiệm tải khi cần.",
    ],
    [
        "Dung lượng dữ liệu",
        "Đáp ứng dữ liệu nghiệp vụ, tài liệu, hình ảnh, video, hợp đồng, nhật ký và dữ liệu tích hợp; có dự phòng tăng trưởng trong thời gian thuê dịch vụ.",
        "Quy mô chính thức căn cứ danh mục dữ liệu khởi tạo và dữ liệu phát sinh hằng năm.",
    ],
    [
        "Mức độ sẵn sàng dịch vụ",
        "Mức chất lượng dịch vụ (SLA) tối thiểu đề xuất: thời gian sẵn sàng từ 99,5%/tháng trở lên, không bao gồm thời gian bảo trì đã thông báo và được phê duyệt.",
        "Đối chiếu nhật ký vận hành, biên bản bảo trì và báo cáo giám sát dịch vụ.",
    ],
    [
        "Thời gian phản hồi chức năng thông thường",
        "Các thao tác nghiệp vụ thông thường đáp ứng trong thời gian không quá 3 giây đối với tối thiểu 95% yêu cầu hợp lệ, không bao gồm báo cáo lớn, xử lý nền hoặc phụ thuộc dịch vụ ngoài.",
        "Kiểm tra trong vận hành thử hoặc công cụ giám sát hiệu năng.",
    ],
    [
        "Thời gian phản hồi kết nối tích hợp",
        "Các giao dịch tích hợp thông thường đáp ứng trong thời gian không quá 5 giây đối với tối thiểu 95% yêu cầu hợp lệ, trừ trường hợp lỗi hoặc chậm từ dịch vụ bên thứ ba.",
        "Theo nhật ký giao diện lập trình ứng dụng (API), nhật ký lỗi và báo cáo đối soát.",
    ],
    [
        "Tiếp nhận sự cố",
        "Sự cố nghiêm trọng cần được tiếp nhận trong thời gian không quá 30 phút kể từ khi Chủ đầu tư thông báo qua kênh hỗ trợ thống nhất; sự cố thông thường tiếp nhận trong giờ làm việc theo hợp đồng.",
        "Theo phiếu yêu cầu hỗ trợ, nhật ký tiếp nhận và biên bản xử lý.",
    ],
    [
        "Khắc phục và khôi phục dịch vụ",
        "Sự cố làm gián đoạn dịch vụ trọng yếu cần có phương án khôi phục trong thời gian không quá 4 giờ; sự cố không trọng yếu xử lý theo mức ưu tiên được quy định trong hợp đồng.",
        "Áp dụng theo phân loại sự cố và cam kết mức chất lượng dịch vụ.",
    ],
    [
        "Sao lưu và điểm khôi phục dữ liệu",
        "Dữ liệu nghiệp vụ cần được sao lưu tối thiểu hằng ngày; mục tiêu điểm khôi phục dữ liệu không quá 24 giờ đối với dữ liệu vận hành chính.",
        "Đối chiếu kế hoạch sao lưu, nhật ký sao lưu và kiểm tra khôi phục mẫu.",
    ],
    [
        "Lưu giữ nhật ký",
        "Nhật ký truy cập, thao tác quản trị, tích hợp và sự kiện an toàn thông tin cần được lưu giữ tối thiểu 12 tháng hoặc theo yêu cầu pháp luật/hợp đồng.",
        "Phục vụ kiểm tra, truy vết, xử lý sự cố và nghiệm thu vận hành.",
    ],
]
table = insert_table_after(doc, intro2, len(sla_rows), 3)
for i, row in enumerate(sla_rows):
    for j, value in enumerate(row):
        set_cell(table.cell(i, j), value, bold=(i == 0))
format_table(table)

# VI. Thay bảng danh mục dịch vụ bằng bảng làm rõ phạm vi, trách nhiệm và định lượng tích hợp.
target_table = next(
    tbl
    for tbl in doc.tables
    if len(tbl.rows) > 0 and "Hạng mục dịch vụ/thành phần" in tbl.cell(0, 1).text
)
remove_table(target_table)

vi_anchor = next(
    p
    for p in doc.paragraphs
    if p.text.strip().startswith("Danh mục được trình bày theo nhóm thành phần dịch vụ")
)
vi_note = insert_paragraph_after(
    vi_anchor,
    "Danh mục dưới đây làm rõ phạm vi cấu phần thuộc gói thuê dịch vụ CNTT, trách nhiệm cấu hình/tích hợp/vận hành của đơn vị cung cấp dịch vụ và các trường hợp cần xác định riêng trong hồ sơ mời thầu hoặc hợp đồng. Các hạng mục nêu “hoặc tương đương” được hiểu là không chỉ định nhãn hiệu, công nghệ hoặc nhà cung cấp cụ thể.",
    normal,
)
service_rows = [
    [
        "STT",
        "Hạng mục dịch vụ/thành phần",
        "Yêu cầu thiết kế",
        "Phạm vi gói thuê",
        "Trách nhiệm đơn vị cung cấp dịch vụ",
        "Ghi chú chi phí/hợp đồng",
    ],
    [
        "1",
        "Nền tảng cung cấp dịch vụ",
        "Đáp ứng vận hành ứng dụng web, quản trị dữ liệu, phân quyền, tích hợp, báo cáo, giám sát, sao lưu và hỗ trợ người dùng.",
        "Thuộc gói thuê dịch vụ CNTT.",
        "Cung cấp, cấu hình, vận hành, bảo trì, giám sát và bảo đảm mức chất lượng dịch vụ (SLA).",
        "Không chỉ định nền tảng cụ thể; nhà cung cấp đề xuất giải pháp hoặc tương đương.",
    ],
    [
        "2",
        "Tài nguyên tính toán, lưu trữ và đường truyền",
        "Đáp ứng quy mô người dùng, dữ liệu, truy cập, mở rộng và điều kiện an toàn vận hành theo Chương IV.",
        "Thuộc gói thuê hoặc theo phương án hạ tầng được phê duyệt.",
        "Bảo đảm tài nguyên, sao lưu, khôi phục, giám sát, cảnh báo và báo cáo vận hành.",
        "Thông số chính thức xác định trong HSMT/hợp đồng theo quy mô được duyệt.",
    ],
    [
        "3",
        "Dịch vụ thư điện tử",
        "Cung cấp và tích hợp dịch vụ email phục vụ quản trị, thông báo, hỗ trợ người dùng và vận hành hệ thống.",
        "Thuộc gói tích hợp kèm theo: 5 năm; mỗi năm 60 địa chỉ email, dung lượng lưu trữ 25GB SSD, 5 Mail List.",
        "Cấu hình tên miền/tài khoản, danh sách thư, phân quyền, nhật ký gửi nhận, hỗ trợ lỗi và đồng bộ thông báo hệ thống.",
        "Chi phí cấu hình, tích hợp và đồng bộ dữ liệu thuộc phạm vi gói; điều kiện sử dụng chi tiết xác định trong hợp đồng.",
    ],
    [
        "4",
        "Tin nhắn thương hiệu SMS Brandname",
        "Gửi thông báo, xác thực, nhắc lịch sự kiện, cảnh báo giao dịch hoặc thông tin cần thiết theo kịch bản được phê duyệt.",
        "Thuộc gói tích hợp kèm theo: 5 năm; mỗi năm 3.600 SMS.",
        "Cấu hình kết nối, mẫu tin, trạng thái gửi, nhật ký, đối soát sản lượng và xử lý lỗi với nhà cung cấp SMS.",
        "Sản lượng vượt hạn mức hoặc thay đổi chính sách cước được xác định theo HSMT/hợp đồng.",
    ],
    [
        "5",
        "Giải pháp thực tế ảo VR360",
        "Hiển thị không gian tham quan/trưng bày số phục vụ giới thiệu sản phẩm, công nghệ, gian hàng hoặc khu trưng bày.",
        "Thuộc gói tích hợp kèm theo: 10 điểm cảnh quay dựng mới.",
        "Tổ chức cấu hình, tích hợp hiển thị, gắn thông tin liên kết, kiểm thử truy cập và bàn giao dữ liệu/đường dẫn khai thác.",
        "Chi phí sản xuất nội dung ngoài 10 điểm hoặc cập nhật bổ sung thực hiện theo phạm vi được duyệt.",
    ],
    [
        "6",
        "Chữ ký số",
        "Hỗ trợ ký số hồ sơ, văn bản, xác nhận giao dịch hoặc tài liệu điện tử theo quy trình nghiệp vụ được phê duyệt.",
        "Thuộc gói tích hợp kèm theo: 5 năm.",
        "Tích hợp kỹ thuật, cấu hình luồng ký, ghi nhận nhật ký, kiểm tra trạng thái ký và hỗ trợ vận hành.",
        "Chứng thư số/tài khoản ký cụ thể thực hiện theo quy định pháp luật và phạm vi hợp đồng.",
    ],
    [
        "7",
        "Hợp đồng điện tử",
        "Tạo lập, gửi ký, theo dõi trạng thái, lưu vết và quản lý hợp đồng điện tử phục vụ giao dịch công nghệ.",
        "Thuộc gói tích hợp kèm theo: 5 năm; mỗi năm 2.000 hợp đồng điện tử.",
        "Tích hợp, đồng bộ trạng thái hợp đồng, lưu vết giao dịch, hỗ trợ tra cứu, báo cáo và đối soát sản lượng.",
        "Sản lượng vượt hạn mức hoặc thay đổi mẫu/luồng ký xác định theo HSMT/hợp đồng.",
    ],
    [
        "8",
        "Thông báo tự động qua web/email/SMS",
        "Cảnh báo tài khoản, hồ sơ, sản phẩm, sự kiện, giao dịch, hợp đồng và các nội dung quản trị cần theo dõi.",
        "Thuộc phạm vi cấu hình của Hệ thống; kênh email/SMS sử dụng theo hạn mức tương ứng.",
        "Thiết lập mẫu thông báo, điều kiện kích hoạt, lịch gửi, nhật ký và cơ chế chống gửi trùng.",
        "Phụ thuộc hạn mức dịch vụ tích hợp đã phê duyệt.",
    ],
    [
        "9",
        "Trò chuyện trực tuyến, cuộc gọi video, phát trực tuyến",
        "Hỗ trợ trao đổi, tư vấn, kết nối cung - cầu, hội thảo hoặc sự kiện trực tuyến khi được phê duyệt trong phạm vi dịch vụ.",
        "Thuộc phạm vi tích hợp/cấu hình nếu được xác định trong HSMT; dịch vụ nền bên ngoài có thể theo hợp đồng riêng.",
        "Tích hợp, cấu hình kênh, kiểm tra truy cập, phân quyền, nhật ký và hướng dẫn sử dụng.",
        "Cần nêu rõ trong HSMT chi phí sử dụng dịch vụ nền thuộc gói thuê hay thanh toán riêng.",
    ],
    [
        "10",
        "Ứng dụng trí tuệ nhân tạo",
        "Hỗ trợ chatbot tư vấn, tìm kiếm thông minh, gợi ý sản phẩm, gợi ý nhà cung cấp/công nghệ và tìm kiếm bằng giọng nói nếu được phê duyệt.",
        "Thuộc phạm vi chức năng/tích hợp của Hệ thống; hạn mức sử dụng dịch vụ AI bên thứ ba phải xác định rõ trong HSMT/hợp đồng.",
        "Cấu hình kịch bản, kiểm soát dữ liệu đầu vào/đầu ra, giám sát chi phí sử dụng, nhật ký và cơ chế can thiệp của người quản trị.",
        "Không mặc nhiên bao gồm mọi chi phí dịch vụ AI bên ngoài nếu HSMT/hợp đồng chưa quy định.",
    ],
    [
        "11",
        "Kết nối giao diện lập trình ứng dụng (API)",
        "Kết nối với cổng thông tin, hệ thống báo cáo, dịch vụ định danh/thông báo hoặc các hệ thống khác khi có yêu cầu.",
        "Thuộc gói tích hợp theo danh sách kết nối được phê duyệt.",
        "Xây dựng/cấu hình kết nối, xác thực, phân quyền, nhật ký, tài liệu đặc tả, kiểm thử và xử lý lỗi.",
        "Kết nối phát sinh ngoài danh sách được xử lý theo thay đổi phạm vi.",
    ],
    [
        "12",
        "Giám sát, sao lưu, bảo mật và hỗ trợ vận hành",
        "Theo dõi trạng thái dịch vụ, cảnh báo sự cố, sao lưu, khôi phục, bảo vệ dữ liệu và hỗ trợ người dùng.",
        "Thuộc gói thuê dịch vụ CNTT trong toàn bộ thời gian thuê.",
        "Duy trì giám sát, báo cáo định kỳ, hỗ trợ sự cố, sao lưu/khôi phục mẫu và phối hợp nghiệm thu SLA.",
        "Gắn với cam kết mức chất lượng dịch vụ và trách nhiệm bảo hành, bảo trì 05 năm.",
    ],
]
table = insert_table_after(doc, vi_note, len(service_rows), 6)
for i, row in enumerate(service_rows):
    for j, value in enumerate(row):
        set_cell(table.cell(i, j), value, bold=(i == 0))
format_table(table)

# VIII. Cập nhật bảng nghiệm thu để nối với SLA và định lượng tích hợp.
for tbl in doc.tables:
    if len(tbl.rows) > 0 and tbl.cell(0, 0).text.strip() == "Nhóm nghiệm thu":
        for row in tbl.rows:
            name = row.cells[0].text.strip()
            if name == "Tích hợp":
                row.cells[1].text = (
                    "Kiểm tra email, SMS Brandname, VR360, chữ ký số, hợp đồng điện tử, thông báo tự động và các giao diện lập trình ứng dụng (API) "
                    "theo phạm vi đã được phê duyệt; đối chiếu hạn mức/sản lượng tích hợp khi nghiệm thu."
                )
            elif name == "Vận hành":
                row.cells[1].text = (
                    "Kiểm tra mức chất lượng dịch vụ (SLA), nhật ký vận hành, thời gian sẵn sàng, thời gian phản hồi, tiếp nhận/khắc phục sự cố, "
                    "sao lưu, khôi phục và báo cáo giám sát."
                )
        break

# Chuẩn hóa một số cụm từ SLA đã có.
for p in doc.paragraphs:
    if "mức chất lượng dịch vụ theo hợp đồng" in p.text:
        for run in p.runs:
            run.text = run.text.replace(
                "mức chất lượng dịch vụ theo hợp đồng",
                "mức chất lượng dịch vụ (SLA) theo hợp đồng",
            )

doc.save(DST)
print(DST)
