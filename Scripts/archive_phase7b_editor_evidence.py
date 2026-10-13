"""Archive fresh historic validator output, then restore accepted evidence from pre-phase backup."""
from pathlib import Path
import argparse,shutil,hashlib,json
root=Path(__file__).resolve().parents[1]
backup=Path('D:/Business as Usual Backups/pre-phase7B-2026-10-11')
parser=argparse.ArgumentParser();parser.add_argument('phase');options=parser.parse_args()
source=root/'Docs/Diagnostics'/options.phase
destination=root/'Docs/Diagnostics/Phase7B/Regression'/options.phase
baseline=backup/'Docs/Diagnostics'/options.phase
changed=[]
for path in source.rglob('*'):
    if not path.is_file():continue
    relative=path.relative_to(source);prior=baseline/relative
    if prior.exists() and hashlib.sha256(path.read_bytes()).digest()==hashlib.sha256(prior.read_bytes()).digest():continue
    saved=destination/relative;saved.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(path,saved);changed.append(str(relative))
    if prior.exists():shutil.copy2(prior,path)
print(json.dumps({'phase':options.phase,'fresh_files_archived':len(changed),'files':changed[:8]}))
