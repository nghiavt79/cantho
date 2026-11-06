# NỘI DUNG TIẾP THU – GIẢI TRÌNH

**Hoạt động:** Kế hoạch thuê dịch vụ công nghệ số – Sàn giao dịch công nghệ thành phố Cần Thơ
**Phạm vi soạn:** Phần I – mục 7, 8, 9 và Phần II – mục 7, 8, 9
**Căn cứ kỹ thuật:** Hồ sơ thiết kế chi tiết PHẦN 7 (VTISC V1.2)

> Thuật ngữ dùng thống nhất theo góp ý II.1: **Chủ trì thuê**, **kế hoạch thuê**, **dự toán thuê**, **chi phí thuê dịch vụ công nghệ số**.
> Ô `[ ... ]` là số liệu cần Chủ trì thuê chốt trước khi trình.

---

# PHẦN I – THÀNH PHẦN HỒ SƠ VÀ TÍNH THỐNG NHẤT

## Mục 7

**Nội dung góp ý:** Ý kiến/thẩm định của đơn vị có thẩm quyền đối với chất lượng dịch vụ, Khung kiến trúc số, kết nối và chia sẻ dữ liệu theo điểm b, c khoản 2 Điều 67.

**Tiếp thu/Giải trình:** Tiếp thu

**Nội dung tiếp thu/giải trình:**

Chủ trì thuê sẽ có văn bản đề nghị đơn vị chuyên trách về công nghệ thông tin/chuyển đổi số thành phố cho ý kiến trước khi phê duyệt kế hoạch thuê. Đơn vị triển khai chịu trách nhiệm chuẩn bị đầy đủ hồ sơ kỹ thuật kèm theo, gồm ba nhóm nội dung:

**a) Chất lượng dịch vụ (điểm b):** thuyết minh phạm vi chức năng, mức độ đáp ứng yêu cầu nghiệp vụ và các chỉ tiêu chất lượng dịch vụ cam kết trong suốt 05 năm thuê — tính sẵn sàng của hệ thống, thời gian phản hồi, khả năng phục vụ đồng thời, thời gian khắc phục sự cố, phạm vi hỗ trợ kỹ thuật; kèm phương án kiểm thử, nghiệm thu và cơ chế đo lường, báo cáo định kỳ (Mục V và Mục VIII hồ sơ thiết kế chi tiết).

**b) Sự phù hợp với Khung kiến trúc số:** hệ thống được thiết kế bám theo Khung Kiến trúc Chính phủ điện tử/Chính phủ số Việt Nam và Khung kiến trúc số thành phố Cần Thơ, phân lớp đầy đủ: người dùng/kênh truy cập – ứng dụng – dữ liệu – tích hợp – bảo mật – vận hành (Mục III hồ sơ thiết kế chi tiết).

**c) Kết nối và chia sẻ dữ liệu (điểm c):** hệ thống cung cấp giao diện kết nối theo chuẩn HTTP/REST, dữ liệu định dạng mở (JSON/XML/CSV), sẵn sàng kết nối với nền tảng tích hợp – chia sẻ dữ liệu dùng chung của thành phố (LGSP) và các hệ thống thông tin liên quan khi được cấp có thẩm quyền cho phép. Danh mục dữ liệu chia sẻ, tần suất đồng bộ, cơ chế xác thực và phân quyền truy cập sẽ được thống nhất bằng văn bản với đơn vị quản lý nền tảng dùng chung.

**Hồ sơ gửi lấy ý kiến gồm:** thuyết minh tuân thủ khung kiến trúc; danh mục quy chuẩn, tiêu chuẩn kỹ thuật áp dụng; sơ đồ kiến trúc logic và kiến trúc triển khai vật lý; mô hình dữ liệu; danh mục API và phương án kết nối – chia sẻ dữ liệu; bảng cam kết chỉ tiêu chất lượng dịch vụ. Văn bản ý kiến của đơn vị có thẩm quyền được bổ sung vào hồ sơ kế hoạch thuê trước khi trình phê duyệt.

---

## Mục 8

