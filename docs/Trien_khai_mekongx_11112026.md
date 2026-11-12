# Cập nhật mekongx.vn 11/11/2026: nút sang gian hàng Triển lãm 3D

Gói `deploy-mekongx-2026-11-11.zip` (tại `D:\2026\CanTho`). **Đã chép lên máy chủ**, kiểm tra trên mekongx.vn 12/11: 28 NCC có nút, cả 28 mở được gian hàng. Ngày 12/11 giới hạn còn 9 NCC, xem `Trien_khai_mekongx_12112026.md`.

## Nội dung

| Mục | Trước | Sau |
|---|---|---|
| Trang chi tiết nhà cung ứng | Không có đường sang Triển lãm 3D | Nút "Xem gian hàng triển lãm 3D" dưới bảng liên hệ |
| Trang chi tiết sản phẩm | Không có đường sang Triển lãm 3D | Nút "Xem gian hàng triển lãm 3D của NCC" ở cột phải (khối Quan tâm / hình thức trưng bày / QR) |

- Nút mở tab mới tới `https://mekongx.vn/trienlam/Exhibition/Booth/{CungUngId}`. Trang tiếng Anh hiện "View 3D exhibition booth", dùng mã của bản NCC tiếng Anh (triển lãm cũng có gian hàng cho bản này, ví dụ 8949 = MISA EN).
- Triển lãm `/trienlam` là ứng dụng riêng; mã gian hàng trùng `NhaCungUng.CungUngId`. Triển lãm chỉ dựng gian hàng cho NCC đang kích hoạt, `StatusId = 3` và có `LinhVucId`; NCC khác mở ra 404. Nút áp dụng đúng điều kiện này (`Helpers/ExhibitionHelper.cs`) để không dẫn tới trang lỗi. Đã đối chiếu trên mekongx.vn 11/11: 8096, 8888 có gian hàng; 8901 (chưa kích hoạt) 404.
- Chữ trên nút viết thẳng trong view (vi/en), không thêm khóa vào bảng dịch, không có SQL.

## Chưa làm

- Chiều ngược lại (từ gian hàng về trang NCC, sản phẩm trên Sàn) phải sửa trong ứng dụng triển lãm. Gian hàng hiện chỉ có link về trang chủ mekongx.vn và website riêng của công ty; danh sách sản phẩm của gian hàng đang rỗng.

## Kiểm tra

- Local: trang NCC MISA (8096), sản phẩm MISA eSign (35394), MISA tiếng Anh (8949) có nút, link đúng; NCC 8893 (chưa khai lĩnh vực) không có nút. Ở local bấm nút ra lỗi vì không chạy triển lãm.
- Sau khi chép lên máy chủ: kiểm tra như mục "Kiem tra sau cap nhat" trong `HUONG_DAN.txt`, bấm nút phải mở được gian hàng A02 (MISA).

## Gói, mã nguồn

- Gói chỉ gồm `app/TechExchangeApp.dll` và `HUONG_DAN.txt`; không SQL, không đổi `appsettings.json`, `wwwroot`. Thay đổi so với bản chạy trên máy chủ (master `1e6c038`, gói 10/11 số 2) chỉ là code C#/Razor nên chỉ khác `TechExchangeApp.dll`.
- Build từ master `df8c072` (nhánh `claude/exhibition-link-product-supplier-9832bd` gộp fast-forward vào master).
- Thư mục publish: `D:\2026\CanThoPublish_20261111`.
