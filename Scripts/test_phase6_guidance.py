"""Two independent Windows processes using production command dispatch and actual NGO/UTP."""
from pathlib import Path
import argparse,json,subprocess,time,traceback,sys,math
ROOT=Path(__file__).resolve().parents[1]
args=argparse.ArgumentParser();args.add_argument('--output',type=Path,default=ROOT/'Docs/Diagnostics/Phase6/TwoProcess');args.add_argument('--scenario',choices=('guidance',),default='guidance');args.add_argument('--exe',type=Path,default=None);args.add_argument('--width',type=int,default=1280);args.add_argument('--height',type=int,default=720);options=args.parse_args()
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
try:
    launch('host');wait(lambda:read('host').get('connected'),60);launch('client');wait(lambda:read('client').get('connected') and len(read('client').get('players',[]))==2,60);synced()
    use('host','job:2');use('client','job:3');time.sleep(3.3)
    def guide(role):return read(role)['guidance']
    def active(role):return [o for o in guide(role)['objectives'] if o['active']]
    check('public_ledger_excludes_personal_action_message',state()['message']=='Town ledger updated.')
    check('two_personal_staff_notes',guide('host')['job']=='Mechanic' and guide('client')['job']=='Sanitation' and guide('host')['note']!=guide('client')['note'])
    check('compact_paper_hud_under_12_percent',all(guide(r)['paperFraction']<.12 for r in ('host','client')))
    check('markers_match_real_interaction_points',all(o['position']==read(r)['targetPoints'][read(r)['targetIds'].index(o['id'])] for r in ('host','client') for o in guide(r)['objectives']))
    command('host','key',target='Tab');wait(lambda:guide('host')['bookOpen']);check('Tab_opens_local_book_and_releases_cursor',not guide('host')['cursorLocked'] and not guide('client')['bookOpen'])
    command('host','key',target='Tab');wait(lambda:not guide('host')['bookOpen']);check('Tab_closes_book_and_restores_capture',guide('host')['cursorLocked'])
    command('host','key',target='J');wait(lambda:guide('host')['staffDesk']);check('J_opens_staff_desk_locally',not guide('host')['cursorLocked'] and not guide('client')['staffDesk'])
    command('host','key',target='J');wait(lambda:not guide('host')['staffDesk']);check('J_closes_staff_desk_and_restores_capture',guide('host')['cursorLocked'])
    capture('host','mechanic-start');capture('client','sanitation-start');save('personal-notes')
    before=read('client')['feedbackCount'];note=guide('client')['note'];place('host','Service Desk');use('host','Service Desk',True);time.sleep(3.3)
    check('actor_feedback_stays_private',read('client')['feedbackCount']==before and guide('client')['note']==note and read('host')['feedbackCount']>0)
    check('only_mechanic_inspection_objectives',len(active('host'))==2 and all(o['icon']=='inspect' for o in active('host')) and all(o['icon']!='inspect' for o in active('client')))
    place('host','Vehicle 0 Inspection');command('host','camera',x=0,y=22,z=3.5);capture('host','inspection-markers');work('host','Vehicle 0 Inspection')
    check('completed_inspection_removes_active_pin',len([o for o in active('host') if o['icon']=='inspect'])==1)
    work('host','Vehicle 1 Inspection');place('host','Parts Request Slip');use('host','Parts Request Slip',True)
    check('repair_spots_visible_while_collecting_parts',len([o for o in guide('host')['objectives'] if o['icon']=='wrench' and not o['active']])==2)
    place('host','Parts Pickup Locker');use('host','Parts Pickup Locker',True)
    check('repair_objectives_activate_after_real_pickup',len(active('host'))==2 and all(o['icon']=='wrench' for o in active('host')))
    place('host','Vehicle 0 Repair');command('host','camera',x=0,y=22,z=3.5);capture('host','repair-markers');capture('host','mechanic-tasks',0);capture('host','mechanic-inventory',1);work('host','Vehicle 0 Repair')
    check('repair_progress_advances_local_guidance',len(active('host'))==1 and 'bay 2' in active('host')[0]['title'])
    place('client','Collection Route Desk');use('client','Collection Route Desk',True);place('client','Route Bag 0');use('client','Route Bag 0',True)
    check('carried_bag_points_only_to_matching_bin',len(active('client'))==1 and 'Paper Sorting Bin' in active('client')[0]['id'])
    capture('client','bag-to-matching-bin');capture('client','sanitation-tasks',0)
    work('client','Paper Sorting Bin');check('sorted_bag_advances_to_file_manifest',active('client')[0]['icon']=='record')
    work('host','Vehicle 1 Repair');place('host','Service Desk');use('host','Service Desk',True);use('host','Service Desk',True);use('host','job:1')
    check('role_change_replaces_personal_guidance',guide('host')['job']=='Shopkeeper' and guide('client')['job']=='Sanitation')
    place('host',pos=(-40,.08,1));time.sleep(.4);check('interior_map_projects_self_to_market',guide('host')['mapPlayer']=={'x':-8.0,'y':0.0,'z':6.5})
    check('shop_has_stockroom_objective',any('Delivery Desk' in o['id'] for o in active('host')))
    place('host','Delivery Desk');use('host','Delivery Desk',True);check('delivery_advances_to_stock_crate_marker',any('Stock Crate' in o['id'] for o in active('host')))
    place('host','Stock Crate');use('host','Stock Crate',True);check('carried_crate_marks_three_product_shelves',len(active('host'))==3 and all(o['icon']=='parts' and 'Shelf' in o['id'] for o in active('host')))
    place('host','Sunbeam Cola Shelf');use('host','Sunbeam Cola Shelf',True);time.sleep(.8);check('stocked_shelf_retires_its_objective',len(active('host'))==2 and all('Sunbeam Cola Shelf' not in o['id'] for o in active('host')))
    capture('host','shopkeeper-hud');capture('host','shopkeeper-tasks',0);capture('host','shopkeeper-inventory',1);capture('host','shopkeeper-receipts',2);capture('host','shopkeeper-summary',3);save('guidance-completed')

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
