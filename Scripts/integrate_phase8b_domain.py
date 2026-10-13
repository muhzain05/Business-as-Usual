from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]/'Unity/BusinessAsUsual/Assets'
def edit(path,old,new):
 p=ROOT/path;s=p.read_text(encoding='utf-8-sig');assert old in s,(path,old[:90]);p.write_text(s.replace(old,new,1),encoding='utf-8')
edit('Town/Scripts/WorkdayDomain.cs','public bool physical,removed,oldPlaced,repairTested,serviceSigned,shiftSigned;','public bool physical,removed,oldPlaced,repairTested,serviceSigned,shiftSigned,suspicious;\n        public string parcelNote="Direct transfer",serviceNote="Initial diagnosis";public int noteAmendments;')
edit('Town/Scripts/WorkdayDomain.cs','public WorkdayDomain(bool physical=false)','public WorkdayDomain(bool physical=false,bool suspicious=false)')
edit('Town/Scripts/WorkdayDomain.cs','physical=physical,items','physical=physical,suspicious=suspicious,items')
actions='''            if(s.suspicious&&(action=="hold-return"||action=="hold-yard")){
                if(profession==0)return false;string place=action=="hold-return"?"returns":"yard-hold";var held=Held(actor);
                if(held!=null){if(s.items.Any(i=>i.custody==place)){message="This hold is occupied. Collect it before restaging another load.";return false;}var group=s.items.Where(i=>i.holder==actor).ToArray();foreach(var i in group)Move(i,place,actor,time);Record(1,group[0].id,"Holding slip","Load restaged at "+place+". Contents and earlier handler not recorded.",time);message="Load supported in the temporary hold. R declares its transfer note.";return true;}
                var load=s.items.FirstOrDefault(i=>i.custody==place);if(load==null){message="Hold is empty. Carry a parcel or return here.";return false;}
                if(part.custody==place&&pack.custody==place){Move(part,"parcel",actor,time,actor);Move(pack,"parcel",actor,time,actor);}else Move(load,load.material=="stock"?"stock-carry":"waste-carry",actor,time,actor);
                message="Load lifted from its hold. Continue the ordinary delivery or collection.";return true;
            }
            if(s.suspicious&&action=="return-note"){
                if(profession==0||!new[]{"returns","yard-hold"}.Contains(part.custody)){message="Restage the sealed repair parcel before writing its note.";return false;}
                s.parcelNote=s.parcelNote=="Direct transfer"?"Held for stock check":"Direct transfer";Observe(part,"declared "+s.parcelNote,actor,time);Record(1,pack.id,"Declared transfer",s.parcelNote+" / destination Garage. This is a declaration, not an observed route.",time);message="Transfer note: "+s.parcelNote+".";return true;
            }
            if(s.suspicious&&action=="amend-service"){
                if(!mechanic||!s.diagnosed||s.noteAmendments>=2){message="Check the vehicle before correcting its service note.";return false;}
                string before=s.serviceNote;s.serviceNote=s.loadChecked?"Load check performed":"Quick reading / recheck advised";s.noteAmendments++;Observe(old,"service note amendment",actor,time);Record(2,old.id,"Service amendment",before+" → "+s.serviceNote+". Correction reason is not independently verified.",time);message="Service note amended; earlier entry retained.";return true;
            }
'''
edit('Town/Scripts/WorkdayDomain.cs','            if(action.StartsWith("cart-unload-")){',actions+'            if(action.StartsWith("cart-unload-")){')
edit('Town/Scripts/LivingWorkday.cs','Domain=new WorkdayDomain(BusinessAsUsual.RescueV2.RescueInterface.Requested);','Domain=new WorkdayDomain(BusinessAsUsual.RescueV2.RescueInterface.Requested,BusinessAsUsual.Phase8B.Phase8BFlow.Enabled);')
edit('Town/Scripts/LivingWorkday.cs','Environment.GetCommandLineArgs().Contains("--bau-rescue");','Environment.GetCommandLineArgs().Contains("--bau-rescue") || BusinessAsUsual.Phase8B.Phase8BFlow.Enabled;')
edit('Town/Scripts/LivingWorkday.cs','Domain.State.physical&&NetworkSession.Instance&&NetworkSession.Instance.Solo','Domain.State.physical&&NetworkSession.Instance&&(NetworkSession.Instance.Solo||shop.Day!=null&&shop.Day.Public.started&&shop.Day.Public.unattended)')
edit('Town/Scripts/LivingWorkday.cs','            bool accepted=Domain.Apply(action,','            bool sealedParcel=Domain.Find("PART-8A-001").holder==player.OwnerClientId;\n            bool accepted=Domain.Apply(action,')
edit('Town/Scripts/LivingWorkday.cs','            if(accepted){\n                if(!support.ContainsKey','            if(accepted){\n                shop.Day?.Accepted(player.OwnerClientId,action=="return-note"&&Domain.State.parcelNote=="Direct transfer"?"note-direct":action,sealedParcel);\n                if(!support.ContainsKey')
edit('RescueV2/Scripts/RescueWorld.cs','        public static Vector3 Stockroom','        public Transform ItemTransform(string id)=>items.TryGetValue(id,out var t)?t:null;\n        public static Vector3 Stockroom')
edit('RescueV2/Scripts/RescueWorld.cs','                if(t.parent&&t.parent.name==t.name)continue;','                if(t.parent&&t.parent.name==t.name)continue;\n                if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled&&t.name=="WasteCart")t.gameObject.SetActive(false);')
edit('RescueV2/Scripts/RescueWorld.cs','bool visible=item.custody!="recorded"','bool visible=item.custody!="unobserved"&&item.custody!="recorded"')
edit('RescueV2/Scripts/RescueWorld.cs','"shipment","market","dispatch","parcel","garage"','"shipment","market","dispatch","parcel","garage","returns","yard-hold"')
edit('RescueV2/Scripts/RescueWorld.cs','                else if(item.custody=="stockroom")','                else if(item.custody=="returns")at=new Vector3(-2.7f,.28f,6.42f);\n                else if(item.custody=="yard-hold")at=new Vector3(11.8f,.28f,-6.52f);\n                else if(item.custody=="stockroom")')
print('Integrated lawful staging, claims, amendments and observation hooks without changing legacy defaults.')
