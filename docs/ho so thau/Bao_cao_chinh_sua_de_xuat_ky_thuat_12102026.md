# Báo cáo chỉnh sửa đề xuất kỹ thuật ngày 12/10/2026

## Kết quả

- Bản khách đã nhận: `De_xuat_ky_thuat_09102026.docx`.
- Căn cứ góp ý: `Gop_y_hoan_thien_de_xuat_ky_thuat_MobiFone.docx`.
- Bản Claude sửa được dùng làm đầu vào: `De_xuat_ky_thuat_Goi4_12102026.docx`.
- Bản chỉnh sửa mới: `De_xuat_ky_thuat_Goi4_12102026_v2.docx`.
- Bản đầu vào được giữ nguyên. Không sửa mã nguồn hoặc xác nhận chức năng thực tế qua việc sửa tài liệu.

Bản v2 đã bổ sung giải pháp cho các điểm còn thiếu và thống nhất các tuyên bố về hiện trạng. Chưa chốt thành hồ sơ nộp thầu: nguồn cung, phiên bản/SKU/phạm vi cấp phép, bộ phụ lục, kiểm thử chức năng và mục tiêu tài nguyên còn cần đội thực hiện xác nhận. Nội dung “Đáp ứng” là cam kết cung cấp, không phải chứng nhận tất cả chức năng đã chạy trên môi trường thật.

## Bảng phản hồi 22 góp ý

