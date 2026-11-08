from kb_gen import *

d = new_doc(landscape=False)
header_block(d, None, 'Cần Thơ, ngày 26 tháng 10 năm 2026')
para(d, 'QUY TRÌNH KIỂM THỬ NỘI BỘ', bold=True, align=AL.CENTER, size=14, after=0)
para(d, '(Quản lý chất lượng sản phẩm của nhà cung cấp dịch vụ)', italic=True, align=AL.CENTER, after=0)
para(d, 'Gói thầu số 4: Thuê dịch vụ thiết lập, vận hành trang web Sàn giao dịch công nghệ Cần Thơ', align=AL.CENTER)
para(d, HD)
para(d, 'Quy trình gồm hai phần: Phần A – quy định áp dụng cho mọi lần kiểm thử; Phần B – lịch thực hiện đợt 1, ghi theo ngày thực tế.')

para(d, 'PHẦN A. QUY ĐỊNH ÁP DỤNG CHUNG', bold=True, align=AL.LEFT)
heading(d, 'A.1. Mục đích, phạm vi')
para(d, '- Bảo đảm người khác cầm hồ sơ có thể làm lại từng ca kiểm thử và đối chiếu được kết quả: mỗi ca có điều kiện trước, dữ liệu, thao tác theo thứ tự, kết quả mong đợi, kết quả quan sát được, mã minh chứng, ngày và người thực hiện.')
para(d, '- Áp dụng cho kiểm thử chức năng, quy trình giao dịch 14 bước, API, hạ tầng – bảo mật – hiệu năng trên hệ thống https://mekongx.vn. Kiểm thử nội bộ do nhà cung cấp tự thực hiện, độc lập với đơn vị vận hành thử và đơn vị giám sát; không thay thế kết quả vận hành thử và nghiệm thu.')

heading(d, 'A.2. Vai trò')
table(d, ['Vai trò', 'Họ và tên', 'Trách nhiệm'], [
    ['Kiểm thử (QA/QC)', 'Lê Phước Trung', 'Lập kịch bản, chuẩn bị dữ liệu, thực hiện ca kiểm thử, ghi kết quả và minh chứng, ghi lỗi, kiểm tra lại'],
    ['Trưởng nhóm kỹ thuật', 'Nguyễn Thị Dung', 'Duyệt kịch bản; phân loại mức độ lỗi; xác nhận kết quả từng phần; ký Biên bản'],
    ['Kỹ sư triển khai', 'Nguyễn Nhĩ Thái', 'Khắc phục lỗi phần mềm; cập nhật bản sửa theo quy trình quản lý thay đổi (NCC-14)'],
    ['Kỹ sư hạ tầng', 'Bùi Nguyễn Hoàng Thái', 'Khắc phục lỗi hạ tầng; cung cấp minh chứng phía máy chủ (log, cấu hình)'],
    ['Đại diện nhà cung cấp', 'Lã Thành Trung', 'Ký Báo cáo tự kiểm tra; gửi hồ sơ cho Chủ trì thuê, đơn vị vận hành thử, đơn vị giám sát'],
], [3.6, 3.6, 8.8])
para(d, 'Nguyên tắc: người sửa lỗi không tự xác nhận lỗi đã đóng; người kiểm thử kiểm tra lại và ghi minh chứng kiểm tra lại.')

