"""Biên bản ghi nhận lỗi vận hành thử lần 1 và Biên bản kết quả khắc phục lỗi lần 1 (27/10/2026)."""
import os, sys
sys.path.insert(0, os.path.dirname(__file__))
from lib27 import *

OUT = r'D:\2026\CanTho\docs\27-10-2026'

THANH_PHAN = [
    ('1. Đại diện nhà cung cấp dịch vụ: Tổng Công ty Viễn thông MobiFone (MobiFone Cần Thơ)', [
        ('Ông: Lã Thành Trung', 'Chức vụ: Phó Giám đốc phụ trách MobiFone Cần Thơ'),
        ('Ông: Bùi Nguyễn Hoàng Thái', 'Chức vụ: Kỹ sư hạ tầng'),
        ('Ông: Lê Phước Trung', 'Chức vụ: Kiểm thử')]),
    ('2. Đại diện đơn vị vận hành thử: Công ty TNHH Công nghệ Tin học Bình Định', [
        ('Ông: Ngô Nguyên Giản', 'Chức vụ: ……………………')]),
    ('3. Đại diện đơn vị giám sát: Công ty TNHH Chuyển Đổi Số 365', [
        ('Ông: Nguyễn Minh Tuyển', 'Chức vụ: Giám sát trưởng'),
        ('Ông: Trần Gia Bảo', 'Chức vụ: Giám sát viên')]),
]
KY = [('ĐẠI DIỆN\nNHÀ CUNG CẤP DỊCH VỤ', 'Lã Thành Trung'), ('ĐẠI DIỆN\nĐƠN VỊ VẬN HÀNH THỬ', 'Ngô Nguyên Giản'),
      ('ĐẠI DIỆN\nĐƠN VỊ GIÁM SÁT', 'Nguyễn Minh Tuyển')]

# (mã, hạng mục, mô tả hiện tượng, mức độ, yêu cầu, thời hạn)
LOI = [
    ('VHT-01', 'Tài khoản quyền xem cấp cho đơn vị giám sát (gs_theodoi) – mục 2.13 Biên bản GS-02',
     'Đăng nhập gs_theodoi vào trang quản trị: mở được toàn bộ chức năng quản trị, có quyền thêm, sửa, xóa như tài khoản quản trị; '
     'chưa giới hạn chỉ xem như Văn bản cung cấp tài khoản quyền xem ngày 26/10/2026',
     'Cao', 'Giới hạn chỉ xem; thao tác thêm, sửa, xóa phải bị từ chối và ghi nhật ký', '17 giờ 00 ngày 27/10/2026'),
    ('VHT-02', 'Trang kiểm tra tình trạng hệ thống https://mekongx.vn/health – mục 2.10 Biên bản GS-02',
     'Truy cập từ mạng bên ngoài, không đăng nhập: trang trả về chi tiết từng thành phần (cơ sở dữ liệu, ổ đĩa, hàng đợi thông báo) '
     'và dung lượng ổ đĩa còn trống 914,7 GB, ngoài trạng thái tổng hợp',
     'Trung bình', 'Chỉ trả trạng thái tổng hợp và mã phản hồi; không hiển thị thông tin thành phần, dung lượng', '17 giờ 00 ngày 27/10/2026'),
    ('VHT-03', 'Gửi thông báo qua thư điện tử (A.1.4, A.4.5, A.8.4) – tồn tại 6 Biên bản GS-02',
     'Kênh gửi thư điện tử của hệ thống đang tắt; thông báo, nhắc lịch chưa gửi được qua thư điện tử',
     'Trung bình', 'Bật kênh gửi thư điện tử, gửi thử, lưu nhật ký gửi thư', '17 giờ 00 ngày 27/10/2026'),
    ('VHT-04', 'Hỗ trợ IPv6 – mục 2.9 Biên bản GS-02',
     'Tra cứu bản ghi tên miền ngày 27/10/2026: mekongx.vn chỉ có bản ghi IPv4 (103.156.3.5), chưa có bản ghi IPv6 (AAAA)',
     'Thấp', 'Cấp địa chỉ IPv6 cho máy chủ, bổ sung bản ghi AAAA, kiểm tra truy cập qua IPv6', 'Trước ngày 09/11/2026'),
]


