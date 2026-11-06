"""Dữ liệu dùng chung: 60 chức năng đợt 1 theo 12 nhóm Phụ lục 3, lịch thực hiện, giá trị theo Phụ lục 5 (bản chị Vân xác nhận 21/10)."""

T11, D11 = 1187709727, 890782296      # mục 1.1: giá HĐ, đợt 1 (75%)
T12, D12 = 558775400, 447020320       # mục 1.2: giá HĐ, đợt 1 (80%)
TONG_HD, TONG_D1 = 2730119729, 2276437218

# (số nhóm, tên nhóm, ngày thực hiện, [(mã, tên chức năng)])
NHOM = [
    (1, 'Quản trị hệ thống', '21/10', [
        ('A.1.1', 'Module xác thực'), ('A.1.2', 'Module quản trị người dùng'), ('A.1.3', 'Module phân quyền người dùng'),
        ('A.1.4', 'Module thông báo'), ('A.1.5', 'Tiện ích (tương thích nhiều thiết bị, giao diện trực quan)')]),
    (2, 'Quản lý tổ chức, doanh nghiệp và thành viên', '22/10', [
        ('A.2.2', 'Danh mục nhà cung cấp (người bán)'), ('A.2.3', 'Quản lý gian hàng nhà cung cấp'),
        ('A.2.4', 'Quản lý hạng nhà cung cấp'), ('A.2.8', 'Quản lý các bài viết của nhà cung cấp'),
        ('A.2.9', 'Ủy quyền bán hàng cho sàn'), ('A.5.1', 'Quản lý danh sách người mua'),
        ('B.2.1', 'Quản lý gian hàng người bán'), ('B.2.2', 'Hồ sơ pháp lý')]),
    (3, 'Quản lý công nghệ, thiết bị và sản phẩm KH&CN', '23/10', [
        ('A.2.1', 'Danh mục lĩnh vực công nghệ'), ('A.2.6', 'Duyệt các chỉnh sửa và thêm mới của sản phẩm'),
        ('A.2.7', 'Quản lý lịch sử chỉnh sửa sản phẩm của nhà cung cấp'), ('A.3.1', 'Danh mục nhóm sản phẩm'),
        ('A.3.2', 'Danh mục sản phẩm'), ('A.8.3', 'Quản lý phiên bản nội dung'),
        ('B.2.5', 'Quản lý sản phẩm'), ('C.5', 'In mã QR cho sản phẩm')]),
    (4, 'Quản lý kết nối cung – cầu công nghệ', '24/10', [
        ('A.2.5', 'Quản lý giao dịch của nhà cung cấp'), ('A.2.10', 'Quản lý lịch sử chat (người bán)'),
        ('A.5.2', 'Quản lý lịch sử mua hàng người mua'), ('A.5.3', 'Quản lý lịch sử chat (người mua)'),
        ('B.2.3', 'Quản lý lịch sử giao dịch của người bán'), ('B.2.4', 'Báo giá'),
        ('B.3.1', 'Đăng tin tìm mua sản phẩm'), ('B.3.3', 'Quan tâm sản phẩm'), ('B.3.4', 'Giỏ hàng sản phẩm'),
        ('C.7', 'Liên hệ trực tiếp – phần chat trực tiếp')]),
    (5, 'Quản lý sự kiện, hội thảo và triển lãm', '25/10', [
        ('A.4.1', 'Quản lý sự kiện, hội thảo, triển lãm'), ('A.4.3', 'Cấu hình gian hàng, giá chi tiết cho từng sự kiện'),
        ('A.4.4', 'Thống kê lượt nhấp, lượt quan tâm'), ('A.4.5', 'Nhắc lịch'), ('A.8.4', 'Thông báo nhắc lịch'),
        ('C.6', 'Phòng họp ảo / Livestream')]),
    (6, 'Cổng thông tin điện tử', '26/10', [
        ('A.4.2', 'Quản lý tin tức, bài đăng'), ('A.5.4', 'Quản lý lịch sử bình luận'), ('A.7.1', 'Bình luận'),
        ('A.7.2', 'Quản lý thông tin các mẫu đăng ký'), ('A.7.3', 'Câu hỏi thường gặp (FAQ)'),
        ('B.1.1', 'Giao diện trang chủ'), ('B.1.3', 'Các thông tin nổi bật'), ('B.1.4', 'Danh sách sản phẩm'),
        ('B.1.5', 'Quản lý banner'), ('B.1.6', 'Các mẫu đăng ký nhận thông tin'), ('B.3.2', 'Bình luận tin tìm mua'),
        ('B.3.5', 'Bình luận sản phẩm'), ('C.3', 'Liên kết với các nền tảng mạng xã hội'), ('C.8', 'Dịch Anh/Việt')]),
    (7, 'Không gian triển lãm số', '25/10', [
        ('A.3.3', 'Danh mục phòng trưng bày'), ('A.3.4', 'Danh mục vị trí trưng bày')]),
    (8, 'Ứng dụng AI', '27/10', [
        ('C.9', 'AI tìm kiếm (tìm kiếm thông minh, tìm kiếm bằng giọng nói)'), ('C.10', 'Chatbot AI')]),
    (9, 'Tích hợp và chia sẻ dữ liệu', '27/10', [
        ('C.4', 'Ký số hợp đồng điện tử')]),
    (10, 'Quản trị dữ liệu và an toàn thông tin', '21/10', [
        ('A.8.5', 'Lịch sử thao tác (nhật ký)')]),
    (11, 'Hạ tầng và vận hành dịch vụ', '', []),
    (12, 'Báo cáo và thống kê', '27/10', [
        ('A.6.1', 'Quản lý thu chi'), ('A.8.1', 'Báo cáo giao dịch, cảnh báo giao dịch mới'),
        ('A.8.2', 'Cảnh báo sản phẩm trùng lặp, quá hạn')]),
]
DOT2_11 = [
    ('B.1.2', 'Tham quan thực tế ảo VR360 (nhóm 7)'), ('C.1', 'Tham quan VR360 (nhóm 7)'),
    ('C.2', 'Tích hợp với sàn giao dịch khác, gồm Sàn giao dịch KH&CN Việt Nam (nhóm 9)'),
    ('C.7', 'Liên hệ trực tiếp – phần gọi video (nhóm 4)'),
]
N_CN = sum(len(g[3]) for g in NHOM)
assert N_CN == 60, N_CN


