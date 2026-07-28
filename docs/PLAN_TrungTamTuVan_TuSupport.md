# Kế hoạch triển khai: Nâng cấp module hỗ trợ thành ticket hỗ trợ/tư vấn Trung tâm

Ngày lập: 2026-07-28  
Trạng thái: chuẩn bị triển khai theo giai đoạn  
Phạm vi: tái dùng module hỗ trợ/chat hiện có, bổ sung lớp ticket nghiệp vụ.

> **Tiền đề dữ liệu (chốt 2026-07-28):** Toàn bộ dữ liệu chat cũ sẽ bị **xóa sạch** trước go-live — cả `ChatConversations`/`ChatMessages` type=1 (chat sản phẩm) lẫn type=2 (hỗ trợ). Chat sản phẩm (type=1) **hiện không được sử dụng** nên xóa an toàn, không mất dữ liệu nghiệp vụ thật. Vì vậy **không cần tương thích ngược, không cần backfill**; triển khai theo hướng "cài lại từ nền sạch". Các mục về tương thích/thread cũ trong §8, §10, §13 đã được đánh dấu *không áp dụng* theo tiền đề này.

---

## 1. Kết luận kiến trúc

Module hỗ trợ hiện tại **có thể tái sử dụng**, không cần xây một module tư vấn mới từ đầu.

Hệ thống đã có khoảng 70-80% nền tảng cần thiết:

- Nút gửi yêu cầu hỗ trợ trong hồ sơ giao dịch.
- Hội thoại gắn với `ProjectId`.
- Gắn bước phát sinh bằng `StepNumber`.
- Trạng thái tiếp nhận, phân công, trả lời, đóng yêu cầu.
- CMS cho cán bộ Trung tâm xử lý.
- Hạ tầng chat hai chiều giữa doanh nghiệp và cán bộ/tư vấn.

Tuy nhiên, **không nên chỉ đổi chữ** từ "Yêu cầu Sàn hỗ trợ" thành "Yêu cầu Trung tâm tư vấn" rồi xem như hoàn thành. Hỗ trợ chung và tư vấn giao dịch là hai loại nghiệp vụ khác nhau:

- Hỗ trợ chung: lỗi tài khoản, hướng dẫn sử dụng, hỏi quy trình, thao tác hệ thống.
- Tư vấn giao dịch/chuyển giao: tìm nhà cung cấp, đánh giá hồ sơ, tư vấn đàm phán, kiểm tra pháp lý, hỗ trợ hợp đồng điện tử.

Hướng đúng: **giữ module chat/CMS hiện có, bổ sung lớp ticket `SupportRequest` để quản lý nghiệp vụ yêu cầu**, đồng thời phân biệt rõ hỗ trợ chung và tư vấn giao dịch.

---

## 2. Nguyên tắc triển khai

1. Không xóa module chat/support hiện tại.
2. Không xây module tư vấn hoàn toàn mới nếu chưa cần.
3. Không để `ChatConversation` kiêm toàn bộ nghiệp vụ ticket, vì sau này khó quản lý SLA, phí, báo cáo và lịch sử xử lý.
4. Mỗi lần người dùng gửi yêu cầu phải có một bản ghi ticket riêng trong `SupportRequest`.
5. Ticket có thể liên kết tới một `ChatConversation`.
6. Giai đoạn đầu có thể tạo một conversation riêng cho mỗi ticket để đơn giản, nhưng về mô hình dữ liệu vẫn phải tách ticket và chat.
7. Trung tâm chỉ tham gia khi người dùng chủ động gửi yêu cầu.
8. Tự động phân công người xử lý **sau khi đã có yêu cầu** là hợp lệ; việc này không mâu thuẫn với nguyên tắc "Trung tâm không mặc định tham gia mọi giao dịch".
9. Cho cả người mua, người bán và thành viên hợp lệ của hồ sơ gửi yêu cầu; không khóa cứng theo buyer.
10. Chưa triển khai quy trình thu phí/thanh toán khi chủ đầu tư chưa chốt chính thức.

---

## 3. Phân loại yêu cầu

### 3.1. RequestType

Tối thiểu cần phân biệt:

| Mã | Code | Tên hiển thị |
|---|---|---|
| 1 | `GeneralSupport` | Hỗ trợ sử dụng hệ thống |
| 2 | `TransactionConsulting` | Tư vấn giao dịch/chuyển giao |

