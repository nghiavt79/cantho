# Kết quả kiểm tra cuối bộ dự thảo gửi khách ngày 28 tháng 10 năm 2026

**Kết luận: bộ dự thảo đã xử lý các lỗi nội dung được ghi nhận qua các lần rà soát; có thể gửi khách để phối hợp góp ý và hoàn thiện.** Trong phạm vi kiểm tra lần cuối, không còn lỗi nội dung đã nêu cần chuyển Claude sửa tiếp. Đây là bộ dự thảo, chưa phải bộ hồ sơ đã ký/nghiệm thu hoàn chỉnh.

Bản này thay thế kết luận và danh sách lỗi trong các bản rà soát trước. Đã đọc trực tiếp TL10 mới và kiểm tra lại ZIP NCC-02 sau khi Claude đóng gói; không sửa các tệp Word, Excel hoặc ZIP.

## Kết quả kiểm tra

| Hạng mục | Kết quả |
|---|---|
| TL10 phần chi tiết API-18 | Đã ghi rõ chưa kiểm thử ở mốc 25/10 và kết quả kiểm tra lại ngày 28/10, dẫn MC-KT-38. Thiếu mã chống giả mạo trả 400; có mã trả 200 với marked = 0. Chỉ xác nhận lớp chống giả mạo; việc thay đổi trạng thái thông báo chưa đọc vẫn theo L04 |
| TL10 mã kết quả API-18 | Đã cập nhật 200, 400 khi thiếu mã, 401 khi chưa đăng nhập từ bản cập nhật 26/10; không còn ghi 302 như trạng thái hiện hành |
| TL10 Bảng 10 dòng 2 và 3 | Đã cập nhật kết quả kiểm tra lại, minh chứng, ngày hoàn thành của L02 và chuẩn hóa JSON/401. Các việc còn mở không bị chuyển đồng loạt sang hoàn thành |
| ZIP NCC-02 | Có 41 tệp; từng tệp khớp byte với thư mục NCC-02 hiện tại, gồm TL10 vừa sửa. Không thiếu tệp của thư mục này, không có thành viên lệch phiên bản |
| Sao lưu | NCC-02, TL01 và GS-02 đã làm rõ thực tế sao lưu, không sử dụng snapshot. TL01 tách nội dung dẫn Phụ lục hợp đồng với nội dung thực hiện. Không yêu cầu triển khai snapshot |
| Phần đo Chrome | NCC-02/TL01/NCC-06 dùng số liệu đáp ứng HTTP; TL04 và TC-HT-01 đã đồng bộ, không còn nhắc thiếu 2 khung giờ. Không yêu cầu đo Chrome |
| VHT-04 hạn khắc phục | Đã đọc đoạn cuối mục 2: hạn trong bảng là đề xuất theo tiến độ để các bên thống nhất khi ký; lỗi phát sinh khi VHT ghi giờ tiếp nhận và áp dụng 8 giờ/24 giờ từ khi tiếp nhận. Không còn nhận xét yêu cầu sửa phần này |
| Các điểm đã sửa trước | Giữ phụ lục NCC-06 có 4 bên ký, phân tách kỳ chuẩn bị/VHT, cập nhật L02 đủ 5 API trong phạm vi kiểm tra lớp chống giả mạo |

Không coi các tên bảng/trường DashboardSnapshot, SnapshotJson, HtmlSnapshot là snapshot hạ tầng. Không coi tên Chrome trong HDSD hoặc công cụ kiểm thử API là yêu cầu đo hiệu năng Chrome.

## Thành phần gửi khách

Có thể gửi các tệp rời theo danh mục; không bắt buộc tạo một ZIP tổng:

1. ZIP NCC-02 bản cuối, chứa văn bản chính, 11 tài liệu và bộ bổ trợ/minh chứng.
2. NCC-03, VHT-04, NCC-05, NCC-06 và thư mục NCC-06 Minh chung.
3. GS-02, NCC-08 nếu thuộc phạm vi xin góp ý đợt này.
4. Checklist V8 nếu cần phối hợp tiến độ; giữ E22 là nguyên văn chị Vân theo xác nhận của anh.

Ghi rõ trong lời gửi hoặc tên gói: **Bộ dự thảo để phối hợp rà soát, bổ sung số liệu và xác nhận các bên**. Các ô …… chờ số liệu thật, người phê duyệt hoặc chữ ký được giữ nguyên theo file Ghi chú; không tính là lỗi soạn thảo của Claude.

Không đưa file Ghi chu - noi dung MobiFone can dien.txt, các bản rà soát, script và bộ nguồn cũ vào danh mục gửi khách. Giữ chúng làm tài liệu nội bộ, tránh gửi cả thư mục gốc mà không chọn tệp.

NCC-07 vẫn chưa có: không cản việc gửi bộ dự thảo theo yêu cầu làm lại hiện tại; cần bổ sung khi tập hợp đầy đủ hồ sơ nghiệm thu theo checklist. Việc khách góp ý bản dự thảo không thay thế các xác nhận hoặc số liệu còn phải hoàn thiện trước phát hành chính thức.

Giới hạn: đã kiểm tra nội dung liên quan và sự khớp ZIP/thư mục; không chạy lại kiểm thử máy chủ. Chưa xác nhận bố cục in từng trang Word do môi trường kết xuất thiếu LibreOffice. Kết luận có thể gửi áp dụng cho mục đích gửi dự thảo xin góp ý trong phạm vi kiểm tra này.
