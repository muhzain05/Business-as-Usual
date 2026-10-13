using System;using System.Linq;using System.Text.Json;using BusinessAsUsual.Town;
class Program {
 static int checks;
 static void Check(bool b,string label){checks++;if(!b)throw new Exception(label);}
 static string State(WorkdayDomain d)=>JsonSerializer.Serialize(d.PublicState(),new JsonSerializerOptions{IncludeFields=true});
 static void Use(WorkdayDomain d,string action,int job,double time){Check(d.Apply(action,job,false,7,time,out var message),action+": "+message);Check(d.Conserved,"Conserved after "+action);}
 static void Reject(WorkdayDomain d,string action,int job,double time){var before=State(d);int audit=d.Audit.Count,records=d.Records.Count;Check(!d.Apply(action,job,false,7,time,out _),"Reject "+action);Check(before==State(d)&&audit==d.Audit.Count&&records==d.Records.Count,"Atomic rejection "+action);}
 static void Main(){
  foreach(int role in new[]{1,2,3})foreach(bool salvage in new[]{false,true}){
   var d=new WorkdayDomain(true);bool started=false;double time=1;
   for(int tick=0;tick<180&&!d.State.closed;tick++,time+=3){var s=d.State;var held=d.Held(7);string action=null;
    if(role==1){if(!s.received)action="receive";else if(!s.verified&&!s.prepared)action="verify";else if(!s.prepared)action="prepare";else if(held?.custody=="parcel")action="deliver";else if(d.Find("PART-8A-001").custody=="dispatch")action="pickup";else if(s.customerOrders<1){var stock=s.items[4];if(held?.custody=="stock-carry")action=held.shelved?"place-order":"stock-0";else action=stock.custody=="checkout"?"scan-order":"take-stock-0";}}
    if(role==2){if(!s.hood&&!s.repairTested)action="hood";else if(!s.diagnosed)action="diagnose";else if(!s.dispatched){}else if(!s.loadChecked&&!s.verified)action="check";else if(salvage&&!s.bench&&!s.released)action="bench";else if(!s.released)action="bolt";else if(!s.removed)action="remove-battery";else if(!s.oldPlaced)action="scrap-battery";else if(!s.installed)action=held?.custody=="component-carry"?"install":"grab-battery";else if(!s.repairTested)action="test-repair";else if(s.hood)action="hood";else if(!s.serviceSigned)action="service-ticket";}
    if(role==3){if(held!=null)action=held.inspected?"sort-"+held.material:"inspect";else if(s.items.Any(i=>i.custody=="sorted"))action="file";else if(d.Find("BAG-8A-001").custody=="square")action="square";else if(d.Find("PACK-8A-001").custody=="garage-waste")action="paper";else if(d.Find("OLD-8A-001").custody=="garage-waste")action="metal";}
    if(action!=null){Use(d,action,role,time);started=true;}
    d.AdvanceMunicipal(role!=1,role!=2,role!=3,time);Check(d.Conserved,"Municipal services conserve stock");
    if(s.serviceSigned&&s.customerOrders>=1&&s.items.Count(i=>i.custody=="recorded")==3)Use(d,"close",role,time);
   }
   Check(started&&d.State.closed&&d.State.serviceSigned,"A real solo profession can finish "+role);
   Check(d.Audit.Any(e=>e.actor==WorkdayDomain.MunicipalActor),"Explicit unattended authorship");Check(d.Records.Any(r=>r.heading.StartsWith("Town service / ")),"Service receipts labelled");
   Check(d.PublicState().records.Length==0&&!State(d).Contains("actor"),"No audit or profession receipts in shared state");
  }
  var x=new WorkdayDomain(true);Use(x,"hood",2,1);Use(x,"diagnose",2,2);var before=State(x);x.AdvanceMunicipal(false,false,false,30);Check(State(x)==before,"Occupied jobs cannot be automated");Reject(x,"install",2,31);Reject(x,"service-ticket",2,31);
  x.AdvanceMunicipal(true,false,true,33);Use(x,"check",2,34);Use(x,"bolt",2,35);Use(x,"remove-battery",2,36);x.Disconnect(7,37);Check(x.Held(7)==null&&x.State.oldPlaced&&x.Find("OLD-8A-001").custody=="garage-waste","Disconnect returns removed battery without stranding repair");Use(x,"grab-battery",2,38);x.Disconnect(7,39);Check(x.Find("PART-8A-001").custody=="garage"&&x.Conserved,"Disconnect returns replacement to supply bench");
  var retail=new WorkdayDomain(true);Use(retail,"receive",1,1);Use(retail,"take-stock-1",1,2);Reject(retail,"stock-0",1,3);Use(retail,"stock-1",1,4);Use(retail,"take-stock-1",1,5);Reject(retail,"place-order",1,6);Use(retail,"return",1,7);Use(retail,"take-stock-0",1,8);Use(retail,"stock-0",1,9);Use(retail,"take-stock-0",1,10);Use(retail,"place-order",1,11);Use(retail,"scan-order",1,12);Reject(retail,"scan-order",1,13);Check(retail.State.customerOrders==1&&retail.Conserved,"One scan sells one conserved product, wrong SKU retained");
  var cart=new WorkdayDomain(true);Use(cart,"square",3,1);Use(cart,"cart",3,2);Check(cart.Find("BAG-8A-001").custody=="cart"&&cart.Held(7)==null,"Loading frees hands without duplicating batches");Use(cart,"cart",3,3);Check(!cart.Apply("cart",3,false,8,4,out _),"Another worker cannot steal a pushed cart");cart.Disconnect(7,5);Check(cart.State.cartPusher==WorkdayDomain.Nobody&&cart.Find("BAG-8A-001").custody=="cart","Disconnect parks cart and retains contents");Use(cart,"cart-unload-3",3,6);Use(cart,"return",3,7);Check(cart.Find("BAG-8A-001").custody=="cart"&&cart.Conserved,"Returning a selected batch restores the same cart slot");Use(cart,"cart-unload-3",3,8);Reject(cart,"sort-metal",3,9);Use(cart,"inspect",3,10);Use(cart,"sort-paper",3,11);Use(cart,"file",3,12);Check(cart.Conserved&&cart.Find("BAG-8A-001").custody=="recorded","Cart collection uses the ordinary sorting and manifest path");
  Console.WriteLine(checks+" physical workday assertions passed");
 }
}

