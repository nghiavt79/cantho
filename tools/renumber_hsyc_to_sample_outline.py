from __future__ import annotations

from pathlib import Path

from docx import Document


ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "docs" / "ho so ky thuat" / "HSYC_KT_San_GD_KHCN_CDT_v2.0_day_du.docx"
OUT = ROOT / "docs" / "ho so ky thuat" / "HSYC_KT_San_GD_KHCN_CDT_v2.1_theo_muc_luc_mau.docx"


TOP_RENAMES = {
    "I. TỔNG QUAN": "1. TỔNG QUAN",
    "1. Giới thiệu dự án": "1.1. Giới thiệu giải pháp",
    "2. Mục tiêu tổng quát": "1.2. Mục tiêu giải pháp đem lại",
    "3. Mục tiêu chi tiết": "1.2.1. Mục tiêu chi tiết",
    "4. Phạm vi cung cấp": "1.2.2. Phạm vi cung cấp",
    "5. Yêu cầu chung đối với nhà thầu/giải pháp": "1.2.3. Yêu cầu chung đối với nhà thầu/giải pháp",
    "II. THUYẾT MINH VỀ GIẢI PHÁP KỸ THUẬT, CÔNG NGHỆ": "2. THUYẾT MINH VỀ GIẢI PHÁP KỸ THUẬT, CÔNG NGHỆ",
    "1. Mô hình kiến trúc phần mềm": "2.1. Mô hình kiến trúc phần mềm",
    "2. Nguyên tắc lựa chọn công nghệ": "2.1.1. Nguyên tắc lựa chọn công nghệ",
    "3. Mô hình kiến trúc triển khai hệ thống": "2.2. Mô hình kiến trúc triển khai hệ thống.",
    "4. Áp dụng chuẩn, tiêu chuẩn": "2.3. Áp dụng chuẩn, tiêu chuẩn",
    "III. THUYẾT MINH YÊU CẦU CHỨC NĂNG CỦA PHẦN MỀM": "3. THUYẾT MINH GIẢI PHÁP ĐÁP ỨNG CÁC YÊU CẦU CHỨC NĂNG CỦA PHẦN MỀM",
    "1. Quy trình nghiệp vụ được tin học hóa": "3.1. Quy trình nghiệp vụ được tin học hóa",
    "2. Các tác nhân tham gia vào hệ thống": "3.2. Các tác nhân tham gia vào hệ thống",
    "3. Danh sách các chức năng": "3.3. Danh sách các chức năng",
    "4. Bảng chuyển đổi yêu cầu chức năng sang trường hợp sử dụng": "3.4. Bảng chuyển đổi yêu cầu chức năng sang trường hợp sử dụng",
    "IV. THUYẾT MINH YÊU CẦU PHI CHỨC NĂNG": "4. THUYẾT MINH GIẢI PHÁP THỰC HIỆN CÁC YÊU CẦU PHI CHỨC NĂNG",
    "1. Yêu cầu cần đáp ứng đối với cơ sở dữ liệu": "4.1. Yêu cầu cần đáp ứng đối với cơ sở dữ liệu",
    "2. Yêu cầu về an toàn thông tin": "4.2. Yêu cầu về an toàn thông tin",
    "3. Yêu cầu về thời gian xử lý và độ phức tạp xử lý": "4.3. Các yêu cầu cần đáp ứng về thời gian xử lý, độ phức tạp xử lý của các phần mềm",
    "4. Ràng buộc môi trường và nền tảng": "4.4. Các ràng buộc đối với hệ thống gồm: ràng buộc môi trường, sự phụ thuộc vào hệ thống nền tảng",
    "5. Yêu cầu chịu lỗi và kiểm soát dữ liệu đầu vào": "4.5. Yêu cầu về mức độ chịu đựng sai hỏng đối với các lỗi cú pháp lập trình, lỗi lô-gic trong xử lý dữ liệu, lỗi kiểm soát tính đúng đắn của dữ liệu đầu vào",
    "6. Yêu cầu về mỹ thuật, kỹ thuật giao diện": "4.6. Yêu cầu về mỹ thuật, kỹ thuật cần đạt được của các giao diện chương trình",
    "7. Yêu cầu năng lực nhà thầu và nhân sự": "4.7. Yêu cầu về năng lực của cán bộ tham gia xây dựng, phát triển, nâng cấp, mở rộng phần mềm.",
    "8. Yêu cầu tính sẵn sàng với IPv6": "4.8. Yêu cầu về tính sẵn sàng với IPv6",
    "9. Yêu cầu cài đặt, hạ tầng, đường truyền, an toàn vận hành": "4.9. Yêu cầu về cài đặt, hạ tầng, đường truyền, an toàn vận hành, khai thác sử dụng",
    "10. Phân loại lỗi nghiệm thu": "4.10. Phân loại lỗi nghiệm thu",
    "V. YÊU CẦU TÍCH HỢP VÀ DỊCH VỤ NGOÀI": "4.11. Yêu cầu tích hợp và dịch vụ ngoài",
    "VI. KẾ HOẠCH TRIỂN KHAI": "5. KẾ HOẠCH TRIỂN KHAI",
    "1. Phương án đào tạo, lắp đặt, cài đặt và triển khai": "5.1. Phương án đào tạo, lắp đặt, cài đặt và triển khai",
    "Triển khai lên môi trường kiểm thử (QA/UAT)": "5.1.1. Triển khai lên môi trưởng kiểm thử (môi trường QA):",
    "Triển khai lên môi trường thực tế (Production)": "5.1.2. Triển khai lên môi trường thực tế (môi trường Production):",
    "Phương án đào tạo và chuyển giao công nghệ": "5.1.3. Phương án đào tạo và chuyển giao công nghệ",
    "2. Phương án bảo hành, bảo trì": "5.2. Phương án bảo hành, bảo trì",
    "3. Phương án kiểm tra và thử nghiệm": "5.3. Phương án kiểm tra và thử nghiệm",
    "4. Kế hoạch triển khai dự kiến": "5.4. Kế hoạch triển khai",
    "5. Bố trí nhân sự": "5.5. Bố trí nhân sự",
    "VII. YÊU CẦU NGHIỆM THU VÀ BÀN GIAO": "5.6. Yêu cầu nghiệm thu và bàn giao",
    "VIII. PHỤ LỤC - BẢNG YÊU CẦU ĐÁP ỨNG TÓM TẮT": "5.7. Phụ lục - Bảng yêu cầu đáp ứng tóm tắt",
}

