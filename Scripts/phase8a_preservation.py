"""Capture the pre-8A index and editable source bytes without staging anything."""
from pathlib import Path
import hashlib, json, shutil, subprocess
ROOT = Path(__file__).resolve().parents[1]
BACKUP = Path('D:/Business as Usual Backups/pre-phase8A-2026-10-12')
BACKUP.mkdir(parents=True, exist_ok=True)
if (BACKUP / 'manifest.json').exists():
    raise SystemExit('Existing baseline preserved; refusing to replace it.')
files = subprocess.check_output(['git', 'ls-files', '-z', '--cached', '--others', '--exclude-standard'], cwd=ROOT).decode().split('\0')
manifest = {}
for name in set(files):
    p = ROOT / name
    if p.is_file() and name.startswith(('Unity/BusinessAsUsual/Assets/', 'Unity/BusinessAsUsual/ProjectSettings/', 'Unity/BusinessAsUsual/Packages/', 'Docs/', 'Scripts/')):
        manifest[name] = hashlib.sha256(p.read_bytes()).hexdigest()
        if p.suffix.lower() in ('.cs', '.unity', '.prefab', '.asset', '.mat', '.json', '.md', '.py', '.ps1', '.meta'):
            target = BACKUP / name
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(p, target)
(BACKUP / 'manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf8')
(BACKUP / 'git-status.txt').write_bytes(subprocess.check_output(['git', 'status', '--porcelain=v1'], cwd=ROOT))
(BACKUP / 'index-tree.txt').write_bytes(subprocess.check_output(['git', 'write-tree'], cwd=ROOT))
print(f'Preserved {len(manifest)} file hashes and editable baseline at {BACKUP}')
