"""Controlled joint/face and bare-body renders on presentation copies only."""
import bpy,math,json
from pathlib import Path
from mathutils import Matrix,Vector
OUT=Path('D:/Business as Usual/Docs/Diagnostics/Phase2/v002/Images')
poses={'arm_raise':{'LeftArm':(0,0,-70),'RightArm':(0,0,70)},
       'crouch':{'LeftUpLeg':(-45,0,0),'RightUpLeg':(-45,0,0),'LeftLeg':(75,0,0),'RightLeg':(75,0,0),'Spine':(15,0,0)},
       'leg_bend':{'LeftLeg':(90,0,0),'RightLeg':(90,0,0)},
       'ankle_flex':{'LeftFoot':(25,0,0),'RightFoot':(-25,0,0)}}
prior=bpy.context.window.scene
for label in ['Before','After']:
    s=bpy.data.scenes['P2_v002_Render_'+label];bpy.context.window.scene=s;r=next(o for o in s.objects if o.type=='ARMATURE')
    r.animation_data.action=None;r.data.pose_position='POSE'
    s.camera.location=(0,-3,.6);s.camera.rotation_euler=(math.pi/2,0,0);s.camera.data.ortho_scale=1.4
    for o in s.objects:
        if o.type=='MESH':o.hide_render=False
    for name,rotations in poses.items():
        for b in r.pose.bones:b.rotation_mode='XYZ';b.matrix_basis=Matrix.Identity(4)
        for namebone,angles in rotations.items():r.pose.bones['mixamorig:'+namebone].rotation_euler=[math.radians(a) for a in angles]
        if name=='arm_raise':
            for side,desired in [('Left',Vector((.7,0,.7))),('Right',Vector((-.7,0,.7)))]:
                bone=r.pose.bones['mixamorig:'+side+'Arm']
                rest=bone.bone.matrix_local.to_quaternion()
                world=(bone.bone.tail_local-bone.bone.head_local).rotation_difference(desired)
                bone.rotation_mode='QUATERNION';bone.rotation_quaternion=rest.inverted()@world@rest
        s.frame_set(1);s.render.filepath=str(OUT/f'{label.lower()}-controlled-{name}.png');bpy.ops.render.render(write_still=True)
    for b in r.pose.bones:b.rotation_mode='QUATERNION';b.matrix_basis=Matrix.Identity(4)
    r.data.pose_position='REST'
s=bpy.data.scenes['P2_v002_Render_After'];bpy.context.window.scene=s
keys=[(key,key.value) for o in s.objects if o.type=='MESH' and o.data.shape_keys for key in o.data.shape_keys.key_blocks if key.name!='Basis']
for o in s.objects:
    if o.type=='MESH' and o.data.shape_keys:
        for k in o.data.shape_keys.key_blocks:
            if k.name=='Blink':k.value=1 if 'Eye_L' in o.name else 0
            if k.name=='SmileWidth':k.value=1
s.render.filepath=str(OUT/'after-face-prototype.png');bpy.ops.render.render(write_still=True)
for k,v in keys:k.value=v
masked=[]
for o in s.objects:
    if o.type!='MESH':continue
    o.hide_render=any(n in o.name for n in ['UpperOutfit','Shorts','Shoe_','Button_','Body_UpperArm','Body_Thigh'])
    for m in o.modifiers:
        if m.type=='MASK':masked.append((m,m.show_render));m.show_render=False
s.render.filepath=str(OUT/'after-body-unclothed-review.png');bpy.ops.render.render(write_still=True)
for m,value in masked:m.show_render=value
for o in s.objects:
    if o.type=='MESH':o.hide_render=False
bpy.context.window.scene=prior
print(json.dumps({'controlled_pairs':4,'face_prototype':True,'bare_body_review':True}))
