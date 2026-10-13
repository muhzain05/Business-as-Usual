"""Blender read-only inspection of our new source scenes; no export or scene save."""
from pathlib import Path
import bpy,json
r=Path('D:/Business as Usual/Docs/Diagnostics/Phase8B/Assets');rows=[]
for name in ['StaffKiosk','ReturnsCabinet']:
 bpy.ops.wm.open_mainfile(filepath=str(r/(name+'.blend')))
 objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
 for o in objects:o.data.calc_loop_triangles()
 rows.append(dict(name=name,meshes=len(objects),polygons=sum(len(o.data.polygons) for o in objects),triangles=sum(len(o.data.loop_triangles) for o in objects),vertices=sum(len(o.data.vertices) for o in objects),objects=[dict(name=o.name,materials=[m.name for m in o.data.materials],uvLayers=len(o.data.uv_layers)) for o in objects]))
(r/'geometry-audit.json').write_text(json.dumps(rows,indent=2),encoding='utf-8');print(rows)
