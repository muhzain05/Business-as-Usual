"""Blender cleanup: replace jagged face extraction with a fitted, thickened bonnet loft."""
import bpy,math
from mathutils import Vector
s=bpy.data.scenes['BAU Rescue Vehicle'];bpy.context.window.scene=s;s.frame_set(1)
old=bpy.data.objects['Hood'];old.name='Extracted bonnet archive';old.hide_render=True;old.hide_set(True)
collection=bpy.data.collections['Rescue production wagon'];root=bpy.data.objects['UtilityWagon']
ys=[-1.205,-1.16,-1.06,-.90,-.75,-.6136]
widths=[.34,.37,.40,.405,.413,.418]
heights=[.711,.741,.772,.797,.820,.83224]
vertices=[];faces=[]
hinge=Vector((0,-.6136,.83224))
for y,w,z in zip(ys,widths,heights):
 for col in range(7):
  t=(col-3)/3;vertices.append(tuple(Vector((t*w,y,z-.024*t*t))-hinge))
for row in range(5):
 for col in range(6):
  a=row*7+col;faces.append((a,a+1,a+8,a+7))
mesh=bpy.data.meshes.new('Clean bonnet loft');mesh.from_pydata(vertices,[],faces);mesh.update()
h=bpy.data.objects.new('Hood',mesh);collection.objects.link(h);h.parent=root;h.location=hinge;h.rotation_mode='XYZ'
mat=bpy.data.materials.new('Bonnet enamel');mat.use_nodes=True;node=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');node.inputs['Base Color'].default_value=(.19,.32,.28,1);node.inputs['Roughness'].default_value=.58;mat.diffuse_color=(.19,.32,.28,1);mesh.materials.append(mat)
for p in mesh.polygons:p.use_smooth=True
solid=h.modifiers.new('Sheet steel thickness','SOLIDIFY');solid.thickness=.012
bevel=h.modifiers.new('Folded bonnet lip','BEVEL');bevel.width=.009;bevel.segments=3
bpy.context.view_layer.objects.active=h;h.select_set(True)
for mod in list(h.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
for frame,angle in [(1,0),(7,-.11),(18,-1.13),(24,-1.19)]:h.rotation_euler.x=angle;h.keyframe_insert(data_path='rotation_euler',frame=frame)
h.animation_data.action.name='HoodCleanOpen'
for o in collection.objects:
 if o.name.startswith('Bay inner fender'):o.location.z=.585
s.frame_set(1);bpy.ops.object.select_all(action='DESELECT')
for o in collection.objects:
 if not o.name.startswith('Extracted bonnet archive'):o.select_set(True)
bpy.ops.export_scene.fbx(filepath=r'D:/Business as Usual/Unity/BusinessAsUsual/Assets/RescueV2/Resources/RescueV2/Models/UtilityWagon.fbx',use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='COPY')
bpy.ops.wm.save_as_mainfile(filepath=r'D:/Business as Usual/Docs/Diagnostics/Phase8ARescueV2/UtilityWagon.blend')
s.frame_set(24)
print('Clean bonnet: fitted loft, folded lip, authored rear-hinge curve')
