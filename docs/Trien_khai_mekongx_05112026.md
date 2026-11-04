# Cập nhật mekongx.vn – 05/11/2026

Khắc phục lỗi L2-01 … L2-11 theo báo cáo kiểm tra chuỗi nghiệp vụ lõi ngày 03/11/2026, trước vận hành thử lần 2 (09–13/11/2026).
Trạng thái: **đã đóng gói `deploy-mekongx-2026-11-05.zip`, chưa chạy trên máy chủ**. Code: `master` trên máy, gồm bản sửa L2, phần đầu tích hợp eContract (chưa có màn hình gọi) và hồ sơ thuê Sàn.

## Nội dung sửa

| Lỗi | Sửa |
|---|---|
| L2-04 | Trang Nhật ký hồ sơ luôn trống do tham số lọc `action` trùng giá trị route; đổi thành `logAction`. Ghi thêm lượt truy cập của bên mua, nhà tư vấn |
| L2-05 | Ghi bảng Log khi đăng ký tài khoản, đăng ký nhà cung ứng, đăng/sửa/xóa sản phẩm, đăng nhu cầu, tạo hồ sơ, chuyển bước |
| L2-07 | Nhắc mời ở bước 3; báo chủ sản phẩm khi có yêu cầu; CMS gán, thu hồi bên bán; cột Bên bán hiện bên được mời; chặn xuất bản khi nhà cung ứng chưa Xuất bản + Kích hoạt |
| NDA | Hồ sơ không áp dụng NDA thì bên bán không phải ký (`ESignGateway.HasUserSignedProjectNda`) |
| L2-08 | Bắt buộc lý do khi Trả về (nhà cung ứng, sản phẩm, nhu cầu), gửi lý do cho người đăng; bỏ biểu tượng gây nhầm |
| L2-09 | `ProductPublishSync`: cập nhật lĩnh vực, dữ liệu tìm kiếm khi đăng, xuất bản; tìm được theo mô tả chi tiết; gợi ý trên trang nhu cầu; biểu mẫu thêm lĩnh vực, ngân sách, thời hạn (ghi vào nội dung phiếu) |
| L2-10 | Trang người bán dùng tên trạng thái của CMS; tiến trình đếm đủ số bước, đánh số đúng bước thật |
| L2-01 | Giới hạn "forms" 20 lần/phút/IP (`Security:FormRateLimitPerMinute`); mỗi tài khoản một hồ sơ nhà cung ứng |

## Gói

- `app\TechExchangeApp.dll`: không đổi thư viện, CSS, JS.
- `sql\add-econtract-columns.sql`: **bắt buộc, chạy trước khi chép dll** vì `ProjectContracts` có cột mới; `ECONTRACT_ENABLED` mặc định 0.
- `sql\deploy-server-2026-11-04.sql`: **bắt buộc**, chạy lại được.
- `sql\tuy-chon-xoa-du-lieu-ra-quet.sql`, `sql\tuy-chon-xoa-san-pham-pentest.sql`: tùy chọn, đều có chế độ chỉ liệt kê.
- `HUONG_DAN.txt`.

## Lưu ý

- Phần xóa của `xoa-du-lieu-ra-quet-2026-11.sql` chưa chạy thử; phải thử trên bản sao trước.
- Kiểm tra `Workflow:VisibleStepCount` trên máy chủ: phải là 14.
- L2-12 (giờ máy chủ) và dữ liệu tài khoản thử xử lý trên máy chủ, không nằm trong code.
- Khi đẩy tải 200 người dùng từ một IP, giới hạn "forms" có thể chặn thao tác gửi biểu mẫu; nới `Security:FormRateLimitPerMinute` trong lúc đo.
