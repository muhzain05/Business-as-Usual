"""Check baseline source bytes and original staging; no changes to protected files."""
from pathlib import Path
import hashlib,json,subprocess
root=Path(__file__).resolve().parents[1]
backup=Path('D:/Business as Usual Backups/pre-phase7B-2026-10-11')
def digest(path):return hashlib.sha256(path.read_bytes()).hexdigest()
groups={
 'original_sunny_working_sources':['Assets/Characters/Sunny'],
 'original_sunny_unity_sources':['Unity/BusinessAsUsual/Assets/Prototype/Art/Sunny','Unity/BusinessAsUsual/Assets/Prototype/Animation'],
 'shop_gameplay_and_ui':['Unity/BusinessAsUsual/Assets/Shopkeeper'],
 'authority':['Unity/BusinessAsUsual/Assets/Multiplayer/Scripts/NetworkShop.cs','Unity/BusinessAsUsual/Assets/Multiplayer/Scripts/NetworkPlayer.cs','Unity/BusinessAsUsual/Assets/Town/Scripts/TownJobs.cs'],
 'legacy_scenes':['Unity/BusinessAsUsual/Assets/Prototype/Scenes','Unity/BusinessAsUsual/Assets/Multiplayer/Scenes'],
 'accepted_diagnostics':['Docs/Diagnostics/Phase3','Docs/Diagnostics/Phase4','Docs/Diagnostics/Phase4.1','Docs/Diagnostics/Phase4.2','Docs/Diagnostics/Phase5','Docs/Diagnostics/Phase6','Docs/Diagnostics/Phase7A']}
results=[]
for name,paths in groups.items():
    files=[]
    for prefix in paths:
        path=backup/prefix
        files+=([path] if path.is_file() else [p for p in path.rglob('*') if p.is_file()])
    differences=[];checked=0
    for prior in files:
        relative=prior.relative_to(backup)
        # Unity's continuously written recovery log is not an accepted test artifact.
        if relative.as_posix().endswith('Phase6/editor-recovery.log'):continue
        current=root/relative;checked+=1
        if not current.exists() or digest(current)!=digest(prior):differences.append(relative.as_posix())
    results.append(dict(group=name,checked=checked,passed=not differences,differences=differences))
index=subprocess.check_output(['git','write-tree'],cwd=root,text=True).strip()
prior=(backup/'index-tree.txt').read_text(encoding='utf-8-sig').strip()
results.append(dict(group='original_staging_tree',passed=index==prior,before=prior,after=index))
before=json.loads((root/'Docs/Diagnostics/Phase7B/Before/host.json').read_text())
after=json.loads((root/'Docs/Diagnostics/Phase7B/Targeted1080/host.json').read_text())
before_points=dict(zip(before['targetIds'],before['targetPoints']));after_points=dict(zip(after['targetIds'],after['targetPoints']))
# Bag/crate interaction points follow the actual carried/dropped object. Different shifts
# legitimately leave those at different positions; preserve every static station instead.
movable=[key for key in before_points if 'Route Bag ' in key or key.endswith('/Stock Crate') or 'Waste Bag' in key]
changed=[key for key in before_points if key not in movable and before_points[key]!=after_points.get(key)]
results.append(dict(group='functional_target_ids_and_static_points',passed=set(before_points)==set(after_points) and not changed,count=len(before_points),dynamic_points_excluded=movable,changed_static_points=changed))
output=root/'Docs/Diagnostics/Phase7B/preservation.json';output.write_text(json.dumps(dict(backup=str(backup),checks=results),indent=2))
for r in results:print(('PASS' if r['passed'] else 'FAIL')+' '+r['group']+' '+str(r.get('differences','')))
raise SystemExit(0 if all(r['passed'] for r in results) else 1)
