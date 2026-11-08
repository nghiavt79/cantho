# Bộ dữ liệu demo — Dự án Cát Sạch MeKong (14 bước)

Dữ liệu để nhập tay qua giao diện 14 bước quy trình giao dịch, cho công nghệ
**"Hệ thống và công nghệ tuyển rửa, xử lý cát sạch (cát biển, cát sông, cát nhiễm mặn)"**
của **Công ty Cổ phần Công nghệ Cát Sạch MeKong**.

## Các nhân vật (nhất quán xuyên suốt 14 bước)

| Vai | Tổ chức | Người đại diện | Điện thoại | Email | Tài khoản |
|---|---|---|---|---|---|
| **Bên mua** (Role 1) | Công ty CP Vật liệu Xây dựng Tây Đô | Nguyễn Thị Dung – Giám đốc | 0939700303 | nguyenthidung.mobifone@gmail.com | `nguoimua` (user 72) |
| **Bên bán** (Role 2) | Công ty CP Công nghệ Cát Sạch MeKong | Mai Phương Trang – Giám đốc | 0939803803 | catdasachct@gmail.com | `catsachmekong` (user 23336) |
| **Sàn / Tư vấn** (Role 3) | Trung tâm Thông tin, Thống kê và Ứng dụng tiến bộ KH&CN | — | — | — | `tuvanvien` (user 18973) |

## Các con số chốt (dùng nhất quán)

- Giá trị hợp đồng: **4.500.000.000 VNĐ**
- Tạm ứng 30%: **1.350.000.000 VNĐ**
- Thời gian triển khai: **5 tháng** — Bảo hành: **24 tháng**
- Công suất: **150 m³/giờ** (mở rộng đến 1.000 m³/giờ)
- Chất lượng cát thành phẩm: tạp chất hữu cơ **< 1%**, ion Cl⁻ **< 0,01%**
- Tiêu chuẩn: **TCVN 13754:2023**, **TCCS 49:2025/CĐBVN**, **TCVN 9436:2012**
- 2 bằng độc quyền sáng chế: **35015** (cấp 17/02/2023), **42793** (cấp 26/12/2024)

## Cách dùng

- Mỗi file `B1..B14` là 1 bước. Trong file, mỗi mục là **Nhãn field → Giá trị** để copy-paste.
- Field ghi `[TẢI FILE]` là ô upload tệp — chuẩn bị file rồi tải lên, không paste được.
- Ô ghi `(tự sinh)` hệ thống tự điền, không cần nhập.
- Cột "Đăng nhập bằng" cho biết nên đăng nhập tài khoản nào để thao tác bước đó.
- Bước 5, 7 có **ký OTP**: hệ đang ở TestMode nên **gõ 6 chữ số bất kỳ** (vd `123456`) là qua.

> Dữ liệu đã seed sẵn trong DB (Project 1035) nhưng một số field để trống. Bộ file
> này chứa **đủ mọi field** để nhập lại đầy đủ nếu cần làm sạch/nhập tay.

## File đính kèm để upload (thư mục `uploads/`)

Các file PDF (thể thức văn bản VN, có sẵn nội dung) để tải lên ở đúng bước:

| Bước | File đính kèm | Dùng cho ô |
|---|---|---|
| B6/B7 | `uploads/B6_HopDong_ChuyenGiaoCongNghe.(html/pdf/docx)` | Hợp đồng chuyển giao (soạn theo mẫu chuẩn – 17 điều) |
| B4 | `uploads/B4_HoSoNangLuc_CatSachMeKong.pdf` | Hồ sơ năng lực đính kèm |
| B5 | `uploads/B5_BienBanThuongLuong.pdf` | Biên bản thương lượng |
| B8 | `uploads/B8_UyNhiemChi_TamUng30.pdf` | Chứng từ chuyển tiền |
| B9 | `uploads/B9_BaoCaoChayThu_Pilot.pdf` | File kết quả / báo cáo |
| B11 | `uploads/B11_BienBanDaoTao.pdf` | Biên bản đào tạo |
| B12 | `uploads/B12_TaiLieuKyThuat_HuongDanVanHanh.pdf` | Tài liệu kỹ thuật / HD sử dụng |
| B13 | `uploads/B13_BienBanNghiemThu.pdf` | Biên bản nghiệm thu |
| B14 | `uploads/B14_BienBanThanhLy.pdf` | Biên bản thanh lý / hóa đơn |

Mỗi văn bản có **2 định dạng** trong `uploads/`: **`.pdf`** (để upload/nộp) và **`.docx`** (để chỉnh sửa tay trước khi dùng). Tên file giống nhau, chỉ khác đuôi.

> Video hướng dẫn (B11) và Source code/Database (B12) không tạo được dạng tài liệu — bỏ trống hoặc dùng file thật nếu có.
