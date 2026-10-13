using UnityEngine;
using BusinessAsUsual.Prototype;

namespace BusinessAsUsual.Shopkeeper
{
    public sealed class StockCrate : Interactable
    {
        public ShopShift shift;
        Transform dockParent;
        Vector3 dockPosition;
        Quaternion dockRotation;
        Collider[] colliders;
        bool[] enabledStates;
        bool held;
        public void ApplyNetworkPose(Transform anchor, Vector3 position, Quaternion rotation)
        {
            if (anchor && !held) for(int i=0;i<colliders.Length;i++) enabledStates[i]=colliders[i].enabled;
            transform.SetParent(anchor ? anchor : dockParent, true);
            if(anchor) { transform.localPosition=Vector3.zero; transform.localRotation=Quaternion.identity; }
            else transform.SetPositionAndRotation(position,rotation);
            for(int i=0;i<colliders.Length;i++) colliders[i].enabled=anchor ? false : enabledStates[i];
            held=anchor;
        }
        public override string Prompt => "Carry stock crate (Q to return)";
        void Awake() { dockParent=transform.parent; dockPosition=transform.position; dockRotation=transform.rotation; colliders=GetComponentsInChildren<Collider>(); enabledStates=new bool[colliders.Length]; for(int i=0;i<colliders.Length;i++)enabledStates[i]=colliders[i].enabled; }
        public override bool CanInteract(PlayerInteractor player) => !BusinessAsUsual.Town.LivingWorkday.Instance && !held && shift.Received && !shift.Closed && ShopShift.Products.AnyInDock(shift);
        public override bool Interact(PlayerInteractor player)
        {
            if(!shift.Carry(this,player)) return false;
            for(int i=0;i<colliders.Length;i++) { enabledStates[i]=colliders[i].enabled; colliders[i].enabled=false; }
            transform.SetParent(player.carryAnchor,false); transform.localPosition=Vector3.zero; transform.localRotation=Quaternion.identity; held=true; return true;
        }
        public void ReturnToDock()
        {
            if(!held) return;
            shift.ReturnCrate(); transform.SetParent(dockParent,true); transform.SetPositionAndRotation(dockPosition,dockRotation);
            for(int i=0;i<colliders.Length;i++) colliders[i].enabled=enabledStates[i]; held=false;
        }
        void OnDisable() { if(held) ReturnToDock(); }
    }
    static class StockProductExtensions
    {
        public static bool AnyInDock(this Product[] products,ShopShift shift) { foreach(var p in products) if(shift.Stock.Count("dock",p.sku)>0) return true; return false; }
    }
}
