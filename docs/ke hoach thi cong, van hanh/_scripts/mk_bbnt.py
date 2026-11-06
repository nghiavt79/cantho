import sys, os
HERE = os.path.dirname(os.path.abspath(__file__))
src = open(os.path.join(HERE, 'mk_bo_ho_so.py'), encoding='utf-8').read()
src = src[:src.index('# 01 ----')].replace('OUT = sys.argv[1]\nos.makedirs(OUT, exist_ok=True)', '')
exec(src)
ST = 'Hoàn tất'
OUT = sys.argv[1]  # thư mục xuất

D = Doc()
D.header('BIÊN BẢN NGHIỆM THU GIAI ĐOẠN', 'Đợt 1 (DỰ THẢO)')
D.can_cu(['Căn cứ Phụ lục 5 – Giá trị nghiệm thu dự kiến đợt 1;'])
D.thanh_phan(vtisc=False)
D.p('Hai bên đã kiểm tra thực tế và thống nhất nghiệm thu khối lượng công việc đợt 1 như sau:', indent=True)
D.table(['Mục', 'Hạng mục nghiệm thu', 'Giá trị đợt 1 (đồng)', 'Kết quả', 'Hồ sơ kèm theo'], [
    ('1.1', 'Thiết lập trang web Sàn: 60 chức năng theo 12 nhóm chức năng Phụ lục 3', '890.782.296', 'Đạt', 'Bảng xác định giá trị (Mục 1.1)'),
    ('1.2', 'Cài đặt, cấu hình, tích hợp, dữ liệu khởi tạo, hướng dẫn sử dụng, đào tạo, vận hành thử', '447.020.320', 'Đạt', 'Bảng xác định giá trị (Mục 1.2); danh sách học viên'),
    ('1.3', 'Bản quyền Windows Server 2025, SQL Server 2025 (1.3.1 – 1.3.4)', '157.290.000', 'Đạt', 'Chứng từ bản quyền'),
    ('1.4', 'Giải pháp tích hợp (1.4.1 – 1.4.5): tin nhắn thương hiệu, email, token ký số, eContract, gói SMS', '88.344.602', 'Đạt', 'Chứng từ kích hoạt dịch vụ'),
    ('2', 'Thuê hạ tầng 02 máy chủ cloud', '693.000.000', 'Đạt', 'Thông tin máy chủ bàn giao'),
    ('', 'Tổng cộng đợt 1 (theo Phụ lục 5)', '2.276.437.218', '', ''),
], [1.1, 7.2, 2.9, 1.5, 4.3], center_cols=(0, 3), bold_rows=(5,))
D.p('Phần thực hiện ở đợt 2: VR360 (10 điểm cảnh quay dựng mới); tích hợp sàn giao dịch khác; gọi video; vận hành, bảo hành, bảo trì, hỗ trợ kỹ thuật 05 năm; nghiệm thu đưa vào vận hành chính thức.', italic=True, indent=True)
D.ket_luan('Hai bên thống nhất nghiệm thu khối lượng công việc đợt 1 với giá trị 2.276.437.218 đồng (đã gồm thuế), làm căn cứ thanh toán lần 1 theo Điều 5 hợp đồng.')
D.ky()

D.d.core_properties.author = 'NghiaVT'; D.d.core_properties.last_modified_by = 'NghiaVT'
D.d.core_properties.title = 'Biên bản nghiệm thu giai đoạn đợt 1'
path = os.path.join(OUT, 'Bien_ban_nghiem_thu_giai_doan_dot1.docx')
D.d.save(path); print('ok', path)
