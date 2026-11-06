from copy import deepcopy
from pathlib import Path

from docx import Document
from docx.text.paragraph import Paragraph


SRC = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.8_ban_chot_truoc_gui.docx"
)
DST = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.9_chot_noi_dung_phan_7.docx"
)


doc = Document(SRC)


def replace_runs(paragraph, replacements):
    for run in paragraph.runs:
        if not run.text:
            continue
        new_text = run.text
        for old, new in replacements.items():
            new_text = new_text.replace(old, new)
        if new_text != run.text:
            run.text = new_text


def insert_paragraph_after(paragraph: Paragraph, text: str, style=None) -> Paragraph:
    new_p = deepcopy(paragraph._p)
    paragraph._p.addnext(new_p)
    new_para = Paragraph(new_p, paragraph._parent)
    new_para.clear()
    if style is not None:
        new_para.style = style
    new_para.add_run(text)
    return new_para


replacements = {
    "Các chỉ tiêu quy mô khai thác, hiệu năng và mức chất lượng dịch vụ theo SLA dự kiến": (
        "Các chỉ tiêu quy mô khai thác, hiệu năng và mức chất lượng dịch vụ theo SLA"
    ),
    "Yêu cầu đầu ra dự kiến": "Yêu cầu đầu ra tối thiểu",
    "Các chỉ tiêu dưới đây là yêu cầu đầu ra tối thiểu để làm cơ sở lập hồ sơ mời thầu, đánh giá kỹ thuật và nghiệm thu; giá trị chính thức được rà soát theo quy mô dữ liệu, số lượng người dùng và phạm vi dịch vụ được phê duyệt.": (
        "Các chỉ tiêu dưới đây là yêu cầu đầu ra tối thiểu để làm cơ sở lập hồ sơ mời thầu, đánh giá kỹ thuật và nghiệm thu. Các giá trị được rà soát khi lập hồ sơ mời thầu theo phạm vi dịch vụ được phê duyệt nhưng không thấp hơn mức tối thiểu xác định tại hồ sơ này, trừ trường hợp có điều chỉnh được cấp có thẩm quyền phê duyệt."
    ),
    "nhằm bảo đảm dịch vụ có khả năng vận hành trong môi trường cơ quan nhà nước": (
        "nhằm bảo đảm dịch vụ CNTT được cung cấp đáp ứng yêu cầu vận hành Hệ thống trong môi trường cơ quan nhà nước"
    ),
    "dữ liệu được quản lý tập trung, dịch vụ được cung cấp qua kênh số, các phân hệ có khả năng tích hợp và mở rộng": (
        "dữ liệu được quản lý tập trung, các chức năng và tiện ích của Hệ thống được cung cấp qua kênh số, các phân hệ có khả năng tích hợp và mở rộng"
    ),
    "Sử dụng hệ thống, quản trị nội dung/dữ liệu": "Sử dụng Hệ thống, quản trị nội dung/dữ liệu",
    "Mức chất lượng dịch vụ theo SLA tối thiểu đề xuất: tỷ lệ sẵn sàng của dịch vụ đạt từ 99,5% trong tháng trở lên": (
        "Tỷ lệ sẵn sàng của dịch vụ theo SLA đạt tối thiểu 99,5% trong tháng"
    ),
}

for p in doc.paragraphs:
    replace_runs(p, replacements)
for tbl in doc.tables:
    for row in tbl.rows:
        for cell in row.cells:
            for p in cell.paragraphs:
                replace_runs(p, replacements)


# Update SLA table header if table text replacement did not catch cell text.
sla_table = None
for tbl in doc.tables:
    if len(tbl.rows) > 0 and tbl.cell(0, 0).text.strip() == "Nhóm chỉ tiêu":
        sla_table = tbl
        tbl.cell(0, 1).text = "Yêu cầu đầu ra tối thiểu"
        for row in tbl.rows:
            if row.cells[0].text.strip() == "Mức độ sẵn sàng dịch vụ":
                row.cells[1].text = (
                    "Tỷ lệ sẵn sàng của dịch vụ theo SLA đạt tối thiểu 99,5% trong tháng, không bao gồm thời gian bảo trì đã thông báo và được phê duyệt."
                )
        break


# Add clarification after the SLA table and before the incident classification table/paragraph.
clarification = (
    "Các chỉ tiêu về tỷ lệ sẵn sàng và thời gian khôi phục dịch vụ được đánh giá độc lập. "
    "Việc đáp ứng thời gian khôi phục tối đa không làm miễn trừ trách nhiệm đáp ứng tỷ lệ sẵn sàng của dịch vụ theo SLA."
)
if sla_table is not None and clarification not in "\n".join(p.text for p in doc.paragraphs):
    marker = doc.add_paragraph(clarification)
    sla_table._tbl.addnext(marker._p)


doc.save(DST)
print(DST)
