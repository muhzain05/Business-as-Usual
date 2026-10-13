"""Derivative rig only. Reuse the tested stance/swing IK solver; never save v004."""
import bpy
from pathlib import Path
ROOT = Path('D:/Business as Usual')
base = (ROOT / 'Scripts/Blender/locomotion_build_v004.py').read_text()
body = base[base.index('def bone(s):'):]
body = body.replace("[('Walk',32,.62,.070,.022,6,68),('Run',24,.34,.100,.034,9,72)]", "[('Walk',20,.60,.130,.032,5,78),('Run',16,.40,.160,.052,8,84)]")
body = body.replace('Sunny_v004_', 'Sunny_Phase7B_').replace("bpy.data.objects['Sunny_Phase7B_Shoe_'+tag]", "bpy.data.objects['Sunny_v004_Shoe_'+tag]")
body = body.replace("'fps':24", "'fps':60")
header = '''import bpy,json,math,hashlib,re
from pathlib import Path
from mathutils import Vector,Matrix,Quaternion
ROOT=Path('D:/Business as Usual');OUT=ROOT/'Docs/Diagnostics/Phase7B/Animation';OUT.mkdir(parents=True,exist_ok=True)
source_scene=bpy.data.scenes['P2_v004_Locomotion']
source_rig=next(o for o in source_scene.objects if o.type=='ARMATURE')
scene=bpy.data.scenes.new('Phase7B_Locomotion');scene.world=source_scene.world;scene.render.fps=60
rig=source_rig.copy();rig.data=source_rig.data.copy();rig.name='SunnyRig';rig.animation_data_clear();scene.collection.objects.link(rig)
bpy.context.window.scene=scene;rig.data.pose_position='POSE';rig.animation_data_create()
'''
tail = '''
art=ROOT/'Unity/BusinessAsUsual/Assets/FirstPerson/Art/Animation';art.mkdir(parents=True,exist_ok=True)
for o in bpy.context.view_layer.objects:o.select_set(False)
rig.hide_set(False);rig.select_set(True);bpy.context.view_layer.objects.active=rig
for clip,end in [('Walk',21),('Run',17)]:
    rig.data.pose_position='POSE';rig.animation_data.action=bpy.data.actions[scene['bau_'+clip+'_action']];rig.animation_data.action_slot=rig.animation_data.action.slots[0]
    scene.frame_start=1;scene.frame_end=end;scene.frame_set(1)
    bpy.ops.export_scene.fbx(filepath=str(art/('Sunny_'+clip+'_Phase7B.fbx')),use_selection=True,object_types={'ARMATURE'},add_leaf_bones=False,use_armature_deform_only=False,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,bake_anim_step=.25,bake_anim_simplify_factor=0)
rig.animation_data.action=None;rig.data.pose_position='REST'
bpy.context.window.scene=source_scene
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Assets/Characters/Sunny/Working/Sunny_Phase7B_Locomotion.blend'),copy=True)
print('Phase7B derivative rig and animation-only FBX exports saved')
'''
exec(compile(header + body + tail, 'phase7b_derivative_solver', 'exec'))