**Nội dung góp ý:** Hồ sơ đề xuất cấp độ an toàn hệ thống thông tin; phương án bảo vệ dữ liệu cá nhân, kiểm thử an toàn và xử lý sự cố.

**Tiếp thu/Giải trình:** Tiếp thu

**Nội dung tiếp thu/giải trình:**

Đơn vị triển khai hỗ trợ Chủ trì thuê xây dựng hồ sơ đề xuất cấp độ an toàn hệ thống thông tin theo quy định. Cụ thể:

**a) Hồ sơ đề xuất cấp độ an toàn hệ thống thông tin**
Lập theo quy định của Luật An toàn thông tin mạng và Nghị định số 85/2016/NĐ-CP về bảo đảm an toàn hệ thống thông tin theo cấp độ. Hồ sơ gồm: thuyết minh hệ thống, xác định cấp độ đề xuất `[dự kiến cấp độ 2 – Chủ trì thuê xác nhận]`, phương án bảo đảm an toàn theo cấp độ và danh mục biện pháp quản lý – kỹ thuật tương ứng. Hồ sơ được trình cấp có thẩm quyền phê duyệt **trước khi hệ thống đưa vào vận hành chính thức**; chi phí lập hồ sơ và các biện pháp bảo đảm an toàn đã nằm trong phạm vi dịch vụ thuê.

**b) Phương án bảo vệ dữ liệu cá nhân** (theo Nghị định số 13/2023/NĐ-CP)
- Xác định phạm vi dữ liệu cá nhân thu thập (tài khoản người dùng, doanh nghiệp/nhà cung ứng, chuyên gia tư vấn) và mục đích xử lý tương ứng;
- Thông báo và lấy sự đồng ý của chủ thể dữ liệu khi đăng ký tài khoản; công bố chính sách bảo vệ dữ liệu cá nhân trên cổng thông tin;
- Kiểm soát truy cập theo vai trò và theo nguyên tắc quyền tối thiểu; mã hóa kênh truyền bằng HTTPS/TLS; băm mật khẩu; tách cấu hình nhạy cảm khỏi mã nguồn;
- Bảo đảm quyền của chủ thể dữ liệu (truy cập, chỉnh sửa, rút lại sự đồng ý, yêu cầu xóa) qua chức năng quản lý hồ sơ cá nhân và đầu mối tiếp nhận yêu cầu;
- Lập Hồ sơ đánh giá tác động xử lý dữ liệu cá nhân; quy định thời hạn lưu trữ và hủy dữ liệu;
- Dữ liệu cá nhân được lưu trữ trên hạ tầng đặt tại Việt Nam trong suốt thời gian thuê.

**c) Kiểm thử an toàn thông tin**
- Kiểm thử theo thông lệ OWASP (ASVS, Top 10): kiểm soát CSRF, kiểm tra dữ liệu đầu vào, quản lý phiên đăng nhập, xử lý lỗi, bảo vệ cookie;
- Kiểm thử phân quyền và truy cập trái phép vào khu quản trị (CMS) và từng bước của quy trình giao dịch;
- Kiểm thử chức năng tải tệp: định dạng, dung lượng, tên tệp, đường dẫn lưu trữ, quyền truy cập tài liệu hợp đồng;
- Rà quét lỗ hổng và kiểm thử xâm nhập trên môi trường thử nghiệm trước nghiệm thu; lập báo cáo kết quả và biên bản khắc phục;
- Định kỳ hằng năm rà soát lại an toàn thông tin trong thời gian thuê, báo cáo kết quả cho Chủ trì thuê.

**d) Phương án xử lý sự cố**
- Phân loại mức độ sự cố; quy định đầu mối tiếp nhận, thời gian phản hồi và thời gian khắc phục theo từng mức `[cam kết SLA – thống nhất trong hợp đồng thuê]`;
- Ghi và giám sát nhật ký truy cập, nhật ký lỗi, nhật ký hợp đồng, nhật ký quy trình nghiệp vụ để phát hiện sớm truy cập bất thường;
- Sao lưu định kỳ cơ sở dữ liệu và thư mục tài liệu tải lên; diễn tập phục hồi trước nghiệm thu và định kỳ trong thời gian thuê, bảo đảm mục tiêu khôi phục `[RPO/RTO – Chủ trì thuê xác nhận]`;
- Báo cáo sự cố an toàn thông tin về cơ quan chức năng theo quy định; lập biên bản, phân tích nguyên nhân và biện pháp phòng ngừa tái diễn;
- Đào tạo, chuyển giao quy trình xử lý sự cố cho bộ phận quản trị của đơn vị vận hành (Mục IX hồ sơ thiết kế chi tiết).