heading(d, 'A.3. Các bước thực hiện')
table(d, ['Bước', 'Việc phải làm', 'Người làm', 'Đầu vào', 'Đầu ra', 'Người xác nhận'], [
    ['1', 'Xác định phạm vi, tiêu chí, môi trường, tài khoản', 'QA/QC', 'Hợp đồng, Phụ lục 3, Chương V HSMT, PL-03', 'Phần B quy trình (lịch đợt)', 'Trưởng nhóm'],
    ['2', 'Lập kịch bản: mỗi ca có mã, điều kiện trước, dữ liệu, thao tác 1–2–3 (màn hình, nút, dữ liệu nhập), kết quả mong đợi, mã minh chứng dự kiến', 'QA/QC', 'Bước 1; HDSD Mục 01a, 01b; Đặc tả API', 'Kịch bản kiểm thử chi tiết (bản lập)', 'Trưởng nhóm'],
    ['3', 'Chuẩn bị dữ liệu, tài khoản: tạo sản phẩm, hồ sơ kiểm thử; ghi mã thật vào mục II Kịch bản', 'QA/QC; Kỹ sư triển khai', 'Kịch bản mục II', 'Mã sản phẩm, mã hồ sơ, danh sách tài khoản', 'Trưởng nhóm'],
    ['4', 'Thực hiện từng ca; ghi kết quả quan sát được; chụp ảnh có thanh địa chỉ, giờ hệ thống hoặc lưu nhật ký; đặt tên tệp theo mã minh chứng', 'QA/QC', 'Kịch bản; môi trường', 'Cột Kết quả thực tế, Minh chứng, Trạng thái, Ngày – người', 'Trưởng nhóm (theo phần)'],
    ['5', 'Ghi lỗi khi kết quả khác mong đợi: mã L-xx, mô tả, mức độ, ca liên quan, minh chứng', 'QA/QC', 'Ca Không đạt', 'Sổ lỗi (Kịch bản mục VII)', 'Trưởng nhóm phân loại'],
    ['6', 'Khắc phục; cập nhật bản sửa, ghi sổ phiên bản', 'Kỹ sư triển khai / hạ tầng', 'Sổ lỗi', 'Bản cập nhật, ghi chú phiên bản', 'Trưởng nhóm'],
    ['7', 'Kiểm tra lại ca lỗi và ca liên quan; ghi minh chứng kiểm tra lại', 'QA/QC', 'Bản cập nhật', 'Cột Kiểm tra lại của Sổ lỗi', 'Trưởng nhóm'],
    ['8', 'Tổng hợp kết quả tính từ từng ca; lập Biên bản tự kiểm thử, Báo cáo tự kiểm tra', 'QA/QC; Trưởng nhóm', 'Kịch bản đã ghi kết quả', 'Biên bản; Báo cáo', 'Đại diện nhà cung cấp'],
    ['9', 'Gửi hồ sơ; theo dõi, khắc phục lỗi phát sinh trong vận hành thử', 'Trưởng nhóm', 'Biên bản, Báo cáo', 'Báo cáo khắc phục (NCC-15)', 'Đại diện nhà cung cấp'],
], [1.0, 4.6, 2.2, 3.0, 3.0, 2.2], size=10, center_cols=(0,))

heading(d, 'A.4. Mã hóa và liên kết hồ sơ')
para(d, 'Chuỗi liên kết: Mã chức năng (ví dụ A.1.1) → Mã ca kiểm thử (TC-CN-01) → Mã minh chứng (MC-TC-CN-01-20261027-1.png) → Mã lỗi (L-xx, nếu có) → Kết quả kiểm tra lại (ngày, mã minh chứng kiểm tra lại).')
table(d, ['Đối tượng', 'Quy tắc mã', 'Ví dụ'], [
    ['Ca kiểm thử chức năng', 'TC-CN-xx (xx = số thứ tự Phụ lục A Biên bản)', 'TC-CN-01 ↔ A.1.1'],
    ['Ca quy trình 14 bước', 'TC-QT-bb + a (thành công), b (điều kiện chuyển bước), c (không có quyền)', 'TC-QT-02c'],
    ['Ca API', 'KT-xx (trùng Đặc tả API PL-03; KT-31 trở đi là ca ghi thành công, không quyền)', 'KT-33'],
    ['Ca hạ tầng', 'TC-HT-xx', 'TC-HT-05'],
    ['Minh chứng', 'MC-<mã ca>-<yyyymmdd>[-<hồ sơ hoặc tài khoản>][-n].png/.txt, lưu thư mục "Minh chung"', 'MC-TC-QT-07a-NDA-20261027-1.png'],
    ['Lỗi', 'L-xx, ghi tại Sổ lỗi (Kịch bản mục VII), Biên bản mục IV', 'L-03'],
], [3.6, 7.6, 4.8])

