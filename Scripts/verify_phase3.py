"""Verify prototype artifacts and protected originals without modifying Unity."""
import hashlib
import json
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DIAG = ROOT / 'Docs/Diagnostics/Phase3'
UNITY = ROOT / 'Unity/BusinessAsUsual'

def digest(path):
    h = hashlib.sha256()
    with path.open('rb') as handle:
        for data in iter(lambda: handle.read(1024*1024), b''):
            h.update(data)
    return h.hexdigest()

def git(*args):
    return subprocess.run(['git', *args], cwd=ROOT, text=True, capture_output=True)

sources = json.loads((ROOT / 'Docs/Diagnostics/asset-inventory.json').read_text())
for item in sources:
    assert digest(ROOT / item['path']) == item['sha256'], item['path']
projects = {}
for version in ['v001', 'v002', 'v003', 'v004']:
    item = json.loads((ROOT / f'Docs/Diagnostics/Phase2/{version}/saved-project.json').read_text())
    path = item.get('project') or item['file']
    assert digest(ROOT / path) == item['sha256'], path
    projects[path] = item['sha256']
phase1 = 'Assets/Characters/Sunny/Working/Sunny_Phase1_Audit.blend'
assert digest(ROOT / phase1) == '965b0370c530bc46105beffe580a17e1ddf805785b77f88365f6a57f13633df0'
projects[phase1] = digest(ROOT / phase1)
export = json.loads((DIAG / 'sunny-export.json').read_text())
for item in export['exports']:
    assert digest(UNITY / 'Assets/Prototype/Art/Sunny' / item['file']) == item['sha256']
report = json.loads((DIAG / 'playmode-validation.json').read_text())
assert report['checks'] and all(c['passed'] for c in report['checks']), 'Live checks failed'
assert report['humanInputTested'] is False
assets = list((UNITY / 'Assets/Prototype').rglob('*'))
missing_meta = [str(p.relative_to(UNITY)) for p in assets if not p.name.endswith('.meta') and not Path(str(p)+'.meta').is_file()]
assert not missing_meta, missing_meta
required = ['Assets/Prototype/Scenes/TownPrototype.unity', 'Assets/Prototype/Prefabs/SunnyPlayer.prefab', 'Assets/Prototype/Animation/SunnyLocomotion.controller']
for path in required:
    assert (UNITY / path).is_file(), path
build_settings = (UNITY / 'ProjectSettings/EditorBuildSettings.asset').read_text()
assert required[0] in build_settings and 'Assets/Scenes/SampleScene.unity' in build_settings
assert 'activeInputHandler: 1' in (UNITY / 'ProjectSettings/ProjectSettings.asset').read_text()
assert json.loads((UNITY / 'Packages/manifest.json').read_text())['dependencies']['com.unity.inputsystem'] == '1.20.0'
for path in ['Library/test', 'Temp/test', 'Logs/test', 'obj/test', 'UserSettings/test', 'Builds/Phase3/test', 'Assembly-CSharp.csproj', 'BusinessAsUsual.sln']:
    assert git('check-ignore', 'Unity/BusinessAsUsual/' + path).returncode == 0, path
assert git('diff', '--cached', '--name-only').stdout.strip() == ''
assert git('remote').stdout.strip() == ''
assert git('rev-parse', '--verify', 'HEAD').returncode != 0, 'Unexpected commit'
for path in [ROOT/'README.md', ROOT/'Docs/phase3-first-playable.md', ROOT/'Docs/phase3-validation.md']:
    for target in re.findall(r'\[[^\]]*\]\(([^)]+)\)', path.read_text(encoding='utf-8')):
        if '://' not in target and not target.startswith('#'):
            assert (path.parent / target.split('#')[0]).exists(), (path.name,target)
for image in ['player-camera.png','town-overview.png']:
    assert (DIAG/image).stat().st_size > 10000
build = json.loads((DIAG/'windows-build.json').read_text())
assert build['result'] == 'Succeeded' and build['errors'] == 0
assert (UNITY/'Builds/Phase3/BusinessAsUsual.exe').is_file()
smoke = json.loads((DIAG/'player-startup-smoke.json').read_text())
assert smoke['engine_initialization_observed'] and smoke['logged_exceptions'] == 0
summary = {'source_assets_sha256_verified':len(sources),'blender_projects_sha256_verified':len(projects),'export_hashes_verified':len(export['exports']),'playmode_checks_passed':len(report['checks']),'human_input_tested':False,'unity_assets_and_meta_verified':len(assets),'windows_build_result':build['result'],'windows_build_errors':build['errors'],'windows_build_warnings':build['warnings'],'caches_and_builds_ignored':True,'staged_files':0,'commits':0,'remotes':0,'required_artifacts':{p:digest(UNITY/p) for p in required}}
(DIAG/'artifact-verification.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
print(json.dumps(summary,indent=2))
