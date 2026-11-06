# Rà soát giao diện MekongX — 31/10/2026

> **Kiểm tra lại sau bản sửa tiếp của Claude (chưa commit):** Những phát hiện bên dưới mô tả bản kiểm tra đầu. Bản working tree mới đã sửa bố cục tablet, vị trí dots và hiện tên logo. Đã build lại và chạy Development tại localhost:7234, xem lại trực tiếp các khổ 320/375/390/768px. Kết luận hiện tại: các lỗi chính đã được sửa, còn cách ngắt cụm từ ở màn 320px chưa triệt để. Ảnh mới nằm tại `outputs/ui-review-20261031/recheck/`.
>
> - Tablet 768px: thẻ tin đã rộng khoảng 206px thay vì 87px, hai khối Tin tức/Chuyên gia xếp chồng, nội dung đọc bình thường (`tablet-768.png`).
> - Mobile 375px: dots nằm phía trên khối tìm kiếm, không còn giao nhau; tên đơn vị hiện dưới logo (`partners-375.png`).
> - Sản phẩm 375px: tiêu đề 23px xuống hai dòng gọn, giữ được cụm “Công nghệ” (`san-pham-375.png`).
> - **Còn sót ở 320px:** tiêu đề Sản phẩm vẫn ngắt “Công” / “nghệ” và “Tài” / “sản trí tuệ”; banner vẫn ngắt “Công” / “Nghệ Cần Thơ”. Giảm font theo breakpoint chỉ cải thiện một số độ rộng, chưa bảo đảm giữ cụm từ (`san-pham-320.png`, `home-320.png`).
> - View đã đổi sang Regex nhận cả `<br>`, `<br/>`, `<br />`, không phân biệt hoa thường; CSS mobile cũng mở rộng phạm vi br. Hợp lý hơn bản đầu.
> - **Đính chính vấn đề hình ảnh của lần kiểm tra đầu:** khi chạy đúng cấu hình Development, ảnh banner, sản phẩm, tin và chuyên gia đã hiển thị. Vì vậy không dùng ảnh thiếu ở lần chạy trước để kết luận lỗi dữ liệu hay mã giao diện của Claude.
> - Chưa kiểm Safari/iPhone thật hoặc xác nhận vuốt cảm ứng. Không sửa mã ứng dụng trong lần kiểm tra lại này.

## Kết luận

**Chưa nên chốt giao diện đạt yêu cầu để bàn giao.** Bản sửa `363e5e6` cải thiện số điện thoại và cách xếp logo trên mobile, nhưng còn lỗi bố cục tablet, chấm chuyển banner bị khối tìm kiếm phủ lên và tên đơn vị phụ thuộc hover. Cân dòng bằng CSS chưa bảo đảm ngắt theo cụm từ tiếng Việt.

Đây là báo cáo kiểm tra; chưa sửa mã ứng dụng, chưa đóng gói hoặc triển khai. Phần PDF/OTP nằm ngoài phạm vi.

## Phạm vi và bằng chứng

- Mã nguồn tại HEAD `80972c4`, nhánh `feature/bo-sung-chuc-nang-hsmt`; đọc tài liệu `Trien_khai_mekongx_31102026.md` và diff `363e5e6`.
- Build và chạy bản local tại `http://localhost:7232`; build thành công, có các cảnh báo sẵn có.
- Trình duyệt Chromium trong ứng dụng; kích thước viewport 320, 375, 390, 430, 768, 1024, 1440px. Thanh cuộn desktop chiếm khoảng 15px, nên chiều rộng vùng tài liệu nhỏ hơn viewport. Đây là kiểm tra responsive trên desktop, không phải kiểm thử Safari/iPhone thật hay giả lập đầy đủ thiết bị cảm ứng.
- Đo kích thước tài liệu/trang và xem DOM của 11 URL trang con ở 375px. Xem ảnh trực tiếp trang chủ, menu mobile, chân trang, Giới thiệu, Sản phẩm, Dịch vụ, Tin tức, Tìm kiếm, Liên hệ, OCOP và Chuyên gia. Không coi việc đo không tràn ngang là bằng chứng toàn bộ nội dung một trang đã đạt thẩm mỹ.
- Dữ liệu đo: `outputs/ui-review-20261031/viewport-results.json`, `page-results.json`. Ảnh bằng chứng cùng thư mục.

## Các vấn đề cần sửa

