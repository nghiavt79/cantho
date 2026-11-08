"""Điền phần nhà cung cấp trong Biên bản GS-02 (bản V4 của đơn vị giám sát) và hoàn thiện NCC-06 (27/10/2026).
Chỉ điền nội dung có minh chứng; ô cần số liệu từ trang quản trị cloud, chứng từ để nguyên ……"""
import copy, os, re, sys
import docx
sys.path.insert(0, os.path.dirname(__file__))
from lib27 import save

OUT = r'D:\2026\CanTho\docs\27-10-2026'
GS02_NAME = '26.10.2026 - GS-02 Bien ban kiem tra ha tang thue va chat luong dich vu - V4.docx'
GS02 = os.path.join(os.path.dirname(__file__), '_goc', GS02_NAME)  # bản gốc của đơn vị giám sát, chưa điền
NCC06 = r'D:\2026\CanTho\docs\Ho_so_muc_1.1_1.5_1.6\1.1 Khoi tao ha tang, cong cu giam sat\NCC-06 Van ban cung cap tai khoan xem cong cu giam sat.docx'


def cells(row):
    out, seen = [], set()
    for c in row.cells:
        if id(c._tc) in seen: continue
        seen.add(id(c._tc)); out.append(c)
    return out


def find_row(t, key):
    for r in t.rows:
        if cells(r)[0].text.strip() == key:
            return r
    raise SystemExit(f'không thấy dòng {key}')


def set_text(cell, text):
    ps = cell.paragraphs
    runs = ps[0].runs
    if runs:
        runs[0].text = text
        for r in runs[1:]: r.text = ''
    else:
        ps[0].add_run(text)
    for p in ps[1:]:
        p._p.getparent().remove(p._p)


def tick(cell, which):
    """which: 'Đạt' | 'Chưa đạt' | 'Theo dõi' — đổi ☐ thành ☒ ở lựa chọn tương ứng."""
    for p in cell.paragraphs:
        label = p.text.replace('☐', '').replace('☒', '').strip()
        if label == which:
            for r in p.runs:
                if '☐' in r.text:
                    r.text = r.text.replace('☐', '☒', 1); return
    raise SystemExit(f'không tick được {which}: {cell.text}')


def para_replace_all(d, old, new):
    n = 0
    for p in d.paragraphs:
        if old in p.text:
            full = p.text.replace(old, new)
            p.runs[0].text = full
            for r in p.runs[1:]: r.text = ''
            n += 1
    if not n: raise SystemExit(f'không thấy đoạn: {old[:40]}')


