"""Full-frame cage diagnostics, controls and exact skeleton compatibility.
Changed topology metrics are descriptive, not equivalent surface-error claims.
"""
import bpy,json,math,re
from pathlib import Path
from mathutils import Vector
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v002'
s=bpy.data.scenes['P2_v002_Repair'];baseline=bpy.data.scenes['P2_v001_Modules_Rest']
r=next(o for o in s.objects if o.type=='ARMATURE');br=next(o for o in baseline.objects if o.type=='ARMATURE')
prior=bpy.context.window.scene
pose_saved={b.name:(b.rotation_mode,b.matrix_basis.copy()) for b in r.pose.bones}
for b in r.pose.bones:b.rotation_mode=br.pose.bones[b.name].rotation_mode
saved=[(arm,arm.animation_data.action,arm.animation_data.action_slot,arm.data.pose_position) for arm in [r,br]]
mods=[(m,m.show_viewport) for obj in s.objects if obj.type=='MESH' for m in obj.modifiers if m.type!='ARMATURE']
for m,value in mods:m.show_viewport=False
def summarize(obj):
    ev=obj.evaluated_get(bpy.context.evaluated_depsgraph_get());mesh=ev.to_mesh();ratios=[];areas=[]
    for e in obj.data.edges:
        a,b=e.vertices;length=(obj.data.vertices[a].co-obj.data.vertices[b].co).length
        if length>1e-7:ratios.append((mesh.vertices[a].co-mesh.vertices[b].co).length/length)
    for p in obj.data.polygons:
        if p.area>1e-9:areas.append(mesh.polygons[p.index].area/p.area)
    coords=[ev.matrix_world@v.co for v in mesh.vertices];ev.to_mesh_clear();ratios.sort();areas.sort()
    return {'p99_edge_ratio':ratios[round((len(ratios)-1)*.99)] if ratios else 1,'max_edge_ratio':max(ratios,default=1),
            'edges_over_1_5':sum(x>1.5 for x in ratios),'edges_under_0_5':sum(x<.5 for x in ratios),
            'min_area_ratio':min(areas,default=1),'p99_area_ratio':areas[round((len(areas)-1)*.99)] if areas else 1,
            'min_z':min((v.z for v in coords),default=0),'max_z':max((v.z for v in coords),default=0)}
report={'version':'v002','method':'Base cages; subdivision/thickness/visibility masks disabled during strain measurements. All integer clip frames evaluated. Rebuilt topology differs, so ratios alone cannot establish improved quality.',
        'rig':{},'clips':{},'controlled_poses':{},'quality_pass':False,
        'visible_failures':['Shoulder/sleeve collapse and skin/cuff gaps in run poses','Shorts knee-opening gaps and pinched knee silhouette','Body is not a welded production skin','Uniform jacket materials approximate palette but do not preserve original texture variation']}
