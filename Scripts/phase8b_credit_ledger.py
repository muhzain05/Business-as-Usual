from pathlib import Path
import json
p=Path('Docs/meshy-credit-ledger.json');d=json.loads(p.read_text(encoding='utf-8-sig'))
for name,task,count in [('Phase8B Staff kiosk','01a1340f-2c33-73c2-b8b5-c13d36acd82f',10000),('Phase8B Returns cabinet','01a1340f-3214-7212-aebf-4f27da1852e2',8000)]:
 if not any(t.get('previewId')==task for t in d['tasks']):d['tasks'].append(dict(asset=name,phase='Phase8B',model='meshy-t2',previewId=task,previewCredits=5,refineCredits=0,refineReservedCredits=10,status='PREVIEW_SUBMITTED',targetFaces=count,formats=['fbx','glb']))
d['authorizationPhase8B']='User October 13 Phase 8B brief: Meshy T2 within previously approved cumulative 300-credit budget.'
d['spentTotal']=sum(t.get('previewCredits',0)+t.get('refineCredits',0) for t in d['tasks']);d['remaining']=d['limit']-d['spentTotal'];d['reservedTotal']=sum(t.get('refineReservedCredits',0) for t in d['tasks'] if t.get('status') in ('PREVIEW_SUBMITTED','TEXTURE_SUBMITTED'))
assert d['spentTotal']+d['reservedTotal']<=d['limit'];p.write_text(json.dumps(d,indent=2)+'\n',encoding='utf-8');print(d['spentTotal'],'committed;',d['reservedTotal'],'reserved;',d['remaining'],'unspent')
