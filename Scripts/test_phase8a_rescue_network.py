"""Two/four native players; shared custody, private profession records, replay and recovery."""
from pathlib import Path
import argparse,sys
p=argparse.ArgumentParser(add_help=False);p.add_argument('--count',type=int,choices=[2,4],required=True);option,rest=p.parse_known_args();sys.argv=[sys.argv[0]]+rest
source=Path(__file__).with_name('test_phase8a_network.py').read_text(encoding='utf-8-sig');exec(compile(source.split('\ntry:\n')[0],str(Path(__file__).with_name('test_phase8a_network.py')),'exec'))
EXE=ROOT/'Builds/Phase8A-RescueV2/BusinessAsUsual.exe';roles=['host']+['client'+str(i) for i in range(1,option.count)];serials={r:0 for r in roles};PORT='7796'
build=json.loads((ROOT/'Docs/Diagnostics/Phase8ARescueV2/build.json').read_text(encoding='utf-8-sig'));assert build['result']=='Succeeded'
assembly=EXE.parent/'BusinessAsUsual_Data/Managed/Assembly-CSharp.dll';(OUT/'run-metadata.json').write_text(json.dumps(dict(assemblySha256=hashlib.sha256(assembly.read_bytes()).hexdigest(),count=option.count,executable=str(EXE)),indent=2))
def launch(role):
 for suffix in ('.json','.control.json'):(OUT/(role+suffix)).unlink(missing_ok=True)
 processes[role]=subprocess.Popen([str(EXE),'-screen-fullscreen','0','-screen-width',str(args.width),'-screen-height',str(args.height),'-logFile',str(OUT/(role+'.log')),'--bau-role',role,'--bau-evidence',str(OUT/(role+'.json')),'--bau-control',str(OUT/(role+'.control.json')),'--bau-port',PORT,'--bau-rescue'],cwd=EXE.parent)
