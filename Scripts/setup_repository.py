"""One-time organization: move exports without changing bytes; no commit or remote."""
from pathlib import Path
import hashlib, json, shutil, subprocess

ROOT = Path(__file__).resolve().parents[1]
if (ROOT / 'Docs/Diagnostics/asset-inventory.json').exists():
    raise RuntimeError('Source inventory already exists; one-time organizer refuses to rerun.')
folders = ['Assets/Characters/Sunny/Source/Original', 'Assets/Characters/Sunny/Source/Rigged',
           'Assets/Characters/Sunny/Source/Animations', 'Assets/Characters/Sunny/Source/Textures',
           'Assets/Characters/Sunny/Source/Archives', 'Assets/Characters/Sunny/Working',
           'Assets/Characters/Sunny/Processed', 'Assets/Characters/Sunny/Rigged',
           'Assets/Animations', 'Assets/Textures', 'Assets/Materials', 'Scripts/Blender', 'Docs/Diagnostics']
for folder in folders:
    (ROOT / folder).mkdir(parents=True, exist_ok=True)
files = list(ROOT.glob('Meshy*.*')) + list((ROOT / 'Meshy_AI_Sunny_Little_Friend_biped').rglob('*'))
inventory = []
for src in files:
    if not src.is_file(): continue
    category = ('Original' if src.suffix == '.blend' else 'Archives' if src.suffix == '.zip' else
                'Textures' if src.suffix == '.png' else 'Animations' if '_Animation_' in src.name else 'Rigged')
    dst = ROOT / 'Assets/Characters/Sunny/Source' / category / src.name
    assert src.resolve().is_relative_to(ROOT) and dst.resolve().is_relative_to(ROOT)
    digest = hashlib.sha256(src.read_bytes()).hexdigest()
    if dst.exists(): raise RuntimeError(f'Destination already exists: {dst}')
    original = str(src.relative_to(ROOT))
    shutil.move(str(src), str(dst))
    assert hashlib.sha256(dst.read_bytes()).hexdigest() == digest
    inventory.append({'original_path': original, 'path': dst.relative_to(ROOT).as_posix(),
                      'bytes': dst.stat().st_size, 'sha256': digest})
(ROOT / 'Docs/Diagnostics/asset-inventory.json').write_text(json.dumps(inventory, indent=2), encoding='utf-8')
if not (ROOT / '.git').exists(): subprocess.run(['git', 'init', '-b', 'main', str(ROOT)], check=True)
for folder in folders:
    if not any((ROOT / folder).iterdir()): (ROOT / folder / '.gitkeep').touch()
print(json.dumps(inventory, indent=2))
