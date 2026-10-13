"""Real Windows host/client transport test. Run after Phase 5 Windows build."""
import argparse, json, time, subprocess, pathlib, sys, math, traceback

ROOT = pathlib.Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser()
parser.add_argument('--output', type=pathlib.Path, default=ROOT / 'Docs/Diagnostics/Phase5/TwoProcess')
parser.add_argument('--exe', type=pathlib.Path, default=None)  # run this suite against a later build
ARGS = parser.parse_args()
OUT = ARGS.output.resolve()
EXE = (ARGS.exe or ROOT / 'Builds/Phase5/BusinessAsUsual.exe').resolve()
OUT.mkdir(parents=True, exist_ok=True)
checks, processes, serials = [], {}, {'host': 0, 'client': 0}
last_reports={}

def read(role):
    for _ in range(20):
        try:
            report=json.loads((OUT / f'{role}.json').read_text())
            last_reports[role]=report
            return report
        except (OSError, ValueError):time.sleep(.01)
    return last_reports.get(role,{})

def wait(predicate, seconds=30):
    end = time.monotonic() + seconds
    while time.monotonic() < end:
        try:
            value = predicate()
            if value: return value
        except (KeyError, IndexError, TypeError): pass
        time.sleep(.08)
    raise AssertionError('Timed out waiting for network state')

def check(name, passed, detail=''):
    checks.append(dict(name=name, passed=bool(passed), details=detail))
    print(('PASS ' if passed else 'FAIL ') + name, flush=True)
    if not passed: raise AssertionError(name)

def launch(role):
    for suffix in ('json', 'control.json'):
        path=OUT / f'{role}.{suffix}'
        if path.exists(): path.unlink()
    for name in (f'{role}.png',f'{role}-carrying.png',f'{role}-cooperative.png'):
        path=OUT/name
        if path.exists():path.unlink()
    processes[role] = subprocess.Popen([str(EXE), '-screen-fullscreen', '0', '-screen-width', '1280', '-screen-height', '720', '-logFile', str(OUT/f'{role}.log'), '--bau-role', role, '--bau-evidence', str(OUT/f'{role}.json'), '--bau-control', str(OUT/f'{role}.control.json'),'--bau-port','7785'], cwd=EXE.parent)

def command(role, action, **kwargs):
    serials[role] += 1
    path=OUT/f'{role}.control.json'
    tmp=path.with_suffix('.tmp')
    tmp.write_text(json.dumps(dict(serial=serials[role],action=action,**kwargs)))
    for attempt in range(100):
        try:tmp.replace(path);break
        except PermissionError:
            if attempt==99:raise
            time.sleep(.01)
    return serials[role]

def settled():
    wait(lambda: read('host')['state']['revision'] == read('client')['state']['revision'])
    time.sleep(.2)

def target(part):
    r=read('host'); return next(t for t in r['targetIds'] if part in t)

def place(role, id=None, position=None):
    if position is None:
        r=read('host');p=r['targetPoints'][r['targetIds'].index(id)]
        # Existing validation approaches shelves from the clear front aisle.
        position=(p['x'], .06, p['z']-1.1)
    token=command(role,'teleport',x=position[0],y=position[1],z=position[2])
    wait(lambda: read(role).get('serial')==token)
    time.sleep(.35)

def use(role,id,action='interact'):
    settled(); rev=read('host')['state']['revision']; command(role,action,target=id)
    wait(lambda: read('host')['state']['revision']>rev)
    settled()

def save(name):
    for role in ('host','client'):
        (OUT/f'{name}-{role}.json').write_text(json.dumps(read(role),indent=2))

def capture_pair(suffix=None):
    time.sleep(.3)
    began=time.time()
    for role in ('host','client'):command(role,'capture',target=suffix)
    def ready():
        paths=[OUT/(f'{role}-{suffix}.png' if suffix else f'{role}.png') for role in ('host','client')]
        return all(p.exists() and p.stat().st_mtime>=began for p in paths)
    wait(ready)
    assert read('host')['connected'] and read('client')['connected'],'Capture must complete while both processes are connected'
    time.sleep(.3)

def equal_state():
    a=read('host')['state'];b=read('client')['state']
    return all(a[k]==b[k] for k in ('revision','orderIndex','received','closed','dropped','holder','quantities','progress','movements','receipts','doorOpen','workUses'))

def conserved():
    q=read('host')['state']['quantities']; return all(sum(q[i::3])==6 for i in range(3))

