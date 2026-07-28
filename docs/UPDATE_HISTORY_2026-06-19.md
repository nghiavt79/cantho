# Lịch sử cập nhật hệ thống - 19/06/2026

Tài liệu này tổng hợp các thay đổi và tính năng mới đã được cập nhật trong phiên làm việc ngày 19/06/2026, dựa trên nội dung bàn giao/cập nhật được cung cấp.

## 1. Cấu hình nhanh trạng thái trực tiếp trong CMS

Mục tiêu: cho phép quản trị viên thay đổi trạng thái của các thực thể trực tiếp từ màn hình danh sách trong CMS, không cần truy cập màn hình chỉnh sửa.

Điểm chính:

- Giữ nguyên `_QuickConfigPartial.cshtml` dành cho Posts để tránh ảnh hưởng luồng đang ổn định.
- Bổ sung cơ chế dropdown/button kết hợp AJAX để đổi trạng thái trực tiếp trên danh sách.

Module áp dụng:

- Quản lý Sản phẩm CNTB: `SanPhamCNTB`.
- Quản lý Chuyên gia / Nhà tư vấn: `NhaTuVanAdmin`.
- Quản lý Nhà cung ứng: `NhaCungUngAdmin`.

## 2. Khắc phục upload hình ảnh cho người dùng frontend

Mục tiêu: đảm bảo người dùng thông thường khi đăng nhập ở frontend có thể đăng tin/bài viết kèm upload hình ảnh thành công và an toàn.

Điểm chính:

- Điều chỉnh luồng API/controller upload để cho phép request từ người dùng frontend, không chỉ giới hạn trong CMS.
- Chỉ cho phép định dạng hình ảnh an toàn: `.jpg`, `.jpeg`, `.png`, `.webp`.
- Có xử lý giới hạn dung lượng và đổi tên file ngẫu nhiên để tránh trùng lặp.
- Không tạo thêm thư mục mới như `PublisherUpload`.
- Thống nhất lưu trữ vào thư mục chuẩn `/Uploads`.
- File upload được phân loại theo cấu trúc thời gian thực: `Uploads/{Năm}/{Tháng}-{Ngày}`.

Ví dụ:

```text
Uploads/2026/6-19
```

## 3. Báo cáo vi phạm trên frontend

Mục tiêu: cung cấp cơ chế để người dùng báo cáo nội dung xấu/vi phạm, đồng thời tái sử dụng bảng `Feedback` để tránh phình to cấu trúc database.

### Backend

Thành phần:

- `ViolationReportDto.cs`
- `FeedbackController.cs`

Điểm chính:

- Thêm action `SubmitViolationReport`.
- Chống spam bằng session, thời gian chờ 60 giây.
- Gom dữ liệu báo cáo thành nội dung text nhiều dòng và lưu vào cột `Content` của bảng `Feedback`.
- Tự động gán:
  - `Title = "Báo cáo vi phạm"`
  - `StatusId = 2`

Dữ liệu báo cáo gồm:

- Loại bài.
- ID nội dung.
- Link bài.
- Nội dung báo cáo.

### Frontend

Thành phần:

- `_ReportViolationModal.cshtml`
- `auth-modals.js`

Điểm chính:

- Modal báo cáo dùng chung.
- Tự động điền họ tên, số điện thoại, email nếu lấy được từ tài khoản đang đăng nhập.
- Các trường tự động điền được khóa sửa bằng `readonly`.
- Nếu người dùng chưa đăng nhập mà bấm báo cáo, hệ thống tự mở modal đăng nhập.
- Sau khi đăng nhập/đăng ký qua popup modal, gọi `window.location.reload()` để người dùng ở lại đúng trang bài viết thay vì bị chuyển sang dashboard.

### Trang detail đã tích hợp nút báo cáo

