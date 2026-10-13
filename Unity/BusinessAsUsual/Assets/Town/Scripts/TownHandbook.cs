using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.Prototype;
using BusinessAsUsual.Shopkeeper;
using BusinessAsUsual.FirstPerson;

namespace BusinessAsUsual.Town
{
    // Shared owner-only paper UI. Transactions and authoritative progression remain in their domains.
    public sealed class TownHandbook : MonoBehaviour
    {
        public enum Page { Tasks, Inventory, Records, Summary }
        public Page CurrentPage;
        public bool Expanded,Choosing;
        public bool TutorialOpen { get; private set; }
        int tutorialPage;
        Profession tutorialJob;
        public void DismissTutorial(){TutorialOpen=false;input.Blocked=PauseMenu.IsOpen;input.SetCursor(!PauseMenu.IsOpen);Invalidate();}
        // Phase 7A: live minimap; M toggles the expanded town map without releasing the mouse.
        public bool MapExpanded;
        public TownMinimap Minimap=>minimap;
        public bool FontsAvailable=>theme!=null&&theme.FontsAvailable;
        public TownGuidance Guide { get; private set; }
        public string PersonalNote=>Guide?.note??"Choose your work";
        public int MarkerCount=>wayfinding?.Count??0;
        public float HudPaperFraction=>(480*142+306*258+640*80+420*76)/(1920f*1080);
        ShopUITheme theme;
        TownGraphics graphics;
        TownWayfinding wayfinding;
        TownMinimap minimap;
        float nextColleagueCheck;
        NetworkPlayer player;
        PrototypeInput input;
        ShopHUD shopHUD;
        Camera ownerCamera;
        InputAction jobsKey,bookKey,mapKey;
        bool blockedCapture,inside;
        float openedAt,rewardUntil;
        int revision=-1,units;
        public void Invalidate(){revision=-1;}
        Profession previousJob;
        JobTask[] tasks=Array.Empty<JobTask>();
        RecordCard[] records=Array.Empty<RecordCard>();
        sealed class RecordCard {public string header,place,body,time,icon;}
        TownJobs town=>NetworkShop.Instance?NetworkShop.Instance.Town:null;
        public Profession LocalJob=>town&&player&&player.IsSpawned?town.Job(player.OwnerClientId):Profession.None;
        void Initialize()
        {
            if(!player)player=GetComponent<NetworkPlayer>();if(!input)input=GetComponent<PrototypeInput>();if(!shopHUD)shopHUD=GetComponent<ShopHUD>();
            if(theme==null)theme=new ShopUITheme();if(graphics==null)graphics=new TownGraphics(theme);
            if(jobsKey==null)jobsKey=new InputAction("Town professions",InputActionType.Button,"<Keyboard>/j");
            if(bookKey==null)bookKey=new InputAction("Town handbook",InputActionType.Button,"<Keyboard>/tab");
            if(mapKey==null)mapKey=new InputAction("Town map",InputActionType.Button,"<Keyboard>/m");
            if(minimap==null)minimap=new TownMinimap(theme,graphics);
        }
        void Awake(){Initialize();}
        void OnEnable(){Initialize();jobsKey.Enable();bookKey.Enable();mapKey.Enable();revision=-1;if(player.IsOwner)input.SetCursor(false);}
        void OnDisable(){jobsKey?.Disable();bookKey?.Disable();mapKey?.Disable();MapExpanded=false;RestoreCapture();wayfinding?.Dispose();wayfinding=null;if(shopHUD)shopHUD.enabled=false;}
        void OnDestroy(){jobsKey?.Dispose();bookKey?.Dispose();mapKey?.Dispose();wayfinding?.Dispose();}
        void Update()
        {
            if(!town||!player.IsOwner||!player.IsSpawned)return;
            shopHUD.enabled=false; // The approved ShopHUD is retained for earlier scenes; town uses one presentation for all jobs.
            if(BusinessAsUsual.RescueV2.RescueInterface.Instance){
                bool rescueRoom=player.transform.position.x<-25;
                if(revision!=NetworkShop.Instance.Revision||inside!=rescueRoom){revision=NetworkShop.Instance.Revision;inside=rescueRoom;Refresh();}
                wayfinding?.Dispose();wayfinding=null;return;
            }
            bool paused=PauseMenu.IsOpen;
            if(NetworkShop.Instance.Workday&&LocalJob!=Profession.None&&!paused){
                if(LocalJob!=tutorialJob){tutorialJob=LocalJob;TutorialOpen=true;tutorialPage=0;Expanded=false;Choosing=false;MapExpanded=false;}
                if(Keyboard.current!=null&&Keyboard.current.hKey.wasPressedThisFrame){TutorialOpen=true;tutorialPage=0;Expanded=false;Choosing=false;MapExpanded=false;}
                if(TutorialOpen){
                    input.Blocked=true;input.SetCursor(false);
                    if(Keyboard.current!=null&&Keyboard.current.enterKey.wasPressedThisFrame){if(tutorialPage==0)tutorialPage=1;else DismissTutorial();}
                    return;
                }
            }
            if(!paused&&NetworkShop.Instance.Workday&&(Choosing||LocalJob==Profession.None)&&Keyboard.current!=null)
                for(int i=1;i<=6;i++)if(Keyboard.current[(Key)((int)Key.Digit1+i-1)].wasPressedThisFrame){Choose(i);break;}
            if(!paused&&mapKey.WasPressedThisFrame()&&LocalJob!=Profession.None&&!Expanded&&!Choosing){MapExpanded=!MapExpanded;TownAudio.Play(TownAudio.Cue.Paper,null,.6f);}
            if(Time.unscaledTime>=nextColleagueCheck){nextColleagueCheck=Time.unscaledTime+.2f;var view=FirstPersonView.Local;minimap.Refresh(player,view&&view.Active?view.ViewCamera.transform.position:player.transform.position+Vector3.up);}
            if(paused)return;
            if(jobsKey.WasPressedThisFrame()){RestoreCapture();MapExpanded=false;Choosing=!Choosing;Expanded=false;openedAt=Time.unscaledTime;input.SetCursor(!Choosing);}
            if(bookKey.WasPressedThisFrame()&&LocalJob!=Profession.None){RestoreCapture();MapExpanded=false;Expanded=!Expanded;Choosing=false;openedAt=Time.unscaledTime;input.SetCursor(!Expanded);}
            bool room=player.transform.position.x<-25;
            if(revision!=NetworkShop.Instance.Revision||inside!=room){revision=NetworkShop.Instance.Revision;inside=room;Refresh();}
            if(!ownerCamera)ownerCamera=GetComponentInChildren<Camera>(true);wayfinding?.Tick(ownerCamera);
            bool over=false;
            if(Mouse.current!=null&&Cursor.lockState!=CursorLockMode.Locked){
                var p=Mouse.current.position.ReadValue();p.y=Screen.height-p.y;float s=ShopHUD.Scale(Screen.width,Screen.height);p=(p-ShopHUD.Offset(Screen.width,Screen.height,s))/s;
                over=(Choosing||LocalJob==Profession.None)?new Rect(450,260,1020,610).Contains(p):Expanded?new Rect(218,62,1540,950).Contains(p):new Rect(48,44,480,142).Contains(p)||new Rect(1564,44,306,258).Contains(p);
                over|=new Rect(550,46,170,38).Contains(p)||new Rect(48,1032,260,34).Contains(p);
                if(over&&input.CaptureCursor.enabled){input.CaptureCursor.Disable();blockedCapture=true;}else if(!over)RestoreCapture();
            }
        }
        public void CloseFromEscape(){if(TutorialOpen){DismissTutorial();return;}RestoreCapture();Expanded=false;Choosing=false;MapExpanded=false;input.SetCursor(true);}
        float Facing()=>FirstPersonView.Local&&FirstPersonView.Local.Active&&FirstPersonView.Local.gameObject==gameObject?FirstPersonView.Local.Yaw:player.Facing.Value;
        void RestoreCapture(){if(blockedCapture&&input&&input.CaptureCursor!=null){input.CaptureCursor.Enable();blockedCapture=false;}}
        void Refresh()
        {
            var job=LocalJob;Guide=TownGuidance.Create(player,NetworkShop.Instance);
            tasks=job==Profession.None?Array.Empty<JobTask>():NetworkShop.Instance.Workday?NetworkShop.Instance.Workday.Tasks(job,NetworkShop.Instance.Workday.IsAssistant(player.OwnerClientId),player.OwnerClientId).Tasks.ToArray():town.Tasks(job).Tasks.ToArray();int count=tasks.Sum(t=>t.progress);
            if(job==previousJob&&count>units)rewardUntil=Time.unscaledTime+1.5f;units=count;previousJob=job;
            if(wayfinding==null)wayfinding=new TownWayfinding(graphics);wayfinding.Bind(Guide);
            if(job==Profession.Mechanic)records=town.ServiceRecords.Select(r=>new RecordCard{header=r.id,place=r.vehicle,body=r.fault+" / "+r.part,time=TimeLabel(r.timestampUtc),icon="wrench"}).ToArray();
            else if(job==Profession.Sanitation)records=town.DisposalRecords.Select(r=>new RecordCard{header=r.id,place=r.source,body=r.materialType+" / "+r.objectDescription,time=TimeLabel(r.timestampUtc),icon="recycle"}).ToArray();
            else if(job==Profession.Shopkeeper)records=NetworkShop.Instance.Shift.Transactions.Receipts.Select(r=>new RecordCard{header=r.id,place=r.customer,body=ShopUIView.Money(r.totalCents),time=TimeLabel(r.timestampUtc),icon="record"}).ToArray();
            else records=Array.Empty<RecordCard>();
        }
        static string TimeLabel(string value)=>DateTime.TryParse(value,out var time)?time.ToUniversalTime().ToString("HH:mm")+" / town copy":"Town copy";
        public void Choose(int job){player.SendCommand("job:"+job);Choosing=false;Expanded=false;RestoreCapture();input.SetCursor(true);}
        bool Click(Rect rect)=>Cursor.lockState!=CursorLockMode.Locked&&!PauseMenu.IsOpen&&ShopUITheme.Hit(rect);
        void OpenBook(){Expanded=true;CurrentPage=Page.Tasks;openedAt=Time.unscaledTime;input.SetCursor(false);}
        void OnGUI()
        {
            if(BusinessAsUsual.RescueV2.RescueInterface.Instance)return;
            if(!town||!player||!player.IsOwner||!player.IsSpawned)return;
            var original=GUI.matrix;var color=GUI.color;float scale=ShopHUD.Scale(Screen.width,Screen.height);var offset=ShopHUD.Offset(Screen.width,Screen.height,scale);GUI.matrix=Matrix4x4.TRS(new Vector3(offset.x,offset.y,0),Quaternion.identity,new Vector3(scale,scale,1));
            try{
                if(TutorialOpen){Tutorial();return;}
                if(Choosing||LocalJob==Profession.None){Selection();return;}
                if(Guide==null)return;
                if(Expanded){float t=Mathf.Clamp01((Time.unscaledTime-openedAt)/ShopUITheme.PaperSeconds);GUI.matrix*=Matrix4x4.Translate(Vector3.up*((1-t)*(1-t)*12));Book();}
                else{HUD();if(MapExpanded&&minimap.Baked)minimap.DrawFull(new Rect(400,110,1120,860),Guide,player.transform.position,Facing(),town,true);}
            }finally{GUI.matrix=original;GUI.color=color;}
        }
        void Tutorial()
        {
            TownGraphics.Card(new Rect(390,190,1140,700));
            theme.Text(440,230,"YOUR FIRST WORKDAY / "+(tutorialPage+1)+" OF 2",25,1,1020);
            theme.Text(440,288,tutorialPage==0?"Ordinary jobs, extraordinary suspicion":"Your job: "+LocalJob,37,2,1020);
            if(tutorialPage==0){
                theme.Text(440,368,"Keep the town supplied: share deliveries, repair the vehicle and process its waste. This workday has no criminal role.",27,0,1010,0,true,95);
                theme.Text(440,486,"W A S D  Move     Mouse  Look     Shift  Walk faster",27,1,1010);
                theme.Text(440,545,"E  Use the thing you are looking at     R  Alternate action",26,1,1010);
                theme.Text(440,603,"Q  Return your carried load     M  Map     Tab  Handbook",26,1,1010);
                theme.Text(440,669,"Follow the single job marker. The next-step card explains what to do when you arrive. H reopens this introduction.",25,0,1010,0,true,84);
            }else{
                bool assistant=NetworkShop.Instance.Workday.IsAssistant(player.OwnerClientId);
                string chain=assistant?"You support a lead: carry deliveries, check intake, stage waste and reset workstations. You do not replace the lead's repair or sorting tasks.":LocalJob==Profession.Shopkeeper?"Receive the shipment → prepare the parcel → carry it to the garage. Serve at least one customer order at your market counter.":LocalJob==Profession.Mechanic?"Request a battery → open the hood → diagnose → receive/check the parcel → loosen the terminal → fit the battery.":"Collect the square bag → inspect it at the yard → sort it → file the manifest. After the garage repair, collect and process its packaging and old battery too.";
                theme.Text(440,373,chain,28,0,1010,0,true,130);
                var guide=NetworkShop.Instance.Workday.Guidance(player);
                theme.Text(440,535,"FIRST: "+guide.note,29,2,1010);
                theme.Text(440,596,guide.detail,25,0,1010,0,true,100);
                theme.Text(440,714,"The jobs depend on each other. With only one player, you can practise your job; a full shift needs the other professions.",23,0,1010,0,true,70);
            }
            theme.Pill(1060,806,420,48,tutorialPage==0?"ENTER / NEXT":"ENTER / START WORK",ShopUITheme.Green,22);
            if(Click(new Rect(1060,806,420,48))){if(tutorialPage==0)tutorialPage=1;else DismissTutorial();}
        }
        void Selection()
        {
            if(NetworkShop.Instance.Workday){WorkdaySelection();return;}
            theme.Art("book",450,260,1020,610);theme.Text(510,295,"Town staff desk",44,2,900);theme.Text(515,367,"Choose your work",27,0,890);
            for(int i=0;i<3;i++){
                float y=435+i*96;bool occupied=town.Owners[i]!=NetworkShop.Nobody&&town.Owners[i]!=player.OwnerClientId;
                TownGraphics.Card(new Rect(520,y,880,76));graphics.Icon(TownGuidance.RoleIcon((Profession)(i+1)),new Rect(532,y+7,58,58),occupied?.4f:1);
                theme.Text(610,y+8,town.Definitions[i].title,30,2,460);theme.Text(610,y+46,occupied?"Assigned to Sunny "+town.Owners[i]:"Available",21,0,710);
                if(!occupied&&Click(new Rect(520,y,880,76)))Choose(i+1);
            }
            theme.Text(520,748,"One person per profession. J returns here.",23,0,870);
            if(!string.IsNullOrEmpty(player.Interactor.Status))theme.Text(520,791,Short(player.Interactor.Status,68),22,1,870);
        }
        void WorkdaySelection()
        {
            TownGraphics.Card(new Rect(430,230,1060,655));theme.Text(480,268,"Town staff desk",40,2,960);
            theme.Text(480,335,"Three professions / one shift assistant",25,0,960);
            string[] descriptions={"Stock, customer orders and garage supplies","Diagnosis, fitting and recoverable scrap","Collection order, sorting and manifests"};
            for(int i=0;i<3;i++) {
                float y=412+i*130;TownGraphics.Card(new Rect(480,y,960,110));graphics.Icon(TownGuidance.RoleIcon((Profession)(i+1)),new Rect(495,y+21,58,58));
                theme.Text(575,y+12,town.Definitions[i].title,28,2,560);theme.Text(575,y+59,descriptions[i],20,0,540);
                bool lead=town.Owners[i]==NetworkShop.Nobody||town.Owners[i]==player.OwnerClientId;
                bool assistant=town.Owners[3]==NetworkShop.Nobody||town.Owners[3]==player.OwnerClientId;
                theme.Pill(1135,y+10,275,38,lead?"LEAD / "+(i+1):"LEAD ASSIGNED",lead?ShopUITheme.Green:ShopUITheme.Rule,18);
                theme.Pill(1135,y+60,275,38,assistant?"ASSIST / "+(i+4):"ASSIST ASSIGNED",assistant?ShopUITheme.Pale:ShopUITheme.Rule,18);
                if(lead&&Click(new Rect(1135,y+10,275,38)))Choose(i+1);
                if(assistant&&Click(new Rect(1135,y+60,275,38)))Choose(i+4);
            }
            theme.Text(480,826,"Assistant: dispatch, intake checks, gate help and workstation resets.",20,0,950);
        }
        static string Short(string text,int limit){if(text.Length<=limit)return text;int end=text.LastIndexOf(' ',limit-3);return text.Substring(0,end>limit/2?end:limit-3)+"...";}
        static string Brief(string text){int stop=text.IndexOf(". ");return Short(stop>0?text.Substring(0,stop+1):text,35);}
        void HUD()
        {
            if(NetworkShop.Instance.Workday)NetworkShop.Instance.Workday.DrawStatus();
            TownGraphics.Card(new Rect(48,44,480,142));graphics.Icon(TownGuidance.RoleIcon(LocalJob),new Rect(60,61,62,62));
            theme.Text(137,61,LocalJob+(NetworkShop.Instance.Workday&&NetworkShop.Instance.Workday.IsAssistant(player.OwnerClientId)?" assistant":""),26,2,365);theme.Text(137,106,PersonalNote,24,1,372);
            Progress(tasks,new Rect(137,152,332,8));theme.Text(512,143,tasks.Count(t=>t.Complete)+"/"+tasks.Length,19,1,65,2);
            if(Click(new Rect(48,44,480,142)))OpenBook();
            if(NetworkShop.Instance.Workday){TownGraphics.Card(new Rect(48,706,670,156));theme.Text(64,720,"NEXT: "+Guide.note,23,1,635);theme.Text(64,761,Guide.detail,21,0,635,0,true,88);}
            if(minimap.Baked)minimap.DrawCompact(new Rect(1564,44,306,258),Guide,player.transform.position,Facing(),town);
            else graphics.Map(new Rect(1564,44,306,258),Guide,player.transform.position,Facing(),town);
            if(Click(new Rect(1564,44,306,258)))OpenBook();
            if(!NetworkShop.Instance.Workday&&LocalJob==Profession.Shopkeeper&&inside&&NetworkShop.Instance.Shift.CurrentOrder!=null&&tasks.Where(t=>t.id.StartsWith("stock:")).All(t=>t.Complete))BasketSlip();
            var item=player.Interactor.Current;bool working=town.IsWorking(player.OwnerClientId),carrying=town.HasBag(player.OwnerClientId)||NetworkShop.Instance.Holder==player.OwnerClientId||(NetworkShop.Instance.Workday&&NetworkShop.Instance.Workday.Holding(player.OwnerClientId));
            if(item||working||carrying){
                TownGraphics.Card(new Rect(640,943,640,80));theme.Key(652,955,"E");
                theme.Text(729,957,working?"Hold to work":item?Action(item):"Place carried load",27,1,530);
                if(working){theme.Line(730,1003,520,false,ShopUITheme.Rule,6);theme.Line(730,1003,520*town.WorkProgress(player.OwnerClientId),false,ShopUITheme.Mustard,6);}
                else theme.Text(730,999,item is WorkdayStation ws&&ws.alternative!=null?"R  "+ws.altLabel+"     Q return":carrying?"Q returns your load":"",17,0,520);
            }
            theme.Text(48,1008,"H  Help    Tab  Handbook    J  Staff desk    M  Map    Esc  Pause",21,0,900);
            if(Time.unscaledTime<rewardUntil){graphics.Icon("check",new Rect(550,100,50,50));theme.Text(613,111,"FILED",17,1,110);}
            if(!string.IsNullOrEmpty(player.Interactor.Status)){TownGraphics.Card(new Rect(48,888,500,55));theme.Text(64,902,Brief(player.Interactor.Status),23,0,465);}
        }
        void BasketSlip()
        {
            var shift=NetworkShop.Instance.Shift;TownGraphics.Card(new Rect(48,210,420,76));int i=0;
            foreach(var line in shift.CurrentOrder.lines){theme.Item(62+i*190,220,line.sku,.50f);theme.Text(103+i*190,231,shift.Stock.Count("basket",line.sku)+" / "+line.quantity,25,1,105);i++;}
        }
        string Action(Interactable item)
        {
            if(item is WorkdayStation w)return w.label;
            if(item is TownStation t){switch(t.kind){case TownStation.Kind.Inspect:return "Inspect bay "+(t.index+1);case TownStation.Kind.Repair:return "Repair bay "+(t.index+1);case TownStation.Kind.Sort:return "Sort "+t.materialType.ToLowerInvariant();case TownStation.Kind.ServiceDesk:case TownStation.Kind.RouteDesk:return "Collect / file / review";case TownStation.Kind.RequestParts:return "Send parts slip";case TownStation.Kind.PackParts:return "Pack the garage parcel";case TownStation.Kind.CollectParts:return "Collect replacement parts";default:return "Enter Daylight Market";}}
            if(item is WasteBag b)return "Collect "+town.Bags[b.index].materialType.ToLowerInvariant()+" bag";
            if(item is StockCrate)return "Pick up stock crate";
            if(item is ShopStation s){switch(s.kind){case ShopStation.Kind.Shelf:return (NetworkShop.Instance.Holder==player.OwnerClientId?"Restock ":"Take ")+ShopUIView.NameFor(s.sku);case ShopStation.Kind.Checkout:return "Ring up basket";case ShopStation.Kind.Delivery:return "Receive delivery";case ShopStation.Kind.Close:return "Review the shift";case ShopStation.Kind.Exit:return "Return to town";}}
            return Short(item.Prompt,38);
        }
        static void Progress(JobTask[] source,Rect r)
        {
            if(source.Length==0)return;float width=r.width/source.Length;
            for(int i=0;i<source.Length;i++){float progress=Mathf.Clamp01((float)source[i].progress/source[i].target);ShopUITheme.Fill(new Rect(r.x+i*width,r.y,width-5,r.height),ShopUITheme.Rule);ShopUITheme.Fill(new Rect(r.x+i*width,r.y,(width-5)*progress,r.height),ShopUITheme.Green);}
        }
        void Book()
        {
            theme.Art("book",218,136);theme.Text(265,168,"DAYLIGHT / STAFF HANDBOOK",21,1,640);graphics.Icon(TownGuidance.RoleIcon(LocalJob),new Rect(265,215,64,64));theme.Text(348,222,LocalJob+" copy",40,2,560);
            string[] names={"TASKS","INVENTORY",LocalJob==Profession.Shopkeeper?"RECEIPTS":"RECORDS","SHIFT REPORT"};
            for(int i=0;i<4;i++){float x=356+i*320;theme.Pill(x,98,266,42,names[i],CurrentPage==(Page)i?ShopUITheme.Mustard:ShopUITheme.Paper);if(Click(new Rect(x,98,266,42)))CurrentPage=(Page)i;}
            if(NetworkShop.Instance.Workday)NetworkShop.Instance.Workday.DrawBook(CurrentPage,player);
            else if(CurrentPage==Page.Tasks)TaskBook();else if(CurrentPage==Page.Inventory)InventoryBook();else if(CurrentPage==Page.Records)RecordBook();else SummaryBook();
            theme.Text(268,895,"WORK SLIPS / STAFF COPY",19,0,635);theme.Text(1653,895,"Tab  Close",22,1,230,2);if(Click(new Rect(1420,882,240,47)))Expanded=false;
        }
        void TaskBook()
        {
            for(int i=0;i<tasks.Length;i++){
                var task=tasks[i];float y=345+i*75;TownGraphics.Card(new Rect(272,y,630,63));graphics.Icon(task.Complete?"check":TownGuidance.TaskIcon(task.id),new Rect(280,y+5,48,48));theme.Text(343,y+12,TownGuidance.TaskTitle(task.id),25,1,455);theme.Text(887,y+13,task.progress+"/"+task.target,24,1,90,2);
            }
            if(minimap.Baked)minimap.DrawFull(new Rect(990,310,636,397),Guide,player.transform.position,Facing(),town,false);
            else graphics.Map(new Rect(990,310,636,397),Guide,player.transform.position,Facing(),town,true);theme.Text(997,735,PersonalNote,29,2,620);theme.Text(997,785,Guide.detail,25,0,620,0,true,87);
        }
        void InventoryBook()
        {
            if(LocalJob==Profession.Sanitation){
                var bags=town.Bags.Where(b=>b.available||b.collected||b.stage>0).ToArray();
                for(int i=0;i<bags.Length;i++){var b=bags[i];float x=i<3?275:997,y=349+i%3*160;TownGraphics.Card(new Rect(x,y,621,139));graphics.Icon(b.stage>0?"check":"bag",new Rect(x+12,y+16,61,61));theme.Text(x+89,y+15,b.source,27,1,520);theme.Text(x+89,y+63,b.materialType+" / "+(b.stage==2?"Filed":b.stage==1?"Sorted":b.holder==player.OwnerClientId?"In your hands":b.holder!=NetworkShop.Nobody?"Being carried":"Pickup"),24,0,516);}
            }
            else if(LocalJob==Profession.Mechanic){
                string[] names={"RESERVE","PICKUP LOCKER","YOUR PARTS","INSTALLED"};for(int i=0;i<4;i++){float x=i<2?275:997,y=350+i%2*222;TownGraphics.Card(new Rect(x,y,620,195));theme.Text(x+20,y+15,names[i],23,1,576);graphics.Icon("battery",new Rect(x+28,y+62,67,67));theme.Text(x+112,y+76,town.Parts.Count(TownJobs.PartAccounts[i],"battery").ToString(),34,2,160);graphics.Icon("filter",new Rect(x+324,y+62,67,67));theme.Text(x+409,y+76,town.Parts.Count(TownJobs.PartAccounts[i],"filter").ToString(),34,2,150);}
                theme.Text(998,814,"A staffed market packs your slip. An unstaffed market uses the finite reserve.",23,0,621,0,true,64);
            }
            else{
                var shift=NetworkShop.Instance.Shift;string[] accounts={"dock","carry","shelf","basket"},names={"STOCKROOM","YOUR CRATE","SHELVES","BASKET"};
                for(int i=0;i<4;i++){float x=i<2?275:997,y=350+i%2*222;TownGraphics.Card(new Rect(x,y,620,195));theme.Text(x+20,y+15,names[i],23,1,576);for(int j=0;j<3;j++){var product=ShopShift.Products[j];theme.Item(x+28+j*194,y+68,product.sku,.63f);theme.Text(x+85+j*194,y+84,shift.Stock.Count(accounts[i],product.sku).ToString(),29,1,96);}}
            }
        }
        void RecordBook()
        {
            theme.Text(273,311,LocalJob==Profession.Mechanic?"Service slips":LocalJob==Profession.Sanitation?"Disposal slips":"Receipt file",30,2,1300);
            if(records.Length==0){graphics.Icon("record",new Rect(280,420,90,90));theme.Text(393,438,"Your file is empty",30,1,1100);theme.Text(393,498,"Processed work is filed at your workplace desk.",24,0,1060);return;}
            for(int i=0;i<records.Length&&i<6;i++){var r=records[i];float x=i<3?275:997,y=367+i%3*159;TownGraphics.Card(new Rect(x,y,622,144));graphics.Icon(r.icon,new Rect(x+10,y+13,47,47));theme.Text(x+76,y+13,r.header,24,1,525);theme.Text(x+17,y+59,r.place,24,1,586);theme.Text(x+17,y+94,Short(r.body,44),22,0,590);theme.Text(x+604,y+16,r.time,16,0,230,2);}
        }
        void SummaryBook()
        {
            theme.Text(272,349,"Your shift",36,2,620);Progress(tasks,new Rect(275,418,590,13));theme.Text(275,467,tasks.Count(t=>t.Complete)+" / "+tasks.Length+" tasks filed",31,1,610);theme.Text(275,539,records.Length+" work slips",28,0,610);
            if(town.Completed[(int)LocalJob-1]){graphics.Icon("check",new Rect(285,619,106,106));theme.Text(413,652,"GOOD SHIFT",32,2,430);theme.Text(413,710,"+3 Town Stability",26,1,430);}else theme.Text(275,650,"Your workplace desk closes the shift.",27,0,610,0,true,96);
            theme.Text(998,349,"Public town ledger",34,2,620);for(int i=0;i<3;i++){var role=(Profession)(i+1);var publicTasks=town.Tasks(role).Tasks.ToArray();float y=425+i*112;graphics.Icon(TownGuidance.RoleIcon(role),new Rect(995,y,57,57));theme.Text(1070,y+3,role.ToString(),27,1,425);theme.Text(1605,y+3,publicTasks.Count(t=>t.Complete)+"/"+publicTasks.Length,27,1,105,2);Progress(publicTasks,new Rect(1071,y+55,535,7));}
            theme.Text(1000,805,"Town Stability  +"+town.Contribution,28,1,620);
        }
    }
}
