# Cập nhật mekongx.vn 13/11/2026: ô nhập OTP hiện sẵn, ghi lỗi gửi SMS OTP

Gói `deploy-mekongx-2026-11-13.zip` (tại `D:\2026\CanTho`). Chưa chép lên máy chủ.

## Lý do

Khách hỏi ở mục Xác thực tài khoản > Số điện thoại "có nhập OTP chưa": bấm gửi mã thì nút treo ở "Đang gửi...", không thấy ô nhập mã. Ô nhập mã có sẵn nhưng chỉ hiện khi gửi SMS thành công; gửi SMS lỗi thì máy chủ trả lỗi 500, trang không xử lý nên treo. Lỗi gửi SMS OTP không ghi vào CSDL nên không tra được nguyên nhân.

## Thay đổi

- `Views/Account/Profile.cshtml`: ô "Nhập mã 6 số" + nút "Xác nhận" (SĐT và Email) hiện ngay khi mở form xác thực. Gửi OTP SĐT lỗi thì báo "Không thể gửi OTP..." và nút gửi trở lại bình thường.
- `Services/VerificationService.cs`: gửi SMS OTP bọc try/catch, ghi kết quả vào dòng `UserOtps` vừa tạo, lỗi thì trả thất bại thay vì để lỗi 500.
- Bảng `UserOtps` thêm `SendStatus` (1 = thành công, 2 = lỗi), `SendError`, `ProviderResponse`. Script `TechExchangeApp/Scripts/add-userotps-send-result.sql`, chạy lại được.

## Gói

- Gồm `app/TechExchangeApp.dll`, `sql/add-userotps-send-result.sql`, `HUONG_DAN.txt`. Không đổi `appsettings.json`, `wwwroot`.
- Phải chạy script **trước** khi chép dll; chép dll trước thì gửi/xác nhận OTP lỗi (thiếu cột).
- Dll đã gồm thay đổi 11/11 và 12/11; nếu máy chủ chưa chép gói 12/11 thì chạy thêm 2 script của gói đó.
- Thư mục publish: `D:\2026\CanThoPublish_20261113`. So với bản 12/11 chỉ khác `TechExchangeApp.dll` (`.exe`, `.xml` khác do build lại, không cần chép).

## Sau khi lên máy chủ

Cho khách bấm gửi OTP lại một lần, xem `UserOtps` (`OtpType = 2`, `SendStatus`, `SendError`) để biết nguyên nhân South Telecom từ chối (IP chưa cho phép, sai khóa, hết số dư, mẫu tin chưa đăng ký...).
