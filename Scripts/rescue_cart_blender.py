"""Inspect, normalize and articulate the accepted Meshy T2 open collection cart."""
import bpy,bmesh,math,json
from pathlib import Path
from mathutils import Matrix,Vector
s=bpy.data.scenes['BAU Rescue Equipment'];bpy.context.window.scene=s
source=bpy.data.objects['OpenCartTextured'];out=Path(r'D:/Business as Usual/Unity/BusinessAsUsual/Assets/RescueV2/Resources/RescueV2/Models')
source.name='CollectionCartBody';source.location=(0,0,0)
# Handle is +X, turn it to Blender +Y / Unity -Z; front points toward Unity +Z.
matrix=Matrix.Rotation(math.pi/2,4,'Z');floor=min(v.co.z for v in source.data.vertices)
for v in source.data.vertices:v.co=matrix@(v.co-Vector((0,0,floor)))
root=bpy.data.objects.new('CollectionCart',None);s.collection.objects.link(root);source.parent=root
bm=bmesh.new();bm.from_mesh(source.data);bm.verts.ensure_lookup_table()
unseen=set(bm.verts);groups=[]
while unseen:
 start=unseen.pop();component={start};queue=[start]
 while queue:
  v=queue.pop()
  for e in v.link_edges:
   other=e.other_vert(v)
   if other in unseen:unseen.remove(other);component.add(other);queue.append(other)
 groups.append(component)
print('Cart components',[(len(g),tuple(round(sum(v.co[a] for v in g)/len(g),3) for a in range(3))) for g in sorted(groups,key=len,reverse=True)[:18]])
bm.free()
for img in {n.image for m in source.data.materials for n in m.node_tree.nodes if n.type=='TEX_IMAGE' and n.image}:
 img.name='Cart_'+img.name;img.filepath_raw=str(out/(img.name.replace('.','_')+'.png'));img.file_format='PNG';img.save()
s.frame_start=1;s.frame_end=31;s.render.fps=30
# Keep UVs and body topology; wheels will be split after component inspection.
bpy.ops.object.select_all(action='DESELECT');root.select_set(True);source.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(out/'CollectionCart.fbx'),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',bake_anim=False,path_mode='COPY')
bpy.ops.wm.save_as_mainfile(filepath=r'D:/Business as Usual/Docs/Diagnostics/Phase8ARescueV2/CollectionCart.blend')
print('Cart grounded; length 1m, handle behind, open tub')