### 3.2. ServiceType

Áp dụng khi `RequestType = TransactionConsulting`.

| Mã | Code | Tên hiển thị |
|---|---|---|
| 1 | `SupplierMatching` | Tìm kiếm, lựa chọn nhà cung cấp |
| 2 | `ProposalEvaluation` | Đánh giá hồ sơ đề xuất |
| 3 | `NegotiationAdvice` | Tư vấn đàm phán thương mại |
| 4 | `LegalReview` | Kiểm tra pháp lý hợp đồng |
| 5 | `ElectronicContractSupport` | Hỗ trợ hợp đồng điện tử |
| 99 | `Other` | Khác |

Với `GeneralSupport`, có thể dùng `ServiceType = null` hoặc `Other`.

---

## 4. Bảng ticket đề xuất: SupportRequest

Tạo bảng mới `SupportRequests` để quản lý nghiệp vụ ticket, không nhồi toàn bộ metadata vào `ChatConversation`.

| Field | Type | Ghi chú |
|---|---|---|
| `Id` | `BIGINT IDENTITY` | Khóa chính ticket |
| `ProjectId` | `INT NOT NULL` | Hồ sơ giao dịch |
| `RequestedByUserId` | `INT NOT NULL` | Người gửi yêu cầu, có thể là buyer/seller/member |
| `RequestType` | `INT NOT NULL` | Hỗ trợ chung hoặc tư vấn giao dịch |
| `ServiceType` | `INT NULL` | Loại dịch vụ tư vấn |
| `SupportContextCode` | `NVARCHAR(100) NULL` | Mã bước/ngữ cảnh nghiệp vụ ổn định |
| `DisplayStepNumber` | `INT NULL` | Số bước đang hiển thị cho người dùng |
| `InternalStepNumber` | `INT NULL` | Bước nội bộ cũ nếu cần map 7/14 bước |
| `Subject` | `NVARCHAR(300) NULL` | Tiêu đề ngắn |
| `Description` | `NVARCHAR(MAX) NULL` | Nội dung người dùng nhập |
| `Status` | `INT NOT NULL` | Trạng thái ticket |
| `AssignedStaffUserId` | `INT NULL` | Cán bộ/tư vấn phụ trách |
| `ConversationId` | `BIGINT NULL` | Liên kết `ChatConversations.Id` |
| `IsPrivateToRequester` | `BIT NOT NULL DEFAULT 1` | Mặc định ticket riêng với người gửi và cán bộ |
| `IsChargeable` | `BIT NULL` | Chỉ để cấu hình tương lai, chưa hiển thị quy trình phí |
| `FeePolicy` | `NVARCHAR(200) NULL` | Ghi chú/chính sách phí nếu sau này cần |
| `AssignedAt` | `DATETIME2 NULL` | Mốc gán cán bộ — phục vụ đo SLA (xem §13.5) |
| `FirstRespondedAt` | `DATETIME2 NULL` | Mốc cán bộ phản hồi lần đầu — đo SLA phản hồi |
| `DueAt` | `DATETIME2 NULL` | Hạn xử lý dự kiến (nullable, dành cho SLA sau) |
| `LastStatusChangedByUserId` | `INT NULL` | Ai đổi trạng thái gần nhất — audit (xem §13.6) |
| `LastStatusChangedAt` | `DATETIME2 NULL` | Thời điểm đổi trạng thái gần nhất |
| `CreatedAt` | `DATETIME2 NOT NULL` | Ngày tạo |
| `UpdatedAt` | `DATETIME2 NULL` | Ngày cập nhật |
| `ClosedAt` | `DATETIME2 NULL` | Ngày đóng |

> Chỉ mục đề xuất cho `SupportRequests` (idempotent, cùng phong cách `add-support-chat-columns.sql`): `IX(ProjectId)`, `IX(AssignedStaffUserId, Status)`, `IX(RequestType, Status, CreatedAt)`, `IX(ConversationId)`. Xem §13.7.

### 4.1. Trạng thái ticket

Không nên chỉ có Open/Closed. Dùng bộ trạng thái đủ cho CMS vận hành:

