from pathlib import Path

from docx import Document


SRC = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.5.1_hoan_thien_SLA_tich_hop_viet_hoa.docx"
)
DST = Path(
    r"D:\2026\CanTho\docs\ho so ky thuat\PHAN_7_THIET_KE_CHI_TIET_San_GD_KHCN_v4.6_chuan_hoa_so_lieu_MobiFone.docx"
)


doc = Document(SRC)


def replace_in_runs(paragraph, mapping):
    for run in paragraph.runs:
        if not run.text:
            continue
        new_text = run.text
        for old, new in mapping.items():
            new_text = new_text.replace(old, new)
        if new_text != run.text:
            run.text = new_text


# 1. Chuẩn hóa thuật ngữ và định nghĩa SLA ngay lần xuất hiện đầu tiên.
paragraph_replacements = {
    "bảo đảm mức chất lượng dịch vụ (SLA) theo hợp đồng": (
        "bảo đảm Thỏa thuận mức dịch vụ (Service Level Agreement - SLA) theo hợp đồng"
    ),
    "duy trì mức chất lượng dịch vụ trong suốt thời gian khai thác": (
        "duy trì mức chất lượng dịch vụ theo SLA trong suốt thời gian khai thác"
    ),
    "Dịch vụ phải đáp ứng các nhóm chức năng nghiệp vụ của Sàn giao dịch Khoa học và Công nghệ.": (
        "Hệ thống phải đáp ứng các nhóm chức năng nghiệp vụ của Sàn giao dịch Khoa học và Công nghệ."
    ),
    "Dịch vụ phải có khả năng mở rộng, tích hợp, sao lưu, khôi phục và duy trì vận hành liên tục theo yêu cầu hợp đồng.": (
        "Dịch vụ CNTT phải bảo đảm Hệ thống có khả năng mở rộng, tích hợp, sao lưu, khôi phục và duy trì vận hành liên tục theo yêu cầu hợp đồng."
    ),
    "Dịch vụ phải bảo đảm an toàn thông tin, bảo vệ dữ liệu cá nhân và phân quyền truy cập phù hợp với vai trò người dùng.": (
        "Hệ thống phải bảo đảm an toàn thông tin, bảo vệ dữ liệu cá nhân và phân quyền truy cập phù hợp với vai trò người dùng."
    ),
    "Dịch vụ phải có giao diện web thân thiện, tương thích thiết bị phổ biến, hỗ trợ quản trị nội dung và báo cáo thống kê.": (
        "Hệ thống phải có giao diện web thân thiện, tương thích thiết bị phổ biến, hỗ trợ quản trị nội dung và báo cáo thống kê."
    ),
    "Dịch vụ phải có tài liệu hướng dẫn, đào tạo, hỗ trợ vận hành và cơ chế bảo hành/bảo trì trong suốt thời gian thuê.": (
        "Dịch vụ CNTT phải có tài liệu hướng dẫn, đào tạo, hỗ trợ vận hành và cơ chế bảo hành/bảo trì trong suốt thời gian thuê."
    ),
    "Các chỉ tiêu quy mô khai thác, hiệu năng và mức chất lượng dịch vụ (SLA) dự kiến": (
        "Các chỉ tiêu quy mô khai thác, hiệu năng và mức chất lượng dịch vụ theo SLA dự kiến"
    ),
    "Thỏa thuận mức dịch vụ (Service Level Agreement - SLA) được hiểu là": (
        "Thỏa thuận mức dịch vụ (Service Level Agreement - SLA) xác định"
    ),
    "Người dùng truy cập dịch vụ qua kênh web bảo mật; hệ thống xác thực và phân quyền theo vai trò.": (
        "Người dùng truy cập Hệ thống qua kênh web bảo mật; Hệ thống thực hiện xác thực và phân quyền theo vai trò."
    ),
    "toàn bộ dịch vụ": "toàn bộ Hệ thống",
    "khả năng vận hành an toàn của toàn bộ dịch vụ": "khả năng vận hành an toàn của toàn bộ Hệ thống",
}

for p in doc.paragraphs:
    replace_in_runs(p, paragraph_replacements)

