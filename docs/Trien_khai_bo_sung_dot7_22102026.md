# Bổ sung code cho 60 chức năng nghiệm thu đợt 1 – 22/10/2026

Nhánh `feature/bo-sung-chuc-nang-hsmt`, chưa commit. Build 0 lỗi.

## Đã làm

| Mã HSMT | Nội dung | Màn hình |
| --- | --- | --- |
| A.5.2 | Mã đơn, giá trị đơn, trạng thái/ngày thanh toán, tổng tiền đã chi | Người bán: Quản lý yêu cầu đặt mua (ô giá trị + “Đã thanh toán”); CMS: Lịch sử mua hàng |
| A.2.5, B.2.3 | Lượt giao dịch (đơn đặt mua chưa hủy) theo sản phẩm và tổng | Người bán: Quản lý sản phẩm; CMS: danh sách sản phẩm |
| A.2.4 | Tiêu chí hạng: số giao dịch, doanh thu, điểm đánh giá sản phẩm; gợi ý hạng dùng cả 3 tiêu chí | CMS: Phân hạng nhà cung cấp |
| A.5.3, A.2.10, C.7 | Chat gửi ảnh, PDF, Word/Excel/PowerPoint (≤10 MB); lưu ngoài wwwroot (`App_Data/chat`), tải qua kiểm tra quyền; quản trị xem, tìm theo tên tệp | Chat; CMS: Lịch sử chat |
| A.8.5 | Nhật ký chuyển bước quy trình (14 bước); nhật ký “ai tải tệp” hồ sơ giao dịch, hợp đồng, xác minh, tệp chat | CMS: Hồ sơ giao dịch > nút Nhật ký; CMS: Nhật ký, sự kiện “Tải tệp” |
| A.4.4 | Lượt nhấp banner/đường dẫn (link `/su-kien/nhap/{id}`), lượt chia sẻ | CMS: Sự kiện > Thống kê |
| A.6.1, A.8.1 | Xuất PDF sổ thu chi; màn hình Báo cáo giao dịch (hồ sơ, đơn, doanh thu) xuất Excel/PDF | CMS: Thu chi; CMS: Báo cáo giao dịch (menu mới) |
| – | Hướng dẫn CMS: sơ đồ 14 bước; bài hướng dẫn trực tuyến 14 bước (qua script) | CMS: Hướng dẫn |

## Kiểm thử trên máy

- Script `dot7` chạy 2 lần không lỗi.
- Đường dẫn theo dõi lượt nhấp, ghi lượt chia sẻ: ghi đúng, chuyển đúng trang sự kiện.
- Tải tệp hồ sơ: ghi nhật ký sự kiện 6; tải tệp chat khi chưa đăng nhập: chuyển trang đăng nhập.
- Các màn hình CMS chưa mở thử trên trình duyệt (chưa có tài khoản quản trị trên máy) – cần đăng nhập kiểm tra.

## Khi triển khai lên máy chủ

1. Chạy `TechExchangeApp/Scripts/dot7-hoan-thien-nghiem-thu-dot1.sql` (có `-f 65001`).
2. Cấp quyền ghi cho `IIS AppPool\<pool>` vào thư mục ứng dụng `App_Data` (tệp chat lưu tại `App_Data/chat`). Không xóa `App_Data` khi chép bản mới.
3. Chép toàn bộ dll như lần 16/10.

## Chưa làm (cần tài khoản/đối tác)

- SMS Brandname thật (cần tài liệu kết nối và tài khoản nhà cung cấp).
- Ký số từ xa bằng chứng thư thật (cần tài khoản CA).
- Đợt 2: C.2 kết nối Sàn KH&CN Việt Nam/sàn khác; gọi video hiện dùng `meet.jit.si` (tham số `VIDEO_CALL_BASE_URL`).
