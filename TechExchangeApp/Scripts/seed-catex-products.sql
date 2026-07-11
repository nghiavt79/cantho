/*
Imports up to 55 products scraped from catex.vn (Sàn giao dịch Công nghệ - Thiết bị
Cần Thơ, catex.vn) — the legacy government tech-exchange site this platform replaces.
Only public product data kept (name/description/manufacturer/origin/image); all
personal seller info (poster name, phone, login username) deliberately excluded.
Imported as StatusId=1 (draft/pending review) for admin sign-off before publishing.
Idempotent — guarded by Name + ProductType.
*/

SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Now DATETIME = GETDATE();

CREATE TABLE #CatexSeed (
    Name NVARCHAR(500), ProductType INT, XuatXu NVARCHAR(500), MoTaNgan NVARCHAR(500),
    MoTa NVARCHAR(MAX), ThongSo NVARCHAR(MAX), QuyTrinhHinhAnh NVARCHAR(500)
);

INSERT INTO #CatexSeed (Name,ProductType,XuatXu,MoTaNgan,MoTa,ThongSo,QuyTrinhHinhAnh) VALUES
(N'Máy lạnh âm trần LG 1 hướng thổi ZTNQ12GULA0 inverter',2,N'Việt Nam',N'Máy lạnh âm trần LG 1 hướng có công suất từ 1.5hp - 2.5hp',N'Máy lạnh âm trần LG 1 hướng có công suất từ 1.5hp - 2.5hp',N'<ul><li><strong>Hãng sản xuất:</strong> LG</li><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/0677ae431ec30929.jpg'),
(N'MÁY TARO KHÍ NÉN AR-22',2,N'Đài Loan',N'Máy ta rô khí nén AR-22 được thiết kế chuyên dùng ta rô kim loại: sắt thép, nhôm hoặc gang. Đây là loại máy ta rô tay sử dụng áp lực khí nén để tạo ra khả năng ta rô trên kim loại. Nó có nhiều ứng dụng trong cơ khí, chế tạo khuôn mẫu.',N'Máy ta rô khí nén AR-22 được thiết kế chuyên dùng ta rô kim loại: sắt thép, nhôm hoặc gang. Đây là loại máy ta rô tay sử dụng áp lực khí nén để tạo ra khả năng ta rô trên kim loại. Nó có nhiều ứng dụng trong cơ khí, chế tạo khuôn mẫu.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/e29401ff29d1d915.png'),
(N'Bộ thiết bị hội nghị truyền hình Yealink MVC320',2,N'Trung Quốc',N'Thiết bị hội nghị Yealink MVC320 mang mọi thứ bạn cần để tương tác cho một phòng họp hoàn chỉnh, kết nối và cộng tác với người dùng',N'Thiết bị hội nghị Yealink MVC320 mang mọi thứ bạn cần để tương tác cho một phòng họp hoàn chỉnh, kết nối và cộng tác với người dùng',N'<ul><li><strong>Tình trạng:</strong> đã qua sử dụng</li></ul>',N'/uploads/san-pham-catex/70fe0d54264b9ba4.jpg'),
(N'Camera hội nghị Oneking HD920-U30-K5',2,N'Trung Quốc',N'Camera Oneking HD920-U30-K5 mang lại trải nghiệm cuộc họp chất lượng cao và khả năng tự động lấy nét làm cho hình ảnh mọi người trong phòng họp trở nên rõ ràng.',N'Camera Oneking HD920-U30-K5 mang lại trải nghiệm cuộc họp chất lượng cao và khả năng tự động lấy nét làm cho hình ảnh mọi người trong phòng họp trở nên rõ ràng.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li><li><strong>Bảo hành:</strong> 12 Tháng</li></ul>',N'/uploads/san-pham-catex/7200a0086f9c934a.jpg'),
(N'Loa hội nghị Yealink MSpeech',2,N'Trung Quốc',N'Loa hội nghị Yealink MSpeech mang đến giải pháp hội nghị audio thông minh hứa hẹn sẽ đem lại cho bạn những trải nghiệm hội họp tuyệt vời nhất, Với những đánh giá chi tiết về chất lượng âm thanh cũng như các tính năng của nó cho cuộc họp ở bất cứ đâu, bất cứ khi nào bạn cần.',N'Loa hội nghị Yealink MSpeech mang đến giải pháp hội nghị audio thông minh hứa hẹn sẽ đem lại cho bạn những trải nghiệm hội họp tuyệt vời nhất, Với những đánh giá chi tiết về chất lượng âm thanh cũng như các tính năng của nó cho cuộc họp ở bất cứ đâu, bất cứ khi nào bạn cần.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li><li><strong>Bảo hành:</strong> 12 Tháng</li></ul>',N'/uploads/san-pham-catex/72068f299d2cb855.jpg'),
(N'Máy dò kim loại Cassel',2,N'Đức',N'Máy móc Công nghiệp',N'Máy móc Công nghiệp',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li><li><strong>Bảo hành:</strong> 24 tháng</li></ul>',N'/uploads/san-pham-catex/9ce63b026b16cb28.jpg'),
(N'BIO SUN - TĂNG CƯỜNG QUANG HỢP CHO CÂY CHAI 1 LÍT',1,N'Mỹ',N'- Đẩy mạnh quang hợp tạo chất hữu cơ nuôi cây, giúp cây phát triển khỏe, bụi to, ra chồi nhiều. Tăng sức đề kháng và cải thiện năng suất cây trồng.
- Bổ sung lượng vi sinh vật đa dạng có ích cho đất, giúp đất khỏe và tăng độ mùn. Kích thích bộ rễ khỏe mạnh để tăng khả năng hấp thu chất dinh dưỡng trong rễ, giảm nhu cầu phân bón bổ sung.',N'- Đẩy mạnh quang hợp tạo chất hữu cơ nuôi cây, giúp cây phát triển khỏe, bụi to, ra chồi nhiều. Tăng sức đề kháng và cải thiện năng suất cây trồng.
- Bổ sung lượng vi sinh vật đa dạng có ích cho đất, giúp đất khỏe và tăng độ mùn. Kích thích bộ rễ khỏe mạnh để tăng khả năng hấp thu chất dinh dưỡng trong rễ, giảm nhu cầu phân bón bổ sung.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/136de4377b88c3f5.jpg'),
(N'BIO SOIL - CẢI TẠO ĐẤT BẠC MÀU, ĐIỀU CHỈNH pH ĐẤT CHAI 1 LÍT',1,N'Mỹ',N'- Cải tạo đất, phục hồi đất bạc màu, cằn cỗi, đất chai do sử dụng thuốc hóa học lâu ngày. Cung cấp mùn cho đất/ rễ, giúp đất tơi xốp, giữ ẩm, chống úng. Tăng cường vi sinh vật có lợi cho đất. Cố định nitơ và cacbon, duy trì, bắt giữ, và liên kết với các chất dinh dưỡng trong đất. Giúp cây cứng cáp khỏe mạnh.

- Chuyển hóa chất hữu cơ trong đất thành các nguồn năng lượng cho cây, phân hủy thuốc trừ sâu và các chất ô nhiễm, giúp vận chuyển các khoáng chất trong vùng rễ. Giúp hạt giống ',N'- Cải tạo đất, phục hồi đất bạc màu, cằn cỗi, đất chai do sử dụng thuốc hóa học lâu ngày. Cung cấp mùn cho đất/ rễ, giúp đất tơi xốp, giữ ẩm, chống úng. Tăng cường vi sinh vật có lợi cho đất. Cố định nitơ và cacbon, duy trì, bắt giữ, và liên kết với các chất dinh dưỡng trong đất. Giúp cây cứng cáp khỏe mạnh.

- Chuyển hóa chất hữu cơ trong đất thành các nguồn năng lượng cho cây, phân hủy thuốc trừ sâu và các chất ô nhiễm, giúp vận chuyển các khoáng chất trong vùng rễ. Giúp hạt giống nảy mầm (ủ giống), giúp rễ khỏe mạnh, phòng ngừa thối rễ. Giảm lượng phân bón hóa học.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/c48e6dac18f7b1f7.jpg'),
(N'BIO K+ - CUNG CẤP KALI VÀ LÂN CHAI 1 LÍT',1,N'Mỹ',N'- Bổ sung Kali và Lân cho cây thông qua lá, thúc đẩy phân hóa mầm hoa/ ra hoa. Tăng độ ngọt (brix), ngon và độ chắc bóng cho trái.
- Tăng độ chắc của thành mạch, giúp tăng khả năng đề kháng sâu bệnh, giữ nước tốt, chống hạn cho cây. Ngăn ngừa vàng lá, lép hạt, giảm rụng trái non, nấm phấn trắng.',N'- Bổ sung Kali và Lân cho cây thông qua lá, thúc đẩy phân hóa mầm hoa/ ra hoa. Tăng độ ngọt (brix), ngon và độ chắc bóng cho trái.
- Tăng độ chắc của thành mạch, giúp tăng khả năng đề kháng sâu bệnh, giữ nước tốt, chống hạn cho cây. Ngăn ngừa vàng lá, lép hạt, giảm rụng trái non, nấm phấn trắng.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/56da0a8f261e354b.jpg'),
(N'BIO PAECIL - ĐẶC TRỊ TUYẾN TRÙNG CHAI 1 LÍT',1,N'Mỹ',N'- Yếu tố sinh học có hiệu quả kiểm soát tuyến trùng tốt hơn so với thuốc trừ sâu hóa học hiện nay cho cây trồng. Hiệu lực kiểm soát từ 3-5 ngày. Nấm chịu được nhiệt độ tối đa là 65 độ C. Nấm Paecilomyces lilacinus sẽ phát triển thành những nút thắt dạng vòng để bắt tuyến trùng.
- Diệt và ức chế ấu trùng. Bảo vệ cây trồng trong suốt thời gian sinh trưởng và phát triển, không tiêu diệt sinh vật có lợi trong đất, không độc hại cho môi trường và con người.',N'- Yếu tố sinh học có hiệu quả kiểm soát tuyến trùng tốt hơn so với thuốc trừ sâu hóa học hiện nay cho cây trồng. Hiệu lực kiểm soát từ 3-5 ngày. Nấm chịu được nhiệt độ tối đa là 65 độ C. Nấm Paecilomyces lilacinus sẽ phát triển thành những nút thắt dạng vòng để bắt tuyến trùng.
- Diệt và ức chế ấu trùng. Bảo vệ cây trồng trong suốt thời gian sinh trưởng và phát triển, không tiêu diệt sinh vật có lợi trong đất, không độc hại cho môi trường và con người.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/7aa5db1f0ebefbad.jpg'),
(N'BIO NPK - BỔ SUNG KHOÁNG CHO CÂY CHAI 1 LÍT',1,N'Mỹ',N'- Liên kết cố định Nitơ trong khí quyển, tạo và tăng nguồn Nitơ dinh dưỡng tự nhiên cung cấp cho cây. Hòa tan Phospho khó tan trong đất giúp cây trồng dễ hấp thụ. Huy động Kali trong đất, giúp cây trồng dễ dàng tổng hợp và hấp thụ, có thể tiết kiệm được 50-60% Kali trong phân hóa học.
- Cải thiện và duy trì độ phì của đất, giúp cây trồng hấp thu các chất Nitrogen, Phospho và Kali tự nhiên một cách tối đa. Cải thiện năng suất, chất lượng cây trồng, tăng sức đề kháng chống lại sâu bệnh ',N'- Liên kết cố định Nitơ trong khí quyển, tạo và tăng nguồn Nitơ dinh dưỡng tự nhiên cung cấp cho cây. Hòa tan Phospho khó tan trong đất giúp cây trồng dễ hấp thụ. Huy động Kali trong đất, giúp cây trồng dễ dàng tổng hợp và hấp thụ, có thể tiết kiệm được 50-60% Kali trong phân hóa học.
- Cải thiện và duy trì độ phì của đất, giúp cây trồng hấp thu các chất Nitrogen, Phospho và Kali tự nhiên một cách tối đa. Cải thiện năng suất, chất lượng cây trồng, tăng sức đề kháng chống lại sâu bệnh hại. Thân thiện với môi trường, con người, hiệu quả kinh tế cao mà không để lại bất kỳ dư lượng.
- Được ưu tiên sử dụng đối với các loại cây trồng cho quả, hạt... để tạo sự chín sớm, chín mùi, tăng chất lượng quả, tăng lượng rix - độ ngọt của trái cây.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/d146e8aaf31cdf75.jpg'),
(N'BIO TRICHO - KIỂM SOÁT NẤM BỆNH HẠI RỄ CHAI 1 LÍT',1,N'Mỹ',N'- Là loại thuốc đặc hiệu tiêu diệt các nấm gây hại cho cây trồng (Fusarium, Phytopthora, Pythium, Scherotina, Rhizoctonia, Erwinia,…) nhưng không ảnh hưởng đến các sinh vật có lợi.
- Bảo vệ cây trồng với tất cả các loại bệnh truyền qua đất (thối rễ, lở cổ rễ, chết rạp...)
- Giúp cây đâm chồi nhanh chóng sau khi gãy đổ hoặc sau khi cắt tỉa, giúp bộ rễ phát triển khỏe mạnh. Không để lại dư lượng thuốc như các sản phẩm diệt nấm khác.',N'- Là loại thuốc đặc hiệu tiêu diệt các nấm gây hại cho cây trồng (Fusarium, Phytopthora, Pythium, Scherotina, Rhizoctonia, Erwinia,…) nhưng không ảnh hưởng đến các sinh vật có lợi.
- Bảo vệ cây trồng với tất cả các loại bệnh truyền qua đất (thối rễ, lở cổ rễ, chết rạp...)
- Giúp cây đâm chồi nhanh chóng sau khi gãy đổ hoặc sau khi cắt tỉa, giúp bộ rễ phát triển khỏe mạnh. Không để lại dư lượng thuốc như các sản phẩm diệt nấm khác.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/f71d396ffb581c07.jpg'),
(N'BIO FOR PLANT- CUNG CẤP MAGIE CHO CÂY TRỒNG HỦ 227GRAM',1,N'Mỹ',N'Bổ sung dinh dưỡng cây trồng:
- Giảm sốc, thúc đẩy nhanh sự đồng hóa và sản sinh năng lượng cho cây.
- Hiệu quả trong trường hợp cây trồng cần phục hồi nhanh, tái tạo lại sức sống cho cây.
- Kích thích sự hấp thụ và cân bằng dinh dưỡng.
- Bổ sung Nitơ và Magie
- Cung cấp độ mùn cần thiết cho đất.
- Thúc đẩy rễ phát triển nhanh, mạnh.',N'Bổ sung dinh dưỡng cây trồng:
- Giảm sốc, thúc đẩy nhanh sự đồng hóa và sản sinh năng lượng cho cây.
- Hiệu quả trong trường hợp cây trồng cần phục hồi nhanh, tái tạo lại sức sống cho cây.
- Kích thích sự hấp thụ và cân bằng dinh dưỡng.
- Bổ sung Nitơ và Magie
- Cung cấp độ mùn cần thiết cho đất.
- Thúc đẩy rễ phát triển nhanh, mạnh.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/7987badb8f76dc09.jpg'),
(N'BIO ENSURE - CUNG CẤP VITAMIN CHO CÂY CHAI 350ML',1,N'Ấn Độ',N'- Phòng ngừa, ức chế hiệu quả sự tăng trưởng của nấm bệnh (thán thư, phấn trắng, sương mai, gỉ sắt, đạo ôn, thối thân, thối cành,….). Thúc đẩy tế bào thực vật tăng trưởng, phát triển.
- Cải thiện hệ miễn dịch của thực vật chống lại những vi sinh gây hại. Có lợi trong việc hòa tan phốt pho và đóng vai trò quan trong trong việc đồng hóa đạm. Cân bằng nước trong cây, giúp cây vượt qua stress.',N'- Phòng ngừa, ức chế hiệu quả sự tăng trưởng của nấm bệnh (thán thư, phấn trắng, sương mai, gỉ sắt, đạo ôn, thối thân, thối cành,….). Thúc đẩy tế bào thực vật tăng trưởng, phát triển.
- Cải thiện hệ miễn dịch của thực vật chống lại những vi sinh gây hại. Có lợi trong việc hòa tan phốt pho và đóng vai trò quan trong trong việc đồng hóa đạm. Cân bằng nước trong cây, giúp cây vượt qua stress.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/e1d6121e078b9ab2.png'),
(N'BIO MAGIC - TĂNG CƯỜNG HIỆU QUẢ BÁM DÍNH CHAI 350ML',1,N'Ấn Độ',N'- Tăng độ bám dính cho thuốc bảo vệ thực vật, giúp tăng hiệu quả sử dụng thuốc.
- Tương thích với hầu hết thuốc trừ sâu.',N'- Tăng độ bám dính cho thuốc bảo vệ thực vật, giúp tăng hiệu quả sử dụng thuốc.
- Tương thích với hầu hết thuốc trừ sâu.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/abe9288a925d1426.jpg'),
(N'BIO CAWACH - PHÒNG TRỪ KIẾN LỬA CHAI 350ML',1,N'Ấn Độ',N'- Chuyên trị kiến lửa
- Không để lại tồn dư thuốc sau sử dụng
- Không chứa độc tố và không gây ảnh hưởng tới các loài côn trùng có ích
- Hiệu quả lâu dài, hiệu lực nhanh và mạnh',N'- Chuyên trị kiến lửa
- Không để lại tồn dư thuốc sau sử dụng
- Không chứa độc tố và không gây ảnh hưởng tới các loài côn trùng có ích
- Hiệu quả lâu dài, hiệu lực nhanh và mạnh',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/39cf42077015f40d.jpg'),
(N'BIO NEEMAKAR - PHÒNG TRỪ CÔN TRÙNG GÂY HẠI 350ML',1,N'Ấn Độ',N'- Chống lại hơn 200 loài côn trùng bộ chích hút, bộ gặm nhai và nhện đỏ.
- Hoạt động như chất lây nhiễm, kháng lại sâu hại mạnh mẽ, ức chế sự tăng trưởng, giết chết giai đoạn nhộng con của côn trùng, bao gồm cả bọ cánh cứng, gây ngán ăn, lây nhiễm và diệt trứng.
- Thúc đẩy, ngăn cản và phá hủy hoạt động giao phối, đẻ trứng của tất cả các loài sâu hại, không để côn trùng phát triển kháng lại sản phẩm.
- Không ảnh hưởng đến sức khỏe con người, thiên địch có ích và sinh vật khác.',N'- Chống lại hơn 200 loài côn trùng bộ chích hút, bộ gặm nhai và nhện đỏ.
- Hoạt động như chất lây nhiễm, kháng lại sâu hại mạnh mẽ, ức chế sự tăng trưởng, giết chết giai đoạn nhộng con của côn trùng, bao gồm cả bọ cánh cứng, gây ngán ăn, lây nhiễm và diệt trứng.
- Thúc đẩy, ngăn cản và phá hủy hoạt động giao phối, đẻ trứng của tất cả các loài sâu hại, không để côn trùng phát triển kháng lại sản phẩm.
- Không ảnh hưởng đến sức khỏe con người, thiên địch có ích và sinh vật khác.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/d2d1e8a2ce646b6a.jpg'),
(N'BIO SUPER GUARD -TRỊ NẤM BỆNH CHO CÂY TRỒNG 350ML',1,N'Ấn Độ',N'- Giúp cây phục hồi nhanh chóng và hiệu quả trong 48 giờ sau khi sử dụng.
- Dùng cho các loại cây ngũ cốc, rau màu, hoa, cây thuốc...
- Ngăn ngừa và chữa trị các loại bệnh gây do nấm và vi khuẩn gây ra trên lá, thân và trái (cháy lá, bệnh sương mai, đạo ôn, thối, thán thư...), kiểm soát các mầm bệnh đã trở nên kháng thuốc.',N'- Giúp cây phục hồi nhanh chóng và hiệu quả trong 48 giờ sau khi sử dụng.
- Dùng cho các loại cây ngũ cốc, rau màu, hoa, cây thuốc...
- Ngăn ngừa và chữa trị các loại bệnh gây do nấm và vi khuẩn gây ra trên lá, thân và trái (cháy lá, bệnh sương mai, đạo ôn, thối, thán thư...), kiểm soát các mầm bệnh đã trở nên kháng thuốc.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/ed80f301b2149d41.jpg'),
(N'AQUA M5 - VI SINH XỬ LÝ KHÍ ĐỘC NH3, NO2, H2S TRONG AO NUÔI THỦY SẢN',1,N'Mỹ',N'- Loại bỏ khí độc NH3, NO2, H2S trong ao nuôi
- Ngăn ngừa mầm bênh phát triển trong ao nuôi
- Ổn định pH và môi trường nước
- Cấp cứu tôm bị thiếu oxy , nổi đầu do sự tích tụ khí độc
- Giúp tôm, cá phát triển tốt, tăng kích cỡ và trọng lượng',N'- Loại bỏ khí độc NH3, NO2, H2S trong ao nuôi
- Ngăn ngừa mầm bênh phát triển trong ao nuôi
- Ổn định pH và môi trường nước
- Cấp cứu tôm bị thiếu oxy , nổi đầu do sự tích tụ khí độc
- Giúp tôm, cá phát triển tốt, tăng kích cỡ và trọng lượng',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/a6712d098156f01c.jpg'),
(N'AQUA KL - XỬ LÝ PHÈN VÀ KIM LOẠI NẶNG TRONG AO NUÔI THỦY SẢN',1,N'Mỹ',N'- Khử phèn và kim loại nặng hiệu quả trong ao nuôi thủy sản.

- Hỗ trợ tăng kiềm cho ao nuôi.

- Giải độc nước. Khắc phục tình trạng khó gây màu nước.

- Khắc phục tốt tình trạng tôm mềm vỏ, chậm lớn, lột xác không hoàn toàn.',N'- Khử phèn và kim loại nặng hiệu quả trong ao nuôi thủy sản.

- Hỗ trợ tăng kiềm cho ao nuôi.

- Giải độc nước. Khắc phục tình trạng khó gây màu nước.

- Khắc phục tốt tình trạng tôm mềm vỏ, chậm lớn, lột xác không hoàn toàn.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/c6e8419320aaf058.jpg'),
(N'AQUA SC - VI SINH XỬ LÝ MÔI TRƯỜNG NƯỚC AO NUÔI THỦY SẢN, HỒ CÁ KOI',1,N'Mỹ',N'- Xử lý chất hữu cơ, thức ăn dư thừa, chất thải của vật nuôi
- Hạn chế hình thành lớp bùn đáy, giữ cân bằng sinh thái cho ao nuôi
- Giảm mùi, khí độc gây hại, tăng tỉ lệ sống của vật nuôi
- Giảm vi sinh gây bệnh và mầm bệnh ảnh hưởng đến vật nuôi
- Giảm tần suất thay nước hồ, hạn chế rong tảo sinh sôi phát triển',N'- Xử lý chất hữu cơ, thức ăn dư thừa, chất thải của vật nuôi
- Hạn chế hình thành lớp bùn đáy, giữ cân bằng sinh thái cho ao nuôi
- Giảm mùi, khí độc gây hại, tăng tỉ lệ sống của vật nuôi
- Giảm vi sinh gây bệnh và mầm bệnh ảnh hưởng đến vật nuôi
- Giảm tần suất thay nước hồ, hạn chế rong tảo sinh sôi phát triển',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/bc9154a08642ebdd.jpg'),
(N'AQUA SC MARINE - VI SINH XỬ LÝ AO NUÔI TÔM NƯỚC MẶN',1,N'Mỹ',N'- Xử lý chất hữu cơ, thức ăn dư thừa, chất thải của vật nuôi
- Hạn chế hình thành lớp bùn đáy, giữ cân bằng sinh thái cho ao nuôi nước lợ/ nước mặn
- Giảm mùi, khí độc gây hại, tăng tỉ lệ sống của vật nuôi
- Giảm vi sinh gây bệnh và mầm bệnh ảnh hưởng đến vật nuôi',N'- Xử lý chất hữu cơ, thức ăn dư thừa, chất thải của vật nuôi
- Hạn chế hình thành lớp bùn đáy, giữ cân bằng sinh thái cho ao nuôi nước lợ/ nước mặn
- Giảm mùi, khí độc gây hại, tăng tỉ lệ sống của vật nuôi
- Giảm vi sinh gây bệnh và mầm bệnh ảnh hưởng đến vật nuôi',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/5252f95851006add.jpg'),
(N'AQUA FG - VI SINH TẨY NHỚT BẠT, RONG RÊU, MÀNG CHẤT BÉO AO NUÔI TRỒNG THỦY SẢN',1,N'Mỹ',N'- Tẩy sạch rong rêu, màng chất béo trong ao nuôi.
- Tẩy trắng bạt, lưới và nhá căn thức ăn
- Hấp thụ khí độc H2S, NH3 trong ao nuôi.
- Giảm ô nhiễm đáy ao, phân hủy thức ăn dư thừa, phân tôm cá ở đáy ao.
- Tạo thêm nguồn vi sinh có lợi cho ao nuôi.',N'- Tẩy sạch rong rêu, màng chất béo trong ao nuôi.
- Tẩy trắng bạt, lưới và nhá căn thức ăn
- Hấp thụ khí độc H2S, NH3 trong ao nuôi.
- Giảm ô nhiễm đáy ao, phân hủy thức ăn dư thừa, phân tôm cá ở đáy ao.
- Tạo thêm nguồn vi sinh có lợi cho ao nuôi.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/00cb02999e01bf7e.jpg'),
(N'AQUA SA - VI SINH XỬ LÝ BÙN ĐÁY AO NUÔI THỦY SẢN',1,N'Mỹ',N'- Xử lý bùn, chất hữu cơ tích tụ đáy, làm sạch môi trường đáy ao
- Ổn định môi trường nước, ngăn chặn tảo, mầm bệnh
- Giảm đáng kể sự hình thành các loại khí độc
- Hạn chế các bệnh đen mang, sưng mang, vàng mang, ....',N'- Xử lý bùn, chất hữu cơ tích tụ đáy, làm sạch môi trường đáy ao
- Ổn định môi trường nước, ngăn chặn tảo, mầm bệnh
- Giảm đáng kể sự hình thành các loại khí độc
- Hạn chế các bệnh đen mang, sưng mang, vàng mang, ....',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/a0203cb9e8bcd114.jpg'),
(N'BIOCLEAN ACF SC MARINE - VI SI XỬ LÝ NƯỚC THẢI KHÓ PHÂN HỦY CÓ ĐỘ MẶN CAO',1,N'Mỹ',N'- Cải thiện nhanh các trường hợp sốc tải khi nước thải có dấu hiệu nhiễm mặn.
- Làm giảm BOD, COD và TSS đầu ra.
- Nâng cao hiệu quả hoạt động của các hệ thống xử lý nước thải có nồng độ các chất xơ, bùn thải và các sợi cellulose cao.
- Phá vỡ các cellulose khó phân hủy.',N'- Cải thiện nhanh các trường hợp sốc tải khi nước thải có dấu hiệu nhiễm mặn.
- Làm giảm BOD, COD và TSS đầu ra.
- Nâng cao hiệu quả hoạt động của các hệ thống xử lý nước thải có nồng độ các chất xơ, bùn thải và các sợi cellulose cao.
- Phá vỡ các cellulose khó phân hủy.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/ed50ab9360f06d0a.jpg'),
(N'BIOCLEAN ACF SA - VI SINH PHÂN HỦY BÙN HỮU CƠ',1,N'Mỹ',N'- Bông bùn sinh học to, bùn dễ lắng
- Hạn chế và kiểm soát mùi hôi từ bùn phân hủy.
- Thúc đẩy nhanh quá trình phân hủy chất hữu cơ khó phân hủy.
- Tăng thời gian lưu bùn (SRT) sinh học, giảm thiểu bùn thải bỏ.
- Ứng dụng trong giảm thiểu bùn ao, hồ và hệ thống xử lý nước thải
- Giúp đạt tiêu chuẩn xả thải Quốc gia.',N'- Bông bùn sinh học to, bùn dễ lắng
- Hạn chế và kiểm soát mùi hôi từ bùn phân hủy.
- Thúc đẩy nhanh quá trình phân hủy chất hữu cơ khó phân hủy.
- Tăng thời gian lưu bùn (SRT) sinh học, giảm thiểu bùn thải bỏ.
- Ứng dụng trong giảm thiểu bùn ao, hồ và hệ thống xử lý nước thải
- Giúp đạt tiêu chuẩn xả thải Quốc gia.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/40317c8c2a727594.jpg'),
(N'BIOCLEAN ACF 0C - VI SINH XỬ LÝ MÙI HÔI, RÁC THẢI, NƯỚC THẢI',1,N'Mỹ',N'BÃI RÁC/ NƯỚC RỈ RÁC:
- Loại bỏ và kiểm soát mùi hôi.
- Giảm chi phí xử lý.
- Phân hủy các chất hữu cơ khó phân hủy.
- Kiểm soát côn trùng hiệu quả.

XỬ LÝ NƯỚC THẢI:
• Loại bỏ và kiểm soát mùi hôi.
• Giảm BOD, COD, SS & các chất hữu cơ.
• Tăng hiệu suất xử lý, giảm chi phí bảo trì.

AO, HỒ, CỐNG RÃNH Ô NHIỄM:
• Loại bỏ và kiểm soát mùi hôi.
• Giảm chi phí xử lý.
• Cải thiện độ trong của nước & cải thiện độ lắng.',N'BÃI RÁC/ NƯỚC RỈ RÁC:
- Loại bỏ và kiểm soát mùi hôi.
- Giảm chi phí xử lý.
- Phân hủy các chất hữu cơ khó phân hủy.
- Kiểm soát côn trùng hiệu quả.

XỬ LÝ NƯỚC THẢI:
• Loại bỏ và kiểm soát mùi hôi.
• Giảm BOD, COD, SS & các chất hữu cơ.
• Tăng hiệu suất xử lý, giảm chi phí bảo trì.

AO, HỒ, CỐNG RÃNH Ô NHIỄM:
• Loại bỏ và kiểm soát mùi hôi.
• Giảm chi phí xử lý.
• Cải thiện độ trong của nước & cải thiện độ lắng.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/e51a19e935794d82.jpg'),
(N'BIOCLEAN ACF  NITRO ACTIVATOR (ACF-NA) - Vi sinh xử lý Nito',1,N'Mỹ',N'Bioclean ACF-NITRO ACTIVATOR (tên mới)
Aquaclean NA (tên cũ)
là vi sinh XỬ LÝ NI TƠ TỔNG CAO, GIẢM AMONIA

- Là sản phẩm nâng cấp của Aquaclean N1 cũ.
- Công thức dạng bột, cải tiến mới nhất 2020.
*** Chuyên dùng xử lý ni tơ, tăng cường & đẩy nhanh quá trình nitrat hóa.

- Tăng cường và đẩy nhanh quá trình Nitrate hóa trong điều kiện hiếu khí.
- Tăng cường và thúc đẩy nhanh quá trình khử Nitrate hóa trong điều kiện thiếu khí.
- Giảm Nitơ tổng đầu ra trong hệ thống xử lý nước ',N'Bioclean ACF-NITRO ACTIVATOR (tên mới)
Aquaclean NA (tên cũ)
là vi sinh XỬ LÝ NI TƠ TỔNG CAO, GIẢM AMONIA

- Là sản phẩm nâng cấp của Aquaclean N1 cũ.
- Công thức dạng bột, cải tiến mới nhất 2020.
*** Chuyên dùng xử lý ni tơ, tăng cường & đẩy nhanh quá trình nitrat hóa.

- Tăng cường và đẩy nhanh quá trình Nitrate hóa trong điều kiện hiếu khí.
- Tăng cường và thúc đẩy nhanh quá trình khử Nitrate hóa trong điều kiện thiếu khí.
- Giảm Nitơ tổng đầu ra trong hệ thống xử lý nước thải.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/7ea98e821dedf365.jpg'),
(N'BIOCLEAN ACF AD ACTIVATOR - Vi sinh Phân Hủy Kỵ Khí',1,N'Mỹ',N'- Đạt tiêu chuẩn xả thải
- Tăng cường hiệu quả loại bỏ BOD/COD
- Giảm H2S
- Giảm hình thành bùn
- Hiệu chỉnh mùi hôi
- Thay đổi động sinh khối
- Cải thiện hiệu quả của hệ thống
- Giảm mùi tại nguồn xả thải
- Phân hủy rộng các hợp chất hữu cơ phức tạp bao gồm các vi khuẩn thiếu khí tùy nghi
- Giảm sinh vật hình sợi',N'- Đạt tiêu chuẩn xả thải
- Tăng cường hiệu quả loại bỏ BOD/COD
- Giảm H2S
- Giảm hình thành bùn
- Hiệu chỉnh mùi hôi
- Thay đổi động sinh khối
- Cải thiện hiệu quả của hệ thống
- Giảm mùi tại nguồn xả thải
- Phân hủy rộng các hợp chất hữu cơ phức tạp bao gồm các vi khuẩn thiếu khí tùy nghi
- Giảm sinh vật hình sợi',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/822b38849449eba6.jpg'),
(N'BIOCLEAN ACF-32 Vi sinh xử lý nước thải đa ngành',1,N'Mỹ',N'- VI SINH XỬ LÝ NƯỚC THẢI nhập khẩu CỦA MỸ
- Vi sinh hiện đại, vi sinh dạng lỏng
- Hiệu quả ổn định từ năm 1974
- Ứng dụng xử lý tất cả các loại nước thải công nghiệp: cao su, dệt nhuộm, chế biến thực phẩm, sản xuất bia, nước giải khát, dầu ăn...
- Giảm chỉ số nhu cầu oxy hóa COD, BOD và chất rắn lơ lửng SS
- Giảm thể tích bùn và các hợp chất khó phân hủy
- Cân bằng cộng đồng vi khuẩn hữu hiệu trong bùn hoạt tính.
- Làm giảm và điều chỉnh mùi của toàn hệ thống
- Hỗ trợ oxy hóa ',N'- VI SINH XỬ LÝ NƯỚC THẢI nhập khẩu CỦA MỸ
- Vi sinh hiện đại, vi sinh dạng lỏng
- Hiệu quả ổn định từ năm 1974
- Ứng dụng xử lý tất cả các loại nước thải công nghiệp: cao su, dệt nhuộm, chế biến thực phẩm, sản xuất bia, nước giải khát, dầu ăn...
- Giảm chỉ số nhu cầu oxy hóa COD, BOD và chất rắn lơ lửng SS
- Giảm thể tích bùn và các hợp chất khó phân hủy
- Cân bằng cộng đồng vi khuẩn hữu hiệu trong bùn hoạt tính.
- Làm giảm và điều chỉnh mùi của toàn hệ thống
- Hỗ trợ oxy hóa hợp chất chứa Ni tơ (quá trình Nitrate hóa).',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/51432495afdea4b9.jpg'),
(N'AQUACLEAN  ACF SC MARINE',1,N'Mỹ',N'Điểm nổi bật:
- Cải thiện nhanh các trường hợp sốc tải khi nước thải có dấu hiệu nhiễm mặn.
- Làm giảm BOD, COD và TSS đầu ra.
- Nâng cao hiệu quả hoạt động của các hệ thống xử lý nước thải có nồng độ các chất xơ, bùn thải và các sợi cellulose cao.
- Phá vỡ các cellulose khó phân hủy.',N'Điểm nổi bật:
- Cải thiện nhanh các trường hợp sốc tải khi nước thải có dấu hiệu nhiễm mặn.
- Làm giảm BOD, COD và TSS đầu ra.
- Nâng cao hiệu quả hoạt động của các hệ thống xử lý nước thải có nồng độ các chất xơ, bùn thải và các sợi cellulose cao.
- Phá vỡ các cellulose khó phân hủy.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/1a28866def00af32.png'),
(N'AQUACLEAN ACF SA',1,N'Mỹ',N'- Bông bùn sinh học to, bùn dễ lắng
- Hạn chế và kiểm soát mùi hôi từ bùn phân hủy.
- Thúc đẩy nhanh quá trình phân hủy chất hữu cơ khó phân hủy.
- Tăng thời gian lưu bùn (SRT) sinh học, giảm thiểu bùn thải bỏ.
- Ứng dụng trong giảm thiểu bùn ao, hồ và hệ thống xử lý nước thải
- Giúp đạt tiêu chuẩn xả thải Quốc gia.',N'- Bông bùn sinh học to, bùn dễ lắng
- Hạn chế và kiểm soát mùi hôi từ bùn phân hủy.
- Thúc đẩy nhanh quá trình phân hủy chất hữu cơ khó phân hủy.
- Tăng thời gian lưu bùn (SRT) sinh học, giảm thiểu bùn thải bỏ.
- Ứng dụng trong giảm thiểu bùn ao, hồ và hệ thống xử lý nước thải
- Giúp đạt tiêu chuẩn xả thải Quốc gia.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/3ef66d879528afb7.png'),
(N'Aquaclean OC',1,N'Mỹ',N'BÃI RÁC/ NƯỚC RỈ RÁC:
- Loại bỏ và kiểm soát mùi hôi.
- Giảm chi phí xử lý.
- Phân hủy các chất hữu cơ khó phân hủy.
- Kiểm soát côn trùng hiệu quả.

XỬ LÝ NƯỚC THẢI:
• Loại bỏ và kiểm soát mùi hôi.
• Giảm BOD, COD, SS & các chất hữu cơ.
• Tăng hiệu suất xử lý, giảm chi phí bảo trì.

AO, HỒ, CỐNG RÃNH Ô NHIỄM:
• Loại bỏ và kiểm soát mùi hôi.
• Giảm chi phí xử lý.
• Cải thiện độ trong của nước & cải thiện độ lắng.',N'BÃI RÁC/ NƯỚC RỈ RÁC:
- Loại bỏ và kiểm soát mùi hôi.
- Giảm chi phí xử lý.
- Phân hủy các chất hữu cơ khó phân hủy.
- Kiểm soát côn trùng hiệu quả.

XỬ LÝ NƯỚC THẢI:
• Loại bỏ và kiểm soát mùi hôi.
• Giảm BOD, COD, SS & các chất hữu cơ.
• Tăng hiệu suất xử lý, giảm chi phí bảo trì.

AO, HỒ, CỐNG RÃNH Ô NHIỄM:
• Loại bỏ và kiểm soát mùi hôi.
• Giảm chi phí xử lý.
• Cải thiện độ trong của nước & cải thiện độ lắng.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/b5994c9b67f48758.png'),
(N'Aquaclean AD',1,N'Mỹ',N'- Đạt tiêu chuẩn xả thải
- Tăng cường hiệu quả loại bỏ BOD/COD
- Giảm H2S
- Giảm hình thành bùn
- Hiệu chỉnh mùi hôi
- Thay đổi động sinh khối
- Cải thiện hiệu quả của hệ thống
- Giảm mùi tại nguồn xả thải
- Phân hủy rộng các hợp chất hữu cơ phức tạp bao gồm các vi khuẩn thiếu khí tùy nghi
- Giảm sinh vật hình sợi',N'- Đạt tiêu chuẩn xả thải
- Tăng cường hiệu quả loại bỏ BOD/COD
- Giảm H2S
- Giảm hình thành bùn
- Hiệu chỉnh mùi hôi
- Thay đổi động sinh khối
- Cải thiện hiệu quả của hệ thống
- Giảm mùi tại nguồn xả thải
- Phân hủy rộng các hợp chất hữu cơ phức tạp bao gồm các vi khuẩn thiếu khí tùy nghi
- Giảm sinh vật hình sợi',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/59896b9e19f6fa88.png'),
(N'Aquaclean NA',1,N'Mỹ',N'Bioclean ACF-NITRO ACTIVATOR (tên mới)
Aquaclean NA (tên cũ)
là vi sinh XỬ LÝ NI TƠ TỔNG CAO, GIẢM AMONIA

- Là sản phẩm nâng cấp của Aquaclean N1 cũ.
- Công thức dạng bột, cải tiến mới nhất 2020.
*** Chuyên dùng xử lý ni tơ, tăng cường & đẩy nhanh quá trình nitrat hóa.

- Tăng cường và đẩy nhanh quá trình Nitrate hóa trong điều kiện hiếu khí.
- Tăng cường và thúc đẩy nhanh quá trình khử Nitrate hóa trong điều kiện thiếu khí.
- Giảm Nitơ tổng đầu ra trong hệ thống xử lý nước ',N'Bioclean ACF-NITRO ACTIVATOR (tên mới)
Aquaclean NA (tên cũ)
là vi sinh XỬ LÝ NI TƠ TỔNG CAO, GIẢM AMONIA

- Là sản phẩm nâng cấp của Aquaclean N1 cũ.
- Công thức dạng bột, cải tiến mới nhất 2020.
*** Chuyên dùng xử lý ni tơ, tăng cường & đẩy nhanh quá trình nitrat hóa.

- Tăng cường và đẩy nhanh quá trình Nitrate hóa trong điều kiện hiếu khí.
- Tăng cường và thúc đẩy nhanh quá trình khử Nitrate hóa trong điều kiện thiếu khí.
- Giảm Nitơ tổng đầu ra trong hệ thống xử lý nước thải.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/eb56b9167b722aa3.png'),
(N'AQUACLEAN ACF-32',1,N'Mỹ',N'- VI SINH XỬ LÝ NƯỚC THẢI nhập khẩu CỦA MỸ
- Vi sinh hiện đại, vi sinh dạng lỏng
- Hiệu quả ổn định từ năm 1974
- Ứng dụng xử lý tất cả các loại nước thải công nghiệp: cao su, dệt nhuộm, chế biến thực phẩm, sản xuất bia, nước giải khát, dầu ăn...
- Giảm chỉ số nhu cầu oxy hóa COD, BOD và chất rắn lơ lửng SS
- Giảm thể tích bùn và các hợp chất khó phân hủy
- Cân bằng cộng đồng vi khuẩn hữu hiệu trong bùn hoạt tính.
- Làm giảm và điều chỉnh mùi của toàn hệ thống
- Hỗ trợ oxy hóa ',N'- VI SINH XỬ LÝ NƯỚC THẢI nhập khẩu CỦA MỸ
- Vi sinh hiện đại, vi sinh dạng lỏng
- Hiệu quả ổn định từ năm 1974
- Ứng dụng xử lý tất cả các loại nước thải công nghiệp: cao su, dệt nhuộm, chế biến thực phẩm, sản xuất bia, nước giải khát, dầu ăn...
- Giảm chỉ số nhu cầu oxy hóa COD, BOD và chất rắn lơ lửng SS
- Giảm thể tích bùn và các hợp chất khó phân hủy
- Cân bằng cộng đồng vi khuẩn hữu hiệu trong bùn hoạt tính.
- Làm giảm và điều chỉnh mùi của toàn hệ thống
- Hỗ trợ oxy hóa hợp chất chứa Ni tơ (quá trình Nitrate hóa).',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/de7baefdb4d3af33.png'),
(N'Chất phủ bóng cho giấy, PVC',1,N'Đài Loan',N'Công ty chúng tôi cung cấp các loại chất phủ bóng dùng trong ngành bao bì với giá cả rất cạnh tranh, chất lượng ổn định và phương thức giao dịch linh hoạt',N'Công ty chúng tôi cung cấp các loại chất phủ bóng dùng trong ngành bao bì với giá cả rất cạnh tranh, chất lượng ổn định và phương thức giao dịch linh hoạt',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li><li><strong>Bảo hành:</strong> 1 năm</li></ul>',N'/uploads/san-pham-catex/58f69da197f55c6e.jpg'),
(N'Bột màu hữu cơ cho sản xuất nhựa, mực in…',1,N'Đài Loan',N'Công ty chúng tôi cung cấp các loại Bột màu hữu cơ có nồng độ màu cao, độ bền ánh sáng tốt cho sản xuất nhựa, mực in… với giá cả rất cạnh tranh, chất lượng ổn định và phương thức giao dịch linh hoạt',N'Công ty chúng tôi cung cấp các loại Bột màu hữu cơ có nồng độ màu cao, độ bền ánh sáng tốt cho sản xuất nhựa, mực in… với giá cả rất cạnh tranh, chất lượng ổn định và phương thức giao dịch linh hoạt',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li><li><strong>Bảo hành:</strong> 1 năm</li></ul>',N'/uploads/san-pham-catex/00ab6613fd1eaecb.jpg'),
(N'Trợ chất cho nhuộm',1,N'Trung Quốc',N'Công ty chúng tôi cung cấp các loại trợ chất cho ngành nhuộm với giá cả rất cạnh tranh, chất lượng ổn định và phương thức giao dịch linh hoạt',N'Công ty chúng tôi cung cấp các loại trợ chất cho ngành nhuộm với giá cả rất cạnh tranh, chất lượng ổn định và phương thức giao dịch linh hoạt',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li><li><strong>Bảo hành:</strong> 1 năm</li></ul>',N'/uploads/san-pham-catex/191118b05b6af862.jpg'),
(N'Trợ chất cho in hấp',1,N'Trung Quốc',N'Công ty chúng tôi cung cấp các chất làm đặc (chướng in) dùng cho in vải bằng thuốc nhuộm hoạt tính và phân tán với giá cả rất cạnh tranh, chất lượng ổn định và phương thức giao dịch linh hoạt',N'Công ty chúng tôi cung cấp các chất làm đặc (chướng in) dùng cho in vải bằng thuốc nhuộm hoạt tính và phân tán với giá cả rất cạnh tranh, chất lượng ổn định và phương thức giao dịch linh hoạt',N'<ul><li><strong>Tình trạng:</strong> đã qua sử dụng</li><li><strong>Bảo hành:</strong> 1 năm</li></ul>',N'/uploads/san-pham-catex/ddce680b0e4ea412.jpg'),
(N'Màu dạng paste dùng cho in vải',1,N'Trung Quốc',N'Công ty chúng tôi cung cấp các loại màu in vải dạng paste có nồng độ màu cao, độ tươi sáng, độ hiện màu tốt, độ bền giặt cao, sử dụng thích hợp cho nhiều chất liệu vải',N'Công ty chúng tôi cung cấp các loại màu in vải dạng paste có nồng độ màu cao, độ tươi sáng, độ hiện màu tốt, độ bền giặt cao, sử dụng thích hợp cho nhiều chất liệu vải',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li><li><strong>Bảo hành:</strong> 1 năm</li></ul>',N'/uploads/san-pham-catex/bd2ad7edd273ea5c.jpg'),
(N'Nhũ tương cho sản xuất sơn gỗ hệ nước',1,N'Anh',N'Công ty chúng tôi cung cấp các loại nhựa nhũ tương Acrylic hệ nước cho sản xuất sơn gỗ tại thị trường Việt Nam với giá cả rất cạnh tranh, chất lượng ổn định và phương thức giao dịch linh hoạt',N'Công ty chúng tôi cung cấp các loại nhựa nhũ tương Acrylic hệ nước cho sản xuất sơn gỗ tại thị trường Việt Nam với giá cả rất cạnh tranh, chất lượng ổn định và phương thức giao dịch linh hoạt',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li><li><strong>Bảo hành:</strong> 1 năm</li></ul>',N'/uploads/san-pham-catex/2bbc6e3373054260.jpg'),
(N'Vi sinh Eco-Sept xử lý bể tự hoại',1,N'Canada',N'Vi sinh Eco-Sept (Powder) cải thiện sự hoạt động của vi sinh vật trong hệ thống bể tự hoại.',N'Vi sinh Eco-Sept (Powder) cải thiện sự hoạt động của vi sinh vật trong hệ thống bể tự hoại.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/faad8a3d81c49845.jpg'),
(N'BCP54/ MSBA100 loại trừ ô nhiễm bệnh trong ao nuôi thủy sản',1,N'Canada',N'BCP54/ MSBA100 loại trừ ô nhiễm bệnh trong ao nuôi thủy sản',N'BCP54/ MSBA100 loại trừ ô nhiễm bệnh trong ao nuôi thủy sản',N'<ul><li><strong>Tình trạng:</strong> đã qua sử dụng</li></ul>',N'/uploads/san-pham-catex/4882311d3d74d911.jpg'),
(N'Vi sinh BCP50 xử lý bùn hoạt tính đô thị',1,N'Canada',N'Vi sinh BCP50 là sản phẩm xử lý nước thải sinh hoạt, bùn hoạt tính đô thị.',N'Vi sinh BCP50 là sản phẩm xử lý nước thải sinh hoạt, bùn hoạt tính đô thị.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/f2023d5ba222fb01.jpg'),
(N'LƯU LƯỢNG KẾ (Rotametter acid)',2,N'Trung Quốc',N'Lưu lượng kế dùng để đo lưu lượng của 1 khối chất lỏng đi qua ống dẫn

Lưu lượng kế Vạn Nghĩa cung cấp sử dụng được cho cả acid.

- Xuất xứ: Trung Quốc

- Mọi thắc mắc khách hàng vui lòng gọi đến số 028 7300 9929( Ms. Minh) để biết thêm chi tiết',N'Lưu lượng kế dùng để đo lưu lượng của 1 khối chất lỏng đi qua ống dẫn

Lưu lượng kế Vạn Nghĩa cung cấp sử dụng được cho cả acid.

- Xuất xứ: Trung Quốc

- Mọi thắc mắc khách hàng vui lòng gọi đến số 028 7300 9929( Ms. Minh) để biết thêm chi tiết',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/505d8b786d7cd103.png'),
(N'TRỤC LÀM SẠCH TÔN',2,N'Trung Quốc',N'Vạn Nghĩa chuyên cung cấp trục làm sạch tôn. Hàng làm theo bản vẽ, nhập khẩu trực tiếp với giá cả cạnh tranh nhất trên thị trường. liên hệ: 028 7300 9929',N'Vạn Nghĩa chuyên cung cấp trục làm sạch tôn. Hàng làm theo bản vẽ, nhập khẩu trực tiếp với giá cả cạnh tranh nhất trên thị trường. liên hệ: 028 7300 9929',N'<ul><li><strong>Tình trạng:</strong> đã qua sử dụng</li></ul>',N'/uploads/san-pham-catex/0899246e7d446887.png'),
(N'BÉC PHUN (GIA CÔNG THEO BẢN VẼ) - SPRAY HEADER',2,N'Việt Nam',N'Vạn Nghĩa chuyên gia công Béc phun (SPRAY HEADER) theo bản vẽ Vật liệu: Nhựa PPH bọc thép. Ứng dụng: sử dụng tẩy rửa tôn trong dây chuyền Tẩy gỉ. Liên hệ: 028 7300 9929 để biết thêm thông tin.',N'Vạn Nghĩa chuyên gia công Béc phun (SPRAY HEADER) theo bản vẽ Vật liệu: Nhựa PPH bọc thép. Ứng dụng: sử dụng tẩy rửa tôn trong dây chuyền Tẩy gỉ. Liên hệ: 028 7300 9929 để biết thêm thông tin.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/776f9944fc0e80b0.png'),
(N'KHỚP NỐI XOAY (BHDTW025A - 010A.03.N727)',2,N'Trung Quốc',N'Vạn Nghĩa chuyên cung cấp Khớp nối xoay -mã hàng: BHDTW025A-010A.03.N727 được gia công theo bản vẽ thiết kế DANIELI, thuộc kiểu kết nối có ren - Xuất xứ: Trung Quốc',N'Vạn Nghĩa chuyên cung cấp Khớp nối xoay -mã hàng: BHDTW025A-010A.03.N727 được gia công theo bản vẽ thiết kế DANIELI, thuộc kiểu kết nối có ren - Xuất xứ: Trung Quốc',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/87c5790ae68ffb56.png'),
(N'BỘ TRAO ĐỔI NHIỆT - GRAPHITE HEAT EXCHANGER',2,N'Trung Quốc',N'Bộ trao đổi nhiệt là thiết bị được sử dụng để trao đổi nhiệt giữa một hay nhiều chất tải nhiệt. Những chất tải nhiệt có thể được ngăn cách bằng các tấm (plate) để ngăn sự pha trộn hoặc tiếp xúc trực tiếp giữa các chất tải nhiệt. Được sử dụng trong thiết bị sưởi ấm, nhà máy năng lượng, nhà máy hóa chất, nhà máy hóa dầu, nhà máy lọc dầu, khu chế tạo khí thiên nhiên, và xử lý chất thải',N'Bộ trao đổi nhiệt là thiết bị được sử dụng để trao đổi nhiệt giữa một hay nhiều chất tải nhiệt. Những chất tải nhiệt có thể được ngăn cách bằng các tấm (plate) để ngăn sự pha trộn hoặc tiếp xúc trực tiếp giữa các chất tải nhiệt. Được sử dụng trong thiết bị sưởi ấm, nhà máy năng lượng, nhà máy hóa chất, nhà máy hóa dầu, nhà máy lọc dầu, khu chế tạo khí thiên nhiên, và xử lý chất thải',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/e7a90c02d9c25cb0.png'),
(N'VAN MÀNG PPH',2,N'Trung Quốc',N'Van màng PPH là một loại Van màng PP đặc biệt, nó có đặc tính chống ăn mòn tốt nhất là chất lỏng axit hỗn hợp. Van màng PPH được sử dụng rộng rãi trong công nghiệp hóa học, đặc biệt là trong nhà máy luyện axit của nhà máy thép.',N'Van màng PPH là một loại Van màng PP đặc biệt, nó có đặc tính chống ăn mòn tốt nhất là chất lỏng axit hỗn hợp. Van màng PPH được sử dụng rộng rãi trong công nghiệp hóa học, đặc biệt là trong nhà máy luyện axit của nhà máy thép.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/38a986f098b56ec5.png'),
(N'VAN BI PVDF (MẶT BÍCH/SOCKET)',2,N'Tây Ban Nha',N'Van bi PVDF được sử dụng để kiểm soát dòng chảy và áp suất, ngắt các loại chất lỏng có tính ăn mòn, bùn, chất lỏng bình thường và khí. Được sử dụng trong các ngành công nghiệp dầu khí tự nhiên, nhưng cũng tìm thấy nhiều trong lĩnh vực sản xuất, lưu trữ hóa chất và thậm chí cả dân dụng.

Liên hệ hotline: 028 7300 9929 - 02873009929
để được hỗ trợ báo giá và tư vấn tốt nhất',N'Van bi PVDF được sử dụng để kiểm soát dòng chảy và áp suất, ngắt các loại chất lỏng có tính ăn mòn, bùn, chất lỏng bình thường và khí. Được sử dụng trong các ngành công nghiệp dầu khí tự nhiên, nhưng cũng tìm thấy nhiều trong lĩnh vực sản xuất, lưu trữ hóa chất và thậm chí cả dân dụng.

Liên hệ hotline: 028 7300 9929 - 02873009929
để được hỗ trợ báo giá và tư vấn tốt nhất',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/e68f15dc947b4ebb.png'),
(N'VAN BI PPH (MẶT BÍCH/SOCKET)',2,N'Trung Quốc',N'Van bi PPH được sử dụng để kiểm soát dòng chảy và áp suất, ngắt các loại chất lỏng có tính ăn mòn, bùn, chất lỏng bình thường và khí. Được sử dụng trong các ngành công nghiệp dầu khí tự nhiên, nhưng cũng tìm thấy nhiều trong lĩnh vực sản xuất, lưu trữ hóa chất và thậm chí cả dân dụng.

Liên hệ hotline: 028 7300 9929 - 02873009929
để được hỗ trợ báo giá và tư vấn tốt nhất',N'Van bi PPH được sử dụng để kiểm soát dòng chảy và áp suất, ngắt các loại chất lỏng có tính ăn mòn, bùn, chất lỏng bình thường và khí. Được sử dụng trong các ngành công nghiệp dầu khí tự nhiên, nhưng cũng tìm thấy nhiều trong lĩnh vực sản xuất, lưu trữ hóa chất và thậm chí cả dân dụng.

Liên hệ hotline: 028 7300 9929 - 02873009929
để được hỗ trợ báo giá và tư vấn tốt nhất',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/88590a505cac9d73.png'),
(N'ỐNG MỀM BỌC PFA/PTFE',2,N'Trung Quốc',N'Vạn nghĩa chuyên cung cấp các loại ống mềm bọc lưới PTFE (theo yêu cầu của khách hàng). Cam kết hàng đẹp - Chất lượng - Giá cả cạnh tranh. Hotline: 028 7300 9929',N'Vạn nghĩa chuyên cung cấp các loại ống mềm bọc lưới PTFE (theo yêu cầu của khách hàng). Cam kết hàng đẹp - Chất lượng - Giá cả cạnh tranh. Hotline: 028 7300 9929',N'<ul><li><strong>Tình trạng:</strong> đã qua sử dụng</li></ul>',N'/uploads/san-pham-catex/7d93765a81dea8a1.png'),
(N'VAN BƯỚM PP/PPH',2,N'Trung Quốc',N'Van bướm PPH là van ( Valve) được sử dụng để điều tiết ( hoặc dùng để đóng/mở đường ống) dòng chảy trong đường kính ống có kích thước lớn bằng cách cánh bướm xoay theo các góc độ khác nhau.',N'Van bướm PPH là van ( Valve) được sử dụng để điều tiết ( hoặc dùng để đóng/mở đường ống) dòng chảy trong đường kính ống có kích thước lớn bằng cách cánh bướm xoay theo các góc độ khác nhau.',N'<ul><li><strong>Tình trạng:</strong> hàng mới 100%</li></ul>',N'/uploads/san-pham-catex/41bd03c0cc4742d4.png');

DECLARE @BaseId INT = (SELECT ISNULL(MAX(ID),0)+1 FROM SanPhamCNTB);

INSERT INTO SanPhamCNTB
    (Name, ProductType, XuatXu, MoTaNgan, MoTa, ThongSo, QuyTrinhHinhAnh,
     Code, StatusId, LanguageId, SiteId, Created, Modified, Creator, OwnerType)
SELECT
    s.Name, s.ProductType, s.XuatXu, s.MoTaNgan, s.MoTa, s.ThongSo, s.QuyTrinhHinhAnh,
    CASE WHEN s.ProductType=1 THEN 'CN-' ELSE 'TB-' END + RIGHT('00000' + CAST(@BaseId + ROW_NUMBER() OVER (ORDER BY s.Name) - 1 AS VARCHAR(5)), 5),
    1, 1, 1, @Now, @Now, 'catex-import', NULL
FROM #CatexSeed s
WHERE NOT EXISTS (SELECT 1 FROM SanPhamCNTB p WHERE p.Name = s.Name AND p.ProductType = s.ProductType);

SELECT @@ROWCOUNT AS RowsInserted;

DROP TABLE #CatexSeed;
GO
