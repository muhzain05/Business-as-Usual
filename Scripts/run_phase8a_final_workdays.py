from pathlib import Path
import subprocess,sys,json
root=Path(__file__).resolve().parents[1];out=root/'Docs/Diagnostics/Phase8A'
cases=[('FinalFour720',[]),('FinalFour1080SanitationAssistant',['--width','1920','--height','1080','--assistant-profession','3']),('FinalRecoveryMechanicAssistant',['--scenario','recovery','--assistant-profession','2'])]
results=[]
for name,extra in cases:
    print('START '+name,flush=True)
    with (out/(name+'-console.log')).open('w') as log:run=subprocess.run([sys.executable,str(root/'Scripts/test_phase8a_network.py'),'--output',str(out/name),*extra],cwd=root,stdout=log,stderr=subprocess.STDOUT)
    results.append(dict(suite=name,exitCode=run.returncode));(out/'final-suite-exits.json').write_text(json.dumps(results,indent=2))
    print('FINISH '+name+' exit='+str(run.returncode),flush=True)
    if run.returncode:print((out/(name+'-console.log')).read_text()[-2500:],flush=True);break
sys.exit(0 if len(results)==3 and all(r['exitCode']==0 for r in results) else 1)
