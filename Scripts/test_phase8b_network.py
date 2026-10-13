"""Real localhost players. Fixture teleports, authenticated production RPCs, private transport and paper boundaries."""
from pathlib import Path
import argparse,sys,math
p=argparse.ArgumentParser(add_help=False);p.add_argument('--players',type=int,choices=[2,4],required=True);opt,rest=p.parse_known_args();sys.argv=[sys.argv[0]]+rest
base=Path(__file__).with_name('test_phase8a_network.py');exec(compile(base.read_text(encoding='utf-8-sig').split('\ntry:\n')[0],str(base),'exec'))
EXE=ROOT/'Builds/Phase8B/BusinessAsUsual.exe';roles=roles[:opt.players]
(OUT/'run-metadata.json').write_text(json.dumps(dict(players=opt.players,assemblySha256=hashlib.sha256((EXE.parent/'BusinessAsUsual_Data/Managed/Assembly-CSharp.dll').read_bytes()).hexdigest(),method='Production NGO commands; local fixture placement; no human usability claims'),indent=2))
def token(r,action,**fields):
 t=cmd(r,action,**fields);wait(lambda:read(r).get('serial')==t);time.sleep(.4)
def use(r,name,alt=False):
 synced();before=state()['revision'];cmd(r,'interact',target=('alt:' if alt else '')+(name if name.startswith(('job:','day:')) or name=='return' else target(name)));wait(lambda:state()['revision']>before);synced()
def act(r,name,alt=False):place(r,name);use(r,name,alt)
def shot(r,name):token(r,'capture',target=name);wait(lambda:(OUT/(r+'-'+name+'.png')).exists())
def click(r,label):
 row=next(t for t in read(r)['rescueUI']['text'] if t['value']==label);token(r,'click',x=row['at']['x']+row['size']['x']/2,y=row['at']['y']+row['size']['y']/2)
def enter(r):
 wait(lambda:read(r).get('rescueUI',{}).get('screen')=='Main',60);click(r,'HOST GAME' if r=='host' else 'JOIN GAME')
 if r!='host':wait(lambda:read(r)['rescueUI']['screen']=='Join');click(r,'JOIN GAME')
 wait(lambda:read(r).get('connected'),60)
def denied(r,action,**fields):
 before=read('host')['rejected'];token(r,action,**fields);wait(lambda:read('host')['rejected']>before)
