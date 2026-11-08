# Kế hoạch làm chức năng còn thiếu theo góp ý MobiFone – Gói thầu số 4

Ngày lập: 12/10/2026 · Nhánh: `feature/bo-sung-chuc-nang-hsmt`

**Căn cứ:** `Gop_y_hoan_thien_de_xuat_ky_thuat_MobiFone.docx` (góp ý 11–15), `De_xuat_ky_thuat_Goi4_12102026_v3.docx` (đang ghi "Đáp ứng"), kiểm tra code ngày 12/10/2026 tại `Tra_loi_ky_thuat_cau_hoi_MobiFone_12102026.md`.

**Mục tiêu:** xong trước buổi demo, như đã báo MobiFone trong văn bản phản hồi. Tổng **~9 ngày công** (1 lập trình viên).

**Đã có, không phải làm:** nhật ký tìm kiếm (bảng `SearchQueryLog`, 502 dòng); quản trị chuyên gia (CMS Nhà tư vấn); biểu đồ thống kê (CMS Thống kê); trạng thái đơn đặt mua 1–4 và hình thức thanh toán.

## Tổng hợp

| # | Góp ý | Việc | Công |
| --- | --- | --- | --- |
| 1 | 11 – A.4.4 | Đếm lượt nhấp banner, nhấp đường dẫn, bấm chia sẻ sự kiện | 1 |
| 2 | 12 – A.5.2 | Mã đơn, giá trị đơn, trạng thái thanh toán, tổng tiền đã chi | 1,5 |
| 3 | 13 – B.2.3 | Số lượt giao dịch của người bán, danh sách, lọc thời gian | 1 |
| 4 | 14 – A.8.5 | Màn hình tra cứu nhật ký tìm kiếm; khóa sửa/xóa bảng nhật ký | 1 |
| 5 | 15 – Báo cáo | Xuất PDF cho 3 báo cáo | 1,5 |
| 6 | 5, PL-06 | Chạy kịch bản quy trình giao dịch (cấu hình 7 và 14 bước) bằng thao tác thật, lập biên bản | 1 |
| 7 | 5 | Gửi thông báo khi xử lý bước 6 (kiểm tra pháp lý) | 0,5 |
| 8 | C.2, PL-03 | API mở cho sàn khác: danh sách sản phẩm, nhu cầu; khóa API; tài liệu xác thực, mã lỗi | 1,5 |
| | | **Cộng** | **9** |

## 1. Thống kê sự kiện (A.4.4) · 1 ngày

Hiện có: `EventInteraction` ghi 3 loại (Xem=1, Quan tâm=2, Nhắc lịch=3) + đăng ký, điểm danh.

- Thêm loại `BannerClick=4`, `LinkClick=5`, `Share=6` vào `EventInteractionTypes` (không cần bảng mới).
- Endpoint `POST /su-kien/{id}/track?type=` (chống ghi trùng theo `VisitorKey` trong 30 phút), gọi bằng `navigator.sendBeacon`:
  - banner trang chủ (`Components/HeroBanner`) khi `ImageAdver.URL` trỏ tới trang sự kiện;
  - thẻ sự kiện ở danh sách tin, khối sự kiện trang chủ;
  - nút chia sẻ trong `_ContentUtilities.cshtml` khi đang ở trang sự kiện (ghi "bấm chia sẻ", không coi là chia sẻ thành công).
- CMS `EventsAdmin` > Thống kê: thêm 3 cột và 3 đường trên biểu đồ theo ngày.
- Bản EN dùng chung endpoint.

## 2. Lịch sử mua hàng (A.5.2) · 1,5 ngày

Hiện có: `OcopOrderRequest` (sản phẩm, nhà cung cấp, số lượng, `GiaThamKhao` dạng chữ, `StatusId` 1 Mới/2 Xác nhận/3 Hoàn tất/4 Hủy, `HinhThucThanhToan`).

- Thêm cột: `GiaTriDon decimal(18,0) NULL` (người bán/quản trị nhập khi xác nhận đơn), `TrangThaiThanhToan tinyint` (0 Chưa thanh toán, 1 Đã thanh toán, 2 Hoàn tiền), `NgayThanhToan`, `NgayHoanTat`. Script `Scripts/dot7-bo-sung-gop-y-mobifone.sql`.
- Mã đơn hiển thị: `DH-` + Id 6 chữ số (không thêm cột).
- Màn hình quản lý đơn của người bán (`QuanLySanPham`) và CMS: nhập giá trị đơn, cập nhật thanh toán.
- CMS `BuyerHistoryAdmin`: thêm cột mã đơn, giá trị, thanh toán, hoàn thành; dòng **Tổng tiền đã chi** = tổng `GiaTriDon` các đơn Hoàn tất và Đã thanh toán, trừ đơn Hủy/Hoàn tiền.
- Ghi chú hồ sơ: Sàn không có cổng thanh toán; trạng thái thanh toán do người bán/quản trị xác nhận.

## 3. Giao dịch của người bán (B.2.3) · 1 ngày

- Định nghĩa "lượt giao dịch" = đơn đặt mua (`OcopOrderRequests.SupplierId`) có trạng thái Xác nhận/Hoàn tất + hồ sơ chuyển giao (`Projects.SelectedSellerId`) đã qua bước ký hợp đồng. Đếm theo Id, không đếm trùng.
- Trang quản lý sản phẩm của người bán: thêm thẻ "Giao dịch" (tổng, theo tháng) và danh sách có lọc từ ngày – đến ngày.
- CMS Nhà cung cấp: thêm cột số giao dịch; trang chi tiết có danh sách, xuất Excel.

