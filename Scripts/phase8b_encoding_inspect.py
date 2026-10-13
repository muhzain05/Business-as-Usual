from pathlib import Path
import json
s=Path('Unity/BusinessAsUsual/Assets/Phase8B/Scripts/RescueInterface.Phase8B.cs').read_text(encoding='utf-8-sig')
print(ascii(s[s.index('Button(c,',s.index('void Row')):s.index('Button(c,',s.index('void Row'))+65]))
j=json.loads(Path('Docs/Diagnostics/Phase8B/UI720/settings.json').read_text(encoding='utf-8-sig'))
print([ascii(t['value']) for t in j['rescueUI']['text'] if len(t['value'])<8])