def tien_nhom():
    """Chia giá trị đợt 1 mục 1.1 theo số chức năng; nhóm cuối nhận phần làm tròn."""
    out, used = {}, 0
    co_cn = [g for g in NHOM if g[3]]
    for g in co_cn[:-1]:
        v = round(D11 * len(g[3]) / N_CN); out[g[0]] = v; used += v
    out[co_cn[-1][0]] = D11 - used
    return out


# Mục 1.2: (mã, công việc, tỷ trọng % trên giá mục 1.2, thời gian, chi tiết)
CV12 = [
    ('1.2.1', 'Cài đặt hệ thống trên hạ tầng của gói thầu', 15, '25 – 26/10',
     'Cài hệ điều hành, cơ sở dữ liệu trên 02 máy chủ cloud; cài ứng dụng; tên miền, chứng thư bảo mật, tường lửa'),
    ('1.2.2', 'Cấu hình hệ thống', 15, '21 – 23/10',
     'Danh mục dùng chung; tham số, mẫu thông báo; quy trình giao dịch 14 bước; tài khoản, vai trò, phân quyền'),
    ('1.2.3', 'Tích hợp, đồng bộ dữ liệu với các giải pháp kèm theo (mục 1.4)', 15, '22 – 26/10',
     'SMS, thư điện tử, token ký số, hợp đồng điện tử, trợ lý AI; hàng đợi gửi, nhật ký, tự gửi lại khi lỗi'),
    ('1.2.4', 'Chuẩn hóa, nhập dữ liệu khởi tạo', 10, '21 – 26/10',
     'Kiểm kê, đối chiếu nguồn và quyền sử dụng; nhập hồ sơ công nghệ, chuyên gia, nhà cung ứng, tin bài'),
    ('1.2.5', 'Hướng dẫn sử dụng, đào tạo', 10, 'Đã thực hiện',
     'Đã tổ chức buổi đào tạo nhóm quản trị, nhóm nghiệp vụ; bàn giao tài liệu hướng dẫn sử dụng, tài liệu quản trị, tài khoản thực hành'),
    ('1.2.6', 'Thiết lập vận hành và vận hành thử', 15, '27 – 30/10',
     'Giám sát, sao lưu tự động, phục hồi thử; kênh hỗ trợ; vận hành thử theo kịch bản nghiệm thu, khắc phục lỗi'),
]
CV12_DOT2 = [('Vận hành chính thức, bảo hành, bảo trì, hỗ trợ kỹ thuật 05 năm', 15), ('Nghiệm thu, bàn giao hồ sơ, đưa vào vận hành chính thức', 5)]


