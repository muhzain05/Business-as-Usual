using System.Linq;
using UnityEngine;
using BusinessAsUsual.Multiplayer;
namespace BusinessAsUsual.RescueV2
{
    // Remote presentation only. The server's controller and movement are never changed by foot IK.
    [DefaultExecutionOrder(150)]public sealed class RescueFootPlant:MonoBehaviour
    {
        sealed class Leg{public Transform upper,knee,foot;public float a,b,minHeight=float.PositiveInfinity,lastZ;public bool planted;public Vector3 anchor;}
        NetworkPlayer player;Leg left,right;public int Plants {get;private set;}
        void Awake(){player=GetComponent<NetworkPlayer>();var bones=GetComponentsInChildren<Transform>();
            Leg Find(string side){Transform Bone(string name)=>bones.FirstOrDefault(t=>t.name=="mixamorig:"+side+name);var l=new Leg{upper=Bone("UpLeg"),knee=Bone("Leg"),foot=Bone("Foot")};if(!l.upper||!l.knee||!l.foot)return null;l.a=Vector3.Distance(l.upper.position,l.knee.position);l.b=Vector3.Distance(l.knee.position,l.foot.position);return l;}
            left=Find("Left");right=Find("Right");}
        void LateUpdate(){if(!player||!player.IsSpawned||player.IsOwner||left==null||right==null)return;Plant(left);Plant(right);}
        void Plant(Leg leg){
            var body=player.Motor.visual;var local=body.InverseTransformPoint(leg.foot.position);float height=leg.foot.position.y-player.transform.position.y;leg.minHeight=Mathf.Min(leg.minHeight,height);
            bool stance=player.Grounded.Value&&player.Speed.Value>.2f&&height<leg.minHeight+.035f&&local.z<leg.lastZ-.0001f;leg.lastZ=local.z;
            if(!stance){leg.planted=false;return;}if(!leg.planted){leg.anchor=leg.foot.position;leg.planted=true;Plants++;}
            if(Vector3.Distance(leg.anchor,leg.foot.position)>.18f){leg.planted=false;return;}
            Vector3 start=leg.upper.position,to=leg.anchor-start;float d=Mathf.Clamp(to.magnitude,Mathf.Abs(leg.a-leg.b)+.003f,leg.a+leg.b-.003f);Vector3 direction=to.normalized,pole=Vector3.ProjectOnPlane(body.forward,direction).normalized;
            float along=(leg.a*leg.a-leg.b*leg.b+d*d)/(2*d);var knee=start+direction*along+pole*Mathf.Sqrt(Mathf.Max(0,leg.a*leg.a-along*along));var footRotation=leg.foot.rotation;
            leg.upper.rotation=Quaternion.FromToRotation(leg.knee.position-start,knee-start)*leg.upper.rotation;leg.knee.rotation=Quaternion.FromToRotation(leg.foot.position-leg.knee.position,start+direction*d-leg.knee.position)*leg.knee.rotation;leg.foot.rotation=footRotation;
        }
    }
}
