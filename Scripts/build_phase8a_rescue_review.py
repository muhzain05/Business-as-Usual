"""Package native evidence and editable-source references; never retouch game screenshots."""
from pathlib import Path
import json,html,hashlib,subprocess
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/Diagnostics/Phase8ARescueV2'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,s):p.parent.mkdir(parents=True,exist_ok=True);p.write_text(s,encoding='utf-8')
suites=[]
for name in ['FinalUI720Validated','FinalUI1080Validated','FinalMechanic','FinalRetailGrounded','FinalSanitationValidated','FinalTwoProcess','FinalFourProcess','FinalArtGrounded']:
    p=OUT/name/'checks.json'
    if p.exists():
        checks=load(p);suites.append(dict(suite=name,passed=sum(bool(c.get('passed')) for c in checks),total=len(checks),path=p.relative_to(OUT).as_posix(),assemblySha256=load(p.parent/'run-metadata.json').get('assemblySha256')))
summary=dict(build=load(OUT/'build.json'),suites=suites,physicalDomainAssertions=367,legacyDomainAssertions=10385,humanUsabilityTest=False,crossMachineLAN=False,render1200='Native Unity offscreen composition, not a 1200px desktop window')
write(OUT/'validation-summary.json',json.dumps(summary,indent=2)+'\n')
assembly=ROOT/'Builds/Phase8A-RescueV2/BusinessAsUsual_Data/Managed/Assembly-CSharp.dll'
summary['currentAssemblySha256']=hashlib.sha256(assembly.read_bytes()).hexdigest()
write(OUT/'validation-summary.json',json.dumps(summary,indent=2)+'\n')

baseline=Path('D:/Business as Usual Backups/pre-phase8A-rescue-v2-2026-10-12')
manifest=load(baseline/'manifest.json');changed=[];missing=[]
for name,expected in manifest.items():
    p=ROOT/name
    if not p.exists():missing.append(name)
    elif hashlib.sha256(p.read_bytes()).hexdigest()!=expected:changed.append(name)
preservation=dict(baseline=str(baseline),baselineFiles=len(manifest),missing=missing,changed=changed,indexStagesUnchanged=subprocess.check_output(['git','ls-files','--stage'],cwd=ROOT)==(baseline/'index-stage.txt').read_bytes(),originalScenesUnchanged=not any(n.endswith('.unity') for n in changed+missing),originalMaterialsUnchanged=not any(n.endswith('.mat') for n in changed+missing),originalPrefabsUnchanged=not any(n.endswith('.prefab') for n in changed+missing))
write(OUT/'preservation.json',json.dumps(preservation,indent=2)+'\n')

ledger=load(ROOT/'Docs/meshy-credit-ledger.json')
for t in ledger['tasks']:
    if t.get('previewId')=='01a1325c-0e19-77ac-9706-28d6d668eea6':t['faceCount']=12341
    if t.get('previewId')=='01a13283-8911-7537-9816-56e17f2c6954':t['faceCount']=5355
    if t.get('status')=='SUCCEEDED':t['refineReservedCredits']=0
ledger['spentTotal']=sum(t.get('previewCredits',0)+t.get('refineCredits',0) for t in ledger['tasks']);ledger['remaining']=ledger['limit']-ledger['spentTotal'];ledger['reservedTotal']=0
write(ROOT/'Docs/meshy-credit-ledger.json',json.dumps(ledger,indent=2)+'\n')