## 4. Nhật ký tìm kiếm và bảo vệ nhật ký (A.8.5) · 1 ngày

- CMS > Quản trị hệ thống > **Nhật ký tìm kiếm**: lọc theo thời gian, từ khóa, kiểu tìm (thường/AI), ngôn ngữ; xuất Excel; quyền xem riêng trong phân quyền.
- Script SQL tạo trigger `INSTEAD OF UPDATE, DELETE` trên các bảng nhật ký (`SearchQueryLog`, `AISearchLogs`, nhật ký CMS, `EntityActionLog`, `ContractAuditLog`, `ESignAuditLog`, `ProjectAccessLog`, `WorkflowTransitionLogs`) → chỉ cho thêm, không cho sửa/xóa từ ứng dụng.
- Tham số `LOG_RETENTION_YEARS = 5` (chỉ ghi nhận chính sách, không tự xóa); đưa vào tài liệu quản trị.

## 5. Xuất PDF báo cáo · 1,5 ngày

Dùng lại `HtmlToPdfService` (iText pdfHTML, đã có font tiếng Việt cho hợp đồng).

- Thêm tiện ích render Razor partial ra chuỗi HTML.
- Nút "Xuất PDF" cạnh "Xuất Excel" ở 3 báo cáo: Thống kê tổng hợp (`AdminAnalytics`), Thu chi (`FinanceAdmin`), Báo cáo giao dịch. PDF gồm tiêu đề, kỳ báo cáo, người xuất, thời điểm, bảng chỉ tiêu; biểu đồ xuất dạng bảng số liệu (pdfHTML không chạy JavaScript).

## 6. Biên bản kiểm thử quy trình giao dịch (PL-06) · 1 ngày

Quy trình mặc định hiển thị rút gọn 7 bước (tham số `Workflow:VisibleStepCount` trong `appsettings.json`, chọn 7/10/14; đổi tham số cần khởi động lại ứng dụng).

- Cấu hình 7 bước: 1 dự án không NDA (bước 2 hiện "Không áp dụng") và 1 dự án có NDA, đi đến bước 7 ký hợp đồng.
- Cấu hình 14 bước: 1 dự án đi đủ 14 bước.
- Dùng 3 tài khoản thật (bên mua, bên bán, chuyên gia).
- Chụp màn hình từng bước, ghi nhật ký chuyển trạng thái, thông báo email; lập biên bản theo mẫu PL-06.
- Lưu ý: ký số chạy chế độ thử (chưa có CA thật) – ghi rõ trong biên bản.

## 7. Thông báo bước 6 – Kiểm tra pháp lý · 0,5 ngày

13/14 bước đã gửi thông báo khi chuyển bước; `LegalReviewController` chưa gửi. Thêm thông báo qua hàng đợi (email + chuông web) cho bên mua, bên bán khi kết quả kiểm tra pháp lý được cập nhật, giống các bước khác. Hồ sơ đang ghi "mỗi lần chuyển bước, hệ thống gửi thông báo".

## 8. API mở cho sàn khác (C.2) và hoàn thiện PL-03 · 1,5 ngày

Kiểm tra ngày 13/10/2026: Swagger hiện có 20 API nội bộ (chat, thông báo, AI, thống kê), chưa có API cung cấp dữ liệu cho sàn khác; tài liệu chưa khai báo xác thực và mã lỗi. Đề xuất mục C.2 đang ghi "API cho sàn khác có tài liệu và phân quyền".

- `GET /api/open/v1/products`, `/products/{id}`, `/demands`, `/suppliers`: chỉ dữ liệu đã xuất bản, phân trang, lọc theo ngày cập nhật, lĩnh vực, ngôn ngữ.
- Xác thực bằng khóa API (header `X-Api-Key`), quản lý khóa trong CMS (cấp, thu hồi, giới hạn lượt gọi); ghi nhật ký gọi API.
- Mã lỗi thống nhất (400, 401, 403, 404, 429, 500) dạng JSON `{code, message}`.
- Swagger: khai báo security scheme, mã lỗi; xuất lại PL-03.

## Thứ tự làm

1. Việc 7 → 4 (nhỏ, nền cho ATTT) → 1 → 3 → 2 (có thay đổi CSDL, làm cùng script `dot7`) → 5 → 6 (chạy cuối, trên bản đã đủ chức năng).
2. Mỗi việc: build 0 lỗi, chạy thử trên máy phát triển, cập nhật `Bang_dap_ung_ho_so_ky_thuat.md`, thêm bản EN cho phần công khai (việc 1).
3. Xong cả 6 việc: cập nhật bảng đáp ứng trong đề xuất (A.4.4, A.5.2, B.2.3, A.8.5, báo cáo) theo đúng mô tả đã làm; bổ sung bước vào `KichBan_Demo_ChuongV.md`.

## Cần chốt

| # | Việc | Ai |
| --- | --- | --- |
| 1 | Định nghĩa "lượt giao dịch" của người bán (mục 3) có đúng ý khách không | Anh Nghĩa |
| 2 | Giá trị đơn do người bán hay quản trị Sàn nhập | Anh Nghĩa |
| 3 | Ngày demo dự kiến | MobiFone |

## Theo dõi

| Việc | Bắt đầu | Xong | Trạng thái |
| --- | --- | --- | --- |
| 1 | | | Chưa làm |
| 2 | | | Chưa làm |
| 3 | | | Chưa làm |
| 4 | | | Chưa làm |
| 5 | | | Chưa làm |
| 6 | | | Chưa làm |
| 7 | | | Chưa làm |
| 8 | | | Chưa làm |
