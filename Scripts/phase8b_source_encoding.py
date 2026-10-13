from pathlib import Path
root=Path('Unity/BusinessAsUsual/Assets')
paths=list((root/'Phase8B').rglob('*.cs'))+[root/p for p in ['Multiplayer/Scripts/NetworkShop.cs','Multiplayer/Scripts/NetworkSession.cs','Multiplayer/Scripts/NetworkTestDriver.cs','Town/Scripts/LivingWorkday.cs','Town/Scripts/WorkdayDomain.cs','RescueV2/Scripts/RescueInterface.cs','RescueV2/Scripts/RescueWorld.cs']]
for p in paths:
 s=p.read_text(encoding='utf-8-sig')
 # apply_patch's local transport can decode new non-ASCII source literals as Windows-1252.
 for original in ['−','°','·','→','‹','›','×','—','–']:
  broken=original.encode('utf-8').decode('cp1252');s=s.replace(broken,original)
 p.write_text(s,encoding='utf-8-sig')
print('Normalized',len(paths),'authorized source files to UTF-8 with BOM.')
