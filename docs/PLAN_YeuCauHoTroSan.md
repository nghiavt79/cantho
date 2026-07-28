# Kế hoạch V3: Tính năng "Yêu cầu Sàn hỗ trợ"

> V3 siết thêm: dùng sentinel `SupplierUserId="0"` (không dùng chuỗi rỗng), nâng cấp ViewModel chat,
> **CMS reply chống race bằng conditional UPDATE atomic** (không chỉ BeginTransaction),
> encode toàn bộ nội dung động, access mirror `Details`, script SQL idempotent tuyệt đối.
> Xem mục **B0 — Nguyên tắc bắt buộc** trước khi code.



> Thành viên dự án bấm **"Yêu cầu Sàn hỗ trợ"** ở **bất kỳ bước nào**, hệ thống tạo/mở lại
> một **hội thoại hỗ trợ** gắn dự án + bước, **trao đổi 2 chiều ngay trên sàn**.
> Đã có tư vấn/đầu mối → gửi thẳng. Chưa có → nằm ở **inbox CMS dạng "chưa gán"**;
> nhân viên CMS đầu tiên trả lời trở thành đầu mối.
>
> Hướng: **tái dùng hạ tầng Chat**, KHÔNG dùng Feedback (một chiều).

Ngày lập: 2026-07-27 · Bản: **V3 (đã siết an toàn, đối chiếu code)** · Trạng thái: *chờ duyệt để code*

---

## PHẦN A — HIỆN TRẠNG (đã kiểm chứng trong code)

### A1. Trang dự án
- `Views/Project/Details.cshtml`, VM `ProjectDetailWithStepsVm`: có `CurrentStep`, `UserRole`, `Steps`; khu nút `<div class="tx-project-actions">`.
- **Quyền vào Details giàu hơn ProjectMembers** — `ProjectController.cs:144-192`: creator / `ProjectMembers` active / `SelectedSellerId` / **invited seller qua `RFQInvitations`** (kèm check NDA). ⇒ endpoint hỗ trợ phải mirror đúng logic này.

### A2. Chat (tái dùng)
- `ChatConversation`: `Id(long)`, `ProductId?`, `ProductType?`, `BuyerUserId(string)`, `SupplierUserId(string)`, `ProductName?`, `IsFromProductDetail`, `Created`, `LastMessageAt`.
- `ChatMessage`: `ConversationId`, `SenderUserId`, `Message`, `IsRead`, `IsSystem`, `Created`.
- `ChatController`: `/chat`, `/chat/{id}`, `POST /api/chat/start`, `POST /api/chat/{id}/send`, `GET /api/chat/{id}/messages`, `GET /api/chat/unread-count`.
- Views: `Views/Chat/Index.cshtml`, `Views/Chat/Conversation.cshtml`.

**Điểm code cần lưu (đã xác minh):**
| Hàm/chỗ | Thực tế | Hệ quả |
|---|---|---|
| `Conversation.cshtml:56-62` | `IsSystem` → `@Html.Raw`; tin thường → `@m.Message` (đã encode) | ⚠️ **Nội dung user phải để `IsSystem=false`** (tránh XSS) |
| `GetConversationsAsync` | `int.Parse(otherUserId)` | ❗ Crash nếu đầu kia rỗng → **cần vá** |
| `GetConversationAsync` | `int.Parse(otherUserId)` | ❗ Crash nếu đầu kia rỗng → **cần vá** |
| `SendMessageAsync` | đã `int.TryParse` | ✅ An toàn; chỉ cần chỉnh câu notify cho support |
| `GetUnreadCountAsync` | so sánh **chuỗi**, không parse | ✅ **Đã an toàn — KHÔNG cần sửa** |
| JS poll `appendMessage` | `escapeHtml` bằng `textContent` | ✅ Tin poll đã escape |

### A3. Feedback — KHÔNG hợp
- `Feedback`: form một chiều, không gắn dự án/bước, không có luồng trả lời → loại.

### A4. Tư vấn / đầu mối — ⚠️ điểm dễ sai
- **Gán tư vấn chạy thật qua `ProjectMembers` với `Role = ProjectRole.Consultant (3)`** — `ProjectMemberService.AddConsultantAsync:75` **CHỈ ghi `ProjectMembers`, KHÔNG ghi bảng `ProjectConsultants`**.
- ⇒ Nguồn xác định đầu mối **ưu tiên `ProjectMembers` Role=3 active**; `ProjectConsultants` chỉ là phụ (có thể rỗng).
- `ProjectRole`: Buyer=1, Seller=2, Consultant=3.

