"""NCC-03 (thông báo sẵn sàng vận hành thử), VHT-04/NCC-05 (biên bản lỗi, khắc phục lỗi lần 1),
NCC-06 (nhật ký hệ thống, số liệu giám sát lần 1, gộp NCC-06a), cập nhật mã trong GS-02 và NCC-08."""
import os
import shutil
import sys
import docx
from docx.shared import Pt

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from lib27 import new_doc, P, H, T, header_cv, sign_cv, sign_grid, AL
from lib28 import ROOT, OUT, HS, BB, replace_doc, replace_in_paragraph, insert_after, set_par, find_par, save

MC27 = os.path.join(ROOT, '..', '27-10-2026', 'Minh chung')
MC28 = os.path.join(ROOT, 'Minh chung')
MC26 = os.path.join(HS, '1.6 Tu kiem tra noi bo, san sang van hanh thu', 'Minh chung')
HD = ('Căn cứ Hợp đồng số HD2600273782_2610221351 ngày 22/10/2026 giữa Trung tâm Ứng dụng tiến bộ khoa học và công nghệ '
      'thành phố Cần Thơ và Tổng Công ty Viễn thông MobiFone về việc thực hiện Gói thầu số 04: Thuê dịch vụ thiết lập, '
      'vận hành trang web Sàn giao dịch công nghệ Cần Thơ;')
KG = ['- Trung tâm Ứng dụng tiến bộ khoa học và công nghệ thành phố Cần Thơ;',
      '- Công ty TNHH Công nghệ Tin học Bình Định (đơn vị vận hành thử);',
      '- Công ty TNHH Chuyển Đổi Số 365 (đơn vị giám sát).']
SO = 'Số: ……/MBF-CT-GPS'


def kinh_gui(d):
    P(d, [('Kính gửi:', 'b')], align='left', indent=False, after=0)
    for x in KG:
        P(d, x, align='left', indent=False, after=0)
    d.add_paragraph()


def ky(d):
    sign_cv(d, ['- Như trên;', '- Lưu: VT, TTGPS.'], 'ĐẠI DIỆN NHÀ CUNG CẤP DỊCH VỤ', 'Lã Thành Trung')


