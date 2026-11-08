from __future__ import annotations

from pathlib import Path

from docx import Document


ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "docs" / "ho so ky thuat" / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.2_thue_dich_vu_CNTT.docx"
OUT = ROOT / "docs" / "ho so ky thuat" / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.3_viet_hoa_phan_tich.docx"


REPLACEMENTS = [
    ("Email/Web", "thư điện tử (Email)/thông báo trên hệ thống"),
    ("Email", "thư điện tử (Email)"),
    ("SMS Brandname", "tin nhắn thương hiệu (SMS Brandname)"),
    ("SMS", "tin nhắn ngắn (SMS)"),
    ("AI Chatbot", "trợ lý tư vấn dùng trí tuệ nhân tạo (AI Chatbot)"),
    ("AI provider", "dịch vụ trí tuệ nhân tạo bên ngoài"),
    ("AI", "trí tuệ nhân tạo (AI)"),
    ("API callback", "địa chỉ gọi lại của giao diện lập trình ứng dụng (API callback)"),
    ("API", "giao diện lập trình ứng dụng (API)"),
    ("VR360", "tham quan thực tế ảo 360 độ (VR360)"),
    ("Video Call", "gọi video (Video Call)"),
    ("video call", "gọi video"),
    ("Livestream", "phát trực tuyến (Livestream)"),
    ("livestream", "phát trực tuyến"),
    ("PC - Tablet - Mobile", "máy tính cá nhân (PC) - máy tính bảng (Tablet) - thiết bị di động (Mobile)"),
    ("PC, Tablet, Mobile", "máy tính cá nhân (PC), máy tính bảng (Tablet), thiết bị di động (Mobile)"),
    ("PC", "máy tính cá nhân (PC)"),
    ("Tablet", "máy tính bảng (Tablet)"),
    ("Mobile", "thiết bị di động (Mobile)"),
    ("cloud", "điện toán đám mây"),
    ("dual-stack", "mô hình song song IPv4/IPv6"),
    ("responsive", "tương thích đa kích thước màn hình (responsive)"),
    ("log/audit", "nhật ký/nhật ký kiểm tra (audit)"),
    ("audit", "nhật ký kiểm tra (audit)"),
    ("log", "nhật ký"),
    ("upload", "tải tệp lên"),
]

