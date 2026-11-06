# Kế hoạch triển khai và nghiệm thu đợt 1 – Gói thầu số 4


## CẬP NHẬT 22/10/2026 – Phụ lục 5 chị Vân xác nhận (thay các số cũ bên dưới)

- File `docs/hop dong/PHỤ LỤC 5- thanh toan lan 1.docx` (chị Vân gửi 21/10, "Vân xác nhận lại"): **bỏ VR360 khỏi đợt 1**; mục 1.4 còn 1.4.1 – 1.4.5 (tin nhắn thương hiệu 27.500.000; email 8.250.000; token 2.594.602; eContract 39.000.000; gói SMS 11.000.000 = 88.344.602).
- Đợt 1: 1.1 = **890.782.296**; 1.2 = 447.020.320; 1.3 = 157.290.000; 1.4 = 88.344.602; 2 = 693.000.000; **tổng 2.276.437.218**. Không ghi tỷ lệ tổng (không tự tính %).
- VR360 (B.1.2, C.1 và giải pháp 10 điểm cảnh) chuyển **đợt 2**.
- Mục 1.1 đợt 1 = **60 chức năng**, gom theo **12 nhóm chức năng Phụ lục 3**, lịch từng ngày 21–28/10 (28/10 kiểm thử tổng thể); tiền chia theo số chức năng mỗi nhóm. Đợt 2 mục 1.1: B.1.2, C.1, C.2, phần gọi video của C.7, nghiệm thu đưa vào vận hành.
- Dữ liệu dùng chung ở `_scripts/data_dot1.py`; tiến độ có đủ mục 1.3.1–1.3.4, 1.4.1–1.4.5, 2.
- Mẫu biểu: hợp đồng (Điều 5) chỉ yêu cầu "biên bản nghiệm thu giai đoạn" và "bảng xác định giá trị khối lượng", không quy định mẫu; nếu Chủ đầu tư/Kho bạc có mẫu riêng thì chuyển sang mẫu đó.

Cập nhật: 21/10/2026

## Yêu cầu

- 19/10/2026: Chủ đầu tư yêu cầu thanh toán lần 1 trong tháng 10, bằng 85% khối lượng. MobiFone nhờ anh Nghĩa lập kế hoạch thi công, vận hành thử; Dung (MobiFone) hỗ trợ xem.
- 20/10/2026: chị Vân (MobiFone) gửi dự thảo hợp đồng `docs/hop dong/19.10_Dự thảo HĐ Gói 4_Sàn GD.docx` và yêu cầu liệt kê công việc nghiệm thu đợt 1 theo giá trị mục 1.1 và 1.2.

## Căn cứ hợp đồng (dự thảo 19/10)

- Giá hợp đồng: 2.730.119.729 đồng, trọn gói. Thời gian thực hiện 30 ngày; dịch vụ 05 năm kể từ nghiệm thu.
- Thanh toán 02 lần. Lần 1 trước 30/10/2026, tối đa 85%; hồ sơ gồm **biên bản nghiệm thu giai đoạn** và **bảng xác định giá trị khối lượng**.
- Nhà thầu phụ 0% (E-ĐKC 6.2), vì vậy văn bản gửi khách chỉ ghi MobiFone, không nhắc VTISC.

### Giá trị đợt 1 (theo số MobiFone, sau thuế)

| Mục | Nội dung | Giá hợp đồng | Đợt 1 | Tỷ lệ |
| --- | --- | ---: | ---: | ---: |
| 1.1 | Thiết lập trang web Sàn | 1.187.709.727 | 890.782.295 | 75% |
| 1.2 | Cài đặt, cấu hình, hướng dẫn, vận hành, bảo hành, bảo trì, hỗ trợ 05 năm | 558.775.400 | 447.020.320 | 80% |
| 1.3 | Bản quyền Windows Server 2025, SQL Server 2025 | 157.290.000 | 157.290.000 | 100% |
| 1.4 | VR360, SMS, email, token ký số, hợp đồng điện tử | 133.344.602 | 133.344.602 | 100% |
| 2 | Hạ tầng 02 máy chủ cloud | 693.000.000 | 693.000.000 | 100% |
| | **Cộng** | **2.730.119.729** | **2.321.437.217** | **85%** |

Làm đúng theo số MobiFone đưa (tin nhắn chị Vân 20/10), không tự tính lại hay nêu chênh lệch – chỉ đạo anh Nghĩa 21/10.

### Phân bổ mục 1.1 và 1.2

Phụ lục 3 có 63 chức năng (A 37, B 16, C 10). Đợt 1 nghiệm thu 62 (C.7 Liên hệ trực tiếp: phần chat ở đợt 1, phần gọi video ở đợt 2; C.2 ở đợt 2). Biên bản có bảng đối chiếu 14 bước quy trình với 10 bước kiểm thử (phần V Chương V): kịch bản 10 bước phủ bước 1, 3, 4, 5, 7; bước 2, 6, 8–14 kiểm thử bổ sung.