playtest=r'''# Phase 8A Rescue V2 — play the living workday

Launch one window from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File Scripts/launch_phase8a.ps1 -Resolution 720
```

The launcher now defaults to **solo** and uses `Builds/Phase8A-RescueV2/BusinessAsUsual.exe`. Choose Shopkeeper, Mechanic or Sanitation. Read the two illustrated introduction cards, pressing Enter to continue. H reopens help. Four windows launch only with explicit `-Mode four`; that mode is for technical testing.

For the main menu rather than immediate solo start, use `-Mode desk`. Main offers START SOLO / HOST WORKDAY / JOIN WORKDAY. A host uses `-Mode host`; a second computer uses `-Mode client -Address <host-ip>`. Network sessions need workers in all three lead jobs to complete every town outcome; four players can add an assistant. Unattended services are explicitly part of solo, not silent multiplayer substitutes.

## Learn from the game

WASD moves; mouse looks; Shift runs when unladen; E uses the aimed object; R offers an optional action; Q returns a carried load or parks the cart. Tab opens the compact JOB / ITEMS / RECORDS / TOWN clipboard. M opens the route map, J reopens the staff desk, Esc opens local pause/settings. Settings include volume, look sensitivity, FOV, reduced motion, display size, fullscreen/windowed mode, quality, V-sync, frame limit and inverted look.

Follow the mustard destination marker, then aim at the highlighted physical object. The nearby prompt gives the input and verb. The handbook's five pictures summarize your job; WHY supplies optional detail. There is no permanent instruction paragraph over the world.

## Mechanic

Open the wagon's hood by its latch. Check its battery posts with E; this diagnosis requests the replacement. R performs one optional load test. A sealed parcel arrives on the receiving bench in solo through a labelled unattended market service. In multiplayer the market prepares it and a worker carries it over.

At the clamp, E starts the wrench interaction. Hold left mouse and drag right until the turn completes; Q cancels. R changes quick swap versus bench recovery before turning: recovery needs two turns and yields metal, quick swap needs one and yields mixed treatment waste. Lift the old battery and put it on the scrap tray. Open the received carton and carry its replacement to the battery seat. E installs it, leaving the hood open. Test the repair, close the hood, and sign the service ticket.

## Shopkeeper

Receive the rear shipment. Stock verification is optional at dispatch with R; E prepares the garage parcel. Carry the single carton to the garage receiving bench. The open store doorway shows the town and exits when walked through; it uses an offset interior transition.

For checkout, lift the labelled stock box, match it to the printed shelf, lift the requested product from that stocked shelf, place it beside the scanner, and scan it. The three available products are Sunbeam Cola, Meadow Tea and Golden Crackers. Each is one conserved stock item; scanning removes that exact item from circulation. Wrong stock and repeated scans cannot manufacture a sale. Customer receipts record this handoff and omit earlier handlers.

## Sanitation

E at the cart handle begins pushing; walking moves the server-owned cart and turns its wheels. Q parks it. Carry a batch to the cart and E loads it, freeing your hands. Up to three actual batches fit. Aim at a particular loaded batch to lift it; R at the handle also lifts a batch.

Collect the square sack, the opened garage packaging, and the old battery. Solo's labelled mechanic service produces the same repair waste while you work. Take the cart to the yard. R at a chute inspects the held material; E at the matching paper, metal or mixed chute sorts it. A wrong chute retains the object. File all processed batches at the manifest desk. Your optional inspection and collection priority affect the record and town supply.

## Finish

The shared town needs at least one checkout, a tested and signed repair, and all three collection tickets. When those are complete, the square clipboard stand signs off the shift. The report shows the outcome and supply total; VIEW RECORDS opens your profession's copies. NEW WORKDAY starts fresh. The default workday is eight minutes; timeout also closes the shift and reports unfinished work. Solo pause freezes the solo clock; multiplayer pause is local and the town keeps running.

In solo, absent professions use explicitly labelled municipal services. They act on the same seven inventory IDs and cannot steal your held parcel. No hidden human teammates, cloned inventory, scripted guilt indicators, daily ejections or criminal win condition were added by this rescue.

## Playtest protocol

For a first-time player, give only the launch command and let the illustrated introduction teach the job. Record time to first correct interaction, wrong target attempts, missed markers, rejected-action recovery, and whether the shift can be completed without spoken instructions. Ask what each receipt proves and what it leaves uncertain. Then repeat with the other jobs and a real group.

Technical evidence and the before/after gallery are in [Rescue review](../Diagnostics/Phase8ARescueV2/review.html). Automated runs arrange safe starting positions and send real per-process keyboard/mouse input or authenticated production commands. They demonstrate functional paths, not human comprehension. No human usability or cross-machine LAN playtest has been performed.
'''
write(ROOT/'Docs/Design/phase8A-playtest.md',playtest)

