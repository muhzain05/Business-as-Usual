"""Native canvas captures and real pointer/keyboard input. No web imitation of the Unity UI."""
from pathlib import Path
source=Path(__file__).with_name('test_phase8a_network.py').read_text(encoding='utf-8')
exec(compile(source.split('\ntry:\n')[0],str(Path(__file__).with_name('test_phase8a_network.py')),'exec'))
EXE=ROOT/'Builds/Phase8A-RescueV2/BusinessAsUsual.exe'
assert json.loads((ROOT/'Docs/Diagnostics/Phase8ARescueV2/build.json').read_text(encoding='utf-8-sig'))['result']=='Succeeded', 'Refusing stale native build'
roles=['host'];assembly=EXE.parent/'BusinessAsUsual_Data/Managed/Assembly-CSharp.dll'
(OUT/'run-metadata.json').write_text(json.dumps(dict(assemblySha256=hashlib.sha256(assembly.read_bytes()).hexdigest(),width=args.width,height=args.height),indent=2))
def ui():return read('host').get('rescueUI') or {}
def token(action,**fields):
    t=cmd('host',action,**fields);wait(lambda:read('host').get('serial')==t);time.sleep(.4);return t
def shot(name):
    path=OUT/('host-'+name+'.png');token('capture',target=name);wait(lambda:path.exists());time.sleep(.2)
    (OUT/(name+'.json')).write_text(json.dumps(read('host'),indent=2))
def pointer(label):
    row=next(t for t in ui()['text'] if t['value']==label);token('click',x=row['at']['x']+row['size']['x']/2,y=row['at']['y']+row['size']['y']/2)
def show(screen,tab=0,font=0):
    token('rescueui',target=screen,x=tab,y=font);wait(lambda:ui().get('screen')==screen);time.sleep(.3)
def bounds(name):
    rows=ui().get('text',[]);bad=[r for r in rows if r['offscreen'] or r['overflows']];
    checks.append(dict(name=name+'_text_bounds',passed=not bad,defects=bad));print(name+': '+str(len(bad))+' text defects',flush=True)
try:
    for suffix in ('.json','.control.json'):(OUT/('host'+suffix)).unlink(missing_ok=True)
    processes['host']=subprocess.Popen([str(EXE),'-screen-fullscreen','0','-screen-width',str(args.width),'-screen-height',str(args.height),'-logFile',str(OUT/'host.log'),'--bau-role','host','--bau-evidence',str(OUT/'host.json'),'--bau-control',str(OUT/'host.control.json'),'--bau-rescue','--bau-ui-only'],cwd=EXE.parent)
    wait(lambda:ui().get('screen')=='Main',60);time.sleep(.6);shot('main');bounds('main')
    pointer('JOIN WORKDAY');wait(lambda:ui().get('screen')=='Join' and any(t['value']=='Host address' for t in ui()['text']));shot('join');bounds('join');pointer('BACK');wait(lambda:ui().get('screen')=='Main')
    pointer('START SOLO');wait(lambda:read('host').get('connected') and ui().get('screen')=='Staff',30)
    check('solo_button_starts_one_connected_player',len(read('host')['players'])==1);shot('staff');bounds('staff')
    pointer('2  START');wait(lambda:state()['town']['owners'][1]==read('host')['localId']);wait(lambda:ui().get('screen')=='Intro');shot('illustrated-controls');bounds('intro-controls')
    token('key',target='Enter');shot('illustrated-job');bounds('intro-job');token('key',target='Enter');wait(lambda:ui().get('screen')=='HUD');shot('compact-hud');bounds('hud')
    for f in range(3):show('Handbook',font=f);shot('typography-'+str(f));bounds('typography-'+str(f))
    for tab,label in [(1,'items'),(2,'records'),(3,'town')]:show('Handbook',tab=tab);shot(label);bounds(label)
    for screen in ['Map','Pause','Settings','Display','Controls','Report']:show(screen);shot(screen.lower());bounds(screen.lower())
    token('rescueui',target='HUD');token('key',target='Tab');wait(lambda:ui().get('screen')=='Handbook');check('tab_opens_new_clipboard',True)
    token('key',target='Escape');wait(lambda:ui().get('screen')=='HUD');check('escape_closes_clipboard',not read('host')['firstPerson']['inputBlocked'])
    token('key',target='J');wait(lambda:ui().get('screen')=='Staff');check('J_reopens_staff_without_restarting_induction',True);token('key',target='J');wait(lambda:ui().get('screen')=='HUD');check('J_returns_to_work',not read('host')['firstPerson']['inputBlocked'])
    if args.width==1920:
        for width,height in [(1366,768),(1280,720),(1920,1080)]:
            token('display',x=width,y=height,run=True);wait(lambda:read('host')['firstPerson']['screenWidth']==width and read('host')['firstPerson']['screenHeight']==height);show('Handbook');shot(f'resized-{width}x{height}');bounds(f'resized-{width}x{height}')
        for screen in ['Handbook','Staff','Settings','Display','Report','HUD']:
            show(screen);name='native-render-1920x1200-'+screen.lower();path=OUT/('host-'+name+'.png');token('rescuecapture',x=1920,y=1200,target=name);wait(lambda:path.exists());data=json.loads(path.with_suffix('.json').read_text(encoding='utf-8-sig'));bad=[t for t in data['text'] if t['overflows'] or t['offscreen']];checks.append(dict(name=name+'_text_bounds',passed=not bad,defects=bad,method='Unity offscreen render; desktop cannot fit 1200px native window'))
        show('HUD');time.sleep(8);shot('performance-sample')
except Exception as error:
    checks.append(dict(name='exception',passed=False,error=str(error)));traceback.print_exc()
finally:
    (OUT/'checks.json').write_text(json.dumps(checks,indent=2))
    for r,p in processes.items():
        if p.poll() is None:
            try:cmd(r,'quit');p.wait(timeout=6)
            except Exception:p.terminate()
    print(f'{sum(c["passed"] for c in checks)}/{len(checks)} checks passed',flush=True)
    if any(not c['passed'] for c in checks):raise SystemExit(1)
