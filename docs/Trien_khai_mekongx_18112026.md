# Cập nhật mekongx.vn 18/11/2026: nội dung SMS OTP theo mẫu South Telecom

Gói `deploy-mekongx-2026-11-18.zip` (tại `D:\2026\CanTho`). Chưa chép lên máy chủ.

## Lý do

Sau gói 13/11, gửi OTP SĐT trên máy chủ báo lỗi; `UserOtps.SendError` ghi "South Telecom lỗi 51: IP máy chủ chưa được South Telecom cho phép" (5 lần thử 17/11). Cấu hình SMS trên máy chủ đúng (`SMS_PROVIDER = SOUTHTELECOM`, `SMS_BRANDNAME = MEKONGX.VN`). South Telecom đã mở IP và đăng ký mẫu tin OTP:

`[MEKONGX.VN] Mã OTP của bạn trên web mekongx.vn là 123456` (123456 = mã ngẫu nhiên).

Nội dung cũ (`[Sàn GDCN Cần Thơ] Mã OTP ...`) không khớp mẫu nên sẽ bị nhà mạng chặn.

## Thay đổi

Đổi nội dung SMS về đúng mẫu ở cả 3 chỗ gửi OTP qua SMS:

- Xác thực SĐT (Tài khoản): `Services/VerificationService.cs`
- Ký NDA: `Controllers/ESignController.cs`
- Ký biên bản Bước 5: `Controllers/NegotiationController.cs`

Đã rà toàn bộ mã nguồn: các OTP khác (MFA CMS, ký hợp đồng, nghiệm thu, quên mật khẩu) gửi qua email; `ESignGateway` chưa gửi SMS (TODO). SMS không phải OTP (nhắc lịch sự kiện, trang kiểm tra kết nối) không đổi, chưa có mẫu đăng ký.

## Gói

- Gồm `app/TechExchangeApp.dll`, `HUONG_DAN.txt`. Không có script SQL, không đổi `appsettings.json`, `wwwroot`, tham số SMS.
- Máy chủ phải đang là bản 13/11 (đã có cột `UserOtps.SendStatus/SendError/ProviderResponse`).
- Thư mục publish: `D:\2026\CanThoPublish_20261118`. So với bản 13/11 chỉ khác `TechExchangeApp.dll`.
