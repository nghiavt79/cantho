# Kịch bản test thật eContract trên mekongx.vn

Lập ngày 07/11/2026. Tài liệu nội bộ, không gửi khách.

## 1. Phạm vi

Kịch bản dùng dữ liệu, tài khoản trên mekongx.vn và eContract Production (`mobifone-econtract.vn`, org 13592). Token thật tạo chữ ký thật, mỗi hợp đồng tốn 1 lượt trong gói.

Sàn trên máy chủ chưa có màn hình gọi eContract, nên kịch bản chia 2 phần:

| Phần | Nội dung | Khi nào chạy |
|---|---|---|
| A | Chuỗi Bước 1–6 trên mekongx.vn, lấy hợp đồng đó ký thật trên eContract. Gọi API bằng script | Sau khi đạt mục 3 |
| B | Sàn tự phát hành, đồng bộ, tải PDF | Sau khi giai đoạn 2–4 được cập nhật lên máy chủ |

**Đạt Phần A chưa phải đã tích hợp xong.** Chỉ đạt Phần B mới báo khách được.

## 2. Thời điểm

Vận hành thử lần 2 diễn ra 09–13/11/2026, Bình Định dùng chính các tài khoản `vht_*`. Anh đã chốt chạy Phần A **trước 09/11**.

| Thời điểm | Việc |
|---|---|
| 07/11 | Kiểm tra mục 3, xử lý điều kiện chưa đạt; chuẩn bị tài khoản API, hộp thư, token |
| 07–08/11 | MX-1, MX-2, MX-3. Không đạt MX-3 thì dừng, chưa làm tiếp |
| 08/11 | MX-4, MX-5 nếu còn thời gian; phần chưa kịp để sau 13/11 |
| Tối 08/11 | Dọn dữ liệu theo mục 7, kiểm tra `/health` và trang hồ sơ của `vht_*` trước đợt vận hành thử |

## 3. Điều kiện trước khi chạy

Chỉ đọc, không sửa dữ liệu máy chủ. Ghi kết quả vào mục 8.

| # | Kiểm tra | Yêu cầu | Tình trạng đã biết |
|---|---|---|---|
| 1 | `https://mekongx.vn/health` | Healthy | Đạt 07/11 |
| 2 | Ngày của `TechExchangeApp.dll` trong `D:\www\sgdcn` | Bản 05/11 trở lên | Chưa xác nhận |
| 3 | `Workflow:VisibleStepCount` trong appsettings máy chủ | 14 | Chưa xác nhận, nghi là 7 |
| 4 | Chỉ mục `UX_ProjectContracts_EContractId`, `UX_ProjectContracts_EContractNo` | Có đủ | Lần chạy script báo lỗi tạo chỉ mục |
| 5 | Nhà cung ứng của `vht_seller01` | `StatusId=3`, `IsActivated=1` | Trước đây `IsActivated=0` (L2-07) |
| 6 | `vht_expert01` | Có hồ sơ tư vấn đang kích hoạt | Trước đây chưa có (L2-11) |
| 7 | Sản phẩm xuất bản của `vht_seller01` | Có, mở được trang chi tiết | Tìm kiếm đang trống vì thiếu Full-Text Search, phải mở bằng đường dẫn trực tiếp |
| 8 | Giờ máy chủ | Lệch dưới 1 phút | Anh báo đã đồng bộ, chưa có số đo |
| 9 | Từ máy chủ ra eContract: `Test-NetConnection mobifone-econtract.vn -Port 443` | Thành công | Chưa kiểm tra, máy chủ mới 203.205.1.9 |
| 10 | Mật khẩu `vht_buyer01`, `vht_seller01`, `vht_expert01` | Đăng nhập được | Bình Định có thể đã đổi; xin lại, không tự đặt lại |

Mục 2, 3, 8, 9 chạy `D:\2026\BangChung_eContract\cong_cu\kiem_tra_may_chu.ps1` trên máy chủ. Mục 4–7 kiểm tra bằng câu lệnh sau trên CSDL máy chủ:

```sql
SET NOCOUNT ON;
SELECT name FROM sys.indexes
 WHERE object_id = OBJECT_ID('dbo.ProjectContracts') AND name LIKE 'UX_ProjectContracts_%';
SELECT c.CungUngId, u.UserName, c.StatusId, c.IsActivated FROM NhaCungUng c
  JOIN Users u ON u.UserId = c.UserId WHERE u.UserName = 'vht_seller01';
SELECT n.TuVanId, n.IsActivated, n.StatusId FROM NhaTuVan n
  JOIN Users u ON u.UserId = n.UserId WHERE u.UserName = 'vht_expert01';
SELECT ID, Code, StatusId FROM SanPhamCNTB WHERE Creator = 'vht_seller01' ORDER BY ID DESC;
SELECT Name, CASE WHEN Name LIKE '%PASSWORD%' THEN '(che)' ELSE Val END AS Val
  FROM SYS_PARAMETERS WHERE Name LIKE 'ECONTRACT%';
```

Anh cần chuẩn bị:

- Tài khoản API eContract (tự nhập khi chạy, không ghi vào tài liệu).
- 2 hộp thư thật do anh quản lý. Không dùng email `@example.com`, số `09020000xx` của tài khoản `vht_*`.
- USB Token và MST/CCCD trên chứng thư.

## 4. Quy định khi chạy

- Mọi tên hồ sơ, hợp đồng bắt đầu bằng `THỬ NGHIỆM`; số hợp đồng trên eContract dạng `THUNGHIEM-MX-xx`.
- Thông tin Bên B ở Bước 1 nhập nhãn thử nghiệm, không dùng tên, số điện thoại của đơn vị có thật.
- Không chạy Bước 7 của Sàn: đó vẫn là luồng ký cũ.
- Không nhập `ECONTRACT_PASSWORD` lên máy chủ, giữ `ECONTRACT_ENABLED=0` trong Phần A.
- Gặp mục "Dừng" thì xử lý xong mới chạy tiếp.
- Bằng chứng lưu tại `D:\2026\BangChung_eContract\Mekongx`, ngoài repo.

## 5. Phần A

### MX-1. Chuỗi Bước 1–6 trên Sàn

| Bước | Tài khoản | Thao tác | Kết quả đúng |
|---|---|---|---|
| 1 | `vht_buyer01` | Mở sản phẩm của `vht_seller01`, gửi yêu cầu chuyển giao, tên có `THỬ NGHIỆM` | Tạo hồ sơ, ghi lại Id hồ sơ |
| 2 | `vht_buyer01`, `vht_seller01` | Ký NDA nếu sản phẩm yêu cầu | Xong Bước 2, hoặc "Không áp dụng" |
| 3 | `vht_buyer01` | Gửi yêu cầu báo giá, mời `vht_seller01` | Tìm thấy nhà cung ứng, bên bán nhận được lời mời |
| 4 | `vht_seller01` | Gửi đề xuất | Xong Bước 4 |
| 5 | `vht_buyer01`, `vht_seller01` | Đàm phán, chốt, chọn `vht_seller01` | Xong Bước 5, hồ sơ có bên bán được chọn |
| 6 | `vht_buyer01` | Thêm `vht_expert01` làm tư vấn | Tư vấn mở được trang hợp đồng |
| 7 | `vht_seller02`, `vht_buyer02` | Mở `/Contract/Index?projectId=<Id>` | Bị từ chối truy cập |
| 8 | `vht_seller01` | Tạo hợp đồng từ mẫu | Bên A là bên bán, Bên B là bên mua |
| 9 | Cả 3 bên | Lần lượt duyệt trên giao diện | Đủ 3 bên duyệt |
| 10 | `vht_seller01` | Thử chốt | Bị từ chối, chỉ bên mua được chốt |
| 11 | `vht_buyer01` | Chốt "Sẵn sàng ký" | Trạng thái Sẵn sàng ký, xong Bước 6 |
| 12 | `gs_theodoi` | Xem nhật ký hồ sơ | Có các lần chuyển bước, không sửa được |

**Dừng** nếu không đến được bước 11: đây là lỗi chuỗi lõi, cần xử lý trước đợt vận hành thử.

