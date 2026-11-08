# Cập nhật mekongx.vn – 08/11/2026

Nội dung: nhúng chữ ký số vào PDF khi ký USB Token (Bước 7) và hiển thị số bước theo cấu hình.

## Nội dung sửa

| Mục | Trước | Sau |
|---|---|---|
| Ký USB Token qua LocalSigner | PDF tải về chỉ có khung vẽ; chữ ký lưu thành chuỗi trong thông tin tài liệu (`SignatureHex`), Adobe không nhận là chữ ký | PDF có chữ ký số chuẩn `adbe.pkcs7.detached`. Hai bên ký nối tiếp, chữ ký trước vẫn hợp lệ |
| Chữ ký không khớp hợp đồng hoặc chứng thư | Vẫn báo ký thành công | Bị từ chối, yêu cầu ký lại |
| Trang Hồ sơ giao dịch của tôi | Luôn "Bước x/6" | Theo `Workflow:VisibleStepCount` (hiện là 14) |
| Trang Tổng quan | Luôn "x/6 bước" | Theo `Workflow:VisibleStepCount`; tên bước 7–14 hiển thị đúng |

Cách ký USB Token mới:

1. Sàn tạo ô chữ ký trong PDF, chừa chỗ trống, gửi mã băm phần cần ký cho LocalSigner.
2. LocalSigner ký mã băm bằng token trên máy người ký (không đổi).
3. Sàn nhúng chữ ký vào chỗ trống và kiểm tra lại; sai thì từ chối.

## File thay đổi

- `TechExchangeApp/Services/PdfSigningService.cs`: thêm chuẩn bị và hoàn tất chữ ký nhúng.
- `TechExchangeApp/Controllers/SigningController.cs`: `ContractHash` nhận thông tin chứng thư, `USBTokenSign` nhúng chữ ký khi đã chuẩn bị.
- `TechExchangeApp/Views/Project/Steps/_Step7.cshtml`: gửi thông tin chứng thư khi lấy mã băm.
- `TechExchangeApp/Services/ProjectService.cs`, `TechExchangeApp/Services/DashboardService.cs`, `TechExchangeApp/Views/Dashboard/Index.cshtml`: số bước theo cấu hình.

Không có script SQL, không thêm thư viện.

## Đã kiểm tra trên local

- Bên mua, bên bán lần lượt ký bằng khóa thử: PDF có 2 chữ ký, kiểm tra bằng OpenSSL đều hợp lệ; chữ ký thứ nhất vẫn đúng sau khi bên kia ký.
- Gửi chữ ký sai: bị từ chối.
- Trang Tổng quan, Hồ sơ giao dịch của tôi: hồ sơ hoàn thành hiện 14/14.

## Chưa kiểm tra

- Ký bằng USB Token thật và mở bằng Adobe Reader.
- Cấu hình `VisibleStepCount` 7 hoặc 10.

## Lưu ý

- Chỉ áp dụng cho ký USB Token. Ký OTP vẫn là khung vẽ như cũ; nếu một bên ký OTP sau khi bên kia đã ký USB Token thì chữ ký USB Token bị mất hiệu lực.
- Hợp đồng đã ký trước bản này không được nhúng lại chữ ký.
- Thời điểm ký lấy theo giờ ghi trong PDF, chưa có dấu thời gian của bên thứ ba.
- Thư mục `wwwroot\uploads\contracts` phải cho tài khoản app pool ghi được (ghi file chuẩn bị `pending_*.pdf` và PDF đã ký).

## Gói

- `app\TechExchangeApp.dll`: build từ nhánh chính đã gộp bản sửa này, không đổi thư viện.
- `HUONG_DAN.txt`.
