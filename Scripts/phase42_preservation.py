"""Capture/compare the accepted workspace without touching the index."""
from pathlib import Path
import hashlib, json, shutil, subprocess, sys
ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Docs/Diagnostics/Phase4.2'
def digest(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def snapshot():
    paths = [p for p in (ROOT/'Unity/BusinessAsUsual/Assets').rglob('*') if p.is_file()]
    paths += [p for p in (ROOT/'Unity/BusinessAsUsual/ProjectSettings').rglob('*') if p.is_file()]
    paths += [p for p in (ROOT/'Docs/UI/Phase4.2').rglob('*') if p.is_file()]
    return {p.relative_to(ROOT).as_posix(): digest(p) for p in paths}
if __name__ == '__main__':
    OUT.mkdir(parents=True, exist_ok=True)
    baseline = OUT/'workspace-before.json'
    index = hashlib.sha256(subprocess.check_output(['git','ls-files','--stage'],cwd=ROOT)).hexdigest()
    if sys.argv[1] == 'capture':
        if baseline.exists(): raise SystemExit('Baseline already exists; refusing to replace it.')
        baseline.write_text(json.dumps({'files':snapshot(),'index':index},indent=2),encoding='utf-8')
        for phase in ['Phase3','Phase4','Phase4.1']:
            shutil.copytree(ROOT/'Docs/Diagnostics'/phase,OUT/'accepted-baseline'/phase)
        shutil.copy2(ROOT/'Unity/BusinessAsUsual/Assets/Shopkeeper/Scripts/ShopHUD.cs',OUT/'ShopHUD-before.cs.txt')
        print('Preserved source hashes, staged entries and previous diagnostic evidence.')
    elif sys.argv[1] == 'check':
        old=json.loads(baseline.read_text(encoding='utf-8')); now=snapshot()
        changed=[p for p,h in old['files'].items() if now.get(p)!=h]
        task_created_before_snapshot=['Unity/BusinessAsUsual/Assets/Shopkeeper/Editor/ShopUICapture.cs']
        result={'stagedEntriesUnchanged':index==old['index'],'changedExistingFiles':changed,
                'createdEarlierInThisTask':task_created_before_snapshot,
                'changedPreExistingUnityFiles':[p for p in changed if p not in task_created_before_snapshot],
                'newFiles':[p for p in now if p not in old['files']]}
        (OUT/'workspace-after.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
        print(json.dumps({k:v for k,v in result.items() if k!='newFiles'} | {'newFilesCount':len(result['newFiles'])},indent=2))
