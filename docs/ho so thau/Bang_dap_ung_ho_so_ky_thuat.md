# Bảng đáp ứng hồ sơ kỹ thuật – Gói thầu số 4 Sàn GDCN Cần Thơ

Ngày lập: 02/10/2026 · **Cập nhật: 04/10/2026** (sau khi bổ sung chức năng Đợt 1–5, nhánh `feature/bo-sung-chuc-nang-hsmt`)

**Căn cứ:** Chương V (file `Phần 2.docx`) đối chiếu với code `TechExchangeApp`, bộ kết nối Sàn quốc gia (`D:\2025\Techport\Auto`) và báo cáo `docs/BaoCao_DapUng_TieuChuan_KyThuat_SanGDCN_CanTho_06072026.html`.

**Ký hiệu:**
- ✅ Đáp ứng: code đã có, đã chạy thử.
- 🟡 Đáp ứng một phần: đã có phần lõi, còn thiếu một phần hoặc chờ cấu hình / tài khoản bên ngoài.
- ❌ Chưa có.

**Kiểm thử 04/10/2026 (máy local):** 57 trang công khai, khu tài khoản và CMS trả kết quả đúng, không có lỗi máy chủ; trang tiếng Anh không còn chuỗi chưa dịch. Chi tiết từng chức năng mới đã chạy thử theo đợt — xem [Ke_hoach_phat_trien_chuc_nang_con_thieu.md](Ke_hoach_phat_trien_chuc_nang_con_thieu.md). Kịch bản demo: [KichBan_Demo_ChuongV.md](KichBan_Demo_ChuongV.md).

## Tổng hợp

| Phần | Số yêu cầu | ✅ | 🟡 | ❌ |
|---|---|---|---|---|
| I. Hạng mục dịch vụ (mục 2.1) | 11 | 8 | 3 | 0 |
| II. Chức năng chi tiết A – B – C | 63 | 59 | 4 | 0 |
| III. Yêu cầu chất lượng, kết nối, ATTT (mục 2.2–2.4, 2.10, 2.11) | 10 | 8 | 2 | 0 |
| **Cộng** | **84** | **75** | **9** | **0** |

*Ngày 02/10/2026: 35 ✅ / 33 🟡 / 16 ❌.*

**9 dòng 🟡 còn lại đều phụ thuộc bên ngoài hoặc cần quyết định của Chủ trì thuê:** VR360 (3 dòng – phải chụp, dựng 10 điểm cảnh), SMS Brandname và chứng thư số CA thật (chờ tài khoản nhà cung cấp), IPv6 (chờ hạ tầng), cấp độ ATTT (chờ Sở chốt cấp 2 hay 3), lịch sử mua hàng gộp (A.5.2) và khung bình luận cho bài viết / gian hàng (A.7.1).

---

## I. Hạng mục dịch vụ (mục 2.1)

| # | Hạng mục | Đáp ứng | Hiện trạng code |
|---|---|---|---|
| 1 | Website Sàn giao dịch công nghệ | ✅ | Phần mềm ASP.NET Core 8, đã vận hành |
| 2 | Bản quyền Windows Server, SQL Server | ✅ | Hệ thống chạy trên Windows Server và SQL Server |
| 3 | VR360 – 10 điểm cảnh quay | 🟡 | Đã có nút "Tham quan VR360" mở tour (`Views/Shared/_SideButtons.cshtml`); 10 điểm cảnh quay phải chụp, dựng mới |
| 4 | SMS Brandname | 🟡 | Hàng đợi, gửi lại, bật/tắt kênh có sẵn; nhắc lịch sự kiện có kênh SMS. Phần gửi đang là `StubSmsSender` — chờ tài khoản Brandname |
| 5 | Email | ✅ | SMTP (`GmailEmailSender`) qua hàng đợi; dùng cho quên mật khẩu, xác nhận đăng ký sự kiện, nhắc lịch, biểu mẫu, mã xác thực CMS |
| 6 | Chữ ký số | 🟡 | Ký OTP, ký từ xa VNPT/FPT/Viettel, USB Token, nhúng chữ ký PDF; ký từ xa đang ở chế độ TESTING — chờ tài khoản CA thật |
| 7 | Hợp đồng điện tử | ✅ | Dự thảo, phiên bản, phê duyệt, góp ý, ký nhiều bên, nhật ký |
| 8 | Thông báo tự động | ✅ | Hàng đợi email/SMS + chuông thông báo web (SignalR); nhắc lịch sự kiện tự động (`EventReminderBackgroundService`) |
| 9 | Tương tác trực tuyến (chat, họp, phát trực tiếp) | ✅ | Chat; nút "Gọi video" tạo phòng họp trực tuyến trong hội thoại (`VideoCallController`); link phát trực tiếp theo từng sự kiện. Nên dựng máy chủ Jitsi riêng (tham số `VIDEO_CALL_BASE_URL`) |
| 10 | Dịch vụ AI | ✅ | Chatbot, tìm kiếm ngữ nghĩa, gợi ý kết nối cung – cầu |
| 11 | Kết nối API, giám sát vận hành | ✅ | API JSON + tài liệu Swagger `/swagger`; `/health` cho giám sát; sao lưu tự động `Scripts/sao-luu` (đã phục hồi thử) |

