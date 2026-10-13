"""Two independent Windows processes using production command dispatch and actual NGO/UTP."""
from pathlib import Path
import argparse,json,subprocess,time,traceback,sys,math
ROOT=Path(__file__).resolve().parents[1]
args=argparse.ArgumentParser();args.add_argument('--output',type=Path,default=ROOT/'Docs/Diagnostics/Phase6/TwoProcess');args.add_argument('--scenario',choices=('full','staffed','entry'),default='full');args.add_argument('--exe',type=Path,default=None);args.add_argument('--width',type=int,default=1280);args.add_argument('--height',type=int,default=720);options=args.parse_args()
OUT=options.output.resolve();OUT.mkdir(parents=True,exist_ok=True)
EXE=(options.exe or ROOT/'Builds/Phase6/BusinessAsUsual.exe').resolve()  # --exe runs this suite against a later build
checks=[];processes={};serials={'host':0,'client':0};last={};NOBODY=18446744073709551615
def read(role):
    for _ in range(40):
        try:last[role]=json.loads((OUT/(role+'.json')).read_text());return last[role]
        except (OSError,ValueError):time.sleep(.01)
    return last.get(role,{})
def wait(predicate,seconds=30):
    end=time.monotonic()+seconds
    while time.monotonic()<end:
        try:
            value=predicate()
            if value:return value
        except (KeyError,IndexError,TypeError):pass
        time.sleep(.08)
    raise AssertionError('Timed out waiting for network state')
def check(name,value):
    checks.append(dict(name=name,passed=bool(value)));print(('PASS ' if value else 'FAIL ')+name,flush=True)
    if not value:raise AssertionError(name)
def command(role,action,**kw):
    serials[role]+=1;p=OUT/(role+'.control.json');tmp=p.with_suffix('.tmp');tmp.write_text(json.dumps(dict(serial=serials[role],action=action,**kw)))
    for i in range(100):
        try:tmp.replace(p);break
        except PermissionError:time.sleep(.01)
    return serials[role]
def launch(role):
    for suffix in ('json','control.json'):(OUT/(role+'.'+suffix)).unlink(missing_ok=True)
    processes[role]=subprocess.Popen([str(EXE),'-screen-fullscreen','0','-screen-width',str(options.width),'-screen-height',str(options.height),'-logFile',str(OUT/(role+'.log')),'--bau-role',role,'--bau-evidence',str(OUT/(role+'.json')),'--bau-control',str(OUT/(role+'.control.json')),'--bau-port','7786'],cwd=EXE.parent)
def state():return read('host')['state']
def town():return state()['town']
def synced():
    wait(lambda:read('host')['state']['revision']==read('client')['state']['revision']);time.sleep(.18)
def equal():
    def same(a,b):
        if isinstance(a,float) and isinstance(b,(float,int)):return math.isclose(a,b,rel_tol=1e-9,abs_tol=1e-6)
        if isinstance(a,dict):return a.keys()==b.keys() and all(same(v,b[k]) for k,v in a.items())
        if isinstance(a,list):return len(a)==len(b) and all(same(x,y) for x,y in zip(a,b))
        return a==b
    a=state();b=read('client')['state'];return all(same(a[k],b[k]) for k in ('revision','received','closed','orderIndex','holder','quantities','progress','receipts','town'))
def target(part):return next(s for s in read('host')['targetIds'] if s.endswith('/'+part) or s==part)
def place(role,part=None,pos=None):
    if part=='Parts Pickup Locker' and pos is None:pos=(-11.5,.08,2.7)
    if pos is None:
        p=read('host')['targetPoints'][read('host')['targetIds'].index(target(part))];pos=(p['x'],.08,p['z']-1.1)
    token=command(role,'teleport',x=pos[0],y=pos[1],z=pos[2]);wait(lambda:read(role).get('serial')==token);time.sleep(.4)
def use(role,part,local=False):
    synced();rev=state()['revision'];command(role,'e' if local else 'interact',target=target(part) if not part.startswith('job:') and part not in ('return','drop') else part)
    wait(lambda:state()['revision']>rev);synced()
