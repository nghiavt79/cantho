# Ghi chú cho Claude khi sửa kế hoạch thi công và nghiệm thu đợt 1

## ĐÃ ĐIỀN 26/10/2026 – Sổ phiên bản NCC-14 theo nhân sự MobiFone

- Theo yêu cầu của anh Nghĩa, Codex đã điền sổ phiên bản NCC-14 bằng lịch sử tài liệu và nhân sự trong mục I Kế hoạch triển khai chi tiết, NCC-03.
- Gồm 03 dòng: gói phần mềm 25/10/2026; cập nhật phần mềm 26/10/2026; tài liệu NCC-14 phiên bản 2.0 ngày 26/10/2026.
- Phần mềm: Nguyễn Thị Dung, Nguyễn Nhĩ Thái; kiểm thử: Lê Phước Trung. Tài liệu: Nguyễn Thị Dung; phối hợp hạ tầng/ATTT: Bùi Nguyễn Hoàng Thái.
- Cột ghi “Đầu mối theo phân công”, có chú thích đầu mối bổ sung xác nhận thực hiện và minh chứng khi chốt từng lần cập nhật. Không dùng phân công làm bằng chứng ai đã trực tiếp thao tác.
- Đã đồng bộ cả bản NCC-14 trong `Ho_so_muc_1.1_1.5_1.6/1.5 Tai lieu HDSD, ky thuat, dac ta API, quy trinh van hanh/` và bản trong `Bieu_mau_NCC_TT41/1_Bieu_mau_day_du/`.
- Đã thay đúng NCC-14 trong 03 zip: `26.10.2026 - Ho so muc 1.1, 1.5, 1.6 - Ke hoach trien khai chi tiet.zip`, `Ho_so_muc_1.1_1.5_1.6_26102026.zip`, `26.10.2026 - Bieu mau NCC TT41 - Ban day du.zip`. Các tệp khác trong zip giữ nguyên.
- Khi tạo lại hồ sơ, giữ các dòng này, không đưa sổ về trống. Lượt này chỉ điền NCC-14; ngưỡng 3 giây trong các tài liệu hiệu năng vẫn cần cập nhật riêng theo mục dưới.

## CHỐT 26/10/2026 – Ngưỡng hiệu năng không quá 3 giây

Anh Nghĩa đã chốt trực tiếp: **≤ 3 giây**. Áp dụng ngưỡng này thay cho **≤ 5 giây** đối với chỉ tiêu thời gian tải trang chính đang được trao đổi với khách.

- Cập nhật đồng bộ NCC-11, BM-01 Phụ lục 2, báo cáo tự kiểm tra nội bộ và các bản sao/tin nhắn liên quan. Sau khi sửa file nguồn, đóng gói lại đúng bộ zip gửi khách, tránh gửi lại phiên bản còn ghi 5 giây.
- Giữ nguyên số liệu đo thực tế và cách đo đã thực hiện. Kết quả 1,0–1,9 giây qua hai lần đo chỉ chứng minh các phép đo đó; chưa chứng minh hiệu năng khi nhiều người truy cập đồng thời hoặc kết quả p95.
- Thống nhất kịch bản, tải đồng thời, môi trường, công cụ và cách đánh giá trước khi kiểm thử tải. Không tự đổi giữa “thời gian tải trang” và “thời gian phản hồi p95”; nếu dùng p95 phải định nghĩa rõ trong hồ sơ và báo cáo đúng kết quả đo.
- Chỉ đạo này chốt ngưỡng thời gian 3 giây, không tự thay đổi số người dùng đồng thời hoặc khẳng định đã đạt kiểm thử tải.
- Nếu đo vượt 3 giây, ghi chưa đạt, xử lý nguyên nhân, tối ưu/mở rộng theo phương án được thống nhất và kiểm thử lại; không hạ ngưỡng về 5 giây để kết luận đạt.

Trạng thái cập nhật này: đã ghi quyết định vào Markdown; chưa sửa các file Word hoặc đóng gói lại zip trong lượt chốt ngưỡng này.


