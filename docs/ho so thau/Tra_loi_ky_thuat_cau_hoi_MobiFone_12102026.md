# Trả lời phần kỹ thuật – câu hỏi xác nhận đề xuất kỹ thuật Gói thầu số 4

Ngày 12/10/2026 · Căn cứ: `Cau_hoi_can_xac_nhan_MobiFone_12102026.md`

Phạm vi: chỉ các câu kiểm tra được trên mã nguồn `TechExchangeApp` (nhánh `feature/bo-sung-chuc-nang-hsmt`), CSDL `TechExchangeNew` bản máy phát triển và bộ kết nối `D:\2025\Techport\Auto`. Các câu về nguồn cung, bản quyền, chi phí, SLA đối tác, nhân sự và hợp đồng (1–4, 6, 9–11, 17–22) thuộc MobiFone, không trả lời ở đây.

Ký hiệu hiện trạng: **Có** = có trong code, đã chạy trên máy phát triển · **Cấu hình** = có code, cần tài khoản/tham số/hạ tầng thật · **Chưa có** = phải phát triển.

## Tóm tắt

| Câu | Nội dung | Hiện trạng |
| --- | --- | --- |
| 5 | RPO CSDL 6 giờ / tệp 24 giờ | Có (lịch sao lưu khớp); RTO 8 giờ chưa đo |
| 7 | Lưu nhật ký 05 năm | Chưa có chính sách; hiện không xóa nhật ký, nhưng chưa có bảo vệ chống sửa/xóa |
| 8 | MFA | Cấu hình (tham số đang tắt) |
| 12 | 7 nhóm chức năng | 2 Có, 3 Có một phần, 2 Chưa có (xem bảng) |
| 13 | Số 4.334 / 1.808 | 4.334 đúng; 1.808 nằm ở bảng web không dùng (web hiển thị 24 chuyên gia); quyền sử dụng chưa xác nhận |
| 14 | SMS, ký số, VR360, IPv6 | Chưa kiểm thử thật |
| 15 | Bộ kết nối quốc gia | Có một phần: chỉ đẩy sản phẩm chào bán |
| 16 | Tài liệu API Sàn quốc gia | Mới có 2 endpoint đang dùng; chưa có tài liệu chính thức |

## Câu 5 – RPO, RTO

- Lịch sao lưu đã viết sẵn (`Scripts/sao-luu/register-backup-tasks.ps1`): CSDL toàn phần 01:00 hằng ngày, vi sai lúc 07:00, 13:00, 19:00 → khoảng cách lớn nhất 6 giờ. **Khớp RPO CSDL 6 giờ.**
- Tệp đính kèm đồng bộ (robocopy /MIR) cùng lần sao lưu toàn phần, 1 lần/ngày → **khớp RPO tệp 24 giờ.**
- Mỗi bản sao lưu được kiểm tra bằng `RESTORE VERIFYONLY`; giữ 30 ngày; có script phục hồi thử `restore-test.ps1`.
- Lưu ý: robocopy /MIR là bản sao gương, xóa tệp ở nguồn thì lần đồng bộ sau cũng xóa ở bản sao → tệp không có lịch sử 30 ngày như CSDL. Nếu cam kết "giữ 30 ngày" cho cả tệp, cần đổi sang bản sao theo ngày hoặc dùng kho sao lưu có phiên bản.
- **RTO 8 giờ chưa đo.** Cần chạy phục hồi thử trên hạ tầng thật, ghi thời gian vào biên bản.
- Script chưa được đăng ký trên máy chủ thật (chỉ chạy thử ở máy phát triển).

## Câu 7 – Nhật ký

