"""Save a new version only; verify protected source and Phase 1 hashes."""
import bpy, json, hashlib
from pathlib import Path

ROOT=Path(globals().get('BAU_PROJECT_ROOT','D:/Business as Usual'))
VERSION=globals().get('BAU_VERSION','v001')
OUT=ROOT/'Docs/Diagnostics/Phase2'/VERSION
inventory=json.loads((OUT/'modular-inventory.json').read_text(encoding='utf-8'))
target=ROOT/f'Assets/Characters/Sunny/Working/Sunny_Phase2_Extraction_{VERSION}.blend'
if target.exists(): raise RuntimeError('Refusing to overwrite a working version; select a new version.')
assert 'bau_phase2_capture_restore' not in bpy.app.driver_namespace,'Restore temporary screenshot state first.'
for item in inventory['sources']:
    assert hashlib.sha256((ROOT/item['path']).read_bytes()).hexdigest()==item['sha256'],item['path']
phase1=ROOT/'Assets/Characters/Sunny/Working/Sunny_Phase1_Audit.blend'
assert hashlib.sha256(phase1.read_bytes()).hexdigest()==inventory['phase1_project_sha256']
previous=bpy.context.window.scene
scene=bpy.data.scenes[inventory['scenes']['rest']]
arm=next(o for o in scene.objects if o.type=='ARMATURE')
assert arm.data.pose_position=='REST'
assert all(not o.hide_render and not o.hide_get() for o in scene.objects if o.type=='MESH')
bpy.context.window.scene=scene
bpy.ops.wm.save_as_mainfile(filepath=str(target),copy=True)
report={'version':VERSION,'project':target.relative_to(ROOT).as_posix(),'bytes':target.stat().st_size,
    'sha256':hashlib.sha256(target.read_bytes()).hexdigest(),
    'phase1_sha256':inventory['phase1_project_sha256'],'source_hashes_verified':9,
    'checkpoint':'extraction complete; reconstruction and rig/weight repairs await approval',
    'active_file_unchanged':bpy.data.filepath,
    'review_scene':scene.name,'mainfile_scope':'current session plus isolated Phase 2 scenes; previous scenes preserved'}
(OUT/'saved-project.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
