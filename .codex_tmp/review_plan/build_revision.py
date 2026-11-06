from pathlib import Path
from docx import Document
from docx.shared import Cm, Pt, RGBColor
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT

OUT=Path(r'D:\2026\CanTho\docs\ke hoach thi cong, van hanh\Ra_soat_Codex')
OUT.mkdir(exist_ok=True)
d=Document();sec=d.sections[0]
sec.page_height=Cm(29.7);sec.page_width=Cm(21)
sec.top_margin=Cm(1.7);sec.bottom_margin=Cm(1.6);sec.left_margin=Cm(1.8);sec.right_margin=Cm(1.8)
for name in ['Normal','Title','Subtitle','Heading 1','Heading 2']:
 s=d.styles[name];s.font.name='Arial';s.font.color.rgb=RGBColor(0,0,0)
 s.font.size=Pt(11 if name=='Normal' else 15 if name=='Title' else 12)
 s.paragraph_format.space_after=Pt(6)
d.styles['Normal'].paragraph_format.line_spacing=1.08
d.styles['Title'].font.bold=True
d.styles['Heading 1'].font.bold=True
d.styles['Heading 1'].paragraph_format.space_before=Pt(10)
foot=sec.footer.paragraphs[0];foot.alignment=2
foot.add_run('Kế hoạch triển khai tháng 10 năm 2026 | ')
fld=OxmlElement('w:fldSimple');fld.set(qn('w:instr'),'PAGE');foot._p.append(fld)
for r in foot.runs:r.font.size=Pt(8)
def p(s,bold=False):
 x=d.add_paragraph();r=x.add_run(s);r.bold=bold
 prev=x._p.getprevious()
 if prev is not None and prev.tag==qn('w:tbl'):x.paragraph_format.space_before=Pt(6)
 return x
def h(s):d.add_heading(s,1)
def page():d.add_page_break()
def table(headers,rows,widths):
 t=d.add_table(rows=1, cols=len(headers));t.alignment=WD_TABLE_ALIGNMENT.CENTER;t.autofit=False
 for c,w in zip(t.columns,widths):c.width=Cm(w)
 for i,s in enumerate(headers):t.rows[0].cells[i].text=s
 for row in rows:
  cells=t.add_row().cells
  for i,s in enumerate(row):cells[i].text=str(s)
 for ri,row in enumerate(t.rows):
  pr=row._tr.get_or_add_trPr();no=OxmlElement('w:cantSplit');pr.append(no)
  if ri==0:
   repeat=OxmlElement('w:tblHeader');pr.append(repeat)
  for ci,c in enumerate(row.cells):
   c.width=Cm(widths[ci]);c.vertical_alignment=WD_CELL_VERTICAL_ALIGNMENT.CENTER
   tcpr=c._tc.get_or_add_tcPr()
   sh=OxmlElement('w:shd');sh.set(qn('w:fill'),'DCE6F1' if ri==0 else ('F5F7F9' if ri%2 else 'FFFFFF'));tcpr.append(sh)
   borders=OxmlElement('w:tcBorders')
   for edge in ['top','left','bottom','right']:
    el=OxmlElement('w:'+edge);el.set(qn('w:val'),'single');el.set(qn('w:sz'),'4');el.set(qn('w:color'),'D9D9D9');borders.append(el)
   tcpr.append(borders)
   margins=OxmlElement('w:tcMar')
   for edge in ['top','left','bottom','right']:
    el=OxmlElement('w:'+edge);el.set(qn('w:w'),'75');el.set(qn('w:type'),'dxa');margins.append(el)
   tcpr.append(margins)
   for pa in c.paragraphs:
    pa.paragraph_format.space_after=Pt(2);pa.paragraph_format.space_before=Pt(2);pa.paragraph_format.line_spacing=1.0
    for r in pa.runs:r.font.size=Pt(10);r.bold=(ri==0)
 return t

