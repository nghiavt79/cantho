"""Dự thảo công văn đề nghị nghiệm thu đợt 1, kèm Danh mục nghiệm thu đợt 1 để các bên đánh giá (27/10/2026).
Số tiền theo Phụ lục 5 (bản chị Vân xác nhận 21/10) — không tự tính lại."""
import os, sys
from docx.enum.section import WD_SECTION, WD_ORIENT
from docx.shared import Cm
sys.path.insert(0, os.path.dirname(__file__))
from lib27 import *

OUT = r'D:\2026\CanTho\docs\27-10-2026'
CHECK = '☐ Đạt\n☐ Chưa đạt'


def money(v):
    return f'{v:,}'.replace(',', '.')


DM = [  # (stt, danh mục, ĐVT, SL, giá trị đợt 1 hoặc None nếu dòng nhóm, hồ sơ minh chứng)
    ('1', 'Thuê dịch vụ thiết lập, vận hành 01 trang web Sàn giao dịch công nghệ Cần Thơ', '', '', None, ''),
    ('1.1', 'Thiết lập trang web Sàn giao dịch công nghệ Cần Thơ', 'Gói', '1', 890782296,
     '60/63 chức năng theo Phụ lục 3 hợp đồng (phần A: 37; phần B: 15; phần C: 08, gồm C.4 ở chế độ thử nghiệm, C.7 phần trò chuyện trực tuyến); '
     'Biên bản tự kiểm thử chức năng đợt 1; Báo cáo tự kiểm tra nội bộ lần 1; kết quả vận hành thử lần 1; Biên bản kết quả khắc phục lỗi lần 1'),
    ('1.2', 'Cài đặt, cấu hình, hướng dẫn sử dụng, vận hành, bảo hành, bảo trì và hỗ trợ kỹ thuật hệ thống trong thời gian 05 năm', 'Gói', '1', 447020320,
     'Cài đặt, cấu hình trên 02 máy chủ; tích hợp các giải pháp mục 1.4; dữ liệu khởi tạo; tài liệu hướng dẫn sử dụng, tài liệu kỹ thuật, '
     'đặc tả API, đặc tả cơ sở dữ liệu; quy trình vận hành (NCC-14); kết quả đào tạo (NCC-08); vận hành thử lần 1'),
    ('1.3', 'Bản quyền phần mềm Windows Server Standard và SQL Server Standard', '', '', None, ''),
    ('1.3.1', 'Windows Server 2025 Standard - 16 Core License Pack', 'Gói', '1', 56000000, 'Chứng từ bản quyền; kết quả kích hoạt trên máy chủ'),
    ('1.3.2', 'Windows Server 2025 - 1 User CAL', 'Gói', '1', 3010000, 'Chứng từ bản quyền'),
    ('1.3.3', 'SQL Server 2025 Standard edition Perpetual 1 Server License', 'Gói', '1', 77000000, 'Chứng từ bản quyền; cài đặt trên máy chủ cơ sở dữ liệu'),
    ('1.3.4', 'SQL Server 2025 - 1 User CAL', 'Gói', '1', 21280000, 'Chứng từ bản quyền'),
    ('1.4', 'Giải pháp tích hợp kèm theo (gồm chi phí cấu hình, tích hợp và đồng bộ dữ liệu giữa các giải pháp)', '', '', None, ''),
    ('1.4.1', 'Giải pháp tin nhắn thương hiệu (05 năm, mỗi năm 3.600 SMS)', 'Gói', '1', 27500000, 'Chứng từ đăng ký thương hiệu, kích hoạt; kết quả gửi tin thử'),
    ('1.4.2', 'Email Hosting gói 3 - 60 tháng (60 địa chỉ, 25 GB, 05 Mail List)', 'Gói', '1', 8250000, 'Chứng từ kích hoạt; danh sách địa chỉ; kết quả gửi thư thử'),
    ('1.4.3', 'Gói combo TOKEN cho khách hàng doanh nghiệp (03 năm, miễn phí thêm 02 năm)', 'Gói', '1', 2594602, 'Chứng từ cấp thiết bị, chứng thư số; kết quả ký thử'),
    ('1.4.4', 'Gói eContract 10.000 tài liệu', 'Gói', '1', 39000000, 'Hợp đồng, tài khoản dịch vụ; kết quả ký hợp đồng thử'),
    ('1.4.5', 'Gói SMS 5.000 tin nhắn', 'Gói', '2', 11000000, 'Chứng từ cấp gói; kết quả gửi tin thử'),
    ('2', 'Thuê hạ tầng máy chủ (cloud) phục vụ cài đặt, vận hành và lưu trữ dữ liệu của hệ thống (02 máy chủ)', 'Hệ thống', '1', 693000000,
     'Biên bản bàn giao 02 máy chủ, cấu hình; tài liệu vị trí trung tâm dữ liệu; tài khoản quyền xem giám sát; Biên bản kiểm tra hạ tầng GS-02'),
]
TONG = 2276437218
assert sum(x[4] for x in DM if x[4]) == TONG