## II. Chức năng chi tiết

### A. Website quản lý

| Mục | Chức năng | Đáp ứng | Hiện trạng code |
|---|---|---|---|
| A.1.1 | Xác thực (đăng nhập, đăng xuất, quên mật khẩu, đổi mật khẩu) | ✅ | Quên mật khẩu `/quen-mat-khau` (link qua email, 30 phút, dùng 1 lần) |
| A.1.2 | Quản trị người dùng | ✅ | `Areas/Cms/Controllers/UsersController.cs` |
| A.1.3 | Phân quyền | ✅ | `RoleAdminController`, `SysFuncPermissionAdminController` |
| A.1.4 | Thông báo qua email | ✅ | `NotificationProcessor` |
| A.1.5 | Tương thích nhiều thiết bị | ✅ | Giao diện responsive |
| A.2.1 | Danh mục lĩnh vực công nghệ | ✅ | `CategoryAdminController` |
| A.2.2 | Danh mục nhà cung cấp, duyệt hồ sơ pháp lý, phân hạng | ✅ | Duyệt hồ sơ (`NhaCungUngAdminController`, `UserVerificationDoc`); phân hạng `SupplierTiersAdmin` |
| A.2.3 | Gian hàng, tính tiền theo số sản phẩm, bật/tắt | ✅ | Gói gian hàng `StorePackagesAdmin`, hạn mức ở trang người bán (bật chặn bằng `STORE_QUOTA_ENFORCED`); bật/tắt gian hàng bằng cờ kích hoạt nhà cung ứng |
| A.2.4 | Hạng nhà cung cấp | ✅ | Hạng Vàng/Bạc/Thành viên, tiêu chí, gợi ý theo số liệu, huy hiệu ở trang nhà cung cấp |
| A.2.5 | Theo dõi lượt xem, liên hệ, giao dịch | ✅ | Cột lượt xem, lượt liên hệ ở CMS và trang người bán; giao dịch theo quy trình 14 bước |
| A.2.6 | Duyệt sản phẩm mới, sản phẩm chỉnh sửa | ✅ | Trạng thái duyệt, xuất bản `SanPhamCNTB.StatusId` |
| A.2.7 | Lịch sử chỉnh sửa sản phẩm | ✅ | Tự ghi mỗi lần lưu (`SanPhamCNTBVersions`); CMS so sánh phiên bản `SanPhamCNTBHistory` |
| A.2.8 | Duyệt bài viết của nhà cung cấp | ✅ | Nhà cung cấp gửi bài `/bai-viet-cua-toi`; CMS Tin bài lọc "Bài nhà cung cấp chờ duyệt" |
| A.2.9 | Sàn đăng bài thay nhà cung cấp | ✅ | CMS tạo sản phẩm, đổi chủ sở hữu (`ChangeOwner`) |
| A.2.10 | Lịch sử chat người bán | ✅ | CMS "Lịch sử chat" lọc theo người bán (`ChatAdmin`) |
| A.3.1 | Nhóm sản phẩm; nhóm trưng bày thực tế và trực tuyến | ✅ | Trường hình thức trưng bày `SanPhamCNTB.DisplayMode`, chọn trong cấu hình nhanh CMS |
| A.3.2 | Danh mục sản phẩm, xem lịch sử chỉnh sửa | ✅ | Xem A.2.7 |
| A.3.3 | Phòng trưng bày | ✅ | `ShowroomsAdmin` |
| A.3.4 | Vị trí trưng bày | ✅ | Vị trí trống / đang dùng, gán sản phẩm; trang sản phẩm hiện vị trí |
| A.4.1 | Sự kiện; danh sách đăng ký, xuất dữ liệu | ✅ | `EventsAdmin`: thông tin sự kiện, danh sách đăng ký, xuất Excel, điểm danh bằng QR |
| A.4.2 | Tin tức, bài đăng | ✅ | `PostsController` |
| A.4.3 | Gian hàng sự kiện, giá theo vị trí | ✅ | Loại gian hàng (diện tích, vị trí, giá, dịch vụ), đăng ký thuê, duyệt |
| A.4.4 | Thống kê lượt click, quan tâm sự kiện | ✅ | Thống kê lượt xem, quan tâm, nhắc lịch, đăng ký, điểm danh theo ngày |
| A.4.5 | Nhắc lịch qua SMS, email, web | ✅ | Mốc nhắc theo từng sự kiện; email + chuông web; SMS khi có Brandname |
| A.5.1 | Danh sách người mua, tìm kiếm, xem, khóa tài khoản | ✅ | Khóa / mở khóa có lý do, đẩy phiên đang đăng nhập ra |
| A.5.2 | Lịch sử mua hàng | 🟡 | Có đơn hàng OCOP (`OcopOrderRequest`) và giỏ hàng; giao dịch công nghệ theo dõi qua dự án; chưa có màn hình gộp theo người mua |
| A.5.3 | Lịch sử chat người mua, tìm từ khóa, gắn cờ | ✅ | `ChatAdmin`: lọc người mua, tìm từ khóa, gắn cờ vi phạm |
| A.5.4 | Lịch sử bình luận, sửa/xóa bình luận | ✅ | `CommentsAdmin` theo người dùng (bình luận sản phẩm); bình luận nhu cầu công nghệ quản lý ở mục Tìm mua |
| A.6.1 | Quản lý thu chi, hoa hồng | ✅ | `FinanceAdmin`: tỷ lệ hoa hồng, ghi nhận hoa hồng, sổ thu chi tự động + nhập tay, báo cáo, xuất Excel |
| A.7.1 | Bình luận bài viết, gian hàng, sản phẩm | 🟡 | Bình luận sản phẩm có; bảng dùng chung đã hỗ trợ bài viết, gian hàng nhưng chưa gắn khung bình luận |
| A.7.2 | Quản lý mẫu đăng ký (form động) | ✅ | `FormsAdmin`: định nghĩa trường, bắt buộc, hành động sau gửi; dữ liệu đã gửi, xuất Excel |
| A.7.3 | FAQ | ✅ | `FaqAdmin` + trang `/hoi-dap`, `/en/faq` |
| A.8.1 | Báo cáo giao dịch, cảnh báo giao dịch mới | ✅ | `AdminAnalyticsController`, dashboard, thông báo |
| A.8.2 | Cảnh báo sản phẩm trùng lặp, quá hạn | ✅ | Quét hằng ngày + "Quét ngay"; `ProductWarningsAdmin` |
| A.8.3 | Phiên bản nội dung sản phẩm | ✅ | Xem A.2.7 |
| A.8.4 | Email xác nhận, nhắc lịch sự kiện | ✅ | Email xác nhận kèm vé QR; nhắc lịch tự động |
| A.8.5 | Lịch sử thao tác (log) | ✅ | Log CMS, `ContractAuditLog`, `EntityActionLog`, `ESignAuditLog`, `ProjectAccessLog` |

