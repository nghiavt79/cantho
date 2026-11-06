exec(open(r'D:\2026\CanTho\docs\28-10-2026\_scripts\audit_extract_codex.py',encoding='utf-8').read().split('records=[]')[0])
import hashlib,difflib,re
recs=json.loads((O/'index.json').read_text(encoding='utf-8'))
def lines(i):return (O/f'{i:02d}.txt').read_text(encoding='utf-8').splitlines()[2:]
def content(i):return [re.sub(r'^P\d+: ','',s) for s in lines(i) if not s.startswith('PART ')]
out=[]
for a,b in [(0,32),(27,35),(28,36),(29,37),(21,38),(22,39),(23,40),(26,41),(24,42),(25,43),(1,12),(5,16)]:
 d=list(difflib.unified_diff(content(a),content(b),fromfile=str(a),tofile=str(b),n=1))
 out+=d+['']
(O/'diffs.txt').write_text('\n'.join(out),encoding='utf-8')
for p in [B/'Ho_so_NCC'/next((B/'Ho_so_NCC').glob('*.zip')).name,Path(r'D:\2026\CanTho\docs\Ho_so_muc_1.1_1.5_1.6.zip')]:
 with ZipFile(p) as z:
  names=[n for n in z.namelist() if not n.endswith('/')]
  print('ZIP',p,'files',len(names))
  if p.parent.name=='Ho_so_NCC':
   root=p.parent/p.stem
   mismatches=[]
   for n in names:
    rel=Path(n)
    if rel.parts[0]==root.name:rel=Path(*rel.parts[1:])
    f=root/rel
    if not f.exists() or hashlib.sha256(f.read_bytes()).digest()!=hashlib.sha256(z.read(n)).digest():mismatches.append(n)
   print('ZIP mismatches',mismatches)
   print('ZIP docx', [Path(n).name for n in names if n.endswith('.docx')])
print('CHANGES V7 TO V8')
w1=openpyxl.load_workbook(next(B.glob('*V7.xlsx'))); w2=openpyxl.load_workbook(next(B.glob('*V8.xlsx')))
for s1 in w1:
 s2=w2[s1.title]
 for row in s1:
  for c in row:
   if c.value!=s2[c.coordinate].value:print(c.coordinate,str(c.value)[:80],'=>',str(s2[c.coordinate].value)[:160])
print('evidence',len(list((B/'Ho_so_NCC'/'NCC-06 Minh chung').glob('*'))))
