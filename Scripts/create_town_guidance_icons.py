"""Original paper-stamp symbols shared by the HUD, map, slips and depth-tested world pins."""
from pathlib import Path
from PIL import Image,ImageDraw
out=Path(__file__).resolve().parents[1]/'Unity/BusinessAsUsual/Assets/Town/Resources/Town/Icons'
out.mkdir(parents=True,exist_ok=True)
ink='#244047';paper='#F6E9C9';gold='#EFC864';green='#649B76';blue='#4089A0'
def icon(name):
    im=Image.new('RGBA',(128,128));d=ImageDraw.Draw(im)
    if name not in ('you','ring'):
        d.rounded_rectangle((10,7,118,112),radius=18,fill=paper,outline=ink,width=4)
        d.polygon([(51,110),(64,124),(77,110)],fill=paper);d.line([(51,111),(64,124),(77,111)],fill=ink,width=4)
    def line(points,color=ink,width=5):d.line(points,fill=color,width=width,joint='curve')
    def box(bounds,fill=gold,radius=5):d.rounded_rectangle(bounds,radius=radius,fill=fill,outline=ink,width=4)
    if name=='shop':
        box((29,44,99,92),green);box((49,64,69,92),paper,2);d.polygon([(23,43),(33,26),(96,26),(105,43)],fill=gold,outline=ink);line([(23,44),(105,44)])
        for x in range(27,101,15):d.rectangle((x,33,x+7,47),fill=paper)
    elif name=='wrench':
        line([(46,76),(86,36)],gold,16);line([(43,80),(83,40)],ink,8);d.polygon([(63,27),(72,20),(72,35),(87,44),(96,34),(100,48),(89,59),(68,49)],fill=gold,outline=ink);d.ellipse((33,75,49,91),fill=ink)
    elif name=='recycle':
        for pts,head in [([(47,44),(64,24),(85,48)],[(81,37),(94,54),(74,53)]), ([(90,58),(80,85),(51,85)],[(65,76),(46,85),(66,94)]), ([(40,80),(28,56),(40,40)],[(29,46),(44,34),(45,55)])]:
            line(pts,green,9);d.polygon(head,fill=ink)
    elif name in ('slip','record'):
        box((36,23,92,93),paper,4)
        for y in (39,53,67):line([(46,y),(79,y)],width=4)
        if name=='record':d.ellipse((66,68,100,102),fill=green,outline=ink,width=3);line([(74,84),(81,91),(94,77)],paper,5)
    elif name=='inspect':
        d.ellipse((27,23,82,78),fill=gold,outline=ink,width=6);d.ellipse((36,32,73,69),fill=paper);line([(76,73),(100,96)],ink,12);line([(41,42),(47,36),(56,34)],green,4)
    elif name=='parts':
        box((29,34,100,88),gold,4);line([(29,47),(100,47)]);line([(62,34),(62,88)]);d.polygon([(70,50),(56,67),(66,67),(58,82),(81,62),(70,62)],fill=ink)
    elif name=='battery':
        box((28,38,101,86),gold);d.rectangle((39,28,52,38),fill=ink);d.rectangle((78,28,91,38),fill=ink);line([(45,54),(45,72)]);line([(36,63),(54,63)]);line([(73,63),(91,63)])
    elif name=='filter':
        box((40,29,87,89),green,8);d.ellipse((40,22,87,40),fill=gold,outline=ink,width=4);line([(51,47),(51,78)],paper,4);line([(67,47),(67,78)],paper,4)
    elif name=='bag':
        d.polygon([(49,23),(68,30),(83,22),(75,39),(94,70),(87,94),(42,94),(32,73),(52,40)],fill=green,outline=ink);line([(49,41),(80,41)],gold,6);line([(49,54),(43,73)],paper,4)
    elif name=='bin':
        box((39,38,90,94),green);box((30,28,99,39),gold,3);line([(51,51),(51,79)],paper,4);line([(72,51),(72,79)],paper,4)
    elif name=='basket':
        d.polygon([(27,46),(101,46),(91,87),(37,87)],fill=gold,outline=ink);line([(39,47),(53,25),(76,25),(91,47)]);line([(51,52),(53,80)],paper,4);line([(74,52),(73,80)],paper,4)
    elif name=='door':
        box((29,23,97,93),green,3);d.polygon([(49,26),(87,36),(87,91),(49,101)],fill=gold,outline=ink);d.ellipse((71,62,77,68),fill=ink)
    elif name in ('clock','wait'):
        d.ellipse((28,22,101,95),fill=gold if name=='wait' else paper,outline=ink,width=5);line([(65,37),(65,59),(84,69)]);d.ellipse((61,55,69,63),fill=ink)
    elif name=='check':
        d.ellipse((27,24,102,99),fill=green,outline=ink,width=4);line([(42,61),(57,77),(88,44)],paper,9)
    elif name=='you':
        d.ellipse((29,32,99,103),fill=paper,outline=ink,width=6);d.polygon([(64,8),(98,94),(64,78),(30,94)],fill=blue,outline=ink)
    elif name=='ring':
        for start in range(0,360,45):d.arc((8,8,120,120),start+4,start+32,fill=gold,width=9)
    # Draw at 128px; these deliberately simple silhouettes remain clear at map-icon size.
    im.save(out/(name+'.png'))
for name in ('shop','wrench','recycle','slip','record','inspect','parts','battery','filter','bag','bin','basket','door','wait','clock','check','you','ring'):icon(name)
print('Created 18 original town symbols.')
