from docx import Document


p = r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.9_chot_noi_dung_phan_7.docx"
doc = Document(p)
text = "\n".join(pa.text for pa in doc.paragraphs)
text += "\n" + "\n".join(c.text for tbl in doc.tables for row in tbl.rows for c in row.cells)

print("paras", len(doc.paragraphs), "tables", len(doc.tables), "shapes", len(doc.inline_shapes))
for key in [
    "Các chỉ tiêu quy mô khai thác, hiệu năng và mức chất lượng dịch vụ theo SLA dự kiến",
    "Các chỉ tiêu quy mô khai thác, hiệu năng và mức chất lượng dịch vụ theo SLA",
    "Yêu cầu đầu ra dự kiến",
    "Yêu cầu đầu ra tối thiểu",
    "tối thiểu đề xuất",
    "Tỷ lệ sẵn sàng của dịch vụ theo SLA đạt tối thiểu 99,5% trong tháng",
    "Các chỉ tiêu về tỷ lệ sẵn sàng và thời gian khôi phục dịch vụ được đánh giá độc lập",
    "không làm miễn trừ trách nhiệm đáp ứng tỷ lệ sẵn sàng",
    "dịch vụ có khả năng vận hành trong môi trường cơ quan nhà nước",
    "dịch vụ CNTT được cung cấp đáp ứng yêu cầu vận hành Hệ thống",
    "dịch vụ được cung cấp qua kênh số",
    "các chức năng và tiện ích của Hệ thống được cung cấp qua kênh số",
    "Sử dụng hệ thống",
    "Sử dụng Hệ thống",
    "MobiFone",
    "phiên/người dùng",
    "better",
]:
    print(key, text.count(key))

for ti, tbl in enumerate(doc.tables):
    if len(tbl.rows) and tbl.cell(0, 0).text.strip() in ["Nhóm chỉ tiêu", "Mức sự cố"]:
        print("\nTABLE", ti, len(tbl.rows), "x", len(tbl.columns))
        for r in tbl.rows[:5]:
            print(" | ".join(c.text.replace("\n", " / ")[:260] for c in r.cells))
