"""Native solo retail/sanitation, accepted commands and actual keyboard handling."""
from pathlib import Path
import argparse,sys
selection=argparse.ArgumentParser(add_help=False);selection.add_argument('--profession',choices=['shopkeeper','sanitation'],required=True);option,rest=selection.parse_known_args();sys.argv=[sys.argv[0]]+rest
prefix=Path(__file__).with_name('test_phase8a_rescue_mechanic.py').read_text(encoding='utf-8-sig').split('\ntry:\n')[0]
exec(compile(prefix,str(Path(__file__).with_name('test_phase8a_rescue_mechanic.py')),'exec'))
def position(name):
 p=point(name);return (p['x'],.08,p['z']-(.85 if p['z']>0 else -.85))
def on(name,alt=False):action(name,position(name),alt=alt)
def walk_exit():
 place('host',pos=(-40,.08,1.0),yaw=180);token('look',x=180,y=0);shot('framed-town-view');token('realinput',run=True);token('capturecursor');token('key',target='W',duration=.85);token('realinput',run=False);wait(lambda:read('host')['players'][0]['position']['x']>-25);check('walking_threshold_retains_same_parcel',item('PART-8A-001')['holder']==read('host')['localId']);shot('outside-market')
try:
 for suffix in ('.json','.control.json'):(OUT/('host'+suffix)).unlink(missing_ok=True)
 processes['host']=subprocess.Popen([str(EXE),'-screen-fullscreen','0','-screen-width',str(args.width),'-screen-height',str(args.height),'-logFile',str(OUT/'host.log'),'--bau-role','host','--bau-evidence',str(OUT/'host.json'),'--bau-control',str(OUT/'host.control.json'),'--bau-rescue','--bau-ui-only'],cwd=EXE.parent)
 wait(lambda:read('host').get('rescueUI',{}).get('screen')=='Main',60);token('solo');wait(lambda:read('host').get('connected'));use('host','job:'+('1' if option.profession=='shopkeeper' else '3'));time.sleep(.8);token('key',target='Enter');token('key',target='Enter');wait(lambda:read('host')['rescueUI']['screen']=='HUD')
 check('one_real_solo_player',len(read('host')['players'])==1)
 if option.profession=='shopkeeper':
  action('Shipment',(-36,.08,9.2));action('Dispatch',(-36,.08,9.2),alt=True);action('Dispatch',(-36,.08,9.2));action('Parcel',(-36,.08,9.2));walk_exit();action('Bay tray',(8.2,.08,6.3));check('conserved_battery_parcel_delivered',wd()['dispatched'] and conserved())
  for index in range(3):
   action('Stock box '+str(index),(-36.4,.08,8.45+index*.5));on('Stock shelf '+str(index));check('stock_matches_printed_shelf_'+str(index),item('ORDER-8A-'+str(index+1).zfill(3))['shelved']);on('Shelf product '+str(index));action('Order counter',(-41.7,.08,2.8));action('Order register',(-40.3,.08,2.8));check('physical_checkout_'+str(index),wd()['customerOrders']==index+1 and conserved())
  token('rescueui',target='Handbook',x=2);shot('retail-receipt');token('rescueui',target='HUD');check('retail_receipts_have_partial_scope',any(r['heading']=='Checkout receipt' and 'Earlier custody' in r['text'] for r in read('host')['workRecords']))
 else:
  action('Collection cart',(7.5,.08,-6.15));before=wd()['cartZ'];token('preview',target='cart-travel');token('realinput',run=True);token('capturecursor');token('key',target='W',duration=2);token('realinput',run=False);token('key',target='Q');check('server_cart_moves_and_parks_from_real_input',wd()['cartZ']>before+1.5 and wd()['cartPusher']==18446744073709551615);shot('pushed-cart')
  action('Square pickup',(-1,.08,7.0));on('Collection cart');check('same_square_batch_loaded',item('BAG-8A-001')['custody']=='cart' and conserved())
  wait(lambda:wd()['serviceSigned'],80)
  action('Paper pickup',(5.3,.08,7.45));on('Collection cart');action('Metal pickup',(4.5,.08,7.8));on('Collection cart');check('three_real_batches_in_cart',sum(i['custody']=='cart' for i in wd()['items'])==3 and conserved());shot('loaded-cart')
  cx,cz=wd()['cartX'],wd()['cartZ'];action('Collection cart',(cx,.08,cz+.95));token('look',x=180,y=0);token('realinput',run=True);token('capturecursor');token('key',target='W',duration=2.2);token('realinput',run=False);token('key',target='Q');check('loaded_cart_returns_toward_yard',wd()['cartZ']<cz-2)
  for index in [3,1,2]:
   on('Cart batch '+str(index));material=wd()['items'][index]['material'];on('Mixed chute',alt=True);on(('Paper' if material=='paper' else 'Metal' if material=='metal' else 'Mixed')+' chute');check('selected_cart_batch_sorted_'+str(index),wd()['items'][index]['custody']=='sorted' and conserved())
  on('Manifest');check('all_collection_tickets_filed',sum(i['custody']=='recorded' for i in wd()['items'])==3);token('rescueui',target='Handbook',x=2);shot('collection-manifest');token('rescueui',target='HUD')
 wait(lambda:wd()['serviceSigned'] and sum(i['custody']=='recorded' for i in wd()['items'])==3 and wd()['customerOrders']>=1,70);action('Shift report',(0,.08,3.9));check('full_'+option.profession+'_solo_shift_completed',wd()['shiftSigned'] and conserved());shot('completed-shift')
except Exception as e:checks.append(dict(name='exception',passed=False,error=str(e)));traceback.print_exc();shot('failure') if read('host') else None
finally:
 (OUT/'checks.json').write_text(json.dumps(checks,indent=2))
 for r,p in processes.items():
  if p.poll() is None:
   try:cmd(r,'quit');p.wait(timeout=6)
   except Exception:p.terminate()
 print(f'{sum(c["passed"] for c in checks)}/{len(checks)} checks passed',flush=True)
 if any(not c['passed'] for c in checks):raise SystemExit(1)
