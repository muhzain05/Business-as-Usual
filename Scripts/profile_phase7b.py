"""Short matched two-player frame-interval sample; not GPU profiling or a human playtest."""
from pathlib import Path
import sys,json,time,statistics
q=None
if '--quality' in sys.argv:
    i=sys.argv.index('--quality');q=int(sys.argv[i+1]);del sys.argv[i:i+2]
root=Path(__file__).resolve().parents[1]
exec(compile((root/'Scripts/test_phase7a_network.py').read_text().split('\ntry:\n',1)[0],'phase7a-harness-prefix','exec'))
samples={'host':[],'client':[]};stamps={};original={}
try:
    launch('host');wait(lambda:read('host').get('listening'));launch('client');wait(lambda:len(read('host').get('players',[]))==2)
    for role,pos in [('host',(0,.08,-2)),('client',(-2,.08,-3))]:
        original[role]=dict(fp(role))
        place(role,pos=pos);look(role,0,-6)
        done(role,command(role,'settings',x=1,y=.12,z=0,run=False))
        done(role,command(role,'display',x=options.width,y=options.height,run=True))
        if q is not None:done(role,command(role,'quality',x=q))
    time.sleep(5)
    end=time.monotonic()+12
    while time.monotonic()<end:
        for role in samples:
            try:
                stamp=(OUT/(role+'.json')).stat().st_mtime_ns
            except OSError:
                # The player atomically replaces its report; a rename can briefly
                # leave no file for Windows stat. Sample the next published frame.
                continue
            if stamps.get(role)!=stamp:
                stamps[role]=stamp;samples[role].append(read(role)['frameMilliseconds'])
        time.sleep(.08)
    def summary(values):
        ordered=sorted(values)
        return dict(samples=len(values),meanMs=statistics.mean(values),medianMs=statistics.median(values),p95Ms=ordered[int(.95*(len(ordered)-1))])
    measured={r:[fp(r)['screenWidth'],fp(r)['screenHeight']] for r in samples}
    assert all(size==[options.width,options.height] for size in measured.values()),measured
    report=dict(method='Two actual Windows Development players, uncapped/vsync off, fixed square positions and view, 5 second warmup + 12 second sample. Unity editor remains open. Sampled Time.unscaledDeltaTime wall frame intervals, not isolated CPU/GPU duration or a release-build benchmark.',exe=str(EXE),resolution=[options.width,options.height],measuredResolution=measured,requestedQuality=q,measuredQuality={r:fp(r)['qualityLevel'] for r in samples},results={r:summary(v) for r,v in samples.items()},raw=samples)
    (OUT/'performance.json').write_text(json.dumps(report,indent=2));print(json.dumps(report['results']))
finally:
    for role,p in processes.items():
        if p.poll() is None:
            if role in original:
                old=original[role]
                done(role,command(role,'settings',x=old['listenerVolume'],y=old['sensitivity'],z=max(0,old['targetFrameRate']),run=old['vSyncCount']!=0))
                if q is not None:done(role,command(role,'quality',x=old['qualityLevel']))
            command(role,'quit')
            try:p.wait(timeout=8)
            except subprocess.TimeoutExpired:p.terminate()
