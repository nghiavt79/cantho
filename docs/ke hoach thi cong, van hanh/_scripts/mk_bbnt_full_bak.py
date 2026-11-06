import sys, os
HERE = os.path.dirname(os.path.abspath(__file__))
src = open(os.path.join(HERE, 'mk_bo_ho_so.py'), encoding='utf-8').read()
src = src[:src.index('# 01 ----')].replace('OUT = sys.argv[1]\nos.makedirs(OUT, exist_ok=True)', '')
exec(src)
ST = 'Hoàn tất'
OUT = sys.argv[1]  # thư mục xuất

D = Doc()
D.header('BIÊN BẢN NGHIỆM THU GIAI ĐOẠN', 'Đợt 1 – làm căn cứ thanh toán lần 1 (DỰ THẢO)')
D.can_cu(['Căn cứ kết quả kiểm tra, vận hành thử thực tế của các bên;'])
D.thanh_phan(vtisc=False)
D.p('Hai bên tiến hành kiểm tra, nghiệm thu khối lượng công việc đợt 1 như sau:', indent=True)

D.h('I. Nội dung kiểm tra')
I = [
    ('Hạ tầng 02 máy chủ cloud: CPU Intel Xeon Gold 6330 hoặc tương đương, tối thiểu 8 lõi, 32 GB RAM, 500 GB SSD, 01 IP; IPv4/IPv6; sao lưu, giám sát; dữ liệu đặt tại Việt Nam'),
    ('Bản quyền: Windows Server 2025 Standard 16 lõi kèm User CAL; SQL Server 2025 Standard kèm User CAL. Số chứng từ: ……………………'),
    ('Cài đặt ứng dụng trên máy chủ cloud; tên miền, chứng thư bảo mật, tường lửa'),
    ('Cấu hình danh mục, tham số, quy trình giao dịch 14 bước; tài khoản quản trị, phân quyền; sao lưu tự động hằng ngày, giám sát'),
    ('Dữ liệu khởi tạo (đã đối chiếu nguồn, quyền sử dụng): hồ sơ công nghệ …… ; chuyên gia …… ; nhà cung ứng …… ; tin bài ……'),
    ('Giải pháp tích hợp đã kích hoạt và kết nối (mục 1.4.1 – 1.4.5): tin nhắn thương hiệu, email 60 địa chỉ, token ký số, eContract, gói SMS; trợ lý AI'),
    ('Chức năng phần mềm theo 12 nhóm chức năng Phụ lục 3: 60 chức năng đạt kiểm thử (chi tiết tại Bảng xác định giá trị khối lượng đợt 1)'),
    ('Kiểm thử nội bộ, rà soát lỗ hổng, phục hồi thử bản sao lưu'),
    ('Đào tạo nhóm quản trị, nhóm nghiệp vụ: đã tổ chức ngày ……/……/2026 (…… học viên, danh sách kèm theo); bàn giao tài liệu hướng dẫn sử dụng, tài liệu quản trị, tài khoản thực hành'),
    ('Dự thảo hồ sơ đề xuất cấp độ và phương án an toàn thông tin cấp độ 2'),
]
D.table(['TT', 'Nội dung', 'Kết quả'], [(str(i + 1), x, 'Đạt') for i, x in enumerate(I)], [0.9, 13.6, 2.5], center_cols=(0, 2))

D.h('II. Kết quả vận hành thử')
D.p('Thời gian: từ ngày ……/10/2026 đến ngày ……/10/2026, theo kịch bản nghiệm thu tại phần V Chương V Hồ sơ mời thầu.', indent=True)
steps = ['Đăng ký nhà cung cấp', 'Duyệt hồ sơ nhà cung cấp', 'Đăng sản phẩm', 'Đăng nhu cầu công nghệ',
         'Mời báo giá, nộp đề xuất', 'Trao đổi trực tuyến', 'Lập hợp đồng điện tử', 'Ký số',
         'Thông báo qua email, SMS', 'Tra cứu nhật ký']
