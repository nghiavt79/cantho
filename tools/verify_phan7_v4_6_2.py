from docx import Document


p = r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.6.2_hoan_thien_cuoi_truoc_gui.docx"
doc = Document(p)
text = "\n".join(pa.text for pa in doc.paragraphs)
text += "\n" + "\n".join(c.text for tbl in doc.tables for row in tbl.rows for c in row.cells)

print("paras", len(doc.paragraphs), "tables", len(doc.tables), "shapes", len(doc.inline_shapes))
for key in [
    "bảo đảm Thỏa thuận mức dịch vụ",
    "bảo đảm các mức chất lượng dịch vụ theo Thỏa thuận mức dịch vụ",
    "better",
    "tương đương hoặc tốt hơn",
    "tổng dung lượng vật lý danh nghĩa 1TB",
    "Dung lượng khai thác hữu dụng phụ thuộc",
    "10.000 tài khoản",
    "300 phiên",
    "1.000 phiên",
    "60 địa chỉ thư điện tử",
    "25GB SSD/tài khoản",
    "3.600 SMS",
    "10 điểm cảnh quay",
    "2.000 hợp đồng điện tử",
]:
    print(key, text.count(key))

for i, p in enumerate(doc.paragraphs[:75]):
    t = p.text.strip()
    if t:
        print(i, p.style.name, repr(t[:320]))

for ti, tbl in enumerate(doc.tables):
    if len(tbl.rows) and (
        tbl.cell(0, 0).text.strip() == "Nhóm chỉ tiêu"
        or (len(tbl.columns) > 1 and tbl.cell(0, 1).text.strip() == "Hạng mục dịch vụ/thành phần")
    ):
        print("\nTABLE", ti, len(tbl.rows), "x", len(tbl.columns))
        for r in tbl.rows[:5]:
            print(" | ".join(c.text.replace("\n", " / ")[:260] for c in r.cells))
