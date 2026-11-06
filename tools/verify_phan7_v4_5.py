from docx import Document


p = r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.5.1_hoan_thien_SLA_tich_hop_viet_hoa.docx"
doc = Document(p)

print("paras", len(doc.paragraphs), "tables", len(doc.tables), "shapes", len(doc.inline_shapes))
for i, para in enumerate(doc.paragraphs):
    t = para.text.strip()
    if any(
        k in t
        for k in [
            "Cơ sở lựa chọn hình thức thuê",
            "Quy ước sử dụng thuật ngữ",
            "Thỏa thuận mức dịch vụ",
            "Kết luận Chương IV",
            "Danh mục dưới đây làm rõ",
        ]
    ):
        print(i, para.style.name, repr(t[:700]))

for ti, tbl in enumerate(doc.tables):
    headers = " | ".join(c.text.replace("\n", " / ") for c in tbl.rows[0].cells)
    if "Nhóm chỉ tiêu" in headers or "Phạm vi gói thuê" in headers or "Nhóm nghiệm thu" in headers:
        print("\nTABLE", ti, "rows", len(tbl.rows), "cols", len(tbl.columns))
        print(headers)
        for r in tbl.rows[1 : min(len(tbl.rows), 6)]:
            print(" | ".join(c.text.replace("\n", " / ")[:180] for c in r.cells))
        if len(tbl.rows) > 6:
            print("... last:", " | ".join(c.text.replace("\n", " / ")[:180] for c in tbl.rows[-1].cells))

text = "\n".join(p.text for p in doc.paragraphs)
text += "\n" + "\n".join(c.text for tbl in doc.tables for row in tbl.rows for c in row.cells)
for k in [
    "Thông tư 45/2025/TT-BTC",
    "10 điểm cảnh quay",
    "3.600 SMS",
    "60 địa chỉ thư điện tử",
    "25GB SSD",
    "2.000 hợp đồng",
    "99,5%",
    "Service Level Agreement",
    "5 danh sách thư (Mail List)",
    "dịch vụ trí tuệ nhân tạo (AI)",
    "Cơ sở lựa chọn hình thức thuê dịch vụ công nghệ thông tin",
    "Quy ước sử dụng thuật ngữ",
]:
    print(k, text.count(k))
