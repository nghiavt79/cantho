# Kế hoạch tích hợp eContract MobiFone — Tạo hợp đồng & ký USB Token

Ngày lập: 04/11/2026
Nguồn: `D:\2026\CanTho\docs\hop dong dien tu\`
- `Dac_ta_tich_hop_API_USB_Token_eContract.docx` — đặc tả 19 API
- `Tạo hợp đồng đơn lẻ không theo mẫu.postman_collection.json` — mẫu `POST /api/v1/tp/contracts/create` (môi trường dev `https://econtractdev.mobifone.ai/service`)
- `USB_TOKEN_EXTERNAL_SIGNING.postman_collection.json` + `USB_TOKEN_LOCAL.postman_environment.json` — luồng ký USB Token
- `API_USB_TOKEN.zip` — trùng nội dung 3 file trên

Mục tiêu: thay phần hợp đồng điện tử và ký số hiện có (Bước 6–7 của quy trình 14 bước) bằng eContract MobiFone.

Cập nhật 04/11/2026 theo `D:\2026\CanTho\docs\PLAN_EContract_USBToken_Review.md`: sửa đảo Bên A/B, thêm Giai đoạn 0, chống tạo trùng, chặn đường ký cũ ở backend, điều kiện hoàn thành Bước 7, mô hình dữ liệu. Chi tiết luồng khôi phục, kiểm thử, rollback xem bản review (mục 5, 7, 8).

---

## 1. Hiện trạng trong code

### 1.1. Bước 6 — Soạn & duyệt hợp đồng
- `Controllers/ContractController.cs`, `Services/ContractService.cs`, `Views/Project/Steps/_Step6.cshtml`
- Tạo tự động từ mẫu HTML (`wwwroot/templates/contract_chuyen_giao_cong_nghe.html`, `SourceType=1`) hoặc upload PDF/DOCX (`SourceType=2`).
- 3 bên (Mua/Bán/Tư vấn) duyệt qua `ContractApprovals`; `SetReadyToSignAsync` → `StatusId=3 ReadyToSign` và `CompleteStep(6)`.

### 1.2. Bước 7 — Ký hợp đồng
- `Controllers/SigningController.cs`, `Services/ContractSigningService.cs`, `Views/Project/Steps/_Step7.cshtml`
- 3 cách ký:
  1. OTP qua email (`ESign:TestMode=true` nhận mọi mã 6 số; OTP ghi log dạng rõ).
  2. CA từ xa VNPT/FPT/Viettel (`Services/Signing/*`) — mới là khung; `CallbackSecret` không gửi cho nhà cung cấp nên callback thật không khớp được.
  3. USB Token qua `TechExchange.LocalSigner` (localhost:15800, Pkcs11Interop): ký SHA-256 file gốc, `PdfSigningService` vẽ ô chữ ký và ghi hex chữ ký vào metadata PDF.