def reject(role,part,**kwargs):
    synced();before=read('host')['rejected'];command(role,'interact',target=target(part) if not part.startswith('job:') else part,**kwargs);wait(lambda:read('host')['rejected']>before);synced()
def hold(role,value):
    token=command(role,'hold',run=value);wait(lambda:read(role).get('serial')==token);time.sleep(.15)
def work(role,part,local=True):
    place(role,part);hold(role,True);use(role,part,local);wait(lambda:all(w['worker']!=read(role)['localId'] for w in town()['works']));hold(role,False);synced()
def capture(role,name,page=None):
    token=command(role,'panel',run=page is not None,x=page or 0);wait(lambda:read(role).get('serial')==token);time.sleep(.4);p=OUT/(role+'-'+name+'.png');p.unlink(missing_ok=True);began=time.time();command(role,'capture',target=name);wait(lambda:p.exists() and p.stat().st_mtime>=began);time.sleep(.25)
def save(name):
    for role in ('host','client'):(OUT/(name+'-'+role+'.json')).write_text(json.dumps(read(role),indent=2))
def conserved():return all(sum(town()['quantities'][i::2])==2 for i in range(2)) and all(sum(state()['quantities'][i::3])==6 for i in range(3))
def navigation():
    # Real motor input through compact streets/entrances; both players walk every segment.
    paths=[('street',(-3,.08,0),(1,0),90,1.7),('garage',(7.8,.08,1.8),(0,1),0,1.4),('yard',(8.7,.08,-2.8),(0,1),180,.65),('alley',(-3,.08,11),(1,0),90,1.7),('square',(-1.8,.08,3),(0,1),0,.7)]
    for name,start,axes,yaw,duration in paths:
        for role in ('host','client'):
            other='client' if role=='host' else 'host';place(other,pos=(-13,.08,-3));place(role,pos=start)
            command(role,'panel',run=False);command(role,'camera',x=yaw,y=16,z=3.5);time.sleep(.3)
            before=next(p['position'] for p in read('host')['players'] if p['owner']==read(role)['localId']);command(role,'move',x=0,y=1,yaw=yaw,duration=duration,run=False);time.sleep(duration+.5)
            after=next(p for p in read('host')['players'] if p['owner']==read(role)['localId']);distance=math.hypot(after['position']['x']-before['x'],after['position']['z']-before['z'])
            check(role+'_walk_'+name,distance>duration*1.45 and after['grounded'] and after['position']['y']>-.2)
            capture(role,'walk-'+name)
    samples=[]
    for _ in range(30):samples.append({r:read(r).get('frameMilliseconds') for r in ('host','client')});time.sleep(.16)
    (OUT/'frame-samples.json').write_text(json.dumps(samples,indent=2))
