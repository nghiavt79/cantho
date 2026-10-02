# Kế hoạch phát triển chức năng còn thiếu – Gói thầu số 4 Sàn GDCN Cần Thơ

Ngày lập: 02/10/2026

**Căn cứ:** [Bang_dap_ung_ho_so_ky_thuat.md](Bang_dap_ung_ho_so_ky_thuat.md) (84 yêu cầu: 35 ✅ / 33 🟡 / 16 ❌) và [Bo_sung_chuc_nang_truoc_demo.md](Bo_sung_chuc_nang_truoc_demo.md).

**Đã kiểm tra lại trong code ngày 02/10/2026:** các mục ❌ đều chưa có (không có Swagger, health check, sao lưu, quên mật khẩu, FAQ, quan tâm sản phẩm, đăng ký sự kiện, phòng trưng bày, hoa hồng, lịch sử phiên bản sản phẩm, bình luận sản phẩm, tìm kiếm giọng nói). QRCoder mới dùng cho OCOP; SMS vẫn là `StubSmsSender`.

**Nguyên tắc sắp thứ tự:** Chương III chấm đạt/không đạt → ưu tiên phủ hết mục ❌ trước, làm việc nhỏ và việc làm nền dùng chung trước, việc phụ thuộc bên ngoài chạy song song.

---

## Lưu ý kỹ thuật trước khi làm

1. **Bảng Users bỏ qua cột Identity.** `AppDbContext` đang `Ignore` `LockoutEnd`, `LockoutEnabled`, `SecurityStamp`, `AccessFailedCount`… nên không dùng được cơ chế khóa và token đặt lại mật khẩu sẵn có của ASP.NET Identity.
   - Khóa tài khoản: thêm cột `IsLocked`, `LockedReason`, `LockedAt`, `LockedBy` vào `Users`; chặn ở bước đăng nhập và khi kiểm tra cookie (`OnValidatePrincipal`) để người đang đăng nhập cũng bị đẩy ra.
   - Quên mật khẩu: bảng riêng `PasswordResetTokens` (token băm, hạn 30 phút, dùng 1 lần).
2. **Sự kiện đang là tin bài.** Không tách hệ thống mới; thêm bảng `EventInfo` gắn 1–1 với bài đăng (thời gian, địa điểm, link livestream, mở/đóng đăng ký). 392 hội thảo hiện có giữ nguyên, chỉ bổ sung thông tin khi cần.
3. **Dùng lại hàng đợi thông báo** (`NotificationWorker`, `NotificationProcessor`) cho nhắc lịch, email xác nhận, SMS — không viết kênh gửi mới.
4. **Mọi bảng mới** phải có script SQL trong `TechExchangeApp/Scripts/`, chạy bằng `sqlcmd -f 65001`; có bản EN (LanguageId) nếu là nội dung hiển thị công khai (FAQ, sự kiện).

---

## Đợt 1 – Việc nhanh và hạ tầng (tuần 1) · ~13,5 ngày công

Mục tiêu: xóa 6 mục ❌ dễ nhất và 3 mục hạ tầng của Phần III.

| # | Mục HSMT | Việc | Cách làm | Công |
|---|---|---|---|---|
| 1.3 | 2.3, 2.4 | Tài liệu API | Swashbuckle, mở `/swagger` có đăng nhập admin; chú thích XML cho API công khai | 1 |
| 1.4 | 2.2 | Health check, uptime | `AddHealthChecks` (SQL, ổ đĩa, hàng đợi thông báo) → `/health`; gắn công cụ giám sát uptime ngoài, cảnh báo email/SMS | 1 |
| 1.5 | 2.2, 2.10 | Sao lưu tự động | SQL Agent job backup full hằng ngày + diff 6 giờ; nén thư mục upload; giữ 30 ngày; **chạy thử phục hồi và lập biên bản** | 1 |
| 2.0a | A.1.1 | Quên mật khẩu | Trang nhập email/SĐT → gửi link qua email (OTP SMS khi có Brandname); bảng `PasswordResetTokens` | 1,5 |
| 2.0b | A.5.1 | Khóa tài khoản | Cột khóa trên `Users`, nút khóa/mở trong CMS có ghi lý do, chặn đăng nhập, ghi log CMS | 1 |
| 2.15 | C.5 | QR sản phẩm | Dùng lại QRCoder của OCOP; hiện QR ở trang chi tiết `SanPhamCNTB`, nút in nhãn | 0,5 |
| 2.17 | C.9 | Tìm kiếm giọng nói | Nút micro ở ô tìm kiếm, Web Speech API (`vi-VN` / `en-US` theo ngôn ngữ trang); ẩn nút nếu trình duyệt không hỗ trợ | 0,5 |
| 2.12 | A.7.3 | FAQ | Bảng `Faq` (nhóm, câu hỏi, trả lời, thứ tự, bật/tắt, LanguageId); CMS + trang `/hoi-dap` và `/en/faq` | 1,5 |
| 2.0c | A.3.1, B.1.4 | Nhóm trưng bày | Cột `DisplayMode` trên `SanPhamCNTB` (trực tuyến / trưng bày tại sàn / cả hai); bộ lọc ở danh sách | 1 |
| 2.14 | B.3.3 | Quan tâm sản phẩm | Bảng `ProductFavorites`; nút trái tim ở thẻ và trang chi tiết; trang "Sản phẩm quan tâm" trong workspace | 1,5 |
| 1.1 | 2.1 | SMS Brandname thật | Lớp `ISmsSender` cho nhà cung cấp được chọn, thay `StubSmsSender`; khóa API để trong biến môi trường | 1,5 |
| 1.2 | 2.1 | Ký số CA thật | Cấu hình tài khoản thật ít nhất 1 CA; chạy trọn vòng ký từ xa + USB Token trên hợp đồng PDF | 1 |

