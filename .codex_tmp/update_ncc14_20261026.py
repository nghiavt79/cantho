from pathlib import Path
from copy import deepcopy
import shutil,json
from docx import Document
from docx.shared import Pt,Cm
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT
base=Path(r'D:\2026\CanTho\docs\ke hoach thi cong, van hanh')
tmp=Path(r'D:\2026\CanTho\.codex_tmp\ncc14_20261026');tmp.mkdir(exist_ok=True)
files=list(base.rglob('NCC-14*.docx'))
assert len(files)==2
rows=[
 ['1','Phần mềm\nGói 25/10/2026','25/10/2026','Cập nhật bộ kết nối SMS South Telecom ở chế độ thử; bổ sung nhật ký đăng nhập, log hệ thống theo ngày, thống kê lượt truy cập và thời gian phản hồi hỗ trợ. Chưa xác nhận gửi SMS thật.','Nguyễn Thị Dung; Nguyễn Nhĩ Thái\nKiểm thử: Lê Phước Trung'],
 ['2','Phần mềm\nCập nhật 26/10/2026','26/10/2026','Sửa API-05 gợi ý nhà cung cấp; chức năng chấm điểm của chuyên gia ở bước 4; hiển thị ký OTP nghiệm thu ở bước 13. Cập nhật theo tin nhắn báo cáo triển khai ngày 26/10/2026.','Nguyễn Nhĩ Thái; Nguyễn Thị Dung\nKiểm thử: Lê Phước Trung'],
 ['3','Tài liệu NCC-14\nPhiên bản 2.0','26/10/2026','Cập nhật quy trình vận hành, hỗ trợ, xử lý sự cố, sao lưu và quản lý thay đổi. Bổ sung sổ phiên bản phần mềm và tài liệu, đối chiếu các bản cập nhật 25-26/10/2026.','Nguyễn Thị Dung\nPhối hợp hạ tầng/ATTT: Bùi Nguyễn Hoàng Thái']
]
for i,f in enumerate(files,1):
 if not (tmp/f'original_{i}.docx').exists():shutil.copy2(f,tmp/f'original_{i}.docx')
 d=Document(f)
 tables=[t for t in d.tables if len(t.columns)==5 and 'Phiên bản' in ' '.join(c.text for c in t.rows[0].cells)]
 assert len(tables)==1
 t=tables[0];template=deepcopy(t.rows[1]._tr)
 while len(t.rows)>1:t._tbl.remove(t.rows[-1]._tr)
 headers=['STT','Phiên bản hoặc gói cập nhật','Ngày cập nhật','Nội dung thay đổi','Đầu mối theo phân công']
 for c,text in zip(t.rows[0].cells,headers):c.text=text
 for data in rows:
  t._tbl.append(deepcopy(template))
  for c,text in zip(t.rows[-1].cells,data):c.text=text
 widths=[1.05,3.05,2.2,6.2,4.3]
 total=(d.sections[-1].page_width-d.sections[-1].left_margin-d.sections[-1].right_margin)/Cm(1)
 factor=total/sum(widths);t.autofit=False
 for c,w in zip(t.columns,widths):c.width=Cm(w*factor)
 for ri,row in enumerate(t.rows):
  for ci,c in enumerate(row.cells):
   c.width=Cm(widths[ci]*factor);c.vertical_alignment=WD_CELL_VERTICAL_ALIGNMENT.CENTER
   for p in c.paragraphs:
    p.paragraph_format.space_before=Pt(3);p.paragraph_format.space_after=Pt(3)
    p.paragraph_format.line_spacing=1.05
    for r in p.runs:r.font.name='Times New Roman';r.font.size=Pt(10.5);r.bold=ri==0
  trpr=row._tr.get_or_add_trPr()
  for el in list(trpr):
   if el.tag==qn('w:trHeight'):trpr.remove(el)
  if not trpr.findall(qn('w:cantSplit')):trpr.append(OxmlElement('w:cantSplit'))
 repeat=OxmlElement('w:tblHeader');t.rows[0]._tr.get_or_add_trPr().append(repeat)
 # Clarify that named people follow the plan, rather than inventing proof of execution.
 for p in list(d.paragraphs):
  if p.text.startswith('Đầu mối trong sổ được ghi theo mục I'):p._p.getparent().remove(p._p)
 note=d.add_paragraph('Đầu mối trong sổ được ghi theo mục I Kế hoạch triển khai chi tiết và danh sách nhân sự NCC-03. Khi chốt từng lần cập nhật, đầu mối bổ sung xác nhận thực hiện và minh chứng liên quan. Mã gói phần mềm được phân biệt với số phiên bản tài liệu NCC-14.')
 note.paragraph_format.space_before=Pt(6)
 t._tbl.addnext(note._p)
 for r in note.runs:r.font.name='Times New Roman';r.font.size=Pt(10.5);r.italic=True
 d.save(f)
(tmp/'files.json').write_text(json.dumps([str(f) for f in files],ensure_ascii=False),encoding='utf-8')
print('Updated 2 NCC-14 files with 3 sourced history entries each.')
