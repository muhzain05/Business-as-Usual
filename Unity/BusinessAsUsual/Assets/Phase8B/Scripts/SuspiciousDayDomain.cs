using System;
using System.Linq;
using System.Collections.Generic;
namespace BusinessAsUsual.Phase8B
{
    [Serializable] public sealed class SharedPaper {public string heading,text,reference,interval,attribution;}
    [Serializable] public sealed class DayPublicState {
        public bool started,closed,solo,unattended=true;public double startedAt;public int band;
        public ulong[] ready=Array.Empty<ulong>();public SharedPaper[] papers=Array.Empty<SharedPaper>();
        public string bulletin="Choose a workplace. Unstaffed jobs use labelled town services.";
    }
    [Serializable] public sealed class PrivateDayCopy {
        public string personalTitle,personalNote,arrangementTitle,arrangementNote;public bool practice,hasArrangement,detour,declaredDirect,productive,personalDone;
        public int Progress=>(detour?1:0)+(declaredDirect?1:0)+(productive?1:0);
        public bool Complete=>hasArrangement&&Progress==3;
    }
    // The only serializable public object is Public. Assignments stay in a server dictionary.
    public sealed class SuspiciousDayDomain {
        public readonly DayPublicState Public=new DayPublicState();
        readonly Dictionary<ulong,PrivateDayCopy> copies=new Dictionary<ulong,PrivateDayCopy>();
        readonly Dictionary<ulong,int> professions=new Dictionary<ulong,int>();
        readonly Dictionary<string,ulong> identities=new Dictionary<string,ulong>();
        ulong arrangementOwner=ulong.MaxValue;
        public void Register(ulong actor,int profession,string identity=""){
            if(!string.IsNullOrEmpty(identity)&&identities.TryGetValue(identity,out var prior)&&prior!=actor&&copies.TryGetValue(prior,out var preserved)){
                copies[actor]=preserved;professions[actor]=profession;if(arrangementOwner==prior)arrangementOwner=actor;copies.Remove(prior);professions.Remove(prior);
            }
            if(!string.IsNullOrEmpty(identity))identities[identity]=actor;
            if(!copies.ContainsKey(actor)||!Public.started&&professions[actor]!=profession){
                Public.ready=Public.ready.Where(id=>id!=actor).ToArray();
                string[] titles={"","A promised favor","Correct the rushed note","A reusable carton"};
                string[] notes={"","Make room at dispatch. Restage a parcel at either ordinary hold.","After a load test, amend the service note before signing.","Restage the empty packaging at returns, then file its material ticket."};
                copies[actor]=new PrivateDayCopy{personalTitle=titles[profession],personalNote=notes[profession]};
            }professions[actor]=profession;
        }
        public bool Ready(ulong actor,out string reason){reason="Choose a profession first.";if(Public.started||!copies.ContainsKey(actor))return false;Public.ready=Public.ready.Concat(new[]{actor}).Distinct().ToArray();reason="Ready at the staff hub.";return true;}
        public bool Start(bool solo,int seed,double now,ulong host,ulong[] connected,out string reason){
            reason="Every connected worker must choose a job and be ready.";if(Public.started||connected.Length==0||connected.Any(id=>!Public.ready.Contains(id)||!copies.ContainsKey(id)))return false;
            Public.started=true;Public.solo=solo;Public.startedAt=now;Public.band=1;
            var eligible=connected.OrderBy(id=>id).ToArray();arrangementOwner=solo?host:eligible[new Random(seed).Next(eligible.Length)];
            var copy=copies[arrangementOwner];copy.hasArrangement=true;copy.practice=solo;
            copy.arrangementTitle=solo?"Practice a quiet detour":"Keep a detour off the slip";
            copy.arrangementNote="Restage the garage parcel at RETURNS or YARD HOLD. Declare direct transfer on its slip. Complete useful work too.";
            Public.bulletin="Opening work · customers, a repair request and collection share the same supplies.";reason="The workday is open.";return true;
        }
        public void Disconnect(ulong actor){Public.ready=Public.ready.Where(id=>id!=actor).ToArray();}
        public PrivateDayCopy CopyFor(ulong actor)=>copies.TryGetValue(actor,out var c)?c:new PrivateDayCopy();
        public void Accepted(ulong actor,string action,bool sealedParcel){
            if(!Public.started||Public.closed||!copies.TryGetValue(actor,out var c))return;
            if(action=="hold-return"||action=="hold-yard"){if(sealedParcel&&actor==arrangementOwner)c.detour=true;if(professions[actor]==1||professions[actor]==3&&!sealedParcel)c.personalDone=true;}
            if(action=="note-direct"&&c.detour&&actor==arrangementOwner)c.declaredDirect=true;
            if(action=="amend-service"&&professions[actor]==2)c.personalDone=true;
            if(action=="scan-order"||action=="service-ticket"||action=="file"||action=="check"||action=="assist")c.productive=true;
        }
        public bool Share(SharedPaper paper){if(!Public.started||paper==null||Public.papers.Length>=12)return false;Public.papers=Public.papers.Concat(new[]{paper}).ToArray();return true;}
        public void Tick(double now,double duration,bool closed){
            if(!Public.started)return;Public.closed=closed;Public.band=closed?4:now-Public.startedAt>duration*.72?3:now-Public.startedAt>duration*.35?2:1;
            Public.bulletin=closed?"Shift filed · compare the paper copies and your accounts.":Public.band==3?"Last collections · finish the repair, serve an order and file the batches.":Public.band==2?"Midday rush · a late delivery can be a check, a favor or a detour.":"Opening work · serve, repair and collect.";
        }
    }
}
