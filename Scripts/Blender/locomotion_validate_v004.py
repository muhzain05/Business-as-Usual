"""Whole-cycle motion/deformation checks, including between-frame sampling."""
import bpy,json,math,hashlib
from pathlib import Path
from mathutils.bvhtree import BVHTree
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v004'
scene=bpy.data.scenes['P2_v004_Locomotion'];base=bpy.data.scenes['P2_v004_Baseline'];rig=next(o for o in scene.objects if o.type=='ARMATURE');br=next(o for o in base.objects if o.type=='ARMATURE');bpy.context.window.scene=scene
def digest(o):
    d={'verts':[list(v.co) for v in o.data.vertices],'polys':[[list(p.vertices),p.material_index,p.use_smooth] for p in o.data.polygons],'uv':[[list(v.uv) for v in l.data] for l in o.data.uv_layers],'normals':[list(n.vector) for n in o.data.corner_normals],'weights':[[[o.vertex_groups[g.group].name,g.weight] for g in v.groups] for v in o.data.vertices],'materials':[m.name for m in o.data.materials],'keys':[[k.name,[list(p.co) for p in k.data]] for k in o.data.shape_keys.key_blocks] if o.data.shape_keys else []}
    return hashlib.sha256(json.dumps(d,sort_keys=True).encode()).hexdigest()
protected={}
for o in scene.objects:
    if o.type=='MESH':
        role=o.name.removeprefix('Sunny_v004_');old=next(x for x in base.objects if x.name=='V004_Baseline_'+role);assert digest(o)==digest(old),role;protected[role]=digest(o)
assert len(rig.data.bones)==53
for b in rig.data.bones:
    old=br.data.bones[b.name];assert b.matrix_local==old.matrix_local;assert (b.parent.name if b.parent else None)==(old.parent.name if old.parent else None)
def span(a):return {'min':min(a),'max':max(a),'range':max(a)-min(a)}
report={'geometry_weights_uv_material_shape_keys_unchanged':True,'protected_mesh_digests':protected,'bones':53,'rest_hierarchy_unchanged':True,'working_height':1.2,'clips':{}}
names=['Shorts','LowerLeg_L','LowerLeg_R','Shoe_L','Shoe_R'];objects={n:bpy.data.objects['Sunny_v004_'+n] for n in names}
for clip,N in [('Walk',32),('Run',24)]:
    rig.animation_data.action=bpy.data.actions[scene['bau_'+clip+'_action']];rig.animation_data.action_slot=rig.animation_data.action.slots[0];rig.data.pose_position='POSE';rows=[];start=None;end=None
    for i in range(N*4+1):
        f=1+i*.25;scene.frame_set(int(f),subframe=f-int(f));bpy.context.view_layer.update();dg=bpy.context.evaluated_depsgraph_get();metrics={};trees={};coords={}
        for name,obj in objects.items():
            flags=[(m,m.show_viewport) for m in obj.modifiers if m.type!='ARMATURE']
            for m,state in flags:m.show_viewport=False
            bpy.context.view_layer.update();ev=obj.evaluated_get(dg);mesh=ev.to_mesh();mesh.calc_loop_triangles();ps=[ev.matrix_world@v.co for v in mesh.vertices];ts=[tuple(t.vertices) for t in mesh.loop_triangles];rs=[]
            for e in obj.data.edges:
                a,b=e.vertices;l=(obj.data.vertices[a].co-obj.data.vertices[b].co).length
                if l>1e-7:rs.append((ps[a]-ps[b]).length/l)
            rs.sort();tree=BVHTree.FromPolygons(ps,ts,all_triangles=True,epsilon=1e-7);trees[name]=tree;coords[name]=ps
            metrics[name]={'p99':rs[round((len(rs)-1)*.99)],'max_ratio':max(rs),'min_ratio':min(rs),'min_z':min(p.z for p in ps),'zero_faces':sum(p.area<1e-10 for p in mesh.polygons),'self_crossings':sum(a<b and not set(ts[a]).intersection(ts[b]) for a,b in tree.overlap(tree))}
            ev.to_mesh_clear()
            for m,state in flags:m.show_viewport=state
        bones={}
        for side,tag in [('Left','L'),('Right','R')]:
            a,b,c=[rig.pose.bones['mixamorig:'+side+n] for n in ['UpLeg','Leg','Foot']];v=a.tail-a.head;w=b.tail-b.head;arm=rig.pose.bones['mixamorig:'+side+'Arm'];av=arm.tail-arm.head
            bones[side]={'ankle':list(c.head),'thigh_pitch':math.degrees(math.atan2(-v.y,-v.z)),'knee':math.degrees(v.angle(w)),'foot_delta':math.degrees(c.matrix.to_quaternion().rotation_difference(c.bone.matrix_local.to_quaternion()).angle),'arm_pitch':math.degrees(math.atan2(-av.y,-av.z))}
        crossings={a+'__'+b:len(trees[a].overlap(trees[b])) for a,b in [('Shorts','LowerLeg_L'),('Shorts','LowerLeg_R'),('Shoe_L','Shoe_R'),('LowerLeg_L','LowerLeg_R')]}
        rows.append({'frame':f,'metrics':metrics,'legs':bones,'hips':list(rig.pose.bones['mixamorig:Hips'].head),'crossings':crossings})
        points=[p for n in names for p in coords[n]]
        if i==0:start=points
        if i==N*4:end=points
    seam=max((a-b).length for a,b in zip(start,end));assert seam<1e-5,seam
    summaries={side:{key:span([r['legs'][side][key] for r in rows]) for key in ['thigh_pitch','knee','foot_delta','arm_pitch']} for side in ['Left','Right']}
    summaries['hips_z']=span([r['hips'][2] for r in rows]);summaries['airborne_samples']=[r['frame'] for r in rows[:-1] if all(r['metrics']['Shoe_'+tag]['min_z']>.003 for tag in ['L','R'])]
    summary={n:{k:max(r['metrics'][n][k] for r in rows) for k in ['p99','max_ratio','self_crossings','zero_faces']} for n in names}
    report['clips'][clip]={'cycle_frames':N,'samples':rows,'motion':summaries,'deformation':summary,'seam_vertex_error':seam,'shoe_pair_intersection_frames':[r['frame'] for r in rows if r['crossings']['Shoe_L__Shoe_R']],'shorts_crossing_integer_frames':[r['frame'] for r in rows if r['frame'].is_integer() and (r['crossings']['Shorts__LowerLeg_L'] or r['crossings']['Shorts__LowerLeg_R'])],'worst_frame':max(rows,key=lambda r:max(r['metrics'][n]['max_ratio'] for n in ['Shorts','LowerLeg_L','LowerLeg_R']))['frame'],'floor_min':min(r['metrics'][n]['min_z'] for r in rows for n in ['Shoe_L','Shoe_R'])}
rig.animation_data.action=None
from mathutils import Matrix
for b in rig.pose.bones:b.matrix_basis=Matrix.Identity(4)
rig.data.pose_position='REST';scene.frame_set(1)
(OUT/'locomotion-validation.json').write_text(json.dumps(report,indent=2))
print(json.dumps({c:{k:v[k] for k in ['motion','deformation','seam_vertex_error','shoe_pair_intersection_frames','shorts_crossing_integer_frames','worst_frame','floor_min']} for c,v in report['clips'].items()}))
