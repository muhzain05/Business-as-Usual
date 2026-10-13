using UnityEngine;
using UnityEngine.SceneManagement;
using BusinessAsUsual.Prototype;

namespace BusinessAsUsual.Shopkeeper
{
    public sealed class ShopStation : Interactable
    {
        public enum Kind { Delivery, Shelf, Checkout, Close, Exit }
        public Kind kind;
        public ShopShift shift;
        public string sku;
        public Transform interactionPoint;
        public override Vector3 InteractionPoint => interactionPoint ? interactionPoint.position : transform.position;
        public override string Prompt
        {
            get {
                switch(kind) {
                    case Kind.Delivery: return shift.Received ? "Delivery received" : "Receive 18-unit delivery";
                    case Kind.Shelf: return (shift.Carried ? "Restock " : "Collect ")+ShopShift.Products[System.Array.FindIndex(ShopShift.Products,p=>p.sku==sku)].name;
                    case Kind.Checkout: return "Fulfill order / print receipt";
                    case Kind.Close: return "Review receipts / finish shift";
                    default: return "Return to TownPrototype";
                }
            }
        }
        public override bool CanInteract(PlayerInteractor player) => !BusinessAsUsual.Town.LivingWorkday.Instance || kind==Kind.Exit;
        public override bool Interact(PlayerInteractor player)
        {
            switch(kind) {
                case Kind.Delivery: return shift.Receive(player);
                case Kind.Shelf:
                    var reach = player.GetComponent<ShopRestockReach>();
                    if (reach && reach.IsActive) return false;
                    bool restocking = shift.Carried;
                    bool used = shift.UseShelf(sku,player);
                    if (used && restocking && reach) reach.Begin(this);
                    return used;
                case Kind.Checkout: return shift.Checkout(player);
                case Kind.Close: return shift.Close(player);
                default:
                    if(!shift.Closed) { shift.Tell(player,"Finish your shift before returning to town."); return false; }
                    SceneManager.LoadScene("TownPrototype"); return true;
            }
        }
    }
}
