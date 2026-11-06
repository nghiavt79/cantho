from pathlib import Path
f=Path(r'D:\2026\CanTho\.codex_tmp\review_plan\build_revision.py')
s=f.read_text(encoding='utf-8')
changes=[
('Tổ chức hoàn thành các đầu ra kỹ thuật, vận hành thử và bàn giao hồ sơ để Chủ trì thuê kiểm tra khối lượng trong tháng 10/2026.', 'Tháng 10 tập trung hoàn thành phần mềm, hạ tầng, các tích hợp sẵn sàng, vận hành thử trong phạm vi được thống nhất và bàn giao hồ sơ. Đề xuất chuyển việc quay dựng, tích hợp và kiểm tra VR360 sang tháng 11/2026.'),
('Cam kết ngày giao cloud, chứng từ bản quyền, tài khoản dịch vụ, tên miền, 10 điểm VR360 và người dùng tham gia.', 'Chốt ngày giao cloud, bản quyền, tài khoản dịch vụ, tên miền và cán bộ thử; thống nhất phân kỳ VR360 tháng 11 và phạm vi thử tháng 10.'),
('tích hợp, VR360 và kiểm thử sẵn sàng xong 24/10. Tổ VR360 phải xác nhận khả năng rút tiến độ ngay ngày 20/10.', 'các tích hợp thuộc đợt tháng 10 và kiểm thử sẵn sàng xong 24/10. Việc vận hành thử từng phần khi chưa có VR360 cần được Chủ trì thuê thống nhất bằng văn bản.'),
("('20-24/10','Chụp, dựng mới 10 điểm VR360, tích hợp và kiểm tra trên máy tính, điện thoại.','Tổ VR360; VTISC','Tour đủ 10 điểm và nội dung bàn giao'),", "('20-24/10','Chốt danh sách 10 điểm VR360 và phương án thực hiện tháng 11; tháng 10 chưa quay dựng.','MobiFone; tổ VR360','Danh sách cảnh; kế hoạch phân kỳ'),"),
('Kiểm tra điều kiện sẵn sàng sau khi các việc trên đạt; lập biên bản cho phép thử.', 'Kiểm tra sẵn sàng phần triển khai tháng 10; ghi VR360 chưa thực hiện và phạm vi cho phép thử.'),
("'Biên bản đủ điều kiện vận hành thử'),", "'Biên bản sẵn sàng theo phạm vi'),"),
('Vận hành thử trên môi trường đã được chấp nhận; chạy kịch bản, theo dõi và sửa lỗi.', 'Vận hành thử phạm vi tháng 10 đã được chấp nhận; ghi VR360 chưa thử, theo dõi và sửa lỗi.'),
("h('5 Điều kiện đầu ra trước khi vận hành thử')", "h('5 Điều kiện đầu ra và phạm vi vận hành thử')"),
('Kiểm tra từng đầu ra trên đúng môi trường cung cấp dịch vụ. Chạy thử sơ bộ trên hệ thống hiện có chỉ phục vụ chuẩn bị; khi chuyển máy chủ phải kiểm thử lại và xác nhận môi trường mới.', 'Kiểm tra đầu ra theo phạm vi đợt tháng 10 trên môi trường cung cấp dịch vụ. VR360 thuộc đợt tháng 11, ghi chưa thực hiện; không xác nhận toàn hệ thống đã hoàn chỉnh. Sau chuyển máy chủ phải kiểm thử lại.'),
("('VR360','Đủ 10 điểm cảnh quay dựng mới, chuyển cảnh, xoay ảnh, nút truy cập trên Sàn; dùng được trên trình duyệt máy tính và điện thoại; có danh mục nội dung bàn giao.'),", "('VR360 tháng 11','Tháng 10 chưa quay dựng, chưa nghiệm thu và không cộng khối lượng quay dựng. Tháng 11 hoàn thành 10 điểm mới, chuyển cảnh, xoay ảnh, nút truy cập; kiểm tra máy tính, điện thoại và bàn giao nội dung.'),"),
('Có báo cáo sẵn sàng, danh sách tồn tại, phạm vi được phép thử và xác nhận của Chủ trì thuê.', 'Có báo cáo sẵn sàng theo phạm vi, ghi VR360 chưa thực hiện, các tồn tại và xác nhận của Chủ trì thuê.'),
('Lập ma trận tới từng yêu cầu chi tiết HSMT, không chỉ ghi tên nhóm.', 'Lập ma trận từng yêu cầu HSMT; nhóm triển lãm số và tích hợp VR360 ghi chưa thử, chuyển tháng 11.'),
('Đề xuất chỉ đóng vận hành thử khi các ca bắt buộc đạt, lỗi ảnh hưởng hoạt động đã được xử lý và thử lại, báo cáo có xác nhận của đơn vị quản lý sử dụng. Nghiệm thu chính thức còn phải đáp ứng đầy đủ phạm vi HSMT và hợp đồng, vận hành ổn định, hồ sơ ATTT đã bàn giao và có biên bản nghiệm thu.', 'Báo cáo thử tháng 10 chỉ kết luận trong phạm vi đã thực hiện, ghi VR360 chưa thử. Đóng đợt thử khi các ca thuộc phạm vi được thống nhất đạt, lỗi ảnh hưởng đã xử lý và thử lại, đơn vị sử dụng xác nhận. Nghiệm thu toàn bộ thực hiện sau khi bổ sung VR360 tháng 11, thử lại các chức năng liên quan và đáp ứng đầy đủ HSMT, hợp đồng, hồ sơ ATTT.'),
("('VR360','10 điểm, giá trị và số điểm đạt','Danh mục cảnh; tour; bàn giao'),", "('VR360 tháng 11','Tháng 10 không tính phần quay dựng chưa làm','Tháng 11 kiểm tra 10 điểm và bàn giao'),"),
('Bảng trên là danh mục đối chiếu, chưa xác nhận tỷ lệ hoặc giá trị. MobiFone lập bảng tính chi tiết theo hợp đồng để Chủ trì thuê kiểm tra trước khi ký.', 'Bảng trên chưa xác nhận tỷ lệ hoặc giá trị. MobiFone tính lại theo hợp đồng; chỉ đạt mục tiêu 85% nếu tổng phần chưa đủ điều kiện, gồm VR360, không vượt 15% trên cùng cơ sở tính.'),
('báo cáo sẵn sàng; biên bản đủ điều kiện thử. Hoàn tất trước khi bắt đầu thử.', 'báo cáo và biên bản sẵn sàng theo phạm vi tháng 10, ghi VR360 chưa làm. Hoàn tất trước đợt thử từng phần.'),
("('Cloud hoặc VR360 chậm','Điều phối nguồn lực và báo mốc mới ngay; kiểm tra lại sau bàn giao. Thử sơ bộ trên mekongx.vn không tự thay thế kết quả trên môi trường hợp đồng.'),", "('Cloud chậm hoặc VR360 chưa sẵn sàng','Cloud chậm phải tính lại mốc thử. VR360 bố trí tháng 11; nếu chưa thể chốt triển khai thì báo lại lịch, không mặc định đạt. Thử sơ bộ không thay kết quả trên môi trường hợp đồng.'),"),
('Nghiệm thu chính thức có thể thực hiện trong tháng 10 nếu đủ điều kiện; không dành sẵn 5% hoặc bắt buộc chuyển nghiệm thu sang tháng 11. Ngày bắt đầu thời gian thuê 05 năm xác định theo nghiệm thu và hợp đồng.', 'Với VR360 chuyển tháng 11, tháng 10 chỉ đề nghị xác nhận phần thực tế đạt theo cơ sở được chấp thuận. Chưa đề nghị nghiệm thu hoàn thành toàn bộ. Ngày bắt đầu thời gian thuê 05 năm xác định theo nghiệm thu và hợp đồng.')
]
for old,new in changes:
 if old not in s:raise ValueError('Missing replacement: '+old)
 s=s.replace(old,new)
