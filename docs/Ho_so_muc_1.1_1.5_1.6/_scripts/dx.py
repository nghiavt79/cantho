import docx
def para_replace(p, old, new):
    runs = p.runs
    full = ''.join(r.text for r in runs)
    i = full.find(old)
    if i < 0: return False
    j = i + len(old); pos = 0; done = False
    for r in runs:
        t = r.text; s, e = pos, pos + len(t); pos = e
        if e <= i or s >= j: continue
        a, b = max(i, s) - s, min(j, e) - s
        if not done:
            r.text = t[:a] + new + t[b:]; done = True
        else:
            r.text = t[:a] + t[b:]
    return True
def all_paras(d):
    for p in d.paragraphs: yield p
    for t in d.tables:
        for row in t.rows:
            seen=set()
            for c in row.cells:
                if id(c._tc) in seen: continue
                seen.add(id(c._tc))
                for p in c.paragraphs: yield p
def rep(d, old, new, count=1, ctx=None):
    n = 0
    for p in all_paras(d):
        if ctx and ctx not in p.text and not ctx_in_row(p, ctx): continue
        while n < count and para_replace(p, old, new): n += 1
        if n >= count: break
    if n != count: raise SystemExit(f"FAIL [{old[:50]}] ctx={ctx} n={n}/{count}")
    return n
def ctx_in_row(p, ctx):
    el = p._p
    while el is not None and not el.tag.endswith('}tr'): el = el.getparent()
    if el is None: return False
    return ctx in ''.join(x.text or '' for x in el.iter() if x.tag.endswith('}t'))
def save(d, f):
    d.core_properties.author = 'NghiaVT'; d.core_properties.last_modified_by = 'NghiaVT'
    d.save(f)

def cell_set(cell, text):
    ps = cell.paragraphs
    p0 = ps[0]
    runs = p0.runs
    if runs:
        runs[0].text = text
        for r in runs[1:]: r.text = ''
    else:
        p0.add_run(text)
    for p in ps[1:]:
        p._p.getparent().remove(p._p)

def para_set(p, text):
    runs = p.runs
    runs[0].text = text
    for r in runs[1:]: r.text = ''

def insert_after(p, text):
    import copy
    new = copy.deepcopy(p._p); p._p.addnext(new)
    from docx.text.paragraph import Paragraph
    np_ = Paragraph(new, p._parent); para_set(np_, text); return np_