def tieu_de(d, ten, ngay):
    quoc_hieu(d, f'Cần Thơ, ngày {ngay} tháng 10 năm 2026')
    P(d, [(ten, 'b')], align='center', indent=False, size=14, after=0, before=6)
    P(d, [('(Lần 1)', 'b')], align='center', indent=False, after=0)
    P(d, [(GOI, 'i')], align='center', indent=False, after=8)


def thanh_phan(d):
    H(d, 'I. THÀNH PHẦN THAM GIA')
    for org, people in THANH_PHAN:
        P(d, [(org, 'b')], indent=False, after=2)
        for a, b in people:
            P(d, f'- {a}; {b}', indent=False, after=2)


def bb_ghi_nhan():
    d = new_doc()
    tieu_de(d, 'BIÊN BẢN GHI NHẬN LỖI VẬN HÀNH THỬ', '27')
    for cc in [f'Căn cứ {HD};',
               'Căn cứ mục 6 Phụ lục VII Thông tư số 41/2026/TT-BKHCN ngày 01 tháng 7 năm 2026 của Bộ trưởng Bộ Khoa học và Công nghệ;',
               'Căn cứ Kế hoạch giám sát số 106/KH-CDS365 ngày 23 tháng 10 năm 2026 của Công ty TNHH Chuyển Đổi Số 365;',
               'Căn cứ kết quả kiểm tra hạ tầng thuê và chất lượng dịch vụ ngày 27 tháng 10 năm 2026 (Biên bản số GS-02/BBKT/CDS365),']:
        P(d, [(cc, 'i')])
    P(d, 'Hôm nay, ngày 27 tháng 10 năm 2026, tại Trung tâm Ứng dụng tiến bộ khoa học và công nghệ thành phố Cần Thơ '
         'và kiểm tra trực tuyến trên hệ thống https://mekongx.vn, chúng tôi gồm:')
    thanh_phan(d)
    H(d, 'II. DANH SÁCH LỖI GHI NHẬN')
    P(d, 'Trong quá trình kiểm tra hạ tầng và vận hành thử lần 1 từ 08 giờ 00 ngày 27/10/2026, các bên thống nhất ghi nhận các lỗi sau:')
    T(d, [6, 10, 21, 33, 10, 20],
      ['STT', 'Mã lỗi', 'Hạng mục, chức năng', 'Hiện tượng ghi nhận', 'Mức độ', 'Yêu cầu khắc phục; thời hạn'],
      [[str(i), m, hm, mt, md, f'{yc}; {th}'] for i, (m, hm, mt, md, yc, th) in enumerate(LOI, 1)],
      size=10, aligns=['center', 'center', 'left', 'left', 'center', 'left'])
    P(d, [('Mức độ lỗi: ', 'b'), ('Cao – chức năng không dùng được, sai dữ liệu, lỗ hổng bảo mật, khắc phục trong ngày; '
                              'Trung bình – dùng được nhưng thiếu kiểm soát an toàn hoặc thiếu một phần, khắc phục trước nghiệm thu đợt 1; '
                              'Thấp – không ảnh hưởng nghiệp vụ, khắc phục theo kế hoạch cập nhật.', 'i')], size=12)
    P(d, 'Tổng số lỗi ghi nhận: 04 lỗi (01 lỗi mức Cao, 02 lỗi mức Trung bình, 01 lỗi mức Thấp).')
    H(d, 'III. Ý KIẾN CỦA CÁC BÊN')
    P(d, '1. Nhà cung cấp dịch vụ: thống nhất danh sách lỗi; khắc phục theo thời hạn nêu trên, báo cáo kết quả khắc phục '
         'để đơn vị vận hành thử, đơn vị giám sát kiểm tra lại.')
    P(d, '2. Đơn vị vận hành thử: ……………………………………………………………', align='left', indent=False)
    P(d, '3. Đơn vị giám sát: ………………………………………………………………', align='left', indent=False)
    P(d, 'Biên bản lập thành 04 (bốn) bản có giá trị như nhau; nhà cung cấp dịch vụ giữ 01 bản, đơn vị vận hành thử giữ 01 bản, '
         'đơn vị giám sát giữ 01 bản, gửi chủ đầu tư 01 bản./.')
    sign_grid(d, KY)
    save(d, os.path.join(OUT, 'Bien ban ghi nhan loi van hanh thu lan 1.docx'))


