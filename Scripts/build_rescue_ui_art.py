"""Original editable pictograms. One geometric source produces SVG and Unity PNG assets."""
from pathlib import Path
import json
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Unity/BusinessAsUsual/Assets/RescueV2/Resources/RescueV2/Icons'
OUT.mkdir(parents=True,exist_ok=True)
INK='#203F40'; PAPER='#F7EBCD'; GOLD='#E7B647'; GREEN='#3E735D'; CORAL='#D36F55'
def rect(x,y,w,h,c=PAPER):return dict(kind='rect',box=[x,y,x+w,y+h],fill=c)
def ellipse(x,y,w,h,c=GOLD):return dict(kind='ellipse',box=[x,y,x+w,y+h],fill=c)
def line(points,c=INK,width=8):return dict(kind='line',points=points,color=c,width=width)
def poly(points,c=PAPER):return dict(kind='polygon',points=points,fill=c)
icons={
 'cola':[poly([(100,33),(156,33),(156,56),(174,76),(184,206),(170,225),(84,225),(70,206),(80,76),(100,56)],CORAL),rect(100,22,56,15,INK),rect(76,113,105,60,PAPER),line([(91,131),(165,131)],GREEN,8),line([(105,151),(148,151)],INK,5)],
 'tea':[rect(62,52,134,169,GREEN),rect(83,31,91,24,INK),rect(78,91,102,93,PAPER),poly([(100,156),(108,119),(150,105),(153,143),(136,165)],GREEN),line([(107,164),(143,119)],PAPER,4)],
 'crackers':[poly([(61,34),(195,34),(183,222),(73,222)],GOLD),rect(77,62,104,58,PAPER),ellipse(91,144,73,58,PAPER),ellipse(102,157,6,6,INK),ellipse(133,155,6,6,INK),ellipse(119,180,6,6,INK)],
 'hood':[poly([(30,140),(30,107),(90,94),(140,58),(201,58),(227,102),(227,140)]),rect(130,65,65,36,GREEN),ellipse(53,127,40,40,INK),ellipse(174,127,40,40,INK),line([(91,94),(60,35),(28,52)],CORAL),line([(51,47),(78,34),(72,57)],CORAL,6)],
 'battery':[rect(60,67,136,131),rect(79,49,28,18,GOLD),rect(150,49,28,18,CORAL),line([(82,104),(109,104)]),line([(95,91),(95,117)]),line([(151,104),(176,104)]),line([(128,130),(105,155),(134,155),(113,183)],CORAL,10)],
 'test':[rect(51,44,154,169),rect(70,62,116,72,GREEN),line([(88,113),(128,82),(164,99)],PAPER,9),ellipse(92,155,24,24,CORAL),ellipse(147,155,24,24,INK),line([(104,180),(76,229),(37,216)],CORAL),line([(159,180),(188,230),(225,214)])],
 'replace':[rect(33,88,66,93),rect(43,76,15,12,GOLD),rect(76,76,15,12,CORAL),rect(157,88,66,93,GREEN),rect(166,76,15,12,GOLD),rect(199,76,15,12,CORAL),line([(39,48),(182,48),(165,32)]),line([(218,214),(73,214),(91,229)],CORAL)],
 'sign':[rect(51,33,152,191),line([(76,65),(177,65)]),line([(76,89),(156,89)],INK,5),ellipse(79,115,101,80,GREEN),line([(101,154),(119,172),(155,137)],PAPER,10)],
 'parcel':[poly([(34,82),(128,34),(222,82),(222,182),(128,229),(34,182)]),line([(34,82),(128,130),(222,82)]),line([(128,130),(128,229)]),poly([(100,48),(189,94),(166,106),(78,60)],GOLD),rect(150,132,52,34,PAPER)],
 'shelf':[rect(28,38,200,182,GREEN),rect(42,48,172,34,PAPER),rect(42,111,172,34,PAPER),rect(42,173,172,32,PAPER),rect(56,50,28,31,CORAL),rect(91,48,28,33,GOLD),rect(127,50,28,31,CORAL),rect(64,110,32,34,GOLD),rect(109,109,32,35,CORAL),line([(199,159),(199,218),(174,198)],PAPER,8)],
 'checkout':[rect(40,87,176,112),rect(53,54,110,58,GREEN),rect(67,69,83,26,PAPER),rect(165,145,32,24,GOLD),line([(54,133),(115,133)]),rect(77,199,99,36,PAPER),line([(90,211),(164,211)],INK,4),line([(90,223),(141,223)],INK,4)],
 'bag':[poly([(99,39),(156,39),(142,69),(195,106),(207,197),(170,221),(81,221),(49,195),(62,106),(114,69)],GREEN),line([(91,74),(163,74)],CORAL),line([(100,37),(93,18)]),line([(153,37),(166,19)]),line([(102,117),(136,102),(145,131)],PAPER),line([(162,145),(159,179),(126,174)],PAPER),line([(101,179),(77,151),(105,136)],PAPER)],
 'sort':[rect(41,82,173,151,GREEN),rect(25,77,205,22,GOLD),poly([(104,14),(157,14),(157,36),(192,36),(130,74),(67,36),(104,36)],CORAL),line([(76,134),(183,134)],PAPER,8),line([(93,165),(166,165)],PAPER,8)],
 'cart':[rect(34,80,163,113,GREEN),line([(29,80),(22,39),(55,39)]),line([(201,194),(218,61),(238,61)]),ellipse(47,190,32,32,INK),ellipse(161,190,32,32,INK),poly([(67,76),(71,45),(107,37),(128,72)],GOLD),poly([(123,77),(135,38),(178,39),(187,77)],CORAL)],
 'map':[poly([(23,47),(87,26),(161,50),(228,29),(228,210),(166,231),(91,211),(23,231)]),line([(87,28),(91,211)],INK,5),line([(161,50),(166,229)],INK,5),ellipse(100,80,65,65,CORAL),poly([(105,132),(131,179),(158,132)],CORAL),ellipse(119,99,26,26,PAPER)],
 'wrench':[poly([(177,24),(203,43),(177,67),(171,88),(155,105),(83,177),(87,196),(61,221),(34,197),(58,173),(76,176),(146,94),(141,72),(152,45)],GOLD)],
 'receipt':[poly([(64,29),(190,29),(190,226),(174,214),(158,226),(142,214),(126,226),(110,214),(94,226),(78,214),(64,226)]),line([(86,64),(166,64)],INK,6),line([(86,95),(169,95)],INK,5),line([(86,122),(153,122)],INK,5),line([(86,165),(169,165)],CORAL,7),line([(86,191),(141,191)],INK,5)],
 'door':[rect(65,27,131,202,GREEN),rect(76,37,108,181,PAPER),ellipse(160,118,12,12,GOLD),line([(26,228),(228,228)]),line([(19,129),(53,129),(39,115)],CORAL,8)],
 'stock':[rect(24,149,208,73,GREEN),rect(45,46,65,104,GOLD),rect(57,30,41,19,CORAL),rect(147,69,61,80,PAPER),rect(155,47,44,24,GREEN),line([(40,178),(218,178)],PAPER,5)],
 'check':[ellipse(34,32,188,188,GREEN),line([(70,128),(109,164),(186,82)],PAPER,16)],
 'mouse':[rect(74,27,110,196,PAPER),line([(129,29),(129,113)]),line([(76,114),(183,114)]),rect(96,35,26,72,GOLD),line([(203,104),(238,104),(221,88)],CORAL)],
 'shop':[rect(32,81,192,149,PAPER),poly([(24,79),(42,30),(212,30),(232,79)],GOLD),line([(24,80),(232,80)],INK,7),rect(49,123,74,63,GREEN),rect(148,123,52,106,CORAL)],
}
for name,shapes in icons.items():
 im=Image.new('RGBA',(256,256));d=ImageDraw.Draw(im);svg=[]
 for s in shapes:
  kind=s['kind'];stroke=INK;width=s.get('width',6)
  if kind=='line':
   pts=[tuple(p) for p in s['points']];d.line(pts,fill=s['color'],width=width,joint='curve')
   for x,y in pts:d.ellipse((x-width/2,y-width/2,x+width/2,y+width/2),fill=s['color'])
   svg.append(f'<polyline points="{" ".join(f"{x},{y}" for x,y in pts)}" fill="none" stroke="{s["color"]}" stroke-width="{width}" stroke-linecap="round" stroke-linejoin="round"/>')
  elif kind in ('rect','ellipse'):
   x,y,x2,y2=s['box'];fn=d.rounded_rectangle if kind=='rect' else d.ellipse
   if kind=='rect':fn((x,y,x2,y2),radius=5,fill=s['fill'],outline=INK,width=6);svg.append(f'<rect x="{x}" y="{y}" width="{x2-x}" height="{y2-y}" rx="5" fill="{s["fill"]}" stroke="{INK}" stroke-width="6"/>')
   else:fn((x,y,x2,y2),fill=s['fill'],outline=INK,width=6);svg.append(f'<ellipse cx="{(x+x2)/2}" cy="{(y+y2)/2}" rx="{(x2-x)/2}" ry="{(y2-y)/2}" fill="{s["fill"]}" stroke="{INK}" stroke-width="6"/>')
  else:
   pts=[tuple(p) for p in s['points']];d.polygon(pts,fill=s['fill']);d.line(pts+[pts[0]],fill=INK,width=6,joint='curve');svg.append(f'<polygon points="{" ".join(f"{x},{y}" for x,y in pts)}" fill="{s["fill"]}" stroke="{INK}" stroke-width="6" stroke-linejoin="round"/>')
 im.save(OUT/(name+'.png'));(OUT/(name+'.svg')).write_text('<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 256 256">'+''.join(svg)+'</svg>',encoding='utf-8')
(OUT/'original-icon-source.json').write_text(json.dumps(icons,indent=2),encoding='utf-8')
print(f'Created {len(icons)} original editable illustrations and matching PNGs')
