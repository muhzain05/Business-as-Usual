"""Export the approved mockup drawing primitives as reusable Unity textures."""
from pathlib import Path
from PIL import Image, ImageDraw
import shutil
import render_handbook_mockups as art
ROOT=Path(__file__).resolve().parents[1]
DEST=ROOT/'Unity/BusinessAsUsual/Assets/Shopkeeper/UI/Resources/Handbook'
def canvas(w,h):
    p=art.Page(background=False)
    p.im=Image.new('RGBA',(w,h),(0,0,0,0));p.d=ImageDraw.Draw(p.im)
    return p
def save(p,name): p.im.save(DEST/(name+'.png'))
def main():
    DEST.mkdir(parents=True,exist_ok=True)
    for name,w,h,fill in [('identity',510,132,art.PAPER),('order-small',430,326,art.PAPER),
        ('task',510,175,art.PAPER),('revenue',372,140,art.PAPER),('carry',637,81,art.PAPER),
        ('prompt',792,134,art.PAPER),('prompt-compact',733,94,art.PAPER),('status',430,185,art.PAPER),('order',988,908,art.PAPER),
        ('book',1484,804,art.PAPER),('receipt-list',340,675,art.YELLOW),('receipt',735,938,art.PALE),
        ('receipt-card',285,111,art.PAPER),('summary',655,126,art.PALE)]:
        p=canvas(w+8,h+9);p.paper(0,0,w,h,fill)
        if name=='book':
            p.d.rectangle((708,15,724,789),fill=art.RULE)
            p.d.line((716,23,716,780),fill=art.INK,width=3)
            for y in [74,264,454,644]:p.d.ellipse((700,y,732,y+28),fill=art.PALE,outline=art.INK,width=3)
        if name=='receipt':
            for x in range(13,723,28):p.d.polygon([(x,934),(x+14,919),(x+28,934)],fill=art.INK)
        save(p,name)
    for sku in ['cola','tea','crackers']:
        p=canvas(64,80);p.item(0,0,sku);save(p,sku)
    p=canvas(90,95);p.bag(4,0);save(p,'bag')
    for label,w,angle in [('PAID',277,-5),('GOOD SHIFT',321,6)]:
        p=canvas(w+40,125);p.stamp(0,0,w,label,art.GREEN,angle);save(p,label.lower().replace(' ','-'))
    fonts=DEST/'Fonts';fonts.mkdir(exist_ok=True)
    for p in (ROOT/'Docs/UI/Phase4.2/Fonts').iterdir():
        if p.suffix in ['.ttf','.txt']:shutil.copy2(p,fonts/p.name)
    print('Exported approved original artwork and licensed fonts; no mockups modified.')
if __name__=='__main__':main()
