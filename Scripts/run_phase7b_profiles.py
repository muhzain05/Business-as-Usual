from pathlib import Path
import subprocess,sys
root=Path(__file__).resolve().parents[1]
for name,exe,w,h,q in [('Before1080','Phase7A',1920,1080,None),('After1080High','Phase7B',1920,1080,1),('After720Performance','Phase7B',1280,720,0)]:
    command=[sys.executable,str(root/'Scripts/profile_phase7b.py'),'--exe',str(root/f'Builds/{exe}/BusinessAsUsual.exe'),'--output',str(root/f'Docs/Diagnostics/Phase7B/Performance/{name}'),'--width',str(w),'--height',str(h),'--port','7794']
    if q is not None:command+=['--quality',str(q)]
    result=subprocess.run(command,cwd=root)
    if result.returncode:sys.exit(result.returncode)
