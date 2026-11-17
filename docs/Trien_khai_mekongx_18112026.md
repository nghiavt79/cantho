# Cập nhật mekongx.vn 18/11/2026: nội dung SMS OTP theo mẫu South Telecom

Gói `deploy-mekongx-2026-11-18.zip` (tại `D:\2026\CanTho`). Đã chép lên máy chủ 18/11/2026, gửi OTP thử thành công.

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

## Kết quả trên máy chủ (18/11/2026)

Gửi OTP xác thực SĐT từ trang Tài khoản, `UserOtps` ghi nhận South Telecom nhận tin:

| Id | Giờ VN | SendStatus | Phản hồi South Telecom |
|---|---|---|---|
| 69 | 18/11 02:17:56 | 1 | `{"status":1,"errorcode":0,"carrier":"viettel","mnp":0}` |
| 70 | 18/11 02:19:19 | 1 | `{"status":1,"errorcode":0,"carrier":"mobifone","mnp":0}` |

- Gửi được cả mạng Viettel và MobiFone; lỗi 51 (IP chưa cho phép) đã hết sau khi South Telecom mở IP.
- `SendStatus = 1` là South Telecom đã nhận tin; xác thực xong cả quy trình khi `Users.PhoneVerified = 1`.
- `UserOtps.CreatedAt`, `ExpiresAt` lưu giờ UTC (chậm 7 giờ so với giờ VN), kiểm tra hạn mã cũng so UTC nên đúng. Xem giờ VN: `DATEADD(HOUR, 7, CreatedAt)`.

Câu tra cứu:

```sql
SELECT TOP 5 o.Id, DATEADD(HOUR, 7, o.CreatedAt) AS GioVN, o.SendStatus, o.SendError, o.ProviderResponse
FROM UserOtps o WHERE o.OtpType = 2 ORDER BY o.Id DESC;
```
