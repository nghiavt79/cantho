"""Bảng xác định giá trị khối lượng đợt 1 – theo Phụ lục 5 chị Vân xác nhận 21/10/2026."""
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from openpyxl import Workbook
from openpyxl.styles import Font, Alignment, PatternFill, Border, Side
from openpyxl.worksheet.datavalidation import DataValidation
from openpyxl.formatting.rule import CellIsRule
from data_dot1 import *

out = sys.argv[1]
F = 'Times New Roman'
thin = Side(style='thin', color='999999'); B = Border(left=thin, right=thin, top=thin, bottom=thin)
HEAD = PatternFill('solid', fgColor='D9E2F3'); GRP = PatternFill('solid', fgColor='E2EFDA'); D2F = PatternFill('solid', fgColor='FCE4D6')
NUM = '#,##0'
ST = ['Hoàn tất', 'Đang thực hiện', 'Chưa hoàn tất']
wb = Workbook()


def head(ws, title, sub, headers, widths):
    ws['A1'] = title; ws['A1'].font = Font(name=F, bold=True, size=14)
    ws['A2'] = sub; ws['A2'].font = Font(name=F, italic=True)
    ws.append([]); ws.append(headers)
    for c in ws[4]:
        c.font = Font(name=F, bold=True); c.fill = HEAD; c.border = B
        c.alignment = Alignment(horizontal='center', vertical='center', wrap_text=True)
    for i, w in enumerate(widths):
        ws.column_dimensions[chr(65 + i)].width = w
    ws.freeze_panes = 'A5'


def row(ws, vals, fill=None, bold=False, center=(), num=()):
    ws.append(list(vals)); r = ws.max_row
    for col in range(1, len(vals) + 1):
        c = ws.cell(r, col); c.font = Font(name=F, bold=bold); c.border = B
        c.alignment = Alignment(vertical='top', wrap_text=True, horizontal='center' if col in center else None)
        if fill: c.fill = fill
        if col in num: c.number_format = NUM
    return r


def status(ws, col, r1, r2):
    dv = DataValidation(type='list', formula1='"' + ','.join(ST) + '"', allow_blank=True)
    ws.add_data_validation(dv); dv.add(f'{col}{r1}:{col}{r2}')
    rng = f'{col}{r1}:{col}{r2}'
    for v, color in (('Hoàn tất', 'C6EFCE'), ('Đang thực hiện', 'FFEB9C'), ('Chưa hoàn tất', 'F8CBAD')):
        ws.conditional_formatting.add(rng, CellIsRule(operator='equal', formula=[f'"{v}"'], fill=PatternFill('solid', fgColor=color)))


def note(ws, text):
    ws.append([]); ws.append(['', text]); r = ws.max_row
    ws.cell(r, 2).font = Font(name=F, italic=True); ws.cell(r, 2).alignment = Alignment(wrap_text=True)


# ---------- Sheet tổng hợp
ws = wb.active; ws.title = 'Tổng hợp đợt 1'
head(ws, 'BẢNG XÁC ĐỊNH GIÁ TRỊ KHỐI LƯỢNG CÔNG VIỆC ĐỢT 1',
     'Theo Phụ lục 5 – Giá trị nghiệm thu dự kiến đợt 1. Đơn vị: đồng, đã gồm thuế.',
     ['Mục', 'Hạng mục', 'Đợt 1', 'Thời gian thực hiện', 'Công việc thực hiện'], [9, 50, 17, 16, 60])
r1 = row(ws, ('1.1', 'Thiết lập trang web Sàn giao dịch công nghệ Cần Thơ (75%)', D11, '21 – 28/10',
              '60 chức năng theo 12 nhóm Phụ lục 3 – xem sheet "Mục 1.1"'), center=(1, 4), num=(3,))
row(ws, ('1.2', 'Cài đặt, cấu hình, hướng dẫn sử dụng, vận hành, bảo hành, bảo trì, hỗ trợ kỹ thuật 05 năm (80%)', D12, '21 – 30/10',
         '06 nhóm công việc – xem sheet "Mục 1.2"'), center=(1, 4), num=(3,))