system=ROOT/'Docs/Design/phase8a-ui-design-system.md'
text=system.read_text(encoding='utf-8-sig').replace('Status: Stage A implementation in progress; native acceptance is recorded separately.','Status: Rescue V2 runtime implemented; native acceptance and remaining limits are recorded in the review gallery.').replace('1280 ? 720','1280 × 720').replace('640 ? 514','640 × 514').replace('1920 ? 1080','1920 × 1080').replace('1366 ? 768','1366 × 768').replace('1920 ? 1200','1920 × 1200').replace('40?44','40–44').replace('24?28','24–28').replace('14?16','14–16')
text=text.split('\n## Delivered runtime and evidence')[0]+'''
## Delivered runtime and evidence

`Assets/RescueV2/Resources/RescueV2/WorkdayCanvas.prefab`, `Components/PaperPanel.prefab`, `Components/ReadingText.prefab` and `WorkdaySkin.asset` are the shared editable Unity sources. Runtime builders use those panel/text foundations for buttons, tabs, glyphs, step tiles, record rows and feedback. Button hover/focus/press/disabled states come from RescueButtonFeedback. Twenty-two original pictograms retain editable SVG counterparts; six licensed static fonts retain their OFL/source manifests. Final choice is Bricolage + Atkinson; Dela + Atkinson and Fredoka + Atkinson are actual native comparisons.

The Figma file contains foundations, three layout comparisons, three typography comparisons, reusable variants and 16 editable production screens (87 instances, 170 text nodes). Production icon scale/button-height fixes were applied. Figma's Starter MCP call limit prevented fetching the final post-fix render; the final Unity canvases and original vector sources are the fully inspected editable equivalent. Earlier Figma images remain labelled as intermediate evidence, not final acceptance captures.

The native screen matrix covers main/join, staff desk, induction controls/job, HUD, handbook tabs, map, pause, settings/display, controls and report at 720p and 1080p. Same-process resizing covers 1366×768, 1280×720 and 1920×1080. Six 1920×1200 compositions are genuine Unity offscreen renders because this desktop cannot fit a native 1200px window. See the [raw validation and gallery](../Diagnostics/Phase8ARescueV2/review.html); no browser reconstruction is presented as game evidence.
'''
write(system,text)
book=ROOT/'Docs/Design/phase8a-handbook-redesign.md'
text=book.read_text(encoding='utf-8-sig').split('## Verification status')[0]+'''## Verification status

The new clipboard is implemented as a native TMP/uGUI interface. Native 720p and 1080p captures compare the three real font options, with Bricolage + Atkinson selected. JOB, ITEMS, RECORDS and TOWN render without reported text overflow; actual pointer/keyboard flows cover solo start, profession choice, introduction and reopening views. Resizing and a 1920×1200 Unity render are captured separately. Records contain only the targeted profession's copies; public inventory snapshots omit audit truth.

All three solo professions complete a conserved shift. Native repair, stocking/checkout, cart handling, sorting and report evidence is linked from the [before/after gallery](../Diagnostics/Phase8ARescueV2/review.html). Technical automation does not establish first-time-player comprehension. The Figma Starter call limit prevented final post-fix screenshot retrieval; final Unity canvases remain editable and visually inspected. Known art/transition/performance limits are recorded alongside the evidence.
'''
write(book,text)

