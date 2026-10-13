"""Archive this phase's fresh outputs and restore the current user's accepted baseline."""
from pathlib import Path
import argparse,shutil,json,hashlib
root=Path(__file__).resolve().parents[1];backup=Path('D:/Business as Usual Backups/pre-phase8A-2026-10-12')
parser=argparse.ArgumentParser();parser.add_argument('phase',choices=['Phase3','Phase4','Phase4.1','Phase4.2','Phase7A']);args=parser.parse_args()
source=root/'Docs/Diagnostics'/args.phase;destination=root/'Docs/Diagnostics/Phase8A/EditorRegression'/args.phase
files=[]
baseline=json.loads((backup/'manifest.json').read_text())
for path in source.rglob('*'):
    if not path.is_file():continue
    relative=path.relative_to(source);prior=backup/'Docs/Diagnostics'/args.phase/relative
    if baseline.get(path.relative_to(root).as_posix())==hashlib.sha256(path.read_bytes()).hexdigest():continue
    if prior.exists() and hashlib.sha256(prior.read_bytes()).digest()==hashlib.sha256(path.read_bytes()).digest():continue
    target=destination/relative;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(path,target);files.append(str(relative))
    if prior.exists():shutil.copy2(prior,path)
print(json.dumps(dict(phase=args.phase,archived=files)))
