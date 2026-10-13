"""Executed in the user's Blender: authored articulation on the inspected Meshy source."""
import bpy,bmesh,math,json
from pathlib import Path
from mathutils import Vector,Matrix
out=Path('D:/Business as Usual/Unity/BusinessAsUsual/Assets/RescueV2/Resources/RescueV2/Models')
out.mkdir(parents=True,exist_ok=True)
source=bpy.data.objects['RescueWagonSource']
collection=bpy.data.collections.new('Rescue production wagon');bpy.context.scene.collection.children.link(collection)
root=bpy.data.objects.new('UtilityWagon',None);collection.objects.link(root)
body=source.copy();body.data=source.data.copy();body.name='Body';collection.objects.link(body);source.hide_set(True)
# Front is source -X. Extract the upward bonnet skin; preserve the original UVs.
selected=[]
for f in body.data.polygons:
 c=f.center
 if -.469<c.x<-.236 and abs(c.y)<.157 and c.z>-.024 and f.normal.z>.30:selected.append(f.index)
hood=body.copy();hood.data=body.data.copy();hood.name='Hood';collection.objects.link(hood)
def keep(obj,indices,invert=False):
 bm=bmesh.new();bm.from_mesh(obj.data);bm.faces.ensure_lookup_table()
 bmesh.ops.delete(bm,geom=[f for f in bm.faces if (f.index in indices)==invert],context='FACES')
 bmesh.ops.delete(bm,geom=[v for v in bm.verts if not v.link_faces],context='VERTS')
 bm.to_mesh(obj.data);bm.free();obj.data.update()
keep(hood,set(selected));keep(body,set(selected),True)
# Apply a real 2.6m length, wheels resting 2cm above floor, front along Blender -Y.
convert=Matrix.Rotation(math.pi/2,4,'Z') @ Matrix.Scale(2.6,4)
for obj in (body,hood):
 for v in obj.data.vertices:v.co=convert@v.co+Vector((0,0,.74904))
 obj.parent=root
hinge=Vector((0,-.6136,.83224))
for v in hood.data.vertices:v.co-=hinge
hood.location=hinge;hood.rotation_mode='XYZ'
solid=hood.modifiers.new('Bonnet thickness','SOLIDIFY');solid.thickness=.012
bpy.context.view_layer.objects.active=hood;hood.select_set(True)
bpy.ops.object.modifier_apply(modifier=solid.name);hood.select_set(False)
def material(name,rgb,metal=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*rgb,1);m.use_nodes=True
 p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*rgb,1);p.inputs['Roughness'].default_value=.62;p.inputs['Metallic'].default_value=metal
 return m
ink=material('Graphite',( .08,.13,.13));teal=material('Town teal',(.12,.28,.26));cream=material('Warm enamel',(.77,.70,.51));brass=material('Brass',(.65,.48,.16),.5);coral=material('Positive terminal',(.67,.22,.16))
def box(name,at,size,mat,parent=root,bevel=.02):
 bpy.ops.mesh.primitive_cube_add(size=1,location=at);o=bpy.context.object;o.name=name;o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 for c in list(o.users_collection):c.objects.unlink(o)
 collection.objects.link(o);o.parent=parent;o.data.materials.append(mat)
 if bevel:
  mod=o.modifiers.new('Crafted edges','BEVEL');mod.width=bevel;mod.segments=2
  bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
 return o
box('Engine well',(0,-.89,.55),(.73,.55,.05),ink)
box('Engine rear bulkhead',(0,-.63,.66),(.75,.04,.26),teal)
box('Radiator',(0,-1.13,.62),(.61,.065,.18),ink)
for x in (-.365,.365):box('Bay inner fender',(x,-.88,.65),(.025,.52,.20),teal)
box('Hood latch',(0,-1.215,.708),(.10,.025,.035),brass,bevel=.009)
for x in (-.25,.25):box('Bonnet hinge',(x,-.612,.807),(.05,.055,.03),brass,bevel=.005)
box('Battery tray',(.235,-.865,.605),(.25,.29,.025),ink)
# Clips are authored around the cowl hinge, never the bonnet centre.
for frame,angle in [(1,0),(7,-.11),(18,-1.13),(24,-1.19)]:
 hood.rotation_euler.x=angle;hood.keyframe_insert(data_path='rotation_euler',frame=frame)
hood.animation_data.action.name='HoodOpen';bpy.context.scene.render.fps=30;bpy.context.scene.frame_end=24;bpy.context.scene.frame_set(1)
# Save texture files without baking generated lighting into new materials.
for img in bpy.data.images:
 if img.name.startswith('Image') or img.name.startswith('texture'):
  img.filepath_raw=str(out/(img.name.split('.')[0]+'.png'));img.file_format='PNG';img.save()
bpy.ops.object.select_all(action='DESELECT')
for obj in collection.objects:obj.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(out/'UtilityWagon.fbx'),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_force_startend_keying=True,path_mode='COPY',embed_textures=False)
bpy.ops.wm.save_as_mainfile(filepath='D:/Business as Usual/Docs/Diagnostics/Phase8ARescueV2/UtilityWagon.blend')
(out/'wagon-articulation.json').write_text(json.dumps(dict(source='01a13132-ea9b-73e8-8c16-eb0336078bdf',sourceFaces=len(source.data.polygons),bonnetFaces=len(hood.data.polygons),selectedSkinFaces=len(selected),lengthMetres=2.6,hingeBlender=list(hinge),front='-Y',clip='HoodOpen',duration=.8),indent=2))
print('Exported articulated vehicle; extracted skin faces',len(selected))


