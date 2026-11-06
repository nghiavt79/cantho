# Các nội dung đề nghị MobiFone xác nhận để hoàn thiện đề xuất kỹ thuật

Kính gửi Bộ phận kỹ thuật và đầu mối dự án MobiFone Cần Thơ,

Để hoàn thiện bản đề xuất kỹ thuật Gói thầu số 4 sau góp ý của khách, đề nghị xác nhận các nội dung dưới đây, kèm thông tin và tài liệu chứng minh tương ứng. Bản đang rà soát là `De_xuat_ky_thuat_Goi4_12102026_v2.docx`.

Với mỗi nội dung, vui lòng trả lời: thông tin được xác nhận; phần cần điều chỉnh; tài liệu/file minh chứng; người phụ trách và thời điểm hoàn thành. Nếu chức năng chưa có, đề nghị ghi rõ phần cần bổ sung và kế hoạch thực hiện trước mốc vận hành thử/nghiệm thu.

## 1. Nguồn cung và bản quyền

**Đầu mối đề nghị: MobiFone, kỹ sư hạ tầng và kỹ sư tích hợp.**

1. Cloud sử dụng nhà cung cấp và gói nào? Đề nghị cung cấp vị trí trung tâm dữ liệu tại Việt Nam, cấu hình từng máy chủ, CPU tương đương yêu cầu, IP, hỗ trợ IPv6, đường truyền, khả năng mở rộng và tài liệu xác nhận nguồn cung.
2. Windows Server Standard và SQL Server Standard sử dụng phiên bản, SKU và mô hình cấp phép nào? Số lượng giấy phép/quyền sử dụng có phù hợp 02 máy chủ, số lõi và đối tượng truy cập không? Tài liệu nào chứng minh quyền sử dụng hợp pháp đủ 05 năm và phương án khi phiên bản hết hỗ trợ?
3. Đề nghị điền thông tin cho từng dịch vụ trong bảng sau; nêu rõ dịch vụ tự cung cấp hoặc mua ngoài và chi phí đã nằm trong giá chào hay chưa.

| Dịch vụ | Nhà cung cấp, sản phẩm/gói cần xác nhận | Quy mô cần đối chiếu | Tài liệu/đầu mối cần cung cấp |
| --- | --- | --- | --- |
| Email | Nhà cung cấp, gói, dung lượng áp dụng chung hay từng hộp thư | 60 địa chỉ, 25 GB, 05 Mail List; 05 năm | Xác nhận dịch vụ, SLA, SMTP, chống thư rác |
| SMS Brandname | Nhà cung cấp, gói, thương hiệu đăng ký | 3.600 tin/năm; 05 năm | Quy trình đăng ký, API, trạng thái gửi, SLA |
| Chữ ký số | Tổ chức chứng thực, dịch vụ ký từ xa, hỗ trợ USB Token | Phạm vi người ký/chứng thư; 05 năm | Xác nhận quyền sử dụng, API và phương án kiểm thử thật |
| Hợp đồng điện tử | Mô-đun tự cung cấp hoặc sản phẩm mua ngoài | 2.000 hợp đồng/năm; 05 năm | Phạm vi hạn mức, lưu trữ, ký nhiều bên, SLA |
| AI | Nhà cung cấp, mô hình/gói hoặc phương án tự vận hành | Hạn mức truy vấn, tài nguyên, dữ liệu gửi ra ngoài | Chính sách dữ liệu, chi phí, SLA và phương án dự phòng |
| Họp trực tuyến/video | Máy chủ riêng hoặc dịch vụ cụ thể | Số phòng, người tham gia, tài nguyên | Cấu hình, quyền sử dụng, SLA, phương án bảo mật |
| VR360 | Đơn vị thực hiện, công cụ và quyền sử dụng nội dung | Chụp/dựng mới 10 điểm | Lịch thực hiện, quyền bàn giao và minh chứng mẫu |