*1.1 và 1.2 phụ thuộc hợp đồng/tài khoản của nhà cung cấp — làm code trước, cấu hình khi có tài khoản.*

## Đợt 2 – Sản phẩm và tương tác (tuần 2) · ~11,5 ngày công

| # | Mục HSMT | Việc | Cách làm | Công |
|---|---|---|---|---|
| 2.3 | A.2.7, A.3.2, A.8.3 | Lịch sử phiên bản sản phẩm | Bảng `SanPhamCNTBVersions` lưu ảnh chụp JSON mỗi lần lưu (người sửa, thời điểm, số phiên bản); màn hình CMS so sánh 2 phiên bản, tô đậm trường thay đổi (giá, mô tả, trạng thái pháp lý) | 3 |
| 2.13 | A.8.2 | Cảnh báo trùng lặp, quá hạn | Tác vụ nền chạy đêm: trùng tên (chuẩn hóa không dấu) và trùng nội dung (dùng embedding đã có), quá hạn N tháng không cập nhật (tham số trong `SYS_PARAMETERS`); danh sách cảnh báo trong CMS + thông báo admin | 2 |
| 2.14a | B.3.5, A.7.1, A.5.4 | Bình luận sản phẩm | Bảng bình luận dùng chung cho sản phẩm/gian hàng/bài viết (`TargetType`, `TargetId`); CMS xem theo người dùng, sửa, ẩn, xóa | 3 |
| 2.9 | A.5.3, A.2.10 | Quản lý chat trong CMS | Xem hội thoại theo người mua/người bán, tìm từ khóa trong `ChatMessage`, gắn cờ vi phạm + ghi chú | 2 |
| — | A.2.5, B.2.3 | Thống kê lượt liên hệ | Ghi sự kiện bấm "Liên hệ", mở chat, gửi yêu cầu báo giá theo gian hàng; hiện ở trang quản lý của người bán và CMS | 1 |
| — | A.2.8 | Duyệt bài nhà cung cấp | Kiểm tra luồng nhà cung cấp gửi bài → chờ duyệt → xuất bản; bổ sung nếu thiếu | 0,5 |

## Đợt 3 – Sự kiện (tuần 3) · ~11 ngày công

| # | Mục HSMT | Việc | Cách làm | Công |
|---|---|---|---|---|
| — | A.4.1, C.6 | Nền sự kiện | Bảng `EventInfo` gắn bài đăng: bắt đầu/kết thúc, địa điểm, link livestream, hạn và giới hạn đăng ký | 1,5 |
| 2.5 | A.4.1, B.1.6 | Đăng ký tham gia | Form đăng ký trên trang sự kiện, email xác nhận kèm QR vé; CMS: danh sách, xuất Excel, điểm danh bằng quét QR | 3 |
| 2.7 | A.4.4 | Thống kê | Ghi lượt click, "Quan tâm", "Nhắc lịch" theo sự kiện; biểu đồ trong CMS | 1,5 |
| 2.8 | A.4.5, A.8.4 | Nhắc lịch | Cấu hình mốc nhắc theo sự kiện (mặc định 24 giờ, 1 giờ); tác vụ nền đẩy vào hàng đợi thông báo theo kênh email/SMS/web | 2 |
| 2.6 | A.4.3 | Gian hàng sự kiện | Loại gian hàng (9 m², 18 m²…), giá theo vị trí, dịch vụ đi kèm; doanh nghiệp đăng ký thuê, admin duyệt | 2,5 |

## Đợt 4 – Nhà cung cấp, trưng bày, thu chi, biểu mẫu (tuần 4–5) · ~14,5 ngày công

| # | Mục HSMT | Việc | Cách làm | Công |
|---|---|---|---|---|
| 2.1 | A.2.2, A.2.4 | Phân hạng nhà cung cấp | Danh mục hạng (Vàng, Bạc, Thành viên), tiêu chí (số sản phẩm, giao dịch, hồ sơ pháp lý, đánh giá); admin gán hoặc gợi ý tự động; huy hiệu ở gian hàng | 2,5 |
| 2.2 | A.2.3 | Gói gian hàng | Gói (số sản phẩm tối đa, giá, thời hạn); chặn đăng vượt gói; admin bật/tắt gian hàng | 2,5 |
| 2.10 | A.6.1 | Thu chi, hoa hồng | Cấu hình % hoa hồng theo loại giao dịch; sổ thu chi (phí gian hàng, phí gian hàng sự kiện, hoa hồng); báo cáo doanh thu, xuất Excel | 3 |
| 2.4 | A.3.3, A.3.4 | Phòng, vị trí trưng bày | 2 danh mục Phòng và Vị trí; gán sản phẩm `DisplayMode` = trưng bày vào vị trí; sơ đồ trống/đang dùng | 2,5 |
| 2.11 | A.7.2, B.1.6 | Biểu mẫu động | Định nghĩa trường (loại, bắt buộc, thứ tự), hành động sau gửi (thông báo, chuyển trang, gửi email); dùng cho đăng ký nhận bản tin | 4 |

