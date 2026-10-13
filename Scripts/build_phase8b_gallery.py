from pathlib import Path
from html import escape
from PIL import Image
import json
r=Path(__file__).resolve().parents[1];out=r/'Docs/Diagnostics/Phase8B'
panels=[
 ('Before / Phase 8A rescue entry','BaselineUI/host-main.png','Native baseline recorded before Phase 8B.'),
 ('After / neighborhood main menu','UI1080/host-main-menu.png','Real town geometry, six functional menu actions.'),
 ('Before / anonymous destination','BaselineUI/host-compact-hud.png','Earlier native HUD used a distance-only route pin.'),
 ('After / named turn guidance','UI720/host-named-turn-guidance.png','The destination and turn direction are explicit; decorative houses are not task targets.'),
 ('Loading','UI720/host-loading.png','Actual town load and connection transition; one brief tip.'),
 ('Choose a profession','UI720/host-profession-selection.png','Lead jobs and a helping profession.'),
 ('Staff hub','FourProcess/host-staff-hub.png','Four real processes ready before the host opens the day.'),
 ('During the shift / current crew','UI720/host-current-shift-crew.png','The staff desk explains the fixed job and opens shared copies instead of offering unavailable role changes.'),
 ('Private copy','SoloShop/host-private-note.png','Only this worker receives their private note; solo is labelled practice.'),
 ('Supported returns','SoloShop/host-temporary-hold.png','The same parcel is restaged on a physical receiving tray.'),
 ('Alternative yard hold','SoloMechanic/host-temporary-hold.png','A second ordinary route, with a declaration separate from observation.'),
 ('Mechanic / open hood','SoloMechanic/host-hood-after.png','Accepted Rescue V2 vehicle and animation reused; physical repair is still central.'),
 ('Sanitation / collection','SoloSanitation/host-loaded-cart.png','Existing loads travel on the pushable cart; the obsolete pole cart is retired.'),
 ('Compare selected copies','FourProcess/host-two-selected-copies.png','A declaration and an amendment are pinned voluntarily; no automatic guilt.'),
 ('End of the shared day','FourProcess/host-shared-shift-summary.png','Public work outcomes, with private progress only for its holder.'),
 ('Settings','UI720/host-settings.png','Separate sound controls and saved player preferences.'),
 ('Display and resizing','UI1080/host-resized-1366.png','Actual native resize; labels refresh after the mode change.'),
 ('Credits','UI720/host-credits.png','Project attribution, tools and bundled font acknowledgements.'),
 ('Return to menu','UI1080/host-returned-main-menu.png','Networking is cleaned up; a fresh shift can start.')]
sections=[]
for title,path,caption in panels:
 assert (out/path).exists(),path
 sections.append(f'<figure><a href="{escape(path)}"><img loading="lazy" src="{escape(path)}" alt="{escape(title)}"></a><figcaption><b>{escape(title)}</b><p>{escape(caption)}</p></figcaption></figure>')
clips=[]
for folder,pattern,name in [('SoloMechanic','host-preview-hood-30-*.png','hood'),('SoloSanitation','host-preview-cart-travel-*.png','cart')]:
 frames=sorted((out/folder).glob(pattern))
 if len(frames)<8:continue
 imgs=[Image.open(p).convert('RGB') for p in frames];gif=out/(name+'-native-sequence.gif');imgs[0].save(gif,save_all=True,append_images=imgs[1:],duration=120,loop=0,optimize=False)
 clips.append(dict(name=name,source=[str(p.relative_to(out)) for p in frames],frames=len(frames),gif=gif.name,description='Derived GIF of consecutive unaltered native frame captures; nominal capture interval 0.1s, GIF playback 0.12s.'))
cliphtml=''.join(f'<figure><img src="{c["gif"]}" alt="Native {c["name"]} sequence"><figcaption>{c["description"]}</figcaption></figure>' for c in clips)
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Business as Usual / Phase 8B native review</title><style>body{margin:0;background:#173b37;color:#f6eccf;font:17px/1.55 system-ui,sans-serif}main{max-width:1320px;margin:auto;padding:40px 24px}h1{font-size:clamp(28px,4vw,48px);line-height:1.1}a{color:#ecc252}header p{max-width:80ch}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,480px),1fr));gap:22px}figure{margin:0;background:#f6eccf;color:#173b37;border-radius:12px;overflow:hidden}img{width:100%;height:auto;display:block}figcaption{padding:16px 20px}figcaption p{margin:5px 0}footer{margin-top:32px}</style><main><header><p>DAYLIGHT / IMPLEMENTATION EVIDENCE</p><h1>Ordinary jobs.<br>Extraordinary suspicion.</h1><p>Phase 8B: actual Windows player captures. These show the implemented menu, work chain and first suspicious day. Automated fixture placement and real native input are distinguished in the validation report. No human usability or fun result is claimed.</p><p><a href="../../Design/phase8b-validation-report.md">Validation</a> · <a href="../../Design/phase8b-playtest.md">Play one window</a> · <a href="../../Design/phase8b-remaining-issues.md">Remaining issues</a></p></header><div class="grid">'''+''.join(sections)+'''</div><h2>Native frame sequences</h2><div class="grid">'''+cliphtml+'''</div><footer>Original PNGs are linked at full resolution. GIFs are derived previews; no retouching, generated mockups or fake gameplay. Solo work captures use build 8; multiplayer captures use the final validation build. See recorded assembly fingerprints for exact provenance.</footer></main></html>'''
(out/'review.html').write_text(html,encoding='utf-8');(out/'gallery-manifest.json').write_text(json.dumps(dict(panels=panels,clips=clips),indent=2),encoding='utf-8');print(len(panels),'native screenshots;',len(clips),'frame-sequence GIFs.')
