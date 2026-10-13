"""Original in-place cartoon cycles; v003 geometry and weights remain byte-equivalent."""
import bpy,json,math,hashlib,re
from pathlib import Path
from mathutils import Vector,Matrix,Quaternion
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v004';OUT.mkdir(parents=True,exist_ok=True)
INPUT=ROOT/'Assets/Characters/Sunny/Working/Sunny_LegRepair_v003.blend'
assert hashlib.sha256(INPUT.read_bytes()).hexdigest()=='866c1026badab8f902fdb1908492b56f0e3b105c8fb3e8bd5376cf80aeef682a'
assert not (ROOT/'Assets/Characters/Sunny/Working/Sunny_Locomotion_v004.blend').exists()
if 'P2_v004_Locomotion' not in bpy.data.scenes:
    with bpy.data.libraries.load(str(INPUT),link=False) as (src,dst):dst.scenes=['P2_v003_LegRepair']
    baseline=dst.scenes[0];baseline.name='P2_v004_Baseline'
    def role(o):return re.sub(r'\.\d+$','',o.name).split('Sunny_v003_')[-1]
    originals={role(o):o for o in baseline.objects}
    for name,o in originals.items():o.name='V004_Baseline_'+name
    scene=bpy.data.scenes.new('P2_v004_Locomotion');scene.world=baseline.world;scene.render.engine=baseline.render.engine;scene.render.fps=24
    copies={}
    for old in baseline.objects:
        obj=old.copy();obj.data=old.data.copy();obj.name='Sunny_v004_'+old.name.removeprefix('V004_Baseline_');scene.collection.objects.link(obj);copies[old]=obj
    for old,obj in copies.items():
        obj.parent=copies.get(old.parent)
        for m in obj.modifiers:
            if m.type=='ARMATURE':m.object=copies.get(m.object,m.object)
        if obj.type=='MESH' and obj.data.shape_keys and obj.data.shape_keys.animation_data:
            for fc in obj.data.shape_keys.animation_data.drivers:
                for var in fc.driver.variables:
                    for t in var.targets:
                        if t.id in copies:t.id=copies[t.id]
scene=bpy.data.scenes['P2_v004_Locomotion'];bpy.context.window.scene=scene
rig=next(o for o in scene.objects if o.type=='ARMATURE');rig.data.pose_position='POSE';rig.animation_data_create()
def bone(s):return rig.pose.bones['mixamorig:'+s]
def smooth(t):t=max(0,min(1,t));return t*t*(3-2*t)
def set_world(b,head,rot):b.matrix=Matrix.LocRotScale(head,rot,Vector((1,1,1)));bpy.context.view_layer.update()
def point(b,head,tail):set_world(b,head,(b.bone.tail_local-b.bone.head_local).rotation_difference(tail-head)@b.bone.matrix_local.to_quaternion())
def solve(side,goal,footq):
    a,b,c=[bone(side+s) for s in ['UpLeg','Leg','Foot']];hip=a.head.copy();direction=goal-hip;distance=direction.length;L=a.bone.length;M=b.bone.length
    assert distance<L+M+1e-5,(side,distance,L+M)
    direction.normalize();along=(L*L-M*M+distance*distance)/(2*distance);h=math.sqrt(max(0,L*L-along*along))
    pole=Vector((0,-1,0));pole=(pole-direction*pole.dot(direction)).normalized();knee=hip+direction*along+pole*h
    point(a,hip,knee);point(b,knee,goal);set_world(c,goal,footq)
def curves(a):return [c for l in a.layers for s in l.strips if hasattr(s,'channelbags') for bag in s.channelbags for c in bag.fcurves]
footcoords={}
for side,tag in [('Left','L'),('Right','R')]:
    shoe=bpy.data.objects['Sunny_v004_Shoe_'+tag];foot=bone(side+'Foot');footcoords[side]=[foot.bone.matrix_local.inverted()@v.co for v in shoe.data.vertices]
