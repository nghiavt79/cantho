from pathlib import Path

from docx import Document
from docx.oxml import OxmlElement


BASE = Path(r"D:\2026\CanTho\docs\ho so ky thuat")
SRC = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3 - 05072026_002.docx"
DST = BASE / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.4_bo_sung_phan_tich_chuong_IV.docx"


def set_para(paragraph, text):
    paragraph._p.clear_content()
    paragraph.add_run(text)


def insert_after(paragraph, text):
    new_p = OxmlElement("w:p")
    paragraph._p.addnext(new_p)
    new_para = paragraph._parent.add_paragraph()
    new_para._p = new_p
    new_para.add_run(text)
    return new_para


def find_para(doc, prefix):
    for paragraph in doc.paragraphs:
        if paragraph.text.strip().startswith(prefix):
            return paragraph
    raise RuntimeError(f"Không tìm thấy đoạn bắt đầu bằng: {prefix}")


def add_after_prefix(doc, prefix, text):
    para = find_para(doc, prefix)
    insert_after(para, text)


def main():
    doc = Document(SRC)

    set_para(
        find_para(doc, "Do dự án thực hiện theo định hướng thuê dịch vụ CNTT"),
        "Phương án kỹ thuật cần cân bằng giữa yêu cầu kiểm soát của Chủ đầu tư và quyền chủ động công nghệ của đơn vị cung cấp dịch vụ. Thiết kế không quy định cứng nền tảng triển khai, nhưng đặt ra các ràng buộc kỹ thuật tối thiểu để bảo đảm dịch vụ vận hành an toàn, ổn định và có thể kiểm soát.",
    )
    add_after_prefix(
        doc,
        "Phương án kỹ thuật cần cân bằng",
        "Cơ sở lựa chọn phương án này là đặc thù của dự án: Chủ đầu tư cần kiểm soát yêu cầu đầu ra, dữ liệu và chất lượng dịch vụ, trong khi nền tảng kỹ thuật cụ thể có thể do nhà cung cấp đề xuất. So với phương án chỉ định sẵn nền tảng hoặc cấu hình, cách tiếp cận theo ràng buộc đầu ra giúp tăng tính cạnh tranh, giảm nguy cơ khóa công nghệ và vẫn giữ đủ căn cứ để nghiệm thu.",
    )

    add_after_prefix(
        doc,
        "Hạ tầng dịch vụ được xem xét ở góc độ năng lực đầu ra",
        "Phương án được ưu tiên là mô tả hạ tầng theo năng lực cần đạt thay vì mô tả cấu hình máy chủ cụ thể. Cơ sở lựa chọn là quy mô người dùng, dữ liệu, tần suất truy cập và yêu cầu vận hành có thể thay đổi trong quá trình khai thác. Ưu điểm của phương án này là cho phép mở rộng linh hoạt, đánh giá được chất lượng dịch vụ theo các chỉ tiêu đo lường và tránh phụ thuộc vào một cấu hình kỹ thuật cố định.",
    )

    add_after_prefix(
        doc,
        "Đối với hình thức thuê dịch vụ CNTT, thiết kế chi tiết không chỉ định cụ thể máy chủ",
        "So với phương án mua sắm, lắp đặt và sở hữu từng thành phần phần cứng/phần mềm, phương án mô tả theo yêu cầu kỹ thuật và mức chất lượng dịch vụ phù hợp hơn với mục tiêu thuê dịch vụ. Cách lựa chọn này giúp Chủ đầu tư tập trung kiểm soát tính sẵn sàng, an toàn, khả năng bàn giao dữ liệu và chi phí vòng đời, thay vì phải quản trị chi tiết từng công cụ kỹ thuật nội bộ.",
    )

    set_para(
        find_para(doc, "Kết nối dịch vụ ngoài: các dịch vụ tích hợp được xác định"),
        "Kết nối dịch vụ ngoài: các dịch vụ tích hợp thuộc phạm vi triển khai phải được cấu hình theo yêu cầu dịch vụ, có cơ chế xác thực, phân quyền, truy vết, theo dõi lỗi và khả năng thay thế khi cần.",
    )
    add_after_prefix(
        doc,
        "Phương án kết nối được lựa chọn theo nguyên tắc mở",
        "Cơ sở lựa chọn phương án kết nối mở là hệ thống cần liên thông với nhiều nhóm đối tượng và dịch vụ khác nhau nhưng vẫn phải kiểm soát dữ liệu chia sẻ. So với phương án trao đổi thủ công hoặc kết nối đóng theo từng trường hợp riêng lẻ, mô hình giao diện lập trình ứng dụng (API), định dạng dữ liệu phổ biến và nhật ký truy vết giúp giảm chi phí tích hợp, tăng khả năng mở rộng và tạo căn cứ kiểm tra trách nhiệm khi có lỗi phát sinh.",
    )

    add_after_prefix(
        doc,
        "Nội dung an toàn thông tin tại mục này chỉ tập trung",
        "Phương án được đề xuất là áp dụng an toàn thông tin theo lớp và theo rủi ro: xác thực, phân quyền, bảo mật truy cập, bảo vệ dữ liệu, kiểm soát tải tệp lên, nhật ký kiểm toán, sao lưu và ứng cứu sự cố. So với phương án chỉ kiểm tra bảo mật ở cuối quá trình triển khai, cách tiếp cận này đưa yêu cầu an toàn vào ngay từ thiết kế, giúp giảm rủi ro vận hành và thuận lợi xác định tiêu chí nghiệm thu.",
    )

    add_after_prefix(
        doc,
        "Nhóm ràng buộc này tập trung vào dữ liệu và trách nhiệm dịch vụ",
        "Cơ sở lựa chọn là dữ liệu của Sàn giao dịch có giá trị quản lý lâu dài, bao gồm hồ sơ nhà cung cấp, sản phẩm/công nghệ, giao dịch, báo cáo và nhật ký vận hành. So với phương án chỉ quy định quyền sử dụng phần mềm, việc ràng buộc rõ quyền khai thác, trích xuất, sao lưu, bàn giao và duy trì dữ liệu giúp Chủ đầu tư giảm rủi ro phụ thuộc nhà cung cấp và bảo đảm khả năng chuyển tiếp dịch vụ khi cần.",
    )

    add_after_prefix(
        doc,
        "Đối với IPv6, thiết kế không quy định cấu hình kỹ thuật chi tiết",
        "Phương án sẵn sàng IPv6 được lựa chọn thay vì yêu cầu chuyển đổi tức thời toàn bộ hạ tầng. Cách tiếp cận này phù hợp với điều kiện triển khai thực tế, cho phép hệ thống vận hành ổn định trên môi trường hiện hữu nhưng vẫn bảo đảm khả năng mở rộng khi hạ tầng của Chủ đầu tư, nhà cung cấp hoặc các hệ thống liên thông chuyển sang IPv6.",
    )

    # Add a conclusion before Chapter V.
    chapter_v = find_para(doc, "V. THUYẾT MINH GIẢI PHÁP KỸ THUẬT:")
    conclusion = OxmlElement("w:p")
    chapter_v._p.addprevious(conclusion)
    conclusion_para = chapter_v._parent.add_paragraph()
    conclusion_para._p = conclusion
    conclusion_para.add_run(
        "Kết luận Chương IV: Phương án kỹ thuật được lựa chọn theo nguyên tắc kiểm soát đầu ra, trung lập công nghệ, mở về tích hợp, chặt chẽ về dữ liệu và an toàn thông tin, đồng thời sẵn sàng mở rộng hạ tầng khi nhu cầu khai thác tăng. Các phân tích tại Chương IV là cơ sở để Chương V thuyết minh mô hình kiến trúc, phân lớp chức năng, nguyên lý hoạt động, giải pháp quản lý hệ thống và giải pháp kết nối theo hướng có thể nghiệm thu, vận hành và giám sát trong thực tế."
    )

    doc.save(DST)
    print(DST)


if __name__ == "__main__":
    main()
