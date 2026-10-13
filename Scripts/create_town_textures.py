"""Original surface textures and town notices, using the approved bundled fonts."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import random,math
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Unity/BusinessAsUsual/Assets/Town/Textures';OUT.mkdir(parents=True,exist_ok=True)
FONT=ROOT/'Unity/BusinessAsUsual/Assets/Shopkeeper/UI/Resources/Handbook/Fonts'
random.seed(606)
for name,base,grain in [('plaster',(221,206,175),8),('asphalt',(107,124,119),7),('paving',(207,196,168),5),('wood',(136,99,61),9)]:
    image=Image.new('RGB',(256,256));pixels=image.load()
    for y in range(256):
        for x in range(256):
            delta=random.randint(-grain,grain)
            if name=='wood':delta+=int(math.sin(y*.09+math.sin(x*.018)*1.2)*10)
            if name=='paving' and (x%64<2 or y%64<2):delta-=15
            pixels[x,y]=tuple(max(0,min(255,v+delta)) for v in base)
    image.save(OUT/(name+'.png'))
paper='#F6E9C9';ink='#244047';mustard='#EFC864';coral='#CE735A'
def poster(name,title,subtitle,body,icon):
    image=Image.new('RGB',(512,640),paper);draw=ImageDraw.Draw(image)
    regular=ImageFont.truetype(str(FONT/'RoundedMplus1c-Regular.ttf'),25);bold=ImageFont.truetype(str(FONT/'RoundedMplus1c-ExtraBold.ttf'),27);display=ImageFont.truetype(str(FONT/'DelaGothicOne-Regular.ttf'),38)
    draw.rounded_rectangle((15,15,497,625),radius=14,outline=ink,width=7);draw.rectangle((28,29,484,94),fill=mustard)
    draw.text((42,43),'DAYLIGHT / STAFF EDITION',fill=ink,font=bold)
    draw.text((42,126),title,fill=ink,font=display);draw.text((42,193),subtitle,fill=ink,font=bold)
    if icon=='wrench':
        draw.line((310,302,190,426),fill=ink,width=34);draw.arc((283,263,355,335),45,325,fill=ink,width=17);draw.ellipse((165,408,211,455),outline=ink,width=12)
    elif icon=='recycle':
        draw.polygon([(220,276),(315,297),(307,379),(234,429),(169,367)],outline=ink,width=10);draw.line((220,276,258,336),fill=ink,width=13);draw.line((307,379,240,375),fill=ink,width=13)
    elif icon=='clock':
        draw.ellipse((187,273,336,422),fill=mustard,outline=ink,width=9);draw.line((259,343,260,292),fill=ink,width=8);draw.line((259,343,222,358),fill=ink,width=8)
    else:
        draw.rounded_rectangle((178,283,344,431),radius=20,fill='#649B76',outline=ink,width=8);draw.arc((204,247,316,341),180,360,fill=ink,width=10)
    draw.multiline_text((42,467),body,fill=ink,font=regular,spacing=8);draw.line((42,594,470,594),fill=coral,width=4)
    image.save(OUT/(name+'.png'))
poster('notice-town','GOOD MORNING','One town. A little teamwork.','Choose your profession.\nKeep the square welcoming.\nDo your shift. Leave it tidy.','clock')
poster('notice-garage','MIKAN MOTOR','Service tickets / repair records','Hold E while positioned.\nParts: Daylight Market.\nAsk. Inspect. Repair. Record.','wrench')
poster('notice-yard','SORT IT RIGHT','Paper / Mixed / Metal','Pick up your route bag.\nMatch the label on the bin.\nFile the disposal manifest.','recycle')
poster('notice-shop','DAYLIGHT','Your neighborhood market','Fresh groceries. Useful parts.\nMunicipal reserve pickup.\nPlease mind the delivery lane.','bag')
poster('notice-alley','FOUND AGAIN','Community notices','One umbrella, no owner.\nThe alley lamp stays on.\nLost items: town noticeboard.','clock')
print('Created four seamless surfaces and five original handbook notices.')