def at(role,name,pos,alt=False):place(role,pos=pos);use(role,name,alt=alt)
def photo(role,label):cmd(role,'capture',target=label);wait(lambda:(OUT/(role+'-'+label+'.png')).exists());save(label)
def show(role,screen,tab=0):t=cmd(role,'rescueui',target=screen,x=tab);wait(lambda:read(role)['serial']==t);time.sleep(.4)
try:
 for r in roles:launch(r);wait(lambda:read(r).get('connected'),60)
 wait(lambda:all(len(read(r).get('players',[]))==option.count for r in roles));synced();check('actual_'+str(option.count)+'_process_roster',True)
 for r,job in zip(roles,[1,2,3,6]):use(r,'job:'+str(job));time.sleep(.3);cmd(r,'key',target='Enter');time.sleep(.4);cmd(r,'key',target='Enter');time.sleep(.4)
 check('one_owner_camera_listener_input_and_viewmodel',all(read(r)[k]==1 for r in roles for k in ['cameras','listeners','inputs']) and all(read(r)['firstPerson']['viewmodels']==1 for r in roles))
 place('host',pos=(-3,.08,-1.2),yaw=90);place('client1',pos=(0,.08,-1.2));t=cmd('host','preview',target='remote-walk');wait(lambda:read('host')['serial']==t);cmd('client1','move',x=0,y=1,yaw=0,duration=2);time.sleep(2.4);check('remote_walk_uses_foot_planting',next(p for p in read('host')['players'] if p['owner']==read('client1')['localId'])['footPlants']>0);photo('host','remote-walk')
 at('client1','Hood',(6.4,.08,4.1));at('client1','Probes',(6.635,.08,4.58));at('host','Shipment',(-36,.08,9.2));at('host','Dispatch',(-36,.08,9.2),alt=True);use('host','Dispatch')
 carrier='client3' if option.count==4 else 'host';place(carrier,pos=(-36,.08,9.2));synced();rev=state()['revision'];accepted=read('host')['accepted'];cmd(carrier,'raw',target=target('Parcel'),sequence=100000,revision=rev);wait(lambda:item('PART-8A-001')['holder']==read(carrier)['localId']);synced();rejected=read('host')['rejected'];cmd(carrier,'raw',target=target('Parcel'),sequence=100000,revision=rev);wait(lambda:read('host')['rejected']>rejected);check('replayed_transfer_changes_custody_once',read('host')['accepted']==accepted+1 and conserved())
 if option.count==4:
  cmd(carrier,'disconnect');wait(lambda:item('PART-8A-001')['custody']=='dispatch' and len(read('host')['players'])==3);check('courier_disconnect_recovers_same_parcel',item('PACK-8A-001')['holder']==18446744073709551615)
  cmd(carrier,'reconnect');wait(lambda:read(carrier).get('connected') and len(read(carrier).get('players',[]))==4,60);synced();use(carrier,'job:6');show(carrier,'HUD');place(carrier,pos=(-36,.08,9.2));use(carrier,'Parcel')
 at(carrier,'Bay tray',(8.2,.08,6.3));check('all_clients_share_same_delivered_objects',all(next(i for i in read(r)['state']['workday']['items'] if i['id']=='PART-8A-001')['custody']=='garage' for r in roles))
 at('client1','Terminal',(6.68,.08,4.58),alt=True);use('client1','Terminal');use('client1','Terminal');at('client1','Lift battery',(6.635,.08,4.58));at('client1','Scrap tray',(5.3,.08,7));at('client1','Replacement',(8.2,.08,6.3));at('client1','Socket',(6.635,.08,4.58));at('client1','Repair test',(6.635,.08,4.58));at('client1','Hood',(6.4,.08,4.1));at('client1','Service ticket',(7.65,.08,6.3));check('shared_manual_repair_signed_and_conserved',wd()['serviceSigned'] and item('OLD-8A-001')['material']=='metal' and conserved())
 at('host','Stock box 0',(-36.4,.08,8.45));place('host','Stock shelf 0');use('host','Stock shelf 0');use('host','Shelf product 0');at('host','Order counter',(-41.7,.08,2.8));at('host','Order register',(-40.3,.08,2.8));check('shared_retail_scan_uses_same_stock',wd()['customerOrders']==1 and conserved())
 if option.count==4:
  waste='client2'
  for name,pos,chute in [('Square pickup',(-1,.08,7),'Paper chute'),('Paper pickup',(5.3,.08,7.45),'Paper chute'),('Metal pickup',(4.5,.08,7.8),'Metal chute')]:
   at(waste,name,pos);place(waste,'Mixed chute');use(waste,'Mixed chute',alt=True);place(waste,chute);use(waste,chute)
  place(waste,'Manifest');use(waste,'Manifest');check('network_collection_reuses_repair_packaging_and_scrap',sum(i['custody']=='recorded' for i in wd()['items'])==3 and conserved());at('host','Shift report',(0,.08,3.9));check('four_worker_shift_completed',wd()['shiftSigned'])
 for r in roles:
  job=1 if r=='host' else 2 if r=='client1' else 3
  check('private_record_scope_'+r,all(record['profession']==job for record in read(r)['workRecords']));check('shared_payload_omits_audit_and_private_records_'+r,read(r)['state']['workday']['records']==[] and 'actor' not in json.dumps(read(r)['state']['workday']));show(r,'Handbook',2);photo(r,'private-record-copy')
 check('records_differ_between_professions',read('host')['workRecords']!=read('client1')['workRecords']);check('one_local_canvas_per_worker',all(read(r)['rescueUI']['activeCanvases']==1 for r in roles));save('final-shared-state')
except Exception as e:checks.append(dict(name='exception',passed=False,error=str(e)));traceback.print_exc();save('failure')
finally:
 (OUT/'checks.json').write_text(json.dumps(checks,indent=2))
 for r,p in processes.items():
  if p.poll() is None:
   try:cmd(r,'quit');p.wait(timeout=6)
   except Exception:p.terminate()
 print(f'{sum(c["passed"] for c in checks)}/{len(checks)} checks passed',flush=True)
 if any(not c['passed'] for c in checks):raise SystemExit(1)
