# Cập nhật mekongx.vn – 10/11/2026

Hai gói trong ngày:

1. `deploy-mekongx-2026-11-10.zip`: khắc phục lỗi L2 lần 2 theo Báo cáo kiểm tra lại ngày 09/11/2026 của Bình Định. **Đã chép lên máy chủ**, đã kiểm tra trên mekongx.vn.
2. `deploy-mekongx-2026-11-10-2.zip`: sửa chuông thông báo trên đầu trang. Chưa chép lên máy chủ.

## Gói 1: khắc phục lỗi L2 lần 2

### Nội dung sửa

| Mục | Trước | Sau |
|---|---|---|
| L2-07: ô chọn trong hộp Gán bên bán (CMS › Dự án) | Nhãn "-- Chọn nhà tư vấn --" | "-- Chọn nhà cung ứng --" (nhãn lấy theo từng ô) |
| L2-09: gợi ý sản phẩm ở trang nhu cầu | Lấy sản phẩm trùng bất kỳ lĩnh vực nào, lẫn máy xúc, máy quang phổ, máy làm kem ở tin #109 | Chỉ gợi ý sản phẩm có đủ mọi lĩnh vực của nhu cầu; không lấy sản phẩm khác để bù đủ số lượng |
| L2-15: giá bán dự kiến / giá tham khảo | Nhận số âm ("-100") | Báo lỗi, không lưu; áp dụng CMS và trang người dùng, kể cả gửi thẳng yêu cầu |
| L2-04: nhật ký bước 1 của hồ sơ không áp dụng NDA | "mở bước 2 – NDA" | "bước 2 – NDA: không áp dụng; mở bước 3" (bản ghi cũ giữ nguyên) |

### File thay đổi

- `Areas/Cms/Views/ProjectsAdmin/Index.cshtml`: nhãn select2 lấy từ option rỗng của từng ô.
- `Controllers/NhucaucongngheController.cs`: gợi ý theo đủ lĩnh vực.
- `Areas/Cms/Controllers/SanPhamCNTBController.cs`: `SanPhamCNTBFormVm` kiểm tra giá âm (`GiaBanDuKien` là HTML CKEditor; `OriginalPrice`, `SellPrice` ≥ 0).
- `Areas/Cms/Views/SanPhamCNTB/Create.cshtml`, `Edit.cshtml`, `EditCongNghe.cshtml`: hiện lỗi dưới ô giá.
- `Services/WorkflowService.cs`: nội dung nhật ký bước 1 khi không áp dụng NDA.
- `Areas/Cms/Views/HuongDanCms/Index.cshtml`: hướng dẫn CMS theo các nội dung trên.
- `TechExchangeApp.Tests/Application/GiaThamKhaoValidationTests.cs`: 12 trường hợp, đạt.

### Dữ liệu (đã chạy trên máy chủ 09–10/11, không nằm trong gói)

Script `Scripts/sua-l2-lan2-du-lieu-2026-11-10.sql` (xem trước rồi chạy thật, chạy lại an toàn):

- L2-07: NCC 17924 (vht_seller01) đổi tên, liên hệ theo tài khoản, đường dẫn `nha-cung-ung-01-van-hanh-thu`; tạo NCC 17926 cho vht_seller02; cả hai Xuất bản, Kích hoạt.
- L2-09: VHT2-SP-01 (44050) lĩnh vực 9, 18; VHT2-SP-02 (44051) lĩnh vực 5, 14; đặt ngày xuất bản; dữ liệu tìm kiếm có mô tả chi tiết; thủ tục `uspReIndexSearchContents` thêm `MoTa`.
- L2-14: thu hồi phiên đăng nhập vht_buyer01 (tăng `SessionVersion`).

Sao lưu: các bảng `*_Backup_L2Lan2_<ngàygiờ>`. Kiểm tra: `Scripts/kiem-tra-buoc0-l2-lan2-2026-11-09.sql` (chỉ in mục chưa đạt).

### Đã kiểm tra trên mekongx.vn (10/11)

- `/health` Healthy.
- Tin nhu cầu #109 chỉ gợi ý VHT2-SP-01.
- Tìm "lọc sinh học" ra VHT2-SP-01, "tấm thu nhiệt" ra VHT2-SP-02.
- Trang Thủy sản có VHT2-SP-01, Cơ khí có VHT2-SP-02.
- Sản phẩm "Quy trinh pentest" (CN-43687) không còn trên danh sách, tìm kiếm; đường dẫn chi tiết chuyển hướng.

### Chưa kiểm tra

- Nhãn ô chọn bên bán và chặn giá âm trên CMS (cần đăng nhập CMS).
- Nhật ký bước 1 khi không áp dụng NDA (chỉ ghi khi tạo hồ sơ mới).

### Còn lại, không thuộc gói

- L2-03 (2 hồ sơ "Quy trinh pentest") và 2 hồ sơ eContract của L2-14: giữ đến khi đối tác kiểm thử xong, sau đó loại khỏi dữ liệu vận hành.
- L2-14: đặt lại mật khẩu vht_buyer01 trên CMS, bàn giao riêng cho Bình Định.
- L2-13 (tin nhu cầu công khai hiện điện thoại, email): chờ ý kiến chủ đầu tư.
- Văn bản trả lời: `docs/10-11-2026/Thong bao ket qua khac phuc loi L2 lan 2.docx` (chưa điền số văn bản).

## Gói 2: chuông thông báo

| Mục | Trước | Sau |
|---|---|---|
| Chuông thông báo trên đầu trang | Luôn "Không có thông báo", không có số chưa đọc, không nhận thông báo tức thời | Hiện danh sách, số chưa đọc, nhận thông báo qua SignalR |

Nguyên nhân: trong `Views/Shared/_Layout.cshtml`, dòng `const TP_LOCALE = @(isEnLayout ? "'en-US'" : "'vi-VN'");` bị Razor mã hóa dấu nháy thành `&#x27;`, trình duyệt báo lỗi cú pháp (`Unexpected token '&'`) nên cả đoạn script thông báo không chạy. API `/api/notifications/latest`, `/unread-count` vẫn trả đúng dữ liệu. Lỗi có từ 03/08/2026 (commit `95d136d`, thêm tiếng Anh), ảnh hưởng mọi tài khoản.

Sửa: `const TP_LOCALE = '@(isEnLayout ? "en-US" : "vi-VN")';` (dấu nháy ngoài biểu thức Razor). Đã rà, không còn chỗ nào viết cùng kiểu.

Kiểm tra sau khi chép: đăng nhập tài khoản có thông báo, bấm chuông, danh sách và số chưa đọc khớp trang "Thông báo của tôi".

## Gói, mã nguồn

- Cả hai gói chỉ gồm `app\TechExchangeApp.dll` và `HUONG_DAN.txt`; không có SQL, không đổi `appsettings.json`, `wwwroot`.
- Build từ nhánh chính (master), đã gộp nhánh eContract / ký số USB Token (`ad02491`, bản chạy trên máy chủ từ 08/11). So với bản publish trước chỉ khác `TechExchangeApp.dll`.
- Gói 1: master `66afdf9` (đã push GitHub). Gói 2: master `187a85f`.
- Thư mục publish: `D:\2026\CanThoPublish_20261110` (gói 1), `D:\2026\CanThoPublish_20261110_2` (gói 2).
- Lưu ý: từ nay build gói từ master. Trước 10/11 máy chủ chạy bản build từ nhánh eContract, không phải master; build từ master cũ sẽ làm mất phần ký số.
