from pathlib import Path

from docx import Document


SRC = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v5.0_bo_sung_6_diem_tham_dinh.docx"
)
DST = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v5.0.1_bo_sung_6_diem_tham_dinh.docx"
)


doc = Document(SRC)

# Loại bỏ thông số cấu hình khảo sát khỏi phần yêu cầu kỹ thuật, để tránh bị hiểu là tiêu chí bắt buộc.
for tbl in doc.tables:
    if len(tbl.rows) > 0 and tbl.cell(0, 0).text.strip() == "Nhóm chỉ tiêu":
        for row in tbl.rows:
            if row.cells[0].text.strip() == "Dung lượng dữ liệu":
                row.cells[1].text = (
                    "Yêu cầu dung lượng dữ liệu được xác định theo năng lực lưu trữ khả dụng, khả năng mở rộng, cơ chế phân bổ/dự phòng, sao lưu, khôi phục và tăng trưởng dữ liệu trong thời gian thuê. "
                    "Các thông số cấu hình trong hồ sơ khảo sát giá chỉ dùng để ước lượng chi phí, không được xem là yêu cầu cấu hình kỹ thuật bắt buộc."
                )
                row.cells[2].text = (
                    "Nghiệm thu căn cứ dung lượng khả dụng, báo cáo giám sát tài nguyên, kết quả sao lưu/khôi phục, khả năng mở rộng và tài liệu chứng minh phương án lưu trữ; không sử dụng cấu hình khảo sát làm tiêu chí loại hoặc tiêu chí khóa công nghệ."
                )
        break

for tbl in doc.tables:
    if len(tbl.rows) > 0 and len(tbl.columns) > 1 and tbl.cell(0, 1).text.strip() == "Hạng mục dịch vụ/thành phần":
        for row in tbl.rows:
            if row.cells[1].text.strip() == "Tài nguyên tính toán, lưu trữ và đường truyền":
                row.cells[2].text = (
                    "Yêu cầu thiết kế đối với hạ tầng được xác định theo năng lực đầu ra: đáp ứng quy mô tài khoản, truy cập đồng thời, dung lượng khả dụng, sao lưu/khôi phục, giám sát, an toàn thông tin, mở rộng và SLA. "
                    "Các thông số cấu hình trong hồ sơ khảo sát giá chỉ là cơ sở tham chiếu chi phí, không phải cấu hình bắt buộc và không dùng làm tiêu chí khóa công nghệ."
                )
                row.cells[3].text = (
                    "Thuộc gói thuê hạ tầng 05 năm phục vụ cài đặt, vận hành và lưu trữ dữ liệu của Hệ thống; đơn vị cung cấp dịch vụ được đề xuất hạ tầng vật lý, ảo hóa, điện toán đám mây hoặc mô hình tương đương đáp ứng yêu cầu đầu ra."
                )
                row.cells[4].text = (
                    "Chứng minh năng lực tài nguyên, dung lượng khả dụng, phương án dự phòng, sao lưu, khôi phục, giám sát, cảnh báo, an toàn thông tin và báo cáo vận hành theo SLA."
                )
                row.cells[5].text = (
                    "Đánh giá theo năng lực đầu ra, khả năng mở rộng, an toàn vận hành và tổng chi phí thuê dịch vụ được phê duyệt; không chỉ định máy chủ, bộ xử lý, hệ điều hành, cơ sở dữ liệu hoặc mô hình hạ tầng cụ thể."
                )
        break

doc.save(DST)
print(DST)
