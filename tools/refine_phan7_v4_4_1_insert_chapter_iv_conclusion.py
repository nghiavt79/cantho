from pathlib import Path

from docx import Document


BASE = Path(r"D:\2026\CanTho\docs\ho so ky thuat")
SRC = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.4_bo_sung_phan_tich_chuong_IV.docx"
DST = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.4.1_bo_sung_phan_tich_chuong_IV.docx"


CONCLUSION = (
    "Kết luận Chương IV: Phương án kỹ thuật được lựa chọn theo nguyên tắc kiểm soát đầu ra, "
    "trung lập công nghệ, mở về tích hợp, chặt chẽ về dữ liệu và an toàn thông tin, đồng thời "
    "sẵn sàng mở rộng hạ tầng khi nhu cầu khai thác tăng. Các phân tích tại Chương IV là cơ sở "
    "để Chương V thuyết minh mô hình kiến trúc, phân lớp chức năng, nguyên lý hoạt động, giải pháp "
    "quản lý hệ thống và giải pháp kết nối theo hướng có thể nghiệm thu, vận hành và giám sát trong thực tế."
)


def main():
    doc = Document(SRC)

    if any(p.text.strip().startswith("Kết luận Chương IV") for p in doc.paragraphs):
        doc.save(DST)
        print(DST)
        return

    for paragraph in doc.paragraphs:
        if paragraph.text.strip() == "V. THUYẾT MINH GIẢI PHÁP KỸ THUẬT:":
            paragraph.insert_paragraph_before(CONCLUSION)
            break
    else:
        raise RuntimeError("Không tìm thấy tiêu đề Chương V")

    doc.save(DST)
    print(DST)


if __name__ == "__main__":
    main()
