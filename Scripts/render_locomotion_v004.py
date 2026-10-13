"""Two complete loops at 24 fps, with synchronized normalized-cycle comparisons."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Docs/Diagnostics/Phase2/v004';P=OUT/'Images';font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
manifest={'fps':24,'loops_encoded':2,'comparison_sync':'Normalized cycle phase; v003 source clips are time-scaled to v004 duration.','previews':[]}
for mode,N,views in [('walk',32,['side','front']),('run',24,['side','three_quarter'])]:
    for view in views:
        solo=[];pairs=[]
        for f in range(1,N+1):
            a=Image.open(P/f'v003-{mode}-{view}-{f:03}.png').convert('RGB');b=Image.open(P/f'v004-{mode}-{view}-{f:03}.png').convert('RGB')
            im=Image.new('RGB',(480,680),'#20252b');im.paste(b,(0,40));ImageDraw.Draw(im).text((8,10),f'v004 {mode} | {view} | frame {f}',font=font,fill='white');solo.append(im)
            pair=Image.new('RGB',(960,680),'#20252b');pair.paste(a,(0,40));pair.paste(b,(480,40));d=ImageDraw.Draw(pair);d.text((8,10),f'v003 | cycle phase {(f-1)/N:.2f}',font=font,fill='white');d.text((488,10),f'v004 {mode} | frame {f}',font=font,fill='white');pairs.append(pair)
        durations=[round((i+1)*1000/24/10)*10-round(i*1000/24/10)*10 for i in range(2*N)]
        for kind,frames in [('preview',solo),('comparison',pairs)]:
            seq=frames+frames;name=f'{mode}-{view}-{kind}-two-loops.gif';seq[0].save(OUT/name,save_all=True,append_images=seq[1:],duration=durations,loop=0,optimize=False)
            manifest['previews'].append({'file':name,'frames':2*N,'duration_ms':sum(durations),'loops_encoded':2})
        sheet=Image.new('RGB',(1440,((N+2)//3)*204),'#20252b')
        for i,im in enumerate(pairs):sheet.paste(im.resize((480,204)),((i%3)*480,(i//3)*204))
        sheet.save(OUT/f'{mode}-{view}-all-frames.png')
(OUT/'preview-manifest.json').write_text(json.dumps(manifest,indent=2));print(json.dumps(manifest))
