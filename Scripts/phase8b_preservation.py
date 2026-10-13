from pathlib import Path
import hashlib,json,shutil,subprocess
root=Path(__file__).resolve().parents[1];backup=Path('D:/Business as Usual Backups/pre-phase8B-2026-10-13')
backup.mkdir(parents=True,exist_ok=True)
if (backup/'manifest.json').exists():raise SystemExit('Existing Phase 8B baseline retained')
names=subprocess.check_output(['git','ls-files','-z','--cached','--others','--exclude-standard'],cwd=root).decode().split('\0');manifest={}
for name in sorted(set(names)):
 p=root/name
 if p.is_file() and (name.startswith(('Unity/BusinessAsUsual/Assets/','Unity/BusinessAsUsual/ProjectSettings/','Unity/BusinessAsUsual/Packages/','Assets/','Docs/','Scripts/')) or p.suffix.lower()=='.pdf'):
  manifest[name]=hashlib.sha256(p.read_bytes()).hexdigest()
  if p.suffix.lower() in ('.cs','.unity','.prefab','.asset','.mat','.json','.md','.py','.ps1','.meta'):
   q=backup/name;q.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,q)
shutil.copy2(root/'.git/index',backup/'index')
(backup/'index-stage.txt').write_bytes(subprocess.check_output(['git','ls-files','--stage'],cwd=root))
(backup/'git-status.txt').write_bytes(subprocess.check_output(['git','status','--short'],cwd=root))
(backup/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(f'Preserved {len(manifest):,} baseline hashes/editable originals and exact staged index.')
