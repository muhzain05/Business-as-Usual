using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.Prototype;
using BusinessAsUsual.Shopkeeper;

namespace BusinessAsUsual.Town
{
    public sealed class TownObjective
    {
        public string id,title,icon;
        public Interactable target;
        public Vector3 point;
        public bool active=true;
        public Vector3 Position=>target?target.InteractionPoint:point;
    }
    // A local presentation of existing authoritative work; never changes tasks or inventory.
    public sealed class TownGuidance
    {
        public Profession job;
        public string note,detail;
        public readonly List<TownObjective> objectives=new List<TownObjective>();
        public TownObjective Primary=>objectives.FirstOrDefault(o=>o.active);
        public static Vector3 MapPosition(Vector3 world)=>world.x<-25?new Vector3(-8,0,6.5f):world;
        public static string RoleIcon(Profession role)=>role==Profession.Mechanic?"wrench":role==Profession.Sanitation?"recycle":"shop";
        public static string TaskIcon(string id)=>id=="receive"||id=="route"?"slip":id=="inspect"?"inspect":id=="parts"||id.StartsWith("stock:")?"parts":id=="repair"?"wrench":id=="collect"?"bag":id=="sort"?"recycle":id=="orders"?"basket":"record";
        public static string TaskTitle(string id)=>id=="receive"?"Receive delivery / slips":id=="route"?"Pick up route slip":id=="inspect"?"Inspect both bays":id=="parts"?"Collect parts":id=="repair"?"Repair both bays":id=="collect"?"Collect route bags":id=="sort"?"Match & sort":id=="record"?"File your records":id=="orders"?"Serve three orders":id=="close"?"Review & close":id.StartsWith("stock:")?"Restock "+ShopUIView.NameFor(id.Substring(6)):id;
        public static float Productivity(TownJobs jobs)
        {
            var tasks=new[]{Profession.Shopkeeper,Profession.Mechanic,Profession.Sanitation}.SelectMany(job=>jobs.Tasks(job).Tasks).ToArray();
            return tasks.Length==0?0:tasks.Sum(t=>Mathf.Clamp01((float)t.progress/t.target))/tasks.Length;
        }
        // Explicit SelectMany avoids exposing another profession's detailed work in this view.
        public static TownGuidance Create(NetworkPlayer player,NetworkShop shop)
        {
            if(shop.Workday)return shop.Workday.Guidance(player);
            var view=new TownGuidance {job=shop.Town.Job(player.OwnerClientId)};
            var jobs=shop.Town;var shift=shop.Shift;ulong actor=player.OwnerClientId;
            void Add(string name,string title,string icon,bool active=true)
            {
                var item=shop.Targets.FirstOrDefault(p=>p.Key==name||p.Key.EndsWith("/"+name));
                if(item.Value)view.objectives.Add(new TownObjective{id=item.Key,title=title,icon=icon,target=item.Value,active=active});
            }
            void Station(TownStation.Kind kind,string title,string icon,int index=-1,string material=null,bool active=true)
            {
                foreach(var station in shop.Targets.Values.OfType<TownStation>().Where(s=>s.kind==kind&&(index<0||s.index==index)&&(material==null||s.materialType==material)))
                    view.objectives.Add(new TownObjective{id=NetworkShop.Id(station),title=title,icon=icon,target=station,active=active});
            }
            void Set(string note,string detail){view.note=note;view.detail=detail;}
            if(view.job==Profession.None){Set("Choose your work","J opens the staff desk.");return view;}
            if(view.job==Profession.Mechanic)
            {
                if(!jobs.Started[1]){Set("Collect service slips","The garage desk has today's two vehicles.");Station(TownStation.Kind.ServiceDesk,"Garage desk","slip");}
                else if(jobs.Completed[1]){Set("Workshop shift filed","Your service records are in the handbook.");}
                else if(jobs.Services.Any(s=>!s.inspected)){
                    Set("Inspect the vehicles","Hold E at the magnifying-glass work points.");
                    for(int i=0;i<jobs.Services.Length;i++)if(!jobs.Services[i].inspected)Station(TownStation.Kind.Inspect,"Inspect bay "+(i+1),"inspect",i);
                }
                else if(!jobs.PartsRequested){Set("Send the parts slip","The market supplies your replacements.");Station(TownStation.Kind.RequestParts,"Parts request","slip");}
                else if(jobs.Services.Any(s=>!s.repaired&&jobs.Parts.Count("mechanic",s.part)==0)){
                    bool ready=TownJobs.PartSkus.Any(s=>jobs.Parts.Count("counter",s)>0)||jobs.Owners[0]==NetworkShop.Nobody;
                    Set(ready?"Collect replacement parts":"Await the market parcel",ready?"The pickup locker is beside Daylight Market.":"Your shopkeeper needs to pack the request.");Station(TownStation.Kind.CollectParts,"Market parts locker",ready?"parts":"wait");
                }
                else if(jobs.Services.Any(s=>!s.repaired)){
                    Set("Repair the marked bays","Hold E at the wrench work point.");
                    for(int i=0;i<jobs.Services.Length;i++)if(!jobs.Services[i].repaired)Station(TownStation.Kind.Repair,"Repair bay "+(i+1),"wrench",i);
                }
                else {bool filed=jobs.Services.All(s=>s.recorded);Set(filed?"Close your workshop shift":"File the service slips","Return your paperwork to the garage desk.");Station(TownStation.Kind.ServiceDesk,filed?"Shift review":"File service slips","record");}
                if(jobs.Started[1]&&!jobs.Completed[1])for(int i=0;i<jobs.Services.Length;i++)if(jobs.Services[i].inspected&&!jobs.Services[i].repaired&&!view.objectives.Any(o=>o.target is TownStation s&&s.kind==TownStation.Kind.Repair&&s.index==i))Station(TownStation.Kind.Repair,"Repair bay "+(i+1),"wrench",i,active:false);
            }
            else if(view.job==Profession.Sanitation)
            {
                var held=jobs.Bags.FirstOrDefault(b=>b.holder==actor);
                if(!jobs.Started[2]){Set("Pick up your route slip","The collection yard desk starts your round.");Station(TownStation.Kind.RouteDesk,"Yard route desk","slip");}
                else if(jobs.Completed[2])Set("Route manifest filed","Your collection work is recorded.");
                else if(held!=null){Set("Sort this "+held.materialType.ToLowerInvariant()+" bag","Match its symbol at the collection yard.");Station(TownStation.Kind.Sort,held.materialType+" bin","bin",material:held.materialType);}
                else{
                    bool records=jobs.Bags.Any(b=>b.stage==1),close=jobs.DisposalRecords.Count>=3;
                    Set(records?"File your sorted bags":close?"Review your route":"Collect the marked bags",records||close?"Return to the yard desk; extra pickups remain optional.":"Square, alley and yard pickups share the same town block.");
                    if(records||close)Station(TownStation.Kind.RouteDesk,records?"File manifest":"Route review","record");
                    foreach(var bag in shop.Targets.Values.OfType<WasteBag>()){
                        var state=jobs.Bags[bag.index];if(state.available&&state.stage==0&&state.holder==NetworkShop.Nobody)
                            view.objectives.Add(new TownObjective{id=NetworkShop.Id(bag),target=bag,title=state.source+" pickup",icon="bag"});
                    }
                }
            }
            else
            {
                bool packing=jobs.PartsRequested&&TownJobs.PartSkus.Any(s=>jobs.Parts.Count("counter",s)+jobs.Parts.Count("mechanic",s)+jobs.Parts.Count("installed",s)==0);
                if(shift.Closed&&!packing){Set("Market shift filed","Return to the town square when ready.");if(player.transform.position.x<-25)Add("Town Exit","Return to town","door");}
                else if(player.transform.position.x>-25){
                    Set(packing?"A garage parts slip is waiting":"Open Daylight Market","Open the exterior door, step through, then E to enter.");
                    var door=shop.Targets.Values.OfType<SimpleDoor>().FirstOrDefault();
                    if(door&&!door.IsOpen)Add(door.name,"Market door","door");else Add("Enter Daylight Market","Market entrance","shop");
                }
                else if(packing){Set("Pack the garage request","Move the real parts to the pickup locker.");Station(TownStation.Kind.PackParts,"Parts packing counter","parts");}
                else if(!shift.Received){Set("Receive the delivery","The rear stockroom has your sealed parcel.");Add("Delivery Desk","Delivery desk","slip");}
                else if(shift.Tasks.Tasks.Any(t=>t.id.StartsWith("stock:")&&!t.Complete)){
                    if(shop.Holder==actor){Set("Restock the shelves","E at each product marker places carried stock.");foreach(var t in shift.Tasks.Tasks.Where(t=>t.id.StartsWith("stock:")&&!t.Complete))foreach(var shelf in shop.Targets.Values.OfType<ShopStation>().Where(s=>s.kind==ShopStation.Kind.Shelf&&s.sku==t.id.Substring(6)))Add(shelf.name,ShopUIView.NameFor(shelf.sku)+" shelf","parts");}
                    else{Set("Pick up the stock crate","Carry the delivery to its product shelves.");Add("Stock Crate","Stock crate","parts");}
                }
                else if(shift.CurrentOrder!=null){
                    var missing=shift.CurrentOrder.lines.Where(l=>shift.Stock.Count("basket",l.sku)<l.quantity).ToArray();
                    Set(missing.Length>0?"Fill the customer's basket":"Ring up this basket",missing.Length>0?"The order slip is in your handbook.":"Checkout files the customer's receipt.");
                    if(missing.Length>0)foreach(var line in missing)foreach(var shelf in shop.Targets.Values.OfType<ShopStation>().Where(s=>s.kind==ShopStation.Kind.Shelf&&s.sku==line.sku))Add(shelf.name,ShopUIView.NameFor(line.sku)+" shelf","basket");
                    else Add("Checkout","Checkout","record");
                }
                else if(!shift.Closed){Set("Review and close the shop","Your receipts are ready for the closing terminal.");Add("Closing Terminal","Shop shift review","record");}
                else{Set("Market shift filed","Return to the town square when ready.");Add("Town Exit","Return to town","door");}
            }
            return view;
        }
    }
}
