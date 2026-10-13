using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessAsUsual.Shopkeeper
{
    [Serializable] public class StockLine
    {
        public string sku;
        public int quantity;
        public StockLine(string sku, int quantity) { this.sku = sku; this.quantity = quantity; }
    }
    [Serializable] public class Product
    {
        public string sku, name;
        public int priceCents;
        public Product(string sku, string name, int cents) { this.sku = sku; this.name = name; priceCents = cents; }
    }
    [Serializable] public class StockMovement
    {
        public int sequence, quantity;
        public string sku, from, to, reason;
    }
    // Integer quantities, atomic bundles, and an append-only journal keep job logic auditable.
    public sealed class InventoryLedger
    {
        readonly Dictionary<string, Dictionary<string, int>> accounts = new Dictionary<string, Dictionary<string, int>>();
        readonly List<StockMovement> journal = new List<StockMovement>();
        public IReadOnlyList<StockMovement> Journal => journal.AsReadOnly();
        public void RestoreNetworkState(StockMovement[] movements, int[] quantities)
        {
            RestoreNetworkAccounts(new[]{"delivery","dock","carry","shelf","basket","sold"},
                ShopShift.Products.Select(p=>p.sku).ToArray(),quantities,movements);
        }
        public void RestoreNetworkAccounts(string[] accountIds, string[] skus, int[] quantities, StockMovement[] movements)
        {
            if(accountIds==null||skus==null||quantities==null||quantities.Length!=accountIds.Length*skus.Length)
                throw new ArgumentException("Invalid network inventory shape");
            accounts.Clear(); journal.Clear();
            int i=0; foreach(var id in accountIds) foreach(var sku in skus) Seed(id,sku,quantities[i++]);
            journal.AddRange(movements);
        }
        Dictionary<string, int> Account(string id)
        {
            if (!accounts.TryGetValue(id, out var a)) accounts.Add(id, a = new Dictionary<string, int>());
            return a;
        }
        public int Count(string account, string sku) => Account(account).TryGetValue(sku, out var n) ? n : 0;
        public int Total(string sku) => accounts.Values.Sum(a => a.TryGetValue(sku, out var n) ? n : 0);
        public void Seed(string account, string sku, int quantity)
        {
            if (quantity < 0 || string.IsNullOrEmpty(sku)) throw new ArgumentException("Invalid initial stock");
            Account(account)[sku] = checked(Count(account, sku) + quantity);
        }
        public bool Transfer(string from, string to, string sku, int quantity, string reason)
            => TransferBundle(from, to, new[] { new StockLine(sku, quantity) }, reason);
        public bool TransferBundle(string from, string to, IEnumerable<StockLine> lines, string reason)
        {
            if (from == to || string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || lines == null) return false;
            var requested = lines.ToArray();
            if (requested.Length == 0 || requested.Any(l => l == null || string.IsNullOrEmpty(l.sku) || l.quantity <= 0)) return false;
            var merged = requested.GroupBy(l => l.sku).Select(g => new StockLine(g.Key, checked(g.Sum(l => l.quantity)))).ToArray();
            if (merged.Any(l => Count(from, l.sku) < l.quantity || (long)Count(to, l.sku) + l.quantity > int.MaxValue)) return false;
            foreach (var l in merged)
            {
                Account(from)[l.sku] -= l.quantity;
                Account(to)[l.sku] = Count(to, l.sku) + l.quantity;
                journal.Add(new StockMovement { sequence = journal.Count + 1, sku = l.sku, quantity = l.quantity, from = from, to = to, reason = reason });
            }
            return true;
        }
    }
    [Serializable] public class CustomerOrder
    {
        public string id, customer;
        public StockLine[] lines;
        public CustomerOrder(string id, string customer, params StockLine[] lines) { this.id = id; this.customer = customer; this.lines = lines; }
    }
    [Serializable] public class ReceiptLine
    {
        public string sku, name;
        public int quantity, unitPriceCents, totalCents;
    }
    [Serializable] public class Receipt
    {
        public string id, orderId, customer, timestampUtc;
        public int totalCents;
        public ReceiptLine[] lines;
    }
    [Serializable] public class JobTask
    {
        public string id, description;
        public int target, progress;
        public bool Complete => progress >= target;
    }
    public sealed class TaskBoard
    {
        public readonly List<JobTask> Tasks = new List<JobTask>();
        public void Add(string id, string description, int target) => Tasks.Add(new JobTask { id = id, description = description, target = target });
        public void Set(string id, int progress) { var task = Tasks.First(t => t.id == id); task.progress = Math.Max(0, Math.Min(task.target, progress)); }
        public bool Complete => Tasks.All(t => t.Complete);
    }
    public sealed class TransactionBook
    {
        readonly List<Receipt> receipts = new List<Receipt>();
        public IReadOnlyList<Receipt> Receipts => receipts.AsReadOnly();
        public int RevenueCents => receipts.Sum(r => r.totalCents);
        public void RestoreNetworkState(Receipt[] records) { receipts.Clear(); receipts.AddRange(records); }
        public bool TrySell(CustomerOrder order, InventoryLedger stock, IReadOnlyList<Product> products, out Receipt receipt)
        {
            receipt = null;
            if (order == null || string.IsNullOrEmpty(order.id) || order.lines == null || order.lines.Length==0 || stock==null || products==null || receipts.Any(r => r.orderId == order.id)) return false;
            if (order.lines.Any(l => l==null || l.quantity<=0 || !products.Any(p => p.sku == l.sku && p.priceCents>=0))) return false;
            var lines = order.lines.GroupBy(l => l.sku).Select(g => {
                var p = products.First(x => x.sku == g.Key); int n = g.Sum(l => l.quantity);
                return new ReceiptLine { sku = p.sku, name = p.name, quantity = n, unitPriceCents = p.priceCents, totalCents = checked(n * p.priceCents) };
            }).ToArray();
            int total = checked(lines.Sum(l => l.totalCents));
            if (!stock.TransferBundle("basket", "sold", order.lines, "sale:" + order.id)) return false;
            receipt = new Receipt { id = "BAU-" + (receipts.Count + 1).ToString("D4"), orderId = order.id, customer = order.customer, timestampUtc = DateTime.UtcNow.ToString("o"), lines = lines, totalCents = total };
            receipts.Add(receipt);
            return true;
        }
    }
}
