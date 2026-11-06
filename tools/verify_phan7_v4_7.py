from docx import Document


p = r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.7_hoan_thien_SLA_trung_lap_truoc_gui.docx"
doc = Document(p)
text = "\n".join(pa.text for pa in doc.paragraphs)
text += "\n" + "\n".join(c.text for tbl in doc.tables for row in tbl.rows for c in row.cells)

print("paras", len(doc.paragraphs), "tables", len(doc.tables), "shapes", len(doc.inline_shapes))
for key in [
    "MobiFone",
    "báo giá khảo sát phục vụ lập thiết kế",
    "hồ sơ khảo sát giá phục vụ lập thiết kế và dự toán",
    "phiên/người dùng",
    "300 phiên truy cập đồng thời",
    "1.000 phiên truy cập đồng thời",
    "thử nghiệm tải đối với mức tối thiểu 300 phiên",
    "có phương án khôi phục",
    "phải được khôi phục trong thời gian không quá 4 giờ",
    "Mức 1 - Nghiêm trọng",
    "25GB SSD/tài khoản",
    "25 GB/tài khoản",
    "Cách hiểu này",
    "phương án chỉ tích hợp",
    "Đào tạo/ hướng dẫn",
    "biện pháp hướng dẫn thi hành",
    "biện pháp để hướng dẫn thi hành",
    "Trí tuệ nhân tạo",
    "trí tuệ nhân tạo",
    "thư điện tử (email)",
    "thư điện tử (Email)",
    "10 điểm cảnh quay",
    "3.600 SMS",
    "2.000 hợp đồng điện tử",
]:
    print(key, text.count(key))

for ti, tbl in enumerate(doc.tables):
    if len(tbl.rows) and (
        tbl.cell(0, 0).text.strip() in ["Nhóm chỉ tiêu", "Mức sự cố"]
        or (len(tbl.columns) > 1 and tbl.cell(0, 1).text.strip() == "Hạng mục dịch vụ/thành phần")
    ):
        print("\nTABLE", ti, len(tbl.rows), "x", len(tbl.columns))
        for r in tbl.rows[:6]:
            print(" | ".join(c.text.replace("\n", " / ")[:260] for c in r.cells))
