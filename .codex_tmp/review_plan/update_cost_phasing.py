from pathlib import Path
f=Path(r'D:\2026\CanTho\.codex_tmp\review_plan\build_revision.py')
s=f.read_text(encoding='utf-8')
repls=[
('Tháng 10 tập trung hoàn thành phần mềm, hạ tầng, các tích hợp sẵn sàng, vận hành thử trong phạm vi được thống nhất và bàn giao hồ sơ. Đề xuất chuyển việc quay dựng, tích hợp và kiểm tra VR360 sang tháng 11/2026.', 'Tháng 10 ưu tiên hoàn thiện phần mềm, dữ liệu, tài liệu và kiểm thử bằng tài nguyên hiện có được phép sử dụng. Đề xuất chuyển VR360 cùng các khoản mua mới bản quyền, thuê hoặc kích hoạt dịch vụ chưa có nguồn lực sang tháng 11/2026.'),
('Chốt ngày giao cloud, bản quyền, tài khoản dịch vụ, tên miền và cán bộ thử; thống nhất phân kỳ VR360 tháng 11 và phạm vi thử tháng 10.', 'Kiểm kê hạ tầng và quyền sử dụng hiện có; phân kỳ các khoản mua mới sang tháng 11; thống nhất môi trường và phạm vi thử tháng 10.'),
('Điều kiện để giữ mốc: bàn giao cloud trước hết ngày 22/10; cài đặt và chuyển dữ liệu xong 23/10; các tích hợp thuộc đợt tháng 10 và kiểm thử sẵn sàng xong 24/10. Việc vận hành thử từng phần khi chưa có VR360 cần được Chủ trì thuê thống nhất bằng văn bản.', 'Ngày 20-22/10 xác định môi trường hiện có và quyền sử dụng. Lịch thử 25-29/10 chỉ áp dụng cho phần đủ điều kiện, được Chủ trì thuê thống nhất. Cloud, bản quyền hoặc dịch vụ phải mua mới có thể chuyển tháng 11; khi đó tháng 10 không ghi đã bàn giao các hạng mục này.'),
("('20-22/10','Giao 02 cloud, bản quyền; cấu hình mạng, SSL/TLS, kho sao lưu riêng và giám sát.','MobiFone','Hạ tầng đủ điều kiện cài đặt'),", "('20-22/10','Kiểm kê hạ tầng, quyền dùng hệ điều hành/CSDL và sao lưu hiện có; xác định phần cần mua mới tháng 11.','MobiFone','Biên bản kiểm kê; phạm vi môi trường thử'),"),
("('20-23/10','Cấp email, SMS, ký số, hợp đồng điện tử; hoàn thiện hồ sơ và liên hệ đầu mối API bên ngoài.','MobiFone; Chủ trì thuê phối hợp','Tài khoản dịch vụ; biên nhận yêu cầu'),", "('20-23/10','Rà tài khoản dịch vụ hiện có; chuẩn bị API, hồ sơ đăng ký; dự kiến khoản mua hoặc kích hoạt tháng 11.','MobiFone; VTISC','Danh mục sẵn có/chưa có; hồ sơ chuẩn bị'),"),
('Cài ứng dụng, chuyển CSDL và tệp lên cloud sau khi nhận đủ hạ tầng; đối chiếu toàn vẹn.', 'Cài hoặc cập nhật ứng dụng trên môi trường hiện có đủ điều kiện; đối soát CSDL và tệp. Chuyển cloud mới bố trí tháng 11 nếu chưa có.'),
('Cấu hình phân quyền, tích hợp thật; kiểm thử chức năng, tải, ATTT và phục hồi trên cloud.', 'Kiểm thử phần mềm, phân quyền, ATTT, phục hồi trên môi trường hiện có; chỉ thử thật kết nối đã có quyền dùng.'),
('ghi VR360 chưa thực hiện và phạm vi cho phép thử.', 'ghi các hạng mục hoãn và phạm vi cho phép thử.'),
('ghi VR360 chưa thử, theo dõi và sửa lỗi.', 'ghi các kết nối chưa thử thật; theo dõi và sửa lỗi.'),
('Lập, hoàn thiện và bàn giao hồ sơ đề xuất cấp độ, phương án ATTT theo cấu hình thực tế.', 'Soạn hồ sơ ATTT theo hiện trạng và thiết kế; tháng 11 cập nhật cấu hình mới, hoàn thiện trước nghiệm thu.'),
("'Hồ sơ hoàn chỉnh và biên nhận bàn giao'),", "'Hồ sơ theo hiện trạng; danh mục cần cập nhật'),"),
('Kiểm tra đầu ra theo phạm vi đợt tháng 10 trên môi trường cung cấp dịch vụ. VR360 thuộc đợt tháng 11, ghi chưa thực hiện; không xác nhận toàn hệ thống đã hoàn chỉnh. Sau chuyển máy chủ phải kiểm thử lại.', 'Tháng 10 kiểm tra phần mềm trên môi trường hiện có được phép sử dụng; ghi riêng mua mới, cấp phép và tích hợp chưa kích hoạt. Bảng dưới nêu yêu cầu cuối cùng, không mặc định phải hoàn tất mọi dòng trong tháng 10. Sau bổ sung tháng 11 phải kiểm tra lại.'),
('Có cấu hình thực tế, vị trí lưu tại Việt Nam, quyền quản trị và chứng từ quyền sử dụng Windows Server Standard, SQL Server Standard đủ thời gian thuê.', 'Khi bàn giao phải có cấu hình, vị trí tại Việt Nam và quyền dùng Windows Server Standard, SQL Server Standard đủ thời gian thuê. Phần mua mới chưa có chuyển tháng 11; tháng 10 xác minh quyền dùng môi trường hiện có.'),
('SMS thương hiệu tạm chỉ ghi nhận là thử kỹ thuật.', 'Gói mới hoặc thương hiệu chưa kích hoạt chuyển tháng 11; tháng 10 chỉ thử bằng tài khoản được phép, tách rõ kết quả thử kỹ thuật.'),
('Kiểm tra quyền sử dụng dịch vụ và 2.000 hợp đồng/năm trong 05 năm.', 'Khi bàn giao kiểm tra quyền dùng và 2.000 hợp đồng/năm trong 05 năm. Phần chứng thư/gói mới chưa có chuyển tháng 11; tháng 10 không ghi đã ký số thật nếu chỉ dùng mô phỏng.'),
('tài khoản thật hoặc tình trạng chờ phải ghi rõ.', 'ghi rõ tài khoản thật, môi trường thử hoặc mô phỏng. Gói AI/API cần mua mới chuyển tháng 11; phần mềm kết nối có thể chuẩn bị trước.'),
('Rà soát tài khoản, phân quyền, SSL/TLS, lỗ hổng và kiểm thử lại sau khắc phục trên cloud. Có báo cáo sẵn sàng theo phạm vi, ghi VR360 chưa thực hiện, các tồn tại và xác nhận của Chủ trì thuê.', 'Rà tài khoản, phân quyền, SSL/TLS và lỗ hổng trên môi trường thử. Báo cáo ghi rõ phần hoãn, hạn chế môi trường và kết quả thực tế; tháng 11 kiểm tra lại trên hạ tầng được bàn giao.'),
('Chạy trọn luồng tại phần V Chương V:', 'Kịch bản nghiệm thu đầy đủ tại phần V Chương V:'),
('Đối chiếu quy trình 14 bước có và không có NDA theo hồ sơ đề xuất được chấp nhận.', 'Tháng 10 thử phần sẵn sàng; bước dùng dịch vụ chưa kích hoạt ghi chưa thử thật hoặc mô phỏng. Tháng 11 chạy lại trọn luồng và đối chiếu quy trình 14 bước theo hồ sơ được chấp nhận.'),
('nhóm triển lãm số và tích hợp VR360 ghi chưa thử, chuyển tháng 11.', 'VR360 và các kết nối trả phí chưa kích hoạt ghi chưa thử thật, chuyển tháng 11.'),
('ghi VR360 chưa thử. Đóng đợt thử', 'ghi VR360, hạ tầng và dịch vụ hoãn chưa được kiểm tra thật. Đóng đợt thử'),
('sau khi bổ sung VR360 tháng 11, thử lại các chức năng liên quan', 'sau khi bổ sung hạ tầng, bản quyền, dịch vụ và VR360 tháng 11, thử lại trọn luồng'),
("('Cloud và bản quyền','Mã hợp đồng, giá trị, phần được chấp nhận','Cấu hình, cấp phép, bàn giao'),", "('Cloud và bản quyền','Phần mua mới chưa có không tính tháng 10','Kiểm kê; chứng từ quyền dùng; bàn giao'),"),
("('Email và SMS','Thiết lập, quyền dùng và kỳ dịch vụ','Tài khoản; kiểm tra gửi thật'),", "('Email và SMS','Tách phần mềm và gói dịch vụ chưa cấp','Tài khoản; quyền dùng; kiểm tra gửi thật'),"),
("('Ký số và hợp đồng điện tử','Thiết lập, quyền dùng và kỳ dịch vụ','Chứng thư; PDF; nhật ký'),", "('Ký số và hợp đồng điện tử','Tách phần mềm và chứng thư/gói chưa cấp','Chứng thư; PDF; nhật ký thật'),"),
('gồm VR360, không vượt 15%', 'gồm VR360 và các khoản hoãn, không vượt 15%'),
('Biên bản giao cloud, bản quyền; cài đặt, dữ liệu; kiểm tra tích hợp; báo cáo kiểm thử và ATTT; báo cáo và biên bản sẵn sàng theo phạm vi tháng 10, ghi VR360 chưa làm. Hoàn tất trước đợt thử từng phần.', 'Biên bản môi trường hiện có, quyền sử dụng, cài đặt và dữ liệu; báo cáo kiểm thử, ATTT, sẵn sàng theo phạm vi tháng 10. Cloud/bản quyền mua mới chưa có ghi chưa bàn giao; bổ sung sau tháng 11.'),
('Cloud chậm phải tính lại mốc thử. VR360 bố trí tháng 11; nếu chưa thể chốt triển khai thì báo lại lịch, không mặc định đạt. Thử sơ bộ không thay kết quả trên môi trường hợp đồng.', 'Nếu môi trường hiện có thiếu quyền dùng hoặc năng lực thì chỉ chuẩn bị phần mềm/tài liệu, chưa tổ chức thử thật. Mua mới và VR360 bố trí tháng 11; thử lại khi đủ điều kiện.'),
('Với VR360 chuyển tháng 11, tháng 10 chỉ đề nghị xác nhận phần thực tế đạt theo cơ sở được chấp thuận.', 'Với các khoản mua mới và VR360 chuyển tháng 11, tháng 10 chỉ đề nghị xác nhận phần thực tế đạt theo cơ sở được chấp thuận.'),
("page();h('10 Kế hoạch bổ sung VR360 trong tháng 11')", "page();h('11 Kế hoạch bổ sung VR360 trong tháng 11')"),
('Báo cáo kiểm thử và vận hành thử ghi rõ VR360 chưa triển khai, chưa có kết quả thử.', 'Báo cáo tháng 10 ghi rõ VR360 và các dịch vụ hoãn chưa triển khai, chưa có kết quả thử thật.'),
('Bảng khối lượng thanh toán đợt 1 không cộng phần quay dựng, tích hợp VR360 chưa thực hiện.', 'Bảng thanh toán đợt 1 không cộng cloud, bản quyền, gói dịch vụ và VR360 chưa cung cấp; phần mềm kết nối được đối chiếu riêng, tránh tính trùng.'),
('Sau bàn giao VR360, cập nhật báo cáo thử, tài liệu, xác nhận hoàn tất các tồn tại', 'Sau bàn giao đầy đủ hạ tầng, bản quyền, dịch vụ và VR360, cập nhật báo cáo thử, tài liệu, xác nhận hoàn tất các tồn tại')
]
for a,b in repls:
 if a not in s:raise ValueError('Missing: '+a)
 s=s.replace(a,b)