## 2. Sao lưu và an toàn thông tin

**Đầu mối đề nghị: Kỹ sư hạ tầng và ATTT.**

4. Kho sao lưu độc lập với máy chủ ứng dụng/CSDL và dữ liệu gốc sẽ dùng sản phẩm nào, đặt tại đâu ở Việt Nam? Dung lượng bao nhiêu GB, cách tính giữ ít nhất 30 ngày, phân quyền, mã hóa và hạn chế quyền xóa thế nào? Chi phí kho và lưu lượng đã bao gồm chưa?
5. Có xác nhận thực hiện được RPO CSDL tối đa 6 giờ, tệp tối đa 24 giờ và RTO tối đa 8 giờ không? Đề nghị cung cấp phương pháp sao lưu, cách khôi phục CSDL/tệp nhất quán và biên bản phục hồi thử; nếu chưa có kết quả, ghi lịch kiểm thử.
6. WAF, chống mã độc, giám sát và phối hợp chống DDoS sử dụng sản phẩm/gói nào? Đề nghị nêu hạn mức, tài nguyên, người xử lý cảnh báo và chi phí. Phần nào cần cấu hình/bổ sung trước vận hành thử?
7. Có xác nhận phương án lưu nhật ký trong suốt 05 năm thuê không? Đây là thời hạn đề xuất trong v2, chưa phải thời hạn đã chứng minh hoặc ngưỡng bắt buộc của HSMT. Đề nghị xác nhận dung lượng, quyền xem/xuất, bảo vệ khỏi sửa/xóa, xử lý dữ liệu nhạy cảm và bàn giao. Nếu cần thay đổi thời hạn, cung cấp phương án và căn cứ.
8. Ai chịu trách nhiệm bật MFA, hoàn thiện chính sách bảo vệ dữ liệu cá nhân, kiểm thử ATTT và lập hồ sơ cấp độ 2? Đề nghị xác nhận mốc hoàn thành và tài liệu bàn giao.

## 3. SLA và hiệu năng

**Đầu mối đề nghị: Cán bộ phụ trách dịch vụ, kỹ sư hạ tầng và tích hợp.**

9. MobiFone có xác nhận chịu SLA tổng thể kể cả dịch vụ mua ngoài: sẵn sàng ≥ 99,5%/tháng, phản hồi ≤ 02 giờ theo phạm vi Mục 9, khắc phục nghiêm trọng ≤ 08 giờ và thông thường ≤ 24 giờ không? Đề nghị cung cấp SLA riêng của từng đối tác, phạm vi đo, đầu mối và cơ chế xử lý khi đối tác chậm khắc phục.
10. Có thống nhất chỉ loại trừ thời gian bảo trì kế hoạch đã thông báo và được Bên thuê thống nhất khi tính SLA không? Đề nghị đối chiếu bản HSMT/điều kiện hợp đồng chính thức và ghi rõ điều khoản nếu cần chỉnh.
11. Kỹ thuật có chấp thuận mục tiêu kiểm thử đề xuất: 200 người đồng thời, p95 ≤ 3 giây cho tra cứu/danh sách thông thường, lỗi < 1% trong 30 phút tải ổn định không? Dữ liệu thử đề xuất là 10.000 sản phẩm, 2.000 chuyên gia, 5.000 hồ sơ giao dịch. Các con số này chưa được kiểm thử và không phải số liệu thực tế. Đề nghị xác nhận hoặc thay bằng ngưỡng khả thi, cấu hình tương ứng, kịch bản đo riêng dịch vụ ngoài và phương án tăng tài nguyên trong giá chào.

## 4. Chức năng và hiện trạng dữ liệu

**Đầu mối đề nghị: Đội phần mềm, nghiệp vụ và kiểm thử.**