## CẬP NHẬT 23/10/2026 – Hồ sơ nghiệm thu theo mẫu của Chủ đầu tư

- Chị Vân gửi mẫu `docs/hop dong/21.10.2026 - Mau BBNT khoi luong cong viec hoan thanh.docx` (mẫu của dự án đầu tư khác). Anh Nghĩa: đây chỉ là mẫu, **phải theo dự thảo hợp đồng; không có đơn vị tư vấn quản lý dự án và đơn vị giám sát** (chỉ Chủ đầu tư + Nhà thầu).
- Thư mục `Ho_so_nghiem_thu_dot1/` (script `_scripts/mk_hs_nghiemthu.py`, dữ liệu Phụ lục 3 ở `_scripts/pl3.json`):
  1. `01_Bien_ban_nghiem_thu_khoi_luong_dot1.docx` – theo mẫu; Phụ lục 01 (khối lượng, giá trị theo Phụ lục 5; VR360 KL đợt 1 = 0); Phụ lục 02 (Bảng 1: 60 chức năng "Đáp ứng"; Bảng 2: use case, khổ ngang).
  2. `02_Cong_van_de_nghi_nghiem_thu_dot1.docx`
  3. `03_Bien_ban_kiem_tra_dieu_kien_trien_khai.docx` (gộp kiểm tra phần mềm trước cài đặt)
  4. `04_Bien_ban_kiem_thu_chuc_nang_dot1.docx` (gộp báo cáo kiểm thử, kịch bản 10 bước, đối chiếu 14 bước)
- Căn cứ không áp dụng (khảo sát, thiết kế chi tiết, giám sát, năng lực nhà thầu) đã bỏ; thay QĐ thiết kế bằng QĐ 669/QĐ-SKHCN.

## CẬP NHẬT 22/10/2026 (2) – gọn biên bản

- Chỉ đạo anh Nghĩa: "không thể nào mà cứ ngày nào cũng có biên bản". Cả đợt chỉ có **1 biên bản nghiệm thu giai đoạn** (cuối đợt, 1 bảng 5 dòng theo Phụ lục 5: hạng mục, giá trị, kết quả, hồ sơ kèm theo) + **bảng xác định giá trị** (Excel 3 sheet: Tổng hợp, Mục 1.1, Mục 1.2).
- Kế hoạch: cột Kết quả ghi kết quả công việc (vd "Gửi được tin nhắn thử"), không ghi biên bản trung gian; bỏ các mục điều kiện, hồ sơ, phối hợp, rủi ro.
- Không đưa bảng 11 nội dung kiểm tra, 10/14 bước, danh sách học viên vào biên bản ký; để riêng khi được hỏi. Bản đầy đủ cũ: `_scripts/mk_bbnt_full_bak.py`, `_scripts/mk_plan_full_bak.py`.
- Đào tạo (1.2.5) đã tổ chức → ghi "Đã thực hiện".

## CẬP NHẬT 22/10/2026 – Phụ lục 5 chị Vân xác nhận (thay các số cũ bên dưới)

- File `docs/hop dong/PHỤ LỤC 5- thanh toan lan 1.docx` (chị Vân gửi 21/10, "Vân xác nhận lại"): **bỏ VR360 khỏi đợt 1**; mục 1.4 còn 1.4.1 – 1.4.5 (tin nhắn thương hiệu 27.500.000; email 8.250.000; token 2.594.602; eContract 39.000.000; gói SMS 11.000.000 = 88.344.602).
- Đợt 1: 1.1 = **890.782.296**; 1.2 = 447.020.320; 1.3 = 157.290.000; 1.4 = 88.344.602; 2 = 693.000.000; **tổng 2.276.437.218**. Không ghi tỷ lệ tổng (không tự tính %).
- VR360 (B.1.2, C.1 và giải pháp 10 điểm cảnh) chuyển **đợt 2**.
- Mục 1.1 đợt 1 = **60 chức năng**, gom theo **12 nhóm chức năng Phụ lục 3**, lịch từng ngày 21–28/10 (28/10 kiểm thử tổng thể); tiền chia theo số chức năng mỗi nhóm. Đợt 2 mục 1.1: B.1.2, C.1, C.2, phần gọi video của C.7, nghiệm thu đưa vào vận hành.
- Dữ liệu dùng chung ở `_scripts/data_dot1.py`; tiến độ có đủ mục 1.3.1–1.3.4, 1.4.1–1.4.5, 2.
- Mẫu biểu: hợp đồng (Điều 5) chỉ yêu cầu "biên bản nghiệm thu giai đoạn" và "bảng xác định giá trị khối lượng", không quy định mẫu; nếu Chủ đầu tư/Kho bạc có mẫu riêng thì chuyển sang mẫu đó.