- Các bảng nhật ký hiện có: nhật ký thao tác CMS, `EntityActionLog`, `ContractAuditLog`, `ESignAuditLog`, `ProjectAccessLog`, `WorkflowTransitionLogs`, `AISearchLogs`.
- Không có tác vụ tự xóa nhật ký → dữ liệu được giữ vô thời hạn theo dung lượng CSDL. Chưa có chính sách thời hạn lưu, chưa có cơ chế chống sửa/xóa (người có quyền CSDL vẫn sửa được).
- **Nhật ký tìm kiếm:** có. Mọi lượt tìm kiếm thường và tìm kiếm AI được ghi vào bảng `SearchQueryLog` qua thủ tục `uspLogSearchQuery` (từ khóa, số kết quả, kiểu tìm, ngôn ngữ, thời gian xử lý); CSDL máy phát triển có 502 dòng. Tìm kiếm gợi ý nhà cung cấp bằng AI ghi thêm vào `AISearchLogs`. Chưa có màn hình tra cứu nhật ký tìm kiếm trong CMS (mới có thống kê từ khóa nổi bật).
- Để cam kết "lưu 05 năm, chống sửa/xóa": cần (1) màn hình tra cứu nhật ký tìm kiếm trong CMS, (2) phân quyền chỉ đọc/chỉ thêm cho bảng nhật ký, (3) đưa nhật ký vào sao lưu dài hạn.

## Câu 8 – MFA

Có code xác thực 2 lớp cho tài khoản quản trị CMS, bật bằng tham số `CMS_MFA_ENABLED` (mặc định `0` = tắt), cần cấu hình gửi email thật. Phía kỹ thuật bật khi đưa vào vận hành thử.

## Câu 12 – Chức năng

| Chức năng | Hiện trạng | Chi tiết |
| --- | --- | --- |
| Quy trình 14 bước | Có | Định nghĩa 14 bước trong `WorkflowService`; trạng thái 0/1/2 (chưa bắt đầu/đang làm/hoàn thành); phân quyền theo vai trò bên mua/bên bán/chuyên gia và theo bước (`StepPermissions`); NDA chặn xem từ bước 3; có controller riêng cho từng bước (NDA, RFQ, đề xuất, đàm phán, pháp lý, hợp đồng, tạm ứng, thử nghiệm, bàn giao, đào tạo, nghiệm thu, thanh lý); nhật ký chuyển trạng thái. CSDL có 8 dự án mẫu, 74 bước ở trạng thái hoàn thành — **dữ liệu nạp mẫu, chưa có biên bản chạy trọn vòng bằng thao tác thật** |
| A.4.4 Thống kê sự kiện | Chưa có (một phần) | Chỉ ghi 3 loại tương tác: xem, quan tâm, nhắc lịch; cùng đăng ký và điểm danh. **Chưa ghi lượt nhấp banner, lượt nhấp đường dẫn, lượt bấm chia sẻ** |
| A.5.2 Lịch sử mua hàng | Có một phần | Có: mã yêu cầu (Id), ngày, sản phẩm, nhà cung cấp, số lượng, giá, trạng thái. **Chưa có: trạng thái thanh toán, giao hàng/hoàn thành tách riêng, tổng tiền đã chi.** Sàn không có cổng thanh toán nên "trạng thái thanh toán" phải là trạng thái do người dùng/quản trị cập nhật |
| B.2.3 Giao dịch người bán | Chưa có | Có lượt xem, lượt liên hệ từng sản phẩm. **Chưa có số lượt giao dịch, danh sách và lọc theo thời gian** |
| A.8.5 Nhật ký | Có một phần | Có nhật ký thao tác và nhật ký tìm kiếm; thiếu màn hình tra cứu nhật ký tìm kiếm, chính sách thời hạn và bảo vệ chống sửa/xóa (câu 7) |
| Báo cáo | Có một phần | Có biểu đồ (trang Thống kê CMS) và xuất Excel. **Chưa có xuất PDF cho báo cáo** (dịch vụ tạo PDF hiện chỉ dùng cho hợp đồng ký số) |
| Tổ chức/chuyên gia | Có | Có: hồ sơ tổ chức 6 loại hình (viện/trung tâm NC, trường ĐH, DN KH&CN, DN sản xuất, tổ chức trung gian/tư vấn, khác), duyệt hồ sơ pháp lý; đăng ký và quản trị tư vấn viên; trang dịch vụ tư vấn; danh bạ chuyên gia công khai lấy từ hồ sơ tư vấn viên, quản trị tại CMS Nhà tư vấn (thêm, sửa, xóa, duyệt, xuất Excel). **Không có loại hồ sơ nhà đầu tư riêng** |

Ước lượng công phát triển phần còn thiếu (1 lập trình viên): xem `Ke_hoach_chuc_nang_con_thieu_12102026.md`.

