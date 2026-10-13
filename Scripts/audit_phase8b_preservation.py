from pathlib import Path
import hashlib,json,subprocess
r=Path(__file__).resolve().parents[1];b=Path('D:/Business as Usual Backups/pre-phase8B-2026-10-13');m=json.loads((b/'manifest.json').read_text(encoding='utf-8-sig'))
changed=[]
for name,h in m.items():
 p=r/name
 if not p.exists() or hashlib.sha256(p.read_bytes()).hexdigest()!=h:changed.append(name)
stage=subprocess.check_output(['git','ls-files','--stage'],cwd=r)
out=dict(baselineFiles=len(m),stageUnchanged=stage==(b/'index-stage.txt').read_bytes(),indexBytesUnchanged=(r/'.git/index').read_bytes()==(b/'index').read_bytes(),changedBaselineFiles=changed)
(r/'Docs/Diagnostics/Phase8B/preservation-audit.json').write_text(json.dumps(out,indent=2),encoding='utf-8');print(json.dumps(out,indent=2))
