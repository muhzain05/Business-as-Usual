"""Targeted lower-body retopology and leg-only motion retarget. Immutable v002 input."""
import bpy,bmesh,json,math,hashlib,re
from pathlib import Path
from mathutils import Vector,Matrix,Quaternion
from mathutils.bvhtree import BVHTree
from collections import defaultdict
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v003';OUT.mkdir(parents=True,exist_ok=True)
PROJECT=ROOT/'Assets/Characters/Sunny/Working/Sunny_LegRepair_v003.blend'
assert not PROJECT.exists() and 'P2_v003_LegRepair' not in bpy.data.scenes
INPUT=ROOT/'Assets/Characters/Sunny/Working/Sunny_Phase2_Repair_v002.blend'
assert hashlib.sha256(INPUT.read_bytes()).hexdigest()=='d00d1f0dc2da21469b11262277dfd1857438d7cc1676bdb113a7bf51249b4926'
for label in ['Walk','Run']:
    name='P2_v002_'+label+'_Review'
    if name not in bpy.data.scenes:
        with bpy.data.libraries.load(str(INPUT),link=False) as (src,dst):dst.scenes=[name]
with bpy.data.libraries.load(str(INPUT),link=False) as (src,dst):dst.scenes=['P2_v002_Repair']
before=dst.scenes[0];before.name='P2_v003_Before'
def role(o):return re.sub(r'\.\d+$','',o.name).split('Sunny_v002_')[-1]
bmapping={role(o):o for o in before.objects}
for name,obj in bmapping.items():obj.name='Before_v003_'+name
after=bpy.data.scenes.new('P2_v003_LegRepair');after.world=before.world;after.render.engine=before.render.engine;after.render.fps=24
copies={}
for old in before.objects:
    obj=old.copy();obj.data=old.data.copy();obj.name='Sunny_v003_'+role(old) if 'Sunny_v002_' in old.name else 'Sunny_v003_'+old.name.split('Before_v003_')[-1]
    after.collection.objects.link(obj);copies[old]=obj
for old,obj in copies.items():
    obj.parent=copies.get(old.parent)
    for mod in obj.modifiers:
        if mod.type=='ARMATURE':mod.object=copies.get(mod.object,mod.object)
objects={name:copies[obj] for name,obj in bmapping.items()};rig=objects['ReferenceRig'];rig.data.pose_position='REST'
bpy.context.window.scene=after
upper=[n for n in objects if not any(n.startswith(x) for x in ['LowerLeg','Shoe','Shorts','Body_Pelvis','Body_Thigh','Body_ConcealedFoot']) and n!='ReferenceRig']
def mesh_digest(o):
    d={'v':[list(v.co) for v in o.data.vertices],'p':[list(p.vertices) for p in o.data.polygons],
       'uv':[[list(p.uv) for p in layer.data] for layer in o.data.uv_layers],
       'w':[[[o.vertex_groups[g.group].name,g.weight] for g in v.groups] for v in o.data.vertices],
       'materials':[m.name for m in o.data.materials]}
    return hashlib.sha256(json.dumps(d,sort_keys=True).encode()).hexdigest()
report={'version':'v003','upper_body_digests':{n:mesh_digest(bmapping[n]) for n in upper},'changes':[], 'animation':{}}
def smooth(t):t=max(0,min(1,t));return t*t*(3-2*t)
def set_weights(obj,fn):
    obj.vertex_groups.clear()
    for v in obj.data.vertices:
        for n,w in fn(v.co).items():
            if w>1e-6:
                group=obj.vertex_groups.get('mixamorig:'+n) or obj.vertex_groups.new(name='mixamorig:'+n);group.add([v.index],w,'REPLACE')
def leg_weights(p,side):
    # Consistent horizontal bands, avoiding transverse weight changes around fat short joints.
    knee=smooth((.306-p.z)/.062);ankle=smooth((.174-p.z)/.047)
    return {side+'UpLeg':1-knee,side+'Leg':knee}
