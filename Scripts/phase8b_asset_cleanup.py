"""Batch Blender cleanup and editable production sources for the inspected T2 assets."""
import bpy,math,json
from pathlib import Path
from mathutils import Vector
root=Path('D:/Business as Usual');source=root/'Docs/Diagnostics/Phase8B/Assets';out=root/'Unity/BusinessAsUsual/Assets/Phase8B/Resources/Phase8B/Models';out.mkdir(parents=True,exist_ok=True);report=[]
def box(name,at,scale,mat):
 bpy.ops.mesh.primitive_cube_add(size=1,location=at);o=bpy.context.object;o.name=name;o.scale=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mat);return o
for short,name,height in [('kiosk','StaffKiosk',1.78),('returns','ReturnsCabinet',.88)]:
 bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=str(source/(short+'-textured.glb')));objects=[o for o in bpy.context.scene.objects if o.type=='MESH'];points=[o.matrix_world@Vector(c) for o in objects for c in o.bound_box];minimum=Vector(tuple(min(v[i] for v in points) for i in range(3)));maximum=Vector(tuple(max(v[i] for v in points) for i in range(3)));center=(minimum+maximum)/2;factor=height/(maximum.z-minimum.z);stretch=1.45 if short=='returns' else 1
 for o in objects:
  world=o.matrix_world.copy()
  for v in o.data.vertices:
   p=world@v.co;v.co=Vector(((p.x-center.x)*factor*stretch,(p.y-center.y)*factor,(p.z-minimum.z)*factor))
  o.matrix_world.identity();o.name=name+'Body'
  for m in o.data.materials:
   m.name=name+'_'+m.name
   if m.use_nodes:
    for node in m.node_tree.nodes:
     if node.type=='TEX_IMAGE' and node.image:
      node.image.name=name+'_Albedo';node.image.filepath_raw=str(out/(name+'_Albedo.png'));node.image.file_format='PNG';node.image.save()
 wood=bpy.data.materials.new(name+'_Wood');wood.use_nodes=True;node=next(n for n in wood.node_tree.nodes if n.type=='BSDF_PRINCIPLED');node.inputs['Base Color'].default_value=(.28,.17,.08,1);node.inputs['Roughness'].default_value=.85
 paper=bpy.data.materials.new(name+'_Paper');paper.use_nodes=True;node=next(n for n in paper.node_tree.nodes if n.type=='BSDF_PRINCIPLED');node.inputs['Base Color'].default_value=(.94,.86,.66,1);node.inputs['Roughness'].default_value=.9
 if short=='kiosk':
  front=(minimum.y-center.y)*factor
  objects.append(box('WritingCounter',(0,front-.13,.775),(.99,.42,.045),wood))
  for x in [-.42,.42]:objects.append(box('CounterSupport',(x,front-.26,.375),(.055,.055,.75),wood))
  objects.append(box('BlankStaffBoard',(0,front-.022,1.19),(.78,.026,.53),paper))
 else:
  # A real tray supports the conserved carton at the runtime's 0.28m slot height.
  objects.append(box('LeftReceivingTray',(-.22,-.09,.23),(.43,.39,.05),wood))
  objects.append(box('ReturnsLabel',(0,-.28,.76),(.59,.018,.12),paper))
 pivot=bpy.data.objects.new(name,None);bpy.context.scene.collection.objects.link(pivot)
 for o in objects:o.parent=pivot
 bpy.ops.object.select_all(action='DESELECT');pivot.select_set(True)
 for o in objects:o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=str(out/(name+'.fbx')),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',bake_anim=False,path_mode='COPY')
 bpy.ops.wm.save_as_mainfile(filepath=str(source/(name+'.blend')))
 for o in objects:o.data.calc_loop_triangles()
 report.append(dict(name=name,height=height,meshes=len(objects),polygons=sum(len(o.data.polygons) for o in objects),triangles=sum(len(o.data.loop_triangles) for o in objects),operations=['world transforms applied','grounded pivot','separate texture names','authored supports/label surfaces','FBX + editable BLEND']))
(source/'production-cleanup.json').write_text(json.dumps(report,indent=2));print(report)