12. Đề nghị đánh dấu từng chức năng dưới đây: đã có và đã kiểm thử / đã có nhưng cần cấu hình hoặc kiểm thử / cần phát triển. Cung cấp ảnh, biên bản hoặc lịch hoàn thành; không xác nhận chỉ dựa trên nội dung mô tả trong Word.

| Chức năng | Nội dung cần xác nhận | Minh chứng đề nghị |
| --- | --- | --- |
| Quy trình 14 bước | Vai trò, trạng thái, NDA, chứng từ, nhật ký, thông báo và kịch bản trọn vòng | Biên bản kiểm thử từng bước, ảnh/video và phiên bản hệ thống |
| A.4.4 | Nhấp banner/đường dẫn sự kiện, hành động chia sẻ; phân biệt xem, quan tâm, đăng ký, điểm danh | Cơ chế ghi nhận và báo cáo đối soát |
| A.5.2 | Mã đơn, ngày đặt, sản phẩm, nhà cung cấp, giá trị, thanh toán, giao hàng/hoàn thành, tổng chi | Màn hình, dữ liệu mẫu và công thức tổng chi/hủy/hoàn tiền |
| B.2.3 | Lượt giao dịch, danh sách liên quan, lọc thời gian và chống đếm trùng | Công thức đếm, ảnh và kết quả kiểm thử |
| A.8.5 | Nhật ký tìm kiếm cùng nhật ký thao tác; quyền truy cập/xuất | Mẫu nhật ký, cấu hình lưu và kiểm thử quyền |
| Báo cáo | Excel/PDF, biểu đồ/dashboard, nguồn, kỳ, chỉ tiêu và phân quyền | File xuất mẫu và đối soát số liệu |
| Tổ chức/chuyên gia | Hồ sơ viện/trường, chuyên gia, tư vấn viên, nhà đầu tư, dịch vụ tư vấn; duyệt và công bố | Danh mục trường, thao tác và ảnh chức năng |

13. Các số 4.300 hồ sơ công nghệ/thiết bị và 1.808 hồ sơ chuyên gia trước đây lấy từ nguồn nào, tại thời điểm nào? Có biên bản kiểm kê và quyền sử dụng/công bố cho Sàn Cần Thơ không? Nếu chưa có, đề nghị thống nhất không đưa các số này vào bản chốt.
14. SMS, ký số từ xa/USB Token, nhúng và kiểm tra chữ ký PDF, VR360 và IPv6 đã được kiểm thử trên môi trường thật đến đâu? Đề nghị tách rõ mô phỏng/tài khoản thử nghiệm với vận hành thật và nêu phần còn phải hoàn thiện.

## 5. Kết nối Sàn quốc gia và tiến độ

**Đầu mối đề nghị: Chủ nhiệm dự án, kỹ sư tích hợp và đầu mối Bên thuê.**

15. Có bộ kết nối quốc gia đã vận hành thực tế không? Đề nghị cung cấp hệ thống/phiên bản, đơn vị vận hành và bằng chứng được phép sử dụng. Xác nhận riêng xác thực, đẩy sản phẩm, tin chào mua, đăng ký gian hàng địa phương, mã định danh, ánh xạ lĩnh vực, nhật ký, gửi lại và đối soát; không dùng bằng chứng một chức năng để xác nhận toàn bộ kết nối.
16. Ai cung cấp tài liệu API, tài khoản/quyền truy cập và danh mục lĩnh vực chính thức? Đã có các tài liệu này chưa? Đề nghị xác nhận định dạng, endpoint, tần suất/hạn mức, đầu mối và thời điểm dự kiến được cấp.
17. Có bảo đảm mốc vận hành thử ngày 20 và nghiệm thu trong 30 ngày với các phụ thuộc API, SMS Brandname, chứng thư và tên miền không? Đề nghị nêu kế hoạch chuẩn bị sớm, ngày cần nhận thông tin và cách xử lý vướng mắc. Kiểm thử mô phỏng/brand thử không thay nghiệm thu dịch vụ thật; không tự gia hạn hoặc giảm phạm vi.

