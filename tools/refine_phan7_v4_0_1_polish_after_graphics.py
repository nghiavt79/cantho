from pathlib import Path

from docx import Document


BASE = Path(r"D:\2026\CanTho\docs\ho so ky thuat")
SRC = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.0_ky_thuat_hoa_so_do_chuyen_nghiep.docx"
DST = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.0.1_ky_thuat_hoa_so_do_chuyen_nghiep.docx"


def set_para(paragraph, text):
    paragraph._p.clear_content()
    paragraph.add_run(text)


def main():
    doc = Document(SRC)

    replacements = {
        "Các tiện ích tích hợp được kích hoạt theo phạm vi dịch vụ và được quản lý qua cơ chế quản trị cấu hình, phân quyền và truy vết, theo dõi lỗi; không coi danh sách tiện ích là chỉ định cố định về công nghệ hoặc nhà cung cấp.": (
            "Các tiện ích tích hợp được kích hoạt theo phạm vi dịch vụ và được quản lý bằng cơ chế cấu hình, phân quyền, truy vết và theo dõi lỗi; danh sách tiện ích không được hiểu là chỉ định cố định về công nghệ hoặc nhà cung cấp."
        ),
        "Dữ liệu phát sinh được lưu trữ, sao lưu, phân quyền và có khả năng xuất/bàn giao theo quy định của hợp đồng thuê dịch vụ.": (
            "Dữ liệu phát sinh được lưu trữ, sao lưu, phân quyền và có khả năng xuất/bàn giao theo quy định của hợp đồng."
        ),
        "thiết kế không xác định chi tiết vị trí lắp đặt thiết bị của đơn vị cung cấp dịch vụ. Thay vào đó, thiết kế quy định yêu cầu tối thiểu đối với môi trường cung cấp dịch vụ, hạ tầng vận hành, kết nối, bảo mật, sao lưu và giám sát. Nếu dịch vụ sử dụng thiết bị tại địa điểm của Chủ đầu tư, phương án bố trí phải được trình duyệt riêng và bảo đảm không ảnh hưởng hạ tầng hiện hữu.": (
            "Thiết kế không xác định chi tiết vị trí lắp đặt thiết bị của đơn vị cung cấp dịch vụ. Thay vào đó, tài liệu quy định yêu cầu tối thiểu đối với môi trường cung cấp dịch vụ, hạ tầng vận hành, kết nối, bảo mật, sao lưu và giám sát. Nếu dịch vụ sử dụng thiết bị tại địa điểm của Chủ đầu tư, phương án bố trí phải được trình duyệt riêng và bảo đảm không ảnh hưởng hạ tầng hiện hữu."
        ),
        "lớp tích hợp chỉ mở các kết nối được xác định và có kiểm soát": (
            "lớp tích hợp chỉ mở các kết nối có mục đích rõ ràng và có kiểm soát"
        ),
    }

    for paragraph in doc.paragraphs:
        text = paragraph.text
        for old, new in replacements.items():
            text = text.replace(old, new)
        if text != paragraph.text:
            set_para(paragraph, text)

    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                text = cell.text
                for old, new in replacements.items():
                    text = text.replace(old, new)
                if text != cell.text:
                    cell.text = text

    doc.save(DST)
    print(DST)


if __name__ == "__main__":
    main()
