using System;
using System.Linq;

namespace BusinessAsUsual.Shopkeeper
{
    // Read-only formatted presentation. Rebuilt when the authoritative state changes, never writes it.
    public sealed class ShopUIView
    {
        public sealed class Item
        {
            public string sku,name,unit,line;
            public int needed,basket,shelf,carry,dock,sold;
        }
        public Item[] items;
        public string customer,context,orderId,orderValue,revenue,activeTitle,activeDetail,message;
        public int completedTasks,receiptCount,missing,orderNumber;
        public bool closed,carried;
        public static string Money(int cents)=>"$"+(cents/100).ToString(System.Globalization.CultureInfo.InvariantCulture)+"."+(Math.Abs(cents%100)).ToString("D2",System.Globalization.CultureInfo.InvariantCulture);
        public static string Name(string customer)=>customer.Split('/')[0].Trim();
        public ShopUIView(ShopShift shift)
        {
            var order=shift.CurrentOrder;
            closed=shift.Closed;carried=shift.Carried;message=shift.Message;
            receiptCount=shift.Transactions.Receipts.Count;revenue=Money(shift.Transactions.RevenueCents);
            orderNumber=Math.Min(shift.OrderIndex+1,shift.Orders.Length);
            customer=order==null?"All served!":Name(order.customer);
            context=order==null?"Review receipts and close your shift.":order.customer.Contains("/")?order.customer.Split('/')[1].Trim():"Customer order";
            orderId=order?.id??"";
            items=ShopShift.Products.Select(p=>{
                int needed=order?.lines.Where(l=>l.sku==p.sku).Sum(l=>l.quantity)??0;
                return new Item{sku=p.sku,name=p.name,unit=Money(p.priceCents),line=Money(p.priceCents*needed),needed=needed,
                    basket=shift.Stock.Count("basket",p.sku),shelf=shift.Stock.Count("shelf",p.sku),carry=shift.Stock.Count("carry",p.sku),
                    dock=shift.Stock.Count("dock",p.sku),sold=shift.Stock.Count("sold",p.sku)};
            }).ToArray();
            orderValue=Money(order?.lines.Sum(l=>l.quantity*ShopShift.Products.First(p=>p.sku==l.sku).priceCents)??0);
            missing=items.Sum(i=>Math.Max(0,i.needed-i.basket));completedTasks=shift.Tasks.Tasks.Count(t=>t.Complete);
            if(closed){activeTitle="Shift complete!";activeDetail="Head to the front exit.";}
            else if(!shift.Received){activeTitle="Receive the delivery";activeDetail="18 units at the rear stockroom";}
            else if(shift.Tasks.Tasks.Any(t=>t.id.StartsWith("stock:")&&!t.Complete)){
                activeTitle="Restock the shelves";
                activeDetail=string.Join("   ",items.Select(i=>NameFor(i.sku)+" "+shift.Tasks.Tasks.First(t=>t.id=="stock:"+i.sku).progress+"/6"));
            }
            else if(order!=null){activeTitle="Fill "+customer+"’s basket";activeDetail=string.Join(" + ",items.Where(i=>i.needed>0).Select(i=>i.needed+" "+NameFor(i.sku)))+" • customer "+orderNumber+"/"+shift.Orders.Length;}
            else {activeTitle="Review & close shift";activeDetail="All customers served. Visit the terminal.";}
        }
        public static string NameFor(string sku)=>sku=="cola"?"Cola":sku=="tea"?"Tea":"Crackers";
    }
}
