# Rà soát và kế hoạch thay thế hợp đồng ký điện tử bằng eContract MobiFone

Ngày rà soát: 04/11/2026. Phạm vi công việc hiện tại: đối chiếu tài liệu, code và lập kế hoạch; chưa triển khai tích hợp hoặc gọi môi trường MobiFone.

## 1. Kết luận về plan Claude

Plan tại `.claude/worktrees/api-hop-dong-ky-usb-token-26ca31/docs/PLAN_EContract_USBToken.md` đúng hướng về API tạo/phát hành, hai nhánh plugin USB, đồng bộ trạng thái và giữ quy trình duyệt nội bộ. Tuy nhiên, **chưa nên triển khai nguyên bản**: có lỗi đảo Bên A/B, thiếu kiểm soát tạo trùng khi timeout, thiếu chặn các đường ký cũ ở backend và chưa định nghĩa đầy đủ điều kiện hoàn thành hợp đồng.

Thông điệp “API Tạo hợp đồng đơn lẻ không theo mẫu kèm bộ API ký USB token” phù hợp với việc Sàn tự cung cấp PDF và dữ liệu người ký rồi giao eContract quản lý phát hành, ký và tài liệu kết quả. “Không theo mẫu” là cách tạo trên eContract (`is_template=false`), không có nghĩa phải bỏ mẫu HTML đang dùng trong Sàn.

Tài liệu được giao tự ghi rõ chưa xác nhận API APP là public ổn định, chưa kiểm chứng plugin/hardware và ký end-to-end Production. Vì vậy, đây là bộ tài liệu để thiết kế và làm thử nghiệm tích hợp, chưa đủ bằng chứng cam kết giao diện ký độc lập trong Sàn hoạt động hoàn chỉnh.

## 2. Các phát hiện cần sửa

| Mức | Nội dung trong plan cũ | Kết quả đối chiếu và điều chỉnh |
|---|---|---|
| P0 | Bên A = Mua, Bên B = Bán | Sai với `ContractService.BuildHtmlSnapshot` và mẫu `contract_chuyen_giao_cong_nghe.html`: **Bên A = Bán/chuyển giao; Bên B = Mua/nhận chuyển giao**. Role nội bộ Buyer=1, Seller=2; role recipient eContract SIGNER=3. Không dùng chung các enum này. Với PDF upload, phải xác nhận vai của từng bên trên tài liệu. |
| P0 | Dùng client đăng nhập tài khoản tích hợp rồi proxy API ký | Tài khoản tạo/phát hành và phiên đúng recipient là hai quyền khác nhau. Việc proxy qua backend không tự giải quyết danh tính người ký. Chỉ chốt mô hình sau khi MobiFone cung cấp signer session/SSO/delegation được hỗ trợ; tuyệt đối không mặc định token người tạo có quyền ký thay. |
| P0 | Lỗi tạo/phát hành thì nút “Gửi lại eContract” | Nếu tạo thành công nhưng mất response, gửi lại có thể tạo hợp đồng thứ hai. Phải lưu tiến trình từng thao tác, lưu ID ngay sau create, chỉ retry issue trên ID đã biết; kết quả không xác định phải đối soát trước khi tạo lại. `contract_no` không được mặc định là idempotency key của nhà cung cấp. |
| P0 | Ẩn OTP/CA bằng cấu hình trên giao diện | Chưa đủ: backend phải chặn mọi đường ký cũ đối với hợp đồng mang provider ECONTRACT. Rà `SigningController`, callback CA, `EContractController.SignAsBenA/SignAsBenB` đang mô phỏng ký và `ProjectController.UpdateEContractData` cho cập nhật trạng thái từ form. |
| P0 | Ghi đủ hai `ContractSignatures` rồi dùng `TryCompleteStep7Async` | Hàm hiện tại chỉ xét có bản ghi role 1 và 2, chưa kiểm provider, trạng thái eContract, PDF cuối hay tính hợp lệ chữ ký. Cần điều kiện riêng cho ECONTRACT và cơ chế hoàn thành một lần; không tạo bản ghi “đã ký hợp lệ” chỉ từ status=30. |
| P1 | Upload PDF/DOCX; tới bước chốt lấy PDF | API create yêu cầu PDF. `HtmlToPdfService` hiện chuyển HTML, chưa phải bộ chuyển DOCX. Đợt đầu chỉ cho PDF hoặc HTML được chuyển và xem trước; DOCX phải có pipeline chuyển đổi, kiểm layout rồi duyệt lại PDF trước chốt. |
| P1 | Tái dùng `FindTextPosition` | Hàm hiện là private; chỉ nên tách thuật toán tìm vùng ký thành service riêng. PDF scan hoặc upload tùy ý cần UI chọn vùng trên từng trang; không tự đặt vị trí khi không tìm thấy anchor. |
| P1 | Chờ tới Giai đoạn 3 mới xác nhận quyền API APP | Giai đoạn 2 đã cần đọc recipients/fields và trạng thái qua APP. Phải xác minh quyền và identity ngay ở thử nghiệm đầu tiên, trước khi thiết kế UI ký. TP bulk upload/approval không thay thế được prepare/merge v2. |
| P1 | Chỉ giữ `EContractRecipients` với FieldId/DocumentId | Một recipient có thể có nhiều field/document. Tách recipient, field và phiên thao tác; lưu UserId cùng thông tin đại diện, provider/tenant/environment và version/hash PDF. |
| P1 | Bỏ qua `EContracts` như dữ liệu cũ | Project hiện có cả `ProjectContracts` và `EContracts`, cùng đường cập nhật cũ. Phải xác định nguồn dữ liệu chính, cách đọc lịch sử và khóa các lối ghi trạng thái cũ khi chuyển đổi. |
| P1 | Lỗi `AdvancePaymentController:127` là sửa tùy chọn | Đúng là create tạm ứng đang gọi `CompleteStep(..., 7)`. Với phạm vi thay ký Bước 7, phải loại bỏ khả năng đánh dấu ký xong qua tạm ứng. Không sửa máy móc thành hoàn thành Bước 8 ngay khi tạo: nhánh xác nhận ở controller đang hoàn thành Bước 8. |

