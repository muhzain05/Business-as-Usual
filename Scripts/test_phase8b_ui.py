"""Actual native menu pointer flows, loading, hub, private practice and persistent sound settings."""
from pathlib import Path
source=Path(__file__).with_name('test_phase8a_rescue_ui.py').read_text(encoding='utf-8-sig').split('\ntry:\n')[0]
exec(compile(source,str(Path(__file__).with_name('test_phase8a_rescue_ui.py')),'exec'))
EXE=ROOT/'Builds/Phase8B/BusinessAsUsual.exe';assembly=EXE.parent/'BusinessAsUsual_Data/Managed/Assembly-CSharp.dll'
(OUT/'run-metadata.json').write_text(json.dumps(dict(assemblySha256=hashlib.sha256(assembly.read_bytes()).hexdigest(),executable=str(EXE),width=args.width,height=args.height),indent=2))
def launch():
 for suffix in ('.json','.control.json'):(OUT/('host'+suffix)).unlink(missing_ok=True)
 latest.clear();processes['host']=subprocess.Popen([str(EXE),'-screen-fullscreen','0','-screen-width',str(args.width),'-screen-height',str(args.height),'-logFile',str(OUT/'host.log'),'--bau-role','host','--bau-evidence',str(OUT/'host.json'),'--bau-control',str(OUT/'host.control.json'),'--bau-phase8b','--bau-rescue','--bau-ui-only'],cwd=EXE.parent)
 wait(lambda:ui().get('screen')=='Main',60);time.sleep(.5)
def minus(row):
 y=next(t for t in ui()['text'] if t['value']==row)['at']['y'];options=[t for t in ui()['text'] if t['value'] in ('−','-')];t=min(options,key=lambda t:abs(t['at']['y']-y));token('click',x=t['at']['x']+t['size']['x']/2,y=t['at']['y']+t['size']['y']/2)
try:
 launch();shot('main-menu');bounds('main-menu');check('six_real_menu_actions',all(any(t['value']==v for t in ui()['text']) for v in ['START SOLO','HOST GAME','JOIN GAME','SETTINGS','CREDITS','QUIT']))
 pointer('CREDITS');wait(lambda:ui()['screen']=='Credits');shot('credits');bounds('credits');pointer('BACK');pointer('SETTINGS');wait(lambda:ui()['screen']=='Settings');shot('settings');bounds('settings')
 previous={k:read('host')[k] for k in ['master','music','effects']};token('settings',x=.8,y=.7,z=.7);show('Settings');before={k:read('host')[k] for k in previous};minus('Music');minus('Sound effects');expected={k:read('host')[k] for k in previous};check('separate_music_and_effects_controls',expected['music']<before['music'] and expected['effects']<before['effects'])
 cmd('host','quit');processes['host'].wait(timeout=8);launch();check('sound_preferences_persist_across_processes',all(abs(read('host')[k]-expected[k])<.001 for k in expected));token('settings',x=previous['master'],y=previous['music'],z=previous['effects'])
 pointer('START SOLO');wait(lambda:ui()['screen']=='Loading');shot('loading');bounds('loading');wait(lambda:read('host').get('connected') and ui()['screen']=='Staff',45);shot('profession-selection');bounds('profession-selection')
 pointer('1  START');wait(lambda:ui()['screen']=='Intro');shot('role-intro');bounds('role-intro');token('key',target='Enter');shot('job-preview');token('key',target='Enter');wait(lambda:state()['day']['started'],30);wait(lambda:ui()['screen']=='Private');shot('private-practice');bounds('private-practice');check('solo_is_one_worker_and_labelled_practice',len(read('host')['players'])==1 and read('host')['dayPrivate']['practice'])
 token('key',target='Enter');wait(lambda:ui()['screen']=='HUD');shot('day-hud');bounds('day-hud');token('look',x=180,y=0);shot('named-turn-guidance');check('destination_names_replace_ambiguous_distance_only_pin',any(t['value'] in ['MARKET ENTRY','OPEN MARKET','OPEN SHOP DOOR'] or 'MARKET' in t['value'] for t in ui()['text']))
 token('key',target='J');wait(lambda:ui()['screen']=='Staff');shot('current-shift-crew');bounds('current-shift-crew');check('staff_desk_explains_fixed_day_job',any(t['value']=='Your shift crew' for t in ui()['text']));pointer('SHARED COPIES');wait(lambda:ui()['screen']=='Board');shot('shared-board-access');bounds('shared-board-access');pointer('BACK');wait(lambda:ui()['screen']=='HUD')
 for screen in ['Handbook','Pause','Settings','Display']:
  show(screen);shot(screen.lower());bounds(screen.lower())
except Exception as e:checks.append(dict(name='exception',passed=False,error=str(e)));traceback.print_exc()
finally:
 (OUT/'checks.json').write_text(json.dumps(checks,indent=2))
 for r,p in processes.items():
  if p.poll() is None:
   try:cmd(r,'quit');p.wait(timeout=6)
   except Exception:p.terminate()
 print(f'{sum(c["passed"] for c in checks)}/{len(checks)} checks passed',flush=True)
 if any(not c['passed'] for c in checks):raise SystemExit(1)