### 1. Ưu tiên cao — Thẻ Tin tức và Chuyên gia quá hẹp trên tablet

Tại viewport **768px**, `.home-v2-grid--news` chia hai cột, mỗi cột khoảng 339px. Bên trong mỗi khối, `.home-v2-news-list` và `.home-v2-expert-list` tiếp tục chia ba cột. Mỗi thẻ chỉ còn khoảng **87px vùng trong**, chữ rơi từng từ, thẻ tin cao khoảng **733px**. Đây là lỗi bố cục rõ ràng dù trang không tràn ngang.

- Mã liên quan: `wwwroot/css/home-redesign.css:2076` (ba cột bên trong), `:2497` (hai cột bên ngoài), `:2645` (chỉ chuyển một cột khi ≤767px).
- Ảnh: `outputs/ui-review-20261031/home-768-news-experts.png`.
- Hướng sửa: có breakpoint tablet riêng; đưa hai khối lớn về một cột hoặc giảm số cột thẻ bên trong theo chiều rộng thực tế. Kiểm lại 767/768, 820, 991/992, 1024 và 1199/1200px.
- Vấn đề đã có trong bố cục hiện tại, không phải do riêng phần CSS mới của commit `363e5e6` gây ra.

### 2. Ưu tiên vừa — Khối tìm kiếm phủ lên chấm chuyển banner trên mobile

Đã đo ở 320, 375, 390 và 430px: vùng chấm chuyển banner nằm trong vùng khối tìm kiếm. Ví dụ tại 375px, khối tìm kiếm bắt đầu khoảng **y=301.6px**, chấm chuyển banner bắt đầu khoảng **y=316.6px**. Ảnh cho thấy chấm trắng nằm trên nền trắng trong phần đầu khối tìm kiếm, mất vị trí và độ tương phản.

- Nguyên nhân: `.home-v2-search-wrap--overlap` giữ `margin-top: -35px` và wrapper có z-index 3; mobile lại đưa dots xuống `bottom: 12px`, z-index 2.
- Mã liên quan: `wwwroot/css/home-redesign.css:879`, `:1058`, `:1064`, `:2560`.
- Ảnh: `outputs/ui-review-20261031/home-375-top.png`.
- Hướng sửa: chừa riêng vùng điều khiển trong banner, điều chỉnh overlap hoặc bỏ overlap trên mobile. Kiểm cả hai slide và lúc chuyển slide.

### 3. Ưu tiên vừa — Tên doanh nghiệp/đối tác chỉ hiện khi hover

Các logo có tên thật trong DOM, nhưng `.home-v2-partner--logo .home-v2-partner-name` có `opacity: 0`, chỉ đổi thành 1 khi `:hover`. Phần tử chứa là `div`, không có đường tương tác bàn phím để hiện tên. Ở giao diện mobile mặc định chỉ nhìn thấy logo nhỏ; không thể dựa vào hover để người dùng cảm ứng nhận diện đơn vị.

- Mã liên quan: `wwwroot/css/home-redesign.css:2227`, `:2248`; `Views/Home/Index.cshtml:223`, `:242`.
- Ảnh: `outputs/ui-review-20261031/partners-375.png`.
- Hướng sửa: luôn hiện tên gọn dưới logo ở mobile/cảm ứng; cho phép hai dòng tên, giữ chiều cao thẻ đồng đều. Không cần đổi dữ liệu.
- Hai hàng logo đã có overflow riêng và cách xếp khoảng 2,5 ô đúng hướng. Chưa xác nhận thao tác vuốt/snap trên điện thoại thật.

### 4. Cần chỉnh typography — Cân dòng chưa giữ được cụm từ tiếng Việt

Ảnh mobile 375px cho thấy banner ngắt `... Công` / `Nghệ Cần Thơ`; trang Sản phẩm ngắt `Sản phẩm Công` / `nghệ, Thiết bị &` / `Tài sản trí tuệ`. Đây là cách ngắt theo độ rộng, chưa có chủ ý theo nghĩa. `text-wrap: balance` chỉ cân độ dài dòng, không tự giữ cụm “Công nghệ”. Riêng tiêu đề banner còn bị ép trở lại `text-wrap: wrap`.