Các điểm Claude nêu đúng: API create dùng multipart `file_contract` + `jsonRequest`; USB SignType=2, field ký số type=3; phải issue; merge chưa lưu PDF nên cần upload rồi approval; recipient APPROVAL=2 không đồng nghĩa toàn bộ hợp đồng SIGNED=30; PDF người ký sau phải là bản mới nhất; chưa có webhook đã xác nhận.

## 3. Phạm vi đề xuất

1. Giữ Bước 6: soạn hợp đồng, góp ý, duyệt Mua/Bán/Tư vấn, lịch sử phiên bản. Chốt một PDF bất biến mà các bên đã xem và duyệt. Tư vấn duyệt nội bộ; không tự thêm tư vấn thành người ký eContract.
2. Thay phần phát hành, ký và lấy bản đã ký ở Bước 6–7 bằng eContract. Đợt đầu dùng USB Token; provider được chốt theo từng hợp đồng.
3. Bước 7 hiển thị người ký, lượt ký, lỗi plugin, tiến trình đồng bộ và PDF cuối. Backend quyết định quyền và trạng thái; form không tự ghi “đã ký”.
4. Hợp đồng lịch sử giữ nguyên file, chữ ký và provider. Hợp đồng đã ký dở không tự chuyển sang provider mới. NDA Bước 2 cần được kiểm kê; đề xuất chuyển trong đợt sau vì có luồng `ESignController`/`ESignGateway` riêng.

## 4. Kiến trúc và dữ liệu

### 4.1. Các thành phần

