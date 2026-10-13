using UnityEngine;
using BusinessAsUsual.Prototype;
using BusinessAsUsual.Multiplayer;

namespace BusinessAsUsual.Town
{
    public sealed class TownStation : Interactable
    {
        public enum Kind { ServiceDesk, Inspect, Repair, RequestParts, PackParts, CollectParts, RouteDesk, Sort, Portal }
        public Kind kind;
        public int index;
        public string materialType;
        public string label;
        public Transform point;
        public Vector3 destination;
        public override Vector3 InteractionPoint => point?point.position:transform.position+Vector3.up*.65f;
        public override string Prompt => label;
        public override bool CanInteract(PlayerInteractor player)
        {
            if(kind==Kind.Portal)return true;
            if(LivingWorkday.Instance)return false;
            var owner=player.GetComponent<NetworkPlayer>();var jobs=TownJobs.Instance;
            if(!owner||!jobs)return false;
            var required=kind==Kind.PackParts?Profession.Shopkeeper:
                kind==Kind.RouteDesk||kind==Kind.Sort?Profession.Sanitation:Profession.Mechanic;
            return jobs.Job(owner.OwnerClientId)==required;
        }
        public override bool Interact(PlayerInteractor player) => false; // NetworkShop is the authority boundary.
    }
    public sealed class WasteBag : Interactable
    {
        public int index;
        public override Vector3 InteractionPoint => transform.position+Vector3.up*.15f;
        public override string Prompt => "Collect route bag / "+TownJobs.Instance.Bags[index].materialType;
        public override bool CanInteract(PlayerInteractor player)
        {
            if(LivingWorkday.Instance)return false;
            var jobs=TownJobs.Instance;var owner=player.GetComponent<NetworkPlayer>();
            if(!jobs||!owner||jobs.Job(owner.OwnerClientId)!=Profession.Sanitation||
                !jobs.Started[2]||jobs.Completed[2]||jobs.HasBag(owner.OwnerClientId))return false;
            var bag=jobs.Bags[index];
            return bag.available&&bag.stage==0&&bag.holder==BusinessAsUsual.Multiplayer.NetworkShop.Nobody;
        }
        public override bool Interact(PlayerInteractor player) => false;
    }
}
