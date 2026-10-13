"""Sequential suites: windows players share preferences, so never run these concurrently."""
from pathlib import Path
import subprocess,json,sys,time
root=Path(__file__).resolve().parents[1]
out=root/'Docs/Diagnostics/Phase7B/Regression'
town='Builds/Phase7B/BusinessAsUsual.exe'
suites=[
 ('Targeted1080','test_phase7b_network.py',['--output','Docs/Diagnostics/Phase7B/Targeted1080','--width','1920','--height','1080','--port','7789']),
 ('Targeted720','test_phase7b_network.py',['--output','Docs/Diagnostics/Phase7B/Targeted720','--width','1280','--height','720','--port','7790']),
 ('Phase7A720','test_phase7a_network.py',['--width','1280','--height','720','--port','7787']),
 ('Phase7A1080','test_phase7a_network.py',['--width','1920','--height','1080','--port','7788']),
 ('Phase6Full','test_phase6_network.py',['--scenario','full']),
 ('Phase6Staffed','test_phase6_network.py',['--scenario','staffed']),
 ('Phase6Entry','test_phase6_network.py',['--scenario','entry']),
 ('Phase6Guidance','test_phase6_guidance.py',[]),
 ('Phase5Controlled','test_phase5_phase7b.py',[])]
results=[];out.mkdir(parents=True,exist_ok=True)
for name,script,extra in suites:
    command=[sys.executable,str(root/'Scripts'/script),*extra]
    if not name.startswith('Targeted'):
        command+=['--output',str(out/name),'--exe',str(root/('Builds/Phase7B-Phase5Regression/BusinessAsUsual.exe' if name=='Phase5Controlled' else town))]
    print('START '+name,flush=True)
    with (out/(name+'-console.log')).open('w') as log:run=subprocess.run(command,cwd=root,stdout=log,stderr=subprocess.STDOUT)
    results.append(dict(suite=name,exitCode=run.returncode));(out/'suite-exits.json').write_text(json.dumps(results,indent=2))
    print('FINISH '+name+' exit='+str(run.returncode),flush=True)
    if run.returncode:print((out/(name+'-console.log')).read_text()[-1800:],flush=True)
    time.sleep(1)
sys.exit(0 if all(r['exitCode']==0 for r in results) else 1)