def staffed():
    launch('host');wait(lambda:read('host').get('connected'),60);launch('client');wait(lambda:read('client').get('connected') and len(read('client').get('players',[]))==2,60);synced()
    use('host','job:1');use('client','job:2');check('fresh_session_resets_all_jobs_and_records',not any(town()['started']) and not town()['serviceRecords'] and town()['quantities']==[2,2,0,0,0,0,0,0])
    place('client','Service Desk');use('client','Service Desk',True);reject('client','job:3');check('cannot_abandon_active_shift',town()['owners'][1]==read('client')['localId'])
    place('client','Vehicle 0 Inspection');hold('client',True);use('client','Vehicle 0 Inspection',True);command('client','move',x=0,y=-1,yaw=0,duration=.5);time.sleep(.7);hold('client',False);wait(lambda:town()['works'][0]['worker']==NOBODY);check('moving_cancels_timed_work',not town()['services'][0]['inspected'])
    work('client','Vehicle 0 Inspection');work('client','Vehicle 1 Inspection');place('client','Parts Request Slip');use('client','Parts Request Slip',True)
    place('client','Parts Pickup Locker');reject('client','Parts Pickup Locker');check('occupied_shop_requires_actual_packing',town()['quantities']==[2,2,0,0,0,0,0,0])
    place('host',pos=(-40,.08,1))
    place('host','Parts Packing Counter');use('host','Parts Packing Counter',True);check('shopkeeper_packs_real_parts_for_colleague',town()['quantities']==[1,1,1,1,0,0,0,0] and conserved())
    reject('host','Parts Packing Counter');check('duplicate_packing_cannot_mint_parts',town()['quantities']==[1,1,1,1,0,0,0,0])
    place('client','Parts Pickup Locker');use('client','Parts Pickup Locker',True);check('client_collects_host_packed_stock',town()['quantities']==[1,1,0,0,1,1,0,0] and equal())
    capture('client','mechanic-inventory',1);work('client','Vehicle 0 Repair');work('client','Vehicle 1 Repair');place('client','Vehicle 0 Repair');reject('client','Vehicle 0 Repair');check('replayed_repair_cannot_consume_again',town()['quantities'][6:8]==[1,1] and conserved())
    place('client','Service Desk');use('client','Service Desk',True);records=json.loads(json.dumps(town()['serviceRecords']));use('client','Service Desk',True);reject('client','Service Desk');check('closed_service_records_immutable',town()['serviceRecords']==records and len(records)==2)
    capture('client','mechanic-records',2);capture('client','mechanic-report',3)
    use('client','job:3');place('client','Collection Route Desk');use('client','Collection Route Desk',True);place('client','Route Bag 0');use('client','Route Bag 0',True);place('client','Metal Sorting Bin');reject('client','Metal Sorting Bin');check('wrong_material_bin_rejected_without_bag_loss',town()['bags'][0]['holder']==read('client')['localId'] and town()['bags'][0]['stage']==0)
    use('client','return');capture('client','sanitation-inventory',1);save('staffed-completed');navigation()
def entry():
    launch('host');wait(lambda:read('host').get('connected'),60);launch('client');wait(lambda:read('client').get('connected') and len(read('client').get('players',[]))==2,60);synced()
    use('host','job:1');use('client','job:2');place('host',pos=(-7.8,.08,2.6))
    use('host','Shop Door',True);check('market_door_opens_with_production_E',all(state()['doorOpen']))
    command('host','move',x=0,y=1,yaw=0,duration=.65,run=False);time.sleep(1.1)
    check('player_walks_through_market_door',next(p['position']['z'] for p in read('host')['players'] if p['owner']==read('host')['localId'])>3.8)
    capture('host','market-doorway');use('host','Enter Daylight Market',True)
    wait(lambda:next(p['position']['x'] for p in read('host')['players'] if p['owner']==read('host')['localId']) < -25)
    check('production_E_enters_connected_interior',len(read('host')['players'])==2 and equal());capture('host','market-interior')
    place('host',pos=(-40,.08,1));use('host','Town Exit',True)
    check('entry_exit_preserves_both_assignments',town()['owners'][:2]==[read('host')['localId'],read('client')['localId']] and len(read('host')['players'])==2 and equal())

