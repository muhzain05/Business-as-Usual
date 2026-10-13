"""Restore only byte-verified pre-task accepted evidence after historic validators.
Media not copied by the source snapshot can be recovered from earlier backups only
when its digest exactly equals the pre-8A manifest. Never substitute a stale file.
"""
from pathlib import Path
import hashlib,json,shutil
root=Path(__file__).resolve().parents[1];backup=Path('D:/Business as Usual Backups/pre-phase8A-2026-10-12')
manifest=json.loads((backup/'manifest.json').read_text())
candidates=[backup,Path('D:/Business as Usual Backups/pre-phase7B-2026-10-11'),Path('D:/Business as Usual Backups/pre-phase7A-2026-10-11')]
restored=[];missing=[]
for name,expected in manifest.items():
    if not any(name.startswith('Docs/Diagnostics/'+phase+'/') for phase in ['Phase3','Phase4','Phase4.1','Phase4.2','Phase7A']):continue
    path=root/name
    if path.exists() and hashlib.sha256(path.read_bytes()).hexdigest()==expected:continue
    for source in candidates:
        prior=source/name
        if prior.is_file() and hashlib.sha256(prior.read_bytes()).hexdigest()==expected:
            shutil.copy2(prior,path);restored.append(name);break
    else:missing.append(name)
print(json.dumps(dict(restored=restored,unrecoverable=missing),indent=2))
if missing:raise SystemExit(1)