def mat_skin():
    # Flat skin palette avoids baked garment colours leaking onto the new calf cage.
    m=bpy.data.materials.new('Sunny_v003_ConcealedLegSkin');m.use_nodes=True
    n=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');n.inputs['Base Color'].default_value=(.87,.62,.43,1);n.inputs['Roughness'].default_value=.8
    m.diffuse_color=(.87,.62,.43,1);return m
skin=mat_skin()
if 'P2_v001_Modules_Rest' not in bpy.data.scenes:
    with bpy.data.libraries.load(str(ROOT/'Assets/Characters/Sunny/Working/Sunny_Phase2_Extraction_v001.blend'),link=False) as (src,dst):dst.scenes=['P2_v001_Modules_Rest']
for side,tag in [('Left','L'),('Right','R')]:
    old=objects['LowerLeg_'+tag];original=next(o for o in bpy.data.scenes['P2_v001_Modules_Rest'].objects if o.name.endswith('LowerLeg_'+tag))
    src=original.data;src.calc_loop_triangles();triangles=list(src.loop_triangles)
    tree=BVHTree.FromPolygons([v.co for v in src.vertices],[list(t.vertices) for t in triangles],all_triangles=True)
    hip=rig.data.bones['mixamorig:'+side+'UpLeg'].head_local;knee=rig.data.bones['mixamorig:'+side+'Leg'].head_local;ankle=rig.data.bones['mixamorig:'+side+'Foot'].head_local
    verts=[];faces=[];N=24
    zs=[.112,.126,.140,.154,.174,.197,.222,.243,.255,.269,.282,.295,.307,.325,.349,.377,.404,.416]
    for z in zs:
        if z>knee.z:c=knee.lerp(hip,(z-knee.z)/(hip.z-knee.z))
        else:c=ankle.lerp(knee,(z-ankle.z)/(knee.z-ankle.z))
        # Match original visible calf centre; covered thigh aligns to the bones.
        if z<.285:c.x+=(.003 if side=='Left' else -.003);c.y+=.009
        rx=.049 if z<.285 else .050+min(1,(z-.285)/.10)*.008
        ry=.061 if z<.285 else .061
        for i in range(N):
            t=2*math.pi*i/N;verts.append((c.x+rx*math.cos(t),c.y+ry*math.sin(t),z))
    for k in range(len(zs)-1):
        for i in range(N):j=(i+1)%N;faces.append((k*N+i,k*N+j,(k+1)*N+j,(k+1)*N+i))
    faces.extend([tuple(reversed(range(N))),tuple((len(zs)-1)*N+i for i in range(N))])
    mesh=bpy.data.meshes.new('Sunny_v003_Leg_'+tag+'_QuadCage');mesh.from_pydata(verts,[],faces);mesh.update()
    for m in old.data.materials:mesh.materials.append(m)
    mesh.materials.append(skin);layer=mesh.uv_layers.new(name='UVMap')
    # Closest-triangle barycentric UV transfer on the visible calf; no texture generation.
    for poly in mesh.polygons:
        poly.use_smooth=True
        if poly.center.z>.282:poly.material_index=len(mesh.materials)-1
        for loopid in poly.loop_indices:
            p=mesh.vertices[mesh.loops[loopid].vertex_index].co;closest,normal,index,distance=tree.find_nearest(p)
            tri=triangles[index];a,b,c=[src.vertices[v].co for v in tri.vertices]
            v0=b-a;v1=c-a;v2=closest-a;den=v0.dot(v0)*v1.dot(v1)-v0.dot(v1)**2
            y=(v1.dot(v1)*v2.dot(v0)-v0.dot(v1)*v2.dot(v1))/den if abs(den)>1e-12 else 0
            z=(v0.dot(v0)*v2.dot(v1)-v0.dot(v1)*v2.dot(v0))/den if abs(den)>1e-12 else 0
            uv=[src.uv_layers.active.data[i].uv for i in tri.loops];layer.data[loopid].uv=uv[0]*(1-y-z)+uv[1]*y+uv[2]*z
    old.data=mesh
    for poly in mesh.polygons:poly.material_index=len(mesh.materials)-1
    for mod in list(old.modifiers):
        if mod.type!='ARMATURE':old.modifiers.remove(mod)
    set_weights(old,lambda p,s=side:leg_weights(p,s))
    next(m for m in old.modifiers if m.type=='ARMATURE').use_deform_preserve_volume=True
    report['changes'].append('Local thigh/knee/shin/ankle quad cage '+tag+': 24 vertices per ring, 18 rings. Transferred UVs retained; flat skin palette used because source UV reprojection leaks baked cuff colours. Appearance is approximate.')
    # Keep oversized shoes rigid; no geometry/material edits.
    set_weights(objects['Shoe_'+tag],lambda p,s=side:{s+'Foot':1})
    foot=objects['Body_ConcealedFoot_'+tag];set_weights(foot,lambda p,s=side:{s+'Foot':1})