- **Hạn chế:** cách 3 không phải chữ ký số PDF chuẩn (không nhúng PKCS#7/CMS), server không xác minh; bên ký sau ký hash bản gốc chứ không ký bản đã có chữ ký bên kia.
- Hoàn thành Bước 7: `ContractSigningService.TryCompleteStep7Async` khi đủ chữ ký role 1 (Mua) và role 2 (Bán) → `FullySigned` + `CompleteStep(7)` + thông báo.

### 1.3. Dữ liệu
- `ProjectContracts`, `ContractApprovals`, `ContractSignatureRequests`, `ContractSignatures`, `ContractAuditLogs` (script `Create_Step6_Step7_Full.sql`).
- Bảng cũ `EContracts` (`EContractController`, `ProjectController.UpdateEContractData`) song song với `ProjectContracts` → chốt `ProjectContracts` là nguồn chính; `EContracts` chỉ đọc lịch sử, khóa ghi.
- Tham số ký trong `SYS_PARAMETERS` nhóm `CONTRACT`: `SIGNING_MODE`, `SIGNING_PROVIDER_DEFAULT`, `SIGNING_VNPT_*`, `SIGNING_FPT_*`, `SIGNING_VIETTEL_*`, `ESIGN_OTP_*`.

### 1.4. Vai các bên trong hợp đồng
- Mẫu `contract_chuyen_giao_cong_nghe.html` và `ContractService.BuildHtmlSnapshot`: **Bên A = Bán (bên chuyển giao), Bên B = Mua (bên nhận chuyển giao)**.
- Role nội bộ: Buyer=1, Seller=2, Tư vấn=3. Role recipient eContract: SIGNER=3. Hai bộ giá trị này không dùng chung.

### 1.5. Các đường ghi "đã ký" / hoàn thành Bước 7 cũ (phải chặn với hợp đồng eContract)
- `SigningController`: OTP, CA từ xa, `Callback/{provider}`, `MockCallback`, `USBTokenSign`.
- `EContractController.SignAsBenA/SignAsBenB` (ký mô phỏng, bảng `EContracts`).
- `ProjectController.UpdateEContractData` (cập nhật trạng thái từ form).
- `AdvancePaymentController.cs:127`: nhánh **tạo** xác nhận tạm ứng gọi `CompleteStep(..., 7)`; nhánh xác nhận ở :208 mới hoàn thành Bước 8. Cách sửa: bỏ lời gọi ở :127, không đổi thành 8.

---

## 2. Phạm vi thay thế

| Giữ lại | Thay bằng eContract |
|---|---|
| Bước 6: soạn, góp ý, duyệt 3 bên, xuất PDF | Tạo hợp đồng đơn lẻ không theo mẫu (`tp/contracts/create`) + phát hành (`tp/contracts/my-contract/issue`) |
| Bảng `ProjectContracts` | Người ký đăng nhập và ký USB Token trên web eContract bằng mật khẩu tạm eContract gửi (đã chốt, mục 5A) |
| Hoàn thành Bước 7 bằng `CompleteStep(7)` | Theo dõi trạng thái + tải PDF đã ký (`tp/contracts`, `tp/contracts/{id}/documents`) |
| | Bỏ LocalSigner, OTP, CA từ xa, `PdfSigningService` cho hợp đồng |

Ngoài phạm vi (chờ quyết định): ký NDA Bước 2 (`ESignController`, `ESignGateway`) — đề xuất làm đợt sau.

Nguyên tắc:
- Tư vấn duyệt nội bộ ở Bước 6, không thêm thành người ký eContract.
- Provider ký chốt theo từng hợp đồng (cột `SigningProvider`); đổi cấu hình mặc định không đổi provider hợp đồng đã chốt.
- Hợp đồng lịch sử giữ nguyên file/chữ ký/provider; hợp đồng đang ký dở không chuyển sang eContract.
- "Không theo mẫu" là cách tạo trên eContract (`is_template=false`), vẫn dùng mẫu HTML của Sàn để sinh PDF.

---

## 3. Kế hoạch thực hiện

### Giai đoạn 0 — Kiểm kê và thử kết nối Production (trước khi viết code)
- [ ] Chốt phạm vi Bước 6–7 / NDA.
- [ ] Thử thủ công trên Production `mobifone-econtract.vn` (org 13592) bằng Postman: auth → create → issue → 2 người ký nhận mật khẩu tạm, ký USB Token trên web eContract → tp/contracts thấy status 30 → tải PDF cuối.
- [ ] Kiểm chứng khi thử: vị trí ô ký (gốc tọa độ, pixel/point); tạo trùng `contract_no` có bị từ chối không; `type_id` có bắt buộc không; `create` có trả đủ recipient để theo dõi từng bên không; 2 bên khác tổ chức khai báo thế nào.
- **Xong khi:** có bộ request/response đã che bí mật + PDF có 2 chữ ký; hoặc xác định rõ phần bị chặn phía MobiFone.

### Giai đoạn 1 — Nền tảng (đã làm, chưa bật cho người dùng, chưa commit)
- [x] `Services/EContracts/EContractClient.cs` (typed HttpClient): auth, create, issue, tra cứu, documents, tải PDF. Kiểm HTTP + `code`/`success`; chỉ tự retry API đọc; 401 đăng nhập lại một lần; thao tác ghi gặp timeout/5xx trả lỗi `Unknown` để đối soát; không log token/mật khẩu/PDF/URL tải.
- [x] Tải PDF chỉ theo URL https do API trả, host phải thuộc eContract hoặc `ECONTRACT_DOWNLOAD_HOSTS`; token chỉ gửi tới host eContract; kiểm cả chuyển hướng, giới hạn 50 MB, kiểm đầu file PDF.
- [x] Tham số `SYS_PARAMETERS` nhóm `CONTRACT`: `ECONTRACT_ENABLED` (0), `ECONTRACT_ENV`, `ECONTRACT_BASE_URL`, `ECONTRACT_EMAIL`, `ECONTRACT_PASSWORD`, `ECONTRACT_TYPE_ID`, `ECONTRACT_ORG_ID` (13592), `ECONTRACT_DOWNLOAD_HOSTS`. Mật khẩu lưu qua `/Secure/Encrypt` (cơ chế hiện có của Sàn: XOR + khóa trong appsettings, chỉ ở mức che giấu).
- [x] `Scripts/add-econtract-columns.sql` (chạy lặp an toàn, đã thử trên DB tạm; **PHẢI chạy trước khi deploy** vì `ProjectContracts` có cột mới):
  - `ProjectContracts`: `SigningProvider`, `EContractEnv`, `EContractId`, `EContractNo`, `EContractStatus`, `IntegrationState`, `FrozenPdfPath`, `FrozenPdfSha256`, `EContractIssuedAt`, `EContractSyncedAt`, `EContractCompletedAt`.
  - Bảng `EContractRecipients`, `EContractFields`, `EContractOperations`.
  - Unique (`EContractEnv`, `EContractId`), (`EContractEnv`, `EContractNo`), `OperationKey`.
- [x] `Services/EContracts/SignaturePlacementService.cs`: tìm vị trí thật của "Đại diện Bên A/B", tính tọa độ cộng dồn; không tìm thấy hoặc trang xoay thì trả null.
- [x] Unit test: `EContractClientTests`, `SignaturePlacementServiceTests`.

Khác plan ban đầu:
- Không thêm `RowVersion`: sẽ làm các đoạn cập nhật cũ của `ProjectContracts` có thể gặp lỗi concurrency. Chống trùng dùng unique index + `EContractOperations`.
- Không tách `PdfSigningService.FindTextPosition`: hàm đó chỉ kiểm có chữ rồi đặt theo cột cố định, không cho tọa độ thật. Viết mới, giữ nguyên luồng cũ.
- Chưa kiểm chứng với eContract thật: wire format, gốc tọa độ, vị trí ô ký trên PDF thật (GĐ0).

### Giai đoạn 2 — Chốt PDF và phát hành
- [ ] Khi "Sẵn sàng ký" (`ContractService.SetReadyToSignAsync`): khóa một PDF bất biến (`FrozenPdfPath` + hash) mà 3 bên đã duyệt.
  - HTML → PDF bằng `HtmlToPdfService`; PDF upload dùng nguyên.
  - DOCX: đợt đầu chưa hỗ trợ (chưa có bộ chuyển DOCX→PDF) — yêu cầu upload PDF.
- [ ] Mapping người ký: **Seller (role 2) → Bên A; Buyer (role 1) → Bên B**. PDF upload: người chốt xác nhận vai từng bên.
- [ ] Vùng ký: tìm theo "Đại diện Bên A/B"; không tìm thấy (PDF scan/upload tùy ý) → giao diện chọn vùng trên trang, không tự đặt vị trí.
- [ ] `tp/contracts/create` (`is_template=false`): mỗi bên 1 recipient `role=3`, `sign_type=[{id:2}]`, `card_id`; mỗi người ký ≥1 field `type=3`. Không copy nguyên payload mẫu (mẫu có reviewer, OTP, `type_id=538`, email test).
- [ ] Chống tạo trùng:
  - Ghi `EContractOperations` (Pending) trước khi gọi; lưu `contractId` ngay khi có.
  - Timeout create → `Unknown`, đối soát theo cách MobiFone xác nhận; chưa xác định được thì chuyển vận hành xử lý, **không** cho bấm tạo lại.
  - Issue lỗi khi đã có ID → chỉ retry issue trên ID đó.
- [ ] Issue body `[contractId]` → kiểm `success=true` → đọc lại trạng thái (≥20 là hợp lệ; >20 thì đối soát, không báo lỗi).
- [ ] Sau phát hành không sửa PDF / đổi người ký tại Sàn; sửa nội dung cần quy trình hủy của MobiFone rồi tạo version mới.
- Bước 6 hoàn thành khi PDF đã khóa và duyệt xong; Bước 7 mở xem tiến trình, chỉ hiện hướng dẫn ký khi đã phát hành.

### Giai đoạn 3 — Giao diện Bước 7 (ký trên eContract)
- [ ] `_Step7.cshtml`: với hợp đồng `SigningProvider=ECONTRACT`, thay các nút ký bằng khối hiển thị: người ký, thứ tự ký, trạng thái, nút "Kiểm tra trạng thái", PDF đã ký khi hoàn tất.
- [ ] Hướng dẫn người ký: kiểm tra email/SMS nhận mật khẩu tạm → đăng nhập `mobifone-econtract.vn` → cài plugin eContract → chọn nhà cung cấp chữ ký số → ký. Có link mở eContract; chỉ mở trang công khai, không tự ghép URL ký.
- [ ] Trước khi gửi lên eContract: kiểm email/SĐT và MST/CCCD từng bên, thiếu thì chặn và báo rõ cần bổ sung.
- [ ] Sàn không nhận, không lưu mật khẩu tạm, PIN hay chứng thư của người ký.

### Giai đoạn 4 — Đồng bộ, hoàn thành Bước 7, chặn đường cũ
- [ ] Worker polling `tp/contracts` có phân trang, đối chiếu đúng `EContractId`; tần suất theo quota, có backoff; nút "Kiểm tra trạng thái" giới hạn tần suất.
- [ ] `status=30` → `tp/contracts/{id}/documents` → bản `type=2` (FINALLY) → tải PDF (không forward JWT sang presigned URL; kiểm host, kích thước, định dạng) → lưu ngoài webroot, tải qua endpoint kiểm quyền.
- [ ] Hoàn thành Bước 7 bằng hàm riêng cho ECONTRACT (không dùng nguyên `TryCompleteStep7Async`): đủ 2 bên đúng mapping + status 30 + đã lưu PDF cuối/hash; chạy một lần (khóa giữa worker và nút bấm), thông báo không trùng.
  - Chưa tải/kiểm xong: hiển thị "Đã ký trên eContract, đang đồng bộ", chưa mở Bước 8.
  - `ContractSignatures`: tạo `ContractSignatureRequest` trước (`SignatureRequestId` bắt buộc); không bịa cert serial/hash/thời điểm ký; chưa có bằng chứng xác minh thì `VerificationStatus=Unknown`.
- [ ] Status 31/32/40: dừng ký mới; hợp đồng đã hoàn tất rồi bị thanh lý thì giữ lịch sử, không lùi quy trình.
- [ ] Chặn ở backend mọi đường ghi cũ (mục 1.5) với hợp đồng `SigningProvider=ECONTRACT`; bỏ `CompleteStep(...,7)` ở `AdvancePaymentController.cs:127`.

### Giai đoạn 5 — UAT, hướng dẫn, chuyển đổi
- [ ] Kiểm thử theo mục 7 bản review (happy path, quyền, khôi phục, thiết bị, đồng bộ, hồi quy) với token thật trên domain triển khai.
- [ ] Cập nhật `Areas/Cms/Views/HuongDanCms/Index.cshtml` và trang hướng dẫn "Ký hợp đồng".
- [ ] Triển khai: thêm schema trước, cờ `ECONTRACT_ENABLED` tắt, nghiệm thu staging, bật cho hợp đồng mới. Không xóa code/bảng/file cũ đợt đầu.
- [ ] Rollback: tắt tạo mới qua eContract, vẫn đọc/đối soát hợp đồng đã phát hành; không đưa hợp đồng ký dở về OTP/LocalSigner; không drop cột external ID.
- [ ] Gói deploy: chỉ file thay đổi + `add-econtract-columns.sql` + `HUONG_DAN`.

---

## 4. Danh mục API eContract dùng

| # | Method | Endpoint | Dùng ở | Loại |
|---|---|---|---|---|
| 01 | POST | `/api/v1/auth` | GĐ1 | Auth |
| 02 | POST | `/api/v1/tp/contracts/create` | GĐ2 | TP |
| 03 | PUT | `/api/v1/tp/contracts/my-contract/issue` | GĐ2 | TP |
| 04 | GET | `/api/v1/contracts/{contractId}` | GĐ2–3 | APP |
| 05 | GET | `/api/v1/documents/by-contract/{contractId}` | GĐ3 | APP |
| 06 | GET | `/api/v1/fields/by-contract/{contractId}` | GĐ2–3 | APP |
| 07 | GET | `/api/v1/organizations/basic-info/{orgId}` | GĐ3 | APP |
| 08 | POST | `/api/v1/contracts/check-mst-exist` | GĐ3 | APP |
| 09 | GET | `/api/v1/sign/check-process-is-sign/{recipientId}` | GĐ3 | APP |
| 10 | POST | `/api/v1/processes/digital-sign/{recipientId}/create-empty-token` | GĐ3 (v2) | APP |
| 11 | POST | `/api/v1/processes/digital-sign/{recipientId}/merge-time-stamp` | GĐ3 (v2) | APP |
| 12 | POST | `/api/v1/sign/token/genImage` | GĐ3 (v1) | APP |
| 13 | PUT | `/api/v1/processes/digital-sign/{fieldId}` | GĐ3 | APP |
| 14 | PUT | `/api/v1/processes/approval/{recipientId}` | GĐ3 | APP |
| 15 | POST | `/api/v1/tp/contracts/multi/usb-token` | Thay thế 13 | TP |
| 16 | PUT | `/api/v1/tp/contracts/approval/{recipientId}` | Thay thế 14 | TP |
| 17 | GET | `/api/v1/tp/contracts` | GĐ4 | TP |
| 18 | GET | `/api/v1/tp/contracts/{contractId}/documents` | GĐ4 | TP |
| 19 | GET | `{DocumentDto.path}` | GĐ4 | URL trả về |

**Đã chốt ký trên eContract: Sàn chỉ gọi 01, 02, 03, 17, 18, 19.** Các API 04–16 không dùng; để lại bảng làm tra cứu. Bổ sung khi MobiFone gửi tài liệu: API báo cáo trạng thái, API Danh sách Loại tài liệu.

Giá trị quan trọng: SignType USB_TOKEN=2; FieldType ký số=3; role người ký=3; Contract status 0/10/20/30 = DRAFT/CREATED/PROCESSING/SIGNED, 31/32/40 = REJECTED/CANCEL/LIQUIDATED; Recipient status 2 = APPROVAL; Document type 2 = FINALLY.

---

## 5. Thông tin đã xem trên tài khoản Production (04/11/2026, chỉ đọc)

| Mục | Giá trị |
|---|---|
| Môi trường | dev `https://econtractdev.mobifone.ai`; nội bộ `https://econtract.mobifone.vn`; **kinh doanh (Production) `https://mobifone-econtract.vn`** |
| API base Production | `https://mobifone-econtract.vn/service/api/v1` |
| Đăng nhập giao diện | SSO MobiFone (`auth-sso.mobifone.vn`, realm `sso-mobifone`); API đăng nhập bằng `/service/api/v1/auth` (mục 5A) |
| Tổ chức | Trung tâm Ứng dụng tiến bộ KH&CN TP Cần Thơ — org ID **13592**, mã tổ chức 1801316342, tổ chức cha |
| Gói dịch vụ | 10.000 tài liệu, hạn 28/10/2027; chưa tạo tài liệu nào |
| Người dùng | 1 tài khoản Admin, đăng nhập bằng Email và SĐT |
| Loại tài liệu | **Chưa có** → phải tạo trên Production để lấy `type_id` (538 là ID của dev) |
| Plugin USB | Giao diện có cả v1 (`http://localhost:6704`) và v2 (`https://127.0.0.1:14424/process`); chưa rõ tổ chức cấu hình bản nào |
| Bộ cài plugin trên trang | `/assets/upload/mobi_token_sign_setup.zip` (9,3 MB, cập nhật 03/09/2026) → `mobi_token_sign_setup.exe` "Mobifone PKI Sign" 1.0.0.0 (2019), Advanced Installer, **không ký số**. Thành phần: .NET Framework, OWIN self-host Web API có CORS, iTextSharp 4.1.6 ký cả file PDF → khớp **plugin v1** (`/api/mobi/getcert`, `/api/mobi/signpdf`). Không có MBFCASignPlugin (v2) trong bộ này. |
| Tạo tài liệu | Có "Tài liệu đơn lẻ không theo mẫu": tên, số, loại, ngày hết hạn ký, ngày hết hiệu lực, file đính kèm → Xác định người ký → Thiết kế → Hoàn tất |
| Chứng thư số server | Tài khoản Admin không có quyền xem |

Hệ quả cho kế hoạch:
- `ECONTRACT_ORG_ID=13592`; `ECONTRACT_BASE_URL` Production như trên.
- Thử nghiệm (GĐ0) làm thẳng trên Production (anh Nghĩa xác nhận 04/11/2026: không ảnh hưởng); hợp đồng thử đặt tên có tiền tố "THỬ NGHIỆM" để phân biệt.
- Trước khi chạy Production: tạo Loại tài liệu (cần đồng ý), lấy `type_id`.

## 5A. Phản hồi của MobiFone (đợt 1)

| # | MobiFone trả lời | Tác động |
|---|---|---|
| 1 | Base URL Production `https://mobifone-econtract.vn/`. Bộ API trong tài liệu là bộ thực tế đang dùng; phát sinh thì báo anh Khang. Tài khoản do chị Băng Nhi gửi. | Dùng base URL + `/service/api/v1`. Chưa có câu chữ riêng về việc Sàn gọi API APP. |
| 2 | Đăng nhập `/service/api/v1/auth` bằng username/mật khẩu trong mail thông báo tạo tài khoản. | Server Sàn dùng tài khoản này. Chỉ lưu mật khẩu ở cấu hình đã mã hóa, không đưa vào repo. |
| 3 | Đến lượt ký, hệ thống gửi KH/Đối tác mật khẩu tạm có thời hạn kèm username (SĐT/email) để **đăng nhập và ký trên eContract**. Phương thức ký chọn khi tạo hợp đồng; khi ký, người ký chọn NCC chữ ký số trong danh sách đã tích hợp. | **Không có phiên ký cho Sàn.** Không thực hiện được GĐ3 như thiết kế; xem mục dưới. |
| 4 | Có API báo cáo trạng thái xử lý hợp đồng. Có chức năng Kiểm tra chữ ký số trên web; API thì nhờ anh Khang hỏi TT3. | `VerificationStatus` giữ `Unknown` cho đến khi có API kiểm tra. |
| 5 | Ký token trên web cần plugin eContract (có link tải). | Người ký tự cài plugin. Chưa rõ v1/v2, nhưng không còn quan trọng nếu ký trên eContract. |
| 6 | Trang bắt đầu từ 1; tọa độ cộng dồn theo trang, đơn vị pixel. | Chưa nêu gốc tọa độ, chiều cao trang. Giả thuyết cần thử: 1 pixel = 1 point PDF, gốc trên-trái (mẫu `page=3`, `y=2360` → cách đỉnh trang 3 khoảng 676 nếu A4 cao 842). |
| 7 | Tránh trùng bằng Số tài liệu (truyền khi tạo) và Mã tài liệu (trả về sau khi tạo). Sửa/xóa khi chưa phát hành; hủy khi đã phát hành. | Sinh `contract_no` duy nhất theo hợp đồng + version. Thử xem tạo trùng số có bị từ chối không (đặc tả có nhắc lỗi "trùng số"). Chưa rõ tra cứu theo số bằng API nào và hủy có API không. |
| 8 | Chị Băng Nhi, anh Khang tạo Loại tài liệu (nếu muốn quản lý theo loại); lấy `type_id` bằng API Danh sách Loại tài liệu. | API này chưa có trong tài liệu đã nhận; câu trả lời cho thấy `type_id` có thể không bắt buộc, cần thử. |

**Chưa được trả lời:** webhook báo ký xong; có được thử trên Production không và cách đánh dấu hợp đồng thử; bên Mua, bên Bán thuộc hai tổ chức; xử lý khi đã lưu PDF nhưng approval lỗi; NCC nào trong danh sách (VINA-CA có không); endpoint API báo cáo trạng thái và Danh sách Loại tài liệu.

### Đã chốt: ký trên eContract, Sàn tạo và đồng bộ

Anh Nghĩa chốt sau phản hồi đợt 1. Đây là phương án dự phòng trong bản review mục 9.

- Bỏ khỏi phạm vi: controller ký, phiên ký, adapter plugin, `check-mst`, prepare/merge, upload, approval.
- API dùng: 01, 02, 03, 17, 18, 19 — đều là API xác thực, TP hoặc URL trả về, không phụ thuộc API APP.
- Bên Mua/Bán phải có email hoặc SĐT chính xác trên Sàn, `card_id` khớp chứng thư nếu ký USB.
- Trạng thái từng người ký: lấy từ API báo cáo trạng thái nếu có; chưa có thì Sàn chỉ hiện trạng thái cả hợp đồng.

## 6. Câu hỏi đã gửi MobiFone

1. **API APP** nào được cam kết cho Sàn gọi (contract, fields, prepare, merge, digital-sign, approval…); tenant dùng legacy workflow đúng tài liệu này không; cấp quyền dev và Production.
2. **Tài khoản tích hợp:** Production đăng nhập qua SSO MobiFone → server Sàn lấy token bằng cách nào (tài khoản riêng, client credentials…)?
3. **Danh tính người ký:** cách cấp phiên/SSO cho Bên Mua và Bên Bán; người ký bên ngoài có cần tài khoản eContract riêng không; mô hình backend Sàn làm trung gian có được hỗ trợ không? *(quyết định kiến trúc)*
4. **Xác minh chữ ký:** ai chịu trách nhiệm kiểm integrity, trust chain, OCSP/CRL, timestamp; có API/bằng chứng không; thời điểm ký và thông tin chứng thư từng bên lấy ở đâu?
5. **Plugin:** bộ cài trên trang là "Mobifone PKI Sign" 1.0.0.0 (v1, 2019) — tổ chức 13592 dùng v1 hay v2 (`usb_token_version`); nếu v2 thì nguồn cài MBFCASignPlugin; v1 có bản mới hơn/bản có ký số không; hệ điều hành/trình duyệt/driver hỗ trợ; TLS/CORS/local network policy khi gọi từ tên miền Sàn.
6. **Tọa độ ô ký:** đơn vị, gốc tọa độ, page index/rotation; cách tính `coordinate_y` (mẫu `page=3`, `y=2360`); quyền dùng `sign/multi/usb-token/page-info`.
7. **Khôi phục:** tra cứu sau timeout create (idempotency), xử lý marker `check-process-is-sign`, khóa đồng thời, quy trình hủy/thu hồi/tạo lại.
8. **Tài khoản thử và thông số còn thiếu:** 2 tài khoản người ký thử trên Production (org 13592) và USB Token thử nghiệm; quota/rate limit API; webhook; danh sách host tải file; Sàn tự tạo Loại tài liệu trên Production hay MobiFone tạo.

## 7. Cần anh Nghĩa quyết định
- Ký NDA Bước 2: đề xuất chuyển ở đợt sau.
- Đợt đầu chỉ nhận PDF/HTML, chưa nhận DOCX?
- Có cần Loại tài liệu "Hợp đồng chuyển giao công nghệ" không? Nếu cần, nhờ chị Băng Nhi hoặc anh Khang tạo.
