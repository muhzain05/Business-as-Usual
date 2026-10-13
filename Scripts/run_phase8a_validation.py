"""Sequential Windows tests; never overlap suites that share player preferences."""
from pathlib import Path
import json,subprocess,sys
root=Path(__file__).resolve().parents[1];out=root/'Docs/Diagnostics/Phase8A';out.mkdir(parents=True,exist_ok=True)
exe='Builds/Phase8A/BusinessAsUsual.exe'
suites=[
 ('FourProcess720','test_phase8a_network.py',['--width','1280','--height','720']),
 ('FourProcess1080SanitationAssistant','test_phase8a_network.py',['--width','1920','--height','1080','--assistant-profession','3']),
 ('RecoveryMechanicAssistant','test_phase8a_network.py',['--scenario','recovery','--assistant-profession','2']),
 ('LegacyPhase6Full','test_phase6_network.py',['--exe',exe,'--scenario','full']),
 ('LegacyPhase6Staffed','test_phase6_network.py',['--exe',exe,'--scenario','staffed']),
 ('LegacyPhase6Entry','test_phase6_network.py',['--exe',exe,'--scenario','entry']),
 ('LegacyGuidance','test_phase6_guidance.py',['--exe',exe]),
 ('LegacyPhase7A','test_phase7a_network.py',['--exe',exe]),
 ('LegacyPhase7B','test_phase7b_network.py',['--exe',exe])]
results=[]
for name,script,extra in suites:
    print('START '+name,flush=True)
    with (out/(name+'-console.log')).open('w') as log:
        run=subprocess.run([sys.executable,str(root/'Scripts'/script),'--output',str(out/name),*extra],cwd=root,stdout=log,stderr=subprocess.STDOUT)
    results.append(dict(suite=name,exitCode=run.returncode));(out/'suite-exits.json').write_text(json.dumps(results,indent=2))
    print('FINISH '+name+' exit='+str(run.returncode),flush=True)
    if run.returncode:
        print((out/(name+'-console.log')).read_text()[-2500:],flush=True)
        break
sys.exit(0 if len(results)==len(suites) and all(r['exitCode']==0 for r in results) else 1)
