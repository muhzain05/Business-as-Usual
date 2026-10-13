"""Read-only source/project preservation and artifact checks; no quality-pass assertion."""
import json,hashlib,subprocess,re
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Docs/Diagnostics/Phase2/v003'
sources=json.loads((ROOT/'Docs/Diagnostics/asset-inventory.json').read_text())
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
for item in sources:
    p=ROOT/item['path'];assert p.stat().st_size==item['bytes'];assert sha(p)==item['sha256'],p
protected={'Sunny_Phase1_Audit.blend':'965b0370c530bc46105beffe580a17e1ddf805785b77f88365f6a57f13633df0','Sunny_Phase2_Extraction_v001.blend':'75fa375d8080389a5fe9bb6cc8ec423decf04a330f451f87b73bbcdb6bc95afe','Sunny_Phase2_Repair_v002.blend':'d00d1f0dc2da21469b11262277dfd1857438d7cc1676bdb113a7bf51249b4926'}
for name,h in protected.items():assert sha(ROOT/'Assets/Characters/Sunny/Working'/name)==h,name
saved=json.loads((OUT/'saved-project.json').read_text());assert sha(ROOT/saved['file'])==saved['sha256'];assert saved['bones']==53 and saved['working_height']==1.2
v=json.loads((OUT/'leg-validation.json').read_text())
for label,n in [('Walk',26),('Run',17)]:
    clip=v['clips'][label];assert [r['frame'] for r in clip['integer_frames']]==list(range(1,n+1));assert len(clip['quarter_frames'])==(n-1)*3
    assert clip['upper_max_world_position_difference']==0;assert not clip['shoe_collision_frames']
    for view in ['front','side','three_quarter']:
        with Image.open(OUT/f'{label.lower()}-{view}-comparison.gif') as im:assert im.n_frames==n
controls=json.loads((OUT/'controlled-pose-validation.json').read_text());assert len(controls['poses'])==12
image_count=0
for p in OUT.rglob('*.png'):
    with Image.open(p) as im:im.verify()
    image_count+=1
assert len(list((OUT/'Images').glob('*.png')))==300
for p in OUT.glob('*.json'):json.loads(p.read_text())
for p in (ROOT/'Scripts').rglob('*.py'):compile(p.read_text(encoding='utf-8'),str(p),'exec')
for doc in [ROOT/'README.md',ROOT/'Docs/leg-deformation-repair.md']:
    for target in re.findall(r'\]\(([^)]+)\)',doc.read_text(encoding='utf-8')):
        if '://' not in target and not target.startswith('#'):assert (doc.parent/target.split('#')[0]).exists(),target
paths=[x['path'] for x in sources]+[saved['file']]+[str(p.relative_to(ROOT)) for p in OUT.rglob('*') if p.suffix in ['.png','.gif']]
attrs=subprocess.check_output(['git','check-attr','filter','--',*paths],cwd=ROOT,text=True)
assert all(s.endswith(': lfs') for s in attrs.splitlines()) and len(attrs.splitlines())==len(paths)
for p in paths:assert subprocess.run(['git','check-ignore',p],cwd=ROOT,capture_output=True).returncode==1,p
assert not subprocess.check_output(['git','diff','--cached','--name-only'],cwd=ROOT,text=True).strip()
assert not subprocess.check_output(['git','remote'],cwd=ROOT,text=True).strip()
assert subprocess.run(['git','rev-parse','--verify','HEAD'],cwd=ROOT,capture_output=True).returncode!=0
report={'nine_original_source_hashes_verified':len(sources),'prior_projects_hash_unchanged':list(protected),'v003_saved_hash_verified':True,'protected_meshes':len(saved['protected_mesh_hashes']),'all_animation_samples':166,'renders':300,'png_artifacts_verified':image_count,'gif_clips_verified':6,'controlled_pose_versions':12,'lfs_tracking_verified':True,'required_assets_ignored':False,'staged_files':0,'commits':0,'remotes':0,'quality_gate':'FAILED: residual shorts contact and deep-flexion collapse. Integrity checks are not a visual pass.'}
(OUT/'artifact-verification.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
