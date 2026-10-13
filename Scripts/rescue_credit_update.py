from pathlib import Path
import json
p=Path('Docs/meshy-credit-ledger.json');d=json.loads(p.read_text(encoding='utf-8-sig'))
for t in d['tasks']:
 if t.get('previewId')=='01a1312d-d516-7203-b242-e95cf1db0746':t['status']='SUCCEEDED'
entries=[dict(asset='RescueV2 Engine',previewId='01a13259-01a5-758b-8d8c-e953585fd53d',refineId='01a1325c-0dcb-70fd-8e65-7e4f2b0f32b1',previewCredits=5,refineCredits=10,faceCount=8979,status='TEXTURE_SUBMITTED'),dict(asset='RescueV2 Cart rejected cabinet',previewId='01a13259-085a-75d8-b4fb-8a227bd69e91',previewCredits=5,refineCredits=0,faceCount=12355,status='REJECTED_GEOMETRY',reason='Closed roof and cabinet silhouette; not an open collection cart.'),dict(asset='RescueV2 Open cart retry',previewId='01a1325c-0e19-77ac-9706-28d6d668eea6',previewCredits=5,refineCredits=0,status='PREVIEW_SUBMITTED')]
for t in entries:
 if not any(x.get('previewId')==t['previewId'] for x in d['tasks']):d['tasks'].append(dict(t,phase='Phase8ARescueV2',model='meshy-t2',formats=['fbx','glb']))
d['spentTotal']=sum(t.get('previewCredits',0)+t.get('refineCredits',0) for t in d['tasks']);d['remaining']=d['limit']-d['spentTotal'];d['reservedTotal']=sum(t.get('refineReservedCredits',0) for t in d['tasks'] if t.get('status') in ('TEXTURE_SUBMITTED','PREVIEW_SUBMITTED'))
p.write_text(json.dumps(d,indent=2)+'\n',encoding='utf-8');print('Committed',d['spentTotal'],'remaining',d['remaining'],'reserved',d['reservedTotal'])
