"""Render measured mesh components without editing Blender geometry."""
import json, math, colorsys
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parents[1]
data=json.loads((ROOT/'Docs/Diagnostics/component-geometry.json').read_text())
verts=data['vertices']; faces=data['faces']; ids=data['component_by_vertex']
labels=['Upper clothing (jacket/shirt/collar)','Right shoe','Left shoe','Right forearm + hand',
        'Left forearm + hand','Shorts','Head shell','Right exposed leg','Left exposed leg',
        'Mouth','Right eye','Left eye','Top button','Middle button','Bottom button']
W,H=1600,1550
image=Image.new('RGB',(W,H),(23,29,40)); draw=ImageDraw.Draw(image)
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
title=ImageFont.truetype('C:/Windows/Fonts/arialbd.ttf',25)
draw.text((25,15),'Sunny: 15 connected components — measured source geometry',font=title,fill='white')
draw.text((25,52),'Orthographic front projections; each panel auto-fitted. Colors identify components, not source materials.',font=font,fill=(190,201,216))
def paint(selected, box, color_by_component=True):
    eligible=[f for f in faces if selected is None or ids[f[0]]==selected]
    used=[verts[i] for f in eligible for i in f]
    xmin,xmax=min(p[0] for p in used),max(p[0] for p in used)
    zmin,zmax=min(p[2] for p in used),max(p[2] for p in used)
    x,y,w,h=box; scale=min(w/max(xmax-xmin,.001),h/max(zmax-zmin,.001))*.9
    def xy(p): return (x+w/2+(p[0]-(xmin+xmax)/2)*scale,y+h/2-(p[2]-(zmin+zmax)/2)*scale)
    for f in sorted(eligible,key=lambda f:sum(verts[i][1] for i in f)/len(f),reverse=True):
        cid=ids[f[0]]; hue=(cid*.61803398875)%1
        color=colorsys.hsv_to_rgb(hue,.5,.85)
        a,b,c=[verts[i] for i in f]
        u=[b[j]-a[j] for j in range(3)]; v=[c[j]-a[j] for j in range(3)]
        normal=[u[1]*v[2]-u[2]*v[1],u[2]*v[0]-u[0]*v[2],u[0]*v[1]-u[1]*v[0]]
        mag=math.sqrt(sum(n*n for n in normal)) or 1
        light=.55+.45*abs((normal[1]*-.7+normal[2]*.7)/mag)
        draw.polygon([xy(p) for p in (a,b,c)],fill=tuple(int(255*c*light) for c in color))
for panel in range(16):
    x=(panel%4)*400; y=100+(panel//4)*355
    draw.rectangle((x+8,y+5,x+392,y+345),outline=(58,69,84),width=1)
    cid=panel-1 if panel else None
    text='All components' if cid is None else f'{cid}: {labels[cid]}'
    draw.text((x+20,y+17),text,font=font,fill='white')
    if cid is not None: draw.text((x+20,y+42),f'{ids.count(cid):,} vertices',font=font,fill=(190,201,216))
    paint(cid,(x+20,y+75,360,250))
image.save(ROOT/'Docs/Diagnostics/connected-components.png')
print('Saved connected-components.png')
