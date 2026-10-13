using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using BusinessAsUsual.FirstPerson;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.Town;
namespace BusinessAsUsual.RescueV2
{
    [DefaultExecutionOrder(210)]
    public sealed class RescueHands:MonoBehaviour
    {
        FirstPersonView view;NetworkPlayer player;GameObject rig;Animation animationPlayer;Transform left,right,wrench,probeLeft,probeRight;
        string current="Idle";float until,bob;Vector3 contact;bool heldLast;
        public string Pose=>current;public int Gestures {get;private set;}
        public bool Visible=>rig&&rig.activeInHierarchy;
        public void Setup(NetworkPlayer owner){
            player=owner;view=owner.GetComponent<FirstPersonView>();if(!view||!view.ViewCamera)return;
            var prefab=Resources.Load<GameObject>("RescueV2/Models/GloveRig");if(!prefab)return;
            rig=Instantiate(prefab,view.ViewCamera.transform,false);rig.name="Authored work gloves";var model=rig.transform.GetChild(0);animationPlayer=model.GetComponent<Animation>();if(!animationPlayer)animationPlayer=model.gameObject.AddComponent<Animation>();animationPlayer.enabled=true;animationPlayer.playAutomatically=false;
            foreach(string name in new[]{"Idle","Reach","Grip","Tool","Probe","Stamp","Sort"}){var clip=Resources.Load<AnimationClip>("RescueV2/Models/Gloves_"+name);if(clip){animationPlayer.AddClip(clip,name);animationPlayer[name].wrapMode=name=="Idle"||name=="Grip"?WrapMode.Loop:WrapMode.Once;}}
            left=model.GetComponentsInChildren<Transform>().First(t=>t.name=="LeftWrist");right=model.GetComponentsInChildren<Transform>().First(t=>t.name=="RightWrist");
            var parts=model.GetComponentsInChildren<Transform>();wrench=parts.FirstOrDefault(t=>t.name=="Wrench");probeLeft=parts.FirstOrDefault(t=>t.name=="ProbeLeft");probeRight=parts.FirstOrDefault(t=>t.name=="ProbeRight");
            foreach(var r in rig.GetComponentsInChildren<Renderer>())r.shadowCastingMode=ShadowCastingMode.Off;
            owner.GetComponent<FirstPersonHands>().enabled=false;Play("Idle",view.ViewCamera.transform.position);
        }
        public void Play(string name,Vector3 at){if(!animationPlayer||!animationPlayer[name])return;current=name;contact=at;animationPlayer[name].time=0;animationPlayer.CrossFade(name,.12f);until=Time.time+animationPlayer[name].length;Gestures++;}
        public void Begin(string action,Vector3 at)=>Play(action=="bolt"?"Tool":action=="check"||action=="diagnose"||action=="test-repair"||action=="inspect"||action=="verify"?"Probe":action=="file"||action=="service-ticket"||action=="close"?"Stamp":action.StartsWith("sort-")?"Sort":"Reach",at);
        void LateUpdate(){
            if(!rig||!player||!player.IsSpawned)return;var w=LivingWorkday.Instance;var held=w?w.Domain.Held(player.OwnerClientId):null;bool pushing=w&&w.World&&w.World.Cart&&w.Domain.State.cartPusher==player.OwnerClientId;bool carrying=held!=null||pushing;var cam=view.ViewCamera.transform;
            if(carrying!=heldLast){heldLast=carrying;Play(carrying?"Grip":"Reach",player.Interactor.carryAnchor.position);}
            if(!carrying&&Time.time>until&&current!="Idle")Play("Idle",cam.position);
            if(carrying&&current!="Grip")Play("Grip",player.Interactor.carryAnchor.position);
            bob+=view.PlanarSpeed*Time.deltaTime*3.2f;float walk=Mathf.Clamp01(view.PlanarSpeed/2.2f);rig.transform.localPosition=new Vector3(Mathf.Sin(bob)*.006f*walk,-Mathf.Abs(Mathf.Cos(bob))*.008f*walk,0);
            if(wrench)wrench.gameObject.SetActive(current=="Tool");if(probeLeft)probeLeft.gameObject.SetActive(current=="Probe");if(probeRight)probeRight.gameObject.SetActive(current=="Probe");
            if(pushing){var handle=w.World.Cart.Handle;left.position=handle-w.World.Cart.transform.right*.18f;right.position=handle+w.World.Cart.transform.right*.18f;}
            else if(carrying){float width=held.id.StartsWith("ORDER")?(held.shelved?.125f:.25f):held.custody=="parcel"?.25f:.18f;var anchor=player.Interactor.carryAnchor.position;left.position=anchor-cam.right*width+cam.up*.09f;right.position=anchor+cam.right*width+cam.up*.09f;}
            else if(current!="Idle"){
                float n=Mathf.Clamp01(1-(until-Time.time)/Mathf.Max(.01f,animationPlayer[current].length));float strength=Mathf.Sin(n*Mathf.PI);var target=cam.InverseTransformPoint(contact);target=new Vector3(Mathf.Clamp(target.x,-.22f,.22f),Mathf.Clamp(target.y,-.40f,.12f),Mathf.Clamp(target.z,.30f,.74f));right.position=Vector3.Lerp(right.position,cam.TransformPoint(target),strength*.65f);
            }
        }
        void OnDestroy(){if(rig)Destroy(rig);}
    }
}