for k in KHAC:
    row(ws, (k[0], k[1], k[2], k[3], k[4]), center=(1, 4), num=(3,))
last = ws.max_row
row(ws, ('', 'Tổng cộng đợt 1', f'=SUM(C{r1}:C{last})', '', ''), bold=True, num=(3,), fill=GRP)
note(ws, 'VR360 (10 điểm cảnh quay dựng mới) không thuộc đợt 1 theo Phụ lục 5; thực hiện và nghiệm thu ở đợt 2.')
note(ws, 'Lịch thực hiện: xem kế hoạch triển khai.')

# ---------- Sheet mục 1.1
ws = wb.create_sheet('Mục 1.1')
head(ws, 'MỤC 1.1 – THIẾT LẬP TRANG WEB SÀN GIAO DỊCH CÔNG NGHỆ CẦN THƠ',
     f'Giá trị mục 1.1: 1.187.709.727 đồng. Đợt 1 (75%): 890.782.296 đồng, gồm 60 chức năng, chia theo 12 nhóm chức năng Phụ lục 3.',
     ['STT', 'Nhóm chức năng / Mã – chức năng', 'Ngày thực hiện', 'Số chức năng', 'Giá trị đợt 1', 'Trạng thái'], [7, 62, 14, 11, 17, 15])
tien = tien_nhom()
first = ws.max_row + 1
for so, ten, ngay, cns in NHOM:
    if not cns:
        row(ws, (str(so), f'{so}. {ten} – thực hiện tại mục 1.2 và mục 2', '', 0, 0, ''), fill=GRP, bold=True, center=(1, 3, 4), num=(5,))
        continue
    row(ws, (str(so), f'{so}. {ten}', ngay, len(cns), tien[so], 'Hoàn tất'), fill=GRP, bold=True, center=(1, 3, 4, 6), num=(5,))
    for ma, cn in cns:
        row(ws, ('', f'   {ma}  {cn}', ngay, '', '', ''), center=(3,))
lastr = ws.max_row
status(ws, 'F', first, lastr)
row(ws, ('', 'Cộng đợt 1 (60 chức năng)', '', N_CN, f'=SUM(E{first}:E{lastr})', ''),
    bold=True, center=(4,), num=(5,), fill=GRP)
ws.append([])
row(ws, ('', 'Chuyển đợt 2 (25% mục 1.1 = 296.927.431 đồng)', '', '', '', ''), bold=True, fill=D2F)
for ma, cn in DOT2_11:
    row(ws, ('', f'   {ma}  {cn}', 'Tháng 11', '', '', 'Chưa hoàn tất'), center=(3, 6))
row(ws, ('', '   Hoàn thiện, kiểm thử toàn bộ, nghiệm thu đưa vào vận hành', 'Tháng 11', '', '', 'Chưa hoàn tất'), center=(3, 6))
note(ws, 'Phụ lục 3 có 63 chức năng (A 37, B 16, C 10). Mã C.7 “Liên hệ trực tiếp” gồm chat và gọi video: phần chat ở đợt 1 (nhóm 4), phần gọi video ở đợt 2. '
         'Giá trị đợt 1 chia đều theo số chức năng của từng nhóm (để các bên xác nhận). Nhóm 11 không có chức năng phần mềm riêng, thuộc mục 1.2 và mục 2.')

# ---------- Sheet mục 1.2
ws = wb.create_sheet('Mục 1.2')
head(ws, 'MỤC 1.2 – CÀI ĐẶT, CẤU HÌNH, HƯỚNG DẪN SỬ DỤNG, VẬN HÀNH, BẢO HÀNH, BẢO TRÌ, HỖ TRỢ KỸ THUẬT 05 NĂM',
     'Giá trị mục 1.2: 558.775.400 đồng. Đợt 1 (80%): 447.020.320 đồng.',
     ['Mã', 'Công việc', 'Thời gian', 'Tỷ trọng', 'Giá trị đợt 1', 'Trạng thái'], [8, 70, 13, 10, 17, 15])
first = ws.max_row + 1
for c, v in zip(CV12, tien_12()):
    row(ws, (c[0], c[1], c[3], f'{c[2]}%', v, 'Hoàn tất'), fill=GRP, bold=True, center=(1, 3, 4, 6), num=(5,))
    row(ws, ('', '   ' + c[4], '', '', '', ''))
