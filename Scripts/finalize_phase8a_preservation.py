"""Restore build-only writes and prove protected pre-task content and index remain intact."""
from pathlib import Path
import hashlib,json,shutil,subprocess
root=Path(__file__).resolve().parents[1];backup=Path('D:/Business as Usual Backups/pre-phase8A-2026-10-12')
manifest=json.loads((backup/'manifest.json').read_text());out=root/'Docs/Diagnostics/Phase8A';out.mkdir(parents=True,exist_ok=True)
side_effects=['Unity/BusinessAsUsual/ProjectSettings/ProjectSettings.asset','Unity/BusinessAsUsual/Assets/DefaultNetworkPrefabs.asset','Unity/BusinessAsUsual/Assets/Shopkeeper/Materials/Shop_Paper.mat','Unity/BusinessAsUsual/Assets/Shopkeeper/Materials/Shop_Screen.mat']
restored=[]
for name in side_effects:
    prior=backup/name;path=root/name
    if prior.is_file() and hashlib.sha256(path.read_bytes()).hexdigest()!=manifest[name]:shutil.copy2(prior,path);restored.append(name)
intentional={
 'Unity/BusinessAsUsual/Assets/Multiplayer/Scripts/'+name+'.cs' for name in ['NetworkSession','NetworkShop','NetworkPlayer','NetworkTestDriver']
}|{'Unity/BusinessAsUsual/Assets/Town/Scripts/'+name+'.cs' for name in ['TownJobs','TownHandbook','TownGuidance']}|{'Unity/BusinessAsUsual/Assets/FirstPerson/Scripts/'+name+'.cs' for name in ['FirstPersonHands','FirstPersonView']}
changed=[];protected=[];runtime_logs=[]
for name,expected in manifest.items():
    path=root/name;actual=hashlib.sha256(path.read_bytes()).hexdigest() if path.exists() else None
    if actual!=expected:
        if name in intentional:changed.append(name)
        elif name.endswith('/editor-recovery.log'):runtime_logs.append(name)
        else:protected.append(name)
tree=subprocess.check_output(['git','write-tree'],cwd=root).decode().strip();initial=(backup/'index-tree.txt').read_text().strip()
report=dict(backup=str(backup),indexTree=tree,indexUnchanged=tree==initial,intentionalExistingCodeChanges=changed,restoredBuildSideEffects=restored,expectedLiveEditorLogs=runtime_logs,unexpectedProtectedChanges=protected,baselineFilesVerified=len(manifest),meshyCreditsSpent=0)
(out/'preservation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
if protected or tree!=initial:raise SystemExit(1)