# ---------------------------------------------------------------- NCC-03
def ncc03():
    d = new_doc()
    header_cv(d, SO, 'V/v thông báo sẵn sàng vận hành thử lần 1; cung cấp tài khoản thử, dữ liệu mẫu',
              'Cần Thơ, ngày 26 tháng 10 năm 2026')
    kinh_gui(d)
    P(d, HD + ' Kế hoạch triển khai chi tiết kèm Công văn số 2815/MBF.CT-KDGPS ngày 22/10/2026 (cập nhật ngày 25/10/2026); '
             'Văn bản đề xuất chỉ số, ngưỡng chất lượng dịch vụ; cung cấp tài khoản quyền xem giám sát và tài liệu dịch vụ (NCC-02) '
             'ngày 26/10/2026,')
    P(d, 'Tổng Công ty Viễn thông MobiFone thông báo trang web Sàn giao dịch công nghệ Cần Thơ tại địa chỉ https://mekongx.vn '
         'đã hoàn thành thiết lập, tự kiểm tra nội bộ và sẵn sàng vận hành thử lần 1 từ 08 giờ ngày 27/10/2026, cụ thể như sau:')
    H(d, '1. Tình trạng sẵn sàng')
    T(d, [7, 30, 40, 25], ['STT', 'Nội dung', 'Tình trạng', 'Căn cứ'], [
        ['1', 'Hệ thống, tên miền, chứng thư SSL', 'Hoạt động tại https://mekongx.vn; chứng thư SSL hiệu lực đến 24/01/2027; '
                                                  'chỉ chấp nhận giao thức mã hóa TLS 1.2, TLS 1.3', 'Minh chứng MC-HT-04, MC-HT-05 ngày 26/10/2026'],
        ['2', 'Chức năng phần mềm (12 nhóm chức năng)', 'Đã tự kiểm thử từ ngày 22/10 đến ngày 25/10/2026',
         'NCC-02, Tài liệu số 01, 05'],
        ['3', 'Tài liệu hướng dẫn sử dụng, quy trình vận hành, đặc tả API', 'Đã gửi kèm NCC-02', 'NCC-02, Tài liệu số 03, 06 – 11'],
        ['4', 'Công cụ giám sát, quyền xem', 'Đã cấp tài khoản quyền xem gs_theodoi cho đơn vị giám sát', 'NCC-02, mục 1, mục 4'],
        ['5', 'Tồn tại đã biết', '11 tồn tại; tồn tại 1 – 5 xử lý trong vận hành thử lần 1, trước nghiệm thu đợt 1',
         'NCC-02, Tài liệu số 04, mục III'],
    ], size=11)
    H(d, '2. Tài khoản thử')
    T(d, [7, 18, 22, 20, 35], ['STT', 'Tài khoản', 'Đơn vị sử dụng', 'Vai trò', 'Phạm vi sử dụng trong vận hành thử'], [
        ['1', 'ttud_admin', 'Chủ trì thuê', 'Quản trị Sàn', 'CMS: duyệt hồ sơ, sản phẩm; cấu hình; theo dõi giao dịch; báo cáo'],
        ['2', 'ttud_bientap', 'Chủ trì thuê', 'Biên tập nội dung', 'CMS: tin bài, sự kiện, trang thông tin'],
        ['3', 'vht_admin', 'Đơn vị vận hành thử', 'Quản trị (kiểm thử CMS)', 'Các chức năng quản trị CMS theo kịch bản vận hành thử'],
        ['4', 'vht_buyer01, vht_buyer02', 'Đơn vị vận hành thử', 'Người mua công nghệ',
         'Tìm kiếm, gửi nhu cầu, yêu cầu báo giá, quy trình giao dịch phía bên mua'],
        ['5', 'vht_seller01, vht_seller02', 'Đơn vị vận hành thử', 'Người bán (nhà cung ứng)',
         'Hồ sơ nhà cung ứng, đăng công nghệ, thiết bị, báo giá, quy trình giao dịch phía bên bán'],
        ['6', 'vht_expert01', 'Đơn vị vận hành thử', 'Chuyên gia, tư vấn', 'Hồ sơ chuyên gia, tiếp nhận và trả lời yêu cầu tư vấn'],
        ['7', 'gs_theodoi', 'Đơn vị giám sát', 'Theo dõi (chỉ xem)', 'Nhật ký hệ thống, thống kê, yêu cầu hỗ trợ (NCC-02, mục 1)'],
        ['8', 'ncc_hotro', 'Nhà cung cấp dịch vụ', 'Hỗ trợ người dùng', 'Tiếp nhận, phản hồi yêu cầu hỗ trợ trong vận hành thử'],
    ], size=11, aligns=['center', 'left', 'left', 'left', 'left'])
    P(d, 'Các tài khoản trên đã tạo trên hệ thống, đang hoạt động (các tài khoản vht_* tạo ngày 25/10/2026); tài khoản gs_theodoi đã cấp theo NCC-02.')
    P(d, 'Mật khẩu ban đầu được gửi riêng cho người được từng đơn vị cử sử dụng, không ghi trong văn bản này; người sử dụng đổi '
         'mật khẩu ở lần đăng nhập đầu tiên và không chia sẻ tài khoản. Mọi thao tác của tài khoản thử được ghi nhật ký hệ thống. '
         'Đơn vị vận hành thử cần thêm tài khoản theo kịch bản thì thông báo cho đầu mối tại mục 5.')
    H(d, '3. Dữ liệu mẫu')
    P(d, '- Dữ liệu công nghệ, thiết bị: …… bản ghi đã xuất bản; nhà cung ứng: …… hồ sơ; chuyên gia, tư vấn: …… hồ sơ; '
         'tin bài, sự kiện: …… bài (số liệu tại ngày 26/10/2026);', indent=False)
    P(d, '- Hồ sơ giao dịch mẫu mã PRJ-CATSACH-2026, đã đi đủ 14 bước quy trình giao dịch, dùng để xem trình tự, biểu mẫu từng bước;',
      indent=False)
    P(d, '- Dữ liệu do đơn vị vận hành thử tạo trong quá trình vận hành thử được ghi nhận theo tài khoản thử; sau vận hành thử, '
         'nhà cung cấp xóa hoặc giữ lại theo ý kiến của Chủ trì thuê.', indent=False)
    H(d, '4. Quyền xem công cụ giám sát')
    P(d, 'Đơn vị giám sát dùng tài khoản gs_theodoi theo dõi tính sẵn sàng, tài nguyên máy chủ, nhật ký hệ thống, thống kê và '
         'yêu cầu hỗ trợ trong suốt thời gian vận hành thử; chu kỳ giám sát, ngưỡng cảnh báo theo NCC-02 mục 1, mục 2. '
         'Tài khoản chỉ có quyền xem, thao tác ghi bị từ chối và được ghi nhật ký.')
    H(d, '5. Đầu mối hỗ trợ trong vận hành thử')
    P(d, '- Đầu mối chung: Bà Trần Thị Hồng Vân – Phó Giám đốc Trung tâm Kinh doanh Giải pháp số, MobiFone Cần Thơ; '
         'điện thoại 0908199093; thư điện tử van.tthihong@mobifone.vn;', indent=False)
    P(d, '- Đầu mối kỹ thuật: Bà Nguyễn Thị Dung – Trưởng nhóm kỹ thuật; điện thoại ……………; thư điện tử ……………;', indent=False)
    P(d, '- Thời gian phản hồi yêu cầu hỗ trợ ≤ 02 giờ trong giờ hành chính; lỗi phát sinh được ghi vào Biên bản ghi nhận lỗi '
         '(VHT-04) và khắc phục theo thời hạn: lỗi nghiêm trọng ≤ 08 giờ, lỗi khác ≤ 24 giờ.', indent=False)
    P(d, 'Tổng Công ty Viễn thông MobiFone trân trọng đề nghị Quý đơn vị phối hợp tổ chức vận hành thử lần 1 theo kế hoạch./.')
    ky(d)
    save(d, os.path.join(OUT, 'NCC-03 Thong bao san sang van hanh thu lan 1, tai khoan thu, du lieu mau.docx'))


