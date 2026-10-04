# Kịch bản demo chức năng theo Chương V – Gói thầu số 4 Sàn GDCN Cần Thơ

Ngày lập: 04/10/2026 · Thời lượng dự kiến: 60–75 phút · Căn cứ: [Bang_dap_ung_ho_so_ky_thuat.md](Bang_dap_ung_ho_so_ky_thuat.md)

## 1. Chuẩn bị máy demo (làm trước 1 ngày)

| # | Việc | Ghi chú |
|---|---|---|
| 1 | Triển khai mã nguồn nhánh `feature/bo-sung-chuc-nang-hsmt` | |
| 2 | Chạy lần lượt các script trong `TechExchangeApp/Scripts/`: `dot1-…`, `seed-i18n-dot1-…`, `dot2-…`, `seed-i18n-dot2-…`, `dot3-su-kien-…`, `seed-i18n-dot3-…`, `dot4-…`, `seed-i18n-dot4-…`, `dot5-…`, `seed-i18n-dot5-…` | `sqlcmd -f 65001 -I`. Thiếu cờ `-f 65001` thì tiếng Việt bị lỗi font |
| 2b | Chạy `seed-huong-dan-su-dung-hsmt.sql` | Thêm 7 bài hướng dẫn người dùng cho chức năng mới vào trang `/huong-dan` |
| 3 | Chạy `seed-demo-hsmt.sql` | Tạo 8 câu FAQ, 1 sự kiện mẫu (diễn ra sau 7 ngày, 2 loại gian hàng), 3 gói gian hàng, 1 phòng trưng bày 10 vị trí. **Không chạy trên máy chủ thật** |
| 4 | Chuẩn bị 3 tài khoản: **Quản trị**, **Nhà cung cấp** (gắn với một hồ sơ nhà cung ứng có vài sản phẩm), **Người mua** | Mỗi tài khoản có email nhận được thư thật |
| 5 | Tham số hệ thống: `NOTIFICATION_ENABLE_EMAIL = true` (để thư thật đến hộp thư); muốn demo xác thực 2 lớp thì `CMS_MFA_ENABLED = 1` | Đổi tham số có hiệu lực sau khoảng 1 phút |
| 6 | Tại CMS → Phân hạng nhà cung cấp, bấm "Áp dụng hạng gợi ý"; gán tay hạng Vàng cho nhà cung cấp dùng để demo | |
| 7 | Tại CMS → Sản phẩm, đặt 2–3 sản phẩm "Trưng bày tại Sàn" (Cấu hình nhanh) và gán vào vị trí ở Phòng trưng bày | |
| 8 | Mở sẵn 3 trình duyệt (hoặc 3 cửa sổ ẩn danh) cho 3 tài khoản; 1 điện thoại đăng nhập CMS để quét QR điểm danh | |

## 2. Trình tự demo

Cột **Tài khoản**: QT = quản trị, NCC = nhà cung cấp, NM = người mua, K = khách chưa đăng nhập.

### A. Website quản lý

