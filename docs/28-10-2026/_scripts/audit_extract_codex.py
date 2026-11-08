from pathlib import Path
from zipfile import ZipFile
from lxml import etree as E
import json, openpyxl
B=Path(r'D:\2026\CanTho\docs\28-10-2026')
O=B/'_review_codex'; O.mkdir(exist_ok=True)
ns={'w':'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
records=[]
for i,p in enumerate(B.rglob('*.docx')):
 with ZipFile(p) as z:
  parts=[n for n in z.namelist() if n=='word/document.xml' or n.startswith(('word/header','word/footer','word/comments')) and n.endswith('.xml')]
  lines=[]
  for n in parts:
   root=E.fromstring(z.read(n))
   lines.append('PART '+n)
   for j,par in enumerate(root.findall('.//w:p',ns)):
    t=''.join(par.itertext()) if False else ''.join(par.xpath('.//w:t/text()',namespaces=ns))
    if t.strip(): lines.append(f'P{j+1}: {t}')
  txt='\n'.join(lines)
  (O/f'{i:02d}.txt').write_text(str(p)+'\n'+txt,encoding='utf-8')
  records.append({'id':i,'path':str(p),'chars':len(txt),'images':len([n for n in z.namelist() if n.startswith('word/media/')]),'embeddings':len([n for n in z.namelist() if n.startswith('word/embeddings/')])})
(O/'index.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
for p in B.glob('*.xlsx'):
 wb=openpyxl.load_workbook(p,data_only=False)
 out=[]
 for s in wb:
  out.append('SHEET '+s.title)
  for row in s:
   vals=[f'{c.column_letter}={c.value}' for c in row if c.value is not None]
   if vals:out.append(f'ROW {row[0].row}: '+' | '.join(vals))
 (O/(p.stem+'.txt')).write_text('\n'.join(out),encoding='utf-8')
for r in records:print(r['id'],r['chars'],r['images'],Path(r['path']).relative_to(B))
