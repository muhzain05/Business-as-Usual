"""Normalize Meshy assets on export copies; retain raw GLBs and original Sunny scene."""
import bpy,json
from pathlib import Path
from mathutils import Vector,Matrix
ROOT=Path('D:/Business as Usual')
OUT=ROOT/'Unity/BusinessAsUsual/Assets/Town/Phase7B/Models';OUT.mkdir(parents=True,exist_ok=True)
source=bpy.data.scenes['Phase7B_Meshy_Inspection']
scene=bpy.data.scenes.new('Phase7B_Environment_Export');scene.render.fps=24;bpy.context.window.scene=scene
rows=[]
for name,size in [('Market',(7.7,6.86,4.56)),('Garage',(7.7,6.78,4.5)),('Cottage',(5.8,5.6,5.8)),('Tree',(3.4,3.3,4.3)),('Lamp',(.71,.70,3.0))]:
    root=source.objects[name];src=next(o for o in root.children_recursive if o.type=='MESH')
    obj=src.copy();obj.data=src.data.copy();obj.parent=None;obj.name=name;scene.collection.objects.link(obj)
    matrix=src.matrix_world.copy();matrix.translation-=root.location
    coords=[matrix@v.co for v in obj.data.vertices];lo=Vector(tuple(min(v[j] for v in coords) for j in range(3)));hi=Vector(tuple(max(v[j] for v in coords) for j in range(3)))
    for vertex,p in zip(obj.data.vertices,coords):
        vertex.co=Vector(((p.x-(lo.x+hi.x)/2)*size[0]/(hi.x-lo.x),(p.y-(lo.y+hi.y)/2)*size[1]/(hi.y-lo.y),(p.z-lo.z)*size[2]/(hi.z-lo.z)))
    obj.matrix_world=Matrix.Identity(4)
    for o in scene.objects:o.select_set(False)
    obj.select_set(True);bpy.context.view_layer.objects.active=obj
    bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,path_mode='RELATIVE',embed_textures=False)
    rows.append({'asset':name,'dimensions':size,'faces':len(obj.data.polygons),'source':str(ROOT/'Assets/Environment/Phase7B/Meshy'/(name+'.glb'))})
bpy.context.window.scene=bpy.data.scenes[source['previous_scene']]
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Assets/Environment/Phase7B/Phase7B_Environment.blend'),copy=True)
(ROOT/'Docs/Diagnostics/Phase7B/meshy-imports.json').write_text(json.dumps(rows,indent=2))
print(json.dumps(rows))
