"""Validate packaged evidence, local gallery links and protected baseline claims."""
from pathlib import Path
from html.parser import HTMLParser
from urllib.parse import unquote,urlparse
import json,hashlib
from PIL import Image
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Docs/Diagnostics/Phase8ARescueV2'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
class Links(HTMLParser):
 def __init__(self):super().__init__();self.links=[]
 def handle_starttag(self,tag,attrs):self.links.extend(v for k,v in attrs if k in ('href','src'))
parser=Links();parser.feed((OUT/'review.html').read_text(encoding='utf-8'));missing=[]
for link in parser.links:
 if not urlparse(link).scheme and not (OUT/unquote(link.split('#')[0])).resolve().exists():missing.append(link)
assert not missing,missing
summary=load(OUT/'validation-summary.json');assert len(summary['suites'])==8
assert all(s['passed']==s['total'] and s['total']>0 for s in summary['suites'])
assert summary['build']['result']=='Succeeded' and summary['build']['errors']==0
assert summary['currentAssemblySha256']==hashlib.sha256((ROOT/'Builds/Phase8A-RescueV2/BusinessAsUsual_Data/Managed/Assembly-CSharp.dll').read_bytes()).hexdigest()
preservation=load(OUT/'preservation.json');assert preservation['indexStagesUnchanged'] and not preservation['missing'];assert all(preservation[k] for k in ['originalScenesUnchanged','originalMaterialsUnchanged','originalPrefabsUnchanged'])
for width,height,folder in [(1280,720,'FinalUI720Validated'),(1920,1080,'FinalUI1080Validated')]:
 with Image.open(OUT/folder/'host-typography-0.png') as im:assert im.size==(width,height)
for p in (OUT/'FinalUI1080Validated').glob('host-native-render-1920x1200-*.png'):
 with Image.open(p) as im:assert im.size==(1920,1200)
 evidence=load(p.with_suffix('.json'));assert all(not t['overflows'] and not t['offscreen'] for t in evidence['text'])
assert len(list((OUT/'FinalUI1080Validated').glob('host-native-render-1920x1200-*.png')))==6
ledger=load(ROOT/'Docs/meshy-credit-ledger.json');assert ledger['spentTotal']==140 and ledger['remaining']==160 and ledger['reservedTotal']==0
provenance=load(OUT/'asset-provenance.json')
for asset in provenance['assets']:
 if 'editableSource' in asset:
  p=Path(asset['editableSource']);assert (ROOT/p if len(p.parts)>1 else OUT/p).exists(),str(p)
for p in [ROOT/'Docs/Design/phase8A-playtest.md',ROOT/'Docs/Design/phase8a-ui-design-system.md',ROOT/'Docs/Design/phase8a-handbook-redesign.md',ROOT/'Docs/Design/phase8a-rescue-v2-verification.md']:
 text=p.read_text(encoding='utf-8');assert 'Ã' not in text and 'Â' not in text,p.name
print(f"PASS {len(parser.links)} gallery links, 8 passing native suites ({sum(s['total'] for s in summary['suites'])} checks), source fingerprints, display evidence, credit ledger and baseline preservation")