def bb_khac_phuc():
    d = new_doc(landscape=True)
    tieu_de(d, 'BIÊN BẢN KẾT QUẢ KHẮC PHỤC LỖI', '27')
    for cc in [f'Căn cứ {HD};',
               'Căn cứ mục 6 Phụ lục VII Thông tư số 41/2026/TT-BKHCN ngày 01 tháng 7 năm 2026 của Bộ trưởng Bộ Khoa học và Công nghệ;',
               'Căn cứ Biên bản ghi nhận lỗi vận hành thử lần 1 ngày 27 tháng 10 năm 2026,']:
        P(d, [(cc, 'i')])
    P(d, 'Hôm nay, ngày 27 tháng 10 năm 2026, lúc 09 giờ 30, các bên kiểm tra lại kết quả khắc phục lỗi trên hệ thống https://mekongx.vn, chúng tôi gồm:')
    thanh_phan(d)
    H(d, 'II. KẾT QUẢ KHẮC PHỤC CÁC LỖI GHI NHẬN TRONG VẬN HÀNH THỬ LẦN 1')
    rows = [
        ['1', 'VHT-01', 'Cao',
         'Trang quản trị chưa có chế độ tài khoản chỉ xem; tài khoản gs_theodoi được tạo với quyền quản trị',
         'Bổ sung chế độ tài khoản chỉ xem cho trang quản trị, áp dụng cho gs_theodoi: chỉ mở được Tổng quan, Nhật ký, Thống kê, '
         'Yêu cầu hỗ trợ; mọi thao tác thêm, sửa, xóa và chức năng khác bị từ chối, ghi nhật ký loại "Từ chối thao tác"; '
         'không có quyền quản trị trên hồ sơ giao dịch. Cập nhật phần mềm ngày 27/10/2026',
         'Đăng nhập gs_theodoi: xem được 04 trang theo dõi; mở Quản lý người dùng, Tham số hệ thống, Sản phẩm và bấm cập nhật số liệu, '
         'đóng yêu cầu hỗ trợ đều bị từ chối với thông báo "Tài khoản chỉ có quyền xem"; Nhật ký ghi đủ từng lần từ chối, đúng tài khoản, thời điểm (MC-HT-09-20261027)',
         'Đạt'],
        ['2', 'VHT-02', 'Trung bình',
         'Trang trả về toàn bộ kết quả kiểm tra từng thành phần cho mọi người truy cập',
         'Trang công khai chỉ trả trạng thái tổng hợp; chi tiết từng thành phần chỉ hiển thị khi đăng nhập tài khoản có quyền quản trị. '
         'Cập nhật phần mềm ngày 27/10/2026',
         'Truy cập từ mạng bên ngoài, không đăng nhập: trang chỉ trả {"status":"Healthy"}, mã phản hồi 200; '
         'không hiển thị thông tin thành phần, dung lượng ổ đĩa (MC-HT-03-20261027-2)',
         'Đạt'],
        ['3', 'VHT-03', 'Trung bình',
         'Chưa hoàn tất cấu hình máy chủ gửi thư của tên miền',
         'Cấu hình máy chủ gửi thư, bật kênh gửi thư điện tử; gửi thử thông báo, nhắc lịch',
         '……',
         'Đang khắc phục'],
        ['4', 'VHT-04', 'Thấp',
         'Máy chủ chưa được cấp địa chỉ IPv6',
         'Đề nghị cấp địa chỉ IPv6 cho máy chủ; bổ sung bản ghi AAAA cho tên miền',
         'Chưa thực hiện kiểm tra lại',
         'Chưa khắc phục; hạn trước 09/11/2026'],
    ]
    T(d, [5, 7, 8, 18, 29, 29, 10],
      ['STT', 'Mã lỗi', 'Mức độ', 'Nguyên nhân', 'Biện pháp khắc phục', 'Kết quả kiểm tra lại', 'Đánh giá'],
      rows, size=10, aligns=['center', 'center', 'center', 'left', 'left', 'left', 'center'])
    H(d, 'III. CÁC LỖI PHÁT HIỆN KHI TỰ KIỂM TRA NỘI BỘ, ĐÃ KHẮC PHỤC TRƯỚC VẬN HÀNH THỬ')
    P(d, 'Để các bên kiểm tra độc lập (tồn tại 7 Biên bản GS-02), nhà cung cấp dịch vụ báo cáo các lỗi phát hiện trong tự kiểm tra nội bộ '
         'ngày 25/10/2026, đã khắc phục và kiểm tra lại ngày 26/10/2026:')
    T(d, [5, 7, 29, 29, 22, 8],
      ['STT', 'Mã lỗi', 'Nội dung lỗi', 'Biện pháp khắc phục', 'Kết quả kiểm tra lại; minh chứng', 'Đánh giá'],
      [['1', 'L-01', 'API-05 (gợi ý nhà cung cấp) trả lỗi 500 khi gửi yêu cầu hợp lệ',
        'Sửa xử lý dữ liệu gợi ý; cập nhật phần mềm ngày 26/10/2026',
        'Trả danh sách nhà cung cấp, mã phản hồi 200; MC-KT-07-20261026', 'Đạt'],
       ['2', 'L-02', '05 API ghi dữ liệu chưa kiểm tra mã xác thực yêu cầu của phiên đăng nhập',
        'Bắt buộc mã xác thực yêu cầu với 05 API; cập nhật phần mềm ngày 26/10/2026',
        'Yêu cầu thiếu mã bị từ chối; MC-KT-08, 09, 17, 24-20261026-vht_buyer01', 'Đạt'],
       ['3', 'L-03', 'API thông báo chuyển hướng tới trang đăng nhập (mã 302) thay vì báo chưa đăng nhập (mã 401)',
        'Các API trả mã 401 kèm thông báo khi chưa đăng nhập; cập nhật phần mềm ngày 26/10/2026',
        'Trả mã 401; MC-KT-22, 23-20261026-2', 'Đạt']],
      size=10, aligns=['center', 'center', 'left', 'left', 'left', 'center'])
    H(d, 'IV. TỔNG HỢP VÀ KIẾN NGHỊ')
    P(d, 'Lỗi ghi nhận trong vận hành thử lần 1: 04 lỗi; đã khắc phục, kiểm tra lại đạt: 02 lỗi (VHT-01, VHT-02); '
         'đang khắc phục: 01 lỗi (VHT-03); chưa khắc phục: 01 lỗi (VHT-04, phụ thuộc cấp địa chỉ IPv6 cho hạ tầng, hạn trước 09/11/2026).')
    P(d, 'Nhà cung cấp dịch vụ hoàn thành lỗi VHT-03 trong ngày 27/10/2026, VHT-04 trước ngày 09/11/2026 và báo cáo để các bên kiểm tra lại.')
    P(d, 'Ý kiến đơn vị vận hành thử: ……………………………………………………………………………………………', align='left', indent=False)
    P(d, 'Ý kiến đơn vị giám sát: ……………………………………………………………………………………………', align='left', indent=False)
    P(d, 'Biên bản lập thành 04 (bốn) bản có giá trị như nhau; nhà cung cấp dịch vụ giữ 01 bản, đơn vị vận hành thử giữ 01 bản, '
         'đơn vị giám sát giữ 01 bản, gửi chủ đầu tư 01 bản./.')
    sign_grid(d, KY)
    save(d, os.path.join(OUT, 'Bien ban ket qua khac phuc loi lan 1.docx'))


if __name__ == '__main__':
    bb_ghi_nhan(); bb_khac_phuc(); print('ok')
