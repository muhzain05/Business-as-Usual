"""Verify artifact integrity and source preservation without editing assets."""
import hashlib, json, re, subprocess
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
inventory=json.loads((ROOT/'Docs/Diagnostics/asset-inventory.json').read_text(encoding='utf-8'))
for item in inventory:
    path=ROOT/item['path']
    assert path.is_file(),f'Missing source: {path}'
    assert path.stat().st_size==item['bytes'],f'Size changed: {path}'
    assert hashlib.sha256(path.read_bytes()).hexdigest()==item['sha256'],f'Hash changed: {path}'
    ignored=subprocess.run(['git','-C',str(ROOT),'check-ignore',str(path)],capture_output=True)
    assert ignored.returncode==1,f'Source unexpectedly ignored or git check failed: {path}'
required=['README.md','.gitignore','Docs/game-design.md','Docs/art-direction.md',
          'Docs/character-pipeline.md','Docs/character-audit.md',
          'Assets/Characters/Sunny/Working/Sunny_Phase1_Audit.blend']
for name in required: assert (ROOT/name).is_file(),f'Missing deliverable: {name}'
for path in (ROOT/'Docs/Diagnostics').glob('*.json'): json.loads(path.read_text(encoding='utf-8'))
images=['original-views.png','run-frames.png','walk-frames.png','auxiliary-obstruction.png','connected-components.png']
for name in images:
    with Image.open(ROOT/'Docs/Diagnostics'/name) as im: im.verify()
for path in (ROOT/'Scripts').rglob('*.py'): compile(path.read_text(encoding='utf-8'),str(path),'exec')
for path in [ROOT/'README.md',*(ROOT/'Docs').glob('*.md')]:
    for target in re.findall(r'\]\(([^)]+)\)',path.read_text(encoding='utf-8')):
        if '://' not in target and not target.startswith('#'):
            assert (path.parent/target.split('#')[0]).exists(),f'Broken link in {path}: {target}'
revision=subprocess.run(['git','-C',str(ROOT),'rev-parse','--verify','HEAD'],capture_output=True)
assert revision.returncode!=0,'A commit now exists; review expected phase boundary.'
remotes=subprocess.check_output(['git','-C',str(ROOT),'remote'],text=True).strip()
assert not remotes,'Remote hosting has been configured.'
summary={'source_files_verified':len(inventory),'source_bytes':sum(x['bytes'] for x in inventory),
    'required_deliverables_verified':len(required),'screenshots_and_diagrams_verified':len(images),
    'scripts_compile':True,'markdown_links_resolve':True,'no_commit':True,'no_remote':True}
(ROOT/'Docs/Diagnostics/phase1-validation.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
print(json.dumps(summary,indent=2))
