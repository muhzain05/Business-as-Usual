"""Preserve the accepted Phase 6 assets, evidence and staging while refining the town."""
from pathlib import Path
import hashlib,json,subprocess,shutil,sys
root=Path(__file__).resolve().parents[1]
out=root/'Docs/Diagnostics/Phase6Addendum'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
if sys.argv[1]=='baseline':
    assert not (out/'baseline.json').exists()
    out.mkdir(parents=True,exist_ok=True)
    paths=[p for folder in ('Assets','Unity/BusinessAsUsual/Assets','Unity/BusinessAsUsual/ProjectSettings','Unity/BusinessAsUsual/Packages','Docs/UI') for p in (root/folder).rglob('*') if p.is_file()]
    (out/'baseline.json').write_text(json.dumps({p.relative_to(root).as_posix():sha(p) for p in paths},indent=2))
    (out/'index-before.txt').write_bytes(subprocess.check_output(['git','ls-files','--stage'],cwd=root))
    for name in ('After','TwoProcess1080'):
        shutil.copytree(root/'Docs/Diagnostics/Phase6'/name,out/'Before'/name)
    print('Recorded accepted assets/index and copied current town/UI comparison evidence.')
else:
    baseline=json.loads((out/'baseline.json').read_text())
    changed=[p for p,h in baseline.items() if not (root/p).exists() or sha(root/p)!=h]
    allowed=['Unity/BusinessAsUsual/Assets/Town/Scenes/DaylightTown.unity','Unity/BusinessAsUsual/ProjectSettings/EditorBuildSettings.asset']
    protected=[p for p in changed if not p.endswith('.cs') and p not in allowed and not p.startswith('Unity/BusinessAsUsual/Assets/Town/Materials/Text-')]
    same=(out/'index-before.txt').read_bytes()==subprocess.check_output(['git','ls-files','--stage'],cwd=root)
    result=dict(indexUnchanged=same,changedExistingFiles=changed,protectedAssetsChanged=protected)
    (out/'preservation.json').write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2));assert same and not protected
