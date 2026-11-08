from __future__ import annotations

from pathlib import Path

from docx import Document


ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "docs" / "ho so ky thuat" / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.0.docx"
OUT = ROOT / "docs" / "ho so ky thuat" / "PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v3.1_bam_sat_chuc_nang.docx"


FUNCTION_ROWS = [
    ["1", "Quản trị hệ thống", "Đăng nhập, đăng xuất, quên mật khẩu.\nQuản lý tài khoản và hồ sơ người dùng.\nPhân quyền theo vai trò và chức năng.\nNhật ký thao tác (Log).\nCấu hình hệ thống, tham số vận hành.\nThông báo qua Email/Web.\nGiao diện tương thích PC, Tablet, Mobile."],
    ["2", "Quản lý nhà cung cấp", "Quản lý hồ sơ pháp lý.\nDanh mục lĩnh vực công nghệ.\nQuản lý gian hàng.\nQuản lý hạng thành viên.\nDuyệt hồ sơ và sản phẩm.\nQuản lý bài viết, tin giới thiệu.\nTheo dõi lịch sử giao dịch và lịch sử chỉnh sửa."],
    ["3", "Quản lý sản phẩm và công nghệ", "Danh mục nhóm sản phẩm.\nDanh mục sản phẩm.\nHồ sơ công nghệ.\nHình ảnh, video, tài liệu.\nQuản lý phòng trưng bày.\nQuản lý vị trí trưng bày.\nQuản lý trạng thái pháp lý sản phẩm."],
    ["4", "Quản lý sự kiện - triển lãm", "Tạo và quản lý hội thảo.\nQuản lý triển lãm.\nĐăng ký tham dự.\nQuản lý gian hàng sự kiện.\nBanner quảng bá.\nNhắc lịch qua SMS, Email.\nThống kê lượt quan tâm và lượt tham gia."],
    ["5", "Quản lý người mua", "Hồ sơ người mua.\nĐăng nhu cầu tìm mua.\nGiỏ hàng.\nBình luận, đánh giá.\nLịch sử mua hàng.\nLịch sử trao đổi.\nQuản lý khiếu nại và hỗ trợ."],
    ["6", "Quản lý giao dịch", "Báo giá.\nĐơn hàng.\nHợp đồng.\nDoanh thu.\nHoa hồng.\nThu chi.\nTheo dõi trạng thái giao dịch."],
    ["7", "Cổng thông tin Sàn giao dịch", "Trang chủ.\nTin tức.\nSự kiện.\nBanner.\nDanh mục công nghệ.\nTra cứu sản phẩm.\nTìm kiếm nâng cao.\nĐăng ký nhận thông tin."],
    ["8", "Tiện ích và tích hợp", "Chat trực tuyến.\nVideo Call.\nLivestream.\nMã QR sản phẩm.\nLiên kết mạng xã hội.\nTích hợp sàn khác.\nSong ngữ Việt - Anh.\nTham quan VR360."],
    ["9", "Ứng dụng AI", "AI Chatbot tư vấn.\nAI tìm kiếm thông minh.\nTìm kiếm bằng giọng nói.\nGợi ý sản phẩm.\nGợi ý nhà cung cấp.\nGợi ý công nghệ phù hợp."],
    ["10", "Giải pháp tích hợp", "VR360.\nSMS Brandname.\nEmail.\nChữ ký số.\nHợp đồng điện tử.\nThông báo tự động.\nKết nối API tích hợp."],
    ["11", "Hạ tầng triển khai", "Máy chủ ứng dụng.\nBản quyền hệ điều hành.\nBản quyền CSDL.\nSao lưu dữ liệu.\nGiám sát hệ thống.\nBảo mật và an toàn thông tin.\nĐào tạo, chuyển giao.\nBảo hành, bảo trì 05 năm."],
    ["12", "Báo cáo - thống kê", "Báo cáo nhà cung cấp.\nBáo cáo sản phẩm.\nBáo cáo giao dịch.\nBáo cáo doanh thu.\nBáo cáo sự kiện.\nThống kê lượt truy cập.\nXuất dữ liệu Excel/PDF."],
]