*2.10 làm sau 2.2 và 2.6 vì ghi nhận thu từ hai mục này.*

## Đợt 5 – Video call và an toàn thông tin (tuần 5–6) · ~9,5 ngày công

| # | Mục HSMT | Việc | Cách làm | Công |
|---|---|---|---|---|
| 2.16 | C.7, I.9 | Video call | Nút "Gọi video" trong màn hình chat → tạo phòng họp nhúng (Jitsi Meet tự dựng hoặc dịch vụ họp trực tuyến), mời người bán, người mua, cán bộ Sàn | 3 |
| — | 2.11 | Security headers | CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy | 0,5 |
| — | 2.11 | Xác thực 2 lớp | OTP email/SMS bắt buộc cho tài khoản quản trị CMS | 2 |
| — | 2.11 | Đồng ý xử lý dữ liệu cá nhân | Ô đồng ý khi đăng ký, lưu thời điểm và phiên bản điều khoản; trang chính sách | 1,5 |
| — | 2.2 | IPv6 | Cấu hình binding và kiểm tra khi có hạ tầng | 0,5 |
| — | 2.4 | Đặc tả API, CSDL | Xuất Swagger + sơ đồ CSDL các bảng nghiệp vụ chính | 2 |

## Đợt 6 – Kiểm thử và kịch bản demo (tuần 6) · ~3 ngày công

- Chạy lại toàn bộ 84 dòng trong bảng đáp ứng, cập nhật trạng thái.
- Kiểm tra bản EN cho các trang công khai mới (FAQ, sự kiện, bình luận, quan tâm).
- Soạn kịch bản demo theo thứ tự mục Chương V, dữ liệu mẫu sẵn trong DB.

---

## Tổng hợp

| Đợt | Nội dung | Ngày công | Mục ❌ được xử lý |
|---|---|---|---|
| 1 | Việc nhanh, hạ tầng | 13,5 | A.7.3, B.3.3, sao lưu (+ 🟡 A.1.1, A.5.1, C.5, C.9) |
| 2 | Sản phẩm, tương tác | 11,5 | A.2.7, A.8.2, A.8.3, B.3.5 |
| 3 | Sự kiện | 11 | A.4.3, A.4.4, A.4.5, A.8.4 |
| 4 | Nhà cung cấp, trưng bày, thu chi | 14,5 | A.2.4, A.3.3, A.3.4, A.6.1, A.7.2 |
| 5 | Video call, ATTT | 9,5 | — (🟡 C.7, 2.11, 2.2, 2.4) |
| 6 | Kiểm thử, demo | 3 | — |
| **Cộng** | | **~63** | **16/16** |

Với 2 lập trình viên: khoảng **6 tuần**. Nếu demo sớm hơn, làm xong Đợt 1–3 (~36 ngày công, ~3,5 tuần với 2 người) là đã phủ 11/16 mục ❌ và toàn bộ hạ tầng.

## Cần chốt trước khi bắt đầu

| # | Việc | Ảnh hưởng |
|---|---|---|
| 1 | Ngày demo dự kiến | Quyết định cắt phạm vi theo đợt |
| 2 | Nhà cung cấp SMS Brandname và tài khoản | Mục 1.1, OTP quên mật khẩu, nhắc lịch SMS |
| 3 | Tài khoản CA thật (VNPT/FPT/Viettel) | Mục 1.2 |
| 4 | Video call: tự dựng Jitsi hay thuê dịch vụ | Mục 2.16, chi phí máy chủ |
| 5 | Cấp độ ATTT (2 hay 3) | Phạm vi Đợt 5 |

## Theo dõi tiến độ

| Đợt | Phụ trách | Bắt đầu | Hạn | Trạng thái |
|---|---|---|---|---|
| 1 | | 02/10/2026 | | Code xong 10/12 mục, đã chạy thử trên local (nhánh `feature/bo-sung-chuc-nang-hsmt`). Còn 1.1 SMS, 1.2 ký số: chờ tài khoản nhà cung cấp |
| 2 | | 02/10/2026 | | Code xong 6/6 mục, đã chạy thử trên local. Bình luận mới gắn ở trang sản phẩm (bảng dùng chung cho gian hàng, bài viết) |
| 3 | | 02/10/2026 | | Code xong 5/5 mục, đã chạy thử trên local. Nhắc lịch qua email + chuông web; SMS chờ Brandname thật (mục 1.1) |
| 4 | | | | |
| 5 | | | | |
| 6 | | | | |
