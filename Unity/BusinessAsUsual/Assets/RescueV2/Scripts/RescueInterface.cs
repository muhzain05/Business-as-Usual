using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.Prototype;
using BusinessAsUsual.FirstPerson;
using BusinessAsUsual.Town;
using BusinessAsUsual.Shopkeeper;

namespace BusinessAsUsual.RescueV2
{
    // One local canvas. Job/inventory/record views are projections of the authenticated workday.
    // The canvas never owns stock, awards work or serializes private pages into multiplayer state.
    public sealed partial class RescueInterface : MonoBehaviour
    {
        public static RescueInterface Instance {get;private set;}
        public static bool Requested=>!NetworkTestDriver.Requested||Environment.GetCommandLineArgs().Contains("--bau-rescue")||BusinessAsUsual.Phase8B.Phase8BFlow.Enabled;
        public RescueSkin skin;
        public string ScreenName {get;private set;}="Main";
        public string CurrentAction {get;private set;}="Choose a job";
        public string LastStamp {get;private set;}="";
        public int PrivateRecordCount {get;private set;}
        public int FontComparison;
        public bool ReducedMotion {get;private set;}
        public bool MenuOpen=>ScreenName!="HUD";
        public bool HasClipboard=>ScreenName=="Handbook"||ScreenName=="Map";
        public int SelectedTab {get;private set;}
        RectTransform root,content,hud,prompt,mini,stampContainer;
        TextMeshProUGUI objective,roleLabel,status,promptLabel,promptAlt,stampLabel,inputGlyph;
        Image roleIcon,actionIcon,promptIcon;
        RawImage miniImage;
        RectTransform miniPlayer,miniTarget,routePin;
        InputAction help,book,staff,map;
        NetworkPlayer player;
        NetworkSession session;
        string fingerprint="",lastFeedback="";
        float refreshAt,openedAt,stampUntil;
        Profession introduced=Profession.None;
        int introPage,recordIndex;
        bool detailOpen;
        readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        readonly List<GameObject> temp=new List<GameObject>();
        readonly List<Image> progress=new List<Image>();
        TownHandbook Handbook=>player?player.GetComponent<TownHandbook>():null;
        LivingWorkday Workday=>NetworkShop.Instance?NetworkShop.Instance.Workday:null;
        Profession Job=>player&&NetworkShop.Instance&&NetworkShop.Instance.Town?NetworkShop.Instance.Town.Job(player.OwnerClientId):Profession.None;
        static readonly Vector2 Mid=new Vector2(.5f,.5f);
        static readonly Vector2 TopLeft=new Vector2(0,1);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap(){SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;Loaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);}
        static void Loaded(Scene scene,LoadSceneMode mode)
        {
            if(!Requested||!FindFirstObjectByType<TownJobs>())return;
            if(Instance)return;
            var prefab=Resources.Load<GameObject>("RescueV2/WorkdayCanvas");
            if(prefab)Instantiate(prefab);else new GameObject("Workday UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(RescueInterface));
        }
        void Awake()
        {
            if(Instance&&Instance!=this){Destroy(gameObject);return;}Instance=this;
            skin=skin?skin:Resources.Load<RescueSkin>("RescueV2/WorkdaySkin");if(!skin){enabled=false;return;}
            ReducedMotion=PlayerPrefs.GetInt("bau.rescue.reducedMotion",0)==1;
            var canvas=GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=80;
            var scale=GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1280,720);scale.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scale.matchWidthOrHeight=.5f;
            if(!FindFirstObjectByType<EventSystem>()){var e=new GameObject("Workday UI input",typeof(EventSystem),typeof(InputSystemUIInputModule));e.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();}
            root=GetComponent<RectTransform>();
            help=KeyAction("Help","<Keyboard>/h");book=KeyAction("Handbook","<Keyboard>/tab");staff=KeyAction("Staff desk","<Keyboard>/j");map=KeyAction("Map","<Keyboard>/m");
            BuildHUD();if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled){DontDestroyOnLoad(gameObject);var events=FindFirstObjectByType<EventSystem>();if(events)DontDestroyOnLoad(events.gameObject);BuildPhase8BDestination();}Show("Main");
        }
        InputAction KeyAction(string name,string binding){var a=new InputAction(name,InputActionType.Button,binding);a.Enable();return a;}
        void OnDestroy(){help?.Dispose();book?.Dispose();staff?.Dispose();map?.Dispose();if(Instance==this)Instance=null;}
        RectTransform Node(string name,Transform parent,Vector2 size,Vector2 at,Vector2? anchor=null)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor??TopLeft;r.pivot=TopLeft;r.sizeDelta=size;r.anchoredPosition=new Vector2(at.x,-at.y);return r;
        }
        Image Plate(string name,Transform parent,float x,float y,float w,float h,Color color,Vector2? anchor=null)
        {
            var prefab=Resources.Load<GameObject>("RescueV2/Components/PaperPanel");RectTransform r;
            if(prefab){r=Instantiate(prefab,parent,false).GetComponent<RectTransform>();r.name=name;r.anchorMin=r.anchorMax=anchor??TopLeft;r.pivot=TopLeft;r.sizeDelta=new Vector2(w,h);r.anchoredPosition=new Vector2(x,-y);}else r=Node(name,parent,new Vector2(w,h),new Vector2(x,y),anchor);
            var img=r.GetComponent<Image>()??r.gameObject.AddComponent<Image>();img.color=color;
            var paper=Resources.Load<Sprite>("RescueV2/Panel");if(paper){img.sprite=paper;img.type=Image.Type.Sliced;}img.raycastTarget=false;return img;
        }
        TMP_FontAsset Display=>FontComparison==0?skin.display:skin.comparisonHeadings[Mathf.Clamp(FontComparison-1,0,skin.comparisonHeadings.Length-1)];
        TextMeshProUGUI Text(Transform parent,string text,float x,float y,float w,float h,int size=18,bool bold=false,bool display=false,Color? color=null)
        {
            var prefab=Resources.Load<GameObject>("RescueV2/Components/ReadingText");RectTransform r;
            if(prefab){r=Instantiate(prefab,parent,false).GetComponent<RectTransform>();r.name="Text / "+text;r.anchorMin=r.anchorMax=TopLeft;r.pivot=TopLeft;r.sizeDelta=new Vector2(w,h);r.anchoredPosition=new Vector2(x,-y);}else r=Node("Text / "+text,parent,new Vector2(w,h),new Vector2(x,y));
            var t=r.GetComponent<TextMeshProUGUI>()??r.gameObject.AddComponent<TextMeshProUGUI>();t.font=display?Display:bold?(text.Any(char.IsDigit)?skin.readingBold:skin.labels):skin.reading;t.fontSize=size;t.color=color??skin.ink;t.text=text;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Overflow;t.raycastTarget=false;t.margin=Vector4.zero;t.lineSpacing=2;return t;
        }
        Sprite Sprite(string name)
        {
            if(!sprites.TryGetValue(name,out var s)){s=Resources.Load<Sprite>("RescueV2/Icons/"+name);sprites[name]=s;}return s;
        }
        Image Icon(Transform parent,string name,float x,float y,float size)
        {
            var r=Node("Illustration / "+name,parent,new Vector2(size,size),new Vector2(x,y));var image=r.gameObject.AddComponent<Image>();image.sprite=Sprite(name);image.preserveAspect=true;image.raycastTarget=false;return image;
        }
        Button Button(Transform parent,string label,float x,float y,float w,float h,Action action,bool primary=false,bool enabled=true)
        {
            var image=Plate("Button / "+label,parent,x,y,w,h,primary?skin.mustard:skin.white);image.raycastTarget=true;
            var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;b.interactable=enabled;b.transition=Selectable.Transition.None;
            var feedback=image.gameObject.AddComponent<RescueButtonFeedback>();feedback.Configure(skin,primary?skin.mustard:skin.white);
            var t=Text(image.transform,label,12,0,w-24,h,18,true);t.alignment=TextAlignmentOptions.Midline;
            b.onClick.AddListener(()=>{TownAudio.Play(TownAudio.Cue.Click);action();});return b;
        }
        void ClearContent(){if(content)Destroy(content.gameObject);temp.Clear();content=null;fingerprint="";}
        RectTransform Card(string name,float w=640,float h=516)
        {
            var shade=Plate("Local reading shade",root,0,0,0,0,new Color(.04f,.10f,.10f,.36f));shade.rectTransform.anchorMin=Vector2.zero;shade.rectTransform.anchorMax=Vector2.one;shade.rectTransform.offsetMin=shade.rectTransform.offsetMax=Vector2.zero;shade.raycastTarget=true;
            content=shade.rectTransform;
            var bg=Plate(name,content,-w/2,-h/2,w,h,skin.paper,Mid).rectTransform;bg.pivot=TopLeft;
            // The clip sits above the content inset, never over the diagram or type.
            var clip=Plate("Clipboard clip",bg,w/2-34,-7,68,13,skin.ink);clip.raycastTarget=false;
            return bg;
        }
        public void Show(string screen)
        {
            ClearContent();ScreenName=screen;openedAt=Time.unscaledTime;detailOpen=false;
            hud.gameObject.SetActive(screen=="HUD");prompt.gameObject.SetActive(false);mini.gameObject.SetActive(screen=="HUD");
            if(player){var input=player.GetComponent<PrototypeInput>();input.Blocked=screen!="HUD"||PauseMenu.IsOpen;input.SetCursor(screen=="HUD"&&!PauseMenu.IsOpen);}
            if(Handbook){Handbook.Expanded=screen=="Handbook";Handbook.Choosing=screen=="Staff";Handbook.MapExpanded=screen=="Map";}
            if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled&&BuildPhase8BScreen(screen))return;
            if(screen=="Main")Main();else if(screen=="Staff")Staff();else if(screen=="Intro")Intro();else if(screen=="Handbook")Book();else if(screen=="Map")Map();else if(screen=="Pause")Pause();else if(screen=="Settings")Settings();else if(screen=="Display")DisplaySettings();else if(screen=="Controls")Controls();else if(screen=="Report")Report();
        }
        void BuildHUD()
        {
            hud=Node("Compact work HUD",root,new Vector2(350,96),new Vector2(24,24));
            Plate("Task slip",hud,0,0,326,90,skin.paper);roleIcon=Icon(hud,"shop",10,12,46);roleLabel=Text(hud,"",66,10,244,23,16,true);objective=Text(hud,"",66,33,244,29,22,true);
            for(int i=0;i<5;i++)progress.Add(Plate("Step "+i,hud,66+i*46,72,38,4,skin.rule));
            prompt=Node("Context prompt",root,new Vector2(352,62),new Vector2(-176,90),new Vector2(.5f,0));prompt.pivot=TopLeft;prompt.anchoredPosition=new Vector2(-176,90);
            Plate("Prompt card",prompt,0,0,352,62,skin.paper);Plate("Keycap",prompt,10,10,40,40,skin.white);inputGlyph=Text(prompt,"E",10,11,40,38,22,true);inputGlyph.alignment=TextAlignmentOptions.Midline;
            promptLabel=Text(prompt,"",60,8,280,28,20,true);promptAlt=Text(prompt,"",60,36,280,20,14);
            mini=Node("Compact map",root,new Vector2(152,154),new Vector2(-176,24),new Vector2(1,1));Plate("Map frame",mini,0,0,152,154,skin.paper);Text(mini,"DAYLIGHT",10,7,133,20,14,true);
            var r=Node("Live map image",mini,new Vector2(132,112),new Vector2(10,32));miniImage=r.gameObject.AddComponent<RawImage>();miniImage.raycastTarget=false;
            miniPlayer=Plate("You",r,0,0,9,9,skin.green).rectTransform;miniPlayer.pivot=Mid;miniTarget=Plate("Next stop",r,0,0,12,12,skin.mustard).rectTransform;miniTarget.pivot=Mid;
            routePin=Node("Destination pin",root,new Vector2(92,47),Vector2.zero);Plate("Pin body",routePin,0,0,92,47,skin.paper);var diamond=Plate("Destination diamond",routePin,15,15,14,14,skin.mustard);diamond.rectTransform.localRotation=Quaternion.Euler(0,0,45);Text(routePin,"",36,7,52,25,16,true);routePin.gameObject.SetActive(false);
            var s=Node("Confirmed result",root,new Vector2(310,42),new Vector2(24,82),new Vector2(0,0));s.pivot=TopLeft;s.anchoredPosition=new Vector2(24,82);stampContainer=s;Plate("Result slip",s,0,0,310,42,skin.paper);stampLabel=Text(s,"",10,0,290,42,18,true);stampLabel.alignment=TextAlignmentOptions.Midline;stampLabel.color=skin.green;s.gameObject.SetActive(false);
            Text(hud,"Tab  Work slip",0,98,180,20,14,false,false,skin.white);
        }
        void Main()
        {
            var c=Card("Start workday",560,506);Text(c,"DAYLIGHT MUNICIPAL / STAFF EDITION",32,28,496,22,14,true);Text(c,"Business\nas Usual",32,64,496,108,44,true,true);
            Text(c,"Ordinary jobs, extraordinary suspicion.",32,184,496,26,18);Icon(c,"shop",384,59,122);
            Button(c,"START SOLO",32,244,496,52,()=>{session=FindFirstObjectByType<NetworkSession>();session?.StartSolo();},true);
            Button(c,"HOST WORKDAY",32,308,242,48,()=>{session=FindFirstObjectByType<NetworkSession>();session?.StartSession(true);});
            Button(c,"JOIN WORKDAY",286,308,242,48,()=>Show("Join"));
            Text(c,"Solo: unattended town services help finish your shift.",32,379,496,45,16);
            Button(c,"SETTINGS",32,444,242,38,()=>Show("Settings"));Button(c,"QUIT",286,444,242,38,()=>Application.Quit());
        }
        void Staff()
        {
            var c=Card("Choose a profession",700,500);Text(c,"STAFF DESK",28,24,644,24,14,true);Text(c,"What is your job?",28,57,644,45,32,true,true);
            string[] labels={"Shopkeeper","Mechanic","Sanitation"},icons={"shop","wrench","cart"},verbs={"Stock · serve · supply","Inspect · repair · verify","Collect · sort · recover"};
            for(int i=0;i<3;i++){
                int job=i+1;float x=28+i*218;Plate("Profession "+job,c,x,122,204,270,skin.white);Icon(c,icons[i],x+44,143,116);Text(c,labels[i],x+12,269,180,31,23,true);Text(c,verbs[i],x+12,310,180,43,16);
                bool free=NetworkShop.Instance.Town.Owners[i]==NetworkShop.Nobody||NetworkShop.Instance.Town.Owners[i]==player.OwnerClientId;
                Button(c,free?(job+"  START"):"ASSIGNED",x+12,354,180,38,()=>Choose(job),true,free);
            }
            if(!session.Solo)Button(c,"SHIFT ASSISTANT",28,410,310,42,()=>{SelectedTab=4;AssistantDesk();});
            Text(c,session.Solo?"One window. Choose a job and complete its shift.":"Three leads and one assistant share the town.",28,461,644,24,16);
        }
        void AssistantDesk()
        {
            ClearContent();var c=Card("Assistant assignment",600,388);Text(c,"Help a workplace",28,40,544,44,32,true,true);string[] names={"Market dispatch","Garage intake","Waste collection"};
            for(int i=0;i<3;i++){int j=i+4;Button(c,names[i],28,109+i*65,544,48,()=>Choose(j),i==0,NetworkShop.Instance.Town.Owners[3]==NetworkShop.Nobody||NetworkShop.Instance.Town.Owners[3]==player.OwnerClientId);}Button(c,"BACK",28,317,180,42,()=>Show("Staff"));
        }
        void Choose(int value){player.SendCommand("job:"+value);if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled){pendingProfession=value>3?value-3:value;setupAt=Time.unscaledTime;BusinessAsUsual.Phase8B.Phase8BFlow.LoadingLabel="Preparing your work slip";BusinessAsUsual.Phase8B.Phase8BFlow.Tip="Different workplaces share the same supplies.";Show("Assigning");}else{ScreenName="Assigning";ClearContent();}}
        void Intro()
        {
            var c=Card("Illustrated first shift",660,480);Text(c,Job.ToString().ToUpperInvariant()+" / FIRST SHIFT",28,23,604,24,14,true);Text(c,"Learn by doing",28,57,604,46,32,true,true);
            string[] labels=introPage==0?new[]{"Move","Look","Use"}:StepLabels(Job);string[] icons=introPage==0?new[]{"map","mouse","check"}:StepIcons(Job);
            int count=Math.Min(labels.Length,5);for(int i=0;i<count;i++){float width=604f/count;Icon(c,icons[i],28+i*width+(width-86)/2,128,86);Text(c,labels[i],28+i*width,224,width-8,44,18,true).alignment=TextAlignmentOptions.Center;}
            if(introPage==0){Text(c,"W A S D",43,292,156,29,20,true).alignment=TextAlignmentOptions.Center;Text(c,"MOUSE",245,292,156,29,20,true).alignment=TextAlignmentOptions.Center;Text(c,"E",446,292,156,29,20,true).alignment=TextAlignmentOptions.Center;Text(c,"Aim at the marked object. Your hands show the result.",28,349,604,29,18);}
            else{Text(c,"Follow the marker. Tab opens this work slip.",28,318,604,29,18);Text(c,session.Solo?"Town services support the jobs nobody is staffing.":"Help colleagues move the same supplies between jobs.",28,361,604,26,16);}
            Button(c,introPage==0?"ENTER  NEXT":"ENTER  START WORK",28,416,604,42,()=>{if(introPage==0){introPage=1;Show("Intro");}else{introduced=Job;EnterPhase8BWork();}},true);
        }
        static string[] StepLabels(Profession job)=>job==Profession.Mechanic?new[]{"Open","Check","Replace","Verify","Sign"}:job==Profession.Shopkeeper?new[]{"Receive","Stock","Serve","Dispatch","File"}:new[]{"Collect","Inspect","Sort","File","Finish"};
        static string[] StepIcons(Profession job)=>job==Profession.Mechanic?new[]{"hood","test","replace","test","sign"}:job==Profession.Shopkeeper?new[]{"parcel","shelf","checkout","parcel","receipt"}:new[]{"cart","bag","sort","receipt","check"};
        public void OpenBook(int tab=0){SelectedTab=tab;Show("Handbook");}
        void Tabs(RectTransform c)
        {
            string[] names=BusinessAsUsual.Phase8B.Phase8BFlow.Enabled?new[]{"JOB","ITEMS","RECORDS","TOWN","PRIVATE"}:new[]{"JOB","ITEMS","RECORDS","TOWN"};var r=Node("Attached paper tabs",c,new Vector2(592,40),new Vector2(24,104));var layout=r.gameObject.AddComponent<HorizontalLayoutGroup>();layout.spacing=8;layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandWidth=false;layout.childForceExpandHeight=false;
            for(int i=0;i<names.Length;i++){int tab=i;float tabWidth=names.Length==5?112:142;var b=Button(r,names[i],0,0,tabWidth,40,()=>OpenBook(tab),i==SelectedTab);var e=b.gameObject.AddComponent<LayoutElement>();e.preferredWidth=tabWidth;e.preferredHeight=40;}
        }
        void Book()
        {
            if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled&&SelectedTab==4){PrivateSlip();return;}
            var c=Card("Employee work slip",640,514);Text(c,Job.ToString().ToUpperInvariant()+" / STAFF COPY",24,22,494,21,14,true);Text(c,SelectedTab==0?"Today's work":SelectedTab==1?"Items in hand":SelectedTab==2?"Your paper file":"Town today",24,48,494,48,32,true,true);Button(c,"×",561,24,55,42,()=>Show("HUD"));Tabs(c);
            if(SelectedTab==0)JobSlip(c);else if(SelectedTab==1)Items(c);else if(SelectedTab==2)Records(c);else Town(c);
            Text(c,"Tab  Close     H  Illustrated help",24,477,400,22,14);Button(c,"HELP",499,468,117,30,()=>{introPage=1;Show("Intro");});
        }
        void JobSlip(RectTransform c)
        {
            var icons=StepIcons(Job);var labels=StepLabels(Job);var done=StepProgress();
            var r=Node("Illustrated action sequence",c,new Vector2(592,91),new Vector2(24,160));var group=r.gameObject.AddComponent<HorizontalLayoutGroup>();group.spacing=12;group.childControlHeight=group.childControlWidth=true;group.childForceExpandHeight=group.childForceExpandWidth=false;
            for(int i=0;i<5;i++){var p=Plate("Action "+labels[i],r,0,0,108,91,done[i]?skin.green:skin.white);var e=p.gameObject.AddComponent<LayoutElement>();e.preferredWidth=108;e.preferredHeight=91;Icon(p.transform,done[i]?"check":icons[i],29,7,50);var t=Text(p.transform,labels[i],4,64,100,23,16,true,false,done[i]?skin.white:skin.ink);t.alignment=TextAlignmentOptions.Center;}
            if(detailOpen){Plate("Optional work notes",c,24,260,592,188,skin.white);Text(c,CurrentAction,40,279,560,40,24,true,true);Text(c,Detail(),40,332,460,96,16);}
            else {Icon(c,ActionIcon(),30,265,142);Text(c,CurrentAction,192,271,409,55,28,true,true);Text(c,QuickHint(),192,340,409,50,18);Text(c,Workday?.Holding(player.OwnerClientId)==true?"ONE LOAD / IN YOUR HANDS":"Supplies are shared across the town.",24,424,450,24,16);}
            Button(c,detailOpen?"LESS":"WHY?",510,415,106,32,()=>{detailOpen=!detailOpen;RefreshBook();});
        }
        void RefreshBook(){bool was=detailOpen;Show("Handbook");detailOpen=was;if(was){ClearContent();Book();}}
        bool[] StepProgress()
        {
            var s=Workday.Domain.State;if(Job==Profession.Mechanic)return new[]{s.hood||s.diagnosed,s.diagnosed,s.installed,s.repairTested,s.serviceSigned};
            if(Job==Profession.Shopkeeper)return new[]{s.received,s.items.Any(i=>i.shelved)||s.customerOrders>=1,s.customerOrders>=1,s.dispatched,s.shiftSigned};
            return new[]{s.items.Any(i=>i.id=="BAG-8A-001"&&i.custody!="square"),s.items.Any(i=>i.inspected&&new[]{"BAG-8A-001","PACK-8A-001","OLD-8A-001"}.Contains(i.id)),s.items.Count(i=>i.custody=="sorted"||i.custody=="recorded")>=3,s.items.Count(i=>i.custody=="recorded")>=3,s.shiftSigned};
        }
        void Items(RectTransform c)
        {
            var held=Workday.Domain.State.items.Where(i=>i.holder==player.OwnerClientId).ToArray();if(held.Length==0){Icon(c,"parcel",220,166,172);Text(c,"Hands free",24,353,592,34,24,true).alignment=TextAlignmentOptions.Center;Text(c,"Aim at a package or tool to pick it up.",24,400,592,28,18).alignment=TextAlignmentOptions.Center;return;}
            if(held.Any(i=>i.custody=="parcel")){var carton=held.First(i=>i.id=="PACK-8A-001");Icon(c,"parcel",220,163,166);Text(c,"Garage parcel",24,342,592,38,26,true).alignment=TextAlignmentOptions.Center;Text(c,carton.id+" / battery and packaging",24,395,592,27,18).alignment=TextAlignmentOptions.Center;Text(c,Workday.Domain.State.verified?"Stock contents checked":"Sealed / contents unchecked",24,438,592,26,18).alignment=TextAlignmentOptions.Center;return;}
            for(int i=0;i<held.Length;i++){var item=held[i];float x=24+i*296;Plate("Carried item",c,x,155,284,279,skin.white);Icon(c,item.material=="stock"?item.sku:item.material=="battery"||item.id=="OLD-8A-001"?"battery":"bag",x+75,172,130);Text(c,item.material=="stock"?ProductName(item.sku):item.material=="battery"?"Replacement battery":item.id=="OLD-8A-001"?"Used battery":"Collected batch",x+16,318,252,49,22,true);Text(c,item.id,x+16,384,252,22,16);}
            Text(c,"Q  Return safely     R  Inspect at a work point",24,448,592,24,16);
        }
        void Records(RectTransform c)
        {
            var records=NetworkShop.Instance.IsServer?Workday.RecordsFor(player.OwnerClientId):Workday.LocalRecords;PrivateRecordCount=records.Length;
            if(records.Length==0){Icon(c,"receipt",213,169,176);Text(c,"No processed work yet",24,364,592,31,24,true).alignment=TextAlignmentOptions.Center;return;}
            recordIndex=Mathf.Clamp(recordIndex,0,records.Length-1);var record=records[records.Length-1-recordIndex];
            Icon(c,"receipt",36,164,79);Text(c,record.heading,133,165,459,50,24,true,true);Text(c,record.reference,133,223,459,27,18,true);Plate("Receipt rule",c,24,265,592,2,skin.rule);
            Text(c,record.text,24,285,592,92,20);Text(c,record.interval,24,395,380,25,16);Text(c,"Earlier handlers: not recorded",24,432,450,22,16);
            if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled)Button(c,"PIN COPY",492,438,124,28,()=>player.SendCommand("day:share:"+(records.Length-1-recordIndex)));
            Button(c,"‹",492,397,52,36,()=>{recordIndex++;Show("Handbook");},false,recordIndex<records.Length-1);Button(c,"›",556,397,60,36,()=>{recordIndex--;Show("Handbook");},false,recordIndex>0);
        }
        void Town(RectTransform c)
        {
            Icon(c,"map",36,164,120);Text(c,"Supply  "+Workday.Domain.State.stability+" / 20",183,175,400,39,28,true,true);Text(c,session.Solo?"SOLO / TOWN SERVICES ACTIVE":"MULTIPLAYER / SHARED WORK",183,227,409,40,16,true);
            Text(c,"Customer orders",24,310,440,30,20,true);Text(c,Workday.Domain.State.customerOrders+" / 3",499,310,117,30,20,true);Text(c,"Repair",24,354,440,30,20,true);Text(c,Workday.Domain.State.installed?"DONE":"OPEN",499,354,117,30,20,true);
            Text(c,"Waste filed",24,398,440,30,20,true);Text(c,Workday.Domain.State.items.Count(i=>i.custody=="recorded")+" / 3",499,398,117,30,20,true);
        }
        void Map()
        {
            var c=Card("Route card",690,560);Text(c,"DAYLIGHT / YOUR NEXT STOP",24,24,590,38,26,true,true);Button(c,"×",612,23,54,38,()=>Show("HUD"));
            var inside=player.transform.position.x<-25;var tex=Resources.Load<Texture2D>(inside?"Town/Map/DaylightMarketMap":"Town/Map/DaylightTownMap");var world=inside?TownMinimap.MarketWorld:TownMinimap.TownWorld;float mapWidth=420*world.width/world.height;var r=Node("Full map",c,new Vector2(mapWidth,420),new Vector2((690-mapWidth)/2,79));var image=r.gameObject.AddComponent<RawImage>();image.texture=tex;image.raycastTarget=false;
            PlaceMapDot(r,player.transform.position,world,skin.green,13);var g=Handbook?.Guide;if(g?.Primary!=null)PlaceMapDot(r,g.Primary.Position,world,skin.mustard,17);
            Text(c,"YOU       NEXT STOP       /       M Close",24,520,640,24,16);
        }
        void PlaceMapDot(RectTransform parent,Vector3 at,Rect world,Color color,float size)
        {
            var uv=new Vector2(Mathf.InverseLerp(world.xMin,world.xMax,at.x),Mathf.InverseLerp(world.yMin,world.yMax,at.z));var p=Plate("Map marker",parent,Mathf.Clamp01(uv.x)*parent.sizeDelta.x-size/2,(1-Mathf.Clamp01(uv.y))*parent.sizeDelta.y-size/2,size,size,color);p.rectTransform.localRotation=Quaternion.Euler(0,0,color==skin.mustard?45:0);
        }
        void Pause()
        {
            var c=Card("Staff break",450,435);Text(c,"STAFF BREAK",28,30,394,23,14,true);Text(c,"Take a breather",28,69,394,49,32,true,true);Text(c,session.Solo?"Your work is safe here.":"Your colleagues keep working.",28,132,394,30,18);
            Button(c,"RESUME",28,196,394,46,Resume,true);Button(c,"SETTINGS",28,255,191,44,()=>Show("Settings"));Button(c,"CONTROLS",231,255,191,44,()=>Show("Controls"));Button(c,"LEAVE WORKDAY",28,317,394,43,()=>player.GetComponent<PauseMenu>().Leave(false));
        }
        void Settings()
        {
            var c=Card("Settings",580,512);Text(c,"Your preferences",28,29,524,45,32,true,true);
            SettingSlider(c,"Volume",100,GameSettings.Master,0,1,GameSettings.SetMaster);
            SettingSlider(c,"Look sensitivity",179,GameSettings.Sensitivity,.02f,.5f,GameSettings.SetSensitivity);
            SettingSlider(c,"Field of view",258,GameSettings.FieldOfView,60,95,GameSettings.SetFieldOfView);
            Button(c,"REDUCED MOTION  "+(ReducedMotion?"ON":"OFF"),28,346,524,42,()=>{ReducedMotion=!ReducedMotion;PlayerPrefs.SetInt("bau.rescue.reducedMotion",ReducedMotion?1:0);Show("Settings");},ReducedMotion);
            Button(c,"DISPLAY & PERFORMANCE",28,399,524,42,()=>Show("Display"));
            Button(c,"BACK",28,464,160,30,()=>Show(player?"Pause":"Main"));
        }
        void SettingSlider(RectTransform parent,string title,float y,float value,float min,float max,Action<float> changed)
        {
            Text(parent,title,28,y,385,26,20,true);Text(parent,value>1?Mathf.RoundToInt(value).ToString():Mathf.RoundToInt(value/max*100)+"%",438,y,114,26,18,true);
            var track=Plate("Slider / "+title,parent,28,y+37,524,9,skin.rule);track.raycastTarget=true;var slider=track.gameObject.AddComponent<Slider>();slider.minValue=min;slider.maxValue=max;
            var fill=Plate("Fill",track.transform,0,0,524,9,skin.green);slider.fillRect=fill.rectTransform;
            var handle=Plate("Handle",track.transform,0,-6,20,22,skin.ink);slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.value=value;slider.onValueChanged.AddListener(v=>changed(v));
        }
        void DisplaySettings(){
            var c=Card("Display settings",620,548);Text(c,"Display & performance",28,28,564,45,30,true,true);
            var sizes=new[]{new Vector2Int(1280,720),new Vector2Int(1366,768),new Vector2Int(1920,1080),new Vector2Int(1920,1200)};
            for(int i=0;i<sizes.Length;i++){var size=sizes[i];Button(c,size.x+" × "+size.y,28+(i%2)*288,101+(i/2)*54,276,42,()=>{GameSettings.SetDisplay(Screen.fullScreenMode,size);Show("Display");},Screen.width==size.x&&Screen.height==size.y);}
            Button(c,Screen.fullScreenMode==FullScreenMode.Windowed?"MODE / WINDOWED":"MODE / FULLSCREEN",28,225,564,42,()=>{GameSettings.SetDisplay(Screen.fullScreenMode==FullScreenMode.Windowed?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed,new Vector2Int(Screen.width,Screen.height));Show("Display");});
            Button(c,"QUALITY / "+GameSettings.QualityName(GameSettings.Quality),28,279,564,42,()=>{GameSettings.SetQuality((GameSettings.Quality+1)%QualitySettings.names.Length);Show("Display");});
            Button(c,"V-SYNC / "+(GameSettings.VSync?"ON":"OFF"),28,333,564,42,()=>{GameSettings.SetVSync(!GameSettings.VSync);Show("Display");});
            Button(c,"FRAME LIMIT / "+(GameSettings.FrameCap==0?"NONE":GameSettings.FrameCap.ToString()),28,387,564,42,()=>{int index=Array.IndexOf(GameSettings.FrameCaps,GameSettings.FrameCap);GameSettings.SetFrameCap(GameSettings.FrameCaps[(index+1)%GameSettings.FrameCaps.Length]);Show("Display");});
            Button(c,"INVERT LOOK / "+(GameSettings.InvertY?"ON":"OFF"),28,441,564,42,()=>{GameSettings.SetInvertY(!GameSettings.InvertY);Show("Display");});Button(c,"BACK",28,500,160,30,()=>Show("Settings"));
        }
        void Controls()
        {
            var c=Card("Controls",600,458);Text(c,"At your fingertips",28,30,544,43,32,true,true);string[] left={"W A S D","MOUSE","E","R","Q","TAB / H","M / ESC"},right={"Move","Look","Use marked object","Optional work action","Return carried load","Work slip / illustrated help","Route map / staff break"};
            for(int i=0;i<left.Length;i++){Text(c,left[i],28,99+i*40,142,31,18,true);Text(c,right[i],182,99+i*40,390,31,18);}Button(c,"BACK",28,410,170,30,()=>Show(player?"Pause":"Main"));
        }
        void Report()
        {
            var s=Workday.Domain.State;bool complete=s.serviceSigned&&s.customerOrders>=1&&s.items.Count(i=>i.custody=="recorded")>=3;
            var c=Card("Shift report",620,482);Text(c,"DAYLIGHT / END OF SHIFT",28,26,564,25,14,true);Text(c,s.shiftSigned?"Shift filed":"Shift time ended",28,62,564,50,36,true,true);Icon(c,complete?"check":"receipt",222,130,172);
            Text(c,Job+" shift",28,322,564,32,24,true).alignment=TextAlignmentOptions.Center;Text(c,"Town supply  "+Workday.Domain.State.stability+" / 20",28,365,564,28,20,true).alignment=TextAlignmentOptions.Center;
            Button(c,"NEW WORKDAY",28,421,278,38,()=>session.Stop(),true);Button(c,"VIEW RECORDS",318,421,274,38,()=>OpenBook(2));
        }
        public void Resume(){player?.GetComponent<PauseMenu>()?.SetOpen(false,false);Show("HUD");}
        public bool HandleEscape(){if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled&&(ScreenName=="Loading"||ScreenName=="Assigning"||ScreenName=="WorkplaceEntry"||Day&&!Day.Public.started&&(ScreenName=="Hub"||ScreenName=="Staff"||ScreenName=="Intro")))return true;if(ScreenName=="HUD")return false;if(ScreenName=="Display")Show("Settings");else if(ScreenName=="Settings"||ScreenName=="Controls")Show(player?"Pause":"Main");else if(ScreenName=="Main"||ScreenName=="Join")return false;else Resume();return true;}
        public void Preview(string name,int tab=0,int font=0){FontComparison=font;SelectedTab=tab;introduced=Job;Show(name);}
        void Update()
        {
            session=session?session:FindFirstObjectByType<NetworkSession>();
            if(!player)player=FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsSpawned&&p.IsOwner);
            if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled&&TickPhase8B())return;
            if(!session)return;
            if(ScreenName=="Join"&&content==null)Join();
            if(player&&Workday){
                if(Job==Profession.None&&(ScreenName=="Main"||ScreenName=="Assigning"))Show("Staff");
                else if(Job!=Profession.None&&(ScreenName=="Staff"&&introduced!=Job||ScreenName=="Assigning"||ScreenName=="Main")){introPage=0;Show("Intro");}
                if(PauseMenu.IsOpen&&ScreenName=="HUD")Show("Pause");
                if(Workday.Domain.State.closed&&ScreenName=="HUD")Show("Report");
                if(ScreenName=="Intro"&&Keyboard.current!=null&&Keyboard.current.enterKey.wasPressedThisFrame){if(introPage==0){introPage=1;Show("Intro");}else{introduced=Job;EnterPhase8BWork();}}
                if(ScreenName=="Staff"&&Keyboard.current!=null)for(int i=1;i<=6;i++)if(Keyboard.current[(Key)((int)Key.Digit1+i-1)].wasPressedThisFrame){Choose(i);break;}
                if(!PauseMenu.IsOpen&&Job!=Profession.None){if(help.WasPressedThisFrame()){introPage=1;Show("Intro");}else if(book.WasPressedThisFrame()) {if(ScreenName=="Handbook")Show("HUD");else OpenBook();}else if(map.WasPressedThisFrame())Show(ScreenName=="Map"?"HUD":"Map");else if(staff.WasPressedThisFrame())Show(ScreenName=="Staff"?"HUD":"Staff");}
                var input=player.GetComponent<PrototypeInput>();input.Blocked=MenuOpen||PauseMenu.IsOpen;
                if(MenuOpen&&Cursor.lockState==CursorLockMode.Locked)input.SetCursor(false);
                var guide=Handbook?.Guide??Workday.Guidance(player);CurrentAction=ShortAction(guide?.note??"Choose a job");
                if(Time.unscaledTime>=refreshAt){refreshAt=Time.unscaledTime+.15f;RefreshHUD(guide);}
                if(player.LastFeedback!=lastFeedback){lastFeedback=player.LastFeedback;LastStamp=ShortFeedback(lastFeedback);stampUntil=Time.unscaledTime+1.7f;TownAudio.Play(TownAudio.Cue.Paper);}
                stampLabel.text=LastStamp;stampLabel.color=skin.ink;stampContainer.gameObject.SetActive(ScreenName=="HUD"&&Time.unscaledTime<stampUntil&&!string.IsNullOrEmpty(LastStamp));
                string next=NetworkShop.Instance.Revision+":"+Job+":"+CurrentAction+":"+SelectedTab;if(ScreenName=="Handbook"&&fingerprint!=next){fingerprint=next;bool more=detailOpen;ClearContent();detailOpen=more;Book();fingerprint=next;}
            }
            if(content&&content.childCount>0){var card=content.GetChild(0) as RectTransform;if(card){float t=ReducedMotion?1:Mathf.Clamp01((Time.unscaledTime-openedAt)/skin.feedbackSeconds);var a=card.anchoredPosition;card.anchoredPosition=new Vector2(a.x,card.sizeDelta.y/2+(1-t)*(1-t)*9);}}
        }
        void Join()
        {
            var c=Card("Join workday",540,335);Text(c,"Join the town",28,30,484,44,32,true,true);Text(c,"Host address",28,99,484,26,18,true);
            var bg=Plate("Address entry",c,28,141,484,43,skin.white);bg.raycastTarget=true;var input=bg.gameObject.AddComponent<TMP_InputField>();var t=Text(bg.transform,session.Address,12,6,460,31,20);t.raycastTarget=true;input.textComponent=t;input.textViewport=bg.rectTransform;input.text=session.Address;input.onValueChanged.AddListener(s=>session.Address=s);
            Button(c,"JOIN",28,214,230,48,()=>session.StartSession(false),true);Button(c,"BACK",270,214,242,48,()=>Show("Main"));Text(c,session.Status,28,280,484,36,16);
        }
        static string ShortAction(string note)
        {
            if(LivingWorkday.Instance&&LivingWorkday.Instance.Domain.State.physical)return note;
            if(note.Contains("Receive"))return "Receive shipment";if(note.Contains("Prepare"))return "Pack garage order";if(note.Contains("Pick up garage"))return "Grab garage parcel";if(note.Contains("Deliver parcel"))return "Deliver package";if(note.Contains("customer"))return "Serve customer order";if(note.Contains("Request"))return "Request battery";if(note.Contains("Open"))return "Open hood";if(note.Contains("Diagnose"))return "Check battery";if(note.Contains("incoming"))return "Test replacement";if(note.Contains("Loosen"))return "Remove old battery";if(note.Contains("Fit"))return "Install replacement";if(note.Contains("Inspect"))return "Inspect waste";if(note.StartsWith("Sort"))return "Sort waste";if(note.Contains("File sorted"))return "File waste ticket";if(note.Contains("square bag"))return "Collect square bag";if(note.Contains("packaging"))return "Collect packaging";if(note.Contains("old battery"))return "Collect old battery";if(note.Contains("Waiting"))return "Check town service";return note;
        }
        static string ProductName(string sku)=>sku=="cola"?"Sunbeam Cola":sku=="tea"?"Meadow Tea":"Golden Crackers";
        string ActionIcon()=>CurrentAction.Contains("cola")?"cola":CurrentAction.Contains("crackers")?"crackers":CurrentAction.Contains("tea")?"tea":CurrentAction.Contains("hood")?"hood":CurrentAction.Contains("Test")||CurrentAction.Contains("Check battery")?"test":CurrentAction.Contains("battery")||CurrentAction.Contains("replacement")?"battery":CurrentAction.Contains("cart")?"cart":CurrentAction.Contains("Sort")?"sort":CurrentAction.Contains("waste")||CurrentAction.Contains("Collect")?"bag":CurrentAction.Contains("customer")?"checkout":CurrentAction.Contains("File")||CurrentAction.Contains("complete")?"sign":"parcel";
        string QuickHint()
        {
            var target=Handbook?.Guide?.Primary?.target as WorkdayStation;if(target==null)return "Follow the marker to the next work point.";
            if(target.action.StartsWith("take-stock-"))return "Lift the product with the requested label.";
            if(target.action.StartsWith("stock-"))return "Match this box to its printed shelf label.";
            if(target.action.StartsWith("cart-unload"))return "Aim at a batch inside the cart. Lift it.";
            if(target.action=="cart")return "E pushes or loads. R lifts a batch.";
            if(target.action=="place-order")return "Set the customer's product beside the scanner.";
            if(target.action=="scan-order")return "Scan the placed product to print its receipt.";
            switch(target.action){case "hood":return "Lift the marked hood latch.";case "diagnose":return "Place the leads on the two battery posts.";case "bolt":return "Hold left mouse. Drag right to loosen.";case "install":return "Seat the replacement in its tray.";case "deliver":return "Set the package in the receiving tray.";case "receive":return "Open the marked shipping carton.";case "prepare":return "Wrap the garage order at dispatch.";case "customer":return "Scan the goods on the checkout counter.";case "file":return "Stamp the processed collection ticket.";default:return "Aim at the highlighted object. Press E.";}
        }
        string Detail()=>Handbook?.Guide?.detail??"The town shares one set of supplies.";
        static string ShortFeedback(string value)
        {
            if(string.IsNullOrEmpty(value))return "";if(value.Contains("Not available"))return "Follow the highlighted work step";if(value.Contains("Move closer"))return "Get closer to the marked object";int stop=value.IndexOf('.');var text=stop>0?value.Substring(0,stop):value;if(text.Length>45)text=text.Substring(0,42)+"…";return text;
        }
        void RefreshHUD(TownGuidance guide)
        {
            if(!player||!Workday)return;roleLabel.text=Job.ToString().ToUpperInvariant();objective.text=CurrentAction;roleIcon.sprite=Sprite(Job==Profession.Mechanic?"wrench":Job==Profession.Sanitation?"cart":"shop");var done=StepProgress();for(int i=0;i<progress.Count;i++)progress[i].color=done[i]?skin.green:skin.rule;
            bool inside=player.transform.position.x<-25;var world=inside?TownMinimap.MarketWorld:TownMinimap.TownWorld;miniImage.texture=Resources.Load<Texture2D>(inside?"Town/Map/DaylightMarketMap":"Town/Map/DaylightTownMap");float mapWidth=112*world.width/world.height;miniImage.rectTransform.sizeDelta=new Vector2(mapWidth,112);miniImage.rectTransform.anchoredPosition=new Vector2((152-mapWidth)/2,-32);
            MapPosition(miniPlayer,player.transform.position,world,mapWidth,112);miniPlayer.localRotation=Quaternion.Euler(0,0,-(FirstPersonView.Local?FirstPersonView.Local.Yaw:0));
            var primary=guide?.Primary;miniTarget.gameObject.SetActive(primary!=null);if(primary!=null){MapPosition(miniTarget,primary.Position,world,mapWidth,112);miniTarget.localRotation=Quaternion.Euler(0,0,45);}
            var item=player.Interactor.Current;prompt.gameObject.SetActive(ScreenName=="HUD"&&item);if(item){inputGlyph.text=Workday.Turning?"LMB":"E";inputGlyph.fontSize=Workday.Turning?14:22;promptLabel.text=Workday.Turning?"Drag to loosen clamp":item is WorkdayStation station?station.Prompt.Split(new[]{" / R "},StringSplitOptions.None)[0]:item is SimpleDoor?"Open door":"Use object";promptAlt.text=Workday.Turning?"LMB DRAG RIGHT / Q CANCEL":item is WorkdayStation ws&&ws.alternative!=null?"R  "+ws.altLabel:Workday.Holding(player.OwnerClientId)?"Q  Return safely":"";}
            if(item is ShopStation shopExit&&shopExit.kind==ShopStation.Kind.Exit){inputGlyph.text="W";promptLabel.text="Walk to town";promptAlt.text="Through the framed doorway";}
            if(item is TownStation entrance&&entrance.kind==TownStation.Kind.Portal&&NetworkShop.Instance.Targets.Values.OfType<SimpleDoor>().Any(d=>d.IsOpen)){inputGlyph.text="W";promptLabel.text="Enter market";promptAlt.text="Walk through the open doorway";}
            if(BusinessAsUsual.Phase8B.Phase8BFlow.Enabled){Phase8BDestination(guide);return;}
            routePin.gameObject.SetActive(false);if(ScreenName=="HUD"&&primary!=null&&FirstPersonView.Local){var cam=FirstPersonView.Local.ViewCamera;float dist=Vector3.Distance(player.transform.position,primary.Position);if(dist>2.4f){var pos=cam.WorldToViewportPoint(primary.Position);float x=pos.z<0?1-pos.x:pos.x,y=pos.z<0?1-pos.y:pos.y;var size=root.rect.size;routePin.anchorMin=routePin.anchorMax=TopLeft;routePin.anchoredPosition=new Vector2(Mathf.Clamp(x*size.x,360,size.x-190),-Mathf.Clamp((1-y)*size.y,136,size.y-150));routePin.GetComponentInChildren<TextMeshProUGUI>().text=Mathf.RoundToInt(dist)+" m";routePin.gameObject.SetActive(true);}}
        }
        static void MapPosition(RectTransform r,Vector3 p,Rect world,float w,float h){r.anchoredPosition=new Vector2(Mathf.Clamp(Mathf.InverseLerp(world.xMin,world.xMax,p.x)*w,8,w-8),-Mathf.Clamp((1-Mathf.InverseLerp(world.yMin,world.yMax,p.z))*h,8,h-8));}
    }
}

