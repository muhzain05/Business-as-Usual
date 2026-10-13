"""Native one-window workdays with a genuine menu/hub entry and the two ordinary detour routes."""
from pathlib import Path
import argparse,sys
p=argparse.ArgumentParser(add_help=False);p.add_argument('--profession',choices=['shopkeeper','mechanic','sanitation'],required=True);p.add_argument('--route',choices=['returns','yard'],default='returns');opt,rest=p.parse_known_args();sys.argv=[sys.argv[0]]+rest
source=Path(__file__).with_name('test_phase8a_rescue_mechanic.py').read_text(encoding='utf-8-sig').split('\ntry:\n')[0];exec(compile(source,str(Path(__file__).with_name('test_phase8a_rescue_mechanic.py')),'exec'))
EXE=ROOT/'Builds/Phase8B/BusinessAsUsual.exe';assembly=EXE.parent/'BusinessAsUsual_Data/Managed/Assembly-CSharp.dll';assert json.loads((ROOT/'Docs/Diagnostics/Phase8B/build.json').read_text(encoding='utf-8-sig'))['result']=='Succeeded'
(OUT/'run-metadata.json').write_text(json.dumps(dict(assemblySha256=hashlib.sha256(assembly.read_bytes()).hexdigest(),executable=str(EXE),profession=opt.profession,route=opt.route,width=args.width,height=args.height),indent=2))
def on(name,alt=False):p=point(name);action(name,(p['x'],.08,p['z']-.86 if p['z']>0 else p['z']+.86),alt=alt)
def share(index):
 synced();before=state()['revision'];cmd('host','interact',target='day:share:'+str(index));wait(lambda:state()['revision']>before);synced()
def hold_route():
 route='Returns counter' if opt.route=='returns' else 'Yard holding';on(route);check('parcel_restaged_on_'+opt.route,item('PART-8A-001')['custody']==('returns' if opt.route=='returns' else 'yard-hold') and conserved());on(route,alt=True);on(route,alt=True);check('covert_note_is_a_regular_supported_declaration',wd()['parcelNote']=='Direct transfer');shot('temporary-hold');on(route);action('Bay tray',(8.2,.08,6.3));check('same_parcel_delivery_continues_after_detour',wd()['dispatched'] and conserved())
try:
 for suffix in ('.json','.control.json'):(OUT/('host'+suffix)).unlink(missing_ok=True)
 processes['host']=subprocess.Popen([str(EXE),'-screen-fullscreen','0','-screen-width',str(args.width),'-screen-height',str(args.height),'-logFile',str(OUT/'host.log'),'--bau-role','host','--bau-evidence',str(OUT/'host.json'),'--bau-control',str(OUT/'host.control.json'),'--bau-phase8b','--bau-rescue','--bau-ui-only','--bau-workday-seconds','480'],cwd=EXE.parent)
 wait(lambda:read('host').get('rescueUI',{}).get('screen')=='Main',60);token('solo');wait(lambda:read('host').get('connected'),40);use('host','job:'+str({'shopkeeper':1,'mechanic':2,'sanitation':3}[opt.profession]));time.sleep(.8);token('key',target='Enter');token('key',target='Enter');wait(lambda:state().get('day',{}).get('started'),30);wait(lambda:read('host')['rescueUI']['screen']=='Private');shot('private-note');token('key',target='Enter');wait(lambda:read('host')['rescueUI']['screen']=='HUD');check('one_worker_with_explicit_services',len(read('host')['players'])==1 and state()['day']['solo'] and state()['day']['unattended'])
 if opt.profession=='shopkeeper':
  action('Shipment',(-36,.08,9.2));action('Dispatch',(-36,.08,9.2));action('Parcel',(-36,.08,9.2));hold_route()
  for index in range(3):
   action('Stock box '+str(index),(-36.4,.08,8.45+index*.5));on('Stock shelf '+str(index));on('Shelf product '+str(index));action('Order counter',(-41.7,.08,2.8));action('Order register',(-40.3,.08,2.8));check('checkout_conserved_'+str(index),wd()['customerOrders']==index+1 and conserved())
 elif opt.profession=='mechanic':
  walk_into('Hood',(6.4,.08,4.1),4.82);action('Hood',(6.4,.08,4.1));action('Probes',(6.635,.08,4.58));wait(lambda:wd()['dispatched'],30);action('Bay tray',(8.2,.08,6.3));hold_route();action('Probes',(6.635,.08,4.58),alt=True);on('Service amendment');check('service_correction_retains_an_actual_copy',any(r['heading']=='Service amendment' for r in read('host')['workRecords']))
  action('Terminal',(6.68,.08,4.58),torque=True);action('Lift battery',(6.635,.08,4.58));action('Scrap tray',(5.3,.08,7));action('Replacement',(8.2,.08,6.3));action('Socket',(6.635,.08,4.58));action('Repair test',(6.635,.08,4.58));action('Hood',(6.4,.08,4.1));action('Service ticket',(7.65,.08,6.3));check('tested_repair_signed',wd()['serviceSigned'] and conserved())
 else:
  wait(lambda:wd()['dispatched'],35);action('Bay tray',(8.2,.08,6.3));hold_route();action('Collection cart',(7.5,.08,-6.15));token('preview',target='cart-travel');token('realinput',run=True);token('capturecursor');token('key',target='W',duration=2);token('realinput',run=False);token('key',target='Q');action('Square pickup',(-1,.08,7));on('Collection cart');wait(lambda:wd()['serviceSigned'],85);action('Paper pickup',(5.3,.08,7.45));on('Returns counter');on('Returns counter');on('Collection cart');action('Metal pickup',(4.5,.08,7.8));on('Collection cart');check('three_same_batches_loaded',sum(i['custody']=='cart' for i in wd()['items'])==3 and conserved());shot('loaded-cart')
  for index in [3,1,2]:
   on('Cart batch '+str(index));material=wd()['items'][index]['material'];on('Mixed chute',alt=True);on(('Paper' if material=='paper' else 'Metal' if material=='metal' else 'Mixed')+' chute')
  on('Manifest');check('manifest_and_personal_return_complete',sum(i['custody']=='recorded' for i in wd()['items'])==3 and read('host')['dayPrivate']['personalDone'])
 token('rescueui',target='Handbook',x=2);shot('professional-paper');share(0);token('rescueui',target='Board');shot('shared-copy');check('shared_board_only_contains_selected_copy',len(state()['day']['papers'])==1);token('rescueui',target='HUD')
 wait(lambda:wd()['serviceSigned'] and wd()['customerOrders']>=1 and sum(i['custody']=='recorded' for i in wd()['items'])==3,80);action('Shift report',(0,.08,3.9));wait(lambda:read('host')['rescueUI']['screen']=='Report');shot('shift-summary');check('full_solo_'+opt.profession+'_day_completed',wd()['shiftSigned'] and conserved())
 check('private_arrangement_requires_detour_declaration_and_real_work',all(read('host')['dayPrivate'][k] for k in ['detour','declaredDirect','productive']))
except Exception as e:checks.append(dict(name='exception',passed=False,error=str(e)));traceback.print_exc();shot('failure') if read('host') else None
finally:
 (OUT/'checks.json').write_text(json.dumps(checks,indent=2))
 for r,p in processes.items():
  if p.poll() is None:
   try:cmd(r,'quit');p.wait(timeout=6)
   except Exception:p.terminate()
 print(f'{sum(c["passed"] for c in checks)}/{len(checks)} checks passed',flush=True)
 if any(not c['passed'] for c in checks):raise SystemExit(1)
