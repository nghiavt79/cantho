from docx import Document


p = r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.6_chuan_hoa_so_lieu_MobiFone.docx"
doc = Document(p)
print("paras", len(doc.paragraphs), "tables", len(doc.tables), "shapes", len(doc.inline_shapes))

text = "\n".join(pa.text for pa in doc.paragraphs)
text += "\n" + "\n".join(c.text for tbl in doc.tables for row in tbl.rows for c in row.cells)

checks = [
    "Thỏa thuận mức dịch vụ (Service Level Agreement - SLA)",
    "10.000 tài khoản",
    "300 phiên",
    "1.000 phiên",
    "02 máy chủ",
    "500GB SSD",
    "1TB",
    "60 địa chỉ thư điện tử",
    "25GB SSD/tài khoản",
    "5 danh sách thư",
    "3.600 SMS",
    "10 điểm cảnh quay",
    "2.000 hợp đồng điện tử",
    "tích hợp và duy trì kết nối kỹ thuật phục vụ chữ ký số trong 05 năm",
    "chi phí sử dụng dịch vụ trí tuệ nhân tạo (AI) bên thứ ba",
    "phí sử dụng nền tảng bên thứ ba chỉ thuộc gói",
]
for k in checks:
    print(k, text.count(k))

for i, pa in enumerate(doc.paragraphs):
    t = pa.text.strip()
    if i < 75 and t:
        print(i, pa.style.name, repr(t[:260]))
    if any(k in t for k in ["Người dùng truy cập Hệ thống", "Kết luận Chương IV"]):
        print("HIT", i, pa.style.name, repr(t[:350]))

for ti, tbl in enumerate(doc.tables):
    if len(tbl.rows) > 0 and (
        tbl.cell(0, 0).text.strip() == "Nhóm chỉ tiêu"
        or (len(tbl.columns) > 1 and tbl.cell(0, 1).text.strip() == "Hạng mục dịch vụ/thành phần")
        or tbl.cell(0, 0).text.strip() == "Nhóm nghiệm thu"
    ):
        print("\nTABLE", ti, len(tbl.rows), "x", len(tbl.columns))
        print(" | ".join(c.text.replace("\n", " / ")[:180] for c in tbl.rows[0].cells))
        for r in tbl.rows[1:min(6, len(tbl.rows))]:
            print(" | ".join(c.text.replace("\n", " / ")[:220] for c in r.cells))
        if len(tbl.rows) > 6:
            print("... last:", " | ".join(c.text.replace("\n", " / ")[:220] for c in tbl.rows[-1].cells))