d.add_paragraph('Kế hoạch thi công triển khai và vận hành thử', 'Title')
p('Sàn giao dịch công nghệ Cần Thơ\nMục tiêu xác nhận khối lượng phục vụ thanh toán lần 1 trong tháng 10 năm 2026',True)
h('1 Mục tiêu và nguyên tắc xác nhận')
p('Tháng 10 ưu tiên hoàn thiện phần mềm, dữ liệu, tài liệu và kiểm thử bằng tài nguyên hiện có được phép sử dụng. Đề xuất chuyển VR360 cùng các khoản mua mới bản quyền, thuê hoặc kích hoạt dịch vụ chưa có nguồn lực sang tháng 11/2026. Mục tiêu 85% chỉ được xác nhận khi có cơ sở tính theo hợp đồng, khối lượng thực tế được chấp nhận và đầy đủ minh chứng tương ứng.')
p('Ngày 30/10 là mốc mục tiêu hoàn thiện và nộp hồ sơ. Thời điểm thanh toán thực tế thực hiện theo điều kiện và quy trình tại hợp đồng; việc nộp hồ sơ không đồng nghĩa đã được thanh toán.')
h('2 Căn cứ và phạm vi')
p('Căn cứ kỹ thuật: Chương V Yêu cầu về kỹ thuật, mục I (trang PDF 1), III.2.1 (trang 3-7), III.2.2 và 2.3 (trang 14-15), III.2.7 (trang 25-26), III.2.11 và phần V (trang 30). Căn cứ tiến độ đề xuất: hồ sơ đề xuất kỹ thuật Gói 4 ngày 13/10/2026, mục 1.2, 3.2 và 8; phải đối chiếu bản được chấp thuận khi ký hợp đồng.')
p('Phạm vi gồm phần mềm và dữ liệu khởi tạo, 02 máy chủ cloud, bản quyền, các dịch vụ tích hợp, VR360, kiểm thử, vận hành thử, đào tạo và hồ sơ an toàn thông tin. Dịch vụ duy trì 05 năm được theo dõi riêng theo hợp đồng, không ghi nhận là đã cung cấp đủ chỉ vì hoàn tất thiết lập.')
h('3 Các nội dung thống nhất tại cuộc họp ngày 20 tháng 10')
table(['Nội dung','Kết quả phải chốt'],[
('Ngày hiệu lực hợp đồng','Xác định D0; tính hạn 20 ngày đưa vào vận hành thử và hạn 30 ngày hoàn thành theo cách tính của hợp đồng.'),
('Cơ sở 85%','Chốt mẫu số, danh mục công việc, đơn giá hoặc trọng số được chấp thuận; phân biệt tỷ lệ hoàn thành với tỷ lệ chi trả.'),
('Điều kiện kỹ thuật','Kiểm kê hạ tầng và quyền sử dụng hiện có; phân kỳ các khoản mua mới sang tháng 11; thống nhất môi trường và phạm vi thử tháng 10.'),
('Tiến độ tăng tốc','Thống nhất nguồn lực và thời lượng thử 25-29/10. Nếu phải giữ lịch 8 ngày như mục 8 hồ sơ đề xuất thì điều chỉnh lịch tương ứng.')
],[4.2,13.2])
p('Các ngày ở trang sau là lịch điều hành đề xuất. Không thay thế thời hạn hợp đồng hoặc tự cho phép lùi hạng mục sang tháng 11.',True)

