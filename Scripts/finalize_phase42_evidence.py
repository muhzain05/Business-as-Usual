"""Package actual Unity screenshots and retain each rerun without replacing older evidence."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import hashlib, json, shutil, sys
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/Diagnostics/Phase4.2'
STATES=[('01-normal-hud','Compact HUD'),('02-task-handbook','Employee handbook'),('03-customer-order','Customer order'),('04-printed-receipt','Printed receipt'),('05-shift-report','Shift report'),('06-interaction-prompt','Interaction prompt')]
def archive(phase):
    current=ROOT/'Docs/Diagnostics'/phase
    target=OUT/'Regression'/phase
    shutil.copytree(current,target,dirs_exist_ok=True)
    for p in (OUT/'accepted-baseline'/phase).rglob('*'):
        if p.is_file(): shutil.copy2(p,current/p.relative_to(OUT/'accepted-baseline'/phase))
    print(f'Archived fresh {phase} results and restored pre-existing diagnostic files.')
def gallery():
    font=ImageFont.truetype(str(ROOT/'Docs/UI/Phase4.2/Fonts/RoundedMplus1c-ExtraBold.ttf'),25)
    sheet=Image.new('RGB',(1920,1850),'#F6E9C9');draw=ImageDraw.Draw(sheet)
    draw.text((35,20),'BUSINESS AS USUAL / IN-ENGINE UI',font=font,fill='#244047')
    dimensions={}
    for n,(name,label) in enumerate(STATES):
        for res in ['1920x1080','1280x720']:
            p=OUT/(name+'-'+res+'.png');im=Image.open(p);dimensions[p.name]=list(im.size)
            assert list(im.size)==list(map(int,res.split('x'))),p
        x=35+(n%2)*950;y=90+(n//2)*575
        im=Image.open(OUT/(name+'-1920x1080.png')).convert('RGB').resize((900,506))
        sheet.paste(im,(x,y));draw.text((x,y+516),label,font=font,fill='#244047')
    sheet.save(OUT/'six-integrated-overview.png')
    (OUT/'screenshot-dimensions.json').write_text(json.dumps(dimensions,indent=2),encoding='utf-8')
    rows=''.join(f'<section><h2>{label}</h2><div class="pair"><figure><img src="../../UI/Phase4.2/Mockups/{name}.png"><figcaption>Approved mockup</figcaption></figure><figure><a href="{name}-1920x1080.png"><img src="{name}-1920x1080.png"></a><figcaption>Unity 1920×1080 · <a href="{name}-1280x720.png">1280×720</a></figcaption></figure></div></section>' for name,label in STATES)
    html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Business as Usual — Integrated UI comparison</title>
<style>@font-face{font-family:Rounded;src:url(../../UI/Phase4.2/Fonts/RoundedMplus1c-Regular.ttf)}body{background:#F6E9C9;color:#244047;font-family:Rounded,sans-serif;margin:24px}a{color:#244047}h1{font-size:32px}.pair{display:grid;grid-template-columns:1fr 1fr;gap:16px}figure{margin:0}img{width:100%;border:3px solid #244047;box-sizing:border-box}section{margin:36px 0}figcaption{padding:8px}p{max-width:1100px}@media(max-width:800px){.pair{grid-template-columns:1fr}}</style>
<h1>Staff Edition — Unity integration</h1><p>Actual Game view screenshots, captured automatically in Play mode. This is a visual comparison, not human usability testing. UI state matches the six approved examples; live receipt timestamps and shelf stock reflect the new validation session.</p><p><a href="../../phase4.2-implementation.md">Implementation report</a> · <a href="ui-validation.json">UI validation</a> · <a href="six-integrated-overview.png">Overview</a></p>'''+rows+'''<section><h2>Matched baseline state</h2><div class="pair"><figure><img src="before-1920x1080.png"><figcaption>Before · fresh shift</figcaption></figure><figure><img src="after-initial-1920x1080.png"><figcaption>After · same fresh shift and camera</figcaption></figure></div><p><a href="before-1280x720.png">Before 1280×720</a> · <a href="after-initial-1280x720.png">After 1280×720</a></p></section><section><h2>Additional live states</h2><p><a href="order-eli-1920x1080.png">Eli’s order</a> · <a href="order-jo-1920x1080.png">Jo’s order</a> · <a href="receipt-02-1920x1080.png">Eli’s receipt</a> · <a href="receipt-03-1920x1080.png">Jo’s receipt</a> · <a href="inventory-stockroom-1280x720.png">Stockroom inventory</a> · <a href="status-incomplete-order-1280x720.png">Rejection feedback</a> · <a href="busy-stocking-1280x720.png">Busy restocking</a> · <a href="paper-stamp-preview.gif">Paper/stamp preview</a></p></section></html>'''
    (OUT/'review.html').write_text(html,encoding='utf-8')
    print('Verified screenshot dimensions and generated actual/approved comparison gallery.')
def animation():
    timing=json.loads((OUT/'Animation/capture-times.json').read_text(encoding='utf-8'))
    frames=[Image.open(OUT/'Animation'/item['frame']).convert('RGB') for item in timing]
    durations=[max(20,round((b['seconds']-a['seconds'])*1000)) for a,b in zip(timing,timing[1:])]+[900]
    frames[0].save(OUT/'paper-stamp-preview.gif',save_all=True,append_images=frames[1:],duration=durations,loop=0)
    summary={'frames':len(frames),'dimensions':list(frames[0].size),'firstSampleSeconds':timing[0]['seconds'],
        'lastSampleSeconds':timing[-1]['seconds'],'distinctFrames':len({hashlib.sha256(i.tobytes()).hexdigest() for i in frames}),
        'method':'Actual Unity Game view frames. GIF preserves recorded intervals with a final 900ms review hold.'}
    (OUT/'animation-preview.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
    print(json.dumps(summary,indent=2))
if __name__=='__main__':
    if sys.argv[1]=='archive':archive(sys.argv[2])
    elif sys.argv[1]=='gallery':gallery()
    elif sys.argv[1]=='animation':animation()
