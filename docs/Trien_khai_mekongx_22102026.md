# Triển khai mekongx.vn – 22/10/2026

Cập nhật máy chủ từ bản 16/10/2026 (commit `0895ad9`) lên bản 22/10/2026: bổ sung code cho 60 chức năng nghiệm thu đợt 1 (Gói thầu số 4) và hướng dẫn CMS.
Trạng thái: **đã đóng gói, chưa chạy trên máy chủ**.

## 1. Code đưa lên (nhánh `feature/bo-sung-chuc-nang-hsmt`)

| Commit | Nội dung |
|---|---|
| `5d100ea` | Lịch sử mua hàng (mã đơn, giá trị, thanh toán, tổng đã chi); lượt giao dịch người bán; tiêu chí phân hạng NCC (giao dịch, doanh thu, đánh giá); chat gửi ảnh/tệp; nhật ký chuyển bước và tải tệp; lượt nhấp/chia sẻ sự kiện; báo cáo giao dịch; xuất PDF; sơ đồ 14 bước |
| `25980e6`, `e91dae5` | Ghi chú kiểm thử, gói triển khai (`docs/Trien_khai_bo_sung_dot7_22102026.md`) |
| `f298c4d` | Hướng dẫn CMS: chức năng mới, tham số ký số từ xa, SMS |

Chi tiết chức năng theo mã HSMT: xem `docs/Trien_khai_bo_sung_dot7_22102026.md`.

## 2. Gói triển khai

- `deploy-mekongx-2026-10-22.zip` (thư mục gốc repo, không đưa vào git):
  - `app\` – toàn bộ file ứng dụng, publish Release ra thư mục sạch `D:\2026\CanThoPublish_20261022` (97 file).
  - `sql\deploy-server-2026-10-22.sql`
  - `HUONG_DAN.txt` – các bước cập nhật.
- Không gồm `appsettings*.json` và `wwwroot\` (wwwroot không đổi so với 16/10).
- So với gói 16/10: danh sách file giống nhau, chỉ thêm `TechExchangeApp.pdb`; có đủ Swashbuckle, HtmlAgilityPack, iText.
- Chạy thử bản publish trên máy (cổng 5299): trang chủ 200; CMS, tệp chat chuyển trang đăng nhập đúng.

## 3. Cơ sở dữ liệu

`TechExchangeApp/Scripts/deploy-server-2026-10-22.sql` (bản sao của `dot7-hoan-thien-nghiem-thu-dot1.sql`), chạy lại nhiều lần an toàn:

```
sqlcmd -S <server> -d <database> -U <user> -P <pwd> -f 65001 -b -I -i deploy-server-2026-10-22.sql
```

- Thêm cột: `OcopOrderRequests` (GiaTriDon, TrangThaiThanhToan, NgayThanhToan), `SupplierTiers` (MinTransactions, MinRevenue, MinRating), `ChatMessages` (Attachment*).
- Tạm tính giá trị các đơn đã hoàn tất từ giá tham khảo khi là số.
- Bài hướng dẫn trực tuyến "Quy trình hồ sơ giao dịch" đổi 6 bước → 14 bước (giữ nguyên đường dẫn).
- Dòng kết quả cuối: `GiaTriDon 9, MinRevenue 9, ChatAttach 1000, Guide14 1` (Guide14 = 0 nếu máy chủ không có bài hướng dẫn đó).

## 4. Thứ tự trên máy chủ

1. Sao lưu cơ sở dữ liệu.
2. Chạy script SQL ở mục 3.
3. Dừng app pool (hoặc đặt `app_offline.htm` vào `D:\www\sgdcn\`).
4. Chép đè toàn bộ `app\` vào `D:\www\sgdcn\`.
5. **Mới:** tạo `D:\www\sgdcn\App_Data`, cấp quyền Modify cho `IIS AppPool\<pool>` (tệp chat lưu ở `App_Data\chat`). Các lần sau không xóa thư mục này.
6. Khởi động lại app pool; kiểm tra trang chủ, đăng nhập, CMS > Báo cáo giao dịch, CMS > Hướng dẫn CMS, gửi thử 1 ảnh trong chat.

## 5. Còn chờ

- SMS Brandname: chưa có bộ kết nối nhà cung cấp; `NOTIFICATION_ENABLE_SMS` chỉ bật sau khi nối (tin nhắn hiện chỉ ghi nhật ký).
- Ký số từ xa: điền tài khoản `SIGNING_VNPT_…` (hoặc VIETTEL/FPT), đổi `SIGNING_MODE = PRODUCTION`.
- Chưa thử trên máy bằng tài khoản người bán (quản lý đơn, quản lý sản phẩm) và gửi tệp chat thật.
- Push nhánh lên GitHub (chưa push).