def main():
    d = new_doc()
    header_cv(d, 'Số: ……/MBF-CT-GPS', 'V/v đề nghị nghiệm thu khối lượng hoàn thành đợt 1 Gói thầu số 4',
              'Cần Thơ, ngày …… tháng 10 năm 2026')
    P(d, 'Kính gửi: Trung tâm Ứng dụng tiến bộ khoa học và công nghệ thành phố Cần Thơ.', align='center', indent=False)
    P(d, f'Căn cứ {HD};')
    P(d, 'Căn cứ Điều 5 Hợp đồng về thanh toán lần thứ nhất theo khối lượng nghiệm thu thực tế;')
    P(d, 'Căn cứ kết quả kiểm tra hạ tầng, vận hành thử lần 1 và kết quả khắc phục lỗi ngày 27 tháng 10 năm 2026,')
    P(d, 'Tổng Công ty Viễn thông MobiFone đã hoàn thành khối lượng công việc đợt 1 với giá trị đề nghị nghiệm thu '
         f'{money(TONG)} đồng (đã gồm thuế), chi tiết tại Danh mục nghiệm thu đợt 1 kèm theo.')
    P(d, 'Tổng Công ty Viễn thông MobiFone kính đề nghị:')
    P(d, '1. Trung tâm Ứng dụng tiến bộ khoa học và công nghệ thành phố Cần Thơ tổ chức nghiệm thu khối lượng công việc hoàn thành đợt 1 vào ngày ……/10/2026;')
    P(d, '2. Đơn vị giám sát (Công ty TNHH Chuyển Đổi Số 365) và đơn vị vận hành thử (Công ty TNHH Công nghệ Tin học Bình Định) '
         'đánh giá từng nội dung của Danh mục nghiệm thu đợt 1 trong báo cáo giám sát và báo cáo kết quả vận hành thử lần 1, làm căn cứ nghiệm thu.')
    P(d, 'Các nội dung đang hoàn thiện, Tổng Công ty Viễn thông MobiFone hoàn thành trước ngày nghiệm thu: chứng từ bản quyền phần mềm; '
         'chứng từ kích hoạt các gói dịch vụ tích hợp; tài liệu vị trí trung tâm dữ liệu; bật kênh gửi thư điện tử. '
         'Bổ sung IPv6 cho tên miền hoàn thành trước ngày 09/11/2026.')
    P(d, 'Nội dung chuyển nghiệm thu đợt 2: tham quan thực tế ảo VR360 (B.1.2, C.1 và giải pháp 10 điểm cảnh); '
         'tích hợp với sàn giao dịch khác (C.2); phần gọi video của chức năng liên hệ trực tiếp (C.7).')
    P(d, [('Hồ sơ kèm theo:', 'b')], indent=False, after=2)
    for x in ['- Danh mục nghiệm thu đợt 1;',
              '- Biên bản tự kiểm thử chức năng phần mềm đợt 1; Báo cáo tự kiểm tra nội bộ, sẵn sàng vận hành thử lần 1;',
              '- Biên bản kiểm tra hạ tầng thuê và chất lượng dịch vụ (GS-02); Biên bản ghi nhận lỗi vận hành thử lần 1; Biên bản kết quả khắc phục lỗi lần 1;',
              '- Dự thảo Biên bản nghiệm thu khối lượng công việc hoàn thành đợt 1; Bảng xác định giá trị khối lượng công việc đợt 1.']:
        P(d, x, indent=False, after=2)
    P(d, 'Trân trọng./.', before=6)
    sign_cv(d, ['- Như trên;', '- Công ty TNHH Chuyển Đổi Số 365 (đơn vị giám sát);',
                '- Công ty TNHH Công nghệ Tin học Bình Định (đơn vị vận hành thử);', '- Lưu: VT, TTGPS.'],
            'ĐẠI DIỆN NHÀ CUNG CẤP DỊCH VỤ', 'Lã Thành Trung')

    # Danh mục — trang ngang
    s = d.add_section(WD_SECTION.NEW_PAGE)
    s.orientation = WD_ORIENT.LANDSCAPE
    s.page_width, s.page_height = Cm(29.7), Cm(21.0)
    s.left_margin, s.right_margin = Cm(2.0), Cm(1.5)
    P(d, [('DANH MỤC NGHIỆM THU ĐỢT 1', 'b')], align='center', indent=False, size=14, after=0)
    P(d, [(f'(Kèm theo Công văn số ……/MBF-CT-GPS ngày ……/10/2026 của Tổng Công ty Viễn thông MobiFone)', 'i')],
      align='center', indent=False, after=0)
    P(d, [('Đơn vị tính giá trị: đồng (đã gồm thuế)', 'i')], align='right', indent=False, size=11)
    rows, groups = [], []
    for i, (stt, ten, dvt, sl, v, hs) in enumerate(DM, 1):
        if v is None:
            rows.append([stt, ten, '', '', '', '', '']); groups.append(i)
        else:
            rows.append([stt, ten, dvt, sl, money(v), hs, CHECK])
    rows.append(['', 'Tổng cộng', '', '', money(TONG), '', ''])
    t = T(d, [6, 30, 7, 6, 13, 41, 11],
          ['STT', 'Danh mục (theo Phụ lục 5 Hợp đồng)', 'Đơn vị tính', 'Số lượng', 'Giá trị đề nghị nghiệm thu đợt 1',
           'Hồ sơ, minh chứng', 'Đánh giá của đơn vị giám sát, vận hành thử'],
          rows, size=10, aligns=['center', 'left', 'center', 'center', 'right', 'left', 'left'])
    for i in groups:
        for c in t.rows[i].cells[:2]:
            for r in c.paragraphs[0].runs: r.bold = True
    for c in t.rows[-1].cells:
        for r in c.paragraphs[0].runs: r.bold = True
    P(d, [('Ghi chú: ', 'b'), ('Đơn vị giám sát, đơn vị vận hành thử đánh dấu kết quả và ghi ý kiến (nếu có) vào cột đánh giá hoặc trong báo cáo giám sát, '
                            'báo cáo kết quả vận hành thử lần 1. Nội dung chuyển đợt 2 không thuộc Danh mục này.', 'i')], size=11, indent=False)
    save(d, os.path.join(OUT, 'Cong van de nghi nghiem thu dot 1 kem danh muc nghiem thu.docx'))
    print('ok')


if __name__ == '__main__':
    main()
