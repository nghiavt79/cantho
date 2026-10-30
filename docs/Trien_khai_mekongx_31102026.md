# Cập nhật mekongx.vn – 31/10/2026

Hai nội dung: (1) chỉnh giao diện trên điện thoại theo góp ý của khách; (2) PDF hợp đồng có khung chữ ký điện tử cho luồng ký OTP.
Trạng thái: **đã commit, chưa đóng gói, chưa chạy trên máy chủ**. Code: nhánh `feature/bo-sung-chuc-nang-hsmt`, commit `363e5e6` (giao diện) và `2a33c72` (ký OTP).

## 1. Giao diện điện thoại (commit `363e5e6`)

Khách phản ánh trên điện thoại chữ bị rớt dòng lẻ, không đẹp.

| Vị trí | Hiện tượng | Cách sửa |
|---|---|---|
| Banner trang chủ – dòng mô tả | Nội dung banner (bảng `ImagesAdver`) có `<br>` cứng sau "công nghệ," → trên màn hẹp chữ "nghệ," rơi thành 1 dòng riêng | Trên màn ≤ 767px ẩn `<br>`, chữ chảy liền; máy tính giữ 2 dòng như cũ. Không sửa dữ liệu |
| Chân trang – số điện thoại | "+(84)" một dòng, "292 3907 037" một dòng: iPhone tự biến số thành liên kết, dính CSS `a { display:block }` | Số điện thoại thành liên kết `tel:+842923907037`, luôn nằm trên 1 dòng; bấm vào gọi được |
| Tiêu đề, dòng ngắn toàn site (vd. "…TP. CẦN THƠ" → "THƠ", "Bạn cần hỗ trợ chuyển giao công nghệ?" → "nghệ?") | Chữ cuối rơi lẻ xuống dòng dưới | Trên điện thoại: tiêu đề chia đều chữ giữa các dòng (`text-wrap: balance`), đoạn văn tránh để 1 chữ đứng riêng (`text-wrap: pretty`). Riêng tiêu đề banner giữ cách xuống dòng cũ. Cần iOS 17.5+; máy cũ hiển thị như trước |
| Trang chủ – "Doanh nghiệp/Đơn vị tiêu biểu", "Đối tác đồng hành" | Mỗi logo một dòng (31 + 8 ô xếp dọc), phải cuộn rất dài | Mỗi mục còn 1 hàng **vuốt ngang**, thấy khoảng 2,5 ô, dừng ngay ngắn từng ô. Máy tính giữ dạng lưới |

Đã kiểm tra ở khổ 375px trên máy local: trang chủ và 14 trang cấp 2 (Giới thiệu, Quy định chung, Sản phẩm, danh mục sản phẩm, Dịch vụ tư vấn và trang chi tiết, Tin sự kiện, Tìm mua công nghệ, OCOP, Liên hệ, Hỏi đáp, Chuyên gia, Công nghệ, Tìm kiếm) — không trang nào tràn ngang, không còn số điện thoại bị tách dòng.

File: `Views/Shared/Components/HeroBanner/Default.cshtml`, `Views/Shared/_Footer.cshtml`, `wwwroot/css/home-redesign.css`.

## 2. PDF hợp đồng ký OTP (commit `2a33c72`)

- Khi Bên mua/Bên bán ký bằng mã OTP qua email, hệ thống tự tạo PDF hợp đồng từ nội dung hợp đồng (bỏ dòng "bản nháp") và chèn khung **"Chữ ký điện tử (OTP)"** cho từng bên đã ký: họ tên, "Xác thực: Mã OTP gửi qua email", mã ký kèm IP, giờ ký thực tế.
- Tạo PDF ngay sau khi ký, khi tải file đã ký, và tạo bù khi mở Bước 7 nếu hợp đồng còn thiếu.
- Hợp đồng đã có chữ ký USB/chứng thư số: chỉ chồng thêm chữ ký OTP mới nhất lên file đã ký.
- Khung chữ ký gọn hơn, đặt giữa, ngay dưới dòng "Đại diện Bên A/B".
- Lỗi khi tạo PDF chỉ ghi log, không làm hỏng thao tác ký.

File: `Controllers/SigningController.cs`, `Interfaces/IContractSigningService.cs`, `Services/ContractSigningService.cs`, `Services/PdfSigningService.cs`, `Views/Project/Steps/_Step7.cshtml`.

## 3. Gói triển khai (khi đóng gói)

- `app\TechExchangeApp.dll` – view Razor đã biên dịch vào dll.
- `app\wwwroot\css\home-redesign.css`.
- **Không có SQL**: dùng các cột sẵn có (`SignedFilePath`, `SignedFileName`, `Sha256Signed`).
- Thư mục `wwwroot\uploads\contracts` trên máy chủ cần quyền ghi cho app pool (để lưu PDF ký OTP).

## 4. Lưu ý

- Kiểm tra máy chủ đã chạy gói 27/10/2026 (`deploy-mekongx-2026-10-27.zip`) chưa; nếu chưa thì chạy gói đó trước.
- Sau khi cập nhật: mở trang chủ bằng điện thoại thật (iPhone) xem banner, chân trang, 2 mục vuốt ngang; trình duyệt có thể giữ CSS cũ — tải lại trang (link CSS có `asp-append-version` nên thường tự nhận bản mới).