lastr = ws.max_row
status(ws, 'F', first, lastr)
row(ws, ('', 'Cộng đợt 1', '', '80%', f'=SUM(E{first}:E{lastr})', ''), bold=True, center=(4,), num=(5,), fill=GRP)
ws.append([])
row(ws, ('', 'Chuyển đợt 2 (20% mục 1.2 = 111.755.080 đồng)', '', '', '', ''), bold=True, fill=D2F)
for t, pct in CV12_DOT2:
    row(ws, ('', '   ' + t, 'Từ tháng 11', f'{pct}%', '', 'Chưa hoàn tất'), center=(3, 4, 6))
note(ws, 'Tỷ trọng từng nhóm công việc là đề xuất để các bên xác nhận.')

# ---------- Sheet đối chiếu 14 bước quy trình với kịch bản kiểm thử
ws = wb.create_sheet('Đối chiếu 14 bước')
head(ws, 'ĐỐI CHIẾU QUY TRÌNH GIAO DỊCH 14 BƯỚC VỚI KỊCH BẢN KIỂM THỬ',
     'Kịch bản kiểm thử 10 bước theo phần V Chương V Hồ sơ mời thầu: đăng ký nhà cung cấp → duyệt → đăng sản phẩm → đăng nhu cầu → mời báo giá, nộp đề xuất → trao đổi trực tuyến → lập hợp đồng điện tử → ký số → thông báo email/SMS → nhật ký.',
     ['Bước', 'Bước quy trình', 'Kiểm thử tương ứng', 'Kết quả', 'Minh chứng'], [8, 42, 40, 12, 46])
W14 = [
    ('1', 'Tạo yêu cầu chuyển giao', 'Kiểm thử bước 4 (đăng nhu cầu, gửi yêu cầu)'),
    ('2', 'Ký thỏa thuận bảo mật (khi sản phẩm yêu cầu)', 'Kiểm thử bổ sung'),
    ('3', 'Tạo yêu cầu báo giá, mời nhà cung cấp', 'Kiểm thử bước 5'),
    ('4', 'Nộp và chấm điểm hồ sơ đề xuất', 'Kiểm thử bước 5'),
    ('5', 'Đàm phán thương mại', 'Kiểm thử bước 6'),
    ('6', 'Kiểm tra pháp lý', 'Kiểm thử bổ sung'),
    ('7', 'Lập và ký hợp đồng điện tử', 'Kiểm thử bước 7, 8'),
    ('8', 'Xác nhận tạm ứng', 'Kiểm thử bổ sung'),
    ('9', 'Thử nghiệm', 'Kiểm thử bổ sung'),
    ('10', 'Bàn giao và triển khai', 'Kiểm thử bổ sung'),
    ('11', 'Đào tạo và chuyển giao vận hành', 'Kiểm thử bổ sung'),
    ('12', 'Bàn giao hồ sơ kỹ thuật', 'Kiểm thử bổ sung'),
    ('13', 'Nghiệm thu', 'Kiểm thử bổ sung'),
    ('14', 'Thanh lý hợp đồng', 'Kiểm thử bổ sung'),
]
first = ws.max_row + 1
for b, nd, kt in W14:
    row(ws, (b, nd, kt, 'Đạt', 'Nhật ký hồ sơ giao dịch (CMS > Hồ sơ giao dịch > Nhật ký): thời điểm hoàn thành bước, người thực hiện'), center=(1, 4))
note(ws, 'Kịch bản 10 bước kiểm tra các bước quy trình 1, 3, 4, 5, 7 và các bước chuẩn bị (đăng ký, duyệt nhà cung cấp, đăng sản phẩm). '
         'Các bước 2, 6, 8 – 14 kiểm thử bổ sung trên cùng hồ sơ giao dịch. Thông báo email, SMS và nhật ký (kiểm thử bước 9, 10) được kiểm tra ở tất cả 14 bước.')

wb.properties.creator = 'NghiaVT'; wb.properties.lastModifiedBy = 'NghiaVT'
wb.save(out); print('ok', out)