---

## Mục 9

**Nội dung góp ý:** Kế hoạch vận hành nội dung: bộ máy quản trị, cán bộ môi giới/tư vấn, quy trình kiểm duyệt, dữ liệu khởi tạo, đối tác cung cấp dữ liệu và KPI vận hành 05 năm.

**Tiếp thu/Giải trình:** Tiếp thu

**Nội dung tiếp thu/giải trình:** Đơn vị triển khai tiếp thu, làm rõ như sau:

### 9.1. Bộ máy quản trị, vận hành

Chủ trì thuê thành lập **Tổ vận hành Sàn giao dịch công nghệ**, các vị trí có thể kiêm nhiệm trong giai đoạn đầu:

| TT | Vị trí | Số lượng | Nhiệm vụ chính |
|----|--------|----------|----------------|
| 1 | Tổ trưởng/phụ trách vận hành | 01 | Điều hành chung, phê duyệt nội dung, báo cáo định kỳ |
| 2 | Quản trị hệ thống | 01 | Máy chủ, cơ sở dữ liệu, sao lưu – phục hồi, tài khoản, phân quyền, cấu hình email/AI/ký số, giám sát nhật ký |
| 3 | Biên tập – kiểm duyệt nội dung | 02 | Kiểm duyệt hồ sơ công nghệ, nhà cung ứng, chuyên gia, tin bài, quảng cáo |
| 4 | Cán bộ môi giới/tư vấn công nghệ | 02 | Tiếp nhận nhu cầu, kết nối cung – cầu, đồng hành quy trình giao dịch 14 bước |
| 5 | Hỗ trợ người dùng và truyền thông | 01 | Trực kênh hỗ trợ, xử lý phản hồi, truyền thông sự kiện, triển lãm trực tuyến, livestream |

Đơn vị triển khai bố trí **đội hỗ trợ kỹ thuật thường trực trong suốt 05 năm thuê**: 01 đầu mối kỹ thuật, hỗ trợ trong giờ hành chính và trực sự cố ngoài giờ theo cam kết SLA; đồng thời đào tạo chuyển giao để bộ máy của Chủ trì thuê tự chủ vận hành.

*(Số lượng nêu trên là đề xuất; Chủ trì thuê quyết định theo cơ cấu tổ chức thực tế.)*

### 9.2. Cán bộ môi giới/tư vấn

- Yêu cầu năng lực: chuyên môn khoa học – công nghệ hoặc kinh tế – kỹ thuật, được tập huấn về quy trình chuyển giao công nghệ, định giá công nghệ, sở hữu trí tuệ và kỹ năng đàm phán;
- Được cấp tài khoản vai trò **chuyên gia tư vấn (consultant)** trên hệ thống, tham gia trực tiếp vào các bước của quy trình giao dịch 14 bước; toàn bộ hoạt động tư vấn được lưu vết trên hệ thống phục vụ theo dõi, đánh giá và tính KPI;
- Được đào tạo theo chương trình tại Mục IX hồ sơ thiết kế chi tiết: quản trị nội dung, vận hành quy trình nghiệp vụ, hợp đồng điện tử và ký số, AI Support, sao lưu dữ liệu, theo dõi nhật ký và xử lý sự cố thường gặp;
- Ngoài lực lượng cơ hữu, hệ thống cho phép huy động mạng lưới chuyên gia/tổ chức trung gian bên ngoài tham gia tư vấn theo từng hồ sơ giao dịch.

### 9.3. Quy trình kiểm duyệt nội dung

Kiểm duyệt thực hiện trên phân hệ CMS, gồm 3 bước:

1. **Tiếp nhận:** nội dung do doanh nghiệp/nhà cung ứng/chuyên gia đăng tải được lưu ở trạng thái *chờ duyệt*, chưa hiển thị công khai.
2. **Thẩm định:** cán bộ kiểm duyệt rà soát tính đầy đủ, chính xác của thông tin công nghệ/thiết bị; hồ sơ pháp lý của tổ chức; quyền sở hữu trí tuệ; hình ảnh – tài liệu đính kèm; sự phù hợp với quy định pháp luật.
3. **Phê duyệt và xuất bản:** người có thẩm quyền phê duyệt để hiển thị công khai, hoặc từ chối kèm lý do gửi về người đăng để chỉnh sửa.

Hệ thống lưu đầy đủ trạng thái (*nháp – chờ duyệt – đã duyệt – từ chối – gỡ*) và lịch sử xử lý (người thực hiện, thời điểm, nội dung thay đổi). Thời hạn xử lý đề xuất: **tối đa 02 ngày làm việc**. Nội dung vi phạm được gỡ khỏi cổng thông tin và ghi nhận vào nhật ký hệ thống.

### 9.4. Dữ liệu khởi tạo

Chi tiết tại **Phần II – mục 7** của bảng này.

### 9.5. Đối tác cung cấp dữ liệu

- Sở Khoa học và Công nghệ thành phố Cần Thơ và các đơn vị trực thuộc;
- Cơ quan quản lý thông tin, thống kê khoa học và công nghệ cấp quốc gia;
- Viện nghiên cứu, trường đại học, tổ chức khoa học công nghệ trên địa bàn và khu vực Đồng bằng sông Cửu Long;
- Doanh nghiệp khoa học công nghệ, chủ thể OCOP, hiệp hội doanh nghiệp, hợp tác xã;
- Các sàn giao dịch công nghệ và sự kiện kết nối cung – cầu công nghệ (Techmart, Techdemo và tương đương).

Việc cung cấp, cập nhật dữ liệu thực hiện theo thỏa thuận/quy chế phối hợp chia sẻ dữ liệu do Chủ trì thuê ban hành, tuân thủ quy định về bản quyền và bảo vệ dữ liệu cá nhân.

### 9.6. KPI vận hành 05 năm

| Chỉ tiêu | Năm 1 | Năm 2 | Năm 3 | Năm 4 | Năm 5 |
|---|---|---|---|---|---|
| Hồ sơ công nghệ/thiết bị được xác thực, công bố (lũy kế) | `[ ]` | `[ ]` | `[ ]` | `[ ]` | `[ ]` |
| Nhà cung ứng đăng ký (lũy kế) | `[ ]` | `[ ]` | `[ ]` | `[ ]` | `[ ]` |
| Chuyên gia tư vấn, tổ chức trung gian tham gia (lũy kế) | `[ ]` | `[ ]` | `[ ]` | `[ ]` | `[ ]` |
| Nhu cầu công nghệ tiếp nhận trong năm | `[ ]` | `[ ]` | `[ ]` | `[ ]` | `[ ]` |
| Lượt kết nối cung – cầu thành công trong năm | `[ ]` | `[ ]` | `[ ]` | `[ ]` | `[ ]` |
| Hợp đồng/thỏa thuận chuyển giao được ký trong năm | `[ ]` | `[ ]` | `[ ]` | `[ ]` | `[ ]` |
| Giá trị giao dịch qua sàn trong năm | `[ ]` | `[ ]` | `[ ]` | `[ ]` | `[ ]` |
| Lượt truy cập cổng thông tin trong năm | `[ ]` | `[ ]` | `[ ]` | `[ ]` | `[ ]` |
| Sự kiện truyền thông, triển lãm trực tuyến trong năm | `[ ]` | `[ ]` | `[ ]` | `[ ]` | `[ ]` |
| Thời gian ghép nối trung bình (từ khi tiếp nhận nhu cầu) | `[ ]` ngày | `[ ]` | `[ ]` | `[ ]` | `[ ]` |
| Tỷ lệ hồ sơ kiểm duyệt đúng hạn (≤ 02 ngày làm việc) | ≥ 90% | ≥ 92% | ≥ 95% | ≥ 95% | ≥ 95% |
| Tỷ lệ hài lòng của người dùng | ≥ 80% | ≥ 82% | ≥ 85% | ≥ 85% | ≥ 85% |
| Tỷ lệ thời gian hệ thống sẵn sàng (uptime) | ≥ 99% | ≥ 99% | ≥ 99% | ≥ 99% | ≥ 99% |