- Mã liên quan: `wwwroot/css/home-redesign.css:2685`, `:2695`; tiêu đề banner `:920`.
- Ảnh: `home-375-top.png`, `san-pham-375.png` trong thư mục bằng chứng.
- Hướng sửa: điều chỉnh cỡ chữ/độ rộng theo từng nhóm tiêu đề; giữ các cụm ngắn có nghĩa như “Công nghệ”, “Cần Thơ” tại các vị trí cần thiết. Không áp dụng nowrap cho cả tiêu đề dài.
- Tiêu đề banner có line-clamp ba dòng: nội dung hiện tại chưa tái hiện mất chữ, nhưng cần kiểm nếu CMS đổi tiêu đề dài hơn. Đây là rủi ro bảo trì, không phải lỗi cắt nội dung đã xác nhận.

## Đề xuất cải thiện trải nghiệm

### Trang chủ vẫn dài và nhiều khoảng trống trên điện thoại

Chiều cao tài liệu ở 375px khoảng **8.426px**. Sau khi rút gọn logo, các nhóm thống kê, ô chức năng và sáu bước quy trình vẫn xếp hoàn toàn một cột. Nên cân nhắc thống kê hai cột và quy trình dạng danh sách dọc gọn hơn. Đây là đề xuất thiết kế, không phải lỗi chức năng hay kết luận trái yêu cầu khách đã duyệt.

Ảnh: `outputs/ui-review-20261031/home-375-full.png`.

### Hình ảnh ở bản local chưa đủ để đánh giá bản bàn giao

Trang chủ đang dùng ảnh mặc định “NO IMAGE AVAILABLE” ở nhiều sản phẩm/tin; ảnh chuyên gia là ảnh mặc định, banner hiển thị nền màu thay vì ảnh minh họa. Trang danh sách tin có ảnh tải lỗi. Những biểu hiện này làm giao diện kém hoàn thiện, nhưng **chưa kết luận máy chủ đang bị cùng lỗi**: cần đối chiếu dữ liệu và thư mục upload của môi trường bàn giao. Không nên đánh giá chất lượng giao diện chỉ trên dữ liệu thiếu ảnh.

Ảnh: `outputs/ui-review-20261031/tin-su-kien-375.png` và ảnh toàn trang chủ.

## Những phần đã đạt trong phạm vi kiểm tra

- Số điện thoại chân trang hiển thị liền một dòng, liên kết thực tế `tel:+842923907037`; ảnh `footer-375.png`. Chưa thực hiện cuộc gọi.
- Mô tả banner hiện tại có `<br>` trong đoạn `p`; bản mobile đã bỏ ngắt dòng cứng, không thấy từ “nghệ,” đứng riêng như phản ánh trong tài liệu.
- Hai danh sách logo hiển thị một hàng cuộn riêng ở mobile; desktop dùng lưới.
- Không ghi nhận tràn ngang toàn tài liệu ở bảy kích thước trang chủ và 11 URL trang con đã đo. Một số carousel/filter có phần tử nằm ngoài khung do cuộn ngang có chủ đích.
- Menu mobile mở được, bố cục các mục rõ ở 375px.

## Lưu ý về độ bền của bản sửa

- `Description.Replace("<br>", "<br> ")` chỉ thêm khoảng trắng với đúng chuỗi `<br>` viết thường. Nếu CMS dùng `<br/>`, `<br />` hoặc biến thể chữ hoa, CSS vẫn có thể ẩn phần tử br trong p nhưng không tự thêm dấu cách giữa hai từ. Chưa gặp lỗi dính từ trên dữ liệu hiện tại. Nên chuẩn hóa bằng parser HTML hiện có nếu muốn bao phủ các biến thể.
- CSS ẩn br chỉ áp dụng `.home-v2-hero__content p br`, không bao phủ mọi cấu trúc HTML CMS. Không nên ghi nhận là đã xử lý mọi loại nội dung banner.
- `text-wrap` là cải thiện phụ thuộc trình duyệt, chưa có fallback xử lý riêng cho tiêu đề quan trọng ở máy cũ; phạm vi iPhone/Safari thật vẫn cần kiểm sau khi chỉnh.

## Thứ tự xử lý đề nghị

1. Sửa breakpoint tablet cho Tin tức/Chuyên gia và vị trí dots banner.
2. Hiện tên đơn vị trên cảm ứng; chỉnh cách ngắt cụm từ ở tiêu đề quan trọng.
3. Rút gọn các khối mobile dài sau khi thống nhất cách trình bày với khách.
4. Bổ sung/đối chiếu ảnh đúng môi trường, kiểm lại Safari iPhone thật, vuốt logo, hai slide và menu. Sau đó mới đóng gói.