rows='\n'.join(f"| {s['suite']} | {s['passed']}/{s['total']} | [{s['path']}](../Diagnostics/Phase8ARescueV2/{s['path']}) |" for s in suites)
perfpath=OUT/'FinalUI1080Validated/performance-sample.json'
perf=load(perfpath).get('performance',{}) if perfpath.exists() else {}
performance=(f"Observed native single-player UI/resizing sample on {perf.get('gpu')}, {perf.get('cpu')}, {perf.get('graphics')}: {perf.get('samples')} sampled frames, median {perf.get('medianFrameMs',0):.2f} ms, p95 {perf.get('p95FrameMs',0):.2f} ms. The final photographed frame reported {perf.get('drawCalls')} draw calls, {perf.get('batches')} batches and {perf.get('triangles'):,} triangles. Rendering counters were available. This mixed menu/capture/resizing run is not a controlled GPU benchmark; do not convert it to a promised framerate. The first 12,000 post-warmup frame intervals are retained." if perf else 'Performance evidence pending.')
report=f'''# Phase 8A Rescue V2 — verification and limits

The rescue is implemented in a separate Windows development build. [Launch/playtest instructions](phase8A-playtest.md), [native before/after gallery](../Diagnostics/Phase8ARescueV2/review.html), [research](phase8a-ui-ux-research.md), [design system](phase8a-ui-design-system.md), and [editable Figma](https://www.figma.com/design/J2QJX5ZjjCNuMgbK2achnI).

## Delivered

One-window solo launch; illustrated job selection and induction; compact clipboard/HUD/map/prompts; targeted work slips, inventory and receipts; pause/settings/report; Meshy T2 wagon, engine, open collection cart and carton; Blender-fitted and articulated bonnet/wheels; smaller grounded workbenches; real battery handling; matched stocking and checkout; three-batch cart collection/sorting; seven authored glove gestures with tools; bounded remote foot planting; shared server authority and labelled solo services. Original scenes, materials, prefabs and prior builds are retained. Ordinary jobs and partial evidence remain the core identity.

## Native results

| Suite | Passed | Evidence |
|---|---|---|
{rows}

Physical-domain assertions: **367**. Legacy regression: **10,385** including four workflows and 10,000 seeded commands. Build: **{summary['build']['result']}**, {summary['build']['errors']} errors, {summary['build']['warnings']} warning(s). Build/assembly fingerprints are in validation-summary.json and per-run metadata. Mechanic and four-process evidence precede the final staff-screen routing and diagnostic sampling-only revision; later suites record the final assembly. Earlier failed attempts remain as history and are excluded from this acceptance table.

Tests use real Windows standalone players. Fixtures teleport to safe work points; interaction either uses virtual keyboard/mouse devices belonging to that player process or the authenticated production RPC boundary. Solo mechanic checks real W collision against car and bench and actual mouse clamp turning. Retail crosses the doorway carrying the same parcel. Sanitation pushes a loaded cart, selects actual batches and files them. Two/four processes check conserved custody, replay rejection, disconnect recovery, profession privacy, and exactly one owned camera/listener/input/canvas. Remote walking exercises the presentation foot plant; it is not proof that sliding is eliminated at all speeds.

Native 720p/1080p screens and 1366×768 resizing report no text-bound/overflow defects in tested states. Six 1920×1200 images are Unity offscreen compositions, not native desktop-window tests. Original PNGs are unmodified. GIFs are downsampled real capture sequences. Screenshots were visually inspected; automated bounds alone were not used to judge art. Final polish corrects cracker UVs on a separate mesh/material, resizes imported fixture wrappers only once, narrows the grip for stocked retail units, distinguishes municipal from human colleagues in waiting instructions, and reuses/disposes the highlight material. FinalRetailGrounded and FinalArtGrounded exercise the last assembly; the other suites retain their earlier fingerprints and source-revision context.

## Performance

{performance}

Diagnostics sample percentiles at most once per second and are opt-in for the test driver. Cart movement is replicated at 10 Hz independently of inventory revision; wheel motion follows measured displacement. The town-view camera updates at 10 Hz only while inside the store. The inherited town still has many materials and repeated props; broad batching/LOD/occlusion work remains outside this focused rescue. Multi-process frame times are contention measurements on one machine, not multiplayer hardware requirements.

## Remaining defects and limits

- **No human usability test.** Automated completion does not prove that an unfamiliar player understands the route or every affordance. The documented first-time-player protocol remains required before calling onboarding validated with players.
- **No cross-machine LAN test.** Two and four native processes use loopback on this PC. Firewalls, packet loss and real voice/social interactions are unverified.
- **Store transition.** A framed live town view replaces the blank doorway; walking crosses to the same town with conserved inventory. The interior remains offset, and the view is fixed rather than a seamless, parallax-correct architectural portal.
- **Art.** Primary props are grounded and scale coherent in inspected views. Meshy wagon texture/details are stylized and contain uneven generated paint; engine material treatment is matte. Authored gloves are segmented FK meshes, not a skinned hand rig or physics finger contact. Tools follow authored poses; attachments and generic sorting/checkout gestures remain approximate. Existing secondary store props and decorative packaging can still have awkward labels.
- **Remote animation.** Short standing/walking captures were inspected and foot planting is active. Running, jumps, slopes, network jitter and turn transitions need a dedicated animation playtest; no perfect-contact claim.
- **Figma limit.** Sixteen editable production screens and reusable components exist. Production icon/button fixes were applied, but the Figma Starter MCP quota blocked retrieving the final post-fix render. Earlier Figma screenshots are intermediate. Final native Unity layouts provide the inspected editable equivalent.
- **Prototype scope.** Three retail units, one repair, three waste batches and one workday are intentionally finite. Municipal services stand in for absent solo professions and use the same objects; they are not fully simulated NPC coworkers. Two humans without the third lead cannot complete the whole town through hidden services. The assistant supports existing tasks but is not a fourth full profession.
- **Build warnings.** The installed unrelated Pipeline package reports missing RuntimePipelineConfig and disables that package in player builds. The game's URP rendering is active in the recorded native images. Existing Sentis shader warnings appear when affected assets are rebuilt; the final build's raw warning count is recorded above. This rescue did not repair those unrelated packages.

## Credits and preservation

Meshy cumulative usage is **{ledger['spentTotal']}/{ledger['limit']} credits**, **{ledger['remaining']} remaining**, no reservations. Rescue added 65: wagon 15, engine 15, rejected closed-cart preview 5, open cart 15, carton 15. Rejected generation is charged and retained in the ledger. Blender authored glove clips, bonnet cleanup and wheel separation incurred no Meshy credits. [Ledger](../meshy-credit-ledger.json).

The pre-rescue baseline contains {len(manifest):,} hashes/editable originals at `{baseline}`. [Preservation audit](../Diagnostics/Phase8ARescueV2/preservation.json) records changes and confirms index stage entries unchanged: **{preservation['indexStagesUnchanged']}**. No commit/push/reset/stage/discard was performed. Original scene/material/prefab hashes are unchanged. Build-only preloadedAssets mutation was restored from this baseline. The AST graph is refreshed separately without LLM API cost. [Asset provenance and imported triangle counts](../Diagnostics/Phase8ARescueV2/asset-provenance.json) distinguish generated sources from Blender cleanup and original project assets.
'''
write(ROOT/'Docs/Design/phase8a-rescue-v2-verification.md',report)
write(OUT/'progress.md','# Rescue V2 delivery status\n\nStages A–D implemented. Native validation, editable artifacts, credits, preservation and remaining limits are recorded in [verification](../../Design/phase8a-rescue-v2-verification.md) and the [review gallery](review.html). No human usability or cross-machine LAN test is claimed.\n')

