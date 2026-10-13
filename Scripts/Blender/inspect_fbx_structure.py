"""Read raw supplied FBX arrays with Blender's bundled parser."""
import json
from pathlib import Path
from io_scene_fbx import parse_fbx

ROOT=Path(globals().get('BAU_PROJECT_ROOT', 'D:/Business as Usual'))
report={}
for path in (ROOT/'Assets/Characters/Sunny/Source').rglob('*.fbx'):
    tree,version=parse_fbx.parse(str(path)); geometries=[]
    objects=next(e for e in tree.elems if e.id==b'Objects')
    for elem in objects.elems:
        if elem.id!=b'Geometry': continue
        v=next((e for e in elem.elems if e.id==b'Vertices'),None)
        p=next((e for e in elem.elems if e.id==b'PolygonVertexIndex'),None)
        geometries.append({'name':str(elem.props[1]),'raw_vertices':len(v.props[0])//3 if v else None,
            'raw_polygons':sum(x<0 for x in p.props[0]) if p else None})
    report[path.name]={'fbx_version':version,'geometries':geometries}
(ROOT/'Docs/Diagnostics/fbx-structure.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print('Raw FBX geometry report written; no import or scene edits.')
