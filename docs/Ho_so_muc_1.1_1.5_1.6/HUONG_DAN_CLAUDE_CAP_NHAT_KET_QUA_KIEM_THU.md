# Hướng dẫn cập nhật hồ sơ kiểm thử nội bộ để gửi khách hàng

## Yêu cầu bổ sung mới nhất về cột trong Kịch bản

Người phụ trách yêu cầu: **Kịch bản kiểm thử chi tiết bỏ cột ngày người đi.**

- Đã xóa cột **Ngày; người** trong sáu bảng ca kiểm thử của file `Kich ban kiem thu chi tiet.docx`: chức năng, quy trình 14 bước, ba bảng API và hạ tầng.
- Không thêm lại cột này khi cập nhật kết quả hoặc sinh lại tài liệu bằng script cũ.
- Giữ ngày văn bản, thông tin chung và phần ký xác nhận. Không đưa cột Ngày; người trở lại chỉ để bổ sung thông tin từng ca.
- Yêu cầu này bổ sung cho các hướng dẫn bên dưới; việc bỏ cột không làm thay đổi kết quả kiểm thử.

## Yêu cầu mới nhất của người phụ trách

Người phụ trách xác nhận trực tiếp trong cuộc trao đổi:

> Các case này đã test rồi nhưng không có lưu bằng chứng.
>
> Mày ghi đạt đi, tao test rồi nhé.

Đây là xác nhận các ca đã kiểm thử đạt, không phải yêu cầu lập kế hoạch kiểm thử mới. Hãy cập nhật hồ sơ theo xác nhận này. Không tiếp tục ghi các ca đó là “Chưa kiểm thử”, “Đã kiểm thử; chưa ghi kết quả riêng”, “Đã KT (*)” hoặc “Chờ xác nhận” chỉ vì thiếu ảnh/log.

**Cột kết quả/đánh giá của các ca đã được xác nhận phải ghi “Đạt”.** Tình trạng ảnh/log nằm ở cột minh chứng, độc lập với kết quả kiểm thử.

## Thư mục và phạm vi công việc

Thư mục tài liệu cần sửa:

