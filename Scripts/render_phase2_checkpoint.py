"""Measured reconstruction plan and rest comparison; no Blender geometry edits."""
import json, math, argparse
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageChops, ImageStat

ROOT=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser()
parser.add_argument('--version',default='v001')
args=parser.parse_args()
OUT=ROOT/'Docs/Diagnostics/Phase2'/args.version
inventory=json.loads((OUT/'modular-inventory.json').read_text(encoding='utf-8'))
source=json.loads((ROOT/'Docs/Diagnostics/component-geometry.json').read_text(encoding='utf-8'))
scale=inventory['normalization']['scale']
verts=[[p[0]*scale,p[1]*scale,p[2]*scale+.6] for p in source['vertices']]
faces=source['faces']; ids=source['component_by_vertex']
underside_path=OUT/'shorts-underside-review.json'
underside=json.loads(underside_path.read_text(encoding='utf-8')) if underside_path.exists() else {}
shorts=next(m for m in inventory['modules'] if m['semantic_name']=='Shorts')
cap_faces={shorts['original_polygon_ids'][i] for i in underside.get('candidate_underside_polygons',[])}
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',20)
small=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',17)
title=ImageFont.truetype('C:/Windows/Fonts/arialbd.ttf',30)

image=Image.new('RGB',(1600,1250),(21,28,39)); draw=ImageDraw.Draw(image)
draw.text((30,20),'Sunny Phase 2: proposed reconstruction boundaries',font=title,fill='white')
draw.text((30,65),'Planning diagram only. No new body surfaces or rig changes have been built.',font=font,fill=(187,202,219))
draw.text((30,105),'Cyan: retained exposed skin     Orange: proposed hidden surfaces     Magenta: measured join boundaries',font=font,fill=(207,218,232))

def dashed(points,fill,width=3,dash=10):
    for a,b in zip(points,points[1:]):
        length=math.dist(a,b)
        if length<1: continue
        n=math.ceil(length/dash)
        for i in range(0,n,2):
            t=i/n; u=min((i+1)/n,1)
            draw.line((a[0]+(b[0]-a[0])*t,a[1]+(b[1]-a[1])*t,
                       a[0]+(b[0]-a[0])*u,a[1]+(b[1]-a[1])*u),fill=fill,width=width)

def panel(view,x):
    draw.text((x+80,165),view.upper()+' (Z up)',font=font,fill='white')
    factor=580; cx=x+260; cy=990
    def xy(p): return (cx+(p[0] if view=='front' else p[1])*factor,cy-p[2]*factor)
    depth_axis=1 if view=='front' else 0
    for fi in sorted(range(len(faces)),key=lambda fi:sum(verts[i][depth_axis] for i in faces[fi]),reverse=view=='front'):
        f=faces[fi]
        cid=ids[f[0]]
        color=(72,98,110) if cid in [3,4,7,8] else (55,62,76) if cid in [0,1,2,5,12,13,14] else (82,82,93)
        if cid in [3,4,7,8]: color=(37,147,161)
        if fi in cap_faces: color=(177,124,60)
        draw.polygon([xy(verts[i]) for i in f],fill=color)
    for region in inventory['proposed_regions']:
        if 'bounds' in region:
            box=region['bounds']; axis=0 if view=='front' else 1
            p1=xy(box['min']); p2=xy(box['max'])
            rect=[(min(p1[0],p2[0]),min(p1[1],p2[1])),(max(p1[0],p2[0]),min(p1[1],p2[1])),
                  (max(p1[0],p2[0]),max(p1[1],p2[1])),(min(p1[0],p2[0]),max(p1[1],p2[1]))]
            dashed(rect+[rect[0]],(255,173,83))
        elif 'start' in region:
            dashed([xy(region['start']),xy(region['end'])],(255,173,83),width=9,dash=12)
    for module in inventory['modules']:
        if module['id'] not in [3,4,7,8]: continue
        for loop in module['boundary_clusters']:
            for a,b in loop['edges']:
                va=module['original_vertex_ids'][a]; vb=module['original_vertex_ids'][b]
                draw.line([xy(verts[va]),xy(verts[vb])],fill=(244,115,212),width=3)
    foot=[-.133,.038,.114] if view=='front' else [0,.038,.114]
    # Proposed concealed foot envelope is an outline, not a generated mesh.
    for side in [-1,1] if view=='front' else [1]:
        xvalues=[.04*side,.20*side] if view=='front' else [-.15,.10]
        p1=xy([min(xvalues),0,.04] if view=='front' else [0,min(xvalues),.04])
        p2=xy([max(xvalues),0,.16] if view=='front' else [0,max(xvalues),.16])
        dashed([(p1[0],p1[1]),(p2[0],p1[1]),p2,(p1[0],p2[1]),p1],(255,173,83),width=2)
    return xy

