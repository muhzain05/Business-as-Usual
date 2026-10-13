"""Blender batch audit of the two generated previews; no user scene is opened or modified."""
import bpy,math,json
from pathlib import Path
from mathutils import Vector
root=Path('D:/Business as Usual/Docs/Diagnostics/Phase8B/Assets');result=[]
for name in ['kiosk','returns']:
 bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=str(root/(name+'-preview.glb')))
 objects=[o for o in bpy.context.scene.objects if o.type=='MESH'];bounds=[o.matrix_world@Vector(c) for o in objects for c in o.bound_box];minimum=Vector(tuple(min(v[i] for v in bounds) for i in range(3)));maximum=Vector(tuple(max(v[i] for v in bounds) for i in range(3)));center=(minimum+maximum)/2;size=(maximum-minimum).length
 mat=bpy.data.materials.new('Preview clay');mat.diffuse_color=(.28,.48,.40,1);mat.use_nodes=True;node=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');node.inputs['Base Color'].default_value=(.28,.48,.40,1);node.inputs['Roughness'].default_value=.85
 for o in objects:o.data.materials.clear();o.data.materials.append(mat)
 bpy.ops.object.camera_add(location=center+Vector((1.3,-1.7,1.15))*size);cam=bpy.context.object;cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=size*1.25;bpy.context.scene.camera=cam
 for offset,power in [((2,-3,4),1400),((-3,-1,2),900)]:
  bpy.ops.object.light_add(type='AREA',location=center+Vector(offset)*size);lamp=bpy.context.object;lamp.data.energy=power;lamp.data.shape='DISK';lamp.data.size=size*3;lamp.rotation_euler=(center-lamp.location).to_track_quat('-Z','Y').to_euler()
 s=bpy.context.scene;s.render.engine='CYCLES';s.cycles.samples=24;s.render.resolution_x=720;s.render.resolution_y=720;s.render.resolution_percentage=100;s.world=bpy.data.worlds.new('Preview daylight');s.world.color=(.45,.45,.45);s.render.image_settings.file_format='PNG';s.render.filepath=str(root/(name+'-preview.png'));bpy.ops.render.render(write_still=True)
 result.append(dict(name=name,meshes=len(objects),triangles=sum(len(o.data.polygons) for o in objects),min=list(minimum),max=list(maximum)))
(root/'preview-audit.json').write_text(json.dumps(result,indent=2));print(result)
