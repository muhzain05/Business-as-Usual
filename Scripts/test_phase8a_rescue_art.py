"""Final material/geometry smoke in the actual native player. No inventory fixtures are invented."""
from pathlib import Path
source=Path(__file__).with_name('test_phase8a_rescue_mechanic.py').read_text(encoding='utf-8-sig').split('\ntry:\n')[0]
exec(compile(source,str(Path(__file__).with_name('test_phase8a_rescue_mechanic.py')),'exec'))
try:
 for suffix in ('.json','.control.json'):(OUT/('host'+suffix)).unlink(missing_ok=True)
 processes['host']=subprocess.Popen([str(EXE),'-screen-fullscreen','0','-screen-width',str(args.width),'-screen-height',str(args.height),'-logFile',str(OUT/'host.log'),'--bau-role','host','--bau-evidence',str(OUT/'host.json'),'--bau-control',str(OUT/'host.control.json'),'--bau-rescue','--bau-ui-only'],cwd=EXE.parent)
 wait(lambda:read('host').get('rescueUI',{}).get('screen')=='Main',60);token('solo');wait(lambda:read('host').get('connected'));use('host','job:1');time.sleep(.8);token('key',target='Enter');token('key',target='Enter');wait(lambda:read('host')['rescueUI']['screen']=='HUD')
 place('host',pos=(-40.3,.08,2.8));token('look',x=180,y=14);shot('checkout-packaging');check('final_native_material_scene_runs',len(read('host')['players'])==1 and conserved())
 aim('Order counter',(-41.7,.08,2.8));token('realinput',run=True);token('capturecursor');token('key',target='W',duration=1.1);token('realinput',run=False);z=read('host')['players'][0]['position']['z'];check('checkout_blocks_actual_W_movement',2.1<z<2.75);shot('checkout-collision')
 action('Stock box 2',(-36.4,.08,9.45));onpoint=point('Stock shelf 2');action('Stock shelf 2',(onpoint['x'],.08,onpoint['z']-.85));check('final_product_uses_conserved_stock',item('ORDER-8A-003')['shelved']);action('Shelf product 2',(onpoint['x'],.08,onpoint['z']-.85));shot('held-crackers')
 token('rescueui',target='Handbook',x=1);shot('stock-inventory');check('inventory_has_one_physical_carried_unit',item('ORDER-8A-003')['holder']==read('host')['localId']);token('rescueui',target='HUD');token('key',target='Q');wait(lambda:item('ORDER-8A-003')['holder']==18446744073709551615);check('safe_return_preserves_same_shelf_stock',item('ORDER-8A-003')['custody']=='shelf' and conserved())
 token('key',target='J');wait(lambda:read('host')['rescueUI']['screen']=='Staff');check('final_staff_reopens',True);token('key',target='J');wait(lambda:read('host')['rescueUI']['screen']=='HUD')
except Exception as e:checks.append(dict(name='exception',passed=False,error=str(e)));traceback.print_exc()
finally:
 (OUT/'checks.json').write_text(json.dumps(checks,indent=2))
 for r,p in processes.items():
  if p.poll() is None:
   try:cmd(r,'quit');p.wait(timeout=6)
   except Exception:p.terminate()
 print(f'{sum(c["passed"] for c in checks)}/{len(checks)} checks passed',flush=True)
 if any(not c['passed'] for c in checks):raise SystemExit(1)
