"""Save new prototype only after source/audit hashes and skeleton checks pass."""
import bpy,json,hashlib
from pathlib import Path
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v002'
PROJECT=ROOT/'Assets/Characters/Sunny/Working/Sunny_Phase2_Repair_v002.blend'
assert not PROJECT.exists(),'Refusing to overwrite a saved repair version.'
inventory=json.loads((ROOT/'Docs/Diagnostics/asset-inventory.json').read_text())
for item in inventory:assert hashlib.sha256((ROOT/item['path']).read_bytes()).hexdigest()==item['sha256']
protected={'Assets/Characters/Sunny/Working/Sunny_Phase1_Audit.blend':'965b0370c530bc46105beffe580a17e1ddf805785b77f88365f6a57f13633df0',
           'Assets/Characters/Sunny/Working/Sunny_Phase2_Extraction_v001.blend':'75fa375d8080389a5fe9bb6cc8ec423decf04a330f451f87b73bbcdb6bc95afe'}
for path,digest in protected.items():assert hashlib.sha256((ROOT/path).read_bytes()).hexdigest()==digest
source=bpy.data.scenes['P2_v002_Repair'];rig=next(o for o in source.objects if o.type=='ARMATURE')
assert len(rig.data.bones)==53
for label in ['Walk','Run']:
    name='P2_v002_'+label+'_Review'
    assert name not in bpy.data.scenes
    s=bpy.data.scenes.new(name);s.world=source.world;s.render.fps=24;s.unit_settings.scale_length=1;s.render.engine=source.render.engine
    copies={}
    for old in source.objects:
        if old.get('bau_nonproduction'):continue
        obj=old.copy();obj.name=name+'_'+old.name
        if obj.type=='ARMATURE':obj.data=old.data.copy()
        s.collection.objects.link(obj);copies[old]=obj
    for old,obj in copies.items():
        obj.parent=copies.get(old.parent)
        for mod in obj.modifiers:
            if mod.type=='ARMATURE':mod.object=copies.get(mod.object,mod.object)
    arm=next(o for o in s.objects if o.type=='ARMATURE')
    ref=next(o for o in bpy.data.scenes['P2_v001_Reference_'+label].objects if o.type=='ARMATURE')
    arm.animation_data.action=ref.animation_data.action;arm.animation_data.action_slot=ref.animation_data.action_slot;arm.data.pose_position='POSE'
    for bone in arm.pose.bones:bone.rotation_mode='QUATERNION'
    s.frame_start,s.frame_end=map(int,arm.animation_data.action.frame_range);s.frame_set(s.frame_start)
    bpy.context.window.scene=s;arm.hide_set(True)
for img in bpy.data.images:
    if img.name.startswith('Sunny_v002_Head') and img.packed_file:
        img.filepath=bpy.path.relpath(img.filepath,start=str(PROJECT.parent))
bpy.context.window.scene=source;rig.data.pose_position='REST';source.frame_set(1);rig.hide_set(True)
source['bau_quality_pass']=False;source['bau_readiness']='Prototype only; visible clothing/joint defects remain. See Docs/phase2-repair-report.md.'
source['bau_default_masks']='Occlusion trial disabled to expose remaining clipping and gaps.'
bpy.ops.wm.save_as_mainfile(filepath=str(PROJECT),copy=True)
result={'version':'v002','project':str(PROJECT.relative_to(ROOT)).replace('\\','/'),'bytes':PROJECT.stat().st_size,
        'sha256':hashlib.sha256(PROJECT.read_bytes()).hexdigest(),'source_hashes_verified':9,'protected_projects':protected,
        'quality_pass':False,'review_scene':source.name,'walk_scene':'P2_v002_Walk_Review','run_scene':'P2_v002_Run_Review'}
(OUT/'saved-project.json').write_text(json.dumps(result,indent=2));print(json.dumps(result))
