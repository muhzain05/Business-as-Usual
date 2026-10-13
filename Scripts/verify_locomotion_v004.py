"""Version preservation, previews and motion diagnostics. Integrity is not a quality pass."""
from pathlib import Path
import json,hashlib,subprocess,re
from PIL import Image
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Docs/Diagnostics/Phase2/v004'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
source=json.loads((ROOT/'Docs/Diagnostics/asset-inventory.json').read_text())
for r in source:assert sha(ROOT/r['path'])==r['sha256'] and (ROOT/r['path']).stat().st_size==r['bytes'],r['path']
old={'Sunny_Phase1_Audit.blend':'965b0370c530bc46105beffe580a17e1ddf805785b77f88365f6a57f13633df0','Sunny_Phase2_Extraction_v001.blend':'75fa375d8080389a5fe9bb6cc8ec423decf04a330f451f87b73bbcdb6bc95afe','Sunny_Phase2_Repair_v002.blend':'d00d1f0dc2da21469b11262277dfd1857438d7cc1676bdb113a7bf51249b4926','Sunny_LegRepair_v003.blend':'866c1026badab8f902fdb1908492b56f0e3b105c8fb3e8bd5376cf80aeef682a'}
for name,h in old.items():assert sha(ROOT/'Assets/Characters/Sunny/Working'/name)==h,name
saved=json.loads((OUT/'saved-project.json').read_text());assert sha(ROOT/saved['file'])==saved['sha256'];assert saved['bones']==53 and saved['working_height']==1.2
reopened=json.loads((OUT/'saved-file-reopen-check.json').read_text());assert reopened['all_file_images_packed']
for c,N in [('Walk',32),('Run',24)]:
    check=reopened['checks'][c];assert check['action']==saved['actions'][c] and check['range']==[1,N] and check['moving_feet'] and check['cuff_drivers_target_review_rig']
v=json.loads((OUT/'locomotion-validation.json').read_text());assert v['geometry_weights_uv_material_shape_keys_unchanged']
for c,N in [('Walk',32),('Run',24)]:
    assert len(v['clips'][c]['samples'])==N*4+1;assert v['clips'][c]['seam_vertex_error']<1e-5;assert not v['clips'][c]['shoe_pair_intersection_frames'];assert v['clips'][c]['floor_min']>=0
manifest=json.loads((OUT/'preview-manifest.json').read_text());assert len(manifest['previews'])==8
for p in manifest['previews']:
    with Image.open(OUT/p['file']) as im:
        assert im.n_frames==p['frames'];total=0
        for f in range(im.n_frames):im.seek(f);total+=im.info['duration']
        assert total==p['duration_ms'] and p['loops_encoded']==2
for p in OUT.rglob('*.png'):
    with Image.open(p) as im:im.verify()
for p in OUT.glob('*.json'):json.loads(p.read_text())
for p in (ROOT/'Scripts').rglob('*.py'):compile(p.read_text(encoding='utf-8'),str(p),'exec')
for doc in [ROOT/'Docs/locomotion-v004.md',ROOT/'README.md']:
    for target in re.findall(r'\]\(([^)]+)\)',doc.read_text(encoding='utf-8')):
        if '://' not in target and not target.startswith('#'):assert (doc.parent/target.split('#')[0]).exists(),target
paths=[r['path'] for r in source]+[saved['file']]+[str(p.relative_to(ROOT)) for p in OUT.rglob('*') if p.suffix in ['.png','.gif']]
attrs=subprocess.check_output(['git','check-attr','filter','--',*paths],cwd=ROOT,text=True);assert len(attrs.splitlines())==len(paths) and all(s.endswith(': lfs') for s in attrs.splitlines())
for p in paths:assert subprocess.run(['git','check-ignore',p],cwd=ROOT,capture_output=True).returncode==1,p
assert not subprocess.check_output(['git','diff','--cached','--name-only'],cwd=ROOT,text=True).strip();assert not subprocess.check_output(['git','remote'],cwd=ROOT,text=True).strip();assert subprocess.run(['git','rev-parse','--verify','HEAD'],cwd=ROOT,capture_output=True).returncode!=0
report={'sources_sha256_verified':len(source),'prior_projects_sha256_unchanged':list(old),'v004_saved_hash_verified':True,'saved_project_reopened_and_review_rigs_verified':True,'protected_meshes':len(v['protected_mesh_digests']),'animation_samples':226,'two_loop_animated_previews':8,'lfs_tracking_and_required_assets_verified':True,'scripts_compile':True,'documentation_links_valid':True,'commits':0,'staged_files':0,'remotes':0,'quality_gate':'Review candidate: residual shorts contact and knee folding. Motion metrics are not a production-quality pass.'}
(OUT/'artifact-verification.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
