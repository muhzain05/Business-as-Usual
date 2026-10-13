"""Targeted Phase7B acceptance: real two-process authority, markers, resolution and cadence."""
from pathlib import Path
shared = Path(__file__).with_name('test_phase7a_network.py')
code = shared.read_text().split('\ntry:\n', 1)[0]
code = code.replace('Docs/Diagnostics/Phase7A/TwoProcess','Docs/Diagnostics/Phase7B/Targeted').replace('Builds/Phase7A/BusinessAsUsual.exe','Builds/Phase7B/BusinessAsUsual.exe')
exec(compile(code, str(shared), 'exec'))

def markers(role): return fp(role)['mapMarkerIds']
def active(role): return [o for o in read(role)['guidance']['objectives'] if o['active']]
def verify_markers(role, label):
    wait(lambda: set(markers(role)) == set(o['id'] for o in active(role)), 5)
    check(label, set(markers(role)) == set(o['id'] for o in active(role)))
    check(label+'_coordinates', all(p == o['position'] for p,o in zip(fp(role)['mapMarkerWorld'],active(role))))
def work(role, part):
    place(role,part); hold(role,True);use(role,part,True)
    wait(lambda: all(w['worker']!=read(role)['localId'] for w in town()['works']),15)
    hold(role,False);synced()
def native(role,label):
    wait(lambda: fp(role)['internalWidth']==options.width and fp(role)['internalHeight']==options.height,10)
    data=fp(role)
    check(label, data['screenWidth']==options.width and data['screenHeight']==options.height and data['internalWidth']==options.width and data['internalHeight']==options.height and data['renderScale']==1 and not data['dynamicResolution'],str({k:data[k] for k in ['screenWidth','screenHeight','internalWidth','internalHeight','renderScale','dynamicResolution']}))