### A5. Thông báo
- `INotificationQueueService.QueueAsync(userId, projectId?, title, content, channel, url?)`.

### A6. CMS
- Controller CMS: `[Area("Cms")] [Authorize(Policy="CmsAccess")]`.
- Menu trái hardcode trong `_LayoutAdminLTE.cshtml`, nhóm **"Giao dịch"**.

---

## PHẦN B — KẾ HOẠCH

### B0. Nguyên tắc bắt buộc (10 điểm siết ở V3)
1. **Không dùng `SupplierUserId=""`.** "Chưa gán" = `AssignedStaffUserId=null`, `SupportStatus=1`, `SupplierUserId="0"` (sentinel). Mọi chỗ resolve user id phải `int.TryParse` **và `> 0`**. (`SupplierUserId` là `string` non-null — [ChatConversation.cs:23](../TechExchangeApp/Entities/ChatConversation.cs).)
2. **Product chat phải gắn type ngay từ query cũ.** `StartConversationAsync`: tạo với `ConversationType=1`; **query existing thêm `ConversationType==1`** ([ChatService.cs:47](../TechExchangeApp/Services/ChatService.cs#L47)) để không lẫn với support.
3. **Nâng cấp ViewModel** ([Interfaces/IChatService.cs](../TechExchangeApp/Interfaces/IChatService.cs)): thêm vào `ChatConversationVm` **và** `ChatConversationDetailVm`: `ConversationType`, `ProjectId`, `ProjectName`, `StepNumber`, `SupportStatus`, `AssignedStaffUserId`. Không có thì view không render đúng support.
4. **Không render raw user content ở bất kỳ luồng nào.** System message support **chỉ server tạo**: `HtmlEncoder.Default.Encode(project.ProjectName)`, URL build nội bộ, step là int. User message + CMS reply **luôn `IsSystem=false`** (view `@Html.Raw` chỉ cho `IsSystem=true`).
5. **Access endpoint = đúng ai vào được `Details`.** Thứ tự: project tồn tại → `ProjectMembers` active → `SelectedSellerId==userId` → invited seller active (`RFQInvitations`) **+ đã ký NDA** → consultant `ProjectMembers.Role=3`. Seller non-selected chưa ký NDA → **API trả 403** (không redirect SignNda như Details). Nguyên tắc vàng: chỉ người thực sự vào được Details mới tạo được support.
6. **CMS reply chống race bằng conditional UPDATE atomic, ĐẶT TRONG transaction ngắn.** `BeginTransaction` một mình không đủ (READ COMMITTED vẫn lọt lost-update), nhưng vẫn cần transaction ngắn để tránh "nhận nhưng chưa có message". Trình tự:
   ```
   begin transaction
     UPDATE ChatConversations
     SET AssignedStaffUserId=@me, SupplierUserId=@meStr, SupportStatus=2
     WHERE Id=@id AND ConversationType=2 AND SupportStatus=1 AND AssignedStaffUserId IS NULL;
     -- rowsAffected=1 → mình vừa nhận, đi tiếp
     -- rowsAffected=0 → reload: nếu AssignedStaffUserId==me → đi tiếp; khác → chặn mềm, rollback, KHÔNG add message
     add ChatMessage(IsSystem=false); update LastMessageAt; SaveChanges
   commit
   -- notify requester SAU commit; notify fail không rollback message
   ```
   `WHERE` siết đủ `ConversationType=2 AND SupportStatus=1` để không lỡ update nhầm thread đã đóng/lệch trạng thái. (EF Core `ExecuteUpdate`/`ExecuteSqlRaw` trong transaction.)
7. **SQL script idempotent tuyệt đối:** `COL_LENGTH` trước khi `ADD`; `sys.indexes` trước khi tạo index; default constraint đặt tên rõ; backfill sau khi add; **unique filtered index chỉ tạo khi không còn duplicate active** — nếu có dup thì `PRINT` cảnh báo, **không làm chết deploy**.
8. **CMS inbox KHÔNG gọi `MarkAsReadAsync`** (vì `IsRead` là cờ global trên message, sẽ lật trạng thái chưa đọc của member). CMS chỉ hiển thị thread + reply.
9. **`GetUnreadCountAsync` giữ nguyên** — so sánh chuỗi, với sentinel `"0"` (không ai đăng nhập id 0) là an toàn; support chưa gán không tính vào ai.
10. **URL notify theo vai:** member → `/chat/{id}`; staff/CMS → `/cms/SupportRequestsAdmin/Thread/{id}`.

### B1. DB (SQL script, idempotent, không EF migration)
File `TechExchangeApp/Scripts/add-support-chat-columns.sql`. Thêm cột vào `ChatConversations`:

| Cột | Kiểu | Ý nghĩa |
|---|---|---|
| `ConversationType` | `INT NOT NULL DEFAULT 1` | 1=chat sản phẩm, 2=hỗ trợ Sàn |
| `ProjectId` | `INT NULL` | dự án của yêu cầu |
| `StepNumber` | `INT NULL` | bước phát sinh |
| `SupportStatus` | `INT NOT NULL DEFAULT 0` | 0=n/a, 1=chưa gán, 2=đang xử lý, 3=đã đóng (để dành) |
| `AssignedStaffUserId` | `INT NULL` | nhân viên/tư vấn phụ trách (field nghiệp vụ chính) |

Yêu cầu script:
- `IF COL_LENGTH(...) IS NULL` mới `ADD` (idempotent).
- Backfill dữ liệu cũ: `ConversationType=1`, `SupportStatus=0`.
- Index (tạo nếu chưa có):
  - `(ConversationType, ProjectId, BuyerUserId)`
  - `(ConversationType, SupportStatus, LastMessageAt)` — cho inbox CMS
  - Unique filtered (1 support active / requester / project):
    trên `(ProjectId, BuyerUserId)` với predicate chắc chạy mọi bản SQL Server (tránh `IN`):
    `WHERE ConversationType = 2 AND ProjectId IS NOT NULL AND SupportStatus >= 1 AND SupportStatus <= 2`
- Không đụng dữ liệu cũ.

### B2. Entity + Enum
- `Entities/ChatConversation.cs`: thêm `ProjectId?`, `StepNumber?`, `ConversationType=1`, `SupportStatus=0`, `AssignedStaffUserId?`.
- Enum mới:
```csharp
public enum ChatConversationType { Product = 1, PlatformSupport = 2 }
public enum SupportConversationStatus { None = 0, Unassigned = 1, InProgress = 2, Closed = 3 }
```
- `SupplierUserId` vẫn set = staff id (string) khi đã gán để `/chat/{id}` phía member tái dùng được; **nghiệp vụ dựa vào `AssignedStaffUserId`**.

### B3. Vá `ChatService` (chỉ những hàm THỰC SỰ cần)
- Helper nội bộ: `IsPlatformSupport(c)`, `IsUnassignedSupport(c)` (type=2 & `SupplierUserId` không parse ra `>0`), `ResolveUserDisplayNameAsync(userId)`, `ResolveSupportProjectTitleAsync(c)`.
- `StartConversationAsync` (product): tạo `ConversationType=1`; **query existing thêm `ConversationType==1`** (B0.2).
- `GetConversationsAsync`: nếu type=2 → tiêu đề = tên dự án/"Hỗ trợ"; đối tác member = **"Sàn hỗ trợ"** khi chưa gán; resolve user id bằng `TryParse` **và `>0`** (sentinel `"0"` không parse thành user).
- `GetConversationAsync`: member xem nếu là `BuyerUserId`; staff xem nếu là `SupplierUserId`; resolve tên người gửi bằng **`TryParse`** an toàn.
- `SendMessageAsync`: nhánh support → member gửi & đã gán → notify staff; member gửi & chưa gán → chỉ update `LastMessageAt`, không push; product chat giữ nguyên. Chỉnh câu notify cho support (thay vì "về sản phẩm…").
- **`GetUnreadCountAsync`: KHÔNG sửa** (đã an toàn — đối chiếu A2).
- `MarkAsReadAsync`: giữ nguyên cho member; CMS dùng luồng đọc riêng, không phụ thuộc `IsRead`.

### B4. Access helper (mirror Details)
Tách helper `CanAccessProjectAsync(projectId, userId)` (private trong `SupportController` giai đoạn đầu; có thể nâng thành `ProjectAccessHelper` sau), cho phép nếu:
- creator/buyer, hoặc `ProjectMembers` active, hoặc `SelectedSellerId==userId`, hoặc **invited seller `RFQInvitations` active + đã ký NDA** (đúng B0.5; seller non-selected chưa ký NDA → API trả **403**, không redirect), hoặc `ProjectMembers` Role=3 (tư vấn).
> Bám sát đúng nhánh của `ProjectController.Details` để không cấp/chặn sai.

### B5. Xác định đầu mối khi start
1. Tìm đầu mối: **`ProjectMembers` Role=3 active trước** (ổn định theo `JoinedDate`); phụ: `ProjectConsultants` active nếu có.
2. Có đầu mối → `AssignedStaffUserId=staffId`, `SupplierUserId=staffId.ToString()`, `SupportStatus=InProgress`, notify đầu mối.
3. Chưa có → `AssignedStaffUserId=null`, `SupplierUserId="0"` (sentinel, xem B0.1), `SupportStatus=Unassigned`, không push, hiện ở inbox CMS.

### B6. API tạo yêu cầu — `Controllers/SupportController.cs`
`POST /api/support/start` body `{ projectId, stepNumber?, message }`:
1. Validate đăng nhập + project tồn tại.
2. `stepNumber` trong 1..14; thiếu → lấy current step từ workflow.
3. `CanAccessProjectAsync` (B4), không thì 403.
4. Tìm support active: `ConversationType=2 & ProjectId & BuyerUserId=currentUser & SupportStatus IN (1,2)`.
5. Có → thêm **user message** (`IsSystem=false`) nếu body có `message`; trả `conversationId`.
6. Chưa có → tạo conversation (B5) + **system message** chỉ chứa context server-side đã encode (tên dự án/bước/link) + **user message riêng `IsSystem=false`** chứa nội dung người dùng.
7. Trả `{ success, conversationId, redirectUrl = "/chat/{id}" }`.

> ⚠️ Tuyệt đối **không** đưa nội dung người dùng vào message `IsSystem=true` (view `Html.Raw`).

### B7. Giao diện member — `Views/Project/Details.cshtml`
- Nút **"Yêu cầu Sàn hỗ trợ"** trong `tx-project-actions`, hiện cho **mọi user vào được Details** (không chỉ `UserRole==1`).
- Modal: chọn **bước** (mặc định `Model.CurrentStep`) + textarea nội dung → AJAX `POST /api/support/start` (kèm anti-forgery header nếu dự án đang dùng) → success redirect `/chat/{conversationId}`.
- **Phase 1 không nhúng chat vào Details** — mở `/chat/{id}` (ít rủi ro).

### B8. Giao diện `/chat/{id}` (chỉnh nhẹ)
- Tiêu đề: product chat như cũ; support chat = **"Hỗ trợ Sàn"**.
- Badge: product = tên sản phẩm; support = **tên dự án + bước**.
- System message: giữ `Html.Raw` cho link nội bộ tạo server-side; nội dung động phải encode trước khi lưu; message user luôn `IsSystem=false`.

### B9. Inbox CMS — `Areas/Cms/Controllers/SupportRequestsAdminController.cs` `[Authorize(Policy="CmsAccess")]`
- `Index(status, search, projectId, page)`: list `ConversationType=2`; lọc **tất cả / chưa gán / đang xử lý / của tôi**; cột: dự án · bước · requester · assigned staff · tin cuối · thời gian · trạng thái.
- `Thread(long id)`: xem transcript + context (route CMS riêng, không đi qua `GetConversationAsync`).
- `Reply(long id, message)` (POST, anti-forgery), theo **B0.6 — conditional UPDATE atomic**:
  1. Validate message không rỗng, `<= 2000`, trim.
  2. **Nhận xử lý atomic** (trong transaction ngắn, B0.6): `UPDATE … SET AssignedStaffUserId=@me, SupplierUserId=@me, SupportStatus=2 WHERE Id=@id AND ConversationType=2 AND SupportStatus=1 AND AssignedStaffUserId IS NULL`.
     - `rowsAffected=1` → mình vừa nhận, đi tiếp.
     - `rowsAffected=0` → reload: nếu `AssignedStaffUserId==me` (mình đã là chủ) → đi tiếp; nếu là người khác → **chặn mềm** "Yêu cầu đang do X phụ trách", **không add message**.
  3. Add `ChatMessage(SenderUserId=currentCmsUserId, IsSystem=false)`, update `LastMessageAt`, `SaveChanges`.
  4. **Sau khi lưu xong** mới notify `BuyerUserId` (link `/chat/{id}`); notify fail không rollback message.
- `Assign` / `Close`: **để phase 2**.
- Views `Areas/Cms/Views/SupportRequestsAdmin/Index.cshtml` + `Thread.cshtml`.

### B10. Menu CMS — `_LayoutAdminLTE.cshtml`
- Thêm biến `isSupportRequestsAdmin`, đưa vào `isTransactionGroup`.
- Menu item nhóm "Giao dịch": icon life-ring, text **"Yêu cầu hỗ trợ Sàn"**, link `Cms/SupportRequestsAdmin/Index`.

### B11. Ma trận thông báo
| Sự kiện | Người nhận | URL |
|---|---|---|
| Tạo support (đã có đầu mối) | Đầu mối (staff/tư vấn) | CMS `Thread/{id}` |
| Tạo support (chưa gán) | — (chỉ inbox CMS) | — |
| Member nhắn tiếp (đã gán) | Đầu mối | `/chat/{id}` |
| Member nhắn tiếp (chưa gán) | — (inbox CMS) | — |
| Staff reply | Requester | `/chat/{id}` |

---

## PHẦN C — CHỐT & KIỂM THỬ

### C1. Chốt trước khi code (xem đầy đủ ở **B0**)
- Phase 1: **redirect `/chat/{id}`**, không nhúng chat vào Details.
- **`AssignedStaffUserId` là field nghiệp vụ chính**; chưa gán = `SupplierUserId="0"` (sentinel), không dùng chuỗi rỗng.
- **Đầu mối ưu tiên `ProjectMembers` Role=3** (không phải `ProjectConsultants`).
- **CMS reply chống race bằng conditional UPDATE atomic** (B0.6), không chỉ BeginTransaction.
- Nâng cấp VM chat (B0.3); product chat gắn `ConversationType=1` cả khi tạo lẫn query (B0.2).
- **Không đưa user message vào system raw HTML** (B0.4); **không sửa `GetUnreadCountAsync`**; **CMS không gọi `MarkAsReadAsync`** (B0.8).
- Access endpoint **mirror `ProjectController.Details`** gồm invited seller + NDA; API trả **403** thay vì redirect (B0.5).
- URL notify theo vai: member `/chat/{id}`, staff `/cms/SupportRequestsAdmin/Thread/{id}` (B0.10).

### C2. Danh sách file
**Mới:**
- `TechExchangeApp/Scripts/add-support-chat-columns.sql`
- `TechExchangeApp/Enums/ChatConversationType.cs`, `TechExchangeApp/Enums/SupportConversationStatus.cs`
- `TechExchangeApp/Controllers/SupportController.cs` (+ DTO `SupportStartRequest`)
- `TechExchangeApp/Areas/Cms/Controllers/SupportRequestsAdminController.cs`
- `TechExchangeApp/Areas/Cms/Views/SupportRequestsAdmin/Index.cshtml`, `Thread.cshtml`

**Sửa:**
- `TechExchangeApp/Entities/ChatConversation.cs` (5 cột mới)
- `TechExchangeApp/Interfaces/IChatService.cs` (nâng cấp `ChatConversationVm` + `ChatConversationDetailVm`)
- `TechExchangeApp/Services/ChatService.cs` (helper + nhánh support, giữ product chat)
- `TechExchangeApp/Views/Project/Details.cshtml` (nút + modal + JS)
- `TechExchangeApp/Views/Chat/Index.cshtml`, `TechExchangeApp/Views/Chat/Conversation.cshtml` (nhãn support, giữ escape)
- `TechExchangeApp/Areas/Cms/Views/Shared/_LayoutAdminLTE.cshtml` (menu)

### C3. Kiểm thử (acceptance bắt buộc)
- Build sạch: `dotnet build TechExchangeApp.sln`.
- Manual:
  1. Product chat cũ vẫn tạo/mở/gửi được; list + unread count không lỗi.
  2. Dự án **chưa có** tư vấn → member tạo support `Unassigned`; `/chat/{id}` mở không crash; inbox CMS thấy unassigned.
  3. CMS reply → gán staff; member thấy reply; requester được notify.
  4. Dự án **đã có** tư vấn (`ProjectMembers` Role=3) → support tạo thẳng `InProgress`; đầu mối được notify.
  5. Member nhắn tiếp: đã gán → notify staff; chưa gán → không lỗi, không tạo thread mới.
  6. **Race:** 2 staff cùng reply thread chưa gán → 1 nhận, người kia bị chặn mềm, **không sinh message rác** (B0.6).
  7. **Access:** user không có quyền dự án → `POST /api/support/start` trả **403**.
  8. **XSS:** gửi `<script>alert(1)</script>` → hiển thị dạng text, **không chạy script** (B0.4).

### C4. Rủi ro / để sau
- DB chạy tay bằng script (bạn tự chạy); tôi không tự đụng DB.
- Chưa gán → không push admin (chưa có danh sách userId admin). Nếu cần, phase 2.
- `Assign`/`Close`/chuyển xử lý/`SupportStatus=3`: phase 2.
- Nhúng chat vào trang dự án: phase 2.