# 2. Cập nhật bảng SLA/quy mô khai thác.
for tbl in doc.tables:
    if len(tbl.rows) > 0 and tbl.cell(0, 0).text.strip() == "Nhóm chỉ tiêu":
        for row in tbl.rows:
            key = row.cells[0].text.strip()
            if key == "Quy mô tài khoản và nhóm người dùng":
                row.cells[1].text = (
                    "Quy mô thiết kế dự kiến tối thiểu 10.000 tài khoản/người dùng có định danh cho các nhóm quản trị, cán bộ nghiệp vụ, lãnh đạo, nhà cung cấp, người mua, chuyên gia; "
                    "đồng thời hỗ trợ khách truy cập công khai không yêu cầu đăng nhập theo chính sách vận hành."
                )
                row.cells[2].text = (
                    "Số lượng chính thức được rà soát khi lập HSMT; yêu cầu tối thiểu này dùng làm cơ sở tính năng lực mở rộng và nghiệm thu."
                )
            elif key == "Truy cập đồng thời":
                row.cells[1].text = (
                    "Đáp ứng tối thiểu 300 phiên/người dùng truy cập đồng thời ở giai đoạn vận hành ban đầu và có khả năng mở rộng lên tối thiểu 1.000 phiên đồng thời khi nhu cầu khai thác tăng."
                )
                row.cells[2].text = (
                    "Kiểm tra qua hồ sơ năng lực, phương án kỹ thuật, báo cáo giám sát và thử nghiệm tải khi cần."
                )
            elif key == "Dung lượng dữ liệu":
                row.cells[1].text = (
                    "Tài nguyên lưu trữ vận hành tham chiếu tối thiểu theo nguồn báo giá: 02 máy chủ, mỗi máy chủ có 500GB SSD; tổng tài nguyên lưu trữ thô tối thiểu 1TB, chưa bao gồm phần mở rộng, sao lưu và dự phòng theo phương án kỹ thuật."
                )
                row.cells[2].text = (
                    "Căn cứ số liệu báo giá MobiFone Cần Thơ; dung lượng chính thức cần tính thêm dữ liệu hình ảnh, video, hợp đồng, nhật ký và tăng trưởng trong 05 năm."
                )
            elif key == "Mức độ sẵn sàng dịch vụ":
                row.cells[1].text = (
                    "Mức chất lượng dịch vụ theo SLA tối thiểu đề xuất: thời gian sẵn sàng từ 99,5%/tháng trở lên, không bao gồm thời gian bảo trì đã thông báo và được phê duyệt."
                )
        break

