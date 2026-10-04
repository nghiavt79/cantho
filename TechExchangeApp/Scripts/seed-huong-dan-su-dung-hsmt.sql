/*
Bổ sung bài hướng dẫn sử dụng (/huong-dan) cho các chức năng mới theo HSMT (Đợt 1–6).

- Thêm bài vào menu con có sẵn của cây 'huong-dan-su-dung': 'nguoi-mua-hang', 'nha-cung-ung'.
- Upsert theo QueryString; Creator = 'huong-dan-hsmt' để seed-huong-dan-su-dung-v2.sql không ẩn các bài này khi chạy lại.
- Chạy sau seed-huong-dan-su-dung(-v2).sql. Chạy: sqlcmd -S <server> -d TechExchangeNew -f 65001 -I -i seed-huong-dan-su-dung-hsmt.sql
*/
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
SET NOCOUNT ON;

BEGIN TRANSACTION;

DECLARE @now DATETIME = SYSDATETIME();
DECLARE @rootId INT = (SELECT TOP 1 MenuId FROM Menu WHERE QueryString = 'huong-dan-su-dung' AND ParentId = 0 ORDER BY MenuId);
IF @rootId IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR(N'Chưa có cây menu huong-dan-su-dung — chạy seed-huong-dan-su-dung-v2.sql trước.', 16, 1);
    RETURN;
END

DECLARE @Articles TABLE
(
    QueryString NVARCHAR(300) NOT NULL PRIMARY KEY,
    MenuQueryString NVARCHAR(300) NOT NULL,
    Title NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Body NVARCHAR(MAX) NOT NULL
);

