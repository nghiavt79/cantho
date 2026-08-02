#!/usr/bin/env python3
"""
i18n audit tool (dev) — cổng chống sót khi gom UI text về DB.

Chiều A: mọi key dùng qua Html.Text/Tr/TrHtml / I18n.T  ->  phải có trong DB.
Chiều B (backlog): còn bao nhiêu chuỗi inline T("vi","en") và ternary isEn? chưa migrate.
+ Liệt kê key ĐỘNG (interpolated $"...") không kiểm tĩnh được, và key trong DB chưa thấy dùng.

Chạy:  python Scripts/i18n_audit.py     (cwd = thư mục TechExchangeApp)
"""
import os, re, subprocess, sys, collections
try:
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
except Exception:
    pass

APP_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))  # ...\TechExchangeApp
SCAN_DIRS = ["Views", "Areas", "Controllers", "Services"]
EXTS = (".cshtml", ".cs")

# --- DB connection (dev) ---
SQLCMD = ["sqlcmd", "-S", "localhost", "-d", "TechExchangeNew", "-U", "sa", "-P", "111111", "-C"]

# key literal — Html.Text/Tr/TrHtml("key") (1 tham số) và I18n.T(ctx, "key") (2 tham số)
RE_HTMLTEXT = re.compile(r'Html\.(?:Text|Tr|TrHtml)\(\s*"([^"]+)"')
RE_I18NT = re.compile(r'I18n\.T\(\s*[^,"]+,\s*"([^"]+)"')
# key động (interpolated $"...")
RE_DYNAMIC = re.compile(r'(?:Html\.(?:Text|Tr|TrHtml)\(\s*\$"|I18n\.T\(\s*[^,"]+,\s*\$")')
# local helper T("vi","en") — T không đứng sau ký tự định danh hoặc dấu chấm
RE_INLINE_T = re.compile(r'(?<![\w.])T\(\s*"')
# ternary/điều kiện ngôn ngữ
RE_ISEN = re.compile(r'isEn\s*\?|IsEnglish\(')

def iter_files():
    for d in SCAN_DIRS:
        base = os.path.join(APP_DIR, d)
        for root, _, files in os.walk(base):
            for f in files:
                if f.endswith(EXTS):
                    yield os.path.join(root, f)

def rel(p): return os.path.relpath(p, APP_DIR).replace("\\", "/")

def db_keys():
    out = subprocess.run(SQLCMD + ["-h", "-1", "-W", "-Q",
        "SET NOCOUNT ON; SELECT [Key] FROM UiTranslations"],
        capture_output=True, text=True)
    keys = set()
    for line in out.stdout.splitlines():
        s = line.strip()
        if s and not s.startswith("(") and " rows affected" not in s:
            keys.add(s)
    return keys

def main():
    static_keys = {}   # key -> [locations]
    dynamic_hits = []  # locations
    inline_t = collections.Counter()
    isen = collections.Counter()

    for path in iter_files():
        try:
            text = open(path, encoding="utf-8").read()
        except Exception:
            continue
        for i, line in enumerate(text.splitlines(), 1):
            for rx in (RE_HTMLTEXT, RE_I18NT):
                for m in rx.finditer(line):
                    static_keys.setdefault(m.group(1), []).append(f"{rel(path)}:{i}")
            if RE_DYNAMIC.search(line):
                dynamic_hits.append(f"{rel(path)}:{i}")
            n = len(RE_INLINE_T.findall(line))
            if n: inline_t[rel(path)] += n
            n2 = len(RE_ISEN.findall(line))
            if n2: isen[rel(path)] += n2

    db = db_keys()
    used = set(static_keys)
    missing = sorted(used - db)          # A: dùng nhưng thiếu DB
    unused = sorted(db - used)           # D: trong DB nhưng không thấy dùng tĩnh

    print("=" * 70)
    print("i18n AUDIT")
    print("=" * 70)
    print(f"DB keys: {len(db)} | static keys used: {len(used)} | dynamic hits: {len(dynamic_hits)}")
    print()
    print(f"[A] KEY THIẾU TRONG DB (Html.Text/Tr dùng nhưng chưa seed): {len(missing)}")
    for k in missing:
        print(f"    ✗ {k}   ({static_keys[k][0]})")
    print()
    print(f"[A2] KEY ĐỘNG (interpolated, không kiểm tĩnh được): {len(dynamic_hits)}")
    for loc in dynamic_hits[:40]:
        print(f"    ~ {loc}")
    print()
    print(f"[B] BACKLOG inline T(\"vi\",\"en\") — tổng {sum(inline_t.values())} ở {len(inline_t)} file:")
    for f, c in inline_t.most_common():
        print(f"    {c:4d}  {f}")
    print()
    print(f"[C] isEn?/IsEnglish (cần triage text-vs-logic) — tổng {sum(isen.values())} ở {len(isen)} file:")
    for f, c in isen.most_common():
        print(f"    {c:4d}  {f}")
    print()
    print(f"[D] DB key chưa thấy dùng tĩnh (có thể động/đã bỏ): {len(unused)}")
    for k in unused[:40]:
        print(f"    ? {k}")

    # exit code != 0 nếu có key thiếu (dùng cho CI/gate)
    return 1 if missing else 0

if __name__ == "__main__":
    sys.exit(main())