page();h('4 Tiến độ triển khai đến ngày 30 tháng 10')
p('Ngày 20-22/10 xác định môi trường hiện có và quyền sử dụng. Lịch thử 25-29/10 chỉ áp dụng cho phần đủ điều kiện, được Chủ trì thuê thống nhất. Cloud, bản quyền hoặc dịch vụ phải mua mới có thể chuyển tháng 11; khi đó tháng 10 không ghi đã bàn giao các hạng mục này.')
table(['Mốc','Công việc và quan hệ phụ thuộc','Chủ trì','Đầu ra'],[
('20/10','Họp chốt phạm vi, D0, cơ sở 85%, phân công nhân sự và bộ kịch bản.','MobiFone và Chủ trì thuê','Biên bản họp; danh mục đầu vào'),
('20-22/10','Kiểm kê hạ tầng, quyền dùng hệ điều hành/CSDL và sao lưu hiện có; xác định phần cần mua mới tháng 11.','MobiFone','Biên bản kiểm kê; phạm vi môi trường thử'),
('20-23/10','Rà tài khoản dịch vụ hiện có; chuẩn bị API, hồ sơ đăng ký; dự kiến khoản mua hoặc kích hoạt tháng 11.','MobiFone; VTISC','Danh mục sẵn có/chưa có; hồ sơ chuẩn bị'),
('20-23/10','Kiểm kê dữ liệu và quyền sử dụng; chuẩn bị gói triển khai, kế hoạch chuyển đổi và quay lui.','VTISC','Danh mục dữ liệu; gói triển khai'),
('20-24/10','Chốt danh sách 10 điểm VR360 và phương án thực hiện tháng 11; tháng 10 chưa quay dựng.','MobiFone; tổ VR360','Danh sách cảnh; kế hoạch phân kỳ'),
('23/10','Cài hoặc cập nhật ứng dụng trên môi trường hiện có đủ điều kiện; đối soát CSDL và tệp. Chuyển cloud mới bố trí tháng 11 nếu chưa có.','VTISC; MobiFone','Biên bản cài đặt và đối soát'),
('23-24/10','Kiểm thử phần mềm, phân quyền, ATTT, phục hồi trên môi trường hiện có; chỉ thử thật kết nối đã có quyền dùng.','VTISC; MobiFone','Báo cáo kiểm thử; lỗi và kết quả thử lại'),
('25/10','Kiểm tra sẵn sàng phần triển khai tháng 10; ghi các hạng mục hoãn và phạm vi cho phép thử.','MobiFone; Chủ trì thuê','Biên bản sẵn sàng theo phạm vi'),
('25-29/10','Vận hành thử phạm vi tháng 10 đã được chấp nhận; ghi các kết nối chưa thử thật; theo dõi và sửa lỗi.','VTISC; cán bộ Chủ trì thuê','Nhật ký thử; minh chứng; xác nhận thử lại'),
('27-28/10','Đào tạo quản trị và nghiệp vụ trên phiên bản ổn định; bàn giao hướng dẫn.','VTISC','Biên bản; học viên; tài liệu'),
('20-29/10','Soạn hồ sơ ATTT theo hiện trạng và thiết kế; tháng 11 cập nhật cấu hình mới, hoàn thiện trước nghiệm thu.','MobiFone; VTISC','Hồ sơ theo hiện trạng; danh mục cần cập nhật'),
('30/10','Chốt kết quả thử, kiểm tra hồ sơ, xác nhận khối lượng đạt thực tế và nộp hồ sơ thanh toán.','MobiFone; Chủ trì thuê','Bảng xác nhận; hồ sơ theo hợp đồng')
],[2.1,8.1,3.1,4.1])
p('Dành ngày 31/10 để bổ sung hồ sơ nếu đầu mối tiếp nhận làm việc và hợp đồng cho phép. Đào tạo 27-28/10 chưa được dùng làm bằng chứng hoàn thành nếu phiên bản sau đó thay đổi nghiệp vụ trọng yếu.')

