using System;
using System.Linq;
using UnityEngine;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.Town;
using BusinessAsUsual.RescueV2;
namespace BusinessAsUsual.Phase8B {
    public sealed class SuspiciousDayService:MonoBehaviour {
        public SuspiciousDayDomain Domain {get;private set;}=new SuspiciousDayDomain();
        public DayPublicState Public {get;private set;}=new DayPublicState();
        public PrivateDayCopy Local=new PrivateDayCopy();NetworkShop shop;
        public void Initialize(NetworkShop adapter){shop=adapter;Public=Domain.Public;CreateStations();}
        public bool LobbyCommand(NetworkPlayer player,string command,out string reason){reason="Unknown staff request.";if(!shop.IsServer)return false;var job=(int)shop.Town.Job(player.OwnerClientId);
            if(command=="day:ready"){if(job==0){reason="Choose a job first.";return false;}Domain.Register(player.OwnerClientId,job,NetworkSession.Instance.IdentityFor(player.OwnerClientId));return Domain.Ready(player.OwnerClientId,out reason);}
            if(command=="day:start"){
                if(player.OwnerClientId!=shop.NetworkManager.LocalClientId){reason="The host opens the day.";return false;}
                int seed=Guid.NewGuid().GetHashCode();var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--bau-seed");if(index>=0&&index+1<args.Length&&int.TryParse(args[index+1],out var chosen))seed=chosen;
                bool ok=Domain.Start(NetworkSession.Instance.Solo,seed,shop.Workday.Now,player.OwnerClientId,shop.NetworkManager.ConnectedClientsIds.ToArray(),out reason);if(ok)shop.Workday.Domain.State.began=shop.Workday.Now;return ok;
            }
            if(command.StartsWith("day:share:")&&int.TryParse(command.Substring(10),out int recordIndex)){
                var records=shop.Workday.RecordsFor(player.OwnerClientId);if(recordIndex<0||recordIndex>=records.Length){reason="Choose one of your paper copies.";return false;}var r=records[recordIndex];
                bool ok=Domain.Share(new SharedPaper{heading=r.heading,text=r.text,reference=r.reference,interval=r.interval,attribution=shop.Town.Job(player.OwnerClientId)+" / Sunny "+player.OwnerClientId});reason=ok?"Your copy is pinned to the staff board.":"The board is full.";return ok;
            }return false;
        }
        public void Assigned(ulong actor){Domain.Register(actor,(int)shop.Town.Job(actor),NetworkSession.Instance.IdentityFor(actor));}
        public void Accepted(ulong actor,string action,bool sealedParcel){Domain.Accepted(actor,action,sealedParcel);}
        public PrivateDayCopy CopyFor(ulong actor)=>Domain.CopyFor(actor);
        public void Restore(DayPublicState state){Public=state;}
        public void Tick(){if(shop.IsServer){Domain.Tick(shop.Workday.Now,shop.Workday.Domain.State.durationSeconds,shop.Workday.Domain.State.closed);Public=Domain.Public;}}
        public bool CanStart=>Public.started;
        void CreateStations(){var w=shop.Workday;var parent=w.Stations["Shift report"].transform.parent;
            w.Make("Returns counter",new Vector3(-2.7f,.8f,6.2f),"hold-return","return-note",0,parent,"Restage / collect parcel","Write transfer note");
            w.Make("Yard holding",new Vector3(11.8f,.8f,-6.3f),"hold-yard","return-note",0,parent,"Restage / collect parcel","Write transfer note");
            w.Make("Service amendment",new Vector3(7.9f,.8f,7.15f),"amend-service",null,2,parent,"Correct service note",null);
            var service=w.Stations["Service amendment"];service.transform.position=new Vector3(7.9f,.8f,7.15f);
            AddCabinet(new Vector3(-2.7f,.025f,6.55f),0,"RETURNS / CHECKS");AddCabinet(new Vector3(11.8f,.025f,-6.65f),180,"YARD / TEMPORARY HOLD");
            foreach(var t in parent.GetComponentsInChildren<Transform>())if(t.name=="Town report desk"||t.name=="Town report stand"||t.name=="Town report clipboard")t.gameObject.SetActive(false);
            var report=w.Stations["Shift report"];report.alternative="board";report.altLabel="Compare shared copies";
            var kiosk=Resources.Load<GameObject>("Phase8B/StaffKiosk");if(kiosk){var g=Instantiate(kiosk,parent);g.name="Daylight staff kiosk";g.transform.position=new Vector3(0,.025f,5.4f);w.World.BindWorkPoint("Shift report",g.transform);}
            w.Make("Staff board",new Vector3(0,1.19f,5.25f),"board",null,0,parent,"Compare shared copies",null);
            Label(parent,new Vector3(0,1.19f,5.25f),0,"STAFF / SHARED COPIES",.69f);
            Label(parent,new Vector3(7.9f,.93f,7.02f),0,"SERVICE NOTES",.46f);
        }
        void AddCabinet(Vector3 at,float yaw,string label){var prefab=Resources.Load<GameObject>("Phase8B/ReturnsCabinet");if(!prefab)return;var parent=shop.Workday.Stations["Shift report"].transform.parent;var g=Instantiate(prefab,parent);g.name=label;g.transform.SetPositionAndRotation(at,Quaternion.Euler(0,yaw,0));shop.Workday.World.BindWorkPoint(yaw==0?"Returns counter":"Yard holding",g.transform);Label(parent,at+Quaternion.Euler(0,yaw,0)*new Vector3(0,.83f,-.265f),yaw,label,.78f);}
        void Label(Transform parent,Vector3 at,float yaw,string text,float width){var g=new GameObject(text+" sign");g.transform.SetParent(parent);g.transform.SetPositionAndRotation(at,Quaternion.Euler(0,yaw,0));var t=g.AddComponent<TMPro.TextMeshPro>();t.font=Resources.Load<RescueSkin>("RescueV2/WorkdaySkin").labels;t.text=text;t.fontSize=1.5f;t.enableAutoSizing=true;t.fontSizeMin=.5f;t.fontSizeMax=1.5f;t.color=new Color(.95f,.89f,.72f);t.alignment=TMPro.TextAlignmentOptions.Center;t.rectTransform.sizeDelta=new Vector2(width,.13f);}
        // Ordinary public progress is shared; unseen moving-item custody is not.
        public WorkdayState ObservedState(ulong observer){var state=JsonUtility.FromJson<WorkdayState>(JsonUtility.ToJson(shop.Workday.Capture()));var player=FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsSpawned&&p.OwnerClientId==observer);if(!player)return state;
            foreach(var item in state.items){if(item.holder==observer||item.custody=="installed"||item.custody=="recorded"||item.custody=="customer")continue;
                // The battery mesh is hidden inside a sealed carton and has no independently observable location.
                bool sealedPart=item.id=="PART-8A-001"&&(item.custody=="shipment"||item.custody=="market"||item.custody=="dispatch"||item.custody=="parcel"||item.custody=="garage"||item.custody=="returns"||item.custody=="yard-hold");
                var visual=shop.Workday.World.ItemTransform(sealedPart?"PACK-8A-001":item.id);if(!visual){item.custody="unobserved";item.holder=WorkdayDomain.Nobody;item.inspected=false;item.fromCart=false;item.revision=0;continue;}Vector3 at=visual.position+Vector3.up*.15f;bool sameRoom=(at.x<-25)==(player.transform.position.x<-25),near=Vector3.Distance(player.Interactor.Origin,at)<10;
                bool facing=Vector3.Distance(player.Interactor.Origin,at)<3||Vector3.Dot(Quaternion.Euler(0,player.LookYaw.Value,0)*Vector3.forward,(at-player.Interactor.Origin).normalized)>.15f;
                bool seen=sameRoom&&near&&facing&&(!Physics.Linecast(player.Interactor.Origin,at,out var hit,player.Interactor.interactionMask,UnityEngine.QueryTriggerInteraction.Ignore)||hit.collider.transform.IsChildOf(visual));
                if(!seen){item.custody="unobserved";item.holder=WorkdayDomain.Nobody;item.inspected=false;item.fromCart=false;item.revision=0;}
            }return state;
        }
    }
}
