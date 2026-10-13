"""Read-only checkpoint integrity checks, apart from writing a verification report."""
import argparse, hashlib, json, re, subprocess
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser(); parser.add_argument('--version',default='v001'); args=parser.parse_args()
OUT=ROOT/'Docs/Diagnostics/Phase2'/args.version
inventory=json.loads((OUT/'modular-inventory.json').read_text(encoding='utf-8'))
saved=json.loads((OUT/'saved-project.json').read_text(encoding='utf-8'))
for item in inventory['sources']:
    p=ROOT/item['path']; assert p.stat().st_size==item['bytes'],p
    assert hashlib.sha256(p.read_bytes()).hexdigest()==item['sha256'],p
    result=subprocess.run(['git','-C',str(ROOT),'check-ignore',str(p)],capture_output=True)
    assert result.returncode==1,f'Source ignored or git check failed: {p}'
phase1=ROOT/'Assets/Characters/Sunny/Working/Sunny_Phase1_Audit.blend'
assert hashlib.sha256(phase1.read_bytes()).hexdigest()==inventory['phase1_project_sha256']
project=ROOT/saved['project']
assert project.stat().st_size==saved['bytes']
assert hashlib.sha256(project.read_bytes()).hexdigest()==saved['sha256']
validation=inventory['extraction_validation']; assert validation['passed']
assert validation['vertices_total']==7897 and validation['triangles_total']==15304 and len(inventory['modules'])==15
vertex_ids=[i for m in inventory['modules'] for i in m['original_vertex_ids']]
face_ids=[i for m in inventory['modules'] for i in m['original_polygon_ids']]
assert sorted(vertex_ids)==list(range(7897)) and sorted(face_ids)==list(range(15304))
assert validation['max_uv_error']==0 and validation['max_rest_position_error']==0 and validation['max_weight_error']==0
assert validation['max_corner_normal_error']<.001 and validation['material_slots_preserved']
animation=json.loads((OUT/'extraction-animation-validation.json').read_text(encoding='utf-8'))
for label,count in [('Walk',26),('Run',17)]:
    clip=animation['clips'][label]
    assert clip['frames_checked']==list(range(1,count+1))
    assert clip['passed_extraction_equivalence'] and clip['max_position_difference']<1e-5
    assert not clip['missing_bone_paths']
images=['extracted-modules.png','head-shell-baked-face.png','before-rest-views.png',
    'after-extraction-rest-views.png','reconstruction-boundaries.png','before-after-extraction.png',
    'walk_before.png','walk_after.png','run_before.png','run_after.png','walk-run-extraction-comparison.png']
for name in images:
    with Image.open(OUT/name) as im: im.verify()
for p in OUT.glob('*.json'): json.loads(p.read_text(encoding='utf-8'))
for p in (ROOT/'Scripts').rglob('*.py'): compile(p.read_text(encoding='utf-8'),str(p),'exec')
for p in [ROOT/'README.md',*(ROOT/'Docs').glob('*.md')]:
    for target in re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8')):
        if '://' not in target and not target.startswith('#'):
            assert (p.parent/target.split('#')[0]).exists(),f'Broken link: {p}: {target}'
paths=[item['path'] for item in inventory['sources']]+[saved['project']]
attributes=subprocess.check_output(['git','-C',str(ROOT),'check-attr','filter','--',*paths],text=True)
assert len(attributes.splitlines())==len(paths) and all(line.endswith(': lfs') for line in attributes.splitlines())
assert (ROOT/'.git/hooks/pre-push').is_file(),'LFS pre-push hook missing'
head=subprocess.run(['git','-C',str(ROOT),'rev-parse','--verify','HEAD'],capture_output=True)
assert head.returncode!=0,'A commit now exists; approval boundary needs review.'
assert not subprocess.check_output(['git','-C',str(ROOT),'remote'],text=True).strip()
report={'version':args.version,'source_hashes_verified':9,'phase1_project_hash_unchanged':True,
    'phase2_project_hash_verified':True,'module_partition_exact':True,'uv_material_alignment_preserved':True,
    'custom_normals_within_tolerance':True,'walk_frames_equivalent':26,'run_frames_equivalent':17,
    'images_verified':len(images),'scripts_compile':True,'documentation_links_resolve':True,
    'lfs_attributes_and_hook_verified':True,'no_commit':True,'no_remote':True,
    'readiness':'extraction checkpoint only; reconstruction and deformation repairs pending approval'}
(OUT/'checkpoint-validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