| Mã | Code | Tên hiển thị |
|---|---|---|
| 1 | `New` | Mới tiếp nhận |
| 2 | `Assigned` | Đã phân công |
| 3 | `InProgress` | Đang xử lý |
| 4 | `WaitingForUser` | Chờ doanh nghiệp bổ sung |
| 5 | `Responded` | Đã phản hồi |
| 6 | `Completed` | Đã hoàn thành |
| 7 | `Cancelled` | Đã hủy |

### 4.2. Quan hệ với ChatConversation

Giai đoạn đầu:

- Mỗi ticket tạo một `ChatConversation` riêng để đơn giản UX và phân quyền.
- `SupportRequest.ConversationId` trỏ tới conversation.
- `ChatConversation.ProjectId` vẫn giữ để tương thích màn hình chat hiện có.
- Có thể bổ sung `ChatConversation.SupportRequestId` nếu cần truy ngược nhanh.

Sau này nếu muốn nhiều ticket dùng chung một phòng trao đổi, chỉ cần thay logic mapping mà không phá mô hình ticket.

---

## 5. Ánh xạ bước nghiệp vụ

Không nên gắn cứng dữ liệu tư vấn bằng số bước 1-6, vì hệ thống cũ có 7/14 bước và có thể thay đổi hiển thị.

Ticket nên lưu:

- `SupportContextCode`: mã nghiệp vụ ổn định, ví dụ `SUPPLIER_MATCHING`, `PROPOSAL_EVALUATION`, `NEGOTIATION`, `LEGAL_REVIEW`, `E_CONTRACT`.
- `DisplayStepNumber`: số bước đang hiển thị cho người dùng, ví dụ 5.
- `InternalStepNumber`: bước nội bộ cũ nếu cần, ví dụ legal review có thể là step 6 trong workflow cũ.

### Bảng map tham chiếu

| Quy trình hiển thị mới | Mã context đề xuất | Bước nội bộ cũ |
|---|---|---|
| B1. Gửi yêu cầu báo giá/yêu cầu chuyển giao | `REQUEST_INIT` | Step khởi tạo tương ứng |
| B2. NDA nếu một bên yêu cầu | `NDA` | NDA |
| B3. RFQ | `RFQ` | Tạo/gửi RFQ |
| B4. Hồ sơ đề xuất | `PROPOSAL_EVALUATION` | Nộp/đánh giá đề xuất |
| B5. Đàm phán và pháp lý | `NEGOTIATION`, `LEGAL_REVIEW` | Có thể gom nhiều bước nội bộ |
| B6. Hợp đồng điện tử | `E_CONTRACT` | Ký hợp đồng |

---

## 6. UX đề xuất

Không nên xóa hỗ trợ chung. Dùng một nút tổng quát:

**Yêu cầu Trung tâm hỗ trợ**

Trong modal hỏi:

**Bạn cần hỗ trợ nội dung nào?**

- Hướng dẫn sử dụng hệ thống.
- Tư vấn giao dịch/chuyển giao.

Nếu chọn **Hướng dẫn sử dụng hệ thống**:

- Chỉ cần chọn bước/ngữ cảnh nếu có.
- Nhập nội dung cần hỗ trợ.

Nếu chọn **Tư vấn giao dịch/chuyển giao**:

- Hiện thêm `ServiceType`.
- Hiện bước/ngữ cảnh giao dịch.
- Nhập tiêu đề/nội dung cần tư vấn.

### Wording cần dùng

- Nút: `Yêu cầu Trung tâm hỗ trợ`
- Request type 1: `Hỗ trợ sử dụng hệ thống`
- Request type 2: `Tư vấn giao dịch/chuyển giao`
- CMS menu: `Yêu cầu hỗ trợ/tư vấn`
- Không đổi toàn bộ chữ "hỗ trợ" thành "tư vấn", vì như vậy làm mất chức năng hỗ trợ kỹ thuật/hướng dẫn sử dụng.

---

## 7. Quyền truy cập

Đây là phần bắt buộc phải kiểm tra kỹ.

### 7.1. Quyền tạo ticket

Chỉ user có quyền hợp lệ với hồ sơ mới được tạo ticket:

- Buyer/creator.
- Seller được chọn.
- Seller được mời và đủ điều kiện truy cập hồ sơ.
- Consultant/member đang active trong `ProjectMembers`.
- Các role CMS/admin theo policy hiện có nếu cần.

