"""Native pointer settings persistence, resize, host hub, leave/restart and unreachable join."""
from pathlib import Path
s=Path(__file__).with_name('test_phase8b_ui.py');exec(compile(s.read_text(encoding='utf-8-sig').split('\ntry:\n')[0],str(s),'exec'))
def value(row):
 y=next(t for t in ui()['text'] if t['value']==row)['at']['y'];return next(t['value'] for t in ui()['text'] if abs(t['at']['y']-y)<3 and t['value'] and t['value'][0].isdigit())
def label(prefix):return next(t['value'] for t in ui()['text'] if t['value'].startswith(prefix))
def plus(row):
 y=next(t for t in ui()['text'] if t['value']==row)['at']['y'];t=min((t for t in ui()['text'] if t['value']=='+'),key=lambda t:abs(t['at']['y']-y));token('click',x=t['at']['x']+t['size']['x']/2,y=t['at']['y']+t['size']['y']/2)
def adjust(row,want):
 for _ in range(24):
  v=float(value(row).rstrip('\u00b0'))
  if abs(v-want)<.001:return
  (minus if v>want else plus)(row)
 raise AssertionError('Could not restore '+row)
try:
 launch();shot('main-menu');bounds('main-menu');pointer('SETTINGS');adjust('Look sensitivity',.12);adjust('Field of view',92);old={'sensitivity':value('Look sensitivity'),'fov':value('Field of view'),'motion':label('REDUCED MOTION')};minus('Look sensitivity');minus('Field of view');pointer(old['motion']);new={'sensitivity':value('Look sensitivity'),'fov':value('Field of view'),'motion':label('REDUCED MOTION')};check('look_fov_motion_controls_change',all(new[k]!=old[k] for k in old));pointer('DISPLAY & QUALITY');oldquality=label('QUALITY /');pointer(oldquality);newquality=label('QUALITY /');check('quality_control_changes_preset',newquality!=oldquality);shot('display-settings');bounds('display-settings');pointer('BACK')
 cmd('host','quit');processes['host'].wait(timeout=8);launch();pointer('SETTINGS');check('look_fov_motion_persist_across_processes',value('Look sensitivity')==new['sensitivity'] and value('Field of view')==new['fov'] and label('REDUCED MOTION')==new['motion']);pointer('DISPLAY & QUALITY');check('quality_preference_persists',label('QUALITY /')==newquality)
 # Real native resize and fullscreen transition, then return to a compact window.
 pointer('1366 \u00d7 768');time.sleep(1);shot('resized-1366');bounds('resized-1366');pointer(label('MODE /'));time.sleep(1);shot('fullscreen');bounds('fullscreen');check('fullscreen_button_changes_mode',label('MODE /')=='MODE / FULLSCREEN');pointer(label('MODE /'));pointer('1280 \u00d7 720');time.sleep(1);check('windowed_button_changes_mode',label('MODE /')=='MODE / WINDOWED')
 for i in range(10):
  if label('QUALITY /')==oldquality:break
  pointer(label('QUALITY /'))
 check('quality_restored',label('QUALITY /')==oldquality);pointer('BACK');pointer(new['motion'])
 # Restore the one-step changes through real plus buttons.
 for row in ['Look sensitivity','Field of view']:
  y=next(t for t in ui()['text'] if t['value']==row)['at']['y'];t=min((t for t in ui()['text'] if t['value']=='+'),key=lambda t:abs(t['at']['y']-y));token('click',x=t['at']['x']+t['size']['x']/2,y=t['at']['y']+t['size']['y']/2)
 pointer('BACK');pointer('HOST GAME');wait(lambda:read('host').get('connected') and ui()['screen']=='Staff',45);pointer('2  START');wait(lambda:ui()['screen']=='Intro');token('key',target='Enter');token('key',target='Enter');wait(lambda:ui()['screen']=='Hub');shot('staff-hub');bounds('staff-hub');pointer('OPEN THE DAY');wait(lambda:ui()['screen']=='Private');shot('private-mechanic');bounds('private-mechanic');token('key',target='Enter');wait(lambda:ui()['screen']=='HUD')
 # Actual pause action shuts down networking and loads the main menu.
 token('key',target='Escape');wait(lambda:ui()['screen']=='Pause');shot('pause');pointer('LEAVE WORKDAY');wait(lambda:ui()['screen']=='Main',35);shot('returned-main-menu');bounds('returned-main-menu');check('return_to_menu_cleans_network_session',not read('host').get('connected'))
 pointer('START SOLO');wait(lambda:read('host').get('connected') and ui()['screen']=='Staff',45);check('fresh_solo_can_start_after_host_return',len(read('host')['players'])==1 and not state()['day']['started']);token('leave');wait(lambda:ui()['screen']=='Main',35)
 pointer('JOIN GAME');wait(lambda:ui()['screen']=='Join');shot('join-address');bounds('join-address');pointer('JOIN GAME');wait(lambda:ui()['screen'] in ['ConnectionError','Main'],25);shot('unreachable-host');check('unreachable_join_does_not_strand_player',ui()['screen'] in ['ConnectionError','Main']);
 if ui()['screen']=='ConnectionError':pointer('BACK TO MENU');wait(lambda:ui()['screen']=='Main',30)
except Exception as e:checks.append(dict(name='exception',passed=False,error=str(e)));traceback.print_exc()
finally:
 (OUT/'checks.json').write_text(json.dumps(checks,indent=2))
 for r,p in processes.items():
  if p.poll() is None:
   try:cmd(r,'quit');p.wait(timeout=6)
   except Exception:p.terminate()
 print(f'{sum(c["passed"] for c in checks)}/{len(checks)} checks passed',flush=True)
 if any(not c['passed'] for c in checks):raise SystemExit(1)