| STT | Vị trí trong bản v2 | Nội dung xử lý | Việc còn phải xác nhận và đầu mối |
| --- | --- | --- | --- |
| 1 | Mục 2, 3.1; PL-01/02 | Ghi Windows Server Standard cho 02 máy chủ, SQL Server Standard cho CSDL; quyền sử dụng đủ 05 năm, phạm vi cấp phép và nâng cấp khi hết hỗ trợ | MobiFone/hạ tầng chốt phiên bản, SKU, mô hình và số giấy phép; đính kèm xác nhận hợp pháp |
| 2 | Mục 1.4, 2 | Đặt mã PL-00 đến PL-08, tên file, vị trí và thời điểm cung cấp; phân biệt tài liệu HSDT với chứng từ kích hoạt | Chủ nhiệm dự án hoàn thiện phụ lục; tên file là quy ước dự kiến, chưa phải tài liệu đã được tạo |
| 3 | Mục 3.1, 6.1 | Tách kho sao lưu khỏi ứng dụng/CSDL/dữ liệu gốc; lưu tại Việt Nam, phân quyền riêng, mã hóa, giữ ít nhất 30 ngày; công thức dung lượng và dự phòng; chi phí trong giá chào | Hạ tầng chốt mức GB, vị trí, gói lưu trữ, bảo vệ quyền xóa và chi phí tại PL-01 |
| 4 | Mục 6.1 | Giữ sửa của Claude: RPO CSDL 6 giờ, tệp 24 giờ; bổ sung kiểm tra nhất quán CSDL/tệp và đo RTO 8 giờ | Hạ tầng kiểm thử khôi phục và dung lượng thực tế |
| 5 | Mục 3.1; PL-06 | Giữ bảng 14 bước, vai trò, trạng thái và chứng từ của Claude; bỏ khẳng định đã chạy trọn vòng khi chưa có biên bản | Kỹ thuật/kiểm thử xác nhận quyền, trạng thái, NDA, nhật ký, thông báo từng bước |
| 6 | Mục 5.2, 9 | Thay SLA chung theo nhà cung cấp bằng SLA tổng thể của Nhà thầu; đối tác không được loại khỏi trách nhiệm; giữ đầu mối Claude bổ sung | Tích hợp/MobiFone chốt SLA riêng, phạm vi đo, tài liệu và đầu mối đối tác tại PL-01 |
| 7 | Mục 1.3, C.2, 5.3 | Thống nhất kết nối quốc gia là phạm vi triển khai cần kiểm chứng; gian hàng địa phương, chào bán/chào mua, mã định danh, ánh xạ, gửi lại/đối soát | Tích hợp lấy tài liệu/API/quyền truy cập; PL-06 chứng minh hiện trạng và kiểm thử thật |
| 8 | Mục 7, 12 | Giữ sửa Claude về quyền quản lý, sử dụng và sở hữu dữ liệu/tài liệu/cấu hình/kết quả hợp đồng | Chủ nhiệm dự án đối chiếu điều kiện hợp đồng; không suy diễn thành bàn giao toàn bộ mã nguồn |
| 9 | Mục 7.2 | Giữ hoàn trả dịch vụ chưa thực hiện và không khóa dữ liệu sau chấm dứt, ngoại lệ đã nêu | Chủ nhiệm dự án đối chiếu chính xác hợp đồng và nghĩa vụ đối trừ |
| 10 | Mục 1.4, 3.1, 6.1; PL-01 | Đặt nơi chốt nguồn cung; mô tả WAF, giám sát, chống mã độc, phối hợp chống DDoS, tài nguyên/chi phí bao gồm | MobiFone chốt nhà cung cấp/gói cloud, email, SMS, AI, họp, CA, hợp đồng điện tử, sản phẩm bảo vệ; chưa tự chọn đối tác |
| 11 | A.4.4 | Thêm nhấp banner, đường dẫn/trang sự kiện, hành động chia sẻ, tách chỉ số khác; không coi nhấp chia sẻ là chia sẻ thành công ngoài Sàn | Phần mềm/kiểm thử bổ sung hoặc xác nhận tracking; PL-06 |
| 12 | A.5.2 | Thêm mã đơn, ngày đặt, sản phẩm, nhà cung cấp, giá trị, thanh toán, giao hàng/hoàn thành, tổng chi; tách giỏ hàng, hủy/hoàn tiền | Phần mềm/kiểm thử xác nhận nguồn đơn và khoản thanh toán; PL-06 |
| 13 | B.2.3 | Thêm lượt giao dịch, danh sách đơn/hồ sơ, trạng thái, lọc theo thời gian, chống đếm trùng | Phần mềm/kiểm thử xác nhận công thức và dữ liệu; PL-06 |
| 14 | A.8.5, 6.1 | Thêm nhật ký tìm kiếm, quyền xem/xuất, bảo vệ khỏi sửa/xóa; đề xuất lưu trong 05 năm thuê; phân biệt với sao lưu 30 ngày | ATTT/hạ tầng xác nhận chính sách dữ liệu, dung lượng và triển khai; mốc 05 năm là đề xuất, không được mô tả là ngưỡng HSMT bắt buộc |
| 15 | Mục 3.1, A.6.1, A.8.1 | Thêm PDF/Excel, biểu đồ/dashboard, nguồn, kỳ báo cáo, chỉ tiêu và quyền xem | Phần mềm/kiểm thử xác nhận xuất PDF, biểu đồ và đối soát; PL-06 |
| 16 | Mục 3.1 | Bổ sung cấu trúc và thao tác hồ sơ viện/trường, chuyên gia, tư vấn viên, nhà đầu tư, dịch vụ tư vấn; kiểm soát công bố | Nghiệp vụ/phần mềm xác nhận phạm vi HSMT và chức năng thực tế |
| 17 | Mục 10.3 | Giữ kế hoạch đào tạo bổ sung Claude viết | Chủ nhiệm/đào tạo xác nhận nguồn lực và các thời hạn đề xuất |
| 18 | Mục 1.3, 3.1, 4, C.2, 5.3 | Bỏ số 4.300/1.808 chưa có kiểm kê; bỏ tuyên bố đã vận hành kết nối/chạy trọn vòng; yêu cầu nguồn và quyền dữ liệu tại PL-07 | Chủ nhiệm/dữ liệu cung cấp kiểm kê và quyền sử dụng; kỹ thuật bổ sung PL-06 |
| 19 | Mục 3.2, 8 | Giữ số 06 người và nhóm VR360 đã thống nhất | Chủ nhiệm xác nhận danh sách, phân công và năng lực |
| 20 | Mục 11.4 | Giữ loại trừ chỉ bảo trì kế hoạch đã thông báo/thống nhất, bỏ bất khả kháng tự thêm | Đối chiếu lần cuối HSMT và hợp đồng |
| 21 | Mục 3.3 | Đề xuất 200 người đồng thời, p95 ≤ 3 giây cho tra cứu thường, lỗi < 1%, 30 phút; dữ liệu thử 10.000 sản phẩm/2.000 chuyên gia/5.000 giao dịch; đo riêng dịch vụ ngoài và tăng tài nguyên khi chưa đạt | Kỹ thuật/hạ tầng phê duyệt trước nộp; các ngưỡng là đề xuất chưa được đo, không phải yêu cầu HSMT hay năng lực đã chứng minh |
| 22 | Mục 8 | Giữ bảng phụ thuộc và mốc 20/30 ngày; nêu rõ mô phỏng/brand thử/Token không thay nghiệm thu thật, không tự gia hạn hoặc thu hẹp phạm vi | Chủ nhiệm/tích hợp chuẩn bị sớm, xác nhận tài khoản thật và điều kiện sẵn sàng |