Không được cho người ngoài đoán `ProjectId` để mở hội thoại.

### 7.2. Quyền xem ticket

- Người gửi ticket được xem ticket của mình.
- Cán bộ được phân công được xem ticket.
- CMS/admin có phạm vi quản trị được xem theo policy.
- Buyer không mặc định được xem ticket riêng do seller gửi nếu ticket đánh dấu riêng tư.
- Seller không mặc định được xem ticket riêng do buyer gửi nếu ticket đánh dấu riêng tư.

### 7.3. File đính kèm

Nếu sau này ticket có file pháp lý, báo giá, NDA:

- Kiểm tra quyền tải file theo `SupportRequest`.
- Không dùng link public nếu file chứa thông tin nhạy cảm.
- Khi đóng ticket vẫn giữ lịch sử tin nhắn, file và người xử lý.

---

## 8. Giai đoạn triển khai

### Giai đoạn 1 - Chuẩn bị nền ticket

Mục tiêu: thêm lớp `SupportRequest`, chưa đổi nhiều UI.

Việc cần làm:

1. Tạo SQL script idempotent:
   - `TechExchangeApp/Scripts/add-support-request-table.sql`
2. Tạo entity `SupportRequest`.
3. Tạo enum:
   - `SupportRequestType`
   - `SupportServiceType`
   - `SupportRequestStatus`
4. Thêm `DbSet<SupportRequest>`.
5. Khi tạo support hiện tại, đồng thời tạo một `SupportRequest` liên kết conversation.
6. ~~Backward compatible: conversation support cũ không có ticket vẫn xem được.~~ *Không áp dụng — chat cũ đã xóa sạch (xem tiền đề đầu file).*

Tiêu chí nghiệm thu:

- Build sạch.
- Tạo yêu cầu hỗ trợ mới hoạt động.
- Database có ticket mới cho mỗi request mới.
- Không có đường tạo conversation type=2 nào thiếu `SupportRequest` đi kèm.

### Giai đoạn 2 - Phân biệt hỗ trợ chung và tư vấn giao dịch

Mục tiêu: bổ sung phân loại request đúng nghiệp vụ.

Việc cần làm:

1. Modal thêm lựa chọn `RequestType`.
2. Nếu chọn tư vấn giao dịch thì hiện dropdown `ServiceType`.
3. API nhận `requestType`, `serviceType`, `subject`, `description`.
4. CMS inbox hiển thị:
   - Loại yêu cầu.
   - Loại dịch vụ tư vấn.
   - Người gửi.
   - Hồ sơ.
   - Trạng thái.
5. Không đổi toàn bộ wording "hỗ trợ" thành "tư vấn".

Tiêu chí nghiệm thu:

- Tạo được ticket hỗ trợ chung.
- Tạo được ticket tư vấn giao dịch.
- CMS phân biệt được hai loại.
- Cả buyer và seller/member hợp lệ đều gửi được.

### Giai đoạn 3 - Ticket độc lập, chat liên kết

Mục tiêu: mỗi yêu cầu nghiệp vụ là một ticket riêng, nhưng chat chỉ là kênh trao đổi.

Việc cần làm:

1. Sửa logic không tìm active conversation theo `ProjectId + BuyerUserId`.
2. Tạo ticket mới cho mỗi submit.
3. Tạo conversation riêng cho ticket ở phase đầu.
4. Lưu `ConversationId` vào ticket.
5. CMS thao tác theo `SupportRequest.Id`, không chỉ theo `ChatConversation.Id`.
6. Có thể thêm mã hiển thị: `REQ-{Id}`.

Tiêu chí nghiệm thu:

- Cùng hồ sơ, cùng user gửi hai yêu cầu khác nhau -> có hai ticket.
- CMS báo cáo theo ticket, không phụ thuộc hoàn toàn vào chat.
- Chat vẫn dùng được để trao đổi từng ticket.

### Giai đoạn 4 - Mở rộng trạng thái xử lý

Mục tiêu: CMS theo dõi được yêu cầu đang mắc ở đâu.

Việc cần làm:

1. CMS cho cập nhật status:
   - Mới tiếp nhận.
   - Đã phân công.
   - Đang xử lý.
   - Chờ doanh nghiệp bổ sung.
   - Đã phản hồi.
   - Đã hoàn thành.
   - Đã hủy.
