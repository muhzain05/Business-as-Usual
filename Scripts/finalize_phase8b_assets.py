from pathlib import Path
import json,hashlib
r=Path(__file__).resolve().parents[1];p=r/'Docs/meshy-credit-ledger.json';d=json.loads(p.read_text(encoding='utf-8-sig'))
for t in d['tasks']:
 if t.get('phase')=='Phase8B':t['status']='SUCCEEDED';t['acceptedFor']='Integrated native menu/holds; normalized in Blender with grounded pivots and authored support surfaces.'
d['spentTotal']=sum(t.get('previewCredits',0)+t.get('refineCredits',0) for t in d['tasks']);d['reservedTotal']=sum(t.get('refineReservedCredits',0) for t in d['tasks']);d['remaining']=d['limit']-d['spentTotal']
assert d['spentTotal']==170 and d['reservedTotal']==0 and d['spentTotal']<=d['limit']
p.write_text(json.dumps(d,indent=2)+'\n',encoding='utf-8')
folder=r/'Docs/Diagnostics/Phase8B/Assets';geometry=json.loads((folder/'geometry-audit.json').read_text(encoding='utf-8-sig'));cleanup=json.loads((folder/'production-cleanup.json').read_text(encoding='utf-8-sig'))
for row in cleanup:
 audit=next(g for g in geometry if g['name']==row['name']);row['polygons']=audit['polygons'];row['triangles']=audit['triangles']
(folder/'production-cleanup.json').write_text(json.dumps(cleanup,indent=2),encoding='utf-8')
assets=[]
for f in list(folder.iterdir())+list((r/'Unity/BusinessAsUsual/Assets/Phase8B/Resources/Phase8B').rglob('*')):
 if f.is_file() and f.name!='provenance.json':assets.append(dict(path=str(f.relative_to(r)),bytes=f.stat().st_size,sha256=hashlib.sha256(f.read_bytes()).hexdigest()))
(r/'Docs/Diagnostics/Phase8B/Assets/provenance.json').write_text(json.dumps(dict(phaseCost=30,cumulativeSpent=d['spentTotal'],reserved=0,remaining=d['remaining'],model='meshy-t2',tasks=[t for t in d['tasks'] if t.get('phase')=='Phase8B'],files=assets,cleanupTool='Blender 5.2.2 LTS batch CLI; MCP unavailable'),indent=2),encoding='utf-8')
print('30 new / 170 cumulative / 300 authorized; 130 remaining; zero reserved.')