section='''page();h('10 Phân kỳ bản quyền và các khoản chi phí')
p('Nguyên tắc đề xuất: các khoản phải mua mới hoặc kích hoạt có phí mà chưa sẵn sàng được xem xét thực hiện tháng 11. Dịch vụ đã có quyền sử dụng và đủ điều kiện vẫn dùng cho công việc tháng 10. Không chuyển toàn bộ công việc chỉ vì có liên quan đến chi phí; tách phần chuẩn bị phần mềm khỏi phần cung cấp tài nguyên hoặc quyền dùng.')
table(['Hạng mục','Tháng 10','Tháng 11 nếu chưa có'],[
('Windows Server và SQL Server','Kiểm kê phiên bản, mô hình và quyền dùng hiện có; chuẩn bị cấu hình, tài liệu.','Mua/cấp quyền dùng phù hợp, cấu hình và bàn giao chứng từ.'),
('02 cloud và sao lưu','Dùng môi trường hiện có đủ điều kiện; chuẩn bị gói chuyển đổi, phương án quay lui.','Thuê/bàn giao mới, chuyển dữ liệu, thử lại ATTT, hiệu năng và phục hồi.'),
('Email và SMS Brandname','Hoàn thiện bộ kết nối, hàng đợi và nhật ký; thử thật nếu đã có tài khoản được phép.','Kích hoạt gói, thương hiệu và hạn mức; thử gửi nhận, lỗi và gửi lại.'),
('Ký số và hợp đồng điện tử','Hoàn thiện luồng, giao diện, PDF và nhật ký; phân biệt thử thật với mô phỏng.','Cấp chứng thư/tài khoản/gói; ký thật, kiểm tra PDF, quyền dùng và hạn mức.'),
('AI và API có phí','Chuẩn bị kết nối, kiểm soát truy cập và xử lý lỗi; dùng tài khoản hiện có nếu được phép.','Kích hoạt gói/quota cần mua; thử thật và xác nhận kết quả.'),
('VR360 và khoản mua khác','VR360 chỉ chuẩn bị 10 vị trí và nội dung; kiểm kê nhu cầu SSL, giám sát hoặc công cụ khác.','Quay dựng VR360; khoản mua khác chỉ bổ sung khi thực sự cần và được thống nhất.')
],[3.8,6.8,6.8])
h('Cách chốt danh mục và lịch')
p('Ngày 20-22/10, MobiFone cùng VTISC lập danh sách từng khoản: đã có/chưa có, quyền sử dụng, bên cung cấp, nhu cầu thanh toán, thời gian cấp và ảnh hưởng kiểm thử. Chỉ xác nhận đã có khi kiểm tra được; không suy từ ứng dụng đang chạy rằng mọi licence hoặc tài khoản dịch vụ đều đầy đủ.')
p('Đề xuất thống nhất danh mục và nguồn lực trong đầu tháng 11; ngày mua, cấp và kích hoạt cụ thể phụ thuộc xác nhận của đầu mối và nhà cung cấp. Không hứa thời gian cấp licence, thương hiệu hay chứng thư khi chưa có xác nhận. Sau kích hoạt, cấu hình và kiểm thử lại trên môi trường bàn giao trước nghiệm thu toàn bộ.')
p('Các bước chuẩn bị hoàn thành tháng 10 chỉ được tính riêng khi có căn cứ hợp đồng và minh chứng. Dịch vụ chưa kích hoạt, quyền dùng chưa được cấp hoặc tài nguyên chưa bàn giao ghi chưa hoàn thành, kể cả khi bộ kết nối đã viết xong.')

'''
s=s.replace("page();h('11 Kế hoạch",section+"page();h('11 Kế hoạch")
f.write_text(s,encoding='utf-8')
r=Path(r'D:\2026\CanTho\docs\ke hoach thi cong, van hanh\Ra_soat_Codex\Ra_soat_ban_Claude_va_de_xuat_sua.md')
t=r.read_text(encoding='utf-8')
insert='''## Cập nhật phân kỳ các khoản mua mới và trả phí

Theo chỉ đạo bổ sung, rà soát để chuyển sang tháng 11 các khoản phải mua mới hoặc kích hoạt có phí chưa sẵn sàng: licence Windows Server/SQL Server, cloud và sao lưu mới, gói email/SMS, chứng thư ký số, hợp đồng điện tử, AI/API có phí và VR360. Không mặc định mọi khoản đều thiếu: cái đã có quyền dùng, đủ điều kiện thì tiếp tục sử dụng tháng 10.

Tháng 10 ưu tiên hoàn thiện phần mềm, cấu hình có thể chuẩn bị, dữ liệu, tài liệu, đào tạo và kiểm thử theo môi trường hiện có. Tách phần mềm kết nối khỏi dịch vụ mua ngoài. Thử mô phỏng hoặc sandbox phải ghi đúng loại, không thay kết quả ký/gửi/tích hợp thật. Nếu môi trường hiện có chưa đủ quyền dùng hoặc năng lực thì chỉ chuẩn bị, chưa coi là vận hành thử thực tế.

Không còn đặt việc mua/bàn giao mới 02 cloud và licence trước 22/10 làm tiền đề bắt buộc của lịch đề xuất. Ngày 20-22/10 chuyển thành kiểm kê điều kiện hiện có, lập danh mục khoản hoãn và xác định phạm vi thử được thống nhất. Khi chuyển lên cloud mới tháng 11 phải thử lại.

Mục tiêu 85% cần tính lại với **toàn bộ khoản hoãn**, không chỉ VR360. Chưa có bảng giá và điều kiện hợp đồng nên chưa thể kết luận đạt 85%. Bản kế hoạch bổ sung ma trận phân kỳ từng nhóm và giữ nguyên yêu cầu xác nhận theo thực tế. Các nhận xét về bản Claude bên dưới là lịch sử rà soát; phương án điều hành cập nhật này thay cho giả định mua mới trong tháng 10.

'''
t=t.replace('## Cập nhật theo quyết định phân kỳ VR360',insert+'## Cập nhật theo quyết định phân kỳ VR360',1)
t=t.replace('Lịch tháng 10 phụ thuộc cloud bàn giao trước hết 22/10, chuyển đổi xong 23/10, các tích hợp thuộc đợt này và kiểm thử xong 24/10.', 'Lịch tháng 10 dựa vào môi trường và quyền sử dụng hiện có được kiểm kê ngày 20-22/10; phần mua mới chuyển tháng 11 nếu chưa sẵn sàng.')
t=t.replace('“Anh đã cập nhật kế hoạch: tháng 10 tập trung phần mềm, hạ tầng, các tích hợp sẵn sàng, thử theo phạm vi thống nhất và hoàn thiện hồ sơ khối lượng; đề xuất quay dựng, tích hợp và bàn giao VR360 trong tháng 11.', '“Anh đã cập nhật kế hoạch: tháng 10 ưu tiên phần mềm, dữ liệu, tài liệu và kiểm thử bằng tài nguyên hiện có đủ điều kiện; đề xuất các khoản mua mới bản quyền, cloud, dịch vụ có phí chưa sẵn sàng và VR360 thực hiện tháng 11.')
t=t.replace('không cộng phần VR360 chưa làm.', 'không cộng các khoản mua mới, dịch vụ và VR360 chưa cung cấp.')
r.write_text(t,encoding='utf-8')
print('Updated cost and licence phasing')