## Câu 13 – Số liệu

Đếm trên CSDL `TechExchangeNew` bản máy phát triển ngày 12/10/2026: **4.334** hồ sơ sản phẩm công nghệ, thiết bị (bản tiếng Việt, tất cả cùng trạng thái `StatusId = 3`) và **1.808** dòng trong bảng `ChuyenGia`. Lưu ý: bảng `ChuyenGia` không được website dùng (chỉ còn liên kết sang trang cũ); trang Chuyên gia trên web hiển thị **24** hồ sơ từ bảng tư vấn viên `NhaTuVan`. Đây là dữ liệu của hệ thống VTISC đang vận hành, không phải dữ liệu của Sàn Cần Thơ. **Quyền đưa dữ liệu này sang Sàn Cần Thơ chưa được xác nhận** → đồng ý không đưa số liệu vào bản chốt cho đến khi có văn bản.

## Câu 14 – Kiểm thử thật

| Hạng mục | Hiện trạng |
| --- | --- |
| SMS Brandname | Hàng đợi, gửi lại, bật/tắt kênh có; phần gửi là `StubSmsSender` (giả lập). **Chưa gửi SMS thật lần nào** |
| Ký số từ xa | Có bộ kết nối VNPT/FPT/Viettel; tham số `SIGNING_MODE = TESTING`, nhánh thử là giả lập. **Chưa ký với CA thật** |
| USB Token | Có ứng dụng ký cục bộ `TechExchange.LocalSigner`; chưa có biên bản thử với Token thật |
| Nhúng chữ ký PDF, kiểm tra mã băm | Có code (`PdfSigningService`); đã chạy với chứng thư thử |
| VR360 | Có nút mở tour; **10 điểm cảnh chưa chụp** |
| IPv6 | Máy chủ web hỗ trợ; **chưa cấu hình, chưa thử** |

## Câu 15 – Bộ kết nối Sàn quốc gia

- Ứng dụng WinForms `IntergrateData` (`D:\2025\Techport\Auto`), đang dùng cho hệ thống VTISC hiện hành.
- **Có:** lấy mã truy cập qua `sso.techmartvietnam.vn/connect/token`; đẩy sản phẩm qua `platform.techmartvietnam.vn/.../Products/InsertProductExternal`; theo lô 20 sản phẩm, tự đăng nhập lại sau 30 lô; chọn sản phẩm theo khoảng ngày; lưu danh sách mã đã đồng bộ để không gửi trùng; có gửi trường lĩnh vực.
- **Chưa có:** đăng ký gian hàng địa phương, tin chào mua, ánh xạ mã lĩnh vực theo bộ mã của Bộ, nhật ký từng bản ghi, gửi lại tự động, đối soát. Ứng dụng chạy tay (không phải dịch vụ nền trong Sàn).
- Tài khoản đang dùng là của đơn vị vận hành hiện tại; Sàn Cần Thơ cần tài khoản riêng do Bộ cấp.

## Câu 16 – Tài liệu API

Phía kỹ thuật mới có 2 địa chỉ đang dùng (lấy mã truy cập, đẩy sản phẩm). Chưa có tài liệu API chính thức, danh mục mã lĩnh vực, hạn mức gọi. Cần Bên thuê gửi văn bản đề nghị Bộ KH&CN cấp.

## Phụ lục kỹ thuật phía VTISC cung cấp được

| Phụ lục | Hiện có | Còn phải làm |
| --- | --- | --- |
| PL-03 Đặc tả API | Swagger tại `/swagger` (cần đăng nhập quản trị) | Xuất ra file JSON/PDF đính kèm |
| PL-04 Đặc tả CSDL | `DacTa_CSDL_BoSung_HSMT.md` (chỉ các bảng bổ sung) | Lập đặc tả các bảng nghiệp vụ chính |
| PL-05 Ảnh chức năng, hướng dẫn | Trang `/huong-dan`, 7 bài hướng dẫn mới | Chụp ảnh màn hình theo bảng đáp ứng |
| PL-06 Biên bản kiểm thử | Kiểm thử 57 trang trên máy phát triển ngày 04/10 | Chạy kịch bản nghiệm thu trọn vòng, lập biên bản |
