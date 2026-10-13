from pathlib import Path
import json
p=Path('Docs/meshy-credit-ledger.json');d=json.loads(p.read_text(encoding='utf-8-sig'))
ids={'01a1340f-2c33-73c2-b8b5-c13d36acd82f':'01a13429-f3f7-726e-8889-2ca3f352c2e6','01a1340f-3214-7212-aebf-4f27da1852e2':'01a13429-ff9b-7762-a002-a769353078a0'}
for t in d['tasks']:
 if t.get('previewId') in ids:t.update(refineId=ids[t['previewId']],refineCredits=10,refineReservedCredits=0,status='TEXTURE_SUBMITTED')
d['spentTotal']=sum(t.get('previewCredits',0)+t.get('refineCredits',0) for t in d['tasks']);d['remaining']=d['limit']-d['spentTotal'];d['reservedTotal']=0
p.write_text(json.dumps(d,indent=2)+'\n',encoding='utf-8');print(d['spentTotal'],'/ 300 committed;',d['remaining'],'remaining')
