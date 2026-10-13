"""Create a local, portable review gallery from the recorded Phase 6 evidence."""
from pathlib import Path
from html import escape
import json, statistics, math

root = Path(__file__).resolve().parents[1] / 'Docs/Diagnostics/Phase6'
samples = json.loads((root/'Staffed1080/frame-samples.json').read_text())
metrics = {}
for role in ('host', 'client'):
    values = sorted(s[role] for s in samples if s.get(role) is not None)
    metrics[role] = dict(samples=len(values), medianMs=statistics.median(values),
                         p95Ms=values[math.ceil(.95*len(values))-1], minMs=min(values), maxMs=max(values))
(root/'performance-observations.json').write_text(json.dumps(dict(
    hardware=dict(cpu='Intel Core i7-11700F',gpu='NVIDIA GeForce RTX 3060 Ti',physicalMemoryBytes=33983561728),
    method='30 reported frame samples per process over approximately 4.8 seconds, two windowed 1920x1080 Development players and Unity Editor running on the same PC. Timing observations, not GPU profiling or shipping benchmarks.',
    processes=metrics), indent=2))

parts = ['''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width">
<title>Daylight Town — Phase 6 review</title><style>
body{background:#163235;color:#f5e9cc;font:17px/1.5 system-ui;margin:0 auto;max-width:1450px;padding:32px}h1,h2{color:#f3cd72}a{color:#f3cd72}img{display:block;width:100%;height:auto;border-radius:5px}figure{margin:0}figcaption{padding:8px 0}.pair,.cards{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:18px;margin:20px 0 40px}.cards{grid-template-columns:repeat(3,minmax(0,1fr))}section{margin-top:40px}small{color:#c3d1c1}@media(max-width:850px){.pair,.cards{grid-template-columns:1fr}}button{font:inherit;background:#f3cd72;border:0;padding:8px 16px;cursor:pointer}dialog{max-width:96vw;background:#163235;border:0;padding:12px}dialog img{max-height:85vh;width:auto;max-width:94vw}dialog::backdrop{background:#000b}
</style><h1>Daylight Town</h1><p>Phase 6: Shopkeeper, Mechanic and Sanitation in a shared two-player neighborhood.</p>
<p><a href="../../phase6-testing.md">Play guide</a> · <a href="../../phase6-validation.md">Validation report</a> · <a href="../../phase6-environment.md">Environment notes</a></p>
<p>Matched original and improved town views at 1280×720. Street and workplace captures use a camera 1.7 m above the ground; the overhead pair uses the same 32 m viewpoint. Click any image to enlarge.</p>''']
def figure(path, label):
    return f'<figure><img loading="lazy" src="{escape(path)}" alt="{escape(label)}" onclick="show(this)"><figcaption>{escape(label)}</figcaption></figure>'
for view in ('street','square','shopfront','garage','yard','alley','overhead'):
    parts.append(f'<section><h2>{view.title()}</h2><div class="pair">'+figure(f'Before/{view}.png','Before')+figure(f'After/{view}.png','After')+'</div></section>')
parts.append('<section><h2>Three professions — 1920×1080</h2><small>Actual game-process captures. Automated gameplay; no human usability claim.</small><div class="cards">')
for label, prefix in (('Shopkeeper','host-shopkeeper'),('Mechanic','host-mechanic'),('Sanitation','client-sanitation')):
    for state in ('hud','handbook','inventory','records','report'):
        path=f'TwoProcess1080/{prefix}-{state}.png'
        if (root/path).exists():parts.append(figure(path, f'{label} / {state}'))
parts.append('</div></section><section><h2>Two-player navigation</h2><div class="pair">')
for role in ('host','client'):
    parts.append(figure(f'Staffed1080/{role}-walk-square.png',f'{role.title()} / square traversal'))
parts.append('</div></section><section><h2>Automated checks</h2><ul>')
for relative in ('Regression/Phase3/playmode-validation.json','Regression/Phase4/playmode-validation.json','Regression/Phase4.1/polish-validation.json','Regression/Phase4.2/ui-validation.json','Regression/Phase5/validation.json','TwoProcess1080/validation.json','Staffed1080/validation.json','MarketEntry720/validation.json'):
    file=root/relative
    if file.exists():
        report=json.loads(file.read_text(encoding='utf-8-sig')); checks=report['checks']
        parts.append(f'<li><a href="{relative}">{relative.split("/")[-2]}</a>: {sum(c["passed"] for c in checks)}/{len(checks)} passed</li>')
parts.append('</ul><p>Frame observations: '+ '; '.join(f'{role} median {m["medianMs"]:.2f} ms, p95 {m["p95Ms"]:.2f} ms' for role,m in metrics.items())+'. Short development-run observations on one PC; draw calls and shipping performance still need profiler measurements.</p></section>')
parts.append('''<dialog id="zoom"><button onclick="this.parentElement.close()">Close</button><img id="large" alt=""></dialog><script>function show(el){large.src=el.src;large.alt=el.alt;zoom.showModal()}zoom.addEventListener('click',e=>{if(e.target===zoom)zoom.close()})</script></html>''')
(root/'review.html').write_text('\n'.join(parts),encoding='utf-8')
print(json.dumps(metrics,indent=2))
print('Created Docs/Diagnostics/Phase6/review.html')