front=panel('front',20); side=panel('side',600)
callouts=[('1  Torso + short neck', [0,0,.66], 330),
          ('2  Pelvis inside shorts', [0,0,.38], 555),
          ('3  Sleeve-hidden upper arms', [.20,.04,.59], 430),
          ('4  Covered thighs to knee rings', [.10,.03,.32], 665),
          ('5  Ankle / concealed foot joins', [.13,.04,.13], 805)]
for text,point,y in callouts:
    draw.text((1120,y),text,font=font,fill=(255,188,108))
    target=side(point)
    draw.line([(1105,y+12),(1070,y+12),target],fill=(160,128,94),width=2)
    draw.ellipse((target[0]-4,target[1]-4,target[0]+4,target[1]+4),fill=(255,173,83))
draw.text((35,1040),'Join rings: forearm/hand 24 R / 26 L vertices; lower legs 24 at each end.',font=font,fill=(243,144,215))
draw.text((35,1080),'Retain current garment, head, hands, shoes and visible leg silhouette. Fit reconstruction beneath them.',font=font,fill=(196,211,229))
draw.text((35,1120),'Boxes and dashed lines are approximate work envelopes, not final anatomical surfaces or cut instructions.',font=font,fill=(196,211,229))
draw.text((35,1160),'Amber shorts underside: review existing caps for local cuff openings; this is not a deletion mask.',font=font,fill=(230,188,121))
draw.text((35,1200),'The head remains closed and separate. Shoe boundaries need local inspection before sealing.',font=small,fill=(196,211,229))
image.save(OUT/'reconstruction-boundaries.png')

before=Image.open(OUT/'before-rest-views.png').convert('RGB')
after=Image.open(OUT/'after-extraction-rest-views.png').convert('RGB')
assert before.size==after.size
w,h=before.size; halfw=w//2; halfh=h//2
tiles=[(0,0,halfw,halfh),(halfw,0,w,halfh),(0,halfh,halfw,h)]
comparison=Image.new('RGB',(1500,1150),(21,28,39)); pen=ImageDraw.Draw(comparison)
pen.text((25,15),'Original appearance vs extracted assembly — rest pose',font=title,fill='white')
pen.text((25,60),'Same camera distances and view directions. Extraction only; weights and deformation remain unrepaired.',font=small,fill=(194,209,226))
metrics=[]
for i,box in enumerate(tiles):
    b=before.crop(box).resize((480,480)); a=after.crop(box).resize((480,480))
    comparison.paste(b,(10+i*500,130)); comparison.paste(a,(10+i*500,660))
    pen.text((20+i*500,100),'Before: '+['front','side','back'][i],font=font,fill='white')
    pen.text((20+i*500,630),'After extraction: '+['front','side','back'][i],font=font,fill='white')
    stats=ImageStat.Stat(ImageChops.difference(before.crop(box),after.crop(box)))
    metrics.append({'view':['front','side','back'][i],'mean_absolute_rgb_difference_0_255':sum(stats.mean)/3})
comparison.save(OUT/'before-after-extraction.png')
(OUT/'rest-image-comparison.json').write_text(json.dumps({'purpose':'same-view rest appearance comparison; not repair validation',
    'views':metrics,'note':'Rendering variation and normal quantization can change pixels; inspect visually as well.'},indent=2),encoding='utf-8')
print(json.dumps(metrics,indent=2))

clips=Image.new('RGB',(1500,560),(21,28,39)); pen=ImageDraw.Draw(clips)
pen.text((25,15),'Walk / run extraction checkpoint — existing defects retained',font=title,fill='white')
pen.text((25,60),'Upper outfit, pelvis and leg distortion is unchanged. These are compatibility comparisons, not repaired motion.',font=small,fill=(194,209,226))
labels=['Walk: reference / frame 20','Walk: extracted / frame 20','Run: reference / frame 9','Run: extracted / frame 9']
for i,mode in enumerate(['walk_before','walk_after','run_before','run_after']):
    path=OUT/(mode+'.png')
    if not path.exists(): continue
    # Preserve aspect rather than stretching the source screenshot.
    im=Image.open(path).convert('RGB'); im.thumbnail((365,365))
    x=i*375+(375-im.width)//2
    clips.paste(im,(x,150+(365-im.height)//2))
    pen.text((i*375+10,110),labels[i],font=small,fill='white')
clips.save(OUT/'walk-run-extraction-comparison.png')
