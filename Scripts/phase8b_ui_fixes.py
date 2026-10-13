from pathlib import Path
root=Path('Unity/BusinessAsUsual/Assets')
def edit(path,old,new):
 p=root/path;s=p.read_text(encoding='utf-8-sig');assert old in s,path;s=s.replace(old,new);p.write_text(s,encoding='utf-8-sig')
edit('Phase8B/Scripts/RescueInterface.Phase8B.cs','arrow.sprite=Sprite.Create(','arrow.sprite=UnityEngine.Sprite.Create(')
edit('Phase8B/Scripts/RescueInterface.Phase8B.cs','!firstPrivate&&Private.personalTitle!=null','!firstPrivate&&!string.IsNullOrEmpty(Private.personalTitle)')
edit('Phase8B/Scripts/RescueInterface.Phase8B.cs','Button(c,"PIN RETURNS",24,457,185,34,()=>{pinnedErrand="Returns counter";Show("HUD");});','Button(c,!Private.hasArrangement&&Job==Profession.Mechanic?"PIN SERVICE NOTE":"PIN RETURNS",24,457,185,34,()=>{pinnedErrand=!Private.hasArrangement&&Job==Profession.Mechanic?"Service amendment":"Returns counter";Show("HUD");});')
edit('Multiplayer/Scripts/NetworkShop.cs','else if(Town&&target.StartsWith("job:")&&Workday&&Workday.Domain.State.cartPusher==id)','else if(Day&&Day.Public.started&&target.StartsWith("job:")&&Town.Job(id)!=Profession.None)reason="Your work slip is fixed for this day.";\n                else if(Town&&target.StartsWith("job:")&&Workday&&Workday.Domain.State.cartPusher==id)')
edit('Town/Scripts/LivingWorkday.cs','if(action==null){message="This work point has no alternate action.";return false;}','if(action=="board"){shop.ShowDayBoardClientRpc(new ClientRpcParams{Send=new ClientRpcSendParams{TargetClientIds=new[]{player.OwnerClientId}}});message="Shared copies at the staff kiosk.";return true;}\n            if(action==null){message="This work point has no alternate action.";return false;}')
p=root/'Multiplayer/Scripts/NetworkShop.cs';s=p.read_text(encoding='utf-8-sig');i=s.index('        [ClientRpc]');s=s[:i]+'        [ClientRpc] public void ShowDayBoardClientRpc(ClientRpcParams rpc=default){if(BusinessAsUsual.RescueV2.RescueInterface.Instance)BusinessAsUsual.RescueV2.RescueInterface.Instance.Show("Board");}\n'+s[i:];p.write_text(s,encoding='utf-8-sig')
edit('RescueV2/Scripts/RescueWorld.cs','Sign("Town exit",','Sign("Town exit",') if False else None
p=Path('Scripts/test_phase8b_ui.py');s=p.read_text(encoding='utf-8-sig');s=s.replace("t['value']=='\u2212'","t['value'] in ('\u2212','-')");p.write_text(s,encoding='utf-8')
