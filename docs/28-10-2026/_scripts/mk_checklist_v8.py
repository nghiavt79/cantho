"""Checklist V8 (28/10/2026): cập nhật hồ sơ nhà thầu theo hướng dẫn chị Vân 27/10; nội dung thay đổi tô chữ đỏ."""
import copy
import os
import openpyxl
from openpyxl.styles import Font

ROOT = r'D:\2026\CanTho\docs\28-10-2026'
SRC = os.path.join(ROOT, '27.10.2026 - Checklist ho so theo trinh tu thoi gian - V7.xlsx')
DST = os.path.join(ROOT, '28.10.2026 - Checklist ho so theo trinh tu thoi gian - V8.xlsx')
HS = 'Ho_so_NCC'

# mã -> {cột: nội dung}; D tên, E tiến độ MobiFone, I tình trạng, K số/ký hiệu, ghi chú
SUA = {
    'NCC-02': {
        'D': 'Văn bản đề xuất chỉ số, ngưỡng chất lượng dịch vụ; cung cấp tài khoản quyền xem giám sát và tài liệu dịch vụ; '
             'kèm 11 tài liệu: danh mục chức năng, log – nhật ký, quy trình vận hành (NCC-04), báo cáo tự kiểm tra, biên bản '
             'tự kiểm thử, hướng dẫn sử dụng, mô tả dịch vụ, đặc tả API, đặc tả cơ sở dữ liệu',
        'E': 'Lập lại 28/10 từ văn bản đã gửi chị Quyên (NCC-06 cũ), giữ nội dung chính; bổ sung bảng chỉ số đề xuất, '
             'đơn vị vận hành thử, danh mục 11 tài liệu; chờ ký phát hành',
        'K': f'Văn bản ngày 26/10/2026; {HS}/NCC-02 ... (thư mục và tệp zip)',
    },
    'NCC-03': {
        'E': 'Đã dự thảo 28/10 (ghi ngày 26/10/2026): tình trạng sẵn sàng, 10 tài khoản thử, dữ liệu mẫu, đầu mối hỗ trợ; '
             'MobiFone điền số liệu dữ liệu mẫu, điện thoại đầu mối kỹ thuật',
        'K': f'{HS}/NCC-03 Thong bao san sang van hanh thu lan 1...',
    },
    'NCC-04': {
        'E': 'Không lập văn bản riêng: gộp vào NCC-02 (Tài liệu số 03 – Quy trình phiên bản 2.0, có Sổ theo dõi phiên bản)',
        'I': 'Không phát hành riêng (gộp NCC-02)',
        'K': 'Gộp vào NCC-02 (Tài liệu số 03); người phê duyệt Tài liệu số 03 chưa ghi',
        'L': None,
    },
    'NCC-05': {
        'E': 'Đã dự thảo 28/10 từ biên bản khắc phục lỗi; đã điền kết quả L01, L02 (05/05 API), L03, L07; còn L04, L05, L06, L08 – L10; '
             '4 bên ký, Chủ trì thuê xác nhận',
        'I': 'Đang dự thảo',
        'K': f'{HS}/NCC-05 Bien ban khac phuc loi...',
    },
    'NCC-06': {
        'D': 'Nhật ký hệ thống, số liệu giám sát lần 1; nhật ký cập nhật phiên bản; kết quả khắc phục tồn tại 1 – 5 '
             '(gộp NCC-06a)',
        'E': 'Đã dự thảo 28/10, gộp NCC-06a (mục 4 và Phụ lục xác nhận 4 bên); MobiFone điền kỳ vận hành thử, tỷ lệ sẵn sàng, '
             'CPU, RAM, sao lưu, chứng từ bản quyền, minh chứng SSL tự động, '
             'luồng ghi 10 API, gửi thử thư điện tử',
        'J': None,
        'K': f'{HS}/NCC-06 ... kèm thư mục NCC-06 Minh chung',
    },
    'NCC-06a': {
        'E': 'Gộp vào NCC-06 (mục 4, Phụ lục xác nhận kết quả khắc phục)',
        'I': 'Không phát hành riêng (gộp NCC-06)',
        'K': 'Gộp vào NCC-06: Phụ lục xác nhận kết quả khắc phục, 4 bên ký, chủ đầu tư xác nhận; chưa ký',
    },
    'NCC-08': {
        'I': 'Đang dự thảo',
        'K': f'Dự thảo cập nhật 28/10: Phụ lục 4 ghi mã NCC-02, NCC-03, VHT-04, NCC-05, NCC-06; {HS}/NCC-08 ...',
    },
}
VHT04_LAN1 = {'K': 'NCC đã dự thảo 28/10 (10 lỗi L01 – L10); 4 bên ký, Chủ trì thuê xác nhận; '
                   f'{HS}/VHT-04 Bien ban ghi nhan loi...'}
