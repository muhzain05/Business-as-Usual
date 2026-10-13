"""Original deterministic retail graphics; no downloaded or generated assets."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import random

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Unity/BusinessAsUsual/Assets/Shopkeeper/Textures'
OUT.mkdir(parents=True, exist_ok=True)
def font(n,bold=True):
    return ImageFont.truetype('C:/Windows/Fonts/'+('arialbd.ttf' if bold else 'arial.ttf'),n)
def center(d,y,t,f,c):
    box=d.textbbox((0,0),t,font=f);d.text(((512-(box[2]-box[0]))/2,y),t,font=f,fill=c)
brands=[('Sunbeam','COLA','#af342d','#fbe4a8','sun'),('Meadow','GREEN TEA','#247d69','#f0e8ba','leaf'),('Golden','CRACKERS','#e8b33b','#6e3c27','grain'),('Cloud','OAT MILK','#80c6d2','#fbefcf','cloud'),('Orchard','PEACH SODA','#ed9584','#ffe7a8','fruit'),('Tide','SEA SALT','#355d9c','#e8efd7','wave'),('Little Fox','COCOA','#90583e','#f5cf87','fox'),('Garden','TOMATO SOUP','#ba4634','#d5e8ad','fruit')]
for i,(brand,kind,bg,fg,motif) in enumerate(brands):
    im=Image.new('RGB',(512,512),bg);d=ImageDraw.Draw(im)
    d.rectangle((16,16,496,496),outline=fg,width=5)
    d.rectangle((0,34,512,77),fill=fg);center(d,40,'DAYLIGHT MARKET',font(24),'#313e38')
    center(d,92,brand.upper(),font(49),fg);center(d,152,kind,font(28),fg)
    if motif=='sun':
        for a in range(12):
            import math
            x,y=256+105*math.cos(a*math.pi/6),292+105*math.sin(a*math.pi/6)
            d.line((256,292,x,y),fill=fg,width=7)
        d.ellipse((181,217,331,367),fill=fg);d.arc((208,238,307,330),0,180,fill=bg,width=6)
    elif motif=='leaf':
        for x,y in [(170,300),(240,270),(285,315)]:d.ellipse((x,y-55,x+75,y+45),fill=fg);d.line((x+10,y+32,x+65,y-38),fill=bg,width=4)
    elif motif=='grain':
        for x in [200,256,312]:
            d.line((x,340,x,223),fill=fg,width=5)
            for y in [240,270,300]:d.ellipse((x-27,y-20,x,y+4),fill=fg);d.ellipse((x,y-28,x+27,y-4),fill=fg)
    elif motif=='cloud':
        for x,y,r in [(200,290,45),(255,255,65),(318,290,50)]:d.ellipse((x-r,y-r,x+r,y+r),fill=fg)
        d.rectangle((168,287,352,340),fill=fg)
    elif motif=='fox':
        d.polygon([(170,222),(233,254),(280,254),(346,222),(334,331),(256,371),(181,331)],fill=fg)
        d.ellipse((215,295,227,307),fill=bg);d.ellipse((287,295,299,307),fill=bg);d.polygon([(243,326),(269,326),(256,338)],fill=bg)
    elif motif=='wave':
        import math
        for y in [260,290,320]:d.line([(x,y+20*math.sin(x/48)) for x in range(110,405)],fill=fg,width=9)
    else:
        d.ellipse((180,224,333,372),fill=fg);d.polygon([(250,244),(269,199),(312,206)],fill='#3c754a');d.arc((205,253,308,348),-60,70,fill=bg,width=5)
    center(d,390,'FRESH • LOCAL • EVERYDAY',font(20),fg)
    center(d,426,'250 ml' if i!=2 else 'CRISP & BUTTERY',font(22),fg)
    rng=random.Random(i)
    for x in range(166,346,5):d.rectangle((x,465,x+rng.choice([1,2,3]),489),fill=fg)
    im.save(OUT/(brand.replace(' ','')+'.png'))
# Tile pattern and wood grain are purpose-built, seamless, subtle surfaces.
im=Image.new('RGB',(512,512),'#b48761');d=ImageDraw.Draw(im);rng=random.Random(18)
for y in range(512):
    if y%128==0:d.line((0,y,512,y),fill='#72593e',width=3)
    else:
        c=rng.randint(-9,9);d.line((0,y,512,y),fill=(180+c,135+c,97+c))
for y,x in [(0,190),(128,370),(256,80),(384,290)]:d.line((x,y,x,y+128),fill='#806143',width=2)
im.save(OUT/'OakFloor.png')
print('Created 9 original retail textures')
