# Cập nhật mekongx.vn 20/11/2026: sửa lỗi theo báo cáo kiểm thử 19/11

Gói `deploy-mekongx-2026-11-20.zip` (tại `D:\2026\CanTho`). Chưa chép lên máy chủ.

## Lý do

Khách gửi báo cáo kiểm thử `docs/19-11-2026/Kiểm thử.docx`, 4 lỗi trên mekongx.vn (BUG-001..004).

## Thay đổi

| Lỗi | Hiện tượng | Xử lý |
|---|---|---|
| BUG-001 | Ô tìm kiếm trang chủ: bấm Tìm kiếm khi chưa nhập từ khóa chuyển sang trang trống | Chặn gửi khi ô trống, hiện nhắc "nhập từ khóa" (`Views/Home/Index.cshtml`). Đã giải thích với khách: phải nhập từ khóa mới tìm kiếm |
| BUG-002 | Trang sản phẩm theo lĩnh vực: bộ lọc Khoảng giá / Trạng thái bấm "Áp dụng" không có tác dụng | Lọc phía máy chủ qua tham số `price`, `status`; giữ bộ lọc khi đổi trang, sắp xếp, số dòng (`ProductController`, `ProductService`, `ProductByCateViewModel`, `Views/Product/ProductByCate.cshtml`) |
| BUG-003 | Trang đặt hàng OCOP: link "OCOP" chuyển sang `/en/ocop` | Đổi thành `href="/ocop"` (`Views/OcopOrder/Index.cshtml`, `Success.cshtml`). Nguyên nhân: route `en_ocop` khai báo trước `ocop_index` nên tag helper sinh URL tiếng Anh |
| BUG-004 | `/en/ocop` không có sản phẩm | Script tạo bản tiếng Anh cho 20 sản phẩm OCOP (ID 35373–35392) |

Quy ước bộ lọc BUG-002:

- Giá theo `OriginalPrice` (VND): không có giá hoặc ≤ 0 là "Liên hệ báo giá"; dưới 5 triệu; 5–20 triệu; trên 20 triệu.
- Trạng thái: chưa có trường dữ liệu riêng, mọi sản phẩm đang hiển thị coi là "Đang giới thiệu" (khớp trang chi tiết); "Sẵn sàng chuyển giao", "Có tư vấn triển khai" chưa có sản phẩm nào.

Script BUG-004 (`Scripts/seed-en-ocop-products-2026-11-19.sql`, trong gói là `sql/dich-tieng-anh-san-pham-ocop.sql`):

- Mỗi sản phẩm thêm 1 dòng `SanPhamCNTB` với `LanguageId = 2`, `OriginalId` = ID bản Việt, dịch tên, mô tả, xuất xứ, chứng nhận, người liên hệ, địa chỉ; chép `SanPhamCNTBCategory`.
- Chỉ tạo khi bản Việt khớp cả ID và tên; bỏ qua sản phẩm đã có bản tiếng Anh nên chạy lại được.
- Phải chạy `sqlcmd` có `-f 65001` (file UTF-8 không BOM).

## Gói

- Gồm `app/TechExchangeApp.dll`, `sql/dich-tieng-anh-san-pham-ocop.sql`, `HUONG_DAN.txt`. Không đổi `appsettings.json`, `wwwroot`.
- Máy chủ phải đang là bản 18/11.
- Thư mục publish: `D:\2026\CanThoPublish_20261120`. So với bản 18/11 chỉ khác `TechExchangeApp.dll`.

## Kiểm tra trên máy local

- Lĩnh vực Điện, điện tử, CNTT (326 sản phẩm): `price=contact` → 325, `0-5` → 1, `20+` → 0; `status=intro` → 326, `ready` → 0; sang trang vẫn giữ bộ lọc.
- `/en/ocop`: 20 sản phẩm (2 trang 12 + 8), trang chi tiết tiếng Anh.

## Còn tồn

- Dòng "Producer" ở trang chi tiết OCOP tiếng Anh vẫn hiện tên nhà cung ứng tiếng Việt (hồ sơ `NhaCungUng` chưa có bản tiếng Anh).
- BUG-001: tham số chọn trường tìm kiếm (`field`) chưa được áp dụng khi có từ khóa.
- Chưa đánh lại chỉ mục tìm kiếm cho 20 bản tiếng Anh.

Câu kiểm tra sau khi chạy script trên máy chủ:

```sql
SELECT COUNT(*) FROM SanPhamCNTB WHERE ProductType = 4 AND LanguageId = 2 AND StatusId = 3; -- 20
```