GHI_CHU = ('- Cập nhật V8 (28/10/2026): theo hướng dẫn của MobiFone ngày 27/10/2026 – NCC-02 lập lại từ văn bản đã gửi '
           '(NCC-06 cũ), kèm 11 tài liệu; NCC-04 gộp vào NCC-02; NCC-06a gộp vào NCC-06; dự thảo NCC-03, VHT-04, NCC-05, '
           'NCC-06; tài liệu kèm theo đổi mã cũ NCC-06, 11, 12, 14 thành NCC-02, Tài liệu số 01, 02, NCC-04; '
           'nội dung thay đổi tô chữ đỏ. Sửa theo rà soát 28/10: L02 đủ 05/05 API (API-18 đo 28/10); tách đo tải trang trên trình duyệt với '
           'đo HTTP; NCC-06 thêm kỳ số liệu và Phụ lục xác nhận 4 bên; bỏ mô tả snapshot (chỉ có sao lưu); ghi chú hạn khắc phục '
           'VHT-04; NCC-08 chưa khẳng định hoàn thành khi chưa có xác nhận.')


def do(cell):
    f = copy.copy(cell.font)
    cell.font = Font(name=f.name, size=f.size, bold=f.bold, italic=f.italic, underline=f.underline, color='FFFF0000')


def main():
    wb = openpyxl.load_workbook(SRC)
    ws = wb['Checklist ho so']
    seen_vht04 = False
    last_note = None
    for r in range(1, ws.max_row + 1):
        ma = (ws[f'B{r}'].value or '').strip() if isinstance(ws[f'B{r}'].value, str) else ''
        a = ws[f'A{r}'].value
        if isinstance(a, str) and a.startswith('- Cập nhật V'):
            last_note = r
        sua = None
        if ma in SUA and ma != 'NCC-05':  # NCC-05 có 2 dòng (lần 1, lần 2): chỉ sửa lần 1, xét bên dưới
            sua = SUA[ma]
        if ma == 'VHT-04' and not seen_vht04:
            sua = VHT04_LAN1
            seen_vht04 = True
        if ma == 'NCC-05' and ws[f'D{r}'].value and '(lần 1)' in ws[f'D{r}'].value:
            sua = SUA['NCC-05']
        if not sua:
            continue
        for col, v in sua.items():
            c = ws[f'{col}{r}']
            c.value = v
            if v is not None:
                do(c)
        print(r, ma, list(sua))
    n = last_note + 1
    ws.insert_rows(n)
    ws[f'A{n}'].value = GHI_CHU
    ws[f'A{n}']._style = copy.copy(ws[f'A{last_note}']._style)
    do(ws[f'A{n}'])
    for rng in list(ws.merged_cells.ranges):
        if rng.min_row == last_note and rng.max_row == last_note:
            ws.merge_cells(start_row=n, start_column=rng.min_col, end_row=n, end_column=rng.max_col)
    ws.row_dimensions[n].height = ws.row_dimensions[last_note].height
    wb.properties.creator = 'MobiFone'
    wb.properties.lastModifiedBy = 'MobiFone'
    wb.save(DST)
    print('saved', DST)


if __name__ == '__main__':
    main()