# ---------------------------------------------------------------- VHT-04, NCC-05
def bon_ben(d):
    """Thêm Chủ trì thuê vào thành phần, số bản và mục xác nhận (4 bên ký, Chủ trì thuê xác nhận)."""
    p = find_par(d, '3. Đơn vị giám sát')
    insert_after(p, '4. Chủ trì thuê – Trung tâm Ứng dụng tiến bộ khoa học và công nghệ thành phố Cần Thơ: '
                    'Ông/Bà ……………………, chức vụ ……………………')
    replace_doc(d, [(r'03 \(ba\) bản', '04 (bốn) bản')])
    last = d.tables[-1]
    blk = [('XÁC NHẬN CỦA CHỦ TRÌ THUÊ', 'b'), ('(Ký, ghi rõ họ tên, đóng dấu)', 'i'), ('', ''), ('', ''), ('', ''), ('……………………', 'b')]
    anchor = last._tbl
    for txt, f in blk:
        np_ = d.add_paragraph()
        r = np_.add_run(txt); r.bold = 'b' in f; r.italic = 'i' in f; r.font.size = Pt(12)
        np_.alignment = AL['center']; np_.paragraph_format.space_after = Pt(0)
        anchor.addnext(np_._p); anchor = np_._p
    d.paragraphs[-1].paragraph_format.keep_with_next = False


# kết quả đã có minh chứng; ô còn lại để nhà cung cấp điền
KQ = {
    'L01': ('26/10/2026, bản cập nhật 26/10/2026',
            'API-05 trả kết quả gợi ý nhà cung cấp, không còn lỗi 500 (MC-KT-06, MC-KT-07-20261026)'),
    'L02': ('26/10/2026, bản cập nhật 26/10/2026; API-18 kiểm tra lại 28/10/2026',
            'Thiếu mã chống giả mạo: bị từ chối (HTTP 400); có mã hợp lệ: qua lớp kiểm tra. Đã xác nhận đủ 05/05 API '
            '(API-06, 07, 11, 17: MC-KT-08, 09, 17, 24-20261026-vht_buyer01; API-18: MC-KT-38-20261028-vht_buyer01)'),
    'L03': ('26/10/2026, bản cập nhật 26/10/2026',
            'Gọi API thông báo khi chưa đăng nhập: kiểm tra lại sau khắc phục (MC-KT-22-20261026-2, MC-KT-23-20261026-2)'),
    'L07': ('27/10/2026, bản cập nhật 27/10/2026',
            'Truy cập từ mạng ngoài, /health chỉ trả trạng thái tổng hợp, HTTP 200 (MC-HT-03-20261027-2)'),
}