## 6. Nhân sự, đào tạo, quyền dữ liệu và phụ lục

**Đầu mối đề nghị: Chủ nhiệm dự án và bộ phận phụ trách hồ sơ/hợp đồng.**

18. Có xác nhận Tổ triển khai 06 người và nhóm VR360 không? Đề nghị cung cấp danh sách, vai trò, năng lực, khả năng bố trí theo tiến độ và đầu mối vận hành/hỗ trợ sau nghiệm thu.
19. Có xác nhận kế hoạch đào tạo bổ sung không tính thêm chi phí trong phạm vi thuê tại Mục 10.3 không? Đặc biệt: hướng dẫn trước/ngay khi cập nhật, đào tạo quy trình trong 05 ngày làm việc, đào tạo lại theo yêu cầu tối đa 10 ngày làm việc. Nếu cần chỉnh thời hạn, đề nghị nêu rõ.
20. Có thống nhất quyền quản lý, sử dụng và sở hữu của Bên thuê đối với dữ liệu, tài liệu, cấu hình và kết quả hình thành theo hợp đồng; phương án bàn giao, không khóa dữ liệu và hoàn trả dịch vụ chưa thực hiện không? Đề nghị xác nhận theo điều kiện hợp đồng chính thức, kể cả ngoại lệ/đối trừ và quyền của bên thứ ba; nêu riêng nếu hợp đồng yêu cầu mã nguồn.
21. Đề nghị xác nhận phiên bản HSMT Chương III/IV/V và điều kiện hợp đồng được dùng để chốt hồ sơ; cung cấp file chính thức. Bộ phận hồ sơ kiểm tra các căn cứ pháp luật trong đề xuất còn phù hợp trước khi phát hành.
22. Ai cung cấp từng phụ lục PL-01 đến PL-08, file thực tế tên gì và khi nào hoàn thành? Đề nghị điền bảng dưới đây; tên file trong v2 chỉ là quy ước dự kiến.

| Phụ lục | Nội dung | Người phụ trách | Tên file thực tế | Ngày hoàn thành |
| --- | --- | --- | --- | --- |
| PL-01 | Nguồn cung, cấu hình, SLA, kho sao lưu và các gói dịch vụ | Chưa xác nhận | Chưa cung cấp | Chưa xác nhận |
| PL-02 | Bản quyền và quyền sử dụng hợp pháp | Chưa xác nhận | Chưa cung cấp | Chưa xác nhận |
| PL-03 | Đặc tả API/OpenAPI | Chưa xác nhận | Chưa cung cấp | Chưa xác nhận |
| PL-04 | Đặc tả CSDL | Chưa xác nhận | Chưa cung cấp | Chưa xác nhận |
| PL-05 | Ảnh chức năng và hướng dẫn | Chưa xác nhận | Chưa cung cấp | Chưa xác nhận |
| PL-06 | Biên bản kiểm thử và tích hợp | Chưa xác nhận | Chưa cung cấp | Chưa xác nhận |
| PL-07 | Kiểm kê và quyền sử dụng dữ liệu | Chưa xác nhận | Chưa cung cấp | Chưa xác nhận |
| PL-08 | Phản hồi 22 góp ý của khách | Chưa xác nhận | Chưa cung cấp | Chưa xác nhận |

## Mẫu phản hồi

| Số câu hỏi | Nội dung xác nhận hoặc điều chỉnh | File minh chứng | Người xác nhận | Ngày hoàn thành phần còn thiếu |
| --- | --- | --- | --- | --- |
| Điền số câu | Điền câu trả lời cụ thể | Điền tên file/vị trí | Điền tên và vai trò | Điền ngày hoặc ghi đã hoàn thành |

Sau khi nhận phản hồi, các thông tin được xác nhận sẽ được cập nhật vào bản đề xuất và phụ lục tương ứng trước khi chốt hồ sơ.
