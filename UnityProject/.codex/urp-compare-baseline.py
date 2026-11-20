import json, hashlib, sys, difflib
from pathlib import Path
root = Path(__file__).resolve().parents[1]
base = root / '.codex/runs/urp-preactivation-20261120T115653Z'
rows = json.loads((base / 'hashes.json').read_text(encoding='utf-8-sig'))
old = {Path(r['Path']).relative_to(root).as_posix():r['Hash'].lower() for r in rows}
now = {}
for folder in ('Assets','ProjectSettings','Packages'):
    for p in (root / folder).rglob('*'):
        if p.is_file() and 'Packages/com.unity.pipeline/' not in p.relative_to(root).as_posix():
            now[p.relative_to(root).as_posix()] = hashlib.sha256(p.read_bytes()).hexdigest()
result = {'changed':[p for p,h in old.items() if p in now and now[p]!=h], 'removed':sorted(set(old)-set(now)), 'added':sorted(set(now)-set(old))}
output = root / sys.argv[1]
output.mkdir(parents=True,exist_ok=False)
(output/'changes.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
text=[]
for name in ('GraphicsSettings.asset','QualitySettings.asset'):
    a = (base/'ProjectSettings'/name).read_text(encoding='utf-8-sig').splitlines(True)
    b = (root/'ProjectSettings'/name).read_text(encoding='utf-8-sig').splitlines(True)
    text.extend(difflib.unified_diff(a,b,fromfile='before/'+name,tofile='after/'+name))
(output/'settings.diff').write_text(''.join(text),encoding='utf-8')
print(json.dumps(result,ensure_ascii=False,indent=2))
print('Evidence:', output)