| Mục | Tài khoản | Thao tác | Kết quả cần thấy |
|---|---|---|---|
| A.1.1 | K | Trang đăng nhập → "Quên mật khẩu?" → nhập email → mở thư → đặt mật khẩu mới | Đăng nhập được bằng mật khẩu mới; mở lại link cũ báo "đã được sử dụng" |
| A.1.2, A.1.3 | QT | CMS → Quản trị hệ thống → Người dùng, Vai trò, Phân quyền | Danh sách, tìm kiếm, phân quyền theo chức năng |
| A.5.1 | QT | Người dùng → nút khóa trên tài khoản NM, nhập lý do | NM đang đăng nhập bị đẩy ra; đăng nhập lại báo "tài khoản đã bị tạm khóa". Mở khóa để demo tiếp |
| A.2.2, A.2.4 | QT | Sản phẩm & Đối tác → Phân hạng nhà cung cấp | Bảng tiêu chí; số liệu từng nhà cung cấp; hạng gợi ý; gán hạng. Mở trang nhà cung cấp ngoài website thấy huy hiệu hạng |
| A.2.3 | QT → NCC | Gói gian hàng → cấp "Gói 5 sản phẩm" cho NCC; NCC mở Quản lý sản phẩm | NCC thấy "x / 5 sản phẩm (gói đến …)"; Thu chi có khoản "Phí gian hàng" |
| A.2.5, B.2.3 | NCC | Quản lý sản phẩm | Cột lượt xem, lượt liên hệ từng sản phẩm và tổng |
| A.2.7, A.3.2, A.8.3 | QT | Sửa mô tả hoặc giá một sản phẩm → nút "Lịch sử chỉnh sửa" | Danh sách phiên bản, người sửa; so sánh 2 phiên bản tô màu trường thay đổi |
| A.2.8 | NCC → QT | NCC: Bài viết của tôi → Viết bài mới → Gửi duyệt. QT: Tin bài → "Bài nhà cung cấp chờ duyệt" → xuất bản | Bài hiện ngoài trang Tin tức |
| A.2.10, A.5.3 | NM → QT | NM nhắn tin cho NCC từ trang sản phẩm. QT: Giao dịch → Lịch sử chat, lọc theo người bán / từ khóa → mở hội thoại → gắn cờ | Từ khóa được tô sáng; hội thoại gắn cờ nổi lên đầu danh sách |
| A.3.1, A.3.3, A.3.4 | QT | Phòng trưng bày → xem vị trí trống/đang dùng → gán thêm 1 sản phẩm | Trang chi tiết sản phẩm hiện "Vị trí trưng bày: Phòng trưng bày 1 – A-0x" |
| A.4.1 | QT | Nội dung website → Sự kiện → mở sự kiện mẫu → Thiết lập | Thời gian, địa điểm, link phát trực tiếp, hạn đăng ký, số chỗ, mốc nhắc lịch |
| A.4.3 | QT | Cùng màn hình → Loại gian hàng | Gian 9 m², 18 m², giá, dịch vụ đi kèm |
| A.4.4 | QT | Sự kiện → Thống kê (sau phần B.1.6 bên dưới) | Lượt xem, quan tâm, nhắc lịch, đăng ký, điểm danh theo ngày |
| A.4.5, A.8.4 | QT | Sự kiện → "Gửi nhắc lịch đến hạn" (hoặc đặt giờ sự kiện gần hiện tại để thấy nhắc tự động) | Thư nhắc lịch đến hộp thư người đăng ký; chuông thông báo trên web |
| A.5.2 | QT | Giao dịch → Lịch sử mua hàng (hoặc nút giỏ hàng ở danh sách Người dùng) → chọn NM | Trên một màn hình: yêu cầu đặt mua, hồ sơ giao dịch, phiếu yêu cầu, tin tìm mua, giỏ hàng, sự kiện, gian hàng |
| A.5.4, A.7.1 | NM → QT | NM bình luận ở trang sản phẩm, tin bài và trang nhà cung cấp. QT: Nội dung website → Bình luận → lọc theo người dùng → ẩn / sửa | Cột đối tượng ghi rõ sản phẩm / bài viết / gian hàng; bình luận ẩn không còn trên website |
| A.6.1 | QT | Giao dịch → Thu chi → đặt tỷ lệ hoa hồng → ghi nhận hoa hồng cho một hợp đồng → Xuất Excel | Số tiền = giá trị × tỷ lệ; báo cáo theo nhóm, theo tháng |
| A.7.2 | QT | Nội dung website → Biểu mẫu → tạo biểu mẫu mới 3 trường → mở trang công khai → gửi thử → xem dữ liệu, Xuất Excel | Trường bắt buộc báo lỗi khi bỏ trống; dữ liệu hiện trong CMS |
| A.7.3 | QT → K | Câu hỏi thường gặp → bật/tắt một câu → mở `/hoi-dap` | Câu tắt không hiện ngoài website |
| A.8.2 | QT | Sản phẩm & Đối tác → Cảnh báo sản phẩm → "Quét ngay" | Danh sách trùng tên, trùng mô tả, quá hạn; đánh dấu đã xử lý |

### B. Website bán hàng

