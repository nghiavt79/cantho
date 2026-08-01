/*
Seed/update khu vực /huong-dan theo nghiệp vụ mới.

Cơ chế:
- Dùng lại Menu root QueryString = 'huong-dan-su-dung'.
- Upsert các menu con và bài viết trong Contents.
- Các bài cũ nằm trong cây hướng dẫn nhưng không còn trong danh sách v2 sẽ được chuyển StatusId = 2
  để không hiển thị công khai, nhưng không xóa dữ liệu.

Chạy trên SQL Server. File này nên được lưu UTF-8.
*/

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @now DATETIME = SYSDATETIME();
DECLARE @rootId INT;

IF NOT EXISTS (SELECT 1 FROM Menu WHERE QueryString = 'huong-dan-su-dung' AND ParentId = 0)
BEGIN
    INSERT INTO Menu
        (Title, Description, Sort, MenuPosition, StatusId, Created, Creator, ParentId, QueryString, LanguageId, Domain, SiteId)
    VALUES
        (N'Hướng dẫn sử dụng',
         N'Tài liệu hướng dẫn sử dụng Sàn Giao dịch Công nghệ theo nghiệp vụ hiện hành.',
         200, '', 1, @now, 'system', 0, 'huong-dan-su-dung', 1, '', 1);
END

SET @rootId = (SELECT TOP 1 MenuId FROM Menu WHERE QueryString = 'huong-dan-su-dung' AND ParentId = 0 ORDER BY MenuId);

UPDATE Menu
SET
    Title = N'Hướng dẫn sử dụng',
    Description = N'Tài liệu hướng dẫn sử dụng Sàn Giao dịch Công nghệ theo nghiệp vụ hiện hành.',
    StatusId = 1,
    Modified = @now,
    Modifier = 'system'
WHERE MenuId = @rootId;

DECLARE @Menus TABLE
(
    QueryString NVARCHAR(300) NOT NULL PRIMARY KEY,
    Title NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Sort INT NOT NULL
);

INSERT INTO @Menus (QueryString, Title, Description, Sort) VALUES
(N'nguoi-mua-hang', N'Người mua / bên có nhu cầu', N'Tìm sản phẩm, gửi yêu cầu, theo dõi hồ sơ giao dịch và yêu cầu hỗ trợ từ Trung tâm.', 1),
(N'nha-cung-ung', N'Nhà cung ứng / bên bán', N'Cập nhật hồ sơ nhà cung ứng, đăng sản phẩm, xử lý yêu cầu trực tiếp và tham gia hồ sơ RFQ.', 2),
(N'nha-tu-van', N'Chuyên gia tư vấn', N'Đăng ký hồ sơ chuyên gia và tham gia hỗ trợ/tư vấn trong các hồ sơ giao dịch.', 3),
(N'quy-trinh-ho-so-giao-dich', N'Quy trình hồ sơ giao dịch', N'Tóm tắt các bước của hồ sơ giao dịch/chuyển giao công nghệ đang áp dụng trên hệ thống.', 4),
(N'ocop-thiet-bi-truc-tiep', N'OCOP & thiết bị liên hệ trực tiếp', N'Luồng gửi yêu cầu đặt mua/quan tâm trực tiếp, không đi qua quy trình chuyển giao nhiều bước.', 5);

UPDATE m
SET
    m.Title = src.Title,
    m.Description = src.Description,
    m.Sort = src.Sort,
    m.StatusId = 1,
    m.Modified = @now,
    m.Modifier = 'system'
FROM Menu m
JOIN @Menus src ON src.QueryString = m.QueryString
WHERE m.ParentId = @rootId;

INSERT INTO Menu
    (Title, Description, Sort, MenuPosition, StatusId, Created, Creator, ParentId, QueryString, LanguageId, Domain, SiteId)
SELECT
    src.Title, src.Description, src.Sort, '', 1, @now, 'system', @rootId, src.QueryString, 1, '', 1
FROM @Menus src
WHERE NOT EXISTS (
    SELECT 1 FROM Menu m WHERE m.ParentId = @rootId AND m.QueryString = src.QueryString
);

