/*
Seeds the "Hướng dẫn sử dụng" (User Guide) docs section: 1 root Menu, 3 role
sub-menus (Người mua hàng, Nhà cung ứng, Nhà tư vấn), and sample Content
articles per role. Reuses the existing Menu/Contents tables (no schema change).
Guarded with IF NOT EXISTS so the script is safe to re-run.
*/

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

DECLARE @now DATETIME = SYSDATETIME();
DECLARE @rootId INT, @muaHangId INT, @nccId INT, @tuVanId INT;

IF NOT EXISTS (SELECT 1 FROM Menu WHERE QueryString = 'huong-dan-su-dung')
BEGIN
    INSERT INTO Menu (Title, Description, Sort, MenuPosition, StatusId, Created, Creator, ParentId, QueryString, LanguageId, Domain, SiteId)
    VALUES (N'Hướng dẫn sử dụng', N'Tài liệu hướng dẫn sử dụng Sàn Giao dịch Công nghệ theo từng vai trò.', 200, '', 1, @now, 'system', 0, 'huong-dan-su-dung', 1, '', 1);
END
SET @rootId = (SELECT MenuId FROM Menu WHERE QueryString = 'huong-dan-su-dung');

IF NOT EXISTS (SELECT 1 FROM Menu WHERE QueryString = 'nguoi-mua-hang' AND ParentId = @rootId)
BEGIN
    INSERT INTO Menu (Title, Sort, StatusId, Created, Creator, ParentId, QueryString, LanguageId, Domain, SiteId)
    VALUES (N'Người mua hàng', 1, 1, @now, 'system', @rootId, 'nguoi-mua-hang', 1, '', 1);
END
SET @muaHangId = (SELECT MenuId FROM Menu WHERE QueryString = 'nguoi-mua-hang' AND ParentId = @rootId);

IF NOT EXISTS (SELECT 1 FROM Menu WHERE QueryString = 'nha-cung-ung' AND ParentId = @rootId)
BEGIN
    INSERT INTO Menu (Title, Sort, StatusId, Created, Creator, ParentId, QueryString, LanguageId, Domain, SiteId)
    VALUES (N'Nhà cung ứng', 2, 1, @now, 'system', @rootId, 'nha-cung-ung', 1, '', 1);
END
SET @nccId = (SELECT MenuId FROM Menu WHERE QueryString = 'nha-cung-ung' AND ParentId = @rootId);

IF NOT EXISTS (SELECT 1 FROM Menu WHERE QueryString = 'nha-tu-van' AND ParentId = @rootId)
BEGIN
    INSERT INTO Menu (Title, Sort, StatusId, Created, Creator, ParentId, QueryString, LanguageId, Domain, SiteId)
    VALUES (N'Nhà tư vấn', 3, 1, @now, 'system', @rootId, 'nha-tu-van', 1, '', 1);
END
SET @tuVanId = (SELECT MenuId FROM Menu WHERE QueryString = 'nha-tu-van' AND ParentId = @rootId);

-- ── Người mua hàng ──

IF NOT EXISTS (SELECT 1 FROM Contents WHERE QueryString = 'gui-yeu-cau-chuyen-giao-cong-nghe')
BEGIN
    INSERT INTO Contents (Title, QueryString, Description, Contents, StatusId, MenuId, Created, Creator, PublishedDate, LanguageId, Domain, SiteId)
    VALUES (
        N'Tìm công nghệ, thiết bị và gửi yêu cầu chuyển giao',
        'gui-yeu-cau-chuyen-giao-cong-nghe',
        N'Hướng dẫn tìm kiếm sản phẩm và gửi yêu cầu chuyển giao công nghệ, thiết bị, sáng chế.',
        N'<h2>Tìm kiếm sản phẩm</h2>
<p>Vào menu <strong>Sản phẩm</strong> trên trang chủ để xem danh sách Công nghệ, Thiết bị, Sáng chế/Sở hữu trí tuệ theo từng lĩnh vực. Bạn cũng có thể dùng ô tìm kiếm ở góc trên bên phải để tìm nhanh theo từ khóa.</p>
<h2>Gửi yêu cầu chuyển giao</h2>
<p>Trên trang chi tiết sản phẩm, bấm nút <strong>Liên hệ báo giá</strong> để mở "Phiếu yêu cầu chuyển giao công nghệ". Điền thông tin liên hệ, mô tả nhu cầu và bấm <strong>Gửi yêu cầu</strong>.</p>
<h2>Theo dõi tiến trình</h2>
<p>Sau khi gửi, hệ thống tạo một dự án và bạn sẽ đi qua các bước sau:</p>
<ul>
<li>Yêu cầu chuyển giao công nghệ</li>
<li>Thỏa thuận bảo mật (NDA)</li>
<li>Yêu cầu báo giá (RFQ)</li>
<li>Nộp hồ sơ đề xuất</li>
<li>Đàm phán thương mại</li>
<li>Kiểm tra pháp lý</li>
<li>Ký hợp đồng điện tử</li>
</ul>
<p>Xem lại tiến độ bất cứ lúc nào tại mục <strong>Dự án của tôi</strong> trong menu tài khoản.</p>',
        3, @muaHangId, @now, N'system', @now, 1, 'VN', 1);