TECH_ROWS = [
    ["Quản trị hệ thống", "Thiết kế phân hệ xác thực, phân quyền, quản lý tài khoản, cấu hình, log, thông báo và giao diện responsive; là lớp nền cho tất cả phân hệ nghiệp vụ."],
    ["Quản lý nhà cung cấp", "Thiết kế hồ sơ pháp lý, lĩnh vực công nghệ, gian hàng, hạng thành viên, quy trình duyệt, bài viết giới thiệu và lịch sử giao dịch/chỉnh sửa."],
    ["Quản lý sản phẩm và công nghệ", "Thiết kế danh mục nhóm sản phẩm, hồ sơ công nghệ, thiết bị, tài sản trí tuệ, hình ảnh/video/tài liệu, phòng trưng bày, vị trí trưng bày và trạng thái pháp lý."],
    ["Quản lý sự kiện - triển lãm", "Thiết kế quản lý hội thảo, triển lãm, đăng ký tham dự, gian hàng sự kiện, banner quảng bá, nhắc lịch SMS/Email và thống kê lượt quan tâm/tham gia."],
    ["Quản lý người mua", "Thiết kế hồ sơ người mua, nhu cầu tìm mua, giỏ hàng, bình luận/đánh giá, lịch sử mua hàng, lịch sử trao đổi, khiếu nại và hỗ trợ."],
    ["Quản lý giao dịch", "Thiết kế báo giá, đơn hàng, hợp đồng, doanh thu, hoa hồng, thu chi và theo dõi trạng thái giao dịch; liên kết với hợp đồng điện tử/ký số."],
    ["Cổng thông tin Sàn giao dịch", "Thiết kế trang chủ, tin tức, sự kiện, banner, danh mục công nghệ, tra cứu sản phẩm, tìm kiếm nâng cao và đăng ký nhận thông tin."],
    ["Tiện ích và tích hợp", "Thiết kế chat trực tuyến, video call, livestream, QR sản phẩm, chia sẻ mạng xã hội, tích hợp sàn khác, song ngữ Việt - Anh và VR360 theo phạm vi triển khai."],
    ["Ứng dụng AI", "Thiết kế AI chatbot tư vấn, AI tìm kiếm thông minh, tìm kiếm bằng giọng nói, gợi ý sản phẩm, gợi ý nhà cung cấp và gợi ý công nghệ phù hợp."],
    ["Giải pháp tích hợp", "Thiết kế tích hợp VR360, SMS Brandname, Email, chữ ký số, hợp đồng điện tử, thông báo tự động và API tích hợp."],
    ["Hạ tầng triển khai", "Thiết kế máy chủ ứng dụng, hệ điều hành, CSDL, backup, giám sát, bảo mật, đào tạo/chuyển giao và bảo hành/bảo trì 05 năm."],
    ["Báo cáo - thống kê", "Thiết kế báo cáo nhà cung cấp, sản phẩm, giao dịch, doanh thu, sự kiện, thống kê lượt truy cập và xuất Excel/PDF."],
]


def table_after(doc, paragraph, headers, rows):
    tbl = doc.add_table(rows=1, cols=len(headers))
    tbl.style = "Table Grid"
    for i, h in enumerate(headers):
        tbl.rows[0].cells[i].text = h
    for row in rows:
        cells = tbl.add_row().cells
        for i, v in enumerate(row):
            cells[i].text = v
    paragraph._p.addnext(tbl._tbl)
    return tbl


def insert_paragraph_after(paragraph, text, style=None):
    new_p = paragraph.insert_paragraph_before("")
    paragraph._p.addnext(new_p._p)
    if style:
        new_p.style = style
    new_p.add_run(text)
    return new_p


def main():
    doc = Document(str(SRC))

    inserted_function_table = False
    inserted_tech_table = False
    for idx, para in enumerate(doc.paragraphs):
        text = " ".join(para.text.split())
        if text == "2. Yêu cầu cụ thể:" and not inserted_function_table:
            intro = insert_paragraph_after(
                para,
                "Các yêu cầu cụ thể của dự án được xác định theo 12 nhóm chức năng chính sau đây. Đây là cơ sở để thiết kế kiến trúc, cơ sở dữ liệu, phân hệ phần mềm, phương án tích hợp, kế hoạch triển khai, đào tạo và nghiệm thu.",
            )
            table_after(doc, intro, ["STT", "Nhóm chức năng", "Mô tả chi tiết"], FUNCTION_ROWS)
            inserted_function_table = True
        if text == "1. Quan điểm xây dựng" and not inserted_tech_table:
            # Find the paragraph after the bullet group by inserting immediately after heading with a short bridge.
            bridge = insert_paragraph_after(
                para,
                "Thiết kế giải pháp kỹ thuật được xây dựng bám sát 12 nhóm chức năng trọng tâm của Sàn giao dịch Khoa học và Công nghệ như sau:",
            )
            table_after(doc, bridge, ["Nhóm chức năng", "Định hướng thiết kế kỹ thuật"], TECH_ROWS)
            inserted_tech_table = True

    doc.core_properties.title = "Phần 7 - Thiết kế chi tiết - Bám sát 12 nhóm chức năng"
    doc.core_properties.subject = "Bản v3.1 cập nhật theo danh mục chức năng người dùng cung cấp"
    doc.save(str(OUT))
    print(str(OUT).encode("ascii", errors="backslashreplace").decode("ascii"))
    print(f"inserted_function_table={inserted_function_table}, inserted_tech_table={inserted_tech_table}")


if __name__ == "__main__":
    main()