| Mục | Đợt 1 | Đợt 2 |
| --- | --- | --- |
| 1.1 | A. Website quản lý 37 chức năng (35%); B. Website bán hàng 15 chức năng (20%); C. Tiện ích 7 chức năng (10%); C.4 ký số hợp đồng (5%); VR360 trên giao diện (5%) = **75%** | Kết nối Sàn KH&CN Việt Nam và sàn khác (7%); gọi video (3%); kiểm thử toàn bộ, nghiệm thu đưa vào vận hành (15%) = 25% |
| 1.2 | Cài đặt (15%); cấu hình (15%); tích hợp (15%); dữ liệu khởi tạo (10%); hướng dẫn, đào tạo (10%); thiết lập vận hành, vận hành thử (15%) = **80%** | Vận hành, bảo hành, bảo trì, hỗ trợ 05 năm (15%); nghiệm thu, bàn giao hồ sơ (5%) = 20% |

## Điều kiện để đạt mục tiêu đợt 1 và các nội dung hồ sơ

**Làm rõ ngày 21/10/2026:** Danh sách VR360, licence, cloud và dịch vụ cần hoàn thành trong tháng 10 dưới đây là điều kiện để đạt gần 85% theo cơ cấu dự thảo, không phải xác nhận người phụ trách đã chốt kinh phí, mua sắm hoặc lịch quay. Trước đó người phụ trách đề xuất hoãn các khoản chưa có kinh phí sang tháng 11; nếu vẫn hoãn thì phải tính giá trị nghiệm thu đợt 1 thấp hơn và trao đổi lại với MobiFone. Khi gửi tin nhắn dùng “Để đạt mục tiêu 85%, cần hoàn thành...”, không ghi “đã chốt” khi chưa xác nhận. Đọc thêm `GHI_CHU_CHO_CLAUDE.md` để áp dụng cách hiểu mới nhất.


- VR360 làm trong **tháng 10** (Phụ lục 5 tính 100% vào đợt 1): chụp 22–23/10, dựng 24–28/10, bàn giao 29–30/10.
- Bản quyền, cloud, các gói dịch vụ tích hợp bàn giao/kích hoạt trong tháng 10.
- Không ghi tên mekongx.vn, số liệu dữ liệu cứng (4.353 / 1.808 / 84) hay kết quả pentest nội bộ trong văn bản gửi khách; số lượng dữ liệu chốt tại biên bản.
- Gom bộ hồ sơ gọn nhất: chỉ 3 file. Trạng thái chỉ theo dõi trong Excel; biên bản và kế hoạch không có cột Trạng thái. Biên bản ghi Kết quả "Đạt".
- Ghi 85% (không ghi 85,0%). Author mọi file là NghiaVT.
- Đào tạo, kiểm thử quy trình đã làm trước (kịch bản đào tạo, 9 tài khoản thực hành team anh Duy, file `Kich_ban_kiem_thu_San_giao_dich_cong_nghe_tested.docx` đã gửi Dung) – cần xin lại file kết quả test.

## Tệp

| Tệp | Nội dung |
| --- | --- |
| `Ke_hoach_trien_khai_van_hanh_thu_T10_2026.docx` | Kế hoạch: mục tiêu, điều kiện, tiến độ 20–30/10, giá trị đợt 1, hồ sơ, phối hợp, rủi ro |
| `Bien_ban_nghiem_thu_giai_doan_dot1.docx` | Dự thảo biên bản nghiệm thu giai đoạn giữa Chủ đầu tư và MobiFone, kèm danh sách học viên |
| `_scripts/` | Script Python sinh 3 file (sửa script rồi chạy lại, Author NghiaVT) |
| `Bang_xac_dinh_gia_tri_khoi_luong_dot1.xlsx` | Tổng hợp đợt 1 + chi tiết công việc mục 1.1, 1.2; cột Trạng thái (đợt 1 Hoàn tất, đợt 2 Chưa hoàn tất) |
| `Ke_hoach_va_ho_so_nghiem_thu_dot1_gui_MobiFone.zip` | Bộ 3 file trên để gửi MobiFone |
| `_ban_cu/` | Bộ 12 mẫu biên bản cũ và các bản trước, không dùng nữa |
| `Ra_soat_Codex/`, `GHI_CHU_CHO_CLAUDE.md` | Rà soát nội bộ của Codex, không gửi khách |

## Việc còn treo

- MobiFone chốt ngày bàn giao 02 máy chủ cloud và chứng từ bản quyền (chiếm 850.290.000 đồng của đợt 1).
- Kích hoạt gói SMS, email, token ký số, hợp đồng điện tử; đăng ký thương hiệu SMS.
- Quay dựng 10 điểm VR360 trước 30/10 (chụp 22–23, dựng 24–28, kiểm tra 29–30/10).
- Vận hành thử trên máy chủ cloud 27–30/10, sau khi chuyển ứng dụng, dữ liệu 25–26/10.
- MobiFone xác nhận mốc nghiệm thu, nộp hồ sơ, thanh toán (Điều 5 hợp đồng).
- Điền số liệu thực tế vào biên bản (số chứng từ bản quyền, số lượng dữ liệu, thời gian vận hành thử, danh sách học viên).
- Xin lại file kết quả kiểm thử quy trình từ Dung.
