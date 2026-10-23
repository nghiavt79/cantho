# Triển khai mekongx.vn – 24/10/2026

Cập nhật máy chủ từ bản 22/10/2026 (commit `2391743`) lên bản 24/10/2026: kết nối SMS Brandname South Telecom (CSKH trả trước).
Trạng thái: **đã đóng gói, chưa chạy trên máy chủ**. Code SMS **chưa commit** tại thời điểm đóng gói.

## 1. Code đưa lên (nhánh `feature/bo-sung-chuc-nang-hsmt`)

- `Services/SouthTelecomSmsSender.cs` – gửi qua API `https://api-04.worldsms.vn/apidebit/sendSMS`, chuẩn hóa số về `84…`, tự chọn tin có dấu/không dấu, lỗi cố định không gửi lại.
- `Services/SmsSenderRouter.cs` – chọn nhà cung cấp theo `SMS_PROVIDER` (`SOUTHTELECOM` / `STUB`).
- `Controllers/Api/SmsDlrController.cs` – `GET /api/sms/dlr` nhận báo cáo giao tin, chỉ nhận từ IP trong `SMS_DLR_ALLOWED_IPS`.
- `NotificationProcessor` – gửi kèm smsid `N{Id}`, lưu phản hồi nhà cung cấp.
- `/Secure/TestSms` – hiện phản hồi nhà cung cấp; CMS cho nhập giá trị tham số tối đa 1000 ký tự (trước 50).
- Hướng dẫn CMS – mục "Tin nhắn SMS".
- Tài liệu nhà cung cấp: `docs/sms/Southtelecom - Hướng dẫn kết nối API SMS CSKH Trả trước.pdf`.

## 2. Gói triển khai

- `deploy-mekongx-2026-10-24.zip` (thư mục gốc repo, không đưa vào git):
  - `app\` – publish Release ra thư mục sạch `D:\2026\CanThoPublish_20261024` (97 file, danh sách giống gói 22/10).
  - `sql\deploy-server-2026-10-24.sql`
  - `HUONG_DAN.txt` – các bước cập nhật và cấu hình SMS.
- Không gồm `appsettings*.json`, `wwwroot\`.
- Chạy thử bản publish trên máy (cổng 5299): trang chủ 200, `/api/sms/dlr` 403 (chặn IP lạ đúng), `/Secure/TestSms` chuyển trang đăng nhập.

## 3. Cơ sở dữ liệu

`TechExchangeApp/Scripts/deploy-server-2026-10-24.sql` (bản sao `add-sms-southtelecom-params.sql`), chạy lại nhiều lần an toàn:

```
sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -i deploy-server-2026-10-24.sql
```

- Thêm tham số nhóm SMS: `SMS_API_URL`, `SMS_API_KEY` (rỗng), `SMS_BRANDNAME` (rỗng), `SMS_DLR_ENABLED` = 1, `SMS_DLR_ALLOWED_IPS` (IP South Telecom theo tài liệu).
- `SMS_PROVIDER`: đổi `TWILIO` → `STUB`, cập nhật mô tả.
- Không thêm cột/bảng.

## 4. Thứ tự trên máy chủ

1. Sao lưu cơ sở dữ liệu.
2. Chạy script SQL ở mục 3.
3. Dừng app pool (hoặc đặt `app_offline.htm` vào `D:\www\sgdcn\`).
4. Chép đè toàn bộ `app\` vào `D:\www\sgdcn\` (không xóa `App_Data`).
5. Khởi động lại app pool; kiểm tra trang chủ, đăng nhập, CMS > Tham số hệ thống (nhóm SMS), `https://mekongx.vn/api/sms/dlr` trả 403.

Sau bước này hệ thống vẫn chỉ ghi nhật ký, chưa gửi tin thật.

## 5. Khi có tài khoản South Telecom

1. Nhập `SMS_API_KEY` (không kèm chữ Basic), `SMS_BRANDNAME`, đổi `SMS_PROVIDER = SOUTHTELECOM`; đợi ~1 phút (cache tham số 60 giây).
2. Đăng nhập, mở `/Secure/TestSms?to=09xxxxxxxx`.
3. Bật `NOTIFICATION_ENABLE_SMS = 1`.
4. Gửi South Telecom: IP máy chủ (whitelist) và địa chỉ DLR `https://mekongx.vn/api/sms/dlr`.

## 6. Còn chờ

- Khách cung cấp key, brandname; South Telecom whitelist IP máy chủ.
- Đăng ký template với nhà mạng (nếu brandname bị áp template); nội dung các mẫu tin OTP/nhắc sự kiện chưa sửa (vẫn có dấu, tiền tố `[TechPort]`).
- Chưa thử gửi tin thật (đã thử gọi API bằng khóa giả: nhận đúng lỗi 40) và chưa thử DLR thật từ South Telecom.