2. Khi reply lần đầu có thể chuyển `New/Assigned` sang `InProgress` hoặc `Responded`.
3. Khi đóng thì set `Completed` hoặc `Cancelled`, lưu `ClosedAt`.

Tiêu chí nghiệm thu:

- CMS lọc được ticket theo status.
- Trạng thái ticket không chỉ phụ thuộc Open/Closed.
- Lịch sử chat không mất khi ticket đóng.

### Giai đoạn 5 - Phí dịch vụ, chỉ chuẩn bị cấu hình

Mục tiêu: không triển khai thu phí khi chưa được chốt, chỉ để chỗ mở rộng.

Việc cần làm:

1. Có thể thêm nullable/cấu hình:
   - `IsChargeable`
   - `FeePolicy`
2. Không làm:
   - Mức phí chi tiết.
   - Báo phí.
   - Chấp thuận phí.
   - Thanh toán dịch vụ.
3. Chỉ triển khai quy trình phí khi chủ đầu tư xác nhận:
   - Dịch vụ nào thu phí.
   - Đơn vị thu.
   - Căn cứ thu.
   - Quy trình tài chính.

Tiêu chí nghiệm thu:

- Không xuất hiện checkout/thanh toán.
- Không làm hệ thống hiểu sai phạm vi dịch vụ công.
- Có điểm mở rộng nếu sau này cần.

### Giai đoạn 6 - Báo cáo và tra cứu

Mục tiêu: phục vụ vận hành Trung tâm.

Việc cần làm:

1. Bộ lọc CMS:
   - RequestType.
   - ServiceType.
   - Status.
   - AssignedStaffUserId.
   - ProjectId.
   - Date range.
2. Dashboard:
   - Tổng ticket.
   - Ticket hỗ trợ chung.
   - Ticket tư vấn giao dịch.
   - Ticket chưa gán.
   - Ticket đang xử lý.
   - Ticket đã hoàn thành.
3. Export Excel nếu cần.

---

## 9. File dự kiến

### File mới

- `TechExchangeApp/Scripts/add-support-request-table.sql`
- `TechExchangeApp/Entities/SupportRequest.cs`
- `TechExchangeApp/Enums/SupportRequestType.cs`
- `TechExchangeApp/Enums/SupportServiceType.cs`
- `TechExchangeApp/Enums/SupportRequestStatus.cs`

### File sửa

- `TechExchangeApp/Data/AppDbContext.cs`
- `TechExchangeApp/Controllers/SupportController.cs`
- `TechExchangeApp/Services/ChatService.cs`
- `TechExchangeApp/Interfaces/IChatService.cs`
- `TechExchangeApp/Areas/Cms/Controllers/SupportRequestsAdminController.cs`
- `TechExchangeApp/Views/Project/Details.cshtml`
- `TechExchangeApp/Views/Chat/Index.cshtml`
- `TechExchangeApp/Views/Chat/Conversation.cshtml`
- `TechExchangeApp/Areas/Cms/Views/SupportRequestsAdmin/Index.cshtml`
- `TechExchangeApp/Areas/Cms/Views/SupportRequestsAdmin/Thread.cshtml`
- `TechExchangeApp/Areas/Cms/Views/Shared/_LayoutAdminLTE.cshtml`

---

## 10. Checklist kiểm thử

### Member

- Buyer tạo được ticket hỗ trợ chung.
- Buyer tạo được ticket tư vấn giao dịch.
- Seller/member hợp lệ tạo được ticket tư vấn giao dịch.
- User không có quyền hồ sơ không tạo được ticket.
- Ticket tạo xong mở được chat liên kết.
- Nội dung `<script>` hiển thị dạng text, không chạy script.

### CMS

- CMS thấy ticket mới.
- CMS phân biệt hỗ trợ chung và tư vấn giao dịch.
- CMS thấy service type của tư vấn giao dịch.
- CMS gán được người phụ trách.
- CMS reply được.
- CMS cập nhật trạng thái.
- CMS đóng ticket và vẫn xem được lịch sử.

### Phân quyền

- Người gửi chỉ xem ticket của mình.
- Buyer không tự động xem ticket riêng do seller gửi.
- Seller không tự động xem ticket riêng do buyer gửi.
- Cán bộ chỉ xem ticket được phân công hoặc thuộc quyền CMS/admin.
- Không truy cập được ticket bằng cách đoán `ProjectId` hoặc `ConversationId`.