Số liệu KPI được tổng hợp tự động từ dashboard và các bảng nhật ký của hệ thống, phục vụ báo cáo định kỳ hằng quý/năm cho Chủ trì thuê. KPI là căn cứ nghiệm thu dịch vụ từng năm.

*(Các chỉ tiêu định lượng thống nhất với nội dung mục II.4 – Mục tiêu đầu ra.)*

---

# PHẦN II – Ý KIẾN THÀNH VIÊN HỘI ĐỒNG THẨM ĐỊNH VỀ NỘI DUNG KẾ HOẠCH THUÊ

## Mục 7 – Dữ liệu khởi tạo

**Nội dung góp ý:** Xác định số lượng tối thiểu hồ sơ công nghệ, thiết bị, chuyên gia, tổ chức trung gian và nhu cầu phải nhập/chuẩn hóa trước nghiệm thu, nguồn dữ liệu, nguồn khai thác và cơ chế cập nhật.

**Tiếp thu/Giải trình:** Tiếp thu

**Nội dung tiếp thu/giải trình:** Đơn vị triển khai cung cấp thông tin dữ liệu như sau:

**a) Khối lượng dữ liệu tối thiểu phải nhập/chuẩn hóa trước nghiệm thu**

| TT | Nhóm dữ liệu | Số lượng tối thiểu trước nghiệm thu |
|----|---|---|
| 1 | Hồ sơ công nghệ, thiết bị, tài sản trí tuệ chào bán | `[ 500 ]` hồ sơ |
| 2 | Nhu cầu công nghệ (bên mua) | `[ 100 ]` hồ sơ |
| 3 | Nhà cung ứng/doanh nghiệp công nghệ | `[ 200 ]` hồ sơ |
| 4 | Chuyên gia tư vấn | `[ 50 ]` hồ sơ |
| 5 | Tổ chức trung gian (viện, trường, tổ chức KH&CN, đơn vị môi giới) | `[ 20 ]` hồ sơ |
| 6 | Tin bài, video truyền thông khoa học công nghệ | `[ 200 ]` bản ghi |
| 7 | Tài liệu cơ sở tri thức phục vụ AI Support | `[ 100 ]` tài liệu |
| 8 | Danh mục nền: lĩnh vực công nghệ, phân nhóm sản phẩm, menu, tham số cấu hình | Đầy đủ theo thiết kế |

*(Số lượng nêu trên là đề xuất của đơn vị triển khai; Chủ trì thuê chốt và đưa vào bảng kiểm nghiệm thu.)*

Toàn bộ hồ sơ phải đạt yêu cầu chuẩn hóa: đủ trường thông tin bắt buộc, có phân loại lĩnh vực, có hình ảnh/tài liệu minh họa, có thông tin đầu mối liên hệ, đã sinh chỉ mục tìm kiếm và dữ liệu vector hóa phục vụ tìm kiếm ngữ nghĩa, gợi ý kết nối cung – cầu.

**b) Nguồn dữ liệu và nguồn khai thác**
- Dữ liệu sẵn có của Sở Khoa học và Công nghệ thành phố Cần Thơ và các đơn vị trực thuộc;
- Kết quả nghiên cứu, nhiệm vụ khoa học công nghệ các cấp đã nghiệm thu trên địa bàn;
- Dữ liệu công bố của cơ quan quản lý thông tin, thống kê khoa học và công nghệ cấp quốc gia;
- Viện nghiên cứu, trường đại học, tổ chức khoa học công nghệ khu vực Đồng bằng sông Cửu Long;
- Doanh nghiệp khoa học công nghệ, chủ thể OCOP, hợp tác xã, hiệp hội doanh nghiệp trên địa bàn;
- Dữ liệu công khai từ các sàn giao dịch công nghệ, sự kiện kết nối cung – cầu (Techmart, Techdemo và tương đương) trong phạm vi được phép sử dụng;
- Khảo sát, thu thập trực tiếp từ doanh nghiệp và chuyên gia trong giai đoạn thiết lập hệ thống năm 2026.

