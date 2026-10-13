"""Offline, original UI review artwork. Does not import or replace Unity UI."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageEnhance
import json, math, random

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Docs/UI/Phase4.2'
OUT.mkdir(parents=True, exist_ok=True)
MOCK = OUT / 'Mockups'
MOCK.mkdir(exist_ok=True)
FONTS = OUT / 'Fonts'
PAPER = '#F6E9C9'
INK = '#244047'
GREEN = '#649B76'
YELLOW = '#EFC864'
CORAL = '#CE735A'
PALE = '#FFF7E5'
RULE = '#D4C7A8'
font_files = {'display': 'DelaGothicOne-Regular.ttf', 'body': 'RoundedMplus1c-Regular.ttf',
              'bold': 'RoundedMplus1c-ExtraBold.ttf', 'hand': 'YuseiMagic-Regular.ttf'}
font_cache = {}
def font(size, role='body'):
    key = (size, role)
    if key not in font_cache: font_cache[key] = ImageFont.truetype(str(FONTS / font_files[role]), size)
    return font_cache[key]

class Page:
    def __init__(self, background=True, dim=False):
        if background:
            path = ROOT / 'Docs/Diagnostics/Phase4.1/after-shelf-world.png'
            self.im = Image.open(path).convert('RGB').resize((1920,1080))
            if dim: self.im = ImageEnhance.Brightness(self.im).enhance(.54)
        else: self.im = Image.new('RGB',(1920,1080),PAPER)
        self.d = ImageDraw.Draw(self.im)
        self.bounds = []
    def text(self,x,y,value,size=28,role='body',color=INK,align='left',limit=None):
        f=font(size,role)
        width=self.d.textlength(value,font=f)
        if limit is not None and width>limit:
            raise ValueError(f'Overflow: {value!r} {width:.1f}>{limit}')
        if align=='right': x-=width
        if align=='center': x-=width/2
        self.d.text((x,y),value,font=f,fill=color)
        return width
    def line(self,x,y,w,color=RULE,width=2,dashed=False):
        if dashed:
            for dx in range(0,w,20): self.d.line((x+dx,y,x+min(dx+10,w),y),fill=color,width=width)
        else:self.d.line((x,y,x+w,y),fill=color,width=width)
    def paper(self,x,y,w,h,fill=PAPER,tab=None):
        self.d.rectangle((x+7,y+8,x+w+7,y+h+8),fill=INK)
        self.d.rounded_rectangle((x,y,x+w,y+h),radius=9,fill=fill,outline=INK,width=4)
        # A quiet paper grain only inside the opaque card.
        rng=random.Random(x+y+w)
        for _ in range(w*h//3800):
            xx=rng.randint(x+10,x+w-10); yy=rng.randint(y+10,y+h-10)
            self.d.point((xx,yy),fill=RULE)
        if tab:
            tab_width=max(158,math.ceil(self.d.textlength(tab,font=font(18,'bold')))+26)
            self.d.rectangle((x+22,y-17,x+22+tab_width,y+12),fill=YELLOW,outline=INK,width=3)
            self.text(x+22+tab_width/2,y-15,tab,18,'bold',align='center')
        self.bounds.append([x,y,w,h])
    def pill(self,x,y,w,text,fill=YELLOW,h=44):
        self.d.rounded_rectangle((x,y,x+w,y+h),radius=7,fill=fill,outline=INK,width=3)
        self.text(x+w/2,y+5,text,22,'bold',align='center',limit=w-18)
    def stamp(self,x,y,w,label,color=GREEN,angle=-6):
        stamp=Image.new('RGBA',(w,80),(0,0,0,0));d=ImageDraw.Draw(stamp)
        d.rounded_rectangle((3,3,w-4,72),radius=6,outline=color,width=5)
        d.rectangle((10,10,w-11,65),outline=color,width=1)
        f=font(32,'display'); tw=d.textlength(label,font=f)
        d.text(((w-tw)/2,15),label,font=f,fill=INK)
        stamp=stamp.rotate(angle,resample=Image.Resampling.BICUBIC,expand=True)
        self.im.paste(stamp,(x,y),stamp)
        self.d=ImageDraw.Draw(self.im)
    def key(self,x,y,key):
        self.d.rounded_rectangle((x+3,y+4,x+61,y+60),radius=8,fill=INK)
        self.d.rounded_rectangle((x,y,x+58,y+56),radius=7,fill=PALE,outline=INK,width=3)
        self.text(x+29,y+5,key,32,'bold',align='center')
    def item(self,x,y,kind,scale=1):
        def pt(a,b):return(x+int(a*scale),y+int(b*scale))
        if kind=='cola':
            self.d.rounded_rectangle((*pt(6,0),*pt(49,72)),radius=7,fill=CORAL,outline=INK,width=3)
            self.d.ellipse((*pt(6,0),*pt(49,10)),fill=PALE,outline=INK,width=2)
            self.d.ellipse((*pt(17,26),*pt(39,48)),fill=YELLOW,outline=INK,width=2)
            for a in range(0,360,45):
                self.d.line((pt(28+15*math.cos(math.radians(a)),37+15*math.sin(math.radians(a))),pt(28+20*math.cos(math.radians(a)),37+20*math.sin(math.radians(a)))),fill=INK,width=2)
        elif kind=='tea':
            self.d.polygon([pt(8,15),pt(47,15),pt(52,74),pt(3,74)],fill=GREEN,outline=INK,width=3)
            self.d.rectangle((*pt(14,0),*pt(41,15)),fill=PAPER,outline=INK,width=3)
            self.d.ellipse((*pt(18,32),*pt(38,53)),fill=PAPER,outline=INK,width=2)
            self.d.line((pt(22,48),pt(33,35)),fill=INK,width=2)
        else:
            self.d.rectangle((*pt(1,0),*pt(55,73)),fill=YELLOW,outline=INK,width=3)
            self.d.line((pt(1,10),pt(55,10)),fill=INK,width=2)
            for xx,yy in [(15,27),(32,44),(13,52)]:
                self.d.rectangle((*pt(xx,yy),*pt(xx+15,yy+13)),fill=PALE,outline=INK,width=2)
    def bag(self,x,y):
        self.d.arc((x+13,y,x+63,y+50),180,360,fill=INK,width=4)
        self.d.polygon([(x+5,y+25),(x+72,y+25),(x+81,y+88),(x-4,y+88)],fill=YELLOW,outline=INK,width=4)
        self.d.ellipse((x+24,y+43,x+49,y+69),fill=CORAL,outline=INK,width=3)
        for dx in [-18,18]:self.d.line((x+36+dx,y+55,x+36+dx*1.4,y+55),fill=INK,width=3)
    def footer(self,label):
        # Explicit review marker, outside gameplay-critical information.
        self.d.rectangle((0,1044,1920,1080),fill=INK)
        self.text(28,1049,'BUSINESS AS USUAL  /  UI PROTOTYPE',17,'bold',PAPER)
        self.text(1890,1049,label,17,'body',PAPER,'right')
    def save(self,name):self.im.save(MOCK/name)

def hud(p,stage='stock',prompt=True):
    p.paper(48,44,510,132)
    p.bag(70,65)
    p.text(174,60,'Business as Usual',32,'display',limit=365)
    p.text(177,115,'DAYLIGHT MARKET  /  STAFF EDITION',18,'bold',limit=360)
    p.paper(1442,44,430,326,tab='ORDER SLIP')
    p.text(1466,65,'Mara',38,'display')
    p.text(1468,124,'Morning break  •  S1-001',23)
    p.line(1464,167,384)
    for yy,kind,name,count in [(189,'cola','Sunbeam Cola','0 / 2' if stage=='stock' else '1 / 2'),(267,'crackers','Golden Crackers','0 / 1')]:
        p.item(1468,yy,kind,.66)
        p.text(1524,yy-2,name,24,'bold',limit=242)
        p.text(1524,yy+32,'IN BASKET',17,'body')
        p.text(1848,yy+27,count,27,'bold',align='right')
    p.paper(48,815,510,175,tab='ON YOUR SHIFT')
    p.text(73,835,'Restock the shelves' if stage=='stock' else 'Fill Mara’s basket',30,'display',limit=460)
    p.text(75,892,'Cola 0/6   Tea 0/6   Crackers 0/6' if stage=='stock' else '2 Cola + 1 Crackers  •  customer 1/3',24)
    p.text(75,942,'1/6 tasks complete' if stage=='stock' else '4/6 tasks complete',22,'bold')
    p.text(533,942,'Tab Receipts',20,'body',align='right')
    p.paper(1500,850,372,140)
    p.text(1525,868,'SHIFT REVENUE',19,'bold')
    p.text(1525,900,'$0.00',38,'display')
    p.text(1844,945,'0 receipts',21,align='right')
    if prompt:
        p.paper(637,922,733,94)
        p.key(657,940,'E')
        p.text(737,939,'Restock Sunbeam Cola' if stage=='stock' else 'Collect Sunbeam Cola',30,'bold',limit=603)
        p.text(738,977,'Place 6 units  •  Q returns the crate' if stage=='stock' else 'Shelf 5/6  •  Basket 1/2  •  collect 1 unit',20)

def book(p,title,subtitle,active='TASKS'):
    p.paper(218,136,1484,804,fill=PAPER)
    p.d.rectangle((926,151,942,925),fill=RULE)
    p.d.line((934,159,934,916),fill=INK,width=3)
    for y in [210,400,590,780]:
        p.d.ellipse((918,y,950,y+28),fill=PALE,outline=INK,width=3)
    p.text(265,166,'DAYLIGHT MARKET  /  STAFF EDITION',22,'bold')
    p.text(265,208,title,44,'display',limit=635)
    p.text(266,278,subtitle,24)
    p.line(262,332,625,INK,3)
    for x,name in [(356,'TASKS'),(676,'INVENTORY'),(996,'RECEIPTS'),(1316,'SHIFT REPORT')]:
        p.d.rounded_rectangle((x,98,x+266,140),radius=5,fill=YELLOW if name==active else PAPER,outline=INK,width=3)
        p.text(x+133,102,name,22,'bold',align='center')
    p.text(1653,889,'Tab Close  •  Esc Release mouse',22,align='right')

def render():
    p=Page();hud(p)
    p.paper(637,44,637,81)
    p.text(662,61,'CARRYING',20,'bold')
    p.text(805,61,'Cola 6  •  Tea 6  •  Crackers 6',24,'bold')
    p.text(662,91,'18 units in the stock crate',18)
    p.footer('Normal HUD  /  receiving complete, before stocking')
    p.save('01-normal-hud.png')

    p=Page(dim=True);book(p,'Your shift, on paper.','Receive. Restock. Serve. Leave it tidy.')
    tasks=[('01','Receive one sealed delivery','1 / 1',True),('02','Restock Sunbeam Cola','0 / 6',False),('03','Restock Meadow Tea','0 / 6',False),('04','Restock Golden Crackers','0 / 6',False),('05','Fulfill customer orders','0 / 3',False),('06','Review receipts & close shift','0 / 1',False)]
    for i,(num,label,count,done) in enumerate(tasks):
        y=366+i*72
        p.d.rounded_rectangle((260,y,882,y+62),radius=5,fill=PALE,outline=RULE,width=2)
        p.pill(272,y+10,47,num,PAPER if done else YELLOW,h=42)
        p.text(335,y+14,label,24,'bold',limit=415)
        p.text(863,y+14,count,24,'bold',align='right')
        if done:p.d.line((335,y+39,640,y+39),fill=GREEN,width=2)
    p.text(984,180,'Inventory card',39,'display')
    p.text(984,240,'COUNTED SHIFT STOCK ONLY',20,'bold')
    for x,kind,name in [(1108,'cola','Cola'),(1317,'tea','Tea'),(1525,'crackers','Crackers')]:
        p.item(x+44,309,kind,1.3)
        p.text(x+80,415,name,26,'bold',align='center')
    cols=[1054,1275,1496]
    for i,(label,counts) in enumerate([('Delivered',[6,6,6]),('In crate',[6,6,6]),('On shelf',[0,0,0]),('In basket',[0,0,0]),('Sold',[0,0,0])]):
        y=472+i*59;p.line(980,y-7,646)
        p.text(989,y,label,24,'bold')
        for x,c in zip([1188,1397,1605],counts):p.text(x,y,str(c),27,'bold',align='center')
    p.text(990,797,'Q returns the remaining crate stock.',23,'hand')
    p.text(990,839,'Decorative assortment is not inventory.',21)
    p.footer('Expanded handbook  /  existing task and inventory data')
    p.save('02-task-handbook.png')

    p=Page(dim=True)
    p.paper(466,80,988,908,tab='CUSTOMER ORDER')
    p.text(514,113,'One for the morning break.',39,'display',limit=890)
    p.text(517,180,'MARA  /  ORDER S1-001  /  CUSTOMER 1 OF 3',24,'bold')
    p.line(512,239,894,INK,3)
    p.text(590,261,'ITEM',20,'bold');p.text(1138,261,'IN BASKET',20,'bold',align='right');p.text(1399,261,'LINE VALUE',20,'bold',align='right')
    for y,kind,name,count,price,line in [(335,'cola','Sunbeam Cola','1 / 2','$1.80 each','$3.60'),(496,'crackers','Golden Crackers','0 / 1','$1.50 each','$1.50')]:
        p.item(523,y,kind,1.4);p.text(623,y+8,name,32,'bold',limit=458);p.text(623,y+62,price,25)
        p.text(1138,y+35,count,34,'bold',align='right');p.text(1399,y+35,line,32,'bold',align='right');p.line(513,y+126,892)
    p.text(517,677,'ORDER VALUE',24,'bold');p.text(1398,658,'$5.10',56,'display',align='right')
    p.pill(517,753,326,'NEEDS 2 MORE ITEMS',YELLOW,h=51)
    p.text(519,837,'Collect 1 Cola and 1 Crackers at their shelves.',27,'hand')
    p.text(519,900,'Bring the complete basket to Checkout. E prints the receipt.',23,limit=890)
    p.footer('Customer order slip  /  values before payment, quantities exact')
    p.save('03-customer-order.png')

    p=Page(dim=True)
    p.paper(286,182,340,675,fill=YELLOW,tab='RECEIPT BOOK')
    p.text(312,211,'Printed today',30,'display',limit=287)
    p.text(314,270,'3 unique receipts',23)
    for i,(who,val,ident) in enumerate([('Mara','$5.10','BAU-0001'),('Eli','$5.20','BAU-0002'),('Jo','$6.20','BAU-0003')]):
        y=338+i*142;p.paper(310,y,285,111,fill=PALE if i==0 else PAPER)
        p.text(330,y+13,ident,22,'bold');p.text(330,y+49,who,26,'bold');p.text(575,y+53,val,25,'bold',align='right')
    p.text(314,798,'Tab Close receipt book',21)
    p.paper(687,62,735,938,fill=PALE)
    p.bag(715,88);p.text(835,97,'DAYLIGHT',43,'display',limit=560);p.text(835,154,'NEIGHBORHOOD MARKET',23,'bold')
    p.line(725,213,657,INK,2,dashed=True)
    p.text(727,236,'RECEIPT BAU-0001',25,'bold');p.text(727,283,'Order S1-001  •  Mara / morning break',25)
    # Receipt timestamp uses the original accepted validation fixture, explicitly UTC.
    report=json.loads((ROOT/'Docs/Diagnostics/Phase4.1/accepted-baseline/Phase4/playmode-validation.json').read_text())
    rec=report['summary']['receipts'][0]
    p.text(727,328,rec['timestampUtc'][:19].replace('T','  ')+' UTC',22)
    p.line(725,380,657,INK,2,dashed=True)
    p.text(727,410,'QTY / ITEM',20,'bold');p.text(1380,410,'AMOUNT',20,'bold',align='right')
    for y,name,qty,unit,total in [(463,'Sunbeam Cola',2,'$1.80','$3.60'),(578,'Golden Crackers',1,'$1.50','$1.50')]:
        p.text(727,y,f'{qty}  ×  {name}',30,'bold',limit=635);p.text(756,y+47,f'Unit price {unit}',23);p.text(1380,y+47,total,27,'bold',align='right')
    p.line(725,701,657,INK,2,dashed=True)
    p.text(727,732,'TOTAL',39,'display');p.text(1380,722,'$5.10',53,'display',align='right')
    p.stamp(946,806,277,'PAID',GREEN,-5)
    p.text(1055,917,'Thanks for stopping by.',25,'hand',align='center')
    for x in range(700,1410,28):p.d.polygon([(x,996),(x+14,981),(x+28,996)],fill=INK)
    p.footer('Printed receipt  /  accepted fixture timestamp, no invented taxes or fees')
    p.save('04-printed-receipt.png')

    p=Page(dim=True);book(p,'Shift complete!','All orders served. Your receipts are filed.','SHIFT REPORT')
    p.stamp(1271,174,321,'GOOD SHIFT',GREEN,6)
    p.text(269,371,'REVENUE',23,'bold');p.text(265,415,'$16.50',90,'display')
    p.line(262,548,625)
    for y,label,value in [(576,'Customer orders','3 / 3'),(646,'Tasks completed','6 / 6'),(716,'Town Stability','+3')]:
        p.text(269,y,label,29,'bold');p.text(862,y,value,34,'bold',align='right')
    p.text(985,301,'Receipt ledger',34,'display')
    for i,(who,total,ident) in enumerate([('Mara','$5.10','BAU-0001'),('Eli','$5.20','BAU-0002'),('Jo','$6.20','BAU-0003')]):
        y=367+i*103;p.line(981,y-10,644);p.text(990,y,who,30,'bold');p.text(990,y+44,ident,22);p.text(1628,y+14,total,32,'bold',align='right')
    p.paper(979,713,655,126,fill=PALE)
    p.text(1000,734,'DELIVERED 18  /  SOLD 9  /  REMAINING 9',22,'bold',limit=610)
    p.text(1001,784,'3 Cola  •  3 Tea  •  3 Crackers on the shelves',23)
    p.text(267,841,'Head to the front exit. E returns to town.',24,'hand',limit=620)
    p.footer('Shift report  /  three receipts, $16.50 revenue, +3 Town Stability')
    p.save('05-shift-report.png')

    p=Page();hud(p,'collect',prompt=False)
    p.paper(626,882,792,134,tab='SHELF / E')
    p.key(647,909,'E');p.item(730,908,'cola',.8)
    p.text(803,905,'Collect Sunbeam Cola',33,'bold',limit=570)
    p.text(804,961,'Take 1 unit  •  Shelf 5/6  •  Basket 1/2',25)
    p.footer('Interaction prompt  /  one contextual action, character and shelf remain clear')
    p.save('06-interaction-prompt.png')

    # Design-system plate, separate from the six gameplay-state mockups.
    p=Page(background=False)
    p.text(68,39,'Business as Usual',62,'display');p.text(71,129,'DAYLIGHT MARKET  /  RETRO EMPLOYEE HANDBOOK',25,'bold')
    p.stamp(1413,54,351,'STAFF EDITION',CORAL,5)
    for i,(name,value) in enumerate([('Warm paper',PAPER),('Dark ink',INK),('Muted green',GREEN),('Mustard',YELLOW),('Muted coral',CORAL)]):
        x=68+i*367;p.d.rectangle((x,220,x+337,320),fill=value,outline=INK,width=3)
        p.text(x,337,name,25,'bold');p.text(x,376,value.upper(),24)
    p.line(68,442,1780,INK,3)
    p.text(68,476,'Friendly ink. Precise figures.',43,'display')
    p.text(70,549,'Dela Gothic One / headings and completion stamps',27,'bold')
    p.text(70,599,'M PLUS Rounded 1c / item names, quantities, descriptions',27)
    p.text(70,649,'Yusei Magic / occasional instruction in the margins',27,'hand')
    p.paper(68,740,524,220,tab='TASK CARD')
    p.text(94,765,'Restock Meadow Tea',32,'display',limit=466)
    p.text(96,826,'0 / 6 stocked',28,'bold')
    p.d.rectangle((96,888,550,906),fill=RULE);p.d.rectangle((96,888,247,906),fill=GREEN)
    p.paper(645,740,555,220,tab='ORDER SLIP')
    p.item(671,798,'cola',1.1);p.text(758,779,'Sunbeam Cola',32,'bold');p.text(758,838,'Basket 1 / 2',28);p.text(758,889,'$1.80 each',26)
    p.paper(1254,740,584,220,tab='INTERACTION')
    p.key(1281,777,'E');p.text(1369,781,'Collect 1 unit',31,'bold')
    p.stamp(1381,856,262,'COMPLETE',GREEN,-3)
    p.footer('SIL OFL 1.1 fonts  /  bold outlines, opaque paper, restrained accents')
    p.save('design-system.png')
    entries=[('01-normal-hud.png','Normal HUD'),('02-task-handbook.png','Task handbook'),('03-customer-order.png','Customer order'),('04-printed-receipt.png','Printed receipt'),('05-shift-report.png','Shift report'),('06-interaction-prompt.png','Interaction prompt')]
    contact=Image.new('RGB',(1920,1275),PAPER);d=ImageDraw.Draw(contact)
    d.text((40,22),'BUSINESS AS USUAL / UI REVIEW',font=font(36,'display'),fill=INK)
    for i,(name,label) in enumerate(entries):
        x=40+(i%2)*944;y=99+(i//2)*389
        thumb=Image.open(MOCK/name).resize((896,504)).resize((896,504))
        # 16:9 at 640x360 makes a compact six-screen overview.
        thumb=thumb.resize((640,360));contact.paste(thumb,(x,y))
        d.text((x+655,y+40),label.replace(' ','\n',1),font=font(25,'bold'),fill=INK)
    contact.save(OUT/'six-mockups-overview.png')
    links=''.join(f'<button data-src="Mockups/{n}">{label}</button>' for n,label in entries)
    thumbs=''.join(f'<a href="Mockups/{n}" target="_blank"><img src="Mockups/{n}" alt="{label}"><span>{label}</span></a>' for n,label in entries)
    html=f'''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Business as Usual — Handbook UI review</title>
<style>@font-face{{font-family:Dela;src:url(Fonts/DelaGothicOne-Regular.ttf)}}@font-face{{font-family:Rounded;src:url(Fonts/RoundedMplus1c-Regular.ttf)}}*{{box-sizing:border-box}}body{{margin:0;background:{PAPER};color:{INK};font-family:Rounded,sans-serif}}header{{padding:24px 4%;border-bottom:3px solid {INK}}}h1{{font:32px Dela;margin:0 0 12px}}p{{margin:10px 0}}nav{{display:flex;gap:8px;flex-wrap:wrap;margin:20px 0}}button{{padding:12px 18px;color:{INK};background:{PALE};border:2px solid {INK};border-radius:4px;font:16px Rounded;cursor:pointer}}button[aria-pressed=true]{{background:{YELLOW}}}main{{padding:24px 4%}}#hero{{width:100%;display:block;border:3px solid {INK}}}.grid{{display:grid;grid-template-columns:repeat(2,1fr);gap:24px;margin-top:32px}}.grid img{{width:100%;display:block}}a{{color:{INK};text-decoration:none}}.grid span{{display:block;padding:10px;font-weight:bold}}.note{{max-width:1000px}}@media(max-width:700px){{h1{{font-size:23px}}.grid{{grid-template-columns:1fr}}}}</style>
<header><h1>Business as Usual — Staff Edition</h1><p>Six visual prototypes for approval. The working Unity interface has not been replaced.</p><nav>{links}</nav></header>
<main><img id="hero" src="Mockups/01-normal-hud.png" alt="Normal HUD"><p id="caption">Normal HUD</p><p class="note">Original paper, packaging and stamp artwork. Data follows the current shift: 6 units per product, 3 orders, receipt totals $5.10 / $5.20 / $6.20, revenue $16.50. These are design mockups over an in-engine background, not implemented UI screenshots. Order value is distinct from paid revenue.</p><p><a href="Mockups/design-system.png">View design system</a> · <a href="../../Diagnostics/Phase4.1/before-shelf-world.png">Before labels</a> · <a href="../../Diagnostics/Phase4.1/after-shelf-world.png">After labels</a></p><div class="grid">{thumbs}</div></main>
<script>const buttons=[...document.querySelectorAll('button[data-src]')];function select(b){{document.getElementById('hero').src=b.dataset.src;document.getElementById('hero').alt=b.textContent;document.getElementById('caption').textContent=b.textContent;buttons.forEach(x=>x.setAttribute('aria-pressed',String(x===b)))}}buttons.forEach(b=>b.addEventListener('click',()=>select(b)));select(buttons[0]);document.addEventListener('keydown',e=>{{if(/^[1-6]$/.test(e.key))select(buttons[Number(e.key)-1])}})</script></html>'''
    (OUT/'review.html').write_text(html,encoding='utf-8')
    tokens={'palette':{'paper':PAPER,'ink':INK,'green':GREEN,'mustard':YELLOW,'coral':CORAL},'referenceResolution':[1920,1080],'safeMargin':48,'spacing':[8,16,24,32,48],'outline':4,'shadowOffset':[7,8],'type':{'display':'Dela Gothic One','body':'M PLUS Rounded 1c Regular','emphasis':'M PLUS Rounded 1c ExtraBold','annotation':'Yusei Magic'},'fontPixels':{'display':[32,44,62],'body':[24,28],'numeric':[27,34,53],'micro':20},'transitionsMs':{'paper':150,'stamp':180,'restock':650},'mockups':[n for n,_ in entries]}
    (OUT/'design-tokens.json').write_text(json.dumps(tokens,indent=2))
    print('Rendered six full-size mockups, design plate, overview and review gallery.')

if __name__=='__main__':render()