heading(d, 'A.5. Ghi kết quả và trạng thái')
para(d, '- Kết quả thực tế ghi điều quan sát được: mã hồ sơ, mã sản phẩm tạo ra; trạng thái chuyển thành gì; thông báo nhận ở đâu; tệp nào sinh ra; mã HTTP. Không ghi "đúng như mong đợi".')
para(d, '- Trạng thái: Đạt (đủ minh chứng, đúng mong đợi); Đạt – phạm vi thử nghiệm (chỉ kiểm thử chế độ thử nghiệm/mô phỏng, ghi rõ phạm vi); Đạt một phần (ghi rõ phần chưa kiểm thử); Không đạt (ghi mã lỗi); Chưa kiểm thử; Không áp dụng (ghi lý do).')
para(d, '- Không ghi Đạt khi chưa có minh chứng. Ca phụ thuộc kênh chưa bật (email, SMS) để Chưa kiểm thử hoặc Không đạt kèm mã lỗi.')
para(d, '- Tổng hợp tính từ cột Trạng thái của từng ca; Biên bản và Báo cáo dẫn số liệu từ bảng tổng hợp của Kịch bản.')
para(d, '- Ngày lập, ngày kiểm thử, ngày kiểm tra lại ghi đúng ngày thực hiện; mỗi ca ghi người thực hiện; Trưởng nhóm ký xác nhận từng phần.')

heading(d, 'A.6. Mức độ lỗi và thời hạn')
table(d, ['Mức độ', 'Tiêu chí', 'Thời hạn khắc phục'], [
    ['Cao', 'Chức năng không dùng được, lỗi hệ thống (HTTP 500), sai dữ liệu, lỗ hổng bảo mật', 'Trước khi bắt đầu vận hành thử hoặc trong ngày'],
    ['Trung bình', 'Dùng được nhưng thiếu kiểm soát an toàn, thiếu một phần, chưa kiểm thử đủ luồng', 'Trước nghiệm thu đợt 1'],
    ['Thấp', 'Hiển thị, thông báo, mã lỗi chưa thống nhất; không ảnh hưởng nghiệp vụ', 'Theo kế hoạch cập nhật'],
], [2.6, 8.4, 5.0])

heading(d, 'A.7. Lưu minh chứng')
para(d, '- Ảnh chụp toàn cửa sổ trình duyệt, thấy thanh địa chỉ và đồng hồ hệ thống; tệp nhật ký ghi thời điểm, yêu cầu, kết quả. Công cụ tự động: chay_kiem_thu_api.ps1 (API), curl/openssl (hạ tầng).')
para(d, '- Lưu tại thư mục "Minh chung" cùng hồ sơ; không chỉnh sửa tệp sau khi lưu; không ghi mật khẩu, mã OTP vào minh chứng.')

para(d, 'PHẦN B. LỊCH THỰC HIỆN ĐỢT 1 (THEO NGÀY THỰC TẾ)', bold=True, align=AL.LEFT)
table(d, ['Ngày', 'Nội dung', 'Người thực hiện', 'Kết quả, hồ sơ'], [
    ['25/10/2026', 'Kiểm thử 22 API ở mức từ chối dữ liệu sai, truy cập trái phép', 'Nhóm kỹ thuật', 'Bảng kiểm thử KT-01…KT-30 trong Đặc tả API PL-03; phát hiện L-01 … L-04'],
    ['26/10/2026', 'Sửa L-01 (API-05), cập nhật lên mekongx.vn', 'Nguyễn Nhĩ Thái', 'Gói cập nhật 26/10/2026'],
    ['26/10/2026', 'Lập Quy trình, Kịch bản kiểm thử chi tiết', 'Lê Phước Trung', 'Hai tài liệu này'],
    ['26/10/2026', 'Chạy ca hạ tầng (TC-HT) và ca API ẩn danh; kiểm tra lại L-01', 'Nhóm kỹ thuật MobiFone Cần Thơ', 'Minh chứng MC-HT-xx-20261026, MC-KT-xx-20261026; phát hiện L-06'],
    ['27 – 28/10/2026 (kế hoạch)', 'Chuẩn bị dữ liệu mục II; chạy TC-CN, TC-QT trên 02 hồ sơ; chạy script API có đăng nhập; kiểm tra lại L-02, L-03, L-05', 'Lê Phước Trung', 'Ghi vào Kịch bản; lập Biên bản sau khi chạy xong'],
], [2.6, 6.2, 2.8, 4.4], size=10)

signatures(d, [('NGƯỜI LẬP', 'Lê Phước Trung'), ('TRƯỞNG NHÓM KỸ THUẬT', 'Nguyễn Thị Dung')])
save(d, DIR + r'\Quy trinh kiem thu noi bo.docx', 'Quy trình kiểm thử nội bộ')
print('ok')