def gs02():
    d = docx.Document(GS02)
    T = d.tables
    ht, cl, tt = T[2], T[5], T[6]

    r = cells(find_row(ht, '2.7'))
    set_text(r[4], 'Truy cập https://mekongx.vn bình thường; TLS 1.0, TLS 1.1 bị từ chối; TLS 1.2, TLS 1.3 được chấp nhận (MC-HT-05-20261027)')
    tick(r[5], 'Đạt')
    r = cells(find_row(ht, '2.8'))
    set_text(r[4], 'Chứng thư cấp cho mekongx.vn, hiệu lực từ 26/10/2026 đến 24/01/2027 (MC-HT-04-20261027); minh chứng cấu hình gia hạn tự động: ……')
    r = cells(find_row(ht, '2.9'))
    set_text(r[4], 'Chưa có bản ghi AAAA, chỉ có bản ghi IPv4 (MC-HT-06-20261027); ghi nhận lỗi VHT-04, hạn bổ sung trước 09/11/2026')
    tick(r[5], 'Chưa đạt')
    r = cells(find_row(ht, '2.10'))
    set_text(r[4], 'Trước khắc phục: trả chi tiết từng thành phần và dung lượng ổ đĩa (MC-HT-03-20261027, lỗi VHT-02). '
                   'Sau cập nhật ngày 27/10/2026: chỉ trả trạng thái tổng hợp "Healthy", mã HTTP 200 (MC-HT-03-20261027-2)')
    tick(r[5], 'Đạt')
    r = cells(find_row(ht, '2.13'))
    set_text(r[4], 'Trang quản trị: trước khắc phục gs_theodoi có quyền quản trị đầy đủ (lỗi VHT-01); sau cập nhật ngày 27/10/2026 chỉ xem '
                   'Tổng quan, Nhật ký, Thống kê, Yêu cầu hỗ trợ; thao tác thêm, sửa, xóa bị từ chối, ghi nhật ký "Từ chối thao tác" (MC-HT-09-20261027). '
                   'Công cụ giám sát, trang quản trị cloud: ……')

    r = cells(find_row(cl, '7'))
    set_text(r[3], 'Nhà cung cấp tự đo: 1,24 – 1,67 giây (26/10/2026); 1,45 – 1,76 giây (27/10/2026, MC-HT-01-20261027); '
                   'kết quả đo của đơn vị vận hành thử: ……; kiểm thử tải 200 người dùng đồng thời thực hiện trong vận hành thử lần 2')

    set_text(cells(find_row(tt, '5'))[4], 'Đang thực hiện')
    set_text(cells(find_row(tt, '6'))[4], 'Đang khắc phục (lỗi VHT-03)')
    set_text(cells(find_row(tt, '7'))[4], 'Đã khắc phục ngày 26/10/2026; kiểm tra lại trả kết quả, mã HTTP 200 (MC-KT-07-20261026); chờ đơn vị giám sát kiểm tra độc lập')
    for k, v in [('10', 'Đang thực hiện'), ('11', 'Đang thực hiện'), ('12', 'Đang thực hiện'),
                 ('13', 'Đang thực hiện; kết quả chức năng ghi theo mục 4 phần IV')]:
        set_text(cells(find_row(tt, k))[4], v)

    # Thành phần: chức vụ theo Quy trình kiểm thử nội bộ
    W = '{http://schemas.openxmlformats.org/wordprocessingml/2006/main}t'
    for row in T[0].rows:
        if 'Lã Thành Trung' in ''.join(t.text or '' for t in row._tr.iter(W)):
            for c in cells(row):
                if c.text.strip().startswith('Chức vụ:'):
                    set_text(c, 'Chức vụ: Phó Giám đốc phụ trách MobiFone Cần Thơ')
    for p in cells(T[0].rows[0])[0]._tc.getparent().getparent().iter('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}t'):
        if p.text and '………… (phụ trách hạ tầng)' in p.text:
            p.text = p.text.replace('………… (phụ trách hạ tầng)', 'Kỹ sư hạ tầng (phụ trách hạ tầng)')

    # Mục 1: hồ sơ chưa có tại thời điểm kiểm tra (theo tồn tại 1, 2 mục 6)
    hs = T[1]
    r = cells(find_row(hs, '2'))
    set_text(r[2], 'Chưa có tại thời điểm kiểm tra; bàn giao trước nghiệm thu đợt 1 (tồn tại 2)'); set_text(r[3], 'Chưa cung cấp')
    r = cells(find_row(hs, '3'))
    set_text(r[2], 'Chưa có tại thời điểm kiểm tra (tồn tại 1)'); set_text(r[3], 'Chưa cung cấp')

    # Mục 2: nội dung xác định bằng hồ sơ
    set_text(cells(find_row(ht, '2.11'))[4], 'Chưa có chứng từ bản quyền tại thời điểm kiểm tra; bàn giao trước nghiệm thu đợt 1 (tồn tại 2)')
    set_text(cells(find_row(ht, '2.12'))[4], 'Chưa có văn bản xác nhận vị trí trung tâm dữ liệu tại thời điểm kiểm tra (tồn tại 3)')

    # Mục 3: khối lượng
    kl = T[3]
    set_text(cells(find_row(kl, '1'))[3], '60/63 chức năng đề nghị đợt 1: phần A đạt 33/37, đạt một phần 04 (phần gửi thư điện tử, lỗi VHT-03); '
                                           'phần B đạt 15/16; phần C đạt 08/10 (mục 4); vận hành thử lần 1 đang thực hiện')
    set_text(cells(find_row(kl, '2'))[3], 'Đã cài đặt, cấu hình hệ thống trên máy chủ cloud; đã cung cấp tài liệu hướng dẫn sử dụng (người sử dụng, '
                                           'cán bộ quản trị, hướng dẫn nhanh), đặc tả API, đặc tả cơ sở dữ liệu, quy trình vận hành (NCC-14); '
                                           'đào tạo từ 29/10/2026 đến 06/11/2026')
    for k in ('3', '4', '5', '6'):
        set_text(cells(find_row(kl, k))[3], 'Chưa có chứng từ bản quyền tại thời điểm kiểm tra (tồn tại 2)')
    set_text(cells(find_row(kl, '8'))[3], 'Chưa có chứng từ đăng ký thương hiệu tại thời điểm kiểm tra (tồn tại 4); kết quả gửi tin thật: ……')
    set_text(cells(find_row(kl, '9'))[3], 'Đã cấu hình hộp thư no-reply@mekongx.vn cho hệ thống gửi thư (26/10/2026); kênh gửi thư đang khắc phục (VHT-03); '
                                           'chứng từ kích hoạt, danh sách địa chỉ: chưa có (tồn tại 4)')
    for k in ('10', '11', '12'):
        set_text(cells(find_row(kl, k))[3], 'Chưa có chứng từ tại thời điểm kiểm tra (tồn tại 4)')

    # Mục 4: chức năng — theo Biên bản tự kiểm thử chức năng phần mềm đợt 1
    cn = T[4]
    r = cells(find_row(cn, '1'))
    set_text(r[3], 'Đạt 33/37; đạt một phần 04: A.1.4, A.4.1, A.4.5, A.8.4 (phần gửi thư điện tử, lỗi VHT-03)')
    r = cells(find_row(cn, '2'))
    set_text(r[3], 'Đạt 15/16; B.1.2 tham quan thực tế ảo VR360 thực hiện đợt 2'); tick(r[4], 'Đạt')
    r = cells(find_row(cn, '3'))
    set_text(r[3], 'Đạt 08/10 (C.4 ở chế độ thử nghiệm; C.7 phần trò chuyện trực tuyến); C.1 tham quan VR360, C.2 tích hợp với sàn giao dịch khác: thực hiện đợt 2')
    tick(r[4], 'Đạt')
    r = cells(find_row(cn, '4'))
    set_text(r[3], 'Thông báo trên web: đạt; thư điện tử: chưa đạt, kênh gửi thư đang khắc phục (VHT-03); SMS: chờ tài khoản thương hiệu')
    tick(r[4], 'Chưa đạt')
    r = cells(find_row(cn, '5'))
    set_text(r[3], 'Ký số ở chế độ thử nghiệm: đạt (TC-CN-54); ký bằng chứng thư số của tổ chức chứng thực: đợt 2'); tick(r[4], 'Đạt')
    r = cells(find_row(cn, '6'))
    set_text(r[3], 'Trò chuyện trực tuyến giữa người bán, người mua và Sàn: đạt (TC-CN-57); gọi video: đợt 2'); tick(r[4], 'Đạt')
    r = cells(find_row(cn, '7'))
    set_text(r[3], 'Đạt 14/14 bước trên hồ sơ có và không áp dụng thỏa thuận bảo mật (TC-QT-01 – TC-QT-14); bước 7 ký ở chế độ thử nghiệm; '
                   'thông báo thư điện tử kiểm tra lại sau khi khắc phục VHT-03')
    tick(r[4], 'Đạt')

    # Mục 5: chất lượng dịch vụ — số liệu tra cứu bằng gs_theodoi
    set_text(cells(find_row(cl, '2'))[3], 'Số yêu cầu 0 (từ 26/10/2026 đến 27/10/2026); chưa phát sinh yêu cầu để tính tỷ lệ (MC-HT-10-20261027)')
    set_text(cells(find_row(cl, '3'))[3], 'Chưa ghi nhận sự cố gián đoạn dịch vụ; lỗi VHT-01 (mức Cao), VHT-02 khắc phục trong ngày 27/10/2026')
    set_text(cells(find_row(cl, '5'))[3], 'Ổ đĩa máy chủ ứng dụng còn trống 914,7 GB (MC-HT-03-20261027); CPU cao nhất ……%; RAM ……%')
    set_text(cells(find_row(cl, '10'))[3], 'Tra cứu bằng gs_theodoi (26/10 – 27/10/2026): nhật ký đăng nhập 07, đăng nhập không thành công 02, tải tệp 04, '
                                           'từ chối thao tác 10; mỗi dòng có thời điểm, tài khoản, IP; giao diện chỉ tìm kiếm, không có chức năng sửa, xóa '
                                           '(MC-HT-10-20261027). Nhật ký hồ sơ giao dịch, ký số: ……')

    last = tt.rows[-1]._tr
    for stt, ton_tai, yc, th, tinh_trang in [
        ('14', 'Tài khoản gs_theodoi có quyền quản trị đầy đủ, chưa giới hạn chỉ xem (lỗi VHT-01)',
         'Giới hạn chỉ xem; thao tác sửa bị từ chối, ghi nhật ký', '17h ngày 27/10/2026', 'Đã khắc phục ngày 27/10/2026'),
        ('15', 'Trang /health hiển thị chi tiết từng thành phần, dung lượng ổ đĩa (lỗi VHT-02)',
         'Chỉ trả trạng thái tổng hợp', '17h ngày 27/10/2026', 'Đã khắc phục ngày 27/10/2026')]:
        nr = copy.deepcopy(last); tt._tbl.append(nr)
        row = tt.rows[-1]
        for c, v in zip(cells(row), [stt, ton_tai, yc, th, tinh_trang]):
            set_text(c, v)

    # Ý kiến nhà cung cấp: giữ nhãn in đậm, chỉ thay phần chấm ở run thứ hai
    p = next(p for p in d.paragraphs if p.text.startswith('2. Nhà cung cấp dịch vụ:'))
    p.runs[1].text = (' đã khắc phục lỗi VHT-01, VHT-02 ngày 27/10/2026; khắc phục VHT-03 trong ngày 27/10/2026; '
                      'bổ sung IPv6 (VHT-04) trước ngày 09/11/2026; bổ sung chứng từ bản quyền, chứng từ kích hoạt các gói dịch vụ tích hợp, '
                      'tài liệu vị trí trung tâm dữ liệu trước nghiệm thu đợt 1 (chi tiết tại Biên bản ghi nhận lỗi vận hành thử lần 1 '
                      'và Biên bản kết quả khắc phục lỗi lần 1 ngày 27/10/2026).')
    tmp = os.path.join(os.path.dirname(__file__), '_goc', '_tmp_gs02.docx')
    save(d, tmp)
    # Giữ nguyên mọi phần khác của file gốc (styles, settings, footer...), chỉ thay nội dung và thông tin tác giả
    import zipfile
    out = os.path.join(OUT, GS02_NAME)
    with zipfile.ZipFile(GS02) as zo, zipfile.ZipFile(tmp) as zn, zipfile.ZipFile(out, 'w', zipfile.ZIP_DEFLATED) as zw:
        for info in zo.infolist():
            data = zn.read(info.filename) if info.filename in ('word/document.xml', 'docProps/core.xml') else zo.read(info.filename)
            zw.writestr(info, data)
    os.remove(tmp)
  # điền thẳng vào file của đơn vị giám sát, giữ nguyên tên