### Tương thích *(không áp dụng — chat cũ đã xóa sạch, xem tiền đề đầu file)*

- ~~Product chat cũ vẫn chạy.~~
- ~~Support conversation cũ chưa có `SupportRequest` vẫn mở được.~~
- ~~Danh sách chat không crash với conversation support chưa gán.~~

Thay bằng kiểm thử nền sạch:

- Sau khi xóa dữ liệu, tạo chat sản phẩm mới (type=1) hoạt động bình thường.
- Tạo yêu cầu hỗ trợ mới (type=2) luôn sinh kèm `SupportRequest`.

---

## 11. Không nên làm ngay

- Không xây module tư vấn hoàn toàn mới.
- Không để mỗi lần bấm chỉ tạo chat rời rạc mà không có ticket quản lý.
- Không triển khai thu phí/thanh toán khi chưa có yêu cầu chính thức.
- Không đổi toàn bộ "hỗ trợ" thành "tư vấn".
- Không chỉ cho buyer gửi yêu cầu.
- Không gắn cứng dữ liệu bằng số bước 1-6.
- Không cho chat/ticket chứa file nhạy cảm bằng link public.

---

## 13. Điều chỉnh bắt buộc sau rà soát code hiện có

> Mục này bổ sung sau khi đối chiếu kế hoạch với code đang chạy: `SupportController`, `ChatService.StartSupportConversationAsync`, `SupportRequestsAdminController`, entity `ChatConversation`, script `add-support-chat-columns.sql`, các enum `SupportConversationStatus`/`ChatConversationType`. Định hướng tổng thể **không đổi**; đây là các chỗ phải chèn thêm để không vỡ khi triển khai và đủ độ chuyên nghiệp.

### 🔴 Blocking — mâu thuẫn trực tiếp với code hiện tại

**13.1. Bỏ unique index + logic dedup đang chạy (Giai đoạn 3).**
Hiện tại hệ thống **chặn cứng** việc có 2 hội thoại hỗ trợ mở cho cùng (dự án, người gửi):
- Unique filtered index `UX_ChatConversations_Support_Active` trên `(ProjectId, BuyerUserId)` — `add-support-chat-columns.sql`.
- Nhánh tái dùng `existing` trong `StartSupportConversationAsync` — `ChatService.cs`.

Việc cần làm:
- Không tạo lại `UX_ChatConversations_Support_Active` khi dựng nền sạch (hoặc drop nếu còn). *Không cần bước dọn trùng vì dữ liệu cũ đã xóa.*
- Gỡ nhánh tái dùng `existing`; mỗi submit tạo một `SupportRequest` + conversation riêng.
- Nếu vẫn muốn chống trùng, chuyển ràng buộc "1 active" sang tầng ticket theo nghiệp vụ, không theo `(ProjectId, BuyerUserId)`.

**13.2. Chốt nguồn sự thật về trạng thái.**
Vẫn tồn tại song song 2 cột: `ChatConversation.SupportStatus` (None/Unassigned/InProgress/Closed) và `SupportRequest.Status` (7 giá trị). CMS hiện lọc theo `ChatConversation.SupportStatus`.
- **Chốt: `SupportRequest.Status` là nguồn sự thật.**
- Cho tới khi CMS đọc hoàn toàn theo ticket: mỗi lần đổi `SupportRequest.Status` thì đồng bộ ngược `ChatConversation.SupportStatus` (map: New/Assigned→Unassigned/InProgress; Completed/Cancelled→Closed) để màn chat không lệch.
- *Không phải xử lý dữ liệu lịch sử — nền dựng sạch nên chỉ cần đảm bảo dữ liệu mới luôn nhất quán.*

**13.3. ~~Backfill ticket cho conversation hỗ trợ cũ~~ — KHÔNG ÁP DỤNG.**
Theo tiền đề xóa sạch chat cũ, **không còn conversation type=2 lịch sử để backfill**. Thay bằng yêu cầu:
- Đảm bảo mọi conversation hỗ trợ *tạo mới* đều luôn kèm một `SupportRequest` (không có đường tạo conversation type=2 "trần" không ticket).
- CMS inbox mới chỉ cần liệt kê theo `SupportRequest`, không phải lo hiển thị dữ liệu cũ.