END

IF NOT EXISTS (SELECT 1 FROM Contents WHERE QueryString = 'dat-mua-san-pham-ocop')
BEGIN
    INSERT INTO Contents (Title, QueryString, Description, Contents, StatusId, MenuId, Created, Creator, PublishedDate, LanguageId, Domain, SiteId)
    VALUES (
        N'Đặt mua sản phẩm OCOP',
        'dat-mua-san-pham-ocop',
        N'Hướng dẫn đặt mua đặc sản OCOP, chọn hình thức thanh toán và theo dõi đơn hàng.',
        N'<h2>Chọn sản phẩm OCOP</h2>
<p>Vào menu <strong>OCOP</strong> để xem Góc trưng bày sản phẩm OCOP kèm mã truy xuất nguồn gốc, hạng sao OCOP và thông tin nhà cung ứng.</p>
<h2>Đặt mua</h2>
<p>Trên trang chi tiết sản phẩm, bấm <strong>Liên hệ đặt mua</strong>, điền số lượng, địa chỉ giao hàng, và chọn hình thức thanh toán:</p>
<ul>
<li><strong>COD</strong> — thanh toán khi nhận hàng</li>
<li><strong>Chuyển khoản</strong> — nhà cung ứng sẽ gửi thông tin tài khoản (nếu đã cập nhật, hệ thống hiện sẵn số tài khoản/ngân hàng ngay trên trang xác nhận)</li>
</ul>
<h2>Theo dõi và huỷ đơn</h2>
<p>Xem lại đơn đã đặt tại mục <strong>Đơn hàng OCOP của tôi</strong> trong menu tài khoản. Bạn có thể bấm <strong>Huỷ đơn</strong> khi đơn còn ở trạng thái "Mới đặt". Nhà cung ứng sẽ liên hệ qua số điện thoại bạn cung cấp để xác nhận và giao hàng.</p>',
        3, @muaHangId, @now, N'system', @now, 1, 'VN', 1);
END

-- ── Nhà cung ứng ──

IF NOT EXISTS (SELECT 1 FROM Contents WHERE QueryString = 'dang-ky-ho-so-nha-cung-ung')
BEGIN
    INSERT INTO Contents (Title, QueryString, Description, Contents, StatusId, MenuId, Created, Creator, PublishedDate, LanguageId, Domain, SiteId)
    VALUES (
        N'Đăng ký hồ sơ nhà cung ứng',
        'dang-ky-ho-so-nha-cung-ung',
        N'Hướng dẫn đăng ký và cập nhật hồ sơ nhà cung ứng, bao gồm thông tin nhận chuyển khoản.',
        N'<h2>Đăng ký hồ sơ</h2>
<p>Đăng nhập, vào menu tài khoản &gt; <strong>Hồ sơ nhà cung ứng</strong>. Điền đầy đủ các trường bắt buộc (đánh dấu *): tên đơn vị, loại hình tổ chức, địa chỉ, người đại diện, lĩnh vực hoạt động, dịch vụ KH&amp;CN... rồi bấm <strong>Cập nhật hồ sơ</strong>.</p>
<p>Hồ sơ sẽ chuyển sang trạng thái "Đang duyệt", chờ Ban quản trị xét duyệt trước khi hiển thị công khai.</p>
<h2>Bổ sung thông tin nhận chuyển khoản</h2>
<p>Trong cùng form hồ sơ có mục <strong>Thông tin nhận chuyển khoản</strong>: Số tài khoản, Ngân hàng, Chủ tài khoản. Thông tin này sẽ tự động hiện cho khách hàng khi họ đặt mua sản phẩm OCOP và chọn hình thức thanh toán "Chuyển khoản" — giúp khách không cần gọi điện hỏi lại.</p>',
        3, @nccId, @now, N'system', @now, 1, 'VN', 1);