def figure(path,title,caption='Unmodified native Unity capture; click for original.'):
    p=ROOT/path
    if not p.exists():return ''
    import os
    rel=Path(os.path.relpath(p,OUT)).as_posix()
    return f'<figure><a href="{html.escape(rel)}"><img loading="lazy" src="{html.escape(rel)}" alt="{html.escape(title)}"></a><figcaption><strong>{html.escape(title)}</strong><br>{html.escape(caption)}</figcaption></figure>'
pairs=[('Handbook','Docs/Diagnostics/Phase8A/FinalFour720/client1-service-slip.png','Docs/Diagnostics/Phase8ARescueV2/FinalUI720Validated/host-typography-0.png'),('Staff desk','Docs/Diagnostics/Phase8A/FinalFour720/host-staff-desk.png','Docs/Diagnostics/Phase8ARescueV2/FinalUI720Validated/host-staff.png'),('Garage','Docs/Diagnostics/Phase8A/FinalFour720/client1-repair-view.png','Docs/Diagnostics/Phase8ARescueV2/FinalMechanic/host-terminal-before.png')]
cards=[]
for title,before,after in pairs:cards.append('<section><h2>'+title+' — before / after</h2><div class="pair">'+figure(before,title+' before')+figure(after,title+' after')+'</div></section>')
cards.append('<section><h2>Final interface — 720p / 1080p</h2><div class="grid">')
for folder in ['FinalUI720Validated','FinalUI1080Validated']:
    for name in ['main','join','staff','illustrated-controls','illustrated-job','compact-hud','typography-0','typography-1','typography-2','items','records','town','map','pause','settings','display','controls','report']:
        cards.append(figure(f'Docs/Diagnostics/Phase8ARescueV2/{folder}/host-{name}.png',folder+' / '+name))
cards.append('</div></section><section><h2>Native resizing / 1920×1200 render</h2><div class="grid">')
for p in sorted((OUT/'FinalUI1080Validated').glob('host-*')):
    if p.suffix=='.png' and ('resized-' in p.name or '1920x1200-' in p.name):cards.append(figure(p.relative_to(ROOT),p.stem,'Native Unity render; 1200px images use an offscreen target, resize images use an actual window.'))
