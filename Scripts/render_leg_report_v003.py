"""Compare every leg motion frame and all requested controlled poses."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Docs/Diagnostics/Phase2/v003';P=OUT/'Images'
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',20)
def pair(mode,view,f=1):
    out=Image.new('RGB',(920,600),'#20252b');d=ImageDraw.Draw(out)
    d.text((10,8),f'Previous v002 — {mode} f{f}',font=font,fill='white');d.text((470,8),'v003 leg repair',font=font,fill='white')
    for i,label in enumerate(['before','after']):out.paste(Image.open(P/f'{label}-{mode}-{view}-{f:03}.png').convert('RGB'),(i*460,40))
    return out
validation=json.loads((OUT/'leg-validation.json').read_text())
for mode,count in [('walk',26),('run',17)]:
    for view in ['front','side','three_quarter']:
        frames=[pair(mode,view,f) for f in range(1,count+1)]
        frames[0].save(OUT/f'{mode}-{view}-comparison.gif',save_all=True,append_images=frames[1:],duration=42,loop=0,optimize=False)
        cols=4;sheet=Image.new('RGB',(1600,((count+3)//4)*261),'#20252b')
        for i,im in enumerate(frames):sheet.paste(im.resize((400,261)),((i%4)*400,(i//4)*261))
        sheet.save(OUT/f'{mode}-{view}-all-frames.png')
    worst=sorted(set([int(validation['clips'][mode.title()]['worst_frame_by_after_strain']),int(validation['clips'][mode.title()]['worst_frame_by_before_strain'])]))
    for f in worst:
        out=Image.new('RGB',(1380,1200),'#20252b');d=ImageDraw.Draw(out)
        for i,view in enumerate(['front','side','three_quarter']):
            d.text((i*460+10,8),f'v002 {view} / {mode} f{f}',font=font,fill='white');d.text((i*460+10,610),f'v003 {view} / {mode} f{f}',font=font,fill='white')
            for row,label in enumerate(['before','after']):out.paste(Image.open(P/f'{label}-{mode}-{view}-{f:03}.png').convert('RGB'),(i*460,row*600+40))
        out.save(OUT/f'{mode}-worst-frame-{f:03}-comparison.png')
for mode in ['idle','hip_flexion','knee_bends','foot_lift','forward_step','crouch','full_rest']:
    out=Image.new('RGB',(2760,600),'#20252b')
    for i,view in enumerate(['front','side','three_quarter']):out.paste(pair(mode,view),(i*920,0))
    out.save(OUT/f'{mode}-comparison.png')
print(json.dumps({'walk_frames':26,'run_frames':17,'views':3,'controls':6,'full_rest_context':True}))
