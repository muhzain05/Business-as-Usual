"""One native solo player, real E input and mouse torque, authority/collision validation."""
from pathlib import Path
source=Path(__file__).with_name('test_phase8a_network.py').read_text(encoding='utf-8')
exec(compile(source.split('\ntry:\n')[0],str(Path(__file__).with_name('test_phase8a_network.py')),'exec'))
import math
EXE=ROOT/'Builds/Phase8A-RescueV2/BusinessAsUsual.exe';roles=['host']
build=json.loads((ROOT/'Docs/Diagnostics/Phase8ARescueV2/build.json').read_text(encoding='utf-8-sig'))
if build.get('result')!='Succeeded':raise RuntimeError('Native validation requires the latest rescue build to succeed; refusing to use a stale executable.')
assembly=EXE.parent/'BusinessAsUsual_Data/Managed/Assembly-CSharp.dll'
(OUT/'run-metadata.json').write_text(json.dumps(dict(assemblySha256=hashlib.sha256(assembly.read_bytes()).hexdigest(),executable=str(EXE),width=args.width,height=args.height),indent=2))
def token(action,**fields):
 t=cmd('host',action,**fields);wait(lambda:read('host').get('serial')==t);time.sleep(max(.4,fields.get('duration',0)+.15))
def shot(name):
 token('capture',target=name);wait(lambda:(OUT/('host-'+name+'.png')).exists());save(name)
def point(name):return read('host')['targetPoints'][read('host')['targetIds'].index(target(name))]
def aim(name,pos):
 place('host',pos=pos);p=point(name);dx,dz=p['x']-pos[0],p['z']-pos[2]
 token('look',x=math.degrees(math.atan2(dx,dz)),y=math.degrees(math.atan2(pos[1]+1-p['y'],math.hypot(dx,dz))))
def action(name,pos,alt=False,torque=False):
 aim(name,pos);before=state()['revision'];shot(name.lower().replace(' ','-')+'-before')
 if name in ('Hood','Probes','Terminal','Lift battery','Socket','Stock shelf 0','Order register','Collection cart','Mixed chute'):
  token('preview',target=name.lower().replace(' ','-')+'-'+str(before))
 if torque:token('turn')
 elif alt:token('key',target='R')
 else:token('key',target='E')
 wait(lambda:state()['revision']>before);time.sleep(.9)
 shot(name.lower().replace(' ','-')+'-after')
def walk_into(name,pos,maximum_z):
 aim(name,pos);token('realinput',run=True);token('capturecursor');token('key',target='W',duration=1.1);token('realinput',run=False)
 p=read('host')['players'][0]['position'];check(name.lower().replace(' ','_')+'_blocks_real_W_movement',pos[2]+.15<p['z']<maximum_z);shot(name.lower().replace(' ','-')+'-collision')
try:
 for suffix in ('.json','.control.json'):(OUT/('host'+suffix)).unlink(missing_ok=True)
 processes['host']=subprocess.Popen([str(EXE),'-screen-fullscreen','0','-screen-width',str(args.width),'-screen-height',str(args.height),'-logFile',str(OUT/'host.log'),'--bau-role','host','--bau-evidence',str(OUT/'host.json'),'--bau-control',str(OUT/'host.control.json'),'--bau-rescue','--bau-ui-only'],cwd=EXE.parent)
 wait(lambda:read('host').get('rescueUI',{}).get('screen')=='Main',60);token('solo');wait(lambda:read('host').get('connected'));use('host','job:2');time.sleep(.8);token('key',target='Enter');token('key',target='Enter');wait(lambda:read('host')['rescueUI']['screen']=='HUD')
 check('one_actual_solo_player',len(read('host')['players'])==1 and wd()['physical']);check('authored_gloves_replace_old_fists',read('host')['rescueUI']['authoredHandsVisible']);walk_into('Hood',(6.4,.08,4.1),4.82);action('Hood',(6.4,.08,4.10));check('hood_open_from_real_E',wd()['hood'])
 action('Probes',(6.635,.08,4.58));check('visible_diagnosis_requests_supply',wd()['diagnosed'] and wd()['requested'])
 wait(lambda:wd()['dispatched'],22);check('same_parcel_arrives_by_labelled_service',item('PART-8A-001')['custody']=='garage' and conserved())
 action('Probes',(6.635,.08,4.58),alt=True)
 check('optional_load_test_one_clear_input',wd()['loadChecked'])
 action('Terminal',(6.68,.08,4.58),torque=True);check('real_mouse_turn_releases_clamp',wd()['released'])
 action('Lift battery',(6.635,.08,4.58));check('old_component_is_carried',item('OLD-8A-001')['custody']=='removed-carry')
 action('Scrap tray',(5.30,.08,7.0));check('old_component_in_scrap_tray',wd()['oldPlaced'])
 action('Replacement',(8.2,.08,6.30));check('replacement_carried_packaging_released',item('PART-8A-001')['custody']=='component-carry' and item('PACK-8A-001')['custody']=='garage-waste')
 walk_into('Replacement',(8.2,.08,6.3),7.10)
 action('Socket',(6.635,.08,4.58));check('replacement_seated_hood_stays_open',wd()['installed'] and wd()['hood'])
 action('Repair test',(6.635,.08,4.58));check('voltage_test_committed',wd()['repairTested'])
 action('Hood',(6.4,.08,4.10));check('hood_closed_after_test',not wd()['hood'])
 action('Service ticket',(7.65,.08,6.30));check('service_ticket_signed',wd()['serviceSigned'])
 wait(lambda:sum(i['custody']=='recorded' for i in wd()['items'])==3,45)
 action('Shift report',(0,.08,3.9));check('complete_solo_shift_filed',wd()['closed'] and conserved());shot('completed-shift')
except Exception as e:checks.append(dict(name='exception',passed=False,error=str(e)));traceback.print_exc();shot('failure') if read('host') else None
finally:
 (OUT/'checks.json').write_text(json.dumps(checks,indent=2))
 for r,p in processes.items():
  if p.poll() is None:
   try:cmd(r,'quit');p.wait(timeout=6)
   except Exception:p.terminate()
 print(f'{sum(c["passed"] for c in checks)}/{len(checks)} checks passed',flush=True)
 if any(not c['passed'] for c in checks):raise SystemExit(1)