page();h('5 Điều kiện đầu ra và phạm vi vận hành thử')
p('Tháng 10 kiểm tra phần mềm trên môi trường hiện có được phép sử dụng; ghi riêng mua mới, cấp phép và tích hợp chưa kích hoạt. Bảng dưới nêu yêu cầu cuối cùng, không mặc định phải hoàn tất mọi dòng trong tháng 10. Sau bổ sung tháng 11 phải kiểm tra lại.')
table(['Nhóm đầu ra','Điều kiện chấp nhận và minh chứng'],[
('Hạ tầng và bản quyền','02 máy chủ, mỗi máy tối thiểu 8 Core, 32 GB RAM, 500 GB SSD, 01 IP; CPU theo yêu cầu HSMT hoặc tương đương. Khi bàn giao phải có cấu hình, vị trí tại Việt Nam và quyền dùng Windows Server Standard, SQL Server Standard đủ thời gian thuê. Phần mua mới chưa có chuyển tháng 11; tháng 10 xác minh quyền dùng môi trường hiện có.'),
('Ứng dụng và dữ liệu','Ghi phiên bản, tên miền, môi trường; đối chiếu số bản ghi theo nhóm, tệp đính kèm và quyền sử dụng; loại hoặc tách dữ liệu thử. Có chấp thuận dữ liệu dùng vận hành thử. Số liệu kiểm kê được xác nhận riêng, không mặc định dùng số trong bản kế hoạch cũ.'),
('Email và SMS','Email: 60 địa chỉ, 25 GB, 05 Mail List theo yêu cầu HSMT; SMS: 3.600 tin/năm trong 05 năm. Kiểm tra tài khoản, quyền sử dụng, gửi nhận thật, hàng đợi, ghi trạng thái, gửi lại và bật/tắt. Gói mới hoặc thương hiệu chưa kích hoạt chuyển tháng 11; tháng 10 chỉ thử bằng tài khoản được phép, tách rõ kết quả thử kỹ thuật.'),
('Chữ ký số và hợp đồng','Kiểm tra luồng ký từ xa, USB Token theo phạm vi cam kết; nhúng chữ ký vào PDF, xác minh chứng thư và toàn vẹn, tải tệp và nhật ký. OTP chỉ xác thực, không thay bằng chứng ký số. Khi bàn giao kiểm tra quyền dùng và 2.000 hợp đồng/năm trong 05 năm. Phần chứng thư/gói mới chưa có chuyển tháng 11; tháng 10 không ghi đã ký số thật nếu chỉ dùng mô phỏng.'),
('VR360 tháng 11','Tháng 10 chưa quay dựng, chưa nghiệm thu và không cộng khối lượng quay dựng. Tháng 11 hoàn thành 10 điểm mới, chuyển cảnh, xoay ảnh, nút truy cập; kiểm tra máy tính, điện thoại và bàn giao nội dung.'),
('AI và kết nối ngoài','Kiểm tra AI dựa trên dữ liệu Sàn, tìm kiếm và gợi ý, bật/tắt, không sửa dữ liệu nghiệp vụ. Mỗi kết nối có chiều dữ liệu, API, xác thực, nhật ký, gửi lại, xử lý lỗi, SLA và đầu mối; ghi rõ tài khoản thật, môi trường thử hoặc mô phỏng. Gói AI/API cần mua mới chuyển tháng 11; phần mềm kết nối có thể chuẩn bị trước.'),
('Sao lưu và giám sát','Sao lưu hằng ngày, giữ tối thiểu 30 ngày, bản sao tách biệt; dữ liệu và sao lưu lưu tại Việt Nam. Thử phục hồi CSDL và tệp; đo RPO/RTO theo mức được chấp thuận. Theo dõi CPU, RAM, ổ đĩa, website, sao lưu, hạn SSL và kiểm tra cảnh báo.'),
('An toàn và sẵn sàng','Rà tài khoản, phân quyền, SSL/TLS và lỗ hổng trên môi trường thử. Báo cáo ghi rõ phần hoãn, hạn chế môi trường và kết quả thực tế; tháng 11 kiểm tra lại trên hạ tầng được bàn giao.')
],[3.5,13.9])
p('Không suy ra “đã đạt” từ việc có mã nguồn, giao diện hoặc biểu mẫu. Các chức năng, dịch vụ và tài nguyên phải có kết quả kiểm tra thực tế tương ứng.',True)

