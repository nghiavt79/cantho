# Cập nhật mekongx.vn 12/11/2026: nút gian hàng Triển lãm 3D theo số gian hàng

Gói `deploy-mekongx-2026-11-12.zip` (tại `D:\2026\CanTho`). Chưa chép lên máy chủ.

## Lý do

Gói 11/11 hiện nút cho mọi NCC có gian hàng (28 NCC trên mekongx.vn, gồm cả 14 cơ sở thực phẩm, 3 tài khoản bán thử, 2 NCC vận hành thử). Khách cho biết đợt này chỉ có 10 điểm, tức 10 nhà cung ứng mình nhập từ phiếu mẫu (`CSDL Mau/SanphamCNTBMau/NHÀ CUNG ỨNG`, báo cáo `Bao_cao_import_sanpham_nhacungung_20260711.docx`).

Bản đầu của gói 12/11 dùng danh sách mã cố định (appsettings). Đã bỏ, thay bằng thuộc tính có sẵn của triển lãm theo ý người dùng.

## Cách xác định NCC tham gia triển lãm

- Ứng dụng triển lãm (`Trienlam-main`, mã nguồn ở `D:\2026\CanTho\Trienlam-main.zip`) đọc thẳng bảng `NhaCungUng`, thêm các cột `BoothNumber`, `PanoramaUrl`, `BoothSortOrder`, `BoothIndustryId`, `ProductsJson`, `ImageGalleryJson`. Thiết lập gian hàng ở trang quản trị triển lãm là gán số gian hàng (`BoothNumber`).
- Sàn hiện nút "Xem gian hàng triển lãm 3D" khi NCC có `BoothNumber`, đang kích hoạt, `StatusId = 3`. Thêm/bớt NCC: gán hoặc xóa số gian hàng trên trang quản trị triển lãm, không sửa code.
- Bản tiếng Anh xét theo bản tiếng Việt gốc (`OriginalId`) và mở gian hàng của bản tiếng Việt.

## Số gian hàng 9 NCC

Trước 12/11 máy chủ chỉ có A01–A03. Script `TechExchangeApp/Scripts/gan-so-gian-hang-trien-lam-9-ncc.sql` gán A04–A09 cho 6 NCC còn lại (ảnh 360° mặc định `booth_default.png`), không đổi số đã có, chạy lại được.

| Số | Mã | Nhà cung ứng |
|---|---|---|
| A01 | 8904 | Công ty Cổ phần Công nghệ Cát Sạch MeKong (có sẵn) |
| A02 | 8096 | Công ty Cổ phần MISA (có sẵn) |
| A03 | 8887 | Công ty TNHH China Ecotek Việt Nam (có sẵn) |
| A04 | 7225 | Công ty Cổ phần Machinex Việt Nam |
| A05 | 8888 | Công ty Cổ phần Công nghệ KATEC |
| A06 | 8889 | Công ty Cổ phần TPT |
| A07 | 8890 | Công ty Cổ phần Công nghệ Aquadelta |
| A08 | 8891 | Công ty CP XNK Hóa chất và Thiết bị Kim Ngưu |
| A09 | 8892 | Công ty TNHH Phúc Trường Hải |

## Còn thiếu

- NCC thứ 10 là Công ty TNHH 3D Smart Solutions (mã số thuế 0313566379): có phiếu nhưng chưa nhập vào hệ thống (không có trong `sgdcnCanTho`, `TechExchangeNew`, không có trên mekongx.vn). Phiếu ghi website `katec.vn` (trùng KATEC), cần hỏi lại khách trước khi nhập. Nhập xong thì gán số A10 trên trang quản trị triển lãm.

## Kiểm tra local

Đã chạy script trên `TechExchangeNew` (CSDL local chưa có cột `BoothNumber`, `PanoramaUrl` nên script thêm cột). Trang NCC MISA, Machinex, Phúc Trường Hải, sản phẩm MISA eSign, MISA EN (8949), Machinex EN (8950) có nút, link tới gian hàng bản tiếng Việt; Cơ sở bánh tét Chín Cẩm (8873), Vạn Nghĩa (8893) không có nút.

## Gói, mã nguồn

- Gói gồm `app/TechExchangeApp.dll`, `sql/gan-so-gian-hang-trien-lam-9-ncc.sql`, `HUONG_DAN.txt`. Không đổi `appsettings.json`, `wwwroot`.
- Phải chạy script trước hoặc cùng lúc chép dll; nếu chỉ chép dll thì chỉ A01–A03 có nút.
- Thư mục publish: `D:\2026\CanThoPublish_20261112`.
