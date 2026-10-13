"""Original articulated work gloves and authored contact gestures; no character source overwrite."""
import bpy,math
from pathlib import Path
from mathutils import Vector
scene=bpy.data.scenes.new('BAU Rescue Work Gloves');bpy.context.window.scene=scene;scene.render.fps=30;scene.frame_start=1;scene.frame_end=151
root=bpy.data.objects.new('WorkGloves',None);scene.collection.objects.link(root)
def mat(name,color):
 m=bpy.data.materials.new(name);m.use_nodes=True;n=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');n.inputs['Base Color'].default_value=(*color,1);n.inputs['Roughness'].default_value=.80;m.diffuse_color=(*color,1);return m
cream=mat('Glove canvas',(.70,.64,.46));teal=mat('Work sleeve',(.12,.27,.25));ink=mat('Rubber grip',(.075,.14,.13))
def empty(name,parent,at):
 o=bpy.data.objects.new(name,None);scene.collection.objects.link(o);o.parent=parent;o.location=at;o.rotation_mode='XYZ';return o
def rounded(name,parent,at,size,material,radius=.008):
 bpy.ops.mesh.primitive_cube_add(size=1);o=bpy.context.object;o.name=name;o.parent=parent;o.location=at;o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(material)
 bevel=o.modifiers.new('Sewn rounded form','BEVEL');bevel.width=radius;bevel.segments=3;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=bevel.name)
 for p in o.data.polygons:p.use_smooth=True
 return o
hands=[];joints=[]
for side,sign in [('Left',1),('Right',-1)]:
 hand=empty(side+'Wrist',root,(sign*.18,-.38,-.28));hands.append((hand,sign,hand.location.copy()))
 rounded(side+'Palm',hand,(0,-.020,0),(.075,.085,.033),cream,.014)
 rounded(side+'Back patch',hand,(0,-.018,.0175),(.056,.052,.003),teal,.007)
 cuff=rounded(side+'Cuff',hand,(0,.041,-.002),(.087,.035,.054),teal,.009)
 sleeve=rounded(side+'Forearm',hand,(sign*.025,.152,-.056),(.090,.225,.070),teal,.026);sleeve.rotation_euler.x=-.38
 rounded(side+'Cuff seam',hand,(0,.052,.024),(.080,.004,.003),cream,.001)
 for index,(x,length) in enumerate([(-.025,.060),(-.008,.067),(.010,.061),(.027,.047)]):
  first=empty(side+'Finger'+str(index)+'A',hand,(x,-.061,0));first.rotation_euler.z=(index-1.5)*.055
  previous=first
  for j in range(3):
   joint=previous if j==0 else empty(side+'Finger'+str(index)+chr(65+j),previous,(0,-length/3,0))
   rounded(side+'Finger'+str(index)+' segment '+str(j),joint,(0,-length/6,0),(.015,length/3+.005,.020),cream,.007)
   joints.append((joint,j,index));previous=joint
  rounded(side+'Grip'+str(index),previous,(0,-length/6,-.009),(.012,length/3,.003),ink,.002)
 thumb=empty(side+'ThumbA',hand,(-sign*.043,-.007,-.005));thumb.rotation_euler.z=-sign*.7
 rounded(side+'Thumb proximal',thumb,(0,-.016,0),(.024,.040,.024),cream,.010)
 tip=empty(side+'ThumbB',thumb,(0,-.034,0));rounded(side+'Thumb tip',tip,(0,-.014,0),(.024,.032,.024),cream,.010);joints.append((tip,1,4))
# A single scene take contains seven reusable authored sections, imported as real Unity clips.
sections=[('Idle',1,21),('Reach',22,42),('Grip',43,63),('Tool',64,84),('Probe',85,105),('Stamp',106,126),('Sort',127,151)]
for name,start,end in sections:
 for frame,t in [(start,0),((start+end)//2,.5),(end,1)]:
  pulse=math.sin(t*math.pi)
  for hand,sign,rest in hands:
   hand.location=rest.copy();hand.rotation_euler=(0,0,sign*.10)
   if name=='Idle':hand.location.z+=.003*pulse
   elif name=='Grip':hand.location.x=sign*.145;hand.location.y=-.48;hand.location.z=-.33;hand.rotation_euler.z=sign*.32
   elif name=='Tool' and sign<0:hand.location+=Vector((.055,-.08*pulse,.075*pulse));hand.rotation_euler.y=.55*pulse
   elif name=='Probe' and sign<0:hand.location+=Vector((.045,-.11*pulse,.095*pulse));hand.rotation_euler.x=.10*pulse
   elif name=='Stamp' and sign<0:hand.location+=Vector((.07,-.07*pulse,.07*math.sin(t*math.pi*2)));hand.rotation_euler.x=-.12*pulse
   elif name=='Sort':hand.location+=Vector((-sign*.025,-.12*pulse,.11*pulse))
   elif name=='Reach' and sign<0:hand.location+=Vector((.04,-.11*pulse,.10*pulse))
   hand.keyframe_insert(data_path='location',frame=frame);hand.keyframe_insert(data_path='rotation_euler',frame=frame)
  for joint,j,index in joints:
   joint.rotation_euler.x=(.9 if name=='Grip' else .50 if name=='Tool' else .18 if name=='Reach' else .28)+(.08*pulse if name=='Idle' else 0)
   joint.keyframe_insert(data_path='rotation_euler',frame=frame)
scene.frame_set(1);bpy.ops.object.select_all(action='SELECT')
out=Path(r'D:/Business as Usual/Unity/BusinessAsUsual/Assets/RescueV2/Resources/RescueV2/Models')
bpy.ops.export_scene.fbx(filepath=str(out/'WorkGloves.fbx'),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='COPY')
bpy.ops.wm.save_as_mainfile(filepath=r'D:/Business as Usual/Docs/Diagnostics/Phase8ARescueV2/WorkGloves.blend')
print('Authored original articulated gloves: Idle, Reach, Grip, Tool, Probe, Stamp, Sort')
