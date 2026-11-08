from docx import Document


p = r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v5.0.1_bo_sung_6_diem_tham_dinh.docx"
doc = Document(p)
text = "\n".join(pa.text for pa in doc.paragraphs)
text += "\n" + "\n".join(c.text for tbl in doc.tables for row in tbl.rows for c in row.cells)

print("paras", len(doc.paragraphs), "tables", len(doc.tables), "shapes", len(doc.inline_shapes))
checks = [
    "3090/QĐ-BKHCN",
    "Khung kiến trúc tổng thể quốc gia số",
    "Kết luận tuân thủ Chương III",
    "không được xem là yêu cầu cấu hình bắt buộc",
    "không phải cấu hình bắt buộc",
    "không sử dụng cấu hình khảo sát làm tiêu chí khóa công nghệ",
    "02 máy chủ",
    "8 lõi",
    "RAM 32GB",
    "SSD 500GB",
    "quyền quản lý, khai thác, trích xuất",
    "không được xác lập quyền riêng đối với dữ liệu",
    "Mức 2 - Cao",
    "Tiếp nhận không quá 1 giờ làm việc",
    "Mục tiêu khắc phục không quá 8 giờ làm việc",
    "Điều kiện đo thời gian phản hồi 3 giây/5 giây",
    "5. Kế hoạch kết thúc và chuyển tiếp dịch vụ",
    "đầu mối chịu trách nhiệm đầu cuối",
    "trách nhiệm phối hợp đầu cuối",
    "MobiFone",
    "phiên/người dùng",
    "better",
]
for key in checks:
    print(key, text.count(key))

for ti, tbl in enumerate(doc.tables):
    header = " | ".join(c.text.replace("\n", " / ")[:100] for c in tbl.rows[0].cells) if tbl.rows else ""
    if any(k in header for k in ["Nhóm quy chuẩn", "Lớp kiến trúc", "Nhóm chỉ tiêu", "Mức sự cố", "Hạng mục dịch vụ", "Nội dung", "Tài liệu/sản phẩm"]):
        print("\nTABLE", ti, len(tbl.rows), "x", len(tbl.columns), header)
        for r in tbl.rows[: min(7, len(tbl.rows))]:
            print(" | ".join(c.text.replace("\n", " / ")[:260] for c in r.cells))