try:
    launch('host');wait(lambda: read('host').get('connected'),60)
    delivery=target('Delivery Desk');crate=target('Stock Crate')
    place('host',delivery);use_rev=read('host')['state']['revision'];command('host','interact',target=delivery)
    wait(lambda: read('host')['state']['received'])
    launch('client');wait(lambda: read('client').get('connected') and len(read('client').get('players',[]))==2,60)
    settled(); check('host_client_connection_and_late_join_snapshot',equal_state() and read('client')['state']['received'])
    a,b=read('host'),read('client')
    check('separate_spawn_and_ownership',len(a['players'])==2 and {p['owner'] for p in a['players']}=={0,b['localId']} and sum(p['local'] for p in b['players'])==1)
    check('one_local_camera_listener_input_hud',all(read(r)[k]==1 for r in ('host','client') for k in ('cameras','listeners','inputs','huds')))
    place('host',position=(-3,.06,3));place('client',position=(-4,.06,3))
    start=read('host'); snapshot_count=start['snapshots'];
    command('host','move',x=0,y=1,yaw=0,duration=1,run=False)
    command('client','move',x=0,y=1,yaw=0,duration=1,run=True,jump=True)
    wait(lambda:any(p['jumping'] for p in read('host')['players']),3)
    check('jump_state_replicated',wait(lambda:any(p['jumping'] for p in read('client')['players']),3))
    time.sleep(.2); moving=read('host');save('movement')
    check('server_movement_and_running_state',any(p['speed']>.5 for p in moving['players']) and any(p['running'] for p in moving['players']))
    time.sleep(1);a,b=read('host'),read('client')
    check('independent_controls_move_both_characters',all(p['position']['z']-next(q for q in start['players'] if q['owner']==p['owner'])['position']['z']>1 for p in a['players']))
    check('remote_movement_synchronization',all(math.dist((p['position']['x'],p['position']['y'],p['position']['z']),(next(q for q in b['players'] if q['owner']==p['owner'])['position']['x'],next(q for q in b['players'] if q['owner']==p['owner'])['position']['y'],next(q for q in b['players'] if q['owner']==p['owner'])['position']['z']))<.25 for p in a['players']))
    check('shared_snapshots_not_sent_each_frame',a['snapshots']==snapshot_count)
    # A far-away direct request must be rejected by the host, even though it bypasses local detection.
    rejected=a['rejected'];command('client','interact',target=crate);wait(lambda: read('host')['rejected']>rejected)
    check('server_range_validation',read('host')['state']['holder']==18446744073709551615)
    # Replay host's delivery command sequence 1: no additional inventory or revenue.
    rejected=read('host')['rejected'];command('host','raw',target=delivery,sequence=1,revision=use_rev,order='S1-001');wait(lambda: read('host')['rejected']>rejected)
    check('duplicate_command_prevention',conserved())
    place('host',crate);place('client',crate)
    settled(); rev=read('host')['state']['revision']
    command('host','interact',target=crate);command('client','interact',target=crate)
    wait(lambda: read('host')['state']['holder']!=18446744073709551615);time.sleep(.5);settled()
    check('pickup_contention_single_holder',read('host')['state']['revision']==rev+1 and sum(read('host')['state']['quantities'][6:9])==18 and equal_state())
    holder='host' if read('host')['state']['holder']==0 else 'client'
    check('carried_object_is_local_to_holder',read(holder)['localCarried'] and not read('client' if holder=='host' else 'host')['localCarried'])
    p=next(p['position'] for p in read('host')['players'] if p['owner']==read('host')['state']['holder']);place('client' if holder=='host' else 'host',position=(p['x']-.65,.06,p['z']+.65))
    capture_pair('carrying')
    other='client' if holder=='host' else 'host';rejected=read('host')['rejected'];held=read('host')['state']['holder']
    command(other,'interact',target='return');wait(lambda:read('host')['rejected']>rejected)
    check('only_holder_can_return',read('host')['state']['holder']==held)
    rejected=read('host')['rejected'];rev=read('host')['state']['revision']
    command(holder,'raw',target='return',sequence=100,revision=rev-1,order='S1-001');wait(lambda:read('host')['rejected']>rejected)
    check('delayed_stale_command_rejected',read('host')['state']['revision']==rev and read('host')['state']['holder']==held)
    # Drop on clear ground, then recover the same crate and return it.
    place(holder,position=(0,.06,3));use(holder,'drop',action='e');check('authoritative_E_drop_conserves_stock',conserved() and read('host')['state']['holder']==18446744073709551615)
    save('dropped');p=read('host')['state']['cratePosition'];place('client',position=(p['x'],.06,p['z']+1.0));rev=read('host')['state']['revision'];command('client','e');wait(lambda:read('host')['state']['revision']>rev);settled()
    check('local_E_pickup_detection',read('host')['state']['holder']==read('client')['localId'])
    command('client','disconnect');wait(lambda: len(read('host')['players'])==1 and read('host')['state']['holder']==18446744073709551615)
    check('disconnect_while_carrying_recovery',sum(read('host')['state']['quantities'][3:6])==18 and conserved())
    command('client','reconnect');wait(lambda: read('client').get('connected') and len(read('client')['players'])==2);settled()
    check('client_reconnection_and_full_snapshot',equal_state())
    # Stateful door and workstation adapters, using the existing Phase 3 components.
    for part,field in [('Shared Door Test Fixture','doorOpen'),('Shared Workstation Test Fixture','workUses')]:
        id=target(part);place('host',id);use('host',id);check('shared_'+field,equal_state() and bool(read('host')['state'][field][0]))
    door=target('Shared Door Test Fixture');place('client',door);use('client',door);check('door_closes_on_both_processes',not read('host')['state']['doorOpen'][0] and equal_state())
    # Exercise the production E dispatcher while carrying, not only direct shelf RPCs.
    place('host',crate);use('host',crate,action='e')
    shelf=target('Sunbeam Cola Shelf');place('host',shelf);place('client',position=(0,.06,3))
    use('host',shelf,action='e')
    check('host_local_E_restocks_shelf_keeps_crate',read('host')['state']['quantities'][9]==6 and read('host')['state']['holder']==0 and conserved() and equal_state())
    time.sleep(.85);use('host','return')
    place('host',position=(0,.06,3));place('client',crate);use('client',crate,action='e')
    for sku,part in [('tea','Meadow Tea Shelf'),('crackers','Golden Crackers Shelf')]:
        shelf=target(part);place('client',shelf);place('host',shelf);settled();rev=read('host')['state']['revision']
        command('client','e');command('host','e')
        wait(lambda: read('host')['state']['revision']>rev);time.sleep(.85);settled()
        check('simultaneous_stocking_'+sku,read('host')['state']['revision']==rev+1 and conserved() and equal_state())
        check('client_local_E_restocks_'+sku,read('host')['state']['quantities'][9+['cola','tea','crackers'].index(sku)]==6 and not read('host')['state']['dropped'])
    check('empty_crate_auto_return',read('host')['state']['holder']==18446744073709551615)
    checkout=target('Checkout')
    for index,lines in enumerate([[('cola',2),('crackers',1)],[('tea',1),('crackers',2)],[('cola',1),('tea',2)]]):
        for sku,n in lines:
            shelf=target(dict(cola='Sunbeam Cola Shelf',tea='Meadow Tea Shelf',crackers='Golden Crackers Shelf')[sku])
            for j in range(n):
                role='host' if j%2==0 else 'client';place(role,shelf);use(role,shelf)
        place('host',checkout);place('client',checkout);settled();rev=read('host')['state']['revision']
        command('host','interact',target=checkout);command('client','interact',target=checkout)
        wait(lambda: read('host')['state']['orderIndex']==index+1);time.sleep(.5);settled()
        check('shared_order_and_duplicate_receipt_'+str(index+1),read('host')['state']['revision']==rev+1 and len(read('host')['state']['receipts'])==index+1 and equal_state() and conserved())
    place('host',position=(-3.1,.06,4));place('client',position=(-3.1,.06,5))
    capture_pair('cooperative')
    closing=target('Closing Terminal');place('client',closing);use('client',closing)
    check('shared_shift_completion',read('host')['state']['closed'] and all(p>=t for p,t in zip(read('host')['state']['progress'],[1,6,6,6,3,1])) and sum(r['totalCents'] for r in read('host')['state']['receipts'])==1650 and equal_state())
    save('completed')
    capture_pair()
    command('host','quit');wait(lambda: processes['host'].poll() is not None)
    wait(lambda: not read('client').get('connected'),30)
    check('host_disconnect_ends_client_session',not read('client')['listening'])
except Exception as e:
    traceback.print_exc()
    checks.append(dict(name='unexpected_failure',passed=False,details=traceback.format_exc()))
    print(str(e),flush=True)
finally:
    for role,p in processes.items():
        if p.poll() is None:
            command(role,'quit')
            try:p.wait(timeout=8)
            except subprocess.TimeoutExpired:p.terminate()
    report=dict(method='Two independent Windows Development builds, actual NGO/Unity Transport localhost UDP. Automated commands and server test placement; no human-controlled playtest.',humanControlled=False,checks=checks)
    (OUT/'validation.json').write_text(json.dumps(report,indent=2))
    print(f'{sum(c["passed"] for c in checks)}/{len(checks)} passed',flush=True)
    sys.exit(0 if all(c['passed'] for c in checks) else 1)