PROCESS_TITLES = [
    "Quy trình đăng ký và kích hoạt tài khoản",
    "Quy trình quản lý hồ sơ nhà cung ứng",
    "Quy trình quản lý hồ sơ chuyên gia",
    "Quy trình đăng và duyệt sản phẩm/công nghệ",
    "Quy trình đăng nhu cầu công nghệ",
    "Quy trình tìm kiếm và gợi ý thông minh",
    "Quy trình AI Support/chatbot",
    "Quy trình khởi tạo hồ sơ giao dịch/chuyển giao",
    "Quy trình NDA/bảo mật thông tin",
    "Quy trình RFQ và mời nhà cung ứng",
    "Quy trình nộp và đánh giá đề xuất",
    "Quy trình đàm phán thương mại",
    "Quy trình rà soát pháp lý",
    "Quy trình hợp đồng điện tử và ký số",
    "Quy trình tạm ứng, thử nghiệm, bàn giao",
    "Quy trình nghiệm thu và thanh lý",
    "Quy trình báo cáo thống kê",
]


def set_para_text(paragraph, text: str) -> None:
    if paragraph.runs:
        paragraph.runs[0].text = text
        for run in paragraph.runs[1:]:
            run.text = ""
    else:
        paragraph.add_run(text)


def main() -> None:
    doc = Document(str(SRC))
    process_map = {title: f"3.1.{idx}. {title}" for idx, title in enumerate(PROCESS_TITLES, start=1)}
    heading2_prefixes = ("4.11.", "5.6.", "5.7.")
    heading3_prefixes = ("1.2.1.", "1.2.2.", "1.2.3.", "2.1.1.")
    renamed = 0
    for para in doc.paragraphs:
        text = " ".join(para.text.split())
        if text in TOP_RENAMES:
            set_para_text(para, TOP_RENAMES[text])
            renamed += 1
        elif text in process_map:
            set_para_text(para, process_map[text])
            renamed += 1
        new_text = " ".join(para.text.split())
        if new_text.startswith(heading2_prefixes):
            para.style = doc.styles["Heading 2"]
        if new_text.startswith(heading3_prefixes):
            para.style = doc.styles["Heading 3"]

    doc.core_properties.title = "Hồ sơ yêu cầu kỹ thuật - Sàn giao dịch KHCN - Theo mục lục mẫu"
    doc.core_properties.subject = "Bản v2.1 theo khung sườn người dùng cung cấp"
    doc.save(str(OUT))
    print(f"renamed={renamed}")
    print(str(OUT).encode("ascii", errors="backslashreplace").decode("ascii"))


if __name__ == "__main__":
    main()
