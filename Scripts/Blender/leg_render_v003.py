"""Fixed-camera lower-body before/after renders, complete clips, controlled poses."""
import bpy,math,json
from pathlib import Path
from mathutils import Vector,Matrix,Quaternion
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v003/Images';OUT.mkdir(parents=True,exist_ok=True)
def setup(label,source):
    name='P2_v003_Render_'+label
    if name in bpy.data.scenes:return bpy.data.scenes[name]
    s=bpy.data.scenes.new(name);s.render.engine=source.render.engine;s.render.fps=24;s.render.resolution_x=460;s.render.resolution_y=560;s.render.resolution_percentage=100
    s.render.image_settings.file_format='PNG';s.world=bpy.data.worlds.new(name+'_World');s.world.use_nodes=True
    bg=next(n for n in s.world.node_tree.nodes if n.type=='BACKGROUND');bg.inputs[0].default_value=(.05,.05,.05,1);bg.inputs[1].default_value=.6
    copies={}
    for old in source.objects:
        if old.get('bau_nonproduction'):continue
        obj=old.copy();obj.name=name+'_'+old.name
        if old.type=='ARMATURE' or old.name.endswith('Shorts'):obj.data=old.data.copy()
        s.collection.objects.link(obj);copies[old]=obj;obj.hide_render=False
    for old,obj in copies.items():
        obj.parent=copies.get(old.parent)
        for m in obj.modifiers:
            if m.type=='ARMATURE':m.object=copies.get(m.object,m.object)
        if obj.type=='MESH' and obj.data.shape_keys and obj.data.shape_keys.animation_data:
            for fc in obj.data.shape_keys.animation_data.drivers:
                for var in fc.driver.variables:
                    for target in var.targets:
                        if target.id in copies:target.id=copies[target.id]
    cam=bpy.data.cameras.new(name+'_Camera');camera=bpy.data.objects.new(cam.name,cam);s.collection.objects.link(camera);s.camera=camera;cam.type='ORTHO'
    for i,(pos,power,size) in enumerate([((-2,-3,3),140,3),((2,-1,2),90,3),((0,3,2),140,2)]):
        data=bpy.data.lights.new(name+'_Light'+str(i),'AREA');data.energy=power;data.shape='DISK';data.size=size
        light=bpy.data.objects.new(data.name,data);s.collection.objects.link(light);light.location=pos;light.rotation_euler=(Vector((0,0,.4))-light.location).to_track_quat('-Z','Y').to_euler()
    return s
scenes={label:setup(label,bpy.data.scenes[name]) for label,name in [('Before','P2_v003_Before'),('After','P2_v003_LegRepair')]}
def reset(rig):
    rig.animation_data.action=None;rig.data.pose_position='POSE'
    for b in rig.pose.bones:b.rotation_mode='QUATERNION';b.matrix_basis=Matrix.Identity(4)
def point_bone(bone,head,tail):
    q=(bone.bone.tail_local-bone.bone.head_local).rotation_difference(tail-head)@bone.bone.matrix_local.to_quaternion()
    bone.matrix=Matrix.LocRotScale(head,q,Vector((1,1,1)));bpy.context.view_layer.update()
def ik(rig,side,goal):
    a=rig.pose.bones['mixamorig:'+side+'UpLeg'];b=rig.pose.bones['mixamorig:'+side+'Leg'];c=rig.pose.bones['mixamorig:'+side+'Foot']
    hip=a.head.copy();direction=goal-hip;distance=min(a.bone.length+b.bone.length-.0005,direction.length);direction.normalize();goal=hip+direction*distance
    L=a.bone.length;M=b.bone.length;along=(L*L-M*M+distance*distance)/(2*distance);height=math.sqrt(max(0,L*L-along*along))
    pole=Vector((0,-1,0));pole=(pole-direction*pole.dot(direction)).normalized();knee=hip+direction*along+pole*height
    point_bone(a,hip,knee);point_bone(b,knee,goal);c.matrix=Matrix.LocRotScale(goal,c.bone.matrix_local.to_quaternion(),Vector((1,1,1)));bpy.context.view_layer.update()
