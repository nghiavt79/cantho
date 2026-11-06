from pathlib import Path
from docx import Document
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
p=Path(r'D:\2026\CanTho\docs\2 Tich hop hd dien tu sms\HopDong_ThuNghiem.docx')
d=Document(p)
for st in d.styles:
 if st.type==1:
  for el in list(st.element.xpath('.//w:pBdr')): el.getparent().remove(el)
  rf=st.element.xpath('.//w:rFonts')
  for f in rf:
   for k in list(f.attrib):
    if 'Theme' in k: del f.attrib[k]
   f.set(qn('w:ascii'),'Times New Roman'); f.set(qn('w:hAnsi'),'Times New Roman')
for para in d.paragraphs:
 if 'Độc lập' in para.text: para.text='Độc lập – Tự do – Hạnh phúc'
 for r in para.runs:
  import re
  r.text=re.sub(r':(?=\S)',': ',r.text)
  r.font.name='Times New Roman'
d.save(p)