**c) Cơ chế cập nhật dữ liệu trong 05 năm vận hành**
1. **Doanh nghiệp/chuyên gia tự đăng tải** qua tài khoản trên hệ thống, cán bộ kiểm duyệt phê duyệt trước khi hiển thị công khai (quy trình tại mục I.9.3);
2. **Cán bộ vận hành nhập trực tiếp** trên phân hệ CMS đối với dữ liệu do cơ quan quản lý cung cấp;
3. **Nhập liệu hàng loạt** bằng biểu mẫu Excel/CSV theo cấu trúc chuẩn, có kiểm tra trùng lặp và hợp lệ trước khi ghi nhận;
4. **Kết nối, chia sẻ dữ liệu qua API** với các hệ thống thông tin khác khi được cấp có thẩm quyền cho phép;
5. **Rà soát định kỳ 06 tháng/lần**: hệ thống cảnh báo hồ sơ quá hạn cập nhật, yêu cầu chủ hồ sơ xác nhận lại; hồ sơ không còn hiệu lực được chuyển sang trạng thái lưu trữ;
6. **Chỉ tiêu bổ sung hằng năm** được đưa vào bảng KPI tại mục I.9.6 và là căn cứ nghiệm thu dịch vụ từng năm.

---

## Mục 8 – Sở hữu và bàn giao

**Nội dung góp ý:** Làm rõ quyền sở hữu dữ liệu, tên miền, tên tài khoản quản trị, cấu hình, quy trình tài liệu, dữ liệu AI và tài sản số; quy định định dạng xuất, thời gian hỗ trợ chuyển tiếp, xóa dữ liệu và xác nhận chấm dứt quyền truy cập.

**Tiếp thu/Giải trình:** Tiếp thu

**Nội dung tiếp thu/giải trình:** Đơn vị triển khai đưa giải pháp như sau:

**a) Quyền sở hữu**

| Đối tượng | Chủ sở hữu | Ghi chú |
|---|---|---|
| Toàn bộ dữ liệu nghiệp vụ (hồ sơ công nghệ, nhà cung ứng, chuyên gia, nhu cầu, người dùng, giao dịch, hợp đồng, nhật ký) | **Chủ trì thuê** | Đơn vị triển khai chỉ được xử lý trong phạm vi cung cấp dịch vụ |
| Tên miền, chứng thư số TLS | **Chủ trì thuê** | Đăng ký đứng tên Chủ trì thuê ngay từ đầu |
| Tài khoản quản trị cao nhất của hệ thống, máy chủ, cơ sở dữ liệu | **Chủ trì thuê** | Bàn giao tại thời điểm nghiệm thu đưa vào sử dụng |
| Cấu hình hệ thống, danh mục, tham số, quy tắc nghiệp vụ, phân quyền, biểu mẫu | **Chủ trì thuê** | Có tài liệu mô tả kèm theo |
| Cơ sở tri thức và dữ liệu AI (tài liệu tri thức, dữ liệu vector hóa, nhật ký hỏi – đáp) | **Chủ trì thuê** | Xuất được dưới dạng tệp độc lập |
| Tài sản số: logo, hình ảnh, video, nội dung số hóa, cảnh VR360, tài liệu truyền thông | **Chủ trì thuê** | Bàn giao tệp gốc, độ phân giải gốc |
| Tài liệu quy trình, hướng dẫn quản trị, hướng dẫn người dùng, tài liệu kỹ thuật | **Chủ trì thuê** | Định dạng có thể chỉnh sửa |
| Nền tảng phần mềm dùng chung, mã nguồn khung của đơn vị triển khai | Đơn vị triển khai | Chủ trì thuê được cấp quyền sử dụng trong thời gian thuê (xem mục 9) |

