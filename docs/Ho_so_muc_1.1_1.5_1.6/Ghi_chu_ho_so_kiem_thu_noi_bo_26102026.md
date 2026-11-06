# Hồ sơ kiểm thử nội bộ – ghi chú 26/10/2026

## Bối cảnh

- Chị Tâm (CĐT) yêu cầu bộ tài liệu kiểm thử nội bộ của nhà cung cấp, **độc lập với Gói 6 (giám sát) và Gói 7 (vận hành thử)**: quy trình → kịch bản → báo cáo.
- Vòng góp ý 2 (khách): "người khác cầm vào có thể làm lại bài test và kiểm chứng kết quả" – mỗi bước rõ ai làm / đầu vào / đầu ra / ai xác nhận; thao tác 1–2–3 có màn hình, nút, dữ liệu; kết quả thực tế là điều quan sát được; mỗi ca có mã minh chứng; chưa test ghi "Chưa kiểm thử"; KT-22, 23 trả 302 phải ghi sai lệch; tổng hợp tính từ từng ca; liên kết mã chức năng → ca → minh chứng → lỗi → kiểm tra lại; ngày thật. 14 bước: 2 hồ sơ (có / không NDA) × (thành công, điều kiện chuyển bước, không quyền). API: tách ca từ chối với ca ghi thành công.
- Đã chọn hướng: **biểu mẫu để QA chạy thật** – không điền kết quả khi chưa có minh chứng.

## Bộ tài liệu (thư mục `1.6 Tu kiem tra noi bo, san sang van hanh thu/`)

1. `Quy trinh kiem thu noi bo.docx` (ngày 26/10/2026): Phần A quy định chung (vai trò, 9 bước có người làm / đầu vào / đầu ra / người xác nhận, quy tắc mã và liên kết, quy tắc ghi kết quả – trạng thái, mức độ lỗi, lưu minh chứng); Phần B lịch đợt 1 theo ngày thực tế.
2. `Kich ban kiem thu chi tiet.docx` (ngày 26/10/2026): dữ liệu kiểm thử DL-01, DL-02, HS-NDA, HS-KNDA (ô mã thật để trống); TC-CN-01…60; TC-QT-01a…14c (3 ca × 2 hồ sơ); API V.1 từ chối / V.2 ghi thành công KT-03, KT-31…40 / V.3 không quyền KT-41…43; TC-HT-01…08; Sổ lỗi L-01…L-06; tổng hợp tính tự động.
3. `Bien ban tu kiem thu chuc nang phan mem dot 1.docx`: ngày ký để trống; kết quả mục I–III và Phụ lục A = "Chưa kiểm thử", cột minh chứng → mã ca TC-…; mục IV gắn L-01…L-05.
4. `Bao cao tu kiem tra noi bo, san sang van hanh thu lan 1.docx`: mục II gắn mã minh chứng MC-HT/MC-KT, IPv6 Không đạt L-06, log Chưa kiểm thử, 60 chức năng Chưa kiểm thử; mục III gắn mã lỗi; mục 1.5 thêm Quy trình, Kịch bản.
5. `Minh chung/`: 7 nhật ký hạ tầng + 11 nhật ký API ẩn danh chạy thật ngày 26/10/2026 (curl/openssl); `chay_kiem_thu_api.ps1` chạy ca API có đăng nhập, tự ghi MC-KT-xx-<ngày>-<tài khoản>.txt + CSV tổng hợp.
6. PL-03 (thư mục 1.5): KT-22, 23 không đạt 25/10 (L-03) – kiểm tra lại đạt 26/10; KT-08, 09, 17, 24 kiểm tra lại đạt (L-02); Bảng 5: Đạt 13, Không đạt 0.

## Tổng hợp tại 26/10/2026 (tính từ từng ca, sau kiểm tra lại L-02, L-03)

| Phần | Số ca | Đạt | Đạt một phần | Không đạt | Chưa kiểm thử | Không áp dụng |
|---|---|---|---|---|---|---|
| Chức năng | 60 | 0 | 0 | 0 | 60 | 0 |
| 14 bước (ca × hồ sơ) | 84 | 0 | 0 | 0 | 81 | 3 |
| API | 44 | 14 | 1 | 0 | 29 | 0 |
| Hạ tầng | 8 | 5 | 1 | 1 | 1 | 0 |
| Cộng | 196 | 19 | 2 | 1 | 171 | 3 |

