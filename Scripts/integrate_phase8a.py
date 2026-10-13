"""One-shot checked integration edits; baseline copies are preserved separately."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]/'Unity/BusinessAsUsual/Assets'
def edit(path, changes):
    p=ROOT/path;s=p.read_text(encoding='utf8')
    for before,after in changes:
        assert before in s,(path,before[:90])
        s=s.replace(before,after,1)
    p.write_text(s,encoding='utf8')
edit('Town/Scripts/WorkdayDomain.cs', [('diagnosed, checked, hood','diagnosed, loadChecked, hood'),('s.checked','s.loadChecked'),('s.checked','s.loadChecked'),('s.checked','s.loadChecked'),('checked=State.checked','loadChecked=State.loadChecked')])
edit('Town/Scripts/LivingWorkday.cs',[('tasks[i].label','tasks[i].description')])
edit('Multiplayer/Scripts/NetworkSession.cs',[
('response.Approved=Manager.ConnectedClientsIds.Count<2;', 'int capacity=FindFirstObjectByType<BusinessAsUsual.Town.TownJobs>()?4:2; response.Approved=Manager.ConnectedClientsIds.Count<capacity;'),
('"This prototype supports two players."','"This shift is full ("+capacity+" players)."'),
('theme.Text(620,390,"Two-player LAN / localhost shift",26,0,650);','theme.Text(620,390,FindFirstObjectByType<BusinessAsUsual.Town.TownJobs>()?"Four-player LAN / living workday":"Two-player LAN / localhost shift",26,0,650);')])
edit('Town/Scripts/TownJobs.cs',[
('public ulong[] Owners=','public int AssistantProfession=1;\n        public ulong[] Owners='),
('return i<0?Profession.None:(Profession)(i+1);','return i<0?Profession.None:i==3?(Profession)AssistantProfession:(Profession)(i+1);'),
('public TaskBoard Tasks(Profession job)=>','public TaskBoard Tasks(Profession job)=>adapter.Workday?adapter.Workday.Tasks(job):'),
('requested>3)return false;','requested>(adapter.Workday?6:3))return false;\n            if(adapter.Workday&&requested>3) {\n                ulong who=player.OwnerClientId;int previous=Array.IndexOf(Owners,who);\n                if(Owners[3]!=NetworkShop.Nobody&&Owners[3]!=who){reason="The shift already has an assistant.";return false;}\n                if(adapter.Workday.Holding(who)){reason="Return your carried load first.";return false;}\n                if(previous>=0&&(adapter.Workday.Domain.State.received||adapter.Workday.Domain.State.requested)){reason="Keep your assignment until this workday is filed.";return false;}\n                if(previous>=0)Owners[previous]=NetworkShop.Nobody;\n                Owners[3]=who;AssistantProfession=requested-3;reason="Shift assistant / "+((Profession)AssistantProfession);Tell(player,reason);return true;\n            }'),
('if(old==slot){reason="You already have this profession.";return false;}','if(old==slot){reason="You already have this profession.";return false;}\n            if(adapter.Workday&&(adapter.Workday.Holding(id)||old>=0&&(adapter.Workday.Domain.State.received||adapter.Workday.Domain.State.requested))){reason="Keep your assignment and return carried work first.";return false;}'),
('public bool Disconnected(ulong id)\n        {','public bool Disconnected(ulong id)\n        {\n            if(adapter.Workday)adapter.Workday.Domain.Disconnect(id,adapter.Workday.Now);'),
('public int contribution;','public int contribution,assistantProfession;'),
('new State{owners=Owners','new State{assistantProfession=AssistantProfession,owners=Owners'),
('{Owners=state.owners;','{AssistantProfession=state.assistantProfession;Owners=state.owners;')])
edit('Multiplayer/Scripts/NetworkShop.cs',[
('public TownJobs Town { get; private set; }','public TownJobs Town { get; private set; }\n        public LivingWorkday Workday { get; private set; }\n        void Start(){if(Town&&LivingWorkday.Requested){Workday=gameObject.AddComponent<LivingWorkday>();Workday.Initialize(this);}}'),
('else if(target=="return"||target=="drop") {','else if(target=="return"||target=="drop") {\n                    if(Workday&&Workday.Holding(id)){accepted=Workday.Domain.Apply("return",(int)Town.Job(id),Workday.IsAssistant(id),id,Workday.Now,out reason);if(accepted)player.Interactor.Notify(reason);} else'),
('} else if(!Targets.TryGetValue(target,out var item))','} else if(!Targets.TryGetValue(target.StartsWith("alt:")?target.Substring(4):target,out var item))'),
('else if(Town&&(item is TownStation||item is WasteBag))','else if(Workday&&item is WorkdayStation wd)accepted=Workday.Use(player,wd,target.StartsWith("alt:"),out reason);\n                else if(Town&&(item is TownStation||item is WasteBag))'),
('public TownJobs.State town;','public TownJobs.State town;\n            public WorkdayState workday;'),
('return new Snapshot { town=','return new Snapshot { workday=Workday?Workday.Capture():null,town='),
('StateClientRpc(json);','StateClientRpc(json);\n            if(Workday)foreach(var client in NetworkManager.ConnectedClientsIds)\n                WorkRecordsClientRpc(JsonUtility.ToJson(new RecordEnvelope{records=Workday.RecordsFor(client)}),new ClientRpcParams{Send=new ClientRpcSendParams{TargetClientIds=new[]{client}}});'),
('        [ClientRpc]\n        void StateClientRpc','        [Serializable] public class RecordEnvelope {public WorkRecord[] records;}\n        [ClientRpc] void WorkRecordsClientRpc(string json,ClientRpcParams delivery=default){if(Workday)Workday.LocalRecords=JsonUtility.FromJson<RecordEnvelope>(json).records;}\n        [ClientRpc]\n        void StateClientRpc'),
('if(Town&&state.town!=null)Town.Restore(state.town);','if(Town&&state.town!=null)Town.Restore(state.town);\n            if(Workday&&state.workday!=null)Workday.Restore(state.workday);')])
edit('Multiplayer/Scripts/NetworkPlayer.cs',[
('(NetworkShop.Instance.Town&&NetworkShop.Instance.Town.HasBag(OwnerClientId))','(NetworkShop.Instance.Town&&NetworkShop.Instance.Town.HasBag(OwnerClientId))||(NetworkShop.Instance.Workday&&NetworkShop.Instance.Workday.Holding(OwnerClientId))'),
('new Vector3(OwnerClientId==0?-.65f:.65f,.08f,-.8f)','new Vector3(((int)(OwnerClientId%4)-1.5f)*.75f,.08f,-.8f)'),
('var town=NetworkShop.Instance.Town;','var town=NetworkShop.Instance.Town;\n            if(NetworkShop.Instance.Workday&&Interactor.Current is WorkdayStation){SendCommand(NetworkShop.Id(Interactor.Current));if(hands&&hands.enabled)hands.Play(FirstPersonHands.Gesture.Reach,Interactor.Current.InteractionPoint);return true;}'),
('if(IsOwner){Interactor.Notify(message);','if(IsOwner){if(hands&&hands.enabled&&NetworkShop.Instance.Workday)hands.Play(FirstPersonHands.Gesture.Reach,Interactor.Current?Interactor.Current.InteractionPoint:transform.position+transform.forward);Interactor.Notify(message);')])
edit('FirstPerson/Scripts/FirstPersonHands.cs',[
('bag = jobs && jobs.HasBag(me) || previewCarry == 2','bag = jobs && jobs.HasBag(me) || shop && shop.Workday && shop.Workday.Holding(me) || previewCarry == 2')])
edit('Town/Scripts/TownGuidance.cs',[
('var view=new TownGuidance {job=shop.Town.Job(player.OwnerClientId)};','if(shop.Workday)return shop.Workday.Guidance(player);\n            var view=new TownGuidance {job=shop.Town.Job(player.OwnerClientId)};')])
edit('Town/Scripts/TownHandbook.cs',[
('tasks=job==Profession.None?Array.Empty<JobTask>():town.Tasks(job).Tasks.ToArray();','tasks=job==Profession.None?Array.Empty<JobTask>():NetworkShop.Instance.Workday?NetworkShop.Instance.Workday.Tasks(job,NetworkShop.Instance.Workday.IsAssistant(player.OwnerClientId)).Tasks.ToArray():town.Tasks(job).Tasks.ToArray();'),
('            theme.Art("book",450,260,1020,610);','            if(NetworkShop.Instance.Workday){WorkdaySelection();return;}\n            theme.Art("book",450,260,1020,610);'),
('theme.Text(137,61,LocalJob.ToString(),28,2,365);','theme.Text(137,61,LocalJob+(NetworkShop.Instance.Workday&&NetworkShop.Instance.Workday.IsAssistant(player.OwnerClientId)?" assistant":""),26,2,365);'),
('theme.Text(730,999,carrying?"Q returns your load":"",17,0,480);','theme.Text(730,999,item is WorkdayStation ws&&ws.alternative!=null?"R  "+ws.altLabel+"     Q return":carrying?"Q returns your load":"",17,0,520);'),
('            if(item is TownStation t){','            if(item is WorkdayStation w)return w.label;\n            if(item is TownStation t){'),
('            if(CurrentPage==Page.Tasks)TaskBook();','            if(NetworkShop.Instance.Workday)NetworkShop.Instance.Workday.DrawBook(CurrentPage,player);\n            else if(CurrentPage==Page.Tasks)TaskBook();'),
('        static string Short(string text,int limit)', '''        void WorkdaySelection()
        {
            TownGraphics.Card(new Rect(430,230,1060,655));theme.Text(480,268,"Town staff desk",40,2,960);
            theme.Text(480,335,"Three professions / one shift assistant",25,0,960);
            string[] descriptions={"Stock, customer orders and garage supplies","Diagnosis, fitting and recoverable scrap","Collection order, sorting and manifests"};
            for(int i=0;i<3;i++) {
                float y=412+i*130;TownGraphics.Card(new Rect(480,y,960,110));graphics.Icon(TownGuidance.RoleIcon((Profession)(i+1)),new Rect(495,y+21,58,58));
                theme.Text(575,y+12,town.Definitions[i].title,28,2,560);theme.Text(575,y+59,descriptions[i],20,0,540);
                bool lead=town.Owners[i]==NetworkShop.Nobody||town.Owners[i]==player.OwnerClientId;
                bool assistant=town.Owners[3]==NetworkShop.Nobody||town.Owners[3]==player.OwnerClientId;
                theme.Pill(1135,y+10,275,38,lead?"LEAD / "+(i+1):"LEAD ASSIGNED",lead?ShopUITheme.Green:ShopUITheme.Rule,18);
                theme.Pill(1135,y+60,275,38,assistant?"ASSIST / "+(i+4):"ASSIST ASSIGNED",assistant?ShopUITheme.Pale:ShopUITheme.Rule,18);
                if(lead&&Click(new Rect(1135,y+10,275,38)))Choose(i+1);
                if(assistant&&Click(new Rect(1135,y+60,275,38)))Choose(i+4);
            }
            theme.Text(480,826,"Assistant: dispatch, intake checks, gate help and workstation resets.",20,0,950);
            if(Keyboard.current!=null)for(int i=1;i<=6;i++)if(Keyboard.current[(Key)((int)Key.Digit1+i-1)].wasPressedThisFrame){Choose(i);break;}
        }
        static string Short(string text,int limit)''')])
print('Integrated Phase8A without touching the Git index.')
