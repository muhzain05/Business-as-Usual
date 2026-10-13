using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.Prototype;
using BusinessAsUsual.Shopkeeper;

namespace BusinessAsUsual.Town
{
    public enum Profession { None, Shopkeeper, Mechanic, Sanitation }
    [Serializable] public class JobDefinition { public int id; public string title; public bool selectable; }
    [Serializable] public class ServiceRecord { public string id,vehicle,fault,part,timestampUtc; public ulong actor; public Vector3 location; }
    [Serializable] public class DisposalRecord { public string id,source,materialType,objectDescription,timestampUtc; public ulong actor; public Vector3 location; }
    [Serializable] public class ServiceRequest { public string id,vehicle,fault,part; public bool inspected,repaired,recorded; }
    [Serializable] public class BagState {
        public string id,source,materialType,objectDescription;
        public bool available,collected;
        public int stage; // 0 collectable, 1 sorted, 2 recorded.
        public ulong holder=NetworkShop.Nobody;
        public Vector3 origin,position;
    }
    public sealed class TownJobs : MonoBehaviour
    {
        public static TownJobs Instance { get; private set; }
        public readonly JobDefinition[] Definitions={
            new JobDefinition{id=1,title="Shopkeeper",selectable=true},new JobDefinition{id=2,title="Mechanic",selectable=true},new JobDefinition{id=3,title="Sanitation",selectable=true},
            new JobDefinition{id=4,title="Reserved",selectable=false},new JobDefinition{id=5,title="Reserved",selectable=false},new JobDefinition{id=6,title="Reserved",selectable=false}};
        public int AssistantProfession=1;
        public ulong[] Owners=Enumerable.Repeat(NetworkShop.Nobody,6).ToArray();
        public bool[] Started=new bool[6],Completed=new bool[6];
        public readonly TaskBoard MechanicTasks=new TaskBoard(),SanitationTasks=new TaskBoard();
        public readonly InventoryLedger Parts=new InventoryLedger();
        public static readonly string[] PartAccounts={"warehouse","counter","mechanic","installed"},PartSkus={"battery","filter"};
        public ServiceRequest[] Services={new ServiceRequest{id="SV-001",vehicle="Mikan delivery van",fault="Weak battery",part="battery"},new ServiceRequest{id="SV-002",vehicle="Municipal mini truck",fault="Clogged oil filter",part="filter"}};
        public BagState[] Bags;
        public readonly List<ServiceRecord> ServiceRecords=new List<ServiceRecord>();
        public readonly List<DisposalRecord> DisposalRecords=new List<DisposalRecord>();
        public bool PartsRequested;
        public string Message="Town shift open.";
        [Serializable] public class WorkState { public ulong worker=NetworkShop.Nobody;public string target="";public double started;public float duration;public Vector3 position; }
        public WorkState[] Works={new WorkState(),new WorkState()};
        public WorkState Work(ulong id)=>Works.FirstOrDefault(w=>w.worker==id);
        public bool IsWorking(ulong id)=>Work(id)!=null;
        NetworkShop adapter;
        WasteBag[] props;
        public Profession Job(ulong id) {int i=Array.IndexOf(Owners,id);return i<0?Profession.None:i==3?(Profession)AssistantProfession:(Profession)(i+1);}
        public TaskBoard Tasks(Profession job)=>adapter.Workday?adapter.Workday.Tasks(job):job==Profession.Mechanic?MechanicTasks:job==Profession.Sanitation?SanitationTasks:adapter.Shift.Tasks;
        public int Contribution => (adapter.Shift.Closed?3:0)+(Completed[1]?3:0)+(Completed[2]?3:0);
        public bool HasBag(ulong id)=>Bags.Any(b=>b.holder==id);
        public float WorkProgress(ulong id){var w=Work(id);return w==null?0:Mathf.Clamp01((float)((adapter.NetworkManager.ServerTime.Time-w.started)/w.duration));}
        void Awake()
        {
            Instance=this;adapter=GetComponent<NetworkShop>();
            MechanicTasks.Add("receive","Receive two service tickets",1);MechanicTasks.Add("inspect","Inspect both vehicles",2);MechanicTasks.Add("parts","Collect replacement parts",2);MechanicTasks.Add("repair","Repair both faults",2);MechanicTasks.Add("record","File service records",2);MechanicTasks.Add("close","Close mechanic shift",1);
            SanitationTasks.Add("route","Receive collection route",1);SanitationTasks.Add("collect","Collect three marked bags",3);SanitationTasks.Add("sort","Sort three waste bags",3);SanitationTasks.Add("record","File three disposal records",3);SanitationTasks.Add("close","Close sanitation shift",1);
            Parts.Seed("warehouse","battery",2);Parts.Seed("warehouse","filter",2);
            props=FindObjectsByType<WasteBag>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(p=>p.index).ToArray();
            Bags=props.Select(p=>new BagState{id="WB-"+(p.index+1).ToString("D3"),source=p.index==0?"Town Square":p.index==1?"Back Alley":p.index==2?"Sanitation Yard":p.index==3?"Daylight Market":"Garage",materialType=p.index==0||p.index==3?"Paper":p.index==1?"Mixed":"Metal",objectDescription=p.index==1?"Discarded umbrella and wrappers":p.index>=4?"Replaced vehicle parts":"Ordinary packaging",available=p.index<3,origin=p.transform.position,position=p.transform.position}).ToArray();
        }
        void OnDestroy(){if(Instance==this)Instance=null;}
        public bool Assign(NetworkPlayer player,string value,out string reason)
        {
            reason="Choose an available profession.";
            if(!int.TryParse(value,out var requested)||requested<1||requested>(adapter.Workday?6:3))return false;
            if(adapter.Workday&&requested>3) {
                ulong who=player.OwnerClientId;int previous=Array.IndexOf(Owners,who);
                if(Owners[3]!=NetworkShop.Nobody&&Owners[3]!=who){reason="The shift already has an assistant.";return false;}
                if(adapter.Workday.Holding(who)){reason="Return your carried load first.";return false;}
                if(previous>=0&&(adapter.Workday.Domain.State.received||adapter.Workday.Domain.State.requested)){reason="Keep your assignment until this workday is filed.";return false;}
                if(previous>=0)Owners[previous]=NetworkShop.Nobody;
                Owners[3]=who;AssistantProfession=requested-3;reason="Shift assistant / "+((Profession)AssistantProfession);Tell(player,reason);return true;
            }
            ulong id=player.OwnerClientId;int old=Array.IndexOf(Owners,id),slot=requested-1;
            if(Owners[slot]!=NetworkShop.Nobody&&Owners[slot]!=id){reason="That profession is already assigned.";return false;}
            if(old==slot){reason="You already have this profession.";return false;}
            if(adapter.Workday&&(adapter.Workday.Holding(id)||old>=0&&!(old==3&&Owners[slot]==NetworkShop.Nobody)&&(adapter.Workday.Domain.State.received||adapter.Workday.Domain.State.requested))){reason="Keep your assignment and return carried work first.";return false;}
            if(HasBag(id)||adapter.Holder==id||IsWorking(id)){reason="Finish or return your carried work first.";return false;}
            if(old>=0&&Started[old]&&!IsComplete((Profession)(old+1))){reason="Finish your shift before changing profession.";return false;}
            if(old>=0)Owners[old]=NetworkShop.Nobody;
            Owners[slot]=id;reason="Assigned: "+Definitions[slot].title;Tell(player,reason);return true;
        }
        bool IsComplete(Profession p)=>p==Profession.Shopkeeper?adapter.Shift.Closed:Completed[(int)p-1];
        public bool CanUseShop(NetworkPlayer player,out string reason)
        {reason="Shopkeeper duties require the Shopkeeper assignment.";return Job(player.OwnerClientId)==Profession.Shopkeeper;}
        void Tell(NetworkPlayer player,string message){Message="Town work updated.";player.Interactor.Notify(message);}
        bool Require(NetworkPlayer p,Profession job,out string reason)
        {reason="This work belongs to "+job+".";return Job(p.OwnerClientId)==job;}
        public bool Use(NetworkPlayer player,Interactable item,out string reason)
        {
            reason="Action unavailable.";ulong id=player.OwnerClientId;
            if(item is WasteBag prop) {
                if(!Require(player,Profession.Sanitation,out reason))return false;
                if(!Started[2]||Completed[2]){reason="Receive your collection route first.";return false;}
                var bag=Bags[prop.index];if(HasBag(id)||adapter.Holder==id||!bag.available||bag.stage!=0||bag.holder!=NetworkShop.Nobody){reason="Bag unavailable or hands occupied.";return false;}
                bag.holder=id;bag.collected=true;bag.position=player.transform.position;SanitationTasks.Set("collect",Bags.Count(b=>b.collected));Tell(player,"Carrying "+bag.materialType+" waste. Hold E at its sorting bin; Q returns it.");return true;
            }
            var station=item as TownStation;if(!station)return false;
            if(station.kind==TownStation.Kind.Portal) {if(HasBag(id)||adapter.Holder==id){reason="Return your carried load before entering.";return false;}player.Motor.Teleport(station.destination);Tell(player,station.label);return true;}
            if(IsWorking(id)){reason="Timed work in progress. Release E to cancel.";return false;}
            if(station.kind==TownStation.Kind.PackParts) {
                if(!Require(player,Profession.Shopkeeper,out reason))return false;
                if(!PartsRequested){reason="No parts request yet.";return false;}
                if(!SupplyParts("shopkeeper pack")){reason="Parts are already packed or installed.";return false;}Tell(player,"Parts packed at the pickup counter for the mechanic.");return true;
            }
            Profession needed=station.kind==TownStation.Kind.RouteDesk||station.kind==TownStation.Kind.Sort?Profession.Sanitation:Profession.Mechanic;
            if(!Require(player,needed,out reason))return false;
            int slot=(int)needed-1;
            if(Completed[slot]){reason="Your shift is already complete.";return false;}
            if(station.kind==TownStation.Kind.ServiceDesk){
                if(!Started[1]){Started[1]=true;MechanicTasks.Set("receive",1);Tell(player,"Two service tickets received. Hold E at each vehicle's inspection point.");return true;}
                var records=Services.Where(s=>s.repaired&&!s.recorded).ToArray();
                if(records.Length>0){foreach(var service in records){service.recorded=true;ServiceRecords.Add(new ServiceRecord{id="SR-"+service.id,vehicle=service.vehicle,fault=service.fault,part=service.part,actor=id,location=station.InteractionPoint,timestampUtc=DateTime.UtcNow.ToString("o")});}MechanicTasks.Set("record",ServiceRecords.Count);Tell(player,"Service records filed. Return to close the completed shift.");return true;}
                if(Services.All(s=>s.recorded)){Completed[1]=true;MechanicTasks.Set("close",1);Tell(player,"Mechanic shift complete. +3 Town Stability.");return true;}reason="Inspect, obtain parts and repair both vehicles first.";return false;
            }
            if(station.kind==TownStation.Kind.RouteDesk){
                if(!Started[2]){Started[2]=true;SanitationTasks.Set("route",1);Tell(player,"Route received: Square, Alley and Yard. Collect a bag with E, then hold E at its matching bin.");return true;}
                var sorted=Bags.Where(b=>b.stage==1).ToArray();
                if(sorted.Length>0){foreach(var bag in sorted){bag.stage=2;DisposalRecords.Add(new DisposalRecord{id="DR-"+bag.id,source=bag.source,materialType=bag.materialType,objectDescription=bag.objectDescription,actor=id,location=bag.origin,timestampUtc=DateTime.UtcNow.ToString("o")});}SanitationTasks.Set("record",DisposalRecords.Count);Tell(player,"Disposal manifest filed. Return to close the route.");return true;}
                if(DisposalRecords.Count>=3&&!HasBag(id)){Completed[2]=true;SanitationTasks.Set("close",1);Tell(player,"Sanitation shift complete. +3 Town Stability.");return true;}reason="Process and record at least three bags first.";return false;
            }
            if(!Started[slot]){reason="Receive your workplace instructions first.";return false;}
            if(station.kind==TownStation.Kind.RequestParts){if(!Services.Any(s=>s.inspected)){reason="Inspect a vehicle to identify its fault first.";return false;}if(PartsRequested){reason="Parts request already filed.";return false;}PartsRequested=true;Tell(player,"Parts slip sent to Daylight Market. Collect there after packing, or use its municipal reserve if unstaffed.");return true;}
            if(station.kind==TownStation.Kind.CollectParts){
                if(!PartsRequested){reason="File the parts request at the garage first.";return false;}
                if(Owners[0]==NetworkShop.Nobody)SupplyParts("unstaffed municipal reserve");
                var lines=PartSkus.Where(sku=>Parts.Count("counter",sku)>0).Select(sku=>new StockLine(sku,Parts.Count("counter",sku))).ToArray();
                if(!Parts.TransferBundle("counter","mechanic",lines,"mechanic collection")){reason="Await shopkeeper packing. The municipal reserve activates when Shopkeeper is unassigned.";return false;}
                MechanicTasks.Set("parts",PartSkus.Sum(sku=>Parts.Count("mechanic",sku)+Parts.Count("installed",sku)));Tell(player,"Replacement parts collected. Hold E at the repair points.");return true;
            }
            if(station.kind==TownStation.Kind.Inspect||station.kind==TownStation.Kind.Repair){
                var service=Services[station.index];
                if(station.kind==TownStation.Kind.Inspect&&service.inspected){reason="Fault identified: "+service.fault+" / part: "+service.part;return false;}
                if(station.kind==TownStation.Kind.Repair&&(!service.inspected||service.repaired||Parts.Count("mechanic",service.part)<1)){reason="Inspect this vehicle and collect its replacement part first.";return false;}
                BeginWork(player,station,station.kind==TownStation.Kind.Inspect?1.6f:3.2f);Tell(player,"Hold E and remain at the vehicle to "+(station.kind==TownStation.Kind.Inspect?"inspect.":"repair."));return true;
            }
            if(station.kind==TownStation.Kind.Sort){var bag=Bags.FirstOrDefault(b=>b.holder==id);if(bag==null||bag.materialType!=station.materialType){reason="Bring a "+station.materialType+" bag to this bin.";return false;}BeginWork(player,station,2f);Tell(player,"Hold E to sort "+bag.materialType+" waste.");return true;}
            return false;
        }
        bool SupplyParts(string reason)
        {
            var lines=PartSkus.Where(sku=>Parts.Count("counter",sku)+Parts.Count("mechanic",sku)+Parts.Count("installed",sku)==0).Select(sku=>new StockLine(sku,1)).ToArray();
            return Parts.TransferBundle("warehouse","counter",lines,reason);
        }
        void BeginWork(NetworkPlayer p,TownStation station,float duration)
        {var work=Works[(int)Job(p.OwnerClientId)-2];work.worker=p.OwnerClientId;work.target=NetworkShop.Id(station);work.started=adapter.NetworkManager.ServerTime.Time;work.duration=duration;work.position=p.transform.position;}
        public bool Advance()
        {
            if(!adapter.IsServer)return false;
            bool changed=false;foreach(var work in Works)if(work.worker!=NetworkShop.Nobody)changed|=AdvanceWork(work);return changed;
        }
        bool AdvanceWork(WorkState work)
        {
            var player=FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsSpawned&&p.OwnerClientId==work.worker);
            var station=adapter.Targets.TryGetValue(work.target,out var item)?item as TownStation:null;
            double elapsed=adapter.NetworkManager.ServerTime.Time-work.started;
            if(!player||!station||!NetworkShop.InRange(player.Interactor,station)||(player.transform.position-work.position).sqrMagnitude>.16f||(!player.WorkHeld&&elapsed>.35)) {Message="Town work updated.";if(player)player.SendFeedback("Work paused. Hold E at your work point.");ClearWork(work);return true;}
            if(elapsed<work.duration)return false;
            ulong actor=work.worker;
            string feedback="Work complete.";
            if(station.kind==TownStation.Kind.Inspect){Services[station.index].inspected=true;MechanicTasks.Set("inspect",Services.Count(s=>s.inspected));feedback="Inspection: "+Services[station.index].fault+".";}
            else if(station.kind==TownStation.Kind.Repair){var service=Services[station.index];if(!service.repaired&&Parts.Transfer("mechanic","installed",service.part,1,"repair:"+service.id)){service.repaired=true;MechanicTasks.Set("repair",Services.Count(s=>s.repaired));Bags[4+station.index].available=true;feedback="Repair complete. File your service slip.";}}
            else {var bag=Bags.FirstOrDefault(b=>b.holder==actor);if(bag!=null){bag.holder=NetworkShop.Nobody;bag.stage=1;SanitationTasks.Set("sort",Bags.Count(b=>b.stage>0));feedback="Sorted. File your route manifest.";}}
            Message="Town work updated.";player.SendFeedback(feedback);
            ClearWork(work);return true;
        }
        void ClearWork(WorkState work){work.worker=NetworkShop.Nobody;work.target="";work.duration=0;work.started=0;work.position=Vector3.zero;}
        public bool ReturnBag(NetworkPlayer player,bool drop,out string reason)
        {
            reason="You are not carrying a bag.";var bag=Bags.FirstOrDefault(b=>b.holder==player.OwnerClientId);if(bag==null)return false;
            Vector3 position=bag.origin;
            if(drop){position=player.transform.position+player.Motor.visual.forward*.9f+Vector3.up*.35f;if(Physics.CheckBox(position,new Vector3(.24f,.28f,.24f),Quaternion.identity,player.Interactor.interactionMask,QueryTriggerInteraction.Ignore)||Physics.Linecast(player.Interactor.Origin,position,player.Interactor.interactionMask,QueryTriggerInteraction.Ignore)){reason="No clear space for the bag. Q returns it to its collection point.";return false;}if(!Physics.Raycast(position+Vector3.up,Vector3.down,out var hit,2,player.Interactor.interactionMask,QueryTriggerInteraction.Ignore)){reason="Place bags on solid ground.";return false;}position=hit.point+Vector3.up*.3f;}
            var work=Work(player.OwnerClientId);if(work!=null)ClearWork(work);bag.holder=NetworkShop.Nobody;bag.position=position;Tell(player,drop?"Bag placed. E picks it up again.":"Bag returned to its marked collection point.");return true;
        }
        public bool Disconnected(ulong id)
        {
            if(adapter.Workday)adapter.Workday.Domain.Disconnect(id,adapter.Workday.Now);
            bool changed=false;int slot=Array.IndexOf(Owners,id);if(slot>=0){Owners[slot]=NetworkShop.Nobody;changed=true;}
            foreach(var bag in Bags.Where(b=>b.holder==id)){bag.holder=NetworkShop.Nobody;bag.position=bag.origin;changed=true;}
            var work=Work(id);if(work!=null){ClearWork(work);changed=true;}return changed;
        }
        public void OnShopAction(){if(adapter.Shift.Received){Started[0]=true;Bags[3].available=true;}Completed[0]=adapter.Shift.Closed;}
        void LateUpdate()
        {
            if(adapter.Workday){foreach(var prop in props)prop.gameObject.SetActive(false);return;}
            if(Bags==null)return;
            var players=FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
            foreach(var prop in props){var bag=Bags[prop.index];var holder=players.FirstOrDefault(p=>p.IsSpawned&&p.OwnerClientId==bag.holder);prop.gameObject.SetActive(bag.available&&bag.stage==0);prop.transform.position=holder?holder.Interactor.carryAnchor.position:bag.position;foreach(var collider in prop.GetComponentsInChildren<Collider>())collider.enabled=bag.holder==NetworkShop.Nobody;}
        }
        [Serializable] public class State {
            public ulong[] owners;public bool[] started,completed;public int[] mechanicProgress,sanitationProgress,quantities;
            public bool partsRequested;public ServiceRequest[] services;public BagState[] bags;public ServiceRecord[] serviceRecords;public DisposalRecord[] disposalRecords;public StockMovement[] movements;
            public string message;public WorkState[] works;public int contribution,assistantProfession;
        }
        public State Capture()=>new State{assistantProfession=AssistantProfession,owners=Owners,started=Started,completed=Completed,mechanicProgress=MechanicTasks.Tasks.Select(t=>t.progress).ToArray(),sanitationProgress=SanitationTasks.Tasks.Select(t=>t.progress).ToArray(),quantities=PartAccounts.SelectMany(a=>PartSkus.Select(s=>Parts.Count(a,s))).ToArray(),partsRequested=PartsRequested,services=Services,bags=Bags,serviceRecords=ServiceRecords.ToArray(),disposalRecords=DisposalRecords.ToArray(),movements=Parts.Journal.ToArray(),message=Message,works=Works,contribution=Contribution};
        public void Restore(State state)
        {AssistantProfession=state.assistantProfession;Owners=state.owners;Started=state.started;Completed=state.completed;Services=state.services;Bags=state.bags;PartsRequested=state.partsRequested;Message=state.message;Works=state.works;for(int i=0;i<state.mechanicProgress.Length;i++)MechanicTasks.Tasks[i].progress=state.mechanicProgress[i];for(int i=0;i<state.sanitationProgress.Length;i++)SanitationTasks.Tasks[i].progress=state.sanitationProgress[i];Parts.RestoreNetworkAccounts(PartAccounts,PartSkus,state.quantities,state.movements);ServiceRecords.Clear();ServiceRecords.AddRange(state.serviceRecords);DisposalRecords.Clear();DisposalRecords.AddRange(state.disposalRecords);}
    }
}
