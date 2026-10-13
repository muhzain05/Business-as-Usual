using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using BusinessAsUsual.Prototype;

namespace BusinessAsUsual.Shopkeeper
{
    public sealed class ShopShift : MonoBehaviour
    {
        public static readonly Product[] Products = { new Product("cola", "Sunbeam Cola", 180), new Product("tea", "Meadow Tea", 220), new Product("crackers", "Golden Crackers", 150) };
        public readonly InventoryLedger Stock = new InventoryLedger();
        public readonly TransactionBook Transactions = new TransactionBook();
        public readonly TaskBoard Tasks = new TaskBoard();
        public CustomerOrder[] Orders { get; private set; }
        public int OrderIndex { get; private set; }
        public CustomerOrder CurrentOrder => OrderIndex < Orders.Length ? Orders[OrderIndex] : null;
        public bool Received { get; private set; }
        public bool Closed { get; private set; }
        public StockCrate Carried { get; private set; }
        public string Message { get; private set; } = "Welcome to Daylight Market. Receive the delivery at the rear stockroom.";
        public int StabilityContribution => Closed ? 3 : 0;
        InputAction drop;
        void Awake()
        {
            foreach (var p in Products) Stock.Seed("delivery", p.sku, 6);
            Orders = new[] {
                new CustomerOrder("S1-001", "Mara / morning break", new StockLine("cola",2), new StockLine("crackers",1)),
                new CustomerOrder("S1-002", "Eli / lunch supplies", new StockLine("tea",1), new StockLine("crackers",2)),
                new CustomerOrder("S1-003", "Jo / afternoon pickup", new StockLine("cola",1), new StockLine("tea",2)) };
            Tasks.Add("receive", "Receive one sealed delivery", 1);
            foreach (var p in Products) Tasks.Add("stock:"+p.sku, "Restock 6 " + p.name, 6);
            Tasks.Add("orders", "Fulfill three customer orders", 3);
            Tasks.Add("close", "Review receipts and close shift", 1);
            drop = new InputAction("Return stock crate", InputActionType.Button, "<Keyboard>/q"); drop.Enable();
        }
        void Update()
        {
            if (!BusinessAsUsual.Multiplayer.NetworkSession.Active && Carried && drop.WasPressedThisFrame()) Carried.ReturnToDock();
        }
        void OnDestroy() { drop?.Dispose(); }
        public void Tell(PlayerInteractor player, string message) { Message = message; if (player) player.Notify(message); }
        public void RestoreNetworkState(int orderIndex, bool received, bool closed, StockCrate carried, string message)
        { OrderIndex=orderIndex; Received=received; Closed=closed; Carried=carried; Message=message; }
        public bool Receive(PlayerInteractor player)
        {
            if (Closed || Received) { Tell(player,"Delivery already received."); return false; }
            if (!Stock.TransferBundle("delivery","dock",Products.Select(p=>new StockLine(p.sku,6)),"delivery received")) return false;
            Received = true; Tasks.Set("receive",1); Tell(player,"18 units received. Pick up the sealed crate beside the delivery desk."); return true;
        }
        public bool Carry(StockCrate crate, PlayerInteractor player)
        {
            if (!Received || Carried || Closed || !player || player.HeldBox || !player.carryAnchor) return false;
            var lines = Products.Where(p=>Stock.Count("dock",p.sku)>0).Select(p=>new StockLine(p.sku,Stock.Count("dock",p.sku))).ToArray();
            if (!Stock.TransferBundle("dock","carry",lines,"crate pickup")) return false;
            Carried = crate; Tell(player,"Stock crate carried. E at each labeled shelf restocks it. Q returns the crate to the dock."); return true;
        }
        public void ReturnCrate()
        {
            var lines=Products.Where(p=>Stock.Count("carry",p.sku)>0).Select(p=>new StockLine(p.sku,Stock.Count("carry",p.sku))).ToArray();
            if(lines.Length>0) Stock.TransferBundle("carry","dock",lines,"crate returned");
            Carried=null;
        }
        public bool UseShelf(string sku, PlayerInteractor player)
        {
            if (Closed) return false;
            var product = Products.First(p=>p.sku==sku);
            int carried = Stock.Count("carry",sku);
            if (Carried)
            {
                int n = Math.Min(carried,6-Stock.Count("shelf",sku));
                if (n<=0) { Tell(player,"This crate has no more " + product.name + ". Continue to the other shelves."); return false; }
                if(!Stock.Transfer("carry","shelf",sku,n,"shelf restock")) return false;
                Tasks.Set("stock:"+sku,Tasks.Tasks.First(t=>t.id=="stock:"+sku).progress+n);
                Tell(player,product.name+" stocked: "+Stock.Count("shelf",sku)+"/6.");
                if(Products.All(p=>Stock.Count("carry",p.sku)==0)) { Carried.ReturnToDock(); Tell(player,"Restocking complete. Read the current order and collect its products with E at the shelves."); }
                return true;
            }
            if (!Products.All(p=>Tasks.Tasks.First(t=>t.id=="stock:"+p.sku).Complete)) { Tell(player,"Finish restocking all three products first."); return false; }
            var order=CurrentOrder;
            int needed=order?.lines.Where(l=>l.sku==sku).Sum(l=>l.quantity) ?? 0;
            if(Stock.Count("basket",sku)>=needed) { Tell(player,"No more " + product.name + " needed for the current order."); return false; }
            if(!Stock.Transfer("shelf","basket",sku,1,"pick:"+order.id)) { Tell(player,"Shelf is empty."); return false; }
            Tell(player,"Collected " + product.name + ". Take the completed order to checkout."); return true;
        }
        public bool Checkout(PlayerInteractor player)
        {
            if (Closed || CurrentOrder==null) { Tell(player,"All orders served. Review the shift at the closing terminal."); return false; }
            if(!Transactions.TrySell(CurrentOrder,Stock,Products,out var receipt)) { Tell(player,"Order incomplete. Collect the requested quantities from the shelves."); return false; }
            OrderIndex++; Tasks.Set("orders",OrderIndex);
            Tell(player,receipt.id+" printed: $"+(receipt.totalCents/100f).ToString("F2")+". "+(CurrentOrder==null?"All customers served. Close the shift.":"Next customer: "+CurrentOrder.customer)); return true;
        }
        public bool Close(PlayerInteractor player)
        {
            if(Closed) return false;
            if(!Tasks.Tasks.Where(t=>t.id!="close").All(t=>t.Complete) || Carried || Products.Any(p=>Stock.Count("basket",p.sku)>0 || Stock.Count("dock",p.sku)>0 || Stock.Count("carry",p.sku)>0)) { Tell(player,"Finish delivery, restocking and every customer order before closing."); return false; }
            Closed=true; Tasks.Set("close",1);
            Tell(player,"Shift complete! 3 customers, $"+(Transactions.RevenueCents/100f).ToString("F2")+" revenue, +3 Town Stability. Receipts saved.");
            try { var report=new ShiftSummary { receipts=Transactions.Receipts.ToArray(), movements=Stock.Journal.ToArray(), tasks=Tasks.Tasks.ToArray(), revenueCents=Transactions.RevenueCents, stabilityContribution=StabilityContribution, timestampUtc=DateTime.UtcNow.ToString("o") }; File.WriteAllText(Path.Combine(Application.persistentDataPath,"shopkeeper-last-shift.json"),JsonUtility.ToJson(report,true)); }
            catch(Exception e) { Message += " Export failed: "+e.Message; Debug.LogWarning(e.Message); }
            return true;
        }
        [Serializable] public class ShiftSummary { public Receipt[] receipts; public StockMovement[] movements; public JobTask[] tasks; public int revenueCents,stabilityContribution; public string timestampUtc; }
    }
}