try:
 for r in roles:
  for suffix in ('.json','.control.json'):(OUT/(r+suffix)).unlink(missing_ok=True)
  latest.pop(r,None)
  processes[r]=subprocess.Popen([str(EXE),'-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-logFile',str(OUT/(r+'.log')),'--bau-role',r,'--bau-evidence',str(OUT/(r+'.json')),'--bau-control',str(OUT/(r+'.control.json')),'--bau-port','7788','--bau-phase8b','--bau-rescue','--bau-ui-only','--bau-seed','81','--bau-workday-seconds','600'],cwd=EXE.parent)
  enter(r)
 wait(lambda:all(len(read(r).get('players',[]))==opt.players for r in roles),45)
 check('actual_'+str(opt.players)+'_connected_processes',all(p.poll() is None for p in processes.values()))
 check('one_camera_listener_input_and_native_canvas_per_worker',all(read(r)[k]==1 for r in roles for k in ['cameras','listeners','inputs']) and all(read(r)['rescueUI']['activeCanvases']==1 for r in roles))
 for r,job in zip(roles,[1,2,3,4]):
  use(r,'job:'+str(job));wait(lambda:read(r)['rescueUI']['screen']=='Intro');token(r,'key',target='Enter');token(r,'key',target='Enter');wait(lambda:read(r)['rescueUI']['screen']=='Hub')
 shot('host','staff-hub');save('ready-hub');check('day_waits_for_every_connected_worker',not state()['day']['started'] and len(state()['day']['ready'])==opt.players)
 use('host','day:start');wait(lambda:all(read(r)['rescueUI']['screen']=='Private' for r in roles));time.sleep(.7)
 arrangement=[r for r in roles if read(r)['dayPrivate']['hasArrangement']];check('exactly_one_private_arrangement_independent_of_public_job',len(arrangement)==1);owner=arrangement[0]
 for r in roles:shot(r,'private-work-note');token(r,'key',target='Enter')
 check('public_snapshot_has_no_secret_assignment_or_seed',all(all(k not in json.dumps(read(r)['state']) for k in ['arrangementOwner','arrangementNote','hasArrangement','personalNote','seed']) for r in roles))
 check('other_workers_have_real_personal_secrets',all(read(r)['dayPrivate']['personalTitle'] and not read(r)['dayPrivate']['practice'] for r in roles))
 denied('client1','raw',target='day:share:0',sequence=0,revision=state()['revision']);check('replayed_day_command_is_rejected',not state()['day']['papers'])
 denied('client1','raw',target=target('Returns counter'),sequence=1000,revision=state()['revision']-1);check('stale_day_command_cannot_move_material',item('PART-8A-001')['custody']=='shipment' and conserved())
 denied('client1','interact',target=target('Hood'));check('remote_work_request_requires_real_range_and_line_of_sight',not wd()['hood'])
 act('client1','Hood');act('client1','Probes');act('host','Shipment');act('host','Dispatch');act(owner,'Parcel')
 # Remote workers cannot receive an unseen exact parcel location through their state snapshot.
 for r in roles:
  if r!=owner:place(r,pos=(10,.08,-3))
 time.sleep(.8);check('remote_clients_receive_unobserved_custody',all(next(i for i in read(r)['state']['workday']['items'] if i['id']=='PART-8A-001')['custody']=='unobserved' for r in roles[1:] if r!=owner))
 act(owner,'Returns counter');check('ordinary_hold_moves_the_same_sealed_pair',item('PART-8A-001')['custody']=='returns' and item('PACK-8A-001')['custody']=='returns' and conserved());shot(owner,'ordinary-hold')
 act(owner,'Returns counter',True);act(owner,'Returns counter',True);act(owner,'Returns counter');act(owner,'Bay tray');check('route_declaration_is_independent_of_the_real_detour',wd()['parcelNote']=='Direct transfer' and read(owner)['dayPrivate']['detour'] and read(owner)['dayPrivate']['declaredDirect'])
 act('client1','Probes',True);act('client1','Service amendment')
 if opt.players==4:act('client3','Bay tray',True)
 pos=(6.68,.08,4.58);place('client1',pos=pos);t=read('client1')['targetPoints'][read('client1')['targetIds'].index(target('Terminal'))];dx,dz=t['x']-pos[0],t['z']-pos[2];token('client1','look',x=math.degrees(math.atan2(dx,dz)),y=math.degrees(math.atan2(pos[1]+1-t['y'],math.hypot(dx,dz))));save('before-client-torque');check('visible_received_carton_exposes_its_coarse_intake_state',next(i for i in read('client1')['state']['workday']['items'] if i['id']=='PART-8A-001')['custody']=='garage');turn('client1');wait(lambda:wd()['released']);check('real_client_mouse_turn_releases_the_clamp',wd()['hood']);act('client1','Lift battery');act('client1','Scrap tray');act('client1','Replacement');act('client1','Socket');act('client1','Repair test');act('client1','Hood');act('client1','Service ticket')
 # One actual order is enough for the shared shift; material identity is conserved.
 act('host','Stock box 0');act('host','Stock shelf 0');act('host','Shelf product 0');act('host','Order counter');act('host','Order register')
 if opt.players==4:
  for point in ['Square pickup','Paper pickup','Metal pickup']:
   act('client2',point)
   if point=='Paper pickup':act('client2','Returns counter');act('client2','Returns counter')
   act('client2','Collection cart')
  for index in [3,1,2]:
   act('client2','Cart batch '+str(index));act('client2','Mixed chute',True);material=wd()['items'][index]['material'];act('client2',('Paper' if material=='paper' else 'Metal' if material=='metal' else 'Mixed')+' chute')
  act('client2','Manifest')
 else:wait(lambda:sum(i['custody']=='recorded' for i in wd()['items'])==3,60)
 check('shared_work_chain_completes_without_desync',wd()['serviceSigned'] and wd()['customerOrders']==1 and sum(i['custody']=='recorded' for i in wd()['items'])==3 and conserved())
 check('profession_paper_copies_are_targeted',all(all(p['profession']==job for p in read(r)['workRecords']) for r,job in zip(roles,[1,2,3,1])))
 check('public_work_state_has_no_records_or_server_audit',all(not read(r)['state']['workday']['records'] and read(r).get('serverAuditCount',0)==0 for r in roles[1:]))
 shop_index=max(i for i,p in enumerate(read('host')['workRecords']) if p['heading']=='Declared transfer');mechanic_index=next(i for i,p in enumerate(read('client1')['workRecords']) if p['heading']=='Service amendment');use('host','day:share:'+str(shop_index));use('client1','day:share:'+str(mechanic_index));check('voluntary_board_contains_only_two_selected_papers',len(state()['day']['papers'])==2)
 act('host','Staff board');wait(lambda:read('host')['rescueUI']['screen']=='Board');check('physical_noticeboard_opens_during_work',not wd()['closed']);shot('host','two-selected-copies');token('host','rescueui',target='HUD');save('completed-chain')
 check('covert_owner_also_completed_useful_work',read(owner)['dayPrivate']['productive'])
 # A live application reconnect keeps its identity and private progress while authenticating a new client id.
 r=roles[-1];previous=read(r)['dayPrivate'];oldid=read(r)['localId'];cmd(r,'disconnect');wait(lambda:len(read('host')['players'])==opt.players-1);enter(r);wait(lambda:len(read('host')['players'])==opt.players,35);use(r,'job:'+str([1,2,3,4][roles.index(r)]));time.sleep(.7);token(r,'key',target='Enter')
 check('live_rejoin_preserves_owner_only_progress',read(r)['localId']!=oldid and read(r)['dayPrivate']==previous);save('rejoined')
 token(r,'key',target='J');wait(lambda:read(r)['rescueUI']['screen']=='Staff' and any(t['value']=='Your shift crew' for t in read(r)['rescueUI']['text']));token(r,'key',target='J');wait(lambda:read(r)['rescueUI']['screen']=='HUD');check('rejoined_worker_can_reopen_staff_and_return_to_work',True)
 act('host','Shift report');wait(lambda:all(read(r)['rescueUI']['screen']=='Report' for r in roles));shot('host','shared-shift-summary');check('same_shift_report_reaches_all_connected_players',all(read(r)['state']['workday']['closed'] and read(r)['state']['day']['closed'] for r in roles));save('completed-day')
except Exception as e:checks.append(dict(name='exception',passed=False,error=str(e)));traceback.print_exc();save('failure')
finally:
 (OUT/'checks.json').write_text(json.dumps(checks,indent=2))
 for r,p in processes.items():
  if p.poll() is None:
   try:cmd(r,'quit');p.wait(timeout=6)
   except Exception:p.terminate()
 print(f'{sum(c["passed"] for c in checks)}/{len(checks)} checks passed',flush=True)
 if any(not c['passed'] for c in checks):raise SystemExit(1)