nov='''page();h('10 Kế hoạch bổ sung VR360 trong tháng 11')
p('Đề xuất phân kỳ quay dựng 10 điểm VR360 sang tháng 11/2026. MobiFone phối hợp Chủ trì thuê thống nhất phạm vi, thời hạn và ảnh hưởng đến nghiệm thu, thanh toán; đối chiếu hạn hợp đồng để thực hiện thủ tục điều chỉnh nếu cần. Việc đưa vào kế hoạch chưa thay thế văn bản chấp thuận.')
p('Tháng 10 chỉ chuẩn bị danh sách điểm cảnh, nội dung giới thiệu, lịch bố trí mặt bằng, yêu cầu bàn giao và phương án tích hợp. Phần chuẩn bị chỉ được ghi nhận riêng nếu hợp đồng cho phép và có minh chứng; không quy thành khối lượng quay dựng đã hoàn thành.')
table(['Mốc đề xuất','Công việc','Đầu ra'],[
('Trước ngày quay','Chốt đơn vị thực hiện, điều kiện triển khai, 10 vị trí và lịch phối hợp.','Kế hoạch quay dựng được xác nhận'),
('Ngày N đến N+1','Chụp mới 10 điểm tại không gian trưng bày, kiểm tra chất lượng ảnh gốc.','Danh mục ảnh và biên bản hiện trường'),
('N+2 đến N+5','Xử lý ảnh, dựng tour, chuyển cảnh và nội dung giới thiệu.','Tour dự thảo đủ 10 điểm'),
('N+6 đến N+7','Tích hợp nút tham quan; thử máy tính, điện thoại; thử lại nghiệp vụ liên quan.','Kết quả kiểm thử và danh sách chỉnh sửa'),
('N+8 đến N+9','Hoàn thiện, đào tạo bổ sung phần VR360 và bàn giao nội dung.','Tour hoàn chỉnh; tài liệu và biên bản bàn giao')
],[3.1,8.3,6.0])
p('N là ngày bắt đầu quay được xác nhận trong tháng 11 sau khi đủ điều kiện thực hiện. Lịch N đến N+9 là phương án điều hành đề xuất, cần đơn vị quay dựng xác nhận. Muốn hoàn tất trong tháng 11 phải bố trí N và thời gian dự phòng phù hợp; chưa chốt ngày cụ thể khi điều kiện triển khai chưa sẵn sàng.')
h('Ảnh hưởng đến hồ sơ tháng 10')
p('Báo cáo kiểm thử và vận hành thử ghi rõ VR360 chưa triển khai, chưa có kết quả thử. Biên bản sẵn sàng chỉ kết luận cho phạm vi đã được thống nhất; nếu Chủ trì thuê yêu cầu toàn hệ thống hoàn chỉnh trước vận hành thử, đợt tháng 10 chỉ được ghi là kiểm thử hoặc thử sơ bộ.')
p('Bảng khối lượng thanh toán đợt 1 không cộng phần quay dựng, tích hợp VR360 chưa thực hiện. Khả năng đạt 85% phải tính lại cùng toàn bộ các phần còn thiếu theo giá trị hoặc trọng số đã được chấp thuận; không dùng tỷ trọng 8% do bản cũ tự đề xuất làm căn cứ.')
p('Sau bàn giao VR360, cập nhật báo cáo thử, tài liệu, xác nhận hoàn tất các tồn tại và đề nghị nghiệm thu toàn bộ khi đủ điều kiện. Nếu lịch tháng 11 vượt thời hạn hợp đồng thì xử lý điều chỉnh trước khi thực hiện theo tiến độ mới.')

'''
s=s.replace("path=OUT/'Ke_hoach",nov+"path=OUT/'Ke_hoach")
f.write_text(s,encoding='utf-8')

