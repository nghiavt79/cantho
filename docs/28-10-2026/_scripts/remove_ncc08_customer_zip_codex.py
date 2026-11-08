from pathlib import Path
from zipfile import ZipFile,ZIP_DEFLATED
import hashlib,json,os
B=Path(r'D:\2026\CanTho\docs\28-10-2026')
p=B/'Bo_du_thao_gui_khach_28-10-2026.zip'
tmp=B/'_noi_bo'/'customer_zip_update.tmp'
assert not tmp.exists()
with ZipFile(p) as z:
 assert z.testzip() is None
 data={n:z.read(n) for n in z.namelist() if not n.endswith('/')}
remove=[n for n in data if Path(n).name.startswith(('NCC-07 ','NCC-08 '))]
assert len(remove)==1 and Path(remove[0]).name.startswith('NCC-08 '),remove
for n in remove:del data[n]
man=[n for n in data if n.endswith('00_Danh_muc_ho_so_du_thao.txt')]
assert len(man)==1
text=data[man[0]].decode('utf-8-sig')
assert '; NCC-08.' in text
text=text.replace('; NCC-08.','.')
data[man[0]]=text.encode('utf-8-sig')
assert not any(Path(n).name.startswith(('NCC-07 ','NCC-08 ')) for n in data)
with ZipFile(tmp,'x',ZIP_DEFLATED,compresslevel=6) as z:
 for n,b in data.items():z.writestr(n,b)
with ZipFile(tmp) as z:
 assert z.testzip() is None
 assert len(z.namelist())==len(set(z.namelist()))==len(data)
 for n,b in data.items():assert z.read(n)==b
os.replace(tmp,p)
report={'zip':str(p),'files':len(data),'bytes':p.stat().st_size,'removed':remove,'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'verification':'All retained members match previous package, except updated customer contents list. NCC-07 and NCC-08 documents absent.'}
(B/'_review_codex_lan2'/'customer_package_verification.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(report,ensure_ascii=True))
