"""Final actual-process regression suite; isolates test ports from ordinary play."""
from pathlib import Path
import subprocess,sys,json,time
root=Path(__file__).resolve().parents[1]
out=root/'Docs/Diagnostics/Phase6Addendum'
suites=[('TownFull1080','test_phase6_network.py',['--scenario','full','--width','1920','--height','1080']),('Staffed1080','test_phase6_network.py',['--scenario','staffed','--width','1920','--height','1080']),('MarketEntry720','test_phase6_network.py',['--scenario','entry']),('Guidance720','test_phase6_guidance.py',[])]
results=[]
for name,script,flags in suites:
 print('Running '+name,flush=True);start=time.monotonic()
 result=subprocess.run([sys.executable,str(root/'Scripts'/script),'--output',str(out/name),*flags],cwd=root)
 results.append(dict(suite=name,exitCode=result.returncode,seconds=time.monotonic()-start))
 (out/'process-suite-results.json').write_text(json.dumps(results,indent=2))
 if result.returncode:sys.exit(result.returncode)
sys.exit(0)