- `EContractClient`: typed HttpClient, DTO riêng từng endpoint, kiểm HTTP và business response; tách credential/token cache người tạo và signer, không dùng cache chung cho mọi người ký. Chỉ retry đọc an toàn; mutation cần chính sách theo kết quả đối soát.
- `EContractIntegrationService`: chốt PDF, tạo, phát hành, lưu định danh và xử lý khôi phục. Không giữ transaction DB qua lời gọi HTTP ngoài.
- `EContractSigningService` và controller: kiểm user/đại diện được phép, phiên bản đang hoạt động, lượt ký và chuỗi contract–recipient–field–document. Mọi mutation từ trình duyệt dùng antiforgery; ID client gửi không phải bằng chứng quyền.
- `SignaturePlacementService`: xử lý vùng ký và chuyển tọa độ theo page size/rotation/scale đã xác nhận. Không gọi phần vẽ metadata của `PdfSigningService` lên PDF eContract đã ký.
- Adapter JavaScript plugin v1/v2: chỉ gọi plugin trên máy người ký, xử lý encoding/request/response theo collection. PIN/private key nằm trên thiết bị; server không nhận PIN.
- Worker đồng bộ và dịch vụ hoàn tất: đối soát trạng thái, tải tài liệu và hoàn thành workflow một lần. Dùng outbox hoặc cơ chế tương đương cho thông báo để chạy lại không gửi trùng.

### 4.2. Mô hình dữ liệu dự kiến

- `ProjectContracts`: thêm SigningProvider, EContractId, EContractStatus, IntegrationState, môi trường/tenant, ContractNo, FrozenPdfPath/Hash, IssuedAt, SyncedAt, CompletedAt, concurrency token. Giữ trạng thái Sàn riêng với enum eContract.
- `EContractRecipients`: local ContractId, UserId/đại diện đã xác nhận, business role, recipientId, email/phone, MST/CCCD, ordering và trạng thái; snapshot thông tin tại thời điểm phát hành.
- `EContractFields`: recipient mapping, fieldId, documentId, page, tọa độ/kích thước. Quan hệ một recipient–nhiều field.
- `EContractOperations` hoặc cấu trúc tương đương: operation key, hành động, contract/version, recipient/field, hash đầu vào, trạng thái Pending/Succeeded/Failed/Unknown, correlation ID, lỗi đã lược bỏ dữ liệu nhạy cảm, thời gian và lease chống xử lý trùng.
- Phiên ký: ràng buộc user + recipient + certificate fingerprint + document version/hash + prepare result, hạn dùng và trạng thái. Dữ liệu tạm không ghi log, không cho client tự tráo giữa các phiên.
- Ràng buộc unique cho mapping provider/environment/external contract; khóa chống tạo hai hợp đồng ngoài cho cùng local version. Tạo `ContractSignatureRequest` trước khi ghi `ContractSignature` vì model hiện có `SignatureRequestId` bắt buộc.
- Mật khẩu/client secret lưu qua cơ chế bảo vệ bí mật hiện có sau khi kiểm tra triển khai; DB cấu hình chỉ lưu tham chiếu hoặc ciphertext có bảo vệ khóa, không chỉ ghi chú “mã hóa”.

Đây là thiết kế dự kiến, cần chốt tên cột, độ dài và khóa dựa trên response thực tế trước khi viết migration. Migration bổ sung phải chạy lặp an toàn và không sửa dữ liệu chữ ký lịch sử.

## 5. Luồng nghiệp vụ và khôi phục

### 5.1. Chốt và phát hành

`Draft → Review → ReadyToSign (PDF đã khóa) → Creating → Created → Issuing → Processing`.

Bước 6 có thể hoàn thành khi PDF và duyệt nội bộ đã chốt. Bước 7 được mở để xem tiến trình, nhưng chỉ bật ký khi eContract đã phát hành và mapping đã được xác nhận. Không coi `ReadyToSign` là đã phát hành thành công.

- Chốt PDF, thông tin hai bên, quyền đại diện, thứ tự ký, email/MST và vị trí ký. Mẫu hiện tại: Seller/role2 → Bên A; Buyer/role1 → Bên B. PDF upload phải xác nhận mapping theo nội dung.
- Create với `is_template=false`; mẫu collection có cả reviewer, USB và OTP nên không copy nguyên participants/fields. `type_id=538`, email, số hợp đồng và ngày trong mẫu không phải giá trị Production mặc định.
- Lưu contractId ngay sau create trước khi issue. Issue với body `[contractId]`, xác nhận `success=true`, đọc lại trạng thái hợp lệ; nếu đã tiến xa hơn 20 thì đối soát trạng thái hiện tại, không kết luận thất bại chỉ vì không còn đúng 20.
- Nếu create timeout: `Unknown`, đối soát theo cơ chế được MobiFone xác nhận; nếu chưa tìm được duy nhất kết quả thì yêu cầu xử lý vận hành, không bật nút tạo lại vô điều kiện. Issue lỗi có ID thì thao tác phục hồi trên ID đó.
- Sau phát hành không sửa PDF hoặc đổi người ký tại Sàn. Yêu cầu sửa nội dung cần quy trình hủy/thu hồi được nhà cung cấp hỗ trợ rồi tạo phiên bản mới; tài liệu hiện chưa đủ API để tự thực hiện quy trình này.

