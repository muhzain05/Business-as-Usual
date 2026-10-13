"""Fixed-camera, matching-light before/after renders and every walk/run frame.
Run once to prepare presentation scenes, then pass BAU_RENDER_JOBS in subsequent calls.
"""
import bpy,json,math
from pathlib import Path
from mathutils import Vector
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase2/v002/Images';OUT.mkdir(parents=True,exist_ok=True)
def setup(label,source):
    name='P2_v002_Render_'+label
    if name in bpy.data.scenes:return bpy.data.scenes[name]
    s=bpy.data.scenes.new(name);s.render.engine=source.render.engine
    s.render.resolution_x=500;s.render.resolution_y=650;s.render.resolution_percentage=100
    s.render.image_settings.file_format='PNG';s.render.fps=24
    s.world=bpy.data.worlds.new(name+'_World');s.world.use_nodes=True
    bg=next(n for n in s.world.node_tree.nodes if n.type=='BACKGROUND');bg.inputs[0].default_value=(.055,.055,.055,1);bg.inputs[1].default_value=.6
    objects={}
    for o in source.objects:
        if o.get("bau_nonproduction"):continue
        p=o.copy();p.name=name+'_'+o.name
        if p.type=='ARMATURE':p.data=o.data.copy()
        s.collection.objects.link(p);objects[o]=p;p.hide_render=False
    for o,p in objects.items():
        p.parent=objects.get(o.parent)
        for m in p.modifiers:
            if m.type=='ARMATURE':m.object=objects.get(m.object,m.object)
    cam=bpy.data.cameras.new(name+'_Camera');camera=bpy.data.objects.new(cam.name,cam);s.collection.objects.link(camera);s.camera=camera
    cam.type='ORTHO';cam.ortho_scale=1.40;cam.lens=50
    for index,(position,power,size) in enumerate([((-2,-3,3),140,3),((2,-1,2),90,3),((0,3,2),140,2)]):
        data=bpy.data.lights.new(name+'_Light'+str(index),'AREA');data.energy=power;data.shape='DISK';data.size=size
        light=bpy.data.objects.new(data.name,data);s.collection.objects.link(light);light.location=position;light.rotation_euler=(Vector((0,0,.6))-light.location).to_track_quat('-Z','Y').to_euler()
    if hasattr(s,'eevee') and hasattr(s.eevee,'taa_render_samples'):s.eevee.taa_render_samples=16
    return s
scenes={label:setup(label,bpy.data.scenes[source]) for label,source in [('Before','P2_v001_Modules_Rest'),('After','P2_v002_Repair')]}
jobs=globals().get('BAU_RENDER_JOBS',[])
prior=bpy.context.window.scene
for job in jobs:
    label,mode,view,frame=job;s=scenes[label];bpy.context.window.scene=s
    rig=next(o for o in s.objects if o.type=='ARMATURE');rig.hide_set(False)
    for b in rig.pose.bones:b.rotation_mode='QUATERNION'
    target=Vector((0,.0,.6));camera=s.camera
    camera.location={'front':(0,-3,.6),'side':(3,0,.6),'back':(0,3,.6)}[view]
    camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.ortho_scale=1.4
    for o in s.objects:
        if o.type=='MESH':o.hide_render=False
    if mode=='jacket':
        for o in s.objects:
            if o.type=='MESH':o.hide_render=not any(n in o.name for n in ['UpperOutfit','Button_'])
        target=Vector((0,.02,.595));camera.location=target+{'front':Vector((0,-3,0)),'side':Vector((3,0,0)),'back':Vector((0,3,0))}[view]
        camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.ortho_scale=.86
    if mode in ['walk','run']:
        ref=next(o for o in bpy.data.scenes['P2_v001_Reference_'+mode.title()].objects if o.type=='ARMATURE')
        rig.animation_data.action=ref.animation_data.action;rig.animation_data.action_slot=ref.animation_data.action_slot;rig.data.pose_position='POSE'
    else:rig.data.pose_position='REST'
    s.frame_set(frame);s.render.filepath=str(OUT/f'{label.lower()}-{mode}-{view}-{frame:03}.png')
    bpy.ops.render.render(write_still=True)
bpy.context.window.scene=prior
print(json.dumps({'rendered':len(jobs),'output':str(OUT),'presentation_scenes':list(scenes)}))
