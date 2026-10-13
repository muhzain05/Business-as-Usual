using System.Linq;
using UnityEngine;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.Town;
namespace BusinessAsUsual.RescueV2
{
    // Cart movement follows the server-controlled worker. Inventory changes still use normal commands.
    public sealed class RescueCart:MonoBehaviour
    {
        LivingWorkday work;NetworkShop shop;Transform model;AnimationClip rolling;float distance,sendAt;Vector3 last;
        public Vector3 Handle=>transform.TransformPoint(new Vector3(0,.84f,-.45f));
        public void Setup(LivingWorkday w,NetworkShop adapter){
            work=w;shop=adapter;model=transform.GetChild(0);foreach(var a in GetComponentsInChildren<Animation>())a.enabled=false;rolling=Resources.Load<AnimationClip>("RescueV2/Models/CartRoll");
            var body=gameObject.AddComponent<BoxCollider>();body.center=new Vector3(0,.46f,0);body.size=new Vector3(.54f,.57f,.75f);gameObject.layer=2;
            w.Make("Collection cart",Handle,"cart","cart-unload",3,w.Stations["Manifest"].transform.parent,"Push cart","Lift batch");
            foreach(int index in new[]{1,2,3})w.Make("Cart batch "+index,transform.position,"cart-unload-"+index,null,3,w.Stations["Manifest"].transform.parent,"Lift this batch",null);
            ApplyPose();last=transform.position;
        }
        public Vector3 Slot(int index)=>transform.TransformPoint(index==1?new Vector3(0,.31f,.19f):index==2?new Vector3(.12f,.31f,-.16f):new Vector3(-.12f,.31f,-.15f));
        public void StepServer(){
            var s=work.Domain.State;if(s.cartPusher==WorkdayDomain.Nobody)return;
            var p=FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).FirstOrDefault(n=>n.IsSpawned&&n.OwnerClientId==s.cartPusher);if(!p)return;
            var rotation=Quaternion.Euler(0,p.LookYaw.Value,0);var goal=p.transform.position+rotation*Vector3.forward*1.05f;goal.y=.025f;
            Vector3 at=new Vector3(s.cartX,.025f,s.cartZ),delta=Vector3.ClampMagnitude(goal-at,2.6f*Time.deltaTime);
            if(delta.magnitude>.001f&&!Physics.BoxCast(at+Vector3.up*.46f,new Vector3(.25f,.27f,.34f),delta.normalized,out _,rotation,delta.magnitude+.02f,~(1<<2),QueryTriggerInteraction.Ignore)){
                s.cartX+=delta.x;s.cartZ+=delta.z;s.cartYaw=Mathf.MoveTowardsAngle(s.cartYaw,p.LookYaw.Value,150*Time.deltaTime);
            }
            if(Time.unscaledTime>=sendAt){sendAt=Time.unscaledTime+.10f;shop.CartPoseClientRpc(s.cartX,s.cartZ,s.cartYaw,++shop.CartPoseSequence);}
        }
        public void ApplyPose(){var s=work.Domain.State;transform.SetPositionAndRotation(new Vector3(s.cartX,.025f,s.cartZ),Quaternion.Euler(0,s.cartYaw,0));
            distance+=Vector3.Distance(last,transform.position);last=transform.position;if(rolling)rolling.SampleAnimation(model.gameObject,Mathf.Repeat(distance/(Mathf.PI*.19f),1)*rolling.length);
            work.Stations["Collection cart"].transform.position=Handle;
            foreach(int index in new[]{1,2,3})work.Stations["Cart batch "+index].transform.position=Slot(index)+Vector3.up*.17f;
        }
    }
}