try:
    if options.scenario=='entry':entry()
    elif options.scenario=='staffed':staffed()
    else:
        launch('host');wait(lambda:read('host').get('connected'),60);launch('client');wait(lambda:read('client').get('connected') and len(read('client').get('players',[]))==2,60);synced()
        check('two_owned_players_one_input_camera_listener',all(read(r)[k]==1 for r in ('host','client') for k in ('cameras','listeners','inputs','townHuds')))
        check('town_excludes_black_debug_fixtures',not any('Test Fixture' in t for t in read('host')['targetIds']))
        capture('host','job-selection')
        use('host','job:2');use('client','job:3');check('different_jobs_synchronize',town()['owners'][1]==0 and town()['owners'][2]==read('client')['localId'] and equal())
        reject('client','job:2');check('cannot_steal_colleague_assignment',town()['owners'][1]==0 and equal())
        reject('host','job:6');check('unimplemented_professions_rejected',sum(n!=NOBODY for n in town()['owners'])==2)
        place('client','Service Desk');reject('client','Service Desk');check('private_job_tasks_require_assignment',not town()['started'][1])
        check('other_profession_station_has_no_E_prompt','Service Desk' not in read('client')['currentTarget'])
        place('host','Service Desk');use('host','Service Desk',local=True);place('client','Collection Route Desk');use('client','Collection Route Desk',local=True)
        place('host','Vehicle 0 Inspection');use('host','Vehicle 0 Inspection',local=True);wait(lambda:town()['works'][0]['worker']==NOBODY);check('release_E_cancels_inspection',not town()['services'][0]['inspected'])
        place('client','Route Bag 0');use('client','Route Bag 0',local=True);check('authoritative_visible_bag_pickup',town()['bags'][0]['holder']==read('client')['localId'])
        place('client',pos=(0,.08,-1));use('client','drop',local=True);check('normal_E_bag_drop',town()['bags'][0]['holder']==NOBODY and town()['bags'][0]['stage']==0)
        bag=town()['bags'][0]['position'];place('client',pos=(bag['x'],.08,bag['z']+1));use('client','Route Bag 0',local=True)
        command('client','disconnect');wait(lambda:len(read('host')['players'])==1 and town()['bags'][0]['holder']==NOBODY)
        check('disconnect_recovers_bag_and_releases_role',town()['owners'][2]==NOBODY and town()['bags'][0]['position']==town()['bags'][0]['origin'])
        command('client','reconnect');wait(lambda:read('client').get('connected') and len(read('client')['players'])==2,60);synced();use('client','job:3');check('rejoin_can_resume_shared_job',town()['started'][2] and equal())
        place('client','Route Bag 0');use('client','Route Bag 0',local=True);place('client','Paper Sorting Bin');hold('client',True);use('client','Paper Sorting Bin',local=True)
        place('host','Vehicle 0 Inspection');hold('host',True);use('host','Vehicle 0 Inspection',local=True)
        check('mechanic_and_sanitation_work_simultaneously',all(w['worker']!=NOBODY for w in town()['works']))
        wait(lambda:town()['services'][0]['inspected'] and town()['bags'][0]['stage']==1);hold('host',False);hold('client',False);synced()
        work('host','Vehicle 1 Inspection');check('two_fault_types_identified',all(s['inspected'] for s in town()['services']) and len({s['fault'] for s in town()['services']})==2)
        place('host','Parts Request Slip');use('host','Parts Request Slip',local=True);place('host','Parts Pickup Locker');use('host','Parts Pickup Locker',local=True)
        check('unoccupied_shop_fallback_transfers_real_stock',town()['quantities']==[1,1,0,0,1,1,0,0] and any(m['reason']=='unstaffed municipal reserve' for m in town()['movements']) and conserved())
        # Releasing a repair before its deadline must not consume its part.
        place('host','Vehicle 0 Repair');hold('host',True);use('host','Vehicle 0 Repair',local=True);hold('host',False);wait(lambda:town()['works'][0]['worker']==NOBODY)
        check('repair_cancel_keeps_part_and_fault',not town()['services'][0]['repaired'] and town()['quantities'][4]==1)
        capture('host','mechanic-hud');capture('host','mechanic-handbook',0);capture('host','mechanic-inventory',1)
        work('host','Vehicle 0 Repair');work('host','Vehicle 1 Repair');check('repairs_install_parts_and_generate_shared_waste',all(s['repaired'] for s in town()['services']) and town()['quantities'][6:8]==[1,1] and all(town()['bags'][i]['available'] for i in (4,5)) and conserved())
        place('host','Service Desk');use('host','Service Desk',local=True);use('host','Service Desk',local=True);check('mechanic_shift_records_and_contribution',town()['completed'][1] and len(town()['serviceRecords'])==2 and town()['contribution']==3 and equal())
        capture('host','mechanic-records',2);capture('host','mechanic-report',3)
        use('host','job:1');place('host','Delivery Desk');use('host','Delivery Desk',local=True);check('shop_delivery_generates_packaging_bag',town()['bags'][3]['available'] and state()['received'])
        capture('host','shopkeeper-hud');capture('host','shopkeeper-handbook',0)
        # Sanitation still has its independent route while the other player runs the original shop shift.
        place('host','Stock Crate');use('host','Stock Crate',local=True)
        place('host',pos=(-40,.08,1));reject('host','Town Exit');check('carried_stock_cannot_leave_shop',state()['holder']==read('host')['localId'] and next(p['position']['x'] for p in read('host')['players'] if p['owner']==read('host')['localId']) < -25)
        for part in ('Sunbeam Cola Shelf','Meadow Tea Shelf','Golden Crackers Shelf'):
            place('host',part);use('host',part,local=True);time.sleep(.8)
        check('original_shop_E_restocking_in_shared_town',state()['quantities'][9:12]==[6,6,6] and state()['holder']==NOBODY and conserved())
        place('client',pos=(8,.08,-3.4));command('client','camera',x=180,y=16,z=3.5);time.sleep(.3);capture('client','sanitation-hud');capture('client','sanitation-handbook',0);capture('client','sanitation-inventory',1)
        for i in range(1,6):
            if i==5:
                p=town()['bags'][i]['origin'];place('client',pos=(p['x']-.4,.08,p['z']+.3))
            else:place('client','Route Bag '+str(i))
            use('client','Route Bag '+str(i),local=True)
            material=town()['bags'][i]['materialType'];work('client',material+' Sorting Bin')
            check('sort_bag_'+str(i),town()['bags'][i]['stage']==1 and town()['bags'][i]['holder']==NOBODY)
        check('three_collection_points_and_cross_job_waste_processed',all(b['stage']==1 for b in town()['bags']) and town()['sanitationProgress'][1:3]==[3,3])
        place('client','Collection Route Desk');use('client','Collection Route Desk',local=True);use('client','Collection Route Desk',local=True)
        check('sanitation_manifest_complete_no_duplicate_records',town()['completed'][2] and len(town()['disposalRecords'])==6 and len({r['id'] for r in town()['disposalRecords']})==6 and equal())
        products=dict(cola='Sunbeam Cola Shelf',tea='Meadow Tea Shelf',crackers='Golden Crackers Shelf')
        for index,lines in enumerate([[('cola',2),('crackers',1)],[('tea',1),('crackers',2)],[('cola',1),('tea',2)]]):
            for sku,n in lines:
                for j in range(n):place('host',products[sku]);use('host',products[sku],local=True)
            place('host','Checkout');use('host','Checkout',local=True);check('shop_order_'+str(index+1),state()['orderIndex']==index+1 and len(state()['receipts'])==index+1 and equal())
        place('host','Closing Terminal');use('host','Closing Terminal',local=True)
        check('all_three_shifts_complete',state()['closed'] and town()['completed'][:3]==[True,True,True] and town()['contribution']==9 and sum(r['totalCents'] for r in state()['receipts'])==1650 and conserved() and equal())
        capture('host','shopkeeper-report',3);capture('client','sanitation-records',2);capture('client','sanitation-report',3);save('completed')
        # Exit remains within the existing shared session, then both players traverse the town with real movement.
        place('host',pos=(-40,.08,1.0));use('host','Town Exit');check('shop_exit_keeps_network_session',len(read('host')['players'])==2 and read('host')['connected'])
        place('host',pos=(-1,.08,0));place('client',pos=(1,.08,0));start=read('host')['players'];command('host','move',x=0,y=1,yaw=0,duration=1.1,run=False);command('client','move',x=0,y=1,yaw=0,duration=1.1,run=False);time.sleep(1.6);finish=read('host')['players']
        check('two_players_traverse_street_to_square',all(p['position']['z']-next(q['position']['z'] for q in start if q['owner']==p['owner'])>1.8 for p in finish))
        command('host','quit');wait(lambda:processes['host'].poll() is not None);wait(lambda:not read('client').get('listening'),30);check('host_shutdown_ends_session',True)
except Exception:
    try:save('failure')
    except Exception:pass
    traceback.print_exc();checks.append(dict(name='unexpected_failure',passed=False,details=traceback.format_exc()))
finally:
    for role,process in processes.items():
        if process.poll() is None:
            command(role,'quit')
            try:process.wait(timeout=8)
            except subprocess.TimeoutExpired:process.terminate()
    report=dict(method='Two independent Windows processes, actual NGO/UTP localhost UDP, production E dispatch and held input, opt-in server test placement.',humanControlled=False,checks=checks)
    (OUT/'validation.json').write_text(json.dumps(report,indent=2));print(f'{sum(c["passed"] for c in checks)}/{len(checks)} passed',flush=True);sys.exit(0 if all(c['passed'] for c in checks) else 1)
