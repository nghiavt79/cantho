from pathlib import Path

from docx import Document


SRC = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.6.1_chot_so_lieu_MobiFone_SLA.docx"
)
DST = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.6.2_hoan_thien_cuoi_truoc_gui.docx"
)


doc = Document(SRC)


def replace_run_text(container, replacements):
    for p in container.paragraphs:
        for run in p.runs:
            if not run.text:
                continue
            new_text = run.text
            for old, new in replacements.items():
                new_text = new_text.replace(old, new)
            if new_text != run.text:
                run.text = new_text


replacements = {
    "bảo đảm Thỏa thuận mức dịch vụ (Service Level Agreement - SLA) theo hợp đồng": (
        "bảo đảm các mức chất lượng dịch vụ theo Thỏa thuận mức dịch vụ (Service Level Agreement - SLA) được quy định trong hợp đồng"
    ),
    "Dịch vụ phải đáp ứng đầy đủ 12 nhóm chức năng": "Hệ thống phải đáp ứng đầy đủ 12 nhóm chức năng",
    "Dịch vụ phải sẵn sàng tích hợp": "Hệ thống phải có khả năng tích hợp",
    "Dịch vụ phải có cơ chế giám sát": "Hệ thống phải có cơ chế giám sát",
    "cấu hình tương đương/better": "cấu hình tương đương hoặc tốt hơn",
    "tương đương/better": "tương đương hoặc tốt hơn",
}

replace_run_text(doc, replacements)
for tbl in doc.tables:
    for row in tbl.rows:
        for cell in row.cells:
            replace_run_text(cell, replacements)


# Bảng SLA/quy mô: sửa cách diễn đạt dung lượng 02 máy chủ x 500GB và thêm câu dẫn.
for tbl in doc.tables:
    if len(tbl.rows) > 0 and tbl.cell(0, 0).text.strip() == "Nhóm chỉ tiêu":
        for row in tbl.rows:
            key = row.cells[0].text.strip()
            if key == "Quy mô tài khoản và nhóm người dùng":
                row.cells[2].text = (
                    "Các giá trị quy mô được xác định theo phạm vi khai thác dự kiến, nhu cầu mở rộng trong thời gian thuê và số liệu đầu vào phục vụ lập thiết kế; được rà soát khi lập HSMT nhưng không thấp hơn yêu cầu tối thiểu được phê duyệt tại hồ sơ này."
                )
            elif key == "Dung lượng dữ liệu":
                row.cells[1].text = (
                    "Hạ tầng tham chiếu gồm 02 máy chủ, mỗi máy có dung lượng lưu trữ SSD tối thiểu 500GB, tương ứng tổng dung lượng vật lý danh nghĩa 1TB. "
                    "Dung lượng khai thác hữu dụng phụ thuộc mô hình phân bổ, dự phòng, nhân bản dữ liệu, hệ điều hành, cơ sở dữ liệu và phương án sao lưu."
                )
                row.cells[2].text = (
                    "Căn cứ số liệu báo giá MobiFone Cần Thơ; đơn vị cung cấp dịch vụ phải chứng minh dung lượng khả dụng đáp ứng nhu cầu vận hành, dữ liệu hình ảnh, video, hợp đồng, nhật ký và tăng trưởng dữ liệu trong 05 năm."
                )
        break


# Bảng Chương VI: viết lại hạ tầng theo hướng tham chiếu, không khóa cấu hình.
for tbl in doc.tables:
    if len(tbl.rows) > 0 and len(tbl.columns) > 1 and tbl.cell(0, 1).text.strip() == "Hạng mục dịch vụ/thành phần":
        for row in tbl.rows:
            if row.cells[1].text.strip() == "Tài nguyên tính toán, lưu trữ và đường truyền":
                row.cells[2].text = (
                    "Theo số liệu đầu vào phục vụ lập thiết kế và báo giá tham chiếu, phương án hạ tầng có thể được cấu hình tương đương 02 máy chủ, mỗi máy khoảng 8 lõi xử lý, RAM 32GB, SSD 500GB và 01 địa chỉ IP. "
                    "Đơn vị cung cấp dịch vụ được đề xuất cấu hình khác hoặc mô hình tài nguyên tương đương hoặc tốt hơn, với điều kiện chứng minh đáp ứng các chỉ tiêu quy mô, hiệu năng, an toàn, khả năng mở rộng và SLA tại Chương IV."
                )
                row.cells[5].text = (
                    "Số liệu tham chiếu từ báo giá MobiFone Cần Thơ; không chỉ định hãng/nhãn hiệu bộ xử lý, hệ điều hành hoặc cơ sở dữ liệu cụ thể và không làm thay đổi nguyên tắc đánh giá theo năng lực đầu ra."
                )
        break


doc.save(DST)
print(DST)