END

IF NOT EXISTS (SELECT 1 FROM Contents WHERE QueryString = 'quan-ly-san-pham-nha-cung-ung')
BEGIN
    INSERT INTO Contents (Title, QueryString, Description, Contents, StatusId, MenuId, Created, Creator, PublishedDate, LanguageId, Domain, SiteId)
    VALUES (
        N'Đăng sản phẩm và quản lý danh mục',
        'quan-ly-san-pham-nha-cung-ung',
        N'Hướng dẫn đăng và quản lý sản phẩm Công nghệ, Thiết bị, Sở hữu trí tuệ.',
        N'<h2>Vào trang quản lý</h2>
<p>Vào menu tài khoản &gt; <strong>Công nghệ &amp; thiết bị</strong> để xem danh sách sản phẩm bạn đã đăng.</p>
<h2>Tạo sản phẩm mới</h2>
<p>Chọn loại sản phẩm cần đăng: <strong>Công nghệ</strong>, <strong>Thiết bị</strong>, hoặc <strong>Sở hữu trí tuệ</strong>. Điền thông tin mô tả, thông số kỹ thuật, giá tham khảo, hình ảnh minh họa rồi gửi để Ban quản trị duyệt trước khi xuất bản.</p>',
        3, @nccId, @now, N'system', @now, 1, 'VN', 1);
END

IF NOT EXISTS (SELECT 1 FROM Contents WHERE QueryString = 'xu-ly-don-ocop-khach-dat')
BEGIN
    INSERT INTO Contents (Title, QueryString, Description, Contents, StatusId, MenuId, Created, Creator, PublishedDate, LanguageId, Domain, SiteId)
    VALUES (
        N'Xử lý đơn khách đặt mua OCOP',
        'xu-ly-don-ocop-khach-dat',
        N'Hướng dẫn xác nhận, giao hàng và huỷ đơn OCOP khách đặt.',
        N'<h2>Xem đơn mới</h2>
<p>Vào menu tài khoản &gt; <strong>Đơn OCOP khách đặt</strong> để xem toàn bộ đơn khách đặt mua sản phẩm của bạn, kèm số điện thoại, địa chỉ giao hàng và hình thức thanh toán khách đã chọn.</p>
<h2>Xác nhận và giao hàng</h2>
<p>Liên hệ khách qua số điện thoại trên đơn để xác nhận, thỏa thuận thời gian giao hàng và thanh toán (thanh toán/giao hàng thực hiện trực tiếp giữa hai bên, ngoài hệ thống). Sau đó:</p>
<ul>
<li>Bấm <strong>Xác nhận</strong> để chuyển đơn sang trạng thái "Đã xác nhận"</li>
<li>Bấm <strong>Đánh dấu hoàn tất</strong> sau khi đã giao hàng xong</li>
<li>Bấm <strong>Huỷ đơn</strong> nếu không thể xử lý được đơn này</li>
</ul>',
        3, @nccId, @now, N'system', @now, 1, 'VN', 1);
END

-- ── Nhà tư vấn ──

IF NOT EXISTS (SELECT 1 FROM Contents WHERE QueryString = 'dang-ky-ho-so-chuyen-gia-tu-van')
BEGIN
    INSERT INTO Contents (Title, QueryString, Description, Contents, StatusId, MenuId, Created, Creator, PublishedDate, LanguageId, Domain, SiteId)
    VALUES (
        N'Đăng ký hồ sơ chuyên gia tư vấn',
        'dang-ky-ho-so-chuyen-gia-tu-van',
        N'Hướng dẫn đăng ký hồ sơ chuyên gia và nhận yêu cầu tư vấn.',
        N'<h2>Đăng ký hồ sơ</h2>
<p>Vào menu tài khoản &gt; <strong>Hồ sơ chuyên gia</strong>, điền thông tin chuyên môn, lĩnh vực tư vấn, kinh nghiệm công tác rồi gửi đăng ký.</p>
<h2>Nhận yêu cầu tư vấn</h2>
<p>Sau khi hồ sơ được Ban quản trị duyệt và xuất bản, khách hàng có thể tìm thấy bạn trong mục <strong>Dịch vụ tư vấn</strong> trên trang chủ và liên hệ trực tiếp để được hỗ trợ.</p>',
        3, @tuVanId, @now, N'system', @now, 1, 'VN', 1);
END