- Tin tức / Sự kiện: `Views/News/Detail.cshtml`.
- Yêu cầu công nghệ: `Views/Nhucaucongnghe/Detail.cshtml`.
- Sản phẩm CNTB: `Views/Product/Detail.cshtml`.
- Nhà cung ứng: `Views/NhaCungUng/Detail.cshtml`.
- Chuyên gia tư vấn: `Views/ChuyenGia/Detail.cshtml`.
- Dịch vụ tư vấn - Chuyên gia: `Views/DichVuTuVan/ChiTietNhaTuVan.cshtml`.
- Dịch vụ tư vấn - Nhà cung ứng: `Views/DichVuTuVan/ChiTietNhaCungUng.cshtml`.
- Tìm kiếm đối tác: `Views/TimKiemDoiTac/Detail.cshtml`.
- Thảo luận: `Views/Forum/Detail.cshtml`.

Ghi chú CMS:

- Ban quản trị vào `FeedbackAdmin` để xem các mục có tiêu đề `Báo cáo vi phạm`.
- Nội dung chi tiết được lưu trong cột `Content`.

## 4. Quản lý diễn đàn và thảo luận

Mục tiêu: hoàn thiện luồng frontend và CMS cho chức năng thảo luận.

### Trang danh sách thảo luận

File:

- `Views/Forum/Index.cshtml`

Thay đổi:

- Kiểm tra xác thực bằng `User.Identity.IsAuthenticated`.
- Ẩn form nếu người dùng chưa đăng nhập.
- Tự động điền họ tên, email, số điện thoại từ session tài khoản.
- Thiết lập readonly cho các trường thông tin người dùng.
- Tích hợp CKEditor 5 cho trường nội dung, lazy load khi mở form.
- Thêm validation JavaScript để chặn gửi nội dung rỗng.
- Ẩn dropdown loại thảo luận, gán mặc định hidden value `1` cho yêu cầu công nghệ thiết bị.

### Trang chi tiết thảo luận

File:

- `Views/Forum/Detail.cshtml`

Thay đổi:

- Chặn hiển thị form trả lời nếu người dùng chưa đăng nhập.
- Tự động điền họ tên, email và thiết lập readonly.
- Tích hợp CKEditor 5 cho khung nhập câu trả lời.
- Sửa lỗi hiển thị raw HTML bằng `System.Net.WebUtility.HtmlDecode` cho nội dung chủ đề và bình luận.

### Controller diễn đàn

File:

- `Controllers/ForumController.cs`

Thay đổi:

- Thêm anti-spam/anti-DOS bằng `IMemoryCache`.
- `CreateDiscussion`: giới hạn 1 phút cho mỗi thao tác tạo chủ đề theo user/IP.
- `AddComment`: giới hạn 30 giây cho mỗi bình luận.
- Khắc phục lỗi `SqlException` do cột `Domain = NULL` trên bảng `ForumYCTB`, `ForumYCDV`.
- Gán `_mainDomain` từ cấu hình appsettings.
- Sửa lỗi biên dịch `CS0117` do gán `Domain` vào entity `CommentsYCTB`, trong khi bảng này không có cột `Domain`.

### CMS quản lý thảo luận

File:

- `Areas/Cms/Views/ForumAdmin/*`

Thay đổi:

- Nâng cấp giao diện quản lý thảo luận trong CMS dùng `_LayoutAdminLTE`.
- Thêm cột thống kê số lượng câu trả lời trong danh sách chủ đề.
- Chuyển tải danh sách bình luận qua Ajax Partial View.
- Thêm tìm kiếm, lọc trạng thái và bulk delete cho bình luận.
- Cập nhật sidebar menu, gom nhóm `Quản lý thảo luận` với icon `fa-people-group`.

## 5. Ghi chú build và triển khai

- Hệ thống build: `0 Error(s)`.
- Nội dung cung cấp ghi nhận đã thực hiện git commit và push lên nhánh `master`.

## 6. Ghi chú rà soát

- Tài liệu này chỉ tổng hợp nội dung cập nhật theo bản ghi được cung cấp.
- Không chứa API key, connection string, token hoặc secret thật.
- Không thay đổi source code, database script hoặc logic xử lý.