def control(rig,name):
    reset(rig);bpy.context.view_layer.update()
    if name=='idle':return
    if name=='crouch':
        hips=rig.pose.bones['mixamorig:Hips'];m=hips.matrix.copy();m.translation.z-=.060;hips.matrix=m;bpy.context.view_layer.update()
    for side in ['Left','Right']:
        a=rig.pose.bones['mixamorig:'+side+'UpLeg'];b=rig.pose.bones['mixamorig:'+side+'Leg'];f=rig.pose.bones['mixamorig:'+side+'Foot']
        if name=='hip_flexion' and side=='Left':
            m=a.matrix.copy();m=Matrix.LocRotScale(a.head,Quaternion(Vector((1,0,0)),math.radians(-45))@a.bone.matrix_local.to_quaternion(),Vector((1,1,1)));a.matrix=m;bpy.context.view_layer.update()
        elif name=='knee_bends':
            b.matrix=Matrix.LocRotScale(b.head,Quaternion(Vector((1,0,0)),math.radians(90))@b.bone.matrix_local.to_quaternion(),Vector((1,1,1)));bpy.context.view_layer.update()
        elif name=='foot_lift' and side=='Left':ik(rig,side,f.bone.head_local+Vector((0,-.03,.05)))
        elif name=='forward_step' and side=='Left':ik(rig,side,f.bone.head_local+Vector((0,-.075,.01)))
        elif name=='crouch':ik(rig,side,f.bone.head_local.copy())
        f.matrix=Matrix.LocRotScale(f.head,f.bone.matrix_local.to_quaternion(),Vector((1,1,1)));bpy.context.view_layer.update()
prior=bpy.context.window.scene;logs=[]
for label,mode,view,frame in globals().get('BAU_LEG_RENDER_JOBS',[]):
    s=scenes[label];bpy.context.window.scene=s;rig=next(o for o in s.objects if o.type=='ARMATURE')
    target=Vector((0,0,.24));s.camera.data.ortho_scale=.91
    if mode=='full_rest':target.z=.54;s.camera.data.ortho_scale=1.42
    direction={'front':Vector((0,-3,0)),'side':Vector((3,0,0)),'three_quarter':Vector((3,-3,.7))}[view]
    s.camera.location=target+direction;s.camera.rotation_euler=(target-s.camera.location).to_track_quat('-Z','Y').to_euler()
    if mode in ['walk','run']:
        source=bpy.data.scenes['P2_v003_LegRepair'] if label=='After' else bpy.data.scenes['P2_v002_'+mode.title()+'_Review']
        action=bpy.data.actions[source['bau_'+mode.title()+'_action']] if label=='After' else next(o for o in source.objects if o.type=='ARMATURE').animation_data.action
        rig.animation_data.action=action;rig.animation_data.action_slot=action.slots[0];rig.data.pose_position='POSE';s.frame_set(frame)
    else:
        s.frame_set(1);control(rig,'idle' if mode=='full_rest' else mode)
    if mode not in ['walk','run','full_rest']:
        logs.append({'version':label,'pose':mode,'view':view,'knees':{side:math.degrees((rig.pose.bones['mixamorig:'+side+'UpLeg'].tail-rig.pose.bones['mixamorig:'+side+'UpLeg'].head).angle(rig.pose.bones['mixamorig:'+side+'Leg'].tail-rig.pose.bones['mixamorig:'+side+'Leg'].head)) for side in ['Left','Right']}})
    s.render.filepath=str(OUT/f'{label.lower()}-{mode}-{view}-{frame:03}.png');bpy.ops.render.render(write_still=True)
if logs:(OUT.parent/'controlled-pose-definitions.json').write_text(json.dumps(logs,indent=2))
bpy.context.window.scene=prior
print(json.dumps({'rendered':len(globals().get('BAU_LEG_RENDER_JOBS',[])),'images':str(OUT)}))