def tien_12():
    out, used = [], 0
    for i, c in enumerate(CV12):
        v = round(T12 * c[2] / 100) if i < len(CV12) - 1 else D12 - used
        out.append(v); used += v
    return out


# Mục 1.3, 1.4, 2 theo Phụ lục 5 (sau thuế): (mã, nội dung, giá trị, thời gian, công việc)
KHAC = [
    ('1.3.1', 'Windows Server 2025 Standard – 16 Core License Pack', 56000000, '21 – 24/10', 'Cấp, kích hoạt bản quyền trên máy chủ; bàn giao chứng từ'),
    ('1.3.2', 'Windows Server 2025 – 1 User CAL', 3010000, '21 – 24/10', 'Cấp, kích hoạt; bàn giao chứng từ'),
    ('1.3.3', 'SQL Server 2025 Standard edition Perpetual 1 Server License', 77000000, '21 – 24/10', 'Cấp, kích hoạt bản quyền; bàn giao chứng từ'),
    ('1.3.4', 'SQL Server 2025 – 1 User CAL', 21280000, '21 – 24/10', 'Cấp, kích hoạt; bàn giao chứng từ'),
    ('1.4.1', 'Giải pháp tin nhắn thương hiệu (05 năm, mỗi năm 3.600 SMS)', 27500000, '21 – 26/10', 'Đăng ký thương hiệu, kích hoạt, kết nối hệ thống, gửi thử'),
    ('1.4.2', 'Email Hosting gói 3 – 60 tháng (60 địa chỉ, 25 GB, 05 Mail List)', 8250000, '21 – 23/10', 'Khởi tạo hộp thư, nhóm thư; kết nối gửi thư tự động'),
    ('1.4.3', 'Gói combo TOKEN (03 năm, miễn phí thêm 02 năm)', 2594602, '22 – 24/10', 'Cấp token, cài đặt, ký thử trên hợp đồng PDF'),
    ('1.4.4', 'Gói eContract 10.000 tài liệu', 39000000, '22 – 26/10', 'Kích hoạt gói; kết nối luồng lập – ký – lưu trữ hợp đồng'),
    ('1.4.5', 'Gói SMS 5.000 tin nhắn (02 gói)', 11000000, '22 – 24/10', 'Kích hoạt gói; kết nối gửi thông báo, mã xác thực'),
    ('2', 'Thuê hạ tầng máy chủ cloud (02 máy chủ)', 693000000, '21 – 24/10', 'Bàn giao 02 máy chủ đúng cấu hình; sao lưu, giám sát'),
]
assert D11 + D12 + sum(k[2] for k in KHAC) == TONG_D1