def ncc06():
    d = docx.Document(NCC06)
    hdr = d.tables[0]
    for p in cells(hdr.rows[0])[0].paragraphs:
        if p.text.startswith('V/v'):
            p.runs[0].text = 'V/v cung cấp tài khoản quyền xem giám sát và các tài liệu có liên quan'
            for r in p.runs[1:]: r.text = ''
    para_replace_all(d, '- Minh chứng: ảnh chụp nội dung trả về khi truy cập https://mekongx.vn/health từ mạng bên ngoài, ngày ……/……/2026 (kèm theo văn bản).',
                     '- Minh chứng: ảnh chụp nội dung trả về khi truy cập https://mekongx.vn/health từ mạng bên ngoài (kèm theo văn bản).')
    para_replace_all(d, 'Người nhận tài khoản (đơn vị giám sát): Ông/Bà ………………………, chức vụ ………………………',
                     'Người nhận tài khoản (đơn vị giám sát): Ông Nguyễn Minh Tuyển, chức vụ Giám sát trưởng')
    # Mục 5: tài liệu gửi kèm (đối tượng kiểm tra tại mục IV.1 Biên bản GS-02)
    anchor = next(p for p in d.paragraphs if p.text.startswith('Người nhận tài khoản'))
    h4 = next(p for p in d.paragraphs if p.text.startswith('4. Quyền xem'))
    item = next(p for p in d.paragraphs if p.text.startswith('- Tài khoản gs_theodoi chỉ có quyền xem'))
    new_h = copy.deepcopy(h4._p); anchor._p.addprevious(new_h)
    from docx.text.paragraph import Paragraph
    ph = Paragraph(new_h, anchor._parent); ph.runs[0].text = '5. Tài liệu gửi kèm'
    for r in ph.runs[1:]: r.text = ''
    for txt in ['- Danh mục chức năng, thông số hiệu năng, phương án mở rộng (NCC-11);',
                '- Văn bản cung cấp log, nhật ký, số liệu giám sát (NCC-12);',
                '- Quy trình vận hành, xử lý sự cố, hỗ trợ, sao lưu, quản lý thay đổi (NCC-14);',
                '- Báo cáo tự kiểm tra nội bộ, sẵn sàng vận hành thử lần 1;',
                '- Biên bản tự kiểm thử chức năng phần mềm đợt 1;',
                '- Thông tin địa chỉ truy cập, mật khẩu ban đầu của tài khoản gs_theodoi (gửi riêng qua kênh bảo mật).']:
        np_ = copy.deepcopy(item._p); anchor._p.addprevious(np_)
        pp = Paragraph(np_, anchor._parent); pp.runs[0].text = txt
        for r in pp.runs[1:]: r.text = ''
    # Mục 2: ghi rõ hạ tầng được giám sát để đơn vị giám sát đối chiếu (yêu cầu chị Quyên 27/10/2026)
    h2 = next(p for p in d.paragraphs if p.text.startswith('2. Chu kỳ giám sát'))
    h2.runs[0].text = '2. Hạ tầng được giám sát, chu kỳ giám sát và ngưỡng cảnh báo'
    for r in h2.runs[1:]: r.text = ''
    mon = next(t for t in d.tables if t.rows[0].cells[1].text.strip() == 'Nội dung giám sát')
    intro = h2._p.getnext()  # đoạn "Chu kỳ giám sát được cấu hình..."
    lead = copy.deepcopy(intro); intro.addprevious(lead)
    pl = Paragraph(lead, h2._parent)
    pl.runs[0].text = ('Hạ tầng thuê theo hợp đồng gồm 02 máy chủ cloud và kho sao lưu của hạ tầng cloud. Cấu hình, địa chỉ thực tế '
                       'do bộ phận hạ tầng của nhà cung cấp ghi khi bàn giao, đối chiếu được trên trang quản trị hạ tầng cloud bằng tài khoản quyền xem:')
    for r in pl.runs[1:]: r.text = ''
    infra = copy.deepcopy(mon._tbl); intro.addprevious(infra)
    from docx.table import Table
    it = Table(infra, h2._parent)
    for tr in it.rows[4:]:
        tr._tr.getparent().remove(tr._tr)
    data = [
        ['Mã', 'Thành phần, vai trò', 'Cấu hình theo hợp đồng', 'Cấu hình, địa chỉ thực tế', 'Nội dung giám sát'],
        ['MC-01', 'Máy chủ ứng dụng web: IIS, ASP.NET Core 8; lưu tệp tải lên, nhật ký ứng dụng',
         'CPU Intel Xeon Gold 6330, 8 core; RAM 32 GB; SSD 500 GB; 01 IP; Windows Server 2025 Standard',
         'Địa chỉ công khai của tên miền mekongx.vn: 103.156.3.5; IP nội bộ: ……; CPU, RAM, SSD: ……',
         'CPU, RAM, dung lượng từng ổ đĩa, băng thông; tính sẵn sàng trang chủ, trang đăng nhập, trang /health'],
        ['MC-02', 'Máy chủ cơ sở dữ liệu SQL Server 2025 Standard',
         'CPU Intel Xeon Gold 6330, 8 core; RAM 32 GB; SSD 500 GB; 01 IP; Windows Server 2025 Standard',
         'IP: ……; CPU, RAM, SSD: ……',
         'CPU, RAM, dung lượng từng ổ đĩa (dữ liệu, nhật ký cơ sở dữ liệu), băng thông; trạng thái cơ sở dữ liệu qua trang /health'],
        ['KSL', 'Kho sao lưu của hạ tầng cloud, tách biệt 02 máy chủ; đặt tại Việt Nam',
         'Snapshot, sao lưu đầy đủ 01:00, vi sai 07:00, 13:00, 19:00; lưu tối thiểu 30 ngày (NCC-14)',
         'Bản sao lưu đang lưu tại máy chủ và được tải về định kỳ; chưa có kho lưu trữ tách biệt, chưa có snapshot',
         'Kết quả từng lần sao lưu; dung lượng bản sao lưu trên máy chủ; thời điểm tải về gần nhất'],
    ]
    for row, vals in zip(it.rows, data):
        for c, v in zip(cells(row), vals):
            set_text(c, v)
    gap = copy.deepcopy(intro); intro.addprevious(gap)
    for r in Paragraph(gap, h2._parent).runs: r.text = ''
    # Bảng ngưỡng: ghi rõ áp dụng cho máy chủ nào
    for key, txt in [('2', 'CPU, RAM của MC-01, MC-02'),
                     ('3', 'Dung lượng ổ đĩa của MC-01, MC-02 (gồm thư mục chứa bản sao lưu)'),
                     ('5', 'Sao lưu tự động MC-01, MC-02; tải bản sao lưu về định kỳ')]:
        set_text(cells(find_row(mon, key))[1], txt)
    # Mục 1: chưa có snapshot (dòng KSL)
    for t in d.tables:
        for r in t.rows:
            for c in cells(r):
                if c.text.strip() == 'CPU, RAM, ổ đĩa, băng thông của 02 máy chủ; trạng thái snapshot, sao lưu':
                    set_text(c, 'CPU, RAM, ổ đĩa, băng thông của 02 máy chủ; trạng thái sao lưu')
    # Khách: bỏ các mã ký hiệu văn bản (NCC-xx)
    for p in list(d.paragraphs):
        if p.text.startswith('- Danh mục số liệu giám sát, nhật ký, thời hạn lưu và quyền xem thống nhất'):
            p._p.getparent().remove(p._p)
    def bo_ma(s):
        s = s.replace('; thời hạn lưu theo NCC-12', '').replace(' tại NCC-14', '')
        return re.sub(r'\s*\(NCC-\d+\)', '', s)
    for p in d.paragraphs:
        if 'NCC-' in p.text:
            for r in p.runs: r.text = bo_ma(r.text)
    for t in d.tables:
        for r in t.rows:
            for c in cells(r):
                if 'NCC-' in c.text:
                    set_text(c, bo_ma(c.text))
    save(d, os.path.join(OUT, 'NCC-06 Cong van cung cap tai khoan quyen xem giam sat va tai lieu lien quan.docx'))


if __name__ == '__main__':
    gs02(); ncc06(); print('ok')
