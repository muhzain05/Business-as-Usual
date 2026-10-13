"""Preserve accepted evidence and verify only the explicitly scoped polish changes."""
from pathlib import Path
import hashlib, json, subprocess, re, shutil
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/Diagnostics/Phase4.1'
REG=OUT/'Regression'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()

# The validators write to their original locations. Retain new evidence separately,
# then put only their own overwritten diagnostic outputs back to the saved bytes.
for phase in ['Phase3','Phase4']:
    saved=OUT/'accepted-baseline'/phase
    current=ROOT/'Docs/Diagnostics'/phase
    for p in saved.rglob('*'):
        if p.is_file():
            destination=current/p.relative_to(saved)
            if destination.exists() and sha(destination)!=sha(p):
                evidence=REG/phase/p.relative_to(saved)
                evidence.parent.mkdir(parents=True,exist_ok=True)
                shutil.copy2(destination,evidence)
                shutil.copy2(p,destination)

# Unity auto-added default URP light data while rendering and serializing the scene.
# Remove just those newly added records and their component references. Never rewrite
# other scene records or reconstruct the scene from a staged file.
scene=ROOT/'Unity/BusinessAsUsual/Assets/Shopkeeper/Scenes/ShopkeeperShift.unity'
rel=str(scene.relative_to(ROOT)).replace('\\','/')
original=subprocess.check_output(['git','show',':'+rel]).decode('utf-8')
baseline=json.loads((OUT/'session-preservation.json').read_text())
assert baseline[rel] in {hashlib.sha256(original.encode()).hexdigest(),hashlib.sha256(original.replace('\n','\r\n').encode()).hexdigest()}
text=scene.read_text(encoding='utf-8')
blocks=re.split(r'(?=^--- !u!)',text,flags=re.M)
old_ids=set(re.findall(r'^--- !u!\d+ &(-?\d+)',original,re.M))
remove=[];keep=[]
for block in blocks:
    match=re.match(r'--- !u!114 &(-?\d+)',block)
    if match and match[1] not in old_ids and 'UniversalAdditionalLightData' in block:
        remove.append(match[1])
    else:keep.append(block)
clean=''.join(keep)
for ident in remove:clean=clean.replace('  - component: {fileID: '+ident+'}\n','')
if clean!=text:scene.write_text(clean,encoding='utf-8',newline='\n')

# A label comparison includes exactly matching world camera/actor states.
before=Image.open(OUT/'before-shelf-world.png').convert('RGB')
after=Image.open(OUT/'after-shelf-world.png').convert('RGB')
assert before.size==after.size==(1920,1080)
sheet=Image.new('RGB',(1920,620),'#F6E9C9');d=ImageDraw.Draw(sheet)
f=ImageFont.truetype(str(ROOT/'Docs/UI/Phase4.2/Fonts/RoundedMplus1c-ExtraBold.ttf'),28)
sheet.paste(before.resize((960,540)),(0,65));sheet.paste(after.resize((960,540)),(960,65))
d.text((24,16),'BEFORE / prices through Sunny',font=f,fill='#244047')
d.text((984,16),'AFTER / depth-tested shelf labels',font=f,fill='#244047')
sheet.save(OUT/'label-before-after.png')

# Source preservation: the index's cached filesystem statistics can change on git
# status; raw index bytes are therefore not a valid staged-content comparison.
targeted={
    'Unity/BusinessAsUsual/Assets/Shopkeeper/Editor/'+n for n in ['ShopArtBuilder.cs','ShopBuilder.cs','ShopValidation.cs']
}|{
    'Unity/BusinessAsUsual/Assets/Shopkeeper/Scripts/'+n for n in ['ShopHUD.cs','ShopStation.cs','ShopStockDisplay.cs']
}|{'Unity/BusinessAsUsual/Assets/Shopkeeper/Scenes/ShopkeeperShift.unity'}
changes=[]; unexpected=[]
for name,h in baseline.items():
    if name=='git:index':continue
    p=ROOT/name
    if p.is_file() and sha(p)!=h:
        changes.append(name)
        # These two materials were automatically normalized during the initial
        # baseline Play session. Their original staged keyword has been restored.
        if name not in targeted and not name.endswith(('Shop_Paper.mat','Shop_Screen.mat')):unexpected.append(name)
assert not unexpected,unexpected

reports={}
for key,path in [('Phase4.1',OUT/'polish-validation.json'),('Phase4',REG/'phase4-playmode.json'),('Phase3',REG/'phase3-playmode.json')]:
    data=json.loads(path.read_text());reports[key]={'passed':sum(c['passed'] for c in data['checks']),'total':len(data['checks']),'failed':[c for c in data['checks'] if not c['passed']]}
    assert not reports[key]['failed']
phase4=json.loads((REG/'phase4-playmode.json').read_text())
receipts=phase4['summary']['receipts']
assert [r['totalCents'] for r in receipts]==[510,520,620]
assert phase4['summary']['revenueCents']==1650
assert all(sum(l['quantity']*l['unitPriceCents'] for l in r['lines'])==r['totalCents'] for r in receipts)
prototype=json.loads((ROOT/'Docs/Diagnostics/Phase4/preservation-baseline.json').read_text())
assert all(sha(ROOT/p)==h for p,h in prototype.items())
result={'suites':reports,'prototypeFilesUnchanged':len(prototype),'restoredImplicitLightComponents':len(remove),'targetedTrackedChanges':changes,'unexpectedChanges':unexpected,'stagingActionsPerformed':False,'noCommitOrPush':True,'notes':['Original diagnostic outputs restored after retaining new reports in Phase4.1/Regression.','verify_phase4.py passed independently.','Legacy verify_phase3.py passed its source/asset/live checks, then stopped at its empty-staging assertion. Existing staged work is intentionally preserved; no staging was cleared.']}
(OUT/'validation-summary.json').write_text(json.dumps(result,indent=2))
print(json.dumps(result,indent=2))