Cập nhật lần cuối: 21/10/2026 (Claude, sau khi đối chiếu `Ra_soat_Codex/Ra_soat_ho_so_Claude_21102026.md`).

Khách rất khó tính: mọi số liệu phải khớp hợp đồng, không ghi điều chưa có minh chứng là đã xong, văn bản ngắn gọn, không thuật ngữ kỹ thuật.

## 1. Diễn biến chỉ đạo của người phụ trách (anh Nghĩa)

| Thời điểm | Chỉ đạo | Hiệu lực |
| --- | --- | --- |
| 19/10 (với Codex) | Chuyển VR360 sang tháng 11 do chưa bố trí kinh phí; rà soát, cân nhắc chuyển licence và các khoản phải chi tiền sang tháng 11 | Đã thay bằng các dòng dưới |
| 19/10 (với Claude) | “Chỉ bỏ phần VR ra tháng 11 thôi, phần licence vẫn bỏ vào tháng 10” | Đã thay bằng dòng dưới |
| 21/10 (với Claude) | Sau khi có dự thảo hợp đồng: **giữ VR360 trong đợt 1 theo Phụ lục 5, đưa quay dựng VR360 về tháng 10** | **Đang áp dụng** |

Lý do: Phụ lục 5 tính 100% licence, cloud, VR360 và các gói tích hợp vào đợt 1; nếu hoãn thì đợt 1 chỉ còn khoảng 49% giá hợp đồng, không đạt mục tiêu.

Chỉ đạo này **chưa có nghĩa** là đã có kinh phí, đã mua licence/cloud, đã kích hoạt gói dịch vụ hay đã chốt lịch quay. Khi viết cho khách dùng “Để đạt mục tiêu 85%, cần hoàn thành các hạng mục sau trong tháng 10…”, không ghi “đã chốt”.

## 2. Căn cứ hợp đồng (dự thảo 19/10, `docs/hop dong/19.10_Dự thảo HĐ Gói 4_Sàn GD.docx`)

- Giá hợp đồng 2.730.119.729 đồng; trọn gói; thực hiện 30 ngày; dịch vụ 05 năm từ nghiệm thu.
- Thanh toán lần 1 “trước ngày 30/10/2026”, **tối đa 85%**; hồ sơ: biên bản nghiệm thu giai đoạn + bảng xác định giá trị khối lượng. Điều 5.2.2: thanh toán trong 30 ngày kể từ khi nhận đủ hồ sơ → không hứa nhận tiền trong tháng 10.
- Nhà thầu phụ 0% (E-ĐKC 6.2): văn bản gửi khách chỉ ghi MobiFone, **không nhắc VTISC**.
- **Số tiền: làm đúng theo số MobiFone đưa, không tự tính lại hay góp ý chênh lệch** (chỉ đạo anh Nghĩa 21/10: “tiền họ đã tính kỹ rồi, chỉ cần làm đúng theo họ”). Hồ sơ dùng số trong tin nhắn chị Vân 20/10: 1.1 = 890.782.295 (75%); 1.2 = 447.020.320 (80%); cùng 1.3 = 157.290.000; 1.4 = 133.344.602; 2 = 693.000.000; tổng 2.321.437.217, ghi là 85%.
- Không nêu trong file hay tin nhắn các chênh lệch tự tính (85,03%, 835.448 đồng, lệch 1 đồng với Phụ lục 5).

