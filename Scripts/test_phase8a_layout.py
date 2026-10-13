"""Real Windows staff-desk captures at four resolutions, using the existing opt-in driver."""
from pathlib import Path
shared=Path(__file__).with_name('test_phase8a_network.py')
exec(compile(shared.read_text().split('\ntry:\n',1)[0],str(shared),'exec'))
from PIL import Image
try:
    launch('host');wait(lambda:read('host').get('connected'),60)
    for width,height in [(1280,720),(1920,1080),(1366,768),(1280,800)]:
        cmd('host','display',run=True,x=width,y=height);wait(lambda:read('host')['firstPerson']['screenWidth']==width and read('host')['firstPerson']['screenHeight']==height)
        capture('host',f'staff-{width}x{height}')
        path=OUT/f'host-staff-{width}x{height}.png'
        with Image.open(path) as image:check(f'native_staff_desk_{width}x{height}',image.size==(width,height))
except Exception:
    checks.append(dict(name='exception',passed=False,error=traceback.format_exc()));traceback.print_exc()
finally:
    (OUT/'checks.json').write_text(json.dumps(checks,indent=2))
    for role,p in processes.items():
        if p.poll() is None:
            try:cmd(role,'quit');p.wait(timeout=8)
            except Exception:p.terminate()
    if any(not c['passed'] for c in checks):raise SystemExit(1)
