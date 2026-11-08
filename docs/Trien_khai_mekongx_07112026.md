# Cập nhật mekongx.vn – 07/11/2026

Máy chủ hiện tại: `203.205.1.9`. Đã chép 2 gói trong ngày; kiểm tra lại trên site sau khi chép gói lần 2: `/health` Healthy, các trang chính HTTP 200, tìm kiếm có kết quả.

## Gói đã chép

| Gói | Nội dung |
|---|---|
| `deploy-mekongx-2026-11-07.zip` | Trợ lý ảo gợi ý theo sản phẩm đã xem và lĩnh vực (nút "Gợi ý cho tôi", HSMT C.10); mẫu hợp đồng giữ khối chữ ký không bị tách trang |
| `deploy-mekongx-2026-11-07-2.zip` | Trang báo lỗi HTTP dùng font-weight 700; thư, SMS đổi TechPort thành tên Sàn; giá trị dự phòng đổi sang mekongx.vn; thư nhu cầu mới mặc định về `no-reply@mekongx.vn` |

Không có script SQL trong 2 gói. Bản publish: `D:\2026\CanThoPublish_20261107`, `D:\2026\CanThoPublish_20261107_2`.

## Xử lý trên máy chủ

| Việc | Kết quả |
|---|---|
| Lỗi HTTP 500 trang `Project/Details` | Do thiếu `add-econtract-columns.sql`; đã xử lý, hồ sơ 2037 mở lại được, đủ 2 chỉ mục `UX_ProjectContracts_*` |
| Tìm kiếm không có kết quả (lỗi 7609) | Máy mới chưa cài Full-Text Search; đã cài và dựng lại `FTC_SearchIndex` |
| Giờ máy chủ (L2-12) | Đã đồng bộ; đo so sánh không còn thấy lệch 04 phút 47 giây |
| SMTP | `mail.mekongx.vn`, cổng 465, `no-reply@mekongx.vn`, `SMTP_SKIP_CERT_CHECK = 1` |

Kiểm tra CSDL thiếu bảng, cột, thủ tục: `TechExchangeApp/Scripts/kiem-tra-thieu-script-2026-11-07.sql` (chỉ đọc). Dòng `AddTab6Tab7Fields.sql` và `UpdateQueryStringByTableName` là báo nhầm.

## Còn tồn tại

- **Thư tới Gmail bị từ chối:** `550 5.7.1`, IP `42.1.67.76`. PTR là `cloudhosting.mobifone.vn` nhưng tên này trỏ về `42.1.67.70`. Cần bên quản trị máy chủ thư sửa PTR hoặc dùng SMTP relay. `TestEmail` báo thành công chỉ có nghĩa máy chủ thư đã nhận thư.
- **`NOTIFICATION_ENABLE_EMAIL`:** lần xem gần nhất là `false`. Khi tắt, thư qua hàng đợi không gửi nhưng vẫn ghi thành công. Chưa xác nhận đã bật.
- **`AppSettings:AdminEmail`:** chưa xác nhận đã thêm; code mặc định dùng `no-reply@mekongx.vn`.
- **L2-07:** chưa xác nhận đã khôi phục hồ sơ nhà cung ứng 17924, mời bên bán trên 02 hồ sơ thử.
- **Tìm kiếm:** tìm không dấu ("may say") không có kết quả; tìm AI "OCOP" hiện trống trong khi tìm thường có 49 kết quả. Chưa xác định nguyên nhân.
- **Trợ lý ảo:** chưa kiểm tra riêng phần đọc lịch sử xem cũ của tài khoản.
- **Site lỗi 500 lúc khoảng 04:09:** `/health` Unhealthy, tự hết sau đó, chưa rõ nguyên nhân; cần xem log.
- **Chưa đổi tên:** số hợp đồng `TechPort-…`, `TechPort Bridge`. Liên kết đối tác `techport.vn`, 2 chuyển hướng cũ, `EncryptKeyword` giữ nguyên có chủ đích.
