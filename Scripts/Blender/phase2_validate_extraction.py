"""Compare extracted modules to the unchanged animation references at every integer frame."""
import bpy, json, re
from pathlib import Path
from mathutils import Vector
from mathutils.kdtree import KDTree

ROOT=Path(globals().get('BAU_PROJECT_ROOT','D:/Business as Usual'))
VERSION=globals().get('BAU_VERSION','v001'); PREFIX='P2_'+VERSION+'_'
OUT=ROOT/'Docs/Diagnostics/Phase2'/VERSION
inventory=json.loads((OUT/'modular-inventory.json').read_text(encoding='utf-8'))
scene=bpy.data.scenes[inventory['scenes']['rest']]
arm=next(o for o in scene.objects if o.type=='ARMATURE')
modules=[bpy.data.objects[m['name']] for m in inventory['modules']]
prior=bpy.context.window.scene
saved=(scene.frame_current,arm.data.pose_position,arm.animation_data.action,arm.animation_data.action_slot)
result={'version':VERSION,'purpose':'extraction equivalence, not a deformation-repair claim','clips':{},'rig_rest_matrices_unchanged':True}
try:
    for label in ['Walk','Run']:
        ref=bpy.data.scenes[inventory['scenes']['references'][label]]
        ref_arm=next(o for o in ref.objects if o.type=='ARMATURE')
        ref_mesh=next(o for o in ref.objects if o.type=='MESH' and o.vertex_groups)
        action=ref_arm.animation_data.action
        for b in arm.data.bones:
            other=ref_arm.data.bones[b.name]
            error=max(abs(b.matrix_local[i][j]-other.matrix_local[i][j]) for i in range(4) for j in range(4))
            assert error<1e-6,(b.name,error)
            assert (b.parent.name if b.parent else None)==(other.parent.name if other.parent else None)
        paths={c.data_path for c in ([c for layer in action.layers for strip in layer.strips
            if hasattr(strip,'channelbags') for bag in strip.channelbags for c in bag.fcurves])}
        missing=[path for path in paths if path.startswith('pose.bones[') and re.findall(r'pose.bones\["([^"]+)"\]',path)[0] not in arm.data.bones]
        assert not missing,missing
        kd=KDTree(len(ref_mesh.data.vertices))
        for v in ref_mesh.data.vertices: kd.insert(ref_mesh.matrix_world@v.co,v.index)
        kd.balance(); module_map={}
        for obj in modules:
            module_map[obj.name]=[kd.find(obj.matrix_world@v.co)[1] for v in obj.data.vertices]
        lo,hi=map(int,action.frame_range)
        original_ref=(ref.frame_current,ref_arm.data.pose_position)
        clip={'action':action.name,'frames_checked':list(range(lo,hi+1)),'missing_bone_paths':missing,'frames':[]}
        try:
            ref_arm.data.pose_position='POSE'
            arm.data.pose_position='POSE'; arm.animation_data.action=action
            arm.animation_data.action_slot=ref_arm.animation_data.action_slot
            for frame in range(lo,hi+1):
                bpy.context.window.scene=ref; ref.frame_set(frame); bpy.context.view_layer.update()
                ev=ref_mesh.evaluated_get(bpy.context.evaluated_depsgraph_get()); temp=ev.to_mesh()
                ref_positions=[ev.matrix_world@v.co for v in temp.vertices]; ev.to_mesh_clear()
                bpy.context.window.scene=scene; scene.frame_set(frame); bpy.context.view_layer.update()
                maximum=0; ratios=[]
                for obj in modules:
                    ev=obj.evaluated_get(bpy.context.evaluated_depsgraph_get()); temp=ev.to_mesh()
                    mapped=module_map[obj.name]
                    maximum=max(maximum,max((ev.matrix_world@v.co-ref_positions[mapped[v.index]]).length for v in temp.vertices))
                    for edge in obj.data.edges:
                        i,j=edge.vertices; L=(obj.data.vertices[i].co-obj.data.vertices[j].co).length
                        if L>1e-7: ratios.append((temp.vertices[i].co-temp.vertices[j].co).length/L)
                    ev.to_mesh_clear()
                ratios.sort()
                clip['frames'].append({'frame':frame,'max_position_difference':maximum,
                    'p99_edge_ratio':ratios[round((len(ratios)-1)*.99)],'max_edge_ratio':max(ratios),
                    'edges_over_1_5':sum(v>1.5 for v in ratios),'edges_under_0_5':sum(v<.5 for v in ratios)})
        finally:
            ref_arm.data.pose_position=original_ref[1]; ref.frame_set(original_ref[0])
        clip['max_position_difference']=max(f['max_position_difference'] for f in clip['frames'])
        assert clip['max_position_difference']<1e-5,clip['max_position_difference']
        clip['passed_extraction_equivalence']=True
        result['clips'][label]=clip
finally:
    bpy.context.window.scene=scene
    arm.animation_data.action=saved[2]; arm.animation_data.action_slot=saved[3]
    arm.data.pose_position=saved[1]; scene.frame_set(saved[0]); bpy.context.view_layer.update()
    bpy.context.window.scene=prior
(OUT/'extraction-animation-validation.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps({'clips':{k:{'frames_checked':len(v['frames_checked']),
    'max_position_difference':v['max_position_difference'],'passed':v['passed_extraction_equivalence'],
    'worst_p99':max(f['p99_edge_ratio'] for f in v['frames']),
    'worst_max_strain':max(f['max_edge_ratio'] for f in v['frames'])} for k,v in result['clips'].items()},
    'note':'original deformation defects remain; no repair implemented'},indent=2))
