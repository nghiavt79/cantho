from pathlib import Path

from docx import Document


SRC = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.6.2_hoan_thien_cuoi_truoc_gui.docx"
)
DST = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.7_hoan_thien_SLA_trung_lap_truoc_gui.docx"
)


doc = Document(SRC)


def replace_in_runs(paragraph, replacements):
    for run in paragraph.runs:
        if not run.text:
            continue
        new_text = run.text
        for old, new in replacements.items():
            new_text = new_text.replace(old, new)
        if new_text != run.text:
            run.text = new_text


global_replacements = {
    "báo giá MobiFone Cần Thơ": "báo giá khảo sát phục vụ lập thiết kế",
    "Số liệu tham chiếu từ báo giá MobiFone Cần Thơ": "Số liệu tham chiếu từ hồ sơ khảo sát giá phục vụ lập thiết kế và dự toán",
    "phiên/người dùng truy cập đồng thời": "phiên truy cập đồng thời",
    "thử nghiệm tải khi cần": "thử nghiệm tải đối với mức tối thiểu 300 phiên truy cập đồng thời",
    "có phương án khôi phục trong thời gian không quá 4 giờ": "phải được khôi phục trong thời gian không quá 4 giờ kể từ thời điểm sự cố được xác nhận",
    "25GB SSD/tài khoản": "25 GB/tài khoản",
    "dung lượng lưu trữ 25 GB/tài khoản": "dung lượng hộp thư tối đa 25 GB/tài khoản",
    "Cách hiểu này tránh cộng dồn thành 300 địa chỉ trong 05 năm; nếu HSMT/hợp đồng quy định khác thì áp dụng theo phạm vi được phê duyệt.": (
        "Số lượng 60 địa chỉ thư điện tử là hạn mức duy trì tối đa tại mỗi thời điểm trong từng năm cung cấp dịch vụ, không cộng dồn qua các năm, trừ trường hợp HSMT/hợp đồng quy định khác."
    ),
    "Áp dụng phương án chỉ tích hợp thuộc gói khi chưa có dự toán hạn mức sử dụng AI; hạn mức chi phí, dữ liệu huấn luyện và trách nhiệm kiểm soát đầu ra cần quy định trong HSMT/hợp đồng.": (
        "Trường hợp chi phí sử dụng dịch vụ trí tuệ nhân tạo (AI) bên thứ ba chưa được xác định trong dự toán gói thuê, phạm vi gói thuê chỉ bao gồm chức năng và tích hợp kỹ thuật; chi phí sử dụng dịch vụ bên thứ ba được xác định theo hạn mức hoặc hợp đồng riêng."
    ),
    "Đào tạo/ hướng dẫn": "Đào tạo/hướng dẫn",
    "biện pháp hướng dẫn thi hành Luật Chuyển giao công nghệ": "biện pháp để hướng dẫn thi hành Luật Chuyển giao công nghệ",
    "Trí tuệ nhân tạo": "trí tuệ nhân tạo",
    "thư điện tử (email)": "thư điện tử (Email)",
}

for p in doc.paragraphs:
    replace_in_runs(p, global_replacements)
for tbl in doc.tables:
    for row in tbl.rows:
        for cell in row.cells:
            for p in cell.paragraphs:
                replace_in_runs(p, global_replacements)


# Chỉnh bảng SLA/quy mô khai thác cho chặt về nghiệm thu.
for tbl in doc.tables:
    if len(tbl.rows) > 0 and tbl.cell(0, 0).text.strip() == "Nhóm chỉ tiêu":
        for row in tbl.rows:
            key = row.cells[0].text.strip()
            if key == "Truy cập đồng thời":
                row.cells[1].text = (
                    "Đáp ứng tối thiểu 300 phiên truy cập đồng thời trong giai đoạn vận hành ban đầu và có khả năng mở rộng lên tối thiểu 1.000 phiên truy cập đồng thời khi nhu cầu khai thác tăng."
                )
                row.cells[2].text = (
                    "Kiểm tra bằng phương án kỹ thuật và thử nghiệm tải đối với mức tối thiểu 300 phiên truy cập đồng thời; khả năng mở rộng lên 1.000 phiên được đánh giá thông qua kiến trúc, phương án mở rộng tài nguyên và thử nghiệm bổ sung khi thuộc phạm vi nghiệm thu."
                )
            elif key == "Khắc phục và khôi phục dịch vụ":
                row.cells[1].text = (
                    "Đối với sự cố làm gián đoạn dịch vụ trọng yếu, dịch vụ phải được khôi phục trong thời gian không quá 4 giờ kể từ thời điểm sự cố được xác nhận, trừ trường hợp bất khả kháng hoặc trường hợp khác được quy định trong hợp đồng. Sự cố không trọng yếu được xử lý theo mức ưu tiên quy định tại SLA."
                )
                row.cells[2].text = (
                    "Áp dụng theo phân loại mức độ sự cố, nhật ký tiếp nhận, biên bản xác nhận sự cố và báo cáo khôi phục dịch vụ."
                )
        break


# Bổ sung bảng phân loại mức độ sự cố ngay sau bảng SLA.
sla_table = None
for tbl in doc.tables:
    if len(tbl.rows) > 0 and tbl.cell(0, 0).text.strip() == "Nhóm chỉ tiêu":
        sla_table = tbl
        break

if sla_table is not None:
    classification = doc.add_table(rows=5, cols=2)
    classification.style = "Table Grid"
    data = [
        ["Mức sự cố", "Tiêu chí phân loại"],
        [
            "Mức 1 - Nghiêm trọng",
            "Hệ thống ngừng hoạt động diện rộng; mất khả năng truy cập chức năng trọng yếu; có nguy cơ mất dữ liệu hoặc sự cố an toàn thông tin nghiêm trọng.",
        ],
        [
            "Mức 2 - Cao",
            "Một phân hệ quan trọng bị gián đoạn, ảnh hưởng nhiều người dùng nhưng Hệ thống vẫn còn khả năng khai thác một phần.",
        ],
        [
            "Mức 3 - Thông thường",
            "Lỗi cục bộ, có phương án xử lý tạm thời, không làm gián đoạn chức năng trọng yếu.",
        ],
        [
            "Mức 4 - Yêu cầu hỗ trợ",
            "Yêu cầu cấu hình, hướng dẫn, hiệu chỉnh nhỏ hoặc hỗ trợ sử dụng; không phải sự cố gây gián đoạn dịch vụ.",
        ],
    ]
    for i, row in enumerate(data):
        for j, value in enumerate(row):
            classification.cell(i, j).text = value
            for p in classification.cell(i, j).paragraphs:
                for run in p.runs:
                    run.bold = i == 0

    marker = doc.add_paragraph()
    marker.add_run("Phân loại mức độ sự cố phục vụ áp dụng SLA").bold = True
    sla_table._tbl.addnext(marker._p)
    marker._p.addnext(classification._tbl)


doc.save(DST)
print(DST)