# 3. Cập nhật bảng Chương VI theo số liệu báo giá và làm rõ phạm vi.
for tbl in doc.tables:
    if len(tbl.rows) > 0 and tbl.cell(0, 1).text.strip() == "Hạng mục dịch vụ/thành phần":
        for row in tbl.rows:
            item = row.cells[1].text.strip()
            if item == "Tài nguyên tính toán, lưu trữ và đường truyền":
                row.cells[2].text = (
                    "Đáp ứng quy mô người dùng, dữ liệu, truy cập, mở rộng và điều kiện an toàn vận hành theo Chương IV; hạ tầng tham chiếu gồm 02 máy chủ, mỗi máy có tối thiểu 8 lõi xử lý, RAM 32GB, SSD 500GB và 01 địa chỉ IP, hoặc cấu hình tương đương/better phù hợp phương án được phê duyệt."
                )
                row.cells[3].text = (
                    "Thuộc gói thuê hạ tầng 05 năm phục vụ cài đặt, vận hành và lưu trữ dữ liệu của Hệ thống."
                )
                row.cells[4].text = (
                    "Bảo đảm tài nguyên, hệ điều hành máy chủ, hệ quản trị cơ sở dữ liệu hoặc tương đương, sao lưu, khôi phục, giám sát, cảnh báo và báo cáo vận hành."
                )
                row.cells[5].text = (
                    "Số liệu tham chiếu từ báo giá MobiFone Cần Thơ; không chỉ định hãng/nhãn hiệu bộ xử lý, hệ điều hành hoặc CSDL cụ thể."
                )
            elif item == "Dịch vụ thư điện tử":
                row.cells[3].text = (
                    "Thuộc gói tích hợp kèm theo: duy trì dịch vụ trong 05 năm; mỗi năm cung cấp và duy trì tối đa 60 địa chỉ thư điện tử (email), dung lượng lưu trữ 25GB SSD/tài khoản, 5 danh sách thư (Mail List)."
                )
                row.cells[5].text = (
                    "Cách hiểu này tránh cộng dồn thành 300 địa chỉ trong 05 năm; nếu HSMT/hợp đồng quy định khác thì áp dụng theo phạm vi được phê duyệt."
                )
            elif item == "Chữ ký số":
                row.cells[3].text = (
                    "Phạm vi gói thuê bao gồm tích hợp và duy trì kết nối kỹ thuật phục vụ chữ ký số trong 05 năm."
                )
                row.cells[5].text = (
                    "Chứng thư số, thiết bị ký, tài khoản ký, hình thức ký từ xa/ký tập trung và số lượng giao dịch ký được xác định riêng trong HSMT/hợp đồng; không mặc nhiên hiểu là cấp không giới hạn chứng thư số."
                )
            elif item == "Trò chuyện trực tuyến, cuộc gọi video, phát trực tuyến":
                row.cells[3].text = (
                    "Tích hợp và cấu hình thuộc gói thuê khi được phê duyệt; phí sử dụng nền tảng bên thứ ba chỉ thuộc gói khi được xác định rõ trong HSMT/hợp đồng."
                )
                row.cells[5].text = (
                    "Nguyên tắc này phân định trách nhiệm kỹ thuật của đơn vị cung cấp dịch vụ và chi phí dịch vụ nền bên ngoài."
                )
            elif item == "Ứng dụng trí tuệ nhân tạo":
                row.cells[3].text = (
                    "Phạm vi gói thuê bao gồm chức năng và tích hợp kỹ thuật của các tiện ích trí tuệ nhân tạo trong Hệ thống; chi phí sử dụng dịch vụ trí tuệ nhân tạo (AI) bên thứ ba được thanh toán theo hạn mức hoặc hợp đồng riêng nếu chưa được đưa vào gói thuê."
                )
                row.cells[5].text = (
                    "Áp dụng phương án chỉ tích hợp thuộc gói khi chưa có dự toán hạn mức sử dụng AI; hạn mức chi phí, dữ liệu huấn luyện và trách nhiệm kiểm soát đầu ra cần quy định trong HSMT/hợp đồng."
                )
            elif item == "Tin nhắn thương hiệu SMS Brandname":
                row.cells[3].text = (
                    "Thuộc gói tích hợp kèm theo: duy trì 05 năm; mỗi năm 3.600 SMS."
                )
            elif item == "Giải pháp thực tế ảo VR360":
                row.cells[3].text = (
                    "Thuộc gói tích hợp kèm theo: 10 điểm cảnh quay dựng mới."
                )
            elif item == "Hợp đồng điện tử":
                row.cells[3].text = (
                    "Thuộc gói tích hợp kèm theo: duy trì 05 năm; mỗi năm 2.000 hợp đồng điện tử."
                )
        break

# 4. Cập nhật bảng nghiệm thu để đồng nhất với phạm vi đã làm rõ.
for tbl in doc.tables:
    if len(tbl.rows) > 0 and tbl.cell(0, 0).text.strip() == "Nhóm nghiệm thu":
        for row in tbl.rows:
            key = row.cells[0].text.strip()
            if key == "Tích hợp":
                row.cells[1].text = (
                    "Kiểm tra thư điện tử (email), tin nhắn thương hiệu SMS Brandname, VR360, chữ ký số, hợp đồng điện tử, thông báo tự động và các giao diện lập trình ứng dụng (API) theo phạm vi đã được phê duyệt; đối chiếu hạn mức/sản lượng tích hợp gồm 10 điểm VR360, 3.600 SMS/năm, 60 địa chỉ thư điện tử/năm, 2.000 hợp đồng điện tử/năm và các điều kiện riêng về chữ ký số."
                )
            elif key == "Chất lượng dịch vụ":
                row.cells[1].text = (
                    "Kiểm tra mức chất lượng dịch vụ theo SLA, thời gian sẵn sàng, thời gian phản hồi, tiếp nhận sự cố, khắc phục/khôi phục dịch vụ, sao lưu, điểm khôi phục dữ liệu, nhật ký vận hành và báo cáo giám sát."
                )
        break

doc.save(DST)
print(DST)