shorts=objects['Shorts'];mesh=shorts.data;bm=bmesh.new();bm.from_mesh(mesh)
bmesh.ops.bisect_plane(bm,geom=list(bm.verts)+list(bm.edges)+list(bm.faces),dist=1e-6,plane_co=(0,0,.278),plane_no=(0,0,1),clear_inner=True,clear_outer=False)
adj=defaultdict(list)
for e in bm.edges:
    if e.is_boundary:
        a,b=e.verts;adj[a].append(b);adj[b].append(a)
unseen=set(adj);loops=[]
while unseen:
    first=next(iter(unseen));stack=[first];group=set()
    while stack:
        v=stack.pop()
        if v in group:continue
        group.add(v);stack.extend(adj[v])
    unseen-=group
    if sum(v.co.z for v in group)/len(group)>.35:continue
    assert all(len(adj[v])==2 for v in group),'Non-simple cuff boundary'
    loop=[first];prev=None;current=first
    while True:
        nxt=next(v for v in adj[current] if v!=prev)
        if nxt==first:break
        loop.append(nxt);prev,current=current,nxt
    loops.append(loop)
assert len(loops)==2
for loop in loops:
    center=sum((v.co for v in loop),Vector())/len(loop);last=loop
    for z,scale in [(.266,1.005),(.253,1.008),(.249,1.006)]:
        ring=[bm.verts.new((center.x+(v.co.x-center.x)*scale,center.y+(v.co.y-center.y)*scale,z)) for v in loop]
        for i in range(len(last)):
            k=(i+1)%len(last);f=bm.faces.new((last[i],last[k],ring[k],ring[i]));f.smooth=True
        last=ring
bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
# Transfer appearance onto the locally remodeled cuffs from the v002 source.
src=bmapping['Shorts'].data;src.calc_loop_triangles();tris=list(src.loop_triangles)
tree=BVHTree.FromPolygons([v.co for v in src.vertices],[list(t.vertices) for t in tris],all_triangles=True)
for poly in mesh.polygons:
    if poly.center.z>.279:continue
    for loopid in poly.loop_indices:
        p=mesh.vertices[mesh.loops[loopid].vertex_index].co;point,n,index,d=tree.find_nearest(p);tri=tris[index]
        a,b,c=[src.vertices[i].co for i in tri.vertices];v0=b-a;v1=c-a;v2=point-a;den=v0.dot(v0)*v1.dot(v1)-v0.dot(v1)**2
        u=(v1.dot(v1)*v2.dot(v0)-v0.dot(v1)*v2.dot(v1))/den if abs(den)>1e-12 else 0
        v=(v0.dot(v0)*v2.dot(v1)-v0.dot(v1)*v2.dot(v0))/den if abs(den)>1e-12 else 0
        q=[src.uv_layers.active.data[i].uv for i in tri.loops];mesh.uv_layers.active.data[loopid].uv=q[0]*(1-u-v)+q[1]*u+q[2]*v
def shorts_weights(p):
    side='Left' if p.x>0 else 'Right'
    pelvis=smooth((p.z-.30)/.088);pelvis=max(pelvis,(1-smooth(abs(p.x)/.070))*smooth((p.z-.275)/.035))
    return {'Hips':pelvis,side+'UpLeg':1-pelvis}