D.table(['Bước', 'Nội dung', 'Kết quả'], [(str(i + 1), s, 'Đạt') for i, s in enumerate(steps)], [1.2, 13.3, 2.5], center_cols=(0, 2))
D.p('Lỗi phát sinh: …… lỗi; đã khắc phục: …… lỗi; còn tồn tại: ……………………', indent=True)
D.p('Đối chiếu quy trình giao dịch 14 bước với kịch bản kiểm thử:', indent=True)
W14 = [
    ('1', 'Tạo yêu cầu chuyển giao', 'Kiểm thử bước 4 (đăng nhu cầu, gửi yêu cầu)'),
    ('2', 'Ký thỏa thuận bảo mật (khi sản phẩm yêu cầu)', 'Kiểm thử bổ sung'),
    ('3', 'Tạo yêu cầu báo giá, mời nhà cung cấp', 'Kiểm thử bước 5'),
    ('4', 'Nộp và chấm điểm hồ sơ đề xuất', 'Kiểm thử bước 5'),
    ('5', 'Đàm phán thương mại', 'Kiểm thử bước 6'),
    ('6', 'Thẩm định pháp lý', 'Kiểm thử bổ sung'),
    ('7', 'Lập và ký hợp đồng điện tử', 'Kiểm thử bước 7, 8'),
    ('8', 'Xác nhận tạm ứng', 'Kiểm thử bổ sung'),
    ('9', 'Thử nghiệm', 'Kiểm thử bổ sung'),
    ('10', 'Bàn giao và triển khai', 'Kiểm thử bổ sung'),
    ('11', 'Đào tạo và chuyển giao vận hành', 'Kiểm thử bổ sung'),
    ('12', 'Bàn giao hồ sơ kỹ thuật', 'Kiểm thử bổ sung'),
    ('13', 'Nghiệm thu', 'Kiểm thử bổ sung'),
    ('14', 'Thanh lý hợp đồng', 'Kiểm thử bổ sung'),
]
D.table(['Bước quy trình', 'Nội dung', 'Kiểm thử tương ứng', 'Kết quả'], [x + ('Đạt',) for x in W14], [1.8, 7.2, 5.5, 2.5], center_cols=(0, 3))
D.p('Kịch bản 10 bước (phần V Chương V) kiểm tra các bước quy trình 1, 3, 4, 5, 7 và các bước chuẩn bị (đăng ký, duyệt nhà cung cấp, đăng sản phẩm). Các bước 2, 6, 8 – 14 được kiểm thử bổ sung trên cùng hồ sơ giao dịch. Thông báo email, SMS và nhật ký (kiểm thử bước 9, 10) được kiểm tra ở tất cả 14 bước quy trình.', italic=True, indent=True)

D.h('III. Giá trị nghiệm thu đợt 1')
D.table(['Mục', 'Hạng mục', 'Đợt 1 (đồng, sau thuế)', 'Tỷ lệ'], [
    ('1.1', 'Thiết lập trang web Sàn giao dịch công nghệ Cần Thơ', '890.782.296', '75%'),
    ('1.2', 'Cài đặt, cấu hình, hướng dẫn sử dụng, vận hành, bảo hành, bảo trì, hỗ trợ kỹ thuật 05 năm', '447.020.320', '80%'),
    ('1.3', 'Bản quyền Windows Server, SQL Server', '157.290.000', '100%'),
    ('1.4', 'Giải pháp tích hợp (1.4.1 – 1.4.5): tin nhắn thương hiệu, email, token ký số, eContract, gói SMS', '88.344.602', '100%'),
    ('2', 'Thuê hạ tầng máy chủ cloud', '693.000.000', '100%'),
    ('', 'Tổng cộng đợt 1 (theo Phụ lục 5)', '2.276.437.218', ''),
], [1.4, 10, 3.6, 2], center_cols=(0, 3), bold_rows=(5,))
D.p('Chi tiết tại Bảng xác định giá trị khối lượng đợt 1 kèm theo.', italic=True, indent=True)

D.h('IV. Phần chuyển đợt 2')
D.bullets(['Mục 1.1 (25%): VR360 trên giao diện (B.1.2, C.1); tích hợp sàn giao dịch khác (C.2); phần gọi video của C.7; kiểm thử toàn bộ, nghiệm thu đưa vào vận hành;',
           'Giải pháp VR360: 10 điểm cảnh quay dựng mới;',
           'Mục 1.2 (20%): vận hành chính thức, bảo hành, bảo trì, hỗ trợ kỹ thuật 05 năm; nghiệm thu, bàn giao hồ sơ.'])

D.ket_luan('Hai bên thống nhất nghiệm thu khối lượng công việc đợt 1 với giá trị …………………… đồng, làm căn cứ thanh toán lần 1 theo Điều 5 hợp đồng.')
D.ky()
D.d.add_page_break()
D.p('DANH SÁCH HỌC VIÊN', bold=True, align=C, size=14)
D.p('(Kèm theo Biên bản nghiệm thu giai đoạn đợt 1)', italic=True, align=C)
D.table(['TT', 'Họ và tên', 'Đơn vị', 'Chức vụ', 'Nhóm', 'Ký tên'],
        [(str(i), '', '', '', '', '') for i in range(1, 16)], [0.9, 4.5, 3.6, 2.8, 2.2, 3])

D.d.core_properties.author = 'NghiaVT'; D.d.core_properties.last_modified_by = 'NghiaVT'
D.d.core_properties.title = 'Biên bản nghiệm thu giai đoạn đợt 1'
path = os.path.join(OUT, 'Bien_ban_nghiem_thu_giai_doan_dot1.docx')
D.d.save(path); print('ok', path)