try:
    launch('host');wait(lambda:read('host').get('connected'),60)
    launch('client');wait(lambda:read('client').get('connected') and len(read('client').get('players',[]))==2,60);synced()
    use('host','job:2');use('client','job:3');time.sleep(.5)
    for r in ['host','client']: native(r,r+'_native_initial')
    for q in [0,1]:
        done('host',command('host','quality',x=q));time.sleep(.8)
        done('host',command('host','display',x=options.width,y=options.height,run=True));time.sleep(.8)
        check('quality_preset_selected_'+str(q),fp('host')['qualityLevel']==q)
        native('host','native_quality_'+str(q))
    check('synchronized_server_clock',fp('host')['serverClock'] and fp('client')['serverClock'] and abs(fp('host')['townClock']-fp('client')['townClock'])<.5)
    check('same_town_phase',abs(fp('host')['townPhase']-fp('client')['townPhase'])<.002)
    save('native-resolution-and-clock')
    place('host',pos=(0,.08,-1));look('host',90,-20)
    wait(lambda:abs(((seen_by('client','host')['facing']-90)+180)%360-180)<8,5)
    check('network_facing_survives_animation_overrides',abs(((seen_by('client','host')['facing']-90)+180)%360-180)<8)
    place('host',pos=(8,.08,1.2));look('host',0,0)
    for fov in [60,74,95]:
        command('host','fov',x=fov);time.sleep(.5);capture('host','hands-rest-fov-'+str(fov));check('hands_visible_fov_'+str(fov),fp('host')['handsVisible'] and fp('host')['fov']==fov)
    command('host','fov',x=74);time.sleep(.3)
    place('host','Service Desk');use('host','Service Desk',True);verify_markers('host','mechanic_initial_inspections');save('inspect-before');capture('host','minimap-before-inspection')
    work('host','Vehicle 0 Inspection');verify_markers('host','one_inspection_after_completion');check('completed_inspection_gone',all('Vehicle 0 Inspection' not in x for x in markers('host')));save('inspect-after');capture('host','minimap-after-inspection')
    check('no_inactive_repair_blue_pin',all('Repair' not in x for x in markers('host')) and any(not o['active'] for o in read('host')['guidance']['objectives']))
    check('private_markers_no_cross_job',all('Sorting' not in x and 'Route Bag' not in x for x in markers('host')) and all('Vehicle' not in x for x in markers('client')))
    work('host','Vehicle 1 Inspection');place('host','Parts Request Slip');use('host','Parts Request Slip',True);verify_markers('host','parts_unlock')
    place('host','Parts Pickup Locker');use('host','Parts Pickup Locker',True);verify_markers('host','repairs_unlock');work('host','Vehicle 0 Repair');verify_markers('host','repair_one_completed');work('host','Vehicle 1 Repair');verify_markers('host','record_unlock')
    place('host','Service Desk');use('host','Service Desk',True);use('host','Service Desk',True);wait(lambda:not markers('host'));check('mechanic_complete_no_markers',not markers('host'))
    place('client','Collection Route Desk');use('client','Collection Route Desk',True);verify_markers('client','sanitation_route');save('route-before')
    for i,material in [(0,'Paper'),(1,'Mixed'),(2,'Metal')]:
        place('client','Route Bag '+str(i));use('client','Route Bag '+str(i),True);verify_markers('client','matching_bin_'+material)
        if i==0:
            look('client',0,0);capture('client','hands-carry-bag');save('bag-held')
        work('client',material+' Sorting Bin');verify_markers('client','sorted_'+material);check('collected_bag_removed_'+str(i),all('Route Bag '+str(i) not in x for x in markers('client')))
    place('client','Collection Route Desk');use('client','Collection Route Desk',True);use('client','Collection Route Desk',True);wait(lambda:not markers('client'));check('sanitation_complete_no_markers',not markers('client'))
    use('host','job:1');place('host',pos=(-40,.08,1));verify_markers('host','shop_delivery_interior')
    place('host',pos=(-8.2,.08,1.2));wait(lambda:len(markers('host'))==1);check('exterior_objective_bound_to_actual_door',len(markers('host'))==1 and 'Door' in markers('host')[0]);save('shop-exterior')
    place('host',pos=(-40,.08,1));place('host','Delivery Desk');use('host','Delivery Desk',True);verify_markers('host','stock_crate_unlock');place('host','Stock Crate');use('host','Stock Crate',True);verify_markers('host','shelves_unlock');look('host',0,0);capture('host','hands-carry-crate')
    for shelf in ['Sunbeam Cola Shelf','Meadow Tea Shelf','Golden Crackers Shelf']:
        place('host',shelf);use('host',shelf,True);time.sleep(.85);verify_markers('host','restock_'+shelf.replace(' ','_'));check('stocked_marker_retires_'+shelf,all(shelf not in o['id'] or o['icon']!='parts' for o in active('host')))
    for order in [('Sunbeam Cola Shelf','Sunbeam Cola Shelf','Golden Crackers Shelf'),('Meadow Tea Shelf','Golden Crackers Shelf','Golden Crackers Shelf'),('Sunbeam Cola Shelf','Meadow Tea Shelf','Meadow Tea Shelf')]:
        for shelf in order:place('host',shelf);use('host',shelf,True)
        verify_markers('host','checkout_ready_'+str(state()['orderIndex']));place('host','Checkout');use('host','Checkout',True)
    verify_markers('host','closing_unlock');place('host','Closing Terminal');use('host','Closing Terminal',True);place('host',pos=(0,.08,-1));wait(lambda:not markers('host'));check('shop_complete_outside_no_stale_marker',not markers('host'))
    place('host',pos=(0,.08,-1));place('client',pos=(-2,.08,-3));look('client',35,0)
    for running,speed,expected in [(False,2.2,2.2/(.26/(.6*(20/60)))),(True,4.2,4.2/(.32/(.4*(16/60))))]:
        command('host','move',x=1,y=0,yaw=0,duration=2.8,run=running);time.sleep(.7)
        data=fp('host');check('cadence_'+('run' if running else 'walk'),abs(data['cadence']-expected)<.1,str(data['cadence']))
        check('remote_speed_'+str(speed),abs(seen_by('client','host')['speed']-speed)<.15)
        foot_samples=[]
        for _ in range(8):
            foot_samples.append(seen_by('client','host')['leftFoot']);time.sleep(.065)
        spread=max(math.dist(tuple(p.values()),tuple(foot_samples[0].values())) for p in foot_samples)
        check('remote_bones_animate_'+('run' if running else 'walk'),spread>.03,str(spread))
        capture('client','remote-'+('run' if running else 'walk'));save('remote-'+('run' if running else 'walk'));time.sleep(1.3);place('host',pos=(0,.08,-1))
    capture('host','native-'+str(options.width)+'x'+str(options.height));save('final')
except Exception:
    try:save('failure')
    except Exception:pass
    traceback.print_exc();checks.append(dict(name='unexpected_failure',passed=False,detail=traceback.format_exc()))
finally:
    for role,process in processes.items():
        if process.poll() is None:
            command(role,'quit')
            try:process.wait(timeout=8)
            except subprocess.TimeoutExpired:process.terminate()
    (OUT/'validation.json').write_text(json.dumps(dict(method='Two actual NGO Windows processes; opt-in placement, production commands. Native URP target descriptor measured.',humanControlled=False,checks=checks),indent=2))
    print(f'{sum(c["passed"] for c in checks)}/{len(checks)} passed',flush=True)
    sys.exit(0 if checks and all(c['passed'] for c in checks) else 1)
