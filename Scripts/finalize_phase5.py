"""Verify paired evidence, record source/build fingerprints and write a local review gallery."""
from pathlib import Path
import hashlib, json, struct, html, datetime, subprocess

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/Diagnostics/Phase5'
PAIR=OUT/'TwoProcess'
host=json.loads((PAIR/'completed-host.json').read_text())
client=json.loads((PAIR/'completed-client.json').read_text())
state_keys=('revision','orderIndex','received','closed','dropped','holder','quantities','progress','movements','receipts','doorOpen','workUses')
assert all(host['state'][k]==client['state'][k] for k in state_keys)
state=host['state'];receipts=state['receipts']
assert state['closed'] and state['orderIndex']==3 and len(receipts)==3
assert len({r['id'] for r in receipts})==len({r['orderId'] for r in receipts})==3
assert sum(r['totalCents'] for r in receipts)==1650
assert all(sum(state['quantities'][i::3])==6 for i in range(3))
assert state['progress']==[1,6,6,6,3,1]
assert state['quantities'][9:12]==[3,3,3] and state['quantities'][15:18]==[3,3,3]
assert all(c['passed'] for c in json.loads((PAIR/'validation.json').read_text())['checks'])

def digest(path):return hashlib.sha256(path.read_bytes()).hexdigest()
images={}
for path in PAIR.glob('*.png'):
    data=path.read_bytes()
    assert data[:8]==b'\x89PNG\r\n\x1a\n'
    w,h=struct.unpack('>II',data[16:24]);assert (w,h)==(1280,720)
    images[path.name]=dict(width=w,height=h,sha256=hashlib.sha256(data).hexdigest())
required={'host.png','client.png','host-cooperative.png','client-cooperative.png','host-carrying.png','client-carrying.png'}
assert required<=images.keys()

source_paths=list((ROOT/'Unity/BusinessAsUsual/Assets/Multiplayer').rglob('*'))
source_paths += [ROOT/'Unity/BusinessAsUsual/Assets/Shopkeeper/Scripts'/n for n in ('ShopHUD.cs','ShopDomain.cs','ShopShift.cs','StockCrate.cs')]
source_paths += [ROOT/'Unity/BusinessAsUsual/Assets/Prototype/Scripts'/n for n in ('PlayerInteractor.cs','SimpleDoor.cs','BasicWorkstation.cs')]
source_paths += [ROOT/'Scripts'/n for n in ('test_phase5_network.py','launch_phase5.ps1')]
build_paths=[ROOT/'Builds/Phase5/BusinessAsUsual.exe',ROOT/'Builds/Phase5/BusinessAsUsual_Data/Managed/Assembly-CSharp.dll']
summary=dict(timestampUtc=datetime.datetime.now(datetime.timezone.utc).isoformat(),independentSharedStateVerified=True,revenueCents=1650,receipts=3,tasksComplete=6,stabilityContribution=3,
    sharedSnapshots=host['snapshots'],sharedSnapshotUtf8Bytes=host['bytes'],serverAccepted=host['accepted'],serverRejected=host['rejected'],
    serverInputCommands=sum(p['inputs'] for p in host['players']),images=images,
    sources={p.relative_to(ROOT).as_posix():digest(p) for p in source_paths if p.is_file()},build={p.relative_to(ROOT).as_posix():digest(p) for p in build_paths})
for role in ('host','client'):
    log=(PAIR/f'{role}.log').read_text(errors='replace')
    assert 'NullReferenceException' not in log and 'MissingReferenceException' not in log and 'Exception:' not in log
(OUT/'validation-summary.json').write_text(json.dumps(summary,indent=2))

intro=f"Two independent Windows processes · 30/30 automated transport checks · $16.50 · 3 unique receipts · +3 Stability. {host['snapshots']} shared snapshots, {host['bytes']:,} application payload bytes. No human-controlled playtest."
cards=[]
for title,a,b in [('Cooperative gameplay','host-cooperative.png','client-cooperative.png'),('Holder-specific carrying UI','host-carrying.png','client-carrying.png'),('Synchronized completed shift','host.png','client.png')]:
    cards.append(f'<section><h2>{title}</h2><div class="pair"><figure><img src="TwoProcess/{a}" alt="Host: {title}"><figcaption>Host / Sunny 0</figcaption></figure><figure><img src="TwoProcess/{b}" alt="Client: {title}"><figcaption>Client / Sunny {client["localId"]}</figcaption></figure></div></section>')
page='<!doctype html><meta charset="utf-8"><title>Phase 5 multiplayer validation</title><style>body{margin:0;padding:32px;background:#244047;color:#f6e9c9;font:17px system-ui}main{max-width:1600px;margin:auto}h1,h2{color:#efc864}p{max-width:1000px;line-height:1.6}.pair{display:grid;grid-template-columns:1fr 1fr;gap:20px}figure{margin:0;background:#152d33;padding:8px}img{width:100%;display:block}figcaption{padding:10px}@media(max-width:850px){.pair{grid-template-columns:1fr}}</style><main><h1>Business as Usual — two-player shift</h1><p>'+html.escape(intro)+'</p>'+''.join(cards)+'</main>'
(OUT/'review.html').write_text(page,encoding='utf-8')
doc=ROOT/'Docs/phase5-validation.md'
text=doc.read_text(encoding='utf-8')
import re
text=re.sub(r'\*\*\d+ shared snapshots\*\* and \*\*[\d,]+ UTF-8 payload bytes\*\*',f'**{host["snapshots"]} shared snapshots** and **{host["bytes"]:,} UTF-8 payload bytes**',text)
doc.write_text(text,encoding='utf-8')
print('Paired state, conservation, receipts, screenshots, logs and fingerprints verified.')
print(f'Snapshots: {host["snapshots"]}; UTF-8 payload bytes: {host["bytes"]:,}')