page();h('6 Tổ chức vận hành thử và kiểm soát chất lượng')
p('Nhà thầu cùng Chủ trì thuê thống nhất người dùng đại diện, dữ liệu được phép, tài khoản theo vai trò, thiết bị, bộ kịch bản và tiêu chí đạt trước ngày bắt đầu. Mỗi kết quả ghi mã thử, thời điểm, phiên bản, người thực hiện, kết quả mong đợi và thực tế, ảnh hoặc nhật ký, lỗi, ngày sửa và kết quả thử lại.')
h('Kịch bản bắt buộc và phạm vi bổ sung')
p('Kịch bản nghiệm thu đầy đủ tại phần V Chương V: đăng ký nhà cung cấp → duyệt → đăng sản phẩm → đăng nhu cầu công nghệ → mời báo giá và nộp đề xuất → trao đổi trực tuyến → lập hợp đồng điện tử → ký số → thông báo email/SMS → nhật ký. Tháng 10 thử phần sẵn sàng; bước dùng dịch vụ chưa kích hoạt ghi chưa thử thật hoặc mô phỏng. Tháng 11 chạy lại trọn luồng và đối chiếu quy trình 14 bước theo hồ sơ được chấp nhận.')
table(['Phạm vi','Nội dung kiểm tra'],[
('12 nhóm chức năng','Quản trị hệ thống; tổ chức và thành viên; công nghệ và sản phẩm; cung cầu; sự kiện; cổng thông tin; triển lãm số; AI; tích hợp; dữ liệu và ATTT; hạ tầng vận hành; báo cáo thống kê. Lập ma trận từng yêu cầu HSMT; VR360 và các kết nối trả phí chưa kích hoạt ghi chưa thử thật, chuyển tháng 11.'),
('Tình huống lỗi','Sai quyền truy cập, hồ sơ bị từ chối, dữ liệu thiếu, dịch vụ ngoài gián đoạn, gửi lại thông báo, kiểm tra chữ ký không hợp lệ, bản PDF bị thay đổi; xác nhận nhật ký và xử lý đúng.'),
('Thiết bị và hiệu năng','Kiểm tra trình duyệt phổ biến, máy tính, máy tính bảng, điện thoại và IPv6 theo phạm vi yêu cầu. Chốt tải đồng thời và ngưỡng đo theo hợp đồng; các chỉ tiêu 200 người, p95 ≤ 3 giây, lỗi < 1% trong hồ sơ đề xuất chỉ áp dụng khi được chấp thuận.'),
('SLA và phục hồi','Giám sát mức sẵn sàng mục tiêu ≥ 99,5%/tháng; phản hồi ≤ 02 giờ trong giờ hành chính; xử lý sự cố nghiêm trọng ≤ 08 giờ, thông thường ≤ 24 giờ. Đo phục hồi, kiểm tra cảnh báo và xuất báo cáo hỗ trợ.'),
('Kết quả thử','Ghi tổng số ca đạt, không đạt, chưa chạy; thống kê lỗi, mức ảnh hưởng và bằng chứng thử lại; tổng hợp ý kiến người sử dụng. Không lấy vài ngày thử để kết luận đã đạt SLA cả tháng.')
],[3.5,13.9])
h('Điều kiện đóng vận hành thử và nghiệm thu')
p('Báo cáo thử tháng 10 chỉ kết luận trong phạm vi đã thực hiện, ghi VR360, hạ tầng và dịch vụ hoãn chưa được kiểm tra thật. Đóng đợt thử khi các ca thuộc phạm vi được thống nhất đạt, lỗi ảnh hưởng đã xử lý và thử lại, đơn vị sử dụng xác nhận. Nghiệm thu toàn bộ thực hiện sau khi bổ sung hạ tầng, bản quyền, dịch vụ và VR360 tháng 11, thử lại trọn luồng và đáp ứng đầy đủ HSMT, hợp đồng, hồ sơ ATTT.')
p('Lịch 25-29/10 bố trí 05 ngày thử, ngắn hơn khoảng ngày 20-27 (08 ngày theo lịch) tại mục 8 hồ sơ đề xuất ngày 13/10. HSMT không nêu số ngày thử tối thiểu riêng; nếu tiến độ hồ sơ đề xuất là cam kết hợp đồng thì phải thống nhất điều chỉnh hợp lệ. Nếu giữ 08 ngày và bắt đầu 25/10, kết thúc sớm nhất 01/11; không cam kết đóng hồ sơ thử trong tháng 10.')

