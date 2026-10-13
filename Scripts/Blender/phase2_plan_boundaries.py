"""Read-only candidate mask for underside review; never removes polygons."""
import bpy,json
from pathlib import Path

ROOT=Path(globals().get('BAU_PROJECT_ROOT','D:/Business as Usual'))
VERSION=globals().get('BAU_VERSION','v001')
OUT=ROOT/'Docs/Diagnostics/Phase2'/VERSION
obj=bpy.data.objects[f'Sunny_{VERSION}_Shorts']
ids=[p.index for p in obj.data.polygons if p.center.z<.305 and p.normal.z<-.55]
report={'module':obj.name,'candidate_underside_polygons':ids,'count':len(ids),
    'selection_rule':'rest centroid Z < 0.305 and normal Z < -0.55; review-only candidate, not an approved deletion mask',
    'note':'Shorts have a waist boundary only; concealed body thighs cannot be attached to an existing garment cuff boundary. Review cap opening/inner rim reconstruction on copied garment after approval.'}
(OUT/'shorts-underside-review.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(f'{len(ids)} underside triangles flagged for review; no geometry changed.')