set_weights(shorts,shorts_weights)
cuffmat=bpy.data.materials.new('Sunny_v003_ShortsCuff');cuffmat.use_nodes=True
node=next(n for n in cuffmat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');node.inputs['Base Color'].default_value=(.019,.03,.043,1);node.inputs['Roughness'].default_value=.8;cuffmat.diffuse_color=(.019,.03,.043,1)
mesh.materials.append(cuffmat)
for poly in mesh.polygons:
    if poly.center.z<.279:poly.material_index=len(mesh.materials)-1

for m in list(shorts.modifiers):
    if m.type=='MASK':shorts.modifiers.remove(m)
    if m.type=='SOLIDIFY':m.use_even_offset=False;m.thickness_clamp=.5
report['changes'].append('Shorts lower 0.029-unit cuff band remodeled; two ordered open cuffs, original upper/crotch surfaces and material retained. Cuffs follow their own thigh; pelvis dominates the waist/crotch. Deep knee motion is reduced rather than pulling the shorts with the shins.')

def curves(action):return [c for l in action.layers for strip in l.strips if hasattr(strip,'channelbags') for bag in strip.channelbags for c in bag.fcurves]
lower_names={b.name for b in rig.data.bones if any(x in b.name for x in ['UpLeg','Leg','Foot','Toe'])}
def channelhash(action,exclude_lower=True):
    data=[]
    for c in curves(action):
        if exclude_lower and any('"'+n+'"' in c.data_path for n in lower_names):continue
        data.append([c.data_path,c.array_index,[[list(k.co),list(k.handle_left),list(k.handle_right),k.interpolation] for k in c.keyframe_points]])
    return hashlib.sha256(json.dumps(data,sort_keys=True).encode()).hexdigest()
for clip in ['Walk','Run']:
    ref=bpy.data.scenes['P2_v002_'+clip+'_Review'];rr=next(o for o in ref.objects if o.type=='ARMATURE')
    action=rr.animation_data.action.copy();action.name='Sunny_v003_'+clip+'_LegRetarget';originalhash=channelhash(rr.animation_data.action)
    for l in action.layers:
        for strip in l.strips:
            if hasattr(strip,'channelbags'):
                for bag in strip.channelbags:
                    for c in list(bag.fcurves):
                        if any('"'+n+'"' in c.data_path for n in lower_names) and ('rotation_' in c.data_path or c.data_path.endswith('scale')):bag.fcurves.remove(c)
    rig.animation_data.action=action;rig.animation_data.action_slot=action.slots[0];rig.data.pose_position='POSE'
    lo,hi=map(int,action.frame_range);log=[]
    for f in range(lo,hi+1):
        bpy.context.window.scene=ref;ref.frame_set(f);bpy.context.view_layer.update()
        sample={side:{'hip':rr.pose.bones['mixamorig:'+side+'UpLeg'].head.copy(),'ankle':rr.pose.bones['mixamorig:'+side+'Foot'].head.copy(),
                      'bend':(rr.pose.bones['mixamorig:'+side+'UpLeg'].tail-rr.pose.bones['mixamorig:'+side+'UpLeg'].head).angle(rr.pose.bones['mixamorig:'+side+'Leg'].tail-rr.pose.bones['mixamorig:'+side+'Leg'].head)} for side in ['Left','Right']}
        bpy.context.window.scene=after;after.frame_set(f);bpy.context.view_layer.update();row={'frame':f}
        for side,tag in [('Left','L'),('Right','R')]:
            upperbone=rig.pose.bones['mixamorig:'+side+'UpLeg'];shin=rig.pose.bones['mixamorig:'+side+'Leg'];foot=rig.pose.bones['mixamorig:'+side+'Foot']
            hip=upperbone.head.copy();a=upperbone.bone.length;b=shin.bone.length
            restankle=foot.bone.head_local
            forward=max(-.075,min(.075,(sample[side]['ankle'].y-sample[side]['hip'].y)*.32))
            lift=(.027 if clip=='Run' else .018)*smooth((sample[side]['ankle'].z-restankle.z)/(.15 if clip=='Run' else .06))
            goal=Vector((hip.x+(.060 if side=='Left' else -.060),hip.y+forward,restankle.z+lift))
            direction=goal-hip;distance=direction.length;minimum=math.sqrt(a*a+b*b+2*a*b*math.cos(math.radians(65)))
            if distance<minimum:
                # Preserve the requested sole clearance; increase fore/aft reach instead of folding the knee.
                extra=math.sqrt(max(0,minimum**2-direction.x**2-direction.z**2));goal.y=hip.y+(extra if forward>=0 else -extra)
                direction=goal-hip;distance=direction.length
            distance=min(a+b-.0005,distance);direction.normalize();goal=hip+direction*distance
            along=(a*a-b*b+distance*distance)/(2*distance);height=math.sqrt(max(0,a*a-along*along))
            pole=Vector((0,-1,0));pole=(pole-direction*pole.dot(direction)).normalized();kneepos=hip+direction*along+pole*height
            for bone,head,tail in [(upperbone,hip,kneepos),(shin,kneepos,goal)]:
                rest=bone.bone.matrix_local.to_quaternion();rotation=(bone.bone.tail_local-bone.bone.head_local).rotation_difference(tail-head)@rest
                bone.matrix=Matrix.LocRotScale(head,rotation,Vector((1,1,1)));bpy.context.view_layer.update()
                bone.rotation_mode='QUATERNION';bone.keyframe_insert('rotation_quaternion',frame=f,group=bone.name)
            foot.matrix=Matrix.LocRotScale(goal,foot.bone.matrix_local.to_quaternion(),Vector((1,1,1)));bpy.context.view_layer.update()
            foot.rotation_mode='QUATERNION';foot.keyframe_insert('rotation_quaternion',frame=f,group=foot.name)
            for bone in rig.pose.bones:
                if 'Toe' in bone.name:bone.rotation_mode='QUATERNION';bone.rotation_quaternion=Quaternion();bone.keyframe_insert('rotation_quaternion',frame=f,group=bone.name)
            row[side]={'ankle_target':list(goal),'knee_target':list(kneepos),'bend_degrees':math.degrees(math.acos(max(-1,min(1,(distance*distance-a*a-b*b)/(2*a*b))))),'sole_clearance_target':goal.z-restankle.z}
        log.append(row)
    # Prevent quaternion sign flips or Bezier overshoot between baked integer samples.
    for bone in lower_names:
        cs=[next((c for c in curves(action) if c.data_path=='pose.bones["'+bone+'"].rotation_quaternion' and c.array_index==i),None) for i in range(4)]
        if any(c is None for c in cs):continue
        previous=None
        for idx in range(len(cs[0].keyframe_points)):
            q=Quaternion([c.keyframe_points[idx].co.y for c in cs])
            if previous is not None and q.dot(previous)<0:
                q.negate()
                for i,c in enumerate(cs):c.keyframe_points[idx].co.y=q[i]
            for c in cs:c.keyframe_points[idx].interpolation='LINEAR'
            previous=q
        for c in cs:c.update()
    assert channelhash(action)==originalhash,'Upper-body animation channel changed'
    report['animation'][clip]={'action':action.name,'frames':log,'upper_channel_hash':originalhash,'upper_channels_unchanged':True}
    after['bau_'+clip+'_action']=action.name
for name in upper:assert mesh_digest(objects[name])==report['upper_body_digests'][name],name
report['upper_body_unchanged']=True
for o in after.objects:
    o.hide_set(o.type=='ARMATURE' or o.get('bau_nonproduction',False))
rig.data.pose_position='REST';after.frame_set(1);bpy.context.window.scene=after
(OUT/'leg-repair-build.json').write_text(json.dumps(report,indent=2))
print(json.dumps({'scene':after.name,'upper_body_unchanged':True,'changes':report['changes'],'clips':{k:max(x[side]['bend_degrees'] for x in v['frames'] for side in ['Left','Right']) for k,v in report['animation'].items()}}))