boneerror=max(abs(b.matrix_local[i][j]-br.data.bones[b.name].matrix_local[i][j]) for b in r.data.bones for i in range(4) for j in range(4))
parents=all((b.parent.name if b.parent else None)==(br.data.bones[b.name].parent.name if br.data.bones[b.name].parent else None) for b in r.data.bones)
assert boneerror<1e-6 and parents and len(r.data.bones)==53
report['rig']={'bones':53,'max_rest_matrix_difference':boneerror,'hierarchy_unchanged':parents,'working_height_units':1.2,'animation_paths_valid':True}
try:
    for clip in ['Walk','Run']:
        ref=next(o for o in bpy.data.scenes['P2_v001_Reference_'+clip].objects if o.type=='ARMATURE')
        action=ref.animation_data.action
        paths=[c.data_path for l in action.layers for st in l.strips if hasattr(st,'channelbags') for bag in st.channelbags for c in bag.fcurves]
        missing=[p for p in paths if p.startswith('pose.bones[') and re.findall(r'pose.bones\["([^"]+)"\]',p)[0] not in r.data.bones]
        assert not missing
        for arm in [r,br]:arm.animation_data.action=action;arm.animation_data.action_slot=ref.animation_data.action_slot;arm.data.pose_position='POSE'
        lo,hi=map(int,action.frame_range);result={'frames_checked':list(range(lo,hi+1)),'missing_paths':missing,'frames':[]}
        for f in range(lo,hi+1):
            row={'frame':f,'before':{},'after':{}}
            for scene,label in [(baseline,'before'),(s,'after')]:
                bpy.context.window.scene=scene;scene.frame_set(f);bpy.context.view_layer.update()
                for obj in scene.objects:
                    if obj.type=='MESH':row[label][obj.name.split('Sunny_')[1].split('_',1)[1]]=summarize(obj)
            result['frames'].append(row)
        for label in ['before','after']:
            result[label+'_summary']={name:{metric:max(row[label][name][metric] for row in result['frames']) for metric in ['p99_edge_ratio','max_edge_ratio','edges_over_1_5','p99_area_ratio']} for name in result['frames'][0][label]}
        report['clips'][clip]=result
    bpy.context.window.scene=s;r.animation_data.action=None;r.data.pose_position='POSE'
    def reset():
        for b in r.pose.bones:b.rotation_mode='XYZ';b.rotation_euler=(0,0,0);b.location=(0,0,0);b.scale=(1,1,1)
    poses={'arm_raise':{'LeftArm':(0,0,-70),'RightArm':(0,0,70)},
           'crouch':{'LeftUpLeg':(-45,0,0),'RightUpLeg':(-45,0,0),'LeftLeg':(75,0,0),'RightLeg':(75,0,0),'Spine':(15,0,0)},
           'leg_bend':{'LeftLeg':(90,0,0),'RightLeg':(90,0,0)},
           'ankle_flex':{'LeftFoot':(25,0,0),'RightFoot':(-25,0,0)}}
    for name,rotations in poses.items():
        reset()
        for bone,angles in rotations.items():r.pose.bones['mixamorig:'+bone].rotation_euler=[math.radians(a) for a in angles]
        if name=='arm_raise':
            for side,desired in [('Left',Vector((.7,0,.7))),('Right',Vector((-.7,0,.7)))]:
                bone=r.pose.bones['mixamorig:'+side+'Arm']
                rest=bone.bone.matrix_local.to_quaternion()
                world=(bone.bone.tail_local-bone.bone.head_local).rotation_difference(desired)
                bone.rotation_mode='QUATERNION';bone.rotation_quaternion=rest.inverted()@world@rest
        s.frame_set(1);bpy.context.view_layer.update()
        report['controlled_poses'][name]={'local_euler_degrees':rotations,'meshes':{o.name:summarize(o) for o in s.objects if o.type=='MESH'}}
    reset()
finally:
    for b in r.pose.bones:
        b.rotation_mode=pose_saved[b.name][0];b.matrix_basis=pose_saved[b.name][1]
    for arm,action,slot,pose in saved:
        arm.animation_data.action=action;arm.animation_data.action_slot=slot;arm.data.pose_position=pose
    for m,value in mods:m.show_viewport=value
    bpy.context.window.scene=s;r.data.pose_position='REST';s.frame_set(1)
inventory=[]
for obj in s.objects:
    if obj.type!='MESH':continue
    weights=[sum(x.weight for x in v.groups if obj.vertex_groups[x.group].name in r.data.bones) for v in obj.data.vertices]
    inventory.append({'name':obj.name,'vertices':len(obj.data.vertices),'polygons':len(obj.data.polygons),
        'materials':[m.name for m in obj.data.materials],'modifiers':[m.type for m in obj.modifiers],
        'deform_weight_sum_max_error':max(abs(w-1) for w in weights),'shape_keys':[k.name for k in obj.data.shape_keys.key_blocks] if obj.data.shape_keys else []})
assert all(i['deform_weight_sum_max_error']<1e-5 for i in inventory)
leak=sum(x.weight for v in bpy.data.objects['Sunny_v002_UpperOutfit'].data.vertices for x in v.groups if any(n in bpy.data.objects['Sunny_v002_UpperOutfit'].vertex_groups[x.group].name for n in ['Head','Leg','Hips']))
report['upper_outfit_head_thigh_pelvis_weight_sum']=leak
assert leak==0
(OUT/'modular-inventory.json').write_text(json.dumps(inventory,indent=2))
(OUT/'deformation-validation.json').write_text(json.dumps(report,indent=2))
print(json.dumps({'rig':report['rig'],'leakage':leak,'clips':{k:{'frames':len(v['frames']),'outfit_before':v['before_summary']['UpperOutfit'],'outfit_after':v['after_summary']['UpperOutfit']} for k,v in report['clips'].items()},'quality_pass':False}))
