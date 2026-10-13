"""Four actual Windows players, production NGO RPCs and revision guards.
Harness teleports only arrange fixtures; actions use the production command boundary.
"""
from pathlib import Path
import argparse, json, subprocess, time, traceback,hashlib

ROOT=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser();parser.add_argument('--output',type=Path,default=ROOT/'Docs/Diagnostics/Phase8A/FourProcess');parser.add_argument('--scenario',choices=['workday','recovery'],default='workday');parser.add_argument('--assistant-profession',type=int,choices=[1,2,3],default=1);parser.add_argument('--width',type=int,default=1280);parser.add_argument('--height',type=int,default=720)
args=parser.parse_args();OUT=args.output.resolve();OUT.mkdir(parents=True,exist_ok=True)
EXE=ROOT/'Builds/Phase8A-Usability/BusinessAsUsual.exe';roles=['host','client1','client2','client3'];processes={};serials={r:0 for r in roles};latest={};checks=[]
assembly=EXE.parent/'BusinessAsUsual_Data/Managed/Assembly-CSharp.dll'
if assembly.exists():(OUT/'run-metadata.json').write_text(json.dumps(dict(assemblySha256=hashlib.sha256(assembly.read_bytes()).hexdigest(),scenario=args.scenario,assistantProfession=args.assistant_profession,width=args.width,height=args.height),indent=2))
def read(role):
    try:latest[role]=json.loads((OUT/(role+'.json')).read_text(encoding='utf-8-sig'))
    except (OSError,ValueError):pass
    return latest.get(role,{})
def wait(fn,timeout=30):
    until=time.monotonic()+timeout
    while time.monotonic()<until:
        try:
            value=fn()
            if value:return value
        except (KeyError,IndexError,TypeError,StopIteration):pass
        time.sleep(.09)
    raise AssertionError('Timeout; '+str({r:read(r).get('feedback') for r in roles}))
def check(name,value):
    checks.append(dict(name=name,passed=bool(value)));print(('PASS ' if value else 'FAIL ')+name,flush=True)
    if not value:raise AssertionError(name)
def cmd(role,action,**fields):
    serials[role]+=1;p=OUT/(role+'.control.json');tmp=p.with_suffix('.tmp');tmp.write_text(json.dumps(dict(serial=serials[role],action=action,**fields)))
    for _ in range(80):
        try:tmp.replace(p);break
        except PermissionError:time.sleep(.01)
    return serials[role]
def state():return read('host')['state']
def wd():return state()['workday']
def item(name):return next(i for i in wd()['items'] if i['id']==name)
def synced():
    wait(lambda:all(read(r).get('state',{}).get('revision')==state()['revision'] for r in roles if read(r).get('connected')));time.sleep(.16)
def target(name):return 'Living Workday/'+name
def place(role,name=None,pos=None,yaw=0):
    if pos is None:
        p=read('host')['targetPoints'][read('host')['targetIds'].index(target(name))];pos=(p['x'],.08,p['z']+(1.0 if name.endswith('chute') else -1.0));yaw=180 if name.endswith('chute') else yaw
    token=cmd(role,'teleport',x=pos[0],y=pos[1],z=pos[2],yaw=yaw);wait(lambda:read(role).get('serial')==token);time.sleep(.35)
def use(role,name,alt=False,local=False):
    synced();before=state()['revision'];cmd(role,'e' if local else 'interact',target=('alt:' if alt else '')+(name if name.startswith('job:') or name in ('return','drop') else target(name)))
    wait(lambda:state()['revision']>before);synced()
def reject(role,name,alt=False,**fields):
    synced();before=read('host')['rejected'];cmd(role,'interact',target=('alt:' if alt else '')+(name if name.startswith('job:') else target(name)),**fields);wait(lambda:read('host')['rejected']>before);synced()
def turn(role):
    synced();before=state()['revision'];cmd(role,'turn');wait(lambda:state()['revision']>before);synced();time.sleep(.8)
def capture(role,name,page=None):
    token=cmd(role,'panel',run=page is not None,x=page or 0);wait(lambda:read(role).get('serial')==token);time.sleep(.4)
    path=OUT/(role+'-'+name+'.png');path.unlink(missing_ok=True);cmd(role,'capture',target=name);wait(lambda:path.exists());time.sleep(.2)
def launch(role):
    for suffix in ('.json','.control.json'):(OUT/(role+suffix)).unlink(missing_ok=True)
    processes[role]=subprocess.Popen([str(EXE),'-screen-fullscreen','0','-screen-width',str(args.width),'-screen-height',str(args.height),'-logFile',str(OUT/(role+'.log')),'--bau-role',role,'--bau-evidence',str(OUT/(role+'.json')),'--bau-control',str(OUT/(role+'.control.json')),'--bau-port','7788','--bau-workday'],cwd=EXE.parent)