Lỗi đã biết sẽ gặp: nút tải PDF trả 404 với hợp đồng tạo từ mẫu. Ghi nhận, làm tiếp theo MX-2.

### MX-2. Lấy nội dung hợp đồng thành PDF

Máy chủ chưa tạo được PDF nên làm thủ công, trên máy anh, với nội dung lấy từ máy chủ:

1. Trên máy chủ, xuất nội dung hợp đồng (thay `<CSDL>`, `<IdHopDong>`, rồi chép file về máy):

```bash
bcp "SELECT HtmlContent FROM dbo.ProjectContracts WHERE Id = <IdHopDong>" queryout contract.html -c -C 65001 -T -d <CSDL>
```

2. Trên máy anh, tạo PDF bằng đúng dịch vụ của Sàn:

```bash
dotnet "D:\2026\BangChung_eContract\cong_cu\pdftool\pdftool.dll" contract.html "D:\2026\CanTho\TechExchangeApp\wwwroot" hop_dong.pdf xem_o_ky.pdf
```

3. Công cụ in trang, x, y của Bên A, Bên B. Mở `xem_o_ky.pdf` kiểm tra khung đỏ.

Kết quả đúng: PDF đủ nội dung, tiếng Việt đúng, có đủ 2 dòng "Đại diện Bên A/B", tìm được 2 ô ký.

**Dừng** nếu không tìm được ô ký. Máy chủ chưa dùng bản mẫu đã chỉnh, nên có thể gặp lại lỗi khối chữ ký bị tách trang: ghi nhận, vẫn chạy tiếp để thử API, nhưng phải sửa trước Phần B.

### MX-3. Một người ký, trọn vòng

Thay `<trang>`, `<x>`, `<y>` bằng tọa độ Bên A từ MX-2:

```bash
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\2026\BangChung_eContract\cong_cu\econtract_thu.ps1" -Buoc phat-hanh -Pdf "hop_dong.pdf" -BangChung "D:\2026\BangChung_eContract\Mekongx\MX-3" -SoHopDong THUNGHIEM-MX-03 -TenHopDong "THỬ NGHIỆM - MX-3" -TrangKy <trang> -X <x> -Y <y>
```

| Bước | Thao tác | Kết quả đúng hoặc cần ghi | Mã |
|---|---|---|---|
| 1 | Script đăng nhập đúng, thử sai 1 lần | Đúng có token; ghi định dạng lỗi khi sai | V0-01, 02 |
| 2 | Tạo hợp đồng, chưa truyền `type_id` | Trả `id`; ghi `recipientId`, `type_id` có bắt buộc không | V0-03, 05 |
| 3 | Phát hành 2 lần | Lần 1 trạng thái 20; ghi phản hồi lần 2 | V0-06, 07 |
| 4 | Mở hộp thư 1, đăng nhập bằng mật khẩu tạm, cài plugin | Nhận được thư; ghi thời hạn, trình duyệt | V0-08 |
| 5 | Xem ô ký trên eContract | Đúng dưới "Đại diện Bên A", không đè chữ | V0-10 |
| 6 | Ký bằng token | Ký thành công | V0-08 |
| 7 | Chạy lại script với `-Buoc kiem-tra` và cùng `-BangChung` | Trạng thái 30, có bản cuối; ghi `path`, host | V0-12, 13 |
| 8 | Mở bản cuối bằng Adobe và trên eContract | 1 chữ ký hợp lệ, nội dung trùng bản gửi | V0-14 |

**Dừng** nếu MX-3 không đạt. Script chạy lại sẽ từ chối tạo trùng; nếu tạo không trả `id`, vào eContract đối soát trước khi tạo lại.

### MX-4. Hai bên ký lần lượt, dùng chung token

Script hiện chỉ gửi 1 người ký, nên kịch bản này làm trên web eContract hoặc Postman. Bên A hộp thư 1 thứ tự 1, Bên B hộp thư 2 thứ tự 2, cùng `card_id`, dùng cùng PDF với 2 ô ký từ MX-2.

