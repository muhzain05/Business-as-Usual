"""Record the current working tree without changing the index; preserve prior evidence."""
from pathlib import Path
import json, hashlib, subprocess, shutil, sys
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/Diagnostics/Phase6'
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
mode=sys.argv[1]
if mode=='baseline':
    OUT.mkdir(parents=True,exist_ok=True)
    assert not (OUT/'baseline.json').exists(), 'Baseline already recorded'
    paths=[p for folder in ('Assets','Unity/BusinessAsUsual/Assets','Unity/BusinessAsUsual/ProjectSettings','Unity/BusinessAsUsual/Packages','Docs/UI') for p in (ROOT/folder).rglob('*') if p.is_file()]
    (OUT/'baseline.json').write_text(json.dumps({p.relative_to(ROOT).as_posix():digest(p) for p in paths},indent=2))
    (OUT/'index-before.txt').write_bytes(subprocess.check_output(['git','ls-files','--stage'],cwd=ROOT))
    for phase in ('Phase3','Phase4','Phase4.1','Phase4.2'):
        shutil.copytree(ROOT/'Docs/Diagnostics'/phase,OUT/'accepted-baseline'/phase)
    print('Recorded working files and index; backed up existing regression evidence.')
elif mode=='archive':
    phase=sys.argv[2]
    shutil.copytree(ROOT/'Docs/Diagnostics'/phase,OUT/'Regression'/phase,dirs_exist_ok=True)
    shutil.copytree(OUT/'accepted-baseline'/phase,ROOT/'Docs/Diagnostics'/phase,dirs_exist_ok=True)
    print('Archived fresh '+phase+' and restored original evidence.')
elif mode=='check':
    baseline=json.loads((OUT/'baseline.json').read_text())
    changed=[path for path,h in baseline.items() if not (ROOT/path).exists() or digest(ROOT/path)!=h]
    protected=[p for p in changed if not p.endswith('.cs') and p!='Unity/BusinessAsUsual/ProjectSettings/EditorBuildSettings.asset']
    sameIndex=(OUT/'index-before.txt').read_bytes()==subprocess.check_output(['git','ls-files','--stage'],cwd=ROOT)
    result=dict(indexUnchanged=sameIndex,changedExistingFiles=changed,protectedAssetsChanged=protected)
    (OUT/'preservation.json').write_text(json.dumps(result,indent=2))
    print(json.dumps(result,indent=2));assert sameIndex and not protected
elif mode=='restore-network-registry':
    # NGO auto-registers newly saved prefabs in the global list. Phase 6 owns a separate list.
    path=ROOT/'Unity/BusinessAsUsual/Assets/DefaultNetworkPrefabs.asset'
    expected=json.loads((OUT/'baseline.json').read_text())[path.relative_to(ROOT).as_posix()]
    lines=path.read_bytes().splitlines(keepends=True)
    starts=[i for i,line in enumerate(lines) if line.startswith(b'  - Override:')]
    if digest(path)!=expected:
        assert len(starts)==2,'Unexpected network registry change; preserve for review'
        candidate=b''.join(lines[:starts[1]])
        assert hashlib.sha256(candidate).hexdigest()==expected,'Restoration must match exact accepted bytes'
        path.write_bytes(candidate)
    print('Global Phase 5 registry matches baseline; Phase 6 uses its own prefab list.')