## Các việc cần chốt trước khi gửi khách

1. MobiFone điền PL-01: nhà cung cấp, sản phẩm/gói, cấu hình, kho sao lưu, SLA riêng, hạn mức và chi phí. Bản v2 không bịa nguồn cung đã mua.
2. Hoàn thiện PL-02: phiên bản, SKU, mô hình cấp phép, số lượng và quyền sử dụng đủ 05 năm cho Windows Server Standard/SQL Server Standard.
3. Kỹ thuật xác nhận các mô tả bổ sung là khả thi, chỉ rõ chức năng có sẵn/chưa có/cần cấu hình; thực hiện PL-06. Việc sửa Word không sửa phần mềm.
4. Xác nhận bộ kết nối quốc gia và tài liệu API chính thức; thông tin xác thực, gửi theo lô, chống trùng từng được Claude mô tả chưa được coi là đã chứng minh.
5. Chốt thời hạn nhật ký, mục tiêu tải và dung lượng sao lưu bằng đánh giá kỹ thuật. Không dùng số đề xuất làm kết quả kiểm thử.
6. Tạo/đính kèm các phụ lục thực tế đúng mã/tên và chuyển bảng phản hồi này thành PL-08 nếu bộ hồ sơ yêu cầu PDF. Chưa có PL-01 đến PL-08 chỉ vì đã ghi tên trong đề xuất.
7. Rà soát Chương III/IV/V và điều kiện hợp đồng phiên bản chính thức. Các căn cứ pháp luật đang có được giữ từ bản đầu vào, chưa được xác minh độc lập ở lượt chỉnh sửa này.
8. Không nộp bản còn dấu `[CẦN XÁC NHẬN]`; chỉ xóa dấu sau khi có xác nhận và chứng từ phù hợp.

## Kiểm tra đã thực hiện

- Tạo v2 từ bản Claude bằng python-docx, bảo toàn bố cục cơ bản, các mục và 22 bảng; không ghi đè bản gốc.
- Đọc lại DOCX đã lưu, kiểm tra các nội dung bổ sung và loại bỏ câu sai về kho sao lưu cùng CSDL, số hồ sơ chưa chứng minh, tuyên bố vận hành kết nối và SLA nhà mạng chung chung.
- Bật lặp hàng tiêu đề của bảng và thống nhất cỡ chữ bảng 10 pt để giảm nguy cơ bảng dài khó đọc.
- Chưa xác nhận bố cục bằng ảnh render: bộ renderer yêu cầu LibreOffice, môi trường chưa tìm thấy `soffice.exe`. Cần kiểm tra trang, bảng, ngắt trang trong Word trước khi phát hành; không coi kiểm tra cấu trúc là kiểm tra hiển thị.
- Chưa chạy kiểm thử ứng dụng, tải, khôi phục hoặc tích hợp thật; không có kết luận đạt toàn bộ HSMT từ lượt sửa tài liệu.
