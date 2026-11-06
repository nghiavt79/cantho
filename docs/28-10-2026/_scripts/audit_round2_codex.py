from pathlib import Path
from zipfile import ZipFile
from lxml import etree as E
import json,re,hashlib,openpyxl,sys
sys.stdout.reconfigure(encoding='utf-8')
B=Path(r'D:\2026\CanTho\docs\28-10-2026'); H=B/'Ho_so_NCC'; O=B/'_review_codex_lan2';O.mkdir(exist_ok=True)
ns={'w':'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
records=[]
for i,p in enumerate(H.rglob('*.docx')):
 with ZipFile(p) as z:
  lines=[]
  for n in z.namelist():
   if n=='word/document.xml' or n.startswith(('word/header','word/footer','word/comments')) and n.endswith('.xml'):
    root=E.fromstring(z.read(n));lines.append('PART '+n)
    for j,par in enumerate(root.findall('.//w:p',ns)):
     t=''.join(par.xpath('.//w:t/text()',namespaces=ns))
     if t.strip():lines.append(f'P{j+1}: {t}')
  txt='\n'.join(lines)
  (O/f'{i:02d}.txt').write_text(str(p)+'\n'+txt,encoding='utf-8')
  records.append({'id':i,'path':str(p),'chars':len(txt)})
  print(i,p.name)
  for line in lines:
   if re.search(r'snapshot|Chrome|trình duyệt.{0,45}(đo|giây)|đo.{0,45}trình duyệt|tải trang|03 giây|02 giây|L-02:|L02|MC-KT-38',line,re.I):print(' MATCH',line)
(O/'index.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
for p in B.glob('*V8.xlsx'):
 wb=openpyxl.load_workbook(p)
 out=[]
 for s in wb:
  for row in s:
   vals=[f'{c.coordinate}={c.value}' for c in row if c.value is not None]
   if vals:out.append(' | '.join(vals))
 (O/'checklist.txt').write_text('\n'.join(out),encoding='utf-8')
for p in H.glob('*.zip'):
 with ZipFile(p) as z:
  names=[n for n in z.namelist() if not n.endswith('/')]; root=H/p.stem;errs=[];seen=set()
  for n in names:
   rel=Path(n)
   if rel.parts[0]==root.name:rel=Path(*rel.parts[1:])
   seen.add(rel.as_posix()); f=root/rel
   if not f.exists() or f.read_bytes()!=z.read(n):errs.append(n)
  missing=[f.relative_to(root).as_posix() for f in root.rglob('*') if f.is_file() and f.relative_to(root).as_posix() not in seen]
  print('ZIP',len(names),'mismatch',errs,'not packed',missing)
  (O/'zip_members.txt').write_text('\n'.join(names),encoding='utf-8')