### 5.2. Ký USB

- Xác thực đúng signer theo phương án MobiFone xác nhận, nạp contract/documents/fields/org và chọn `usb_token_version`. Không nhầm plugin v2 với backend workflow V2; tài liệu này dùng legacy workflow.
- Plugin v2: Op1 → Op2 → kiểm MST/chứng thư → kiểm lượt ký → prepare → Op5 → merge → upload PDF → approval. V1: lấy cert → kiểm tra → genImage → signpdf trên PDF mới nhất → upload → approval.
- V2 dùng `https://127.0.0.1:14424/process`, form `request=base64(JSON)`, kiểm `ResponseCode=0`. `base64TempData` và `hexDigestTempFile` là dữ liệu opaque từ prepare, không hash lại; merge giữ đúng key `filedName`, `isTimestamp` là chuỗi theo contract.
- Kiểm quyền và phiên bản tài liệu ngay trước mutation. Chỉ cho một phiên đang xử lý trên cùng tài liệu; hai tab hoặc hai người ký không được ghi đè PDF của nhau.
- Chọn một cặp upload/approval APP hoặc TP được cấp quyền, không gọi cả hai. Sau upload thành công nhưng approval lỗi, giữ checkpoint để đối soát và tiếp tục approval khi hợp lệ; không bắt ký lại ngay.
- `check-process-is-sign` có side effect marker dù là GET: chỉ gọi khi bắt đầu ký, không đưa vào polling/retry GET chung. Marker lỗi phải theo hướng dẫn nhà cung cấp, không tự sửa trạng thái nội bộ.

### 5.3. Đồng bộ và hoàn tất

- Poll API TP phân trang, đối chiếu đúng EContractId đã lưu; không thấy trên trang đầu không có nghĩa hợp đồng không tồn tại. Tần suất cấu hình theo quota, có backoff và nút kiểm tra thủ công được giới hạn tần suất.
- `status=30` mới là tín hiệu hoàn thành từ nhà cung cấp; lấy documents đúng hợp đồng, chọn FINALLY=2, tải bytes PDF và kiểm định dạng/tài liệu. Link hết hạn thì lấy URL mới.
- Download kiểm host/redirect, timeout và kích thước; chỉ gửi JWT tới endpoint eContract đã xác nhận, không forward JWT sang presigned storage URL. Lưu file ngoài webroot, tải qua endpoint kiểm quyền.
- Hoàn thành Bước 7 khi mapping đúng hai bên, trạng thái nhà cung cấp hoàn thành, đã lưu PDF cuối/hash và metadata ký đối soát được, cùng kết quả xác minh theo chính sách đã chốt. Chưa tải/xác minh xong thì hiển thị “Đã ký trên eContract, đang đồng bộ/kiểm tra”, chưa mở Bước 8.
- Phải chốt ai chịu trách nhiệm kiểm integrity, trust chain, revocation và timestamp: eContract cung cấp bằng chứng/API hoặc Sàn bổ sung bộ xác minh. `check-mst-exist` chỉ đối chiếu định danh. Nếu chưa có bằng chứng xác minh thì giữ `VerificationStatus=Unknown`, không tự gán Valid.
- Không dùng thời điểm polling làm thời điểm ký, không bịa cert serial hoặc signed hash từng bên từ hash PDF cuối. Thiếu dữ liệu cần bổ sung endpoint hoặc giữ trạng thái chưa biết.
- Hoàn thành workflow/audit/thông báo có chống lặp giữa worker và nút kiểm tra. Trạng thái 31/32/40 dừng ký mới; nếu hợp đồng từng hoàn tất rồi bị thanh lý thì giữ lịch sử hoàn tất, không tự lùi quy trình.

