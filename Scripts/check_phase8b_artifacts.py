"""Validate the delivered build, native evidence, budget, stage preservation and gallery links."""
from pathlib import Path
import json,hashlib,re,subprocess
r=Path(__file__).resolve().parents[1];d=r/'Docs/Diagnostics/Phase8B';checks=[]
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def check(name,value):checks.append(dict(name=name,passed=bool(value)));assert value,name
exe=r/'Builds/Phase8B/BusinessAsUsual.exe';assembly=exe.parent/'BusinessAsUsual_Data/Managed/Assembly-CSharp.dll';sha=hashlib.sha256(assembly.read_bytes()).hexdigest()
check('native_build_succeeded',exe.exists() and read(d/'build.json')['result']=='Succeeded' and read(d/'build.json')['errors']==0)
sets=['UI720','UI1080','SoloShop','SoloMechanic','SoloSanitation','TwoProcess','FourProcess','PreferencesRestore'];results=[]
for folder in sets:
 rows=read(d/folder/'checks.json');check(folder+'_checks_pass',bool(rows) and all(c['passed'] for c in rows));meta=read(d/folder/'run-metadata.json');results.append(dict(folder=folder,passed=len(rows),assemblySha256=meta['assemblySha256'],latestAssembly=meta['assemblySha256']==sha))
check('latest_build_has_native_ui_and_four_player_coverage',all(read(d/f/'run-metadata.json')['assemblySha256']==sha for f in ['UI720','TwoProcess','FourProcess']))
for folder in ['SoloShop','SoloMechanic','SoloSanitation']:
 final=read(d/folder/'shift-summary-host.json');w=final['state']['workday'];check(folder+'_real_complete_conserved_day',w['closed'] and w['shiftSigned'] and w['serviceSigned'] and w['customerOrders']>=1 and sum(i['custody']=='recorded' for i in w['items'])==3 and len({i['id'] for i in w['items']})==7);check(folder+'_private_practice_complete',all(final['dayPrivate'][k] for k in ['detour','declaredDirect','productive']))
cart=read(d/'SoloSanitation/loaded-cart-host.json')['state']['workday'];check('native_W_input_moved_the_cart',abs(cart['cartZ']-(-7.5))+abs(cart['cartX']-7.5)>2)
ledger=read(r/'Docs/meshy-credit-ledger.json');check('credit_budget_and_task_statuses',ledger['spentTotal']==170 and ledger['spentTotal']<=ledger['limit']==300 and ledger['reservedTotal']==0 and all(t['status']=='SUCCEEDED' for t in ledger['tasks'] if t.get('phase')=='Phase8B'))
p=read(d/'preservation-audit.json');allowed={'Docs/meshy-credit-ledger.json','Scripts/test_phase8a_network.py'}|{'Unity/BusinessAsUsual/Assets/'+n for n in ['Multiplayer/Scripts/NetworkSession.cs','Multiplayer/Scripts/NetworkShop.cs','Multiplayer/Scripts/NetworkTestDriver.cs','RescueV2/Scripts/RescueInterface.cs','RescueV2/Scripts/RescueWorld.cs','Town/Scripts/LivingWorkday.cs','Town/Scripts/WorkdayDomain.cs']};check('protected_baseline_and_staged_index_intact',p['stageUnchanged'] and p['indexBytesUnchanged'] and set(p['changedBaselineFiles'])<=allowed)
for link in re.findall(r'(?:href|src)="([^"]+)"',(d/'review.html').read_text(encoding='utf-8')):
 check('gallery_link_'+link,(d/link).resolve().is_file())
docs=['phase8b-implementation-plan.md','phase8b-implementation-report.md','phase8b-validation-report.md','phase8b-playtest.md','phase8b-future-roadmap.md','phase8b-remaining-issues.md'];check('all_requested_reports_delivered',all((r/'Docs/Design'/f).is_file() for f in docs))
for f in ['StaffKiosk.blend','ReturnsCabinet.blend','geometry-audit.json','provenance.json']:check('editable_asset_'+f,(d/'Assets'/f).is_file())
summary=dict(assemblySha256=sha,results=results,checks=checks,scope='Native technical evidence; no human fun/usability or cross-machine LAN claim.')
(d/'artifact-validation.json').write_text(json.dumps(summary,indent=2),encoding='utf-8');print(str(len(checks))+' artifact checks passed; latest assembly '+sha)