| Bước | Thao tác | Kết quả đúng hoặc cần ghi | Mã |
|---|---|---|---|
| 1 | Tạo, phát hành | Chấp nhận, hoặc ghi nguyên văn nếu từ chối vì trùng `card_id` | V0-03 |
| 2 | Xem hộp thư 2 khi Bên A chưa ký | Chưa nhận mật khẩu | V0-09 |
| 3 | Bên A ký, xem trạng thái | Vẫn 20 | V0-12 |
| 4 | Bên B ký | Ký trên bản đã có chữ ký Bên A | V0-09 |
| 5 | Tải bản cuối | Trạng thái 30, đủ 2 chữ ký đúng 2 ô | V0-13, 14 |

### MX-5. Các trường hợp không thành công

| KB | Thao tác | Kết quả đúng hoặc cần ghi | Mã |
|---|---|---|---|
| MX-5a | Tạo lại với số hợp đồng đã dùng | Bị từ chối hay tạo trùng | V0-04 |
| MX-5b | Phát hành, người nhận từ chối | Trạng thái 31 | V0-15 |
| MX-5c | Phát hành, rồi hủy | Trạng thái 32; có API hay chỉ trên web | V0-16 |
| MX-5d | Khai `card_id` sai, thử ký | Ghi bị chặn ở bước nào, thông báo ra sao | V0-17 |

Tốn khoảng 6 hợp đồng cho MX-3 đến MX-5.

## 6. Phần B: sau khi cập nhật giai đoạn 2–4 lên máy chủ

Trước khi chạy: chạy script cập nhật, nhập cấu hình bằng `vht_admin` (có OTP qua email, sửa email tài khoản thành hộp thư thật nếu chưa nhận được), bật `ECONTRACT_ENABLED` và đổi email người ký thử sang hộp thư thật.

| KB | Tài khoản | Thao tác | Kết quả đúng | Mã |
|---|---|---|---|---|
| MX-6 | `vht_admin` | Nhập `ECONTRACT_*`, bật cờ | Mật khẩu không hiện bản rõ | F2 |
| MX-7 | `vht_seller01` | Chốt khi thiếu MST/CCCD | Bị chặn, báo rõ cần bổ sung | A5 |
| MX-8 | Cả 3 bên | Duyệt, chốt, phát hành từ Sàn | Đúng Bên A/B, tư vấn không thành người ký; tải được PDF đã khóa | A1, A4 |
| MX-9 | 2 bên | Ký trên eContract, Sàn đồng bộ | Sàn hiện đúng trạng thái từng bên, lưu bản cuối, sang Bước 8 | |
| MX-10 | `vht_seller01` | Từ chối ký; hủy trên eContract | Sàn cập nhật 31, 32, không qua bước | |
| MX-11 | `vht_seller02` | Tải bản cuối của hồ sơ khác | Bị chặn | |
| MX-12 | `gs_theodoi` | Xem nhật ký | Không lộ token, mật khẩu, đường dẫn tải có chữ ký | |

Còn phải có token thứ hai và bên mua, bên bán thật (U1) mới đủ điều kiện báo đã xong.

## 7. Dọn dữ liệu sau khi test

- Lập danh sách Id hồ sơ, hợp đồng đã tạo.
- Sao lưu CSDL rồi mới xóa trên Sàn; viết script riêng theo đúng danh sách, chạy thử trên local trước. Không xóa theo tài khoản vì `vht_*` còn dữ liệu của đợt vận hành thử.
- Hợp đồng trên eContract Production không xóa được bằng các API đang dùng: hủy trên web những hợp đồng còn dở, ghi lại mã để báo MobiFone nếu cần.

## 8. Theo dõi kết quả

| Mục | Ngày | Id hồ sơ/hợp đồng Sàn | Mã eContract | Đạt/Không | Ghi chú |
|---|---|---|---|---|---|
| Điều kiện mục 3 | | | | | |
| MX-1 | | | | | |
| MX-2 | | | | | |
| MX-3 | | | | | |
| MX-4 | | | | | |
| MX-5a | | | | | |
| MX-5b | | | | | |
| MX-5c | | | | | |
| MX-5d | | | | | |