INSERT INTO @Articles (QueryString, MenuQueryString, Title, Description, Body) VALUES
(N'tai-khoan-quen-mat-khau-va-bao-mat', N'nguoi-mua-hang',
 N'Tài khoản: quên mật khẩu, bảo mật và dữ liệu cá nhân',
 N'Cách tự lấy lại mật khẩu, điều khoản dữ liệu cá nhân khi đăng ký và khi tài khoản bị tạm khóa.',
 N'<h2>Quên mật khẩu</h2>
<p>Tại trang đăng nhập, bấm <strong>Quên mật khẩu?</strong> rồi nhập email hoặc tên đăng nhập. Hệ thống gửi liên kết đặt lại mật khẩu tới email đã đăng ký.</p>
<ul>
<li>Liên kết có hiệu lực <strong>30 phút</strong> và chỉ dùng được một lần.</li>
<li>Không thấy thư: kiểm tra mục thư rác, hoặc đợi vài phút rồi gửi lại.</li>
<li>Không còn dùng được email đã đăng ký: liên hệ Trung tâm để được hỗ trợ.</li>
</ul>
<h2>Đồng ý xử lý dữ liệu cá nhân</h2>
<p>Khi đăng ký tài khoản, bạn cần đánh dấu đồng ý điều khoản xử lý dữ liệu cá nhân của Sàn. Hệ thống ghi nhận thời điểm và phiên bản điều khoản bạn đã đồng ý.</p>
<h2>Tài khoản bị tạm khóa</h2>
<p>Nếu đăng nhập báo tài khoản đã bị tạm khóa, vui lòng liên hệ Ban quản lý Sàn để biết lý do và được hỗ trợ mở khóa.</p>'),

(N'quan-tam-binh-luan-va-ma-qr-san-pham', N'nguoi-mua-hang',
 N'Lưu sản phẩm quan tâm, bình luận, mã QR và tìm kiếm',
 N'Các tiện ích trên trang sản phẩm, tin bài, trang nhà cung cấp và ô tìm kiếm.',
 N'<h2>Sản phẩm quan tâm</h2>
<p>Trên trang chi tiết sản phẩm, bấm <strong>Quan tâm</strong> để lưu lại. Danh sách sản phẩm đã lưu nằm ở mục <strong>Sản phẩm quan tâm</strong> trong menu tài khoản; bấm <strong>Bỏ quan tâm</strong> để xóa khỏi danh sách.</p>
<h2>Bình luận</h2>
<p>Bạn có thể bình luận ở trang sản phẩm, trang tin bài và trang nhà cung cấp sau khi đăng nhập. Bình luận hiển thị ngay; bạn xóa được bình luận của mình. Bình luận vi phạm có thể bị Ban quản lý ẩn.</p>
<h2>Mã QR sản phẩm</h2>
<p>Cột bên phải trang sản phẩm có mã QR dẫn tới trang đó và nút <strong>In mã QR</strong> để in nhãn dán tại gian trưng bày.</p>
<h2>Hình thức trưng bày</h2>
<p>Trang <strong>Sản phẩm</strong> có bộ lọc <strong>Trực tuyến</strong> / <strong>Trưng bày tại Sàn</strong>. Sản phẩm trưng bày tại Sàn hiển thị vị trí trưng bày ở trang chi tiết.</p>
<h2>Tìm kiếm bằng giọng nói</h2>
<p>Bấm biểu tượng micro trong ô tìm kiếm, cho phép trình duyệt dùng micro rồi nói từ khóa. Hệ thống tự điền và tìm. Tính năng hoạt động trên Chrome và Edge.</p>'),

(N'dang-ky-tham-gia-su-kien', N'nguoi-mua-hang',
 N'Đăng ký tham gia sự kiện và nhận nhắc lịch',
 N'Đăng ký tham dự, nhận vé QR, đăng ký nhắc lịch và xem phát trực tiếp.',
 N'<h2>Đăng ký tham gia</h2>
<p>Mở tin sự kiện, điền form <strong>Đăng ký tham gia</strong> (họ tên, email bắt buộc). Bạn không cần đăng nhập; nếu đã đăng nhập, thông tin được điền sẵn.</p>
<h2>Vé và điểm danh</h2>
<p>Sau khi đăng ký, hệ thống gửi email xác nhận kèm liên kết <strong>vé có mã QR</strong>. Mang vé (in hoặc trên điện thoại) đến sự kiện để cán bộ quét điểm danh.</p>
<h2>Quan tâm và nhắc lịch</h2>
<ul>
<li><strong>Quan tâm</strong>: đánh dấu sự kiện bạn quan tâm.</li>
<li><strong>Nhắc lịch</strong>: nhập email (và số điện thoại nếu muốn), hệ thống gửi nhắc trước giờ diễn ra. Người đã đăng ký tham gia được nhắc tự động.</li>
</ul>
<h2>Xem trực tuyến</h2>
<p>Nếu sự kiện có phát trực tiếp, nút <strong>Xem phát trực tiếp</strong> hiển thị trong khối thông tin sự kiện.</p>'),

(N'goi-video-hoi-dap-va-nhan-ban-tin', N'nguoi-mua-hang',
 N'Gọi video với nhà cung cấp, câu hỏi thường gặp và nhận bản tin',
 N'Trao đổi trực tuyến bằng video, tra cứu câu hỏi thường gặp và đăng ký nhận bản tin.',
 N'<h2>Gọi video</h2>
<p>Trong <strong>Tin nhắn</strong>, mở cuộc trò chuyện rồi bấm <strong>Gọi video</strong>. Hệ thống tạo phòng họp trực tuyến, mở trong tab mới và gửi lời mời vào cuộc trò chuyện để bên kia bấm tham gia. Cán bộ Sàn có thể cùng tham gia khi cần hỗ trợ.</p>
<h2>Câu hỏi thường gặp</h2>
<p>Liên kết <strong>Câu hỏi thường gặp</strong> ở chân trang tổng hợp giải đáp theo nhóm; có ô tìm kiếm câu hỏi.</p>
<h2>Đăng ký nhận bản tin</h2>
<p>Bấm <strong>Đăng ký nhận bản tin</strong> ở chân trang, điền họ tên, email và lĩnh vực quan tâm để nhận tin công nghệ, sự kiện, nhu cầu tìm mua mới.</p>'),

(N'goi-gian-hang-hang-nha-cung-cap-va-thong-ke', N'nha-cung-ung',
 N'Gói gian hàng, hạng nhà cung cấp và thống kê lượt xem, liên hệ',
 N'Hạn mức số sản phẩm theo gói, hạng của nhà cung cấp và số liệu theo dõi gian hàng.',
 N'<h2>Gói gian hàng và hạn mức</h2>
<p>Trang <strong>Quản lý sản phẩm</strong> hiển thị số sản phẩm đã đăng so với hạn mức của gói gian hàng (ví dụ 4 / 10 sản phẩm) và ngày hết hạn gói. Khi hết hạn mức, liên hệ Sàn để nâng gói.</p>
<h2>Hạng nhà cung cấp</h2>
<p>Sàn xếp hạng nhà cung cấp (Vàng, Bạc, Thành viên) dựa trên số sản phẩm đang xuất bản, lượt liên hệ, lượt xem và việc xác minh hồ sơ pháp lý. Hạng hiển thị trên trang gian hàng của bạn. Tải đủ giấy tờ xác minh trong mục hồ sơ để đủ điều kiện xét hạng cao.</p>
<h2>Thống kê lượt xem và liên hệ</h2>
<p>Danh sách sản phẩm có cột <strong>Lượt xem</strong> và <strong>Liên hệ</strong> (số cuộc trò chuyện và yêu cầu đặt mua/quan tâm), cùng tổng số ở đầu bảng.</p>'),

(N'gui-bai-viet-cho-san-duyet', N'nha-cung-ung',
 N'Gửi bài viết giới thiệu để Sàn duyệt',
 N'Nhà cung cấp soạn bài giới thiệu sản phẩm, giải pháp và gửi Sàn duyệt xuất bản.',
 N'<h2>Soạn bài</h2>
<p>Vào menu tài khoản → <strong>Bài viết của tôi</strong> → <strong>Viết bài mới</strong>. Nhập tiêu đề, tóm tắt và nội dung (để một dòng trống giữa các đoạn).</p>
<h2>Lưu nháp hoặc gửi duyệt</h2>
<ul>
<li><strong>Lưu nháp</strong>: lưu lại để sửa tiếp.</li>
<li><strong>Gửi duyệt</strong>: bài chuyển sang trạng thái chờ duyệt; Sàn có thể bổ sung ảnh, định dạng trước khi xuất bản.</li>
<li>Bài bị trả về sẽ hiện trạng thái <strong>Bị trả về</strong>; sửa rồi gửi duyệt lại.</li>
</ul>'),

(N'dang-ky-thue-gian-hang-su-kien', N'nha-cung-ung',
 N'Đăng ký thuê gian hàng tại sự kiện',
 N'Xem loại gian hàng, giá, dịch vụ đi kèm và gửi đăng ký thuê.',
 N'<h2>Xem gian hàng</h2>
<p>Mở tin sự kiện có mục <strong>Gian hàng tại sự kiện</strong>: bảng liệt kê loại gian hàng (diện tích, vị trí), giá, dịch vụ đi kèm và số gian còn trống.</p>
<h2>Gửi đăng ký</h2>
<p>Bấm <strong>Đăng ký thuê gian hàng</strong>, chọn loại, số lượng và điền thông tin đơn vị, người liên hệ. Đăng ký ở trạng thái chờ duyệt; Sàn sẽ liên hệ xác nhận.</p>');

UPDATE c
SET c.Title = src.Title, c.Description = src.Description, c.Contents = src.Body, c.MenuId = m.MenuId,
    c.StatusId = 3, c.Modified = @now, c.Modifier = 'huong-dan-hsmt', c.Creator = 'huong-dan-hsmt',
    c.PublishedDate = COALESCE(c.PublishedDate, @now), c.LanguageId = 1, c.Domain = 'VN', c.SiteId = 1
FROM Contents c
JOIN @Articles src ON src.QueryString = c.QueryString
JOIN Menu m ON m.ParentId = @rootId AND m.QueryString = src.MenuQueryString;

INSERT INTO Contents (Title, QueryString, Description, Contents, StatusId, MenuId, Created, Creator, PublishedDate, LanguageId, Domain, SiteId)
SELECT src.Title, src.QueryString, src.Description, src.Body, 3, m.MenuId, @now, 'huong-dan-hsmt', @now, 1, 'VN', 1
FROM @Articles src
JOIN Menu m ON m.ParentId = @rootId AND m.QueryString = src.MenuQueryString
WHERE NOT EXISTS (SELECT 1 FROM Contents c WHERE c.QueryString = src.QueryString);

COMMIT TRANSACTION;

SELECT m.QueryString AS Menu, c.Id, c.Title FROM Contents c JOIN Menu m ON m.MenuId = c.MenuId
WHERE c.Creator = 'huong-dan-hsmt' ORDER BY m.Sort, c.Id;