## 6. Kế hoạch triển khai theo đầu ra

| Đợt | Công việc | Điều kiện hoàn thành |
|---|---|---|
| 0. Kiểm kê và thử kết nối | Chốt scope Bước 6–7/NDA; rà mọi đường tạo/ký/cập nhật trạng thái cũ; thử create, issue, đọc mapping và hai người ký thật trên dev; xác nhận APP, signer identity, plugin, tọa độ, xác minh và recovery | Có bộ request/response đã che bí mật, PDF hai chữ ký và kết quả kiểm tra; hoặc xác định rõ phần bị chặn bởi nhà cung cấp |
| 1. Nền tảng | Client/DTO, cấu hình, schema, operation checkpoint, quyền backend, frozen PDF, provider theo hợp đồng | Unit/contract tests về response, phân quyền, chống trùng và migration đạt; chưa bật cho người dùng |
| 2. Chốt và phát hành | Tích hợp `ContractService.SetReadyToSignAsync`, UI thông tin người ký/vùng ký, create/issue/reconcile | Mỗi version chỉ có một liên kết eContract; timeout không tạo trùng; chốt đúng PDF và đúng A/B |
| 3. Ký USB | Controller/service và adapter plugin theo phiên bản thực tế của tenant, UI Bước 7, session signer | Hai bên ký lần lượt trên PDF mới nhất; chặn sai user, sai chứng thư, sai field và phiên cũ |
| 4. Đồng bộ và workflow | Worker, download có quyền, xác minh, hoàn tất một lần, khóa đường ký cũ và đường tạm ứng hoàn thành nhầm Bước 7 | Chỉ PDF cuối đáp ứng chính sách mới mở Bước 8; polling chạy lại không ghi chữ ký/thông báo trùng |
| 5. UAT và chuyển đổi | Kiểm thử token thật trên domain triển khai, migration staging, tài liệu vận hành và xử lý lỗi, rollout có cờ | Có biên bản UAT hai bên, kiểm PDF, rollback đã thử và hướng dẫn hỗ trợ |

Không cần bao mọi endpoint/batch trong đợt đầu nếu luồng đã chốt không dùng. Ưu tiên plugin thực tế của tenant; nhánh còn lại triển khai khi thuộc phạm vi hỗ trợ đã xác nhận.

Các file phải rà/sửa: `Program.cs`, `Data/AppDbContext.cs`, entities/enums, `ContractService`, `ContractSigningService`, `ContractController`, `SigningController`, `EContractController`, `ProjectController.UpdateEContractData`, `AdvancePaymentController`, `_Step6.cshtml`, `_Step7.cshtml`, `Views/Signing`, component quản lý hợp đồng và trang hướng dẫn/CMS. Bổ sung services/controller/worker/script migration và kiểm thử tích hợp ở thư mục phù hợp. Không mặc định chỉ sửa một partial là phủ hết giao diện.

## 7. Kiểm thử và tiêu chí nghiệm thu

- Happy path: PDF từ HTML và PDF upload, đúng Buyer/Seller, hai người ký thật, chứng thư đúng, lượt ký đúng, PDF cuối giữ được cả hai chữ ký và được bộ xác minh theo chính sách chấp nhận.
- Quyền: user ngoài dự án, tư vấn, người mua ký ô người bán, thay contractId/recipientId/fieldId/documentId, gọi trực tiếp OTP/CA/route cũ, callback cũ; đều không làm thay đổi hợp đồng ECONTRACT.
- Recovery: create đã thành công nhưng mất response; issue timeout; mất kết nối sau upload; approval timeout; đóng trình duyệt, hết hạn signer session, hai tab và hai worker. Không tạo trùng, không ký đè, không hoàn thành lặp.
- Thiết bị: plugin chưa cài/sai version, TLS/CORS và quyền truy cập mạng local trên trình duyệt đích, sai PIN, rút token, cert sai MST/hết hạn/bị thu hồi theo cơ chế xác minh đã chốt.
- Đồng bộ: HTTP 200/201 nhưng nghiệp vụ lỗi; TP bulk lỗi từng phần tử; không thấy contract ở trang đầu; status=30 nhưng file chưa có, URL hết hạn, PDF rỗng/sai hợp đồng; giữ trạng thái chờ hoặc lỗi, không mở Bước 8.
- Hồi quy: đủ 3 bên duyệt Bước 6, dữ liệu lịch sử tải được, NDA giữ luồng đang dùng, tạo tạm ứng không tự hoàn thành Bước 7, thông báo không trùng.
- Kiểm build/test phù hợp sau khi có code. Đợt rà soát hiện tại không chạy build hoặc ký thử vì chỉ bổ sung Markdown và chưa có phiên tích hợp được cấp.

