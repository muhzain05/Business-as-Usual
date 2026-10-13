"""Save a new project once, with review scenes and immutable upper-body checks."""
import bpy,json,hashlib,re
from pathlib import Path
from mathutils import Matrix
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v003'
target=ROOT/'Assets/Characters/Sunny/Working/Sunny_LegRepair_v003.blend'
assert not target.exists(),'Refuse overwrite: choose a new version.'
base=bpy.data.scenes['P2_v003_Before'];scene=bpy.data.scenes['P2_v003_LegRepair']
br=next(o for o in base.objects if o.type=='ARMATURE');rig=next(o for o in scene.objects if o.type=='ARMATURE')
assert len(rig.data.bones)==53
for b in rig.data.bones:
    original=br.data.bones[b.name]
    assert (b.parent.name if b.parent else None)==(original.parent.name if original.parent else None)
    assert b.matrix_local==original.matrix_local
protected={}
for old in base.objects:
    role=old.name.removeprefix('Before_v003_')
    if old.type!='MESH' or role.startswith(('LowerLeg','Shorts','Shoe','Body_ConcealedFoot')):continue
    new=next(o for o in scene.objects if o.name=='Sunny_v003_'+role)
    def digest(o):
        d={'vertices':[list(v.co) for v in o.data.vertices],'faces':[list(p.vertices) for p in o.data.polygons],'normals':[list(n.vector) for n in o.data.corner_normals],'uv':[[list(p.uv) for p in layer.data] for layer in o.data.uv_layers],'weights':[[[o.vertex_groups[g.group].name,g.weight] for g in v.groups] for v in o.data.vertices],'material_names':[m.name for m in o.data.materials],'smooth':[p.use_smooth for p in o.data.polygons],'matrix':[list(r) for r in o.matrix_world]}
        return hashlib.sha256(json.dumps(d,sort_keys=True).encode()).hexdigest()
    assert digest(old)==digest(new),role
    protected[role]=digest(new)
for label in ['Walk','Run']:
    name='P2_v003_'+label+'_Review'
    assert name not in bpy.data.scenes
    review=bpy.data.scenes.new(name);review.world=scene.world;review.render.fps=24
    copies={}
    for old in scene.objects:
        obj=old.copy();obj.name=name+'_'+old.name
        if old.type=='ARMATURE' or old.name.endswith('Shorts'):obj.data=old.data.copy()
        review.collection.objects.link(obj);copies[old]=obj
    for old,obj in copies.items():
        obj.parent=copies.get(old.parent)
        for m in obj.modifiers:
            if m.type=='ARMATURE':m.object=copies.get(m.object,m.object)
        if obj.type=='MESH' and obj.data.shape_keys and obj.data.shape_keys.animation_data:
            for fc in obj.data.shape_keys.animation_data.drivers:
                for var in fc.driver.variables:
                    for t in var.targets:
                        if t.id in copies:t.id=copies[t.id]
    r=copies[rig];r.data.pose_position='POSE';r.animation_data.action=bpy.data.actions[scene['bau_'+label+'_action']];r.animation_data.action_slot=r.animation_data.action.slots[0]
    review.frame_start=1;review.frame_end=26 if label=='Walk' else 17;review.frame_set(1)
    for obj in review.objects:obj.hide_set(obj.type=='ARMATURE' or obj.get('bau_nonproduction',False),view_layer=review.view_layers[0])
rig.animation_data.action=None;rig.data.pose_position='REST'
for b in rig.pose.bones:b.matrix_basis=Matrix.Identity(4)
scene.frame_set(1);bpy.context.window.scene=scene
scene['bau_leg_status']='REVIEW REQUIRED: residual cuff clipping and deep-flexion shorts compression; not production ready.'
bpy.ops.wm.save_as_mainfile(filepath=str(target),copy=True)
inventory={'file':str(target.relative_to(ROOT)),'sha256':hashlib.sha256(target.read_bytes()).hexdigest(),'bones':53,'working_height':1.2,'protected_mesh_hashes':protected,'modules':[{'name':o.name,'vertices':len(o.data.vertices),'polygons':len(o.data.polygons),'groups':[g.name for g in o.vertex_groups]} for o in scene.objects if o.type=='MESH'],'status':scene['bau_leg_status']}
(OUT/'saved-project.json').write_text(json.dumps(inventory,indent=2))
print(json.dumps({'saved':str(target),'sha256':inventory['sha256'],'protected_meshes':len(protected)}))