r=Path(r'D:\2026\CanTho\docs\ke hoach thi cong, van hanh\Ra_soat_Codex\Ra_soat_ban_Claude_va_de_xuat_sua.md')
t=r.read_text(encoding='utf-8')
t=t.replace('## Kết luận','''## Cập nhật theo quyết định phân kỳ VR360

Người phụ trách xác nhận chuyển quay dựng VR360 sang tháng 11 vì chưa bố trí được kinh phí. Đây là thông tin điều hành nội bộ; bản kế hoạch đề xuất phân kỳ triển khai, không ghi thành chậm do cơ quan bên ngoài hoặc đã được khách chấp thuận.

- Tháng 10 không cộng khối lượng quay dựng, tích hợp VR360 chưa thực hiện. Phần chuẩn bị chỉ xét riêng khi có căn cứ hợp đồng và minh chứng.
- Khối lượng còn thiếu gồm VR360 và các việc khác phải được tính lại: mục tiêu 85% chỉ khả thi khi phần chưa đủ điều kiện không vượt 15% theo cùng cơ sở được chấp thuận. Chưa có đơn giá hoặc trọng số nên chưa kết luận đạt được.
- Thử tháng 10 theo phạm vi được thống nhất, ghi rõ VR360 chưa thử; không xác nhận toàn bộ hệ thống hoàn chỉnh. Nếu khách yêu cầu đủ toàn hệ thống mới thử, chỉ ghi nhận kiểm thử hoặc thử sơ bộ trong tháng 10.
- Tháng 11 quay dựng đủ 10 điểm, tích hợp, kiểm thử, bàn giao và đào tạo bổ sung; sau đó mới xem xét nghiệm thu toàn bộ khi đủ điều kiện.
- Cần thống nhất với Chủ trì thuê và đối chiếu thời hạn hợp đồng. Quyết định phân kỳ nội bộ chưa thay thế việc chấp thuận tiến độ và điều kiện thanh toán.

## Kết luận''',1)
t=t.replace('Lịch này phụ thuộc cloud bàn giao trước hết 22/10, chuyển đổi xong 23/10, VR360 và kiểm thử xong 24/10; cần nhà cung cấp xác nhận năng lực tăng tốc.', 'Lịch tháng 10 phụ thuộc cloud bàn giao trước hết 22/10, chuyển đổi xong 23/10, các tích hợp thuộc đợt này và kiểm thử xong 24/10. VR360 chuyển tháng 11 theo quyết định cập nhật; việc thử từng phần cần được Chủ trì thuê thống nhất.')
t=t.replace('Không xác nhận trước 85%, không tự lùi nghiệm thu sang tháng 11.', 'Không xác nhận trước 85%; với VR360 chuyển tháng 11, chưa đề nghị nghiệm thu hoàn thành toàn bộ trong tháng 10.')
start=t.index('“Anh đã rà soát kế hoạch theo Chương V.')
end=t.index('”',start)+1
t=t[:start]+'“Anh đã cập nhật kế hoạch: tháng 10 tập trung phần mềm, hạ tầng, các tích hợp sẵn sàng, thử theo phạm vi thống nhất và hoàn thiện hồ sơ khối lượng; đề xuất quay dựng, tích hợp và bàn giao VR360 trong tháng 11. Team cần tính lại phần đạt thực tế theo hợp đồng để kiểm tra mục tiêu 85%, không cộng phần VR360 chưa làm. Đề nghị MobiFone phối hợp Chủ trì thuê thống nhất phân kỳ, phạm vi thử, thời hạn hợp đồng và điều kiện thanh toán đợt 1 trước khi chốt kế hoạch.”'+t[end:]
r.write_text(t,encoding='utf-8')
print('Updated authoring source and internal review for November VR360')
