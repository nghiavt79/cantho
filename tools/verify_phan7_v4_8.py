from docx import Document


p = r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.8_ban_chot_truoc_gui.docx"
doc = Document(p)
text = "\n".join(pa.text for pa in doc.paragraphs)
text += "\n" + "\n".join(c.text for tbl in doc.tables for row in tbl.rows for c in row.cells)

print("paras", len(doc.paragraphs), "tables", len(doc.tables), "shapes", len(doc.inline_shapes))
for key in [
    "yêu cầu đầu ra tối thiểu/dự kiến",
    "yêu cầu đầu ra tối thiểu",
    "thời gian sẵn sàng từ 99,5%",
    "tỷ lệ sẵn sàng của dịch vụ đạt từ 99,5%",
    "10.000 tài khoản/người dùng",
    "10.000 tài khoản người dùng có định danh",
    "toàn bộ dịch vụ",
    "toàn bộ Hệ thống",
    "trí tuệ nhân tạo (AI)",
    "Trí tuệ nhân tạo (AI)",
    "trí tuệ nhân tạo được đặt",
    "Trí tuệ nhân tạo được đặt",
    "MobiFone",
    "phiên/người dùng",
    "better",
]:
    print(key, text.count(key))

for ti, tbl in enumerate(doc.tables):
    if len(tbl.rows) and (
        tbl.cell(0, 0).text.strip() in ["Nhóm chỉ tiêu", "Mức sự cố"]
        or (len(tbl.columns) > 1 and tbl.cell(0, 1).text.strip() == "Hạng mục dịch vụ/thành phần")
    ):
        print("\nTABLE", ti, len(tbl.rows), "x", len(tbl.columns))
        for r in tbl.rows[:5]:
            print(" | ".join(c.text.replace("\n", " / ")[:260] for c in r.cells))
