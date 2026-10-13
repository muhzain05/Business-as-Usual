using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.Prototype;
using BusinessAsUsual.Shopkeeper;
using BusinessAsUsual.FirstPerson;

namespace BusinessAsUsual.Town
{
    // Phase 8A extends the existing command boundary; no second NetworkManager or physics authority.
    public sealed class LivingWorkday : MonoBehaviour
    {
        public static LivingWorkday Instance { get; private set; }
        public WorkdayDomain Domain { get; private set; } = new WorkdayDomain();
        public bool Ready { get; private set; }
        public readonly Dictionary<string,WorkdayStation> Stations = new Dictionary<string,WorkdayStation>();
        public WorkRecord[] LocalRecords = Array.Empty<WorkRecord>();
        public int[] LocalSupport = new int[3];
        readonly Dictionary<ulong,int[]> support = new Dictionary<ulong,int[]>();
        public float WorkdaySeconds=480;
        public int[] SupportFor(ulong id)=>support.TryGetValue(id,out var value)?value:new int[3];
        double pauseOffset,lastClock;
        public bool Advance(){
            double raw=RawTime;if(lastClock==0)lastClock=raw;
            if(BusinessAsUsual.RescueV2.RescueInterface.Instance&&NetworkSession.Instance&&NetworkSession.Instance.Solo&&BusinessAsUsual.FirstPerson.PauseMenu.IsOpen)pauseOffset+=raw-lastClock;
            lastClock=raw;
            bool changed=false;
            if(Domain.State.physical&&NetworkSession.Instance&&(NetworkSession.Instance.Solo||shop.Day!=null&&shop.Day.Public.started&&shop.Day.Public.unattended)){var owners=shop.Town.Owners;changed=Domain.AdvanceMunicipal(owners[0]==WorkdayDomain.Nobody,owners[1]==WorkdayDomain.Nobody,owners[2]==WorkdayDomain.Nobody,Now);}
            return Domain.Expire(Now,WorkdaySeconds)||changed;
        }
        public BusinessAsUsual.RescueV2.RescueWorld World {get;private set;}
        readonly Dictionary<string,Transform> visuals = new Dictionary<string,Transform>();
        Transform hood, terminal, gate;
        NetworkShop shop;
        ShopUITheme theme;
        float lastGesture;
        WorkdayStation torqueTarget;
        NetworkPlayer torquePlayer;
        float torque;
        float torqueEnded;
        public bool Turning=>torqueTarget;
        public bool SuppressLook=>Turning||Time.time<torqueEnded+.15f;
        public void BeginTorque(NetworkPlayer player,WorkdayStation target){torquePlayer=player;torqueTarget=target;torque=0;player.GetComponent<BusinessAsUsual.RescueV2.RescueHands>()?.Begin("bolt",target.InteractionPoint);player.Interactor.Notify("Hold left mouse and drag right to turn the terminal. Q cancels.");}
        public static bool Requested => !NetworkTestDriver.Requested || Environment.GetCommandLineArgs().Contains("--bau-workday") || Environment.GetCommandLineArgs().Contains("--bau-rescue") || BusinessAsUsual.Phase8B.Phase8BFlow.Enabled;
        double RawTime=>shop && shop.IsSpawned ? shop.NetworkManager.ServerTime.Time : Time.timeAsDouble;
        public double Now=>RawTime-pauseOffset;
        public bool Holding(ulong id) => Domain.Held(id)!=null;
        public bool IsAssistant(ulong id)=>shop.Town.Owners[3]==id;
        public void Initialize(NetworkShop adapter)
        {
            Instance=this;shop=adapter;theme=new ShopUITheme();Domain=new WorkdayDomain(BusinessAsUsual.RescueV2.RescueInterface.Requested,BusinessAsUsual.Phase8B.Phase8BFlow.Enabled);
            var args=Environment.GetCommandLineArgs();int durationIndex=Array.IndexOf(args,"--bau-workday-seconds");
            if(durationIndex>=0&&durationIndex+1<args.Length&&float.TryParse(args[durationIndex+1],out var seconds))WorkdaySeconds=Mathf.Clamp(seconds,30,600);
            Domain.State.durationSeconds=WorkdaySeconds;
            var root=new GameObject("Living Workday").transform;
            // Retire legacy interaction triggers, never the solid furniture collision.
            // Do not change authored scene or historic hierarchy IDs. Old task sequences are retained
            // in explicitly opted-in compatibility runs; the living workday uses their existing spaces.
            foreach(var old in shop.Targets.Values.ToArray()) {
                if(old is TownStation t && t.kind!=TownStation.Kind.Portal || old is WasteBag || old is StockCrate || old is ShopStation s && s.kind!=ShopStation.Kind.Exit)
                    foreach(var c in old.GetComponentsInChildren<Collider>())if(c.isTrigger)c.enabled=false;
                if(old is WasteBag)old.gameObject.SetActive(false);
                if(Domain.State.physical&&old is StockCrate)old.gameObject.SetActive(false);
            }
            Make("Shipment",new Vector3(-36.2f,.75f,4.9f),"receive","verify",1,root,"Receive shipment","Check stock");
            Make("Dispatch",new Vector3(-36.2f,.75f,3.4f),"prepare","verify",1,root,"Prepare parcel","Verify contents");
            Make("Parcel",new Vector3(-36.2f,.84f,3.4f),"pickup",null,0,root,"Carry to garage",null);
            Make("Counter",new Vector3(-42.5f,.75f,7.6f),"customer","priority",1,root,"Serve order","Change priority");
            Make("Request",new Vector3(10.3f,.75f,8.2f),"request","reserve",2,root,"Request battery","Self-service recovery");
            Make("Hood",new Vector3(5.35f,.75f,5.5f),"hood",null,2,root,"Open / close hood",null);
            Make("Probes",new Vector3(5.25f,.85f,4.7f),"diagnose","check",2,root,"Quick diagnosis","Full load check");
            Make("Terminal",new Vector3(6.35f,.84f,4.45f),"bolt","bench",2,root,"Turn terminal","Change repair workflow");
            Make("Socket",new Vector3(6.35f,.88f,4.45f),"install",null,2,root,"Seat replacement",null);
            Make("Bay tray",new Vector3(5.05f,.75f,6.8f),"deliver","assist",0,root,"Place parcel","Hold receiving gate");
            Make("Paper pickup",new Vector3(5.1f,.50f,8.55f),"paper",null,3,root,"Lift packaging",null);
            Make("Metal pickup",new Vector3(5.8f,.50f,8.55f),"metal",null,3,root,"Lift old component",null);
            Make("Square pickup",new Vector3(-1,.50f,7.8f),"square",null,3,root,"Lift square bag",null);
            Make("Manifest",new Vector3(10,.90f,-6.48f),"file","order",3,root,"File manifest","Choose collection priority");
            Make("Yard intake",new Vector3(10,.65f,-7),"yard",null,3,root,"Lift staged batch",null);
            Make("Paper chute",new Vector3(6,1.15f,-7.98f),"sort-paper","inspect",3,root,"Sort paper","Inspect batch");
            Make("Mixed chute",new Vector3(7.5f,1.15f,-7.98f),"sort-mixed","inspect",3,root,"Sort mixed","Inspect batch");
            Make("Metal chute",new Vector3(9,1.15f,-7.98f),"sort-metal","inspect",3,root,"Sort metal","Inspect batch");
            Make("Market reset",new Vector3(-38.0f,.8f,7.2f),"cleanup",null,0,root,"Reset workstation",null);
            Make("Garage reset",new Vector3(10.8f,.8f,6.8f),"cleanup",null,0,root,"Reset tool tray",null);
            Make("Yard reset",new Vector3(10.6f,.8f,-8.3f),"cleanup",null,0,root,"Reset sorting station",null);
            Make("Shift report",new Vector3(0,.8f,4.7f),"close",null,0,root,"File living workday",null);
            if(Domain.State.physical){World=gameObject.AddComponent<BusinessAsUsual.RescueV2.RescueWorld>();World.Build(this,shop,root);}
            else {foreach(var item in Domain.State.items) {
                var v=Cube(item.id,Vector3.zero,item.material=="battery"?new Vector3(.36f,.25f,.24f):new Vector3(.38f,.30f,.28f),item.material=="metal"?ShopUITheme.Ink:item.material=="stock"?ShopUITheme.Green:ShopUITheme.Mustard,root);
                visuals.Add(item.id,v);
                var label=new GameObject("Reference").AddComponent<TextMesh>();label.transform.SetParent(v,false);label.transform.localPosition=new Vector3(0,.52f,-.53f);label.characterSize=.022f;label.fontSize=40;label.anchor=TextAnchor.MiddleCenter;label.color=ShopUITheme.Ink;label.text=item.id;
            }
            hood=Cube("Service hood",new Vector3(6.4f,1.16f,5.0f),new Vector3(.95f,.045f,.7f),ShopUITheme.Green,root);
            terminal=Cube("Terminal cap",Stations["Terminal"].transform.position+Vector3.up*.12f,new Vector3(.09f,.09f,.09f),ShopUITheme.Mustard,root);
            gate=Cube("Receiving gate",Stations["Bay tray"].transform.position+new Vector3(0,.15f,.28f),new Vector3(.7f,.06f,.04f),ShopUITheme.Mustard,root);
            }
            // Portal is a coherent short walking threshold, not another E interaction.
            var entrance=shop.Targets.Values.OfType<TownStation>().FirstOrDefault(t=>t.kind==TownStation.Kind.Portal);
            if(entrance)entrance.gameObject.AddComponent<WorkdayThreshold>().entrance=true;
            var exit=shop.Targets.Values.OfType<ShopStation>().FirstOrDefault(t=>t.kind==ShopStation.Kind.Exit);
            if(exit)exit.gameObject.AddComponent<WorkdayThreshold>();
            Ready=true;Physics.SyncTransforms();
        }
        Transform Cube(string name,Vector3 at,Vector3 size,Color color,Transform parent)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent);go.transform.position=at;go.transform.localScale=size;
            // All visible props have fitted colliders; carried props are presentation, not physics stock.
            go.layer=2;var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=color;go.GetComponent<Renderer>().material=material;
            return go.transform;
        }
        public void Make(string name,Vector3 at,string action,string alternative,int job,Transform parent,string label,string altLabel)
        {
            var go=new GameObject(name);go.transform.SetParent(parent);go.transform.position=at;
            var trigger=go.AddComponent<BoxCollider>();trigger.size=new Vector3(.30f,.30f,.30f);trigger.isTrigger=true;
            var station=go.AddComponent<WorkdayStation>();station.action=action;station.alternative=alternative;station.profession=job;station.label=label;station.altLabel=altLabel;
            Stations.Add(name,station);shop.Targets.Add(NetworkShop.Id(station),station);
            // A compact paper plate gives a real-world affordance in the approved palette.
            if(Domain.State.physical)return;
            var plate=Cube(name+" work plate",at-new Vector3(0,.04f,0),new Vector3(.36f,.035f,.30f),ShopUITheme.Paper,parent);
            // Paper affordances are labels, not floating obstacles in the walking aisle.
            plate.GetComponent<Collider>().enabled=false;
        }
        public bool Use(NetworkPlayer player,WorkdayStation station,bool alternate,out string message)
        {
            string action=alternate?station.alternative:station.action;
            if(Domain.State.physical&&station.action=="customer")action=alternate?"priority":Domain.Held(player.OwnerClientId)!=null?"place-order":"scan-order";
            if(Domain.State.physical&&action=="scan-order"&&Domain.State.customerOrders<3&&Domain.State.items[4+Domain.State.customerOrders].custody!="checkout")action="priority";
            if(Domain.State.physical&&action=="diagnose"&&Domain.State.diagnosed&&!Domain.State.loadChecked)action="check";
            if(!alternate&&action=="deliver"&&Domain.State.suspicious&&Domain.Held(player.OwnerClientId)==null&&Domain.Find("PART-8A-001").custody=="garage"&&Domain.Find("PACK-8A-001").custody=="garage")action="collect-parcel";
            if(!alternate&&action=="file"&&IsAssistant(player.OwnerClientId)&&Holding(player.OwnerClientId))action="stage";
            if(action=="board"){shop.ShowDayBoardClientRpc(new ClientRpcParams{Send=new ClientRpcSendParams{TargetClientIds=new[]{player.OwnerClientId}}});message="Shared copies at the staff kiosk.";return true;}
            if(action==null){message="This work point has no alternate action.";return false;}
            bool sealedParcel=Domain.Find("PART-8A-001").holder==player.OwnerClientId;
            bool accepted=Domain.Apply(action,(int)shop.Town.Job(player.OwnerClientId),IsAssistant(player.OwnerClientId),player.OwnerClientId,Now,out message);
            if(accepted){
                shop.Day?.Accepted(player.OwnerClientId,action=="amend-service"&&!Domain.State.loadChecked?"amend-service-quick":action=="return-note"&&Domain.State.parcelNote=="Direct transfer"?"note-direct":action,sealedParcel);
                if(!support.ContainsKey(player.OwnerClientId))support.Add(player.OwnerClientId,new int[3]);
                var contribution=support[player.OwnerClientId];
                if(action=="prepare"||action=="stage"||action=="check"&&IsAssistant(player.OwnerClientId)&&Domain.State.loadChecked||action=="deliver"&&Domain.State.dispatched)contribution[0]++;
                if(action=="assist")contribution[1]++;if(action=="cleanup")contribution[2]++;
                player.Interactor.Notify(message);lastGesture=Time.time;player.WorkdayGestureClientRpc(action,station.InteractionPoint,new ClientRpcParams{Send=new ClientRpcSendParams{TargetClientIds=new[]{player.OwnerClientId}}});}
            return accepted;
        }
        public void Restore(WorkdayState state){Domain.Restore(state);}
        public WorkdayState Capture()=>Domain.PublicState();
        public WorkRecord[] RecordsFor(ulong id)=>Domain.Records.Where(r=>r.profession==(int)shop.Town.Job(id)).ToArray();
        public TaskBoard Tasks(Profession job,bool assistant=false,ulong actor=ulong.MaxValue)
        {
            var board=new TaskBoard();var s=Domain.State;
            void Task(string title,bool done){board.Add(title,title,1);board.Set(title,done?1:0);}
            if(assistant){var done=shop.IsServer?SupportFor(actor):LocalSupport;Task(job==Profession.Sanitation?"Stage a collected batch":job==Profession.Mechanic?"Check bay intake":"Handle a shared delivery",done[0]>0);Task("Assist bay intake",done[1]>0);Task("Reset a workstation",done[2]>0);return board;}
            if(job==Profession.Shopkeeper){Task("Receive shipment",s.received);Task("Deliver garage parcel",s.dispatched);Task("Serve customer orders",s.customerOrders>=1);}
            if(job==Profession.Mechanic){if(s.physical){Task("Open hood",s.hood||s.diagnosed);Task("Check battery",s.diagnosed);Task("Replace battery",s.installed);Task("Verify repair",s.repairTested);Task("Sign ticket",s.serviceSigned);}else{Task("Request a battery",s.requested);Task("Choose diagnosis",s.diagnosed);Task("Install battery",s.installed);}}
            if(job==Profession.Sanitation){Task("Choose collection order",s.items.Any(i=>i.custody=="waste-carry"||i.custody=="recorded"));Task("Sort three batches",s.items.Count(i=>i.custody=="sorted"||i.custody=="recorded")>=3);Task("File manifest",s.items.Count(i=>i.custody=="recorded")>=3);}
            return board;
        }
        public string Note(Profession job,bool assistant)
        {
            if(assistant)return "Dispatch / intake support";
            var s=Domain.State;
            if(job==Profession.Shopkeeper)return !s.received?"Receive the rear shipment":!s.prepared?"Prepare the garage parcel":"Serve counter orders";
            if(job==Profession.Mechanic)return s.installed?"Repair filed / scrap ready":!s.requested?"Request at garage desk":!s.diagnosed?"Lift hood and check probes":!s.dispatched?"Collect market parcel":"Turn terminal and seat battery";
            return s.items.Any(i=>i.custody=="sorted")?"File the yard manifest":"Choose and collect a batch";
        }
        void Update()
        {
            if(!Ready)return;
            var local=FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsSpawned&&p.IsOwner);
            if(torqueTarget) {
                if(!torquePlayer||!torquePlayer.IsSpawned||!torquePlayer.GetComponent<PrototypeInput>().GameplayActive||!NetworkShop.InRange(torquePlayer.Interactor,torqueTarget)||Keyboard.current!=null&&Keyboard.current.qKey.wasPressedThisFrame){torqueTarget=null;torquePlayer=null;return;}
                if(Mouse.current!=null&&Mouse.current.leftButton.isPressed)torque=Mathf.Clamp(torque+Mouse.current.delta.ReadValue().x*.65f,0,90);
                if(Time.time-lastGesture>.45f){lastGesture=Time.time;var authored=torquePlayer.GetComponent<BusinessAsUsual.RescueV2.RescueHands>();if(authored)authored.Begin("bolt",torqueTarget.InteractionPoint);else torquePlayer.GetComponent<FirstPersonHands>()?.Play(FirstPersonHands.Gesture.ToolTurn,torqueTarget.InteractionPoint);}
                if(torque>=60){torquePlayer.SendCommand(NetworkShop.Id(torqueTarget));torqueEnded=Time.time;torqueTarget=null;torquePlayer=null;}
            }
            if(local && local.GetComponent<PrototypeInput>().GameplayActive && Keyboard.current!=null && Keyboard.current.rKey.wasPressedThisFrame && local.Interactor.Current is WorkdayStation target)
                local.SendCommand("alt:"+NetworkShop.Id(target));
        }
        void LateUpdate()
        {
            if(!Ready)return;
            if(World){World.Tick();return;}
            var s=Domain.State;
            foreach(var item in s.items) {
                var visual=visuals[item.id]; bool visible=item.custody!="recorded"&&item.custody!="sorted"&&item.custody!="customer"&&item.custody!="vehicle";
                visual.gameObject.SetActive(visible);
                if(!visible)continue;
                var holder=FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsSpawned&&p.OwnerClientId==item.holder);
                Vector3 at=item.custody=="installed"?Stations["Socket"].transform.position:item.custody=="shipment"?Stations["Shipment"].transform.position:item.custody=="market"||item.custody=="dispatch"?Stations["Dispatch"].transform.position:
                    item.custody=="garage"?Stations["Bay tray"].transform.position:item.custody=="yard"?Stations["Yard intake"].transform.position:item.custody=="square"?Stations["Square pickup"].transform.position:item.custody=="counter"?Stations["Counter"].transform.position:
                    item.material=="paper"?Stations["Paper pickup"].transform.position:Stations["Metal pickup"].transform.position;
                if(holder)at=holder.Interactor.carryAnchor.position+holder.transform.right*(item.material=="battery"?.10f:-.10f);
                else if(item.material=="battery")at+=Vector3.up*.20f;
                if(item.material=="stock")at+=Vector3.right*(Array.IndexOf(s.items,item)-5)*.25f;
                visual.position=visual.position==Vector3.zero?at:Vector3.Lerp(visual.position,at,1-Mathf.Exp(-22*Time.deltaTime));
                visual.rotation=holder?Quaternion.Euler(0,holder.LookYaw.Value,0):Quaternion.identity;
                visual.GetComponent<Collider>().enabled=!holder;
            }
            hood.localRotation=Quaternion.Slerp(hood.localRotation,Quaternion.Euler(s.hood?-55:0,0,0),1-Mathf.Exp(-12*Time.deltaTime));
            terminal.rotation=Quaternion.Slerp(terminal.rotation,Quaternion.Euler(0,s.bolts*90+(Turning?torque:0),0),1-Mathf.Exp(-12*Time.deltaTime));
            terminal.gameObject.SetActive(!s.installed);
            gate.localRotation=Quaternion.Slerp(gate.localRotation,Quaternion.Euler(0,0,Now<s.gateUntil?75:0),1-Mathf.Exp(-12*Time.deltaTime));
        }
        public TownGuidance Guidance(NetworkPlayer player)
        {
            if(World)return World.Guidance(player);
            var job=shop.Town.Job(player.OwnerClientId);bool assistant=IsAssistant(player.OwnerClientId);
            var s=Domain.State;var held=Domain.Held(player.OwnerClientId);
            var g=new TownGuidance {job=job};
            string next=null;
            void Step(string station,string note,string detail){next=station;g.note=note;g.detail=detail;}
            if(s.closed){g.note="Workday finished";g.detail="The shared shift has ended. Start a fresh session to play again.";return g;}
            if(held!=null&&held.custody=="parcel")Step("Bay tray","Deliver parcel to garage","Leave the market through its front door. At the garage receiving tray, press E to lift the gate, then E again to place the parcel. Q returns it.");
            else if(held!=null&&held.custody=="waste-carry") {
                if(assistant)Step("Manifest","Stage batch at yard","Take your load to the yard manifest desk. E stages it for the sanitation lead.");
                else if(!held.inspected)Step("Mixed chute","Inspect your carried batch","Go to the yard chutes. Press R to inspect your load before choosing its matching chute.");
                else Step(held.material=="paper"?"Paper chute":held.material=="metal"?"Metal chute":"Mixed chute","Sort the "+held.material+" batch","Take the inspected load to its matching yard chute and press E. Then file the manifest at the desk.");
            }
            else if(assistant&&job==Profession.Mechanic&&!s.installed&&s.hood&&!s.loadChecked)Step("Probes","Help with intake checks","At the garage probes, press R twice for two load readings. The lead handles the repair.");
            else if(assistant&&job==Profession.Mechanic)Step("Garage reset","Support the mechanic","The lead opens the hood before your intake check. E resets the tool tray; R at the receiving tray helps a parcel carrier.");
            else if(job==Profession.Shopkeeper) {
                var part=Domain.Find("PART-8A-001");
                if(!s.received)Step("Shipment","Receive market shipment","Enter Daylight Market. Find the shipment plate beside the stockroom and press E.");
                else if(!s.prepared)Step("Dispatch","Prepare garage parcel","At market dispatch, R checks the contents (optional); E prepares the parcel.");
                else if(part.custody=="dispatch")Step("Parcel","Pick up garage parcel","At market dispatch, press E to carry the prepared parcel to the garage.");
                else if(assistant)Step("Bay tray","Support garage delivery","Another worker has the parcel. R at the receiving tray holds its gate; E at a reset plate tidies a workstation.");
                else if(s.customerOrders<3)Step("Counter","Serve a customer order","At the market counter, press E to serve an order. Delivery and repair also need your colleagues.");
                else Step("Market reset","Market work complete","Your orders are served. Help colleagues or reset this workstation with E while the shared repair and waste work finish.");
            }
            else if(job==Profession.Mechanic) {
                if(!s.requested)Step("Request","Request a garage battery","Go to the garage request plate beside the rear crate and press E. The market supplies the same battery you will install.");
                else if(!s.hood&&!s.installed)Step("Hood","Open the vehicle hood","Go to the left service bay and press E at the hood plate.");
                else if(!s.diagnosed)Step("Probes","Diagnose the battery","E at the probes gives a quick diagnosis. R twice performs a full check and also checks the incoming part.");
                else if(!s.dispatched)Step("Request","Waiting for garage parcel","The market worker can deliver it. If nobody responds, R at the request plate retrieves the same parcel after 25 seconds.");
                else if(!s.verified&&!s.loadChecked)Step("Probes","Check the incoming battery","The parcel was sent unchecked. Press R twice at the probes before loosening the terminal.");
                else if(!s.released&&!s.installed)Step("Terminal","Loosen battery terminal","Press E at the terminal, then hold left mouse and drag right. R before turning chooses the optional two-turn salvage workflow.");
                else if(!s.installed)Step("Socket","Fit replacement battery","Press E at the socket plate to seat the battery. Repairing it releases packaging and scrap for sanitation.");
                else Step("Garage reset","Repair complete","Sanitation can now collect the packaging and old battery. Help colleagues or reset your tool tray with E.");
            }
            else if(job==Profession.Sanitation) {
                if(s.items.Any(i=>i.custody=="sorted"))Step("Manifest","File sorted batches","Go to the yard manifest desk and press E. Sorting alone does not file the work.");
                else if(s.items.Any(i=>i.custody=="yard"))Step("Yard intake","Collect staged batch","E at the yard intake lifts the batch left by your assistant.");
                else if(Domain.Find("BAG-8A-001").custody=="square")Step("Square pickup","Collect the square bag","Find the bag in the town square and press E. Carry it to the yard chutes; R there inspects the contents.");
                else if(s.installed&&Domain.Find("PACK-8A-001").custody=="garage-waste")Step("Paper pickup","Collect garage packaging","Go behind the left garage service bay. E lifts the discarded packaging.");
                else if(s.installed&&Domain.Find("OLD-8A-001").custody=="garage-waste")Step("Metal pickup","Collect old battery","E at the garage scrap plate lifts the old battery. Inspect it at the yard: repair choices change its treatment.");
                else Step("Yard reset","Waiting for garage waste","The garage repair creates two more batches. Collect the square bag first, then help colleagues or reset the sorting station with E.");
            }
            else {g.note="Choose your work";g.detail="Press J to choose a profession.";}
            if(s.installed&&s.customerOrders>=1&&s.items.Count(i=>i.custody=="recorded")>=3)Step("Shift report","File the shared workday","Everyone's essential work is complete. E at the town shift-report plate finishes the day.");
            if(next!=null) {
                Interactable destination=Stations[next];
                bool market=destination.transform.position.x<-25,inside=player.transform.position.x<-25;
                if(market&&!inside) { destination=shop.Targets.Values.OfType<TownStation>().FirstOrDefault(t=>t.kind==TownStation.Kind.Portal);g.detail="Go to Daylight Market. E opens its front door; walk through the doorway. "+g.detail; }
                else if(!market&&inside)destination=shop.Targets.Values.OfType<ShopStation>().FirstOrDefault(t=>t.kind==ShopStation.Kind.Exit);
                if(destination)g.objectives.Add(new TownObjective {id=NetworkShop.Id(destination),title=market&&!inside?"Enter Daylight Market":!market&&inside?"Exit to town":Stations[next].label,icon=TownGuidance.RoleIcon(job),target=destination});
            }
            return g;
        }
        public void DrawBook(TownHandbook.Page page,NetworkPlayer player)
        {
            var s=Domain.State;var job=shop.Town.Job(player.OwnerClientId);
            theme.Text(275,330,page==TownHandbook.Page.Records?job+" paper file":page==TownHandbook.Page.Summary?"Living workday report":page==TownHandbook.Page.Inventory?"Visible workplace custody":"Work with your colleagues",32,2,1260);
            if(page==TownHandbook.Page.Records) {
                var records=shop.IsServer?RecordsFor(player.OwnerClientId):LocalRecords;
                for(int i=0;i<Math.Min(records.Length,6);i++) {var r=records[records.Length-1-i];float x=i<3?275:997,y=402+i%3*145;
                    TownGraphics.Card(new Rect(x,y,610,132));theme.Text(x+16,y+13,r.heading,25,1,570);theme.Text(x+16,y+51,r.text,21,0,570,0,true,58);theme.Text(x+16,y+105,r.reference+" / "+r.interval,16,0,570);}
                if(records.Length==0)theme.Text(275,430,"No processed work filed yet. Other professions retain their own copies.",25,0,1250);
            } else if(page==TownHandbook.Page.Inventory) {
                // Local observations, not the server audit. No actor IDs or timestamps.
                var items=s.items.Where(i=>i.holder==player.OwnerClientId||job==Profession.Shopkeeper&&(i.custody=="market"||i.custody=="dispatch"||i.custody=="counter")||job==Profession.Mechanic&&(i.custody=="garage"||i.custody=="installed")||job==Profession.Sanitation&&(i.custody=="sorted"||i.custody=="recorded")).ToArray();
                for(int i=0;i<items.Length;i++)theme.Text(275,410+i*54,items[i].id+" / "+items[i].custody,25,1,1240);
                if(items.Length==0)theme.Text(275,430,"Check the workplace trays. Your load appears here while carried.",25,0,1250);
            } else if(page==TownHandbook.Page.Summary) {
                theme.Text(275,420,"Town supply "+s.stability+" / 20",36,2,1100);
                theme.Text(275,495,"Customer orders "+s.customerOrders+" / 3     Repair "+(s.installed?"complete":"waiting"),28,1,1180);
                theme.Text(275,560,"Waste batches filed "+s.items.Count(i=>i.custody=="recorded")+" / 3",28,1,1180);
                theme.Text(275,635,s.closed?"Shift filed. Start a fresh session to repeat.":"Finish one order, the repair and three waste batches; file at the square.",25,0,1250,0,true,90);
            } else {
                var tasks=Tasks(job,IsAssistant(player.OwnerClientId),player.OwnerClientId).Tasks.ToArray();for(int i=0;i<tasks.Length;i++)theme.Text(275,408+i*65,(tasks[i].Complete?"DONE  ":"NEXT  ")+tasks[i].description,28,1,1230);
                var guide=Guidance(player);theme.Text(275,710,guide.note,30,2,1200);
                theme.Text(275,775,guide.detail,23,0,1240,0,true,78);
            }
        }
        public void DrawStatus()
        {
            var s=Domain.State;double left=s.began==0?s.durationSeconds:Math.Max(0,s.durationSeconds-(Now-s.began));
            TownGraphics.Card(new Rect(48,192,480,65));
            theme.Text(64,205,FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).Count(p=>p.IsSpawned)+" STAFF / Workday "+(s.closed?"filed":((int)left/60)+":"+((int)left%60).ToString("D2")),21,1,440);
            theme.Text(64,235,"Town supply "+s.stability+" / 20",17,0,440);
        }
        void OnGUI()
        {
            if(!Turning)return;
            var old=GUI.matrix;float scale=ShopHUD.Scale(Screen.width,Screen.height);var offset=ShopHUD.Offset(Screen.width,Screen.height,scale);
            GUI.matrix=Matrix4x4.TRS(new Vector3(offset.x,offset.y,0),Quaternion.identity,new Vector3(scale,scale,1));
            TownGraphics.Card(new Rect(630,840,660,70));theme.Text(650,855,"Hold LMB + drag right / terminal "+Mathf.RoundToInt(torque)+" degrees",24,1,620);GUI.matrix=old;
        }
        void OnDestroy(){if(Instance==this)Instance=null;}
    }
    public sealed class WorkdayStation : Interactable
    {
        public string action,alternative,label,altLabel;
        public int profession;
        public override Vector3 InteractionPoint=>transform.position;
        public override string Prompt=>LivingWorkday.Instance&&LivingWorkday.Instance.Domain.State.suspicious&&action=="deliver"&&LivingWorkday.Instance.Domain.Find("PART-8A-001").custody=="garage"?"Lift sealed parcel / R Hold receiving gate":LivingWorkday.Instance&&LivingWorkday.Instance.Domain.State.physical&&action=="hood"?(LivingWorkday.Instance.Domain.State.hood?"Close hood":"Open hood"):LivingWorkday.Instance&&LivingWorkday.Instance.Domain.State.physical&&action=="scan-order"&&LivingWorkday.Instance.Domain.State.customerOrders<3&&LivingWorkday.Instance.Domain.State.items[4+LivingWorkday.Instance.Domain.State.customerOrders].custody!="checkout"?"Change work priority":label+(alternative!=null?" / R "+altLabel:"");
        public override bool CanInteract(PlayerInteractor player)
        {
            var n=player.GetComponent<NetworkPlayer>();var w=LivingWorkday.Instance;
            if(!n||!w||!w.Ready)return false;
            int job=(int)TownJobs.Instance.Job(n.OwnerClientId);if(job==0||profession!=0&&job!=profession)return false;
            var s=w.Domain.State;
            if(s.closed)return false;
            if(s.physical){
                var held=w.Domain.Held(n.OwnerClientId);var part=w.Domain.Find("PART-8A-001");bool empty=held==null;
                if(w.IsAssistant(n.OwnerClientId)&&(profession==2&&action!="diagnose"&&action!="request"||action.StartsWith("sort-")))return false;
                switch(action){
                    case "take-stock-0":case "take-stock-1":case "take-stock-2":return empty&&s.items[4+int.Parse(action.Substring(action.Length-1))].custody==(name.StartsWith("Stock box")?"stockroom":"shelf");
                    case "stock-0":case "stock-1":case "stock-2":return held?.custody=="stock-carry"&&!held.shelved;
                    case "place-order":return held?.custody=="stock-carry"&&held.shelved;
                    case "scan-order":return s.customerOrders<3&&!w.IsAssistant(n.OwnerClientId);
                    case "cart":return (held==null||held.custody=="waste-carry")&&(s.cartPusher==WorkdayDomain.Nobody||s.cartPusher==n.OwnerClientId);
                    case "customer":return false;
                    case "hood":return !s.serviceSigned&&(!s.installed||s.repairTested);
                    case "diagnose":return s.hood&&!s.installed&&!s.released&&(!s.diagnosed||!s.verified&&!s.loadChecked);
                    case "bolt":return s.hood&&s.diagnosed&&!s.released&&part.custody=="garage";
                    case "remove-battery":return s.hood&&s.released&&!s.removed&&empty;
                    case "scrap-battery":return held?.custody=="removed-carry";
                    case "grab-battery":return s.removed&&s.oldPlaced&&!s.installed&&empty&&part.custody=="garage";
                    case "install":return s.hood&&held?.custody=="component-carry";
                    case "test-repair":return s.hood&&s.installed&&!s.repairTested;
                    case "service-ticket":return s.installed&&s.repairTested&&!s.hood&&!s.serviceSigned;
                    case "deliver":return held?.custody=="parcel"||s.suspicious&&empty&&part.custody=="garage"&&w.Domain.Find("PACK-8A-001").custody=="garage"&&!s.removed;
                    case "request":return s.requested&&!s.dispatched&&empty&&w.Now-s.requestedAt>=25;
                    case "paper":case "metal":return empty&&w.Domain.Find(action=="paper"?"PACK-8A-001":"OLD-8A-001").custody=="garage-waste";
                    case "square":return empty&&w.Domain.Find("BAG-8A-001").custody=="square";
                    case "file":return s.items.Any(i=>i.custody=="sorted")||held?.custody=="waste-carry"&&w.IsAssistant(n.OwnerClientId);
                    case "close":return s.serviceSigned&&s.customerOrders>=1&&s.items.Count(i=>i.custody=="recorded")>=3;
                    case "cleanup":return w.IsAssistant(n.OwnerClientId)&&s.cleaned<12;
                    case "sort-paper":case "sort-metal":case "sort-mixed":return held?.custody=="waste-carry";
                }
                if(action.StartsWith("cart-unload-"))return empty&&s.items[int.Parse(action.Substring(12))].custody=="cart"&&(s.cartPusher==WorkdayDomain.Nobody||s.cartPusher==n.OwnerClientId);
            }
            if(w.IsAssistant(n.OwnerClientId)&&(action=="customer"||action=="hood"||action=="bolt"||action=="install"||action.StartsWith("sort-")))return false;
            if(action=="prepare")return s.received&&!s.prepared;
            if(action=="hood")return !s.installed;
            if(action=="diagnose")return s.hood&&!s.installed&&(!s.diagnosed||!s.loadChecked&&!s.verified);
            if(action=="customer")return s.customerOrders<3;
            if(action=="pickup")return s.prepared&&!s.dispatched&&w.Domain.Find("PART-8A-001").custody=="dispatch";
            if(action=="install")return s.released&&!s.installed;
            if(action=="bolt")return s.hood&&s.diagnosed&&!s.released&&!s.installed;
            if(action=="paper"||action=="metal")return s.installed&&w.Domain.Find(action=="paper"?"PACK-8A-001":"OLD-8A-001").custody=="garage-waste";
            if(action=="square")return w.Domain.Find("BAG-8A-001").custody=="square";
            if(action=="yard")return s.items.Any(i=>i.custody=="yard");
            if(action=="receive")return !s.received;
            return true;
        }
        public override bool Interact(PlayerInteractor player)=>false;
    }
    public sealed class WorkdayThreshold : MonoBehaviour
    {
        public bool entrance;
        readonly Dictionary<ulong,double> crossed=new Dictionary<ulong,double>();
        void Update()
        {
            var w=LivingWorkday.Instance;var adapter=NetworkShop.Instance;
            if(!w||!adapter||!adapter.IsServer)return;
            foreach(var p in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).Where(p=>p.IsSpawned)) {
                Vector3 delta=p.transform.position-transform.position;
                if(Mathf.Abs(delta.x)>.70f||Mathf.Abs(delta.z)>.40f)continue;
                if(crossed.TryGetValue(p.OwnerClientId,out var when)&&w.Now-when<1)continue;
                if(entrance){var door=adapter.Targets.Values.OfType<SimpleDoor>().FirstOrDefault();if(door&&!door.IsOpen)continue;}
                crossed[p.OwnerClientId]=w.Now;
                p.Motor.Teleport(entrance?new Vector3(-40,.08f,3.2f):new Vector3(-8.2f,.08f,2.4f));
                p.SendFeedback(entrance?"Daylight Market / walk-through threshold":"Back in Daylight Town");
            }
        }
    }
}
