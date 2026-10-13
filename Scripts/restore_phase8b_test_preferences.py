"""Restore the pre-Phase8B sound/quality baseline through native menu controls."""
from pathlib import Path
s=Path(__file__).with_name('test_phase8b_ui.py');exec(compile(s.read_text(encoding='utf-8-sig').split('\ntry:\n')[0],str(s),'exec'))
try:
 launch();original=json.loads((ROOT/'Docs/Diagnostics/Phase8B/FirstUI720/settings.json').read_text(encoding='utf-8-sig'));token('settings',x=original['master'],y=original['music'],z=original['effects']);pointer('SETTINGS');wait(lambda:ui()['screen']=='Settings' and any(t['value']=='DISPLAY & QUALITY' for t in ui()['text']));pointer('DISPLAY & QUALITY');wait(lambda:ui()['screen']=='Display')
 baseline=json.loads((ROOT/'Docs/Diagnostics/Phase8B/BaselineUI/display.json').read_text(encoding='utf-8-sig'));wanted=next(t['value'] for t in baseline['rescueUI']['text'] if t['value'].startswith('QUALITY /'))
 for _ in range(10):
  current=next(t['value'] for t in ui()['text'] if t['value'].startswith('QUALITY /'))
  if current==wanted:break
  pointer(current)
 check('pre_task_quality_and_sound_restored',current==wanted and all(abs(read('host')[k]-original[k])<.001 for k in ['master','music','effects']));shot('restored-preferences')
finally:
 (OUT/'checks.json').write_text(json.dumps(checks,indent=2))
 for r,p in processes.items():
  if p.poll() is None:cmd(r,'quit');p.wait(timeout=6)