## Sổ lỗi

| Mã | Nội dung | Trạng thái |
|---|---|---|
| L-01 | API-05 lỗi 500 | Đã đóng – MC-KT-07-20261026 |
| L-02 | 05 API không kiểm tra mã chống giả mạo | Đã đóng – 26/10, vht_buyer01, MC-KT-08/09/17/24-20261026-vht_buyer01 |
| L-03 | API thông báo trả 302 thay vì 401 | Đã đóng – 26/10, MC-KT-22/23-20261026-2 |
| L-04 | Chưa kiểm thử luồng ghi thành công 10 API | Mở – chạy script 27–28/10 |
| L-05 | Kênh email tắt | Mở – trước 28/10 |
| L-06 | Chưa có bản ghi IPv6 | Mở – trước 09/11 |

## Chạy test thật 26/10 (dừng giữa chừng)

- Đã đăng nhập được vht_seller01, vht_admin trên mekongx.vn. vht_seller01 **chưa có hồ sơ nhà cung ứng** → form Đăng ký Công nghệ mới không có nhà cung ứng để chọn; đã thêm DL-00 vào Kịch bản mục II. Chưa gửi form nhà cung ứng nào.
- CMS › Nhà cung ứng có 3 hồ sơ Nháp chứa chuỗi tấn công mẫu (SQL injection, bxss.me) – dữ liệu rác từ đợt quét bảo mật, nên dọn trước nghiệm thu.
- Người dùng quyết định để phần chạy test, chụp ảnh sau; rà nhanh trước khi gửi: sửa Báo cáo 1.3 (cột "Phạm vi đã kiểm thử" → Chưa kiểm thử + mã ca), 1.4 (chỉ 2 tài khoản đã đăng nhập thử), Biên bản nhóm 9 (C.4 chưa kiểm thử).

## Việc tiếp theo

- **Chạy test thật trên mekongx.vn** bằng trình duyệt trong app: người dùng tự đăng nhập từng vai (vht_admin, buyer01, seller01, seller02, expert01), Claude thao tác, chụp ảnh, ghi mã hồ sơ thật vào Kịch bản. Thứ tự: mục II dữ liệu → TC-QT trên HS-NDA, HS-KNDA → TC-CN.
- Chạy `chay_kiem_thu_api.ps1` (cần Cookie đăng nhập; Claude chưa chạy được vì bị chặn quyền ExecutionPolicy Bypass – script mới kiểm tra cú pháp).
- Đo TC-HT-01 thêm khung trưa, tối; chụp thư mục logs máy chủ (TC-HT-07).
- Rủi ro đã thấy trong code: `ProjectController.UpdateStepData` chỉ kiểm tra là thành viên hồ sơ, không kiểm tra vai trò → nhiều ca TC-QT-xxc (không quyền giữa bên mua / bên bán) có thể Không đạt.
- Xác nhận tên người chạy ngày 26/10 ghi trong hồ sơ (đã đổi thành "Nhóm kỹ thuật MobiFone Cần Thơ") và người kiểm thử PL-03 ngày 25/10 ("Nhóm kỹ thuật").
- Còn trống chờ MobiFone: số công văn Báo cáo, người phê duyệt NCC-14, kiểm tra lại ATTT.
- Zip `Ho_so_muc_1.1_1.5_1.6_phan_hoi_Chi_Quyen.zip` là bản cũ, chưa đóng gói lại.

## Sinh lại tài liệu

Script trong `_scripts/` (python-docx; Biên bản dùng làm mẫu định dạng): `kb2_data.py` (dữ liệu ca), `kb2_quytrinh.py`, `kb2_kichban.py`, `kb_gen.py`, `dx.py`, `kb_data.py`, `pla.json`. Đường dẫn trong script đang trỏ `%TEMP%` – sửa trước khi chạy lại. Sinh lại Kịch bản sẽ ghi đè kết quả đã điền tay → sau khi bắt đầu ghi kết quả thì sửa trực tiếp trong Word.