GHI_CHU_HAN = ('Các lỗi L01 – L10 được phát hiện qua tự kiểm thử, rà soát tài liệu và kiểm tra hạ tầng, hồ sơ của đơn vị giám sát '
               '(GS-02) trong ngày 25/10 – 27/10/2026. Trong đó L06, L08 là nội dung hoàn thiện giao diện theo góp ý tài liệu hướng '
               'dẫn sử dụng; L09, L10 là nội dung hoàn thiện cấu hình hạ tầng. Hạn khắc phục tại bảng trên do nhà cung cấp đề xuất '
               'theo tiến độ, đề nghị đơn vị vận hành thử, đơn vị giám sát, Chủ trì thuê thống nhất khi ký biên bản này. '
               'Lỗi phát sinh khi vận hành thử được ghi bổ sung, kèm giờ tiếp nhận, và khắc phục theo thời hạn tại NCC-04: '
               'lỗi nghiêm trọng ≤ 08 giờ, lỗi thông thường ≤ 24 giờ kể từ khi tiếp nhận; giờ hoàn thành ghi tại NCC-05.')


def vht04_ncc05():
    d = docx.Document(os.path.join(BB, '3a- Bien ban ghi nhan loi van hanh thu lan 1.docx'))
    replace_doc(d)
    insert_after(find_par(d, 'Theo phân loại tại'), GHI_CHU_HAN)
    bon_ben(d)
    save(d, os.path.join(OUT, 'VHT-04 Bien ban ghi nhan loi van hanh thu lan 1 (nha cung cap du thao).docx'))

    d = docx.Document(os.path.join(BB, '3b-Bien ban khac phuc loi van hanh thu lan 1.docx'))
    replace_doc(d)
    t = next(t for t in d.tables if t.rows[0].cells[0].text.strip() == 'Mã')
    for r in t.rows[1:]:
        ma = r.cells[0].text.strip()
        if ma in KQ:
            ngay, kq = KQ[ma]
            for c, v in ((r.cells[4], ngay), (r.cells[5], kq)):
                p = c.paragraphs[0]
                p.runs[0].text = v
                for x in p.runs[1:]:
                    x._r.getparent().remove(x._r)
    replace_doc(d, [(r'Căn cứ Biên bản ghi nhận lỗi trong tự kiểm thử và vận hành thử lần 1',
                     'Căn cứ Biên bản ghi nhận lỗi trong tự kiểm thử và vận hành thử lần 1 (VHT-04)')])
    bon_ben(d)
    save(d, os.path.join(OUT, 'NCC-05 Bien ban khac phuc loi va kiem tra lai van hanh thu lan 1.docx'))