def save(name):
    for r in roles:(OUT/(name+'-'+r+'.json')).write_text(json.dumps(read(r),indent=2))
def conserved():return len(wd()['items'])==7 and len({i['id'] for i in wd()['items']})==7 and all(i['custody'] for i in wd()['items'])
try:
    for r in roles:launch(r);wait(lambda:read(r).get('connected'),60)
    wait(lambda:all(len(read(r).get('players',[]))==4 for r in roles),60);synced()
    check('four_actual_processes_connected',all(p.poll() is None for p in processes.values()))
    wait(lambda:all('4 players' in read(r)['status'] for r in roles));check('four_player_roster_UI_updates_after_other_clients_join',all('4 players' in read(r)['status'] for r in roles))
    check('one_owned_camera_listener_input_hud_each',all(read(r)[k]==1 for r in roles for k in ['cameras','listeners','inputs','townHuds']))
    capture('host','staff-desk')
    token=cmd('host','key',target='Digit1');wait(lambda:state()['town']['owners'][0]==read('host')['localId']);synced()
    check('staff_desk_keyboard_assignment',state()['town']['owners'][0]==read('host')['localId'])
    for r,job in zip(roles[1:],[2,3,args.assistant_profession+3]):use(r,'job:'+str(job))
    for r in roles:
        time.sleep(.3);cmd(r,'key',target='Enter');time.sleep(.3);cmd(r,'key',target='Enter');time.sleep(.3)
    check('three_leads_and_shared_assistant',state()['town']['owners'][:4]==[read(r)['localId'] for r in roles] and state()['town']['assistantProfession']==args.assistant_profession)
    reject('client3','job:2');check('assistant_cannot_steal_lead',state()['town']['owners'][1]==read('client1')['localId'])
    for r,station in zip(roles,['Market reset','Garage reset','Yard reset','Market reset']):place(r,station);use(r,station)
    check('all_four_have_independent_fallback_work',wd()['cleaned']==4)
    place('client1','Request');use('client1','Request');place('client1','Hood');cmd('client1','preview',target='hood');time.sleep(.2);use('client1','Hood');place('client1','Probes');use('client1','Probes')
    if args.scenario=='recovery':
        before=wd()['items'];place('client1','Request');reject('client1','Request',alt=True)
        wait(lambda:read('host')['firstPerson']['townClock']>=wd()['requestedAt']+25,35)
        use('client1','Request',alt=True);check('staffed_unresponsive_market_fallback_same_ids',item('PART-8A-001')['custody']=='garage' and conserved())
        checker='client3' if args.assistant_profession==2 else 'client1'
        place(checker,'Probes');use(checker,'Probes',alt=True)
        if checker=='client3':check('assistant_not_awarded_half_finished_load_check',not wd()['loadChecked'] and read(checker)['workSupport'][0]==0)
        use(checker,'Probes',alt=True)
        check('mechanic_assistant_checks_without_duplicating_installation',wd()['loadChecked'])
        if checker=='client3':check('assistant_earns_own_completed_intake',read(checker)['workSupport'][0]==1)
    else:
        packer='client3' if args.assistant_profession==1 else 'host'
        place(packer,'Shipment');use(packer,'Shipment');reject('client1','Shipment')
        place(packer,'Dispatch');use(packer,'Dispatch',alt=True);use(packer,'Dispatch')
        check('shipment_verification_and_preparation',wd()['verified'] and wd()['prepared'])
        # Two exact identical raw commands must not create another transfer or record.
        place('client3','Parcel');synced();revision=state()['revision'];before=read('host')['accepted']
        cmd('client3','raw',target=target('Parcel'),sequence=500000,revision=revision);wait(lambda:item('PART-8A-001')['holder']==read('client3')['localId']);synced()
        rejected=read('host')['rejected'];cmd('client3','raw',target=target('Parcel'),sequence=500000,revision=revision);wait(lambda:read('host')['rejected']>rejected)
        check('duplicate_sequence_one_custody_change',read('host')['accepted']==before+1 and conserved())
        capture('client3','carried-parcel')
        cmd('client3','disconnect');wait(lambda:len(read('host')['players'])==3 and item('PART-8A-001')['custody']=='dispatch')
        check('disconnect_recovers_parcel_and_assistant_slot',state()['town']['owners'][3]==18446744073709551615 and item('PACK-8A-001')['holder']==18446744073709551615)
        cmd('client3','reconnect');wait(lambda:read('client3').get('connected') and len(read('client3').get('players',[]))==4,60);synced();use('client3','job:'+str(args.assistant_profession+3))
        check('rejoin_restores_shared_workday',wd()['verified'] and wd()['prepared'] and conserved())
        cmd('client3','key',target='Enter');time.sleep(.3);cmd('client3','key',target='Enter');time.sleep(.3)
        place('client3','Parcel');use('client3','Parcel');place('client3','Bay tray');place('client2','Bay tray',pos=(5.05,.08,6.4))
        use('client2','Bay tray',alt=True);use('client3','Bay tray');check('optional_gate_assist_saves_solo_open_step',wd()['assisted']==1 and item('PART-8A-001')['custody']=='garage')
    place('client1','Terminal');use('client1','Terminal',alt=True);turn('client1');turn('client1');check('first_person_mouse_drag_turns_two_terminals',wd()['bolts']==2);place('client1','Socket');use('client1','Socket')
    check('manual_bench_repair_releases_real_packaging_and_scrap',wd()['installed'] and item('OLD-8A-001')['material']=='metal' and item('PACK-8A-001')['custody']=='garage-waste' and conserved())
    capture('client1','service-slip',2);capture('client1','repair-view')
    place('host','Counter');use('host','Counter');check('customer_order_consumes_conserved_counter_item',item('ORDER-8A-001')['custody']=='customer')
    for station in ['Square pickup','Metal pickup','Paper pickup']:
        if station=='Square pickup' and args.assistant_profession==3:
            place('client3',station);use('client3',station);place('client3','Manifest');use('client3','Manifest');place('client2','Yard intake');use('client2','Yard intake');check('sanitation_assistant_transports_to_lead_without_duplicate_progression',item('BAG-8A-001')['holder']==read('client2')['localId'])
        else:place('client2',station);use('client2',station)
        held=next(i for i in wd()['items'] if i['holder']==read('client2')['localId'])
        place('client2','Mixed chute');reject('client2','Mixed chute');check('wrong_sort_retains_'+held['id'],item(held['id'])['holder']==read('client2')['localId'])
        place('client2','Paper chute' if held['material']=='paper' else 'Metal chute');use('client2','Paper chute' if held['material']=='paper' else 'Metal chute',alt=True)
        capture('client2','batch-'+held['id'])
        use('client2','Paper chute' if held['material']=='paper' else 'Metal chute');place('client2','Manifest');use('client2','Manifest')
    capture('client2','sanitation-manifests',2);capture('host','market-receipts',2)
    check('three_connected_waste_batches_recorded',sum(i['custody']=='recorded' for i in wd()['items'])==3 and conserved())
    check('public_snapshots_omit_profession_records_and_server_audit',all(not read(r)['state']['workday']['records'] and 'Audit' not in json.dumps(read(r)['state']['workday']) for r in roles))
    check('targeted_paper_records_match_profession',all(all(record['profession']==job for record in read(role).get('workRecords',[])) for role,job in zip(roles,[1,2,3,args.assistant_profession])))
    check('client_has_no_server_audit',read('host').get('serverAuditCount',0)>0 and all(read(r).get('serverAuditCount',0)==0 for r in roles[1:]))
    # Real movement through the existing market doorway, without the old second E.
    place('host',pos=(-8.2,.08,2.6));door=next(t for t in read('host')['targetIds'] if t.endswith('/Shop Door'))
    if not all(state()['doorOpen']):cmd('host','interact',target=door);wait(lambda:all(state()['doorOpen']))
    cmd('host','move',x=0,y=1,yaw=0,duration=1.1);wait(lambda:next(p['position']['x'] for p in read('host')['players'] if p['owner']==read('host')['localId']) < -25)
    check('walk_through_market_threshold_no_second_E',len(read('host')['players'])==4 and conserved());capture('host','market-entry')
    place('host','Shift report');use('host','Shift report');check('workday_closes_with_conserved_history',wd()['closed'] and wd()['stability']>5 and conserved());capture('host','shift-report',3);save('completed')
except Exception as error:
    checks.append(dict(name='exception',passed=False,error=str(error)));traceback.print_exc();save('failure')
finally:
    (OUT/'checks.json').write_text(json.dumps(checks,indent=2))
    for r,p in processes.items():
        if p.poll() is None:
            try:cmd(r,'quit');p.wait(timeout=6)
            except Exception:p.terminate()
    print(f'{sum(c["passed"] for c in checks)}/{len(checks)} checks passed',flush=True)
    if any(not c['passed'] for c in checks):raise SystemExit(1)
