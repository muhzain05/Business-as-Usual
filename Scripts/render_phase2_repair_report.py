"""Compose all-frame GIFs and fixed-camera comparison sheets from Blender renders."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Docs/Diagnostics/Phase2/v002';P=OUT/'Images'
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',22)
small=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',17)
def row(mode,view='front',frame=1):
    a=Image.open(P/f'before-{mode}-{view}-{frame:03}.png').convert('RGB');b=Image.open(P/f'after-{mode}-{view}-{frame:03}.png').convert('RGB')
    out=Image.new('RGB',(1000,705),'#20252b');out.paste(a,(0,55));out.paste(b,(500,55));d=ImageDraw.Draw(out)
    d.text((15,10),f'Original / {mode} / frame {frame}',font=font,fill='white');d.text((515,10),'v002 prototype — quality not passed',font=font,fill='white');return out
for mode in ['rest','jacket']:
    out=Image.new('RGB',(1500,1370),'#20252b');d=ImageDraw.Draw(out)
    for x,view in enumerate(['front','side','back']):
        d.text((x*500+10,10),f'{view.upper()} — original',font=font,fill='white');d.text((x*500+10,690),'v002 repair prototype',font=font,fill='white')
        out.paste(Image.open(P/f'before-{mode}-{view}-001.png').convert('RGB'),(x*500,40))
        out.paste(Image.open(P/f'after-{mode}-{view}-001.png').convert('RGB'),(x*500,720))
    out.save(OUT/f'{mode}-front-side-back-comparison.png')
for mode,count in [('walk',26),('run',17)]:
    frames=[row(mode,frame=f) for f in range(1,count+1)]
    frames[0].save(OUT/f'{mode}-full-comparison.gif',save_all=True,append_images=frames[1:],duration=[42 if f%3 else 41 for f in range(count)],loop=0,optimize=False)
    # Every frame, including failures, stays on the review sheet.
    thumbs=[]
    for f,img in enumerate(frames,1):
        thumb=img.resize((400,282));thumbs.append(thumb)
    cols=4;sheet=Image.new('RGB',(cols*400,((count+cols-1)//cols)*282),'#20252b')
    for i,img in enumerate(thumbs):sheet.paste(img,((i%cols)*400,(i//cols)*282))
    sheet.save(OUT/f'{mode}-all-frames-sheet.png')
    frames[19 if mode=='walk' else 8].save(OUT/f'{mode}-selected-comparison.png')
out=Image.new('RGB',(2000,1410),'#20252b')
for k,name in enumerate(['arm_raise','crouch','leg_bend','ankle_flex']):
    pair=Image.new('RGB',(1000,705),'#20252b');d=ImageDraw.Draw(pair);d.text((10,10),f'{name}: original / prototype',font=font,fill='white')
    for i,label in enumerate(['before','after']):pair.paste(Image.open(P/f'{label}-controlled-{name}.png').convert('RGB'),(i*500,55))
    out.paste(pair,((k%2)*1000,(k//2)*705))
out.save(OUT/'controlled-poses-comparison.png')
print(json.dumps({'comparison_sheets':7,'walk_gif_frames':26,'run_gif_frames':17,'fixed_camera':True}))