# ---------------------------------------------------------------- NCC-06 (gộp NCC-06a)
def ncc06():
    d = new_doc()
    header_cv(d, SO, 'V/v cung cấp nhật ký hệ thống, số liệu giám sát vận hành thử lần 1 và kết quả khắc phục tồn tại',
              'Cần Thơ, ngày 28 tháng 10 năm 2026')
    kinh_gui(d)
    P(d, HD + ' Văn bản NCC-02 ngày 26/10/2026; Báo cáo tự kiểm tra nội bộ, sẵn sàng vận hành thử lần 1 (bản 20 giờ ngày 26/10/2026),')
    P(d, 'Tổng Công ty Viễn thông MobiFone cung cấp nhật ký hệ thống, số liệu giám sát trong vận hành thử lần 1 và kết quả khắc phục '
         'tồn tại 1 – 5 trước nghiệm thu đợt 1 như sau:')
    H(d, '1. Số liệu giám sát')
    P(d, 'Kỳ số liệu gồm: kỳ chuẩn bị từ 08 giờ ngày 26/10/2026 đến 08 giờ ngày 27/10/2026; kỳ vận hành thử lần 1 từ 08 giờ '
         'ngày 27/10/2026 đến …… giờ ngày ……/10/2026. Số đo tại một thời điểm ghi rõ thời điểm đo. Tính sẵn sàng theo hợp đồng '
         '(≥ 99,5%/tháng) được đánh giá theo tháng; số liệu dưới đây là số liệu theo dõi trong kỳ, không dùng để xác nhận đạt '
         'chỉ số tháng.')
    T(d, [7, 22, 18, 30, 25], ['STT', 'Chỉ số', 'Ngưỡng (NCC-02)', 'Kết quả', 'Nguồn, minh chứng'], [
        ['1', 'Tính sẵn sàng (trang chủ, trang đăng nhập, /health)', 'Theo dõi trong kỳ; chỉ số hợp đồng ≥ 99,5%/tháng',
         'Kỳ vận hành thử lần 1: ……% (thời gian hoạt động/tổng thời gian kỳ); số lần gián đoạn: ……; tổng thời gian gián đoạn: …… phút',
         'Công cụ giám sát tính sẵn sàng'],
        ['2', 'Thời gian đáp ứng trang chính', '≤ 03 giây/trang',
         '1,45 – 1,76 giây; 04 đường dẫn /, /dang-nhap, /san-pham, /tin-su-kien × 03 lần; đo bằng curl lúc 09 giờ 04 ngày 27/10/2026',
         'MC-HT-01-20261027'],
        ['3', 'CPU cao nhất MC-01; MC-02', 'Cảnh báo trên 80% liên tục 15 phút', '……%; ……%', 'Trang quản trị hạ tầng cloud'],
        ['4', 'RAM cao nhất MC-01; MC-02', 'Cảnh báo trên 85% liên tục 15 phút', '……%; ……%', 'Trang quản trị hạ tầng cloud'],
        ['5', 'Dung lượng ổ đĩa', 'Cảnh báo sử dụng trên 80%', 'MC-01: còn trống 914,7 GB (09 giờ 03 ngày 27/10/2026); MC-02: ……',
         'MC-HT-03-20261027; trang quản trị hạ tầng cloud'],
        ['6', 'Chứng thư SSL', 'Cảnh báo còn dưới 30 ngày', 'Hiệu lực từ 26/10/2026 đến 24/01/2027', 'MC-HT-04-20261027'],
        ['7', 'Giao thức mã hóa', 'Chỉ TLS 1.2, TLS 1.3', 'TLS 1.0, 1.1: từ chối; TLS 1.2, 1.3: chấp nhận', 'MC-HT-05-20261027'],
        ['8', 'Trang /health', 'Chỉ trạng thái tổng hợp', 'Trả trạng thái tổng hợp, HTTP 200 (sau bản cập nhật 27/10/2026)',
         'MC-HT-03-20261027-2'],
        ['9', 'Sao lưu tự động', 'Không có lần sao lưu lỗi', 'Số lần sao lưu thành công: ……; lỗi: ……', 'Công cụ giám sát; nhật ký tác vụ sao lưu'],
        ['10', 'Cảnh báo phát sinh, sự cố', 'Sự cố nghiêm trọng xử lý ≤ 08 giờ', 'Số cảnh báo: ……; sự cố nghiêm trọng: ……',
         'Lịch sử cảnh báo trên công cụ giám sát'],
    ], size=10.5)
    H(d, '2. Nhật ký hệ thống')
    P(d, 'Tra cứu bằng tài khoản quyền xem gs_theodoi tại CMS › Nhật ký hệ thống, khoảng ngày 26/10/2026 – 27/10/2026, số liệu đến '
         '09 giờ 45 ngày 27/10/2026 (minh chứng MC-HT-10-20261027):')
    T(d, [7, 34, 15, 45], ['STT', 'Loại thao tác', 'Số dòng', 'Ghi chú'], [
        ['1', 'Đăng nhập', '07', 'Mới nhất #1177, 09 giờ 29 ngày 27/10/2026'],
        ['2', 'Đăng nhập không thành công', '02', 'Mới nhất #1169, 16 giờ 26 ngày 26/10/2026'],
        ['3', 'Tải tệp', '04', 'Mới nhất #1175, 16 giờ 41 ngày 26/10/2026'],
        ['4', 'Từ chối thao tác', '10', 'Kiểm tra quyền chỉ xem của tài khoản gs_theodoi; mới nhất #1187, 09 giờ 30 ngày 27/10/2026'],
        ['5', 'Thêm, sửa, xóa', '00', 'Không phát sinh'],
        ['6', 'Yêu cầu hỗ trợ, tư vấn (26/10/2026 – 09 giờ 45 ngày 27/10/2026)', '00', 'Chưa có yêu cầu; không có yêu cầu quá 02 giờ chưa phản hồi'],
    ], size=11, aligns=['center', 'left', 'center', 'left'])
    P(d, 'Mỗi dòng nhật ký có mã, thời điểm, loại thao tác, nội dung, tài khoản, địa chỉ IP; giao diện không có chức năng sửa, xóa '
         'nhật ký. Số liệu đến hết vận hành thử lần 1: ……. Bản trích tệp log ứng dụng, nhật ký truy cập web được cung cấp trong '
         '01 ngày làm việc khi có yêu cầu (NCC-02, Tài liệu số 02).')
    H(d, '3. Nhật ký cập nhật phiên bản trong giai đoạn thiết lập, vận hành thử lần 1')
    T(d, [7, 16, 48, 30], ['STT', 'Ngày', 'Nội dung cập nhật', 'Kết quả kiểm tra sau cập nhật'], [
        ['1', '25/10/2026', 'Gửi tin nhắn thương hiệu; nhật ký đăng nhập, log hệ thống ra tệp; thống kê lượt truy cập, '
                            'thời gian phản hồi hỗ trợ', 'Trang chủ HTTP 200; /health Healthy; /cms chuyển trang đăng nhập'],
        ['2', '26/10/2026', 'Khắc phục L01 (API-05), L02 (mã chống giả mạo 05 API ghi dữ liệu), L03 (API trả lỗi 401 khi '
                            'chưa đăng nhập); gửi thư điện tử qua cổng 465',
         'Kiểm tra lại L01, L02 (04/05 API), L03 đạt (MC-KT-xx-20261026); API-18 đạt ngày 28/10/2026 (MC-KT-38-20261028)'],
        ['3', '27/10/2026', 'Khắc phục L07 (/health chỉ trả trạng thái tổng hợp); tài khoản CMS chỉ xem cho gs_theodoi',
         'MC-HT-03-20261027-2; MC-HT-09-20261027'],
    ], size=11)
    P(d, 'Các lần cập nhật thực hiện theo quy trình quản lý thay đổi, phiên bản (NCC-04, mục VI) và ghi vào Sổ theo dõi phiên bản.')
    H(d, '4. Kết quả khắc phục tồn tại 1 – 5 (Báo cáo tự kiểm tra nội bộ, bản 20 giờ ngày 26/10/2026)')
    T(d, [7, 22, 33, 25, 15], ['STT', 'Tồn tại', 'Đã thực hiện', 'Minh chứng', 'Tình trạng'], [
        ['1', 'Chứng thư SSL thời hạn 90 ngày, chưa có minh chứng gia hạn tự động (L10)',
         'Đã gia hạn ngày 26/10/2026, hiệu lực đến 24/01/2027; cấu hình tác vụ gia hạn tự động: ……',
         'MC-HT-04-20261027; ảnh chụp tác vụ gia hạn tự động, nhật ký lần gia hạn thử: ……', '……'],
        ['2', 'Chứng từ bản quyền Windows Server, SQL Server', '……', 'Chứng từ số ……, ngày ……', '……'],
        ['3', 'L-01: API-05 gợi ý nhà cung cấp lỗi 500 (L01)', 'Sửa lỗi, cập nhật ngày 26/10/2026; kiểm tra lại đạt',
         'MC-KT-06, MC-KT-07-20261026', 'Hoàn thành'],
        ['4', 'L-04: kết quả riêng, ảnh, nhật ký luồng ghi thành công 10 API; API-18; L-02 (L04, L02)',
         'L-02: bổ sung kiểm tra mã chống giả mạo 05 API, cập nhật ngày 26/10/2026; kiểm tra lại đạt 04/05 API (API-06, 07, 11, 17); '
         'API-18 kiểm tra riêng đạt ngày 28/10/2026. Luồng ghi thành công 10 API: ……',
         'MC-KT-08, 09, 17, 24-20261026-vht_buyer01; MC-KT-38-20261028-vht_buyer01; ảnh, nhật ký luồng ghi: ……',
         'L-02: Hoàn thành (05/05 API); L-04: ……'],
        ['5', 'L-05: kênh thông báo thư điện tử đang tắt (L05)',
         'Phần mềm hỗ trợ gửi thư qua cổng 465 (bản cập nhật 26/10/2026); bật kênh, gửi thử thông báo chuyển bước, nhắc lịch, '
         'mã OTP: ……', 'Nhật ký gửi thư (trạng thái Đã gửi), ảnh chụp hộp thư nhận: ……', '……'],
    ], size=10.5)
    P(d, 'Nội dung còn để trống được nhà cung cấp cập nhật kèm minh chứng trước khi đơn vị vận hành thử lập Báo cáo kết quả vận hành '
         'thử lần 1 (VHT-05a) và trước nghiệm thu đợt 1. Tồn tại 6 – 11 thực hiện theo thời hạn tại Báo cáo tự kiểm tra nội bộ.')
    P(d, 'Gửi kèm: thư mục minh chứng MC-HT-xx-20261027, MC-KT-xx-20261026, MC-KT-38-20261028.', indent=False)
    P(d, 'Kết quả khắc phục tồn tại tại mục 4 được các bên xác nhận tại Phụ lục kèm theo văn bản này (nội dung NCC-06a).')
    P(d, 'Tổng Công ty Viễn thông MobiFone trân trọng đề nghị Quý đơn vị xem xét, phối hợp./.')
    ky(d)
    # Phụ lục: xác nhận 4 bên kết quả khắc phục (NCC-06a, Checklist V7 ô K18)
    d.add_page_break()
    P(d, [('PHỤ LỤC', 'b')], align='center', indent=False, after=0)
    P(d, [('XÁC NHẬN KẾT QUẢ KHẮC PHỤC TỒN TẠI 1 – 5 TRƯỚC NGHIỆM THU ĐỢT 1', 'b')], align='center', indent=False)
    P(d, 'Hôm nay, ngày …… tháng 10 năm 2026, đại diện các bên đã xem xét kết quả khắc phục tồn tại 1 – 5 tại mục 4 văn bản này, '
         'đối chiếu minh chứng kèm theo và kiểm tra trên hệ thống https://mekongx.vn.')
    T(d, [7, 35, 22, 36], ['STT', 'Tồn tại', 'Kết quả kiểm tra', 'Ý kiến của đơn vị vận hành thử, đơn vị giám sát'], [
        ['1', 'Gia hạn tự động chứng thư SSL (L10)', '☐ Đạt  ☐ Chưa đạt', ''],
        ['2', 'Chứng từ bản quyền Windows Server, SQL Server', '☐ Đạt  ☐ Chưa đạt', ''],
        ['3', 'API-05 gợi ý nhà cung cấp (L01)', '☐ Đạt  ☐ Chưa đạt', ''],
        ['4', 'Mã chống giả mạo 05 API (L02); luồng ghi thành công 10 API, API-18 (L04)', '☐ Đạt  ☐ Chưa đạt', ''],
        ['5', 'Kênh thông báo thư điện tử (L05)', '☐ Đạt  ☐ Chưa đạt', ''],
    ], size=11, aligns=['center', 'left', 'center', 'left'])
    P(d, 'Kết luận: ☐ Các tồn tại 1 – 5 đã khắc phục đạt yêu cầu;  ☐ Chưa đạt, đề nghị nhà cung cấp tiếp tục khắc phục: '
         '……………………………………………………', indent=False)
    # 2 hàng × 2 bên (sign_grid dùng lại biến k trong vòng lặp nên chỉ gọi tối đa 2 bên mỗi lần)
    sign_grid(d, [('ĐẠI DIỆN NHÀ CUNG CẤP DỊCH VỤ', 'Lã Thành Trung'), ('ĐẠI DIỆN ĐƠN VỊ VẬN HÀNH THỬ', '……………………')])
    sign_grid(d, [('ĐẠI DIỆN ĐƠN VỊ GIÁM SÁT', '……………………'), ('XÁC NHẬN CỦA CHỦ TRÌ THUÊ', '……………………')])
    dst =os.path.join(OUT, 'NCC-06 Nhat ky he thong, so lieu giam sat lan 1 va ket qua khac phuc ton tai')
    save(d, dst + '.docx')
    mc = os.path.join(OUT, 'NCC-06 Minh chung')
    if os.path.exists(mc):
        shutil.rmtree(mc)
    os.makedirs(mc)
    for src in (MC27, MC28, MC26):
        for f in os.listdir(src):
            if f.startswith(('MC-HT', 'MC-KT')) and (src != MC26 or f.startswith('MC-KT')):
                shutil.copy2(os.path.join(src, f), mc)