## 3. Vấn đề còn mở

1. **Kinh phí, lịch** cho licence, cloud, gói dịch vụ, VR360 trong tháng 10: chưa có xác nhận.
2. **Mốc nghiệm thu – nộp hồ sơ – thanh toán:** theo Điều 5 hợp đồng, không tự hứa ngày nhận tiền.

## 4. Quy tắc trình bày hồ sơ

- Bộ gửi khách gọn **3 file**: kế hoạch, biên bản nghiệm thu giai đoạn, bảng xác định giá trị (Excel). Không thêm phụ lục, biểu mẫu khác.
- Kế hoạch và biên bản không có cột Trạng thái; trạng thái chỉ theo dõi trong Excel.
- Biên bản ghi rõ **(DỰ THẢO)**. Cột Kết quả để sẵn “Đạt” theo yêu cầu người phụ trách; chỉ ký khi đã kiểm tra thực tế. Không dùng biên bản dự thảo làm bằng chứng đã hoàn thành.
- Excel: đợt 1 ghi “Hoàn tất”, đợt 2 “Chưa hoàn tất” theo yêu cầu người phụ trách. Phải cập nhật theo thực tế trước khi dùng làm hồ sơ chính thức.
- Tỷ trọng chi tiết trong mục 1.1 (35/20/10/5/5) và 1.2 (15/15/15/10/10/15) là **đề xuất để các bên xác nhận**, không phải quy định của hợp đồng; đã ghi chú trong file.
- Không ghi mekongx.vn, số liệu dữ liệu cứng, kết quả pentest nội bộ, lý do thiếu kinh phí.
- Ghi “85%”, không ghi “85,0%”. Author mọi file: NghiaVT. Xưng “anh” – “Dung/em” trong tin nhắn.

## 5. Tiến độ đã sửa theo rà soát Codex

- Kiểm thử nội bộ 21–24/10 trên môi trường hiện có.
- Bàn giao cloud, licence 20–24/10; chuyển ứng dụng, dữ liệu 25–26/10.
- Kiểm tra sẵn sàng trên máy chủ cloud **27/10**; vận hành thử **27–30/10**; VR360 chụp 22–23, dựng 24–28, kiểm tra trên Sàn 29–30/10.
- Nếu cloud chậm: thử trên môi trường hiện có, nhưng hạ tầng và licence (850.290.000 đồng) chưa nghiệm thu được.

## 6. Tệp

| Tệp | Ghi chú |
| --- | --- |
| `Ke_hoach_trien_khai_van_hanh_thu_T10_2026.docx` | Kế hoạch gửi khách |
| `Bien_ban_nghiem_thu_giai_doan_dot1.docx` | Dự thảo biên bản Chủ đầu tư – MobiFone, kèm danh sách học viên |
| `Bang_xac_dinh_gia_tri_khoi_luong_dot1.xlsx` | Tổng hợp đợt 1 + chi tiết mục 1.1, 1.2 |
| `Ke_hoach_va_ho_so_nghiem_thu_dot1_gui_MobiFone.zip` | Bộ 3 file trên |
| `Ghi_chu_ke_hoach_nghiem_thu_dot1.md` | Ghi chú tổng hợp số liệu, việc còn treo |
| `_ban_cu/` | Bộ 12 mẫu cũ, các bản trước – không dùng |
| `Ra_soat_Codex/` | Rà soát nội bộ của Codex – không gửi khách. Bản Word của Codex có Author “python-docx”, nếu dùng phải sửa thành NghiaVT |

Script sinh 3 file nằm ở `_scripts/` (`mk_plan.py`, `mk_bbnt.py` – cần `mk_bo_ho_so.py` cùng thư mục, `mk_dot1.py`); khi sửa thì sửa script, xuất lại cả 3 file rồi đóng gói lại zip.
