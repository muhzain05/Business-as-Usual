"""Independently verify Phase 4 artifacts and preservation; does not run Unity."""
import hashlib,json
from collections import defaultdict
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/Diagnostics/Phase4'
baseline=json.loads((OUT/'preservation-baseline.json').read_text())
changed=[p for p,h in baseline.items() if not (ROOT/p).exists() or hashlib.sha256((ROOT/p).read_bytes()).hexdigest()!=h]
report=json.loads((OUT/'playmode-validation.json').read_text())
failed=[c for c in report['checks'] if not c['passed']]
balances=defaultdict(lambda:defaultdict(int))
for sku in ('cola','tea','crackers'):balances['delivery'][sku]=6
for i,m in enumerate(report['summary']['movements'],1):
    assert m['sequence']==i and m['quantity']>0
    assert balances[m['from']][m['sku']]>=m['quantity'],m
    balances[m['from']][m['sku']]-=m['quantity'];balances[m['to']][m['sku']]+=m['quantity']
for sku in ('cola','tea','crackers'):
    assert sum(a[sku] for a in balances.values())==6
    assert balances['shelf'][sku]==3 and balances['sold'][sku]==3
    assert balances['carry'][sku]==balances['basket'][sku]==balances['dock'][sku]==balances['delivery'][sku]==0
receipts=report['summary']['receipts']
assert len(receipts)==len({r['id'] for r in receipts})==len({r['orderId'] for r in receipts})==3
assert [r['totalCents'] for r in receipts]==[510,520,620]
for r in receipts:
    assert all(l['totalCents']==l['unitPriceCents']*l['quantity'] for l in r['lines'])
    assert sum(l['totalCents'] for l in r['lines'])==r['totalCents']
assert sum(r['totalCents'] for r in receipts)==report['summary']['revenueCents']==1650
assert report['summary']['stabilityContribution']==3
result={'prototypeFilesVerified':len(baseline),'changedPrototypeFiles':changed,'unityChecks':len(report['checks']),'failedUnityChecks':failed,'inventoryConserved':True,'unitsDelivered':18,'unitsSold':9,'unitsRemaining':9,'revenueCents':1650,'receiptIds':[r['id'] for r in receipts]}
(OUT/'independent-verification.json').write_text(json.dumps(result,indent=2))
print(json.dumps(result,indent=2))
assert not changed and not failed
