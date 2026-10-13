"""Preserve existing validation artifacts and verify source/index ownership."""
from pathlib import Path
import sys, json, hashlib, shutil, subprocess
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/Diagnostics/Phase5'
PHASES=('Phase3','Phase4','Phase4.1','Phase4.2')
def hashfile(p):return hashlib.sha256(p.read_bytes()).hexdigest().upper()
if sys.argv[1]=='backup':
    for phase in PHASES:
        dest=OUT/'accepted-baseline'/phase
        if dest.exists():raise SystemExit('Refusing to overwrite '+str(dest))
        shutil.copytree(ROOT/'Docs/Diagnostics'/phase,dest)
    print('Backed up previous diagnostic evidence.')
elif sys.argv[1]=='archive':
    phase=sys.argv[2]
    shutil.copytree(ROOT/'Docs/Diagnostics'/phase,OUT/'Regression'/phase,dirs_exist_ok=True)
    shutil.copytree(OUT/'accepted-baseline'/phase,ROOT/'Docs/Diagnostics'/phase,dirs_exist_ok=True)
    print('Archived fresh '+phase+' evidence; restored accepted originals.')
elif sys.argv[1]=='restore-build-side-effects':
    before={r['path']:r['sha256'] for r in json.loads((OUT/'files-before.json').read_text(encoding='utf-8-sig'))}
    for path in ('Unity/BusinessAsUsual/Assets/Shopkeeper/Materials/Shop_Paper.mat','Unity/BusinessAsUsual/Assets/Shopkeeper/Materials/Shop_Screen.mat','Unity/BusinessAsUsual/ProjectSettings/ProjectSettings.asset'):
        blob=subprocess.check_output(['git','show',':'+path],cwd=ROOT)
        candidates=(blob,blob.replace(b'\r\n',b'\n').replace(b'\n',b'\r\n'))
        original=next((c for c in candidates if hashlib.sha256(c).hexdigest().upper()==before[path]),None)
        assert original is not None,'Index does not match the recorded pre-task source: '+path
        (ROOT/path).write_bytes(original)
        print('Restored exact pre-task bytes: '+path)
elif sys.argv[1]=='check':
    before=json.loads((OUT/'files-before.json').read_text(encoding='utf-8-sig'))
    changed=[r['path'] for r in before if not (ROOT/r['path']).is_file() or hashfile(ROOT/r['path'])!=r['sha256']]
    index=subprocess.check_output(['git','ls-files','--stage'],cwd=ROOT).decode().splitlines()
    old=(OUT/'index-before.txt').read_text(encoding='utf-8-sig').splitlines()
    protected=[p for p in changed if p.startswith('Assets/') or '/Characters/' in p or '/Animations/' in p or p.startswith('Docs/UI/') or p.endswith(('.fbx','.blend','.png','.anim','.controller','.mat')) or '/Scenes/ShopkeeperShift.unity' in p]
    evidence=[]
    for phase in PHASES:
        evidence.extend(str(p.relative_to(ROOT)) for p in (OUT/'accepted-baseline'/phase).rglob('*') if p.is_file() and hashfile(p)!=hashfile(ROOT/'Docs/Diagnostics'/phase/p.relative_to(OUT/'accepted-baseline'/phase)))
    result=dict(indexUnchanged=old==index,changedExistingFiles=changed,protectedAssetsChanged=protected,previousEvidenceChanged=evidence)
    (OUT/'preservation.json').write_text(json.dumps(result,indent=2))
    print(json.dumps(result,indent=2))
    assert old==index and not protected and not evidence