cards.append('</div></section><section><h2>Physical work and records</h2><div class="grid">')
for folder,names in [('FinalMechanic',['probes-before','terminal-before','lift-battery-after','scrap-tray-after','replacement-after','socket-after','repair-test-after','service-ticket-after','completed-shift']),('FinalRetailGrounded',['framed-town-view','stock-shelf-0-after','order-register-after','retail-receipt','completed-shift']),('FinalSanitationValidated',['loaded-cart','mixed-chute-after','collection-manifest','completed-shift']),('FinalFourProcess',['remote-walk','private-record-copy']),('FinalArtGrounded',['checkout-packaging','checkout-collision','held-crackers','stock-inventory'])]:
    for name in names:cards.append(figure(f'Docs/Diagnostics/Phase8ARescueV2/{folder}/host-{name}.png',folder+' / '+name))
cards.append('</div></section><section><h2>Actual motion capture sequences</h2><div class="grid">')
for folder in ['FinalMechanic','FinalRetailGrounded','FinalSanitationValidated','FinalTwoProcess']:
    parent=OUT/folder;groups={}
    for p in parent.glob('*-preview-*.png'):groups.setdefault(p.stem.rsplit('-',1)[0],[]).append(p)
    for name,frames in groups.items():
        if len(frames)<3:continue
        gif=OUT/(folder+'-'+name+'.gif')
        if not gif.exists() or gif.stat().st_mtime<max(p.stat().st_mtime for p in frames):
            images=[]
            for p in sorted(frames):
                im=Image.open(p).convert('RGB');im.thumbnail((768,480));images.append(im.quantize(colors=128,method=Image.Quantize.FASTOCTREE))
            images[0].save(gif,save_all=True,append_images=images[1:],duration=110,loop=0,optimize=False)
        cards.append(figure(gif.relative_to(ROOT),folder+' / '+name,'Downsampled real native frames. Source PNG sequence retained; no generated or edited gameplay.'))
cards.append('</div></section>')
table=''.join(f'<tr><td>{s["suite"]}</td><td>{s["passed"]}/{s["total"]}</td><td><a href="{s["path"]}">Raw checks</a></td></tr>' for s in suites)
page='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Business as Usual — Rescue V2 review</title><style>body{margin:0;background:#152e2d;color:#f7ebcd;font:17px system-ui;line-height:1.5}main{max-width:1440px;margin:auto;padding:32px}h1{font-size:38px}h2{border-top:1px solid #59736c;padding-top:24px}a{color:#e7b647}.grid,.pair{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:18px}figure{margin:0 0 20px;background:#203f40;border-radius:10px;overflow:hidden}img{width:100%;height:auto;display:block}figcaption{padding:12px}table{border-collapse:collapse}td,th{text-align:left;padding:8px 16px;border-bottom:1px solid #59736c}@media(max-width:800px){.grid,.pair{grid-template-columns:1fr}main{padding:16px}}</style><main><h1>Ordinary jobs, extraordinary suspicion.</h1><p>Phase 8A Rescue V2 — native game evidence, original screenshots, editable source and honest limits.</p><p><a href="../../Design/phase8A-playtest.md">Launch and play</a> · <a href="../../Design/phase8a-rescue-v2-verification.md">Verification and limits</a> · <a href="../../Design/phase8a-ui-ux-research.md">Research</a> · <a href="https://www.figma.com/design/J2QJX5ZjjCNuMgbK2achnI">Editable Figma</a> · <a href="validation-summary.json">Build fingerprints</a> · <a href="preservation.json">Preservation</a></p><p>The launcher defaults to one solo window. Automated fixtures validate functions; no human usability or cross-machine LAN test is claimed. 1920×1200 captures are Unity offscreen renders. Intermediate failed attempts and pre-fix Figma previews remain outside this gallery.</p><table><thead><tr><th>Native suite</th><th>Passed</th><th>Evidence</th></tr></thead><tbody>'''+table+'</tbody></table>'+''.join(cards)+'</main></html>'
write(OUT/'review.html',page)
print('Packaged',len(suites),'native suites;',len(cards),'gallery entries; preserved index:',preservation['indexStagesUnchanged'])
