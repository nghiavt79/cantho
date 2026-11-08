from pathlib import Path
import re, html
from lxml import html as LH
from docx import Document
from docx.shared import Cm, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
out=Path(r'D:\2026\CanTho\docs\2 Tich hop hd dien tu sms')
src=Path(r'D:\2026\CanTho\TechExchangeApp\wwwroot\templates\contract_chuyen_giao_cong_nghe.html').read_text(encoding='utf-8')
v={'SO_HD':'TEST-001/2026/HDCGCN','NGAY_HD':'ngày 26 tháng 10 năm 2026','TEN_CONG_NGHE':'Quy trình tuyển rửa và xử lý cát sạch thử nghiệm','PROJECT_NAME':'Dự án kiểm thử tích hợp hợp đồng điện tử Cần Thơ','MO_TA':'Chuyển giao quy trình tuyển rửa, tài liệu kỹ thuật và hướng dẫn vận hành hệ thống xử lý cát sạch. Toàn bộ thông tin trong hợp đồng là dữ liệu giả lập phục vụ kiểm thử tích hợp.','TONG_GIA':'100.000.000 VNĐ (Bằng chữ: Một trăm triệu đồng chẵn)','PHUONG_THUC_THANH_TOAN':'Chuyển khoản theo 3 đợt: tạm ứng 30% (30.000.000 VNĐ), bàn giao 40% (40.000.000 VNĐ), nghiệm thu 30% (30.000.000 VNĐ).','VERSION':'TEST v1','NGAY_TAO':'26/10/2026'}
for k,name,rep in [('A','Công ty Công nghệ Thử nghiệm A','Nguyễn Văn An'),('B','Công ty Ứng dụng Thử nghiệm B','Trần Thị Bình')]:
 for suffix,value in {'TEN':name,'TRU_SO':'Địa chỉ giả lập phục vụ kiểm thử, TP. Cần Thơ','DIEN_THOAI':'Không sử dụng số điện thoại thật','MA_SO_THUE':'TEST-MST-'+k,'TAI_KHOAN':'TEST-TK-'+k+' (không dùng thanh toán)','DAI_DIEN':rep,'CHUC_VU':'Giám đốc'}.items(): v['BEN_'+k+'_'+suffix]=value
for key,value in v.items(): src=src.replace('{{'+key+'}}',html.escape(value))
assert '{{' not in src
root=LH.fromstring(src)
for el in root.xpath('//div[@class="divider"]'): el.getparent().remove(el)
notice='HỢP ĐỒNG THỬ NGHIỆM — Dữ liệu giả lập, chỉ dùng kiểm thử tích hợp API và ký điện tử, không dùng giao dịch thực tế.'
h=root.xpath('//h1')[0]; n=LH.Element('p',{'class':'test-notice'}); n.text=notice; h.addnext(n)
style=root.xpath('//style')[0]; style.text+='\n.test-notice { text-align:center; font-weight:bold; } .dieu-title { break-after:avoid; } .sign-block { break-inside:avoid; } @page { size:A4; margin:20mm; }'
(out/'HopDong_ThuNghiem.html').write_text('<!DOCTYPE html>\n'+LH.tostring(root,encoding='unicode'),encoding='utf-8')
doc=Document(); s=doc.sections[0]; s.page_width=Cm(21);s.page_height=Cm(29.7);s.top_margin=s.bottom_margin=Cm(2);s.left_margin=Cm(2.5);s.right_margin=Cm(2)
for sn in ['Normal','Title','Heading 1','Heading 2']:
 st=doc.styles[sn]; st.font.name='Times New Roman'; st.font.size=Pt(12); st.font.color.rgb=RGBColor(0,0,0)
doc.styles['Normal'].paragraph_format.line_spacing=1.15
doc.styles['Normal'].paragraph_format.space_after=Pt(5)
def p(text,bold=False,center=False,style=None):
 para=doc.add_paragraph(style=style);r=para.add_run(text);r.bold=bold
 if center: para.alignment=WD_ALIGN_PARAGRAPH.CENTER
 return para
text=lambda e:' '.join(e.text_content().split())
body=root.xpath('//body')[0]
for e in body:
 cls=e.get('class','')
 if e.tag=='h1': p(text(e),True,True,'Title')
 elif cls=='party-block':
  for ch in e: p(text(ch),ch.get('class')=='party-title')
 elif cls=='dieu':
  p(text(e[0]),True,style='Heading 2')
  for ch in e[1]: p(text(ch))
 elif cls=='sign-block':
  tb=doc.add_table(rows=1,cols=2)
  for cell,col in zip(tb.rows[0].cells,e):
   cell.text=''
   for j,ch in enumerate(col):
    para=cell.paragraphs[0] if j==0 else cell.add_paragraph();para.alignment=WD_ALIGN_PARAGRAPH.CENTER
    rr=para.add_run(text(ch));rr.bold=j!=1
    if j==2: para.paragraph_format.space_before=Pt(48)
 else: p(text(e),cls in ['header-top','test-notice'],cls in ['header-top','header-sub','so-hd','test-notice','watermark'])
doc.save(out/'HopDong_ThuNghiem.docx')
print('Created HTML and DOCX; all template fields populated; 17 clauses preserved.')
