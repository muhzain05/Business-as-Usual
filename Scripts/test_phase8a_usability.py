"""Living-mode onboarding and sustained production-RPC walking against solid geometry."""
from pathlib import Path
source=Path(__file__).with_name('test_phase8a_network.py').read_text()
exec(compile(source.split('\ntry:\n')[0],str(Path(__file__).with_name('test_phase8a_network.py')),'exec'))
def pos(role):return next(p['position'] for p in read(role)['players'] if p['owner']==read(role)['localId'])
def key(role,name):
    token=cmd(role,'key',target=name);wait(lambda:read(role).get('serial')==token);time.sleep(.35)
def walk_block(role,name,start,yaw,axis,limit,greater):
    place(role,pos=start,yaw=yaw)
    token=cmd(role,'realinput',run=True);wait(lambda:read(role).get('serial')==token)
    token=cmd(role,'capturecursor');wait(lambda:read(role).get('serial')==token)
    token=cmd(role,'key',target='W',duration=2.1);wait(lambda:read(role).get('serial')==token);time.sleep(2.5)
    token=cmd(role,'realinput',run=False);wait(lambda:read(role).get('serial')==token);time.sleep(.5)
    value=pos(role)[axis]
    print(f'{name}: {axis} {start[{"x":0,"z":2}[axis]]:.3f} -> {value:.3f}',flush=True)
    # Must both move away from the fixture start and stop at its near edge.
    check(name,abs(value-start[{'x':0,'z':2}[axis]])>.12 and (value>limit if greater else value<limit));capture(role,name)
    checks[-1].update(start=start,stop=pos(role),input='W held for 2.1 seconds through production InputActions')
try:
    for r in roles[:2]:launch(r);wait(lambda:read(r).get('connected'),60)
    use('host','job:1');use('client1','job:2');time.sleep(.5)
    check('intro_blocks_local_gameplay',read('host')['firstPerson']['inputBlocked'] and not read('host')['firstPerson']['gameplayActive'])
    capture('host','intro-controls');key('host','Enter');capture('host','intro-shopkeeper');key('host','Enter')
    key('client1','Enter');capture('client1','intro-mechanic');key('client1','Enter')
    wait(lambda:read('host').get('guidance',{}).get('objectives'))
    check('one_next_destination',len(read('host')['guidance']['objectives'])==1)
    key('host','H');check('help_reopens_and_blocks',read('host')['firstPerson']['inputBlocked']);key('host','Escape');check('escape_closes_help',not read('host')['firstPerson']['inputBlocked'])
    for r in roles[:2]:
        walk_block(r,r+'-market-shelf',(-42,.08,4.7),0,'z',5.5,False)
        walk_block(r,r+'-market-counter',(-41,.08,3.0),180,'z',2.4,True)
        walk_block(r,r+'-market-wall',(-43.8,.08,2.8),270,'x',-45.15,True)
        walk_block(r,r+'-yard-bin',(6,.08,-6.8),180,'z',-7.9,True)
        walk_block(r,r+'-service-vehicle',(6.9,.08,4.0),0,'z',5.0,False)
        walk_block(r,r+'-service-desk',(8,.08,8.8),180,'z',8.0,True)
        walk_block(r,r+'-manifest-desk',(10,.08,-7.5),0,'z',-6.5,False)
    place('host','Shipment');use('host','Shipment',local=True)
    check('local_E_receives_new_shipment_not_legacy_work',wd()['received'] and 'Living Workday/' in read('host')['currentTarget'])
    place('host','Dispatch');use('host','Dispatch',local=True)
    place('host','Parcel');use('host','Parcel',local=True)
    wait(lambda:'Deliver parcel' in read('host')['guidance']['note'])
    check('carried_parcel_guidance_is_delivery',read('host')['guidance']['note']=='Deliver parcel to garage')
    capture('host','parcel-next-step')
    place('host','Bay tray');use('host','Bay tray',local=True);use('host','Bay tray',local=True)
    check('receiving_tray_accessible_with_solids',wd()['dispatched'])
    save('complete')
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