**b) Định dạng xuất dữ liệu khi bàn giao**
- Cơ sở dữ liệu: bản sao lưu đầy đủ SQL Server (`.bak`/`.bacpac`) kèm script tạo cấu trúc và từ điển dữ liệu;
- Dữ liệu dạng bảng: xuất bổ sung theo định dạng mở CSV/Excel/JSON để dùng độc lập với nền tảng;
- Tệp đính kèm, hình ảnh, video, hợp đồng: bàn giao theo cây thư mục gốc kèm bảng ánh xạ tệp – bản ghi;
- Dữ liệu AI: tài liệu tri thức (nguyên bản), dữ liệu vector hóa và nhật ký hỏi – đáp dạng JSON/CSV;
- Tài liệu: PDF kèm bản gốc DOCX/XLSX chỉnh sửa được;
- Ngoài ra hệ thống cung cấp **API xuất dữ liệu** để Chủ trì thuê chủ động trích xuất bất kỳ lúc nào trong thời gian thuê.

**c) Bàn giao định kỳ trong thời gian thuê**
Ngoài lần bàn giao khi kết thúc, đơn vị triển khai bàn giao **bản sao dữ liệu đầy đủ định kỳ `[hằng quý]`** về hạ tầng lưu trữ do Chủ trì thuê chỉ định, có biên bản xác nhận. Cơ chế này bảo đảm Chủ trì thuê luôn nắm giữ dữ liệu, không phụ thuộc vào tình trạng hoạt động của nhà cung cấp.

**d) Thời gian hỗ trợ chuyển tiếp**
Khi kết thúc hoặc chấm dứt hợp đồng thuê, đơn vị triển khai hỗ trợ chuyển tiếp tối thiểu `[03 tháng]`, gồm: xuất và bàn giao toàn bộ dữ liệu, hướng dẫn tiếp nhận, hỗ trợ chuyển sang hạ tầng/nhà cung cấp mới, giải đáp kỹ thuật. Trong thời gian này hệ thống vẫn được duy trì để không gián đoạn dịch vụ.

**e) Xóa dữ liệu và chấm dứt quyền truy cập**
1. Sau khi Chủ trì thuê ký biên bản xác nhận đã nhận đủ dữ liệu, đơn vị triển khai xóa toàn bộ bản sao dữ liệu trên hạ tầng của mình (kể cả bản sao lưu) trong thời hạn `[30 ngày]`;
2. Lập **Biên bản xóa dữ liệu** liệt kê phạm vi dữ liệu đã xóa, thời điểm và người thực hiện, gửi Chủ trì thuê;
3. Thu hồi toàn bộ tài khoản truy cập của nhân sự đơn vị triển khai; hủy khóa API, khóa ký số và các thông tin xác thực liên quan;
4. Chủ trì thuê đổi mật khẩu quản trị và xác nhận bằng **Biên bản chấm dứt quyền truy cập**;
5. Đơn vị triển khai cam kết bảo mật thông tin sau khi chấm dứt hợp đồng theo thỏa thuận bảo mật (NDA) ký kèm hợp đồng thuê.

Các nội dung a–e được đưa thành **điều khoản ràng buộc trong hợp đồng thuê dịch vụ**.

---

## Mục 9 – Mã nguồn và khóa nhà cung cấp

**Nội dung góp ý:** Kế hoạch nếu không chuyển giao mã nguồn. Nếu chấp nhận mô hình dịch vụ, phải có API, tài liệu dữ liệu, cấu hình nghiệp vụ, quyền quản trị và phương án chuyển nhà cung cấp đủ để không gián đoạn hoặc mất dữ liệu.

**Tiếp thu/Giải trình:** Tiếp thu

**Nội dung tiếp thu/giải trình:** Đơn vị triển khai đưa giải pháp như sau:

**a) Về mô hình**
Hoạt động được thực hiện theo hình thức **thuê dịch vụ công nghệ số**, do đó mã nguồn nền tảng không thuộc phạm vi chuyển giao. Để bảo đảm Chủ trì thuê không bị phụ thuộc nhà cung cấp (vendor lock-in), đơn vị triển khai cam kết đồng thời **06 nhóm biện pháp** dưới đây, đưa thành điều khoản hợp đồng thuê.