### B. Website bán hàng

| Mục | Chức năng | Đáp ứng | Hiện trạng code |
|---|---|---|---|
| B.1.1 | Trang chủ | ✅ | `HomeController` |
| B.1.2 | Tham quan 3D sự kiện | 🟡 | Có VR360 (nút mở tour); không có mô hình 3D, thuyết minh âm thanh (đã góp ý sửa HSMT) |
| B.1.3 | Thông tin nổi bật | ✅ | Tin tức, sự kiện trên trang chủ |
| B.1.4 | Danh sách sản phẩm, bộ lọc, chia nhóm trưng bày | ✅ | Bộ lọc Trực tuyến / Trưng bày tại Sàn ở `/san-pham` |
| B.1.5 | Quản lý banner | ✅ | `ImageAdverAdminController` |
| B.1.6 | Mẫu đăng ký nhận thông tin | ✅ | Biểu mẫu "Đăng ký nhận bản tin" (link chân trang); đăng ký tham gia sự kiện |
| B.2.1 | Gian hàng người bán | ✅ | Trang nhà cung cấp kèm huy hiệu hạng |
| B.2.2 | Hồ sơ pháp lý | ✅ | `UserVerificationDoc` |
| B.2.3 | Lịch sử giao dịch người bán | ✅ | Trang quản lý sản phẩm: lượt xem, lượt liên hệ từng sản phẩm và tổng |
| B.2.4 | Báo giá | ✅ | RFQ, đề xuất (`ProposalController`) |
| B.2.5 | Quản lý sản phẩm (ảnh, video, mô tả, giá, trạng thái pháp lý) | ✅ | `QuanLySanPham`, các trường pháp lý trong `SanPhamCNTB` |
| B.3.1 | Đăng tin tìm mua | ✅ | `NhucaucongngheController`, RFQ |
| B.3.2 | Bình luận tin tìm mua | ✅ | `CommentsYCTB` |
| B.3.3 | Quan tâm sản phẩm | ✅ | Nút "Quan tâm", trang "Sản phẩm quan tâm" |
| B.3.4 | Giỏ hàng | ✅ | `ShoppingCart` |
| B.3.5 | Bình luận sản phẩm | ✅ | Khung bình luận ở trang chi tiết sản phẩm (VI/EN dùng chung) |

