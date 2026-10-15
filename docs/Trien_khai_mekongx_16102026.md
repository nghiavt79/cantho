# Triển khai mekongx.vn – 16/10/2026

Cập nhật máy chủ từ bản code 04/08/2026 lên bản 16/10/2026 (HSMT Đợt 1–6 + vá bảo mật theo pentest MobiFone 10/2026).
Kết quả: **đã chạy bản mới** – trang chủ, `/en/`, đăng nhập, công nghệ, tin tức, nhà cung ứng, chuyên gia đều trả 200.

## 1. Code đã đưa lên (nhánh `feature/bo-sung-chuc-nang-hsmt`)

| Commit | Nội dung |
|---|---|
| `57fabc5` | Pentest Đợt 1 – vô hiệu phiên khi đăng xuất/đổi mật khẩu (`Users.SessionVersion`), cookie 8 giờ, ẩn trang lỗi chi tiết, `/project` → Dashboard |
| `b9c093c` | Pentest Đợt 2 – chống Stored XSS (`Html.Nl2Br`, `Html.SafeHtml`), kiểm tra nội dung tệp tải lên (`UploadValidationFilter`) |
| `e1ce517` | Pentest Đợt 3 – tạm khóa đăng nhập sai (5 lần/5 phút → 15 phút), chính sách mật khẩu mạnh |
| `dcc857c` | Giới hạn tần suất xác thực 30 yêu cầu/phút/IP (`Security:AuthRateLimitPerMinute`), áp cho mọi OTP email |
| `4fc322e` | Mặc định hiển thị đủ 14 bước quy trình |
| `1c5edec` | `user-portal.css` cho Bài viết của tôi, Sản phẩm quan tâm |

Lỗ hổng TLS/SSL (cấu hình máy chủ) giao MobiFone tự xử lý; hướng dẫn tham khảo nằm trong Phụ lục file Word phản hồi.

## 2. Cơ sở dữ liệu

Chạy `TechExchangeApp/Scripts/deploy-server-2026-10-16.sql` (gộp từ các script gốc, chạy lại nhiều lần an toàn):

```
sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -i deploy-server-2026-10-16.sql
```

- Gồm: `dot1`…`dot5`, `add-users-sessionversion`, `seed-i18n-dot1`…`dot5`, `seed-i18n-pentest-dot3`, `seed-huong-dan-su-dung-hsmt`.
- **Không** gồm `seed-demo-hsmt.sql` (dữ liệu demo) và `seed-huong-dan-su-dung-v2.sql` (máy chủ đã chạy; chạy lại ghi đè bài viết đã sửa).
- Đã thử trên bản sao DB đưa về trạng thái 04/08: chạy OK, chạy lại OK, cấu trúc khớp DB local.
- `sqlcmd` mặc định tắt `QUOTED_IDENTIFIER` → script đã tự `SET QUOTED_IDENTIFIER ON` (thiếu là lỗi Msg 1934).

## 3. Sự cố khi triển khai và cách xử lý

| Hiện tượng | Nguyên nhân | Xử lý |
|---|---|---|
| Toàn site lỗi **500.30** | Thiếu `Swashbuckle.AspNetCore.SwaggerGen.dll` (package mới; ngày trên file là 2024 nên bị sót khi chép theo ngày sửa) | Chép đủ 4 dll Swagger + `Microsoft.OpenApi.dll` |
| Trang chủ lỗi 500 | Thiếu `HtmlAgilityPack.dll` (bộ lọc HTML chống XSS) | Chép toàn bộ dll của bản build |
| Toàn site lại **500.30** | `D:\2026\CanThoPublished` bị lẫn bản publish của **PromoHubApp** → 21 dll dùng chung sai phiên bản (EF Core, SqlClient, System.Text.Json, MailKit…) | Đóng gói lại từ bản publish sạch → `deploy-mekongx-2026-10-16-clean.zip` (97 file), chạy thử local trước khi gửi |

**Bài học:**
- Publish TechExchangeApp ra **thư mục riêng, xóa sạch trước mỗi lần publish**; không dùng chung với project khác.
- Chép **toàn bộ** file ứng dụng (trừ `appsettings.json`, `wwwroot/uploads`), không chọn lọc theo ngày sửa.
- Không chép đè `appsettings.json` của máy chủ (bản publish trỏ DB `localhost`).
- Khi lỗi: bật tạm `stdoutLogEnabled="true"` trong `web.config` (tạo thư mục `logs`, cấp quyền ghi cho `IIS AppPool\<tên pool>`), đọc dòng `fail:`, xong tắt lại.

## 4. Việc còn lại

- Xóa tệp/dữ liệu thử nghiệm MobiFone tạo trong pentest (tệp PDF/DOC chứa mã ở `Uploads/2026/10/09/`, `uploads/proposals/9500_*`, `uploads/negotiations/*Dpayload.doc`; dự án 9500; nhà cung ứng "Don vi tao boi nguoi mua").
- Điền ngày cập nhật vào file Word phản hồi (`error Report/Bao_cao_khac_phuc_lo_hong_mekongx.docx`) rồi gửi MobiFone, đề nghị kiểm thử lại.
- Kiểm tra các chức năng cần đăng nhập (CMS, quy trình 14 bước); người dùng phải đăng nhập lại một lần do phiên cũ hết hiệu lực.
- Xác nhận tham số `CMS_MFA_ENABLED` (xác thực hai lớp trang quản trị) đã bật nếu muốn đưa vào phản hồi.
- Chống spam form đăng nhu cầu công nghệ vẫn đang tắt (commit `865143b`).
- Log còn lỗi cũ `A second operation was started on this context` khi dựng thống kê Dashboard (`AdminDashboardService.RebuildSnapshotAsync`) – không ảnh hưởng trang.
- Push nhánh lên GitHub (chưa push).
