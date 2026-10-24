# Triển khai mekongx.vn – 25/10/2026

Cập nhật máy chủ từ bản 22/10/2026 (commit `2391743`) lên bản 25/10/2026, **gộp luôn gói 24/10/2026 (SMS)** vì gói đó chưa chạy.
Trạng thái: **đã đóng gói, chưa chạy trên máy chủ**. Code: commit `9032f2b` nhánh `feature/bo-sung-chuc-nang-hsmt`.

## 1. Nội dung

- SMS Brandname South Telecom (commit `ca732ec`) – xem `Trien_khai_mekongx_24102026.md`.
- Nhật ký đăng nhập, log hệ thống ra file `logs/app-yyyyMMdd.log`, thống kê lượt truy cập (bảng `SiteVisitDaily`), thống kê phản hồi hỗ trợ ≤ 02 giờ (commit `e3050de`).
- Script 10 tài khoản vận hành thử, mật khẩu `Hello@2026` (commit `9032f2b`) – tùy chọn.

## 2. Gói triển khai

- `deploy-mekongx-2026-10-25.zip` (thư mục gốc repo, không đưa vào git):
  - `app\TechExchangeApp.dll` – chỉ 1 file. Publish Release ra `D:\2026\CanThoPublish_20261025`, so với bản 22/10: chỉ `TechExchangeApp.dll/.exe/.pdb/.xml` và `appsettings.json` khác; IIS chỉ cần `.dll`, `appsettings.json` không đưa (cấu hình `FileLogging` có giá trị mặc định trong code).
  - `sql\deploy-server-2026-10-25.sql` – bắt buộc: tham số SMS (nội dung gói 24/10) + bảng `SiteVisitDaily`.
  - `sql\tuy-chon-create-tai-khoan-van-hanh-thu.sql` – tùy chọn; sửa email thật cho tài khoản CMS trước khi chạy.
  - `HUONG_DAN.txt` – các bước cập nhật, tạo thư mục `logs` và cấp quyền cho app pool, cấu hình SMS.
- Gói 24/10 (`deploy-mekongx-2026-10-24.zip`) không cần chạy nữa.
- Chạy thử bản publish trên máy (cổng 5299): trang chủ 200, `/health` Healthy, `/api/sms/dlr` 403, `/Secure/TestSms` chuyển trang đăng nhập, tạo được file log.

## 3. Cơ sở dữ liệu

`TechExchangeApp/Scripts/deploy-server-2026-10-25.sql`, chạy lại nhiều lần an toàn (đã thử trên DB local):

```
sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -i deploy-server-2026-10-25.sql
```

## 4. Lưu ý trên máy chủ

- Tạo `D:\www\sgdcn\logs` và cấp quyền ghi cho `IIS AppPool\<tên app pool>`; nếu thiếu quyền, ứng dụng vẫn chạy nhưng không có log file.
- Nếu máy chủ chưa chạy gói 22/10 thì chạy gói 22/10 trước.
