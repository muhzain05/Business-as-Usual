using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using BusinessAsUsual.Prototype;
using BusinessAsUsual.Shopkeeper;
using BusinessAsUsual.Town;

namespace BusinessAsUsual.Multiplayer
{
    public sealed class NetworkShop : NetworkBehaviour
    {
        public const ulong Nobody=ulong.MaxValue;
        public static NetworkShop Instance { get; private set; }
        public ShopShift Shift;
        public StockCrate Crate;
        public TownJobs Town { get; private set; }
        public LivingWorkday Workday { get; private set; }
        public BusinessAsUsual.Phase8B.SuspiciousDayService Day {get;private set;}
        void Start(){if(Town&&LivingWorkday.Requested){Workday=gameObject.AddComponent<LivingWorkday>();Workday.Initialize(this);if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled){Day=gameObject.AddComponent<BusinessAsUsual.Phase8B.SuspiciousDayService>();Day.Initialize(this);}}}
        public int Revision { get; private set; }
        public ulong Holder { get; private set; }=Nobody;
        public bool Dropped { get; private set; }
        public int SnapshotsSent { get; private set; }
        public int SnapshotBytes { get; private set; }
        public int CommandsAccepted { get; private set; }
        public int CommandsRejected { get; private set; }
        public readonly Dictionary<string,Interactable> Targets=new Dictionary<string,Interactable>();
        readonly Dictionary<ulong,uint> sequences=new Dictionary<ulong,uint>();
        readonly Dictionary<ulong,float> lastCommand=new Dictionary<ulong,float>();
        Vector3 dockPosition; Quaternion dockRotation;
        Snapshot latest;
        public static string Id(Interactable target)
        {
            string id=target.name; for(var p=target.transform.parent;p;p=p.parent)id=p.name+"/"+id;
            return id;
        }
        void Awake()
        {
            Instance=this; Town=GetComponent<TownJobs>();dockPosition=Crate.transform.position; dockRotation=Crate.transform.rotation;
            foreach(var t in FindObjectsByType<Interactable>(FindObjectsInactive.Include,FindObjectsSortMode.None)) {
                if(t.name=="Shared Door Test Fixture"||t.name=="Shared Workstation Test Fixture")
                    t.gameObject.SetActive(NetworkTestDriver.Requested&&!Town);
                if(t.isActiveAndEnabled)Targets.Add(Id(t),t);
            }
        }
        public override void OnNetworkSpawn() { if(IsServer)NetworkManager.OnClientDisconnectCallback+=Disconnected; }
        public override void OnNetworkDespawn() { if(NetworkManager)NetworkManager.OnClientDisconnectCallback-=Disconnected; }
        public override void OnDestroy() { if(Instance==this)Instance=null; base.OnDestroy(); }
        public bool Execute(NetworkPlayer player,string target,uint sequence,int revision,string order)
        {
            if(!IsServer||!player||!player.IsSpawned)return false;
            ulong id=player.OwnerClientId;
            string reason=""; bool accepted=false; string reach="";
            if(sequences.TryGetValue(id,out var prior)&&sequence<=prior)reason="Duplicate command ignored.";
            else {
                sequences[id]=sequence;
                if(lastCommand.TryGetValue(id,out var when)&&Time.unscaledTime-when<.04f)reason="Please wait for the previous action.";
                else if(revision!=Revision)reason="Shared state changed. Try again.";
                else if(player.GetComponent<ShopRestockReach>().IsActive)reason="Stock placement in progress.";
                else if(Day&&target.StartsWith("day:"))accepted=Day.LobbyCommand(player,target,out reason);
                else if(Day&&!Day.Public.started&&!target.StartsWith("job:")&&target!="return")reason="Check in and ready up at the staff hub first.";
                else if(Day&&Day.Public.started&&target.StartsWith("job:")&&Town.Job(id)!=Profession.None)reason="Your work slip is fixed for this day.";
                else if(Town&&target.StartsWith("job:")&&Workday&&Workday.Domain.State.cartPusher==id)reason="Park the cart before changing jobs.";
                else if(Town&&target.StartsWith("job:"))accepted=Town.Assign(player,target.Substring(4),out reason);
                else if(target=="return"||target=="drop") {
                    if(Workday&&(Workday.Holding(id)||Workday.Domain.State.cartPusher==id)){accepted=Workday.Domain.Apply("return",(int)Town.Job(id),Workday.IsAssistant(id),id,Workday.Now,out reason);if(accepted)player.Interactor.Notify(reason);} else
                    if(Town&&Town.HasBag(id))accepted=Town.ReturnBag(player,target=="drop",out reason);
                    else if(Holder!=id)reason="You are not holding the crate.";
                    else if(target=="return") { ReturnCrate();Shift.Tell(player.Interactor,"Stock crate returned to the dock."); accepted=true; }
                    else accepted=Drop(player,out reason);
                } else if(!Targets.TryGetValue(target.StartsWith("alt:")?target.Substring(4):target,out var item))reason="Unknown interaction.";
                else if(!InRange(player.Interactor,item))reason="Move closer with a clear line of sight.";
                else if(Workday&&(item is TownStation oldTown&&oldTown.kind!=TownStation.Kind.Portal||item is ShopStation oldShop&&oldShop.kind!=ShopStation.Kind.Exit||item is WasteBag||item is StockCrate))reason="Historic task station is inactive during the living workday.";
                else if(!item.CanInteract(player.Interactor))reason="Object is unavailable.";
                else if(Workday&&item is WorkdayStation wd)accepted=Workday.Use(player,wd,target.StartsWith("alt:"),out reason);
                else if(Town&&(item is TownStation||item is WasteBag))accepted=Town.Use(player,item,out reason);
                else if(Town&&(item is ShopStation||item is StockCrate)&&!(item is ShopStation exit&&exit.kind==ShopStation.Kind.Exit)&&!Town.CanUseShop(player,out reason)) { }
                else if(item is StockCrate) {
                    if(Holder!=Nobody)reason="The crate is already held.";
                    else { accepted=Crate.Interact(player.Interactor); if(accepted){Holder=id;Dropped=false;} }
                } else if(item is ShopStation station) {
                    if(station.kind==ShopStation.Kind.Exit&&Town) {if(Holder==id||Town.HasBag(id))reason="Return your carried load before leaving.";else{player.Motor.Teleport(new Vector3(-7.8f,.08f,2.2f));accepted=true;Shift.Tell(player.Interactor,"Back in Daylight Town.");}}
                    else if(station.kind==ShopStation.Kind.Exit)reason="Disconnect from the session before returning to town.";
                    else if(station.kind==ShopStation.Kind.Shelf && Holder!=Nobody&&Holder!=id)reason="Your colleague is carrying the stock crate.";
                    else if(station.kind==ShopStation.Kind.Checkout&&order!=(Shift.CurrentOrder?.id??""))reason="Customer order changed. Try again.";
                    else { bool stocking=station.kind==ShopStation.Kind.Shelf&&Holder==id; accepted=station.Interact(player.Interactor); if(accepted&&stocking)reach=target; if(!Shift.Carried)Holder=Nobody; }
                } else if(item is SimpleDoor||item is BasicWorkstation)accepted=item.Interact(player.Interactor);
                else reason="Interaction is not part of this network slice.";
                lastCommand[id]=Time.unscaledTime;
            }
            if(accepted) { if(Day&&target.StartsWith("job:"))Day.Assigned(id);if(Town)Town.OnShopAction();CommandsAccepted++; Revision++; Publish(); }
            else CommandsRejected++;
            player.SendFeedback(accepted ? player.Interactor.Status??Shift.Message : string.IsNullOrEmpty(reason)?"Action unavailable. Check your order and stock.":reason,reach,accepted);
            return accepted;
        }
        public static bool InRange(PlayerInteractor player,Interactable item)
        {
            var point=item.InteractionPoint;
            if((point-player.Origin).sqrMagnitude>player.reach*player.reach)return false;
            return !Physics.Linecast(player.Origin,point,out var hit,player.interactionMask,QueryTriggerInteraction.Ignore)||hit.collider.GetComponentInParent<Interactable>()==item;
        }
        bool Drop(NetworkPlayer player,out string reason)
        {
            reason="No clear ground for the crate. Move away from walls.";
            var half=new Vector3(.38f,.3f,.3f);
            for(int i=0;i<5;i++) {
                var forward=Quaternion.Euler(0,player.Facing.Value+(i-2)*35,0)*Vector3.forward;
                var origin=player.transform.position+forward*1.15f+Vector3.up*2;
                if(!Physics.Raycast(origin,Vector3.down,out var ground,4,player.Interactor.interactionMask,QueryTriggerInteraction.Ignore))continue;
                var pos=ground.point+Vector3.up*(half.y+.03f);
                if(Physics.Linecast(player.Interactor.Origin,pos,player.Interactor.interactionMask,QueryTriggerInteraction.Ignore))continue;
                if(Physics.CheckBox(pos,half,Quaternion.identity,player.Interactor.interactionMask,QueryTriggerInteraction.Ignore))continue;
                ReturnCrate(); Dropped=true; Crate.transform.SetPositionAndRotation(pos,Quaternion.identity); Physics.SyncTransforms();
                reason=""; Shift.Tell(player.Interactor,"Crate placed on the ground. E picks it up; Q returns it while held."); return true;
            }
            return false;
        }
        void ReturnCrate() { Crate.ReturnToDock(); Holder=Nobody; Dropped=false; Crate.transform.SetPositionAndRotation(dockPosition,dockRotation); }
        void Disconnected(ulong id)
        {
            Day?.Domain.Disconnect(id);
            sequences.Remove(id); lastCommand.Remove(id);
            bool changed=Town&&Town.Disconnected(id);
            if(Holder==id) { ReturnCrate(); changed=true; }
            if(changed){Revision++;Publish();}
        }
        float observationsAt;
        void Update(){if(Day){Day.Tick();if(IsServer&&Time.unscaledTime>=observationsAt){observationsAt=Time.unscaledTime+.25f;Publish();}}if(IsServer&&Workday&&Workday.World&&Workday.World.Cart)Workday.World.Cart.StepServer();if(IsServer&&Town&&(!Day||Day.Public.started)&&(Workday?Workday.Advance():Town.Advance())){Revision++;Publish();}}
        public uint CartPoseSequence;uint cartReceived;
        [ClientRpc(Delivery=RpcDelivery.Unreliable)]public void CartPoseClientRpc(float x,float z,float yaw,uint sequence){if(IsServer||!Workday||!Workday.Domain.State.physical||sequence<=cartReceived)return;cartReceived=sequence;var s=Workday.Domain.State;s.cartX=x;s.cartZ=z;s.cartYaw=yaw;}
        public void ReleaseBeforeDespawn(ulong id)
        {
            if(Holder!=id)return;
            if(IsServer){ReturnCrate();Revision++;Publish();}
            else {
                // The presentation crate is a scene object, never destroy it with a player.
                Crate.ApplyNetworkPose(null,dockPosition,dockRotation);Holder=Nobody;latest=null;
                Shift.RestoreNetworkState(Shift.OrderIndex,Shift.Received,Shift.Closed,null,Shift.Message);
            }
        }
        [Serializable] public class Snapshot
        {
            public int revision,orderIndex;
            public bool received,closed,dropped;
            public ulong holder;
            public string message;
            public int[] quantities,progress;
            public StockMovement[] movements;
            public Receipt[] receipts;
            public Vector3 cratePosition;
            public Quaternion crateRotation;
            public string[] doorIds,workIds;
            public bool[] doorOpen;
            public int[] workUses;
            public TownJobs.State town;
            public WorkdayState workday;
            public BusinessAsUsual.Phase8B.DayPublicState day;
        }
        public Snapshot Capture()
        {
            var doors=Targets.Values.OfType<SimpleDoor>().OrderBy(Id).ToArray();
            var work=Targets.Values.OfType<BasicWorkstation>().OrderBy(Id).ToArray();
            return new Snapshot { day=Day?Day.Public:null,workday=Workday?Workday.Capture():null,town=Town?Town.Capture():null,revision=Revision,orderIndex=Shift.OrderIndex,received=Shift.Received,closed=Shift.Closed,dropped=Dropped,holder=Holder,message=Town?"Town ledger updated.":Shift.Message,
                quantities=new[]{"delivery","dock","carry","shelf","basket","sold"}.SelectMany(a=>ShopShift.Products.Select(p=>Shift.Stock.Count(a,p.sku))).ToArray(),
                progress=Shift.Tasks.Tasks.Select(t=>t.progress).ToArray(),movements=Shift.Stock.Journal.ToArray(),receipts=Shift.Transactions.Receipts.ToArray(),
                cratePosition=Crate.transform.position,crateRotation=Crate.transform.rotation,doorIds=doors.Select(Id).ToArray(),doorOpen=doors.Select(d=>d.IsOpen).ToArray(),workIds=work.Select(Id).ToArray(),workUses=work.Select(w=>w.UseCount).ToArray() };
        }
        public void Publish()
        {
            if(!IsServer||!IsSpawned)return;
            string json=JsonUtility.ToJson(Capture()); SnapshotsSent++; SnapshotBytes+=System.Text.Encoding.UTF8.GetByteCount(json);
            if(Day){foreach(var client in NetworkManager.ConnectedClientsIds){var visible=Capture();visible.workday=Day.ObservedState(client);string scoped=JsonUtility.ToJson(visible);StateClientRpc(scoped,new ClientRpcParams{Send=new ClientRpcSendParams{TargetClientIds=new[]{client}}});PrivateDayClientRpc(JsonUtility.ToJson(Day.CopyFor(client)),new ClientRpcParams{Send=new ClientRpcSendParams{TargetClientIds=new[]{client}}});}}else StateClientRpc(json);
            if(Workday)foreach(var client in NetworkManager.ConnectedClientsIds)
                WorkRecordsClientRpc(JsonUtility.ToJson(new RecordEnvelope{records=Workday.RecordsFor(client),support=Workday.SupportFor(client)}),new ClientRpcParams{Send=new ClientRpcSendParams{TargetClientIds=new[]{client}}});
        }
        [ClientRpc] public void ShowDayBoardClientRpc(ClientRpcParams rpc=default){if(BusinessAsUsual.RescueV2.RescueInterface.Instance)BusinessAsUsual.RescueV2.RescueInterface.Instance.Show("Board");}
        [ClientRpc]void PrivateDayClientRpc(string json,ClientRpcParams delivery=default){if(Day)Day.Local=JsonUtility.FromJson<BusinessAsUsual.Phase8B.PrivateDayCopy>(json);}
        [Serializable] public class RecordEnvelope {public WorkRecord[] records;public int[] support;}
        [ClientRpc] void WorkRecordsClientRpc(string json,ClientRpcParams delivery=default){if(Workday){var copy=JsonUtility.FromJson<RecordEnvelope>(json);Workday.LocalRecords=copy.records;Workday.LocalSupport=copy.support;foreach(var p in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))if(p.IsOwner)p.GetComponent<TownHandbook>()?.Invalidate();}}
        [ClientRpc]
        void StateClientRpc(string json,ClientRpcParams delivery=default)
        {
            if(IsServer)return;
            var state=JsonUtility.FromJson<Snapshot>(json); if(state.revision<Revision)return;
            latest=state; Revision=state.revision; Holder=state.holder;Dropped=state.dropped;
            Shift.Stock.RestoreNetworkState(state.movements,state.quantities); Shift.Transactions.RestoreNetworkState(state.receipts);
            for(int i=0;i<state.progress.Length;i++)Shift.Tasks.Tasks[i].progress=state.progress[i];
            Shift.RestoreNetworkState(state.orderIndex,state.received,state.closed,Holder==Nobody?null:Crate,state.message);
            if(Town&&state.town!=null)Town.Restore(state.town);
            if(Workday&&state.workday!=null)Workday.Restore(state.workday);
            if(Day&&state.day!=null)Day.Restore(state.day);
            for(int i=0;i<state.doorIds.Length;i++)((SimpleDoor)Targets[state.doorIds[i]]).RestoreNetworkState(state.doorOpen[i]);
            for(int i=0;i<state.workIds.Length;i++)((BasicWorkstation)Targets[state.workIds[i]]).RestoreNetworkState(state.workUses[i]);
            ApplyPose();
        }
        void LateUpdate() { if(!IsServer&&latest!=null)ApplyPose(); }
        void ApplyPose()
        {
            var holder=FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsSpawned&&p.OwnerClientId==Holder);
            Crate.ApplyNetworkPose(holder?holder.Interactor.carryAnchor:null,latest.cratePosition,latest.crateRotation);
        }
    }
}