## 8. Chuyển đổi và rollback

Thêm schema trước, triển khai dưới cờ tắt, nghiệm thu staging rồi bật cho hợp đồng mới theo nhóm dự án. Provider đã chốt không đổi chỉ vì đổi cấu hình mặc định. Không xóa code/bảng/file lịch sử trong đợt đầu; backend vẫn chặn đường cũ trên hợp đồng ECONTRACT.

Rollback tắt tạo mới qua eContract, giữ khả năng đọc và đối soát các hợp đồng đã phát hành. Không đưa hợp đồng đã ký một phần về OTP/LocalSigner để ký tiếp. Backup DB/file và ghi rõ thao tác khôi phục; không drop các cột chứa external ID khi rollback ứng dụng.

## 9. Nội dung cần gửi MobiFone xác nhận

Đây là danh sách để anh/team sử dụng, chưa gửi ra bên ngoài:

1. API APP nào được cam kết cho Sàn gọi; tenant dùng legacy workflow đúng với tài liệu này không? Cấp quyền dev và Production tương ứng.
2. Cách cấp signer session/SSO cho người mua và người bán, người ký bên ngoài có cần tài khoản riêng không; mô hình backend proxy có được hỗ trợ không?
3. Trách nhiệm xác minh PDF/chứng thư, cơ chế OCSP/CRL/timestamp và API/bằng chứng kiểm tra; thời điểm ký/cert metadata lấy ở đâu?
4. Nguồn plugin chính thức, version tenant, OS/browser/driver, TLS/CORS/local network policy trên domain Sàn.
5. Đơn vị/gốc tọa độ, page index/rotation và cách chuyển `coordinate_y` trong mẫu nhiều trang; quyền dùng API `sign/multi/usb-token/page-info` nếu cần.
6. Idempotency/tra cứu sau create timeout, recovery marker, khóa đồng thời, quy trình hủy/thu hồi/tạo lại và API được hỗ trợ.
7. Base URL, auth type, type_id, organization, quota/rate limit, hạn hợp đồng, webhook và danh sách host download được cấp.

Nếu API APP/signer session không được hỗ trợ, dừng phần nhúng ký trong Sàn. Phương án thay thế là tạo/phát hành từ Sàn rồi ký trên giao diện eContract **nếu MobiFone cung cấp đường truy cập người ký được hỗ trợ**; Sàn đồng bộ kết quả. Không tự suy ra URL ký hoặc dùng TP upload như một bộ ký USB hoàn chỉnh.

## 10. Nguồn đối chiếu

- Plan Claude: `.claude/worktrees/api-hop-dong-ky-usb-token-26ca31/docs/PLAN_EContract_USBToken.md`.
- `docs/hop dong dien tu/Dac_ta_tich_hop_API_USB_Token_eContract.docx`: mục II, IV, V, VI và VII, được đọc từ nội dung OOXML.
- `docs/hop dong dien tu/Tạo hợp đồng đơn lẻ không theo mẫu.postman_collection.json`: payload thực tế và base URL dev.
- `docs/hop dong dien tu/USB_TOKEN_EXTERNAL_SIGNING.postman_collection.json`: request, mô tả wire contract và luồng plugin/APP/TP.
- Code tại workspace `D:/2026/CanTho/TechExchangeApp`, các file nêu ở mục 2 và 6. Đây là rà soát tĩnh, không phải xác nhận các API ngoài đã chạy thành công.