page();h('7 Cách xác nhận mục tiêu khối lượng 85 phần trăm')
p('Trước khi thực hiện, hai bên phải xác định 85% tính trên toàn hợp đồng, phần thiết lập, giá trị kỳ thanh toán hay cơ sở khác. Chương V cung cấp yêu cầu kỹ thuật, chưa xác lập tỷ trọng thanh toán lần 1 là 85%. Không dùng tỷ trọng 3%, 15%, 8% trong bản kế hoạch cũ nếu chưa có căn cứ được chấp thuận.')
p('Nếu hợp đồng cho phép tính theo giá trị: tỷ lệ hoàn thành = tổng giá trị khối lượng được nghiệm thu trong phạm vi tính / tổng giá trị của chính phạm vi đó × 100%. Mỗi dòng có mã hạng mục, đơn vị, khối lượng hợp đồng, khối lượng đạt, đơn giá hoặc giá trị phân bổ được chấp thuận và minh chứng. Chỉ tính phần việc thực tế được chấp nhận; tránh ghi trùng cài đặt, cấu hình và kiểm thử.')
p('Nếu hợp đồng thanh toán theo mốc hoặc trọn gói: áp dụng điều kiện của mốc và cách phân bổ được chấp thuận. Không tự suy từ số đầu việc đã làm sang tỷ lệ tiền được trả. Giá trị đề nghị chi trả phải xử lý tạm ứng, thu hồi, giữ lại và các khoản khác theo hợp đồng nếu có.')
table(['Hạng mục cần đối chiếu','Cơ sở tính phải điền','Minh chứng tối thiểu'],[
('Cloud và bản quyền','Phần mua mới chưa có không tính tháng 10','Kiểm kê; chứng từ quyền dùng; bàn giao'),
('Ứng dụng và dữ liệu','Phạm vi chức năng, giá trị và kết quả','Cài đặt; kiểm kê; đối soát'),
('Email và SMS','Tách phần mềm và gói dịch vụ chưa cấp','Tài khoản; quyền dùng; kiểm tra gửi thật'),
('Ký số và hợp đồng điện tử','Tách phần mềm và chứng thư/gói chưa cấp','Chứng thư; PDF; nhật ký thật'),
('VR360 tháng 11','Tháng 10 không tính phần quay dựng chưa làm','Tháng 11 kiểm tra 10 điểm và bàn giao'),
('AI và API ngoài','Phạm vi kết nối; phần đạt/chưa đạt','Ma trận; log; xác nhận kiểm tra'),
('Kiểm thử và vận hành thử','Đầu ra; điều kiện; giá trị phân bổ','Báo cáo; xác nhận người dùng'),
('Đào tạo và tài liệu','Số buổi/nhóm; đầu ra theo hợp đồng','Học viên; biên bản; tài liệu'),
('Hồ sơ ATTT','Phạm vi bàn giao; điều kiện chấp nhận','Hồ sơ hoàn chỉnh; biên nhận'),
('Tổng cộng','Giá trị đạt / mẫu số thống nhất','Đối chiếu không trùng khối lượng')
],[4.1,6.5,6.8])
p('Bảng trên chưa xác nhận tỷ lệ hoặc giá trị. MobiFone tính lại theo hợp đồng; chỉ đạt mục tiêu 85% nếu tổng phần chưa đủ điều kiện, gồm VR360 và các khoản hoãn, không vượt 15% trên cùng cơ sở tính.',True)
h('Xử lý chênh lệch so với mục tiêu')
p('Ngày 28/10 tính thử khối lượng có minh chứng; ngày 30/10 chốt theo thực tế. Nếu dưới 85%, báo rõ số đạt, phần thiếu và ngày dự kiến hoàn thành; tăng nguồn lực cho phần có thể hoàn tất. Phần chưa đủ điều kiện không được cộng bằng tỷ trọng đề xuất để đạt mục tiêu.')