### 🟠 Nên bổ sung — mức độ chuyên nghiệp vận hành

**13.4. Tách luồng GeneralSupport vs TransactionConsulting theo pool nhân sự.**
Auto-assign hiện lấy `ProjectMembers Role=Consultant` và dropdown gán chỉ lấy `NhaTuVans`. "Hỗ trợ sử dụng hệ thống" **không nên** giao cho tư vấn viên nghiệp vụ.
- GeneralSupport → pool hỗ trợ kỹ thuật/CMS; TransactionConsulting → tư vấn viên.
- Chỉ auto-assign consultant khi `RequestType=TransactionConsulting`; GeneralSupport để `Unassigned` cho CMS điều phối.

**13.5. Cột đo SLA** (đã thêm vào schema §4): `AssignedAt`, `FirstRespondedAt`, `DueAt`. Set `AssignedAt` khi gán, `FirstRespondedAt` ở reply đầu tiên của cán bộ. Không có các mốc này thì dashboard §8-GĐ6 không đo được thời gian phản hồi.

**13.6. Audit chuyển trạng thái** (đã thêm `LastStatusChangedByUserId`/`LastStatusChangedAt` vào §4). Với ticket dính pháp lý/hợp đồng, cần biết "ai đổi trạng thái, lúc nào". Có thể nâng lên bảng `SupportRequestHistory` nếu sau này cần lịch sử đầy đủ — ghi nhận là điểm mở rộng có chủ đích.

**13.7. Index bảng mới** (đã ghi ở §4). Phục vụ đúng bộ lọc CMS ở Giai đoạn 6.

**13.8. Thông báo cho người gửi.**
Hiện `_notifQueue` chỉ báo cho **cán bộ**. Doanh nghiệp gửi yêu cầu không được báo khi có phản hồi.
- Bổ sung **Giai đoạn 4**: notify người gửi khi cán bộ reply và khi đổi trạng thái sang `Responded`/`Completed`.

### 🟡 Làm cho chặt

- **13.9. Validate server-side:** kiểm tra `RequestType`/`ServiceType` trong phạm vi enum; `ServiceType` bắt buộc khi `RequestType=TransactionConsulting`; `Subject`/`Description` bắt buộc. Bổ sung vào checklist §10.
- **13.10. Kiểu userId:** `ChatConversation.BuyerUserId` là `NVARCHAR(450)`, còn `SupportRequest.*UserId` là `INT` (khớp `ApplicationUser.Id`). Phải parse khi map qua lại — ghi rõ trong service.
- **13.11. Đổi "Sàn" → "Trung tâm":** ngoài wording ở §6, rà các điểm chạm chữ "Sàn"/"Yêu cầu hỗ trợ": comment entity, summary controller, và system message trong `StartSupportConversationAsync`. Vẫn giữ nguyên tắc **không** blanket-replace "hỗ trợ"→"tư vấn".
- **13.12. FK constraints:** theo quy ước "không EF migration", chốt trong script mới: thêm FK `ProjectId`→`Projects`, `ConversationId`→`ChatConversations` (hoặc chỉ index) — thống nhất một kiểu.
- **13.13. Chống double-submit:** khi bỏ dedup, thêm guard chống bấm nhanh tạo ticket rác (debounce nút phía UI + kiểm tra tối thiểu phía server).
- **13.14. Xóa dữ liệu chat cũ là bước có kiểm soát:** viết script xóa riêng, chạy có chủ đích trước go-live (không TRUNCATE ad-hoc lúc deploy). **Backup `ChatConversations`/`ChatMessages` sang bảng `*_DeletedBackup_<ngày>` trước khi xóa** (cùng cách đã làm với `Users_DeletedBackup_20260716`), phòng khi cần đối chiếu/khôi phục.

---

## 14. Kết luận chốt để giao triển khai

Hướng triển khai nên là:

**Giữ chat và CMS hiện có -> bổ sung bảng ticket `SupportRequest` -> phân biệt hỗ trợ chung và tư vấn giao dịch -> cho cả người mua/người bán/thành viên hợp lệ gửi -> gắn loại dịch vụ và mã bước nghiệp vụ -> chưa triển khai phí khi chưa được chủ đầu tư chốt.**

