from pathlib import Path

from docx import Document


SRC = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.5_bo_sung_dinh_luong_SLA_tich_hop.docx"
)
DST = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.5.1_hoan_thien_SLA_tich_hop_viet_hoa.docx"
)


doc = Document(SRC)

replacements = {
    "dịch vụ email": "dịch vụ thư điện tử (email)",
    "email phục vụ": "thư điện tử (email) phục vụ",
    "60 địa chỉ email": "60 địa chỉ thư điện tử (email)",
    "5 Mail List": "5 danh sách thư (Mail List)",
    "dịch vụ AI": "dịch vụ trí tuệ nhân tạo (AI)",
    "chatbot tư vấn": "trợ lý tư vấn tự động (chatbot)",
}

for p in doc.paragraphs:
    for run in p.runs:
        if not run.text:
            continue
        new_text = run.text
        for old, new in replacements.items():
            new_text = new_text.replace(old, new)
        if new_text != run.text:
            run.text = new_text

for tbl in doc.tables:
    for row in tbl.rows:
        for cell in row.cells:
            for p in cell.paragraphs:
                for run in p.runs:
                    if not run.text:
                        continue
                    new_text = run.text
                    for old, new in replacements.items():
                        new_text = new_text.replace(old, new)
                    if new_text != run.text:
                        run.text = new_text

for tbl in doc.tables:
    if len(tbl.rows) > 0 and tbl.cell(0, 0).text.strip() == "Nhóm nghiệm thu":
        for row in tbl.rows:
            name = row.cells[0].text.strip()
            if name == "Tích hợp":
                row.cells[1].text = (
                    "Kiểm tra thư điện tử (email), tin nhắn thương hiệu SMS Brandname, VR360, chữ ký số, hợp đồng điện tử, "
                    "thông báo tự động và các giao diện lập trình ứng dụng (API) theo phạm vi đã được phê duyệt; đối chiếu hạn mức/sản lượng tích hợp khi nghiệm thu."
                )
            elif name == "Chất lượng dịch vụ":
                row.cells[1].text = (
                    "Kiểm tra mức chất lượng dịch vụ (SLA), thời gian sẵn sàng, thời gian phản hồi, tiếp nhận sự cố, "
                    "khắc phục/khôi phục dịch vụ, sao lưu, điểm khôi phục dữ liệu, nhật ký vận hành và báo cáo giám sát."
                )
        break

doc.save(DST)
print(DST)
