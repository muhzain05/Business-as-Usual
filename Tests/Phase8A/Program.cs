using System;
using System.Linq;
using System.Text.Json;
using BusinessAsUsual.Town;

class Program
{
    static int checks;
    static void Check(bool condition,string label){if(!condition)throw new Exception(label);checks++;}
    static string Serialize(object value)=>JsonSerializer.Serialize(value,new JsonSerializerOptions{IncludeFields=true});
    static void Use(WorkdayDomain d,string action,int job=1,ulong id=0,double time=30,bool assistant=false)
    {Check(d.Apply(action,job,assistant,id,time,out var message),action+": "+message);Check(d.Conserved,"conservation after "+action);}
    static void Reject(WorkdayDomain d,string action,int job,ulong id=0,double time=30,bool assistant=false)
    {var before=Serialize(d.State);int events=d.Audit.Count,records=d.Records.Count;Check(!d.Apply(action,job,assistant,id,time,out _),"reject "+action);Check(before==Serialize(d.State)&&events==d.Audit.Count&&records==d.Records.Count,"rejected command does not mutate "+action);}
    static void Main()
    {
        foreach(bool verified in new[]{false,true})foreach(bool bench in new[]{false,true}) {
            var d=new WorkdayDomain();Check(d.Conserved,"initial registry");Reject(d,"receive",2);Reject(d,"install",2);
            Use(d,"receive");Reject(d,"receive",1);if(verified)Use(d,"verify");
            Use(d,"request",2,1);Reject(d,"customer",1);Use(d,"priority");Use(d,"customer");
            Use(d,"prepare");Reject(d,"prepare",1);Use(d,"pickup",1,3,31,true);
            Reject(d,"pickup",2,1);Reject(d,"deliver",2,1);Use(d,"assist",3,2,32);
            Use(d,"deliver",1,3,33,true);Use(d,"hood",2,1);Use(d,"diagnose",2,1);
            if(!verified){Reject(d,"bolt",2,1);Use(d,"check",2,1);Reject(d,"bolt",2,1);Use(d,"check",2,1);}
            Reject(d,"install",2,3,34,true);if(bench)Use(d,"bench",2,1);
            Use(d,"bolt",2,1);if(bench)Use(d,"bolt",2,1);Use(d,"install",2,1);Reject(d,"install",2,1);
            string[] actions={"square","paper","metal"};
            foreach(var action in actions){Use(d,action,3,2);var item=d.Held(2);Reject(d,item.material=="paper"?"sort-metal":"sort-paper",3,2);Use(d,"inspect",3,2);Use(d,"sort-"+item.material,3,2);Use(d,"file",3,3,35,true);}
            Use(d,"close",1);Check(d.State.items.Count(i=>i.custody=="recorded")==3,"three batches");Check(d.State.items.Count(i=>i.custody=="installed")==1,"one installed part");
            Check(d.PublicState().records.Length==0,"no records in broadcast");Check(!Serialize(d.PublicState()).Contains("actor"),"no audit actors in public state");
            Check(d.Records.All(r=>r.profession>=1&&r.profession<=3),"scoped copies");Reject(d,"cleanup",1);
        }
        var recovery=new WorkdayDomain();Use(recovery,"request",2,1,0);Reject(recovery,"reserve",2,1,24);Use(recovery,"reserve",2,1,25);Check(recovery.State.items.Length==7,"reserve reuses finite shipment");
        var carry=new WorkdayDomain();Use(carry,"receive");Use(carry,"prepare");Use(carry,"pickup",1,3,30,true);carry.Disconnect(3,31);Check(carry.State.items.All(i=>i.holder==ulong.MaxValue),"disconnect clears custody");Check(carry.Find("PART-8A-001").custody=="dispatch","disconnect parcel recovery");
        var expired=new WorkdayDomain();Use(expired,"request",2,1,1);Check(!expired.Expire(30,60),"timer has not elapsed");Check(expired.Expire(61,60)&&expired.State.closed&&expired.State.stability==0,"unfinished obligations reduce supply at closing");Check(expired.Conserved,"expired objects retained");
        var sanitation=new WorkdayDomain();Use(sanitation,"square",3,3,30,true);Reject(sanitation,"sort-paper",3,3,31,true);Use(sanitation,"stage",3,3,32,true);Use(sanitation,"yard",3,2,33);Check(sanitation.Held(2).id=="BAG-8A-001","assistant-to-lead intake preserves identity");
        var random=new Random(8);var actionsFuzz=new[]{"receive","verify","prepare","customer","priority","request","reserve","pickup","deliver","hood","diagnose","check","bench","bolt","install","paper","metal","square","order","inspect","sort-paper","sort-metal","sort-mixed","file","return","cleanup","assist","close","unknown"};
        for(int round=0;round<20;round++){var d=new WorkdayDomain();for(int i=0;i<500;i++){d.Apply(actionsFuzz[random.Next(actionsFuzz.Length)],random.Next(4),random.Next(2)==0,(ulong)random.Next(4),i,out _);Check(d.Conserved,"random sequence conservation");}}
        Console.WriteLine($"PASS {checks} domain checks (4 workflows and 10,000 seeded commands)");
    }
}