DECLARE @Articles TABLE
(
    QueryString NVARCHAR(300) NOT NULL PRIMARY KEY,
    MenuQueryString NVARCHAR(300) NOT NULL,
    Title NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Body NVARCHAR(MAX) NOT NULL
);

INSERT INTO @Articles (QueryString, MenuQueryString, Title, Description, Body) VALUES
(N'tong-quan-tai-khoan-va-khong-gian-lam-viec', N'nguoi-mua-hang',
 N'Tổng quan tài khoản và không gian làm việc',
 N'Cách đăng nhập, cập nhật hồ sơ và sử dụng menu tài khoản.',
 N'<h2>Đăng nhập và cập nhật hồ sơ</h2>
<p>Sau khi đăng nhập, vào menu tài khoản để cập nhật hồ sơ cá nhân, số điện thoại, địa chỉ và thông tin liên hệ. Các thông tin này được hệ thống dùng để điền sẵn khi bạn gửi yêu cầu chuyển giao hoặc yêu cầu đặt mua/quan tâm sản phẩm.</p>
<h2>Các mục thường dùng</h2>
<ul>
<li><strong>Tổng quan</strong>: xem các thông tin chính của tài khoản.</li>
<li><strong>Hồ sơ giao dịch của tôi</strong>: theo dõi các hồ sơ giao dịch/chuyển giao công nghệ.</li>
<li><strong>Yêu cầu đặt mua của tôi</strong>: theo dõi yêu cầu OCOP hoặc thiết bị liên hệ trực tiếp.</li>
<li><strong>Tin nhắn</strong>: trao đổi với nhà cung ứng hoặc Trung tâm hỗ trợ.</li>
</ul>'),

(N'tim-san-pham-va-chon-dung-luong', N'nguoi-mua-hang',
 N'Tìm sản phẩm và chọn đúng luồng yêu cầu',
 N'Phân biệt luồng liên hệ báo giá/chuyển giao và luồng đặt mua/quan tâm trực tiếp.',
 N'<h2>Tìm công nghệ, thiết bị, sở hữu trí tuệ</h2>
<p>Bạn có thể tìm sản phẩm từ trang danh mục, thanh tìm kiếm, trang OCOP hoặc các khối sản phẩm nổi bật trên trang chủ.</p>
<h2>Chọn đúng nút thao tác</h2>
<ul>
<li><strong>Liên hệ báo giá</strong>: tạo hồ sơ giao dịch/chuyển giao, phù hợp với công nghệ, thiết bị hoặc tài sản trí tuệ cần trao đổi, RFQ, đề xuất, đàm phán và ký hợp đồng.</li>
<li><strong>Gửi yêu cầu đặt mua / quan tâm</strong>: dùng cho sản phẩm OCOP và thiết bị cho phép liên hệ trực tiếp. Luồng này không tạo hồ sơ chuyển giao nhiều bước.</li>
</ul>
<p>Nếu chưa chắc nên chọn luồng nào, bạn có thể dùng nút yêu cầu Trung tâm tư vấn trong hồ sơ hoặc liên hệ Trung tâm để được hỗ trợ.</p>'),

(N'tao-ho-so-giao-dich-chuyen-giao', N'nguoi-mua-hang',
 N'Tạo hồ sơ giao dịch/chuyển giao',
 N'Cách gửi yêu cầu chuyển giao từ sản phẩm và theo dõi hồ sơ được tạo.',
 N'<h2>Tạo hồ sơ từ trang sản phẩm</h2>
<p>Trên trang chi tiết công nghệ, thiết bị hoặc sở hữu trí tuệ, bấm <strong>Liên hệ báo giá</strong>. Hệ thống mở phiếu yêu cầu chuyển giao và tự điền một phần thông tin sản phẩm.</p>
<h2>Điền thông tin yêu cầu</h2>
<p>Kiểm tra họ tên, điện thoại, email, địa chỉ, lĩnh vực và mô tả nhu cầu. Mô tả càng rõ thì nhà cung ứng và Trung tâm càng dễ hỗ trợ.</p>
<h2>Sau khi gửi</h2>
<p>Hệ thống tạo một hồ sơ giao dịch/chuyển giao trong mục <strong>Hồ sơ giao dịch của tôi</strong>. Người gửi yêu cầu được ghi nhận là bên mua của hồ sơ.</p>'),

(N'theo-doi-ho-so-va-yeu-cau-tu-van', N'nguoi-mua-hang',
 N'Theo dõi hồ sơ và yêu cầu Trung tâm tư vấn',
 N'Cách xem tiến độ, thành viên hồ sơ và gửi yêu cầu hỗ trợ/tư vấn.',
 N'<h2>Theo dõi tiến độ</h2>
<p>Vào <strong>Hồ sơ giao dịch của tôi</strong>, chọn hồ sơ cần xem. Trang chi tiết hiển thị thanh tiến độ, vai trò của bạn trong hồ sơ và nội dung của bước hiện tại.</p>
<h2>Thành viên hồ sơ</h2>
<p>Bên mua có thể quản lý thành viên hồ sơ khi chức năng này được mở. Nhà cung ứng được mời qua RFQ hoặc được chọn trong quá trình xử lý sẽ tham gia hồ sơ theo quyền phù hợp.</p>
<h2>Gửi yêu cầu Trung tâm tư vấn</h2>
<p>Trong chi tiết hồ sơ, bấm <strong>Yêu cầu Trung tâm tư vấn</strong>. Chọn loại yêu cầu, bước cần hỗ trợ, nhập tiêu đề và nội dung. Trung tâm sẽ tiếp nhận và phản hồi qua hệ thống tin nhắn/hỗ trợ.</p>'),

(N'dang-ky-ho-so-nha-cung-ung', N'nha-cung-ung',
 N'Đăng ký và cập nhật hồ sơ nhà cung ứng',
 N'Cách tạo hồ sơ nhà cung ứng trước khi đăng sản phẩm hoặc nhận yêu cầu.',
 N'<h2>Tạo hồ sơ nhà cung ứng</h2>
<p>Đăng nhập, vào menu tài khoản và chọn <strong>Hồ sơ nhà cung ứng</strong>. Điền thông tin đơn vị, người đại diện, lĩnh vực hoạt động, dịch vụ khoa học công nghệ và thông tin liên hệ.</p>
<h2>Thông tin nhận chuyển khoản</h2>
<p>Nếu có bán OCOP hoặc thiết bị liên hệ trực tiếp, nên bổ sung số tài khoản, ngân hàng và chủ tài khoản. Thông tin này giúp khách hàng nhìn thấy hướng dẫn thanh toán khi gửi yêu cầu đặt mua/quan tâm.</p>
<h2>Duyệt hồ sơ</h2>
<p>Hồ sơ có thể cần Ban quản trị xét duyệt trước khi hiển thị công khai hoặc được dùng trong các luồng nghiệp vụ liên quan.</p>'),

(N'dang-va-quan-ly-san-pham', N'nha-cung-ung',
 N'Đăng và quản lý sản phẩm',
 N'Cách đăng công nghệ, thiết bị, sở hữu trí tuệ và cấu hình yêu cầu liên hệ.',
 N'<h2>Vào trang quản lý sản phẩm</h2>
<p>Trong menu tài khoản, chọn <strong>Công nghệ & thiết bị</strong>. Tại đây bạn có thể xem danh sách sản phẩm đã đăng, lọc theo trạng thái, chỉnh sửa hoặc tạo mới.</p>
<h2>Chọn loại sản phẩm</h2>
<ul>
<li><strong>Công nghệ</strong>: dùng cho giải pháp, quy trình, công nghệ cần chuyển giao.</li>
<li><strong>Thiết bị</strong>: dùng cho máy móc, thiết bị; có thể bật cho phép gửi yêu cầu đặt mua/quan tâm trực tiếp.</li>
<li><strong>Sở hữu trí tuệ</strong>: dùng cho sáng chế, giải pháp hữu ích, phần mềm hoặc tài sản trí tuệ khác.</li>
</ul>
<h2>Cấu hình quan trọng</h2>
<ul>
<li><strong>Yêu cầu NDA</strong>: nếu bật, hồ sơ giao dịch sẽ yêu cầu xác nhận thỏa thuận bảo mật trước khi sang RFQ.</li>
<li><strong>Cho phép liên hệ/gửi yêu cầu đặt mua trực tiếp</strong>: áp dụng cho thiết bị, đưa sản phẩm sang luồng yêu cầu trực tiếp thay vì hồ sơ chuyển giao nhiều bước.</li>
</ul>
<p>Sau khi gửi, sản phẩm thường ở trạng thái chờ duyệt trước khi hiển thị công khai.</p>'),

(N'tham-gia-rfq-va-nop-ho-so-de-xuat', N'nha-cung-ung',
 N'Tham gia RFQ và nộp hồ sơ đề xuất',
 N'Luồng nhà cung ứng nhận lời mời, ký NDA và gửi hồ sơ đề xuất.',
 N'<h2>Xem lời mời tham gia</h2>
<p>Khi được mời tham gia RFQ, mục <strong>Lời mời tham gia</strong> sẽ xuất hiện trong menu tài khoản. Mỗi lời mời hiển thị hồ sơ, mã RFQ, hạn nộp và trạng thái NDA/đề xuất.</p>
<h2>Ký NDA nếu được yêu cầu</h2>
<p>Nếu hồ sơ yêu cầu bảo mật, nhà cung ứng phải ký NDA trước khi xem chi tiết hồ sơ và nộp đề xuất. Nếu quá hạn nộp hồ sơ, hệ thống có thể không cho ký hoặc gửi đề xuất.</p>
<h2>Nộp hồ sơ đề xuất</h2>
<p>Sau khi đủ điều kiện truy cập, xem yêu cầu RFQ và gửi hồ sơ đề xuất/báo giá theo biểu mẫu. Bên mua sẽ xem xét các đề xuất để chọn bên bán phù hợp.</p>'),

(N'xu-ly-yeu-cau-khach-gui', N'nha-cung-ung',
 N'Xử lý yêu cầu khách gửi trực tiếp',
 N'Cách xác nhận, hoàn tất hoặc hủy yêu cầu đặt mua/quan tâm OCOP và thiết bị trực tiếp.',
 N'<h2>Xem yêu cầu khách gửi</h2>
<p>Nhà cung ứng vào <strong>Yêu cầu khách gửi</strong> để xem các yêu cầu đặt mua/quan tâm sản phẩm OCOP hoặc thiết bị cho phép liên hệ trực tiếp.</p>
<h2>Trao đổi trực tiếp với khách</h2>
<p>Yêu cầu trực tiếp không đi qua quy trình chuyển giao nhiều bước. Nhà cung ứng chủ động liên hệ khách theo số điện thoại/email/địa chỉ giao hàng để thống nhất chi tiết, giao hàng và thanh toán.</p>
<h2>Cập nhật trạng thái</h2>
<ul>
<li><strong>Xác nhận</strong>: đã tiếp nhận và sẽ xử lý yêu cầu.</li>
<li><strong>Hoàn tất</strong>: đã xử lý xong/giao dịch xong.</li>
<li><strong>Hủy</strong>: không thể xử lý yêu cầu hoặc khách không tiếp tục.</li>
</ul>'),

(N'dang-ky-ho-so-chuyen-gia-tu-van', N'nha-tu-van',
 N'Đăng ký hồ sơ chuyên gia tư vấn',
 N'Cách đăng ký thông tin chuyên gia để tham gia mạng lưới tư vấn.',
 N'<h2>Tạo hồ sơ chuyên gia</h2>
<p>Đăng nhập, vào menu tài khoản và chọn <strong>Hồ sơ chuyên gia</strong>. Điền thông tin cá nhân, lĩnh vực chuyên môn, kinh nghiệm, đơn vị công tác và thông tin liên hệ.</p>
<h2>Sau khi gửi hồ sơ</h2>
<p>Hồ sơ chuyên gia có thể cần Ban quản trị xét duyệt trước khi hiển thị trong mạng lưới chuyên gia hoặc được sử dụng để hỗ trợ các yêu cầu tư vấn.</p>'),

(N'tham-gia-ho-tro-tu-van-ho-so-giao-dich', N'nha-tu-van',
 N'Tham gia hỗ trợ/tư vấn hồ sơ giao dịch',
 N'Vai trò của chuyên gia khi được thêm vào hồ sơ hoặc khi Trung tâm điều phối yêu cầu tư vấn.',
 N'<h2>Vai trò chuyên gia tư vấn</h2>
<p>Chuyên gia có thể hỗ trợ các bên trong việc tìm kiếm nhà cung cấp, đánh giá hồ sơ đề xuất, tư vấn đàm phán thương mại, kiểm tra pháp lý hợp đồng hoặc hỗ trợ ký hợp đồng điện tử.</p>
<h2>Trao đổi trong hồ sơ</h2>
<p>Khi được phân công hoặc thêm vào hồ sơ, chuyên gia theo dõi bước liên quan và trao đổi qua kênh hỗ trợ/tin nhắn của hệ thống.</p>'),

(N'quy-trinh-ho-so-giao-dich-6-buoc', N'quy-trinh-ho-so-giao-dich',
 N'Quy trình hồ sơ giao dịch 6 bước',
 N'Giải thích 6 bước chính người dùng nhìn thấy trong hồ sơ giao dịch.',
 N'<h2>Cách hệ thống đang hiển thị quy trình</h2>
<p>Hồ sơ giao dịch/chuyển giao trên hệ thống hiện được trình bày theo 6 bước chính để người dùng dễ theo dõi và thao tác.</p>
<p>Phần đàm phán thương mại và kiểm tra pháp lý hợp đồng được gộp thành một bước thao tác, giúp người dùng theo dõi hồ sơ gọn hơn.</p>
<h2>6 bước tiến trình chính</h2>
<ol>
<li>Yêu cầu chuyển giao công nghệ</li>
<li>Thỏa thuận bảo mật (NDA), nếu áp dụng</li>
<li>Yêu cầu báo giá (RFQ)</li>
<li>Nộp hồ sơ đề xuất</li>
<li>Đàm phán thương mại / Kiểm tra pháp lý hợp đồng</li>
<li>Ký hợp đồng</li>
</ol>'),

(N'nda-rfq-de-xuat-dam-phan-ky-hop-dong', N'quy-trinh-ho-so-giao-dich',
 N'NDA, RFQ, đề xuất, đàm phán và ký hợp đồng',
 N'Tóm tắt các mốc quan trọng trong hồ sơ giao dịch/chuyển giao.',
 N'<h2>NDA</h2>
<p>NDA chỉ áp dụng khi sản phẩm hoặc các bên yêu cầu bảo mật. Nếu không yêu cầu, hệ thống đánh dấu bước này là không áp dụng và cho phép sang RFQ sau khi có yêu cầu chuyển giao.</p>
<h2>RFQ</h2>
<p>Bên mua lập yêu cầu báo giá, mô tả yêu cầu kỹ thuật, hạn nộp và mời nhà cung ứng phù hợp. Nhà cung ứng được mời sẽ thấy lời mời trong tài khoản.</p>
<h2>Hồ sơ đề xuất</h2>
<p>Nhà cung ứng gửi đề xuất/báo giá. Bên mua xem xét, so sánh và chọn bên bán phù hợp để đi tiếp.</p>
<h2>Đàm phán và kiểm tra pháp lý</h2>
<p>Hai bên thống nhất giá, điều khoản thanh toán, phạm vi thực hiện và nội dung hợp đồng. Trên giao diện hiện tại, phần đàm phán và kiểm tra pháp lý được gộp cùng một khu vực thao tác.</p>
<h2>Ký hợp đồng</h2>
<p>Khi hợp đồng sẵn sàng ký, các bên thực hiện ký theo phương thức hệ thống hỗ trợ, gồm ký OTP/CA/USB token tùy cấu hình triển khai.</p>'),

(N'gui-yeu-cau-dat-mua-quan-tam-truc-tiep', N'ocop-thiet-bi-truc-tiep',
 N'Gửi yêu cầu đặt mua/quan tâm trực tiếp',
 N'Luồng dành cho OCOP và thiết bị được bật liên hệ trực tiếp.',
 N'<h2>Khi nào dùng luồng trực tiếp</h2>
<p>Luồng này áp dụng cho sản phẩm OCOP và thiết bị được nhà cung ứng bật <strong>Cho phép liên hệ/gửi yêu cầu đặt mua trực tiếp</strong>. Đây là yêu cầu kết nối nhanh, không phải hồ sơ chuyển giao công nghệ nhiều bước.</p>
<h2>Cách gửi yêu cầu</h2>
<p>Trên trang chi tiết sản phẩm, bấm <strong>Gửi yêu cầu đặt mua / quan tâm</strong>. Nhập số lượng, thông tin liên hệ, địa chỉ giao hàng và ghi chú nếu có.</p>
<h2>Sau khi gửi</h2>
<p>Yêu cầu xuất hiện trong mục <strong>Yêu cầu đặt mua của tôi</strong>. Nhà cung ứng nhận thông báo và liên hệ trực tiếp với bạn để xác nhận, giao hàng hoặc trao đổi thêm.</p>'),

(N'theo-doi-huy-yeu-cau-truc-tiep', N'ocop-thiet-bi-truc-tiep',
 N'Theo dõi và hủy yêu cầu trực tiếp',
 N'Cách người mua xem trạng thái yêu cầu OCOP/thiết bị trực tiếp.',
 N'<h2>Theo dõi yêu cầu</h2>
<p>Vào <strong>Yêu cầu đặt mua của tôi</strong> để xem sản phẩm, số lượng, nhà cung ứng, thông tin thanh toán nếu nhà cung ứng đã cập nhật và trạng thái xử lý.</p>
<h2>Hủy yêu cầu</h2>
<p>Bạn có thể hủy khi yêu cầu còn ở trạng thái mới gửi. Sau khi nhà cung ứng đã xác nhận hoặc hoàn tất, hãy trao đổi trực tiếp với nhà cung ứng nếu cần thay đổi.</p>
<h2>Lưu ý thanh toán</h2>
<p>Nền tảng ghi nhận yêu cầu và kết nối hai bên. Việc giao hàng/thanh toán được các bên thống nhất trực tiếp ngoài hệ thống, trừ khi có cấu hình nghiệp vụ khác trong từng giai đoạn triển khai.</p>');

UPDATE c
SET
    c.Title = src.Title,
    c.Description = src.Description,
    c.Contents = src.Body,
    c.MenuId = m.MenuId,
    c.StatusId = 3,
    c.Modified = @now,
    c.Modifier = 'system',
    c.PublishedDate = COALESCE(c.PublishedDate, @now),
    c.LanguageId = 1,
    c.Domain = 'VN',
    c.SiteId = 1
FROM Contents c
JOIN @Articles src ON src.QueryString = c.QueryString
JOIN Menu m ON m.ParentId = @rootId AND m.QueryString = src.MenuQueryString;

INSERT INTO Contents
    (Title, QueryString, Description, Contents, StatusId, MenuId, Created, Creator, PublishedDate, LanguageId, Domain, SiteId)
SELECT
    src.Title, src.QueryString, src.Description, src.Body, 3, m.MenuId, @now, 'system', @now, 1, 'VN', 1
FROM @Articles src
JOIN Menu m ON m.ParentId = @rootId AND m.QueryString = src.MenuQueryString
WHERE NOT EXISTS (
    SELECT 1 FROM Contents c WHERE c.QueryString = src.QueryString
);

-- Ẩn các bài cũ thuộc cây hướng dẫn nhưng không còn nằm trong cấu trúc v2.
UPDATE c
SET
    c.StatusId = 2,
    c.Modified = @now,
    c.Modifier = 'system'
FROM Contents c
JOIN Menu m ON m.MenuId = c.MenuId
WHERE m.ParentId = @rootId
  AND NOT EXISTS (SELECT 1 FROM @Articles a WHERE a.QueryString = c.QueryString);

-- Nếu đã từng chạy bản nháp dùng slug cũ, ẩn bài đó để tránh trùng nội dung.
UPDATE Contents
SET
    StatusId = 2,
    Modified = @now,
    Modifier = 'system'
WHERE QueryString IN (
    'quy-trinh-hien-thi-7-buoc-va-luu-vet-14-buoc',
    'quy-trinh-hien-thi-6-buoc-va-luu-vet-14-buoc'
);

COMMIT TRANSACTION;

PRINT N'Đã cập nhật dữ liệu hướng dẫn sử dụng v2.';
