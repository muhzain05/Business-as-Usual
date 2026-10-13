"""Validate artifact integrity and preservation, without claiming artistic readiness."""
from pathlib import Path
import json,hashlib,subprocess,re
from PIL import Image
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Docs/Diagnostics/Phase2/v002'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
inventory=json.loads((ROOT/'Docs/Diagnostics/asset-inventory.json').read_text())
for item in inventory:
    p=ROOT/item['path'];assert p.stat().st_size==item['bytes'] and sha(p)==item['sha256'],item['path']
saved=json.loads((OUT/'saved-project.json').read_text())
for path,digest in saved['protected_projects'].items():assert sha(ROOT/path)==digest,path
p=ROOT/saved['project'];assert sha(p)==saved['sha256'] and p.stat().st_size==saved['bytes']
data=json.loads((OUT/'deformation-validation.json').read_text())
assert not data['quality_pass'] and data['rig']['bones']==53 and data['rig']['max_rest_matrix_difference']==0
assert data['upper_outfit_head_thigh_pelvis_weight_sum']==0
for clip,count in [('Walk',26),('Run',17)]:
    assert data['clips'][clip]['frames_checked']==list(range(1,count+1))
    with Image.open(OUT/f'{clip.lower()}-full-comparison.gif') as img:assert img.n_frames==count
    for f in range(1,count+1):
        for label in ['before','after']:
            with Image.open(OUT/f'Images/{label}-{clip.lower()}-front-{f:03}.png') as img:img.verify()
for path in OUT.rglob('*.json'):json.loads(path.read_text())
for path in list((ROOT/'Scripts/Blender').glob('phase2_*.py'))+[ROOT/'Scripts/render_phase2_repair_report.py']:
    compile(path.read_text(encoding='utf-8'),str(path),'exec')
for doc in ['README.md','Docs/phase2-repair-report.md','Docs/character-pipeline.md','Docs/phase2-reconstruction-checkpoint.md']:
    file=ROOT/doc
    for href in re.findall(r'\]\(([^)]+)\)',file.read_text(encoding='utf-8')):
        if not re.match(r'https?://',href):assert (file.parent/href.split('#')[0]).exists(),(doc,href)
required=[i['path'] for i in inventory]+[saved['project']]+[str(p.relative_to(ROOT)).replace('\\','/') for p in OUT.rglob('*') if p.is_file() and p.suffix in ['.png','.gif']]
ignored=subprocess.run(['git','check-ignore','--',*required],cwd=ROOT,text=True,capture_output=True)
assert not ignored.stdout,ignored.stdout
binary=[i['path'] for i in inventory]+[saved['project'],str((OUT/'walk-full-comparison.gif').relative_to(ROOT)).replace('\\','/')]
attrs=subprocess.run(['git','check-attr','filter','--',*binary],cwd=ROOT,text=True,capture_output=True,check=True).stdout
assert all(line.endswith(': lfs') for line in attrs.splitlines()),attrs
assert (ROOT/'.git/hooks/pre-push').exists()
head=subprocess.run(['git','rev-parse','--verify','HEAD'],cwd=ROOT,capture_output=True).returncode
assert head!=0,'Unexpected commit exists.'
assert not subprocess.run(['git','remote'],cwd=ROOT,text=True,capture_output=True,check=True).stdout.strip()
assert not subprocess.run(['git','diff','--cached','--name-only'],cwd=ROOT,text=True,capture_output=True,check=True).stdout.strip()
result={'sources_sha256_verified':9,'protected_projects_verified':2,'repair_project_sha256_verified':True,
        'walk_frames_rendered_and_evaluated':26,'run_frames_rendered_and_evaluated':17,
        'required_assets_ignored':False,'git_lfs_tracking_verified':True,'commits':0,'remotes':0,'staged_files':0,
        'artifact_integrity_pass':True,'character_quality_pass':False,'next_phase_ready':False}
(OUT/'artifact-validation.json').write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2))