page();h('8 Hồ sơ bàn giao và đề nghị thanh toán')
p('Hồ sơ kỹ thuật dùng để chứng minh đầu ra; hồ sơ thanh toán áp dụng danh mục và điều kiện tại hợp đồng. Bộ biểu mẫu chỉ là công cụ ghi nhận, phải được điền kết quả thật và có người có thẩm quyền xác nhận.')
table(['Nhóm hồ sơ','Thành phần và thời điểm mục tiêu'],[
('Cài đặt và sẵn sàng','Biên bản môi trường hiện có, quyền sử dụng, cài đặt và dữ liệu; báo cáo kiểm thử, ATTT, sẵn sàng theo phạm vi tháng 10. Cloud/bản quyền mua mới chưa có ghi chưa bàn giao; bổ sung sau tháng 11.'),
('Vận hành thử và đào tạo','Nhật ký, báo cáo kết quả thử, lỗi và thử lại, xác nhận đơn vị sử dụng; biên bản đào tạo, học viên, hướng dẫn sử dụng và quản trị. Chốt trước hoặc trong ngày 30/10 nếu đạt.'),
('ATTT và tài liệu kỹ thuật','Hồ sơ đề xuất cấp độ, phương án ATTT tối thiểu cấp độ 2, cấu hình và sơ đồ thực tế; tài liệu API, ma trận tích hợp và biên nhận bàn giao. Hồ sơ hoàn chỉnh phải bàn giao trước nghiệm thu chính thức, theo ngoại lệ tại III.2.11.'),
('Khối lượng và thanh toán','Căn cứ hợp đồng/phụ lục, bảng xác nhận khối lượng và giá trị đạt, các biên bản liên quan; đề nghị thanh toán và chứng từ tài chính, hóa đơn, bảo lãnh nếu điều kiện hợp đồng yêu cầu. Có xác nhận tiếp nhận hồ sơ.')
],[4.1,13.3])
h('9 Trách nhiệm và xử lý chậm tiến độ')
p('MobiFone là đầu mối tổng hợp, chịu trách nhiệm nguồn cung cloud, bản quyền, dịch vụ bên ngoài, hồ sơ hợp đồng và thanh toán. VTISC phụ trách ứng dụng, dữ liệu, tích hợp phần mềm, kiểm thử và đào tạo theo phân công. Tổ VR360 phụ trách chụp dựng; Chủ trì thuê cung cấp đầu vào, phê duyệt dữ liệu, cử cán bộ thử và xác nhận kết quả. Chốt tên và số liên hệ từng đầu mối ngày 20/10.')
p('Báo cáo tiến độ hằng ngày trước 17 giờ: công việc đạt, minh chứng, việc chậm, ảnh hưởng mốc, người xử lý và hạn giải quyết. Riêng ngày 24/10 và 28/10 phải rà soát khả năng giữ mốc và khối lượng dự kiến đạt.')
table(['Tình huống','Cách xử lý và ảnh hưởng'],[
('Cloud chậm hoặc VR360 chưa sẵn sàng','Nếu môi trường hiện có thiếu quyền dùng hoặc năng lực thì chỉ chuẩn bị phần mềm/tài liệu, chưa tổ chức thử thật. Mua mới và VR360 bố trí tháng 11; thử lại khi đủ điều kiện.'),
('SMS, ký số, API chờ ngoài','Ghi phần đã chuẩn bị, biên nhận yêu cầu, phần chưa kiểm tra thật và lịch phối hợp. Không tự ghi đạt toàn bộ hoặc mặc định chuyển sang 15% tháng 11.'),
('Kết quả thử chưa đạt','Sửa lỗi, thử lại và tính lại khối lượng. Không nghiệm thu chính thức khi còn lỗi ảnh hưởng hoạt động hoặc chưa đáp ứng đủ yêu cầu.'),
('Phải kéo dài sang tháng 11','Đối chiếu D0 và hạn 30 ngày; thống nhất phạm vi, lịch xử lý và thủ tục điều chỉnh nếu cần. Việc phụ thuộc ngoài không tự làm thay đổi nghĩa vụ hoặc tỷ lệ thanh toán.')
],[4.1,13.3])
p('Với các khoản mua mới và VR360 chuyển tháng 11, tháng 10 chỉ đề nghị xác nhận phần thực tế đạt theo cơ sở được chấp thuận. Chưa đề nghị nghiệm thu hoàn thành toàn bộ. Ngày bắt đầu thời gian thuê 05 năm xác định theo nghiệm thu và hợp đồng.')

page();h('10 Phân kỳ bản quyền và các khoản chi phí')
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

page();h('11 Kế hoạch bổ sung VR360 trong tháng 11')
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
p('Báo cáo tháng 10 ghi rõ VR360 và các dịch vụ hoãn chưa triển khai, chưa có kết quả thử thật. Biên bản sẵn sàng chỉ kết luận cho phạm vi đã được thống nhất; nếu Chủ trì thuê yêu cầu toàn hệ thống hoàn chỉnh trước vận hành thử, đợt tháng 10 chỉ được ghi là kiểm thử hoặc thử sơ bộ.')
p('Bảng thanh toán đợt 1 không cộng cloud, bản quyền, gói dịch vụ và VR360 chưa cung cấp; phần mềm kết nối được đối chiếu riêng, tránh tính trùng. Khả năng đạt 85% phải tính lại cùng toàn bộ các phần còn thiếu theo giá trị hoặc trọng số đã được chấp thuận; không dùng tỷ trọng 8% do bản cũ tự đề xuất làm căn cứ.')
p('Sau bàn giao đầy đủ hạ tầng, bản quyền, dịch vụ và VR360, cập nhật báo cáo thử, tài liệu, xác nhận hoàn tất các tồn tại và đề nghị nghiệm thu toàn bộ khi đủ điều kiện. Nếu lịch tháng 11 vượt thời hạn hợp đồng thì xử lý điều chỉnh trước khi thực hiện theo tiến độ mới.')

path=OUT/'Ke_hoach_thi_cong_van_hanh_thu_T10_2026_de_xuat_dieu_chinh.docx'
for root in [d.element,d.styles.element]:
 for border in root.xpath('.//w:pBdr'):border.getparent().remove(border)
d.save(path)
print(path)
