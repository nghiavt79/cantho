Chào Khôi, N đã xem bộ API tạo hợp đồng đơn lẻ không theo mẫu và bộ API ký USB Token do Khôi cung cấp. N dự kiến tích hợp bộ API này để thay phần hợp đồng và ký điện tử hiện có của Sàn Cần Thơ. Nhờ Khôi xác nhận giúp một số điểm để triển khai đúng:

1. API và môi trường: các API lấy hợp đồng, tài liệu, vùng ký, chuẩn bị ký (create-empty-token), ghép chữ ký (merge-time-stamp), lưu PDF và xác nhận (approval) có được hỗ trợ chính thức cho hệ thống Sàn gọi không? Nhờ Khôi cung cấp base URL API Production và xác nhận tài khoản của Trung tâm dùng được luồng legacy theo bộ tài liệu đã gửi.

2. Xác thực server: trường hợp đăng nhập web qua SSO MobiFone thì server Sàn lấy token gọi API bằng cách nào? API /api/v1/auth có hỗ trợ email/mật khẩu cho tài khoản này không, hay cần cấp tài khoản tích hợp riêng?

3. Phiên ký của từng người: bên Mua và bên Bán có cần tài khoản eContract riêng không? Khôi hỗ trợ SSO hoặc cơ chế cấp phiên ký cho từng người ký từ Sàn như thế nào? N muốn người dùng thực hiện ký USB Token ngay trên giao diện Sàn.

4. Xác minh chữ ký: eContract có kiểm tra tính toàn vẹn của PDF, chuỗi tin cậy, tình trạng thu hồi chứng thư và dấu thời gian không? Có API hoặc kết quả xác minh kèm thông tin chứng thư, thời điểm ký của từng bên để Sàn lưu và đối soát không?

5. Plugin: bộ cài Mobifone PKI Sign 1.0.0.0 tương ứng nhánh plugin v1 hay v2 trong tài liệu? Nhờ Khôi xác nhận phiên bản áp dụng cho tài khoản của Trung tâm, cung cấp bộ cài được hỗ trợ hiện tại và hướng dẫn cấu hình để gọi plugin từ tên miền Sàn Cần Thơ.

6. Vị trí ô ký: tọa độ dùng đơn vị gì, gốc tọa độ ở đâu và số trang bắt đầu từ 0 hay 1? Trong mẫu có page=3 nhưng coordinate_y=2360, vậy tọa độ y tính riêng từng trang hay cộng dồn qua các trang?

7. Khôi phục khi lỗi: nếu API tạo hợp đồng timeout thì tra cứu lại bằng thông tin nào để tránh tạo trùng? Nếu đã lưu PDF nhưng approval lỗi hoặc timeout thì kiểm tra và tiếp tục thế nào? Khi cần sửa hợp đồng đã phát hành, N thực hiện hủy/thu hồi và tạo lại theo API hoặc quy trình nào?

8. Cấu hình và đồng bộ: nhờ Khôi xác nhận Loại tài liệu và giá trị type_id sử dụng cho tài khoản Trung tâm; type_id=538 trong mẫu có áp dụng được cho tài khoản này không? Đồng thời xác nhận hệ thống có webhook báo hoàn thành ký không; nếu có, nhờ Khôi cung cấp tài liệu tích hợp.

Về thử nghiệm, N muốn test trực tiếp trên Production bằng tài khoản của Trung tâm, tạo hai người dùng thử và dùng USB Token sẵn có để kiểm tra toàn bộ luồng tạo hợp đồng, phát hành, ký và tải PDF đã ký. Nhờ Khôi xác nhận có thể thử theo cách này không, và hướng dẫn cách đánh dấu hợp đồng thử nghiệm nếu cần. N cũng cần thử trường hợp bên Mua và bên Bán thuộc hai tổ chức khác nhau, nhờ Khôi hướng dẫn cách thiết lập.

Cảm ơn Khôi.


