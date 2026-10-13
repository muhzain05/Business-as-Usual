using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using BusinessAsUsual.Prototype;
namespace BusinessAsUsual.Shopkeeper
{
    // Presentation only. All transactions remain in the approved domain.
    [DefaultExecutionOrder(-110)]
    public sealed class ShopHUD : MonoBehaviour
    {
        public ShopShift shift;
        public PlayerInteractor player;
        public bool reducedMotion;
        public enum Panel { None, Receipts, Tasks, Inventory, Order, Report }
        public Panel CurrentPanel { get; private set; }
        public int SelectedReceipt { get; private set; }
        public ShopUIView View { get { RefreshView();return view; } }
        public bool FontsAvailable => theme!=null && theme.FontsAvailable;
        ShopUIView view;
        ShopUITheme theme;
        ShopRestockReach reach;
        PrototypeInput input;
        InputAction receiptKey;
        int signature=int.MinValue;
        bool sawClosed,blockedCapture;
        bool LocalCarried => BusinessAsUsual.Multiplayer.NetworkSession.Active
            ? BusinessAsUsual.Multiplayer.NetworkShop.Instance.Holder==GetComponent<BusinessAsUsual.Multiplayer.NetworkPlayer>().OwnerClientId : view.carried;
        string LocalMessage => BusinessAsUsual.Multiplayer.NetworkSession.Active&&!string.IsNullOrEmpty(player.Status)?player.Status:shift.Message;
        float openedAt,statusAt;
        string lastMessage;
        Vector2 receiptScroll;
        void InitializePresentation()
        {
            if(!reach)reach=GetComponent<ShopRestockReach>();
            if(!input)input=GetComponent<PrototypeInput>();
            if(receiptKey==null)receiptKey=new InputAction("Receipt book",InputActionType.Button,"<Keyboard>/tab");
            if(theme==null)theme=new ShopUITheme();
        }
        void Awake(){InitializePresentation();}
        void OnEnable(){InitializePresentation();receiptKey.Enable();}
        void OnDisable(){receiptKey?.Disable();RestoreCapture();}
        void OnDestroy(){receiptKey?.Dispose();RestoreCapture();}
        void Update()
        {
            if(!shift||!player)return;
            if(receiptKey.WasPressedThisFrame())ToggleBook();
            if(shift.Closed&&!sawClosed)ShowPanel(Panel.Report);
            sawClosed=shift.Closed;
            if(lastMessage!=LocalMessage){lastMessage=LocalMessage;statusAt=Time.unscaledTime;}
            RefreshView();
            // Consume UI clicks after the existing Esc action releases the cursor.
            // Movement, E, Q, camera, cursor release and time scale remain unchanged.
            bool over=false;
            if(input && input.CaptureCursor!=null && Cursor.lockState!=CursorLockMode.Locked && Mouse.current!=null){
                var p=Mouse.current.position.ReadValue();p.y=Screen.height-p.y;float scale=Scale(Screen.width,Screen.height);p=(p-Offset(Screen.width,Screen.height,scale))/scale;
                over=BusinessAsUsual.Multiplayer.NetworkSession.Active && new Rect(48,267,260,40).Contains(p);over|=CurrentPanel!=Panel.None?new Rect(218,62,1540,950).Contains(p):new Rect(1442,44,430,326).Contains(p)||new Rect(48,815,510,175).Contains(p)||new Rect(1500,850,372,140).Contains(p);
            }
            if(over && !blockedCapture && input.CaptureCursor.enabled){input.CaptureCursor.Disable();blockedCapture=true;}
            else if(!over)RestoreCapture();
        }
        void RestoreCapture(){if(blockedCapture && input && input.CaptureCursor!=null){input.CaptureCursor.Enable();blockedCapture=false;}}
        public static float Scale(int w,int h)=>Mathf.Min(w/ShopUITheme.Width,h/ShopUITheme.Height);
        public static Vector2 Offset(int w,int h,float s)=>new Vector2(Mathf.Max(0,(w-ShopUITheme.Width*s)/2),Mathf.Max(0,(h-ShopUITheme.Height*s)/2));
        public void ToggleBook(){ShowPanel(CurrentPanel==Panel.None?Panel.Receipts:Panel.None);}
        public void ShowPanel(Panel p){CurrentPanel=p;openedAt=Time.unscaledTime;receiptScroll=Vector2.zero;if(p==Panel.None)RestoreCapture();}
        public void SelectReceipt(int n){SelectedReceipt=Mathf.Clamp(n,0,Mathf.Max(0,shift.Transactions.Receipts.Count-1));receiptScroll=Vector2.zero;openedAt=Time.unscaledTime;}
        void RefreshView()
        {
            if(!shift||shift.Orders==null)return;
            unchecked{int h=shift.Stock.Journal.Count*397+shift.OrderIndex;h=h*31+shift.Transactions.Receipts.Count;h=h*31+(shift.Closed?1:0);h=h*31+(shift.Carried?1:0);h=h*31+(shift.Message?.GetHashCode()??0);foreach(var t in shift.Tasks.Tasks)h=h*31+t.progress;if(h!=signature||view==null){view=new ShopUIView(shift);signature=h;}}
        }
        bool Click(Rect r)=>Cursor.lockState!=CursorLockMode.Locked && ShopUITheme.Hit(r);
        void OnGUI()
        {
            if(!shift||!player||shift.Orders==null)return;InitializePresentation();RefreshView();if(view==null)return;
            var m=GUI.matrix;var c=GUI.color;
            try{float s=Scale(Screen.width,Screen.height);var o=Offset(Screen.width,Screen.height,s);GUI.matrix=Matrix4x4.TRS(new Vector3(o.x,o.y,0),Quaternion.identity,new Vector3(s,s,1));
                if(CurrentPanel==Panel.None)DrawHUD();
                else{ShopUITheme.Fill(new Rect(-o.x/s,-o.y/s,Screen.width/s,Screen.height/s),new Color(0,0,0,.46f));float t=reducedMotion?1:Mathf.Clamp01((Time.unscaledTime-openedAt)/ShopUITheme.PaperSeconds);GUI.matrix=GUI.matrix*Matrix4x4.Translate(new Vector3(0,(1-t)*(1-t)*12,0));
                    if(CurrentPanel==Panel.Order)DrawOrder();else if(CurrentPanel==Panel.Receipts)DrawReceipts();else if(CurrentPanel==Panel.Report)DrawReport();else DrawHandbook();}
            }finally{GUI.matrix=m;GUI.color=c;}
        }
        void DrawHUD()
        {
            theme.Art("identity",48,44);theme.Art("bag",70,65);theme.Text(174,60,"Business as Usual",32,2,365);theme.Text(177,115,"DAYLIGHT MARKET / STAFF EDITION",18,1,360);
            theme.Art("order-small",1442,44);theme.Tag(1442,44,"ORDER SLIP",160);theme.Text(1466,65,view.customer,38,2,382);theme.Text(1468,124,view.orderId==""?view.context:view.context+" • "+view.orderId,23,0,380,0,true,43);theme.Line(1464,167,384);
            int y=189;foreach(var i in view.items.Where(i=>i.needed>0)){theme.Item(1468,y,i.sku,.66f);theme.Text(1524,y-2,i.name,24,1,242);theme.Text(1524,y+32,"IN BASKET",17,0,170);theme.Text(1848,y+27,i.basket+" / "+i.needed,27,1,140,2);y+=78;}
            if(view.orderId=="")theme.Text(1468,203,"All customer orders served.\nReview the receipt book.",24,0,360,0,true);
            if(Click(new Rect(1442,44,430,326)))ShowPanel(Panel.Order);
            theme.Art("task",48,815);theme.Tag(48,815,"ON YOUR SHIFT",190);theme.Text(73,835,view.activeTitle,30,2,460);theme.Text(75,892,view.activeDetail,24,0,460,0,true,49);theme.Text(75,942,view.completedTasks+"/"+shift.Tasks.Tasks.Count+" tasks complete",22,1,280);theme.Text(533,945,"Tab Receipts",20,0,165,2);
            if(Click(new Rect(48,815,510,175)))ShowPanel(Panel.Tasks);
            theme.Art("revenue",1500,850);theme.Text(1525,868,"SHIFT REVENUE",19,1,330);theme.Text(1525,900,view.revenue,38,2,325);theme.Text(1844,945,view.receiptCount+" receipts",21,0,200,2);
            if(Click(new Rect(1500,850,372,140)))ShowPanel(Panel.Receipts);
            if(LocalCarried){theme.Art("carry",637,44);theme.Text(662,61,"CARRYING",20,1,135);theme.Text(805,61,string.Join(" • ",view.items.Select(i=>ShopUIView.NameFor(i.sku)+" "+i.carry)),24,1,445);theme.Text(662,91,view.items.Sum(i=>i.carry)+" units in the stock crate",18,0,550);}
            DrawPrompt();
            if(Time.unscaledTime-statusAt<5){theme.Art("status",1442,414);theme.Tag(1442,414,"STAFF NOTE",160);theme.Text(1466,442,LocalMessage,22,0,382,0,true,147);}
        }
        void DrawPrompt()
        {
            bool busy=reach && reach.IsActive;var station=player.Current as ShopStation;var item=station && station.kind==ShopStation.Kind.Shelf?view.items.FirstOrDefault(i=>i.sku==station.sku):null;
            bool compact=LocalCarried||busy;float px=compact?637:626,py=compact?922:882;
            theme.Art(compact?"prompt-compact":"prompt",px,py);
            if(!compact)theme.Tag(px,py,item!=null?"SHELF / E":"INTERACTION",190);
            float keyX=compact?657:647,keyY=compact?940:909;
            if(busy)theme.Pill(keyX,keyY,58,56,"…",ShopUITheme.Pale,32);else theme.Key(keyX,keyY,"E");
            float x=737;
            if(item!=null&&!compact){theme.Item(730,908,item.sku,.8f);x=803;}
            string action=busy?reach.ActivityLabel:player.Current?player.Current.Prompt:"Approach a labeled shop station";
            bool placeCrate=LocalCarried&&BusinessAsUsual.Multiplayer.NetworkSession.Active&&item==null;
            if(placeCrate&&!busy)action="Place stock crate on the ground";
            if(station&&station.kind==ShopStation.Kind.Shelf&&BusinessAsUsual.Multiplayer.NetworkSession.Active&&shift.Carried&&!LocalCarried)action="Crate carried by colleague";
            theme.Text(x,compact?939:905,action,compact?30:33,1,(compact?1340:1400)-x);
            string detail=item!=null?(LocalCarried?"Place "+Mathf.Min(item.carry,6-item.shelf)+" units • Q returns the crate":!shift.Received?"Receive the delivery at the rear stockroom first.":shift.Tasks.Tasks.Any(t=>t.id.StartsWith("stock:")&&!t.Complete)?"Finish restocking all three products first.":"Take 1 unit • Shelf "+item.shelf+"/6 • Basket "+item.basket+"/"+item.needed):LocalMessage;
            if(placeCrate)detail="E at a labeled shelf restocks it • Q returns the crate";
            if(busy)detail="Stock placement in progress.";
            theme.Text(x,compact?977:957,detail,compact?20:22,0,(compact?1340:1400)-x,0,true,compact?34:58);
        }
        void Book(string title,string subtitle,Panel active)
        {
            theme.Art("book",218,136);theme.Text(265,166,"DAYLIGHT MARKET / STAFF EDITION",22,1,640);theme.Text(265,208,title,44,2,635);theme.Text(266,278,subtitle,24,0,635,0,true,50);theme.Line(262,332,625,false,ShopUITheme.Ink,3);
            string[] names={"TASKS","INVENTORY","RECEIPTS","SHIFT REPORT"};Panel[] pages={Panel.Tasks,Panel.Inventory,Panel.Receipts,Panel.Report};
            for(int n=0;n<4;n++){float x=356+n*320;theme.Pill(x,98,266,42,names[n],active==pages[n]?ShopUITheme.Mustard:ShopUITheme.Paper);if(Click(new Rect(x,98,266,42)))ShowPanel(pages[n]);}
            theme.Text(1653,889,"Tab Close • Esc Release mouse",22,0,600,2);if(Click(new Rect(1320,882,340,47)))ShowPanel(Panel.None);
        }
        void DrawHandbook()
        {
            Book("Your shift, on paper.","Receive. Restock. Serve. Leave it tidy.",CurrentPanel);int n=0;
            foreach(var task in shift.Tasks.Tasks){int y=366+n*72;ShopUITheme.Fill(new Rect(260,y,622,62),ShopUITheme.Rule);ShopUITheme.Fill(new Rect(262,y+2,618,58),ShopUITheme.Pale);theme.Pill(272,y+10,47,42,(n+1).ToString("D2"),task.Complete?ShopUITheme.Paper:ShopUITheme.Mustard);
                string d=task.description.Replace("Restock 6 ","Restock ").Replace("Fulfill three customer orders","Fulfill customer orders").Replace("Review receipts and close shift","Review receipts & close shift");theme.Text(335,y+14,d,24,1,415);theme.Text(863,y+14,task.progress+" / "+task.target,24,1,83,2);if(task.Complete)theme.Line(335,y+39,305,false,ShopUITheme.Green);n++;}
            theme.Text(984,180,"Inventory card",39,2,650);theme.Text(984,240,"COUNTED SHIFT STOCK ONLY",20,1,640);
            for(n=0;n<3;n++){float x=1108+n*209;theme.Item(x+44,309,view.items[n].sku,1.3f);theme.Text(x+80,415,ShopUIView.NameFor(view.items[n].sku),26,1,180,1);}
            bool dock=view.items.Any(i=>i.dock>0);string[] rows=dock?new[]{"Delivered","In stockroom","In crate","On shelf","In basket","Sold"}:new[]{"Delivered","In crate","On shelf","In basket","Sold"};
            for(n=0;n<rows.Length;n++){int y=472+n*(dock?50:59);theme.Line(980,y-7,646);theme.Text(989,y,rows[n]=="In stockroom"&&BusinessAsUsual.Multiplayer.NetworkSession.Active&&BusinessAsUsual.Multiplayer.NetworkShop.Instance.Dropped?"Dropped crate":rows[n],24,1,190);
                for(int j=0;j<3;j++){var i=view.items[j];int count=rows[n]=="Delivered"?(shift.Received?shift.Stock.Total(i.sku)-shift.Stock.Count("delivery",i.sku):0):rows[n]=="In stockroom"?i.dock:rows[n]=="In crate"?i.carry:rows[n]=="On shelf"?i.shelf:rows[n]=="In basket"?i.basket:i.sold;theme.Text(1188+j*209,y,count.ToString(),27,1,95,1);}}
            theme.Text(990,797,"Q returns the remaining crate stock.",23,3,640);theme.Text(990,839,"Decorative assortment is not inventory.",21,0,640);
        }
        void DrawOrder()
        {
            theme.Art("order",466,80);theme.Tag(466,80,"CUSTOMER ORDER",210);theme.Text(514,113,view.orderId==""?"All customers served!":"One for the "+view.context+".",39,2,890);theme.Text(517,180,view.orderId==""?"RECEIPTS ARE FILED IN YOUR HANDBOOK":view.customer.ToUpperInvariant()+" / ORDER "+view.orderId+" / CUSTOMER "+view.orderNumber+" OF "+shift.Orders.Length,24,1,890);theme.Line(512,239,894,false,ShopUITheme.Ink,3);
            theme.Text(590,261,"ITEM",20,1,250);theme.Text(1138,261,"IN BASKET",20,1,170,2);theme.Text(1399,261,"LINE VALUE",20,1,190,2);
            int y=335;foreach(var i in view.items.Where(i=>i.needed>0)){theme.Item(523,y,i.sku,1.4f);theme.Text(623,y+8,i.name,32,1,458);theme.Text(623,y+62,i.unit+" each",25,0,400);theme.Text(1138,y+35,i.basket+" / "+i.needed,34,1,140,2);theme.Text(1399,y+35,i.line,32,1,215,2);theme.Line(513,y+126,892);y+=161;}
            theme.Text(517,677,"ORDER VALUE",24,1,450);theme.Text(1398,658,view.orderValue,56,2,460,2);theme.Pill(517,753,410,51,view.orderId==""?"ALL ORDERS PAID":view.missing==0?"BASKET READY / UNPAID":"NEEDS "+view.missing+" MORE ITEMS");
            string missing=string.Join(" and ",view.items.Where(i=>i.needed>i.basket).Select(i=>(i.needed-i.basket)+" "+ShopUIView.NameFor(i.sku)));
            theme.Text(519,837,missing==""?view.orderId==""?"Review your receipts at the closing terminal.":"Your basket is ready for checkout.":"Collect "+missing+" at their shelves.",27,3,890);theme.Text(519,900,"Bring the complete basket to Checkout. E prints the receipt.",23,0,890);theme.Text(1399,946,"Tab Close • Esc Release mouse",20,0,550,2);if(Click(new Rect(1070,939,335,42)))ShowPanel(Panel.None);
        }
        void DrawReceipts()
        {
            var receipts=shift.Transactions.Receipts;theme.Art("receipt-list",286,182);theme.Tag(286,182,"RECEIPT BOOK",205);theme.Text(312,211,"Printed today",30,2,287);theme.Text(314,270,receipts.Count+" unique receipts",23,0,287);
            for(int n=0;n<receipts.Count;n++){int y=338+n*142;var r=receipts[n];theme.Art("receipt-card",310,y);if(n==SelectedReceipt)ShopUITheme.Fill(new Rect(314,y+4,277,103),ShopUITheme.Pale);theme.Text(330,y+13,r.id,22,1,245);theme.Text(330,y+49,ShopUIView.Name(r.customer),26,1,140);theme.Text(575,y+53,ShopUIView.Money(r.totalCents),25,1,110,2);if(Click(new Rect(310,y,285,111)))SelectReceipt(n);}
            theme.Text(314,783,"Tab Close receipt book\nEsc Release mouse",21,0,287,0,true,66);if(Click(new Rect(310,777,285,75)))ShowPanel(Panel.None);
            theme.Art("receipt",687,62);theme.Art("bag",715,88);theme.Text(835,97,"DAYLIGHT",43,2,560);theme.Text(835,154,"NEIGHBORHOOD MARKET",23,1,560);theme.Line(725,213,657,true,ShopUITheme.Ink);
            if(receipts.Count==0){theme.Text(727,270,"No receipts printed yet.",30,1,635);theme.Text(727,345,"Fulfill a customer order at Checkout.\nYour receipt will be filed here.",25,0,635,0,true);}
            else{SelectedReceipt=Mathf.Clamp(SelectedReceipt,0,receipts.Count-1);var r=receipts[SelectedReceipt];theme.Text(727,236,"RECEIPT "+r.id,25,1,635);theme.Text(727,283,"Order "+r.orderId+" • "+r.customer,25,0,635);theme.Text(727,328,r.timestampUtc.Length>=19?r.timestampUtc.Substring(0,19).Replace("T","  ")+" UTC":r.timestampUtc,22,0,635);theme.Line(725,380,657,true,ShopUITheme.Ink);theme.Text(727,410,"QTY / ITEM",20,1,400);theme.Text(1380,410,"AMOUNT",20,1,200,2);
                bool scroll=r.lines.Length>2;float contentHeight=Mathf.Max(220,r.lines.Length*115);if(scroll)receiptScroll=GUI.BeginScrollView(new Rect(725,456,674,231),receiptScroll,new Rect(0,0,650,contentHeight));
                for(int n=0;n<r.lines.Length;n++){var l=r.lines[n];int y=scroll?n*115:463+n*115;float x=scroll?2:727;theme.Text(x,y,l.quantity+" × "+l.name,30,1,635);theme.Text(x+29,y+47,"Unit price "+ShopUIView.Money(l.unitPriceCents),23,0,420);theme.Text(scroll?655:1380,y+47,ShopUIView.Money(l.totalCents),27,1,210,2);}if(scroll)GUI.EndScrollView();
                theme.Line(725,701,657,true,ShopUITheme.Ink);theme.Text(727,732,"TOTAL",39,2,350);theme.Text(1380,722,ShopUIView.Money(r.totalCents),53,2,430,2);theme.Stamp("paid",946,806,Time.unscaledTime-openedAt,reducedMotion);theme.Text(1055,917,"Thanks for stopping by.",25,3,600,1);}
            theme.Pill(1480,200,266,46,"TASKS",ShopUITheme.Paper);if(Click(new Rect(1480,200,266,46)))ShowPanel(Panel.Tasks);theme.Pill(1480,263,266,46,"SHIFT REPORT",ShopUITheme.Paper);if(Click(new Rect(1480,263,266,46)))ShowPanel(Panel.Report);
        }
        void DrawReport()
        {
            Book(view.closed?"Shift complete!":"Your shift so far.",view.closed?"All orders served. Your receipts are filed.":"A running tally. Finish your tasks to close.",Panel.Report);if(view.closed)theme.Stamp("good-shift",1271,174,Time.unscaledTime-openedAt,reducedMotion);
            theme.Text(269,371,"REVENUE",23,1,600);theme.Text(265,415,view.revenue,90,2,620);theme.Line(262,548,625);
            string[] labels={"Customer orders","Tasks completed","Town Stability"};string[] values={shift.OrderIndex+" / "+shift.Orders.Length,view.completedTasks+" / "+shift.Tasks.Tasks.Count,"+"+shift.StabilityContribution};for(int n=0;n<3;n++){int y=576+n*70;theme.Text(269,y,labels[n],29,1,435);theme.Text(862,y,values[n],34,1,190,2);}
            theme.Text(985,301,"Receipt ledger",34,2,640);for(int n=0;n<shift.Transactions.Receipts.Count;n++){var r=shift.Transactions.Receipts[n];int y=367+n*103;theme.Line(981,y-10,644);theme.Text(990,y,ShopUIView.Name(r.customer),30,1,370);theme.Text(990,y+44,r.id,22,0,370);theme.Text(1628,y+14,ShopUIView.Money(r.totalCents),32,1,230,2);if(Click(new Rect(980,y-8,653,100))){SelectReceipt(n);ShowPanel(Panel.Receipts);}}
            theme.Art("summary",979,713);int sold=view.items.Sum(i=>i.sold),remaining=view.items.Sum(i=>i.shelf+i.dock+i.carry+i.basket);theme.Text(1000,734,"DELIVERED "+(shift.Received?18:0)+" / SOLD "+sold+" / REMAINING "+remaining,22,1,610);theme.Text(1001,784,string.Join(" • ",view.items.Select(i=>i.shelf+" "+ShopUIView.NameFor(i.sku)))+" on the shelves",23,0,610);theme.Text(267,841,view.closed?BusinessAsUsual.Multiplayer.NetworkSession.Active?"Esc releases the mouse. Disconnect to leave.":"Head to the front exit. E returns to town.":"Continue your shift at the labeled stations.",24,3,620);if(view.message.Contains("Export failed"))theme.Text(979,850,view.message,24,0,660,0,true,70);
        }
    }
}
