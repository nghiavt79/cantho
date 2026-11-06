"""Thống nhất số chức năng theo Phụ lục 3 (63 = A37 + B16 + C10; đợt 1: 62) và thêm bảng đối chiếu 14 bước quy trình."""
import os
D = os.path.dirname(os.path.abspath(__file__))


def patch(fn, R):
    p = os.path.join(D, fn)
    s = open(p, encoding='utf-8').read()
    for a, b in R:
        assert a in s, (fn, a[:70])
        s = s.replace(a, b)
    open(p, 'w', encoding='utf-8').write(s)
    print('patched', fn)


patch('mk_dot1.py', [
    ("'B. Website bán hàng – 15 chức năng (trừ B.1.2 VR360)'", "'B. Website bán hàng – 15 chức năng (B.1.2 VR360 tính ở dòng 5)'"),
    ("'C. Tiện ích khác – 7 chức năng đang hoạt động'", "'C. Tiện ích khác – 7 chức năng: C.3, C.5, C.6, C.7 (phần chat), C.8, C.9, C.10'"),
    ("'C.7 Chat trực tiếp giữa người bán, người mua và Sàn',", "'C.7 Liên hệ trực tiếp – phần chat trực tiếp giữa người bán, người mua và Sàn',"),
    ("('4', 'C.4 Ký số hợp đồng điện tử", "('4', 'C.4 Ký số hợp đồng điện tử (1 chức năng)"),
    ("'B.1.2, C.1 Tham quan VR360 trên giao diện Sàn (10 điểm cảnh)'", "'B.1.2, C.1 Tham quan VR360 trên giao diện Sàn, 10 điểm cảnh (2 chức năng)'"),
    ("'C.2 Kết nối Sàn giao dịch KH&CN Việt Nam và các sàn khác'", "'C.2 Tích hợp với sàn giao dịch khác, gồm Sàn giao dịch KH&CN Việt Nam (1 chức năng)'"),
    ("'C.7 Gọi video giữa người bán, người mua và Sàn (cấu hình máy chủ họp)'", "'C.7 Liên hệ trực tiếp – phần gọi video (cùng mã C.7; phần chat nghiệm thu ở dòng 3)'"),
    ("'Ghi chú: tỷ trọng từng nhóm chức năng là đề xuất để các bên xác nhận; tổng đợt 1 theo số MobiFone thông báo.'",
     "'Ghi chú: Phụ lục 3 có 63 chức năng (A 37, B 16, C 10). Đợt 1 nghiệm thu 62 chức năng (dòng 1–5: 37 + 15 + 7 + 1 + 2), trong đó C.7 nghiệm thu phần chat; đợt 2 gồm C.2 và phần gọi video của C.7. Tỷ trọng từng nhóm là đề xuất để các bên xác nhận; tổng đợt 1 theo số MobiFone thông báo.'"),
    ("'60 chức năng nhóm A, B, C đạt kiểm thử, gồm VR360 trên giao diện (xem sheet Mục 1.1)'",
     "'62/63 chức năng theo Phụ lục 3 đạt kiểm thử, gồm VR360 trên giao diện (xem sheet Mục 1.1)'"),
])

patch('mk_plan.py', [
    ("'Thiết lập trang web Sàn (60 chức năng nhóm A, B, C đạt kiểm thử, gồm VR360 trên giao diện)'",
     "'Thiết lập trang web Sàn (62/63 chức năng theo Phụ lục 3, gồm VR360 trên giao diện)'"),
])

patch('mk_bbnt.py', [
    ("'Chức năng phần mềm theo Phụ lục 3: 60 chức năng nhóm A, B, C đạt kiểm thử (chi tiết tại Bảng xác định giá trị khối lượng đợt 1)'",
     "'Chức năng phần mềm theo Phụ lục 3 (63 chức năng: A 37, B 16, C 10): 62 chức năng đạt kiểm thử; C.2 và phần gọi video của C.7 nghiệm thu đợt 2 (chi tiết tại Bảng xác định giá trị khối lượng đợt 1)'"),
    ("D.p('Lỗi phát sinh: …… lỗi; đã khắc phục: …… lỗi; còn tồn tại: ……………………', indent=True)\n",
     "D.p('Lỗi phát sinh: …… lỗi; đã khắc phục: …… lỗi; còn tồn tại: ……………………', indent=True)\n"
     "D.p('Đối chiếu quy trình giao dịch 14 bước với kịch bản kiểm thử:', indent=True)\n"
     "W14 = [\n"
     "    ('1', 'Tạo yêu cầu chuyển giao', 'Kiểm thử bước 4 (đăng nhu cầu, gửi yêu cầu)'),\n"
     "    ('2', 'Ký thỏa thuận bảo mật (khi sản phẩm yêu cầu)', 'Kiểm thử bổ sung'),\n"
     "    ('3', 'Tạo yêu cầu báo giá, mời nhà cung cấp', 'Kiểm thử bước 5'),\n"
     "    ('4', 'Nộp và chấm điểm hồ sơ đề xuất', 'Kiểm thử bước 5'),\n"
     "    ('5', 'Đàm phán thương mại', 'Kiểm thử bước 6'),\n"
     "    ('6', 'Thẩm định pháp lý', 'Kiểm thử bổ sung'),\n"
     "    ('7', 'Lập và ký hợp đồng điện tử', 'Kiểm thử bước 7, 8'),\n"
     "    ('8', 'Xác nhận tạm ứng', 'Kiểm thử bổ sung'),\n"
     "    ('9', 'Thử nghiệm', 'Kiểm thử bổ sung'),\n"
     "    ('10', 'Bàn giao và triển khai', 'Kiểm thử bổ sung'),\n"
     "    ('11', 'Đào tạo và chuyển giao vận hành', 'Kiểm thử bổ sung'),\n"
     "    ('12', 'Bàn giao hồ sơ kỹ thuật', 'Kiểm thử bổ sung'),\n"
     "    ('13', 'Nghiệm thu', 'Kiểm thử bổ sung'),\n"
     "    ('14', 'Thanh lý hợp đồng', 'Kiểm thử bổ sung'),\n"
     "]\n"
     "D.table(['Bước quy trình', 'Nội dung', 'Kiểm thử tương ứng', 'Kết quả'], [x + ('Đạt',) for x in W14], [1.8, 7.2, 5.5, 2.5], center_cols=(0, 3))\n"
     "D.p('Kịch bản 10 bước (phần V Chương V) kiểm tra các bước quy trình 1, 3, 4, 5, 7 và các bước chuẩn bị (đăng ký, duyệt nhà cung cấp, đăng sản phẩm). Các bước 2, 6, 8 – 14 được kiểm thử bổ sung trên cùng hồ sơ giao dịch. Thông báo email, SMS và nhật ký (kiểm thử bước 9, 10) được kiểm tra ở tất cả 14 bước quy trình.', italic=True, indent=True)\n"),
])
