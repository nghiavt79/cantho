# Cập nhật mekongx.vn 12/11/2026: nút gian hàng Triển lãm 3D chỉ còn 9 NCC

Gói `deploy-mekongx-2026-11-12.zip` (tại `D:\2026\CanTho`). Chưa chép lên máy chủ.

## Lý do

Gói 11/11 hiện nút cho mọi NCC có gian hàng (28 NCC trên mekongx.vn, gồm cả 14 cơ sở thực phẩm, 3 tài khoản bán thử, 2 NCC vận hành thử). Khách cho biết đợt này chỉ có 10 điểm, tức 10 nhà cung ứng mình nhập từ phiếu mẫu (`CSDL Mau/SanphamCNTBMau/NHÀ CUNG ỨNG`, báo cáo `Bao_cao_import_sanpham_nhacungung_20260711.docx`).

## Nội dung

Nút chỉ hiện cho 9 NCC:

| Mã | Nhà cung ứng |
|---|---|
| 8096 | Công ty Cổ phần MISA (gian A02) |
| 7225 | Công ty Cổ phần Machinex Việt Nam |
| 8887 | Công ty TNHH China Ecotek Việt Nam (gian A03) |
| 8888 | Công ty Cổ phần Công nghệ KATEC |
| 8889 | Công ty Cổ phần TPT |
| 8890 | Công ty Cổ phần Công nghệ Aquadelta |
| 8891 | Công ty CP XNK Hóa chất và Thiết bị Kim Ngưu |
| 8892 | Công ty TNHH Phúc Trường Hải |
| 8904 | Công ty Cổ phần Công nghệ Cát Sạch MeKong (gian A01) |

- Bản tiếng Anh xét theo mã bản tiếng Việt gốc (`OriginalId`), ví dụ MISA EN 8949 vẫn có nút.
- Vẫn giữ điều kiện cũ (kích hoạt, `StatusId = 3`, có lĩnh vực) để không dẫn tới trang 404.
- Danh sách mặc định nằm trong `Helpers/ExhibitionHelper.cs`. Đổi danh sách không cần build lại: thêm `"Exhibition": { "BoothSupplierIds": [ ... ] }` vào `appsettings.json` trên máy chủ rồi chạy lại app pool.

## Còn thiếu

- NCC thứ 10 là Công ty TNHH 3D Smart Solutions (mã số thuế 0313566379): có phiếu nhưng chưa nhập vào hệ thống (không có trong `sgdcnCanTho`, `TechExchangeNew`, không có trên mekongx.vn). Phiếu ghi website `katec.vn` (trùng KATEC), cần hỏi lại khách trước khi nhập. Người dùng chọn tạm giới hạn 9 NCC.

## Kiểm tra local

MISA (8096), sản phẩm MISA eSign (35394), MISA EN (8949), Cát Sạch (8904), KATEC (8888) có nút; Cơ sở bánh tét Chín Cẩm (8873) không có nút.

## Gói, mã nguồn

- Gói chỉ gồm `app/TechExchangeApp.dll` và `HUONG_DAN.txt`; không SQL, không đổi `appsettings.json`, `wwwroot`. Thay đổi so với gói 11/11 chỉ ở `Helpers/ExhibitionHelper.cs` và `Program.cs`.
- Thư mục publish: `D:\2026\CanThoPublish_20261112`.
