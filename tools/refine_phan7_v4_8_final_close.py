from pathlib import Path

from docx import Document


SRC = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.7_hoan_thien_SLA_trung_lap_truoc_gui.docx"
)
DST = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.8_ban_chot_truoc_gui.docx"
)


doc = Document(SRC)


def replace_text_runs(paragraph, replacements):
    for run in paragraph.runs:
        if not run.text:
            continue
        new_text = run.text
        for old, new in replacements.items():
            new_text = new_text.replace(old, new)
        if new_text != run.text:
            run.text = new_text


replacements = {
    "yêu cầu đầu ra tối thiểu/dự kiến": "yêu cầu đầu ra tối thiểu",
    "thời gian sẵn sàng từ 99,5%/tháng trở lên": "tỷ lệ sẵn sàng của dịch vụ đạt từ 99,5% trong tháng trở lên",
    "10.000 tài khoản/người dùng có định danh": "10.000 tài khoản người dùng có định danh",
    "toàn bộ dịch vụ": "toàn bộ Hệ thống",
    "Hạ tầng tham chiếu gồm 02 máy chủ": (
        "Theo số liệu đầu vào và báo giá khảo sát phục vụ lập thiết kế, phương án hạ tầng tham chiếu có thể được cấu hình tương đương 02 máy chủ"
    ),
    "Hệ thống ngừng hoạt động diện rộng; mất khả năng truy cập chức năng trọng yếu;": (
        "Hệ thống ngừng hoạt động diện rộng; mất khả năng truy cập chức năng trọng yếu;"
    ),
}

for p in doc.paragraphs:
    replace_text_runs(p, replacements)
for tbl in doc.tables:
    for row in tbl.rows:
        for cell in row.cells:
            for p in cell.paragraphs:
                replace_text_runs(p, replacements)


# Viết hoa đúng ngữ cảnh cho "Trí tuệ nhân tạo (AI)" trong tên nhóm/hạng mục và đầu câu.
for tbl in doc.tables:
    for row in tbl.rows:
        for cell in row.cells:
            text = cell.text.strip()
            if text == "trí tuệ nhân tạo (AI)":
                cell.text = "Trí tuệ nhân tạo (AI)"
            elif text.startswith("trí tuệ nhân tạo được đặt"):
                cell.text = text.replace("trí tuệ nhân tạo được đặt", "Trí tuệ nhân tạo được đặt", 1)
            elif text.startswith("ứng dụng trí tuệ nhân tạo (AI)"):
                cell.text = text.replace("ứng dụng trí tuệ nhân tạo (AI)", "Ứng dụng trí tuệ nhân tạo (AI)", 1)


# Nếu trong bảng SLA vẫn có dấu "/" hoặc câu cũ, ghi lại chính xác các ô liên quan.
for tbl in doc.tables:
    if len(tbl.rows) and tbl.cell(0, 0).text.strip() == "Nhóm chỉ tiêu":
        for row in tbl.rows:
            key = row.cells[0].text.strip()
            if key == "Quy mô tài khoản và nhóm người dùng":
                row.cells[1].text = (
                    "Quy mô thiết kế dự kiến tối thiểu 10.000 tài khoản người dùng có định danh cho các nhóm quản trị, cán bộ nghiệp vụ, lãnh đạo, nhà cung cấp, người mua, chuyên gia; đồng thời hỗ trợ khách truy cập công khai không yêu cầu đăng nhập theo chính sách vận hành."
                )
            elif key == "Mức độ sẵn sàng dịch vụ":
                row.cells[1].text = (
                    "Mức chất lượng dịch vụ theo SLA tối thiểu đề xuất: tỷ lệ sẵn sàng của dịch vụ đạt từ 99,5% trong tháng trở lên, không bao gồm thời gian bảo trì đã thông báo và được phê duyệt."
                )
        break


doc.save(DST)
print(DST)