### C. Tiện ích khác

| Mục | Chức năng | Đáp ứng | Hiện trạng code |
|---|---|---|---|
| C.1 | Tham quan 3D | 🟡 | VR360 (xem B.1.2) |
| C.2 | Tích hợp với sàn giao dịch khác | ✅ | Bộ kết nối Sàn KH&CN quốc gia đang chạy production |
| C.3 | Chia sẻ mạng xã hội | ✅ | `Views/Shared/_ContentUtilities.cshtml` |
| C.4 | Ký số hợp đồng điện tử | ✅ | Xem mục I.6, I.7 (ký từ xa cần cấu hình CA thật) |
| C.5 | In mã QR sản phẩm | ✅ | QR + nút in nhãn ở trang chi tiết mọi sản phẩm |
| C.6 | Phòng họp ảo, livestream | ✅ | Link phát trực tiếp theo từng sự kiện; phòng họp trực tuyến từ chat |
| C.7 | Chat trực tiếp, video call | ✅ | Chat + nút "Gọi video" |
| C.8 | Dịch Anh/Việt | ✅ | Site tiếng Anh `/en/`, quản trị bản dịch |
| C.9 | AI tìm kiếm, tìm kiếm bằng giọng nói | ✅ | Tìm kiếm AI + toàn văn; nút micro trong ô tìm kiếm |
| C.10 | Chatbot AI | ✅ | `AiChatService` |

## III. Yêu cầu chất lượng, kết nối, an toàn thông tin

| Mục | Yêu cầu | Đáp ứng | Hiện trạng |
|---|---|---|---|
| 2.2 | Tính sẵn sàng ≥ 99,5%, giám sát, cảnh báo | ✅ | `/health` (CSDL, ổ đĩa, hàng đợi thông báo); gắn công cụ giám sát uptime bên ngoài khi triển khai |
| 2.2 | Sao lưu định kỳ, phục hồi | ✅ | `Scripts/sao-luu`: Full hằng ngày + Diff 6 giờ, giữ 30 ngày, đã phục hồi thử; đăng ký lịch trên máy chủ bằng `register-backup-tasks.ps1` |
| 2.2 | SSL/TLS, phân quyền, xác thực | ✅ | HTTPS/HSTS, cookie HttpOnly, CSRF, RBAC |
| 2.2 | IPv6 | 🟡 | Kestrel hỗ trợ; cần cấu hình binding khi có hạ tầng |
| 2.2 | Nhật ký hệ thống | ✅ | Xem A.8.5 |
| 2.3 | API, JSON/XML/CSV, xuất nhập dữ liệu | ✅ | API JSON + Swagger; xuất Excel |
| 2.3 | Kết nối Sàn KH&CN Việt Nam | ✅ | Xem C.2 |
| 2.4 | Tài liệu hóa giao diện, cấu trúc dữ liệu | ✅ | Swagger `/swagger`; [DacTa_CSDL_BoSung_HSMT.md](DacTa_CSDL_BoSung_HSMT.md) |
| 2.10 | Bàn giao, trích xuất dữ liệu | ✅ | CSDL SQL Server, xuất Excel |
| 2.11 | ATTT theo cấp độ, bảo vệ dữ liệu cá nhân | 🟡 | Đã có: header bảo mật, xác thực 2 lớp cho quản trị (`CMS_MFA_ENABLED`), đồng ý xử lý dữ liệu cá nhân khi đăng ký, khóa tài khoản. Chờ Sở chốt cấp độ ATTT (2 hay 3) và nội dung chính sách bảo vệ dữ liệu cá nhân |
