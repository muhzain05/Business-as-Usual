"""Create playback scenes and save a new version exactly once."""
import bpy,json,hashlib
from pathlib import Path
from mathutils import Matrix
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v004';TARGET=ROOT/'Assets/Characters/Sunny/Working/Sunny_Locomotion_v004.blend'
assert not TARGET.exists(),'Refuse overwrite; select a new version for reruns.'
scene=bpy.data.scenes['P2_v004_Locomotion'];rig=next(o for o in scene.objects if o.type=='ARMATURE')
validation=json.loads((OUT/'locomotion-validation.json').read_text());assert validation['geometry_weights_uv_material_shape_keys_unchanged'] and validation['bones']==53
for clip,N in [('Walk',32),('Run',24)]:
    name='P2_v004_'+clip+'_Review';assert name not in bpy.data.scenes
    s=bpy.data.scenes.new(name);s.world=scene.world;s.render.engine=scene.render.engine;s.render.fps=24;copies={}
    for old in scene.objects:
        obj=old.copy();obj.name=name+'_'+old.name
        if old.type=='ARMATURE' or old.name.endswith('Shorts'):obj.data=old.data.copy()
        s.collection.objects.link(obj);copies[old]=obj
    for old,obj in copies.items():
        obj.parent=copies.get(old.parent)
        for m in obj.modifiers:
            if m.type=='ARMATURE':m.object=copies.get(m.object,m.object)
        if obj.type=='MESH' and obj.data.shape_keys and obj.data.shape_keys.animation_data:
            for fc in obj.data.shape_keys.animation_data.drivers:
                for var in fc.driver.variables:
                    for t in var.targets:
                        if t.id in copies:t.id=copies[t.id]
    r=copies[rig];r.data.pose_position='POSE';r.animation_data.action=bpy.data.actions[scene['bau_'+clip+'_action']];r.animation_data.action_slot=r.animation_data.action.slots[0];s.frame_start=1;s.frame_end=N;s.frame_set(1)
    for o in s.objects:o.hide_set(o.type=='ARMATURE' or o.get('bau_nonproduction',False),view_layer=s.view_layers[0])
keep={scene['bau_'+clip+'_action'] for clip in ['Walk','Run']}
for a in list(bpy.data.actions):
    if a.name.startswith('Sunny_v004_') and a.name not in keep and a.users==0:bpy.data.actions.remove(a)
rig.animation_data.action=None
for b in rig.pose.bones:b.matrix_basis=Matrix.Identity(4)
rig.data.pose_position='REST';scene.frame_set(1);bpy.context.window.scene=scene
scene['bau_v004_status']='Motion review candidate. Residual shorts/calf contact and knee folding; not production ready.'
assert all(i.packed_file for i in bpy.data.images if i.source=='FILE')
bpy.ops.wm.save_as_mainfile(filepath=str(TARGET),copy=True)
saved={'file':str(TARGET.relative_to(ROOT)),'bytes':TARGET.stat().st_size,'sha256':hashlib.sha256(TARGET.read_bytes()).hexdigest(),'status':scene['bau_v004_status'],'bones':53,'working_height':1.2,'protected_mesh_digests':validation['protected_mesh_digests'],'all_file_textures_packed':True,'review_scenes':['P2_v004_Walk_Review','P2_v004_Run_Review'],'actions':dict((c,scene['bau_'+c+'_action']) for c in ['Walk','Run'])}
(OUT/'saved-project.json').write_text(json.dumps(saved,indent=2));print(json.dumps({'file':saved['file'],'bytes':saved['bytes'],'sha256':saved['sha256'],'status':saved['status']}))