| Mục | Tài khoản | Thao tác | Kết quả cần thấy |
|---|---|---|---|
| B.1.4 | K | `/san-pham` → lọc "Trưng bày tại Sàn" | Chỉ còn sản phẩm trưng bày tại Sàn |
| B.1.6 | K | Chân trang → "Đăng ký nhận bản tin" → gửi | Thông báo cảm ơn; email xác nhận |
| B.1.6, A.4.1 | NM | Mở tin sự kiện mẫu → Quan tâm → Nhắc lịch → Đăng ký tham gia | Email xác nhận kèm link vé QR |
| A.4.1 (điểm danh) | QT (điện thoại) | Quét QR trên vé của NM | Mở trang điểm danh CMS → "Xác nhận điểm danh"; quét lần 2 báo đã điểm danh |
| A.4.3 | NCC | Trang sự kiện → Đăng ký thuê gian hàng; QT duyệt ở Sự kiện → Gian hàng | Thu chi có khoản "Phí gian hàng sự kiện" |
| B.3.3 | NM | Trang sản phẩm → "Quan tâm" → Khu tài khoản → Sản phẩm quan tâm | Sản phẩm có trong danh sách; bỏ quan tâm được |
| B.3.5 | NM | Trang sản phẩm → viết bình luận | Bình luận hiện ngay; xóa được bình luận của mình |
| B.2.1, B.2.2 | K | Trang nhà cung cấp | Thông tin, hồ sơ, huy hiệu hạng |

### C. Tiện ích khác

| Mục | Tài khoản | Thao tác | Kết quả cần thấy |
|---|---|---|---|
| C.5 | K | Trang sản phẩm → "In mã QR" | Nhãn in có tên sản phẩm, mã QR, đường dẫn; quét QR mở đúng trang |
| C.6, C.7 | NM | Hội thoại với NCC → "Gọi video" | Mở phòng họp trực tuyến; lời mời hiện trong hội thoại để NCC và cán bộ Sàn cùng vào |
| C.8 | K | Đổi sang EN: sản phẩm, FAQ, biểu mẫu bản tin, sự kiện | Giao diện tiếng Anh; số liệu quan tâm, bình luận, sự kiện dùng chung với bản tiếng Việt |
| C.9 | K | Ô tìm kiếm trang chủ → nút micro → nói "máy sấy" | Ô tự điền từ khóa và tìm (Chrome, Edge) |
| C.2, C.10 | — | Trình bày như các buổi trước | |

### III. Chất lượng, kết nối, an toàn thông tin

| Mục | Thao tác | Kết quả cần thấy |
|---|---|---|
| 2.2 giám sát | Mở `/health` | Trạng thái CSDL, ổ đĩa, hàng đợi thông báo |
| 2.2 sao lưu | Trình bày thư mục sao lưu và biên bản chạy thử phục hồi (`restore-test.ps1`) | Số bản ghi sau phục hồi khớp CSDL đang chạy |
| 2.3, 2.4 | QT mở `/swagger`; khách mở `/swagger` | Quản trị thấy tài liệu API; khách bị chặn |
| 2.4 | Trình bày [DacTa_CSDL_BoSung_HSMT.md](DacTa_CSDL_BoSung_HSMT.md) | |
| 2.11 (cấp độ 2) | QT vào CMS khi bật xác thực 2 lớp; đăng ký tài khoản không tích ô đồng ý | Phải nhập mã gửi qua email; không đăng ký được nếu chưa đồng ý điều khoản |

## 3. Những mục đáp ứng một phần — cách trình bày

| Mục | Trình bày |
|---|---|
| VR360 (I.3, B.1.2, C.1) | Đã có nút tham quan VR360; 10 điểm cảnh quay thực hiện trong giai đoạn triển khai |
| SMS Brandname (I.4) | Hệ thống gửi SMS qua hàng đợi đã sẵn sàng; kích hoạt khi có tài khoản Brandname |
| Chữ ký số (I.6) | Demo ký OTP và ký USB Token; ký từ xa VNPT/FPT/Viettel kích hoạt khi có tài khoản CA |
| IPv6 | Cấu hình khi có hạ tầng máy chủ |

## 4. Sau buổi demo

- Gỡ dữ liệu mẫu bằng khối "GỠ DỮ LIỆU MẪU" cuối file `seed-demo-hsmt.sql` nếu dùng chung máy.
- Trả các tham số đã đổi ở bước 1.5 về giá trị ban đầu.