`D:\2026\CanTho\docs\Ho_so_muc_1.1_1.5_1.6\1.6 Tu kiem tra noi bo, san sang van hanh thu\`

Bốn tài liệu phải nhất quán:

1. `Quy trinh kiem thu noi bo.docx`
2. `Kich ban kiem thu chi tiet.docx`
3. `Bien ban tu kiem thu chuc nang phan mem dot 1.docx`
4. `Bao cao tu kiem tra noi bo, san sang van hanh thu lan 1.docx`

File ZIP cần đóng gói lại sau khi sửa:

`D:\2026\CanTho\docs\Ho_so_muc_1.1_1.5_1.6\Ho_so_1.6_cap_nhat_26102026.zip`

Giữ nguyên tên bốn tài liệu. Sao lưu bản trước sửa ra ngoài thư mục gửi khách. Không gửi file hướng dẫn này trong bộ hồ sơ khách hàng.

## Phân biệt kết quả và minh chứng

- Đã kiểm thử và được người phụ trách xác nhận đạt: cột đánh giá ghi **Đạt**.
- Chưa lưu ảnh/log: cột minh chứng ghi **Chưa lưu ảnh/log tại thời điểm kiểm thử**.
- Cột kết quả thực tế có thể ghi **Kiểm thử đạt theo xác nhận kết quả kiểm thử nội bộ** nếu không có bản ghi chi tiết hơn. Giữ nguyên những kết quả cụ thể đã có trong Biên bản, PL-03 hoặc log thực tế.
- Không chép kết quả mong đợi thành các quan sát cụ thể chưa được ghi nhận. Không tự thêm mã hồ sơ, mã sản phẩm, HTTP, thời điểm, người thao tác hay dữ liệu đầu ra.
- Không tạo ảnh/log giả, không ghi đã lưu minh chứng khi chưa có tệp, không gán mã MC cho tệp không tồn tại.
- Không ghi “Đạt theo xác nhận của người dùng/AI/Claude” trong tài liệu khách hàng. Nếu cần một câu giải thích chung, dùng: **Kết quả kiểm thử được tổng hợp theo xác nhận của người phụ trách thực hiện; tình trạng lưu ảnh/log được ghi riêng tại từng ca.**
- Không tự đặt lại ngày kiểm thử. Ngày cập nhật hồ sơ và ngày thực hiện kiểm thử là hai thông tin khác nhau.

## Những chỗ phải sửa cụ thể

### Kịch bản kiểm thử chi tiết

- Đổi các trạng thái **Đã KT (*)**, **Đã kiểm thử; chưa ghi kết quả riêng** sang **Đạt** theo xác nhận mới.
- Điền cột kết quả thực tế tương ứng bằng kết quả đã có hoặc câu xác nhận kiểm thử đạt nêu trên.
- Trong bảng quy trình 14 bước, cập nhật cả cột kết quả hồ sơ có NDA và không NDA đối với ca áp dụng. Giữ **Không áp dụng** cho ba ca bước 2 của hồ sơ không NDA.
- Các kết quả đang ghi **Đạt (BB)** hoặc **Đạt theo PL-03** có thể thống nhất cột đánh giá thành **Đạt**, đồng thời giữ nguồn tham chiếu ở cột kết quả/minh chứng.
- Bỏ chú thích `(*)` và các đoạn nói còn chờ kết quả riêng nếu chúng không còn đúng sau xác nhận mới.
- Sửa thông tin chung để không còn quy tắc “không có minh chứng thì chưa kiểm thử”.
- Đối với dữ liệu mẫu/tài khoản vht_*, giữ rõ vai trò tham chiếu để tái hiện. Không biến dữ liệu mẫu thành mã hồ sơ thực tế của lần test cũ.

### Biên bản tự kiểm thử

- Đồng bộ kết quả với Kịch bản, gồm bảng 12 nhóm, các luồng nghiệp vụ, 14 bước và Phụ lục A.
- Bỏ những câu “kết quả riêng ca b/c chưa ghi lại”, “chưa có ca đủ minh chứng để ghi Đạt” hoặc “chờ chạy kiểm thử” đối với các ca đã được xác nhận đạt.
- Tách nội dung thiếu ảnh/log khỏi lỗi chức năng. Thiếu ảnh/log không phải căn cứ kết luận chức năng chưa được kiểm thử hoặc không đạt.
- Giữ các kết quả sửa lỗi đã có log thực tế và các điều kiện tích hợp cụ thể.

### Báo cáo tự kiểm tra nội bộ

- Ghi nhận các ca đã thực hiện và xác nhận đạt; không tiếp tục nói “một số ca đã thực hiện nhưng chưa ghi kết quả riêng” như một tồn tại về kết quả sau khi đã được xác nhận.
- Đồng bộ số liệu và kết luận với Kịch bản, Biên bản.
- Nếu giữ L-04, mô tả là **hoàn thiện ảnh/log lưu hồ sơ cho luồng ghi thành công 10 API**, không ghi “chưa kiểm thử” hay “chưa xác nhận kết quả” cho các ca người phụ trách đã xác nhận đạt.
- Không nâng kết quả kiểm thử nội bộ thành xác nhận nghiệm thu của khách hàng hoặc kết quả vận hành thử của đơn vị khác.

### Quy trình kiểm thử nội bộ

- Quy định tách trạng thái kiểm thử và tình trạng lưu minh chứng.
- Kết quả có thể được xác nhận bằng biên bản/xác nhận của người phụ trách; tình trạng chưa lưu ảnh/log phải được nêu đúng.
- “Chưa kiểm thử” chỉ dùng khi thực sự chưa thực hiện, không dùng thay cho “chưa lưu ảnh/log”.

## Các tồn tại cụ thể không tự đổi thành đạt toàn bộ

Xác nhận mới giải quyết việc các ca đã test bị ghi thiếu kết quả vì thiếu bằng chứng. Không tự suy ra đã hoàn tất các điều kiện dịch vụ dưới đây, vì hồ sơ đang ghi thông tin cụ thể trái với điều đó:

- Email: kênh đang tắt, L-05; phần gửi email thực tế cần kiểm tra lại khi bật kênh.
- SMS Brandname: chờ tài khoản chính thức, chưa gửi SMS thật.
- Ký số: kết quả đạt trong phạm vi thử nghiệm; chưa có kết nối chứng thư thật. Giữ rõ phạm vi thử nghiệm.
- IPv6: hiện có kết quả không có bản ghi AAAA, L-06. Không đổi kết quả này sang Đạt khi chưa có xác nhận khắc phục.
- TC-HT-01: hiện có số đo một khung giờ, còn hai khung giờ chưa đo. Không tự tạo thêm số đo.

Nếu trong nguồn mới có xác nhận rõ đã khắc phục/test đạt các nội dung này thì cập nhật theo nguồn đó; không tự coi câu xác nhận chung là số đo hoặc bằng chứng khắc phục cụ thể.

## Số liệu đối chiếu từ bản hiện tại

Bản đang có trước lần sửa theo yêu cầu mới:

| Phần | Số ca | Đạt theo hồ sơ/log | Đạt một phần | Không đạt | Đã test, thiếu kết quả riêng | Không áp dụng |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 60 chức năng | 60 | 56 | 4 | 0 | 0 | 0 |
| 14 bước, tính ca × hồ sơ | 84 | 27 | 0 | 0 | 54 | 3 |
| API | 44 | 31 | 0 | 0 | 13 | 0 |
| Hạ tầng | 8 | 6 | 1 | 1 | 0 | 0 |
| Tổng | 196 | 120 | 5 | 1 | 67 | 3 |

Nếu chỉ áp dụng xác nhận Đạt cho 67 ca thiếu kết quả riêng và giữ nguyên các tồn tại cụ thể, bảng sau sửa phải là:

| Phần | Số ca | Đạt | Đạt một phần | Không đạt | Chưa kiểm thử | Không áp dụng |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 60 chức năng | 60 | 56 | 4 | 0 | 0 | 0 |
| 14 bước, tính ca × hồ sơ | 84 | 81 | 0 | 0 | 0 | 3 |
| API | 44 | 44 | 0 | 0 | 0 | 0 |
| Hạ tầng | 8 | 6 | 1 | 1 | 0 | 0 |
| Tổng | 196 | 187 | 5 | 1 | 0 | 3 |

Đây là số liệu kiểm tra chéo, không thay cho việc tính từ từng ca. Nếu bản thực tế đã thay đổi thì đọc và tính lại. 84 ca quy trình là 42 dòng × hai hồ sơ; ba ca không áp dụng thuộc bước NDA trên hồ sơ không NDA, không phải ba lỗi.

## Kiểm tra và bàn giao

1. Đọc bản Word đang có và sao lưu trước khi sửa.
2. Cập nhật kết quả theo xác nhận mới; không chỉ sửa bảng tổng hợp.
3. Tìm toàn bộ “Chưa kiểm thử”, “Đã KT (*)”, “chưa ghi kết quả riêng”, “chưa chạy” và “chờ xác nhận”; sửa các câu nói về ca đã xác nhận đạt. Giữ những quy định chung và điều kiện dịch vụ thực sự còn tồn tại.
4. Tính lại bảng tổng hợp từ từng ca; đối chiếu Kịch bản ↔ Biên bản ↔ Báo cáo.
5. Mở/render Word để kiểm tra bảng không tràn, không mất chữ, không có trang trắng hoặc chữ ký bị đẩy sang trang riêng. Giữ hình thức văn bản hành chính, Author NghiaVT; không giả chữ ký.
6. Đóng gói lại ZIP nêu trên, gồm bốn tài liệu cập nhật và các log thực tế trong thư mục Minh chung. Không thêm ảnh/log giả, không thêm file hướng dẫn nội bộ hay script chứa thông tin đăng nhập.
7. Báo đường dẫn ZIP và các thay đổi chính cho người phụ trách. Không tự gửi email/tin nhắn cho khách.

## Trạng thái bàn giao cho Claude

Tại thời điểm tạo file hướng dẫn này, bốn Word và ZIP đang là bản có tổng hợp **120 Đạt; 67 đã test nhưng thiếu kết quả riêng**. Chưa áp dụng lần sửa **187 Đạt** theo xác nhận mới. Claude cần thực hiện việc sửa Word và đóng gói ZIP theo hướng dẫn này.
