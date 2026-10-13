using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessAsUsual.Town
{
    // One server-owned registry for the physical workday. Job views never own inventories.
    // Installed and recorded objects remain in this registry: processing does not delete stock.
    [Serializable] public sealed class WorkItem
    {
        public string id, material, custody,sku;
        public ulong holder = ulong.MaxValue;
        public int revision;
        public bool inspected,shelved,fromCart;
    }
    [Serializable] public sealed class WorkEvent
    {
        public int sequence;
        public string item, from, to, action;
        public ulong actor;
        public double time;
    }
    [Serializable] public sealed class WorkRecord
    {
        public string id, reference, heading, text, interval;
        public int profession;
    }
    // Future statements use a separate attributed schema. They are never accepted as WorkEvents.
    [Serializable] public sealed class WorkClaim { public string text, recordReference; public ulong author; public double statedAt; }
    [Serializable] public sealed class WorkdayState
    {
        public WorkItem[] items;
        public bool received, verified, requested, prepared, dispatched, diagnosed, loadChecked, hood, released, installed, bench, closed;
        public bool physical,removed,oldPlaced,repairTested,serviceSigned,shiftSigned,suspicious;
        public string parcelNote="Direct transfer",serviceNote="Initial diagnosis";public int noteAmendments;
        public int customerOrders, bolts, stability = 5, cleaned, assisted, checkSamples;
        public float durationSeconds=480;
        public double requestedAt, began, gateUntil;
        public string priority = "garage", collectionOrder = "paper";
        public float cartX=7.5f,cartZ=-4.9f,cartYaw;public ulong cartPusher=ulong.MaxValue;public int cartChoice;
        public WorkRecord[] records = Array.Empty<WorkRecord>();
    }
    public sealed class WorkdayDomain
    {
        public WorkdayState State { get; private set; }
        // Internal truth is never part of WorkdayState or any ordinary player RPC.
        public readonly List<WorkEvent> Audit = new List<WorkEvent>();
        public readonly List<WorkRecord> Records = new List<WorkRecord>();
        public const ulong Nobody = ulong.MaxValue;
        public const ulong MunicipalActor=ulong.MaxValue-2;
        ulong recordingActor;
        double servicePulse;
        public WorkdayDomain(bool physical=false,bool suspicious=false)
        {
            State = new WorkdayState { physical=physical,suspicious=suspicious,items = new[] {
                Item("PART-8A-001", "battery", "shipment"),
                Item("PACK-8A-001", "paper", "shipment"),
                Item("OLD-8A-001", "metal", "vehicle"),
                Item("BAG-8A-001", "paper", "square"),
                Item("ORDER-8A-001", "stock", "counter"),
                Item("ORDER-8A-002", "stock", "counter"),
                Item("ORDER-8A-003", "stock", "counter") } };
            if(physical){string[] skus={"cola","tea","crackers"};for(int i=0;i<3;i++){var item=State.items[i+4];item.sku=skus[i];item.custody="stockroom";}}
        }
        static WorkItem Item(string id, string material, string place) => new WorkItem { id=id, material=material, custody=place };
        public WorkItem Find(string id) => State.items.FirstOrDefault(i=>i.id==id);
        public WorkItem Held(ulong actor) => State.items.FirstOrDefault(i=>i.holder==actor);
        public bool Conserved => State.items.Length==7 && State.items.Select(i=>i.id).Distinct().Count()==7 &&
            State.items.All(i=>!string.IsNullOrEmpty(i.custody)) && State.items.Count(i=>i.custody=="installed")<=1 &&
            State.items.Where(i=>i.holder!=Nobody).GroupBy(i=>i.holder).All(g=>g.Count()==1 || g.All(i=>i.custody=="parcel"));
        void Move(WorkItem item,string destination,ulong actor,double time,ulong holder=Nobody)
        {
            if(State.physical&&State.began==0)State.began=time;
            Audit.Add(new WorkEvent { sequence=Audit.Count+1,item=item.id,from=item.custody,to=destination,actor=actor,time=time,action="custody" });
            item.custody=destination; item.holder=holder; item.revision++;
        }
        void Observe(WorkItem item,string action,ulong actor,double time)
        {
            Audit.Add(new WorkEvent {sequence=Audit.Count+1,item=item.id,from=item.custody,to=item.custody,actor=actor,time=time,action=action});item.revision++;
        }
        void Record(int profession,string reference,string title,string text,double time)
        {
            if(recordingActor==MunicipalActor){title="Town service / "+title;text+=" Unattended municipal service.";}
            Records.Add(new WorkRecord { id="WD-"+(Records.Count+1).ToString("D3"),profession=profession,reference=reference,
                heading=title,text=text,interval="Shift minute "+Math.Floor(Math.Max(0,time-State.began)/60) });
        }
        // Runtime validates authenticated ownership, range/LOS, current global revision and sequence
        // BEFORE invoking this domain. This method validates profession and all material preconditions.
        public bool Apply(string action,int profession,bool assistant,ulong actor,double time,out string message)
        {
            recordingActor=actor;
            message="Not available yet.";
            var s=State; var part=Find("PART-8A-001"); var pack=Find("PACK-8A-001"); var old=Find("OLD-8A-001");
            bool shop=profession==1, mechanic=profession==2, waste=profession==3;
            if(s.closed && action!="return"){message="Workday filed. Start a fresh session for another shift.";return false;}
            if(action=="return") {
                var held=State.items.Where(i=>i.holder==actor).ToArray(); if(held.Length==0){if(s.physical&&s.cartPusher==actor){s.cartPusher=Nobody;message="Cart parked.";return true;}return false;}
                foreach(var i in held)Move(i,i.custody=="parcel"?"dispatch":i.material=="stock"?(i.shelved?"shelf":"stockroom"):i.fromCart?"cart":i.id=="PART-8A-001"?"garage":i.material=="metal"?"garage-waste":i.id=="BAG-8A-001"?"square":"garage-waste",actor,time);
                if(s.physical&&held.Contains(old)&&s.removed)s.oldPlaced=true;
                message="Load returned safely to its staging point.";return true;
            }
            if(s.suspicious&&action=="collect-parcel"){if(profession==0||Held(actor)!=null||part.custody!="garage"||pack.custody!="garage"||s.removed)return false;Move(part,"parcel",actor,time,actor);Move(pack,"parcel",actor,time,actor);message="Sealed parcel lifted for a stock check or delivery correction.";return true;}
            if(s.suspicious&&(action=="hold-return"||action=="hold-yard")){
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
            if(action.StartsWith("cart-unload-")){
                if(!s.physical||!waste||Held(actor)!=null||!int.TryParse(action.Substring(12),out int index)||index<1||index>3||s.cartPusher!=Nobody&&s.cartPusher!=actor)return false;
                var load=s.items[index];if(load.custody!="cart")return false;s.cartPusher=Nobody;load.fromCart=true;Move(load,"waste-carry",actor,time,actor);message="Selected batch lifted.";return true;
            }
            switch(action) {
                case "receive":
                    if(!shop||s.received||part.custody!="shipment")return false;
                    Move(part,"market",actor,time);Move(pack,"market",actor,time);s.received=true;if(s.began==0)s.began=time;
                    Record(1,pack.id,"Shipment received","One sealed battery parcel received; contents not yet checked.",time);
                    message="Delivery opened. R checks the battery; E prepares a fast dispatch.";return true;
                case "verify":
                    if(!shop||!s.received||s.verified||part.custody!="market")return false;
                    s.verified=true;part.inspected=true;Observe(part,"stock verification",actor,time);Record(1,pack.id,"Stock check","Battery terminals and quantity checked.",time);
                    message="Terminals checked. Garage can install without a second intake check.";return true;
                case "prepare":
                    if(!shop||!s.received||s.prepared||part.custody!="market")return false;
                    Move(part,"dispatch",actor,time);Move(pack,"dispatch",actor,time);s.prepared=true;
                    Record(1,pack.id,"Dispatch slip",s.verified?"One checked battery prepared for garage.":"One sealed battery prepared; contents unchecked.",time);
                    message="Parcel released at market dispatch. Anyone may carry it to the garage.";return true;
                case "customer":
                    if(!shop||assistant||s.customerOrders>=3)return false;
                    if(s.physical&&actor!=MunicipalActor){message="Place the requested goods on the counter and scan them.";return false;}
                    if(s.priority=="garage"&&s.requested&&!s.prepared){message="Garage priority selected. R changes priority before serving customers.";return false;}
                    Move(Find("ORDER-8A-"+(s.customerOrders+1).ToString("D3")),"customer",actor,time);
                    s.customerOrders++;s.stability=Math.Min(20,s.stability+1);
                    Record(1,"ORDER-"+s.customerOrders,"Customer order","Counter stock served; garage parcel may still be waiting.",time);
                    message="Order handed over. Town supply +1.";return true;
                case "take-stock-0":case "take-stock-1":case "take-stock-2":
                    if(!s.physical||!shop||Held(actor)!=null)return false;
                    var stock=State.items[4+int.Parse(action.Substring(action.Length-1))];if(stock.custody!="stockroom"&&stock.custody!="shelf")return false;
                    Move(stock,"stock-carry",actor,time,actor);message="Stock lifted. Match its printed shelf label.";return true;
                case "stock-0":case "stock-1":case "stock-2":
                    if(!s.physical||!shop)return false;var stocked=Held(actor);var shelf=State.items[4+int.Parse(action.Substring(action.Length-1))];
                    if(stocked==null||stocked.material!="stock"||stocked.custody!="stock-carry")return false;
                    if(stocked.id!=shelf.id){message="Different product. Match the label on the box.";return false;}
                    if(stocked.shelved){message="Already stocked. Take it to checkout.";return false;}
                    stocked.shelved=true;Move(stocked,"shelf",actor,time);Record(1,stocked.id,"Shelf replenishment",stocked.sku+" replenished. Earlier handling is not recorded.",time);message="Shelf stocked.";return true;
                case "place-order":
                    if(!s.physical||!shop||assistant||s.customerOrders>=3)return false;var goods=Held(actor);
                    if(goods==null||!goods.shelved||goods.custody!="stock-carry"){message="Collect the requested product from its shelf.";return false;}
                    if(goods.id!=State.items[4+s.customerOrders].id){message="Customer requested "+State.items[4+s.customerOrders].sku+". Goods retained.";return false;}
                    Move(goods,"checkout",actor,time);message="Goods placed. Scan at the register.";return true;
                case "scan-order":
                    if(!s.physical||!shop||assistant||s.customerOrders>=3)return false;var sale=State.items[4+s.customerOrders];
                    if(sale.custody!="checkout"){message="Place the order on the counter first.";return false;}
                    if(s.priority=="garage"&&s.requested&&!s.prepared){message="Garage priority: dispatch the parcel or change priority.";return false;}
                    Move(sale,"customer",actor,time);s.customerOrders++;s.stability=Math.Min(20,s.stability+1);Record(1,sale.id,"Checkout receipt",sale.sku+" sold / quantity 1. Earlier custody and handlers omitted.",time);message="Order scanned. Receipt printed.";return true;
                case "priority":
                    if(!shop||assistant)return false;s.priority=s.priority=="garage"?"customers":"garage";
                    message="Prioritize "+s.priority+". This changes which counter work you can serve first.";return true;
                case "request":
                    if(!mechanic||s.requested)return false;s.requested=true;s.requestedAt=time;if(s.began==0)s.began=time;
                    Record(2,pack.id,"Parts request","Garage requested one battery. Driver and later custody are not recorded here.",time);
                    message="Parts slip sent. Municipal self-service is available after 25 seconds.";return true;
                case "reserve":
                    if(!mechanic||!s.requested||s.installed||Held(actor)!=null)return false;
                    if(time-s.requestedAt<25){message="Self-service opens 25 seconds after the request. Check the vehicle meanwhile.";return false;}
                    if(part.holder!=Nobody || (part.custody!="shipment"&&part.custody!="market"&&part.custody!="dispatch")){message="The parcel is already being delivered. It can be returned with Q.";return false;}
                    Move(part,"garage",actor,time);Move(pack,"garage",actor,time);s.received=s.prepared=s.dispatched=true;
                    Record(2,pack.id,"Municipal recovery","Same shipment retrieved through self-service; no replacement stock created.",time);
                    message="Municipal parcel collected. The same part and packaging are now at the bay.";return true;
                case "pickup":
                    if(profession==0||Held(actor)!=null||part.custody!="dispatch")return false;
                    Move(part,"parcel",actor,time,actor);Move(pack,"parcel",actor,time,actor);
                    message="Carrying the garage parcel. E at the receiving tray; Q returns it.";return true;
                case "deliver":
                    if(part.holder!=actor||part.custody!="parcel")return false;
                    if(!s.physical&&time>s.gateUntil){s.gateUntil=time+8;message="Receiving gate lifted. Place the parcel while it is open.";return true;}
                    Move(part,"garage",actor,time);Move(pack,"garage",actor,time);s.dispatched=true;
                    Record(2,pack.id,"Bay intake","Sealed parcel placed in bay tray; carrier identity omitted.",time);
                    message="Parcel placed in the receiving tray. Mechanic can inspect and install.";return true;
                case "hood":
                    if(!mechanic||assistant||s.installed&&!s.physical)return false;
                    if(s.physical&&s.installed&&s.hood&&!s.repairTested){message="Test the battery first.";return false;}
                    s.hood=!s.hood;if(s.physical&&s.began==0)s.began=time;
                    message=s.hood?(s.physical?"Hood open. Check the battery posts.":"Hood lifted. E on the terminal probes; R performs the full check."):"Hood closed.";return true;
                case "diagnose":
                    if(!mechanic||assistant||!s.hood||s.diagnosed)return false;s.diagnosed=true;
                    if(s.physical&&!s.requested){s.requested=true;s.requestedAt=time;Record(2,pack.id,"Parts request","One replacement battery requested; carrier is not recorded.",time);}
                    message=s.physical?"10.1 V / weak battery. Replacement requested.":"Quick test: weak battery. Intake check still needed if stock was not verified.";return true;
                case "check":
                    if(!mechanic||!s.hood||s.loadChecked)return false;
                    if(s.physical&&!s.requested){s.requested=true;s.requestedAt=time;Record(2,pack.id,"Parts request","One replacement battery requested; carrier is not recorded.",time);}
                    if(s.physical){s.checkSamples=2;s.diagnosed=s.loadChecked=true;Observe(old,"weak battery under load",actor,time);Record(2,old.id,"Load reading","10.1 V observed under load. Replacement contents were not inspected here.",time);message="10.1 V under load / replace battery.";return true;}
                    s.checkSamples++;Observe(old,"load-test sample",actor,time);
                    if(s.checkSamples<2){message="Load leads connected. R again takes the second reading; quick diagnosis needs only E.";return true;}
                    s.diagnosed=s.loadChecked=true;
                    message="Load test passed: replacement battery required. Diagnosis and intake checked.";return true;
                case "bench":
                    if(!mechanic||assistant||s.released||s.installed)return false;s.bench=!s.bench;
                    message=s.bench?"Bench recovery: two terminal turns, salvage old housing, +1 supply.":"Quick swap: one terminal turn; old battery goes to mixed treatment.";return true;
                case "bolt":
                    if(!mechanic||assistant||!s.hood||!s.diagnosed||s.released||part.custody!="garage")return false;
                    if(!s.physical&&!s.verified&&!s.loadChecked){message="Unchecked dispatch. R at the probes checks the intake before installation.";return false;}
                    s.bolts++;s.released=s.bolts>=(s.bench?2:1);
                    message=s.released?(s.physical?"Clamp released. Lift the old battery.":"Terminal released. E seats the new component; old material enters scrap tray."):"First terminal turned. Finish the second connection.";return true;
                case "install":
                    if(s.physical){
                        if(!mechanic||assistant||!s.hood||!s.released||!s.removed||!s.oldPlaced||s.installed||part.holder!=actor||part.custody!="component-carry"){message="Remove the old battery and grab the replacement.";return false;}
                        Move(part,"installed",actor,time);s.installed=true;s.stability=Math.Min(20,s.stability+(s.bench?3:2));message="Replacement seated. Test both battery posts.";return true;
                    }
                    if(!mechanic||assistant||!s.released||s.installed||part.custody!="garage")return false;
                    Move(part,"installed",actor,time);Move(pack,"garage-waste",actor,time);Move(old,"garage-waste",actor,time);
                    old.material=s.bench?"metal":"mixed";s.installed=true;s.hood=false;s.stability=Math.Min(20,s.stability+(s.bench?3:2));
                    Record(2,pack.id,"Service slip",s.bench?"Battery installed; recoverable metal housing released.":"Battery swapped; mixed old battery released for treatment.",time);
                    message="Battery seated. Repair complete; packaging and old component await sanitation.";return true;
                case "remove-battery":
                    if(!s.physical||!mechanic||assistant||!s.hood||!s.released||s.removed||Held(actor)!=null){message="Loosen the battery clamp first.";return false;}
                    s.removed=true;old.material=s.bench?"metal":"mixed";old.inspected=false;Move(old,"removed-carry",actor,time,actor);message="Old battery lifted. Set it in the scrap tray.";return true;
                case "scrap-battery":
                    if(!s.physical||!mechanic||old.holder!=actor||old.custody!="removed-carry"){message="Carry the old battery here.";return false;}
                    Move(old,"garage-waste",actor,time);s.oldPlaced=true;message="Old battery staged for collection.";return true;
                case "grab-battery":
                    if(!s.physical||!mechanic||assistant||!s.removed||!s.oldPlaced||s.installed||Held(actor)!=null||part.custody!="garage"){message="Place the old battery before taking the replacement.";return false;}
                    Move(part,"component-carry",actor,time,actor);if(pack.custody=="garage")Move(pack,"garage-waste",actor,time);message="Replacement lifted. Seat it in the empty tray.";return true;
                case "test-repair":
                    if(!s.physical||!mechanic||assistant||!s.hood||!s.installed||s.repairTested){message="Fit the replacement first.";return false;}
                    s.repairTested=true;Observe(part,"12.6 V repair test",actor,time);Record(2,part.id,"Voltage reading","12.6 V observed after fitting. Earlier handling is omitted.",time);message="12.6 V / test passed. Close the hood.";return true;
                case "service-ticket":
                    if(!s.physical||!mechanic||assistant||!s.installed||!s.repairTested||s.hood||s.serviceSigned){message="Test the repair and close the hood.";return false;}
                    s.serviceSigned=true;Record(2,pack.id,"Service ticket",s.bench?"Battery fitted and tested; metal housing recovered.":"Battery fitted and tested; old battery sent to mixed treatment.",time);message="Service ticket signed.";return true;
                case "stage":
                    if(!waste||!assistant)return false;var staged=Held(actor);if(staged==null||staged.custody!="waste-carry")return false;
                    Move(staged,"yard",actor,time);message="Batch placed in the yard intake. Lead sorts; you can collect the next load.";return true;
                case "cart":
                    if(!s.physical||!waste)return false;var cartLoad=Held(actor);
                    if(cartLoad!=null){if(cartLoad.custody!="waste-carry"||s.items.Count(i=>i.custody=="cart")>=3){message="Cart holds three collection batches.";return false;}cartLoad.fromCart=true;Move(cartLoad,"cart",actor,time);message="Batch loaded into cart.";return true;}
                    if(s.cartPusher!=Nobody&&s.cartPusher!=actor){message="A colleague is pushing this cart.";return false;}
                    s.cartPusher=s.cartPusher==actor?Nobody:actor;message=s.cartPusher==actor?"Push with WASD. E parks the cart.":"Cart parked.";return true;
                case "cart-unload":
                    if(!s.physical||!waste||Held(actor)!=null||s.cartPusher!=Nobody&&s.cartPusher!=actor)return false;
                    var loads=s.items.Where(i=>i.custody=="cart").ToArray();if(loads.Length==0){message="The cart is empty.";return false;}
                    var picked=loads[s.cartChoice%loads.Length];s.cartPusher=Nobody;picked.fromCart=true;Move(picked,"waste-carry",actor,time,actor);message="Batch lifted from cart. Inspect before sorting.";return true;
                case "cart-choice":
                    if(!s.physical||!waste||Held(actor)!=null||!s.items.Any(i=>i.custody=="cart"))return false;s.cartChoice++;message="Selected next cart batch.";return true;
                case "yard":
                    if(!waste||Held(actor)!=null)return false;var waiting=s.items.FirstOrDefault(i=>i.custody=="yard");if(waiting==null)return false;
                    Move(waiting,"waste-carry",actor,time,actor);message="Intake batch lifted. Inspect or process at its material chute.";return true;
                case "paper": case "metal": case "square":
                    if(!waste||Held(actor)!=null)return false;
                    var item=action=="paper"?pack:action=="metal"?old:Find("BAG-8A-001");
                    if(item.custody!=(action=="square"?"square":"garage-waste"))return false;
                    if(s.physical&&s.began==0)s.began=time;
                    item.fromCart=false;Move(item,"waste-carry",actor,time,actor);message="Load lifted. R at the sorting desk inspects; E processes.";return true;
                case "order":
                    if(!waste||Held(actor)!=null||s.items.Any(i=>i.custody=="sorted"||i.custody=="recorded"))return false;s.collectionOrder=s.collectionOrder=="paper"?"metal":"paper";
                    message="Route priority: "+s.collectionOrder+" first. You can still choose a different pickup.";return true;
                case "inspect":
                    if(!waste)return false;var batch=Held(actor);if(batch==null||batch.custody!="waste-carry"||batch.inspected)return false;
                    batch.inspected=true;Observe(batch,"batch inspection",actor,time);Record(3,batch.id,"Batch inspection","Observed material: "+batch.material+". Earlier handling is outside this manifest.",time);
                    message="Batch opened: "+batch.material+". Use its matching chute.";return true;
                case "sort-paper": case "sort-metal": case "sort-mixed":
                    if(!waste||assistant)return false;var load=Held(actor);if(load==null||load.custody!="waste-carry")return false;
                    string material=action.Substring(5);
                    if(material!=load.material){message="Wrong chute. Load retained; inspect it with R.";return false;}
                    bool first=!s.items.Any(i=>i.custody=="sorted"||i.custody=="recorded");
                    int routeBonus=first&&load.material==s.collectionOrder?1:0;
                    Move(load,"sorted",actor,time);s.stability=Math.Min(20,s.stability+(load.inspected?2:1)+routeBonus);
                    message="Material dropped into its chute. File the partial manifest at the desk.";return true;
                case "file":
                    if(!waste)return false;var sorted=s.items.Where(i=>i.custody=="sorted").ToArray();if(sorted.Length==0)return false;
                    foreach(var i in sorted){Move(i,"recorded",actor,time);Record(3,i.id,"Collection manifest",i.material+" processed; source "+(i.id=="BAG-8A-001"?"square":"garage")+". Previous handlers omitted.",time);}
                    message="Manifest filed. Processed items retained in municipal custody.";return true;
                case "cleanup":
                    if(profession==0||s.cleaned>=12)return false;s.cleaned++;
                    message="Workstation reset. Useful fallback work; up to 12 resets per shift.";return true;
                case "assist":
                    if(profession==0||time<s.gateUntil)return false;s.assisted++;s.gateUntil=time+8;message="Receiving gate held open for 8 seconds.";return true;
                case "close":
                    if(profession==0||!s.installed||s.physical&&!s.serviceSigned||s.customerOrders<1||s.items.Count(i=>i.custody=="recorded")<3){message="Finish the customer order, service ticket and waste tickets.";return false;}
                    s.closed=s.shiftSigned=true;message="Living workday filed. All objects conserved; town supply "+s.stability+"/20.";return true;
            }
            return false;
        }
        public void Disconnect(ulong actor,double time)
        { if(Held(actor)!=null)Apply("return",0,false,actor,time,out _);if(State.cartPusher==actor)State.cartPusher=Nobody; }
        // The host calls this only for absent professions. One registry, explicit municipal authorship.
        // Services do not create humans or replacement stock, and cannot take a human-held item.
        public bool AdvanceMunicipal(bool marketAbsent,bool mechanicAbsent,bool wasteAbsent,double time)
        {
            var s=State;if(!s.physical||s.closed||s.began==0||time-servicePulse<3)return false;servicePulse=time;
            bool Do(string action,int job)=>Apply(action,job,false,MunicipalActor,time,out _);
            var part=Find("PART-8A-001");var pack=Find("PACK-8A-001");
            if(marketAbsent&&s.requested&&!s.dispatched&&time-s.requestedAt>=12&&part.holder==Nobody&&(part.custody=="shipment"||part.custody=="market"||part.custody=="dispatch")){
                recordingActor=MunicipalActor;Move(part,"garage",MunicipalActor,time);Move(pack,"garage",MunicipalActor,time);s.received=s.prepared=s.dispatched=true;Record(2,pack.id,"Supply cabinet","One conserved battery parcel placed in the garage cabinet.",time);return true;
            }
            if(marketAbsent&&s.customerOrders<1)return Do("customer",1);
            if(mechanicAbsent&&!s.serviceSigned){
                if(!s.requested)return Do("request",2);
                if(!s.hood&&!s.repairTested)return Do("hood",2);
                if(!s.diagnosed)return Do("diagnose",2);
                if(!s.dispatched)return false;
                if(!s.verified&&!s.loadChecked)return Do("check",2);
                if(!s.released)return Do("bolt",2);
                if(!s.removed)return Do("remove-battery",2);
                if(!s.oldPlaced)return Do("scrap-battery",2);
                if(!s.installed){if(part.holder!=MunicipalActor)return Do("grab-battery",2);return Do("install",2);}
                if(!s.repairTested)return Do("test-repair",2);
                if(s.hood)return Do("hood",2);
                return Do("service-ticket",2);
            }
            if(wasteAbsent){
                var held=Held(MunicipalActor);if(held!=null&&held.custody=="waste-carry")return !held.inspected?Do("inspect",3):Do("sort-"+held.material,3);
                if(s.items.Any(i=>i.custody=="sorted"))return Do("file",3);
                foreach(var action in new[]{"square","paper","metal"}){var item=Find(action=="square"?"BAG-8A-001":action=="paper"?"PACK-8A-001":"OLD-8A-001");if(item.custody==(action=="square"?"square":"garage-waste"))return Do(action,3);}
            }
            return false;
        }
        public bool Expire(double time,double seconds)
        {
            var s=State;if(s.closed||s.began==0||time-s.began<seconds)return false;
            int missing=(s.customerOrders<1?1:0)+(s.installed&&(!s.physical||s.serviceSigned)?0:1)+(s.items.Count(i=>i.custody=="recorded")<3?1:0);
            s.stability=Math.Max(0,s.stability-missing*2);s.closed=true;return true;
        }
        public WorkdayState PublicState()
        {
            // Records are delivered separately to the relevant profession, not in shared snapshots.
            var copy = new WorkdayState {
                suspicious=State.suspicious,parcelNote=State.parcelNote,serviceNote=State.serviceNote,noteAmendments=State.noteAmendments,physical=State.physical,removed=State.removed,oldPlaced=State.oldPlaced,repairTested=State.repairTested,serviceSigned=State.serviceSigned,shiftSigned=State.shiftSigned,
                items=State.items,received=State.received,verified=State.verified,requested=State.requested,prepared=State.prepared,
                dispatched=State.dispatched,diagnosed=State.diagnosed,loadChecked=State.loadChecked,hood=State.hood,released=State.released,
                installed=State.installed,bench=State.bench,closed=State.closed,customerOrders=State.customerOrders,bolts=State.bolts,checkSamples=State.checkSamples,
                stability=State.stability,cleaned=State.cleaned,assisted=State.assisted,durationSeconds=State.durationSeconds,requestedAt=State.requestedAt,began=State.began,gateUntil=State.gateUntil,
                priority=State.priority,collectionOrder=State.collectionOrder,cartX=State.cartX,cartZ=State.cartZ,cartYaw=State.cartYaw,cartPusher=State.cartPusher,cartChoice=State.cartChoice };
            return copy;
        }
        public void Restore(WorkdayState state) { State=state; }
    }
}