report={'type':'Original in-place stance/swing leg cycles with phase-aligned approved Mixamo upper motion','geometry_weights_materials_changed':False,'clips':{}}
for clip,N,duty,stride,lift,roll,maxbend in [('Walk',32,.62,.070,.022,6,68),('Run',24,.34,.100,.034,9,72)]:
    # Rebuilding is allowed only before the versioned output exists. Old experimental actions remain unused.
    action=bpy.data.actions.new('Sunny_v004_'+clip+'_Cartoon');rig.animation_data.action=action;rows=[]
    ref=bpy.data.scenes['P2_v002_'+clip+'_Review'];reference=next(o for o in ref.objects if o.type=='ARMATURE');rlo,rhi=reference.animation_data.action.frame_range
    upper_names=[b.name for b in rig.pose.bones if b.name!='mixamorig:Hips' and not any(n in b.name for n in ['UpLeg','Leg','Foot','Toe'])]
    phase_shift=5/25 if clip=='Walk' else 3/16
    for k in range(N*4+1):
        f=1+k/4;phase=(k/(N*4))%1
        bpy.context.window.scene=ref;rf=rlo+((phase+phase_shift)%1)*(rhi-rlo);ref.frame_set(int(rf),subframe=rf-int(rf));bpy.context.view_layer.update()
        upper_basis={n:reference.pose.bones[n].matrix_basis.copy() for n in upper_names}
        bpy.context.window.scene=scene
        scene.frame_set(int(f),subframe=f-int(f))
        for b in rig.pose.bones:b.rotation_mode='QUATERNION';b.matrix_basis=Matrix.Identity(4)
        bpy.context.view_layer.update();goals={};qs={};states={}
        for side,offset in [('Left',0),('Right',.5)]:
            p=(phase+offset)%1;stance=p<duty
            if stance:
                u=p/duty;y=-stride+2*stride*u;clear=0
                pitch=-roll*(1-smooth(u/.18))+roll*smooth((u-.80)/.20)
            else:
                u=(p-duty)/(1-duty);y=stride-2*stride*smooth(u);clear=lift*math.sin(math.pi*u)**1.3
                # Continue toe-off (+roll) through swing to heel strike (-roll), without phase jumps.
                pitch=roll*math.cos(math.pi*u)
            foot=bone(side+'Foot');q=Quaternion(Vector((1,0,0)),math.radians(pitch))@foot.bone.matrix_local.to_quaternion()
            lowest=min((q@v).z for v in footcoords[side]);rest=foot.bone.head_local
            goals[side]=Vector((rest.x,rest.y+y,-lowest+clear+.0003));qs[side]=q
            states[side]={'stance':stance,'phase':p,'clearance_target':clear,'pitch_degrees':pitch,'goal':list(goals[side])}
        hips=bone('Hips');q=Quaternion(Vector((1,0,0)),math.radians(2 if clip=='Walk' else 5))@Quaternion(Vector((0,1,0)),math.radians(1.3*math.sin(2*math.pi*phase)))@hips.bone.matrix_local.to_quaternion()
        set_world(hips,hips.bone.head_local.copy(),q)
        # Adjust common hip height to preserve authored foot paths within the existing leg reach.
        low=-1e9;high=1e9
        for side in ['Left','Right']:
            a,b=[bone(side+s) for s in ['UpLeg','Leg']];g=goals[side];xy=(g.x-a.head.x)**2+(g.y-a.head.y)**2
            minimum=math.sqrt(a.bone.length**2+b.bone.length**2+2*a.bone.length*b.bone.length*math.cos(math.radians(maxbend)))
            low=max(low,g.z+math.sqrt(max(0,minimum**2-xy))-(a.head.z-hips.head.z))
            high=min(high,g.z+math.sqrt(max(0,(a.bone.length+b.bone.length-.0008)**2-xy))-(a.head.z-hips.head.z))
        desired=hips.bone.head_local.z+(-.005 if clip=='Walk' else .002)+(.004 if clip=='Walk' else .010)*math.cos(4*math.pi*phase)
        height=max(low,min(high,desired)) if low<=high else high
        pos=hips.bone.head_local.copy();pos.z=height;set_world(hips,pos,q)
        # Preserve approved upper pose vocabulary, aligning its opposing arms to the new left contact.
        for name,basis in upper_basis.items():rig.pose.bones[name].matrix_basis=basis
        bpy.context.view_layer.update()
        for side in ['Left','Right']:solve(side,goals[side],qs[side])
        for b in rig.pose.bones:
            b.keyframe_insert('rotation_quaternion',frame=f,group=b.name)
            if b.name in upper_names:
                b.keyframe_insert('location',frame=f,group=b.name);b.keyframe_insert('scale',frame=f,group=b.name)
        hips.keyframe_insert('location',frame=f,group=hips.name)
        rows.append({'frame':f,'phase':phase,'hips_z':height,'reach_bounds':[low,high],'legs':states})
    for c in curves(action):
        for p in c.keyframe_points:p.interpolation='LINEAR'
        c.update()
    # Quaternion sign continuity, with duplicate end frame retained for seamless looping.
    for b in rig.pose.bones:
        cs=[next(c for c in curves(action) if c.data_path==b.path_from_id('rotation_quaternion') and c.array_index==i) for i in range(4)];prev=None
        for k in range(len(cs[0].keyframe_points)):
            q=Quaternion([c.keyframe_points[k].co.y for c in cs])
            if prev is not None and prev.dot(q)<0:
                q.negate()
                for i,c in enumerate(cs):c.keyframe_points[k].co.y=q[i]
            prev=q
        for c in cs:c.update()
    scene['bau_'+clip+'_action']=action.name
    report['clips'][clip]={'action':action.name,'cycle_frames':N,'fps':24,'stance_fraction':duty,'fore_aft_stride':2*stride,'swing_sole_clearance':lift,'samples':rows,'in_place':True,'upper_motion_source':reference.animation_data.action.name,'upper_motion_phase_shift':phase_shift}
rig.animation_data.action=None
for b in rig.pose.bones:b.matrix_basis=Matrix.Identity(4)
rig.data.pose_position='REST';scene.frame_set(1)
for o in scene.objects:o.hide_set(o.type=='ARMATURE' or o.get('bau_nonproduction',False))
(OUT/'animation-build.json').write_text(json.dumps(report,indent=2))
print(json.dumps({k:{n:v[n] for n in ['action','cycle_frames','stance_fraction','fore_aft_stride','swing_sole_clearance']} for k,v in report['clips'].items()}))
