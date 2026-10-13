"""Package actual Windows captures, derived GIF previews, and validation links."""
from pathlib import Path
import html,json
from PIL import Image
root=Path(__file__).resolve().parents[1];out=root/'Docs/Diagnostics/Phase8A';out.mkdir(parents=True,exist_ok=True)
entries=[]
for name in ['FinalFour720','FinalFour1080SanitationAssistant','FinalRecoveryMechanicAssistant','Layout','LegacyPhase6Full','LegacyPhase6Staffed','LegacyPhase6Entry','LegacyGuidance','LegacyPhase7A','LegacyPhase7B','Phase5Controlled']:
    file=out/name/'checks.json'
    if not file.exists():file=out/name/'validation.json'
    if not file.exists():continue
    data=json.loads(file.read_text());checks=data if isinstance(data,list) else data.get('checks',[])
    entries.append(dict(suite=name,passed=sum(bool(c.get('passed')) for c in checks),total=len(checks),path=str(file.relative_to(out)).replace('\\','/')))
for file in (out/'EditorRegression').rglob('*validation.json'):
    data=json.loads(file.read_text());checks=data.get('checks',[])
    entries.append(dict(suite=str(file.parent.relative_to(out)),passed=sum(bool(c.get('passed')) for c in checks),total=len(checks),path=str(file.relative_to(out)).replace('\\','/')))
(out/'validation-summary.json').write_text(json.dumps(dict(suites=entries,humanPlaytest=False,crossMachineLAN=False),indent=2))
previews=[]
for folder in ['FinalFour720','FinalFour1080SanitationAssistant']:
    frames=sorted((out/folder).glob('client1-preview-hood-*.png'))
    if frames:
        images=[]
        for frame in frames:
            image=Image.open(frame).convert('RGB');image.thumbnail((960,600));images.append(image)
        gif=out/(folder+'-hood.gif');images[0].save(gif,save_all=True,append_images=images[1:],duration=120,loop=0)
        previews.append(f'<figure><img src="{gif.name}" alt="Actual Windows hood interaction frames"><figcaption>Actual Windows capture sequence; scripted test controls, downsampled GIF. Original native PNG frames remain in {folder}.</figcaption></figure>')
cards=[]
for file in sorted((out/'Layout').glob('host-staff-*.png')):
    relative=file.relative_to(out).as_posix();cards.append(f'<figure><a href="{relative}"><img loading="lazy" src="{relative}" alt="Staff desk resolution check"></a><figcaption>Single-process layout check / {file.stem}. Original native screenshot.</figcaption></figure>')
for folder in ['FinalFour720','FinalFour1080SanitationAssistant']:
    for name in ['host-staff-desk','client3-carried-parcel','client1-repair-view','client1-service-slip','client2-sanitation-manifests','host-market-receipts','host-market-entry','host-shift-report']:
        file=out/folder/(name+'.png')
        if file.exists():
            relative=file.relative_to(out).as_posix();cards.append(f'<figure><a href="{relative}"><img loading="lazy" src="{relative}" alt="{html.escape(name)}"></a><figcaption>{html.escape(folder+" / "+name)} — click for the unmodified native screenshot.</figcaption></figure>')
rows=''.join(f'<tr><td>{html.escape(e["suite"])}</td><td>{e["passed"]}/{e["total"]}</td><td><a href="{e["path"]}">Raw checks</a></td></tr>' for e in entries)
page='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Phase 8A — Living Workday Review</title>
<style>body{margin:0;background:#244047;color:#244047;font:17px/1.55 system-ui,sans-serif}main{max-width:1400px;margin:auto;padding:32px;background:#f6e9c9}h1{font-size:42px;line-height:1.2}a{color:#24566c}figure{margin:0;background:#fff7e5;border:1px solid #244047;padding:10px}img{display:block;width:100%;height:auto}figcaption{padding:10px 2px;font-size:14px}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(380px,1fr));gap:18px}table{width:100%;border-collapse:collapse;margin:20px 0}td,th{padding:10px;border-bottom:1px solid #b4ab96;text-align:left}.note{padding:18px;background:#efc864}code{font-size:14px} @media(max-width:500px){main{padding:16px}.grid{grid-template-columns:1fr}h1{font-size:30px}}</style>
<main><h1>Phase 8A: The Living Workday</h1><p>One conserved battery shipment connects market dispatch, manual vehicle work, packaging, scrap and sanitation. Four players share three professions and one distinct assistant slot.</p>
<p class="note">These are real Windows test captures and scripted gameplay previews. Automated correctness is not proof of fun. No human playtest or cross-machine LAN result is claimed.</p>
<p><a href="../../phase8A-implementation-report.md">Implementation report</a> · <a href="../../Design/phase8A-playtest.md">Human playtest</a> · <a href="../../Design/phase8B-scoped-plan.md">Phase 8B boundary</a> · <a href="baseline-audit.md">Baseline audit</a> · <a href="preservation.json">Preservation</a></p>
<h2>Validation</h2><table><tr><th>Suite</th><th>Passes</th><th>Evidence</th></tr>ROWS</table><p>Domain testing also exercises four repair/dispatch workflows and 10,000 seeded action sequences. See <a href="domain-tests.txt">the domain output</a>. Failed early iterations remain in FourProcess/ and FourProcess-v2/; final evidence is explicitly labeled.</p>
<h2>Short previews</h2><div class="grid">PREVIEWS</div><h2>Gameplay and paper copies</h2><div class="grid">CARDS</div>
<p>Launch <code>Scripts/launch_phase8a.ps1 -Mode four -Resolution 720</code> for a local technical session. Four humans should use separate machines and the playtest procedure.</p></main></html>'''
(out/'review.html').write_text(page.replace('ROWS',rows).replace('PREVIEWS',''.join(previews)).replace('CARDS',''.join(cards)),encoding='utf8')
print(f'Packaged {len(entries)} suites, {len(cards)} native captures and {len(previews)} GIF previews.')
