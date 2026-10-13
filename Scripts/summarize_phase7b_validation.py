from pathlib import Path
import json,datetime
root=Path(__file__).resolve().parents[1];out=root/'Docs/Diagnostics/Phase7B';reg=out/'Regression'
files=[
 ('Phase 3 editor',reg/'Phase3/playmode-validation.json'),
 ('Phase 4 editor',reg/'Phase4/playmode-validation.json'),
 ('Phase 4.1 editor',reg/'Phase4.1/polish-validation.json'),
 ('Phase 4.2 editor UI',reg/'Phase4.2/ui-validation.json'),
 ('Phase 7A static',reg/'Phase7A/Editor/static-validation.json'),
 ('Phase 7A editor play',reg/'Phase7A/Editor/playmode-validation.json'),
 ('Phase 7B targeted 1080p',out/'Targeted1080/validation.json'),
 ('Phase 7B targeted 720p',out/'Targeted720/validation.json'),
 ('Phase 7A multiplayer 720p',reg/'Phase7A720/validation.json'),
 ('Phase 7A multiplayer 1080p',reg/'Phase7A1080/validation.json'),
 ('Phase 6 full',reg/'Phase6Full/validation.json'),
 ('Phase 6 staffed',reg/'Phase6Staffed/validation.json'),
 ('Phase 6 entry',reg/'Phase6Entry/validation.json'),
 ('Phase 6 guidance',reg/'Phase6Guidance/validation.json'),
 ('Phase 5 shop multiplayer (fixed revision contention)',reg/'Phase5Controlled/validation.json'),
 ('Map transforms/lifetime',out/'map-validation.json'),
 ('Atmosphere preview',out/'After/atmosphere-validation.json')]
files.append(('Actual town player Animator binding',out/'Animation/prefab-binding-validation.json'))
suites=[]
for label,path in files:
    if not path.exists():suites.append(dict(suite=label,pending=True));continue
    data=json.loads(path.read_text(encoding='utf-8-sig'));checks=data['checks'];passed=sum(c['passed'] for c in checks)
    suites.append(dict(suite=label,passed=passed,total=len(checks),allPassed=passed==len(checks),report=path.relative_to(root).as_posix(),failures=[c for c in checks if not c['passed']]))
animation=json.loads((out/'Animation/imported-stance-validation.json').read_text())['samples']
for word in ['Walk','Run']:
    before=next(s for s in animation if s['clip']=='Phase7A-'+word)['worldStanceDrift'];after=next(s for s in animation if s['clip']=='Phase7B-'+word)['worldStanceDrift']
    suites.append(dict(suite='Imported '+word.lower()+' middle-stance ankle drift',passed=int(after<.02 and after<before*.1),total=1,allPassed=after<.02 and after<before*.1,beforeMeters=before,afterMeters=after,method='Sampled left ankle, not every sole vertex or human gait comfort.'))
preservation=json.loads((out/'preservation.json').read_text())['checks']
suites.append(dict(suite='Protected source/staging preservation',passed=sum(c['passed'] for c in preservation),total=len(preservation),allPassed=all(c['passed'] for c in preservation),report='Docs/Diagnostics/Phase7B/preservation.json'))
results=dict(generatedUtc=datetime.datetime.now(datetime.timezone.utc).isoformat(),build='Builds/Phase7B/BusinessAsUsual.exe',shopRegressionBuild='Builds/Phase7B-Phase5Regression/BusinessAsUsual.exe',method='Live editor and actual two-process NGO Windows tests, with opt-in placement and synthetic input. Not a human or cross-machine LAN playtest.',meshySpent=75,meshyLimit=300,suites=suites,allPassed=all(s.get('allPassed',False) for s in suites))
(out/'validation-summary.json').write_text(json.dumps(results,indent=2))
for s in suites:print(s['suite']+': '+('pending' if s.get('pending') else str(s['passed'])+'/'+str(s['total'])))
raise SystemExit(0 if results['allPassed'] else 1)