# ---------------------------------------------------------------- GS-02, NCC-08: chỉ đổi mã tham chiếu
def doi_ma():
    gs = '1 - 26.10.2026 - GS-02 Bien ban kiem tra ha tang thue va chat luong dich vu - V5.docx'
    d = docx.Document(os.path.join(BB, gs))
    print('GS-02:', replace_doc(d))
    # Hồ sơ dẫn theo NCC-02, Tài liệu số 01 đã sửa: chỉ có sao lưu, không có snapshot
    print('GS-02 sao lưu:', replace_doc(d, [(r'sao lưu, snapshot ở kho sao lưu của hạ tầng cloud',
                                             'sao lưu lưu tại máy chủ, tải về định kỳ; không sử dụng snapshot'),
                                            (r'bản sao lưu, snapshot ở kho sao lưu tách biệt', 'bản sao lưu lưu tại máy chủ, tải về định kỳ'),
                                            (r'^Snapshot, sao lưu$', 'Sao lưu'),
                                            (r'Nhà cung cấp tự đo 1,24 – 1,67 giây \(01/03 khung giờ\)',
                                             'Nhà cung cấp tự đo 1,24 – 1,67 giây (thời gian đáp ứng HTTP, 04 trang × 03 lần)')]))
    save(d, os.path.join(OUT, gs))
    d = docx.Document(os.path.join(BB, '4-Cong van de nghi nghiem thu dot 1 (du thao).docx'))
    n = replace_doc(d)  # đổi mã chung trước, sau đó mới ghi các mã mới (tránh NCC-06 mới bị đổi tiếp)
    n += replace_doc(d, [
        (r'^Công văn cung cấp tài khoản quyền xem giám sát và tài liệu \(ngày 26/10/2026\) kèm 11 tài liệu$',
         'Văn bản đề xuất chỉ số, ngưỡng chất lượng dịch vụ; cung cấp tài khoản quyền xem giám sát và tài liệu (NCC-02, '
         'ngày 26/10/2026), kèm 11 tài liệu'),
        (r'^Báo cáo tự kiểm tra nội bộ, sẵn sàng vận hành thử lần 1; Biên bản tự kiểm thử chức năng phần mềm đợt 1$',
         'Báo cáo tự kiểm tra nội bộ, sẵn sàng vận hành thử lần 1; Biên bản tự kiểm thử chức năng phần mềm đợt 1; '
         'Thông báo sẵn sàng vận hành thử lần 1 (NCC-03)'),
        (r'^Biên bản ghi nhận lỗi; Biên bản khắc phục lỗi vận hành thử lần 1$',
         'Biên bản ghi nhận lỗi (VHT-04); Biên bản khắc phục lỗi vận hành thử lần 1 (NCC-05)'),
        (r'^Minh chứng cấu hình gia hạn tự động chứng thư SSL$',
         'Nhật ký hệ thống, số liệu giám sát lần 1 và kết quả khắc phục tồn tại 1 – 5, gồm minh chứng gia hạn tự động '
         'chứng thư SSL (NCC-06)'),
        (r'Biên bản khắc phục lỗi vận hành thử lần 1 ngày', 'Biên bản khắc phục lỗi vận hành thử lần 1 (NCC-05) ngày'),
        # chưa khẳng định hoàn thành khi kết quả khắc phục chưa được các bên xác nhận
        (r'đã hoàn thành khối lượng thiết lập dịch vụ đợt 1, vận hành thử lần 1 và khắc phục các tồn tại theo yêu cầu của '
         r'đơn vị giám sát\.',
         'đã hoàn thành khối lượng thiết lập dịch vụ đợt 1; kết quả vận hành thử lần 1 và khắc phục các tồn tại theo yêu cầu '
         'của đơn vị giám sát được các bên xác nhận tại Biên bản khắc phục lỗi (NCC-05) và Phụ lục xác nhận kết quả khắc phục '
         'tồn tại kèm NCC-06 ngày …….'),
    ])
    print('NCC-08:', n)
    save(d, os.path.join(OUT, 'NCC-08 Cong van de nghi nghiem thu dot 1 (du thao).docx'))


if __name__ == '__main__':
    ncc03()
    vht04_ncc05()
    ncc06()
    doi_ma()
