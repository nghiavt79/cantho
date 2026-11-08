from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
import hashlib,json,sys
sys.stdout.reconfigure(encoding='utf-8')
B=Path(r'D:\2026\CanTho\docs\28-10-2026'); H=B/'Ho_so_NCC'
out=B/'Bo_du_thao_gui_khach_28-10-2026.zip'
if out.exists():raise RuntimeError('Output already exists; do not overwrite an existing customer package')
root='Bo_du_thao_gui_khach_28-10-2026'
items={};sources={}
def add(p,rel):
 before=p.stat(); data=p.read_bytes();after=p.stat()
 assert (before.st_size,before.st_mtime_ns)==(after.st_size,after.st_mtime_ns),str(p)
 name=f'{root}/{rel}'
 assert name not in items
 items[name]=data;sources[name]={'source':str(p),'sha256':hashlib.sha256(data).hexdigest(),'mtime_ns':after.st_mtime_ns}
docs=sorted(H.glob('*.docx'))
assert len(docs)==6
for p in docs:
 name=p.name
 if name.startswith('1 - '):name='GS-02 Bien ban kiem tra ha tang thue va chat luong dich vu (du thao).docx'
 add(p,'01_Van_ban_du_thao/'+name)
ncc_dirs=[p for p in H.iterdir() if p.is_dir() and p.name.startswith('NCC-02 ')]
assert len(ncc_dirs)==1
ncc=ncc_dirs[0]
for p in sorted(ncc.rglob('*')):
 if p.is_file():
  assert p.suffix.lower() in {'.docx','.txt','.ps1'}
  add(p,'02_NCC-02_va_tai_lieu/'+p.relative_to(ncc).as_posix())
ev=H/'NCC-06 Minh chung'
for p in sorted(ev.iterdir()):
 assert p.is_file() and p.name.startswith('MC-') and p.suffix=='.txt'
 add(p,'03_Minh_chung_NCC-06/'+p.name)
check=B/'28.10.2026 - Checklist ho so theo trinh tu thoi gian - V8.xlsx'
add(check,'04_Checklist/'+check.name)
assert len(items)==74,len(items)
manifest='''DANH MỤC BỘ DỰ THẢO GỬI PHỐI HỢP RÀ SOÁT NGÀY 28/10/2026

01_Van_ban_du_thao
  NCC-03; VHT-04; NCC-05; NCC-06 kèm phụ lục xác nhận; GS-02; NCC-08.

02_NCC-02_va_tai_lieu
  Văn bản NCC-02 và 11 tài liệu TL01–TL11.
  Kèm kịch bản, quy trình kiểm thử, minh chứng và tập lệnh kiểm thử API.
  Quy trình NCC-04 nằm trong TL03, không phát hành riêng.

03_Minh_chung_NCC-06
  Minh chứng giám sát và kiểm thử được viện dẫn trong NCC-06.
  Các log trước và sau khắc phục được giữ để đối chiếu diễn biến kiểm thử.

04_Checklist
  Checklist hồ sơ V8 ngày 28/10/2026.

Bộ tài liệu dùng để phối hợp rà soát, bổ sung số liệu thực tế và xác nhận các bên.
Các vị trí còn để …… dành cho đơn vị phụ trách hoàn thiện; chưa phải bộ đã ký phát hành.
'''
items[f'{root}/00_Danh_muc_ho_so_du_thao.txt']=manifest.encode('utf-8-sig')
for name in items:
 assert not any(s in name.lower() for s in ['ghi chu','ra_soat','_noi_bo','_scripts','_review','v7.xlsx','chi-quyen'])
 assert not name.endswith('.zip')
for name,rec in sources.items():
 p=Path(rec['source']);assert p.stat().st_mtime_ns==rec['mtime_ns'],str(p)
 assert hashlib.sha256(p.read_bytes()).hexdigest()==rec['sha256'],str(p)
with ZipFile(out,'x',ZIP_DEFLATED,compresslevel=6) as z:
 for name,data in items.items():z.writestr(name,data)
with ZipFile(out) as z:
 assert z.testzip() is None
 assert len(z.namelist())==len(set(z.namelist()))==len(items)
 for name,data in items.items():assert z.read(name)==data,name
report={'zip':str(out),'bytes':out.stat().st_size,'files':len(items),'sha256':hashlib.sha256(out.read_bytes()).hexdigest(),'sources':sources}
(B/'_review_codex_lan2'/'customer_package_verification.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in report.items() if k!='sources'},ensure_ascii=False))
print('Verified all ZIP members against selected source bytes; no nested archives, internal notes, review reports or old document sets.')