**b) Cam kết về API và trích xuất dữ liệu**
- Cung cấp bộ **API mở đầy đủ** cho phép truy xuất 100% dữ liệu nghiệp vụ do Chủ trì thuê sở hữu; không giới hạn số lần, không thu phí trích xuất;
- Bàn giao **tài liệu API** (đặc tả OpenAPI/Swagger, ví dụ gọi, cơ chế xác thực, giới hạn kỹ thuật);
- Cung cấp chức năng **xuất dữ liệu hàng loạt** theo định dạng mở (CSV/JSON/Excel) trực tiếp trên giao diện quản trị.

**c) Cam kết về tài liệu dữ liệu**
- **Từ điển dữ liệu** và mô hình dữ liệu (ERD): mô tả toàn bộ bảng, trường, kiểu dữ liệu, ràng buộc và ý nghĩa nghiệp vụ;
- Sơ đồ luồng dữ liệu giữa các phân hệ; danh mục dữ liệu chia sẻ với hệ thống bên ngoài;
- Hướng dẫn khôi phục dữ liệu từ bản sao lưu trên môi trường độc lập.

**d) Cam kết về cấu hình nghiệp vụ**
- Tài liệu mô tả toàn bộ cấu hình nghiệp vụ: danh mục, tham số hệ thống, quy tắc của quy trình giao dịch 14 bước, ma trận phân quyền, biểu mẫu, mẫu hợp đồng, quy tắc thông báo;
- Cấu hình được lưu dưới dạng dữ liệu (không mã hóa cứng trong chương trình) và **xuất được ra tệp** để tái lập trên nền tảng khác.

**e) Cam kết về quyền quản trị**
- Chủ trì thuê được cấp **tài khoản quản trị cao nhất** của ứng dụng, cơ sở dữ liệu và hạ tầng thuê, có toàn quyền tạo/khóa tài khoản, phân quyền, cấu hình và trích xuất dữ liệu;
- Tên miền và chứng thư số đứng tên Chủ trì thuê;
- Mọi thao tác của nhân sự đơn vị triển khai trên hệ thống đều được ghi nhật ký và Chủ trì thuê xem được.

**f) Phương án chuyển nhà cung cấp (exit plan)**
1. Đơn vị triển khai lập **Kế hoạch chuyển tiếp** ngay khi ký hợp đồng, cập nhật hằng năm;
2. Duy trì bản sao dữ liệu đầy đủ định kỳ `[hằng quý]` tại hạ tầng của Chủ trì thuê (mục 8.c) — nếu phải chuyển đổi, dữ liệu đã sẵn sàng, không có nguy cơ mất dữ liệu;
3. Khi chuyển đổi, hỗ trợ tối thiểu `[03 tháng]`: xuất dữ liệu theo định dạng chuẩn, bàn giao tài liệu, phối hợp với nhà cung cấp mới, chạy song song đến khi hệ thống mới ổn định — **không gián đoạn dịch vụ**;
4. **Không thu phí thoát (exit fee)**, không giữ dữ liệu làm điều kiện ràng buộc;
5. Sử dụng công nghệ nền phổ biến, không độc quyền (ASP.NET Core, SQL Server, chuẩn HTTP/REST, định dạng dữ liệu mở), giúp nhà cung cấp khác dễ tiếp nhận;
6. Chế tài: hợp đồng quy định rõ trách nhiệm bồi thường nếu đơn vị triển khai không thực hiện đầy đủ nghĩa vụ bàn giao và chuyển tiếp.

**g) Phương án dự phòng – ký quỹ mã nguồn (tùy chọn)**
Trường hợp Chủ trì thuê yêu cầu mức bảo đảm cao hơn, hai bên có thể thỏa thuận **ký quỹ mã nguồn (source code escrow)** tại bên thứ ba độc lập: mã nguồn và tài liệu kỹ thuật được gửi giữ, chỉ được giải tỏa cho Chủ trì thuê khi xảy ra các tình huống đã thỏa thuận (đơn vị triển khai ngừng hoạt động, mất khả năng cung cấp dịch vụ, hoặc vi phạm nghiêm trọng hợp đồng). Chi phí ký quỹ được tính bổ sung vào dự toán thuê nếu áp dụng.