ANALYSIS_AFTER = {
    "II. DANH MỤC QUY CHUẨN KỸ THUẬT, TIÊU CHUẨN ĐƯỢC ÁP DỤNG": (
        "Cơ sở lựa chọn các tiêu chuẩn nêu trên là bảo đảm dịch vụ được thuê có thể vận hành trong môi trường cơ quan nhà nước, có khả năng kết nối, chia sẻ dữ liệu và đáp ứng yêu cầu an toàn thông tin. Việc mô tả theo nhóm tiêu chuẩn thay vì chỉ định sản phẩm giúp hồ sơ giữ tính mở, tạo điều kiện cho nhiều phương án kỹ thuật tương đương cùng tham gia, đồng thời vẫn bảo đảm Chủ đầu tư có cơ sở nghiệm thu chất lượng dịch vụ."
    ),
    "III. PHÂN TÍCH SỰ TUÂN THỦ KHUNG KIẾN TRÚC TỔNG THỂ QUỐC GIA SỐ:": (
        "Phương án kiến trúc được lựa chọn vì phù hợp nguyên tắc phát triển nền tảng số chuyên ngành: dữ liệu được quản lý tập trung, dịch vụ được cung cấp qua kênh số, các phân hệ có khả năng tích hợp và mở rộng. Ưu điểm của cách tiếp cận này là Chủ đầu tư có thể khai thác dữ liệu phục vụ điều hành, đồng thời giảm phụ thuộc vào từng công cụ riêng lẻ của đơn vị cung cấp dịch vụ."
    ),
    "IV. PHÂN TÍCH LỰA CHỌN PHƯƠNG ÁN KỸ THUẬT, CÔNG NGHỆ, THIẾT BỊ": (
        "Việc phân tích phương án kỹ thuật tập trung vào năng lực cần đáp ứng của dịch vụ thay vì mô tả cách triển khai nội bộ. Cách tiếp cận này phù hợp với hình thức thuê dịch vụ công nghệ thông tin: Chủ đầu tư kiểm soát đầu ra, chất lượng dịch vụ, dữ liệu và an toàn thông tin; đơn vị cung cấp dịch vụ chủ động lựa chọn công nghệ hoặc thiết bị tương đương để đáp ứng yêu cầu."
    ),
    "V. THUYẾT MINH GIẢI PHÁP KỸ THUẬT:": (
        "Giải pháp kỹ thuật được thuyết minh theo logic từ yêu cầu nghiệp vụ đến mô hình dịch vụ, nguyên lý hoạt động, quản lý hệ thống và kết nối. Cơ sở lựa chọn mô hình này là 12 nhóm chức năng trọng tâm của Sàn giao dịch; ưu điểm là dễ đối chiếu giữa yêu cầu, thiết kế, nghiệm thu và vận hành, tránh tình trạng mô tả chức năng rời rạc nhưng không thể kiểm soát chất lượng dịch vụ."
    ),
    "VI. DANH MỤC THIẾT BỊ PHẦN CỨNG, PHẦN MỀM THƯƠNG MẠI:": (
        "Danh mục được trình bày theo nhóm thành phần dịch vụ và năng lực cần đáp ứng, không chỉ định nhãn hiệu hoặc công nghệ cụ thể. Cách thể hiện này bảo đảm tính cạnh tranh, đồng thời vẫn giúp Chủ đầu tư xác định được các cấu phần chi phí cần xem xét trong kế hoạch thuê dịch vụ."
    ),
    "VII. TỔ CHỨC THỰC HIỆN:": (
        "Tổ chức thực hiện được phân định theo vai trò quản lý và trách nhiệm phối hợp, không quy định cơ cấu nhân sự chi tiết của đơn vị cung cấp dịch vụ. Cách phân định này phù hợp với thiết kế chi tiết cho thuê dịch vụ, vì trọng tâm là bảo đảm trách nhiệm đầu ra, cơ chế phối hợp và cơ sở nghiệm thu."
    ),
    "VIII. PHƯƠNG ÁN TỐ CHỨC LẮP ĐẶT, TRIỂN KHAI HỆ THỐNG:": (
        "Phương án triển khai được mô tả ở mức thiết lập, kiểm tra, vận hành thử và nghiệm thu dịch vụ. Nội dung này nhằm bảo đảm dịch vụ có thể đưa vào khai thác an toàn, ổn định, nhưng không đi sâu vào quy trình kỹ thuật nội bộ của đơn vị cung cấp dịch vụ."
    ),
    "IX. YÊU CẦU CHI TIẾT VỀ VIỆC ĐÀO TẠO, TẬP HUẤN HƯỚNG DẪN SỬ DỤNG, QUẢN TRỊ, VẬN HÀNH": (
        "Yêu cầu đào tạo tập trung vào năng lực khai thác dịch vụ của Chủ đầu tư và người dùng, không đào tạo theo hướng tiếp nhận công cụ triển khai nội bộ. Cách tiếp cận này phù hợp với thuê dịch vụ công nghệ thông tin vì Chủ đầu tư cần nắm quy trình sử dụng, quản trị nghiệp vụ, giám sát chất lượng và khai thác dữ liệu."
    ),
    "X. TÀI LIỆU BÀN GIAO": (
        "Danh mục tài liệu bàn giao được xác định theo mục tiêu quản lý, khai thác, nghiệm thu và giám sát dịch vụ. Trọng tâm là tài liệu thiết kế, tài liệu yêu cầu dịch vụ, hướng dẫn sử dụng, hướng dẫn quản trị phía Chủ đầu tư, tài liệu nghiệm thu và dữ liệu phát sinh trong thời gian thuê dịch vụ."
    ),
}


def replace_text(text: str) -> str:
    for src, dst in REPLACEMENTS:
        text = text.replace(src, dst)
    return text


def set_para_text(paragraph, text: str) -> None:
    if paragraph.runs:
        paragraph.runs[0].text = text
        for run in paragraph.runs[1:]:
            run.text = ""
    else:
        paragraph.add_run(text)


def insert_after(paragraph, text: str):
    new_p = paragraph.insert_paragraph_before("")
    paragraph._p.addnext(new_p._p)
    new_p.add_run(text)
    return new_p


def main():
    doc = Document(str(SRC))

    for para in doc.paragraphs:
        new_text = replace_text(para.text)
        if new_text != para.text:
            set_para_text(para, new_text)

    for table in doc.tables:
        for row in table.rows:
            for cell in row.cells:
                for para in cell.paragraphs:
                    new_text = replace_text(para.text)
                    if new_text != para.text:
                        set_para_text(para, new_text)

    inserted = 0
    for para in list(doc.paragraphs):
        key = " ".join(para.text.split())
        if key in ANALYSIS_AFTER:
            inserted_p = insert_after(para, ANALYSIS_AFTER[key])
            inserted_p.style = doc.styles["Normal"]
            inserted += 1

    doc.core_properties.title = "Phần 7 - Thiết kế chi tiết - Việt hóa thuật ngữ và tăng phân tích"
    doc.core_properties.subject = "Bản v3.3 theo góp ý tư vấn thiết kế chi tiết thuê dịch vụ CNTT"
    doc.save(str(OUT))
    print(f"inserted_analysis={inserted}")
    print(str(OUT).encode("ascii", errors="backslashreplace").decode("ascii"))


if __name__ == "__main__":
    main()
